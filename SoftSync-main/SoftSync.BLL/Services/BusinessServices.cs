using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;

namespace SoftSync.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    public UserService(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return null;
        return new UserDto { Id = user.Id, FullName = user.FullName, Age = user.Age, Role = user.Role, Gender = user.Gender, Goal = user.Goal, AvatarUrl = user.AvatarUrl, ExperiencePoints = user.ExperiencePoints, CreatedAt = user.CreatedAt };
    }

    public async Task<UserDto> CreateUserAsync(UserDto dto)
    {
        var user = new ApplicationUser { FullName = dto.FullName, Age = dto.Age, Goal = dto.Goal, CreatedAt = DateTime.UtcNow };
        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();
        dto.Id = user.Id;
        return dto;
    }

    public async Task UpdateProfileAsync(int userId, UserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return;
        if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName;
        user.Age = dto.Age;
        user.Role = dto.Role;
        user.Gender = dto.Gender;
        user.Goal = dto.Goal;
        if (!string.IsNullOrWhiteSpace(dto.AvatarUrl)) user.AvatarUrl = dto.AvatarUrl;
        await _userRepo.SaveChangesAsync();
    }

    public async Task SetAvatarAsync(int userId, string avatarUrl)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return;
        user.AvatarUrl = avatarUrl;
        await _userRepo.SaveChangesAsync();
    }

    public async Task AddExperienceAsync(int userId, int amount)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return;
        user.ExperiencePoints += amount;
        await _userRepo.SaveChangesAsync();
    }

    public async Task AddSkillSelectionsAsync(int userId, List<int> skillIds)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user != null)
        {
            // Simple logic for MVP: just clear and add
            foreach (var sid in skillIds)
            {
                user.SkillSelections.Add(new UserSkillSelection { UserId = userId, SkillId = sid });
            }
            await _userRepo.SaveChangesAsync();
        }
    }
}

public class SkillService : ISkillService
{
    private readonly ISkillRepository _skillRepo;
    public SkillService(ISkillRepository skillRepo) => _skillRepo = skillRepo;
    public async Task<IEnumerable<SkillDto>> GetAllSkillsAsync()
    {
        var skills = await _skillRepo.GetAllAsync();
        return skills.Select(s => new SkillDto { Id = s.Id, Name = s.Name, Description = s.Description, IconName = s.IconName });
    }
}

public class AssessmentService : IAssessmentService
{
    private readonly IAssessmentRepository _assessmentRepo;
    private readonly IUserRepository _userRepo;
    private readonly IAiAssessmentService _aiService;

    public AssessmentService(IAssessmentRepository assessmentRepo, IUserRepository userRepo, IAiAssessmentService aiService)
    {
        _assessmentRepo = assessmentRepo;
        _userRepo = userRepo;
        _aiService = aiService;
    }

