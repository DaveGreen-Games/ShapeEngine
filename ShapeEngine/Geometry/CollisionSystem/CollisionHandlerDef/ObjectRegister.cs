namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

public partial class CollisionHandler
{
    #region Support Classes

    private class ObjectRegister<T>
    {
        public readonly HashSet<T> AllObjects;
        private readonly List<T> tempHolding;
        private readonly List<T> tempRemoving;

        public ObjectRegister(int capacity)
        {
            AllObjects = new(capacity);
            tempHolding = new(capacity / 4);
            tempRemoving = new(capacity / 4);
        }

        public void Add(T obj) => tempHolding.Add(obj);

        public void AddRange(IEnumerable<T> objs) => tempHolding.AddRange(objs);

        public void AddRange(params T[] objs) => tempHolding.AddRange(objs);

        public void Remove(T obj) => tempRemoving.Add(obj);

        public void RemoveRange(IEnumerable<T> objs) => tempRemoving.AddRange(objs);

        public void RemoveRange(params T[] objs) => tempRemoving.AddRange(objs);

        public void Process()
        {
            foreach (var obj in tempRemoving)
            {
                AllObjects.Remove(obj);
            }

            tempRemoving.Clear();

            foreach (var obj in tempHolding)
            {
                AllObjects.Add(obj);
            }

            tempHolding.Clear();
        }

        protected virtual void ObjectAdded(T obj)
        {
        }

        protected virtual void ObjectRemoved(T obj)
        {
        }

        public void Clear()
        {
            foreach (var obj in AllObjects)
            {
                ObjectRemoved(obj);
            }

            AllObjects.Clear();
            tempHolding.Clear();
            tempRemoving.Clear();
        }
    }

    #endregion
}