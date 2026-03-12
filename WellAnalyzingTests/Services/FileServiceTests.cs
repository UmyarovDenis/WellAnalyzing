using Microsoft.Extensions.Logging;
using Moq;
using System.Numerics;
using System.Text;
using WellAnalyzing.Application.Dto;
using WellAnalyzing.Services;

namespace WellAnalyzing.Tests.Services;

public class FileServiceTests
{
    private readonly Mock<ILogger<FileService>> _loggerMock;
    private readonly FileService _sut;

    public FileServiceTests()
    {
        _loggerMock = new Mock<ILogger<FileService>>();
        _sut = new FileService(_loggerMock.Object);
    }

    [Fact]
    public async Task EnumerateCsvRawDataAsync_ValidCsvFile_ReturnsAllRecords()
    {
        var csvContent = @"WellId;X;Y;DepthFrom;DepthTo;Rock;Porosity
A-001;82.10;55.20;0.00;10.00;Sandstone;0.18
A-001;82.10;55.20;10.00;25.00;Limestone;0.07
A-002;90.00;60.00;0.00;15.00;Shale;0.04";

        var filePath = CreateTempFile(csvContent);

        // Act
        var results = new List<RawWellDataDto>();
        await foreach (var record in _sut.EnumerateCsvRawDataAsync(filePath))
        {
            results.Add(record);
        }

        Assert.Equal(3, results.Count);

        var firstRecord = results[0];
        Assert.Equal("A-001", firstRecord.WellId);
        Assert.Equal(82.10f, firstRecord.X);
        Assert.Equal(55.20f, firstRecord.Y);
        Assert.Equal(0.00f, firstRecord.DepthFrom);
        Assert.Equal(10.00f, firstRecord.DepthTo);
        Assert.Equal("Sandstone", firstRecord.Rock);
        Assert.Equal(0.18f, firstRecord.Porosity);

        var thirdRecord = results[2];
        Assert.Equal("A-002", thirdRecord.WellId);
        Assert.Equal(90.00f, thirdRecord.X);
        Assert.Equal(60.00f, thirdRecord.Y);
        Assert.Equal(0.00f, thirdRecord.DepthFrom);
        Assert.Equal(15.00f, thirdRecord.DepthTo);
        Assert.Equal("Shale", thirdRecord.Rock);
        Assert.Equal(0.04f, thirdRecord.Porosity);

        File.Delete(filePath);
    }

    [Fact]
    public async Task EnumerateCsvRawDataAsync_EmptyCsvFile_ReturnsNoRecords()
    {
        var csvContent = @"WellId;X;Y;DepthFrom;DepthTo;Rock;Porosity";
        var filePath = CreateTempFile(csvContent);
        var results = new List<RawWellDataDto>();

        await foreach (var record in _sut.EnumerateCsvRawDataAsync(filePath))
        {
            results.Add(record);
        }

        Assert.Empty(results);

        File.Delete(filePath);
    }

    [Fact]
    public async Task EnumerateCsvRawDataAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        var nonExistentPath = "nonexistent.csv";

        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await foreach (var _ in _sut.EnumerateCsvRawDataAsync(nonExistentPath))
            {

            }
        });
    }

    [Fact]
    public async Task ExportJsonDataAsync_EmptyList_CreatesEmptyJsonArray()
    {
        var emptyList = new List<WellSummaryDto>();
        var tempFilePath = Path.GetTempFileName();

        try
        {
            await _sut.ExportJsonDataAsync(emptyList, tempFilePath);

            Assert.True(File.Exists(tempFilePath));

            var jsonContent = await File.ReadAllTextAsync(tempFilePath);
            Assert.Equal("[]", jsonContent.Trim());
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Fact]
    public async Task ExportJsonDataAsync_InvalidPath_ThrowsExceptionAndLogsError()
    {
        var wellSummaries = new List<WellSummaryDto>
        {
            new("Test", new Vector2(0, 0), 100, 1, 0.15f, "Rock")
        };

        var invalidPath = "invalid:/path?*.json";

        var exception = await Assert.ThrowsAsync<IOException>(async () =>
            await _sut.ExportJsonDataAsync(wellSummaries, invalidPath));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    private static string CreateTempFile(string content)
    {
        var tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, content, Encoding.UTF8);
        return tempFilePath;
    }
}