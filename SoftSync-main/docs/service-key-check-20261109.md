# Service configuration and key checks — 2026-11-09

## Verified results

| Component | Evidence | Result and limit |
| --- | --- | --- |
| Production application | `/health` and `/api/courses/` | HTTP 200; public course data loads through the application's database connection. This does not verify every Preview deployment. |
| XAH key saved locally | Authenticated model catalog | HTTP 200. The tested key can access the provider. Its equality with current Vercel secrets could not be checked. |
| Recorded primary model | `giabaopro1908/gemini-3.8-flash-high` | HTTP 404 during a synthetic JSON request. |
| Recorded fallback model | `tranhieu13102003/gpt-6-luna` | HTTP 200 and valid JSON. |
| User-specified replacement model | `dungcsnd113/gemini-flash-3.8` | HTTP 200 for a short greeting. Real chatbot service also returned Vietnamese/English using one model response; real CV/JD review passed grounded-evidence checks with two actions. |
| Chatbot service | Real `LlmAiAssistantService` with synthetic user ID 0, real provider and configured primary/fallback | Primary returned 404; fallback returned 200. Both languages identified SYNCY and reused one successful response. |
| Production chatbot form | Anonymous headless browser at `/assistant` | Could not submit: the send button stayed disabled after entering text, despite a Blazor WebSocket receiving frames. The test did not reach an API response and does not prove the production chatbot key works. |
| CV analysis | `RealXahCvReviewTests` using a synthetic CV/JD and the working model | Passed the structured review, action and grounded-evidence assertions. No production CV was sent or saved. |
| Blob token saved locally | Authenticated read-only list request, limit 1 | Successful. No object names or contents were printed. Current Vercel variable scope remains unverified. |
| Google OAuth | Production health configuration flag | Configured; a real Google sign-in/token exchange was not performed. |
| OCR | Client/service source configuration | Contract reviewed; live OCR credentials and scan inference remain unverified. |
| Vercel Production/Preview scopes | Existing credentials against both recorded SoftSync projects | HTTP 403. Current variable availability, branch overrides and OCR bindings could not be inspected or changed. |

The direct connection check using an older saved database credential did not
connect. That saved snapshot is not proof of the current production credential;
the live production course API succeeded. Historical variable metadata recorded
the required credentials as Production-only, but current scopes require access
to the Vercel project to confirm.

## Backend corrections

- Allow a failed XAH model to fall back to another model using the same account
  key. The previous handler required different keys, disabling this supported
  configuration. Identical key/model pairs still do not retry; host restrictions,
  admin connection-test exclusions and the single-retry limit remain enforced.
- Mark successful chatbot model replies so the paired Vietnamese/English calls
  reuse the result. Local-knowledge fallback responses are not cached, allowing
  a later request to recover when the provider becomes available.
- Added fallback regression tests and included the chatbot request/context,
  recovery and cross-user cache-isolation tests. The opt-in live chatbot test can
  choose a current model using `SOFTSYNC_ASSISTANT_SMOKE_MODEL`.

Validation: full .NET suite with the synthetic live CV check enabled: 406 passed,
13 skipped, 0 failed. Focused chatbot/fallback suite with live chatbot check
enabled: 27 passed, 0 skipped, 0 failed. A separate live service smoke check also
verified the actual 404-to-200 fallback path with a shared key.

## Required environment mapping

Set secrets in the Vercel dashboard for the intended environment and redeploy.
Never paste real credentials into this file or `.env.vercel.example`.

| Feature | Required names / relationship |
| --- | --- |
| Database | `DATABASE_URL`; `DATABASE_URL_UNPOOLED` for the direct migration connection when supplied by Neon. Use the intended database for each environment. |
| Chatbot / shared AI | `AiApi__Enabled=true`, `AiApi__Provider=Xah`, `AiApi__UseDeploymentConfiguration=true`, `AiApi__BaseUrl=https://api.xah.io/v1/`, `AiApi__ApiKey`, `AiApi__Model`. |
| CV review | `CvLlm__Enabled=true`, `CvLlm__ApiKey`, `CvLlm__Model`. The CV transport reads its own key, not `AiApi__ApiKey`. |
| XAH fallback | `XahFallback__ApiKey`, `XahFallback__Model`. The key may match the primary key when the model differs. |
| OCR | `.NET` reads `CvOcr__ApiKey`; the Python service reads `CV_OCR_API_KEY`. The values must match. Vercel service binding supplies `CV_OCR_SERVICE_URL`; `CvOcr__BaseUrl` is the external-service alternative. |
| Uploads | `BLOB_READ_WRITE_TOKEN` with access to the application's intended store. |
| Google login | `Authentication__Google__ClientId`, `Authentication__Google__ClientSecret`; register the active domain's `/signin-google` callback in Google Cloud. |
| Separate recruitment matching service | `RecruitmentAi__BaseUrl` must point to its deployed service if this feature is used. `vercel.json` in this repo deploys the .NET app and OCR service; it does not deploy the separate recruitment model service. |

`dungcsnd113/gemini-flash-3.8` was subsequently verified as the user's replacement
primary model for both chatbot and CV. The example environment file now uses it.
`tranhieu13102003/gpt-6-luna` was also verified for the synthetic chatbot/CV checks
and remains the example fallback model.
Existing Vercel values may override the checked-in model defaults. Confirm an
alternative model through the provider's authenticated catalog and a synthetic
request before configuring it. Credentials and environment scopes were not
changed by these local checks.
