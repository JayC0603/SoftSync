using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Services;
using SoftSync.Common.Dtos;
using SoftSync.DAL.Data;
using SoftSync.DAL.Repositories;
using Xunit;

namespace SoftSync.Tests;

public sealed class MentorDirectoryTests
{
    [Fact]
    public async Task Directory_profiles_support_requests_for_every_published_mentor()
    {
        var options = new DbContextOptionsBuilder<SoftSyncDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new SoftSyncDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var service = new MentorService(new MentorRepository(db), new MentorSupportRequestRepository(db));

        var directory = (await service.GetAllAsync()).ToList();
        Assert.Equal(3, directory.Count);
        Assert.DoesNotContain(directory, mentor => mentor.AvatarUrl.Contains("pravatar"));
        foreach (var mentor in directory)
        {
            var profile = await service.GetByIdAsync(mentor.Id);
            Assert.NotNull(profile);
            Assert.Equal(mentor.Name, profile.Name);
            Assert.True(await service.SubmitSupportRequestAsync(10, new MentorSupportRequestDto
            {
                MentorId = mentor.Id,
                Goal = "Cải thiện giao tiếp",
                Context = "Tôi cần chuẩn bị một bài thuyết trình trước nhóm học tập.",
                PreferredContact = "Email"
            }));
        }
        Assert.Equal(3, await db.MentorSupportRequests.CountAsync());
    }
}
