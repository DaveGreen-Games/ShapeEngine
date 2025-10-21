using System.Collections.Concurrent;
using System.Diagnostics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;


/// <summary>
/// Handles collision detection, resolution, and spatial queries for registered <see cref="CollisionObject"/> instances.
/// </summary>
/// <remarks>
/// This class manages the registration, update, and removal of collision objects,
/// and provides methods for collision queries and spatial operations.
/// </remarks>
public partial class CollisionHandler
{
    #region Members
    /// <summary>
    /// Gets the number of registered <see cref="CollisionObject"/> instances in the collision system.
    /// </summary>
    public int Count => collisionBodyRegister.AllObjects.Count;

    public bool ParallelProcessing = true;
    private Stopwatch stopwatch = new();
    private long totalFillTime = 0;
    private long totalProcessTime = 0;
    private long totalResolveTime = 0;
    private int updates = 0;
    
    private readonly CollisionObjectRegister collisionBodyRegister;
    
    private readonly IBroadphase broadphase;
    private readonly CollisionStack collisionStack;

    private  FirstContactStack<CollisionObject, CollisionObject> collisionObjectFirstContactRegisterActive;
    private  FirstContactStack<CollisionObject, CollisionObject> collisionObjectFirstContactRegisterTemp;
    
    private  FirstContactStack<Collider, Collider> colliderFirstContactRegisterActive;
    private  FirstContactStack<Collider, Collider> colliderFirstContactRegisterTemp;
 
    private readonly HashSet<Collider> collisionCandidateCheckRegister = [];
    private List<BroadphaseBucket> collisionCandidateBuckets = [];

    private readonly Dictionary<CollisionObject, IntersectSpaceRegister> intersectSpaceRegisters = new(128);
    #endregion

    #region Constructors
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionHandler"/> class with the specified bounds and grid size.
    /// </summary>
    /// <param name="broadphase">The broadphase to use.</param>
    /// <param name="startCapacity">The initial capacity for object registers. Default is 1024.</param>
    public CollisionHandler(IBroadphase broadphase, int startCapacity = 1024)
    {
        this.broadphase = broadphase;
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
    /// <returns><c>true</c> if the object was added; otherwise, <c>false</c>.</returns>
    public bool Add(CollisionObject collisionObject) => collisionBodyRegister.Add(collisionObject);
    
    /// <summary>
    /// Adds a range of <see cref="CollisionObject"/> instances to the collision system.
    /// </summary>
    /// <param name="collisionObjects">The collection of collision objects to add.</param>
    /// <returns>The number of objects added.</returns>
    public int AddRange(IEnumerable<CollisionObject> collisionObjects) => collisionBodyRegister.AddRange(collisionObjects);

    /// <summary>
    /// Adds a range of <see cref="CollisionObject"/> instances to the collision system.
    /// </summary>
    /// <param name="collisionObjects">The collision objects to add.</param>
    /// <returns>The number of objects added.</returns>
    public int AddRange(params CollisionObject[] collisionObjects)=> collisionBodyRegister.AddRange(collisionObjects);

    /// <summary>
    /// Removes a <see cref="CollisionObject"/> from the collision system.
    /// </summary>
    /// <param name="collisionObject">The collision object to remove.</param>
    /// <returns><c>true</c> if the object was removed; otherwise, <c>false</c>.</returns>
    public bool Remove(CollisionObject collisionObject)=> collisionBodyRegister.Remove(collisionObject);

    /// <summary>
    /// Removes a collection of <see cref="CollisionObject"/> instances from the collision system.
    /// </summary>
    /// <param name="collisionObjects">The collection of collision objects to remove.</param>
    /// <returns>The number of objects removed.</returns>
    public int RemoveRange(IEnumerable<CollisionObject> collisionObjects)  => collisionBodyRegister.RemoveRange(collisionObjects);

    /// <summary>
    /// Removes multiple <see cref="CollisionObject"/> instances from the collision system.
    /// </summary>
    /// <param name="collisionObjects">The collision objects to remove.</param>
    /// <returns>The number of objects removed.</returns>
    public int RemoveRange(params CollisionObject[] collisionObjects)  => collisionBodyRegister.RemoveRange(collisionObjects);
    #endregion
    
    #region Public Functions

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
        broadphase.Close();
        
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
        stopwatch.Restart();
        broadphase.Fill(collisionBodyRegister.AllObjects);
        var fillTime = stopwatch.ElapsedMilliseconds;
        totalFillTime += fillTime;
        
        stopwatch.Restart();
        ProcessCollisions(dt);
        var processTime = stopwatch.ElapsedMilliseconds;
        totalProcessTime += processTime;
        
        stopwatch.Restart();
        Resolve();
        var resolveTime = stopwatch.ElapsedMilliseconds;
        totalResolveTime += resolveTime;
        
        updates++;

        if (updates > 60)
        {
            double averageFillTime = totalFillTime / (double)updates;
            double averageProcessTime = totalProcessTime / (double)updates;
            double averageResolveTime = totalResolveTime / (double)updates;
            
            Console.WriteLine($"Collision Handler Average Times over {updates} updates:");
            Console.WriteLine($" - Broadphase Fill Time: {averageFillTime:F2} ms");
            Console.WriteLine($" - Collision Processing Time: {averageProcessTime:F2} ms");
            Console.WriteLine($" - Collision Resolve Time: {averageResolveTime:F2} ms");
            
            totalFillTime = 0;
            totalProcessTime = 0;
            totalResolveTime = 0;
            updates = 0;
        }

    }
    
