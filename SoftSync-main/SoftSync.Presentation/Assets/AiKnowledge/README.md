# SoftSync Assistant Knowledge Base

`softsync-assistant.vi-en.json` is the runtime knowledge source for `KnowledgeBasedAiAssistantService`.

## Enable the LLM

The app uses an OpenAI-compatible `POST /v1/chat/completions` endpoint. Keep secrets out of `appsettings.json` and configure them with environment variables:

```powershell
$env:AiApi__Enabled="true"
$env:AiApi__ApiKey="your-api-key"
$env:AiApi__BaseUrl="https://api.openai.com/"
$env:AiApi__Model="gpt-4.1-mini"
dotnet run --project SoftSync.Presentation
```

Docker/Render use the same names: `AiApi__Enabled`, `AiApi__ApiKey`, `AiApi__BaseUrl`, and `AiApi__Model`. If the API is disabled, unavailable, times out, or returns invalid JSON, the local knowledge service answers instead so the Assistant remains usable.

## Entry schema

- `id`: stable, unique kebab-case identifier.
- `category`: broad domain such as `communication`, `time-management`, `critical-thinking`, or `platform`.
- `titleEn`, `titleVi`: bilingual titles used for retrieval.
- `answerEn`, `answerVi`: concise, actionable, self-contained answers.
- `route`: optional in-app destination appended to the response. Use `/assessment`; the service adds the current user ID.
- `keywords`: English and Vietnamese phrases users are likely to type.
- `tags`: short concepts used as lower-weight retrieval signals.

## Editing rules

1. Keep both language variants semantically equivalent.
2. Prefer one learning objective per entry and practical guidance over definitions alone.
3. Add spelling variants and both accented/unaccented concepts only when needed; retrieval already strips Vietnamese diacritics.
4. Do not include secrets, personal data, generated user content, medical/legal claims, or copyrighted third-party course text.
5. Keep `softsync-overview` because it is the safe fallback for unmatched questions.
6. Validate after editing:

   ```powershell
   Get-Content Assets/AiKnowledge/softsync-assistant.vi-en.json | ConvertFrom-Json
   dotnet build
   ```

## Moving to an external LLM or vector store

Each JSON object is already a retrieval document. Embed `titleEn/titleVi`, `keywords`, `tags`, and the answers; retain `id`, `category`, and `route` as metadata. The `IAiAssistantService` interface can remain unchanged, so the UI does not need to be rewritten.
