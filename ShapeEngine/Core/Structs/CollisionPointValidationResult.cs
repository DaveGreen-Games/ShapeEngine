namespace ShapeEngine.Core.Structs;

public readonly struct CollisionPointValidationResult(
    CollisionPoint combined,
    CollisionPoint closest,
    CollisionPoint furthest,
    CollisionPoint pointingTowards)
{
    public readonly CollisionPoint Combined = combined;
    public readonly CollisionPoint Closest = closest;
    public readonly CollisionPoint Furthest = furthest;
    public readonly CollisionPoint PointingTowards = pointingTowards;

    public CollisionPointValidationResult() : this(new CollisionPoint(), new CollisionPoint(), new CollisionPoint(), new CollisionPoint()) { }
}