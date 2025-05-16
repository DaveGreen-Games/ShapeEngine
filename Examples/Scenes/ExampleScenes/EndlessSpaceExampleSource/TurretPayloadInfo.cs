namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal readonly struct TurretPayloadInfo
{
    public readonly AutogunStats AutogunStats;
    public readonly BulletStats BulletStats;
    public readonly float TravelTime;
    public readonly float SmokeDuration;
    public readonly float Size;
    public readonly float ImpactSize;
    public readonly float ImpactDamage;
    public readonly float ImpactForce;

    public TurretPayloadInfo(AutogunStats autogun, BulletStats bullet, float smokeDuration, float travelTime, float size, float impactSize, float impactDamage, float impactForce)
    {
        SmokeDuration = smokeDuration;
        TravelTime = travelTime;
        AutogunStats = autogun;
        BulletStats = bullet;
        Size = size;
        ImpactSize = impactSize;
        ImpactDamage = impactDamage;
        ImpactForce = impactForce;

    }

}