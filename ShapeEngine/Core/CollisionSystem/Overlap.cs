using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Core.CollisionSystem;

public class Overlap
{
    public readonly Collider Self;
    public readonly Collider Other;
    public bool FirstContact { get; internal set; }
    public Overlap(Collider self, Collider other)
    {
        Self = self;
        Other = other;
        FirstContact = false;
    }
    public Overlap(Collider self, Collider other, bool firstContact)
    {
        Self = self;
        Other = other;
        FirstContact = firstContact;
    }

    private Overlap(Overlap overlap)
    {
        Self = overlap.Self;
        Other = overlap.Other;
    }
    public Overlap Copy() => new(this);
}