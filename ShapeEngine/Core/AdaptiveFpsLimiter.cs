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
    
    // private float limitingWeight = 0.5f;
    
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
    
    
    
    //NOTE: Uses raise cooldown timer
    // - when fps has been lowered, it cannot be raised again until the cooldown timer expires
    // - once the cooldown timer expires, fps can be raised again
    // - implement a system that increases the cooldown timer duration based on how often fps is lowered recently? (eg. exponential backoff) - successful raises reduce the cooldown timer duration back to normal
    
    //NOTE: should try to find a frame rate that uses up almost all the frame time budget (around 1ms left of frame time budget)
    // - if target frame rate <= 0 -> unlimited frame rate tries to reach max frame rate and never goes below min frame rate
    // - if target frame rate > 0 -> tries to reach target frame rate and never goes below min frame rate (and never above target frame rate)
    // - can be enabled or disabled
    
    //NOTE: Use the remaining frame time as target
    // - if there is no remaining time, lower the frame rate
    // - if there is more remaining time than tolerance (1ms for instance), try to raise the frame rate
    
    
    
    private bool IsCooldownActive => cooldownTimer > 0.0;
    private void StartCooldown()
    {
        cooldownTimer = cooldownDuration + additionalCooldownDuration;
    }
    private void StopCooldown()
    {
        cooldownTimer = 0.0;
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

        if (targetFrameRate > 0)
        {
            //TODO: Test with console write line if cooldowns and everything else works as intended
            
            if(targetFrameTime <= frameTime + milliSecondTolerance)//reduce fps
            {
                // bool critical = remainingFrameTime <= 0.0;
                bool critical = targetFrameTime <= frameTime;
                consecutiveSlowerChecks += critical ? 2 : 1;
                consecutiveFasterChecks = 0;
                
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
                if (!IsCooldownActive &&  consecutiveFasterChecks >= requiredConsecutiveChecks)
                {
                    consecutiveFasterChecks = 0;
                    consectiveSlowDowns = 0;
                    
                    additionalCooldownDuration -= consecutiveSlowDownAdditionalCooldown; //decrease additional cooldown duration by 1 second
                    if (additionalCooldownDuration < 0.0) additionalCooldownDuration = 0.0;

                    if (TargetFps < targetFrameRate)
                    {
                        double newFrameTime = frameTime + milliSecondTolerance;
                        int newFps = ShapeMath.MinInt(targetFrameRate, (int)Math.Round(1.0 / newFrameTime));
                        TargetFps = newFps;
                        return newFps;
                    }
                }
            }
            
            //steady -> hold fps
            return TargetFps;
        }
        else
        {
            //TODO: implement
            
            // if (remainingFrameTime > milliSecondTolerance * 2)//raise fps
            // {
            //     consecutiveFasterChecks += 1;
            //     consecutiveSlowerChecks = 0;
            //     if (!IsCooldownActive &&  consecutiveFasterChecks >= requiredConsecutiveChecks)
            //     {
            //         consecutiveFasterChecks = 0;
            //
            //         double newFrameTime = frameTime + milliSecondTolerance;
            //         int newFps = ShapeMath.MaxInt(Limit.Max, (int)Math.Round(1.0 / newFrameTime));
            //         TargetFps = newFps;
            //         return newFps;
            //     }
            //     TargetFps = targetFrameRate;
            //     return targetFrameRate;
            // }

        }
        
        TargetFps = targetFrameRate;
        return targetFrameRate;
    }
}





// double totalFrameTime = 1.0 / TargetFps;
// double remainingFrameTime = totalFrameTime - frameTime;
// if (remainingFrameTime <= 0.0 || remainingFrameTime < milliSecondTolerance)//reduce fps


