using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core;

/// <summary>
/// Adaptive FPS limiter that adjusts the engine's target framerate at runtime based on measured frame times.
/// It reduces the target framerate when frames are consistently slower than the current target and raises it
/// when frames are consistently faster, honoring configured cooldowns, tolerances and limits.
/// </summary>
/// <remarks>
/// Construct with a <see cref="Settings"/> instance. The limiter clamps target values to <see cref="Limit"/> and
/// is not thread-safe — intended for use from the main/update thread.
/// </remarks>
public sealed class AdaptiveFpsLimiter
{
    #region Settings
    
    /// <summary>
    /// Configuration container for the <see cref="AdaptiveFpsLimiter"/>.
    /// Immutable (readonly) struct that holds all tunable parameters used to initialize and update the limiter.
    /// </summary>
    /// <remarks>
    /// Several static presets are provided (Disabled, Default, NoAdditionalCooldown, Simple, Advanced).
    /// Instances are intended to be created via those presets or via the internal/private constructors exposed
    /// by the type; fields are public and readonly to allow cheap value-type copying.
    /// </remarks>
    public readonly struct Settings
    {
        /// <summary>
        /// Preset representing a disabled adaptive FPS limiter.
        /// </summary>
        /// <remarks>
        /// Constructs a default <see cref="Settings"/> instance with the limiter disabled
        /// (Equivalent to <c>Enabled = false</c>). Use this preset when you want an explicit
        /// "limiter off" configuration.
        /// </remarks>
        public static readonly Settings Disabled = new();
        
        /// <summary>
        /// Default preset for the adaptive FPS limiter.
        /// Enables the limiter with balanced, general-purpose values suitable for typical desktop scenarios.
        /// </summary>
        public static readonly Settings Default = new(
            true, 30, 120,
            2.0, 6, 0.5f,
            5, 30, 2,
            1f, 1.25f);
        
        /// <summary>
        /// Preset that enables the limiter but disables the additional raise-FPS cooldown.
        /// Sets RaiseFpsAdditionalCooldownDuration to 0 so repeated slowdowns do not
        /// increase the cooldown applied before raising FPS again.
        /// </summary>
        public static readonly Settings NoAdditionalCooldown = new(
            true, 30, 120,
            2.0, 6, 0.5f,
            5, 30, 0,
            0f, 1.25f);
        
        /// <summary>
        /// Creates a simple preset <see cref="Settings"/> with common, balanced defaults.
        /// Enables the adaptive limiter and sets the minimum/maximum FPS and reduction steps
        /// used when the limiter detects normal or critical slowdowns.
        /// </summary>
        /// <param name="minFps">Minimum allowed FPS. Default is 30.</param>
        /// <param name="maxFps">Maximum allowed FPS. Default is 120.</param>
        /// <param name="framerateReduction">FPS reduction applied on a normal slowdown. Default is 5.</param>
        /// <param name="criticalFramerateReduction">FPS reduction applied on a critical slowdown. Default is 30.</param>
        /// <returns>A new <see cref="Settings"/> instance configured with the provided values.</returns>
        public static Settings Simple(int minFps = 30, int maxFps = 120, int framerateReduction = 5, int criticalFramerateReduction = 30) => new(
            true, minFps, maxFps,
            2.0, 6, 0.5f,
            framerateReduction, criticalFramerateReduction, 2,
            1f, 1.25f);
        
        /// <summary>
        /// Creates an advanced preset <see cref="Settings"/> with fully configurable parameters for cooldowns,
        /// averaging, reduction steps and tolerances. This overload exposes every tunable value for fine-grained
        /// control of the adaptive FPS limiter's behavior.
        /// </summary>
        /// <param name="minFps">Minimum allowed FPS (default 30).</param>
        /// <param name="maxFps">Maximum allowed FPS (default 120).</param>
        /// <param name="raiseFpsCooldownDuration">Base raise-FPS cooldown duration in seconds (default 2.0).</param>
        /// <param name="requiredConsecutiveChecks">Number of consecutive faster/slower checks required to trigger an adjustment (default 6).</param>
        /// <param name="fasterFrameTimeAverageWeight">
        /// Weight used when averaging recent faster frame times (range 0.0..1.0). 0.0 = use only running average; 1.0 = use only most recent frame time (default 0.5f).
        /// </param>
        /// <param name="framerateReduction">FPS reduction applied on a normal slowdown (default 5).</param>
        /// <param name="criticalFramerateReduction">FPS reduction applied on a critical slowdown (default 30).</param>
        /// <param name="requiredConsecutiveExtraCooldownChecks">
        /// Number of consecutive slow-down adjustments required to add extra raise-FPS cooldown (default 2).
        /// </param>
        /// <param name="additionalCooldown">Additional cooldown duration in seconds added per consecutive slowdowns (default 1f).</param>
        /// <param name="millisecondTolerance">Frame time tolerance in milliseconds (default 1.25f). This is converted to seconds internally.</param>
        /// <returns>A new <see cref="Settings"/> instance configured with the provided advanced values.</returns>
        public static Settings Advanced(int minFps = 30, int maxFps = 120, double raiseFpsCooldownDuration = 2.0, int requiredConsecutiveChecks = 6, 
            float fasterFrameTimeAverageWeight = 0.5f, int framerateReduction = 5, int criticalFramerateReduction = 30,
            int requiredConsecutiveExtraCooldownChecks = 2, float additionalCooldown = 1f, float millisecondTolerance = 1.25f) => new
            (
                true, minFps, maxFps,
                raiseFpsCooldownDuration, requiredConsecutiveChecks, fasterFrameTimeAverageWeight,
                framerateReduction, criticalFramerateReduction,
                requiredConsecutiveExtraCooldownChecks, additionalCooldown,
                millisecondTolerance
            );
        
        /// <summary>
        /// Whether the adaptive limiter is enabled. When false the limiter is inactive and will not adjust FPS.
        /// </summary>
        public readonly bool Enabled;
        /// <summary>
        /// Minimum allowed FPS configured for the limiter. Values are clamped to <see cref="MinFpsLimit"/> by the constructor.
        /// </summary>
        public readonly int MinFps;
        /// <summary>
        /// Maximum allowed FPS configured for the limiter. Values are clamped to <see cref="MaxFpsLimit"/> by the constructor.
        /// </summary>
        public readonly int MaxFps;
        /// <summary>
        /// Base cooldown duration (in seconds) that prevents the limiter from raising FPS too quickly after a slowdown.
        /// </summary>
        public readonly double RaiseFpsCooldownDuration; // cooldown in seconds that prevents raising fps too quickly
        /// <summary>
        /// Number of consecutive faster/slower checks required before the limiter performs an FPS adjustment.
        /// </summary>
        public readonly int RequiredConsecutiveChecks; // how many consecutive(slower/faster) checks are required to trigger fps adjustment
        /// <summary>
        /// Weight used when averaging recent faster frame times while deciding to raise FPS.
        /// Range: 0.0 = use running average only, 1.0 = use only the most recent frame time.
        /// </summary>
        public readonly float FasterFrameTimeAverageWeight; // weight for averaging frame time when raising fps (0.0 = only average, 1.0 = only current)
        /// <summary>
        /// FPS reduction applied on a normal slowdown adjustment.
        /// </summary>
        public readonly int FramerateReduction; // by how much to reduce fps on each adjustment
        /// <summary>
        /// FPS reduction applied on a critical slowdown adjustment (larger step than <see cref="FramerateReduction"/>).
        /// </summary>
        public readonly int CriticalFramerateReduction; // by how much to reduce fps on each critical adjustment
        /// <summary>
        /// Number of consecutive slow-down adjustments required to increase the additional raise-FPS cooldown.
        /// </summary>
        public readonly int RequiredConsecutiveExtraCooldownChecks; // how many consecutive slow down adjustments are required to add extra cooldown
        /// <summary>
        /// Additional cooldown duration (in seconds) added for each consecutive slow-down adjustment.
        /// </summary>
        public readonly float RaiseFpsAdditionalCooldownDuration; // extra cooldown duration added on each consecutive slow down adjustment
        /// <summary>
        /// Frame time tolerance in seconds. Small fluctuations within this tolerance are ignored to reduce oscillation.
        /// This value is specified in milliseconds in the constructor and static factory methods
        /// and internally converted to seconds. (tolerance in milliseconds / 1000.0 = <see cref="Tolerance"/>)
        /// </summary>
        public readonly double Tolerance;
        
        /// <summary>
        /// Initializes a default <see cref="Settings"/> instance.
        /// Produces a disabled configuration with commonly used defaults:
        /// Enabled = false, MinFps = 30, MaxFps = 120, RaiseFpsCooldownDuration = 2.0,
        /// RequiredConsecutiveChecks = 6, FasterFrameTimeAverageWeight = 0.5f,
        /// FramerateReduction = 5, CriticalFramerateReduction = 30,
        /// RequiredConsecutiveExtraCooldownChecks = 2, RaiseFpsAdditionalCooldownDuration = 1f,
        /// Tolerance = 1.25 ms (converted to seconds).
        /// </summary>
        public Settings()
        {
            Enabled = false;
            MinFps = 30;
            MaxFps = 120;
            RaiseFpsCooldownDuration = 2.0;
            RequiredConsecutiveChecks = 6;
            FasterFrameTimeAverageWeight = 0.5f;
            FramerateReduction = 5;
            CriticalFramerateReduction = 30;
            RequiredConsecutiveExtraCooldownChecks = 2;
            RaiseFpsAdditionalCooldownDuration = 1f;
            Tolerance = 1.25 / 1000.0;
        }
        private Settings(bool enabled, int minFps, int maxFps,
            double raiseFpsCooldownDuration, int requiredConsecutiveChecks,float fasterFrameTimeAverageWeight, 
            int framerateReduction, int criticalFramerateReduction,
            int requiredConsecutiveExtraCooldownChecks, float raiseFpsAdditionalCooldownDuration, 
            float millisecondTolerance)
        {
            Enabled = enabled;
            MinFps = minFps;
            MaxFps = maxFps;
            RaiseFpsCooldownDuration = raiseFpsCooldownDuration;
            RequiredConsecutiveChecks = requiredConsecutiveChecks;
            FasterFrameTimeAverageWeight = fasterFrameTimeAverageWeight;
            FramerateReduction = framerateReduction;
            CriticalFramerateReduction = criticalFramerateReduction;
            RequiredConsecutiveExtraCooldownChecks = requiredConsecutiveExtraCooldownChecks;
            RaiseFpsAdditionalCooldownDuration = raiseFpsAdditionalCooldownDuration;
            Tolerance = millisecondTolerance / 1000.0;
        }
    }
    #endregion
    
