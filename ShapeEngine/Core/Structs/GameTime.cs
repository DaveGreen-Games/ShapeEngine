namespace ShapeEngine.Core.Structs;

public readonly struct GameTime
{
    /// <summary>
    /// Seconds since start of application
    /// </summary>
    public readonly double TotalSeconds;
    /// <summary>
    /// Frames since start of application
    /// </summary>
    public readonly int TotalFrames;
    /// <summary>
    /// Seconds since last frame
    /// </summary>
    public readonly double ElapsedSeconds;

    public GameTime()
    {
        TotalSeconds = 0;
        TotalFrames = 0;
        ElapsedSeconds = 0;
    }
    public GameTime(double totalSeconds, int totalFrames, double elapsedSeconds)
    {
        this.TotalSeconds = totalSeconds;
        this.TotalFrames = totalFrames;
        this.ElapsedSeconds = elapsedSeconds;
    }

    #region Tick
    public GameTime Tick(double dt) => new(TotalSeconds + dt, TotalFrames + 1, dt);
    public GameTime TickF(float dt) => new(TotalSeconds + dt, TotalFrames + 1, dt);
    #endregion
    
    #region Conversion
    public readonly double TotalDays => TotalSeconds / 86400;
    public readonly double TotalHours => TotalSeconds / 3600;
    public readonly double TotalMinutes => TotalSeconds / 60;
    
    /// <summary>
    /// 1/1000 of a seconds (1000 (1 Thousand) milli seconds = 1 second)
    /// </summary>
    public readonly double TotalMilliSeconds => TotalSeconds * 1000;
    /// <summary>
    /// 1/1000000 of a seconds (1.000.000 (1 Million) micro seconds = 1 second)
    /// </summary>
    public readonly double TotalMicroSeconds => TotalSeconds * 1000000;
    /// <summary>
    /// 1/1000000000 of a seconds (1.000.000.000 (1 Billion) nano seconds = 1 second)
    /// </summary>
    public readonly double TotalNanoSeconds => TotalSeconds * 1000000000;

    /// <summary>
    /// Elapsed seconds in float instead of double.
    /// </summary>
    public readonly float Delta => (float)ElapsedSeconds;
    public readonly int Fps => ElapsedSeconds <= 0f ? 0 : (int)(1f / ElapsedSeconds);
    /// <summary>
    /// 1/1000 of a seconds (1000 (1 Thousand) milli seconds = 1 second)
    /// </summary>
    public readonly double ElapsedMilliSeconds => ElapsedSeconds * 1000;
    /// <summary>
    /// 1/1000000 of a seconds (1.000.000 (1 Million) micro seconds = 1 second)
    /// </summary>
    public readonly double ElapsedMicroSeconds => ElapsedSeconds * 1000000;
    /// <summary>
    /// 1/1000000000 of a seconds (1.000.000.000 (1 Billion) nano seconds = 1 second)
    /// </summary>
    public readonly double ElapsedNanoSeconds => ElapsedSeconds * 1000000000;
    #endregion

    #region Static
    public static double ToMinutes(double seconds) => seconds / 60;
    public static double ToHours(double seconds) => seconds / 3600;
    public static double ToDays(double seconds) => seconds / 86400;
    
    /// <summary>
    /// 1/1000 of a seconds (1000 (1 Thousand) milli seconds = 1 second)
    /// </summary>
    public static double ToMilliSeconds(double seconds) => seconds / 1000;
    /// <summary>
    /// 1/1000000 of a seconds (1.000.000 (1 Million) micro seconds = 1 second)
    /// </summary>
    public static double ToMicroSeconds(double seconds) => seconds / 1000000;
    /// <summary>
    /// 1/1000000000 of a seconds (1.000.000.000 (1 Billion) nano seconds = 1 second)
    /// </summary>
    public static double ToNanoSeconds(double seconds) => seconds / 1000000000;
    #endregion
    
    
    
    // public readonly float Centuries = Decades / 10f;
    // public readonly float Decades = Years / 10f;
    // public readonly float Years = Months / 12f;
    // public readonly float Months => seconds / 2419200;
    // public readonly float Weeks => seconds / 604800;
}