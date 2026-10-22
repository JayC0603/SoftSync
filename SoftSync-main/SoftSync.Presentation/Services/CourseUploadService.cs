using Microsoft.AspNetCore.Components.Forms;

namespace SoftSync.Presentation.Services;

public sealed class CourseUploadService(IWebHostEnvironment environment, IConfiguration configuration, IHttpClientFactory clients)
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
        var token = configuration["BLOB_READ_WRITE_TOKEN"];
        if (!string.IsNullOrWhiteSpace(token))
        {
            var pathname = $"uploads/courses/{folder}/{Guid.NewGuid():N}{extension}";
            using var request = new HttpRequestMessage(HttpMethod.Put,
                "https://vercel.com/api/blob/?pathname=" + Uri.EscapeDataString(pathname));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("x-api-version", "13");
            request.Headers.Add("x-vercel-blob-access", "public");
            request.Headers.Add("x-content-type", file.ContentType);
            request.Headers.Add("x-add-random-suffix", "false");
            request.Headers.Add("x-allow-overwrite", "false");
            await using var stream = file.OpenReadStream(maxBytes);
            request.Content = new StreamContent(stream);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            request.Content.Headers.ContentLength = file.Size;
            using var response = await clients.CreateClient("CourseBlob").SendAsync(request);
            response.EnsureSuccessStatusCode();
            using var result = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return result.RootElement.GetProperty("url").GetString()
                ?? throw new InvalidOperationException("Blob storage returned an empty URL.");
        }
        if (configuration.GetValue<bool>("Hosting:Vercel"))
            throw new InvalidOperationException("Configure BLOB_READ_WRITE_TOKEN to persist course uploads on Vercel.");
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
