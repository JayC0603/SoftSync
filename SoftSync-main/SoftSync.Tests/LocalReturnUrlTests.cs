using SoftSync.BLL.Auth;
using Xunit;

namespace SoftSync.Tests;

public sealed class LocalReturnUrlTests
{
    [Theory]
    [InlineData("cv", "/cv")]
    [InlineData("/cv", "/cv")]
    [InlineData("cv?tab=matching", "/cv?tab=matching")]
    [InlineData("/roadmap", "/roadmap")]
    [InlineData(null, "/dashboard")]
    [InlineData("", "/dashboard")]
    [InlineData("/", "/dashboard")]
    [InlineData("https://external.example/cv", "/dashboard")]
    [InlineData("//external.example", "/dashboard")]
    [InlineData("/\\external.example", "/dashboard")]
    [InlineData("%2f%2fexternal.example", "/dashboard")]
    [InlineData("/%5cexternal.example", "/dashboard")]
    [InlineData("javascript:alert(1)", "/dashboard")]
    [InlineData("cv%0d%0aInjected:header", "/dashboard")]
    public void OAuth_and_password_sign_in_keep_local_destinations_only(string? input, string expected)
        => Assert.Equal(expected, LocalReturnUrl.Normalize(input));
}
