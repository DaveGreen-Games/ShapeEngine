namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

public partial class CollisionHandler
{
    private class FirstContactStack<T, M>(int capacity) : Dictionary<T, HashSet<M>>(capacity)
        where T : class
        where M : class
    {
        public bool RemoveEntry(T first, M second)
        {
            if (TryGetValue(first, out var register))
            {
                bool removed = register.Remove(second);
                if (register.Count <= 0) Remove(first);
                return removed;
            }

            return false;
            // return TryGetValue(first, out var register) && register.Count > 0 && register.Remove(second);
        }

        public bool AddEntry(T first, M second)
        {
            if (TryGetValue(first, out var register))
            {
                return register.Add(second);
            }

            var newRegister = new HashSet<M>(2);
            newRegister.Add(second);
            Add(first, newRegister);
            return true;
        }
    }
}