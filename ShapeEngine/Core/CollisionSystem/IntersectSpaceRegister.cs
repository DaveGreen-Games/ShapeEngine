namespace ShapeEngine.Core.CollisionSystem;

public class IntersectSpaceRegister : List<IntersectSpaceEntry>
{
    public readonly CollisionObject Object;

    public IntersectSpaceRegister(CollisionObject colObject, int capacity) : base(capacity)
    {
        Object = colObject;
    }

    public bool AddEntry(IntersectSpaceEntry entry)
    {
        if (entry.Count <= 0) return false;
        if (entry.Collider.Parent == null || entry.Collider.Parent == Object) return false;
        Add(entry);
        return true;
    }
    
    //TODO: Add filter / query methods here to get specific IntersectSpaceEntries
}