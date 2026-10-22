using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCoursesAndChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatorUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailAltText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Courses_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    EnrolledAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ProgressPercentage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    VideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    VideoDurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    Transcript = table.Column<string>(type: "text", nullable: false),
                    CaptionUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseLessons_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillChallenges_AspNetUsers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillChallenges_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SkillChallenges_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseLessonProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnrollmentId = table.Column<int>(type: "integer", nullable: false),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastPositionSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseLessonProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseLessonProgress_CourseEnrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "CourseEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseLessonProgress_CourseLessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "CourseLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizId = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Scenario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ImageAltText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeQuestions_SkillChallenges_QuizId",
                        column: x => x.QuizId,
                        principalTable: "SkillChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeOptions_ChallengeQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ChallengeQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeOptions_QuestionId_Order",
                table: "ChallengeOptions",
                columns: new[] { "QuestionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeQuestions_QuizId_Order",
                table: "ChallengeQuestions",
                columns: new[] { "QuizId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseId_UserId",
                table: "CourseEnrollments",
                columns: new[] { "CourseId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_UserId",
                table: "CourseEnrollments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessonProgress_EnrollmentId_LessonId",
                table: "CourseLessonProgress",
                columns: new[] { "EnrollmentId", "LessonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessonProgress_LessonId",
                table: "CourseLessonProgress",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessons_CourseId_Order",
                table: "CourseLessons",
                columns: new[] { "CourseId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CreatorId",
                table: "Courses",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SkillId",
                table: "Courses",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChallenges_CourseId",
                table: "SkillChallenges",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChallenges_SkillId",
                table: "SkillChallenges",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillChallenges_TeacherId",
                table: "SkillChallenges",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallengeOptions");

            migrationBuilder.DropTable(
                name: "CourseLessonProgress");

            migrationBuilder.DropTable(
                name: "ChallengeQuestions");

            migrationBuilder.DropTable(
                name: "CourseEnrollments");

            migrationBuilder.DropTable(
                name: "CourseLessons");

            migrationBuilder.DropTable(
                name: "SkillChallenges");

            migrationBuilder.DropTable(
                name: "Courses");
        }
    }
}