    #region Public Members
    
    /// <summary>
    /// Current target frames-per-second that the engine should aim for.
    /// </summary>
    /// <remarks>
    /// Getter is public; setter is private. The limiter updates this value (for example in <see cref="Update"/> and <see cref="ChangeSettings"/>)
    /// and it is clamped to the configured <see cref="Limit"/>. Not thread-safe — intended to be used from the main/update thread.
    /// </remarks>
    public int TargetFps { get; private set; }
    
    /// <summary>
    /// Gets the allowed FPS range as a <see cref="ValueRangeInt"/>.
    /// </summary>
    /// <remarks>
    /// Assigning a new range updates the internal limit and immediately clamps <see cref="TargetFps"/>
    /// into the new bounds. The property's setter is private to ensure updates go through the limiter's logic.
    /// </remarks>
    public ValueRangeInt Limit
    {
        get => limit;
        private set
        {
            limit = value;
            if (TargetFps < limit.Min) TargetFps = limit.Min;
            else if (TargetFps > limit.Max) TargetFps = limit.Max;
        } 
    }
    
    /// <summary>
    /// Whether the adaptive FPS limiter is enabled. When false the limiter is inactive and will not modify the target FPS.
    /// </summary>
    public bool Enabled { get; private set; }

    /// <summary>
    /// Base cooldown duration in seconds that prevents the limiter from raising the target FPS too soon after a slowdown.
    /// </summary>
    public double RaiseFpsCooldownDuration { get; private set; }

