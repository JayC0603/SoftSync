using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace SoftSync.Presentation.Services;

public sealed class PdfDocumentKnowledge
{
    private static readonly Regex TokenPattern = new(@"[\p{L}\p{N}]{2,}", RegexOptions.Compiled);
    private readonly IConfiguration configuration;
    private readonly IWebHostEnvironment environment;
    private readonly ILogger<PdfDocumentKnowledge> logger;
    private readonly Lazy<Task<IReadOnlyList<PdfChunk>>> chunks;

    public PdfDocumentKnowledge(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<PdfDocumentKnowledge> logger)
    {
        this.configuration = configuration;
        this.environment = environment;
        this.logger = logger;
        chunks = new Lazy<Task<IReadOnlyList<PdfChunk>>>(BuildIndexAsync);
    }

    public async Task<IReadOnlyList<PdfExcerpt>> SearchAsync(string query, int limit = 7)
    {
        var allChunks = await chunks.Value;
        if (allChunks.Count == 0) return [];

        var terms = Tokenize(query).Distinct(StringComparer.Ordinal).ToArray();
        if (terms.Length == 0) return [];

        return allChunks
            .Select(chunk => new { Chunk = chunk, Score = Score(chunk.SearchText, terms) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Chunk.DocumentOrder)
            .Take(Math.Clamp(limit, 1, 12))
            .Select(x => new PdfExcerpt(x.Chunk.Title, x.Chunk.Page, x.Chunk.Text))
            .ToArray();
    }

    private Task<IReadOnlyList<PdfChunk>> BuildIndexAsync() => Task.Run<IReadOnlyList<PdfChunk>>(() =>
    {
        var documents = configuration.GetSection("AiDocuments:Files").Get<List<PdfSource>>() ?? [];
        var result = new List<PdfChunk>();

        for (var documentOrder = 0; documentOrder < documents.Count; documentOrder++)
        {
            var source = documents[documentOrder];
            var path = ResolvePath(source.Path);
            if (path is null)
            {
                logger.LogWarning("AI reference PDF was not found: {Path}", source.Path);
                continue;
            }

            try
            {
                using var document = PdfDocument.Open(path);
                foreach (var page in document.GetPages())
                {
                    var text = Normalize(page.Text);
                    foreach (var part in Split(text, 1800, 250))
                    {
                        result.Add(new PdfChunk(
                            documentOrder,
                            string.IsNullOrWhiteSpace(source.Title) ? Path.GetFileNameWithoutExtension(path) : source.Title,
                            page.Number,
                            part,
                            NormalizeForSearch(part)));
                    }
                }
                logger.LogInformation("Indexed AI reference PDF {Title} ({Pages} pages)", source.Title, document.NumberOfPages);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not index AI reference PDF {Path}", path);
            }
        }

        logger.LogInformation("AI PDF knowledge index contains {ChunkCount} chunks from {DocumentCount} configured documents", result.Count, documents.Count);
        return result;
    });

    private string? ResolvePath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath)) return null;
        var candidates = new[]
        {
            configuredPath,
            Path.Combine(environment.ContentRootPath, configuredPath),
            Path.Combine(AppContext.BaseDirectory, configuredPath)
        };
        return candidates.Select(Path.GetFullPath).FirstOrDefault(File.Exists);
    }

    private static double Score(string text, IReadOnlyList<string> terms)
    {
        double score = 0;
        foreach (var term in terms)
        {
            var count = 0;
            var offset = 0;
            while ((offset = text.IndexOf(term, offset, StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += term.Length;
            }
            if (count > 0) score += 1 + Math.Log(1 + count);
        }
        return score * (1 + terms.Count(term => text.Contains(term, StringComparison.Ordinal)) / (double)terms.Count);
    }

    private static IEnumerable<string> Tokenize(string value) =>
        TokenPattern.Matches(NormalizeForSearch(value)).Select(match => match.Value);

    private static string NormalizeForSearch(string value)
    {
        var decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character == 'đ' ? 'd' : character);
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string Normalize(string value) => Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();

    private static IEnumerable<string> Split(string text, int size, int overlap)
    {
        if (string.IsNullOrWhiteSpace(text)) yield break;
        var start = 0;
        while (start < text.Length)
        {
            var length = Math.Min(size, text.Length - start);
            var end = start + length;
            if (end < text.Length)
            {
                var boundary = text.LastIndexOfAny(['.', '!', '?', ';'], end - 1, Math.Min(length, 300));
                if (boundary > start + size / 2) length = boundary - start + 1;
            }
            yield return text.Substring(start, length).Trim();
            if (start + length >= text.Length) yield break;
            start += Math.Max(1, length - overlap);
        }
    }

    private sealed record PdfChunk(int DocumentOrder, string Title, int Page, string Text, string SearchText);
    private sealed class PdfSource
    {
        public string Title { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }
}

public sealed record PdfExcerpt(string Title, int Page, string Text);
