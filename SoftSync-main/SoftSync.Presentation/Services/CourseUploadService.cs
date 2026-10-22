using Microsoft.AspNetCore.Components.Forms;

namespace SoftSync.Presentation.Services;

public sealed class CourseUploadService(IWebHostEnvironment environment)
{
    private static readonly Dictionary<string, string[]> Images = new(StringComparer.OrdinalIgnoreCase) { [".jpg"] = ["image/jpeg"], [".jpeg"] = ["image/jpeg"], [".png"] = ["image/png"], [".webp"] = ["image/webp"] };
    private static readonly Dictionary<string, string[]> Videos = new(StringComparer.OrdinalIgnoreCase) { [".mp4"] = ["video/mp4"], [".webm"] = ["video/webm"] };

    public Task<string> SaveImageAsync(IBrowserFile file) => SaveAsync(file, Images, 5 * 1024 * 1024, "images");
    public Task<string> SaveVideoAsync(IBrowserFile file) => SaveAsync(file, Videos, 200 * 1024 * 1024, "videos");

    private async Task<string> SaveAsync(IBrowserFile file, IReadOnlyDictionary<string, string[]> allowed, long maxBytes, string folder)
    {
        var extension = Path.GetExtension(file.Name).ToLowerInvariant();
        if (!allowed.TryGetValue(extension, out var mimeTypes) || !mimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase) || file.Size <= 0 || file.Size > maxBytes)
            throw new InvalidOperationException("Unsupported file type or file size.");
        var relativeFolder = Path.Combine("uploads", "courses", folder);
        var directory = Path.Combine(environment.WebRootPath, relativeFolder);
        Directory.CreateDirectory(directory);
        var safeName = $"{Guid.NewGuid():N}{extension}";
        var target = Path.Combine(directory, safeName);
        await using var output = File.Create(target);
        await file.OpenReadStream(maxBytes).CopyToAsync(output);
        return "/" + Path.Combine(relativeFolder, safeName).Replace('\\', '/');
    }
}
