using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Entities;
using SoftSync.Common.Enums;

namespace SoftSync.DAL.Data;

public class SoftSyncDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public SoftSyncDbContext(DbContextOptions<SoftSyncDbContext> options) : base(options) { }

    // ApplicationUser is exposed by the Identity base type as `Users`.
    public DbSet<VerificationCode> VerificationCodes { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<UserSkillSelection> UserSkillSelections { get; set; }
    public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }
    public DbSet<AssessmentOption> AssessmentOptions { get; set; }
    public DbSet<AssessmentResult> AssessmentResults { get; set; }
    public DbSet<RoadmapItem> RoadmapItems { get; set; }
    public DbSet<RoleplaySession> RoleplaySessions { get; set; }
    public DbSet<RoleplayTurn> RoleplayTurns { get; set; }
    public DbSet<CaseStudy> CaseStudies { get; set; }
    public DbSet<CaseStudyOption> CaseStudyOptions { get; set; }
    public DbSet<ProgressLog> ProgressLogs { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<Mentor> Mentors { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseLesson> CourseLessons { get; set; }
    public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
    public DbSet<CourseLessonProgress> CourseLessonProgress { get; set; }
    public DbSet<SkillChallenge> SkillChallenges { get; set; }
    public DbSet<ChallengeQuestion> ChallengeQuestions { get; set; }
    public DbSet<ChallengeOption> ChallengeOptions { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-Many: User <-> Skill (Selections)
        modelBuilder.Entity<UserSkillSelection>()
            .HasKey(us => new { us.UserId, us.SkillId });

        modelBuilder.Entity<UserSkillSelection>()
            .HasOne(us => us.User)
            .WithMany(u => u.SkillSelections)
            .HasForeignKey(us => us.UserId);

        modelBuilder.Entity<UserSkillSelection>()
            .HasOne(us => us.Skill)
            .WithMany()
            .HasForeignKey(us => us.SkillId);

        modelBuilder.Entity<RoleplaySession>()
            .HasMany(session => session.Turns)
            .WithOne(turn => turn.Session)
            .HasForeignKey(turn => turn.RoleplaySessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoleplayTurn>()
            .HasIndex(turn => new { turn.RoleplaySessionId, turn.Sequence })
            .IsUnique();

        modelBuilder.Entity<CourseLesson>().HasIndex(x => new { x.CourseId, x.Order }).IsUnique();
        modelBuilder.Entity<Course>().HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatorUserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CourseEnrollment>().HasIndex(x => new { x.CourseId, x.UserId }).IsUnique();
        modelBuilder.Entity<CourseLessonProgress>().HasIndex(x => new { x.EnrollmentId, x.LessonId }).IsUnique();
        modelBuilder.Entity<ChallengeQuestion>().HasIndex(x => new { x.QuizId, x.Order }).IsUnique();
        modelBuilder.Entity<ChallengeOption>().HasIndex(x => new { x.QuestionId, x.Order }).IsUnique();
        modelBuilder.Entity<QuizAttempt>().HasIndex(x => new { x.UserId, x.QuizId, x.CompletedAtUtc });
        modelBuilder.Entity<QuizAttempt>().Property(x => x.ScorePercentage).HasPrecision(5, 2);
        modelBuilder.Entity<QuizAttemptAnswer>().HasIndex(x => new { x.AttemptId, x.QuestionId }).IsUnique();
        modelBuilder.Entity<QuizAttemptAnswer>().HasOne(x => x.SelectedOption).WithMany().HasForeignKey(x => x.SelectedOptionId).OnDelete(DeleteBehavior.Restrict);

        // Seed Data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // 1. Skills
        modelBuilder.Entity<Skill>().HasData(
            new Skill { Id = 1, Name = "Communication", Description = "Effective verbal and non-verbal interaction.", IconName = "bi-chat-dots" },
            new Skill { Id = 2, Name = "Teamwork", Description = "Collaborating effectively with others.", IconName = "bi-people" },
            new Skill { Id = 3, Name = "Time Management", Description = "Organizing and planning your time.", IconName = "bi-clock" },
            new Skill { Id = 4, Name = "Critical Thinking", Description = "Analyzing info to make judgments.", IconName = "bi-lightbulb" },
            new Skill { Id = 5, Name = "Problem Solving", Description = "Finding solutions to complex issues.", IconName = "bi-tools" },
            new Skill { Id = 6, Name = "Emotional Management", Description = "Recognizing and managing emotions.", IconName = "bi-heart" },
            new Skill { Id = 7, Name = "Adaptability", Description = "Adjusting to new conditions.", IconName = "bi-arrow-repeat" }
        );

        // 2. Assessment Questions — 8 per skill (all 7 skills), defined in QuizSeedData.
        modelBuilder.Entity<AssessmentQuestion>().HasData(QuizSeedData.Questions());
        modelBuilder.Entity<AssessmentOption>().HasData(QuizSeedData.Options());

        // 3. Case Studies
        modelBuilder.Entity<CaseStudy>().HasData(
            new CaseStudy { Id = 1, Title = "Group Communication", Scenario = "Your team member is not contributing. What do you do?", SkillId = 1 },
            new CaseStudy { Id = 2, Title = "Missed Deadline", Scenario = "You realize you will miss a deadline tomorrow. What is your first action?", SkillId = 3 }
        );

        modelBuilder.Entity<CaseStudyOption>().HasData(
            new CaseStudyOption { Id = 1, CaseStudyId = 1, OptionText = "Do their work myself.", IsRecommended = false, Feedback = "This leads to burnout and doesn't solve the team dynamic issue." },
            new CaseStudyOption { Id = 2, CaseStudyId = 1, OptionText = "Talk to them privately to understand their situation.", IsRecommended = true, Feedback = "Direct, empathetic communication is key." },
            new CaseStudyOption { Id = 3, CaseStudyId = 2, OptionText = "Work all night and hope for the best.", IsRecommended = false, Feedback = "Risky and doesn't manage expectations." },
            new CaseStudyOption { Id = 4, CaseStudyId = 2, OptionText = "Inform the stakeholders immediately and propose a new timeline.", IsRecommended = true, Feedback = "Transparency and proactive planning are essential." }
        );

        // 4. Demo user is seeded at runtime via UserManager (see DbInitializer),
        //    because an Identity user needs a real PasswordHash/SecurityStamp and
        //    HasData disallows dynamic values like DateTime.UtcNow.

        // 5. Mentors
        modelBuilder.Entity<Mentor>().HasData(
            new Mentor { Id = 1, Name = "Dr. Katherine", Expertise = "Leadership & Soft Skills", ShortBio = "15 years of experience in corporate training.", AvatarUrl = "https://i.pravatar.cc/150?u=katherine" },
            new Mentor { Id = 2, Name = "John Smith", Expertise = "Problem Solving", ShortBio = "Former Google engineer, expert in algorithms and thinking.", AvatarUrl = "https://i.pravatar.cc/150?u=john" }
        );
    }
}
