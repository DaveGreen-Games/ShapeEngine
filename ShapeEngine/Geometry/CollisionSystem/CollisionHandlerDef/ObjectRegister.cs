using ShapeEngine.Core.GameDef;

namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

public partial class CollisionHandler
{
    #region Support Classes

    private class ObjectRegister<T>
    {
        public readonly HashSet<T> AllObjects;
        private readonly HashSet<T> tempHolding;
        private readonly HashSet<T> tempRemoving;

        public ObjectRegister(int capacity)
        {
            AllObjects = new(capacity);
            tempHolding = new(capacity / 4);
            tempRemoving = new(capacity / 4);
        }

        public bool Add(T obj)
        {
            return !tempRemoving.Contains(obj) && !AllObjects.Contains(obj) && tempHolding.Add(obj);
        }

        public int AddRange(IEnumerable<T> objs)
        {
            var added = 0;
            foreach (var obj in objs)
            {
                if(tempRemoving.Contains(obj)) continue;
                if(AllObjects.Contains(obj)) continue;
                if(tempHolding.Add(obj))
                {
                    added++;
                }
            }
            return added;
        }

        public int AddRange(params T[] objs)
        {
            var added = 0;
            foreach (var obj in objs)
            {
                if(tempRemoving.Contains(obj)) continue;
                if(AllObjects.Contains(obj)) continue;
                if(tempHolding.Add(obj))
                {
                    added++;
                }
            }

            return added;
        }

        public bool Remove(T obj)
        {
            if (tempHolding.Remove(obj))
            {
                if (AllObjects.Contains(obj))
                {
                    return tempRemoving.Add(obj);
                }

                return false;
            }
            
            if (!AllObjects.Contains(obj)) return false;
            return tempRemoving.Add(obj);
        }

        public int RemoveRange(IEnumerable<T> objs)
        {
            var removed = 0;
            foreach (var obj in objs)
            {
                if(Remove(obj)) removed++;
            }
            return removed;
        }

        public int RemoveRange(params T[] objs)
        {
            var removed = 0;
            foreach (var obj in objs)
            {
                if(Remove(obj)) removed++;
            }
            return removed;
        }

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