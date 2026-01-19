using ShapeEngine.StaticLib;

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

    public readonly int MinFrameRate;//sets limits on adaptive fps limiter, fixed framerate (if enabled) and frame rate limit (if enabled)
    public readonly int MaxFrameRate;//sets limits on adaptive fps limiter, and max framerate when frame rate limit is disabled
    public readonly double MaxDeltaTime;//implemented
    public readonly int MaxSubsteps;

    public static readonly FramerateSettings Default = new
    (
        60,
        0,
        30,
        120,
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
        60,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Default,
        0.16,
        5
    );
    public static readonly FramerateSettings Unlimited = new
    (
        0,
        0,
        0,
        0,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Disabled,
        0.25,
        5
    );
    public static readonly FramerateSettings UnlimitedAdaptive = new
    (
        0,
        0,
        0,
        500,
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
        int maxFrameRate,
        int unfocusedFrameRateLimit,
        int idleFrameRateLimit, float idleTimeThreshold,
        AdaptiveFpsLimiter.Settings adaptiveFpsLimiterSettings,
        double maxDeltaTime = 0.25,
        int maxSubsteps = 5)
    {
        MinFrameRate = ShapeMath.MaxInt(minFrameRate, 0);
        MaxFrameRate = ShapeMath.MaxInt(maxFrameRate, 0);

        if (MinFrameRate > MaxFrameRate)//Swap if min is greater than max
        {
            (MinFrameRate, MaxFrameRate) = (MaxFrameRate, MinFrameRate);
        }
        
        if (idleTimeThreshold > 0 && idleFrameRateLimit > 0)
        {
            if(MinFrameRate > 0 && idleFrameRateLimit < MinFrameRate) idleTimeThreshold = MinFrameRate;
            if(MaxFrameRate > 0 && idleFrameRateLimit > MaxFrameRate) idleFrameRateLimit = MaxFrameRate;
            IdleTimeThreshold = idleTimeThreshold;
        }
        else
        {
            IdleFrameRateLimit = 0;
            IdleTimeThreshold = 0f;
        }
        
        if (unfocusedFrameRateLimit > 0)
        {
            if(MinFrameRate > 0 && unfocusedFrameRateLimit < MinFrameRate) unfocusedFrameRateLimit = MinFrameRate;
            if(MaxFrameRate > 0 && unfocusedFrameRateLimit > MaxFrameRate) unfocusedFrameRateLimit = MaxFrameRate;
            UnfocusedFrameRateLimit = unfocusedFrameRateLimit;
        }
        else
        {
            UnfocusedFrameRateLimit = 0;
        }

        if (frameRateLimit > 0)
        {
            if(MinFrameRate > 0 && frameRateLimit < MinFrameRate) frameRateLimit = MinFrameRate;
            if(MaxFrameRate > 0 && frameRateLimit > MaxFrameRate) frameRateLimit = MaxFrameRate;
            FrameRateLimit = frameRateLimit;
        }
        else
        {
            FrameRateLimit = MaxFrameRate > 0 ? MaxFrameRate : 0;
        }

        if (fixedFramerate > 0)
        {
            if(MinFrameRate > 0 && FixedFramerate < MinFrameRate) FixedFramerate = MinFrameRate;
            if(MaxFrameRate > 0 && FixedFramerate > MaxFrameRate) FixedFramerate = MaxFrameRate;
            FixedFramerate = fixedFramerate;
        }
        else
        {
            FixedFramerate = 0;
        }

        AdaptiveFpsLimiterSettings = adaptiveFpsLimiterSettings;
        MaxDeltaTime = maxDeltaTime;
        MaxSubsteps = maxSubsteps;
    }
}