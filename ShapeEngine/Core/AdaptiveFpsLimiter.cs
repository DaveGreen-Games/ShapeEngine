using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core;



public sealed class AdaptiveFpsLimiter
{
    #region Settings
    public readonly struct Settings
    {
        public static readonly Settings Disabled = new();
        
        public static readonly Settings Default = new(
            true, 30, 120,
            2.0, 6, 0.5f,
            5, 30, 2,
            1f, 1.25f);
        
        public static readonly Settings NoAdditionalCooldown = new(
            true, 30, 120,
            2.0, 6, 0.5f,
            5, 30, 0,
            0f, 1.25f);
        
        public static Settings Simple(int minFps = 30, int maxFps = 120, int framerateReduction = 5, int criticalFramerateReduction = 30) => new(
            true, minFps, maxFps,
            2.0, 6, 0.5f,
            framerateReduction, criticalFramerateReduction, 2,
            1f, 1.25f);
        
        public static Settings Advanced(int minFps = 30, int maxFps = 120, double raiseFpsCooldownDuration = 2.0, int requiredConsecutiveChecks = 6, 
            float fasterFrameTimeAverageWeight = 0.5f, int framerateReduction = 5, int criticalFramerateReduction = 30,
            int requiredConsectiveExtraCooldownChecks = 2, float additionalCooldown = 1f, float millisecondTolerance = 1.25f) => new
            (
                true, minFps, maxFps,
                raiseFpsCooldownDuration, requiredConsecutiveChecks, fasterFrameTimeAverageWeight,
                framerateReduction, criticalFramerateReduction,
                requiredConsectiveExtraCooldownChecks, additionalCooldown,
                millisecondTolerance
            );
        
        public readonly bool Enabled;
        public readonly int MinFps;
        public readonly int MaxFps;
        public readonly double RaiseFpsCooldownDuration;//cooldown in seconds that prevents raising fps too quickly
        public readonly int RequiredConsecutiveChecks; // how many consecutive(slower/faster)  checks are required to trigger fps adjustment
        public readonly float FasterFrameTimeAverageWeight; //weight for averaging frame time when raising fps (0.0 = only average, 1.0 = only current)
        public readonly int FramerateReduction; //by how much to reduce fps on each adjustment
        public readonly int CriticalFramerateReduction; //by how much to reduce fps on each critical adjustment
        public readonly int RequiredConsecutiveExtraCooldownChecks; //how many consecutive slow down adjustments are required to add extra cooldown
        public readonly float RaiseFpsAdditionalCooldownDuration; //extra cooldown duration added on each consecutive slow down adjustment
        public readonly double Tolerance;//Frame Time tolerance in seconds, makes the limiter less sensitive to minor fluctuations
        
        
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
    public int TargetFps { get; private set; }
    
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
    
    public bool Enabled { get; private set; }
    public double RaiseFpsCooldownDuration { get; private set; }
    public int RequiredConsecutiveChecks { get; private set; }
    public float FasterFrameTimeAverageWeight { get; private set; }
    public int FramerateReductionStep { get; private set; }
    public int CriticalFramerateReductionStep { get; private set; }
    public int RequiredConsecutiveExtraCooldownChecks { get; private set; }
    public double FrameTimeTolerance { get; private set; }
    public float RaiseFpsAdditionalCooldownDuration { get; private set; }
    
    #endregion
    
    #region Private Members

    private ValueRangeInt limit;
    private double cooldownTimer;
    private double additionalCooldownDuration;
    
    private int consecutiveFasterChecks;
    private int consecutiveSlowerChecks;
    private int consecutiveSlowDowns;
    
    private double fasterFrameTimeAccumulator;
    #endregion
    
    #region Static Members
    public static readonly int MinFpsLimit = 1;
    public static readonly int MaxFpsLimit = int.MaxValue;
    #endregion
    
    #region Constructor
    
    public AdaptiveFpsLimiter(Settings settings)
    {
        int min = settings.MinFps;
        int max = settings.MaxFps;
        if(min < MinFpsLimit) min = MinFpsLimit;
        if(max < MaxFpsLimit) max = MaxFpsLimit;
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
    
    #region Public Functions
    
    public int Update(int targetFrameRate, double frameTime, double frameDelta)
    {
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

                if (critical)
                {
                    TargetFps = ShapeMath.MaxInt(Limit.Min, TargetFps - CriticalFramerateReductionStep);
                }
                else
                {
                    TargetFps = ShapeMath.MaxInt(Limit.Min, TargetFps - FramerateReductionStep);
                }
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

                int target = targetFrameRate > 0 ? targetFrameRate > Limit.Max ? Limit.Max : targetFrameRate : Limit.Max;
                
                if (TargetFps < target)
                {
                    double weightedAverageFasterFrameTime = ShapeMath.LerpDouble(averageFasterFrameTime, frameTime, FasterFrameTimeAverageWeight);
                    double newFrameTime = weightedAverageFasterFrameTime + FrameTimeTolerance;
                    int newFps = ShapeMath.MinInt(target, (int)Math.Round(1.0 / newFrameTime));
                    
                    TargetFps = newFps;
                    return newFps;
                }
            }
        }
        //steady -> hold fps
        return TargetFps;
    }

    public bool ChangeSettings(Settings newSettings)
    {
        int min = newSettings.MinFps;
        int max = newSettings.MaxFps;
        if(min < MinFpsLimit) min = MinFpsLimit;
        if(max < MaxFpsLimit) max = MaxFpsLimit;
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