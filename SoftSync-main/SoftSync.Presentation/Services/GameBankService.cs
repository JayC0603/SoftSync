using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SoftSync.Presentation.Services;

public interface IGameBankService
{
    Task<Game1SessionDto?> CreateSessionAsync(int skillId, int questionCount = 5);
}

public sealed class GameBankService : IGameBankService
{
    private static readonly IReadOnlyDictionary<int, GameBankSource> Sources = new Dictionary<int, GameBankSource>
    {
        [1] = new("communication.txt", "Kỹ năng Giao tiếp"),
        [3] = new("time-management.txt", "Kỹ năng Quản lý Thời gian"),
        [4] = new("critical-thinking.txt", "Kỹ năng Tư duy Phản biện")
    };

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<GameBankService> _logger;
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private IReadOnlyDictionary<int, IReadOnlyList<GameBankQuestion>>? _banks;

    public GameBankService(IWebHostEnvironment env, ILogger<GameBankService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<Game1SessionDto?> CreateSessionAsync(int skillId, int questionCount = 5)
    {
        var bank = await GetBankAsync(skillId);
        if (bank.Count == 0)
            return null;

        var selected = bank
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Min(questionCount, bank.Count))
            .Select(question => question.WithShuffledOptions())
            .ToList();

        return new Game1SessionDto
        {
            SkillId = skillId,
            SkillName = Sources.TryGetValue(skillId, out var source) ? source.SkillName : "Game",
            QuestionLimit = Math.Min(questionCount, bank.Count),
            Questions = selected
        };
    }

    private async Task<IReadOnlyList<GameBankQuestion>> GetBankAsync(int skillId)
    {
        await EnsureLoadedAsync();
        return _banks?.TryGetValue(skillId, out var bank) == true
            ? bank
            : Array.Empty<GameBankQuestion>();
    }

