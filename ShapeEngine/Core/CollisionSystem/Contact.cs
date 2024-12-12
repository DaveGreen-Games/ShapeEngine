using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Contains the information of a contact between two colliders.
/// </summary>
public class Contact
{
    public readonly Collider Self;
    public readonly Collider Other;
    // public bool FirstContact { get; internal set; }
    public Contact(Collider self, Collider other)
    {
        Self = self;
        Other = other;
        // FirstContact = false;
    }
    public Contact(Collider self, Collider other, bool firstContact)
    {
        Self = self;
        Other = other;
        // FirstContact = firstContact;
    }

    private Contact(Contact contact)
    {
        Self = contact.Self;
        Other = contact.Other;
        // FirstContact = contact.FirstContact;
    }
    public Contact Copy() => new(this);
}