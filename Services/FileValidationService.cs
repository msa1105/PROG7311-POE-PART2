namespace PROG7311_POE.Services;

public class FileValidationService : IFileValidationService
{
    private static readonly string[] AllowedExtensions = { ".pdf" };
    private static readonly string[] AllowedMimeTypes = { "application/pdf" };

    public bool IsValidPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file provided.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is not allowed. Only PDF files are accepted.");

        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new InvalidOperationException($"MIME type '{file.ContentType}' is not allowed. Only PDF files are accepted.");

        return true;
    }
}
