using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Data;
using Xunit;

namespace SoftSync.Tests;

public sealed class AiProviderHttpTests
{
    [CvHttpAcceptanceFact]
    public async Task Admin_page_actual_Identity_allows_Admin_and_denies_learner_Teacher_and_anonymous()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var origin = new Uri(Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_URL")!);
        const string route = "/admin/ai-configuration";
        using var anonymous = CvHttpAcceptanceTests.Client(origin);
        using var anonymousResponse = await anonymous.GetAsync(route);
        Assert.Equal("/Account/Login", anonymousResponse.RequestMessage!.RequestUri!.AbsolutePath);
        var options = new DbContextOptionsBuilder<SoftSyncDbContext>().UseNpgsql(Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_DB")).Options;
        await using var db = new SoftSyncDbContext(options);
        foreach (var role in new[] { "User", "Teacher", "Admin" })
        {
            var suffix = Guid.NewGuid().ToString("N");
            var email = "ai-http-" + suffix + "@example.invalid"; var password = "Synthetic1!" + suffix;
            using var register = CvHttpAcceptanceTests.Client(origin);
            await CvHttpAcceptanceTests.Register(register, email, password);
            if (role != "User")
            {
                var user = await db.Users.SingleAsync(x => x.Email == email);
                var targetRole = await db.Roles.SingleAsync(x => x.Name == role);
                db.UserRoles.Add(new IdentityUserRole<int> { UserId = user.Id, RoleId = targetRole.Id });
                await db.SaveChangesAsync();
            }
            using var client = CvHttpAcceptanceTests.Client(origin);
            await CvHttpAcceptanceTests.Login(client, email, password);
            using var response = await client.GetAsync(route);
            if (role == "Admin")
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(route, response.RequestMessage!.RequestUri!.AbsolutePath);
                var html = await response.Content.ReadAsStringAsync();
                Assert.Contains("id=\"ai-config-title\"", html);
                Assert.Contains("id=\"ai-secret\"", html);
                Assert.Contains("type=\"password\"", html);
                Assert.Contains("id=\"provider-list-title\"", html);
                Assert.DoesNotContain("EncryptedApiKey", html);
            }
            else
            {
                Assert.Equal("/Account/AccessDenied", response.RequestMessage!.RequestUri!.AbsolutePath);
                Assert.DoesNotContain("id=\"ai-secret\"", await response.Content.ReadAsStringAsync());
            }
        }
    }
}