    /// <summary>
    /// Number of consecutive checks (faster or slower) required before the limiter performs an adjustment.
    /// </summary>
    public int RequiredConsecutiveChecks { get; private set; }

    /// <summary>
    /// Weight applied when averaging recent faster frame times while deciding to raise FPS.
    /// Value is in range [0,1]: 0 = only use the running average, 1 = only use the most recent frame time.
    /// </summary>
    public float FasterFrameTimeAverageWeight { get; private set; }

    /// <summary>
    /// Amount (in FPS) to reduce the target framerate on a normal slowdown adjustment.
    /// </summary>
    public int FramerateReductionStep { get; private set; }

    /// <summary>
    /// Amount (in FPS) to reduce the target framerate on a critical slowdown adjustment.
    /// </summary>
    public int CriticalFramerateReductionStep { get; private set; }

    /// <summary>
    /// Number of consecutive slow-down adjustments required to increase the additional raise-FPS cooldown.
    /// </summary>
    public int RequiredConsecutiveExtraCooldownChecks { get; private set; }

    /// <summary>
    /// Allowed frame time tolerance in seconds. Small fluctuations within this tolerance are ignored to avoid oscillation.
    /// </summary>
    public double FrameTimeTolerance { get; private set; }

    /// <summary>
    /// Additional cooldown duration in seconds added to the raise-FPS cooldown after repeated slowdowns.
    /// </summary>
    /// <remarks>
    /// Total raise-FPS cooldown is calculated as <see cref="RaiseFpsCooldownDuration"/> + this value and is capped by <see cref="MaxRaiseFpsCooldownDuration"/>.
    /// </remarks>
    public float RaiseFpsAdditionalCooldownDuration { get; private set; }

