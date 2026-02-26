using System.Diagnostics;

namespace ShapeEngine.Core;

//TODO: Add docs
public class PerformanceMeasureWatch
{
    #region Structs

    public record MeasurementData(string Title, string Tag, TimeSpan Total, TimeSpan Elapsed, long Count, TimeSpan Min, TimeSpan Max, TimeSpan Average)
    {
        public override string ToString()
        {
            return $"{Title} [{Tag}] -> Elapsed: {Elapsed}, Min: {Min}, Max: {Max}, Average: {Average} = {Total} / {Count}";
        }
        
        public string PrintSeconds()
        {
            return $"{Title} [{Tag}] -> Elapsed: {Elapsed.TotalSeconds:F6}s, Min: {Min.TotalSeconds:F6}s, Max: {Max.TotalSeconds:F6}s, Average: {Average.TotalSeconds:F6}s = {Total.TotalSeconds:F6}s / {Count}";
        }
        public string PrintMilliseconds()
        {
            return $"{Title} [{Tag}] -> Elapsed: {Elapsed.TotalMilliseconds:F3}ms, Min: {Min.TotalMilliseconds:F3}ms, Max: {Max.TotalMilliseconds:F3}ms, Average: {Average.TotalMilliseconds:F3}ms = {Total.TotalMilliseconds:F3}ms / {Count}";
        }
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

        public static double CompareElapsedPercent(MeasurementData a, MeasurementData b)
        {
            if (a.Elapsed == TimeSpan.Zero) return 0;
            return ((b.Elapsed.TotalSeconds - a.Elapsed.TotalSeconds) / a.Elapsed.TotalSeconds) * 100.0;
        }
        public static double CompareTotalPercent(MeasurementData a, MeasurementData b)
        {
            if (a.Total == TimeSpan.Zero) return 0;
            return ((b.Total.TotalSeconds - a.Total.TotalSeconds) / a.Total.TotalSeconds) * 100.0;
        }
        public static double CompareMinPercent(MeasurementData a, MeasurementData b)
        {
            if (a.Min == TimeSpan.Zero) return 0;
            return ((b.Min.TotalSeconds - a.Min.TotalSeconds) / a.Min.TotalSeconds) * 100.0;
        }
        public static double CompareMaxPercent(MeasurementData a, MeasurementData b)
        {
            if (a.Max == TimeSpan.Zero) return 0;
            return ((b.Max.TotalSeconds - a.Max.TotalSeconds) / a.Max.TotalSeconds) * 100.0;
        }
        public static double CompareAveragePercent(MeasurementData a, MeasurementData b)
        {
            if (a.Average == TimeSpan.Zero) return 0;
            return ((b.Average.TotalSeconds - a.Average.TotalSeconds) / a.Average.TotalSeconds) * 100.0;
        }
    }
    
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
    
    public struct MeasurementScope : IDisposable
    {
        private readonly PerformanceMeasureWatch watch;
        private readonly string title;
        private readonly string tag;
        
        public MeasurementScope(PerformanceMeasureWatch watch, string title, string tag)
        {
            this.watch = watch;
            this.title = title;
            this.tag = tag;
            watch.BeginMeasurement(title, tag);
        }

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
    public PerformanceMeasureWatch()
    {
        watch = new Stopwatch();
    }
    #endregion
    
    #region Functions
    /// <summary>
    /// Allows the use of using blocks for automatic measurement.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    /// <code>
    /// using (watch.Measure("MyOperation", "Tag"))
    /// {
    ///     // Code to measure
    /// }
    /// </code>
    public MeasurementScope Measure(string title, string tag = "")
    {
        return new MeasurementScope(this, title, tag);
    }
    
    public void Start()
    {
        lock (_lock)
        {
            watch.Restart();
            measurements.Clear();
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            watch.Stop();
            watch.Reset();
        }
    }
    
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
    public void ClearData()
    {
        lock (_lock)
        {
            measurements.Clear();
        }
    }
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