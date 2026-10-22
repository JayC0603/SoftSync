using Microsoft.Extensions.Configuration;
using Npgsql;
using SoftSync.DAL.Data;
using Xunit;

namespace SoftSync.Tests;

public sealed class PostgresConnectionTests
{
    [Fact]
    public void Encoded_credentials_cannot_inject_connection_options()
    {
        var result = new NpgsqlConnectionStringBuilder(PostgresConnection.Parse(
            "postgresql://user:p%3BHost%3Devil%3D%40%3A@db.example/soft%20sync?sslmode=require"));
        Assert.Equal("db.example", result.Host);
        Assert.Equal("p;Host=evil=@:", result.Password);
        Assert.Equal("soft sync", result.Database);
        Assert.Equal(SslMode.Require, result.SslMode);
    }

    [Fact]
    public void Migration_uses_direct_url_and_runtime_uses_pooler()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DATABASE_URL"] = "postgresql://user:password@pooled.example/softsync",
            ["DATABASE_URL_UNPOOLED"] = "postgresql://user:password@direct.example/softsync"
        }).Build();
        Assert.Equal("pooled.example", new NpgsqlConnectionStringBuilder(PostgresConnection.Resolve(config)).Host);
        Assert.Equal("direct.example", new NpgsqlConnectionStringBuilder(PostgresConnection.Resolve(config, true)).Host);
    }

    [Fact]
    public void Legacy_marketplace_variable_is_supported()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["POSTGRES_URL"] = "postgres://user:password@db.example/softsync"
        }).Build();
        var result = new NpgsqlConnectionStringBuilder(PostgresConnection.Resolve(config));
        Assert.Equal("db.example", result.Host);
        Assert.Equal(SslMode.VerifyFull, result.SslMode);
    }

    [Fact]
    public void Missing_database_configuration_fails_clearly()
        => Assert.Throws<InvalidOperationException>(() => PostgresConnection.Resolve(new ConfigurationBuilder().Build()));
}
