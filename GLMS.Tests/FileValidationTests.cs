using PROG7311_POE.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GLMS.Tests;

/// <summary>
/// Test 2 – Verify file type validation logic.
/// </summary>
public class FileValidationTests
{
    private readonly FileValidationService _sut = new();

    private static IFormFile MakeFakeFile(string fileName, string contentType)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.ContentType).Returns(contentType);
        mock.Setup(f => f.Length).Returns(1024);
        return mock.Object;
    }

    [Fact]
    public void IsValidPdf_WithPdfFile_ReturnsTrue()
    {
        var file = MakeFakeFile("agreement.pdf", "application/pdf");
        var result = _sut.IsValidPdf(file);
        Assert.True(result);
    }

    [Theory]
    [InlineData("malware.exe", "application/octet-stream")]
    [InlineData("document.txt", "text/plain")]
    [InlineData("spreadsheet.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("image.png", "image/png")]
    public void IsValidPdf_WithInvalidFileType_ThrowsInvalidOperationException(
        string fileName, string contentType)
    {
        var file = MakeFakeFile(fileName, contentType);
        Assert.Throws<InvalidOperationException>(() => _sut.IsValidPdf(file));
    }

    [Fact]
    public void IsValidPdf_WithNullFile_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.IsValidPdf(null!));
    }
}
