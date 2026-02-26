using System.Diagnostics;

namespace ShapeEngine.Core;

/// <summary>
/// Provides functionality for measuring and recording performance timings for code sections, supporting tagging and data retrieval.
/// </summary>
public class PerformanceMeasureWatch
{
    #region Structs

    /// <summary>
    /// Represents a set of performance measurement statistics for a specific title and tag.
    /// </summary>
    /// <param name="Title">The name of the measured code section.</param>
    /// <param name="Tag">A category or tag for grouping/filtering measurements.</param>
    /// <param name="Total">The total measured time across all samples.</param>
    /// <param name="Elapsed">The most recent measured elapsed time.</param>
    /// <param name="Count">The number of measurements taken.</param>
    /// <param name="Min">The minimum measured time.</param>
    /// <param name="Max">The maximum measured time.</param>
    /// <param name="Average">The average measured time.</param>
    /// <remarks>
    /// Provides print and comparison utilities for different time units and statistics.
    /// </remarks>
    public record MeasurementData(string Title, string Tag, TimeSpan Total, TimeSpan Elapsed, long Count, TimeSpan Min, TimeSpan Max, TimeSpan Average)
    {
        /// <summary>
        /// Returns a string representation of the measurement data using the default TimeSpan format.
        /// </summary>
        public override string ToString()
        {
            return $"{Title} [{Tag}] -> Elapsed: {Elapsed}, Min: {Min}, Max: {Max}, Average: {Average} = {Total} / {Count}";
        }
        /// <summary>
        /// Returns a string representation of the measurement data in seconds with fixed precision.
        /// </summary>
        public string PrintSeconds()
        {
            return $"{Title} [{Tag}] -> Elapsed: {Elapsed.TotalSeconds:F6}s, Min: {Min.TotalSeconds:F6}s, Max: {Max.TotalSeconds:F6}s, Average: {Average.TotalSeconds:F6}s = {Total.TotalSeconds:F6}s / {Count}";
        }
        /// <summary>
        /// Returns a string representation of the measurement data in milliseconds with fixed precision.
        /// </summary>
        public string PrintMilliseconds()
        {
            return $"{Title} [{Tag}] -> Elapsed: {Elapsed.TotalMilliseconds:F3}ms, Min: {Min.TotalMilliseconds:F3}ms, Max: {Max.TotalMilliseconds:F3}ms, Average: {Average.TotalMilliseconds:F3}ms = {Total.TotalMilliseconds:F3}ms / {Count}";
        }
        /// <summary>
        /// Returns a string representation of the measurement data in nanoseconds, calculated using Stopwatch.Frequency.
        /// </summary>
        /// <remarks>
        /// Nanosecond values are calculated from ticks and Stopwatch.Frequency for accuracy.
        /// </remarks>
        public string PrintNanoseconds()
        {
            // Convert ticks to nanoseconds using Stopwatch.Frequency
            double ticksPerSecond = Stopwatch.Frequency;
            double elapsedNs = Elapsed.Ticks * 1_000_000_000.0 / ticksPerSecond;
            double minNs = Min.Ticks * 1_000_000_000.0 / ticksPerSecond;
            double maxNs = Max.Ticks * 1_000_000_000.0 / ticksPerSecond;
            double avgNs = Average.Ticks * 1_000_000_000.0 / ticksPerSecond;
            double totalNs = Total.Ticks * 1_000_000_000.0 / ticksPerSecond;
            return $"{Title} [{Tag}] -> Elapsed: {elapsedNs:F0}ns, Min: {minNs:F0}ns, Max: {maxNs:F0}ns, Average: {avgNs:F0}ns = {totalNs:F0}ns / {Count}";
        }
        /// <summary>
        /// Calculates the percentage difference in elapsed time between two measurement data entries.
        /// </summary>
        /// <param name="a">The baseline measurement data.</param>
        /// <param name="b">The measurement data to compare against the baseline.</param>
        /// <returns>The percentage difference in elapsed time.</returns>
        public static double CompareElapsedPercent(MeasurementData a, MeasurementData b)
        {
            if (a.Elapsed == TimeSpan.Zero) return 0;
            return ((b.Elapsed.TotalSeconds - a.Elapsed.TotalSeconds) / a.Elapsed.TotalSeconds) * 100.0;
        }
        /// <summary>
        /// Calculates the percentage difference in total time between two measurement data entries.
        /// </summary>
        /// <param name="a">The baseline measurement data.</param>
        /// <param name="b">The measurement data to compare against the baseline.</param>
        /// <returns>The percentage difference in total time.</returns>
        public static double CompareTotalPercent(MeasurementData a, MeasurementData b)
        {
            if (a.Total == TimeSpan.Zero) return 0;
            return ((b.Total.TotalSeconds - a.Total.TotalSeconds) / a.Total.TotalSeconds) * 100.0;
        }
        /// <summary>
        /// Calculates the percentage difference in minimum measured time between two measurement data entries.
        /// </summary>
        /// <param name="a">The baseline measurement data.</param>
        /// <param name="b">The measurement data to compare against the baseline.</param>
        /// <returns>The percentage difference in minimum measured time.</returns>
        public static double CompareMinPercent(MeasurementData a, MeasurementData b)
        {
            if (a.Min == TimeSpan.Zero) return 0;
            return ((b.Min.TotalSeconds - a.Min.TotalSeconds) / a.Min.TotalSeconds) * 100.0;
        }
        /// <summary>
        /// Calculates the percentage difference in maximum measured time between two measurement data entries.
        /// </summary>
        /// <param name="a">The baseline measurement data.</param>
        /// <param name="b">The measurement data to compare against the baseline.</param>
        /// <returns>The percentage difference in maximum measured time.</returns>
        public static double CompareMaxPercent(MeasurementData a, MeasurementData b)
        {
            if (a.Max == TimeSpan.Zero) return 0;
            return ((b.Max.TotalSeconds - a.Max.TotalSeconds) / a.Max.TotalSeconds) * 100.0;
        }
        /// <summary>
        /// Calculates the percentage difference in average measured time between two measurement data entries.
        /// </summary>
        /// <param name="a">The baseline measurement data.</param>
        /// <param name="b">The measurement data to compare against the baseline.</param>
        /// <returns>The percentage difference in average measured time.</returns>
        public static double CompareAveragePercent(MeasurementData a, MeasurementData b)
        {
            if (a.Average == TimeSpan.Zero) return 0;
            return ((b.Average.TotalSeconds - a.Average.TotalSeconds) / a.Average.TotalSeconds) * 100.0;
        }
    }
    