    /// <summary>
    /// Multiplier applied to <see cref="RaiseFpsCooldownDuration"/> to compute the maximum allowed raise-FPS cooldown.
    /// Values below 1.0 are clamped to 1.0 by the property's setter.
    /// </summary>
    public double MaxFpsCooldownDurationFactor
    {
        get => maxFpsCooldownDurationFactor;
        set => maxFpsCooldownDurationFactor = value < 1.0 ? 1.0 : value;
    }
    
    /// <summary>
    /// Maximum allowed raise-FPS cooldown duration in seconds.
    /// Computed as <see cref="RaiseFpsCooldownDuration"/> multiplied by
    /// <see cref="MaxFpsCooldownDurationFactor"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="MaxFpsCooldownDurationFactor"/> is clamped to a minimum of 1.0,
    /// so this value will never be less than <see cref="RaiseFpsCooldownDuration"/>.
    /// </remarks>
    public double MaxRaiseFpsCooldownDuration => RaiseFpsCooldownDuration * MaxFpsCooldownDurationFactor;
    #endregion
    
    #region Private Members

    /// <summary>
    /// The currently allowed FPS range. Backing field for the public <see cref="Limit"/> property.
    /// </summary>
    private ValueRangeInt limit;

    /// <summary>
    /// Remaining cooldown time in seconds before the limiter is allowed to raise the target FPS again.
    /// Decremented each update by the passed frame delta.
    /// </summary>
    private double cooldownTimer;

