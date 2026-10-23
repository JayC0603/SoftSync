using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations;

public partial class AddSyncedMiniGameHistory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GameHistoryRecords",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<int>(type: "integer", nullable: false),
                GameKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                ClientEntryId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                PlayedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                DataJson = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameHistoryRecords", x => x.Id);
                table.ForeignKey(
                    name: "FK_GameHistoryRecords_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GameHistoryRecords_UserId_GameKey_ClientEntryId",
            table: "GameHistoryRecords",
            columns: new[] { "UserId", "GameKey", "ClientEntryId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_GameHistoryRecords_UserId_GameKey_PlayedAtUtc",
            table: "GameHistoryRecords",
            columns: new[] { "UserId", "GameKey", "PlayedAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "GameHistoryRecords");
    }
}
