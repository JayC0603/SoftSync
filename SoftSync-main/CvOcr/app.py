import base64
import binascii
import hmac
import json
import logging
import os
import threading
import time
from contextlib import asynccontextmanager

from fastapi import FastAPI, HTTPException, Request
from starlette.concurrency import run_in_threadpool

from layout import reading_order

MAX_PDF_BYTES = 10 * 1024 * 1024
MAX_BODY_BYTES = 14 * 1024 * 1024
MAX_TEXT = 40_000
worker_lock = threading.Lock()
engine = None


@asynccontextmanager
async def lifespan(app):
    global engine
    if not os.environ.get("CV_OCR_API_KEY"):
        raise RuntimeError("CV_OCR_API_KEY must be configured")
    from engine import create_engine

    # One resident model, one inference at a time. No CV files written to disk.
    engine = create_engine()
    # Libraries may log inference details; application responses never echo input.
    logging.getLogger("paddleocr").setLevel(logging.ERROR)
    yield
    engine = None


app = FastAPI(lifespan=lifespan, docs_url=None, redoc_url=None, openapi_url=None)


@app.get("/health")
def health():
    if engine is None:
        raise HTTPException(503, "OCR is warming up")
    return {"ready": True}


def recognize(pdf, pages):
    import numpy as np
    import pypdfium2 as pdfium

    if not worker_lock.acquire(blocking=False):
        raise HTTPException(429, "OCR is busy; retry later")
    started = time.monotonic()
    try:
        with pdfium.PdfDocument(pdf) as document:
            if len(document) > 40 or any(p > len(document) for p in pages):
                raise HTTPException(422, "Invalid page range")
            result = []
            total_text = 0
            for number in pages:
                if time.monotonic() - started > 110:
                    raise HTTPException(504, "OCR deadline exceeded")
                page = document[number - 1]
                try:
                    width, height = page.get_size()
                    # Cap rendering to 2400 px on the longest side and 200 DPI.
                    scale = min(200 / 72, 2400 / max(width, height))
                    bitmap = page.render(scale=scale)
                    try:
                        image = np.array(bitmap.to_pil().convert("RGB"))[:, :, ::-1].copy()
                    finally:
                        bitmap.close()
                    predictions = list(engine.predict(image))
                    if len(predictions) != 1:
                        raise HTTPException(422, "Unreadable page")
                    raw = predictions[0].json
                    if isinstance(raw, str):
                        raw = json.loads(raw)
                    raw = raw.get("res", raw)
                    blocks = []
                    for text, score, polygon in zip(raw["rec_texts"], raw["rec_scores"], raw["rec_polys"]):
                        text = text.strip()
                        if not text:
                            continue
                        xs, ys = zip(*polygon)
                        blocks.append({"text": text, "confidence": float(score),
                                       "box": [float(min(xs)), float(min(ys)), float(max(xs)), float(max(ys))]})
                    ordered = reading_order(blocks)
                    text = "\n".join(b["text"] for b in ordered)
                    total_text += len(text)
                    if total_text > MAX_TEXT:
                        raise HTTPException(422, "Document text is too long")
                    # The least confident line triggers review even if most lines are clear.
                    confidence = min((b["confidence"] for b in blocks), default=0)
                    result.append({"page": number, "text": text, "confidence": confidence})
                finally:
                    page.close()
            return {"pages": result}
    except HTTPException:
        raise
    except Exception:
        raise HTTPException(422, "Unable to read PDF") from None
    finally:
        worker_lock.release()


@app.post("/ocr")
async def ocr(request: Request):
    expected = os.environ.get("CV_OCR_API_KEY", "")
    supplied = request.headers.get("authorization", "")
    if not expected or not hmac.compare_digest(supplied.encode(), f"Bearer {expected}".encode()):
        raise HTTPException(401, "Invalid OCR credential")
    body = bytearray()
    async for chunk in request.stream():
        body.extend(chunk)
        if len(body) > MAX_BODY_BYTES:
            raise HTTPException(413, "PDF is too large")
    try:
        payload = json.loads(body)
        pages = payload["pages"]
        pdf = base64.b64decode(payload["pdf"], validate=True)
        if (not isinstance(pages, list) or not 1 <= len(pages) <= 10
                or any(type(p) is not int or not 1 <= p <= 40 for p in pages)
                or len(set(pages)) != len(pages)
                or not 1 <= len(pdf) <= MAX_PDF_BYTES or not pdf.startswith(b"%PDF-")):
            raise ValueError()
    except (ValueError, KeyError, TypeError, binascii.Error):
        raise HTTPException(422, "Invalid OCR request") from None
    if engine is None:
        raise HTTPException(503, "OCR is warming up")
    return await run_in_threadpool(recognize, pdf, pages)
