using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.GameDef;
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

namespace ShapeEngine.Geometry.CollisionSystem;


/// <summary>
/// Implements a dynamic spatial hash broadphase for collision detection.
/// Efficiently manages colliders in spatial buckets for fast broadphase queries.
/// Only creates and stores buckets that contain colliders, optimizing memory usage.
/// Old buckets that become empty are recycled for future use.
/// Does not maintain persistent bounds; bounds are recalculated each frame based on added colliders.
/// </summary>
public class BroadphaseDynamicSpatialHash : IBroadphase
{
    #region Members
    /// <summary>
    /// Stores the current bounds of all colliders added to the spatial hash for this frame.
    /// Calculated dynamically from the colliders and used for broadphase queries and debug drawing.
    /// </summary>
    private Rect currentBounds = new();//calculated from added colliders
    
    /// <summary>
    /// Register for tracking colliders and their associated <see cref="BroadphaseBucket"/>s during the current frame.
    /// Used to efficiently manage which buckets each collider is assigned to.
    /// </summary>
    private readonly BroadphaseColliderRegister<BroadphaseBucket> register = new();
    
    /// <summary>
    /// Register for static colliders, mapping each collider to its associated spatial hash coordinates.
    /// Used to efficiently track and update static collider positions within the spatial hash.
    /// </summary>
    private readonly BroadphaseStaticColliderRegister<Coordinates> staticRegister = new();
    
    /// <summary>
    /// Maps spatial hash <see cref="Coordinates"/> to their corresponding <see cref="BroadphaseBucket"/>.
    /// Only buckets containing colliders are stored, optimizing memory usage.
    /// </summary>
    private readonly Dictionary<Coordinates, BroadphaseBucket> buckets = new();
    
    /// <summary>
    /// Stores buckets that are currently available for reuse.
    /// Helps minimize allocations by recycling empty buckets instead of creating new ones.
    /// </summary>
    private readonly HashSet<BroadphaseBucket> availableBuckets = []; //could be a queue or stack as well
    
    /// <summary>
    /// Tracks buckets that were used but are now empty, mapping each <see cref="BroadphaseBucket"/> to its <see cref="Coordinates"/>.
    /// Used for efficient recycling and removal of unused buckets.
    /// </summary>
    private readonly Dictionary<BroadphaseBucket, Coordinates> emptyUsedBuckets = [];
    
