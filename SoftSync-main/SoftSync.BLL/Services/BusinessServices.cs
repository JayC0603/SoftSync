using SoftSync.BLL.Interfaces;
using SoftSync.Common;
using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;
using System.Text.Json;

namespace SoftSync.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    public UserService(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return null;
        return new UserDto
        {
            Id = user.Id, FullName = user.FullName, Age = user.Age, Role = user.Role,
            Gender = user.Gender, Goal = user.Goal, AvatarUrl = user.AvatarUrl,
            ExperiencePoints = user.ExperiencePoints, CreatedAt = user.CreatedAt,
            DisplayName = user.DisplayName,
            CurrentLevel = user.CurrentLevel, DailyStudyMinutes = user.DailyStudyMinutes,
            StudyDaysPerWeek = user.StudyDaysPerWeek, PreferredStudyTime = user.PreferredStudyTime,
            PreferredLanguage = user.PreferredLanguage, Theme = user.Theme, ReduceMotion = user.ReduceMotion
        };
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
        // Idempotent replace: users can re-run the wizard, so we must survive
        // duplicate submissions. Old code just Added rows and hit
        // "Violation of PRIMARY KEY constraint PK_UserSkillSelections" on
        // (UserId, SkillId) the second time. Diff instead: keep what's still
        // selected, drop what was unselected, add what's new.
        var user = await _userRepo.GetWithSkillSelectionsAsync(userId);
        if (user == null) return;

        var desired = new HashSet<int>(skillIds);
        var existing = user.SkillSelections.ToDictionary(s => s.SkillId);

        // Remove selections the user no longer wants.
        foreach (var s in existing.Values.Where(s => !desired.Contains(s.SkillId)).ToList())
            user.SkillSelections.Remove(s);

        // Add newly selected skills only.
        foreach (var sid in desired.Where(id => !existing.ContainsKey(id)))
            user.SkillSelections.Add(new UserSkillSelection { UserId = userId, SkillId = sid });

        await _userRepo.SaveChangesAsync();
    }

    public async Task UpdateSettingsAsync(int userId, UserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return;

        // Account / display
        if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName;
        user.DisplayName = dto.DisplayName?.Trim() ?? string.Empty;
        user.Gender = dto.Gender;
        if (dto.Age > 0) user.Age = dto.Age;

        // Learning personalization
        user.Goal = dto.Goal;
        user.CurrentLevel = dto.CurrentLevel;
        user.DailyStudyMinutes = dto.DailyStudyMinutes;
        user.StudyDaysPerWeek = Math.Clamp(dto.StudyDaysPerWeek, 0, 7);
        user.PreferredStudyTime = dto.PreferredStudyTime;

        // Appearance
        user.PreferredLanguage = dto.PreferredLanguage?.Trim() ?? string.Empty;
        user.Theme = dto.Theme;
        user.ReduceMotion = dto.ReduceMotion;

        await _userRepo.SaveChangesAsync();
    }

    public async Task<List<int>> GetSelectedSkillIdsAsync(int userId)
    {
        var user = await _userRepo.GetWithSkillSelectionsAsync(userId);
        return user?.SkillSelections.Select(s => s.SkillId).ToList() ?? new List<int>();
    }
}

