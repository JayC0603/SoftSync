using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RoadmapScenarioAndReflection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(name: "ReflectionCompletedAtUtc", table: "RoadmapItems", type: "timestamp without time zone", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ReflectionText", table: "RoadmapItems", type: "character varying(2000)", maxLength: 2000, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "ScenarioCompletedAtUtc", table: "RoadmapItems", type: "timestamp without time zone", nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ReflectionCompletedAtUtc", table: "RoadmapItems");
            migrationBuilder.DropColumn(name: "ReflectionText", table: "RoadmapItems");
            migrationBuilder.DropColumn(name: "ScenarioCompletedAtUtc", table: "RoadmapItems");
        }
    }
}
