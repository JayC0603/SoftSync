using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadmapLearningPlanMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContentOrigin",
                table: "RoadmapItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Objective",
                table: "RoadmapItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SkillId",
                table: "RoadmapItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceOrganization",
                table: "RoadmapItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceReference",
                table: "RoadmapItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceReviewStatus",
                table: "RoadmapItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceTitle",
                table: "RoadmapItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentOrigin",
                table: "RoadmapItems");

            migrationBuilder.DropColumn(
                name: "Objective",
                table: "RoadmapItems");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "RoadmapItems");

            migrationBuilder.DropColumn(
                name: "SourceOrganization",
                table: "RoadmapItems");

            migrationBuilder.DropColumn(
                name: "SourceReference",
                table: "RoadmapItems");

            migrationBuilder.DropColumn(
                name: "SourceReviewStatus",
                table: "RoadmapItems");

            migrationBuilder.DropColumn(
                name: "SourceTitle",
                table: "RoadmapItems");
        }
    }
}
