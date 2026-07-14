using SoftSync.Common.Dtos;

namespace SoftSync.Presentation.Services;

public sealed record RoleplayScenario(int Id, string TitleVi, string TitleEn, string ContextVi, string ContextEn, string OpeningVi, string OpeningEn);

public static class CommunicationRoleplayEngine
{
    public static readonly IReadOnlyList<RoleplayScenario> Scenarios =
    [
        S(1,"Thành viên trễ hạn vì bận đi làm thêm","A teammate is late because of a part-time job","Còn 3 ngày đến hạn; Nam chưa gửi phần tóm tắt vì làm ca tối.","Three days remain; Nam has not sent his summary because of night shifts.","Xin lỗi cả nhóm, mình chưa gửi phần tóm tắt được. Đi làm ca tối về mệt quá. Mọi người làm trước phần mình được không?","Sorry, I have not sent my summary. Night shifts leave me exhausted. Can everyone work on their parts first?"),
        S(2,"Roommate làm ồn khi bạn cần ôn thi","A noisy roommate before your exam","Bạn thi lúc 7h sáng; Huy đang chơi game bằng loa ngoài lúc 22h.","Your exam is at 7 a.m.; Huy is gaming loudly at 10 p.m.","Ê, trận này căng quá, cố nốt ván là lên rank rồi!","This match is intense—one more game and I rank up!"),
        S(3,"Xin gia hạn vì ốm đột xuất","Requesting an extension due to illness","Hạn bài trưa mai; bạn nhập viện vì sốt xuất huyết.","The paper is due tomorrow; you are hospitalized with dengue fever.","Đề cương ghi rõ nộp trễ không lý do chính đáng là điểm 0. Em giải thích thế nào?","The syllabus says unjustified late work receives zero. How do you explain this?"),
        S(4,"Tự ý sửa bài của bạn","Editing a teammate's work without permission","Bạn sửa toàn bộ kịch bản Lan viết mà chưa hỏi.","You rewrote Lan's entire script without asking.","Sao bạn tự ý sửa sạch kịch bản mình thức cả đêm viết mà không nói một lời?","Why did you rewrite the script I worked on all night without saying anything?"),
        S(5,"Giữ đúng ngân sách làm mô hình","Keeping a project within budget","Ngân sách 500.000đ; Quân muốn mỗi người đóng thêm 200.000đ để in 3D.","The budget is 500,000 VND; Quan wants everyone to add 200,000 VND for 3D printing.","In 3D đẹp lắm! Mỗi người đóng thêm 200k đi, làm thủ công nhìn nhếch nhác lắm.","3D printing looks great! Everyone should add 200,000; handmade work looks shabby."),
        S(6,"Chọn địa điểm họp phù hợp","Choosing an inclusive meeting place","Bốn người muốn quán cà phê ồn; Pat và Vy không thoải mái.","Four people want a noisy café; Pat and Vy are uncomfortable.","Mọi người đi quán đi... chắc mình với Vy xin vắng rồi đọc biên bản sau.","You can meet at the café... Vy and I may skip it and read the notes later."),
        S(7,"Phản hồi khi điểm bị nhập sai","Responding to an incorrect grade","Hệ thống ghi 7.0 nhưng bạn tính được 8.5.","The system shows 7.0, but your calculation gives 8.5.","Em có chắc là không tính sai không? Quy trình chấm của tôi khá chặt chẽ.","Are you sure you did not miscalculate? My grading process is rigorous."),
        S(8,"Bị phê bình lỗi lập luận trước lớp","Receiving public criticism","Thầy nhận xét chương 2 phiến diện và thiếu dẫn chứng.","The lecturer says chapter two is one-sided and lacks evidence.","Slide đẹp, nhưng lập luận chương 2 phiến diện và không có số liệu. Nhóm em nghĩ sao?","The slides look good, but chapter two is one-sided and has no data. What does your group think?"),
        S(9,"Xin đổi ca làm vì trùng lịch thi","Requesting a shift change for an exam","Ca tối thứ Bảy trùng lịch thi vấn đáp.","Your Saturday evening shift conflicts with an oral exam.","Tối thứ Bảy đông khách, em báo nghỉ vậy ai đứng quầy? Anh không duyệt được.","Saturday is busy. Who will cover the counter if you leave? I cannot approve this."),
        S(10,"Xin tư vấn sau khi trượt môn tiên quyết","Seeking advice after failing a prerequisite","Bạn trượt Toán rời rạc và có nguy cơ trễ tốt nghiệp.","You failed Discrete Mathematics and may graduate late.","Việc trượt môn ảnh hưởng lớn đến kỳ sau. Em đã nghĩ vì sao mình bị điểm kém chưa?","Failing this course seriously affects next term. Have you considered why your grade was low?")
    ];

