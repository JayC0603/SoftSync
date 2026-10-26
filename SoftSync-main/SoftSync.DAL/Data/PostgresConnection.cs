using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SoftSync.DAL.Data;

public static class PostgresConnection
{
    public static string Resolve(IConfiguration configuration, bool direct = false)
    {
        var keys = direct
            ? new[] { "DATABASE_URL_UNPOOLED", "POSTGRES_URL_NON_POOLING", "DATABASE_URL", "POSTGRES_URL", "ConnectionStrings:SoftSyncDb" }
            : new[] { "DATABASE_URL", "POSTGRES_URL", "ConnectionStrings:SoftSyncDb" };
        foreach (var key in keys)
            if (!string.IsNullOrWhiteSpace(configuration[key]))
                return Parse(configuration[key]!);
        throw new InvalidOperationException("Connect a Neon PostgreSQL database through Vercel Marketplace to provide DATABASE_URL.");
    }

    public static string Parse(string value)
    {
        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return new NpgsqlConnectionStringBuilder(value).ConnectionString;

        var uri = new Uri(value, UriKind.Absolute);
        var user = uri.UserInfo.Split(':', 2);
        if (string.IsNullOrEmpty(uri.Host) || string.IsNullOrEmpty(user[0]) || uri.AbsolutePath.Length < 2)
            throw new InvalidOperationException("The PostgreSQL URL must include a host, username and database.");
        var connection = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(user[0]),
            Password = user.Length > 1 ? Uri.UnescapeDataString(user[1]) : "",
            SslMode = SslMode.VerifyFull,
            MaxPoolSize = 20,
            Timeout = 30,
            CommandTimeout = 60
        };
        // Preserve explicit SSL choices for local PostgreSQL and provider URLs.
        foreach (var item in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = item.Split('=', 2);
            if (pair.Length != 2 || !pair[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase)) continue;
            connection.SslMode = Uri.UnescapeDataString(pair[1]).ToLowerInvariant() switch
            {
                "disable" => SslMode.Disable,
                "allow" => SslMode.Allow,
                "prefer" => SslMode.Prefer,
                "require" => SslMode.Require,
                "verify-ca" => SslMode.VerifyCA,
                "verify-full" => SslMode.VerifyFull,
                _ => throw new InvalidOperationException("Unsupported PostgreSQL SSL mode.")
            };
        }
        return connection.ConnectionString;
    }
}
