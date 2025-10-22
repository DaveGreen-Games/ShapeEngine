using System.Collections.Concurrent;
using System.Diagnostics;
using ShapeEngine.Color;
using ShapeEngine.Geometry.PolygonDef;

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

    /// <summary>
    /// Enables or disables parallel processing of collision detection.
    /// When <c>true</c>, the handler uses the parallel processing path
    /// (<see cref="ProcessCollisionsParallel"/>) to process collision objects.
    /// When <c>false</c>, the sequential path (<see cref="ProcessCollisionsSequential"/>) is used.
    /// </summary>
    /// <remarks>
    /// Default is <c>true</c>. If enabled, ensure any external state accessed by collision callbacks
    /// is safe for concurrent access.
    /// </remarks>
    public bool ParallelProcessing = true;
    
    /// <summary>
    /// Pool of per-thread temporary data objects used by the parallel collision processing path.
    /// Rent instances with <see cref="RentThreadLocalData"/> and return them with <see cref="ReturnThreadLocalData"/>.
    /// Using a <see cref="ConcurrentBag{T}"/> allows concurrent reuse across threads and reduces allocations.
    /// </summary>
    private readonly ConcurrentBag<ThreadLocalData> threadLocalPool = [];
    
    /// <summary>
    /// Lock object used to synchronize removals/additions in the
    /// collision-object-level first-contact register across threads.
    /// Protects access to <see cref="collisionObjectFirstContactRegisterActive"/> during parallel processing.
    /// </summary>
    private readonly object objFirstContactLock = new();

    /// <summary>
    /// Lock object used to synchronize removals/additions in the
    /// collider-level first-contact register across threads.
    /// Protects access to <see cref="colliderFirstContactRegisterActive"/> during parallel processing.
    /// </summary>
    private readonly object colliderFirstContactLock = new();

    /// <summary>
    /// Register that stores and manages all <see cref="CollisionObject"/> instances.
    /// Used for adding/removing objects and applying pending register changes.
    /// </summary>
    private readonly CollisionObjectRegister collisionBodyRegister;
       
    /// <summary>
    /// Broadphase spatial structure used to query candidate buckets for collision checks
    /// and to perform debug drawing of the spatial partition.
    /// </summary>
    private readonly IBroadphase broadphase;

    /// <summary>
    /// Accumulates detected collisions as <see cref="CollisionRegister"/> entries for
    /// later resolution by <see cref="ResolveSequential"/>/<see cref="ResolveParallel"/>.
    /// </summary>
    private readonly CollisionStack collisionStack;

    /// <summary>
    /// Active first-contact stack for collision object pairs. Represents contacts that
    /// were reported in the previous processing cycle and are considered 'active'.
    /// </summary>
    private FirstContactStack<CollisionObject, CollisionObject> collisionObjectFirstContactRegisterActive;

    /// <summary>
    /// Temporary first-contact stack for collision object pairs. Populated during the
    /// current processing cycle and swapped with the active stack after resolution.
    /// </summary>
    private FirstContactStack<CollisionObject, CollisionObject> collisionObjectFirstContactRegisterTemp;
       
    /// <summary>
    /// Active first-contact stack for collider pairs. Represents per-collider first-contact
    /// bookkeeping for the previous frame.
    /// </summary>
    private FirstContactStack<Collider, Collider> colliderFirstContactRegisterActive;

    /// <summary>
    /// Temporary first-contact stack for collider pairs. Collected during processing and
    /// swapped into the active register after resolution.
    /// </summary>
    private FirstContactStack<Collider, Collider> colliderFirstContactRegisterTemp;

    /// <summary>
    /// Reusable deduplication set used by the sequential processing path to avoid checking
    /// the same candidate collider multiple times per source collider.
    /// </summary>
    private readonly HashSet<Collider> collisionCandidateCheckRegister = [];

    /// <summary>
    /// Reusable list of broadphase buckets used by the sequential processing path to store
    /// candidate buckets returned by the broadphase query.
    /// </summary>
    private List<BroadphaseBucket> collisionCandidateBuckets = [];

    /// <summary>
    /// Cache mapping <see cref="CollisionObject"/> to its <see cref="IntersectSpaceRegister"/>,
    /// used to store intersection-related state and reduce repeated allocations/lookups.
    /// Initialized with a moderate default capacity.
    /// </summary>
    private readonly Dictionary<CollisionObject, IntersectSpaceRegister> intersectSpaceRegisters = new(128);
    
    
    private Stopwatch stopwatch = new();
    private long totalFillTime = 0;
    private long totalProcessTime = 0;
    private long totalResolveTime = 0;
    private int updates = 0;
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
        if(ParallelProcessing) ProcessCollisionsParallel(dt);
        else ProcessCollisionsSequential(dt);
        var processTime = stopwatch.ElapsedMilliseconds;
        totalProcessTime += processTime;
        
        stopwatch.Restart();
        if(ParallelProcessing) ResolveParallel();
        else ResolveSequential();
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
    
    #region Collision Processing
    
    #region Parallel
    /// <summary>
    /// Holds per-thread temporary data used by the parallel collision processing path.
    /// This pooled structure prevents repeated allocations by reusing:
    /// - <see cref="ObjFirstContact"/>: first-contact stack for collision object pairs.
    /// - <see cref="ColliderFirstContact"/>: first-contact stack for collider pairs.
    /// - <see cref="Buckets"/>: candidate broadphase buckets collected for the current thread.
    /// - <see cref="CheckRegister"/>: deduplication set for candidate colliders within this thread.
    /// Instances are rented from a concurrent pool and cleared before reuse.
    /// </summary>
    private class ThreadLocalData
    {
        public FirstContactStack<CollisionObject, CollisionObject> ObjFirstContact { get; }
        public FirstContactStack<Collider, Collider> ColliderFirstContact { get; }
        public List<BroadphaseBucket> Buckets { get; }
        public HashSet<Collider> CheckRegister { get; }

        public ThreadLocalData(int capacity)
        {
            ObjFirstContact = new FirstContactStack<CollisionObject, CollisionObject>(capacity);
            ColliderFirstContact = new FirstContactStack<Collider, Collider>(capacity);
            Buckets = new List<BroadphaseBucket>();
            CheckRegister = new HashSet<Collider>();
        }

        public void Clear()
        {
            ObjFirstContact.Clear();
            ColliderFirstContact.Clear();
            Buckets.Clear();
            CheckRegister.Clear();
        }
    }
    /// <summary>
    /// Rents a pooled <see cref="ThreadLocalData"/> instance for use in parallel collision processing.
    /// </summary>
    /// <remarks>
    /// Tries to obtain an instance from <see cref="threadLocalPool"/>. If an instance is available
    /// it is cleared and returned. If the pool is empty a new instance is created with a default
    /// capacity suitable for typical workloads.
    /// </remarks>
    /// <returns>A cleared <see cref="ThreadLocalData"/> ready for use by the caller.</returns>
    private ThreadLocalData RentThreadLocalData()
    {
        if (threadLocalPool.TryTake(out var data))
        {
            data.Clear();
            return data;
        }
        return new ThreadLocalData(64);
    }
    /// <summary>
    /// Returns a rented <see cref="ThreadLocalData"/> instance to the internal concurrent pool for reuse.
    /// </summary>
    /// <param name="data">The <see cref="ThreadLocalData"/> instance to return. Must not be <c>null</c>.</param>
    /// <remarks>
    /// The pool is a <see cref="System.Collections.Concurrent.ConcurrentBag{ThreadLocalData}"/>, allowing
    /// concurrent returns from multiple threads. The instance is not cleared here; callers should ensure
    /// that rented instances are cleared on rent (see <see cref="RentThreadLocalData"/> which calls <c>Clear</c>).
    /// </remarks>
    private void ReturnThreadLocalData(ThreadLocalData data)
    {
        threadLocalPool.Add(data);
    }

    /// <summary>
    /// Processes registered collision objects in parallel using a per-thread pooled data structure.
    /// Detects collisions for each object and enqueues resulting <see cref="CollisionRegister"/> instances
    /// for later resolution.
    /// </summary>
    /// <param name="dt">Delta time since the last update, in seconds.</param>
    /// <remarks>
    /// This method uses <c>Parallel.ForEach</c> and rents <see cref="ThreadLocalData"/> instances
    /// via <see cref="RentThreadLocalData"/> to avoid per-iteration allocations. Per-thread first-contact
    /// information is collected and later merged into the handler-level temporary first-contact registers.
    /// Care is taken to synchronize removals from shared active first-contact stacks; merging and returning
    /// of thread-local data happens after the parallel loop completes.
    /// </remarks>
    private void ProcessCollisionsParallel(float dt)
    {
        var collisionRegisters = new ConcurrentBag<(CollisionObject obj, CollisionRegister register)>();
        var usedThreadLocalData = new ConcurrentBag<ThreadLocalData>();
    
        Parallel.ForEach(collisionBodyRegister.AllObjects,
            () => RentThreadLocalData(),  // Rent from pool
            (collisionBody, loopState, threadLocal) =>
            {
                if (!collisionBody.Enabled || !collisionBody.HasColliders) return threadLocal;
    
                CollisionRegister? collisionRegister = null;
                ProcessCollisionObjectThreadSafe(collisionBody, dt, ref collisionRegister,
                    threadLocal.Buckets, threadLocal.CheckRegister,
                    threadLocal.ObjFirstContact, threadLocal.ColliderFirstContact,
                    collisionObjectFirstContactRegisterActive,
                    colliderFirstContactRegisterActive);
    
                if (collisionRegister != null)
                    collisionRegisters.Add((collisionBody, collisionRegister));
    
                return threadLocal;
            },
            threadLocal =>
            {
                usedThreadLocalData.Add(threadLocal);  // Collect for merging
            });
    
        // Add collision registers
        foreach (var (obj, register) in collisionRegisters)
            collisionStack.AddCollisionRegister(obj, register);
    
        // Merge first contact data and return to pool
        foreach (var threadLocal in usedThreadLocalData)
        {
            MergeFirstContacts(threadLocal.ObjFirstContact, collisionObjectFirstContactRegisterTemp);
            MergeFirstContacts(threadLocal.ColliderFirstContact, colliderFirstContactRegisterTemp);
            ReturnThreadLocalData(threadLocal);  // Return to pool
        }
    }
    
    /// <summary>
    /// Thread-safe processing of a single <see cref="CollisionObject"/> used by the parallel
    /// collision processing path.
    /// </summary>
    /// <remarks>
    /// Inspects each enabled <see cref="Collider"/> on <paramref name="collisionBody"/>, queries
    /// the provided broadphase candidate buckets via <paramref name="buckets"/>, computes overlap
    /// tests and optional intersection points, and populates <paramref name="collisionRegister"/>
    /// when collisions are detected. Uses the per-thread collections (<paramref name="buckets"/>,
    /// <paramref name="checkRegister"/>, <paramref name="objFirstContact"/>, and
    /// <paramref name="colliderFirstContact"/>) to avoid allocations and aggregate first-contact
    /// data locally. Shared active first-contact stacks (<paramref name="objFirstContactActive"/> and
    /// <paramref name="colliderFirstContactActive"/>) are used for removals under locks to ensure
    /// correct first-contact semantics across threads.
    /// </remarks>
    /// <param name="collisionBody">The collision object being processed.</param>
    /// <param name="dt">Elapsed time since the last update in seconds.</param>
    /// <param name="collisionRegister">Reference to a <see cref="CollisionRegister"/> that will be created and filled when collisions are found.</param>
    /// <param name="buckets">Per-thread list reused to collect candidate broadphase buckets.</param>
    /// <param name="checkRegister">Per-thread deduplication set for candidate <see cref="Collider"/> instances.</param>
    /// <param name="objFirstContact">Per-thread first-contact stack for collision object pairs.</param>
    /// <param name="colliderFirstContact">Per-thread first-contact stack for collider pairs.</param>
    /// <param name="objFirstContactActive">Shared active first-contact stack for collision object pairs (used for removals under lock).</param>
    /// <param name="colliderFirstContactActive">Shared active first-contact stack for collider pairs (used for removals under lock).</param>
    private void ProcessCollisionObjectThreadSafe(
        CollisionObject collisionBody,
        float dt,
        ref CollisionRegister? collisionRegister,
        List<BroadphaseBucket> buckets,
        HashSet<Collider> checkRegister,
        FirstContactStack<CollisionObject, CollisionObject> objFirstContact,
        FirstContactStack<Collider, Collider> colliderFirstContact,
        FirstContactStack<CollisionObject, CollisionObject> objFirstContactActive,
        FirstContactStack<Collider, Collider> colliderFirstContactActive)
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
                if (projectedShape == null) continue;
            }

            buckets.Clear();
            checkRegister.Clear();
            if (projectedShape != null) broadphase.GetCandidateBuckets(projectedShape, ref buckets);
            else broadphase.GetCandidateBuckets(collider, ref buckets);

            if (buckets.Count <= 0) continue;

            var mask = collider.CollisionMask;
            bool computeIntersections = collider.ComputeIntersections;

            foreach (var bucket in buckets)
            {
                foreach (var candidate in bucket)
                {
                    if (candidate == collider) continue;
                    if (candidate.Parent == null) continue;
                    if (candidate.Parent == collider.Parent) continue;
                    if (!mask.Has(candidate.CollisionLayer)) continue;
                    if (!checkRegister.Add(candidate)) continue;

                    bool overlap = projectedShape?.Overlap(candidate) ?? collider.Overlap(candidate);
                    if (overlap)
                    {
                        bool removed;
                        lock (objFirstContactLock)
                        {
                            removed = objFirstContactActive.RemoveEntry(collider.Parent, candidate.Parent);
                        }
                        var added = objFirstContact.AddEntry(collider.Parent, candidate.Parent);
                        bool firstContactCollisionObject = !removed && added;

                        bool firstContactCollider;
                        lock (colliderFirstContactLock)
                        {
                            firstContactCollider = !colliderFirstContactActive.RemoveEntry(collider, candidate);
                        }
                        colliderFirstContact.AddEntry(candidate, collider);

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

                            if (collisionPoints == null || collisionPoints.Count <= 0)
                            {
                                var refPoint = collider.PrevTransform.Position;
                                if (!candidate.ContainsPoint(refPoint))
                                {
                                    var closest = candidate.GetClosestPoint(refPoint, out float _);
                                    collisionPoints ??= new();
                                    collisionPoints.Add(closest);
                                }
                            }

                            Collision c = new(collider, candidate, firstContactCollider, collisionPoints);
                            collisionRegister ??= new();
                            collisionRegister.AddCollision(c, firstContactCollisionObject);
                        }
                        else
                        {
                            Collision c = new(collider, candidate, firstContactCollider);
                            collisionRegister ??= new();
                            collisionRegister.AddCollision(c, firstContactCollisionObject);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Resolves collisions after parallel detection by processing queued collision data and notifying
    /// collision objects and colliders about contact end events in parallel.
    /// </summary>
    /// <remarks>
    /// - Applies pending register changes.
    /// - Processes and clears the collision stack.
    /// - Iterates the active first-contact registers in parallel and invokes the appropriate
    ///   contact-ended callbacks on collision objects and colliders.
    /// - Clears and swaps active/temp first-contact registers to prepare for the next update.
    /// This method corresponds to the parallel processing path and should be used when
    /// <see cref="ParallelProcessing"/> is enabled.
    /// </remarks>
    private void ResolveParallel()
    {
        collisionBodyRegister.Process();

        collisionStack.ProcessCollisions();
        collisionStack.Clear();

        // Parallel processing of collision object contact endings
        Parallel.ForEach(collisionObjectFirstContactRegisterActive, kvp =>
        {
            var resolver = kvp.Key;
            var others = kvp.Value;
            if(others.Count <= 0) return;
            foreach (var other in others)
            {
                resolver.ResolveContactEnded(other);
            }
        });
        collisionObjectFirstContactRegisterActive.Clear();
        (collisionObjectFirstContactRegisterActive, collisionObjectFirstContactRegisterTemp) = (collisionObjectFirstContactRegisterTemp, collisionObjectFirstContactRegisterActive);

        // Parallel processing of collider contact endings
        Parallel.ForEach(colliderFirstContactRegisterActive, kvp =>
        {
            var self = kvp.Key;
            var resolver = self.Parent;
            if(resolver == null) return;
            var others = kvp.Value;
            if(others.Count <= 0) return;
            foreach (var other in others)
            {
                resolver.ResolveColliderContactEnded(self, other);
            }
        });
        colliderFirstContactRegisterActive.Clear();
        (colliderFirstContactRegisterActive, colliderFirstContactRegisterTemp) = (colliderFirstContactRegisterTemp, colliderFirstContactRegisterActive);
    }
    
    /// <summary>
    /// Merge all entries from <paramref name="source"/> into <paramref name="dest"/>.
    /// For each key in <paramref name="source"/>, this method adds every associated second-entry
    /// to the destination stack.
    /// </summary>
    /// <typeparam name="T1">Type of the first key (reference type).</typeparam>
    /// <typeparam name="T2">Type of the second key (reference type).</typeparam>
    /// <param name="source">Source <see cref="FirstContactStack{T1,T2}"/> to copy entries from.</param>
    /// <param name="dest">Destination <see cref="FirstContactStack{T1,T2}"/> to receive entries.</param>
    /// <remarks>
    /// Existing entries in <paramref name="dest"/> are preserved; duplicate handling is delegated
    /// to the <see cref="FirstContactStack{T1,T2}"/> implementation.
    /// </remarks>
    private void MergeFirstContacts<T1, T2>(FirstContactStack<T1, T2> source, FirstContactStack<T1, T2> dest) where T1 : class where T2 : class
    {
        foreach (var kvp in source)
        {
            foreach (var second in kvp.Value)
                dest.AddEntry(kvp.Key, second);
        }
    }
    #endregion
    
    #region Sequential

    /// <summary>
    /// Sequentially processes all registered collision objects and populates the collision stack.
    /// </summary>
    /// <param name="dt">Elapsed time in seconds since the last update.</param>
    /// <remarks>
    /// This is the non-parallel processing path used when <c>ParallelProcessing</c> is <c>false</c>.
    /// For each enabled <see cref="CollisionObject"/> the method calls <see cref="ProcessCollisionObject"/>
    /// to detect collisions and add resulting <see cref="CollisionRegister"/> instances to the
    /// <see cref="collisionStack"/>.
    /// </remarks>
    private void ProcessCollisionsSequential(float dt)
    {
        foreach (var collisionBody in collisionBodyRegister.AllObjects)
        {
            if (!collisionBody.Enabled || !collisionBody.HasColliders) continue;
            CollisionRegister? collisionRegister = null;
            ProcessCollisionObject(collisionBody, dt, ref collisionRegister);
            if (collisionRegister != null)
                collisionStack.AddCollisionRegister(collisionBody, collisionRegister);
        }
    }
    
    /// <summary>
    /// Processes a single <see cref="CollisionObject"/> in the sequential collision processing path.
    /// </summary>
    /// <param name="collisionBody">The collision object whose colliders will be checked against candidates.</param>
    /// <param name="dt">Delta time in seconds since the last update. Used for shape projection when <see cref="CollisionObject.ProjectShape"/> is enabled.</param>
    /// <param name="collisionRegister">
    /// Reference to an optional <see cref="CollisionRegister"/> that will be created and populated when collisions are detected.
    /// If no collisions are found the value remains <c>null</c>.
    /// </param>
    /// <remarks>
    /// - Iterates through each enabled <see cref="Collider"/> of <paramref name="collisionBody"/>.
    /// - Uses the <see cref="IBroadphase"/> to obtain candidate buckets and a per-handler check register to avoid duplicate candidate checks.
    /// - Supports projected shapes (for continuous motion) when <see cref="CollisionObject.ProjectShape"/> is true.
    /// - Determines overlap and optionally computes intersection points. If shapes overlap but no intersection points are returned,
    ///   a fallback closest-point on the candidate is added so collidable-inside-collidable cases yield at least one contact point.
    /// - Manages first-contact bookkeeping using the handler-level first-contact registers:
    ///   collision object first-contact and collider first-contact registers are updated to compute "first contact" flags.
    /// - This method does not perform collision resolution; it only fills <paramref name="collisionRegister"/> with detected collisions.
    /// </remarks>
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
    
    /// <summary>
    /// Resolves collisions in sequential mdoe after detection by processing queued collision data and notifying
    /// collision objects and colliders about contact end events.
    /// </summary>
    /// <remarks>
    /// This method:
    /// - Applies pending changes in the collision body register.
    /// - Processes and clears the collision stack (performs resolution).
    /// - Iterates active first-contact registers and calls the appropriate
    ///   ResolveContactEnded / ResolveColliderContactEnded callbacks for ended contacts.
    /// - Swaps and clears active/temp first-contact registers to prepare for the next update.
    /// </remarks>
    private void ResolveSequential()
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

