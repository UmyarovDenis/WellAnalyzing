using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WellAnalyzing.Application.Dto;
using WellAnalyzing.Application.Services.Interfaces;
using WellAnalyzing.Services.Interfaces;

namespace WellAnalyzing.Views;

/// <summary>
/// Контекст данных для <see cref="ShellView"/>.
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly INotificationService _notificationService;
    private readonly IWellReportSummaryService _reportSummaryService;
    private readonly ILogger<ShellViewModel> _logger;

    /// <summary>
    /// .ctor.
    /// </summary>
    /// <param name="fileService"><see cref="IFileService"/>.</param>
    /// <param name="notificationService"><see cref="INotificationService"/>.</param>
    /// <param name="reportSummaryService"><see cref="IWellReportSummaryService"/>.</param>
    /// <param name="logger"><see cref="ILogger{TCategoryName}"/>.</param>
    public ShellViewModel(
        IFileService fileService,
        INotificationService notificationService,
        IWellReportSummaryService reportSummaryService,
        ILogger<ShellViewModel> logger)
    {
        _fileService = fileService;
        _notificationService = notificationService;
        _reportSummaryService = reportSummaryService;
        _logger = logger;

        WellSummaries = [];
        ValidationErrors = [];
    }

    /// <summary>
    /// Наблюдаемый список <see cref="WellSummaryDto"/>.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<WellSummaryDto> WellSummaries { get; private set; }

    /// <summary>
    /// Наблюдаемый список сообщений об ошибках.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<string> ValidationErrors { get; private set; }

    [RelayCommand]
    private async Task ImportRawData()
    {
        try
        {
            if (_fileService.TryGetCsvFilePath(out var filePath))
            {
                var rawData = new List<RawWellDataDto>();
                await foreach (var item in _fileService.EnumerateCsvRawDataAsync(filePath))
                {
                    rawData.Add(item);
                }

                var wellReport = await _reportSummaryService.CreateWellReportAsync(rawData);
                var (wellSummary, errors) = wellReport;

                WellSummaries = [.. wellSummary];

                if (errors != null)
                {
                    ValidationErrors = [.. errors];
                }
            }
        }
        catch (Exception ex)
        {
            var errMessage = "Возникли ошибки при формировании сводной информации по скважинам";

            _logger.LogError(ex, "{ErrMessage}: {Details}", errMessage, ex.Message);
            _notificationService.ShowError(errMessage);
        }
    }

    [RelayCommand]
    private async Task ExportJsonData()
    {
        if (WellSummaries.Count == 0)
        {
            _notificationService.ShowMessage("Сформируйте данные для экспорта, путём загрузки из .csv файла");
            return;
        }

        try
        {
            if (_fileService.TryGetJsonSavePath(out var path))
            {
                await _fileService.ExportJsonDataAsync(WellSummaries, path);
                _notificationService.ShowMessage("Данные успешно сохранены");
            }
        }
        catch (Exception ex)
        {
            var errMessage = "Возникли ошибки при экспорте сводной информации по скважинам";

            _logger.LogError(ex, "{ErrMessage}: {Details}", errMessage, ex.Message);
            _notificationService.ShowError(errMessage);
        }
    }
}
