using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Entities;

namespace SoftSync.DAL.Repositories;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> SaveChangesAsync();
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly Data.SoftSyncDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(Data.SoftSyncDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
}

// Specialized interfaces
public interface IUserRepository : IRepository<ApplicationUser>
{
    /// <summary>Loads the user with their skill selections tracked, so the caller can diff/replace them.</summary>
    Task<ApplicationUser?> GetWithSkillSelectionsAsync(int userId);
}
public class UserRepository : Repository<ApplicationUser>, IUserRepository
{
    public UserRepository(Data.SoftSyncDbContext context) : base(context) { }

    public async Task<ApplicationUser?> GetWithSkillSelectionsAsync(int userId) =>
        await _dbSet.Include(u => u.SkillSelections).FirstOrDefaultAsync(u => u.Id == userId);
}

public interface ISkillRepository : IRepository<Skill> { }
public class SkillRepository : Repository<Skill>, ISkillRepository { public SkillRepository(Data.SoftSyncDbContext context) : base(context) { } }

public interface IAssessmentRepository
{
    Task<IEnumerable<AssessmentQuestion>> GetQuestionsBySkillIdsAsync(List<int> skillIds);
    Task<List<int>> GetSelectedSkillIdsAsync(int userId);
    Task<IEnumerable<AssessmentOption>> GetAnsweredOptionsAsync(List<int> optionIds);
    Task SaveResultAsync(AssessmentResult result);
    Task SaveResultsAsync(IEnumerable<AssessmentResult> results);
    Task<IEnumerable<AssessmentResult>> GetResultsByUserIdAsync(int userId);
}

public class AssessmentRepository : IAssessmentRepository
{
    private readonly Data.SoftSyncDbContext _context;
    public AssessmentRepository(Data.SoftSyncDbContext context) => _context = context;

    public async Task<IEnumerable<AssessmentQuestion>> GetQuestionsBySkillIdsAsync(List<int> skillIds)
    {
        return await _context.AssessmentQuestions
            .Where(q => skillIds.Contains(q.SkillId))
            .Include(q => q.Options)
            .Include(q => q.Skill)
            .OrderBy(q => q.SkillId).ThenBy(q => q.Id)
            .ToListAsync();
    }

    // Distinct skills a user picked in the wizard. Falls back to "no skills" (empty)
    // — the service decides what to do when the user hasn't selected any.
    public async Task<List<int>> GetSelectedSkillIdsAsync(int userId)
    {
        return await _context.UserSkillSelections
            .Where(us => us.UserId == userId)
            .Select(us => us.SkillId)
            .Distinct()
            .ToListAsync();
    }

    // The chosen options, each carrying its ScoreValue and owning Question (for SkillId),
    // so the service can compute a real per-skill score.
    public async Task<IEnumerable<AssessmentOption>> GetAnsweredOptionsAsync(List<int> optionIds)
    {
        return await _context.AssessmentOptions
            .Where(o => optionIds.Contains(o.Id))
            .Include(o => o.Question)
            .ToListAsync();
    }

    public async Task SaveResultAsync(AssessmentResult result)
    {
        await _context.AssessmentResults.AddAsync(result);
        await _context.SaveChangesAsync();
    }

    public async Task SaveResultsAsync(IEnumerable<AssessmentResult> results)
    {
        await _context.AssessmentResults.AddRangeAsync(results);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AssessmentResult>> GetResultsByUserIdAsync(int userId)
    {
        return await _context.AssessmentResults
            .Where(r => r.UserId == userId)
            .Include(r => r.Skill)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}

public interface IRoadmapRepository : IRepository<RoadmapItem>
{
    Task<IEnumerable<RoadmapItem>> GetByUserIdAsync(int userId);
}

public class RoadmapRepository : Repository<RoadmapItem>, IRoadmapRepository
{
    public RoadmapRepository(Data.SoftSyncDbContext context) : base(context) { }
    public async Task<IEnumerable<RoadmapItem>> GetByUserIdAsync(int userId)
    {
        return await _dbSet.Where(r => r.UserId == userId).OrderBy(r => r.WeekNumber).ToListAsync();
    }
}

public interface ICaseStudyRepository : IRepository<CaseStudy>
{
    Task<IEnumerable<CaseStudy>> GetBySkillIdAsync(int skillId);
    Task<CaseStudy?> GetWithDetailsAsync(int id);
}

public class CaseStudyRepository : Repository<CaseStudy>, ICaseStudyRepository
{
    public CaseStudyRepository(Data.SoftSyncDbContext context) : base(context) { }
    public async Task<IEnumerable<CaseStudy>> GetBySkillIdAsync(int skillId)
    {
        return await _dbSet.Where(cs => cs.SkillId == skillId).Include(cs => cs.Options).ToListAsync();
    }
    public async Task<CaseStudy?> GetWithDetailsAsync(int id)
    {
        return await _dbSet.Include(cs => cs.Options).FirstOrDefaultAsync(cs => cs.Id == id);
    }
}

public interface IProgressRepository : IRepository<ProgressLog>
{
    Task<IEnumerable<ProgressLog>> GetByUserIdAsync(int userId);
}

public class ProgressRepository : Repository<ProgressLog>, IProgressRepository
{
    public ProgressRepository(Data.SoftSyncDbContext context) : base(context) { }
    public async Task<IEnumerable<ProgressLog>> GetByUserIdAsync(int userId)
    {
        return await _dbSet.Where(p => p.UserId == userId).Include(p => p.Skill).ToListAsync();
    }
}

public interface IChatRepository : IRepository<ChatMessage>
{
    Task<IEnumerable<ChatMessage>> GetByUserIdAsync(int userId);
}

public class ChatRepository : Repository<ChatMessage>, IChatRepository
{
    public ChatRepository(Data.SoftSyncDbContext context) : base(context) { }
    public async Task<IEnumerable<ChatMessage>> GetByUserIdAsync(int userId)
    {
        return await _dbSet.Where(c => c.UserId == userId).OrderBy(c => c.CreatedAt).ToListAsync();
    }
}

public interface IMentorRepository : IRepository<Mentor> { }
public class MentorRepository : Repository<Mentor>, IMentorRepository { public MentorRepository(Data.SoftSyncDbContext context) : base(context) { } }
