using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Goal = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ExperiencePoints = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CurrentLevel = table.Column<int>(type: "integer", nullable: false),
                    DailyStudyMinutes = table.Column<int>(type: "integer", nullable: false),
                    StudyDaysPerWeek = table.Column<int>(type: "integer", nullable: false),
                    PreferredStudyTime = table.Column<int>(type: "integer", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Theme = table.Column<int>(type: "integer", nullable: false),
                    ReduceMotion = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mentors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Expertise = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: false),
                    ShortBio = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IconName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Sender = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WeekNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapItems_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificationCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    CodeHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    Consumed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationCodes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionTextVi = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentQuestions_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentResults_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentResults_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseStudies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Scenario = table.Column<string>(type: "text", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseStudies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseStudies_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    PercentComplete = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressLogs_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSkillSelections",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkillSelections", x => new { x.UserId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_UserSkillSelections_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSkillSelections_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    OptionText = table.Column<string>(type: "text", nullable: false),
                    OptionTextVi = table.Column<string>(type: "text", nullable: false),
                    ScoreValue = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentOptions_AssessmentQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "AssessmentQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseStudyOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CaseStudyId = table.Column<int>(type: "integer", nullable: false),
                    OptionText = table.Column<string>(type: "text", nullable: false),
                    IsRecommended = table.Column<bool>(type: "boolean", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseStudyOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseStudyOptions_CaseStudies_CaseStudyId",
                        column: x => x.CaseStudyId,
                        principalTable: "CaseStudies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Mentors",
                columns: new[] { "Id", "AvatarUrl", "Expertise", "Name", "ShortBio" },
                values: new object[,]
                {
                    { 1, "https://i.pravatar.cc/150?u=katherine", "Leadership & Soft Skills", "Dr. Katherine", "15 years of experience in corporate training." },
                    { 2, "https://i.pravatar.cc/150?u=john", "Problem Solving", "John Smith", "Former Google engineer, expert in algorithms and thinking." }
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Description", "IconName", "Name" },
                values: new object[,]
                {
                    { 1, "Effective verbal and non-verbal interaction.", "bi-chat-dots", "Communication" },
                    { 2, "Collaborating effectively with others.", "bi-people", "Teamwork" },
                    { 3, "Organizing and planning your time.", "bi-clock", "Time Management" },
                    { 4, "Analyzing info to make judgments.", "bi-lightbulb", "Critical Thinking" },
                    { 5, "Finding solutions to complex issues.", "bi-tools", "Problem Solving" },
                    { 6, "Recognizing and managing emotions.", "bi-heart", "Emotional Management" },
                    { 7, "Adjusting to new conditions.", "bi-arrow-repeat", "Adaptability" }
                });

            migrationBuilder.InsertData(
                table: "AssessmentQuestions",
                columns: new[] { "Id", "QuestionText", "QuestionTextVi", "SkillId", "Type" },
                values: new object[,]
                {
                    { 101, "How do you handle two-way exchange of information?", "Bạn xử lý việc trao đổi thông tin hai chiều thế nào?", 1, 0 },
                    { 102, "What most affects your listening habits?", "Thói quen lắng nghe của bạn bị ảnh hưởng nhiều nhất bởi gì?", 1, 0 },
                    { 103, "How do you deal with differences in wording and terminology?", "Bạn xử lý sự khác biệt về từ ngữ, thuật ngữ thế nào?", 1, 0 },
                    { 104, "What is your texting/email style like?", "Phong cách viết tin nhắn/email của bạn thế nào?", 1, 0 },
                    { 105, "When criticized in a conversation, what is your natural reaction?", "Khi bị chỉ trích trong hội thoại, phản ứng tự nhiên của bạn là gì?", 1, 1 },
                    { 106, "How do you adapt your communication style to context?", "Bạn thích ứng phong cách giao tiếp theo bối cảnh thế nào?", 1, 0 },
                    { 107, "How do you recognize your own direct/indirect style?", "Bạn nhận diện phong cách trực tiếp/gián tiếp của mình ra sao?", 1, 0 },
                    { 108, "How do you control your body language?", "Bạn kiểm soát ngôn ngữ cơ thể của mình thế nào?", 1, 0 },
                    { 201, "A team member isn't contributing to a group project. You...", "Một thành viên không đóng góp cho dự án nhóm. Bạn...", 2, 1 },
                    { 202, "When your idea conflicts with a teammate's, you...", "Khi ý tưởng của bạn xung đột với đồng đội, bạn...", 2, 0 },
                    { 203, "The team hits a setback close to a deadline. You...", "Nhóm gặp trục trặc sát hạn chót. Bạn...", 2, 1 },
                    { 204, "How do you treat credit for a shared success?", "Bạn ứng xử với công lao của một thành công chung thế nào?", 2, 0 },
                    { 205, "A quieter teammate hasn't shared their opinion. You...", "Một đồng đội trầm tính chưa nêu ý kiến. Bạn...", 2, 1 },
                    { 206, "When you receive a task you dislike but the team needs, you...", "Khi nhận việc mình không thích nhưng nhóm cần, bạn...", 2, 0 },
                    { 207, "Two teammates are in conflict and it's slowing work. You...", "Hai đồng đội mâu thuẫn làm chậm công việc. Bạn...", 2, 1 },
                    { 208, "How reliable are you with commitments to the team?", "Bạn giữ cam kết với nhóm đáng tin đến đâu?", 2, 0 },
                    { 301, "How does your out-of-class studying go?", "Việc học ngoài giờ lên lớp của bạn diễn ra thế nào?", 3, 0 },
                    { 302, "When you procrastinate on a big task, what is the deepest reason?", "Khi trì hoãn một việc lớn, lý do sâu xa nhất của bạn là gì?", 3, 0 },
                    { 303, "Do you estimate how long a task takes accurately?", "Bạn ước lượng thời gian làm một việc có chính xác không?", 3, 0 },
                    { 304, "What most affects your focus?", "Yếu tố nào ảnh hưởng đến sự tập trung của bạn nhất?", 3, 0 },
                    { 305, "Do you apply scientific time-management methods?", "Bạn có áp dụng phương pháp quản lý thời gian khoa học không?", 3, 0 },
                    { 306, "What do you base on when deciding what to do first?", "Bạn quyết định làm việc gì trước dựa trên điều gì?", 3, 0 },
                    { 307, "How are your study/work goals written?", "Mục tiêu học tập/công việc của bạn được viết ra như thế nào?", 3, 0 },
                    { 308, "When your plan is broken by an unexpected event, how do you react?", "Khi kế hoạch bị phá vỡ bởi biến cố bất ngờ, bạn phản ứng ra sao?", 3, 1 },
                    { 401, "How well do you separate fact from personal opinion?", "Bạn phân biệt sự thật và ý kiến cá nhân tốt đến đâu?", 4, 0 },
                    { 402, "How do you deal with your own confirmation bias?", "Bạn đối phó với thiên kiến xác nhận của bản thân thế nào?", 4, 0 },
                    { 403, "In a group debate, how do you usually react?", "Khi tranh luận nhóm, bạn thường phản ứng ra sao?", 4, 1 },
                    { 404, "How do you verify an unfamiliar source online?", "Bạn kiểm chứng một nguồn tin lạ trên mạng ra sao?", 4, 0 },
                    { 405, "Facing a complex problem, how do you find a solution?", "Khi gặp vấn đề phức tạp, bạn tìm giải pháp thế nào?", 4, 1 },
                    { 406, "Are you willing to change your view when new evidence appears?", "Bạn có sẵn sàng thay đổi quan điểm khi có bằng chứng mới không?", 4, 0 },
                    { 407, "How do you assess the reliability of a research document?", "Bạn đánh giá độ tin cậy một tài liệu nghiên cứu ra sao?", 4, 0 },
                    { 408, "When facing a problem, do you prepare multiple options?", "Khi gặp vấn đề, bạn có chuẩn bị nhiều phương án không?", 4, 0 },
                    { 501, "You hit an unfamiliar problem with no obvious solution. You...", "Bạn gặp một vấn đề lạ không có giải pháp rõ ràng. Bạn...", 5, 1 },
                    { 502, "When a solution fails, you...", "Khi một giải pháp thất bại, bạn...", 5, 0 },
                    { 503, "A problem is too big to tackle at once. You...", "Một vấn đề quá lớn để xử lý một lần. Bạn...", 5, 1 },
                    { 504, "How do you generate solution ideas?", "Bạn tạo ra các ý tưởng giải pháp thế nào?", 5, 0 },
                    { 505, "You lack information to solve a problem. You...", "Bạn thiếu thông tin để giải quyết vấn đề. Bạn...", 5, 1 },
                    { 506, "How do you validate that a solution actually works?", "Bạn kiểm chứng một giải pháp thực sự hiệu quả thế nào?", 5, 0 },
                    { 507, "Your solution works but is inefficient. You...", "Giải pháp của bạn chạy được nhưng chưa tối ưu. Bạn...", 5, 1 },
                    { 508, "When stuck, you...", "Khi bế tắc, bạn...", 5, 0 },
                    { 601, "You receive harsh criticism on your work. You...", "Bạn nhận lời chỉ trích gay gắt về công việc của mình. Bạn...", 6, 1 },
                    { 602, "When you feel angry at work, you...", "Khi tức giận trong công việc, bạn...", 6, 0 },
                    { 603, "A stressful deadline is making you anxious. You...", "Một hạn chót căng thẳng khiến bạn lo lắng. Bạn...", 6, 1 },
                    { 604, "How aware are you of your emotions as they happen?", "Bạn nhận biết cảm xúc của mình ngay khi nó xảy ra đến đâu?", 6, 0 },
                    { 605, "A colleague is visibly upset. You...", "Một đồng nghiệp rõ ràng đang buồn bực. Bạn...", 6, 1 },
                    { 606, "After a setback, how do you recover?", "Sau một thất bại, bạn hồi phục thế nào?", 6, 0 },
                    { 607, "You're frustrated in a meeting. You...", "Bạn bực bội trong một cuộc họp. Bạn...", 6, 1 },
                    { 608, "How do you handle others' strong emotions?", "Bạn xử lý cảm xúc mạnh của người khác thế nào?", 6, 0 },
                    { 701, "Your project's requirements change suddenly. You...", "Yêu cầu của dự án thay đổi đột ngột. Bạn...", 7, 1 },
                    { 702, "How do you feel about learning new tools or methods?", "Bạn cảm thấy thế nào về việc học công cụ hay phương pháp mới?", 7, 0 },
                    { 703, "You're moved to an unfamiliar team overnight. You...", "Bạn bị chuyển sang một nhóm xa lạ chỉ sau một đêm. Bạn...", 7, 1 },
                    { 704, "When plans fall apart, your mindset is...", "Khi kế hoạch đổ vỡ, tâm thế của bạn là...", 7, 0 },
                    { 705, "A tool you rely on is discontinued. You...", "Một công cụ bạn phụ thuộc bị ngừng hỗ trợ. Bạn...", 7, 1 },
                    { 706, "How do you respond to feedback that you should change how you work?", "Bạn phản hồi thế nào khi được góp ý nên thay đổi cách làm việc?", 7, 0 },
                    { 707, "Priorities shift for the third time this week. You...", "Ưu tiên thay đổi lần thứ ba trong tuần. Bạn...", 7, 1 },
                    { 708, "In an uncertain, ambiguous situation you...", "Trong một tình huống mơ hồ, bất định, bạn...", 7, 0 }
                });

            migrationBuilder.InsertData(
                table: "CaseStudies",
                columns: new[] { "Id", "Scenario", "SkillId", "Title" },
                values: new object[,]
                {
                    { 1, "Your team member is not contributing. What do you do?", 1, "Group Communication" },
                    { 2, "You realize you will miss a deadline tomorrow. What is your first action?", 3, "Missed Deadline" }
                });

            migrationBuilder.InsertData(
                table: "AssessmentOptions",
                columns: new[] { "Id", "OptionText", "OptionTextVi", "QuestionId", "ScoreValue" },
                values: new object[,]
                {
                    { 1011, "I focus on saying everything on my mind, paying little attention to the listener's reaction.", "Chỉ tập trung nói hết ý mình, ít để ý phản ứng người nghe", 101, 1 },
                    { 1012, "I pay attention but am reluctant to ask directly whether it's clear.", "Có để ý nhưng ngại hỏi thẳng \"có rõ không\"", 101, 2 },
                    { 1013, "I only check when speaking in person, and skip it in texts/emails.", "Chỉ kiểm tra khi nói trực tiếp, bỏ qua khi nhắn tin/email", 101, 3 },
                    { 1014, "I always watch the listener's cues and proactively ask for feedback.", "Luôn quan sát tín hiệu người nghe và chủ động hỏi phản hồi", 101, 4 },
                    { 1021, "I'm busy preparing a rebuttal in my head and don't hear it all.", "Bận chuẩn bị phản biện trong đầu, không nghe hết", 102, 1 },
                    { 1022, "I easily lose interest if I don't click with the speaker.", "Dễ mất hứng nếu không hợp gu với người nói", 102, 2 },
                    { 1023, "I listen while doing other things, thinking I multitask well.", "Vừa nghe vừa làm việc riêng, nghĩ mình đa nhiệm tốt", 102, 3 },
                    { 1024, "I listen actively and paraphrase back to confirm I understood.", "Lắng nghe chủ động, diễn đạt lại để xác nhận hiểu đúng", 102, 4 },
                    { 1031, "I use jargon/slang and assume others just understand.", "Dùng jargon/từ lóng, mặc định người khác tự hiểu", 103, 1 },
                    { 1032, "I sometimes misunderstand because of regional words or abbreviations.", "Thỉnh thoảng hiểu lầm vì từ địa phương, viết tắt", 103, 2 },
                    { 1033, "I know the differences but am reluctant to re-explain hard terms.", "Biết khác biệt nhưng ngại giải thích lại từ khó", 103, 3 },
                    { 1034, "I proactively choose plain words, define clearly, and give examples.", "Chủ động chọn từ dễ hiểu, định nghĩa rõ, có ví dụ", 103, 4 },
                    { 1041, "I WRITE IN CAPS when urgent and use abbreviations even with superiors.", "Viết HOA khi khẩn cấp, dùng viết tắt cả với cấp trên", 104, 1 },
                    { 1042, "I reply right away when angry, which easily leads to arguments.", "Phản hồi ngay khi tức giận, dễ dẫn đến tranh cãi", 104, 2 },
                    { 1043, "I write carelessly — no subject line, no proofreading.", "Viết tùy tiện, không tiêu đề, không kiểm tra lỗi", 104, 3 },
                    { 1044, "I'm always polite, grammatical, and check the tone before sending.", "Luôn lịch sự, đúng ngữ pháp, kiểm tra tông giọng trước khi gửi", 104, 4 },
                    { 1051, "I get defensive and attack the person back.", "Phản ứng phòng vệ, công kích cá nhân lại", 105, 1 },
                    { 1052, "I go silent, walk away, and avoid the conflict.", "Im lặng, bỏ đi, né tránh xung đột", 105, 2 },
                    { 1053, "I try to listen but feel deeply hurt and lose confidence.", "Cố nghe nhưng tổn thương sâu, mất tự tin", 105, 3 },
                    { 1054, "I calmly clarify the issue while saving face for the other person.", "Bình tĩnh làm rõ vấn đề, giữ thể diện cho đối phương", 105, 4 },
                    { 1061, "I speak exactly the same way to everyone.", "Nói chuyện y hệt nhau với mọi đối tượng", 106, 1 },
                    { 1062, "I try to change but fumble choosing the right channel.", "Cố thay đổi nhưng lúng túng chọn kênh phù hợp", 106, 2 },
                    { 1063, "I'm easily swayed by bias and stereotypes when communicating.", "Dễ bị định kiến, khuôn mẫu chi phối khi giao tiếp", 106, 3 },
                    { 1064, "I always analyze the context and audience before communicating.", "Luôn phân tích bối cảnh, khán giả trước khi giao tiếp", 106, 4 },
                    { 1071, "I speak bluntly, sometimes hurting others.", "Nói thẳng thô, đôi khi làm tổn thương người khác", 107, 1 },
                    { 1072, "I talk in circles until the listener can't follow my point.", "Nói vòng vo đến mức người nghe không hiểu ý", 107, 2 },
                    { 1073, "I'm confused about when to be direct vs. indirect.", "Bối rối không biết khi nào nên trực tiếp/gián tiếp", 107, 3 },
                    { 1074, "I balance it: direct for work, gentle when delivering bad news.", "Cân bằng: trực tiếp cho công việc, mềm mỏng khi tin xấu", 107, 4 },
                    { 1081, "I can't control it — crossed arms, avoiding eye contact.", "Không kiểm soát được, khoanh tay, né ánh mắt", 108, 1 },
                    { 1082, "I'm awkward about personal space (too close/too far).", "Lúng túng về khoảng cách giao tiếp (quá gần/xa)", 108, 2 },
                    { 1083, "I focus only on words and forget expression and posture.", "Chỉ chú trọng câu chữ, quên biểu cảm và tư thế", 108, 3 },
                    { 1084, "I keep my posture, eye contact, and tone appropriate to the context.", "Luôn giữ tư thế, ánh mắt, tông giọng phù hợp bối cảnh", 108, 4 },
                    { 2011, "Talk to them privately to understand what's going on.", "Nói chuyện riêng để hiểu chuyện gì đang xảy ra.", 201, 4 },
                    { 2012, "Quietly do their share yourself.", "Lặng lẽ tự làm luôn phần của họ.", 201, 2 },
                    { 2013, "Report them to the supervisor immediately.", "Báo ngay với người phụ trách.", 201, 2 },
                    { 2014, "Call them out in front of the group.", "Chỉ trích họ trước cả nhóm.", 201, 1 },
                    { 2021, "Look for a solution that combines the best of both.", "Tìm giải pháp kết hợp điểm tốt của cả hai.", 202, 4 },
                    { 2022, "Push for your idea because you're confident in it.", "Cố bảo vệ ý mình vì tự tin vào nó.", 202, 2 },
                    { 2023, "Give up your idea to avoid friction.", "Bỏ ý mình để tránh va chạm.", 202, 2 },
                    { 2024, "Refuse to work with them.", "Từ chối làm việc với họ.", 202, 1 },
                    { 2031, "Rally the group, re-plan, and share the load.", "Tập hợp nhóm, lên lại kế hoạch và chia sẻ khối lượng việc.", 203, 4 },
                    { 2032, "Focus only on finishing your own part.", "Chỉ tập trung hoàn thành phần của mình.", 203, 2 },
                    { 2033, "Wait for someone else to take charge.", "Chờ người khác đứng ra chịu trách nhiệm.", 203, 2 },
                    { 2034, "Blame whoever caused it.", "Đổ lỗi cho người gây ra chuyện.", 203, 1 },
                    { 2041, "Acknowledge everyone's contribution.", "Ghi nhận đóng góp của mọi người.", 204, 4 },
                    { 2042, "Mention the team if asked.", "Nhắc đến cả nhóm nếu được hỏi.", 204, 3 },
                    { 2043, "Highlight my own part first.", "Đề cao phần của mình trước.", 204, 2 },
                    { 2044, "Take the credit myself.", "Nhận hết công về mình.", 204, 1 },
                    { 2051, "Invite them in and ask what they think.", "Mời họ tham gia và hỏi họ nghĩ gì.", 205, 4 },
                    { 2052, "Assume they agree with the group.", "Cho rằng họ đồng ý với cả nhóm.", 205, 2 },
                    { 2053, "Move on without them.", "Bỏ qua và tiếp tục mà không có họ.", 205, 2 },
                    { 2054, "Decide for them.", "Quyết định thay cho họ.", 205, 1 },
                    { 2061, "Take it on and do it well for the team.", "Nhận và làm tốt vì cả nhóm.", 206, 4 },
                    { 2062, "Do it, but with minimal effort.", "Làm, nhưng chỉ ở mức tối thiểu.", 206, 2 },
                    { 2063, "Try to pass it to someone else.", "Tìm cách đẩy cho người khác.", 206, 2 },
                    { 2064, "Refuse.", "Từ chối.", 206, 1 },
                    { 2071, "Help them talk it through and find common ground.", "Giúp họ trao đổi và tìm điểm chung.", 207, 4 },
                    { 2072, "Pick the side you agree with.", "Chọn phe mình đồng tình.", 207, 2 },
                    { 2073, "Stay out of it entirely.", "Đứng ngoài hoàn toàn.", 207, 2 },
                    { 2074, "Tell everyone to just get over it.", "Bảo mọi người dẹp chuyện đó đi.", 207, 1 },
                    { 2081, "I deliver what I promise, on time, consistently.", "Tôi làm đúng điều đã hứa, đúng hạn, ổn định.", 208, 4 },
                    { 2082, "Usually reliable, occasional slips.", "Thường đáng tin, thỉnh thoảng lỡ hẹn.", 208, 3 },
                    { 2083, "Reliable only when reminded.", "Chỉ đáng tin khi được nhắc.", 208, 2 },
                    { 2084, "Often miss commitments.", "Hay thất hứa.", 208, 1 },
                    { 3011, "It depends on whatever schedule others set for me.", "Phụ thuộc lịch người khác sắp xếp", 301, 1 },
                    { 3012, "I study on a whim, with no schedule.", "Học tùy hứng, không lịch trình", 301, 2 },
                    { 3013, "I have a plan but find it hard to stick to.", "Có kế hoạch nhưng khó giữ đúng", 301, 3 },
                    { 3014, "I'm always proactive, with my own strategy.", "Luôn chủ động, có chiến lược riêng", 301, 4 },
                    { 3021, "Fear of failure, so I avoid getting started.", "Sợ thất bại nên né tránh bắt tay vào làm", 302, 1 },
                    { 3022, "I'm easily distracted by my phone and social media.", "Dễ mất tập trung bởi điện thoại, mạng xã hội", 302, 2 },
                    { 3023, "My body is tired, sleep-deprived, low on energy.", "Cơ thể mệt mỏi, thiếu ngủ, thiếu năng lượng", 302, 3 },
                    { 3024, "I rarely have this problem — I start right away.", "Tôi hiếm khi gặp vấn đề này, bắt tay vào việc ngay", 302, 4 },
                    { 3031, "Way off — a task I thought was 1 hour takes 4–5.", "Sai lệch nặng, việc tưởng 1 giờ hóa ra mất 4-5 giờ", 303, 1 },
                    { 3032, "Hard to estimate for material heavy with figures and charts.", "Khó ước lượng với tài liệu nhiều số liệu, biểu đồ", 303, 2 },
                    { 3033, "I often pull all-nighters near the deadline to make up for it.", "Thường phải thức trắng đêm sát hạn để bù đắp", 303, 3 },
                    { 3034, "Fairly accurate, always with buffer time.", "Ước lượng khá sát, luôn có thời gian dự phòng", 303, 4 },
                    { 3041, "I'm constantly pulled in by notifications, messages, games.", "Liên tục bị cuốn vào thông báo, tin nhắn, game", 304, 1 },
                    { 3042, "I'm easily distracted by noise and a messy space.", "Dễ xao nhãng bởi tiếng ồn, không gian bừa bộn", 304, 2 },
                    { 3043, "I sometimes multitask, which lowers my output.", "Thỉnh thoảng làm nhiều việc cùng lúc, hiệu suất giảm", 304, 3 },
                    { 3044, "I always have a quiet space with all notifications off.", "Luôn có không gian yên tĩnh, tắt hết thông báo", 304, 4 },
                    { 3051, "No, I only get motivated when the deadline is imminent.", "Không, chỉ có động lực khi hạn chót cận kề", 305, 1 },
                    { 3052, "I know some but have never applied one successfully.", "Có biết nhưng chưa áp dụng thành công lần nào", 305, 2 },
                    { 3053, "I apply them but often quit halfway (e.g. Pomodoro).", "Có áp dụng nhưng hay bỏ giữa chừng (vd: Pomodoro)", 305, 3 },
                    { 3054, "I apply them fluently: Eat the Frog, break tasks down, daily Top 3.", "Áp dụng nhuần nhuyễn: Eat the Frog, chia việc nhỏ, Top 3 mỗi ngày", 305, 4 },
                    { 3061, "Momentary emotion or panic over the deadline.", "Cảm xúc nhất thời hoặc hoảng loạn vì hạn chót", 306, 1 },
                    { 3062, "I pick easy tasks first and dodge hard-but-important ones.", "Chọn việc dễ trước, né việc khó nhưng quan trọng", 306, 2 },
                    { 3063, "I make a list but struggle to sort out the core tasks.", "Có lập danh sách nhưng khó phân loại việc cốt lõi", 306, 3 },
                    { 3064, "I use the Eisenhower matrix to classify clearly.", "Dùng ma trận Eisenhower để phân loại rõ ràng", 306, 4 },
                    { 3071, "Very vague, hard to measure (\"get better\").", "Rất mơ hồ, khó đo lường (\"học giỏi hơn\")", 307, 1 },
                    { 3072, "Clear but with no specific deadline.", "Rõ ràng nhưng không gắn thời hạn cụ thể", 307, 2 },
                    { 3073, "Specific but unrealistic, not feasible.", "Cụ thể nhưng thiếu thực tế, không khả thi", 307, 3 },
                    { 3074, "Always fully meet the SMART criteria.", "Luôn đạt chuẩn SMART đầy đủ", 307, 4 },
                    { 3081, "Panic, get discouraged, and drop the work.", "Hoảng loạn, nản chí, bỏ dở công việc", 308, 1 },
                    { 3082, "Stubbornly cling to the old plan, extremely stressed.", "Cố chấp bám kế hoạch cũ, căng thẳng tột độ", 308, 2 },
                    { 3083, "Struggle through it alone, without telling anyone.", "Tự loay hoay giải quyết, không báo với ai", 308, 3 },
                    { 3084, "Calmly reassess, proactively inform others, and adjust.", "Bình tĩnh đánh giá lại, chủ động thông báo và điều chỉnh", 308, 4 },
                    { 4011, "I instantly believe convincing claims on social media.", "Tin ngay vào tuyên bố thuyết phục trên mạng xã hội", 401, 1 },
                    { 4012, "I treat the opinion of someone I like as obvious fact.", "Coi ý kiến người mình yêu thích là sự thật hiển nhiên", 401, 2 },
                    { 4013, "I distinguish well, but struggle with cleverly disguised data.", "Phân biệt tốt nhưng khó với số liệu ngụy trang tinh vi", 401, 3 },
                    { 4014, "I always distinguish clearly and demand empirical evidence.", "Luôn phân biệt rõ, yêu cầu bằng chứng thực nghiệm", 401, 4 },
                    { 4021, "I only read news that agrees with me and think others are wrong.", "Chỉ đọc tin cùng quan điểm, nghĩ người khác sai", 402, 1 },
                    { 4022, "I'm annoyed by opposing news and look to refute it.", "Khó chịu khi đọc tin trái chiều, tìm cách bác bỏ", 402, 2 },
                    { 4023, "I read other views but still cherry-pick what favors me.", "Đọc góc nhìn khác nhưng vẫn chọn lọc có lợi cho mình", 402, 3 },
                    { 4024, "I proactively seek many sources and weigh opposing evidence fairly.", "Chủ động tiếp cận đa nguồn, công tâm với bằng chứng trái chiều", 402, 4 },
                    { 4031, "Attack the other person when I'm challenged.", "Công kích cá nhân đối phương khi bị phản bác", 403, 1 },
                    { 4032, "Follow the majority, avoiding independent thinking.", "A dua theo số đông, tránh tư duy độc lập", 403, 2 },
                    { 4033, "Sometimes use extreme examples to distract.", "Đôi khi dùng ví dụ cực đoan để đánh lạc hướng", 403, 3 },
                    { 4034, "Focus on rational analysis, avoiding logical fallacies.", "Tập trung phân tích lý trí, tránh ngụy biện logic", 403, 4 },
                    { 4041, "Trust a nice-looking site and the author's self-introduction.", "Tin vào giao diện đẹp, lời tự giới thiệu của tác giả", 404, 1 },
                    { 4042, "Trust it based on likes and positive comments below.", "Tin theo lượt thích, bình luận tích cực bên dưới", 404, 2 },
                    { 4043, "I know I should verify but am lazy, only doing it when it matters.", "Biết cần kiểm chứng nhưng lười, chỉ làm khi quan trọng", 404, 3 },
                    { 4044, "Read laterally — open many tabs to cross-check independent sources.", "Đọc ngang — mở nhiều tab đối chiếu nguồn độc lập", 404, 4 },
                    { 4051, "Only fix the surface, without exploring the root cause.", "Chỉ giải quyết phần nổi, không tìm hiểu gốc rễ", 405, 1 },
                    { 4052, "Decide hastily on gut feeling.", "Quyết định vội vàng theo cảm tính", 405, 2 },
                    { 4053, "Get stuck after a few \"why\" questions, easily going off track.", "Bế tắc sau vài câu hỏi \"tại sao\", dễ lạc hướng", 405, 3 },
                    { 4054, "Apply the \"5 Whys\" to find the root cause.", "Áp dụng \"5 câu hỏi Tại sao\" để tìm gốc rễ vấn đề", 405, 4 },
                    { 4061, "Stubborn — I cling to my view even when proven wrong.", "Bảo thủ, bám quan điểm dù bị chứng minh sai", 406, 1 },
                    { 4062, "I change, but by emotion/the crowd, not by evidence.", "Thay đổi nhưng theo cảm xúc/số đông, không phải bằng chứng", 406, 2 },
                    { 4063, "I note the new evidence but delay adjusting.", "Ghi nhận bằng chứng mới nhưng trì hoãn điều chỉnh", 406, 3 },
                    { 4064, "I proactively update my thinking when the data changes.", "Chủ động cập nhật tư duy khi dữ liệu thay đổi", 406, 4 },
                    { 4071, "Pick the first Google result, assuming it's most authoritative.", "Chọn kết quả đầu tiên trên Google, tin là uy tín nhất", 407, 1 },
                    { 4072, "It only needs to be well-written and match what I want to prove.", "Chỉ cần viết hay và trùng khớp với điều mình muốn chứng minh", 407, 2 },
                    { 4073, "I only check the author/domain, ignoring conflicts of interest.", "Chỉ xem tên tác giả/tên miền, bỏ qua xung đột lợi ích", 407, 3 },
                    { 4074, "I review comprehensively: author, recency, funding source, peer review.", "Rà soát toàn diện: tác giả, tính cập nhật, nguồn tài trợ, bình duyệt", 407, 4 },
                    { 4081, "I think of only one solution, helpless when it fails.", "Chỉ nghĩ 1 giải pháp, bất lực khi nó thất bại", 408, 1 },
                    { 4082, "I think about it but give up, seeing it as time-consuming.", "Có nghĩ đến nhưng bỏ cuộc vì thấy tốn thời gian", 408, 2 },
                    { 4083, "I only prepare a backup for big things, improvising the rest.", "Chỉ chuẩn bị dự phòng cho việc lớn, còn lại tùy cơ ứng biến", 408, 3 },
                    { 4084, "I always build multiple options and concrete contingency plans.", "Luôn xây nhiều phương án và kế hoạch dự phòng cụ thể", 408, 4 },
                    { 5011, "Define the problem clearly, then explore options.", "Xác định rõ vấn đề, rồi khám phá các phương án.", 501, 4 },
                    { 5012, "Try the first idea that comes to mind.", "Thử ngay ý tưởng đầu tiên nảy ra.", 501, 2 },
                    { 5013, "Wait for someone else to solve it.", "Chờ người khác giải quyết.", 501, 1 },
                    { 5014, "Avoid it and work on something else.", "Né tránh và làm việc khác.", 501, 1 },
                    { 5021, "Analyze why, then try a different approach.", "Phân tích lý do, rồi thử cách khác.", 502, 4 },
                    { 5022, "Repeat it hoping for a different result.", "Lặp lại và hy vọng kết quả khác.", 502, 2 },
                    { 5023, "Give up on that problem.", "Bỏ cuộc với vấn đề đó.", 502, 1 },
                    { 5024, "Blame external factors.", "Đổ lỗi cho yếu tố bên ngoài.", 502, 1 },
                    { 5031, "Break it into smaller, solvable pieces.", "Chia nhỏ thành các phần có thể giải quyết.", 503, 4 },
                    { 5032, "Attack the whole thing head-on.", "Lao thẳng vào toàn bộ vấn đề.", 503, 2 },
                    { 5033, "Wait until it becomes urgent.", "Chờ đến khi nó trở nên cấp bách.", 503, 2 },
                    { 5034, "Hope it resolves itself.", "Hy vọng nó tự được giải quyết.", 503, 1 },
                    { 5041, "Brainstorm several, then evaluate trade-offs.", "Nghĩ nhiều ý, rồi cân nhắc được–mất.", 504, 4 },
                    { 5042, "Go with the most familiar option.", "Chọn phương án quen thuộc nhất.", 504, 2 },
                    { 5043, "Copy what worked elsewhere without adapting.", "Sao chép cách đã hiệu quả ở nơi khác mà không điều chỉnh.", 504, 2 },
                    { 5044, "Pick the first workable idea.", "Chọn ý đầu tiên khả thi.", 504, 2 },
                    { 5051, "Identify what's missing and go find it.", "Xác định thứ còn thiếu và đi tìm nó.", 505, 4 },
                    { 5052, "Make assumptions and proceed.", "Giả định rồi cứ tiến hành.", 505, 2 },
                    { 5053, "Solve a different, easier problem instead.", "Giải một vấn đề khác dễ hơn.", 505, 2 },
                    { 5054, "Stop until someone hands you the info.", "Dừng lại đến khi có người đưa thông tin.", 505, 1 },
                    { 5061, "Test it against real criteria and edge cases.", "Kiểm thử theo tiêu chí thực và các trường hợp biên.", 506, 4 },
                    { 5062, "Check that it looks right.", "Xem qua thấy có vẻ đúng.", 506, 2 },
                    { 5063, "Assume it works if there's no error.", "Cho là chạy được nếu không báo lỗi.", 506, 2 },
                    { 5064, "I don't validate.", "Tôi không kiểm chứng.", 506, 1 },
                    { 5071, "Ship it, then plan a cleaner improvement.", "Dùng trước, rồi lên kế hoạch cải tiến gọn hơn.", 507, 4 },
                    { 5072, "Leave it as-is forever.", "Để nguyên như vậy mãi.", 507, 2 },
                    { 5073, "Scrap it and restart from zero.", "Bỏ hết và làm lại từ đầu.", 507, 2 },
                    { 5074, "Ignore the inefficiency.", "Phớt lờ sự kém hiệu quả.", 507, 1 },
                    { 5081, "Step back and reframe the problem.", "Lùi lại và nhìn nhận lại vấn đề.", 508, 4 },
                    { 5082, "Keep pushing the same way harder.", "Cố đẩy mạnh theo cùng một cách.", 508, 2 },
                    { 5083, "Take a guess and move on.", "Đoán bừa rồi đi tiếp.", 508, 2 },
                    { 5084, "Abandon the task.", "Bỏ dở nhiệm vụ.", 508, 1 },
                    { 6011, "Take a breath, look for the useful part, and respond calmly.", "Hít thở, tìm phần hữu ích và phản hồi bình tĩnh.", 601, 4 },
                    { 6012, "Feel defensive and argue back.", "Thấy bị công kích và cãi lại.", 601, 2 },
                    { 6013, "Shut down and stop engaging.", "Đóng lại và ngừng tương tác.", 601, 1 },
                    { 6014, "Pretend it didn't bother you but stew on it.", "Giả vờ không bận tâm nhưng ấm ức trong lòng.", 601, 2 },
                    { 6021, "Recognize it, pause, and choose how to respond.", "Nhận ra, dừng lại và chọn cách phản hồi.", 602, 4 },
                    { 6022, "Try to hide it but it leaks out.", "Cố giấu nhưng vẫn lộ ra.", 602, 2 },
                    { 6023, "Express it immediately.", "Bộc lộ ngay lập tức.", 602, 1 },
                    { 6024, "Bottle it up completely.", "Dồn nén hoàn toàn.", 602, 2 },
                    { 6031, "Use a coping routine and focus on the next step.", "Dùng thói quen đối phó và tập trung vào bước kế tiếp.", 603, 4 },
                    { 6032, "Push through while ignoring the stress.", "Cố làm tới trong khi phớt lờ căng thẳng.", 603, 2 },
                    { 6033, "Let the anxiety stall your work.", "Để nỗi lo làm đình trệ công việc.", 603, 1 },
                    { 6034, "Vent to everyone around you.", "Trút bực dọc lên mọi người xung quanh.", 603, 2 },
                    { 6041, "Very — I can name what I feel and why.", "Rất rõ — tôi gọi tên được cảm xúc và lý do.", 604, 4 },
                    { 6042, "Somewhat aware in the moment.", "Nhận biết phần nào trong lúc đó.", 604, 3 },
                    { 6043, "I notice only after the fact.", "Chỉ nhận ra sau khi mọi chuyện qua.", 604, 2 },
                    { 6044, "Rarely aware.", "Hiếm khi nhận biết.", 604, 1 },
                    { 6051, "Acknowledge how they feel and offer support.", "Ghi nhận cảm xúc của họ và đề nghị hỗ trợ.", 605, 4 },
                    { 6052, "Give practical advice right away.", "Đưa lời khuyên thực tế ngay.", 605, 3 },
                    { 6053, "Avoid them to not make it worse.", "Tránh họ để không làm mọi việc tệ hơn.", 605, 2 },
                    { 6054, "Tell them to calm down.", "Bảo họ bình tĩnh lại.", 605, 1 },
                    { 6061, "Process it, learn, and move forward.", "Xử lý nó, rút kinh nghiệm và tiến lên.", 606, 4 },
                    { 6062, "Distract myself and avoid it.", "Làm mình sao nhãng và né tránh.", 606, 2 },
                    { 6063, "Dwell on it for a long time.", "Day dứt về nó rất lâu.", 606, 2 },
                    { 6064, "Let it affect everything else.", "Để nó ảnh hưởng đến mọi thứ khác.", 606, 1 },
                    { 6071, "Regulate your tone and stay constructive.", "Điều tiết giọng điệu và giữ tinh thần xây dựng.", 607, 4 },
                    { 6072, "Go quiet and disengage.", "Im lặng và rút lui.", 607, 2 },
                    { 6073, "Let the frustration show sharply.", "Để sự bực bội lộ ra gay gắt.", 607, 1 },
                    { 6074, "Make a sarcastic comment.", "Buông một lời mỉa mai.", 607, 1 },
                    { 6081, "Stay calm and help de-escalate.", "Giữ bình tĩnh và giúp hạ nhiệt.", 608, 4 },
                    { 6082, "Match their energy without thinking.", "Bị cuốn theo cảm xúc của họ mà không nghĩ.", 608, 2 },
                    { 6083, "Withdraw from the situation.", "Rút khỏi tình huống.", 608, 2 },
                    { 6084, "Get overwhelmed.", "Bị choáng ngợp.", 608, 1 },
                    { 7011, "Re-assess, adjust the plan, and move forward.", "Đánh giá lại, điều chỉnh kế hoạch và tiến lên.", 701, 4 },
                    { 7012, "Resist the change and argue for the old plan.", "Chống lại thay đổi và bảo vệ kế hoạch cũ.", 701, 2 },
                    { 7013, "Feel stuck and wait for direction.", "Thấy bế tắc và chờ chỉ đạo.", 701, 2 },
                    { 7014, "Keep working on the outdated plan.", "Vẫn làm theo kế hoạch đã lỗi thời.", 701, 1 },
                    { 7021, "I seek them out and enjoy the challenge.", "Tôi chủ động tìm và thích thử thách.", 702, 4 },
                    { 7022, "I'll learn them when required.", "Tôi học khi bắt buộc.", 702, 3 },
                    { 7023, "I prefer to stick with what I know.", "Tôi thích giữ những gì đã quen.", 702, 2 },
                    { 7024, "I avoid change whenever possible.", "Tôi né tránh thay đổi bất cứ khi nào có thể.", 702, 1 },
                    { 7031, "Stay open, ask questions, and adapt quickly.", "Giữ cởi mở, đặt câu hỏi và thích nghi nhanh.", 703, 4 },
                    { 7032, "Wait for everything to be explained.", "Chờ mọi thứ được giải thích.", 703, 2 },
                    { 7033, "Compare it unfavorably to the old team.", "So sánh theo hướng chê so với nhóm cũ.", 703, 2 },
                    { 7034, "Disengage until things settle.", "Thu mình lại đến khi mọi việc ổn định.", 703, 1 },
                    { 7041, "An opportunity to find a better path.", "Một cơ hội để tìm hướng đi tốt hơn.", 704, 4 },
                    { 7042, "An annoyance I have to tolerate.", "Một sự phiền toái phải chịu đựng.", 704, 2 },
                    { 7043, "A reason to feel discouraged.", "Một lý do để nản lòng.", 704, 2 },
                    { 7044, "A disaster I can't handle.", "Một thảm họa tôi không xử lý nổi.", 704, 1 },
                    { 7051, "Explore alternatives and learn a replacement.", "Tìm giải pháp thay thế và học công cụ mới.", 705, 4 },
                    { 7052, "Keep using it until it fully breaks.", "Dùng tiếp đến khi nó hỏng hẳn.", 705, 2 },
                    { 7053, "Wait for someone to choose a replacement.", "Chờ người khác chọn công cụ thay thế.", 705, 2 },
                    { 7054, "Complain and delay adapting.", "Than phiền và trì hoãn thích nghi.", 705, 1 },
                    { 7061, "Consider it seriously and try adjusting.", "Cân nhắc nghiêm túc và thử điều chỉnh.", 706, 4 },
                    { 7062, "Consider it but rarely change.", "Cân nhắc nhưng hiếm khi thay đổi.", 706, 2 },
                    { 7063, "Feel criticized and defend my way.", "Thấy bị chê và bảo vệ cách của mình.", 706, 2 },
                    { 7064, "Dismiss it.", "Gạt bỏ nó.", 706, 1 },
                    { 7071, "Stay flexible and re-focus on what matters now.", "Giữ linh hoạt và tập trung lại vào việc quan trọng lúc này.", 707, 4 },
                    { 7072, "Get frustrated but comply.", "Bực bội nhưng vẫn làm theo.", 707, 2 },
                    { 7073, "Push back on every change.", "Phản đối mọi thay đổi.", 707, 2 },
                    { 7074, "Lose motivation.", "Mất động lực.", 707, 1 },
                    { 7081, "Stay calm and take sensible next steps.", "Giữ bình tĩnh và thực hiện các bước hợp lý tiếp theo.", 708, 4 },
                    { 7082, "Wait for full certainty before acting.", "Chờ hoàn toàn chắc chắn mới hành động.", 708, 2 },
                    { 7083, "Feel paralyzed by the unknown.", "Bị tê liệt trước điều chưa biết.", 708, 2 },
                    { 7084, "Act rashly without thinking.", "Hành động hấp tấp mà không suy nghĩ.", 708, 1 }
                });

            migrationBuilder.InsertData(
                table: "CaseStudyOptions",
                columns: new[] { "Id", "CaseStudyId", "Feedback", "IsRecommended", "OptionText" },
                values: new object[,]
                {
                    { 1, 1, "This leads to burnout and doesn't solve the team dynamic issue.", false, "Do their work myself." },
                    { 2, 1, "Direct, empathetic communication is key.", true, "Talk to them privately to understand their situation." },
                    { 3, 2, "Risky and doesn't manage expectations.", false, "Work all night and hope for the best." },
                    { 4, 2, "Transparency and proactive planning are essential.", true, "Inform the stakeholders immediately and propose a new timeline." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentOptions_QuestionId",
                table: "AssessmentOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestions_SkillId",
                table: "AssessmentQuestions",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_SkillId",
                table: "AssessmentResults",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_UserId",
                table: "AssessmentResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseStudies_SkillId",
                table: "CaseStudies",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseStudyOptions_CaseStudyId",
                table: "CaseStudyOptions",
                column: "CaseStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UserId",
                table: "ChatMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressLogs_SkillId",
                table: "ProgressLogs",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressLogs_UserId",
                table: "ProgressLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapItems_UserId",
                table: "RoadmapItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSkillSelections_SkillId",
                table: "UserSkillSelections",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCodes_UserId",
                table: "VerificationCodes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssessmentOptions");

            migrationBuilder.DropTable(
                name: "AssessmentResults");

            migrationBuilder.DropTable(
                name: "CaseStudyOptions");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Mentors");

            migrationBuilder.DropTable(
                name: "ProgressLogs");

            migrationBuilder.DropTable(
                name: "RoadmapItems");

            migrationBuilder.DropTable(
                name: "UserSkillSelections");

            migrationBuilder.DropTable(
                name: "VerificationCodes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AssessmentQuestions");

            migrationBuilder.DropTable(
                name: "CaseStudies");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Skills");
        }
    }
}
