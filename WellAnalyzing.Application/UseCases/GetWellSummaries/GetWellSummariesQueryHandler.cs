using MediatR;
using WellAnalyzing.Application.Dto;
using WellAnalyzing.Application.Services.Interfaces;

namespace WellAnalyzing.Application.UseCases.GetWellSummaries;

/// <summary>
/// Обработчик запроса <see cref="GetWellSummariesQuery"/>.
/// </summary>
public sealed class GetWellSummariesQueryHandler : IRequestHandler<GetWellSummariesQuery, WellSummaryDto[]>
{
    private readonly IWellAnalyzer _wellAnalyzer;

    public GetWellSummariesQueryHandler(IWellAnalyzer wellCalculator)
    {
        _wellAnalyzer = wellCalculator;
    }

    /// <inheritdoc />
    public Task<WellSummaryDto[]> Handle(GetWellSummariesQuery request, CancellationToken cancellationToken)
    {
        var summaries =
            request
                .WellSummaryDtos
                .Select(
                    w =>
                        new WellSummaryDto(
                            WellId: w.Id,
                            Position: w.Position,
                            TotalDepth: _wellAnalyzer.CalculateTotalDepth(w),
                            IntervalCount: _wellAnalyzer.CalculateIntervalCount(w),
                            PorosityAverage: _wellAnalyzer.CalculatePorosityAverage(w),
                            Rock: _wellAnalyzer.DetermineRock(w)));

        return Task.FromResult<WellSummaryDto[]>([.. summaries]);
    }
}
