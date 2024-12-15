using ShapeEngine.Lib;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

public class CollisionHandler : IBounds
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
                if(register.Count <= 0) continue;
                foreach (var info in register.Values)
                {
                    resolver.ResolveCollision(info);
                }
            }
        }

    }
    private class CollisionRegister : Dictionary<CollisionObject, CollisionInformation>
    {
        // public List<CollisionInformation>? GetCollisionInformations() => Values.Count <= 0 ? null : Values.ToList();

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

    private class CollisionObjectRegister : ObjectRegister<CollisionObject>
    {
        private readonly CollisionHandler handler;

        public CollisionObjectRegister(int capacity, CollisionHandler handler) : base(capacity)
        {
            this.handler = handler;
        }

        protected override void ObjectAdded(CollisionObject obj)
        {
            obj.OnCollisionSystemEntered(handler);
        }

        protected override void ObjectRemoved(CollisionObject obj)
        {
            obj.OnCollisionSystemLeft(handler);
        }
    }
    
    private class FirstContactStack<T, M>(int capacity) : Dictionary<T, HashSet<M>>(capacity)
        where T : class
        where M : class
    {
        public bool RemoveEntry(T first, M second)
        {
            if (TryGetValue(first, out var register))
            {
                bool removed = register.Remove(second);
                if(register.Count <= 0) Remove(first);
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
    #endregion
    
    #region Members
    private readonly CollisionObjectRegister collisionBodyRegister;
    
    private readonly SpatialHash spatialHash;
    private readonly CollisionStack collisionStack;

    private  FirstContactStack<CollisionObject, CollisionObject> collisionObjectFirstContactRegisterActive;
    private  FirstContactStack<CollisionObject, CollisionObject> collisionObjectFirstContactRegisterTemp;
    
    private  FirstContactStack<Collider, Collider> colliderFirstContactRegisterActive;
    private  FirstContactStack<Collider, Collider> colliderFirstContactRegisterTemp;
 
    private readonly HashSet<Collider> collisionCandidateCheckRegister = new();
    private List<SpatialHash.Bucket> collisionCandidateBuckets = new();

    public int Count => collisionBodyRegister.AllObjects.Count; // collisionBodies.Count;

    public Rect Bounds => spatialHash.Bounds;
    
    private readonly Dictionary<CollisionObject, IntersectSpaceRegister> intersectSpaceRegisters = new(128);
    #endregion

    #region Constructors

    public CollisionHandler(float x, float y, float w, float h, int rows, int cols, int startCapacity = 1024)
    {
        spatialHash = new(x, y, w, h, rows, cols);
        collisionBodyRegister = new(startCapacity, this);
        collisionStack = new(startCapacity / 4);
        colliderFirstContactRegisterActive = new(startCapacity / 4);
        colliderFirstContactRegisterTemp = new(startCapacity / 4);
        collisionObjectFirstContactRegisterActive = new(startCapacity / 4);
        collisionObjectFirstContactRegisterTemp = new(startCapacity / 4);
    }

    public CollisionHandler(Rect bounds, int rows, int cols, int startCapacity = 1024)
    {
        spatialHash = new(bounds.X, bounds.Y, bounds.Width, bounds.Height, rows, cols);
        
        collisionBodyRegister = new(startCapacity, this);
        collisionStack = new(startCapacity / 4);
        colliderFirstContactRegisterActive = new(startCapacity / 4);
        colliderFirstContactRegisterTemp = new(startCapacity / 4);
        collisionObjectFirstContactRegisterActive = new(startCapacity / 4);
        collisionObjectFirstContactRegisterTemp = new(startCapacity / 4);
    }
    
    #endregion
    
    #region Add & Remove Collision Objects
    public void Add(CollisionObject collisionObject) => collisionBodyRegister.Add(collisionObject);
    public void AddRange(IEnumerable<CollisionObject> collisionObjects) => collisionBodyRegister.AddRange(collisionObjects);
    public void AddRange(params CollisionObject[] collisionObjects)=> collisionBodyRegister.AddRange(collisionObjects);
    public void Remove(CollisionObject collisionObject)=> collisionBodyRegister.Remove(collisionObject);
    public void RemoveRange(IEnumerable<CollisionObject> collisionObjects)  => collisionBodyRegister.RemoveRange(collisionObjects);
    public void RemoveRange(params CollisionObject[] collisionObjects)  => collisionBodyRegister.RemoveRange(collisionObjects);
    #endregion
    
    #region Public Functions
    public void ResizeBounds(Rect newBounds) => spatialHash.ResizeBounds(newBounds);

    public void Clear()
    {
        collisionBodyRegister.Clear();
        collisionStack.Clear();
    }
    public void Close()
    {
        Clear();
        spatialHash.Close();
        
    }
    public void Update(float dt)
    {
        spatialHash.Fill(collisionBodyRegister.AllObjects);

        ProcessCollisions(dt);
        
        Resolve();
    }
    
    #endregion
    
    #region Private Functions
    
    private void ProcessCollisions(float dt)
    {
        foreach (var collisionBody in collisionBodyRegister.AllObjects)
        {
            if (!collisionBody.Enabled || !collisionBody.HasColliders) continue;

            CollisionRegister? collisionRegister = null;
            // var oldOverlapRegister = oldOverlapStack.GetRegister(collisionBody);
            
            var passivChecking = collisionBody.Passive;
            if (collisionBody.ProjectShape)
            {
                foreach (var collider in collisionBody.Colliders)
                {
                    if (!collider.Enabled) continue;
                    if (collider.Parent == null) continue;

                    var projected = collider.Project(collisionBody.Velocity * dt);
                    if(projected == null) continue;
                    collisionCandidateBuckets.Clear();
                    collisionCandidateCheckRegister.Clear();
                    spatialHash.GetCandidateBuckets(projected, ref collisionCandidateBuckets);
                    
                    if(collisionCandidateBuckets.Count <= 0) continue;     
                    
                    var mask = collider.CollisionMask;
                    bool computeIntersections = collider.ComputeIntersections;
                    
                    foreach (var bucket in collisionCandidateBuckets)
                    {
                        foreach (var candidate in bucket)
                        {
                            if (candidate == collider) continue;
                            if (candidate.Parent == null) continue;
                            if (candidate.Parent == collider.Parent) continue;
                            if (!mask.Has(candidate.CollisionLayer)) continue;
                            if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                            bool overlap = projected.Overlap(candidate);
                            if (overlap)
                            {
                                //multiple colliders can be involved with the same pair of collision objects, therefore we also have to check if the collision object pair was already added to the temp register.
                                var removed = collisionObjectFirstContactRegisterActive.RemoveEntry(collider.Parent, candidate.Parent);
                                var added = collisionObjectFirstContactRegisterTemp.AddEntry(collider.Parent, candidate.Parent);
                                bool firstContactCollisionObject = !removed && added;
                                
                                bool firstContactCollider = !colliderFirstContactRegisterActive.RemoveEntry(collider, candidate);
                                colliderFirstContactRegisterTemp.AddEntry(candidate, collider);
                                
                                if (computeIntersections)
                                {
                                    CollisionPoints? collisionPoints = null;
                                    if (passivChecking)
                                    {
                                        collisionPoints = candidate.Intersect(projected);
                                    }
                                    else
                                    {
                                        collisionPoints = projected.Intersect(candidate);
                                    }
                                    
                                    //shapes overlap but no collision points means collidable is completely inside other
                                    //closest point on bounds of other are now used for collision point
                                    if (collisionPoints == null || collisionPoints.Count <= 0)
                                    {
                                        var refPoint = collider.PrevTransform.Position;// PrevPosition;
                                        if (!candidate.ContainsPoint(refPoint))
                                        {
                                            var closest = candidate.GetClosestCollisionPoint(refPoint);
                                            collisionPoints ??= new();
                                            collisionPoints.Add(closest);
                                        }
                                    }

                                    Collision c = new(collider, candidate, firstContactCollider, collisionPoints);
                                    collisionRegister??= new();
                                    collisionRegister.AddCollision(c, firstContactCollisionObject);
                                }
                                else
                                {
                                    Collision c = new(collider, candidate, firstContactCollider);
                                    collisionRegister??= new();
                                    collisionRegister.AddCollision(c, firstContactCollisionObject);
                                }
                            }
                        }
                    }
                    
                }
            
            }
            else
            {
                foreach (var collider in collisionBody.Colliders)
                {
                    if (!collider.Enabled) continue;
                    if (collider.Parent == null) continue;
                    collisionCandidateBuckets.Clear();
                    collisionCandidateCheckRegister.Clear();
                    spatialHash.GetRegisteredCollisionCandidateBuckets(collider, ref collisionCandidateBuckets);
                    
                    if(collisionCandidateBuckets.Count <= 0) continue;     
                    
                    var mask = collider.CollisionMask;
                    bool computeIntersections = collider.ComputeIntersections;
                    
                    foreach (var bucket in collisionCandidateBuckets)
                    {
                        foreach (var candidate in bucket)
                        {
                            if (candidate == collider) continue;
                            if (candidate.Parent == null) continue;
                            if (candidate.Parent == collider.Parent) continue;
                            if (!mask.Has(candidate.CollisionLayer)) continue;
                            if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                            bool overlap = collider.Overlap(candidate); // ShapeGeometry.Overlap(collider, candidate);
                            if (overlap)
                            {
                                //multiple colliders can be involved with the same pair of collision objects, therefore we also have to check if the collision object pair was already added to the temp register.
                                var removed = collisionObjectFirstContactRegisterActive.RemoveEntry(collider.Parent, candidate.Parent);
                                var added = collisionObjectFirstContactRegisterTemp.AddEntry(collider.Parent, candidate.Parent);
                                bool firstContactCollisionObject = !removed && added;
                                
                                bool firstContactCollider = !colliderFirstContactRegisterActive.RemoveEntry(collider, candidate);
                                colliderFirstContactRegisterTemp.AddEntry(candidate, collider);
                                
                                if (computeIntersections)
                                {                                                         
                                    CollisionPoints? collisionPoints = null;
                                    if (passivChecking)
                                    {
                                        collisionPoints = candidate.Intersect(collider);
                                    }
                                    else
                                    {
                                        collisionPoints = collider.Intersect(candidate);
                                    }
                                    
                                    //shapes overlap but no collision points means collidable is completely inside other
                                    //closest point on bounds of other are now used for collision point
                                    if (collisionPoints == null || collisionPoints.Count <= 0)
                                    {
                                        var refPoint = collider.PrevTransform.Position;// PrevPosition;
                                        if (!candidate.ContainsPoint(refPoint))
                                        {
                                            var closest = candidate.GetClosestCollisionPoint(refPoint);
                                            collisionPoints ??= new();
                                            collisionPoints.Add(closest);
                                        }
                                    }

                                    Collision c = new(collider, candidate, firstContactCollider, collisionPoints);
                                    collisionRegister??= new();
                                    collisionRegister.AddCollision(c, firstContactCollisionObject);
                                }
                                else
                                {
                                    Collision c = new(collider, candidate, firstContactCollider);
                                    collisionRegister??= new();
                                    collisionRegister.AddCollision(c, firstContactCollisionObject);
                                }
                            }
                        }
                    }
                }
            
            }

            if (collisionRegister != null)
            {
                collisionStack.AddCollisionRegister(collisionBody, collisionRegister);
            }
            
        }
    }
    private void Resolve()
    {
        collisionBodyRegister.Process();

        collisionStack.ProcessCollisions();
        collisionStack.Clear();

        foreach (var kvp in collisionObjectFirstContactRegisterActive)
        {
            var resolver = kvp.Key;
            var others = kvp.Value;
            if(others.Count <= 0) continue;
            foreach (var other in others)
            {
                resolver.ResolveContactEnded(other);
            }
        }
        collisionObjectFirstContactRegisterActive.Clear();
        (collisionObjectFirstContactRegisterActive, collisionObjectFirstContactRegisterTemp) = (collisionObjectFirstContactRegisterTemp, collisionObjectFirstContactRegisterActive);
        
        foreach (var kvp in colliderFirstContactRegisterActive)
        {
            var self = kvp.Key;
            var resolver = self.Parent;
            if(resolver == null) continue;
            var others = kvp.Value;
            if(others.Count <= 0) continue;
            foreach (var other in others)
            {
                resolver.ResolveColliderContactEnded(self, other);
            }
        }
        colliderFirstContactRegisterActive.Clear();
        (colliderFirstContactRegisterActive, colliderFirstContactRegisterTemp) = (colliderFirstContactRegisterTemp, colliderFirstContactRegisterActive);
    }
    
    #endregion
    
    #region Intersect Space
    
    public IntersectSpaceResult? IntersectSpace(CollisionObject colObject, Vector2 origin)
    {
        if(colObject.Colliders.Count <= 0) return null;
        
        foreach (var collider in colObject.Colliders)
        {
            if(!collider.Enabled) continue;
            if(!collider.ComputeIntersections) continue;
            
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return null;
            var collisionMask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                    var parent = candidate.Parent;
                    if(parent == null) continue;
        
                    var collisionPoints = collider.Intersect(candidate);
        
                    if (collisionPoints == null || !collisionPoints.Valid) continue;
                    
                    var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                    if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                    {
                        register.Add(entry);
                    }
                    else
                    {
                        var newRegister = new IntersectSpaceRegister(parent, 2);
                        newRegister.Add(entry);
                        intersectSpaceRegisters.Add(parent, newRegister);
                    }
                }
            }
        }
        
        
        if(intersectSpaceRegisters.Count <= 0) return null;
        
        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }
        intersectSpaceRegisters.Clear();
        return result;
    }
    public IntersectSpaceResult? IntersectSpace(Collider collider, Vector2 origin)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        var collisionMask = collider.CollisionMask;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if(parent == null) continue;
    
                var collisionPoints = collider.Intersect(candidate);
    
                if (collisionPoints == null || !collisionPoints.Valid) continue;
                
                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }
        
        if(intersectSpaceRegisters.Count <= 0) return null;
        
        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }
        intersectSpaceRegisters.Clear();
        return result;
    }
    public IntersectSpaceResult? IntersectSpace(Segment shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if(parent == null) continue;
    
                var collisionPoints = shape.Intersect(candidate);
    
                if (collisionPoints == null || !collisionPoints.Valid) continue;
                
                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }
        
        if(intersectSpaceRegisters.Count <= 0) return null;
        
        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }
        intersectSpaceRegisters.Clear();
        return result;
    }
    public IntersectSpaceResult? IntersectSpace(Triangle shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if(parent == null) continue;
    
                var collisionPoints = shape.Intersect(candidate);
    
                if (collisionPoints == null || !collisionPoints.Valid) continue;
                
                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }
        
        if(intersectSpaceRegisters.Count <= 0) return null;
        
        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }
        intersectSpaceRegisters.Clear();
        return result;
    }
    public IntersectSpaceResult? IntersectSpace(Circle shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if(parent == null) continue;
    
                var collisionPoints = shape.Intersect(candidate);
    
                if (collisionPoints == null || !collisionPoints.Valid) continue;
                
                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }
        
        if(intersectSpaceRegisters.Count <= 0) return null;
        
        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }
        intersectSpaceRegisters.Clear();
        return result;
    }
    public IntersectSpaceResult? IntersectSpace(Rect shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if(parent == null) continue;
    
                var collisionPoints = shape.Intersect(candidate);
    
                if (collisionPoints == null || !collisionPoints.Valid) continue;
                
                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }
        
        if(intersectSpaceRegisters.Count <= 0) return null;
        
        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }
        intersectSpaceRegisters.Clear();
        return result;
    }
    public IntersectSpaceResult? IntersectSpace(Quad shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if(parent == null) continue;
    
                var collisionPoints = shape.Intersect(candidate);
    
                if (collisionPoints == null || !collisionPoints.Valid) continue;
                
                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }
        
        if(intersectSpaceRegisters.Count <= 0) return null;
        
        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }
        intersectSpaceRegisters.Clear();
        return result;
    }
    public IntersectSpaceResult? IntersectSpace(Polygon shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if(parent == null) continue;
    
                var collisionPoints = shape.Intersect(candidate);
    
                if (collisionPoints == null || !collisionPoints.Valid) continue;
                
                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }
        
        if(intersectSpaceRegisters.Count <= 0) return null;
        
        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }
        intersectSpaceRegisters.Clear();
        return result;
    }
    public IntersectSpaceResult? IntersectSpace(Polyline shape, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if(parent == null) continue;
    
                var collisionPoints = shape.Intersect(candidate);
    
                if (collisionPoints == null || !collisionPoints.Valid) continue;
                
                var entry = new IntersectSpaceEntry(candidate, collisionPoints);
                if (intersectSpaceRegisters.TryGetValue(parent, out var register))
                {
                    register.Add(entry);
                }
                else
                {
                    var newRegister = new IntersectSpaceRegister(parent, 2);
                    newRegister.Add(entry);
                    intersectSpaceRegisters.Add(parent, newRegister);
                }
            }
        }
        
        if(intersectSpaceRegisters.Count <= 0) return null;
        
        var result = new IntersectSpaceResult(origin, intersectSpaceRegisters.Count);
        foreach (var register in intersectSpaceRegisters.Values)
        {
            result.Add(register);
        }
        intersectSpaceRegisters.Clear();
        return result;
    }

    #endregion
    
    #region Cast Space
    
    public void CastSpace(CollisionObject collisionBody, ref CastSpaceResult result)
    {
        if (!collisionBody.HasColliders) return;
        CastSpace(collisionBody.Colliders, ref result);
        // if (sorted && result.Count > 1)
        // {
        //     var origin = collisionBody.Transform.Position; // collidable.GetCollider().Pos;
        //     result.Sort
        //     (
        //         (a, b) =>
        //         {
        //             float la = (origin - a.CurTransform.Position).LengthSquared();
        //             float lb = (origin - b.CurTransform.Position).LengthSquared();
        //
        //             if (la > lb) return 1;
        //             if (ShapeMath.EqualsF(la, lb)) return 0;
        //             return -1;
        //         }
        //     );
        // }
    }
    public void CastSpace(List<Collider> colliders, ref CastSpaceResult result)
    {
        if (colliders.Count <= 0) return;
        if(result.Count > 0) result.Clear();

        foreach (var collider in colliders)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;

            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate.Parent == null) continue;
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (collider.Overlap(candidate))
                    {
                        result.AddCollider(candidate);
                    }
                }
            }
        }
    }
    public void CastSpace(HashSet<Collider> colliders, ref CastSpaceResult result)
    {
        if (colliders.Count <= 0) return;
        if(result.Count > 0) result.Clear();

        foreach (var collider in colliders)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return;

            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate.Parent == null) continue;
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (collider.Overlap(candidate))
                    {
                        result.AddCollider(candidate);
                    }
                }
            }
        }
    }

    public void CastSpace(Collider collider,  ref CastSpaceResult result)
    {
        if(result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        var mask = collider.CollisionMask;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (candidate == collider) continue;
                if (!mask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (collider.Overlap(candidate))
                {
                    result.AddCollider(candidate);
                }
            }
        }
        // if (sorted && result.Count > 1)
        // {
        //     var origin = collider.CurTransform.Position;
        //     result.Sort
        //     (
        //         (a, b) =>
        //         {
        //             float la = (origin - a.CurTransform.Position).LengthSquared();
        //             float lb = (origin - b.CurTransform.Position).LengthSquared();
        //
        //             if (la > lb) return 1;
        //             if (ShapeMath.EqualsF(la, lb)) return 0;
        //             return -1;
        //         }
        //     );
        // }
    }
    public void CastSpace(Segment shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if(result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }
    public void CastSpace(Triangle shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if(result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }
    public void CastSpace(Circle shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if(result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }
    public void CastSpace(Rect shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if(result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }
    public void CastSpace(Polygon shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if(result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }
    public void CastSpace(Polyline shape, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if(result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }
    
    
    public int CastSpace(CollisionObject collisionBody)
    {
        if (!collisionBody.HasColliders) return 0;

        int count = 0;
        foreach (var collider in collisionBody.Colliders)
        {
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            if (collisionCandidateBuckets.Count <= 0) return 0;

            var mask = collider.CollisionMask;
            foreach (var bucket in collisionCandidateBuckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collider) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                    if (collider.Overlap(candidate))
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }
    public int CastSpace(Collider collider)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        var mask = collider.CollisionMask;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate == collider) continue;
                if (!mask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                if (collider.Overlap(candidate))
                {
                    count++;
                }
            }
        }

        return count;
    }
    public int CastSpace(Segment shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }
    public int CastSpace(Triangle shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }
    public int CastSpace(Circle shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }
    public int CastSpace(Rect shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }
    public int CastSpace(Polygon shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }
    public int CastSpace(Polyline shape, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(shape, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(shape))
                {
                    count++;
                }
            }
        }

        return count;
    }
    
    
    
    public void SortCastResult(ref List<Collider> result, Vector2 sortOrigin)
    {
        if (result.Count > 1)
        {
            result.Sort
            (
                (a, b) =>
                {
                    float la = (sortOrigin - a.CurTransform.Position).LengthSquared();
                    float lb = (sortOrigin - b.CurTransform.Position).LengthSquared();

                    if (la > lb) return 1;
                    if (ShapeMath.EqualsF(la, lb)) return 0;
                    return -1;
                }
            );
        }
    }

    #endregion
    
    #region Debug
    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
        spatialHash.DebugDraw(border, fill);
    }

    public static HashSet<CollisionObject>? GetParents(List<Collider> colliders)
    {
        if (colliders.Count <= 0) return null;
        
        var parents = new HashSet<CollisionObject>();
        
        foreach (var collider in colliders)
        {
            var parent = collider.Parent;
            if (parent != null)
            {
                parents.Add(parent);
            }
        }

        return parents;
    }
    
    #endregion
}
