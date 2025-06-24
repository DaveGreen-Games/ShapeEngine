namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Represents an overlap between two colliders.
/// </summary>
/// <remarks>
/// Used to encapsulate the relationship between two colliders, including whether it is a first contact.
/// </remarks>
public class Overlap
{
    /// <summary>
    /// The collider representing 'self' in the overlap.
    /// </summary>
    public readonly Collider Self;
    /// <summary>
    /// The collider representing 'other' in the overlap.
    /// </summary>
    public readonly Collider Other;
    /// <summary>
    /// Indicates whether this is the first contact between the colliders.
    /// </summary>
    public bool FirstContact { get; internal set; }
    /// <summary>
    /// Initializes a new instance of the <see cref="Overlap"/> class.
    /// </summary>
    /// <param name="self">The 'self' collider.</param>
    /// <param name="other">The 'other' collider.</param>
    public Overlap(Collider self, Collider other)
    {
        Self = self;
        Other = other;
        FirstContact = false;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Overlap"/> class with first contact information.
    /// </summary>
    /// <param name="self">The 'self' collider.</param>
    /// <param name="other">The 'other' collider.</param>
    /// <param name="firstContact">Whether this is the first contact.</param>
    public Overlap(Collider self, Collider other, bool firstContact)
    {
        Self = self;
        Other = other;
        FirstContact = firstContact;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Overlap"/> class by copying another overlap.
    /// </summary>
    /// <param name="overlap">The overlap to copy.</param>
    private Overlap(Overlap overlap)
    {
        Self = overlap.Self;
        Other = overlap.Other;
        FirstContact = overlap.FirstContact;
    }
    /// <summary>
    /// Creates a copy of this overlap.
    /// </summary>
    /// <returns>A new <see cref="Overlap"/> instance with the same data.</returns>
    public Overlap Copy() => new(this);
}