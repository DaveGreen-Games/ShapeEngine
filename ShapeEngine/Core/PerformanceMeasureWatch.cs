using System.Diagnostics;

namespace ShapeEngine.Core;

//TODO: Add docs
public class PerformanceMeasureWatch
{
    #region Structs

    public record MeasurementData(string Title, string Tag, long Total, long Elapsed, long Count, long Min, long Max, long Average)
    {
        public override string ToString()
        {
            return $"{Title} [{Tag}] -> Elapsed: {Elapsed}, Min: {Min}, Max: {Max}, Average: {Average} = {Total} / {Count}";
        }
    }
    
    private struct Measurement
    {
        public long Start;
        public long Total;
        public long Count;
        public long Elapsed;
        public long Min;
        public long Max;
        public string Tag;
        
        public Measurement(long start, string tag)
        {
            Start = start;
            Total = 0;
            Count = 0;
            Elapsed = 0;
            Min = long.MaxValue;
            Max = long.MinValue;
            Tag = tag;
        }

        private Measurement(long start, long total, long elapsed, long count, long min, long max, string tag)
        {
            Start = start;
            Total = total;
            Elapsed = elapsed;
            Count = count;
            Min = min;
            Max = max;
            Tag = tag;
        }
        public Measurement SetStart(long start)
        {
            return new Measurement(start, Total, Elapsed, Count, Min, Max, Tag);
        }
        public Measurement TakeMeasurement(long end)
        {
            var elapsed = end - Start;
            var newMin = Math.Min(Min, elapsed);
            var newMax = Math.Max(Max, elapsed);
            return new Measurement(0, Total + elapsed, elapsed, Count + 1, newMin, newMax, Tag);
        }
        public long Average => Count > 0 ? Total / Count : 0;
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
                measurements[key] = measurements[key].SetStart(watch.ElapsedTicks);
            }
            else
            {
                measurements.Add(key, new Measurement(watch.ElapsedTicks, tag));
            }
        }
    }

    public void EndMeasurement(string title, string tag = "")
    {
        lock (_lock)
        {
            var key = (title, tag);
            if (!measurements.TryGetValue(key, out var measurement)) return; //it was never started
            if (measurement.Start <= 0) return; //it was never started
            measurements[key] = measurement.TakeMeasurement(watch.ElapsedTicks);
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
    
    // public void Print()
    // {
    //     lock (_lock)
    //     {
    //         if (measurements.Count <= 0)
    //         {
    //             Console.WriteLine("No measurements taken.");
    //             return;
    //         }
    //         
    //         Console.WriteLine($"Performance Measurments Info from last {timer:F2}s:");
    //         foreach (var kvp in measurements)
    //         {
    //             var title = kvp.Key;
    //             var measurement = kvp.Value;
    //             Console.WriteLine($" - {title}: Total Ticks = {measurement.Total}, Average Ticks = {measurement.Average}, Count {measurement.Count}");
    //         }
    //         Console.WriteLine("-----------------------------");
    //     }
    // }
    
    #endregion
}