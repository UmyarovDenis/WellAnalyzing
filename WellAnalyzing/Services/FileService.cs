using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using WellAnalyzing.Application.Dto;
using WellAnalyzing.Services.Interfaces;

namespace WellAnalyzing.Services;

/// <inheritdoc />
public sealed class FileService : IFileService
{
    private static readonly CsvConfiguration Config = new(CultureInfo.InvariantCulture) { Delimiter = ";" };
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly ILogger<FileService> _logger;

    /// <summary>
    /// .ctor.
    /// </summary>
    /// <param name="logger"><see cref="ILogger{TCategoryName}"/>.</param>
    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<RawWellDataDto> EnumerateCsvRawDataAsync(
        string filePath,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        using var streamReader = new StreamReader(filePath);
        using var csvReader = new CsvReader(streamReader, Config);

        await foreach(var rawData in csvReader.GetRecordsAsync<RawWellDataDto>(token))
        {
            yield return rawData;
        }
    }

    /// <inheritdoc />
    public async Task ExportJsonDataAsync(IReadOnlyList<WellSummaryDto> wellSummaries, string filePath, CancellationToken token = default)
    {
        try
        {
            using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, wellSummaries, Options, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "При экспорте данных в Json возникли ошибки: {Message}", ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public bool TryGetCsvFilePath([NotNullWhen(returnValue: true)] out string? filePath)
    {
        filePath = null;

        var openFileDialog = new OpenFileDialog
        {
            Filter = "Данные по скважинам (.csv)|*.csv",
            DefaultExt = ".csv"
        };

        bool? result = openFileDialog.ShowDialog();

        if (result == true)
        {
            filePath = openFileDialog.FileName;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool TryGetJsonSavePath([NotNullWhen(returnValue: true)] out string? filePath)
    {
        filePath = null;

        var saveFileDialog = new SaveFileDialog()
        {
            Filter = "Сводная информация по скважинам (.json)|*.json",
        };

        bool? result = saveFileDialog.ShowDialog();

        if (result == true)
        {
            filePath = saveFileDialog.FileName;
            return true;
        }

        return false;
    }
}
