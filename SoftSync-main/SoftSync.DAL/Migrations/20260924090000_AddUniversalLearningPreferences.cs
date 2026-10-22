using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SoftSync.DAL.Data;

#nullable disable

namespace SoftSync.DAL.Migrations;

[DbContext(typeof(SoftSyncDbContext))]
[Migration("20260924090000_AddUniversalLearningPreferences")]
public sealed class AddUniversalLearningPreferences : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "PreferredLearningMode", table: "AspNetUsers", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<bool>(name: "LargeText", table: "AspNetUsers", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>(name: "HighContrast", table: "AspNetUsers", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>(name: "CaptionEnabled", table: "AspNetUsers", type: "boolean", nullable: false, defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PreferredLearningMode", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "LargeText", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "HighContrast", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "CaptionEnabled", table: "AspNetUsers");
    }
}
