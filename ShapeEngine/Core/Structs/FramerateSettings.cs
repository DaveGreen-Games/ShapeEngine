namespace ShapeEngine.Core.Structs;

public readonly struct FramerateSettings
{
    /// <summary>
    /// The target frame rate limit for the application. 0 or less means unlimited frame rate.
    /// </summary>
    public readonly int FrameRateLimit;
    
    /// <summary>
    /// The target frame rate limit applied when the application window is unfocused.
    /// Set to 0 or less to disable the unfocused-specific limit (no change from the normal limit).
    /// </summary>
    /// <remarks>
    /// Use this to reduce CPU/GPU usage while the window does not have focus.
    /// </remarks>
    public readonly int UnfocusedFrameRateLimit;
    
    /// <summary>
    /// Settings for the adaptive FPS limiter which dynamically adjusts the frame cap.
    /// Contains minimum and maximum target frame rates and whether adaptive limiting is enabled.
    /// </summary>
    public readonly AdaptiveFpsLimiter.Settings AdaptiveFpsLimiterSettings;
    
    /// <summary>
    /// The target frame rate limit applied when the application is idle (no input detected for <see cref="IdleTimeThreshold"/> seconds).
    /// Set to 0 or less to disable the idle-specific limit (no change from the normal limit).
    /// </summary>
    /// <remarks>
    /// Use this to reduce CPU/GPU usage while the window does not have focus.
    /// </remarks>
    public readonly int IdleFrameRateLimit;
    
    /// <summary>
    /// Time in seconds without input after which the application is considered idle.
    /// When the idle period is reached the <see cref="IdleFrameRateLimit"/> may be applied.
    /// Set to 0 or less to disable idle detection.
    /// </summary>
    public readonly float IdleTimeThreshold;
    
    /// <summary>
    /// Gets the fixed framerate used for the fixed update loop.
    /// <list type="bullet">
    /// <item>The physics update uses a delta time of <c>1 / FixedFramerate</c>.</item>
    /// <item>If set to 0 or less, the fixed update loop is disabled and <c>FixedUpdate</c>/<c>InterpolateFixedUpdate</c> will not be called.</item>
    /// <item>Values greater than 0 but less than 30 are clamped to 30.</item>
    /// <item>When enabled, the physics update runs after the normal update function.</item>
    /// </list>
    /// </summary>
    public readonly int FixedFramerate;

    public readonly int MinFrameRate;
    public readonly double MaxDeltaTime;
    public readonly int MaxSubsteps;

    public static readonly FramerateSettings Default = new
    (
        60,
        0,
        30,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Default,
        0.25,
        5
    );
    public static readonly FramerateSettings DefaultFixed = new
    (
        60,
        60,
        30,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Default,
        0.16,
        5
    );
    public static readonly FramerateSettings Unlimited = new
    (
        60,
        0,
        30,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Default,
        0.25,
        5
    );
    public static readonly FramerateSettings UnlimitedAdaptive = new
    (
        60,
        0,
        30,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Default,
        0.25,
        5
    );
    public FramerateSettings(
        int frameRateLimit, 
        int fixedFramerate,
        int minFrameRate, 
        int unfocusedFrameRateLimit,
        int idleFrameRateLimit, float idleTimeThreshold,
        AdaptiveFpsLimiter.Settings adaptiveFpsLimiterSettings,
        double maxDeltaTime = 0.25,
        int maxSubsteps = 5)
    {
        FrameRateLimit = frameRateLimit;
        FixedFramerate = fixedFramerate;
        MinFrameRate = minFrameRate;
        UnfocusedFrameRateLimit = unfocusedFrameRateLimit;
        IdleFrameRateLimit = idleFrameRateLimit;
        IdleTimeThreshold = idleTimeThreshold;
        AdaptiveFpsLimiterSettings = adaptiveFpsLimiterSettings;
        MaxDeltaTime = maxDeltaTime;
        MaxSubsteps = maxSubsteps;
    }
}