    /// <summary>
    /// The maximum number of buckets allowed in the spatial hash.
    /// Controls memory usage and bucket creation limits.
    /// 0 or less means unlimited buckets.
    /// </summary>
    private readonly int maxBuckets;
    /// <summary>
    /// The size of each spatial hash bucket, defined by width and height.
    /// Determines the granularity of spatial partitioning for collision detection.
    /// </summary>
    /// <remarks>
    /// Smaller buckets lead to less objects per bucket but more buckets overall.
    /// </remarks>
    private readonly Size bucketSize;
    /// <summary>
    /// Tracks the total number of buckets created by this instance.
    /// Used to enforce the <see cref="maxBuckets"/> limit and manage bucket recycling.
    /// </summary>
    private int createdBuckets = 0;
    
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of <see cref="BroadphaseDynamicSpatialHash"/> with the specified bucket width, height, and maximum number of buckets.
    /// </summary>
    /// <param name="bucketWidth">The width of each spatial hash bucket.</param>
    /// <param name="bucketHeight">The height of each spatial hash bucket.</param>
    /// <param name="maxBuckets">The maximum number of buckets allowed. 0 or less means unlimited.</param>
    public BroadphaseDynamicSpatialHash(float bucketWidth, float bucketHeight, int maxBuckets)
    {
        this.maxBuckets = maxBuckets;
        bucketSize = new Size(bucketWidth, bucketHeight);
    }
    /// <summary>
    /// Initializes a new instance of <see cref="BroadphaseDynamicSpatialHash"/> with the specified bucket size and maximum number of buckets.
    /// </summary>
    /// <param name="bucketSize">The size of each spatial hash bucket.</param>
    /// <param name="maxBuckets">The maximum number of buckets allowed. 0 or less means unlimited.</param>
    public BroadphaseDynamicSpatialHash(Size bucketSize, int maxBuckets)
    {
        this.maxBuckets = maxBuckets;
        this.bucketSize = bucketSize;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="BroadphaseDynamicSpatialHash"/> with a square bucket size and a maximum number of buckets.
    /// </summary>
    /// <param name="bucketSize">The width and height of each spatial hash bucket.</param>
    /// <param name="maxBuckets">The maximum number of buckets allowed. 0 or less means unlimited.</param>
    public BroadphaseDynamicSpatialHash(float bucketSize, int maxBuckets)
    {
        this.maxBuckets = maxBuckets;
        this.bucketSize = new Size(bucketSize, bucketSize);
    }
    /// <summary>
    /// Initializes a new instance of <see cref="BroadphaseDynamicSpatialHash"/> with the specified bucket size.
    /// Sets <c>maxBuckets</c> to unlimited (0).
    /// </summary>
    /// <param name="bucketSize">The size of each spatial hash bucket.</param>
    public BroadphaseDynamicSpatialHash(Size bucketSize)
    {
        maxBuckets = 0;
        this.bucketSize = bucketSize;
    }

    #endregion
    
    #region Public methods
    /// <summary>
    /// Populates the spatial hash with the provided collection of <see cref="CollisionObject"/>s.
    /// Clears previous state, recalculates bounds, and assigns colliders to buckets for broadphase collision detection.
    /// </summary>
    /// <param name="collisionBodies">Enumerable of collision objects to add to the spatial hash for this frame.</param>
    public void Fill(IEnumerable<CollisionObject> collisionBodies)
    {
        Clear();
        
        currentBounds = new();
        foreach (var body in collisionBodies)
        {
            if (!body.Enabled || !body.HasColliders) continue;
            foreach (var collider in body.Colliders)
            {
                AddCollider(collider, body.MotionType);
            }   
        }
        
        register.Clean();
        staticRegister.Clean();
    }
    /// <summary>
    /// Releases all resources used by the spatial hash, clearing all buckets, registers, and bounds.
    /// Resets the internal state for reuse or disposal.
    /// </summary>
    public void Close()
    {
        register.Close();
        staticRegister.Close();
        buckets.Clear();
        availableBuckets.Clear();
        emptyUsedBuckets.Clear();
        createdBuckets = 0;
        currentBounds = new();
    }
    /// <summary>
    /// Gets the current bounds of all colliders in the spatial hash for this frame.
    /// </summary>
    public Rect GetBounds() => currentBounds;
    /// <summary>
    /// This method is currently not implemented and does not modify internal state.
    /// Bounds are calculated dynamically each frame.
    /// </summary>
    /// <param name="targetBounds">The bounds to set for the spatial hash.</param>
    public void SetBounds(Rect targetBounds) { }
    /// <summary>
    /// Determines whether the current bounds of the spatial hash are valid.
    /// Returns true if both width and height are greater than zero.
    /// </summary>
    public bool HasValidBounds()
    {
        if (currentBounds.Width <= 0) return false;
        return currentBounds.Height > 0;
    }
    /// <summary>
    /// Draws debug visualization for the spatial hash.
    /// Renders the current bounds and all buckets, using specified border and fill colors.
    /// Empty buckets are drawn with reduced alpha.
    /// </summary>
    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
        var borderLineThickness = currentBounds.Size.Max() * 0.0025f;
        if (borderLineThickness <= 1f) borderLineThickness = 1f;
        currentBounds.DrawLines(borderLineThickness, border);
        
        var lineThickness = bucketSize.Max() * 0.0025f;
        if(lineThickness <= 0.5f) lineThickness = 0.5f;
        
        foreach (var kvp in buckets)
        {
            var coords = kvp.Key;
            var bucket = kvp.Value;
            var rect = GetBucketBounds(coords);
            if (bucket.Count == 0)
            {
                rect.Draw(fill.ChangeAlpha(100));
                rect.DrawLines(lineThickness, border.ChangeAlpha(100));
            }
            else
            {
                rect.Draw(fill);
                rect.DrawLines(lineThickness, border);
            }
            
        }
    }
    #endregion
    
