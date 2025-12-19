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
        
        public static readonly Settings NoExtraCooldown = new(
            true, 30, 120,
            2.0, 6, 0.5f,
            5, 30, 0,
            0f, 1.25f);
        
        public static Settings Simple(int minFps = 30, int maxFps = 120, int framerateReduction = 5, int criticalFramerateReduction = 30) => new(
            true, minFps, maxFps,
            2.0, 6, 0.5f,
            framerateReduction, criticalFramerateReduction, 2,
            1f, 1.25f);
        
        public static Settings Advanced(int minFps = 30, int maxFps = 120, 
            double raiseFpsCooldownDuration = 2.0, int requiredConsecutiveChecks = 6, float fasterFrameTimeAverageWeight = 0.5f,
            int framerateReduction = 5, int criticalFramerateReduction = 30,
            int requiredConsectiveExtraCooldownChecks = 2, float additionalCooldown = 1f,
            float millisecondTolerance = 1.25f) => new(
            true, minFps, maxFps,
            raiseFpsCooldownDuration, requiredConsecutiveChecks, fasterFrameTimeAverageWeight,
            framerateReduction, criticalFramerateReduction,
            requiredConsectiveExtraCooldownChecks, additionalCooldown,
            millisecondTolerance);
        
        public readonly bool Enabled;
        public readonly int MinFps;
        public readonly int MaxFps;
        public readonly double RaiseFpsCooldownDuration;//cooldown in seconds that prevents raising fps too quickly
        public readonly int RequiredConsecutiveChecks; // how many consecutive(slower/faster)  checks are required to trigger fps adjustment
        public readonly float FasterFrameTimeAverageWeight; //weight for averaging frame time when raising fps (0.0 = only average, 1.0 = only current)
        public readonly int FramerateReduction; //by how much to reduce fps on each adjustment
        public readonly int CriticalFramerateReduction; //by how much to reduce fps on each critical adjustment
        public readonly int RequiredConsectiveExtraCooldownChecks; //how many consecutive slow down adjustments are required to add extra cooldown
        public readonly float AdditionalCooldown; //extra cooldown duration added on each consecutive slow down adjustment
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
            RequiredConsectiveExtraCooldownChecks = 2;
            AdditionalCooldown = 1f;
            Tolerance = 1.25 / 1000.0;
        }
        private Settings(bool enabled, int minFps, int maxFps,
            double raiseFpsCooldownDuration, int requiredConsecutiveChecks,float fasterFrameTimeAverageWeight, 
            int framerateReduction, int criticalFramerateReduction,
            int requiredConsectiveExtraCooldownChecks, float additionalCooldown, 
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
            RequiredConsectiveExtraCooldownChecks = requiredConsectiveExtraCooldownChecks;
            AdditionalCooldown = additionalCooldown;
            Tolerance = millisecondTolerance / 1000.0;
        }
    }
    #endregion
    
    #region Public Members
    public ValueRangeInt Limit { get; private set; }
    public int TargetFps { get; private set; }
    public bool Enabled { get; set; }
    
    private double cooldownDuration = 2.0;
    private int requiredConsecutiveChecks = 6;
    private float fasterFrameTimeAverageWeight = 0.5f;
    private int frameReductionStep = 5;
    private int criticalFrameReductionStep = 30;
    private int requiredConsectiveSlowDownsForExtraCooldown = 2;
    private double milliSecondTolerance = 1.25 / 1000.0; //1.25ms tolerance
    private float consecutiveSlowDownAdditionalCooldown = 1f;
    
    #endregion
    
    #region Private Members
    private double cooldownTimer = 0.0;
    private double additionalCooldownDuration = 0.0;
    
    private int consecutiveFasterChecks = 0;
    private int consecutiveSlowerChecks = 0;
    private int consectiveSlowDowns = 0;
    
    private double fasterFrameTimeAccumulator = 0.0;
    #endregion
    
    #region Constructor
    
    public AdaptiveFpsLimiter(Settings settings)
    {
        Limit = new ValueRangeInt(settings.MinFps, settings.MaxFps);
        TargetFps = Limit.Min;
        Enabled = settings.Enabled;
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
        
        double targetFrameTime = 1.0 / TargetFps;
        
        if(targetFrameTime <= frameTime + milliSecondTolerance)//reduce fps
        {
            bool critical = targetFrameTime <= frameTime;
            consecutiveSlowerChecks += critical ? 2 : 1;
            consecutiveFasterChecks = 0;
            fasterFrameTimeAccumulator = 0.0;
            if (consecutiveSlowerChecks >= requiredConsecutiveChecks)
            {
                consecutiveSlowerChecks = 0;
                
                consectiveSlowDowns += 1;
                if (consectiveSlowDowns >= requiredConsectiveSlowDownsForExtraCooldown)
                {
                    consectiveSlowDowns = 0;
                    additionalCooldownDuration += consecutiveSlowDownAdditionalCooldown; //increase additional cooldown duration by 1 second
                }
                
                StartCooldown();

                if (critical)
                {
                    TargetFps = ShapeMath.MaxInt(Limit.Min, TargetFps - criticalFrameReductionStep);
                }
                else
                {
                    TargetFps = ShapeMath.MaxInt(Limit.Min, TargetFps - frameReductionStep);
                }
            }
            
            return TargetFps;
        }
        
        //raise fps
        if (targetFrameTime > frameTime + milliSecondTolerance * 2)
        {
            consecutiveFasterChecks += 1;
            consecutiveSlowerChecks = 0;
            fasterFrameTimeAccumulator += frameTime;
            if (!IsCooldownActive &&  consecutiveFasterChecks >= requiredConsecutiveChecks)
            {
                double averageFasterFrameTime = fasterFrameTimeAccumulator / consecutiveFasterChecks;
                
                fasterFrameTimeAccumulator = 0.0;
                consecutiveFasterChecks = 0;
                consectiveSlowDowns = 0;
                
                additionalCooldownDuration -= consecutiveSlowDownAdditionalCooldown; //decrease additional cooldown duration by 1 second
                if (additionalCooldownDuration < 0.0) additionalCooldownDuration = 0.0;

                int target = targetFrameRate > 0 ? targetFrameRate : Limit.Max;
                
                if (TargetFps < target)
                {
                    double weightedAverageFasterFrameTime = ShapeMath.LerpDouble(averageFasterFrameTime, frameTime, fasterFrameTimeAverageWeight);
                    double newFrameTime = weightedAverageFasterFrameTime + milliSecondTolerance;
                    int newFps = ShapeMath.MinInt(target, (int)Math.Round(1.0 / newFrameTime));
                    TargetFps = newFps;
                    return newFps;
                }
            }
        }
        //steady -> hold fps
        return TargetFps;
    }
    #endregion
    
    #region Helper Functions
    private bool IsCooldownActive => cooldownTimer > 0.0;
    private void StartCooldown()
    {
        cooldownTimer = cooldownDuration + additionalCooldownDuration;
    }
    private void StopCooldown()
    {
        cooldownTimer = 0.0;
    }
    #endregion
}