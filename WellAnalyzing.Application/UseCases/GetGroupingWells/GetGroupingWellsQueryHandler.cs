using MediatR;
using System.Numerics;
using WellAnalyzing.Domain.Entities;
using WellAnalyzing.Domain.Enums;

using static System.StringComparison;
using static WellAnalyzing.Domain.Enums.RockType;

namespace WellAnalyzing.Application.UseCases.GetGroupingWells;

/// <summary>
/// Обработчик запроса <see cref="GetGroupingWellsQuery"/>.
/// </summary>
public sealed class GetGroupingWellsQueryHandler : IRequestHandler<GetGroupingWellsQuery, Well[]>
{
    /// <inheritdoc />
    public Task<Well[]> Handle(GetGroupingWellsQuery request, CancellationToken cancellationToken)
    {
        var wells =
            request
                .RawWells
                .GroupBy(w => w.WellId)
                .Select(g =>
                {
                    var wellId = g.Key;
                    var rawIntervals = g.ToArray();
                    var position = new Vector2(rawIntervals[0].X, rawIntervals[0].Y);

                    var intervals =
                        rawIntervals
                            .Select(
                                i =>
                                    new Interval(
                                        i.DepthFrom,
                                        i.DepthTo,
                                        ConvertFromString(i.Rock),
                                        i.Porosity))
                            .ToArray();

                    return new Well(wellId, position, intervals);
                });

        return Task.FromResult<Well[]>([.. wells]);
    }

    private static RockType? ConvertFromString(string? value) =>
        value switch
        {
            not null when string.Equals(nameof(Sandstone), value, OrdinalIgnoreCase) => Sandstone,
            not null when string.Equals(nameof(Limestone), value, OrdinalIgnoreCase) => Limestone,
            not null when string.Equals(nameof(Shale), value, OrdinalIgnoreCase) => Shale,
            _ => null,
        };
}
