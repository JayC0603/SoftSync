using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteVisitAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebsiteVisits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteVisits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebsiteActivityMinutes",
                columns: table => new
                {
                    VisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinuteUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ActiveSeconds = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteActivityMinutes", x => new { x.VisitId, x.MinuteUtc });
                    table.ForeignKey(
                        name: "FK_WebsiteActivityMinutes_WebsiteVisits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "WebsiteVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteActivityMinutes_MinuteUtc",
                table: "WebsiteActivityMinutes",
                column: "MinuteUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteVisits_EndedAtUtc",
                table: "WebsiteVisits",
                column: "EndedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteVisits_LastSeenAtUtc",
                table: "WebsiteVisits",
                column: "LastSeenAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteVisits_StartedAtUtc",
                table: "WebsiteVisits",
                column: "StartedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebsiteActivityMinutes");

            migrationBuilder.DropTable(
                name: "WebsiteVisits");
        }
    }
}
