using WellAnalyzing.Domain.Entities;

namespace WellAnalyzing.Application.Extensions;

/// <summary>
/// Придоставляет расширения для <see cref="Interval"/>.
/// </summary>
internal static class IntervalExtensions
{
    /// <summary>
    /// Блок расширений свойств <see cref="Interval"/>.
    /// </summary>
    /// <param name="interval"><see cref="Interval"/>.</param>
    extension(Interval interval)
    {
        /// <summary>
        /// Глубина интервала, м.
        /// </summary>
        public float Depth => interval.DepthTo - interval.DepthFrom;
    }
}