    /// <summary>
    /// Additional cooldown duration (in seconds) accumulated from repeated slowdowns.
    /// Added to <see cref="RaiseFpsCooldownDuration"/> when starting a cooldown.
    /// </summary>
    private double additionalCooldownDuration;

    /// <summary>
    /// Number of consecutive checks where the measured frame time was sufficiently faster than the current target.
    /// Used to decide when to raise the target FPS.
    /// </summary>
    private int consecutiveFasterChecks;

    /// <summary>
    /// Number of consecutive checks where the measured frame time was sufficiently slower than the current target.
    /// Used to decide when to reduce the target FPS.
    /// </summary>
    private int consecutiveSlowerChecks;

    /// <summary>
    /// Count of consecutive slow-down adjustments that were applied. Used to increase the additional raise-FPS cooldown
    /// after repeated slowdowns to avoid oscillation.
    /// </summary>
    private int consecutiveSlowDowns;

    /// <summary>
    /// Accumulator of recent faster frame times. Used to compute a weighted average when deciding to raise FPS.
    /// </summary>
    private double fasterFrameTimeAccumulator;

    /// <summary>
    /// Multiplier applied to <see cref="RaiseFpsCooldownDuration"/> to compute the maximum allowed raise-FPS cooldown.
    /// Defaults to 4.0. Values below 1.0 are clamped by the property setter.
    /// </summary>
    private double maxFpsCooldownDurationFactor = 4.0;
    #endregion
    
    #region Static Members
    /// <summary>
    /// Minimum allowed FPS limit used to clamp the limiter's configured minimum.
    /// Prevents configuring non-sensical values (zero or negative FPS).
    /// </summary>
    public static readonly int MinFpsLimit = 1;

