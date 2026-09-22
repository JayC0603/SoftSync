using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CompleteQuizAttemptHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Score",
                table: "QuizAttempts",
                newName: "CorrectAnswers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "QuizAttempts",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<int>(
                name: "Result",
                table: "QuizAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ScorePercentage",
                table: "QuizAttempts",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                table: "QuizAttempts",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("""
                UPDATE "QuizAttempts"
                SET "Result" = CASE WHEN "Passed" THEN 1 ELSE 0 END,
                    "ScorePercentage" = "CorrectAnswers" * 10,
                    "StartedAtUtc" = "CompletedAtUtc"
                WHERE "CompletedAtUtc" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "Passed",
                table: "QuizAttempts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Passed",
                table: "QuizAttempts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE "QuizAttempts"
                SET "Passed" = "Result" = 1;
                """);

            migrationBuilder.DropColumn(
                name: "ScorePercentage",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "QuizAttempts");

            migrationBuilder.RenameColumn(
                name: "CorrectAnswers",
                table: "QuizAttempts",
                newName: "Score");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "QuizAttempts",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
