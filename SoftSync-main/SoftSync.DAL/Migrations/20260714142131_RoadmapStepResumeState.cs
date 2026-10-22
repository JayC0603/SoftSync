using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RoadmapStepResumeState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastLearningStep",
                table: "RoadmapItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "video");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScriptCompletedAtUtc",
                table: "RoadmapItems",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SummaryCompletedAtUtc",
                table: "RoadmapItems",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLearningStep",
                table: "RoadmapItems");

            migrationBuilder.DropColumn(
                name: "ScriptCompletedAtUtc",
                table: "RoadmapItems");

            migrationBuilder.DropColumn(
                name: "SummaryCompletedAtUtc",
                table: "RoadmapItems");
        }
    }
}