    private async Task EnsureLoadedAsync()
    {
        if (_banks is not null)
            return;

        await _loadLock.WaitAsync();
        try
        {
            if (_banks is not null)
                return;

            var loaded = new Dictionary<int, IReadOnlyList<GameBankQuestion>>();
            foreach (var (skillId, source) in Sources)
            {
                var path = Path.Combine(_env.ContentRootPath, "Assets", "Game1Banks", source.FileName);
                if (!File.Exists(path))
                {
                    _logger.LogWarning("Game bank file not found at {Path}", path);
                    loaded[skillId] = Array.Empty<GameBankQuestion>();
                    continue;
                }

                var text = await File.ReadAllTextAsync(path, Encoding.UTF8);
                loaded[skillId] = ParseBank(text, source.SkillName);
            }

            _banks = loaded;
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private static IReadOnlyList<GameBankQuestion> ParseBank(string rawText, string skillName)
    {
        var text = rawText.Replace("\r\n", "\n");
        var lines = text
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .ToList();
        var folded = lines.Select(FoldToAscii).ToList();

        var questionStart = folded.FindIndex(line => line.StartsWith("cau 1:", StringComparison.OrdinalIgnoreCase));
        var answerStart = FindAnswerStart(folded);
        if (questionStart < 0 || answerStart < 0 || answerStart <= questionStart)
            return Array.Empty<GameBankQuestion>();

        var answerKey = ParseAnswerKey(lines.Skip(answerStart + 1));
        var questions = new List<GameBankQuestion>();

        for (var index = questionStart; index < answerStart;)
        {
            while (index < answerStart && string.IsNullOrWhiteSpace(lines[index]))
                index++;

            if (index >= answerStart)
                break;

            var questionLine = lines[index].Trim();
            var foldedQuestion = FoldToAscii(questionLine);
            if (!foldedQuestion.StartsWith("cau ", StringComparison.OrdinalIgnoreCase))
            {
                index++;
                continue;
            }

            var colonIndex = questionLine.IndexOf(':');
            if (colonIndex < 0)
            {
                index++;
                continue;
            }

            var header = foldedQuestion[..foldedQuestion.IndexOf(':')];
            var numberMatch = Regex.Match(header, @"^cau\s+(?<num>\d+)$", RegexOptions.IgnoreCase);
            if (!numberMatch.Success)
            {
                index++;
                continue;
            }

            var number = int.Parse(numberMatch.Groups["num"].Value);
            var questionText = questionLine[(colonIndex + 1)..].Trim();
            index++;

            var options = new List<GameBankOption>();
            while (index < answerStart && options.Count < 4)
            {
                var optionLine = lines[index].Trim();
                if (string.IsNullOrWhiteSpace(optionLine))
                {
                    index++;
                    continue;
                }

                var optionMatch = Regex.Match(optionLine, @"^(?<key>[ABCD])\.\s*(?<text>.*)$");
                if (optionMatch.Success)
                {
                    options.Add(new GameBankOption(
                        optionMatch.Groups["key"].Value,
                        StripQuotes(optionMatch.Groups["text"].Value)));
                }

                index++;
            }

            if (options.Count == 4 && answerKey.TryGetValue(number, out var correctKey))
            {
                questions.Add(new GameBankQuestion(
                    number,
                    skillName,
                    questionText,
                    options.Select(o => new GameBankOption(
                        o.Key,
                        o.Text,
                        o.Key.Equals(correctKey, StringComparison.OrdinalIgnoreCase))).ToList()));
            }
        }

        return questions;
    }

    private static Dictionary<int, string> ParseAnswerKey(IEnumerable<string> lines)
    {
        var answerKey = new Dictionary<int, string>();
        var buffer = lines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Trim()).ToList();

        for (var i = 0; i < buffer.Count - 1; i++)
        {
            if (!int.TryParse(buffer[i], out var number))
                continue;

            var next = buffer[i + 1];
            if (Regex.IsMatch(next, "^[ABCD]$", RegexOptions.IgnoreCase))
            {
                answerKey[number] = next.ToUpperInvariant();
                i++;
            }
        }

        return answerKey;
    }

    private static int FindAnswerStart(IReadOnlyList<string> foldedLines)
    {
        for (var i = 0; i < foldedLines.Count - 1; i++)
        {
            if (!Regex.IsMatch(foldedLines[i], @"^\d+$"))
                continue;

            var next = foldedLines[i + 1];
            if (Regex.IsMatch(next, "^[ABCD]$", RegexOptions.IgnoreCase))
                return i;
        }

        return -1;
    }

    private static string StripQuotes(string value)
        => value.Trim().Trim().Trim('"').Trim();

    private static string FoldToAscii(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                builder.Append(ch);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    private sealed record GameBankSource(string FileName, string SkillName);
}

public sealed class Game1SessionDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int QuestionLimit { get; set; }
    public List<GameBankQuestionDto> Questions { get; set; } = new();
}

public sealed class GameBankQuestionDto
{
    public string Id { get; set; } = string.Empty;
    public int Number { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public List<GameBankOptionDto> Options { get; set; } = new();
}

public sealed class GameBankOptionDto
{
    public GameBankOptionDto()
    {
    }

    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

internal sealed record GameBankQuestion(int Number, string SkillName, string QuestionText, List<GameBankOption> Options)
{
    public GameBankQuestionDto WithShuffledOptions()
    {
        var shuffled = Options
            .OrderBy(_ => Random.Shared.Next())
            .Select((option, index) => new GameBankOptionDto
            {
                Id = Guid.NewGuid().ToString("N"),
                Key = option.Key,
                DisplayLabel = ((char)('A' + index)).ToString(),
                Text = option.Text,
                IsCorrect = option.IsCorrect
            })
            .ToList();

        return new GameBankQuestionDto
        {
            Id = Guid.NewGuid().ToString("N"),
            Number = Number,
            SkillName = SkillName,
            QuestionText = QuestionText,
            Options = shuffled
        };
    }
}

internal sealed record GameBankOption(string Key, string Text, bool IsCorrect)
{
    public GameBankOption(string key, string text) : this(key, text, false)
    {
    }
}
