(function () {
    // Enhanced navigation must not start Blazor or register handlers twice.
    if (window.ssBlazorStarted) return;
    window.ssBlazorStarted = true;

    Blazor.start({
        circuit: {
            configureSignalR: function (builder) {
                // Vercel pins a WebSocket to one instance. A separate negotiate
                // request can reach another instance and lose its connection ID.
                builder.withUrl('/_blazor', { transport: 1, skipNegotiation: true });
                builder.withServerTimeout(60000).withKeepAliveInterval(15000);
            },
            reconnectionOptions: {
                maxRetries: 15,
                retryIntervalMilliseconds: function (attempt, maxRetries) {
                    return attempt >= maxRetries ? null : Math.min(1000 * (attempt + 1), 5000);
                }
            }
        }
    }).catch(function (error) {
        console.error('SoftSync could not start its interactive connection.', error);
        // Keep server-rendered links available and expose the existing retry UI.
        var notice = document.getElementById('blazor-error-ui');
        if (notice) notice.style.display = 'block';
    });
})();
