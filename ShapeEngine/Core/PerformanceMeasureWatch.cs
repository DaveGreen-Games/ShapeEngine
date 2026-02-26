using System.Diagnostics;

namespace ShapeEngine.Core;

//TODO: Add docs
//TODO: Add min/max/standard deviation as well
//TODO: Use logger instance instead of console
//TODO: Implement Auto-Measurement Scope? (Implement a disposable struct/class (e.g., using var m = watch.Measure("title");) for automatic timing.)
//TODO: Enforce minimum print interval?
//TODO: Remove Update() and timer auto printing? Just use a Print()/ GetData() methods with optional filters and tags?
//TODO: Add additional concurrent collections if high contention is expected for thread safety.
public class PerformanceMeasureWatch
{
    private struct Measurement
    {
        public long Start;
        public long Total;
        public int Count;
        
        public Measurement(long start)
        {
            Start = start;
            Total = 0;
            Count = 0;
        }

        private Measurement(long start, long total, int count)
        {
            Start = start;
            Total = total;
            Count = count;
        }
        public Measurement SetStart(long start)
        {
            return new Measurement(start, Total, Count);
        }
        public Measurement TakeMeasurement(long end)
        {
            var elapsed = end - Start;
            return new Measurement(0, Total + elapsed, Count + 1);
        }
        public long Average => Count > 0 ? Total / Count : 0;
    }
    
    #region Fields
    private readonly object _lock = new();
    private readonly Dictionary<string, Measurement> measurements = new();
    private readonly Stopwatch watch;
    private float timer = 0f;
    
    public float PrintIntervalSeconds { get; set; }
    #endregion
    
    #region Constructor
    public PerformanceMeasureWatch(float printIntervalSeconds)
    {
        watch = new Stopwatch();
        PrintIntervalSeconds = printIntervalSeconds;
    }
    #endregion
    
    #region Functions
    
    public void Start()
    {
        lock (_lock)
        {
            watch.Restart();
            timer = 0f;
            measurements.Clear();
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            Print();
            watch.Stop();
            watch.Reset();
            timer = 0f;
            measurements.Clear();
        }
    }
    
    public void BeginMeasurement(string title)
    {
        lock (_lock)
        {
            if (measurements.ContainsKey(title))
            {
                measurements[title] = measurements[title].SetStart(watch.ElapsedTicks);
            }
            else
            {
                measurements.Add(title, new Measurement(watch.ElapsedTicks));
            }
        }
    }

    public void EndMeasurement(string title)
    {
        lock (_lock)
        {
            if (!measurements.TryGetValue(title, out var measurement)) return; //it was never started
            if (measurement.Start <= 0) return; //it was never started
            measurements[title] = measurement.TakeMeasurement(watch.ElapsedTicks);
        }
    }

    public void Update(float deltaTime)
    {
        lock (_lock)
        {
            if (!watch.IsRunning) return;
            
            timer += deltaTime;
            if (timer >= PrintIntervalSeconds)
            {
                Print();
                timer = 0f;
                measurements.Clear();
            }
        }
    }
    
    public void Print()
    {
        lock (_lock)
        {
            if (measurements.Count <= 0)
            {
                Console.WriteLine("No measurements taken.");
                return;
            }
            
            Console.WriteLine($"Performance Measurments Info from last {timer:F2}s:");
            foreach (var kvp in measurements)
            {
                var title = kvp.Key;
                var measurement = kvp.Value;
                Console.WriteLine($" - {title}: Total Ticks = {measurement.Total}, Average Ticks = {measurement.Average}, Count {measurement.Count}");
            }
            Console.WriteLine("-----------------------------");
        }
    }
    
    #endregion
}