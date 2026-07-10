using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1011,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Keep talking, assuming they'll figure it out themselves.", "Nói tiếp, nghĩ bạn ấy sẽ tự hiểu ra" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1012,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Speak louder or repeat the exact same sentence.", "Nói to hơn hoặc lặp lại y nguyên câu cũ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1013,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Drop it, thinking it's not important.", "Bỏ qua, nghĩ chuyện đó không quan trọng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1014,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Pause, ask how much they understood, then re-explain a different way.", "Dừng lại, hỏi bạn ấy hiểu đến đâu rồi giải thích lại theo cách khác" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1021,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm busy thinking about what I'll say back.", "Bạn đang mải nghĩ mình sẽ nói lại gì" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1022,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "If I don't like the speaker, I lose interest in listening.", "Nếu không thích người đang nói, bạn mất hứng nghe" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1023,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I listen while doing something else at the same time.", "Bạn vừa nghe vừa làm việc khác cùng lúc" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1024,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I listen carefully and repeat the point back to make sure I understood.", "Bạn nghe kỹ và nhắc lại ý để chắc là mình hiểu đúng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1031,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Still use hard/technical words, assuming everyone understands like me.", "Vẫn dùng từ khó/từ chuyên môn, nghĩ ai cũng hiểu như mình" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1032,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Sometimes cause misunderstanding by using abbreviations.", "Thỉnh thoảng làm người nghe hiểu lầm vì dùng từ viết tắt" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1033,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Know the listener may not understand but am reluctant to re-explain.", "Biết người nghe có thể không hiểu nhưng ngại giải thích lại" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1034,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Choose easy words, explain clearly, with examples.", "Chọn từ dễ hiểu, giải thích rõ, có ví dụ đi kèm" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1041,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Write in ALL CAPS when in a hurry, use abbreviations even when texting teachers.", "Viết CHỮ HOA khi gấp, viết tắt cả khi nhắn cho thầy cô" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1042,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Reply right while I'm angry, which easily starts arguments.", "Trả lời ngay lúc đang bực mình, dễ gây cãi nhau" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1043,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Write in a rush without checking for spelling mistakes.", "Viết vội, không kiểm tra lại lỗi chính tả" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1044,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Write politely, spell correctly, and re-read before sending.", "Viết lịch sự, đúng chính tả, đọc lại trước khi gửi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1051,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Argue back right away, saying they're wrong.", "Cãi lại ngay, nói rằng bạn ấy sai" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1052,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Go silent and not want to talk to them anymore.", "Im lặng, không muốn nói chuyện với bạn ấy nữa" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1053,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Listen but feel down all day and lose confidence.", "Nghe nhưng buồn cả ngày, mất tự tin" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1054,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Calmly ask exactly what isn't good, and thank them for the feedback.", "Bình tĩnh hỏi rõ chỗ chưa tốt, cảm ơn vì đã góp ý" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1061,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Speak exactly the same way to everyone.", "Nói giống hệt nhau với tất cả mọi người" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1062,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Want to change how I speak but don't know how to make it fit.", "Muốn thay đổi cách nói nhưng không biết nên nói sao cho phù hợp" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1063,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Judge by appearance, then guess and speak accordingly.", "Nhìn bề ngoài người khác rồi đoán và nói chuyện theo cách đó" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1064,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Always mind who I'm talking to and where, then choose a fitting way to speak.", "Luôn để ý đang nói với ai, ở đâu, rồi chọn cách nói phù hợp" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1071,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Say it bluntly: \"this part is really bad\".", "Nói thẳng luôn \"phần này làm dở quá\"" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1072,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Don't dare say anything, just leave it be.", "Không dám nói, để im vậy" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1073,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Talk around it endlessly without stating the main point.", "Nói vòng vo mãi mà không nói rõ ý chính" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1074,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Point out clearly what needs fixing, but gently, and suggest how to improve.", "Nói rõ chỗ cần sửa, nhưng nhẹ nhàng và góp ý cách cải thiện" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1081,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Don't pay attention — often cross my arms or avoid eye contact.", "Không để ý, hay khoanh tay hoặc tránh nhìn vào mắt người khác" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1082,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Don't know whether to stand close to or far from the person.", "Không biết nên đứng gần hay xa người đang nói chuyện" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1083,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Only mind my words, forgetting my expression and posture.", "Chỉ chú ý lời mình nói, quên để ý nét mặt và tư thế" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1084,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Always keep eye contact, posture, and tone appropriate to the moment.", "Luôn giữ ánh mắt, tư thế, giọng nói phù hợp lúc đó" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3011,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Leave it and only start when the deadline is near.", "Để đó, gần đến ngày nộp mới bắt đầu làm" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3012,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Start today but with no plan, just working as I go.", "Làm ngay hôm nay nhưng không có kế hoạch, làm tới đâu hay tới đó" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3013,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Split the assignment into several short sessions, doing one part each.", "Chia bài ra làm nhiều buổi nhỏ, mỗi buổi làm một phần" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3014,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Break the work down, set a time for each part, and leave buffer time for anything unexpected.", "Chia nhỏ công việc, đặt thời gian cho từng phần và để dư thời gian phòng khi có việc phát sinh" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3021,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Afraid of doing it wrong, so I don't dare start.", "Sợ làm sai nên chưa dám bắt đầu" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3022,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Absorbed in my phone and social media.", "Mải xem điện thoại, mạng xã hội" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3023,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Tired, sleepy, no energy to work.", "Mệt, buồn ngủ, không có sức làm" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3024,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I rarely leave tasks like that — I usually do them right away.", "Bạn ít khi để việc đó lại, thường làm ngay" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3031,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Don't think about how long it will take, just do it and sort it out later.", "Không nghĩ mất bao lâu, cứ làm rồi tính sau" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3032,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Guess a random figure without checking it.", "Đoán đại một con số, không kiểm tra lại" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3033,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Estimate, but are often wrong because you don't account for unexpected time.", "Ước lượng nhưng thường sai vì không tính thời gian phát sinh" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3034,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Estimate carefully and leave buffer time in case something unexpected comes up.", "Ước lượng cẩn thận và để dư thời gian phòng khi có việc bất ngờ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3041,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Phone, messages, games.", "Điện thoại, tin nhắn, trò chơi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3042,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Surrounding noise, a messy workspace.", "Tiếng ồn xung quanh, chỗ ngồi bừa bộn" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3043,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Doing many things at once, so everything is slow.", "Làm nhiều việc cùng lúc nên việc nào cũng chậm" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3044,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I always find a quiet place and turn off my phone when studying.", "Bạn luôn tìm chỗ yên tĩnh và tắt điện thoại khi học" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3051,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Drop it entirely if it seems a bit hard to apply.", "Bỏ luôn nếu thấy hơi khó áp dụng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3052,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Only do it when someone reminds me.", "Chỉ làm khi có ai đó nhắc" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3053,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Try to keep it up but often forget, doing it inconsistently.", "Cố duy trì nhưng hay quên, không đều" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3054,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Persist with it steadily, adjusting it to fit myself.", "Kiên trì áp dụng đều đặn, điều chỉnh cho phù hợp với mình" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3061,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Do whatever others ask first, regardless of how important that task is.", "Làm việc nào người khác nhờ trước, không cần biết việc đó quan trọng ra sao" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3062,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Do the easy tasks first to get them done quickly.", "Việc nào dễ thì làm trước cho xong nhanh" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3063,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Do the urgent tasks first, regardless of whether they're important.", "Việc nào gấp thì làm trước, không cần biết có quan trọng không" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3064,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Do the important and necessary tasks first, even if they aren't very urgent yet.", "Làm việc quan trọng và cần thiết trước, dù nó chưa gấp lắm" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3071,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Set very general goals, hard to know when they're achieved (e.g. \"study better\").", "Đặt mục tiêu rất chung chung, khó biết khi nào đạt được (ví dụ: \"học giỏi hơn\")" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3072,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Set clear goals but with no due date.", "Đặt mục tiêu rõ nhưng không hẹn ngày xong" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3073,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Set clear goals with a due date, but they're hard to actually accomplish.", "Đặt mục tiêu rõ, có ngày hẹn, nhưng khó thực hiện nổi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3074,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Set clear goals, with a due date, and get them done.", "Đặt mục tiêu rõ ràng, có ngày hẹn, và làm được" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3081,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Worry, feel down, then give up on it entirely.", "Lo lắng, buồn, rồi bỏ luôn không làm nữa" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3082,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Try to follow the old plan even when it no longer makes sense, feeling very stressed.", "Cố làm theo đúng kế hoạch cũ dù không còn hợp lý, thấy rất căng thẳng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3083,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Figure it out alone without telling anyone.", "Tự tìm cách giải quyết một mình, không nói cho ai biết" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3084,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Calmly review it, tell the people involved, and switch to a new plan.", "Bình tĩnh xem lại, nói cho người liên quan biết và đổi kế hoạch mới" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4011,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Believe it right away because many people praise it.", "Tin ngay vì có nhiều người khen" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4012,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Believe it because the article looks nice and professional.", "Tin vì bài viết trình bày đẹp, chuyên nghiệp" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4013,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Half believe it but still share it with friends.", "Nửa tin nửa ngờ nhưng vẫn chia sẻ cho bạn bè" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4014,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Look for other sources (reputable news, experts) to verify before believing it.", "Tìm thêm nguồn khác (báo uy tín, chuyên gia) để kiểm chứng trước khi tin" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4021,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Skip it immediately without finishing, sure that you're right.", "Bỏ qua ngay, không đọc hết vì nghĩ chắc chắn mình đúng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4022,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Read it but look for ways to refute it rather than consider it carefully.", "Đọc nhưng tìm cách bác bỏ thay vì xem xét kỹ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4023,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Read it and keep only the parts that favor your own thinking.", "Đọc và chỉ giữ lại phần nào có lợi cho suy nghĩ của mình" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4024,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Read it all, weigh the reasoning, and be ready to change if it makes sense.", "Đọc hết, xem xét lý lẽ, và sẵn sàng thay đổi nếu thấy hợp lý" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4031,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Use over-the-top examples to distract and confuse others.", "Dùng ví dụ quá đáng để đánh lạc hướng, làm người khác rối" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4032,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Badmouth or belittle whoever opposes your idea.", "Nói xấu, chê bai người phản đối ý mình" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4033,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Follow the majority, not daring to voice your own opinion.", "Nghe theo số đông, không dám nói ý kiến riêng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4034,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Focus on giving clear reasons, without twisting the truth just to win.", "Tập trung nói lý do rõ ràng, không nói sai sự thật chỉ để thắng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4041,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Believe it because many people are sharing it.", "Tin vì thấy nhiều người chia sẻ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4042,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Believe it because the headline is sensational, there must be something to it.", "Tin vì tiêu đề giật gân, chắc phải có gì đó" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4043,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Doubt it but still share it because it's interesting.", "Nghi ngờ nhưng vẫn chia sẻ vì thấy thú vị" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4044,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Check whether any reputable outlet has reported it before believing it.", "Tìm xem tin đó có được báo uy tín nào đăng không rồi mới tin" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4051,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Assume the tests are just hard and not look into it further.", "Nghĩ do đề khó, không tìm hiểu thêm" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4052,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Memorize more similar exercises without understanding why you were wrong.", "Học thuộc thêm nhiều bài tập tương tự mà không hiểu vì sao mình sai" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4053,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Ask friends where you went wrong but stop there.", "Hỏi bạn bè điểm nào mình sai nhưng chỉ dừng ở đó" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4054,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Ask yourself \"why am I wrong\" repeatedly to find what you truly don't understand (e.g. wrong formula or not reading the question carefully).", "Tự hỏi \"tại sao mình sai\" nhiều lần để tìm ra chỗ mình chưa hiểu thật sự (ví dụ: sai công thức hay đọc đề chưa kỹ)" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4061,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Keep your old approach because you're used to it.", "Vẫn giữ cách làm cũ vì đã quen" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4062,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Switch because friends do it the teacher's way too, even without understanding why.", "Đổi theo vì thấy bạn bè cũng làm theo cách thầy cô nói, dù chưa hiểu tại sao" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4063,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Agree you were wrong but still use the old way on the next problem.", "Đồng ý là mình sai nhưng vẫn làm theo cách cũ ở bài sau" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4064,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Review the formula, understand why you were wrong, and apply the correct way from now on.", "Xem lại công thức, hiểu vì sao mình sai và áp dụng cách đúng từ bây giờ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4071,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "The first result that shows up on Google.", "Kết quả đầu tiên hiện ra khi tìm trên Google" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4072,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Whichever page says exactly what I want to say.", "Trang nào viết đúng ý mình muốn nói thì dùng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4073,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Any website whose name sounds reputable, and use it right away.", "Chỉ xem tên website nghe có vẻ uy tín là dùng luôn" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4074,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Check whether it has a clear author, was updated recently, and matches many other sources.", "Xem thông tin đó có tác giả rõ ràng, cập nhật gần đây, và khớp với nhiều nguồn khác không" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4081,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Not prepare anything extra — deal with it later if it fails.", "Không chuẩn bị gì thêm, nếu lỗi thì tính sau" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4082,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Think of a backup plan but drop it because it's too much trouble.", "Nghĩ tới phương án khác nhưng thấy mất công nên thôi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4083,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Only prepare a backup for the most important part.", "Chỉ chuẩn bị phương án dự phòng cho phần quan trọng nhất" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4084,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Prepare a USB, printed copies, or a phone backup in case the computer has trouble.", "Chuẩn bị sẵn USB, in giấy hoặc lưu trên điện thoại phòng khi máy tính gặp sự cố" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "You're explaining something to a friend, but you notice they don't seem to get it. You will:", "Bạn đang giải thích điều gì đó cho bạn mình, nhưng thấy bạn ấy có vẻ chưa hiểu. Bạn sẽ:", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When someone is talking to you, what makes it hard to listen fully?", "Khi người khác nói chuyện với bạn, điều gì khiến bạn khó nghe hết được?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When explaining to someone younger or without your background, you will:", "Khi giải thích cho em nhỏ tuổi hơn hoặc người không cùng chuyên môn với mình, bạn sẽ:" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When texting or writing, how do you usually write?", "Khi nhắn tin hoặc viết bài, bạn thường viết như thế nào?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "You just finished a piece of work and a friend points out something isn't good. You will:", "Bạn vừa làm xong một bài, bạn bè góp ý là bài có chỗ chưa tốt. Bạn sẽ:" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "Do you talk differently depending on who you're talking to?", "Bạn có nói chuyện khác nhau tùy người mình đang nói cùng không?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "You need to tell a teammate that their part of the work isn't good. How will you say it?", "Bạn cần báo cho bạn cùng nhóm biết rằng phần việc của bạn ấy làm chưa tốt. Bạn sẽ nói thế nào?", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When talking, do you pay attention to your eye contact and posture?", "Khi nói chuyện, bạn có để ý đến ánh mắt, tư thế của mình không?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "You have an assignment due in 3 days. What will you do?", "Bạn có một bài tập phải nộp sau 3 ngày nữa. Bạn sẽ làm gì?", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 302,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When there's an important task you keep putting off, what is the main reason?", "Khi có một việc quan trọng mà bạn cứ để đó chưa làm, lý do chính là gì?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 303,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "The teacher assigns a big task to be done in a week. Before starting, you:", "Cô giáo giao một bài tập lớn, cần làm trong 1 tuần. Trước khi bắt đầu, bạn sẽ:", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 304,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "What distracts you the most when studying?", "Điều gì làm bạn mất tập trung nhiều nhất khi học?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 305,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "You try a new study method (e.g. study 25 minutes - rest 5 minutes) for a few days. After that you:", "Bạn thử áp dụng một cách học mới (ví dụ: học 25 phút - nghỉ 5 phút) được vài ngày. Sau đó bạn sẽ:", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 306,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When you have many tasks at once, what do you base your decision on for what to do first?", "Khi có nhiều việc cùng lúc, bạn quyết định làm việc nào trước dựa vào điều gì?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 307,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When setting study goals for this semester, you usually:", "Khi đặt mục tiêu học tập cho học kỳ này, bạn thường:" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 308,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When an unexpected event ruins your plan, what do you do?", "Khi có việc bất ngờ làm hỏng kế hoạch của bạn, bạn làm gì?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "You read an online article claiming a food cures an illness in 3 days, with lots of praise in the comments. You will:", "Bạn đọc được một bài viết trên mạng nói một loại thực phẩm chữa khỏi bệnh trong 3 ngày, kèm rất nhiều lời khen trong phần bình luận. Bạn sẽ:", 1 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "You read an article with a view opposite to yours. You will:", "Bạn đọc một bài viết có quan điểm ngược với suy nghĩ của mình. Bạn sẽ:" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When debating in a group and being challenged, what do you usually do?", "Khi tranh luận trong nhóm và bị phản bác, bạn thường làm gì?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "You see hot news spreading on social media, but no major outlet has reported it. You will:", "Bạn thấy một tin nóng lan truyền trên mạng xã hội, nhưng không thấy báo lớn nào đăng. Bạn sẽ:" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "You keep getting low scores in Math even though you've studied. You will:", "Bạn liên tục bị điểm thấp môn Toán dù đã học bài. Bạn sẽ:" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "You're sure your way of solving a problem is right, but the teacher shows you misunderstood the formula. You will:", "Bạn tin chắc cách giải một bài toán là đúng, nhưng thầy cô chỉ ra bạn đã hiểu sai công thức. Bạn sẽ:" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 407,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When your assignment needs cited information, which source do you usually pick?", "Khi làm bài tập cần trích dẫn thông tin, bạn thường chọn nguồn nào?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 408,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "You're preparing a presentation but worry the computer might fail mid-talk. You will:", "Bạn đang chuẩn bị thuyết trình nhưng lo máy tính có thể bị lỗi lúc trình bày. Bạn sẽ:", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1011,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I focus on saying everything on my mind, paying little attention to the listener's reaction.", "Chỉ tập trung nói hết ý mình, ít để ý phản ứng người nghe" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I only check when speaking in person, and skip it in texts/emails.", "Chỉ kiểm tra khi nói trực tiếp, bỏ qua khi nhắn tin/email" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1014,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I always watch the listener's cues and proactively ask for feedback.", "Luôn quan sát tín hiệu người nghe và chủ động hỏi phản hồi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1021,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm busy preparing a rebuttal in my head and don't hear it all.", "Bận chuẩn bị phản biện trong đầu, không nghe hết" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1022,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I easily lose interest if I don't click with the speaker.", "Dễ mất hứng nếu không hợp gu với người nói" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1023,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I listen while doing other things, thinking I multitask well.", "Vừa nghe vừa làm việc riêng, nghĩ mình đa nhiệm tốt" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1024,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I listen actively and paraphrase back to confirm I understood.", "Lắng nghe chủ động, diễn đạt lại để xác nhận hiểu đúng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1031,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I use jargon/slang and assume others just understand.", "Dùng jargon/từ lóng, mặc định người khác tự hiểu" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I know the differences but am reluctant to re-explain hard terms.", "Biết khác biệt nhưng ngại giải thích lại từ khó" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1034,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I proactively choose plain words, define clearly, and give examples.", "Chủ động chọn từ dễ hiểu, định nghĩa rõ, có ví dụ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1041,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I WRITE IN CAPS when urgent and use abbreviations even with superiors.", "Viết HOA khi khẩn cấp, dùng viết tắt cả với cấp trên" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I write carelessly — no subject line, no proofreading.", "Viết tùy tiện, không tiêu đề, không kiểm tra lỗi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1044,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm always polite, grammatical, and check the tone before sending.", "Luôn lịch sự, đúng ngữ pháp, kiểm tra tông giọng trước khi gửi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1051,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I get defensive and attack the person back.", "Phản ứng phòng vệ, công kích cá nhân lại" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I try to listen but feel deeply hurt and lose confidence.", "Cố nghe nhưng tổn thương sâu, mất tự tin" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1054,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I calmly clarify the issue while saving face for the other person.", "Bình tĩnh làm rõ vấn đề, giữ thể diện cho đối phương" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1061,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I speak exactly the same way to everyone.", "Nói chuyện y hệt nhau với mọi đối tượng" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm easily swayed by bias and stereotypes when communicating.", "Dễ bị định kiến, khuôn mẫu chi phối khi giao tiếp" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1064,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I always analyze the context and audience before communicating.", "Luôn phân tích bối cảnh, khán giả trước khi giao tiếp" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1071,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I speak bluntly, sometimes hurting others.", "Nói thẳng thô, đôi khi làm tổn thương người khác" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1072,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I talk in circles until the listener can't follow my point.", "Nói vòng vo đến mức người nghe không hiểu ý" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1073,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm confused about when to be direct vs. indirect.", "Bối rối không biết khi nào nên trực tiếp/gián tiếp" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1074,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I balance it: direct for work, gentle when delivering bad news.", "Cân bằng: trực tiếp cho công việc, mềm mỏng khi tin xấu" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1081,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I can't control it — crossed arms, avoiding eye contact.", "Không kiểm soát được, khoanh tay, né ánh mắt" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I focus only on words and forget expression and posture.", "Chỉ chú trọng câu chữ, quên biểu cảm và tư thế" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 1084,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I keep my posture, eye contact, and tone appropriate to the context.", "Luôn giữ tư thế, ánh mắt, tông giọng phù hợp bối cảnh" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3011,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "It depends on whatever schedule others set for me.", "Phụ thuộc lịch người khác sắp xếp" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I have a plan but find it hard to stick to.", "Có kế hoạch nhưng khó giữ đúng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3014,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm always proactive, with my own strategy.", "Luôn chủ động, có chiến lược riêng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3021,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Fear of failure, so I avoid getting started.", "Sợ thất bại nên né tránh bắt tay vào làm" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "My body is tired, sleep-deprived, low on energy.", "Cơ thể mệt mỏi, thiếu ngủ, thiếu năng lượng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3024,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I rarely have this problem — I start right away.", "Tôi hiếm khi gặp vấn đề này, bắt tay vào việc ngay" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3031,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Way off — a task I thought was 1 hour takes 4–5.", "Sai lệch nặng, việc tưởng 1 giờ hóa ra mất 4-5 giờ" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I often pull all-nighters near the deadline to make up for it.", "Thường phải thức trắng đêm sát hạn để bù đắp" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3034,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Fairly accurate, always with buffer time.", "Ước lượng khá sát, luôn có thời gian dự phòng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3041,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I'm constantly pulled in by notifications, messages, games.", "Liên tục bị cuốn vào thông báo, tin nhắn, game" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I sometimes multitask, which lowers my output.", "Thỉnh thoảng làm nhiều việc cùng lúc, hiệu suất giảm" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3044,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I always have a quiet space with all notifications off.", "Luôn có không gian yên tĩnh, tắt hết thông báo" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3051,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "No, I only get motivated when the deadline is imminent.", "Không, chỉ có động lực khi hạn chót cận kề" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I apply them but often quit halfway (e.g. Pomodoro).", "Có áp dụng nhưng hay bỏ giữa chừng (vd: Pomodoro)" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3054,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I apply them fluently: Eat the Frog, break tasks down, daily Top 3.", "Áp dụng nhuần nhuyễn: Eat the Frog, chia việc nhỏ, Top 3 mỗi ngày" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3061,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Momentary emotion or panic over the deadline.", "Cảm xúc nhất thời hoặc hoảng loạn vì hạn chót" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3062,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I pick easy tasks first and dodge hard-but-important ones.", "Chọn việc dễ trước, né việc khó nhưng quan trọng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3063,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I make a list but struggle to sort out the core tasks.", "Có lập danh sách nhưng khó phân loại việc cốt lõi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3064,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I use the Eisenhower matrix to classify clearly.", "Dùng ma trận Eisenhower để phân loại rõ ràng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3071,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Very vague, hard to measure (\"get better\").", "Rất mơ hồ, khó đo lường (\"học giỏi hơn\")" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Specific but unrealistic, not feasible.", "Cụ thể nhưng thiếu thực tế, không khả thi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3074,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Always fully meet the SMART criteria.", "Luôn đạt chuẩn SMART đầy đủ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3081,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Panic, get discouraged, and drop the work.", "Hoảng loạn, nản chí, bỏ dở công việc" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Struggle through it alone, without telling anyone.", "Tự loay hoay giải quyết, không báo với ai" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 3084,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Calmly reassess, proactively inform others, and adjust.", "Bình tĩnh đánh giá lại, chủ động thông báo và điều chỉnh" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4011,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I instantly believe convincing claims on social media.", "Tin ngay vào tuyên bố thuyết phục trên mạng xã hội" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I distinguish well, but struggle with cleverly disguised data.", "Phân biệt tốt nhưng khó với số liệu ngụy trang tinh vi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4014,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I always distinguish clearly and demand empirical evidence.", "Luôn phân biệt rõ, yêu cầu bằng chứng thực nghiệm" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4021,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I only read news that agrees with me and think others are wrong.", "Chỉ đọc tin cùng quan điểm, nghĩ người khác sai" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I read other views but still cherry-pick what favors me.", "Đọc góc nhìn khác nhưng vẫn chọn lọc có lợi cho mình" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4024,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I proactively seek many sources and weigh opposing evidence fairly.", "Chủ động tiếp cận đa nguồn, công tâm với bằng chứng trái chiều" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4031,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Attack the other person when I'm challenged.", "Công kích cá nhân đối phương khi bị phản bác" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4032,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Follow the majority, avoiding independent thinking.", "A dua theo số đông, tránh tư duy độc lập" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4033,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Sometimes use extreme examples to distract.", "Đôi khi dùng ví dụ cực đoan để đánh lạc hướng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4034,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Focus on rational analysis, avoiding logical fallacies.", "Tập trung phân tích lý trí, tránh ngụy biện logic" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4041,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Trust a nice-looking site and the author's self-introduction.", "Tin vào giao diện đẹp, lời tự giới thiệu của tác giả" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I know I should verify but am lazy, only doing it when it matters.", "Biết cần kiểm chứng nhưng lười, chỉ làm khi quan trọng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4044,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Read laterally — open many tabs to cross-check independent sources.", "Đọc ngang — mở nhiều tab đối chiếu nguồn độc lập" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4051,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Only fix the surface, without exploring the root cause.", "Chỉ giải quyết phần nổi, không tìm hiểu gốc rễ" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4052,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Decide hastily on gut feeling.", "Quyết định vội vàng theo cảm tính" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4053,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Get stuck after a few \"why\" questions, easily going off track.", "Bế tắc sau vài câu hỏi \"tại sao\", dễ lạc hướng" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4054,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Apply the \"5 Whys\" to find the root cause.", "Áp dụng \"5 câu hỏi Tại sao\" để tìm gốc rễ vấn đề" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4061,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Stubborn — I cling to my view even when proven wrong.", "Bảo thủ, bám quan điểm dù bị chứng minh sai" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I note the new evidence but delay adjusting.", "Ghi nhận bằng chứng mới nhưng trì hoãn điều chỉnh" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4064,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I proactively update my thinking when the data changes.", "Chủ động cập nhật tư duy khi dữ liệu thay đổi" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4071,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "Pick the first Google result, assuming it's most authoritative.", "Chọn kết quả đầu tiên trên Google, tin là uy tín nhất" });

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
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I only check the author/domain, ignoring conflicts of interest.", "Chỉ xem tên tác giả/tên miền, bỏ qua xung đột lợi ích" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4074,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I review comprehensively: author, recency, funding source, peer review.", "Rà soát toàn diện: tác giả, tính cập nhật, nguồn tài trợ, bình duyệt" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4081,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I think of only one solution, helpless when it fails.", "Chỉ nghĩ 1 giải pháp, bất lực khi nó thất bại" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4082,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I think about it but give up, seeing it as time-consuming.", "Có nghĩ đến nhưng bỏ cuộc vì thấy tốn thời gian" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4083,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I only prepare a backup for big things, improvising the rest.", "Chỉ chuẩn bị dự phòng cho việc lớn, còn lại tùy cơ ứng biến" });

            migrationBuilder.UpdateData(
                table: "AssessmentOptions",
                keyColumn: "Id",
                keyValue: 4084,
                columns: new[] { "OptionText", "OptionTextVi" },
                values: new object[] { "I always build multiple options and concrete contingency plans.", "Luôn xây nhiều phương án và kế hoạch dự phòng cụ thể" });

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
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "How do you deal with differences in wording and terminology?", "Bạn xử lý sự khác biệt về từ ngữ, thuật ngữ thế nào?" });

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
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "How are your study/work goals written?", "Mục tiêu học tập/công việc của bạn được viết ra như thế nào?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 308,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "When your plan is broken by an unexpected event, how do you react?", "Khi kế hoạch bị phá vỡ bởi biến cố bất ngờ, bạn phản ứng ra sao?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "How well do you separate fact from personal opinion?", "Bạn phân biệt sự thật và ý kiến cá nhân tốt đến đâu?", 0 });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "How do you deal with your own confirmation bias?", "Bạn đối phó với thiên kiến xác nhận của bản thân thế nào?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "In a group debate, how do you usually react?", "Khi tranh luận nhóm, bạn thường phản ứng ra sao?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "How do you verify an unfamiliar source online?", "Bạn kiểm chứng một nguồn tin lạ trên mạng ra sao?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "Facing a complex problem, how do you find a solution?", "Khi gặp vấn đề phức tạp, bạn tìm giải pháp thế nào?" });

            migrationBuilder.UpdateData(
                table: "AssessmentQuestions",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "QuestionText", "QuestionTextVi" },
                values: new object[] { "Are you willing to change your view when new evidence appears?", "Bạn có sẵn sàng thay đổi quan điểm khi có bằng chứng mới không?" });

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
                columns: new[] { "QuestionText", "QuestionTextVi", "Type" },
                values: new object[] { "When facing a problem, do you prepare multiple options?", "Khi gặp vấn đề, bạn có chuẩn bị nhiều phương án không?", 0 });
        }
    }
}