    #endregion
    
    #region Private Functions
    
    
    
    // private void ProcessCollisions(float dt)
    // {
    //     if (ParallelProcessing)
    //     {
    //         // Thread-local storage for collision candidates
    //         var threadLocalData = new ThreadLocal<(
    //             List<BroadphaseBucket> buckets,
    //             HashSet<Collider> checkRegister
    //         )>(() => (new List<BroadphaseBucket>(), new HashSet<Collider>()));
    //
    //         // Thread-safe collection for collision registers
    //         var collisionRegisters = new ConcurrentBag<(CollisionObject obj, CollisionRegister register)>();
    //
    //         Parallel.ForEach(collisionBodyRegister.AllObjects, collisionBody =>
    //         {
    //             if (!collisionBody.Enabled || !collisionBody.HasColliders) return;
    //
    //             CollisionRegister? collisionRegister = null;
    //             var (buckets, checkRegister) = threadLocalData.Value!;
    //
    //             ProcessCollisionObject(collisionBody, dt, buckets, checkRegister, ref collisionRegister);
    //
    //             if (collisionRegister != null)
    //             {
    //                 collisionRegisters.Add((collisionBody, collisionRegister));
    //             }
    //         });
    //
    //         // Merge results back to collision stack
    //         foreach (var (obj, register) in collisionRegisters)
    //         {
    //             collisionStack.AddCollisionRegister(obj, register);
    //         }
    //
    //         threadLocalData.Dispose();
    //     }
    //     else
    //     {
    //         foreach (var collisionBody in collisionBodyRegister.AllObjects)
    //         {
    //             if (!collisionBody.Enabled || !collisionBody.HasColliders) continue;
    //
    //             CollisionRegister? collisionRegister = null;
    //
    //             ProcessCollisionObject(collisionBody, dt, collisionCandidateBuckets, collisionCandidateCheckRegister, ref collisionRegister);
    //
    //             if (collisionRegister != null)
    //             {
    //                 collisionStack.AddCollisionRegister(collisionBody, collisionRegister);
    //             }
    //         }
    //     }
    // }
    //
    // private void ProcessCollisionObject(CollisionObject collisionBody, float dt, List<BroadphaseBucket> buckets, HashSet<Collider> checkRegister, ref CollisionRegister? collisionRegister)
    // {
    //     var passivChecking = collisionBody.Passive;
    //     foreach (var collider in collisionBody.Colliders)
    //     {
    //         if (!collider.Enabled) continue;
    //         if (collider.Parent == null) continue;
    //
    //         Polygon? projectedShape = null;
    //         if (collisionBody.ProjectShape)
    //         {
    //             projectedShape = collider.Project(collisionBody.Velocity * dt);
    //             if (projectedShape == null) continue;
    //         }
    //
    //         buckets.Clear();
    //         checkRegister.Clear();
    //         if (projectedShape != null) broadphase.GetCandidateBuckets(projectedShape, ref buckets);
    //         else broadphase.GetCandidateBuckets(collider, ref buckets);
    //
    //         if (buckets.Count <= 0) continue;
    //
    //         var mask = collider.CollisionMask;
    //         bool computeIntersections = collider.ComputeIntersections;
    //
    //         foreach (var bucket in buckets)
    //         {
    //             foreach (var candidate in bucket)
    //             {
    //                 if (candidate == collider) continue;
    //                 if (candidate.Parent == null) continue;
    //                 if (candidate.Parent == collider.Parent) continue;
    //                 if (!mask.Has(candidate.CollisionLayer)) continue;
    //                 if (!checkRegister.Add(candidate)) continue;
    //
    //                 bool overlap = projectedShape?.Overlap(candidate) ?? collider.Overlap(candidate);
    //                 if (overlap)
    //                 {
    //                     var removed = collisionObjectFirstContactRegisterActive.RemoveEntry(collider.Parent, candidate.Parent);
    //                     var added = collisionObjectFirstContactRegisterTemp.AddEntry(collider.Parent, candidate.Parent);
    //                     bool firstContactCollisionObject = !removed && added;
    //
    //                     bool firstContactCollider = !colliderFirstContactRegisterActive.RemoveEntry(collider, candidate);
    //                     colliderFirstContactRegisterTemp.AddEntry(candidate, collider);
    //
    //                     if (computeIntersections)
    //                     {
    //                         IntersectionPoints? collisionPoints;
    //                         if (passivChecking)
    //                         {
    //                             collisionPoints = projectedShape != null ? candidate.Intersect(projectedShape) : candidate.Intersect(collider);
    //                         }
    //                         else
    //                         {
    //                             collisionPoints = projectedShape?.Intersect(candidate) ?? collider.Intersect(candidate);
    //                         }
    //
    //                         if (collisionPoints == null || collisionPoints.Count <= 0)
    //                         {
    //                             var refPoint = collider.PrevTransform.Position;
    //                             if (!candidate.ContainsPoint(refPoint))
    //                             {
    //                                 var closest = candidate.GetClosestPoint(refPoint, out float _);
    //                                 collisionPoints ??= new();
    //                                 collisionPoints.Add(closest);
    //                             }
    //                         }
    //
    //                         Collision c = new(collider, candidate, firstContactCollider, collisionPoints);
    //                         collisionRegister ??= new();
    //                         collisionRegister.AddCollision(c, firstContactCollisionObject);
    //                     }
    //                     else
    //                     {
    //                         Collision c = new(collider, candidate, firstContactCollider);
    //                         collisionRegister ??= new();
    //                         collisionRegister.AddCollision(c, firstContactCollisionObject);
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }
    //
    //
    
    
    
    private void ProcessCollisions(float dt)
    {
        foreach (var collisionBody in collisionBodyRegister.AllObjects)
        {
            if (!collisionBody.Enabled || !collisionBody.HasColliders) continue;
    
            CollisionRegister? collisionRegister = null;
            
            ProcessCollisionObject(collisionBody, dt, ref collisionRegister);
    
            if (collisionRegister != null)
            {
                collisionStack.AddCollisionRegister(collisionBody, collisionRegister);
            }
        }
    }
    private void ProcessCollisionObject(CollisionObject collisionBody, float dt, ref CollisionRegister? collisionRegister)
    {
        var passivChecking = collisionBody.Passive;
        foreach (var collider in collisionBody.Colliders)
        {
            if (!collider.Enabled) continue;
            if (collider.Parent == null) continue;

            Polygon? projectedShape = null;
            if (collisionBody.ProjectShape)
            {
                projectedShape = collider.Project(collisionBody.Velocity * dt);
                if(projectedShape == null) continue;
            }
            
            collisionCandidateBuckets.Clear();
            collisionCandidateCheckRegister.Clear();
            if(projectedShape != null) broadphase.GetCandidateBuckets(projectedShape, ref collisionCandidateBuckets);
            else broadphase.GetCandidateBuckets(collider, ref collisionCandidateBuckets);
            
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

                    bool overlap = projectedShape?.Overlap(candidate) ?? collider.Overlap(candidate);
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
                                collisionPoints = projectedShape != null ? candidate.Intersect(projectedShape) : candidate.Intersect(collider);
                            }
                            else
                            {
                                collisionPoints = projectedShape?.Intersect(candidate) ?? collider.Intersect(candidate);
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

    #region Debug
    
    /// <summary>
    /// Draws debug information for the spatial hash using the specified border and fill colors.
    /// </summary>
    /// <param name="border">The color to use for the border.</param>
    /// <param name="fill">The color to use for the fill.</param>
    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
        broadphase.DebugDraw(border, fill);
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