/*public sealed class AdaptiveFpsLimiter
{
    // Config
    public double MinFps = 30;
    public double MaxFps = 240;
    public double UpStep = 5;
    public double DownStep = 10;
    public int AdjustInterval = 30;
    public double SmoothFactor = 0.1;
    public double UpperUtil = 1.05;
    public double LowerUtil = 0.85;
    public bool EnablePrediction = true;
    public double TrendWeight = 0.5;

    // New: lockout & stability
    public int RaiseCooldownIntervals = 120;      // how many adjust cycles to wait after a drop
    public int StableIntervalsBeforeRaise = 15;  // consecutive “good” cycles before we allow a raise

    // State
    public double TargetFps { get; private set; }
    private double _smoothedFrameTimeMs;
    private int _frameCount = 0;
    private readonly Queue<double> _recentFrameTimes = new();

    // Lockout state
    private int _cooldownRemaining = 0;       // adjust cycles left before we can raise
    private int _stableGoodIntervals = 0;     // consecutive intervals within “good” band

    public AdaptiveFpsLimiter(double initialFps = 60)
    {
        TargetFps = Clamp(initialFps, MinFps, MaxFps);
        _smoothedFrameTimeMs = 1000.0 / TargetFps;
    }

    public void Update(double frameTimeMs, double frameDeltaMs)
    {
        _smoothedFrameTimeMs = Lerp(_smoothedFrameTimeMs, frameTimeMs, SmoothFactor);

        const int trendWindow = 15;
        _recentFrameTimes.Enqueue(frameTimeMs);
        if (_recentFrameTimes.Count > trendWindow)
            _recentFrameTimes.Dequeue();

        _frameCount++;
        if (_frameCount % AdjustInterval != 0) return;

        double targetFrameBudget = 1000.0 / TargetFps;
        double utilization = _smoothedFrameTimeMs / targetFrameBudget;
        double fps = TargetFps;

        double trend = EstimateTrendMsPerFrame(_recentFrameTimes); // >0 = getting slower

        bool canRaise = _cooldownRemaining == 0 && _stableGoodIntervals >= StableIntervalsBeforeRaise;

        if (utilization > UpperUtil)
        {
            // Over budget: reduce FPS
            double down = DownStep;
            if (EnablePrediction && trend > 0)
                down += TrendWeight * trend;

            fps -= down;
            _cooldownRemaining = RaiseCooldownIntervals; // start lockout
            _stableGoodIntervals = 0;                    // reset stability counter
        }
        else if (utilization < LowerUtil)
        {
            // Under budget: consider raising
            _stableGoodIntervals++; // count a “good” interval
            if (canRaise)
            {
                double up = UpStep;
                if (EnablePrediction && trend < 0)
                    up += TrendWeight * (-trend);
                fps += up;
            }
        }
        else
        {
            // Within band: hold steady, but accumulate stability
            _stableGoodIntervals++;
        }

        if (_cooldownRemaining > 0)
            _cooldownRemaining--;

        TargetFps = Clamp(fps, MinFps, MaxFps);
    }

    public double GetTargetFrameTimeMs() => 1000.0 / TargetFps;

    private static double Clamp(double v, double min, double max) => Math.Max(min, Math.Min(max, v));
    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    private static double EstimateTrendMsPerFrame(Queue<double> samples)
    {
        if (samples.Count < 2) return 0.0;
        int n = samples.Count;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        int i = 0;
        foreach (var y in samples)
        {
            sumX += i;
            sumY += y;
            sumXY += i * y;
            sumX2 += i * i;
            i++;
        }
        double denom = n * sumX2 - sumX * sumX;
        if (Math.Abs(denom) < 1e-9) return 0.0;
        return (n * sumXY - sumX * sumY) / denom;
    }
}*/

// bool automaticFrameRateLimiterEnabled = true; //get value from game window here
// const double automaticFrameRateLimiterMax = 1.0 / 500.0;
// const double automaticFrameRateLimiterMin = 1.0 / 30.0;
// const int requiredConsecutive = 6;
// double currentFrameTimeLimit = 1.0 / 60.0;
// int consecutiveFasterChecks = 0;
// int consecutiveSlowerChecks = 0;
// double slowFrameTimeAccum = 0.0;
// double slowDownLockTimer = 0.0;
// double slowDownLockDuration = 2.0;
// if (automaticFrameRateLimiterEnabled)
// {
//     if (slowDownLockTimer > 0.0)
//     {
//         slowDownLockTimer -= frameDelta;
//         if (slowDownLockTimer <= 0.0) slowDownLockTimer = 0.0;
//     }
//     
//     if (FrameTime >= currentFrameTimeLimit)
//     {
//         consecutiveSlowerChecks++;
//         consecutiveFasterChecks = 0;
//         slowFrameTimeAccum += FrameTime;
//         if (consecutiveSlowerChecks >= requiredConsecutive)
//         {
//             currentFrameTimeLimit = slowFrameTimeAccum / consecutiveSlowerChecks;
//             slowFrameTimeAccum = 0.0;
//             consecutiveSlowerChecks = 0;
//             slowDownLockTimer = slowDownLockDuration;
//         }
//     }
//     else if (FrameTime < currentFrameTimeLimit)
//     {
//         consecutiveFasterChecks++;
//         consecutiveSlowerChecks = 0;
//         slowFrameTimeAccum = 0.0;
//         if (consecutiveFasterChecks >= requiredConsecutive)
//         {
//             if (slowDownLockTimer <= 0.0)
//             {
//                 currentFrameTimeLimit *= 0.9;//increase fast, because slow check sets it to frameDelta directly
//             }
//             consecutiveFasterChecks = 0;
//         }
//     }
//     else
//     {
//         consecutiveFasterChecks = 0;
//         consecutiveSlowerChecks = 0;
//     }
//     
//     if (currentFrameTimeLimit < automaticFrameRateLimiterMax) currentFrameTimeLimit = automaticFrameRateLimiterMax;
//     if (currentFrameTimeLimit > automaticFrameRateLimiterMin) currentFrameTimeLimit = automaticFrameRateLimiterMin;
//
//     targetFps = Math.Max(1, (int)Math.Round(1.0 / currentFrameTimeLimit));
// }