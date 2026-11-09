# CV review response recovery

Previously, one incomplete JSON response or an invalid evidence excerpt immediately
failed the entire CV review with `ai_invalid_response`. The review service now
validates the response, sends field-level correction instructions with the original
documents, and regenerates the complete review once. It also retries malformed or
truncated provider JSON once. Authentication, rate limits, timeouts and connection
errors are not retried.

Both attempts use the same evidence and schema checks. Invalid output never reaches
the results page or cache. Diagnostic logs contain fixed validation descriptions,
never document excerpts, provider responses or credentials. Cancellation stops the
retry. The workspace allows 240 seconds for two provider requests bounded to 110
seconds each; the Cancel action remains available.

Validation: focused CV review, extraction and history tests: 39 passed, one live
provider test skipped because its opt-in credential was unavailable. Regression
cases include missing/null criteria, invented evidence/requirements, malformed
JSON, persistent failure without caching, no-JD recovery, and cancellation.
