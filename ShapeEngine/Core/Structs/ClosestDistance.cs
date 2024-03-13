using System.Numerics;

namespace ShapeEngine.Core.Structs;

public struct ClosestDistance
{
    public readonly Vector2 A;
    public readonly Vector2 B;
    public readonly float DistanceSquared;

    private float distance = -1;
    public float Distance
    {
        get
        {
            if (distance < 0) distance = MathF.Sqrt(DistanceSquared);

            return distance;
        }
    }

    
    public ClosestDistance()
    {
        
    }
    public ClosestDistance(Vector2 a, Vector2 b)
    {
        A = a;
        B = b;
        DistanceSquared = (a - b).LengthSquared();
    }
    public ClosestDistance(Vector2 a, Vector2 b, float distanceSquared)
    {
        A = a;
        B = b;
        DistanceSquared = distanceSquared;
    }
}
