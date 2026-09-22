using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixCourseCreatorForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AspNetUsers_CreatorId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CreatorId",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CreatorUserId",
                table: "Courses",
                column: "CreatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AspNetUsers_CreatorUserId",
                table: "Courses",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AspNetUsers_CreatorUserId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CreatorUserId",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CreatorId",
                table: "Courses",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AspNetUsers_CreatorId",
                table: "Courses",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
