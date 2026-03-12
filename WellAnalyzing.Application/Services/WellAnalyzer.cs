using WellAnalyzing.Application.Extensions;
using WellAnalyzing.Application.Services.Interfaces;
using WellAnalyzing.Domain.Entities;

namespace WellAnalyzing.Application.Services;

/// <inheritdoc/>
public sealed class WellAnalyzer : IWellAnalyzer
{
    /// <inheritdoc/>
    public int CalculateIntervalCount(Well well) => well.Intervals.Count;

    /// <inheritdoc/>
    public float CalculatePorosityAverage(Well well)
    {
        float sumWeightedPorosity = 0;
        float sumLength = 0;

        foreach (var interval in well.Intervals)
        {
            sumWeightedPorosity += interval.Porosity * interval.Depth;
            sumLength += interval.Depth;
        }

        return sumWeightedPorosity / sumLength;
    }

    /// <inheritdoc/>
    public float CalculateTotalDepth(Well well)
    {
        var start = well.Intervals[0].DepthFrom;
        var end = well.Intervals[^1].DepthTo;

        return end - start;
    }

    /// <inheritdoc/>
    public string DetermineRock(Well well)
    {
        if (well.Intervals.All(i => i.RockType == null))
        {
            return "Порода не указана";
        }

        return
            well
                .Intervals
                .Where(i => i.RockType != null)
                .Select(i => (i.RockType, i.Depth))
                .GroupBy(x => x.RockType)
                .Select(x => (Rock: x.Key, TotalDepth: x.Sum(g => g.Depth)))
                .MaxBy(x => x.TotalDepth)
                .Rock
                .ToString()!;
    }
}