    /// <summary>
    /// Internal struct for accumulating measurement statistics for a specific title and tag.
    /// </summary>
    /// <remarks>
    /// Not intended for public use.
    /// </remarks>
    private struct Measurement
    {
        public TimeSpan Start;
        public TimeSpan Total;
        public long Count;
        public TimeSpan Elapsed;
        public TimeSpan Min;
        public TimeSpan Max;
        public string Tag;
        
        public Measurement(TimeSpan start, string tag)
        {
            Start = start;
            Total = TimeSpan.Zero;
            Count = 0;
            Elapsed = TimeSpan.Zero;
            Min = TimeSpan.MaxValue;
            Max = TimeSpan.MinValue;
            Tag = tag;
        }

        private Measurement(TimeSpan start, TimeSpan total, TimeSpan elapsed, long count, TimeSpan min, TimeSpan max, string tag)
        {
            Start = start;
            Total = total;
            Elapsed = elapsed;
            Count = count;
            Min = min;
            Max = max;
            Tag = tag;
        }
        public Measurement SetStart(TimeSpan start)
        {
            return new Measurement(start, Total, Elapsed, Count, Min, Max, Tag);
        }
        public Measurement TakeMeasurement(TimeSpan end)
        {
            var elapsed = end - Start;
            var newMin = Min < elapsed ? Min : elapsed;
            var newMax = Max > elapsed ? Max : elapsed;
            return new Measurement(TimeSpan.Zero, Total + elapsed, elapsed, Count + 1, newMin, newMax, Tag);
        }
        public TimeSpan Average => Count > 0 ? TimeSpan.FromTicks(Total.Ticks / Count) : TimeSpan.Zero;
    }
    
    /// <summary>
    /// Provides a disposable scope for automatic measurement of code execution time.
    /// </summary>
    /// <remarks>
    /// Use with a using statement to automatically begin and end a measurement.
    /// </remarks>
    public struct MeasurementScope : IDisposable
    {
        /// <summary>
        /// The parent PerformanceMeasureWatch instance.
        /// </summary>
        private readonly PerformanceMeasureWatch watch;
        /// <summary>
        /// The name of the measured code section.
        /// </summary>
        private readonly string title;
        /// <summary>
        /// A category or tag for grouping/filtering measurements.
        /// </summary>
        private readonly string tag;
        
