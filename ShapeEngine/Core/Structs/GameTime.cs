namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents timing information for game loops and frame updates.
/// </summary>
/// <remarks>
/// GameTime provides timing data for both variable and fixed timestep game loops.
/// It tracks total elapsed time since application start, frame counts, and time between frames.
/// This struct is immutable and provides various time unit conversions for game timing needs.
/// </remarks>
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

    /// <summary>
    /// Initializes a new instance of the GameTime struct with default values.
    /// </summary>
    /// <remarks>
    /// Creates a GameTime with zero values for total seconds, total frames, and elapsed seconds.
    /// </remarks>
    public GameTime()
    {
        TotalSeconds = 0;
        TotalFrames = 0;
        ElapsedSeconds = 0;
    }
    /// <summary>
    /// Initializes a new instance of the GameTime struct with specified values.
    /// </summary>
    /// <param name="totalSeconds">The total seconds elapsed since the start of the application.</param>
    /// <param name="totalFrames">The total number of frames processed since the start of the application.</param>
    /// <param name="elapsedSeconds">The seconds elapsed since the last frame.</param>
    public GameTime(double totalSeconds, int totalFrames, double elapsedSeconds)
    {
        this.TotalSeconds = totalSeconds;
        this.TotalFrames = totalFrames;
        this.ElapsedSeconds = elapsedSeconds;
    }

    #region Tick
    /// <summary>
    /// Advances the game time by the specified time delta.
    /// </summary>
    /// <param name="dt">The time delta in seconds to advance the game time by.</param>
    /// <returns>A new GameTime instance with updated total seconds, incremented frame count, and the provided delta as elapsed seconds.</returns>
    public GameTime Tick(double dt) => new(TotalSeconds + dt, TotalFrames + 1, dt);

    /// <summary>
    /// Advances the game time by the specified time delta using a float value.
    /// </summary>
    /// <param name="dt">The time delta in seconds (as float) to advance the game time by.</param>
    /// <returns>A new GameTime instance with updated total seconds, incremented frame count, and the provided delta as elapsed seconds.</returns>
    public GameTime TickF(float dt) => new(TotalSeconds + dt, TotalFrames + 1, dt);
    #endregion
    
    #region Conversion
    /// <summary>
    /// Gets the total elapsed time in days since the application started.
    /// </summary>
    /// <value>
    /// The total elapsed time in days (TotalSeconds divided by 86400).
    /// </value>
    public readonly double TotalDays => TotalSeconds / 86400;
    
    /// <summary>
    /// Gets the total elapsed time in hours since the application started.
    /// </summary>
    /// <value>
    /// The total elapsed time in hours (TotalSeconds divided by 3600).
    /// </value>
    public readonly double TotalHours => TotalSeconds / 3600;
    
    /// <summary>
    /// Gets the total elapsed time in minutes since the application started.
    /// </summary>
    /// <value>
    /// The total elapsed time in minutes (TotalSeconds divided by 60).
    /// </value>
    public readonly double TotalMinutes => TotalSeconds / 60;
    
    /// <summary>
    /// 1/1000 of a seconds (1000 (1 Thousand) milliseconds = 1 second)
    /// </summary>
    public readonly double TotalMilliSeconds => TotalSeconds * 1000;
    /// <summary>
    /// 1/1000000 of a seconds (1.000.000 (1 Million) micro seconds = 1 second)
    /// </summary>
    public readonly double TotalMicroSeconds => TotalSeconds * 1000000;
    /// <summary>
    /// 1/1000000000 of a seconds (1.000.000.000 (1 Billion) nanoseconds = 1 second)
    /// </summary>
    public readonly double TotalNanoSeconds => TotalSeconds * 1000000000;

    /// <summary>
    /// Elapsed seconds in float instead of double.
    /// </summary>
    public readonly float Delta => (float)ElapsedSeconds;
    
    /// <summary>
    /// Gets the current frames per second (FPS) based on the elapsed time between frames.
    /// </summary>
    /// <remarks>
    /// This property calculates FPS by taking the inverse of the elapsed seconds.
    /// If elapsed time is zero or negative, it returns 0 to avoid division by zero.
    /// </remarks>
    /// <value>
    /// The current frames per second as an integer value.
    /// Returns 0 if no time has elapsed between frames.
    /// </value>
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
    /// <summary>
    /// Converts seconds to minutes.
    /// </summary>
    /// <param name="seconds">The time value in seconds to convert.</param>
    /// <returns>The equivalent time value in minutes.</returns>
    public static double ToMinutes(double seconds) => seconds / 60;

    /// <summary>
    /// Converts seconds to hours.
    /// </summary>
    /// <param name="seconds">The time value in seconds to convert.</param>
    /// <returns>The equivalent time value in hours.</returns>
    public static double ToHours(double seconds) => seconds / 3600;

    /// <summary>
    /// Converts seconds to days.
    /// </summary>
    /// <param name="seconds">The time value in seconds to convert.</param>
    /// <returns>The equivalent time value in days.</returns>
    public static double ToDays(double seconds) => seconds / 86400;
    
    /// <summary>
    /// 1/1000 of a seconds (1000 (1 Thousand) milliseconds = 1 second)
    /// </summary>
    public static double ToMilliSeconds(double seconds) => seconds / 1000;
    /// <summary>
    /// 1/1000000 of a seconds (1.000.000 (1 Million) micro seconds = 1 second)
    /// </summary>
    public static double ToMicroSeconds(double seconds) => seconds / 1000000;
    /// <summary>
    /// 1/1000000000 of a seconds (1.000.000.000 (1 Billion) nanoseconds = 1 second)
    /// </summary>
    public static double ToNanoSeconds(double seconds) => seconds / 1000000000;
    #endregion
    
}