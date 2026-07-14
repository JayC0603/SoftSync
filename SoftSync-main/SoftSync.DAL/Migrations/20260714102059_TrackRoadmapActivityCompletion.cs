using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class TrackRoadmapActivityCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PracticeCompletedAtUtc",
                table: "RoadmapItems",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VideoCompletedAtUtc",
                table: "RoadmapItems",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PracticeCompletedAtUtc",
                table: "RoadmapItems");

            migrationBuilder.DropColumn(
                name: "VideoCompletedAtUtc",
                table: "RoadmapItems");
        }
    }
}