    public async Task<IEnumerable<AssessmentQuestionDto>> GetAssessmentQuestionsAsync(int userId)
    {
        // Quiz only the skills the user selected in the wizard. If none were
        // selected, fall back to all skills so the assessment is never empty.
        var skillIds = await _assessmentRepo.GetSelectedSkillIdsAsync(userId);
        if (skillIds.Count == 0)
            skillIds = new List<int> { 1, 2, 3, 4, 5, 6, 7 };

        var questions = await _assessmentRepo.GetQuestionsBySkillIdsAsync(skillIds);
        return questions.Select(q => new AssessmentQuestionDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            QuestionTextVi = q.QuestionTextVi,
            SkillId = q.SkillId,
            SkillName = q.Skill?.Name ?? string.Empty,
            SkillNameVi = SkillNameVi(q.SkillId),
            Options = q.Options
                .Select(o => new AssessmentOptionDto { Id = o.Id, OptionText = o.OptionText, OptionTextVi = o.OptionTextVi })
                .ToList()
        });
    }

    // Vietnamese labels for the fixed set of seeded skills (ids match SoftSyncDbContext).
    private static string SkillNameVi(int skillId) => skillId switch
    {
        1 => "Giao tiếp",
        2 => "Làm việc nhóm",
        3 => "Quản lý thời gian",
        4 => "Tư duy phản biện",
        5 => "Giải quyết vấn đề",
        6 => "Quản lý cảm xúc",
        7 => "Khả năng thích ứng",
        _ => string.Empty
    };

    public async Task SubmitAssessmentAsync(int userId, List<UserAnswerDto> answers)
    {
        // Score for real: load each chosen option (with its ScoreValue and the
        // owning question's SkillId), then average per skill into a 0–100 percentage.
        var optionIds = answers.Select(a => a.OptionId).Where(id => id > 0).Distinct().ToList();
        var chosen = (await _assessmentRepo.GetAnsweredOptionsAsync(optionIds))
            .ToDictionary(o => o.Id);

        var perSkill = new Dictionary<int, (int sum, int count)>();
        foreach (var ans in answers)
        {
            if (!chosen.TryGetValue(ans.OptionId, out var opt) || opt.Question is null)
                continue;
            var skillId = opt.Question.SkillId;
            var agg = perSkill.TryGetValue(skillId, out var cur) ? cur : (sum: 0, count: 0);
            perSkill[skillId] = (agg.sum + opt.ScoreValue, agg.count + 1);
        }

        var now = DateTime.UtcNow;
        var results = new List<AssessmentResult>();
        foreach (var (skillId, agg) in perSkill)
        {
            // Percentage of the maximum attainable score for the answered questions.
            var maxPossible = agg.count * QuizSeedData.MaxOptionScore;
            var score = maxPossible > 0 ? (int)Math.Round(agg.sum * 100.0 / maxPossible) : 0;
            var level = score < 50 ? AssessmentLevel.Weak
                : score < 80 ? AssessmentLevel.Average
                : AssessmentLevel.Good;

            results.Add(new AssessmentResult
            {
                UserId = userId,
                SkillId = skillId,
                Score = score,
                Level = level,
                CreatedAt = now
            });
        }

        if (results.Count > 0)
            await _assessmentRepo.SaveResultsAsync(results);

        // Award XP for completing an assessment.
        var user = await _userRepo.GetByIdAsync(userId);
        if (user != null)
        {
            user.ExperiencePoints += SoftSync.Common.LevelSystem.AssessmentXp;
            await _userRepo.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<AssessmentResultDto>> GetLatestResultsAsync(int userId)
    {
        var results = await _assessmentRepo.GetResultsByUserIdAsync(userId);
        return results.Select(r => new AssessmentResultDto
        {
            Id = r.Id,
            UserId = r.UserId,
            SkillId = r.SkillId,
            SkillName = r.Skill.Name,
            Score = r.Score,
            Level = r.Level,
            CreatedAt = r.CreatedAt
        });
    }
}

public class RoadmapService : IRoadmapService
{
    private readonly IRoadmapRepository _roadmapRepo;
    private readonly IAiRoadmapService _aiService;
    private readonly IUserRepository _userRepo;
    public RoadmapService(IRoadmapRepository roadmapRepo, IAiRoadmapService aiService, IUserRepository userRepo)
    {
        _roadmapRepo = roadmapRepo;
        _aiService = aiService;
        _userRepo = userRepo;
    }

    public async Task<RoadmapDto> GetUserRoadmapAsync(int userId)
    {
        var items = await _roadmapRepo.GetByUserIdAsync(userId);
        if (!items.Any())
        {
            var fakeRoadmap = await _aiService.GenerateRoadmapAsync(userId, new List<string> { "Communication" });
            foreach (var item in fakeRoadmap.Items)
            {
                await _roadmapRepo.AddAsync(new RoadmapItem { UserId = userId, WeekNumber = item.WeekNumber, Title = item.Title, Description = item.Description });
            }
            await _roadmapRepo.SaveChangesAsync();
            items = await _roadmapRepo.GetByUserIdAsync(userId);
        }

        return new RoadmapDto
        {
            UserId = userId,
            Items = items.Select(i => new RoadmapItemDto { Id = i.Id, WeekNumber = i.WeekNumber, Title = i.Title, Description = i.Description, IsCompleted = i.IsCompleted }).ToList()
        };
    }

    public async Task MarkCompleteAsync(int itemId)
    {
        var item = await _roadmapRepo.GetByIdAsync(itemId);
        if (item != null && !item.IsCompleted)
        {
            item.IsCompleted = true;
            await _roadmapRepo.SaveChangesAsync();

            // Award XP once, only on the transition to completed.
            var user = await _userRepo.GetByIdAsync(item.UserId);
            if (user != null)
            {
                user.ExperiencePoints += SoftSync.Common.LevelSystem.RoadmapItemXp;
                await _userRepo.SaveChangesAsync();
            }
        }
    }
}

public class ProgressService : IProgressService
{
    private readonly IProgressRepository _progressRepo;
    public ProgressService(IProgressRepository progressRepo) => _progressRepo = progressRepo;
    public async Task<IEnumerable<ProgressDto>> GetUserProgressAsync(int userId)
    {
        var logs = await _progressRepo.GetByUserIdAsync(userId);
        return logs.Select(l => new ProgressDto { UserId = l.UserId, SkillId = l.SkillId, SkillName = l.Skill.Name, PercentComplete = l.PercentComplete, UpdatedAt = l.UpdatedAt });
    }
}

public class CaseStudyService : ICaseStudyService
{
    private readonly ICaseStudyRepository _repo;
    public CaseStudyService(ICaseStudyRepository repo) => _repo = repo;
    public async Task<IEnumerable<CaseStudyDto>> GetCaseStudiesBySkillAsync(int skillId)
    {
        var list = await _repo.GetBySkillIdAsync(skillId);
        return list.Select(cs => new CaseStudyDto { Id = cs.Id, Title = cs.Title, Scenario = cs.Scenario, SkillId = cs.SkillId });
    }
    public async Task<CaseStudyDto?> GetCaseStudyByIdAsync(int id)
    {
        var cs = await _repo.GetWithDetailsAsync(id);
        if (cs == null) return null;
        return new CaseStudyDto
        {
            Id = cs.Id,
            Title = cs.Title,
            Scenario = cs.Scenario,
            SkillId = cs.SkillId,
            Options = cs.Options.Select(o => new CaseStudyOptionDto { Id = o.Id, OptionText = o.OptionText, IsRecommended = o.IsRecommended, Feedback = o.Feedback }).ToList()
        };
    }
}

public class MentorService : IMentorService
{
    private readonly IMentorRepository _repo;
    public MentorService(IMentorRepository repo) => _repo = repo;
    public async Task<IEnumerable<MentorDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(m => new MentorDto { Id = m.Id, Name = m.Name, Expertise = m.Expertise, ShortBio = m.ShortBio, AvatarUrl = m.AvatarUrl });
    }
}
