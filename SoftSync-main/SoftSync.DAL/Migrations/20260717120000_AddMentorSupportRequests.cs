using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SoftSync.DAL.Data;

#nullable disable

namespace SoftSync.DAL.Migrations;

[DbContext(typeof(SoftSyncDbContext))]
[Migration("20260717120000_AddMentorSupportRequests")]
public partial class AddMentorSupportRequests : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MentorSupportRequests",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<int>(type: "integer", nullable: false),
                MentorId = table.Column<int>(type: "integer", nullable: false),
                Goal = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Skill = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Context = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                PreferredContact = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                ShareLearningContext = table.Column<bool>(type: "boolean", nullable: false),
                Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_MentorSupportRequests", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_MentorSupportRequests_UserId", table: "MentorSupportRequests", column: "UserId");
        migrationBuilder.CreateIndex(name: "IX_MentorSupportRequests_MentorId", table: "MentorSupportRequests", column: "MentorId");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "MentorSupportRequests");
}
