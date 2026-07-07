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
        // Simulate AI logic: more answers = better score
        int score = (answers.Count * 23) % 100;
        var level = score < 50 ? AssessmentLevel.Weak : score < 80 ? AssessmentLevel.Average : AssessmentLevel.Good;

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
public class FakeAiRoadmapService : IAiRoadmapService
{
    public Task<RoadmapDto> GenerateRoadmapAsync(int userId, List<string> weakSkills)
    {
        var roadmap = new RoadmapDto
        {
            UserId = userId,
            Items = new List<RoadmapItemDto>
            {
                new RoadmapItemDto { WeekNumber = 1, Title = "Khám phá bản thân", Description = "Hoàn thành bài đánh giá năng lực và xác định mục tiêu.", IsCompleted = false },
                new RoadmapItemDto { WeekNumber = 2, Title = "Kỹ năng Giao tiếp cơ bản", Description = "Tham gia mini-game về tình huống đối thoại.", IsCompleted = false },
                new RoadmapItemDto { WeekNumber = 3, Title = "Kỹ năng Làm việc nhóm", Description = "Học cách giải quyết xung đột trong team.", IsCompleted = false },
                new RoadmapItemDto { WeekNumber = 4, Title = "Tổng kết tháng", Description = "Xem lại tiến độ và nhận feedback từ AI Mentor.", IsCompleted = false }
            }
        };
        return Task.FromResult(roadmap);
    }
}