    public static RoadmapRoleplayAttemptDto Grade(int scenarioId, List<string> userMessages, List<string> aiMessages, bool vi)
    {
        var text = string.Join(' ', userMessages).ToLowerInvariant();
        var attacking = Has(text,"lười","vô ý thức","tại bạn","you are lazy","your fault","dẹp ngay","không quan tâm");
        var empathy = Has(text,"mình hiểu","mình biết","thông cảm","chia sẻ","i understand","i see","sorry","xin lỗi");
        var question = text.Contains('?');
        var iStatement = Has(text,"mình cảm thấy","mình lo","mình cần","em xin","i feel","i am concerned","i need");
        var solution = Has(text,"có thể","hay mình","đề xuất","gửi trước","chia","thời gian","could","let's","by ","before","deadline");
        var timeBound = text.Any(char.IsDigit) || Has(text,"tối mai","ngày mai","thứ sáu","tomorrow","tonight");
        var eq = attacking ? .5 : empathy ? 3 : 2;
        var listen = empathy && question ? 3 : question ? 2 : empathy ? 1.5 : .5;
        var iScore = attacking ? 0 : iStatement ? 2 : 1;
        var solutionScore = solution && timeBound ? 2 : solution ? 1 : 0;
        var total = Math.Round(eq + listen + iScore + solutionScore, 1);
        var feedback = vi
            ? $"Bạn đạt {total}/10. Hãy tiếp tục giữ bình tĩnh, xác nhận điều đã nghe, dùng câu 'mình cảm thấy/mình lo' và chốt giải pháp có thời hạn cụ thể."
            : $"You scored {total}/10. Stay calm, confirm what you heard, use I-statements, and close with a time-bound solution.";
        return new() { ScenarioId = scenarioId, UserMessages = userMessages, AiMessages = aiMessages, EmotionalIntelligenceScore = eq, ActiveListeningScore = listen, IStatementScore = iScore, SolutionScore = solutionScore, TotalScore = total, Feedback = feedback };
    }

    public static string Reply(string input, int turn, bool vi)
    {
        var positive = Has(input.ToLowerInvariant(),"mình hiểu","xin lỗi","mình lo","có thể","i understand","sorry","could","i feel");
        if (turn == 1) return positive ? (vi ? "Cảm ơn bạn đã lắng nghe. Nhưng mình vẫn còn lo là cách giải quyết này có thực tế không?" : "Thank you for listening. I am still worried whether this solution is realistic.") : (vi ? "Mình thấy bạn chưa thật sự hiểu khó khăn của mình. Bạn có thể nói rõ điều bạn cần không?" : "I do not feel understood yet. Could you clarify what you need?");
        return positive ? (vi ? "Mình hiểu rồi. Bạn có thể đề xuất một bước cụ thể và thời hạn để cả hai cùng thống nhất không?" : "I understand. Can you suggest a concrete next step and deadline?") : (vi ? "Chúng ta đang căng thẳng hơn. Hãy thử nói về cảm nhận và một giải pháp cụ thể nhé." : "This is becoming tense. Try expressing your feelings and proposing a concrete solution.");
    }

    private static bool Has(string value, params string[] terms) => terms.Any(value.Contains);
    private static RoleplayScenario S(int id,string tv,string te,string cv,string ce,string ov,string oe) => new(id,tv,te,cv,ce,ov,oe);
}
