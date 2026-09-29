using System.IO.Compression;
using System.Text;
using System.Security;

namespace SoftSync.Tests;

internal static class CvAcceptanceFixtures
{
    public const string English = "Alex Example\nProfessional Summary\nSoftware developer with evidence from portfolio projects.\nEducation\nExample University, Computer Science, 2024.\nExperience\nSynthetic internship: implemented validation and regression tests.\nProjects\nLearning portal: designed accessible forms and documented API contracts.\nSkills\nC#, PostgreSQL, teamwork, communication.\nAchievements\nDelivered a working prototype and documented test results.";
    public const string Vietnamese = "Nguyễn Minh Mẫu\nTóm tắt\nPhát triển phần mềm và Trí tuệ nhân tạo dựa trên dự án học tập.\nHọc vấn\nĐại học Ví dụ, Công nghệ thông tin, 2024.\nKinh nghiệm\nThực tập giả lập: xây dựng kiểm thử hồi quy và xác thực dữ liệu.\nDự án\nCổng học tập: thiết kế biểu mẫu dễ tiếp cận và tài liệu API.\nKỹ năng\nC#, PostgreSQL, giao tiếp, làm việc nhóm.\nThành tích\nHoàn thành nguyên mẫu và ghi lại bằng chứng kiểm thử.";
    public static byte[] Docx(string text, int targetSize = 0)
    {
        using var memory = new MemoryStream();
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
        {
            Write("[Content_Types].xml", "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/></Types>");
            Write("_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/></Relationships>");
            Write("word/document.xml", "<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>" + string.Concat(text.Split('\n').Select(line => "<w:p><w:r><w:t xml:space=\"preserve\">" + SecurityElement.Escape(line) + "</w:t></w:r></w:p>")) + "</w:body></w:document>");
            if (targetSize > 0)
            {
                // An uncompressed binary part gives a valid bounded package near the upload limit.
                using var padding = zip.CreateEntry("word/media/synthetic.bin", CompressionLevel.NoCompression).Open();
                var chunk = new byte[8192];
                var remaining = targetSize - 4096;
                while (remaining > 0) { var count = Math.Min(chunk.Length, remaining); padding.Write(chunk, 0, count); remaining -= count; }
            }
            void Write(string name, string content)
            {
                using var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false)); writer.Write(content);
            }
        }
        return memory.ToArray();
    }
    public static byte[] Pdf(string text, bool scanOnly = false)
    {
        var lines = text.Split('\n');
        var pageTexts = lines.Chunk(35).ToArray();
        var objects = new List<string> { "", "", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>" };
        var pages = new List<int>();
        foreach (var page in pageTexts)
        {
            var id = objects.Count + 1; pages.Add(id);
            var content = scanOnly ? "q 200 0 0 200 50 550 cm /Im1 Do Q" : "BT /F1 11 Tf 50 750 Td " + string.Join(" 0 -18 Td ", page.Select(line => "(" + line.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)") + ") Tj")) + " ET";
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents {id + 1} 0 R >>");
            objects.Add($"<< /Length {content.Length} >>\nstream\n{content}\nendstream");
        }
        if (scanOnly)
        {
            var imageId = objects.Count + 1;
            foreach (var id in pages) objects[id - 1] = objects[id - 1].Replace("/Font << /F1 3 0 R >>", $"/XObject << /Im1 {imageId} 0 R >>");
            objects.Add("<< /Type /XObject /Subtype /Image /Width 1 /Height 1 /ColorSpace /DeviceGray /BitsPerComponent 8 /Length 1 >>\nstream\n0\nendstream");
        }
        objects[0] = "<< /Type /Catalog /Pages 2 0 R >>";
        objects[1] = $"<< /Type /Pages /Kids [{string.Join(' ', pages.Select(id => id + " 0 R"))}] /Count {pages.Count} >>";
        var source = new StringBuilder("%PDF-1.4\n"); var offsets = new List<int>();
        for (var i = 0; i < objects.Count; i++) { offsets.Add(source.Length); source.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
        var xref = source.Length; source.Append($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets) source.Append($"{offset:D10} 00000 n \n");
        source.Append($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return Encoding.ASCII.GetBytes(source.ToString());
    }
    public static string WriteFixtures()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "Fixtures"); Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, "english.pdf"), Pdf(English));
        File.WriteAllBytes(Path.Combine(directory, "vietnamese.docx"), Docx(Vietnamese));
        File.WriteAllBytes(Path.Combine(directory, "long.pdf"), Pdf(English + "\n" + string.Join('\n', Enumerable.Range(1, 80).Select(i => $"Project evidence {i}: tested validation and documented results."))));
        File.WriteAllBytes(Path.Combine(directory, "image-only.pdf"), Pdf(English, true));
        File.WriteAllBytes(Path.Combine(directory, "near-limit.docx"), Docx(English, 5 * 1024 * 1024 - 1));
        File.WriteAllBytes(Path.Combine(directory, "above-limit.docx"), Docx(English, 5 * 1024 * 1024 + 8192));
        return directory;
    }
}
