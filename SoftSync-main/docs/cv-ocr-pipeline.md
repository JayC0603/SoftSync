# CV extraction, OCR and LLM review

The authenticated `/cv` workspace first extracts readable PDF pages with PdfPig,
using word/block analysis and reading-order detection. TXT files are read as UTF-8.
Pages with fewer than 30 letters/digits or excessive replacement characters are
sent to the optional PaddleOCR service. This heuristic can misclassify sparse
pages and cannot identify every incorrect PDF text layer.

OCR is limited to 10 pages per upload (40 total PDF pages), 10 MB per file and
40,000 extracted characters. The Python service renders only requested pages,
capped at 200 DPI / 2400 pixels on the longest side. Its resident Vietnamese
PP-OCRv5 model supports Vietnamese/Latin text. A conservative whitespace-based
XY-cut orders OCR lines; complex layouts and rotated scans still require visual
acceptance. This is not a guarantee of correct reading order for every CV.

Documents retain the original file and combined text under the existing account
ownership rules. `ExtractionJson` stores page methods, minimum line confidence,
elapsed time and extraction version (not another copy of the text). A low-confidence
page is flagged for preview. Old documents without metadata remain readable.
Re-analysis uses saved text and does not call OCR. Upload a corrected TXT/PDF to
replace unreadable text; this change does not add an extracted-text editor.

## Run OCR

Use Docker with Python 3.11 in the image; the host Python need not run Paddle.
Set `CV_OCR_API_KEY` to a random local secret in the environment or untracked
`.env`, then from the solution directory:

```powershell
docker compose -f docker-compose.yml -f docker-compose.ocr.yml up -d --build web cv-ocr
```

The override connects the server using `CvOcr__BaseUrl=http://cv-ocr:8090/` and
`CvOcr__ApiKey`. The OCR port is not published. `/health` becomes ready after model
download and warmup; first startup requires network access to download model
weights into the named cache volume. `/ocr` requires a Bearer key, does not accept
remote URLs, and does not write CVs to disk or log their contents. Use one Uvicorn
worker; inference and PDFium rendering are serialized and busy calls return 429.
Requests are not retried automatically. The model call itself cannot be interrupted
mid-inference: client cancellation stops waiting, while the worker may finish the
current page before releasing resources. The worker checks a deadline between pages.

For a separately hosted OCR service, configure these server values:

```text
CvOcr__BaseUrl=https://your-private-ocr-service/
CvOcr__ApiKey=<same as service CV_OCR_API_KEY>
CvOcr__TimeoutSeconds=90
```

Without an OCR URL, text PDFs/TXT continue to work and scanned PDFs show a controlled
error. CV reviews use their dedicated XAH configuration; OCR does not fix a slow LLM gateway.
AI keys stay on the server. No paid AI calls are required by the automated tests.

## Vercel Services

Both Vercel configurations include an internal `cv-ocr` container. The .NET
container receives its private address in `CV_OCR_SERVICE_URL` through a service
binding; this takes precedence over `CvOcr:BaseUrl`. There is no public OCR rewrite.
Set the same generated production Secret in `CV_OCR_API_KEY` and `CvOcr__ApiKey`.
`CvOcr/Dockerfile.vercel` downloads and warms the lightweight detector/recognizer
during the image build so instances reuse bundled weights. Vercel can still scale
instances down, so cold-start latency must be measured after deployment.

The Vercel request payload limit also applies to OCR calls: scanned PDFs are
limited to 3 MB before Base64 encoding. Text PDFs/TXT keep the workspace's 10 MB
limit. Larger scans show an actionable error rather than uploading an oversized
request. Vercel deployment uses the existing production database and Blob store.

## Database migration

`AddCvExtractionMetadata` adds one nullable text column to `CvDocuments`; it does
not recreate tables. Apply it using the existing deployment migration process.
The normal startup initializer already applies migrations. No existing database
is modified by the implementation/test commands in this change.

## Review output

The `/cv` workspace uses `https://api.xah.io/v1/chat/completions` with default
model `tranhieu13102003/gpt-6-luna`. Set `CvLlm__ApiKey` as a production Secret.
Optional server overrides: `CvLlm__Model`, `CvLlm__Enabled`,
`CvLlm__TimeoutSeconds` (default 75, max 110), `CvLlm__MaxOutputTokens` (7500),
and `CvLlm__UseJsonMode` (true). A provider rejecting JSON mode returns an
actionable error; change the setting deliberately rather than retrying paid calls
automatically. All requests remain server-side with redirects disabled. Provider
error bodies, keys and CV text are never logged. Other AI features and the legacy
`/cv-review` route keep their existing providers.

Job-specific actions and rewrites identify exact JD excerpts. Requirements are
atomic and weighted: Required=3, Unspecified=2, Preferred=1. Evidence status gets
Met=1, Partial=0.5, NotDemonstrated=0. The server calculates the final JD score
from these values after checking quotes and rejecting duplicate requirements;
the LLM's provisional total is ignored. This measures evidence in the document,
not the person's actual competence or hiring probability. Old saved reviews keep
their original scores and render without the new fields.

One LLM request receives CV text + optional JD. The server checks score ranges,
bilingual fields, priorities, exact CV evidence and exact JD requirement excerpts.
It rejects incomplete JD comparisons, invented quotes and invalid JSON. This
validates structure/provenance, not factual correctness of every model inference.
Suggestions are rendered as escaped text. Scores are advisory, not ATS predictions.

The result retains old strengths/improvements/rewrites for backward compatibility,
adding prioritized `Actions`, follow-up `Questions`, detailed JD `Requirements`
and LLM elapsed time. Older saved reviews show their existing lists. The UI has
separate overview, criteria, JD comparison and edits tabs, plus extracted text
preview, progress stages and cancellation.

## Verification

```powershell
dotnet test SoftSync.Tests --filter "FullyQualifiedName~CvExtractionPipelineTests|FullyQualifiedName~CvWorkspacePersistenceTests"
python -m unittest discover -s CvOcr -p "test_*.py" -v
```

The Python HTTP tests need FastAPI and HTTPX; they mock OCR and do not load Paddle.
Real OCR acceptance: upload a Vietnamese scanned CV, a two-column CV and a mixed
text/scan PDF; check accents, reading order, flagged confidence, cancellation,
reload, account isolation and analyzing twice without another OCR request.
Measure extraction time independently of LLM time on the deployment hardware.

XAH transport tests mock HTTP; the explicitly opted-in real test sends a synthetic
CV/JD through the actual client and validator. It verifies that TypeScript, absent
from the CV, is reported as not demonstrated and not invented in rewrites:

```powershell
# Set the real key through your secret manager/environment; do not paste it into source.
$env:SOFTSYNC_XAH_REAL_AI = "1"
dotnet test SoftSync.Tests --filter "FullyQualifiedName~RealXahCvReviewTests"
Remove-Item Env:SOFTSYNC_XAH_REAL_AI
```