        /// <summary>
        /// Initializes a new measurement scope for the specified title and tag.
        /// </summary>
        /// <param name="watch">The parent PerformanceMeasureWatch instance.</param>
        /// <param name="title">The name of the measured code section.</param>
        /// <param name="tag">A category or tag for grouping/filtering measurements.</param>
        public MeasurementScope(PerformanceMeasureWatch watch, string title, string tag)
        {
            this.watch = watch;
            this.title = title;
            this.tag = tag;
            watch.BeginMeasurement(title, tag);
        }

        /// <summary>
        /// Ends the measurement for the associated title and tag.
        /// </summary>
        public void Dispose()
        {
            watch.EndMeasurement(title, tag);
        }
    }
    #endregion
    
    #region Fields
    private readonly object _lock = new();
    private readonly Dictionary<(string Title, string Tag), Measurement> measurements = new();
    private readonly Stopwatch watch;
    #endregion
    
    #region Constructor
    /// <summary>
    /// Initializes a new instance of the PerformanceMeasureWatch class.
    /// </summary>
    public PerformanceMeasureWatch()
    {
        watch = new Stopwatch();
    }
    #endregion
    
    #region Functions
    /// <summary>
    /// Creates a disposable measurement scope for the specified title and tag.
    /// </summary>
    /// <param name="title">The name of the measured code section.</param>
    /// <param name="tag">A category or tag for grouping/filtering measurements.</param>
    /// <returns>A disposable scope that automatically begins and ends the measurement.</returns>
    /// <remarks>
    /// Use with a using statement for automatic measurement.
    /// </remarks>
    public MeasurementScope Measure(string title, string tag = "")
    {
        return new MeasurementScope(this, title, tag);
    }
    
    /// <summary>
    /// Starts the performance watch and clears all previous measurements.
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            watch.Restart();
            measurements.Clear();
        }
    }

    /// <summary>
    /// Stops and resets the performance watch.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            watch.Stop();
            watch.Reset();
        }
    }
    
    /// <summary>
    /// Begins a new measurement for the specified title and tag.
    /// </summary>
    /// <param name="title">The name of the measured code section.</param>
    /// <param name="tag">A category or tag for grouping/filtering measurements.</param>
    public void BeginMeasurement(string title, string tag = "")
    {
        lock (_lock)
        {
            var key = (title, tag);
            if (measurements.ContainsKey(key))
            {
                measurements[key] = measurements[key].SetStart(watch.Elapsed);
            }
            else
            {
                measurements.Add(key, new Measurement(watch.Elapsed, tag));
            }
        }
    }

    /// <summary>
    /// Ends the measurement for the specified title and tag, recording the elapsed time.
    /// </summary>
    /// <param name="title">The name of the measured code section.</param>
    /// <param name="tag">A category or tag for grouping/filtering measurements.</param>
    public void EndMeasurement(string title, string tag = "")
    {
        lock (_lock)
        {
            var key = (title, tag);
            if (!measurements.TryGetValue(key, out var measurement)) return; //it was never started
            if (measurement.Start == TimeSpan.Zero) return; //it was never started
            measurements[key] = measurement.TakeMeasurement(watch.Elapsed);
        }
    }
    /// <summary>
    /// Clears all recorded measurement data.
    /// </summary>
    public void ClearData()
    {
        lock (_lock)
        {
            measurements.Clear();
        }
    }
    /// <summary>
    /// Retrieves all recorded measurement data, optionally filtered by a predicate.
    /// </summary>
    /// <param name="filter">An optional filter predicate to select specific measurement data.</param>
    /// <returns>An enumerable of MeasurementData records.</returns>
    public IEnumerable<MeasurementData> GetData(Func<MeasurementData, bool>? filter = null)
    {
        lock (_lock)
        {
            var data = measurements.Select(kvp =>
                new MeasurementData(
                    kvp.Key.Title,
                    kvp.Key.Tag,
                    kvp.Value.Total,
                    kvp.Value.Elapsed,
                    kvp.Value.Count,
                    kvp.Value.Min,
                    kvp.Value.Max,
                    kvp.Value.Average
                )
            );
            return filter != null ? data.Where(filter).ToList() : data.ToList();
        }
    }
    
    #endregion
}

