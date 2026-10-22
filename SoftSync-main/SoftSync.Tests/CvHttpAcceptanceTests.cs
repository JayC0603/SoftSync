using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Services;
using SoftSync.DAL.Data;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class CvHttpAcceptanceFact : FactAttribute
{
    public CvHttpAcceptanceFact()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_URL"))
            || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_DB")))
            Skip = "Requires the dedicated isolated runtime acceptance app and PostgreSQL.";
    }
}
public sealed class CvHttpAcceptanceTests
{
    [CvHttpAcceptanceFact][Trait("Category", "CvHttp")]
    public async Task Actual_Identity_login_SSR_result_reload_history_and_cross_user_detail()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var options = new DbContextOptionsBuilder<SoftSyncDbContext>().UseNpgsql(Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_DB")).Options;
        var factory = new CvDbContextFactory(options);
        var origin = new Uri(Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_URL")!);
        var suffix = Guid.NewGuid().ToString("N"); var password = "Synthetic1!" + suffix;
        using var registeredA = Client(origin); using var registeredB = Client(origin);
        var emailA = "cv-http-a-" + suffix + "@example.invalid"; var emailB = "cv-http-b-" + suffix + "@example.invalid";
        await Register(registeredA, emailA, password); await Register(registeredB, emailB, password);
        using var a = Client(origin); using var b = Client(origin);
        await Login(a, emailA, password); await Login(b, emailB, password);
        var page = await a.GetStringAsync("/cv-review");
        Assert.Contains("id=\"cv-file\"", page); Assert.Contains("AI: chưa cấu hình", WebUtility.HtmlDecode(page));
        using var anonymous = Client(origin);
        using var redirect = await anonymous.GetAsync("/cv-review");
        Assert.Equal("/Account/Login", redirect.RequestMessage!.RequestUri!.AbsolutePath);
        await using var db = factory.CreateDbContext();
        var userA = await db.Users.SingleAsync(x => x.Email == emailA); var userB = await db.Users.SingleAsync(x => x.Email == emailB);
        var service = new CvReviewService(factory, new CvTextExtractor(new()), CvRuntimeReadinessTests.Adapter("comparison"), 5 * 1024 * 1024);
        var data = CvAcceptanceFixtures.Docx(CvAcceptanceFixtures.Vietnamese);
        const string jd = "Tuyển developer C# <script>alert('test')</script>";
        var review = await service.UploadAsync(userA.Id, new MemoryStream(data), "http-smoke.docx", CvRuntimeReadinessTests.DocxMime, data.Length, jobDescription: jd);
        Assert.Equal(jd, (await service.GetAsync(userA.Id, review.AnalysisId))!.Result.JobDescription);
        Assert.Equal("Partial", Assert.Single(review.Result.RequirementMatches).Status);
        Assert.Contains("http-smoke.docx", await a.GetStringAsync("/cv-review"));
        for (var reload = 0; reload < 2; reload++)
        {
            var rawHtml = await a.GetStringAsync("/cv-review/" + review.AnalysisId);
            Assert.DoesNotContain("<script>alert('test')</script>", rawHtml);
            var result = WebUtility.HtmlDecode(rawHtml);
            Assert.Contains("id=\"cv-result-heading\"", result); Assert.Contains(review.Result.Summary, result);
            Assert.Contains(jd, result); Assert.Contains("Đối chiếu yêu cầu tuyển dụng", result);
            Assert.Contains("Có bằng chứng một phần", result);
        }
        var denied = WebUtility.HtmlDecode(await b.GetStringAsync("/cv-review/" + review.AnalysisId));
        Assert.DoesNotContain("cv-result-heading", denied); Assert.Contains("Không tìm thấy phân tích", denied);
        Assert.DoesNotContain(jd, denied);
        Assert.Empty(await service.HistoryAsync(userB.Id)); Assert.Null(await service.GetAsync(userB.Id, review.AnalysisId));
        Assert.Null(await service.ReanalyzeAsync(userB.Id, review.DocumentId)); Assert.False(await service.DeleteAsync(userB.Id, review.DocumentId));
        var second = await service.ReanalyzeAsync(userA.Id, review.DocumentId);
        Assert.NotNull(second); Assert.NotEqual(review.AnalysisId, second.AnalysisId);
        Assert.Equal(jd, second.Result.JobDescription);
        Assert.Contains("cv-result-heading", await a.GetStringAsync("/cv-review/" + second.AnalysisId));
        Assert.True(await service.DeleteAsync(userA.Id, review.DocumentId));
        Assert.DoesNotContain("cv-result-heading", await a.GetStringAsync("/cv-review/" + review.AnalysisId));
        Assert.DoesNotContain("http-smoke.docx", await a.GetStringAsync("/cv-review"));
        a.DefaultRequestHeaders.Add("Cookie", "ss-lang=en");
        for (var reload = 0; reload < 2; reload++)
        {
            var english = WebUtility.HtmlDecode(await a.GetStringAsync("/cv-review"));
            Assert.Contains("<html lang=\"en\"", english);
            Assert.Contains("Choose CV", english);
            Assert.Contains("Job description / hiring requirements", english);
            Assert.DoesNotContain("Chọn CV", english);
            Assert.DoesNotContain("Tải CV để phân tích", english);
        }
        var coursePage = WebUtility.HtmlDecode(await a.GetStringAsync("/courses"));
        Assert.Contains("YOUR LEARNING LIBRARY", coursePage);
        Assert.Contains("id=\"course-search\"", coursePage);
        Assert.DoesNotContain("<video", coursePage);
        Assert.DoesNotContain("Khóa học · SoftSync", coursePage);
        var quizHistory = WebUtility.HtmlDecode(await a.GetStringAsync("/game/history"));
        Assert.Contains("Attempt history", quizHistory);
        Assert.DoesNotContain("Lịch sử làm bài", quizHistory);
        a.DefaultRequestHeaders.Remove("Cookie");
        a.DefaultRequestHeaders.Add("Cookie", "ss-lang=vi");
        Assert.Contains("Chọn CV", WebUtility.HtmlDecode(await a.GetStringAsync("/cv-review")));
    }
    internal static HttpClient Client(Uri origin) => new(new HttpClientHandler { CookieContainer = new(), AllowAutoRedirect = true }) { BaseAddress = origin };
    internal static Task Register(HttpClient client, string email, string password) => PostForm(client, "/Account/Register", new()
    { ["Input.FullName"] = "Synthetic CV acceptance user", ["Input.Email"] = email, ["Input.Password"] = password, ["Input.ConfirmPassword"] = password });
    internal static Task Login(HttpClient client, string email, string password) => PostForm(client, "/Account/Login", new()
    { ["Input.Identifier"] = email, ["Input.Password"] = password, ["Input.RememberMe"] = "false" });
    private static async Task PostForm(HttpClient client, string route, Dictionary<string, string> fields)
    {
        var html = await client.GetStringAsync(route);
        foreach (Match match in Regex.Matches(html, "<input[^>]+>"))
        {
            var tag = match.Value;
            if (!tag.Contains("type=\"hidden\"")) continue;
            var name = Regex.Match(tag, "name=\"([^\"]+)\""); var value = Regex.Match(tag, "value=\"([^\"]*)\"");
            if (name.Success) fields[name.Groups[1].Value] = WebUtility.HtmlDecode(value.Groups[1].Value);
        }
        using var response = await client.PostAsync(route, new FormUrlEncodedContent(fields)); response.EnsureSuccessStatusCode();
        Assert.NotEqual(route, response.RequestMessage!.RequestUri!.AbsolutePath);
    }
}
