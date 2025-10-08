using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;


/// <summary>
/// Handles collision detection, resolution, and spatial queries for registered <see cref="CollisionObject"/> instances.
/// Implements <see cref="IBounds"/> to provide bounding information for the collision system.
/// </summary>
/// <remarks>
/// This class manages the registration, update, and removal of collision objects,
/// and provides methods for collision queries and spatial operations.
/// </remarks>
public partial class CollisionHandler : IBounds
{
    #region Members
    /// <summary>
    /// Gets the number of registered <see cref="CollisionObject"/> instances in the collision system.
    /// </summary>
    public int Count => collisionBodyRegister.AllObjects.Count;

    /// <summary>
    /// Gets the bounding rectangle of the collision system.
    /// </summary>
    public Rect Bounds => broadphaseSpatialHash.Bounds;
    
    private readonly CollisionObjectRegister collisionBodyRegister;
    
    //TODO: Has IBroadphase now
    private readonly BroadphaseSpatialHash broadphaseSpatialHash;
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

    //TODO: Constructor now needs IBroadphase parameter instead of all spatial hash parameters
    
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
        broadphaseSpatialHash = new(x, y, w, h, rows, cols);
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
        broadphaseSpatialHash = new(bounds.X, bounds.Y, bounds.Width, bounds.Height, rows, cols);
        
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
    public void ResizeBounds(Rect newBounds) => broadphaseSpatialHash.ResizeBounds(newBounds);

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
        broadphaseSpatialHash.Close();
        
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
        broadphaseSpatialHash.Fill(collisionBodyRegister.AllObjects);

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
                    broadphaseSpatialHash.GetCandidateBuckets(projected, ref collisionCandidateBuckets);
                    
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
                    broadphaseSpatialHash.GetCandidateBuckets(collider, ref collisionCandidateBuckets, true);
                    
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

    #region Debug
    
    /// <summary>
    /// Draws debug information for the spatial hash using the specified border and fill colors.
    /// </summary>
    /// <param name="border">The color to use for the border.</param>
    /// <param name="fill">The color to use for the fill.</param>
    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
        broadphaseSpatialHash.DebugDraw(border, fill);
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

