using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Core.CollisionSystem;

public class Overlap(Collider self, Collider other)
{
    public readonly Collider Self = self;
    public readonly Collider Other = other;
}