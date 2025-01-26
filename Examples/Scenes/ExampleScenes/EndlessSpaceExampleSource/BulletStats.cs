using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal readonly struct BulletStats
{
    public readonly float Size;
    public readonly float Speed;
    public readonly float Damage;
    public readonly float Lifetime;

    public BulletStats(float size, float speed, float damage, float lifetime)
    {
        Size = size;
        Speed = speed;
        Damage = damage;
        Lifetime = lifetime;
    }

    public BulletStats RandomizeSpeed(float min = 0.95f, float max = 1.05f, float shipSpeed = 0f)
    {
        return new(Size, Speed * Rng.Instance.RandF(min, max) + shipSpeed, Damage, Lifetime);
    }
}