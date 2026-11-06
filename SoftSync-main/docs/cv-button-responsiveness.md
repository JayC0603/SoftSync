# CV button responsiveness

The workspace previously read all extracted document text and all saved review
JSON during initialization. This happened during prerender and again when the
interactive component started; a shared busy flag blocked every CV button until
both database queries finished.

Initialization now resolves the authenticated user without querying CV storage.
Opening Library or History changes the screen immediately and loads only that
section, with a local loading message. Navigation remains usable while those reads
are pending. Successful lists are reused within the workspace; failed reads expose
a retry action without locking navigation.

The library query projects metadata only. Selecting a CV retrieves that user's
document text and extraction metadata without the original file bytes, then caches
the selection. A selection response cannot navigate back to Review after the user
has already moved elsewhere or begun deleting/uploading a document. Reads are
cancelled when the workspace is disposed. No schema migration is required.

The SignalR receive limit is now a bounded 256 KB so a supported-length JD with
Vietnamese characters fits together with JSON/event metadata. The previous
32 KB default could reject a text-input event while the UI still showed the
browser's edited value. Reference: [Microsoft Blazor SignalR guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-10.0#maximum-receive-message-size).

Validation: focused review/history/persistence tests passed (40 tests, one optional
live AI test skipped). Browser checks on the local demo at 390px and 1440px covered
menu navigation, library filters, JD templates and upload/text tabs, uploading,
reloading, selecting a stored document and deletion. Sixteen measured actions took
54-611ms locally with no browser exceptions. A 19,000-character Vietnamese input
was acknowledged by the server and preserved after switching JD tabs. These timings are local measurements,
not a production latency guarantee.
