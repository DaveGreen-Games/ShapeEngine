using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core;

public struct AdaptiveFpsLimiterSettings
{
    
}
public sealed class AdaptiveFpsLimiter
{
    private ValueRangeInt limit;
    public double TargetFps { get; private set; }
    public bool Enabled { get; set; }
    
    public AdaptiveFpsLimiter(int minFps, int maxFps, bool enabled)
    {
        limit = new ValueRangeInt(minFps, maxFps);
        TargetFps = minFps;
        Enabled = enabled;
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
    
    //TODO: Figure out formula to calculate target frame time / target frame rate
    
    public int Update(int targetFrameRate, double frameTimeMs, double frameDeltaMs)
    {
        if(!Enabled)
        {
            TargetFps = targetFrameRate;
            return targetFrameRate;
        }

        if (targetFrameRate > 0)
        {
            //TODO: implement
            TargetFps = targetFrameRate;
            return targetFrameRate;
        }
        else
        {
            
        }
        
        
        
        
        
        
        TargetFps = targetFrameRate;
        return targetFrameRate;
    }
}








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