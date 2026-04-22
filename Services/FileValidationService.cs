//Code attribution
//Microsoft. 2023. File Uploads in ASP.NET Core.
//Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads
//[Accessed: 15 January 2025]
//
//Code attribution
//Stack Overflow. 2013. How to check file type (MIME type) when uploading in ASP.NET.
//Available at: https://stackoverflow.com/questions/11230020/check-file-mime-type-with-php-before-upload
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
