using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Services;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using Xunit;

namespace SoftSync.Tests;

public sealed class CvWorkspacePersistenceTests
{
    private static DbContextOptions<SoftSyncDbContext> Options() =>
        new DbContextOptionsBuilder<SoftSyncDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    [Fact]
    public void PostgreSql_migration_creates_the_persistent_tables_and_matches_the_model()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        using var db = new SoftSyncDbContext(new DbContextOptionsBuilder<SoftSyncDbContext>()
            .UseNpgsql("Host=localhost;Database=design_only;Username=design;Password=design").Options);
        var migrator = Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>(db);
        var sql = migrator.GenerateScript("20261018171618_AddAdminAnnouncements", "20261022020333_AddPersistentCvWorkspace");
        Assert.Contains("CREATE TABLE \"CvDocuments\"", sql);
        Assert.Contains("CREATE TABLE \"CvReviews\"", sql);
        Assert.Contains("bytea", sql);
        Assert.Contains("ON DELETE SET NULL", sql);
        Assert.DoesNotContain("DROP TABLE", sql);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    private static async Task Seed(SoftSyncDbContext db)
    {
        db.Users.AddRange(new ApplicationUser { Id = 101, UserName = "cv-owner@test.local" },
            new ApplicationUser { Id = 102, UserName = "other@test.local" });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Original_file_extracted_text_and_review_survive_a_fresh_context()
    {
        var options = Options();
        var reviewId = Guid.NewGuid();
        Guid documentId;
        byte[] original = [37, 80, 68, 70, 45, 49, 46, 55];
        await using (var db = new SoftSyncDbContext(options))
        {
            await Seed(db);
            var service = new CvWorkspaceService(db);
            var doc = await service.AddDocumentAsync(101, "My CV.pdf", "pdf", "Original CV text", original);
            documentId = doc.Id;
            await service.SaveReviewAsync(101, new(reviewId, doc.Id, doc.Name, "Frontend Developer",
                "A detailed job description", "{\"QualityScore\":78}", DateTime.UtcNow));
        }
        await using var fresh = new SoftSyncDbContext(options);
        var restored = new CvWorkspaceService(fresh);
        var document = Assert.Single(await restored.ListDocumentsAsync(101));
        Assert.Equal(documentId, document.Id);
        Assert.Equal("Original CV text", document.Text);
        Assert.Equal(original, (await restored.GetDownloadAsync(101, documentId))!.OriginalFile);
        var review = Assert.Single(await restored.ListReviewsAsync(101));
        Assert.Equal(reviewId, review.Id);
        Assert.Equal("A detailed job description", review.JobDescription);
        Assert.Equal("{\"QualityScore\":78}", review.ResultJson);
    }

    [Fact]
    public async Task Other_accounts_cannot_list_download_delete_or_attach_reviews_to_a_cv()
    {
        await using var db = new SoftSyncDbContext(Options());
        await Seed(db);
        var service = new CvWorkspaceService(db);
        var doc = await service.AddDocumentAsync(101, "private.txt", "txt", "Private CV", [1, 2, 3]);
        var review = new SavedCvReview(Guid.NewGuid(), doc.Id, doc.Name, "Job", "JD", "{}", DateTime.UtcNow);
        await service.SaveReviewAsync(101, review);
        Assert.Empty(await service.ListDocumentsAsync(102));
        Assert.Empty(await service.ListReviewsAsync(102));
        Assert.Null(await service.GetDownloadAsync(102, doc.Id));
        Assert.False(await service.DeleteDocumentAsync(102, doc.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SaveReviewAsync(102, review));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.SaveReviewAsync(102, review with { Id = Guid.NewGuid() }));
        Assert.NotNull(await service.GetDownloadAsync(101, doc.Id));
    }

    [Fact]
    public async Task Deleting_a_cv_preserves_saved_reviews_and_repeated_save_is_idempotent()
    {
        var options = Options();
        await using (var db = new SoftSyncDbContext(options))
        {
            await Seed(db);
            var service = new CvWorkspaceService(db);
            var doc = await service.AddDocumentAsync(101, "cv.txt", "txt", "CV content", [1]);
            var review = new SavedCvReview(Guid.NewGuid(), doc.Id, doc.Name, "Job", "JD", "{}", DateTime.UtcNow);
            await service.SaveReviewAsync(101, review);
            await service.SaveReviewAsync(101, review);
            Assert.Single(await service.ListReviewsAsync(101));
            Assert.True(await service.DeleteDocumentAsync(101, doc.Id));
        }
        await using var fresh = new SoftSyncDbContext(options);
        var restored = new CvWorkspaceService(fresh);
        Assert.Empty(await restored.ListDocumentsAsync(101));
        var saved = Assert.Single(await restored.ListReviewsAsync(101));
        Assert.Null(saved.DocumentId);
        Assert.Equal("{}", saved.ResultJson);
    }

    [Fact]
    public async Task Invalid_uploads_do_not_create_partial_records()
    {
        await using var db = new SoftSyncDbContext(Options());
        await Seed(db);
        var service = new CvWorkspaceService(db);
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddDocumentAsync(101, "file.exe", "exe", "CV", [1]));
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddDocumentAsync(101, "file.txt", "txt", "", [1]));
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddDocumentAsync(101, "file.pdf", "pdf", "CV", []));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AddDocumentAsync(0, "cv.txt", "txt", "CV", [1]));
        Assert.Empty(await service.ListDocumentsAsync(101));
    }
}