public class SkillService : ISkillService
{
    private readonly ISkillRepository _skillRepo;
    public SkillService(ISkillRepository skillRepo) => _skillRepo = skillRepo;
    public async Task<IEnumerable<SkillDto>> GetAllSkillsAsync()
    {
        // Only surface skills that have a finalized quiz bank. Others stay hidden
        // until re-enabled in QuizSeedData.ActiveSkillIds.
        var skills = await _skillRepo.GetAllAsync();
        return skills
            .Where(s => QuizSeedData.ActiveSkillIds.Contains(s.Id))
            .Select(s => new SkillDto { Id = s.Id, Name = s.Name, Description = s.Description, IconName = s.IconName });
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
        // Never quiz a hidden skill: keep only those with a finalized bank.
        skillIds = skillIds.Where(id => QuizSeedData.ActiveSkillIds.Contains(id)).ToList();
        if (skillIds.Count == 0)
            skillIds = QuizSeedData.ActiveSkillIds.ToList();

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
            // Store the raw summed score (8 questions × 1–4 = 8–32) and classify
            // it into a band. See SoftSync.Common.AssessmentScoring.
            var score = agg.sum;
            var level = AssessmentScoring.BandFor(score);

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
        // Repo returns rows newest-first. Guard the results screen against:
        //  - duplicate cards per skill (older attempts still in the table), and
        //  - skills whose quiz was hidden (QuizSeedData.ActiveSkillIds).
        // Keep only the most recent result per active skill.
        var results = await _assessmentRepo.GetResultsByUserIdAsync(userId);
        return results
            .Where(r => QuizSeedData.ActiveSkillIds.Contains(r.SkillId))
            .GroupBy(r => r.SkillId)
            .Select(g => g.First()) // First == newest (repo orders by CreatedAt desc)
            .Select(r => new AssessmentResultDto
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
    private readonly IAssessmentRepository _assessmentRepo;
    private readonly ISkillRepository _skillRepo;
    public RoadmapService(IRoadmapRepository roadmapRepo, IAiRoadmapService aiService, IUserRepository userRepo,
        IAssessmentRepository assessmentRepo, ISkillRepository skillRepo)
    {
        _roadmapRepo = roadmapRepo;
        _aiService = aiService;
        _userRepo = userRepo;
        _assessmentRepo = assessmentRepo;
        _skillRepo = skillRepo;
    }

    public async Task<RoadmapDto> GetUserRoadmapAsync(int userId)
    {
        var focusSkills = await GetFocusSkillNamesAsync(userId);

        var items = (await _roadmapRepo.GetByUserIdAsync(userId)).ToList();
        // A read operation must never destroy a learner's roadmap. The previous
        // refresh check regenerated plans containing the very migration titles it
        // considered stale, so IDs and IsCompleted could be lost on every visit.
        // Plan regeneration should be an explicit, versioned action in the future.
        if (!items.Any())
        {
            var freshRoadmap = await _aiService.GenerateRoadmapAsync(userId, focusSkills);
            foreach (var item in freshRoadmap.Items)
            {
                await _roadmapRepo.AddAsync(new RoadmapItem
                {
                    UserId = userId,
                    WeekNumber = item.WeekNumber,
                    Title = item.Title,
                    Description = item.Description
                });
            }

            await _roadmapRepo.SaveChangesAsync();
            items = (await _roadmapRepo.GetByUserIdAsync(userId)).ToList();
        }

        // Content Review Mode needs stable, real item IDs for every active lesson
        // so editors can open its video, script, summary, and quiz. Add only
        // missing catalog rows; existing completion data is never overwritten.
        if (RoadmapReviewMode.Enabled)
        {
            var allActiveSkills = (await _skillRepo.GetAllAsync())
                .Where(s => QuizSeedData.ActiveSkillIds.Contains(s.Id))
                .Select(s => s.Name)
                .ToList();
            var completeCatalog = await _aiService.GenerateRoadmapAsync(userId, allActiveSkills);
            var nextWeek = items.Count == 0 ? 1 : items.Max(x => x.WeekNumber) + 1;
            var addedMissingItem = false;

            foreach (var catalogItem in completeCatalog.Items)
            {
                if (items.Any(x => string.Equals(x.Title, catalogItem.Title, StringComparison.OrdinalIgnoreCase)))
                    continue;

                await _roadmapRepo.AddAsync(new RoadmapItem
                {
                    UserId = userId,
                    WeekNumber = nextWeek++,
                    Title = catalogItem.Title,
                    Description = catalogItem.Description
                });
                addedMissingItem = true;
            }

            if (addedMissingItem)
            {
                await _roadmapRepo.SaveChangesAsync();
                items = (await _roadmapRepo.GetByUserIdAsync(userId)).ToList();
            }
        }
        else
        {
            // Catalog rows may remain in storage after content review. In normal
            // mode only return the lessons selected by the personalization flow.
            var personalizedCatalog = await _aiService.GenerateRoadmapAsync(userId, focusSkills);
            var allowedTitles = personalizedCatalog.Items
                .Select(x => x.Title)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            items = items.Where(x => allowedTitles.Contains(x.Title)).ToList();
        }

        // Old review/catalog runs may have inserted the same lesson more than
        // once. Present one canonical row per lesson so weeks and videos do not
        // repeat, while preserving the oldest row and its learning progress.
        items = items
            .OrderBy(x => x.WeekNumber)
            .ThenBy(x => x.Id)
            .GroupBy(x => x.Title.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        // Once the four-week Communication catalog exists, keep older generic
        // Communication rows in storage for history but do not count or display
        // them as extra weeks (which previously made Week 1 appear as Week 5).
        if (items.Any(x => CommunicationCatalogTitles.Contains(x.Title.Trim())))
        {
            items = items
                .Where(x => !IsCommunicationTitle(x.Title) || CommunicationCatalogTitles.Contains(x.Title.Trim()))
                .ToList();
        }

        return new RoadmapDto
        {
            UserId = userId,
            Items = items.Select((i, index) => new RoadmapItemDto
            {
                Id = i.Id,
                WeekNumber = index + 1,
                SkillName = ResolveRoadmapSkillName(i.Title),
                Title = i.Title,
                Description = i.Description,
                // Existing completed rows predate granular activity tracking;
                // treat them as fully complete so their green ticks are preserved.
                IsVideoCompleted = i.IsCompleted || i.VideoCompletedAtUtc.HasValue,
                IsPracticeCompleted = i.IsCompleted || i.PracticeCompletedAtUtc.HasValue,
                IsScenarioCompleted = i.IsCompleted || i.ScenarioCompletedAtUtc.HasValue,
                IsReflectionCompleted = i.IsCompleted || i.ReflectionCompletedAtUtc.HasValue,
                VideoCompletedAtUtc = i.VideoCompletedAtUtc,
                ScriptCompletedAtUtc = i.ScriptCompletedAtUtc,
                SummaryCompletedAtUtc = i.SummaryCompletedAtUtc,
                PracticeCompletedAtUtc = i.PracticeCompletedAtUtc,
                ScenarioCompletedAtUtc = i.ScenarioCompletedAtUtc,
                ReflectionCompletedAtUtc = i.ReflectionCompletedAtUtc,
                ReflectionText = i.ReflectionText ?? string.Empty,
                LastLearningStep = i.LastLearningStep,
                QuizHistory = DeserializeQuizHistory(i.QuizHistoryJson),
                IsCompleted = i.IsCompleted
            }).ToList()
        };
    }

    private static string ResolveRoadmapSkillName(string title)
    {
        foreach (var (skillName, aliases) in RoadmapSkillAliases)
        {
            if (aliases.Any(alias => title.Contains(alias, StringComparison.OrdinalIgnoreCase)))
                return skillName;
        }

        return string.Empty;
    }

    private static bool IsCommunicationTitle(string title)
        => RoadmapSkillAliases[0].Aliases.Any(alias => title.Contains(alias, StringComparison.OrdinalIgnoreCase));

    private static readonly HashSet<string> CommunicationCatalogTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Tuần 1 - Nền tảng giao tiếp",
        "Tuần 2 - Lắng nghe chủ động",
        "Tuần 3 - Phản hồi xây dựng",
        "Tuần 4 - Giao tiếp nâng cao"
    };

    private static readonly (string SkillName, string[] Aliases)[] RoadmapSkillAliases =
    {
        ("Giao tiếp", new[] { "Communication", "Giao tiếp", "Kỹ năng Giao tiếp" }),
        ("Quản lý thời gian", new[] { "Time Management", "Quản lý thời gian", "Kỹ năng Quản lý thời gian" }),
        ("Tư duy phản biện", new[] { "Critical Thinking", "Tư duy phản biện", "Kỹ năng Tư duy phản biện" })
    };

    /// <summary>
    /// Picks which skills the roadmap should focus on, restricted to the 3 active
    /// skills (<see cref="QuizSeedData.ActiveSkillIds"/>). Preference order:
    ///  1. The user's assessment results, weakest score first (most to improve).
    ///  2. Skills they selected in the wizard (if they haven't been assessed).
    ///  3. All active skills, as a last resort.
    /// Only real skill names from the DB are returned — no external data.
    /// </summary>
    private async Task<List<string>> GetFocusSkillNamesAsync(int userId)
    {
        // 1. From assessment results (active skills only), weakest first.
        var results = await _assessmentRepo.GetResultsByUserIdAsync(userId);
        var byScore = results
            .Where(r => QuizSeedData.ActiveSkillIds.Contains(r.SkillId))
            .GroupBy(r => r.SkillId)
            .Select(g => g.First()) // newest per skill (repo orders by CreatedAt desc)
            .OrderBy(r => r.Score)  // weakest skill first
            .Select(r => r.Skill.Name)
            .ToList();
        if (byScore.Count > 0) return byScore;

        // 2. Fall back to the skills the user selected (active only).
        var allSkills = (await _skillRepo.GetAllAsync()).ToList();
        var selectedIds = await _assessmentRepo.GetSelectedSkillIdsAsync(userId);
        var selected = allSkills
            .Where(s => selectedIds.Contains(s.Id) && QuizSeedData.ActiveSkillIds.Contains(s.Id))
            .Select(s => s.Name)
            .ToList();
        if (selected.Count > 0) return selected;

        // 3. Last resort: every active skill.
        return allSkills
            .Where(s => QuizSeedData.ActiveSkillIds.Contains(s.Id))
            .Select(s => s.Name)
            .ToList();
    }

    public Task<bool> MarkVideoCompleteAsync(int itemId, int userId)
        => MarkActivityCompleteAsync(itemId, userId, isVideo: true);

    /// <summary>
    /// Marks the required practice as passed. Kept under the existing method name
    /// so current games continue to call the same service contract.
    /// </summary>
    public Task<bool> MarkCompleteAsync(int itemId, int userId)
        => MarkActivityCompleteAsync(itemId, userId, isVideo: false);

    public async Task<bool> MarkScenarioCompleteAsync(int itemId, int userId)
    {
        var item = await _roadmapRepo.GetByIdAsync(itemId);
        if (item is null || item.UserId != userId || !await IsWeekUnlockedAsync(item)
            || !item.PracticeCompletedAtUtc.HasValue)
            return false;

        var changed = false;
        if (!item.ScenarioCompletedAtUtc.HasValue)
        {
            item.ScenarioCompletedAtUtc = DateTime.UtcNow;
            changed = true;
        }

        return await PersistActivityAsync(item, changed);
    }

    public async Task<bool> SaveReflectionAsync(int itemId, int userId, string reflectionText)
    {
        var value = reflectionText?.Trim() ?? string.Empty;
        if (value.Length == 0 || value.Length > 2000)
            return false;

        var item = await _roadmapRepo.GetByIdAsync(itemId);
        if (item is null || item.UserId != userId || !await IsWeekUnlockedAsync(item)
            || !item.ScenarioCompletedAtUtc.HasValue)
            return false;

        var changed = !string.Equals(item.ReflectionText, value, StringComparison.Ordinal)
            || !item.ReflectionCompletedAtUtc.HasValue;
        item.ReflectionText = value;
        item.ReflectionCompletedAtUtc ??= DateTime.UtcNow;
        return await PersistActivityAsync(item, changed);
    }

    public async Task<bool> SaveLearningStepAsync(int itemId, int userId, string step)
    {
        var normalized = step?.Trim().ToLowerInvariant() ?? string.Empty;
        var stepIndex = Array.IndexOf(LearningStepOrder, normalized);
        if (stepIndex < 0)
            return false;

        var item = await _roadmapRepo.GetByIdAsync(itemId);
        if (item is null || item.UserId != userId || !await IsWeekUnlockedAsync(item)
            || !CanOpenStep(item, normalized))
            return false;

        var now = DateTime.UtcNow;
        var changed = false;
        if (normalized == "summary" && !item.ScriptCompletedAtUtc.HasValue)
        {
            item.ScriptCompletedAtUtc = now;
            changed = true;
        }
        if (normalized == "quiz" && !item.SummaryCompletedAtUtc.HasValue)
        {
            item.SummaryCompletedAtUtc = now;
            changed = true;
        }
        if (!string.Equals(item.LastLearningStep, normalized, StringComparison.Ordinal))
        {
            item.LastLearningStep = normalized;
            changed = true;
        }
        if (changed)
            await _roadmapRepo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SaveQuizAttemptAsync(int itemId, int userId, RoadmapQuizAttemptDto attempt)
    {
        var item = await _roadmapRepo.GetByIdAsync(itemId);
        if (item is null || item.UserId != userId || !await IsWeekUnlockedAsync(item)
            || !item.SummaryCompletedAtUtc.HasValue || attempt.Answers.Count == 0
            || attempt.TotalQuestions <= 0 || attempt.Score < 0 || attempt.Score > attempt.TotalQuestions)
            return false;

        var history = DeserializeQuizHistory(item.QuizHistoryJson);
        attempt.AttemptNumber = history.Count + 1;
        attempt.SubmittedAtUtc = DateTime.UtcNow;
        history.Add(attempt);
        item.QuizHistoryJson = JsonSerializer.Serialize(history);
        if (attempt.Passed && !item.PracticeCompletedAtUtc.HasValue)
            item.PracticeCompletedAtUtc = attempt.SubmittedAtUtc;
        item.LastLearningStep = attempt.Passed ? "scenario" : "quiz";
        await _roadmapRepo.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<RoadmapQuizAttemptDto>> GetQuizHistoryAsync(int itemId, int userId)
    {
        var item = await _roadmapRepo.GetByIdAsync(itemId);
        return item is null || item.UserId != userId
            ? Array.Empty<RoadmapQuizAttemptDto>()
            : DeserializeQuizHistory(item.QuizHistoryJson);
    }

    private static List<RoadmapQuizAttemptDto> DeserializeQuizHistory(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<RoadmapQuizAttemptDto>>(json) ?? []; }
        catch (JsonException) { return []; }
    }

    private static readonly string[] LearningStepOrder = ["video", "script", "summary", "quiz", "scenario", "reflection"];

    private static bool CanOpenStep(RoadmapItem item, string step) => item.IsCompleted || step switch
    {
        "video" => true,
        "script" => item.VideoCompletedAtUtc.HasValue,
        "summary" => item.ScriptCompletedAtUtc.HasValue
            || string.Equals(item.LastLearningStep, "script", StringComparison.OrdinalIgnoreCase),
        "quiz" => item.SummaryCompletedAtUtc.HasValue
            || string.Equals(item.LastLearningStep, "summary", StringComparison.OrdinalIgnoreCase),
        "scenario" => item.PracticeCompletedAtUtc.HasValue,
        "reflection" => item.ScenarioCompletedAtUtc.HasValue,
        _ => false
    };

    private async Task<bool> MarkActivityCompleteAsync(int itemId, int userId, bool isVideo)
    {
        var item = await _roadmapRepo.GetByIdAsync(itemId);
        if (item is null || item.UserId != userId || !await IsWeekUnlockedAsync(item))
            return false;

        if (!isVideo && (!item.VideoCompletedAtUtc.HasValue || !item.SummaryCompletedAtUtc.HasValue))
            return false;

        var now = DateTime.UtcNow;
        var activityChanged = false;
        if (isVideo && !item.VideoCompletedAtUtc.HasValue)
        {
            item.VideoCompletedAtUtc = now;
            activityChanged = true;
        }
        else if (!isVideo && item.VideoCompletedAtUtc.HasValue && !item.PracticeCompletedAtUtc.HasValue)
        {
            item.PracticeCompletedAtUtc = now;
            activityChanged = true;
        }

        return await PersistActivityAsync(item, activityChanged);
    }

    private async Task<bool> IsWeekUnlockedAsync(RoadmapItem item)
    {
        var orderedItems = (await _roadmapRepo.GetByUserIdAsync(item.UserId))
            .OrderBy(x => x.WeekNumber)
            .ThenBy(x => x.Id)
            .GroupBy(x => x.Title.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
        var index = orderedItems.FindIndex(x => x.Id == item.Id);
        // A completed week must always remain reviewable, even when old/imported
        // roadmap data has an inconsistent previous-week completion flag.
        return item.IsCompleted || index == 0 || index > 0 && orderedItems[index - 1].IsCompleted;
    }

    private async Task<bool> PersistActivityAsync(RoadmapItem item, bool activityChanged)
    {
        var becameComplete = !item.IsCompleted
            && item.VideoCompletedAtUtc.HasValue
            && item.PracticeCompletedAtUtc.HasValue
            && item.ScenarioCompletedAtUtc.HasValue
            && item.ReflectionCompletedAtUtc.HasValue;

        if (becameComplete)
            item.IsCompleted = true;

        if (activityChanged || becameComplete)
            await _roadmapRepo.SaveChangesAsync();

        if (becameComplete)
        {
            // Award XP once, only on the transition to completed.
            var user = await _userRepo.GetByIdAsync(item.UserId);
            if (user != null)
            {
                user.ExperiencePoints += SoftSync.Common.LevelSystem.RoadmapItemXp;
                await _userRepo.SaveChangesAsync();
            }
        }

        return true;
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
