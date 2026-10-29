# Explicit XAH backup credential

Production settings:

- `XahFallback__ApiKey`: Sensitive production environment variable; no key in source or browser config.
- `XahFallback__Model`: `thanhnhan9023/gpt-6.1-sol`.

Every call first uses the existing primary key and model. Only POST chat-completion
requests to the pinned HTTPS XAH endpoint, authenticated with the deployment's
`AiApi` or `CvLlm` credential, qualify for one backup request. HTTP 401/403,
402/429, 404, 5xx and connection failures can use the backup. HTTP 400, input
errors, malformed/invalid model output and cancellation do not rotate credentials.
The existing request deadline still bounds both attempts; an already expired
deadline does not initiate another request. Admin connection tests explicitly
disable backup so an invalid tested credential cannot appear healthy.

Backup attempts preserve the full prompt, documents and generation settings and
replace only the authorization key and paired model. They never switch to a
different provider or host. Another failed backup returns normally to the feature's
existing error handling, without a credential retry loop. The next independent
request tries the primary again. CV response validation remains in place and CV
results identify the model that actually answered.

Both keys stay in server-only configuration. HTTP logging is disabled for these
clients, redirects are rejected, and custom logs include status codes only.

Validation: 61 focused tests passed, one optional live CV test skipped. Coverage
includes primary success, failure statuses, key/model pairing, preserved JSON
settings, cancellation, network failures, backup failure, admin-test opt-out,
host/key restrictions and actual CV model reporting. A short live JSON request
using the new backup credential/model returned HTTP 200 with the expected object.
