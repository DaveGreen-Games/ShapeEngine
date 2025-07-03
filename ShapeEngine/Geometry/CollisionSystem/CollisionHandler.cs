using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CollisionSystem;


/// <summary>
/// Handles collision detection, resolution, and spatial queries for registered <see cref="CollisionObject"/> instances.
/// Implements <see cref="IBounds"/> to provide bounding information for the collision system.
/// </summary>
/// <remarks>
/// This class manages the registration, update, and removal of collision objects,
/// and provides methods for collision queries and spatial operations.
/// </remarks>
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
                    if (resolver.FilterCollisionPoints && info.TotalCollisionPointCount > 0)
                    {
                        info.GenerateFilteredCollisionPoint(resolver.CollisionPointsFilterType, resolver.Transform.Position);
                    }
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
    /// <summary>
    /// Gets the number of registered <see cref="CollisionObject"/> instances in the collision system.
    /// </summary>
    public int Count => collisionBodyRegister.AllObjects.Count;

    /// <summary>
    /// Gets the bounding rectangle of the collision system.
    /// </summary>
    public Rect Bounds => spatialHash.Bounds;
    
    private readonly CollisionObjectRegister collisionBodyRegister;
    
    private readonly SpatialHash spatialHash;
    private readonly CollisionStack collisionStack;

    private  FirstContactStack<CollisionObject, CollisionObject> collisionObjectFirstContactRegisterActive;
    private  FirstContactStack<CollisionObject, CollisionObject> collisionObjectFirstContactRegisterTemp;
    
    private  FirstContactStack<Collider, Collider> colliderFirstContactRegisterActive;
    private  FirstContactStack<Collider, Collider> colliderFirstContactRegisterTemp;
 
    private readonly HashSet<Collider> collisionCandidateCheckRegister = [];
    private List<SpatialHash.Bucket> collisionCandidateBuckets = [];

    private readonly Dictionary<CollisionObject, IntersectSpaceRegister> intersectSpaceRegisters = new(128);
    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionHandler"/> class with the specified bounds and grid size.
    /// </summary>
    /// <param name="x">The X coordinate of the bounds.</param>
    /// <param name="y">The Y coordinate of the bounds.</param>
    /// <param name="w">The width of the bounds.</param>
    /// <param name="h">The height of the bounds.</param>
    /// <param name="rows">The number of rows in the spatial hash grid.</param>
    /// <param name="cols">The number of columns in the spatial hash grid.</param>
    /// <param name="startCapacity">The initial capacity for object registers. Default is 1024.</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionHandler"/> class with the specified bounding rectangle and grid size.
    /// </summary>
    /// <param name="bounds">The bounding rectangle for the collision system.</param>
    /// <param name="rows">The number of rows in the spatial hash grid.</param>
    /// <param name="cols">The number of columns in the spatial hash grid.</param>
    /// <param name="startCapacity">The initial capacity for object registers. Default is 1024.</param>
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
    /// <summary>
    /// Adds a <see cref="CollisionObject"/> to the collision system.
    /// </summary>
    /// <param name="collisionObject">The collision object to add.</param>
    public void Add(CollisionObject collisionObject) => collisionBodyRegister.Add(collisionObject);

    /// <summary>
    /// Adds a collection of <see cref="CollisionObject"/> instances to the collision system.
    /// </summary>
    /// <param name="collisionObjects">The collection of collision objects to add.</param>
    public void AddRange(IEnumerable<CollisionObject> collisionObjects) => collisionBodyRegister.AddRange(collisionObjects);

    /// <summary>
    /// Adds multiple <see cref="CollisionObject"/> instances to the collision system.
    /// </summary>
    /// <param name="collisionObjects">The collision objects to add.</param>
    public void AddRange(params CollisionObject[] collisionObjects)=> collisionBodyRegister.AddRange(collisionObjects);

    /// <summary>
    /// Removes a <see cref="CollisionObject"/> from the collision system.
    /// </summary>
    /// <param name="collisionObject">The collision object to remove.</param>
    public void Remove(CollisionObject collisionObject)=> collisionBodyRegister.Remove(collisionObject);

    /// <summary>
    /// Removes a collection of <see cref="CollisionObject"/> instances from the collision system.
    /// </summary>
    /// <param name="collisionObjects">The collection of collision objects to remove.</param>
    public void RemoveRange(IEnumerable<CollisionObject> collisionObjects)  => collisionBodyRegister.RemoveRange(collisionObjects);

    /// <summary>
    /// Removes multiple <see cref="CollisionObject"/> instances from the collision system.
    /// </summary>
    /// <param name="collisionObjects">The collision objects to remove.</param>
    public void RemoveRange(params CollisionObject[] collisionObjects)  => collisionBodyRegister.RemoveRange(collisionObjects);
    #endregion
    
    #region Public Functions
    /// <summary>
    /// Resizes the bounds of the collision system.
    /// </summary>
    /// <param name="newBounds">The new bounding rectangle.</param>
    public void ResizeBounds(Rect newBounds) => spatialHash.ResizeBounds(newBounds);

    /// <summary>
    /// Removes all registered collision objects and clears the collision system.
    /// </summary>
    /// <remarks>
    /// This method clears all objects and resets the collision stack.
    /// </remarks>
    public void Clear()
    {
        collisionBodyRegister.Clear();
        collisionStack.Clear();
    }
    /// <summary>
    /// Closes the collision system and releases all resources.
    /// </summary>
    /// <remarks>
    /// This method clears all objects and closes the spatial hash.
    /// </remarks>
    public void Close()
    {
        Clear();
        spatialHash.Close();
        
    }
    /// <summary>
    /// Updates the collision system for the current frame.
    /// </summary>
    /// <param name="dt">The time delta since the last update, in seconds.</param>
    /// <remarks>
    /// This method updates the spatial hash, processes collisions, and resolves them.
    /// </remarks>
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
                            //Only enabled colliders are added to the spatial hash
                            //Therefore only enabled colliders are in each bucket!
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
                                    IntersectionPoints? collisionPoints;
                                    if (passivChecking)
                                    {
                                        collisionPoints = candidate.Intersect(projected);
                                    }
                                    else
                                    {
                                        collisionPoints = projected.Intersect(candidate);
                                    }
                                    
                                    //shapes overlap but no collision points means collidable is completely inside other
                                    //closest point on bounds of other are now used for intersection point
                                    if (collisionPoints == null || collisionPoints.Count <= 0)
                                    {
                                        var refPoint = collider.PrevTransform.Position;// PrevPosition;
                                        if (!candidate.ContainsPoint(refPoint))
                                        {
                                            var closest = candidate.GetClosestPoint(refPoint, out float _);
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
                            //Only enabled colliders are added to the spatial hash
                            //Therefore only enabled colliders are in each bucket!
                            if (candidate == collider) continue;
                            if (candidate.Parent == null) continue;
                            if (candidate.Parent == collider.Parent) continue;
                            if (!mask.Has(candidate.CollisionLayer)) continue;
                            if (!collisionCandidateCheckRegister.Add(candidate)) continue;

                            bool overlap = collider.Overlap(candidate);
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
                                    IntersectionPoints? collisionPoints;
                                    if (passivChecking)
                                    {
                                        collisionPoints = candidate.Intersect(collider);
                                    }
                                    else
                                    {
                                        collisionPoints = collider.Intersect(candidate);
                                    }
                                    
                                    //shapes overlap but no collision points means collidable is completely inside other
                                    //closest point on bounds of other are now used for intersection point
                                    if (collisionPoints == null || collisionPoints.Count <= 0)
                                    {
                                        var refPoint = collider.PrevTransform.Position;// PrevPosition;
                                        if (!candidate.ContainsPoint(refPoint))
                                        {
                                            var closest = candidate.GetClosestPoint(refPoint, out float _);
                                            collisionPoints ??= [];
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
    /// <summary>
    /// Performs intersection queries for all colliders of a <see cref="CollisionObject"/> against the collision system.
    /// </summary>
    /// <param name="colObject">The collision object whose colliders are used for intersection queries</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders on <paramref name="colObject"/> with <c>ComputeIntersections</c> set to true are used.
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
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
    /// <summary>
    /// Performs an intersection query for a single <see cref="Collider"/> against the collision system.
    /// </summary>
    /// <param name="collider">The collider to test for intersections. The collider needs to be enabled!</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Collider collider, Vector2 origin)
    {
        if(!collider.Enabled) return null;
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
    /// <summary>
    /// Performs an intersection query for a <see cref="Segment"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
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
    /// <summary>
    /// Performs an intersection query for a <see cref="Line"/> shape against the collision system.
    /// </summary>
    /// <param name="line">The line shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Line line, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(line, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if(parent == null) continue;
    
                var collisionPoints = line.Intersect(candidate);
    
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
    /// <summary>
    /// Performs an intersection query for a <see cref="Ray"/> shape against the collision system.
    /// </summary>
    /// <param name="ray">The ray shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
    public IntersectSpaceResult? IntersectSpace(Ray ray, Vector2 origin, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(ray, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return null;
        
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                var parent = candidate.Parent;
                if(parent == null) continue;
    
                var collisionPoints = ray.Intersect(candidate);
    
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
    /// <summary>
    /// Performs an intersection query for a <see cref="Triangle"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The triangle shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
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
    /// <summary>
    /// Performs an intersection query for a <see cref="Circle"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The circle shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
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
    /// <summary>
    /// Performs an intersection query for a <see cref="Rect"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The rectangle shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
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
    /// <summary>
    /// Performs an intersection query for a <see cref="Quad"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The quad shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
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
    /// <summary>
    /// Performs an intersection query for a <see cref="Polygon"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The polygon shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
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
    /// <summary>
    /// Performs an intersection query for a <see cref="Polyline"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The polyline shape to test for intersections.</param>
    /// <param name="origin">The origin point for the intersection result.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>An <see cref="IntersectSpaceResult"/> containing intersection data, or null if no intersections are found.</returns>
    /// <remarks>
    /// Only enabled colliders are considered for intersection checks in this query.
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for all colliders of a <see cref="CollisionObject"/> against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="collisionBody">The collision object whose colliders are used for the cast query. Only enabled colliders from the collisionBody are used!</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders of the <paramref name="collisionBody"/> are considered.</item>
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(CollisionObject collisionBody, ref CastSpaceResult result)
    {
        if (!collisionBody.HasColliders) return;
        CastSpace(collisionBody.Colliders, ref result);
    }
    /// <summary>
    /// Performs an overlap query for a list of <see cref="Collider"/> instances against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="colliders">The list of colliders to use for the cast query. Only enabled colliders are used!</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders of the <paramref name="colliders"/> list are considered.</item>
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(List<Collider> colliders, ref CastSpaceResult result)
    {
        if (colliders.Count <= 0) return;
        if(result.Count > 0) result.Clear();

        foreach (var collider in colliders)
        {
            if(!collider.Enabled) continue;
            
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
    /// <summary>
    /// Performs an overlap query for a set of <see cref="Collider"/> instances against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="colliders">The set of colliders to use for the cast query. Only enabled colliders are used!</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders of the <paramref name="colliders"/> set are considered.</item>
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(HashSet<Collider> colliders, ref CastSpaceResult result)
    {
        if (colliders.Count <= 0) return;
        if(result.Count > 0) result.Clear();

        foreach (var collider in colliders)
        {
            if(!collider.Enabled) continue;
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
    /// <summary>
    /// Performs an overlap query for a single <see cref="Collider"/> against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="collider">The collider to use for the cast query. The collider needs to be enabled!</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Collider collider,  ref CastSpaceResult result)
    {
        if (!collider.Enabled) return;
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
    }
    /// <summary>
    /// Performs an overlap query for a <see cref="Segment"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for a <see cref="Line"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="line">The line shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Line line, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if(result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(line, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(line))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }
    /// <summary>
    /// Performs an overlap query for a <see cref="Ray"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="ray">The ray shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
    public void CastSpace(Ray ray, BitFlag collisionMask, ref CastSpaceResult result)
    {
        if(result.Count > 0) result.Clear();
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(ray, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (candidate.Parent == null) continue;
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(ray))
                {
                    result.AddCollider(candidate);
                }
            }
        }
    }
    /// <summary>
    /// Performs an overlap query for a <see cref="Triangle"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The triangle shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for a <see cref="Circle"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The circle shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for a <see cref="Rect"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The rectangle shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for a <see cref="Polygon"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The polygon shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for a <see cref="Polyline"/> shape against the collision system and stores the results in the provided <see cref="CastSpaceResult"/>.
    /// </summary>
    /// <param name="shape">The polyline shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <param name="result">A reference to the result object that will be populated with colliders that overlap.</param>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Only enabled colliders in the collision system are checked.</item>
    /// <item>The result is cleared before being populated.</item>
    /// </list>
    /// </remarks>
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
    
    
    /// <summary>
    /// Performs an overlap query for all colliders of a <see cref="CollisionObject"/> against the collision system.
    /// </summary>
    /// <param name="collisionBody">The collision object whose colliders are used for the cast query. Only enabled colliders from the collisionBody are used!</param>
    /// <returns>The number of colliders in the system that overlap with any collider of the given <paramref name="collisionBody"/>.</returns>
    /// <remarks>
    /// Only enabled colliders of the <paramref name="collisionBody"/> are considered.
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(CollisionObject collisionBody)
    {
        if (!collisionBody.HasColliders) return 0;

        int count = 0;
        foreach (var collider in collisionBody.Colliders)
        {
            if(!collider.Enabled) continue;
            
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
    /// <summary>
    /// Performs an overlap query for a single <see cref="Collider"/> against the collision system.
    /// </summary>
    /// <param name="collider">The collider to use for the cast query. The collider needs to be enabled!</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="collider"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Collider collider)
    {
        if (!collider.Enabled) return 0;
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
    
    /// <summary>
    /// Performs an overlap query for a <see cref="Segment"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for a <see cref="Line"/> shape against the collision system.
    /// </summary>
    /// <param name="line">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="line"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Line line, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(line, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(line))
                {
                    count++;
                }
            }
        }

        return count;
    }
    /// <summary>
    /// Performs an overlap query for a <see cref="Ray"/> shape against the collision system.
    /// </summary>
    /// <param name="ray">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="ray"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
    public int CastSpace(Ray ray, BitFlag collisionMask)
    {
        collisionCandidateBuckets.Clear();
        collisionCandidateCheckRegister.Clear();
        
        spatialHash.GetCandidateBuckets(ray, ref collisionCandidateBuckets);
        if (collisionCandidateBuckets.Count <= 0) return 0;

        int count = 0;
        foreach (var bucket in collisionCandidateBuckets)
        {
            foreach (var candidate in bucket)
            {
                if (!collisionMask.Has(candidate.CollisionLayer)) continue;
                if (!collisionCandidateCheckRegister.Add(candidate)) continue;
                
                if (candidate.Overlap(ray))
                {
                    count++;
                }
            }
        }

        return count;
    }
    /// <summary>
    /// Performs an overlap query for a <see cref="Triangle"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for a <see cref="Circle"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for a <see cref="Rect"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for a <see cref="Polygon"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
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
    /// <summary>
    /// Performs an overlap query for a <see cref="Polyline"/> shape against the collision system.
    /// </summary>
    /// <param name="shape">The segment shape to use for the cast query.</param>
    /// <param name="collisionMask">The collision mask to filter candidate colliders.</param>
    /// <returns>The number of colliders in the system that overlap with the given <paramref name="shape"/>.</returns>
    /// <remarks>
    /// Only enabled colliders in the collision system are checked.
    /// </remarks>
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
    
    /// <summary>
    /// Sorts the given list of <see cref="Collider"/> instances in-place by their distance to the specified <paramref name="sortOrigin"/>.
    /// </summary>
    /// <param name="result">A reference to the list of colliders to sort.</param>
    /// <param name="sortOrigin">The origin point used for sorting by distance.</param>
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
    
    /// <summary>
    /// Draws debug information for the spatial hash using the specified border and fill colors.
    /// </summary>
    /// <param name="border">The color to use for the border.</param>
    /// <param name="fill">The color to use for the fill.</param>
    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
        spatialHash.DebugDraw(border, fill);
    }

    /// <summary>
    /// Retrieves a set of unique <see cref="CollisionObject"/> parents from a list of <see cref="Collider"/> instances.
    /// </summary>
    /// <param name="colliders">The list of colliders to extract parents from.</param>
    /// <returns>
    /// A <see cref="HashSet{CollisionObject}"/> containing the unique parent objects,
    /// or <c>null</c> if the list is empty.
    /// </returns>
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

