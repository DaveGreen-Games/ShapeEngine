using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core;


public sealed class AdaptiveFpsLimiter
{
    public readonly struct Settings
    {
        public static readonly Settings Default = new(30, 120, true);
        public static readonly Settings Disabled = new(30, 120, false);
        
        public readonly bool Enabled;
        public readonly int MinFps;
        public readonly int MaxFps;

        public Settings()
        {
            Enabled = false;
            MinFps = 30;
            MaxFps = 120;
        }
        public Settings(int minFps, int maxFps, bool enabled)
        {
            Enabled = enabled;
            MinFps = minFps;
            MaxFps = maxFps;
        }
    }
    
    public ValueRangeInt Limit { get; private set; }
    public int TargetFps { get; private set; }
    public bool Enabled { get; set; }
    
    
    
    //TODO: add some of the variables to settings
    private double cooldownTimer = 0.0;
    private double cooldownDuration = 2.0;
    private double additionalCooldownDuration = 0.0;
    
    
    private int consecutiveFasterChecks = 0;
    private int consecutiveSlowerChecks = 0;
    private int requiredConsecutiveChecks = 6;

    private double fasterFrameTimeAccumulator = 0.0;
    private float fasterFrameTimeAverageWeight = 0.5f;
    
    private int frameReductionStep = 5;
    private int criticalFrameReductionStep = 30;


    private int consectiveSlowDowns = 0;
    private int requiredConsectiveSlowDownsForExtraCooldown = 2;
    private float consecutiveSlowDownAdditionalCooldown = 1f;
    
    private double milliSecondTolerance = 1.25 / 1000.0; //1.25ms tolerance
    
    
    
    public AdaptiveFpsLimiter(Settings settings)
    {
        Limit = new ValueRangeInt(settings.MinFps, settings.MaxFps);
        TargetFps = Limit.Min;
        Enabled = settings.Enabled;
    }
    
    
    
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
    
    
    
    private bool IsCooldownActive => cooldownTimer > 0.0;
    private void StartCooldown()
    {
        cooldownTimer = cooldownDuration + additionalCooldownDuration;
    }
    private void StopCooldown()
    {
        cooldownTimer = 0.0;
    }
}