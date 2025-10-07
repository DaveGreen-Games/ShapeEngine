namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

public partial class CollisionHandler
{
    #region Support Classes

    private class CollisionStack(int capacity) : Dictionary<CollisionObject, CollisionRegister>(capacity)
    {
        public bool AddCollisionRegister(CollisionObject owner, CollisionRegister register)
        {
            if (register.Count <= 0) return false;

            return TryAdd(owner, register);
        }

        public void ProcessCollisions()
        {
            foreach (var entry in this)
            {
                var resolver = entry.Key;
                var register = entry.Value;
                if (register.Count <= 0) continue;
                foreach (var info in register.Values)
                {
                    if (resolver.FilterCollisionPoints && info.TotalCollisionPointCount > 0)
                    {
                        info.GenerateFilteredCollisionPoint(resolver.CollisionPointsFilterType, resolver.Transform.Position);
                    }

                    resolver.ResolveCollision(info);
                }
            }
        }
    }

    #endregion
}