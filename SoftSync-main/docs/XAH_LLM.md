# XAH LLM deployment

SoftSync uses `tranhieu13102003/gpt-6-luna` through
`https://api.xah.io/v1/chat/completions`. Existing task prompts supply the
SYNCY identity, bilingual coaching, learning rubrics and evidence-based CV/JD
review. This is API integration and prompt customization, not model training.

Set `AiApi__ApiKey` and `CvLlm__ApiKey` as **Secret** production environment
variables in the linked Vercel `softsync` project. Never commit keys or put them
in browser configuration. See `.env.vercel.example` for non-secret settings.

`AiApi:UseDeploymentConfiguration=true` selects the deployed configuration for
SYNCY, tutoring and learning feedback before either legacy Gemini settings or
saved provider configurations. Missing primary credentials disable remote calls;
after configured remote attempts fail, features retain their existing error or
local fallback behavior. Set this
flag to false to restore administrator-managed provider selection. CV review
continues to use its dedicated `CvLlm` client and explicit error states.

An explicitly configured `XahFallback__ApiKey` and `XahFallback__Model` now provide
one backup attempt when the deployment's primary XAH credential receives an
authentication, quota, model-unavailable or server error, or a connection failure.
The original key/model are tried first for every request. See
`xah-key-fallback.md` for scope, cancellation and validation behavior.

XAH provider settings pin the official endpoint, use Bearer authorization, JSON
responses, bounded output tokens, request deadlines, and disabled redirects and
HTTP logging. Existing CV schema validation and learning score rules remain in
place. An API key alone does not prove output quality across all tasks.

Run focused checks from the repository root:

```powershell
dotnet test SoftSync-main/SoftSync.Tests/SoftSync.Tests.csproj --filter 'FullyQualifiedName~GeminiConfigurationTests|FullyQualifiedName~XahCvReviewTests|FullyQualifiedName~AiProviderTests'
```

An opt-in live CV test requires `SOFTSYNC_XAH_REAL_AI=1` and `CvLlm__ApiKey`
in the process environment. It sends only synthetic CV/JD fixtures.
Live acceptance completed in approximately 100 seconds, so CV requests use a
110-second deadline. Short JSON compatibility requests also returned HTTP 200.
Response latency can vary; a deadline still returns the existing retryable error.

Deploy from the linked outer repository using `vercel deploy --prod --yes`.
The existing services configuration builds the ASP.NET application and its OCR
service together. Production database, Blob and OCR credentials must already
be configured. Verify the deployment reaches Ready and check `/health` and `/`.
