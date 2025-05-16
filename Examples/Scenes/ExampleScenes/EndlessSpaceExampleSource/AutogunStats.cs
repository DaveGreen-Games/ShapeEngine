namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal readonly struct AutogunStats
{
    public enum TargetingType
    {
        Closest = 0,
        Furthest = 1,
        LowestHp = 2,
        HighestHp = 3
    }
    public readonly int Clipsize;
    public readonly float ReloadTime;
    public readonly float BulletsPerSecond;
    public readonly float FirerateInterval => 1f / BulletsPerSecond;
    public readonly float DetectionRange;
    public readonly TargetingType Targeting;
    public readonly float Accuracy;

    public AutogunStats(int clipSize, float reloadTime, float bulletsPerSecond, float detectionRange, float accuracy,
        TargetingType targetingType)
    {
        Clipsize = clipSize;
        ReloadTime = reloadTime;
        BulletsPerSecond = bulletsPerSecond;
        DetectionRange = detectionRange;
        Targeting = targetingType;
        Accuracy = accuracy;

    }
}