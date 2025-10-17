using ShapeEngine.Core.GameDef;

namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;

public partial class CollisionHandler
{
    #region Support Classes

    /// <summary>
    /// Manages registration, addition, and removal of objects of type <typeparamref name="T"/>.
    /// Uses temporary holding and removing sets to batch process changes.
    /// </summary>
    private class ObjectRegister<T>(int capacity)
    {
        /// <summary>
        /// Contains all currently registered objects of type <typeparamref name="T"/>.
        /// </summary>
        public readonly HashSet<T> AllObjects = new(capacity);
        
        /// <summary>
        /// Temporarily holds objects to be added during batch processing.
        /// </summary>
        private readonly HashSet<T> tempHolding = new(capacity / 4);
        
        /// <summary>
        /// Temporarily holds objects to be removed during batch processing.
        /// </summary>
        private readonly HashSet<T> tempRemoving = new(capacity / 4);

        /// <summary>
        /// Registers an object of type <typeparamref name="T"/> for addition during batch processing.
        /// Returns true if the object was successfully added to the temporary holding set.
        /// </summary>
        /// <remarks>
        /// An object will not be added if it is already scheduled for removal/addition or if it already exists in the main set.
        /// </remarks>
        public bool Add(T obj)
        {
            return !tempRemoving.Contains(obj) && !AllObjects.Contains(obj) && tempHolding.Add(obj);
        }
        /// <summary>
        /// Registers a range of objects of type <typeparamref name="T"/> for addition during batch processing.
        /// Returns the number of objects successfully added to the temporary holding set.
        /// </summary>
        /// <param name="objs">The collection of objects to add.</param>
        /// <returns>The number of objects added.</returns>
        /// <remarks> Objects will not be added if they are already scheduled for removal/addition or if they already exist in the main set.</remarks>
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
        /// <summary>
        /// Registers a range of objects of type <typeparamref name="T"/> for addition during batch processing.
        /// Returns the number of objects successfully added to the temporary holding set.
        /// </summary>
        /// <param name="objs">The array of objects to add.</param>
        /// <returns>The number of objects added.</returns>
        /// <remarks>
        /// Objects will not be added if they are already scheduled for removal/addition or if they already exist in the main set.
        /// </remarks>
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
        /// <summary>
        /// Registers an object of type <typeparamref name="T"/> for removal during batch processing.
        /// Returns true if the object was successfully added to the temporary removing set.
        /// </summary>
        /// <remarks>
        /// If the object is in the temporary holding set, it is removed from there first.
        /// Only objects present in the main set can be scheduled for removal.
        /// </remarks>
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
        /// <summary>
        /// Registers a range of objects of type <typeparamref name="T"/> for removal during batch processing.
        /// Returns the number of objects successfully added to the temporary removing set.
        /// </summary>
        /// <param name="objs">The collection of objects to remove.</param>
        /// <returns>The number of objects removed.</returns>
        /// <remarks>
        /// Each object is processed by the <c>Remove</c> method, which ensures only objects present in the main set are scheduled for removal.
        /// Objects in the temporary holding set are removed from there first.
        /// </remarks>
        public int RemoveRange(IEnumerable<T> objs)
        {
            var removed = 0;
            foreach (var obj in objs)
            {
                if(Remove(obj)) removed++;
            }
            return removed;
        }
        /// <summary>
        /// Registers a range of objects of type <typeparamref name="T"/> for removal during batch processing.
        /// Returns the number of objects successfully added to the temporary removing set.
        /// </summary>
        /// <param name="objs">The array of objects to remove.</param>
        /// <returns>The number of objects removed.</returns>
        /// <remarks>
        /// Each object is processed by the <c>Remove</c> method, which ensures only objects present in the main set are scheduled for removal.
        /// Objects in the temporary holding set are removed from there first.
        /// </remarks>
        public int RemoveRange(params T[] objs)
        {
            var removed = 0;
            foreach (var obj in objs)
            {
                if(Remove(obj)) removed++;
            }
            return removed;
        }
        /// <summary>
        /// Processes all pending additions and removals.
        /// Removes objects in the temporary removing set from the main set,
        /// then adds objects in the temporary holding set to the main set.
        /// Clears both temporary sets after processing.
        /// </summary>
        public void Process()
        {
            foreach (var obj in tempRemoving)
            {
                AllObjects.Remove(obj);
                ObjectRemoved(obj);
            }

            tempRemoving.Clear();

            foreach (var obj in tempHolding)
            {
                AllObjects.Add(obj);
                ObjectAdded(obj);
            }

            tempHolding.Clear();
        }
        /// <summary>
        /// Called when an object of type <typeparamref name="T"/> is added to the main set.
        /// Override this method to implement custom logic upon addition.
        /// </summary>
        protected virtual void ObjectAdded(T obj)
        {
        }
        /// <summary>
        /// Called when an object of type <typeparamref name="T"/> is removed from the main set.
        /// Override this method to implement custom logic upon removal.
        /// </summary>
        protected virtual void ObjectRemoved(T obj)
        {
        }

        /// <summary>
        /// Clears all registered objects and pending additions/removals.
        /// Invokes <c>ObjectRemoved</c> for each object in the main set before clearing.
        /// </summary>
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