namespace ShapeEngine.Core.CollisionSystem;

public class CastSpaceEntry : List<Collider>
{
    //TODO: Add filter functions (find closest, furthest collider, etc.)
    // add sorting functions as well
    
    public readonly CollisionObject OtherCollisionObject;
    
    public CastSpaceEntry(CollisionObject otherCollisionObject, int capacity) : base(capacity)
    {
        OtherCollisionObject = otherCollisionObject;
    }

    public CastSpaceEntry(CollisionObject otherCollisionObject)
    {
        OtherCollisionObject = otherCollisionObject;
    }
    
    public bool IsFirstContact(Collider collider)
    {
        return !Contains(collider);
    }
}