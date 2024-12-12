using System.Reflection.Emit;
using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Contains the information of an overlap between two collision objects in form of a list of overlaps.
/// An overlap contains the information of the two overlapping colliders.
/// </summary>
public class ContactEndedInformation : List<Contact>
{
    public readonly CollisionObject Self;
    public readonly CollisionObject Other;
        
    public ContactEndedInformation(CollisionObject self, CollisionObject other)
    {
        Self = self;
        Other = other;
    }

    public ContactEndedInformation(CollisionObject self, CollisionObject other, List<Contact> overlaps)
    {
        Self = self;
        Other = other;
        AddRange(overlaps);
    }

    public ContactEndedInformation Copy()
    {
        var contactsCopy = new List<Contact>();
        foreach (var contact in this)
        {
            contactsCopy.Add(contact.Copy());
        }
        return new (Self, Other, contactsCopy);
    }
    internal Contact? PopContact(Collider self, Collider other)
    {
        foreach (var contact in this)
        {
            if (contact.Self == self && contact.Other == other)
            {
                Remove(contact);
                return contact;
            }
        }
        return null;
    }
    
    public List<Contact>? FilterContacts(Predicate<Contact> match)
    {
        if(Count <= 0) return null;
        List<Contact>? filtered = null;
        foreach (var c in this)
        {
            if (match(c))
            {
                filtered??= new();
                filtered.Add(c);
            }
        }
        return filtered;
    }
    
    public HashSet<Collider>? GetAllOtherColliders()
    {
        if(Count <= 0) return null;
        HashSet<Collider> others = new();
        foreach (var c in this)
        {
            others.Add(c.Other);
        }
        return others;
    }
    
    /*public List<Contact>? GetAllFirstContactOverlaps()
    {
        return FilterOverlaps((c) => c.FirstContact);
    }
    public HashSet<Collider>? GetAllOtherFirstContactColliders()
    {
        var filtered = GetAllFirstContactOverlaps();
        if(filtered == null) return null;
        HashSet<Collider> others = new();
        foreach (var c in filtered)
        {
            others.Add(c.Other);
        }
        return others;
    }*/

}