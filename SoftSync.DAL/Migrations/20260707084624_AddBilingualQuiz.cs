using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBilingualQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuestionTextVi",
                table: "AssessmentQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionTextVi",
                table: "AssessmentOptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1011,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I focus on saying everything on my mind, paying little attention to the listener's reaction.", "Chỉ tập trung nói hết ý mình, ít để ý phản ứng người nghe", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1012,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I pay attention but am reluctant to ask directly whether it's clear.", "Có để ý nhưng ngại hỏi thẳng \"có rõ không\"" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1013,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I only check when speaking in person, and skip it in texts/emails.", "Chỉ kiểm tra khi nói trực tiếp, bỏ qua khi nhắn tin/email", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1014,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I always watch the listener's cues and proactively ask for feedback.", "Luôn quan sát tín hiệu người nghe và chủ động hỏi phản hồi", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1021,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I'm busy preparing a rebuttal in my head and don't hear it all.", "Bận chuẩn bị phản biện trong đầu, không nghe hết", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1022,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I easily lose interest if I don't click with the speaker.", "Dễ mất hứng nếu không hợp gu với người nói", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1023,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I listen while doing other things, thinking I multitask well.", "Vừa nghe vừa làm việc riêng, nghĩ mình đa nhiệm tốt", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1024,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I listen actively and paraphrase back to confirm I understood.", "Lắng nghe chủ động, diễn đạt lại để xác nhận hiểu đúng", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1031,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I use jargon/slang and assume others just understand.", "Dùng jargon/từ lóng, mặc định người khác tự hiểu", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1032,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I sometimes misunderstand because of regional words or abbreviations.", "Thỉnh thoảng hiểu lầm vì từ địa phương, viết tắt" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1033,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I know the differences but am reluctant to re-explain hard terms.", "Biết khác biệt nhưng ngại giải thích lại từ khó", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1034,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I proactively choose plain words, define clearly, and give examples.", "Chủ động chọn từ dễ hiểu, định nghĩa rõ, có ví dụ", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1041,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I WRITE IN CAPS when urgent and use abbreviations even with superiors.", "Viết HOA khi khẩn cấp, dùng viết tắt cả với cấp trên", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1042,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I reply right away when angry, which easily leads to arguments.", "Phản hồi ngay khi tức giận, dễ dẫn đến tranh cãi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1043,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I write carelessly — no subject line, no proofreading.", "Viết tùy tiện, không tiêu đề, không kiểm tra lỗi", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1044,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I'm always polite, grammatical, and check the tone before sending.", "Luôn lịch sự, đúng ngữ pháp, kiểm tra tông giọng trước khi gửi", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1051,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I get defensive and attack the person back.", "Phản ứng phòng vệ, công kích cá nhân lại", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1052,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I go silent, walk away, and avoid the conflict.", "Im lặng, bỏ đi, né tránh xung đột" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1053,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I try to listen but feel deeply hurt and lose confidence.", "Cố nghe nhưng tổn thương sâu, mất tự tin", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1054,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I calmly clarify the issue while saving face for the other person.", "Bình tĩnh làm rõ vấn đề, giữ thể diện cho đối phương", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1061,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I speak exactly the same way to everyone.", "Nói chuyện y hệt nhau với mọi đối tượng", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1062,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I try to change but fumble choosing the right channel.", "Cố thay đổi nhưng lúng túng chọn kênh phù hợp" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1063,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I'm easily swayed by bias and stereotypes when communicating.", "Dễ bị định kiến, khuôn mẫu chi phối khi giao tiếp", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1064,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I always analyze the context and audience before communicating.", "Luôn phân tích bối cảnh, khán giả trước khi giao tiếp", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1071,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I speak bluntly, sometimes hurting others.", "Nói thẳng thô, đôi khi làm tổn thương người khác", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1072,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I talk in circles until the listener can't follow my point.", "Nói vòng vo đến mức người nghe không hiểu ý", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1073,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I'm confused about when to be direct vs. indirect.", "Bối rối không biết khi nào nên trực tiếp/gián tiếp", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1074,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I balance it: direct for work, gentle when delivering bad news.", "Cân bằng: trực tiếp cho công việc, mềm mỏng khi tin xấu", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1081,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I can't control it — crossed arms, avoiding eye contact.", "Không kiểm soát được, khoanh tay, né ánh mắt", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1082,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm awkward about personal space (too close/too far).", "Lúng túng về khoảng cách giao tiếp (quá gần/xa)" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1083,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I focus only on words and forget expression and posture.", "Chỉ chú trọng câu chữ, quên biểu cảm và tư thế", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1084,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I keep my posture, eye contact, and tone appropriate to the context.", "Luôn giữ tư thế, ánh mắt, tông giọng phù hợp bối cảnh", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2011,
                column: "OptionTextVi",
                value: "Nói chuyện riêng để hiểu chuyện gì đang xảy ra.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2012,
                column: "OptionTextVi",
                value: "Lặng lẽ tự làm luôn phần của họ.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2013,
                column: "OptionTextVi",
                value: "Báo ngay với người phụ trách.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2014,
                column: "OptionTextVi",
                value: "Chỉ trích họ trước cả nhóm.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2021,
                column: "OptionTextVi",
                value: "Tìm giải pháp kết hợp điểm tốt của cả hai.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2022,
                column: "OptionTextVi",
                value: "Cố bảo vệ ý mình vì tự tin vào nó.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2023,
                column: "OptionTextVi",
                value: "Bỏ ý mình để tránh va chạm.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2024,
                column: "OptionTextVi",
                value: "Từ chối làm việc với họ.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2031,
                column: "OptionTextVi",
                value: "Tập hợp nhóm, lên lại kế hoạch và chia sẻ khối lượng việc.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2032,
                column: "OptionTextVi",
                value: "Chỉ tập trung hoàn thành phần của mình.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2033,
                column: "OptionTextVi",
                value: "Chờ người khác đứng ra chịu trách nhiệm.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2034,
                column: "OptionTextVi",
                value: "Đổ lỗi cho người gây ra chuyện.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2041,
                column: "OptionTextVi",
                value: "Ghi nhận đóng góp của mọi người.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2042,
                column: "OptionTextVi",
                value: "Nhắc đến cả nhóm nếu được hỏi.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2043,
                column: "OptionTextVi",
                value: "Đề cao phần của mình trước.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2044,
                column: "OptionTextVi",
                value: "Nhận hết công về mình.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2051,
                column: "OptionTextVi",
                value: "Mời họ tham gia và hỏi họ nghĩ gì.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2052,
                column: "OptionTextVi",
                value: "Cho rằng họ đồng ý với cả nhóm.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2053,
                column: "OptionTextVi",
                value: "Bỏ qua và tiếp tục mà không có họ.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2054,
                column: "OptionTextVi",
                value: "Quyết định thay cho họ.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2061,
                column: "OptionTextVi",
                value: "Nhận và làm tốt vì cả nhóm.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2062,
                column: "OptionTextVi",
                value: "Làm, nhưng chỉ ở mức tối thiểu.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2063,
                column: "OptionTextVi",
                value: "Tìm cách đẩy cho người khác.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2064,
                column: "OptionTextVi",
                value: "Từ chối.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2071,
                column: "OptionTextVi",
                value: "Giúp họ trao đổi và tìm điểm chung.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2072,
                column: "OptionTextVi",
                value: "Chọn phe mình đồng tình.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2073,
                column: "OptionTextVi",
                value: "Đứng ngoài hoàn toàn.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2074,
                column: "OptionTextVi",
                value: "Bảo mọi người dẹp chuyện đó đi.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2081,
                column: "OptionTextVi",
                value: "Tôi làm đúng điều đã hứa, đúng hạn, ổn định.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2082,
                column: "OptionTextVi",
                value: "Thường đáng tin, thỉnh thoảng lỡ hẹn.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2083,
                column: "OptionTextVi",
                value: "Chỉ đáng tin khi được nhắc.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 2084,
                column: "OptionTextVi",
                value: "Hay thất hứa.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3011,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "It depends on whatever schedule others set for me.", "Phụ thuộc lịch người khác sắp xếp", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3012,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I study on a whim, with no schedule.", "Học tùy hứng, không lịch trình" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3013,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I have a plan but find it hard to stick to.", "Có kế hoạch nhưng khó giữ đúng", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3014,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I'm always proactive, with my own strategy.", "Luôn chủ động, có chiến lược riêng", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3021,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Fear of failure, so I avoid getting started.", "Sợ thất bại nên né tránh bắt tay vào làm", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3022,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm easily distracted by my phone and social media.", "Dễ mất tập trung bởi điện thoại, mạng xã hội" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3023,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "My body is tired, sleep-deprived, low on energy.", "Cơ thể mệt mỏi, thiếu ngủ, thiếu năng lượng", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3024,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I rarely have this problem — I start right away.", "Tôi hiếm khi gặp vấn đề này, bắt tay vào việc ngay", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3031,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Way off — a task I thought was 1 hour takes 4–5.", "Sai lệch nặng, việc tưởng 1 giờ hóa ra mất 4-5 giờ", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3032,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Hard to estimate for material heavy with figures and charts.", "Khó ước lượng với tài liệu nhiều số liệu, biểu đồ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3033,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I often pull all-nighters near the deadline to make up for it.", "Thường phải thức trắng đêm sát hạn để bù đắp", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3034,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Fairly accurate, always with buffer time.", "Ước lượng khá sát, luôn có thời gian dự phòng", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3041,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I'm constantly pulled in by notifications, messages, games.", "Liên tục bị cuốn vào thông báo, tin nhắn, game", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3042,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm easily distracted by noise and a messy space.", "Dễ xao nhãng bởi tiếng ồn, không gian bừa bộn" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3043,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I sometimes multitask, which lowers my output.", "Thỉnh thoảng làm nhiều việc cùng lúc, hiệu suất giảm", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3044,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I always have a quiet space with all notifications off.", "Luôn có không gian yên tĩnh, tắt hết thông báo", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3051,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "No, I only get motivated when the deadline is imminent.", "Không, chỉ có động lực khi hạn chót cận kề", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3052,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I know some but have never applied one successfully.", "Có biết nhưng chưa áp dụng thành công lần nào" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3053,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I apply them but often quit halfway (e.g. Pomodoro).", "Có áp dụng nhưng hay bỏ giữa chừng (vd: Pomodoro)", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3054,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I apply them fluently: Eat the Frog, break tasks down, daily Top 3.", "Áp dụng nhuần nhuyễn: Eat the Frog, chia việc nhỏ, Top 3 mỗi ngày", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3061,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Momentary emotion or panic over the deadline.", "Cảm xúc nhất thời hoặc hoảng loạn vì hạn chót", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3062,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I pick easy tasks first and dodge hard-but-important ones.", "Chọn việc dễ trước, né việc khó nhưng quan trọng", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3063,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I make a list but struggle to sort out the core tasks.", "Có lập danh sách nhưng khó phân loại việc cốt lõi", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3064,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I use the Eisenhower matrix to classify clearly.", "Dùng ma trận Eisenhower để phân loại rõ ràng", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3071,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Very vague, hard to measure (\"get better\").", "Rất mơ hồ, khó đo lường (\"học giỏi hơn\")", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3072,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Clear but with no specific deadline.", "Rõ ràng nhưng không gắn thời hạn cụ thể" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3073,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Specific but unrealistic, not feasible.", "Cụ thể nhưng thiếu thực tế, không khả thi", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3074,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Always fully meet the SMART criteria.", "Luôn đạt chuẩn SMART đầy đủ", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3081,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Panic, get discouraged, and drop the work.", "Hoảng loạn, nản chí, bỏ dở công việc", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3082,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Stubbornly cling to the old plan, extremely stressed.", "Cố chấp bám kế hoạch cũ, căng thẳng tột độ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3083,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Struggle through it alone, without telling anyone.", "Tự loay hoay giải quyết, không báo với ai", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3084,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Calmly reassess, proactively inform others, and adjust.", "Bình tĩnh đánh giá lại, chủ động thông báo và điều chỉnh", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4011,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I instantly believe convincing claims on social media.", "Tin ngay vào tuyên bố thuyết phục trên mạng xã hội", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4012,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I treat the opinion of someone I like as obvious fact.", "Coi ý kiến người mình yêu thích là sự thật hiển nhiên" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4013,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I distinguish well, but struggle with cleverly disguised data.", "Phân biệt tốt nhưng khó với số liệu ngụy trang tinh vi", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4014,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I always distinguish clearly and demand empirical evidence.", "Luôn phân biệt rõ, yêu cầu bằng chứng thực nghiệm", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4021,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I only read news that agrees with me and think others are wrong.", "Chỉ đọc tin cùng quan điểm, nghĩ người khác sai", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4022,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm annoyed by opposing news and look to refute it.", "Khó chịu khi đọc tin trái chiều, tìm cách bác bỏ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4023,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I read other views but still cherry-pick what favors me.", "Đọc góc nhìn khác nhưng vẫn chọn lọc có lợi cho mình", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4024,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I proactively seek many sources and weigh opposing evidence fairly.", "Chủ động tiếp cận đa nguồn, công tâm với bằng chứng trái chiều", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4031,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Attack the other person when I'm challenged.", "Công kích cá nhân đối phương khi bị phản bác", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4032,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Follow the majority, avoiding independent thinking.", "A dua theo số đông, tránh tư duy độc lập", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4033,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Sometimes use extreme examples to distract.", "Đôi khi dùng ví dụ cực đoan để đánh lạc hướng", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4034,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Focus on rational analysis, avoiding logical fallacies.", "Tập trung phân tích lý trí, tránh ngụy biện logic", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4041,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Trust a nice-looking site and the author's self-introduction.", "Tin vào giao diện đẹp, lời tự giới thiệu của tác giả", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4042,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Trust it based on likes and positive comments below.", "Tin theo lượt thích, bình luận tích cực bên dưới" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4043,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I know I should verify but am lazy, only doing it when it matters.", "Biết cần kiểm chứng nhưng lười, chỉ làm khi quan trọng", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4044,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Read laterally — open many tabs to cross-check independent sources.", "Đọc ngang — mở nhiều tab đối chiếu nguồn độc lập", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4051,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Only fix the surface, without exploring the root cause.", "Chỉ giải quyết phần nổi, không tìm hiểu gốc rễ", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4052,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Decide hastily on gut feeling.", "Quyết định vội vàng theo cảm tính", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4053,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Get stuck after a few \"why\" questions, easily going off track.", "Bế tắc sau vài câu hỏi \"tại sao\", dễ lạc hướng", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4054,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Apply the \"5 Whys\" to find the root cause.", "Áp dụng \"5 câu hỏi Tại sao\" để tìm gốc rễ vấn đề", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4061,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Stubborn — I cling to my view even when proven wrong.", "Bảo thủ, bám quan điểm dù bị chứng minh sai", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4062,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I change, but by emotion/the crowd, not by evidence.", "Thay đổi nhưng theo cảm xúc/số đông, không phải bằng chứng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4063,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I note the new evidence but delay adjusting.", "Ghi nhận bằng chứng mới nhưng trì hoãn điều chỉnh", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4064,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I proactively update my thinking when the data changes.", "Chủ động cập nhật tư duy khi dữ liệu thay đổi", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4071,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "Pick the first Google result, assuming it's most authoritative.", "Chọn kết quả đầu tiên trên Google, tin là uy tín nhất", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4072,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "It only needs to be well-written and match what I want to prove.", "Chỉ cần viết hay và trùng khớp với điều mình muốn chứng minh" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4073,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I only check the author/domain, ignoring conflicts of interest.", "Chỉ xem tên tác giả/tên miền, bỏ qua xung đột lợi ích", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4074,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I review comprehensively: author, recency, funding source, peer review.", "Rà soát toàn diện: tác giả, tính cập nhật, nguồn tài trợ, bình duyệt", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4081,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I think of only one solution, helpless when it fails.", "Chỉ nghĩ 1 giải pháp, bất lực khi nó thất bại", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4082,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I think about it but give up, seeing it as time-consuming.", "Có nghĩ đến nhưng bỏ cuộc vì thấy tốn thời gian", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4083,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I only prepare a backup for big things, improvising the rest.", "Chỉ chuẩn bị dự phòng cho việc lớn, còn lại tùy cơ ứng biến", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4084,
                columns: new[] { "OptionText", "OptionTextVi", "ScoreValue" },
                values: new object[] { "I always build multiple options and concrete contingency plans.", "Luôn xây nhiều phương án và kế hoạch dự phòng cụ thể", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5011,
                column: "OptionTextVi",
                value: "Xác định rõ vấn đề, rồi khám phá các phương án.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5012,
                column: "OptionTextVi",
                value: "Thử ngay ý tưởng đầu tiên nảy ra.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5013,
                column: "OptionTextVi",
                value: "Chờ người khác giải quyết.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5014,
                column: "OptionTextVi",
                value: "Né tránh và làm việc khác.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5021,
                column: "OptionTextVi",
                value: "Phân tích lý do, rồi thử cách khác.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5022,
                column: "OptionTextVi",
                value: "Lặp lại và hy vọng kết quả khác.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5023,
                column: "OptionTextVi",
                value: "Bỏ cuộc với vấn đề đó.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5024,
                column: "OptionTextVi",
                value: "Đổ lỗi cho yếu tố bên ngoài.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5031,
                column: "OptionTextVi",
                value: "Chia nhỏ thành các phần có thể giải quyết.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5032,
                column: "OptionTextVi",
                value: "Lao thẳng vào toàn bộ vấn đề.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5033,
                column: "OptionTextVi",
                value: "Chờ đến khi nó trở nên cấp bách.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5034,
                column: "OptionTextVi",
                value: "Hy vọng nó tự được giải quyết.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5041,
                column: "OptionTextVi",
                value: "Nghĩ nhiều ý, rồi cân nhắc được–mất.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5042,
                column: "OptionTextVi",
                value: "Chọn phương án quen thuộc nhất.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5043,
                column: "OptionTextVi",
                value: "Sao chép cách đã hiệu quả ở nơi khác mà không điều chỉnh.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5044,
                column: "OptionTextVi",
                value: "Chọn ý đầu tiên khả thi.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5051,
                column: "OptionTextVi",
                value: "Xác định thứ còn thiếu và đi tìm nó.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5052,
                column: "OptionTextVi",
                value: "Giả định rồi cứ tiến hành.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5053,
                column: "OptionTextVi",
                value: "Giải một vấn đề khác dễ hơn.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5054,
                column: "OptionTextVi",
                value: "Dừng lại đến khi có người đưa thông tin.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5061,
                column: "OptionTextVi",
                value: "Kiểm thử theo tiêu chí thực và các trường hợp biên.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5062,
                column: "OptionTextVi",
                value: "Xem qua thấy có vẻ đúng.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5063,
                column: "OptionTextVi",
                value: "Cho là chạy được nếu không báo lỗi.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5064,
                column: "OptionTextVi",
                value: "Tôi không kiểm chứng.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5071,
                column: "OptionTextVi",
                value: "Dùng trước, rồi lên kế hoạch cải tiến gọn hơn.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5072,
                column: "OptionTextVi",
                value: "Để nguyên như vậy mãi.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5073,
                column: "OptionTextVi",
                value: "Bỏ hết và làm lại từ đầu.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5074,
                column: "OptionTextVi",
                value: "Phớt lờ sự kém hiệu quả.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5081,
                column: "OptionTextVi",
                value: "Lùi lại và nhìn nhận lại vấn đề.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5082,
                column: "OptionTextVi",
                value: "Cố đẩy mạnh theo cùng một cách.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5083,
                column: "OptionTextVi",
                value: "Đoán bừa rồi đi tiếp.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 5084,
                column: "OptionTextVi",
                value: "Bỏ dở nhiệm vụ.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6011,
                column: "OptionTextVi",
                value: "Hít thở, tìm phần hữu ích và phản hồi bình tĩnh.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6012,
                column: "OptionTextVi",
                value: "Thấy bị công kích và cãi lại.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6013,
                column: "OptionTextVi",
                value: "Đóng lại và ngừng tương tác.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6014,
                column: "OptionTextVi",
                value: "Giả vờ không bận tâm nhưng ấm ức trong lòng.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6021,
                column: "OptionTextVi",
                value: "Nhận ra, dừng lại và chọn cách phản hồi.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6022,
                column: "OptionTextVi",
                value: "Cố giấu nhưng vẫn lộ ra.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6023,
                column: "OptionTextVi",
                value: "Bộc lộ ngay lập tức.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6024,
                column: "OptionTextVi",
                value: "Dồn nén hoàn toàn.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6031,
                column: "OptionTextVi",
                value: "Dùng thói quen đối phó và tập trung vào bước kế tiếp.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6032,
                column: "OptionTextVi",
                value: "Cố làm tới trong khi phớt lờ căng thẳng.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6033,
                column: "OptionTextVi",
                value: "Để nỗi lo làm đình trệ công việc.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6034,
                column: "OptionTextVi",
                value: "Trút bực dọc lên mọi người xung quanh.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6041,
                column: "OptionTextVi",
                value: "Rất rõ — tôi gọi tên được cảm xúc và lý do.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6042,
                column: "OptionTextVi",
                value: "Nhận biết phần nào trong lúc đó.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6043,
                column: "OptionTextVi",
                value: "Chỉ nhận ra sau khi mọi chuyện qua.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6044,
                column: "OptionTextVi",
                value: "Hiếm khi nhận biết.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6051,
                column: "OptionTextVi",
                value: "Ghi nhận cảm xúc của họ và đề nghị hỗ trợ.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6052,
                column: "OptionTextVi",
                value: "Đưa lời khuyên thực tế ngay.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6053,
                column: "OptionTextVi",
                value: "Tránh họ để không làm mọi việc tệ hơn.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6054,
                column: "OptionTextVi",
                value: "Bảo họ bình tĩnh lại.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6061,
                column: "OptionTextVi",
                value: "Xử lý nó, rút kinh nghiệm và tiến lên.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6062,
                column: "OptionTextVi",
                value: "Làm mình sao nhãng và né tránh.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6063,
                column: "OptionTextVi",
                value: "Day dứt về nó rất lâu.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6064,
                column: "OptionTextVi",
                value: "Để nó ảnh hưởng đến mọi thứ khác.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6071,
                column: "OptionTextVi",
                value: "Điều tiết giọng điệu và giữ tinh thần xây dựng.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6072,
                column: "OptionTextVi",
                value: "Im lặng và rút lui.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6073,
                column: "OptionTextVi",
                value: "Để sự bực bội lộ ra gay gắt.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6074,
                column: "OptionTextVi",
                value: "Buông một lời mỉa mai.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6081,
                column: "OptionTextVi",
                value: "Giữ bình tĩnh và giúp hạ nhiệt.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6082,
                column: "OptionTextVi",
                value: "Bị cuốn theo cảm xúc của họ mà không nghĩ.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6083,
                column: "OptionTextVi",
                value: "Rút khỏi tình huống.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 6084,
                column: "OptionTextVi",
                value: "Bị choáng ngợp.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7011,
                column: "OptionTextVi",
                value: "Đánh giá lại, điều chỉnh kế hoạch và tiến lên.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7012,
                column: "OptionTextVi",
                value: "Chống lại thay đổi và bảo vệ kế hoạch cũ.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7013,
                column: "OptionTextVi",
                value: "Thấy bế tắc và chờ chỉ đạo.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7014,
                column: "OptionTextVi",
                value: "Vẫn làm theo kế hoạch đã lỗi thời.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7021,
                column: "OptionTextVi",
                value: "Tôi chủ động tìm và thích thử thách.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7022,
                column: "OptionTextVi",
                value: "Tôi học khi bắt buộc.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7023,
                column: "OptionTextVi",
                value: "Tôi thích giữ những gì đã quen.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7024,
                column: "OptionTextVi",
                value: "Tôi né tránh thay đổi bất cứ khi nào có thể.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7031,
                column: "OptionTextVi",
                value: "Giữ cởi mở, đặt câu hỏi và thích nghi nhanh.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7032,
                column: "OptionTextVi",
                value: "Chờ mọi thứ được giải thích.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7033,
                column: "OptionTextVi",
                value: "So sánh theo hướng chê so với nhóm cũ.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7034,
                column: "OptionTextVi",
                value: "Thu mình lại đến khi mọi việc ổn định.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7041,
                column: "OptionTextVi",
                value: "Một cơ hội để tìm hướng đi tốt hơn.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7042,
                column: "OptionTextVi",
                value: "Một sự phiền toái phải chịu đựng.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7043,
                column: "OptionTextVi",
                value: "Một lý do để nản lòng.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7044,
                column: "OptionTextVi",
                value: "Một thảm họa tôi không xử lý nổi.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7051,
                column: "OptionTextVi",
                value: "Tìm giải pháp thay thế và học công cụ mới.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7052,
                column: "OptionTextVi",
                value: "Dùng tiếp đến khi nó hỏng hẳn.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7053,
                column: "OptionTextVi",
                value: "Chờ người khác chọn công cụ thay thế.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7054,
                column: "OptionTextVi",
                value: "Than phiền và trì hoãn thích nghi.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7061,
                column: "OptionTextVi",
                value: "Cân nhắc nghiêm túc và thử điều chỉnh.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7062,
                column: "OptionTextVi",
                value: "Cân nhắc nhưng hiếm khi thay đổi.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7063,
                column: "OptionTextVi",
                value: "Thấy bị chê và bảo vệ cách của mình.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7064,
                column: "OptionTextVi",
                value: "Gạt bỏ nó.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7071,
                column: "OptionTextVi",
                value: "Giữ linh hoạt và tập trung lại vào việc quan trọng lúc này.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7072,
                column: "OptionTextVi",
                value: "Bực bội nhưng vẫn làm theo.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7073,
                column: "OptionTextVi",
                value: "Phản đối mọi thay đổi.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7074,
                column: "OptionTextVi",
                value: "Mất động lực.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7081,
                column: "OptionTextVi",
                value: "Giữ bình tĩnh và thực hiện các bước hợp lý tiếp theo.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7082,
                column: "OptionTextVi",
                value: "Chờ hoàn toàn chắc chắn mới hành động.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7083,
                column: "OptionTextVi",
                value: "Bị tê liệt trước điều chưa biết.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 7084,
                column: "OptionTextVi",
                value: "Hành động hấp tấp mà không suy nghĩ.");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "How do you handle two-way exchange of information?", "Bạn xử lý việc trao đổi thông tin hai chiều thế nào?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "What most affects your listening habits?", "Thói quen lắng nghe của bạn bị ảnh hưởng nhiều nhất bởi gì?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "How do you deal with differences in wording and terminology?", "Bạn xử lý sự khác biệt về từ ngữ, thuật ngữ thế nào?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "What is your texting/email style like?", "Phong cách viết tin nhắn/email của bạn thế nào?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When criticized in a conversation, what is your natural reaction?", "Khi bị chỉ trích trong hội thoại, phản ứng tự nhiên của bạn là gì?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "How do you adapt your communication style to context?", "Bạn thích ứng phong cách giao tiếp theo bối cảnh thế nào?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "How do you recognize your own direct/indirect style?", "Bạn nhận diện phong cách trực tiếp/gián tiếp của mình ra sao?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "How do you control your body language?", "Bạn kiểm soát ngôn ngữ cơ thể của mình thế nào?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 201,
                column: "QuestionTextVi",
                value: "Một thành viên không đóng góp cho dự án nhóm. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 202,
                column: "QuestionTextVi",
                value: "Khi ý tưởng của bạn xung đột với đồng đội, bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 203,
                column: "QuestionTextVi",
                value: "Nhóm gặp trục trặc sát hạn chót. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 204,
                column: "QuestionTextVi",
                value: "Bạn ứng xử với công lao của một thành công chung thế nào?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 205,
                column: "QuestionTextVi",
                value: "Một đồng đội trầm tính chưa nêu ý kiến. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 206,
                column: "QuestionTextVi",
                value: "Khi nhận việc mình không thích nhưng nhóm cần, bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 207,
                column: "QuestionTextVi",
                value: "Hai đồng đội mâu thuẫn làm chậm công việc. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 208,
                column: "QuestionTextVi",
                value: "Bạn giữ cam kết với nhóm đáng tin đến đâu?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "How does your out-of-class studying go?", "Việc học ngoài giờ lên lớp của bạn diễn ra thế nào?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 302,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When you procrastinate on a big task, what is the deepest reason?", "Khi trì hoãn một việc lớn, lý do sâu xa nhất của bạn là gì?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 303,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "Do you estimate how long a task takes accurately?", "Bạn ước lượng thời gian làm một việc có chính xác không?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 304,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "What most affects your focus?", "Yếu tố nào ảnh hưởng đến sự tập trung của bạn nhất?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 305,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "Do you apply scientific time-management methods?", "Bạn có áp dụng phương pháp quản lý thời gian khoa học không?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 306,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "What do you base on when deciding what to do first?", "Bạn quyết định làm việc gì trước dựa trên điều gì?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 307,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "How are your study/work goals written?", "Mục tiêu học tập/công việc của bạn được viết ra như thế nào?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 308,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "When your plan is broken by an unexpected event, how do you react?", "Khi kế hoạch bị phá vỡ bởi biến cố bất ngờ, bạn phản ứng ra sao?", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "How well do you separate fact from personal opinion?", "Bạn phân biệt sự thật và ý kiến cá nhân tốt đến đâu?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "How do you deal with your own confirmation bias?", "Bạn đối phó với thiên kiến xác nhận của bản thân thế nào?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "In a group debate, how do you usually react?", "Khi tranh luận nhóm, bạn thường phản ứng ra sao?", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "How do you verify an unfamiliar source online?", "Bạn kiểm chứng một nguồn tin lạ trên mạng ra sao?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "Facing a complex problem, how do you find a solution?", "Khi gặp vấn đề phức tạp, bạn tìm giải pháp thế nào?", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "Are you willing to change your view when new evidence appears?", "Bạn có sẵn sàng thay đổi quan điểm khi có bằng chứng mới không?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 407,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "How do you assess the reliability of a research document?", "Bạn đánh giá độ tin cậy một tài liệu nghiên cứu ra sao?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 408,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When facing a problem, do you prepare multiple options?", "Khi gặp vấn đề, bạn có chuẩn bị nhiều phương án không?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 501,
                column: "QuestionTextVi",
                value: "Bạn gặp một vấn đề lạ không có giải pháp rõ ràng. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 502,
                column: "QuestionTextVi",
                value: "Khi một giải pháp thất bại, bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 503,
                column: "QuestionTextVi",
                value: "Một vấn đề quá lớn để xử lý một lần. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 504,
                column: "QuestionTextVi",
                value: "Bạn tạo ra các ý tưởng giải pháp thế nào?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 505,
                column: "QuestionTextVi",
                value: "Bạn thiếu thông tin để giải quyết vấn đề. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 506,
                column: "QuestionTextVi",
                value: "Bạn kiểm chứng một giải pháp thực sự hiệu quả thế nào?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 507,
                column: "QuestionTextVi",
                value: "Giải pháp của bạn chạy được nhưng chưa tối ưu. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 508,
                column: "QuestionTextVi",
                value: "Khi bế tắc, bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 601,
                column: "QuestionTextVi",
                value: "Bạn nhận lời chỉ trích gay gắt về công việc của mình. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 602,
                column: "QuestionTextVi",
                value: "Khi tức giận trong công việc, bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 603,
                column: "QuestionTextVi",
                value: "Một hạn chót căng thẳng khiến bạn lo lắng. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 604,
                column: "QuestionTextVi",
                value: "Bạn nhận biết cảm xúc của mình ngay khi nó xảy ra đến đâu?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 605,
                column: "QuestionTextVi",
                value: "Một đồng nghiệp rõ ràng đang buồn bực. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 606,
                column: "QuestionTextVi",
                value: "Sau một thất bại, bạn hồi phục thế nào?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 607,
                column: "QuestionTextVi",
                value: "Bạn bực bội trong một cuộc họp. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 608,
                column: "QuestionTextVi",
                value: "Bạn xử lý cảm xúc mạnh của người khác thế nào?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 701,
                column: "QuestionTextVi",
                value: "Yêu cầu của dự án thay đổi đột ngột. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 702,
                column: "QuestionTextVi",
                value: "Bạn cảm thấy thế nào về việc học công cụ hay phương pháp mới?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 703,
                column: "QuestionTextVi",
                value: "Bạn bị chuyển sang một nhóm xa lạ chỉ sau một đêm. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 704,
                column: "QuestionTextVi",
                value: "Khi kế hoạch đổ vỡ, tâm thế của bạn là...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 705,
                column: "QuestionTextVi",
                value: "Một công cụ bạn phụ thuộc bị ngừng hỗ trợ. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 706,
                column: "QuestionTextVi",
                value: "Bạn phản hồi thế nào khi được góp ý nên thay đổi cách làm việc?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 707,
                column: "QuestionTextVi",
                value: "Ưu tiên thay đổi lần thứ ba trong tuần. Bạn...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 708,
                column: "QuestionTextVi",
                value: "Trong một tình huống mơ hồ, bất định, bạn...");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionTextVi",
                table: "AssessmentQuestions");

            migrationBuilder.DropColumn(
                name: "OptionTextVi",
                table: "AssessmentOptions");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1011,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Politely note you'd like to finish, then continue your point.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1012,
                column: "OptionText",
                value: "Stop talking and let the moment go.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1013,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Raise your voice to talk over them.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1014,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Say nothing but complain about them later.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1021,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Essential — I paraphrase to confirm I understood.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1022,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Fairly important, I try to listen most of the time.", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1023,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Somewhat — I mostly wait for my turn to talk.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1024,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Not important.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1031,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Use plain language and a relatable example.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1032,
                column: "OptionText",
                value: "Explain it fully with all the technical terms.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1033,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Send them a document to read on their own.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1034,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Assume they'll figure it out.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1041,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Be specific, kind, and focus on the behavior.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1042,
                column: "OptionText",
                value: "Give general praise and avoid the hard parts.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1043,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Point out only what went wrong.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1044,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Avoid giving feedback at all.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1051,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Share your view calmly with reasons and ask for others'.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1052,
                column: "OptionText",
                value: "Go along with it to keep the peace.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1053,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Send a blunt message showing your frustration.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1054,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Stay silent and disengage.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1061,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Ask a clarifying question right away.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1062,
                column: "OptionText",
                value: "Guess the meaning and reply.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1063,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Wait and hope it becomes clear later.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1064,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Ignore it.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1071,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Pause, check in, and re-explain the key point.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1072,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Slow down but keep going as planned.", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1073,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Speed up to finish sooner.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1074,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Ignore it and push through.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1081,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Adapt tone and detail to the reader.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1082,
                column: "OptionText",
                value: "Keep it short regardless of the reader.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1083,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Write the same way for everyone.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1084,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Don't think about tone.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3011,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Inform stakeholders and propose a new timeline.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3012,
                column: "OptionText",
                value: "Work all night and hope it's enough.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3013,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Say nothing and hand it in late.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3014,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Ask for the deadline to be dropped.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3021,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Review priorities and plan the day.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3022,
                column: "OptionText",
                value: "Jump into whatever feels urgent.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3023,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Check messages first, then wing it.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3024,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Start with the easiest tasks.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3031,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Prioritize by impact and deadline, then focus.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3032,
                column: "OptionText",
                value: "Do them in the order they arrived.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3033,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Do the quickest ones and leave the big one.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3034,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Try to multitask all three at once.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3041,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Block focus time and silence interruptions.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3042,
                column: "OptionText",
                value: "Take breaks whenever a distraction appears.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3043,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Try to resist but often give in.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3044,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I'm easily pulled off-task.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3051,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Assess its true priority before switching.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3052,
                column: "OptionText",
                value: "Drop everything and switch immediately.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3053,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Ignore it until my current task is done.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3054,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Panic and lose track of both.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3061,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Fairly accurately, with buffer for surprises.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3062,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Roughly right most of the time.", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3063,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I usually underestimate.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3064,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I don't estimate at all.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3071,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Break it into small steps and start one now.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3072,
                column: "OptionText",
                value: "Wait until you feel motivated.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3073,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Do smaller tasks to feel productive.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3074,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Keep putting it off.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3081,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Reflect on what worked and adjust next week.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3082,
                column: "OptionText",
                value: "Just move on to the weekend.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3083,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Feel behind but don't review why.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3084,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Rarely think about it.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4011,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Check the source and how the data was gathered.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4012,
                column: "OptionText",
                value: "Trust it if it sounds reasonable.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4013,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Share it if it matches what I believe.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4014,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Accept it because it was widely posted.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4021,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Weigh evidence and consider alternatives first.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4022,
                column: "OptionText",
                value: "Go with it since everyone agrees.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4023,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Support it to avoid standing out.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4024,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Reject it just to be different.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4031,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Actively seek views that challenge mine.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4032,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Consider other views if they come up.", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4033,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Mostly look for support for my view.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4034,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Rarely question my first reaction.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4041,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Examine the reasoning and evidence behind each.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4042,
                column: "OptionText",
                value: "Follow whoever is more senior.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4043,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Pick the advice that's easier to follow.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4044,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Get stuck and do nothing.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4051,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I identify and test them explicitly.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4052,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I notice them sometimes.", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4053,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I rarely question them.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4054,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I don't think about them.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4061,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Accept the data and revise the approach.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4062,
                column: "OptionText",
                value: "Look for reasons to dismiss the data.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4063,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Keep going and hope it improves.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4064,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Ignore the data.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4071,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Break it into parts and check each.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4072,
                column: "OptionText",
                value: "Judge it as a whole by gut feel.");

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4073,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Ask what others think of it.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4074,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "Accept or reject it quickly.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4081,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I look for confounders and other explanations.", 4 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4082,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I'm cautious but not always sure.", 3 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4083,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I often assume one causes the other.", 2 });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4084,
                columns: new[] { "OptionText", "ScoreValue" },
                values: new object[] { "I don't consider the difference.", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "A teammate interrupts you mid-sentence in a meeting. What do you do?", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 102,
                column: "QuestionText",
                value: "How important is active listening in a conversation?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "You need to explain a complex idea to a non-expert. You...", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 104,
                column: "QuestionText",
                value: "When giving feedback to a peer, you tend to...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 105,
                column: "QuestionText",
                value: "You disagree with a decision in a group chat. You...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 106,
                column: "QuestionText",
                value: "How do you handle a message you don't fully understand?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "You're presenting and notice the audience looks confused. You...", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 108,
                column: "QuestionText",
                value: "In written communication, you...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "You realize you will miss a deadline tomorrow. Your first action is to...", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 302,
                column: "QuestionText",
                value: "How do you start your workday?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 303,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "You have three tasks due and can't finish all. You...", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 304,
                column: "QuestionText",
                value: "How do you handle distractions while working?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 305,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "A new urgent request lands mid-task. You...", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 306,
                column: "QuestionText",
                value: "How well do you estimate how long tasks take?");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 307,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "You keep procrastinating on a big task. You...", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 308,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "At the end of the week you...", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 401,
                column: "QuestionText",
                value: "You read a surprising statistic online. You...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "A popular solution is proposed for a problem. You...", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "When you form an opinion, you...", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "Two experts give you conflicting advice. You...", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "How do you treat your own assumptions?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "QuestionText", "Type" },
                values: new object[] { "Data shows your favorite approach is underperforming. You...", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 407,
                column: "QuestionText",
                value: "Faced with a complex claim, you first...");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 408,
                column: "QuestionText",
                value: "How do you distinguish correlation from causation?");
        }
    }
}
