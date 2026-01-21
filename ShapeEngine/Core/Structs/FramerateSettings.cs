using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents immutable framerate configuration used by the engine.
/// Contains global and context-specific frame rate limits (focused, unfocused, idle),
/// fixed-update framerate, adaptive FPS limiter settings, delta-time constraints and
/// dynamic substepping parameters. Designed as a value type for inexpensive copies.
/// </summary>
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
    /// Fixed update framerate (in frames per second) used for a fixed-timestep update loop.
    /// A value of 0 disables fixed-timestep updates. When enabled the constructor will
    /// clamp this value to <see cref="MinFrameRate"/> / <see cref="MaxFrameRate"/> if those bounds are active.
    /// </summary>
    /// <remarks>
    /// A fixed frame rate guarantees consistent update intervals regardless of rendering performance.
    /// The fixed framerate is not affected by <see cref="IdleFrameRateLimit"/> or <see cref="UnfocusedFrameRateLimit"/>.
    /// </remarks>
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
    /// Minimum allowed target framerate (in frames per second) used when dynamic substepping is active.
    /// This value constrains how low the effective framerate may go when splitting a large frame delta
    /// into multiple substeps. Clamped to be non-negative in the constructor. Set to 0 to disable the lower bound.
    /// </summary>
    /// <remarks>
    /// When dynamic substepping is enabled, this value effectively guarantees the maximum delta the update loop can receive.
    /// Even when the delta time explodes to very high values, the worst case scenario is 1 substep per frame with a delta of
    /// 1 / MinDynamicSubsteppingFramerate seconds. 
    /// </remarks>
    public readonly int MinDynamicSubsteppingFramerate;

    /// <summary>
    /// Maximum number of dynamic substeps allowed when dynamic substepping is active.
    /// Limits how many smaller update steps the engine may perform in a single frame to smooth out large frame deltas.
    /// Clamped to be non-negative in the constructor; a value of 0 effectively disables dynamic substepping.
    /// </summary>
    /// <remarks>
    /// The engine will reduce the allowed substeps every consecutive frame where substepping was performed to avoid spiraling.
    /// Frames without substepping increase the allowed substeps back up to this maximum.
    /// </remarks>
    public readonly int MaxDynamicSubsteps;

    
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
    /// - Dynamic substepping enabled with a minimum framerate of 30 and a maximum of 6 substeps.
    /// </summary>
    /// <remarks>
    /// Immutable convenience instance intended for initialization when no user configuration is provided.
    /// </remarks>
    public static FramerateSettings Default => new
    (
        60,
        0,
        30,
        120,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Default,
        0.2,
        30,
        6
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
    public static FramerateSettings DefaultFixed => new
    (
        60,
        60,
        30,
        60,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Disabled,
        0.16,
        0,
        0
    );
    
    /// <summary>
    /// Predefined framerate settings representing an "unlimited" configuration.
    /// - No explicit global frame rate cap (FrameRateLimit = 0 -> unlimited).
    /// - Fixed framerate disabled (FixedFramerate = 0).
    /// - No minimum or maximum bounds applied (MinFrameRate = 0, MaxFrameRate = 0).
    /// - Unfocused (30Fps) and idle (30Fps/120s) limits remain enabled to reduce background CPU/GPU usage.
    /// - Adaptive FPS limiter disabled.
    /// - Max delta time of 0.25 seconds.
    /// - Dynamic substepping enabled with a minimum framerate of 30 and a maximum of 6 substeps.
    /// </summary>
    public static FramerateSettings Unlimited => new
    (
        0,
        0,
        0,
        0,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Disabled,
        0.2,
        30,
        6
    );
    
    /// <summary>
    /// Predefined framerate settings for an "unlimited adaptive" configuration:
    /// - No explicit global frame rate cap (FrameRateLimit = 0 -> unlimited).
    /// - Fixed framerate disabled (FixedFramerate = 0).
    /// - No minimum bound (MinFrameRate = 0) and a high MaxFrameRate to allow adaptive behavior.
    /// - Unfocused (30Fps) and idle (30Fps/120s) limits remain enabled to reduce background CPU/GPU usage.
    /// - Adaptive FPS limiter enabled with default settings.
    /// - Max delta time of 0.25 seconds
    /// - Dynamic substepping enabled with a minimum framerate of 30 and a maximum of 6 substeps.
    /// </summary>
    public static FramerateSettings UnlimitedAdaptive => new
    (
        0,
        0,
        0,
        500,
        30,
        30, 120f,
        AdaptiveFpsLimiter.Settings.Default,
        0.2,
        30,
        6
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
    /// <param name="minDynamicSubsteppingFramerate">Minimum allowed target framerate (in frames per second) used for dynamic substepping. </param>
    /// <param name="maxDynamicSubsteps">Maximum number of dynamic substeps allowed when dynamic substepping is active. Setting this to 0 disables dynamic substepping.</param>
    public FramerateSettings(
        int frameRateLimit, 
        int fixedFramerate,
        int minFrameRate, 
        int maxFrameRate,
        int unfocusedFrameRateLimit,
        int idleFrameRateLimit, float idleTimeThreshold,
        AdaptiveFpsLimiter.Settings adaptiveFpsLimiterSettings,
        double maxDeltaTime = 0.2,
        int minDynamicSubsteppingFramerate = 30,
        int maxDynamicSubsteps = 6
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
            if (MinFrameRate > 0 && fixedFramerate < MinFrameRate) fixedFramerate = MinFrameRate;
            if (MaxFrameRate > 0 && fixedFramerate > MaxFrameRate) fixedFramerate = MaxFrameRate;
            FixedFramerate = fixedFramerate;
        }
        else
        {
            FixedFramerate = 0;
        }

        AdaptiveFpsLimiterSettings = adaptiveFpsLimiterSettings;
        MaxDeltaTime = maxDeltaTime;
        
        if(maxDynamicSubsteps <= 0 || minDynamicSubsteppingFramerate <= 0)
        {
            MaxDynamicSubsteps = 0;
            MinDynamicSubsteppingFramerate = 0;
        }
        else
        {
            MinDynamicSubsteppingFramerate = minDynamicSubsteppingFramerate;
            MaxDynamicSubsteps = maxDynamicSubsteps;
        }
    }
}