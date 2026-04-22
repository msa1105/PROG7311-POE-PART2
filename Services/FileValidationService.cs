//Code attribution
//OpenAI. 2025. ChatGPT (Version GPT-4). [Large language model]
//Available at: https://chat.openai.com/
//[Accessed: 15 January 2025]

namespace PROG7311_POE.Services;

public class FileValidationService : IFileValidationService
{
    private static readonly string[] AllowedExtensions = { ".pdf" };
    private static readonly string[] AllowedMimeTypes = { "application/pdf" };
    private static readonly string[] BlockedExtensions = { ".exe", ".bat", ".cmd", ".com", ".scr", ".vbs", ".js" };

    public bool IsValidPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file provided.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // Explicitly block dangerous file types
        if (BlockedExtensions.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is strictly prohibited for security reasons.");

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is not allowed. Only PDF files are accepted.");

        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new InvalidOperationException($"MIME type '{file.ContentType}' is not allowed. Only PDF files are accepted.");

        return true;
    }
}
