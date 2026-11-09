def create_engine():
    import numpy as np
    from paddleocr import PaddleOCR

    engine = PaddleOCR(
        lang="vi", ocr_version="PP-OCRv5", device="cpu",
        text_detection_model_name="PP-OCRv5_mobile_det",
        use_doc_orientation_classify=False, use_doc_unwarping=False,
        use_textline_orientation=False, enable_mkldnn=False, cpu_threads=2,
    )
    list(engine.predict(np.full((64, 256, 3), 255, dtype=np.uint8)))
    return engine
