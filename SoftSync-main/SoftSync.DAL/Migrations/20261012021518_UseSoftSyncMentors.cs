using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UseSoftSyncMentors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Mentors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvatarUrl", "Expertise", "Name", "ShortBio" },
                values: new object[] { "/images/mentor-nguyen-hoang-van-anh.png", "Tâm lý và phát triển cá nhân", "Th.S. Nguyễn Hoàng Vân Anh", "Cố vấn chuyên môn của SoftSync, đồng hành cùng người học xây dựng sự tự tin, hiểu bản thân và phát triển thói quen học tập bền vững." });

            migrationBuilder.UpdateData(
                table: "Mentors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvatarUrl", "Expertise", "Name", "ShortBio" },
                values: new object[] { "/images/mentor-nguyen-thuy-to-tram.png", "Kỹ năng mềm và phát triển cộng đồng", "Th.S. Nguyễn Thuỵ Tố Trâm", "Cố vấn chính của SoftSync, định hướng chuyên môn và hỗ trợ người học ứng dụng kỹ năng giao tiếp, làm việc nhóm vào các tình huống thực tế." });

            migrationBuilder.InsertData(
                table: "Mentors",
                columns: new[] { "Id", "AvatarUrl", "Expertise", "Name", "ShortBio" },
                values: new object[] { 3, "/images/mentor-nguyen-xuan.png", "Giáo dục và công nghệ", "Th.S. Nguyễn Xuân", "Cố vấn chuyên môn của SoftSync, phát triển phương pháp học tập thực tiễn và ứng dụng công nghệ vào trải nghiệm học kỹ năng mềm." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Mentors",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Mentors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvatarUrl", "Expertise", "Name", "ShortBio" },
                values: new object[] { "https://i.pravatar.cc/150?u=katherine", "Leadership & Soft Skills", "Dr. Katherine", "15 years of experience in corporate training." });

            migrationBuilder.UpdateData(
                table: "Mentors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvatarUrl", "Expertise", "Name", "ShortBio" },
                values: new object[] { "https://i.pravatar.cc/150?u=john", "Problem Solving", "John Smith", "Former Google engineer, expert in algorithms and thinking." });
        }
    }
}
