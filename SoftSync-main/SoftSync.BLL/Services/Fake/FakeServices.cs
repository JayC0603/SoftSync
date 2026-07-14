using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;

namespace SoftSync.BLL.Services.Fake;

// TODO [AI-TEAM: Kiệt/Chánh] - Thay FakeAiAssessmentService bằng implementation
// gọi API AI thật tại đây. Giữ nguyên interface IAiAssessmentService (ở SoftSync.BLL),
// chỉ cần đổi class đăng ký trong DI tại Program.cs.
public class FakeAiAssessmentService : IAiAssessmentService
{
    public Task<AssessmentResultDto> EvaluateAsync(List<UserAnswerDto> answers)
    {
        // Simulate AI logic. Real scoring lives in AssessmentService; this fake is
        // only a placeholder. Produce a raw score in the valid 8–32 range and band it.
        int score = SoftSync.Common.AssessmentScoring.MinScore
            + (answers.Count % (SoftSync.Common.AssessmentScoring.MaxScore - SoftSync.Common.AssessmentScoring.MinScore + 1));
        var level = SoftSync.Common.AssessmentScoring.BandFor(score);

        return Task.FromResult(new AssessmentResultDto
        {
            SkillId = 1, // Defaulting to Communication for demo
            Score = score,
            Level = level,
            CreatedAt = DateTime.UtcNow
        });
    }
}

// TODO [AI-TEAM: Kiệt/Chánh] - Thay FakeAiAssistantService bằng implementation
// gọi API chatbot thật tại đây.
public class FakeAiAssistantService : IAiAssistantService
{
    public Task<string> GetReplyAsync(string userMessage, int userId)
    {
        var english = userMessage.StartsWith("[en]");
        userMessage = userMessage.Length > 4 && (english || userMessage.StartsWith("[vi]")) ? userMessage[4..] : userMessage;
        if (english)
        {
            var lower = userMessage.ToLowerInvariant();
            var enReply = lower.Contains("communication") || lower.Contains("giao tiếp")
                ? "To improve communication, start by practicing active listening and asking clear follow-up questions."
                : lower.Contains("roadmap") || lower.Contains("lộ trình")
                    ? "Your personalized learning roadmap is available in the Roadmap section."
                    : $"Thanks for your message: '{userMessage}'. I'm ready to support your soft-skills learning journey.";
            return Task.FromResult(enReply);
        }
        string reply = "Chào bạn! Tôi là trợ lý SoftSync AI. ";
        if (userMessage.ToLower().Contains("giao tiếp"))
            reply += "Để cải thiện kỹ năng giao tiếp, hãy bắt đầu bằng việc lắng nghe chủ động hơn nhé.";
        else if (userMessage.ToLower().Contains("lộ trình"))
            reply += "Tôi đã tạo lộ trình học tập cá nhân hóa cho bạn trong tab Roadmap rồi đấy!";
        else
            reply += $"Cảm ơn bạn đã nhắn tin: '{userMessage}'. Tôi sẵn sàng đồng hành cùng bạn trên con đường phát triển kỹ năng mềm.";

        return Task.FromResult(reply);
    }
}

// TODO [AI-TEAM: Kiệt/Chánh] - Thay FakeAiRoadmapService bằng implementation
// sinh lộ trình thật từ LLM/AI.
//
// Hiện tại roadmap được sinh cơ học từ danh sách kỹ năng yếu (do RoadmapService
// truyền vào, đã lọc theo QuizSeedData.ActiveSkillIds — chỉ 3 kỹ năng active).
// Mỗi kỹ năng cấp 2 tuần (nền tảng → thực hành). Số tuần vì vậy thay đổi theo
// đúng số kỹ năng người học cần cải thiện thay vì chèn các tuần chung giống nhau.
// Nội dung mô tả bám theo tên kỹ năng lấy từ DB, không tham chiếu tài liệu ngoài.
public class FakeAiRoadmapService : IAiRoadmapService
{
    public Task<RoadmapDto> GenerateRoadmapAsync(int userId, List<string> weakSkills)
    {
        var items = new List<RoadmapItemDto>();
        var week = 1;

        // 2 tuần cho mỗi kỹ năng người dùng cần cải thiện (đã được caller giới hạn
        // trong 3 kỹ năng active, sắp xếp theo điểm yếu → mạnh).
        foreach (var skill in weakSkills)
        {
            var vi = ToVietnameseSkillName(skill);
            items.Add(new RoadmapItemDto
            {
                WeekNumber = week++,
                Title = $"Nền tảng {vi}",
                Description = $"Nắm lý thuyết cốt lõi của {vi.ToLower()} và làm bài luyện tập cơ bản để hình thành thói quen."
            });
            items.Add(new RoadmapItemDto
            {
                WeekNumber = week++,
                Title = $"Thực hành {vi}",
                Description = $"Áp dụng {vi.ToLower()} vào tình huống thực tế qua case study và tự phản tư sau mỗi buổi."
            });
        }

        return Task.FromResult(new RoadmapDto { UserId = userId, Items = items });
    }

    /// <summary>Ánh xạ tên kỹ năng trong DB (tiếng Anh) sang tên hiển thị tiếng Việt cho roadmap.</summary>
    private static string ToVietnameseSkillName(string englishName) => englishName switch
    {
        "Communication"     => "Kỹ năng Giao tiếp",
        "Time Management"   => "Kỹ năng Quản lý thời gian",
        "Critical Thinking" => "Kỹ năng Tư duy phản biện",
        _ => englishName
    };
}
