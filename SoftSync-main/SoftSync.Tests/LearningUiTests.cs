using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftSync.Common.Dtos;
using SoftSync.BLL.Services;
using SoftSync.Common.Enums;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;
using SoftSync.Presentation.Components.Shared;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class LearningUiTests
{
    private static async Task<string> Render<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        await using var services = new ServiceCollection().AddLogging().AddSingleton(new LocalizationService(AppLanguage.En)).BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }

    [Fact]
    public async Task Page_heading_is_semantic_and_encodes_user_content()
    {
        var html = await Render<PageHeader>(new() { ["Title"] = "<script>unsafe</script>", ["HeadingId"] = "learning-title" });
        Assert.Contains("<h1 id=\"learning-title\">", html);
        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Save_button_reports_real_busy_state_and_disables_actions(bool busy, bool disabled)
    {
        var html = await Render<SaveButton>(new() { ["Busy"] = busy, ["Disabled"] = disabled });
        Assert.Contains("type=\"button\"",html);
        Assert.Contains($"aria-busy=\"{busy.ToString().ToLowerInvariant()}\"",html);
        if(busy || disabled) Assert.Contains("disabled",html);
        else Assert.DoesNotContain("disabled",html);
        if(busy) { Assert.Contains("Saving",html); Assert.Contains("aria-hidden=\"true\"",html); }
        else { Assert.Contains("Save",html); Assert.DoesNotContain("spinner-border",html); }
        Assert.DoesNotContain("Saved",html); // Completion is only announced by the service outcome, never by this button.
    }

    [Fact]
    public async Task Empty_state_announces_status_and_has_real_navigation()
    {
        var html = await Render<EmptyState>(new() { ["Title"] = "No courses", ["ActionHref"] = "/courses", ["ActionLabel"] = "Courses" });
        Assert.Contains("role=\"status\"", html);
        Assert.Contains("href=\"/courses\"", html);
        Assert.Contains("<h2>", html);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Lesson_media_retains_native_controls_and_readable_transcript(bool captions)
    {
        var html = await Render<LessonMedia>(new() { ["Lesson"] = new CourseLessonDto
        { Id = 1, Title = "Listening", VideoUrl = "/example.webm", CaptionUrl = captions ? "/example.vtt" : "", Transcript = "<script>example</script>\nSecond line" } });
        Assert.Contains("controls", html);
        Assert.Contains("preload=\"metadata\"", html);
        Assert.Contains("<details>", html);
        Assert.Contains("<summary>", html);
        Assert.Contains("&lt;script&gt;", html);
        Assert.DoesNotContain("<script>", html);
        if(captions) Assert.Contains("kind=\"captions\"", html);
        else Assert.Contains("Captions are not available", html);
    }

    [CvHttpAcceptanceFact]
    public async Task Discovery_does_not_embed_videos_and_lesson_routes_show_only_selected_published_content()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var origin = new Uri(Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_URL")!);
        using var client = CvHttpAcceptanceTests.Client(origin);
        var suffix = Guid.NewGuid().ToString("N"); var email = "learning-ui-" + suffix + "@example.invalid"; var password = "Synthetic1!" + suffix;
        await CvHttpAcceptanceTests.Register(client, email, password);
        await CvHttpAcceptanceTests.Login(client, email, password);
        await using var db = new SoftSyncDbContext(new DbContextOptionsBuilder<SoftSyncDbContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("SOFTSYNC_CV_ACCEPTANCE_DB")).Options);
        var owner = await db.Users.SingleAsync(x=>x.Email==email);
        var first = new CourseLesson { Title = "First lesson", Order = 1, VideoUrl = "/first-"+suffix+".mp4", CaptionUrl = "/caption.vtt", Transcript = "First transcript" };
        var second = new CourseLesson { Title = "Second lesson", Order = 2, VideoUrl = "/second-"+suffix+".mp4", Transcript = "Second transcript" };
        var course = new Course { Title = "Published " + suffix, CreatorUserId = owner.Id, SkillId = 1, Status = CourseStatus.Published, Lessons = [first,second] };
        var draft = new Course { Title = "Private draft " + suffix, CreatorUserId = owner.Id, SkillId = 1, Status = CourseStatus.Draft };
        db.Courses.AddRange(course,draft); await db.SaveChangesAsync();
        var discovery = await client.GetStringAsync("/courses");
        Assert.Contains(course.Title, discovery);
        Assert.DoesNotContain(draft.Title, discovery);
        Assert.DoesNotContain("<video", discovery);
        var detail = await client.GetStringAsync($"/courses/{course.Id}");
        Assert.Contains($"/courses/{course.Id}/lessons/{first.Id}", detail);
        Assert.DoesNotContain("<video", detail);
        var lesson = await client.GetStringAsync($"/courses/{course.Id}/lessons/{first.Id}");
        Assert.Contains(first.VideoUrl, lesson);
        Assert.DoesNotContain(second.VideoUrl, lesson);
        Assert.Contains("First transcript", lesson);
        Assert.Contains("kind=\"captions\"", lesson);
        Assert.DoesNotContain("<video", await client.GetStringAsync($"/courses/{draft.Id}/lessons/{first.Id}"));
        Assert.DoesNotContain("<video", await client.GetStringAsync($"/courses/{course.Id}/lessons/{int.MaxValue}"));
        using var learnerDenied = await client.GetAsync("/teacher/courses");
        Assert.Equal("/Account/AccessDenied", learnerDenied.RequestMessage!.RequestUri!.AbsolutePath);
        var teacherRole=await db.Roles.SingleAsync(x=>x.Name=="Teacher");
        db.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<int>{UserId=owner.Id,RoleId=teacherRole.Id});
        await db.SaveChangesAsync();
        using var teacher=CvHttpAcceptanceTests.Client(origin);
        await CvHttpAcceptanceTests.Login(teacher,email,password);
        teacher.DefaultRequestHeaders.Add("Cookie","ss-lang=en");
        var studio=WebUtility.HtmlDecode(await teacher.GetStringAsync("/teacher/courses"));
        Assert.Contains("Teacher Studio",studio);
        Assert.Contains("1. Details",studio);
        Assert.Contains("2. Lessons",studio);
        Assert.Contains("3. Final quiz",studio);
        Assert.Contains("id=\"course-skill\"",studio);
        Assert.DoesNotContain("type=\"number\"",studio);
        var ownQuiz = new SkillChallenge { TeacherId = owner.Id, SkillId = 1, Title = "Owned standalone " + suffix, Status = CourseStatus.Draft };
        var foreignOwner = new ApplicationUser { UserName = "other-"+suffix, NormalizedUserName = "OTHER-"+suffix };
        db.Users.Add(foreignOwner); await db.SaveChangesAsync();
        var foreignQuiz = new SkillChallenge { TeacherId = foreignOwner.Id, SkillId = 1, Title = "Private quiz " + suffix, Status = CourseStatus.Draft };
        db.SkillChallenges.AddRange(ownQuiz, foreignQuiz); await db.SaveChangesAsync();
        var ownEditor = WebUtility.HtmlDecode(await teacher.GetStringAsync($"/teacher/courses?managedQuizId={ownQuiz.Id}"));
        Assert.Contains(ownQuiz.Title, ownEditor);
        Assert.Contains("id=\"question-text\"", ownEditor);
        var deniedEditor = WebUtility.HtmlDecode(await teacher.GetStringAsync($"/teacher/courses?managedQuizId={foreignQuiz.Id}"));
        Assert.DoesNotContain(foreignQuiz.Title, deniedEditor);
        Assert.Contains("do not have permission", deniedEditor);
        ownQuiz.Questions = Enumerable.Range(1, 19).Select(index => new ChallengeQuestion {
            Order = index, QuestionText = $"Question {index}",
            Options = Enumerable.Range(1, 4).Select(option => new ChallengeOption { Order = option, Text = $"Option {option}", IsCorrect = option == 1 }).ToList()
        }).ToList();
        ownQuiz.Status = CourseStatus.Published;
        await db.SaveChangesAsync();
        var gamePage = WebUtility.HtmlDecode(await teacher.GetStringAsync($"/game?quizId={ownQuiz.Id}"));
        Assert.Contains("Question 1 of 19", gamePage);
        Assert.Contains("aria-valuemax=\"19\"", gamePage);
        var variableAttempt = await db.QuizAttempts.SingleAsync(x => x.UserId == owner.Id && x.QuizId == ownQuiz.Id);
        Assert.Equal(19, variableAttempt.TotalQuestions);
        var quizService = new ChallengeService(new CourseRepository(db));
        var result = await quizService.SubmitAsync(variableAttempt.Id, owner.Id, ownQuiz.Questions.OrderBy(x => x.Order).Select((question, index) => new StudentQuizAnswerDto {
            QuestionId = question.Id, SelectedOptionId = question.Options.Single(x => x.Order == (index < 10 ? 1 : 2)).Id
        }).ToList());
        Assert.NotNull(result);
        Assert.Equal(19, result.TotalQuestions);
        Assert.Equal(52.63m, result.ScorePercentage);
        Assert.Equal(QuizAttemptResult.Pass, result.Result);
        var savedAttemptId = variableAttempt.Id;
        db.ChangeTracker.Clear();
        var stored = await quizService.GetAttemptAsync(savedAttemptId, owner.Id);
        Assert.Equal(result.ScorePercentage, stored!.ScorePercentage);
        Assert.Equal(19, await db.QuizAttemptAnswers.CountAsync(x => x.AttemptId == savedAttemptId));
        Assert.Null(await quizService.GetAttemptAsync(savedAttemptId, foreignOwner.Id));
        using var adminDenied=await teacher.GetAsync("/admin/ai-configuration");
        Assert.Equal("/Account/AccessDenied",adminDenied.RequestMessage!.RequestUri!.AbsolutePath);
        using var anonymous = CvHttpAcceptanceTests.Client(origin);
        using var redirect = await anonymous.GetAsync($"/courses/{course.Id}/lessons/{first.Id}");
        Assert.Equal("/Account/Login", redirect.RequestMessage!.RequestUri!.AbsolutePath);
    }
}