    #region Private methods
    /// <summary>
    /// Clears and resets the spatial hash for a new frame.
    /// Recycles empty buckets, removes unused buckets, and prepares all buckets for reuse.
    /// </summary>
    private void Clear()
    {
        foreach (var kvp in emptyUsedBuckets)
        {
            buckets.Remove(kvp.Value);
            availableBuckets.Add(kvp.Key);
        }
        emptyUsedBuckets.Clear();
        
        foreach (var kvp in buckets)
        {
            var coords = kvp.Key;
            var bucket = kvp.Value;
            //was not used this frame, remove from structure
            if (bucket.Count == 0) //should not happen, but just in case (emptyUsedBuckets should clear all of them out before)
            {
                availableBuckets.Add(bucket);
                buckets.Remove(coords);
            }
            else
            {
                bucket.Clear();
                emptyUsedBuckets.Add(bucket, coords);
            }
        }
    }
    /// <summary>
    /// Adds a <see cref="Collider"/> to the spatial hash, assigning it to the appropriate buckets based on its position and motion type.
    /// Handles both static and dynamic colliders, updating internal registers and bounds accordingly.
    /// </summary>
    /// <param name="collider">The collider to add.</param>
    /// <param name="motionType">The motion type of the collider (static or dynamic).</param>
    private void AddCollider(Collider collider, MotionType motionType)
    {
        if (!collider.Enabled) return;
        
        HashSet<Coordinates>? staticColliderCoords = null;
        if (motionType == MotionType.Static)
        {
            var entryExists = staticRegister.AddEntry(collider, 1, out var staticSet, out var staticBounds);
            if (entryExists)
            {
                var resultSet = register.AddEntry(collider, staticSet.Count);
                if(resultSet == null) return; //already added this frame
                foreach (var coords in staticSet)
                {
                    AddColliderToBucket(collider, coords, ref resultSet);
                }
        
                currentBounds = currentBounds.Union(staticBounds);
                return;
            }
            
            staticColliderCoords = staticSet;
        }
        
        int bucketCount;
        int minX, maxX, minY, maxY;
        
        if (collider.BroadphaseType == BroadphaseType.Point)
        {
            var position = collider.CurTransform.Position;
            minX = maxX = (int)Math.Floor(position.X / bucketSize.Width);
            minY = maxY = (int)Math.Floor(position.Y / bucketSize.Height);
            bucketCount = 1;

            if (staticColliderCoords != null)
            {
                var staticBounds = new Rect(position, new Size(1, 1), AnchorPoint.Center);
                staticRegister.UpdateRect(collider, staticBounds);
            }
        }
        else
        {
            var boundingBox = collider.GetBoundingBox();
            minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
            maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
            minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
            maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);
            bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
            
            if (staticColliderCoords != null)
            {
                staticRegister.UpdateRect(collider, boundingBox);
            }
        }
        
        var registerSet = register.AddEntry(collider, bucketCount);
        if(registerSet == null) return; //already added this frame
        
        if (bucketCount > 1)
        {
            var tlBucketRect = GetBucketBounds(new Coordinates(minX, minY));
            var brBucketRect = GetBucketBounds(new Coordinates(maxX, maxY));
            var totalBucketsRect = new Rect(tlBucketRect.TopLeft, brBucketRect.BottomRight);
            currentBounds = currentBounds.Union(totalBucketsRect);
        }
        else
        {
            currentBounds = currentBounds.Union(GetBucketBounds(new Coordinates(minX, minY)));
        }
        
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                var coords = new Coordinates(x, y);
                
                //collider does not overlap with this bucket - only checked with more than 4 buckets to save performance
                if (bucketCount > 4 && collider.BroadphaseType == BroadphaseType.FullShape && !OverlapsColliderShapeWithBucketRect(coords, collider)) continue;

                staticColliderCoords?.Add(coords);