    /// <summary>
    /// Maximum allowed FPS limit used to clamp the limiter's configured maximum.
    /// Set to <see cref="int.MaxValue"/> to indicate effectively no practical upper bound.
    /// </summary>
    public static readonly int MaxFpsLimit = int.MaxValue;
    #endregion
    
    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveFpsLimiter"/> class using the provided <see cref="Settings"/>.
    /// </summary>
    /// <param name="settings">Configuration used to initialize enabled state, allowed FPS range, cooldowns,
    /// averaging weights and reduction steps.</param>
    /// <remarks>
    /// The constructor:
    /// - Clamps the configured minimum and maximum FPS to the static bounds (<see cref="MinFpsLimit"/> and <see cref="MaxFpsLimit"/>).
    /// - Swaps min/max when they are supplied in the wrong order.
    /// - Assigns <see cref="Limit"/>, which will also clamp <see cref="TargetFps"/> to the resulting range.
    /// - Applies settings to the limiter's public properties. The limiter is not thread-safe and is intended to be used
    ///   from the engine's main/update thread.
    /// </remarks>
    public AdaptiveFpsLimiter(Settings settings)
    {
        int min = settings.MinFps;
        int max = settings.MaxFps;
        if(min < MinFpsLimit) min = MinFpsLimit;
        if(max > MaxFpsLimit) max = MaxFpsLimit;
        if (min > max)
        {
            (min, max) = (max, min);
        }
        
        Limit = new ValueRangeInt(min, max); //TargetFps will be clamped to new limits in setter
        Enabled = settings.Enabled;
        RaiseFpsCooldownDuration = settings.RaiseFpsCooldownDuration;
        RequiredConsecutiveChecks = settings.RequiredConsecutiveChecks;
        FasterFrameTimeAverageWeight = settings.FasterFrameTimeAverageWeight;
        FramerateReductionStep = settings.FramerateReduction;
        CriticalFramerateReductionStep = settings.CriticalFramerateReduction;
        RequiredConsecutiveExtraCooldownChecks = settings.RequiredConsecutiveExtraCooldownChecks;
        RaiseFpsAdditionalCooldownDuration = settings.RaiseFpsAdditionalCooldownDuration;
        FrameTimeTolerance = settings.Tolerance;
    }
    #endregion
    
    
    /// <summary>
    /// Updates the limiter's internal state for a single frame and returns the adjusted target FPS.
    /// The method updates cooldown timers, evaluates the measured frame time against the current target,
    /// and may reduce or raise <see cref="TargetFps"/> according to configured thresholds and cooldowns.
    /// </summary>
    /// <param name="targetFrameRate">Requested/desired target framerate from the engine (used as an upper bound when raising).</param>
    /// <param name="frameTime">Measured frame time of the current frame in seconds.</param>
    /// <param name="frameDelta">Elapsed time since the last update in seconds (used to decrement cooldown timers).</param>
    /// <returns>The resulting target FPS after applying the adaptive limiter logic.</returns>
    internal int Update(int targetFrameRate, double frameTime, double frameDelta)//TODO: Add vsync mode parameter
    {
        
        //TODO: Now also adpats when certain vsync modes are active?
        
        //NOTE: All vsync modes that enabled adaptive fps limiter will only use descrete steps for limiting.
        // - 120hz monitor with adaptive mode will try to reach 120 fps, if not possible will try 60 fps, then 30 fps, etc.
        // - 60hz monitor with 2x adaptive mode will try to reach 120 fps, if not possible will try 60 fps, then 30 fps, etc.
        // - The allowed steps will be 30, 60, 120, 240 (for standard hz monitors).
        // - 30 will always be the minimum for adpative vsnc modes.
        
        //Q: Does it make sense to limit to fps limit with adaptive vsync modes?
        //NOTE: Should still adhere to limit -> if adaptive quadruple on a 120hz monitor is bigger than max fps limit, find the lowest one allowed
        // - The same goes for the minimum fps limit
        
        
        if(!Enabled)
        {
            TargetFps = targetFrameRate;
            return targetFrameRate;
        }
        
        if(cooldownTimer > 0.0)
        {
            cooldownTimer -= frameDelta;
            if(cooldownTimer < 0.0) cooldownTimer = 0.0;
        }
        
        
        if(TargetFps <= 0) TargetFps = Limit.Min;
        double targetFrameTime = 1.0 / TargetFps;
        
        if(targetFrameTime <= frameTime + FrameTimeTolerance)//reduce fps
        {
            bool critical = targetFrameTime <= frameTime;
            consecutiveSlowerChecks += critical ? 2 : 1;
            consecutiveFasterChecks = 0;
            fasterFrameTimeAccumulator = 0.0;
            if (consecutiveSlowerChecks >= RequiredConsecutiveChecks)
            {
                consecutiveSlowerChecks = 0;
                
                consecutiveSlowDowns += 1;
                if (consecutiveSlowDowns >= RequiredConsecutiveExtraCooldownChecks)
                {
                    consecutiveSlowDowns = 0;
                    additionalCooldownDuration += RaiseFpsAdditionalCooldownDuration; //increase additional cooldown duration by 1 second
                }
                
                StartCooldown();
                
                TargetFps = ShapeMath.MaxInt(Limit.Min, TargetFps - (critical ? CriticalFramerateReductionStep : FramerateReductionStep));
                
            }
            
            return TargetFps;
        }
        
        //raise fps
        if (targetFrameTime > frameTime + FrameTimeTolerance * 2)
        {
            consecutiveFasterChecks += 1;
            consecutiveSlowerChecks = 0;
            fasterFrameTimeAccumulator += frameTime;
            if (!IsCooldownActive &&  consecutiveFasterChecks >= RequiredConsecutiveChecks)
            {
                double averageFasterFrameTime = fasterFrameTimeAccumulator / consecutiveFasterChecks;
                
                fasterFrameTimeAccumulator = 0.0;
                consecutiveFasterChecks = 0;
                consecutiveSlowDowns = 0;
                
                additionalCooldownDuration -= RaiseFpsAdditionalCooldownDuration; //decrease additional cooldown duration by 1 second
                if (additionalCooldownDuration < 0.0) additionalCooldownDuration = 0.0;

                int target = targetFrameRate > 0 ? ShapeMath.MinInt(targetFrameRate, Limit.Max) : Limit.Max;
                
                if (TargetFps < target)
                {
                    double newFrameTime = ShapeMath.LerpDouble(averageFasterFrameTime, frameTime, FasterFrameTimeAverageWeight) + FrameTimeTolerance;
                    int newFps = ShapeMath.MinInt(target, (int)Math.Round(1.0 / newFrameTime));
                    
                    TargetFps = newFps;
                    return newFps;
                }
            }
        }
        //steady -> hold fps
        return TargetFps;
    }
    
    
    #region Public Functions
    
