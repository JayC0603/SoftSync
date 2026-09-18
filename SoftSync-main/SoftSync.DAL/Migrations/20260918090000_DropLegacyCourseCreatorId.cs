using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SoftSync.DAL.Data;

#nullable disable

namespace SoftSync.DAL.Migrations;

/// <summary>
/// The original Course migration accidentally created both CreatorUserId and
/// CreatorId. FixCourseCreatorForeignKey moved the FK but did not remove the
/// required legacy column, so inserts failed because EF no longer writes it.
/// </summary>
[DbContext(typeof(SoftSyncDbContext))]
[Migration("20260918090000_DropLegacyCourseCreatorId")]
public sealed class DropLegacyCourseCreatorId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "CreatorId", table: "Courses");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<int>(name: "CreatorId", table: "Courses", type: "integer", nullable: true);
}
