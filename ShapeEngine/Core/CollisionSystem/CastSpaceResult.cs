namespace ShapeEngine.Core.CollisionSystem;

//NOTE: Theoretically giving a CastSpace Function a previous CastSpaceResult, the function could determine first contact as well!
//TODO: Add a way to determine first contact on the cast space result class -> giving it a collider and if it exists it is not first contact?
public class CastSpaceResult(int capacity) : Dictionary<CollisionObject, CastSpaceEntry>(capacity)
{
    //TODO: Add filter functions (find closest, furthest collider, etc.)
    // add sorting functions as well
    public bool AddCollider(Collider collider)
    {
        var parent = collider.Parent;
        if (parent == null) return false;

        if (TryGetValue(parent, out var entry))
        {
            entry.Add(collider);
        }
        else
        {
            var newEntry = new CastSpaceEntry(parent, 2);
            newEntry.Add(collider);
            Add(parent, newEntry);
        }

        return true;
    }

    //CHECK: Does this work the way I want it to?
    // Do I have to remove the entry as well?
    public bool IsFirstContact(Collider collider)
    {
        if(collider.Parent == null) return false;
        return TryGetValue(collider.Parent, out var entry) && entry.IsFirstContact(collider);
    }
}