using System.Numerics;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Core.Structs;

public struct ClosestDistance
{
    public readonly Vector2 A;
    public readonly Vector2 B;
    public readonly float DistanceSquared;
    
    public bool Valid => DistanceSquared > 0;

    private float distance = -1;
    public float Distance
    {
        get
        {
            if (distance < 0)
            {
                distance = DistanceSquared <= 0f ? 0f : MathF.Sqrt(DistanceSquared);
            }

            return distance;
        }
    }

    public Segment GetSegment() => new(A, B);

    public Vector2 DirToB
    {
        get
        {
            if (Distance <= 0) return new();
            return (B - A) / Distance;
        }
    }
    public Vector2 DirToA
    {
        get
        {
            if (Distance <= 0) return new();
            return (A - B) / Distance;
        }
    }

    public Vector2 DisplacementToB => (B - A);
    public Vector2 DisplacementToA => (A - B);
    
    public ClosestDistance()
    {
        A = new();
        B = new();
        DistanceSquared = 0f;
        distance = 0f;
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

    public ClosestDistance ReversePoints() => new (B, A);
}
