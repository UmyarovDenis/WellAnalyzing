using MediatR;
using Microsoft.Extensions.Logging;
using WellAnalyzing.Application.Dto;
using WellAnalyzing.Application.Services.Interfaces;
using WellAnalyzing.Application.UseCases.GetGroupingWells;
using WellAnalyzing.Application.UseCases.GetValidationErrors;
using WellAnalyzing.Application.UseCases.GetWellSummaries;

namespace WellAnalyzing.Application.Services;

/// <inheritdoc/>
public sealed class WellReportSummaryService : IWellReportSummaryService
{
    private readonly IMediator _mediator;
    private readonly ILogger<WellReportSummaryService> _logger;

    /// <summary>
    /// .ctor.
    /// </summary>
    /// <param name="mediator"><see cref="IMediator"/>.</param>
    /// <param name="logger"><see cref="ILogger{TCategoryName}"/>.</param>
    public WellReportSummaryService(
        IMediator mediator,
        ILogger<WellReportSummaryService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<WellReportDto> CreateWellReportAsync(IReadOnlyList<RawWellDataDto> rawData)
    {
        try
        {
            var wells = await _mediator.Send(new GetGroupingWellsQuery(rawData));
            var validationErrors = await _mediator.Send(new GetValidationErrorsQuery(wells));
            var wellSummaries = await _mediator.Send(new GetWellSummariesQuery(wells));

            return new(wellSummaries, validationErrors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            throw;
        }
    }
}
