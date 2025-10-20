namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

public partial class CollisionHandler
{
    private class CollisionRegister: Dictionary<CollisionObject, CollisionInformation>
    {
        public bool AddCollision(Collision collision, bool firstContact)
        {
            var selfParent = collision.Self.Parent;
            var otherParent = collision.Other.Parent;

            if (selfParent == null || otherParent == null) return false;

            if (TryGetValue(otherParent, out var cols))
            {
                cols.Add(collision);
            }
            else
            {
                var colInfo = new CollisionInformation(selfParent, otherParent, firstContact);
                colInfo.Add(collision);

                Add(otherParent, colInfo);
            }

            return true;
        }
    }
}