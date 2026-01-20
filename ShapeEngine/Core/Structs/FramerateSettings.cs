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

    /// <summary>
    /// Minimum allowed frame rate used to constrain other framerate-related settings.
    /// - Applies as the lower bound for the adaptive FPS limiter.
    /// - Clamps the fixed framerate when enabled.
    /// - Clamps any explicit frame rate limit when enabled.
    /// Set to 0 to disable the lower bound.
    /// </summary>
    public readonly int MinFrameRate;

    /// <summary>
    /// Maximum allowed frame rate used to constrain other framerate-related settings.
    /// - Applies as the upper bound for the adaptive FPS limiter.
    /// - When no explicit frame rate limit is set, this value is used as the effective cap.
    /// Set to 0 to disable the upper bound.
    /// </summary>
    public readonly int MaxFrameRate;

    /// <summary>
    /// Maximum allowed delta time (in seconds) for a single frame update.
    /// The frame delta will be clamped to this value to avoid very large time steps,
    /// prevent a spiral of death when fixed framerate is used, and to limit how many
    /// substeps the engine may perform in a single frame.
    /// </summary>
    public readonly double MaxDeltaTime;

    /// <summary>
    /// Factor controlling dynamic substepping activation when fixed framerate is disabled.
    /// If the current frame delta exceeds expectedTimestep * (this factor + 1), dynamic
    /// substepping will be used to split the update into multiple smaller steps to smooth
    /// out large frame time spikes.
    /// Set to 0 to disable dynamic substepping. This does not apply when fixed framerate is enabled.
    /// Value is clamped to be non-negative.
    /// </summary>
    public readonly float DynamicSubsteppingThresholdFactor;

    
    /// <summary>
    /// Default framerate settings used by the engine.
    /// Provides sensible defaults for normal (non-fixed) operation:
    /// - Target frame rate limit of 60.
    /// - Fixed framerate disabled (0 = disabled).
    /// - Min frame rate of 30.
    /// - Max frame rate of 120.
    /// - Unfocused frame rate limit of 30.
    /// - Idle frame rate limit of 30 after 120 seconds of inactivity.
    /// - Adaptive FPS limiter enabled with default settings. <see cref="AdaptiveFpsLimiter.Settings.Default"/>
    /// - Max delta time of 0.25 seconds.
    /// - Dynamic substepping threshold factor of 4.0f.
    /// </summary>
    /// <remarks>
    /// Immutable convenience instance intended for initialization when no user configuration is provided.
    /// </remarks>
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
        4f
    );
    
    /// <summary>
    /// Predefined framerate settings configured for fixed-timestep operation.
    /// - Target frame rate limit: 60
    /// - Fixed framerate (enabled): 60
    /// - Min/Max frame rate bounds: 30 / 60
    /// - Unfocused and idle limits: 30 FPS (idle applied after 120s)
    /// - Adaptive FPS limiter: disabled
    /// - Max delta time: 0.16s
    /// - Dynamic substepping: disabled
    /// </summary>
    public static readonly FramerateSettings DefaultFixed = new
    (
        60,
        60,
        30,
        60,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Disabled,
        0.16,
        0f
    );
    
    /// <summary>
    /// Predefined framerate settings representing an "unlimited" configuration.
    /// - No explicit global frame rate cap (FrameRateLimit = 0 -> unlimited).
    /// - Fixed framerate disabled (FixedFramerate = 0).
    /// - No minimum or maximum bounds applied (MinFrameRate = 0, MaxFrameRate = 0).
    /// - Unfocused (30Fps) and idle (30Fps/120s) limits remain enabled to reduce background CPU/GPU usage.
    /// - Adaptive FPS limiter disabled.
    /// - Max delta time of 0.25 seconds.
    /// - Dynamic substepping disabled.
    /// </summary>
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
        0f
    );
    
    /// <summary>
    /// Predefined framerate settings for an "unlimited adaptive" configuration:
    /// - No explicit global frame rate cap (FrameRateLimit = 0 -> unlimited).
    /// - Fixed framerate disabled (FixedFramerate = 0).
    /// - No minimum bound (MinFrameRate = 0) and a high MaxFrameRate to allow adaptive behavior.
    /// - Unfocused (30Fps) and idle (30Fps/120s) limits remain enabled to reduce background CPU/GPU usage.
    /// - Adaptive FPS limiter enabled with default settings.
    /// - Max delta time of 0.25 seconds
    /// - Dynamic substepping disabled.
    /// </summary>
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
        0f
    );
    
    /// <summary>
    /// Initializes a new <see cref="FramerateSettings"/> instance with the specified values.
    /// Values are validated and clamped where appropriate.
    /// </summary>
    /// <param name="frameRateLimit">Global frame rate limit. 0 or less means unlimited;
    /// clamped to <paramref name="minFrameRate"/> / <paramref name="maxFrameRate"/> when those bounds are set.</param>
    /// <param name="fixedFramerate">Fixed update framerate. 0 or less disables fixed-timestep; otherwise clamped to bounds.</param>
    /// <param name="minFrameRate">Minimum allowed frame rate bound. Set to 0 to disable the lower bound.</param>
    /// <param name="maxFrameRate">Maximum allowed frame rate bound. Set to 0 to disable the upper bound.</param>
    /// <param name="unfocusedFrameRateLimit">Frame rate limit applied when the application is unfocused.
    /// 0 or less disables the unfocused-specific limit; clamped to bounds when set.</param>
    /// <param name="idleFrameRateLimit">Frame rate limit applied when the application is idle.
    /// 0 or less disables the idle-specific limit.</param>
    /// <param name="idleTimeThreshold">Seconds without input after which the application is considered idle.
    /// 0 or less disables idle detection.</param>
    /// <param name="adaptiveFpsLimiterSettings">Settings for the adaptive FPS limiter.</param>
    /// <param name="maxDeltaTime">Maximum allowed delta time (seconds) for a single frame. Defaults to 0.25.</param>
    /// <param name="dynamicSubsteppingThresholdFactor">Factor controlling dynamic substepping activation when fixed framerate is disabled.
    /// Clamped to non-negative; 0 disables dynamic substepping.</param>
    public FramerateSettings(
        int frameRateLimit, 
        int fixedFramerate,
        int minFrameRate, 
        int maxFrameRate,
        int unfocusedFrameRateLimit,
        int idleFrameRateLimit, float idleTimeThreshold,
        AdaptiveFpsLimiter.Settings adaptiveFpsLimiterSettings,
        double maxDeltaTime = 0.25,
        float dynamicSubsteppingThresholdFactor = 0.0f
        // int maxSubsteps = 5
        )
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
        DynamicSubsteppingThresholdFactor = MathF.Max(dynamicSubsteppingThresholdFactor, 0f);
        // MaxSubsteps = maxSubsteps;
    }
}