    /// <summary>
    /// Applies the provided <see cref="Settings"/> to the limiter.
    /// Updates limits, enabled state and all related configuration properties,
    /// clamps the current target FPS to the new limit and resets internal runtime state.
    /// </summary>
    /// <param name="newSettings">The new settings to apply.</param>
    /// <returns>True if the settings were applied successfully.</returns>
    public bool ChangeSettings(Settings newSettings)
    {
        int min = newSettings.MinFps;
        int max = newSettings.MaxFps;
        if(min < MinFpsLimit) min = MinFpsLimit;
        if(max > MaxFpsLimit) max = MaxFpsLimit;
        if (min > max)
        {
            (min, max) = (max, min);
        }
        
        Limit = new ValueRangeInt(min, max); //TargetFps will be clamped to new limits in setter
        
        Enabled = newSettings.Enabled;
        RaiseFpsCooldownDuration = newSettings.RaiseFpsCooldownDuration;
        RequiredConsecutiveChecks = newSettings.RequiredConsecutiveChecks;
        FasterFrameTimeAverageWeight = newSettings.FasterFrameTimeAverageWeight;
        FramerateReductionStep = newSettings.FramerateReduction;
        CriticalFramerateReductionStep = newSettings.CriticalFramerateReduction;
        RequiredConsecutiveExtraCooldownChecks = newSettings.RequiredConsecutiveExtraCooldownChecks;
        RaiseFpsAdditionalCooldownDuration = newSettings.RaiseFpsAdditionalCooldownDuration;
        FrameTimeTolerance = newSettings.Tolerance;

        ResetState();
        
        return true;
    }
    
    /// <summary>
    /// Enables or disables the adaptive FPS limiter.
    /// </summary>
    /// <param name="enabled">
    /// True to enable the limiter; false to disable. When disabling, the limiter's internal runtime state is reset.
    /// Calling with the current value is a no-op.
    /// </param>
    /// <remarks>
    /// This method is intended to be called from the engine's main/update thread and is not thread-safe.
    /// </remarks>
    public void SetEnabled(bool enabled)
    {
        if(Enabled == enabled) return;
        Enabled = enabled;
        if (!Enabled)
        {
            ResetState();
        }
    }
    #endregion
    
    #region Helper Functions
    private bool IsCooldownActive => cooldownTimer > 0.0;
    private void StartCooldown()
    {
        cooldownTimer = RaiseFpsCooldownDuration + additionalCooldownDuration;
        
        //limits the maximum cooldown duration to prevent excessively long cooldowns (e.g. due to many consecutive slow downs)
        if(cooldownTimer > MaxRaiseFpsCooldownDuration) cooldownTimer = MaxRaiseFpsCooldownDuration;
    }
    private void StopCooldown()
    {
        cooldownTimer = 0.0;
    }
    private void ResetState()
    {
        consecutiveFasterChecks = 0;
        consecutiveSlowerChecks = 0;
        consecutiveSlowDowns = 0;
        fasterFrameTimeAccumulator = 0.0;
        additionalCooldownDuration = 0.0;
        StopCooldown();
    }
    #endregion
}