namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal struct ExplosivePayloadInfo
{
    public float Force;
    public float Radius;
    public float Damage;
    public float TravelTime;
    public float SmokeDuration;

    public ExplosivePayloadInfo(float smokeDuration, float travelTime, float damage, float radius, float force)
    {
        SmokeDuration = smokeDuration;
        TravelTime = travelTime;
        Damage = damage;
        Radius = radius;
        Force = force;
    }
    
}