                AddColliderToBucket(collider, coords, ref registerSet);
            }
        }

    }
    /// <summary>
    /// Adds the specified <see cref="Collider"/> to the bucket at the given <see cref="Coordinates"/>.
    /// Updates the <paramref name="registerSet"/> with the bucket reference.
    /// Handles bucket creation, recycling, and memory management.
    /// </summary>
    /// <param name="collider">The collider to add to the bucket.</param>
    /// <param name="coords">The spatial hash coordinates of the target bucket.</param>
    /// <param name="registerSet">Reference to the set tracking buckets for this collider.</param>
    private void AddColliderToBucket(Collider collider, Coordinates coords, ref HashSet<BroadphaseBucket> registerSet)
    {
        if (buckets.TryGetValue(coords, out var bucket))//bucket already exists
        {
            if(bucket.Count == 0) emptyUsedBuckets.Remove(bucket);//no longer empty
            bucket.Add(collider);
            registerSet.Add(bucket);
        }
        else //bucket does not exist yet
        {
            if (availableBuckets.Count > 0)
            {
                bucket = availableBuckets.First();
                availableBuckets.Remove(bucket);
            }
            else
            {
                if (maxBuckets <= 0)
                {
                    if (emptyUsedBuckets.Count > 0)
                    {
                        var kvp = emptyUsedBuckets.First();
                        bucket = kvp.Key;
                        buckets.Remove(kvp.Value);
                        emptyUsedBuckets.Remove(bucket);
                    }
                    else
                    {
                        //create a new bucket
                        bucket = [];
                        createdBuckets++;
                    }
                }
                else
                {
                    if (createdBuckets >= maxBuckets)
                    {
                        if (emptyUsedBuckets.Count > 0)
                        {
                            var kvp = emptyUsedBuckets.First();
                            bucket = kvp.Key;
                            buckets.Remove(kvp.Value);
                            emptyUsedBuckets.Remove(bucket);
                        }
                        else
                        {
                            Game.Instance.Logger.LogWarning($"BroadphaseDynamicSpatialHash: Max bucket count of {maxBuckets} reached and no empty used buckets available! Cannot add new bucket for collider {collider}.");
                            return;
                        }
                    }
                    else
                    {
                        //create a new bucket
                        bucket = [];
                        createdBuckets++;
                    }
                }
            }
            
            bucket.Add(collider);
            buckets[coords] = bucket;
            registerSet.Add(bucket);
        }
    }
    /// <summary>
    /// Checks if the specified <see cref="Collider"/> overlaps with the bounds of the bucket at the given <see cref="Coordinates"/>.
    /// Used to optimize bucket assignment for colliders with <c>BroadphaseType.FullShape</c>.
    /// </summary>
    /// <param name="coords">The spatial hash coordinates of the bucket.</param>
    /// <param name="collider">The collider to test for overlap.</param>
    /// <returns><c>true</c> if the collider overlaps the bucket bounds; otherwise, <c>false</c>.</returns>
    private bool OverlapsColliderShapeWithBucketRect(Coordinates coords, Collider collider)
    {
        var bucketRect = GetBucketBounds(coords);
        return collider.Overlap(bucketRect);
    }
    /// <summary>
    /// Calculates and returns the bounds of the bucket at the specified spatial hash <paramref name="coords"/>.
    /// The bounds are determined by multiplying the coordinates by the bucket size.
    /// </summary>
    /// <param name="coords">The spatial hash coordinates of the bucket.</param>
    /// <returns>A <see cref="Rect"/> representing the bounds of the bucket.</returns>
    private Rect GetBucketBounds(Coordinates coords)
    {
        return new Rect(coords.X * bucketSize.Width, coords.Y * bucketSize.Height, bucketSize.Width, bucketSize.Height);
    }
    #endregion
    
    #region GetCandidateBuckets methods
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="CollisionObject"/>.
    /// Iterates through each collider in the object and accumulates their candidate buckets into the provided list.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="collisionObject">The collision object whose colliders are to be checked.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for all colliders in the object.</returns>
    public int GetCandidateBuckets(CollisionObject collisionObject, ref List<BroadphaseBucket> candidateBuckets)
    {
        var count = 0;
        
        foreach (var collider in collisionObject.Colliders)
        {
            count += GetCandidateBuckets(collider, ref candidateBuckets);
        }

        return count;
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain the specified <see cref="Collider"/>.
    /// Uses the internal register if available, otherwise falls back to shape-based queries.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="collider">The collider to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for the collider.</returns>
    public int GetCandidateBuckets(Collider collider, ref List<BroadphaseBucket> candidateBuckets)
    {
        if (!collider.Enabled) return 0;

        if (register.TryGetEntry(collider, out var registerBuckets) && registerBuckets != null)
        {
            foreach (var bucket in registerBuckets)
            {
                candidateBuckets.Add(bucket);
            }
            return registerBuckets.Count;   
        }

        if (collider.BroadphaseType == BroadphaseType.Point)
        {
            return GetCandidateBuckets(collider.CurTransform.Position, ref candidateBuckets);
        }
        
        bool testFullShape = collider.BroadphaseType == BroadphaseType.FullShape;
        
        switch (collider.GetShapeType())
        {
            case ShapeType.Circle: return  GetCandidateBucketsExtended(collider.GetCircleShape(), ref candidateBuckets, testFullShape);
            case ShapeType.Segment: return  GetCandidateBucketsExtended(collider.GetSegmentShape(), ref candidateBuckets, testFullShape);
            case ShapeType.Line: return  GetCandidateBucketsExtended(collider.GetLineShape(), ref candidateBuckets, testFullShape);
            case ShapeType.Ray: return  GetCandidateBucketsExtended(collider.GetRayShape(), ref candidateBuckets, testFullShape);
            case ShapeType.Triangle: return  GetCandidateBucketsExtended(collider.GetTriangleShape(), ref candidateBuckets, testFullShape);
            case ShapeType.Rect: return  GetCandidateBuckets(collider.GetRectShape(), ref candidateBuckets);
            case ShapeType.Quad: return  GetCandidateBucketsExtended(collider.GetQuadShape(), ref candidateBuckets, testFullShape);
            case ShapeType.Poly: return  GetCandidateBucketsExtended(collider.GetPolygonShape(), ref candidateBuckets, testFullShape);
            case ShapeType.PolyLine: return  GetCandidateBucketsExtended(collider.GetPolylineShape(), ref candidateBuckets, testFullShape);
        }

        return 0;
    }
    /// <summary>
    /// Retrieves the candidate bucket containing the specified point.
    /// Calculates the spatial hash coordinates for the point and adds the corresponding bucket to the candidate list if it exists and is not empty.
    /// Returns the number of buckets added (0 or 1).
    /// </summary>
    /// <param name="point">The point to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The number of candidate buckets found (0 or 1).</returns>
    public int GetCandidateBuckets(Vector2 point, ref List<BroadphaseBucket> candidateBuckets)
    {
        int added = 0;
        
        var coordinateX = (int)Math.Floor(point.X / bucketSize.Width);
        var coordinateY = (int)Math.Floor(point.Y / bucketSize.Height);
        var coordinates = new Coordinates(coordinateX, coordinateY);
        
        if (buckets.TryGetValue(coordinates, out var bucket) && bucket.Count > 0)
        {
            candidateBuckets.Add(bucket);
            added++;
        }

        return added;
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Triangle"/>.
    /// Uses extended shape-based queries to accumulate candidate buckets into the provided list.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="triangle">The triangle shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for the triangle.</returns>
    public int GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets)
    {
        return GetCandidateBucketsExtended(triangle, ref candidateBuckets, true);
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Rect"/>.
    /// Iterates over all buckets overlapping the rectangle and adds non-empty buckets to the provided list.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="rect">The rectangle shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for the rectangle.</returns>
    public int GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets)
    {
        var minX = (int)Math.Floor(rect.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(rect.Right / bucketSize.Width);
        var minY = (int)Math.Floor(rect.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(rect.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coordinates(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coordinates(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                    
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Quad"/>.
    /// Uses extended shape-based queries to accumulate candidate buckets into the provided list.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="quad">The quad shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for the quad.</returns>
    public int GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets)
    {
        return GetCandidateBucketsExtended(quad, ref candidateBuckets, true);
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Polygon"/>.
    /// Uses extended shape-based queries to accumulate candidate buckets into the provided list.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="poly">The polygon shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for the polygon.</returns>
    public int GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets)
    {
        return GetCandidateBucketsExtended(poly, ref candidateBuckets, true);
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Polyline"/>.
    /// Uses extended shape-based queries to accumulate candidate buckets into the provided list.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="polyLine">The polyline shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for the polyline.</returns>
    public int GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets)
    {
        return GetCandidateBucketsExtended(polyLine, ref candidateBuckets, true);
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Segment"/>.
    /// Uses extended shape-based queries to accumulate candidate buckets into the provided list.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="segment">The segment shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for the segment.</returns>
    public int GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets)
    {
        return GetCandidateBucketsExtended(segment, ref candidateBuckets, true);
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Line"/>.
    /// Uses extended shape-based queries to accumulate candidate buckets into the provided list.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="line">The line shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for the line.</returns>
    public int GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets)
    {
        return GetCandidateBucketsExtended(line, ref candidateBuckets, true);
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Ray"/>.
    /// Uses extended shape-based queries to accumulate candidate buckets into the provided list.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="ray">The ray shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for the ray.</returns>
    public int GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets)
    {
        return GetCandidateBucketsExtended(ray, ref candidateBuckets, true);
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Circle"/>.
    /// Uses extended shape-based queries to accumulate candidate buckets into the provided list.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="circle">The circle shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <returns>The total number of candidate buckets found for the circle.</returns>
    public int GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets)
    {
        return GetCandidateBucketsExtended(circle, ref candidateBuckets, true);
    }
    
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Segment"/>.
    /// Iterates over all buckets overlapping the segment's bounding box and adds non-empty buckets to the provided list.
    /// If <paramref name="testFullShape"/> is true, only buckets whose bounds overlap the segment are included.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="segment">The segment shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <param name="testFullShape">Whether to test for full shape overlap with bucket bounds.</param>
    /// <returns>The total number of candidate buckets found for the segment.</returns>
    private int GetCandidateBucketsExtended(Segment segment, ref List<BroadphaseBucket> candidateBuckets, bool testFullShape = true)
    {
        var boundingBox = segment.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coordinates(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coordinates(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;

                    if (testFullShape)
                    {
                        var bucketRect = GetBucketBounds(coords);
                        if (!segment.OverlapShape(bucketRect)) continue;
                    }
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Line"/>.
    /// Iterates over all buckets overlapping the line's bounding box and adds non-empty buckets to the provided list.
    /// If <paramref name="testFullShape"/> is true, only buckets whose bounds overlap the line are included.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="line">The line shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <param name="testFullShape">Whether to test for full shape overlap with bucket bounds.</param>
    /// <returns>The total number of candidate buckets found for the line.</returns>
    private int GetCandidateBucketsExtended(Line line, ref List<BroadphaseBucket> candidateBuckets, bool testFullShape = true)
    {
        var boundingBox = line.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coordinates(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coordinates(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                    
                    if (testFullShape)
                    {
                        var bucketRect = GetBucketBounds(coords);
                        if (!line.OverlapShape(bucketRect)) continue; 
                    }
                    
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Ray"/>.
    /// Iterates over all buckets overlapping the ray's bounding box and adds non-empty buckets to the provided list.
    /// If <paramref name="testFullShape"/> is true, only buckets whose bounds overlap the ray are included.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="ray">The ray shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <param name="testFullShape">Whether to test for full shape overlap with bucket bounds.</param>
    /// <returns>The total number of candidate buckets found for the ray.</returns>
    private int GetCandidateBucketsExtended(Ray ray, ref List<BroadphaseBucket> candidateBuckets, bool testFullShape = true)
    {
        var boundingBox = ray.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coordinates(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coordinates(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    if (testFullShape)
                    {
                        var bucketRect = GetBucketBounds(coords);
                        if (!ray.OverlapShape(bucketRect)) continue;
                    }
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Circle"/>.
    /// Iterates over all buckets overlapping the circle's bounding box and adds non-empty buckets to the provided list.
    /// If <paramref name="testFullShape"/> is true, only buckets whose bounds overlap the circle are included.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="circle">The circle shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <param name="testFullShape">Whether to test for full shape overlap with bucket bounds.</param>
    /// <returns>The total number of candidate buckets found for the circle.</returns>
    private int GetCandidateBucketsExtended(Circle circle, ref List<BroadphaseBucket> candidateBuckets, bool testFullShape = true)
    {
        var boundingBox = circle.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coordinates(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coordinates(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    if (testFullShape)
                    {
                        var bucketRect = GetBucketBounds(coords);
                        if (!circle.OverlapShape(bucketRect)) continue;
                    }
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Triangle"/>.
    /// Iterates over all buckets overlapping the triangle's bounding box and adds non-empty buckets to the provided list.
    /// If <paramref name="testFullShape"/> is true, only buckets whose bounds overlap the triangle are included.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="triangle">The triangle shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <param name="testFullShape">Whether to test for full shape overlap with bucket bounds.</param>
    /// <returns>The total number of candidate buckets found for the triangle.</returns>
    private int GetCandidateBucketsExtended(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets, bool testFullShape = true)
    {
        var boundingBox = triangle.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coordinates(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coordinates(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    if (testFullShape)
                    {
                        var bucketRect = GetBucketBounds(coords);
                        if (!triangle.OverlapShape(bucketRect)) continue;
                    }
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Quad"/>.
    /// Iterates over all buckets overlapping the quad's bounding box and adds non-empty buckets to the provided list.
    /// If <paramref name="testFullShape"/> is true, only buckets whose bounds overlap the quad are included.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="quad">The quad shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <param name="testFullShape">Whether to test for full shape overlap with bucket bounds.</param>
    /// <returns>The total number of candidate buckets found for the quad.</returns>
    private int GetCandidateBucketsExtended(Quad quad, ref List<BroadphaseBucket> candidateBuckets, bool testFullShape = true)
    {
        var boundingBox = quad.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coordinates(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coordinates(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    if (testFullShape)
                    {
                        var bucketRect = GetBucketBounds(coords);
                        if (!quad.OverlapShape(bucketRect)) continue;
                    }
                    
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Polygon"/>.
    /// Iterates over all buckets overlapping the polygon's bounding box and adds non-empty buckets to the provided list.
    /// If <paramref name="testFullShape"/> is true, only buckets whose bounds overlap the polygon are included.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="poly">The polygon shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <param name="testFullShape">Whether to test for full shape overlap with bucket bounds.</param>
    /// <returns>The total number of candidate buckets found for the polygon.</returns>
    private int GetCandidateBucketsExtended(Polygon poly, ref List<BroadphaseBucket> candidateBuckets, bool testFullShape = true)
    {
        var boundingBox = poly.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coordinates(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coordinates(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    if (testFullShape)
                    {
                        var bucketRect = GetBucketBounds(coords);
                        if (!poly.OverlapShape(bucketRect)) continue;
                    }
                
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }
    /// <summary>
    /// Retrieves all candidate buckets that may contain colliders for the given <see cref="Polyline"/>.
    /// Iterates over all buckets overlapping the polyline's bounding box and adds non-empty buckets to the provided list.
    /// If <paramref name="testFullShape"/> is true, only buckets whose bounds overlap the polyline are included.
    /// Returns the total number of candidate buckets found.
    /// </summary>
    /// <param name="polyLine">The polyline shape to check for candidate buckets.</param>
    /// <param name="candidateBuckets">A reference to the list where candidate buckets will be added.</param>
    /// <param name="testFullShape">Whether to test for full shape overlap with bucket bounds.</param>
    /// <returns>The total number of candidate buckets found for the polyline.</returns>
    private int GetCandidateBucketsExtended(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets, bool testFullShape = true)
    {
        var boundingBox = polyLine.GetBoundingBox();
        
        var minX = (int)Math.Floor(boundingBox.Left / bucketSize.Width);
        var maxX = (int)Math.Floor(boundingBox.Right / bucketSize.Width);
        var minY = (int)Math.Floor(boundingBox.Top / bucketSize.Height);
        var maxY = (int)Math.Floor(boundingBox.Bottom / bucketSize.Height);

        int bucketCount = (1 + maxX - minX) * (1 + maxY - minY);
        if (bucketCount <= 0) return 0;
        
        int added = 0;
        if (bucketCount == 1)
        {
            var coords = new Coordinates(minX, minY);
            if (buckets.TryGetValue(coords, out var bucket) && bucket.Count > 0)
            {
                candidateBuckets.Add(bucket);
                added++;
            }
        }
        else
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var coords = new Coordinates(x, y);
                    if (!buckets.TryGetValue(coords, out var bucket)) continue;
                    if(bucket.Count <= 0) continue;
                
                    if (testFullShape)
                    {
                        var bucketRect = GetBucketBounds(coords);
                        if (!polyLine.OverlapShape(bucketRect)) continue;
                    }
                    
                    candidateBuckets.Add(bucket);
                    added++;
                }
            }
        }

        return added;
    }
    
    #endregion
}