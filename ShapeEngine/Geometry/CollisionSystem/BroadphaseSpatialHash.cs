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

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Implements a spatial hash grid for efficient broad-phase collision detection and spatial queries.
/// </summary>
/// <remarks>
/// The spatial hash divides a 2D space into a grid of buckets, each containing colliders that overlap its region.
/// Provides fast lookup for collision candidates and supports dynamic resizing and grid changes.
/// </remarks>
public class BroadphaseSpatialHash : IBroadphase
{
    #region Public Members
    /// <summary>
    /// Gets the bounds of the spatial hash grid.
    /// </summary>
    public Rect Bounds { get; private set; }
    /// <summary>
    /// Gets the width of each grid cell.
    /// </summary>
    public float SpacingX { get; private set; }
    /// <summary>
    /// Gets the height of each grid cell.
    /// </summary>
    public float SpacingY { get; private set; }
    /// <summary>
    /// Gets the total number of buckets in the grid.
    /// </summary>
    public int BucketCount { get; private set; }
    /// <summary>
    /// Gets the number of rows in the grid.
    /// </summary>
    public int Rows { get; private set; }
    /// <summary>
    /// Gets the number of columns in the grid.
    /// </summary>
    public int Cols { get; private set; }
    #endregion

    #region Private Members
    /// <summary>
    /// Array of buckets representing the spatial hash grid cells.
    /// Each bucket stores colliders that overlap its region.
    /// </summary>
    private BroadphaseBucket[] buckets;
    /// <summary>
    /// Register for tracking colliders and their associated bucket IDs in the spatial hash.
    /// Used for efficient lookup and management of dynamic colliders.
    /// </summary>
    private readonly BroadphaseColliderRegister<int> register = new();
    /// <summary>
    /// Register for caching static colliders and their associated bucket IDs in the spatial hash.
    /// Used to optimize lookups for colliders that do not move.
    /// </summary>
    private readonly BroadphaseStaticColliderRegister<int> staticRegister = new();
    /// <summary>
    /// Indicates whether a bounds resize has been queued to be applied on the next clear.
    /// </summary>
    private bool boundsResizeQueued;
    /// <summary>
    /// Stores the new bounds to be applied to the spatial hash grid on the next clear operation.
    /// </summary>
    private Rect newBounds;
    /// <summary>
    /// Temporary holder for cell IDs (bucket indices) during spatial queries.
    /// Used to avoid repeated allocations when collecting candidate buckets or colliders.
    /// </summary>
    private HashSet<int> idHolder = new(128);
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="BroadphaseSpatialHash"/> class with explicit bounds and grid size.
    /// </summary>
    /// <param name="x">The X coordinate of the grid's top-left corner.</param>
    /// <param name="y">The Y coordinate of the grid's top-left corner.</param>
    /// <param name="w">The width of the grid.</param>
    /// <param name="h">The height of the grid.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    /// <param name="cols">The number of columns in the grid.</param>
    public BroadphaseSpatialHash(float x, float y, float w, float h, int rows, int cols)
    {
        this.Bounds = new(x, y, w, h);
        this.Rows = rows;
        this.Cols = cols;
        this.SetSpacing();
        this.BucketCount = rows * cols;
        this.buckets = new BroadphaseBucket[this.BucketCount];
        for (int i = 0; i < BucketCount; i++)
        {
            this.buckets[i] = new();
        }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="BroadphaseSpatialHash"/> class with a bounding rectangle and grid size.
    /// </summary>
    /// <param name="bounds">The bounding rectangle for the grid.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    /// <param name="cols">The number of columns in the grid.</param>
    public BroadphaseSpatialHash(Rect bounds, int rows, int cols)
    {
        this.Bounds = bounds;
        this.Rows = rows;
        this.Cols = cols;
        this.SetSpacing();
        this.BucketCount = rows * cols;
        this.buckets = new BroadphaseBucket[this.BucketCount];
        for (int i = 0; i < BucketCount; i++)
        {
            this.buckets[i] = new();
        }
    }
    #endregion
    
    #region Public

    public Rect GetBounds() => Bounds;

    /// <summary>
    /// Fills the spatial hash with all colliders from the provided collision objects.
    /// </summary>
    /// <param name="collisionBodies">The collection of collision objects to add.</param>
    public void Fill(IEnumerable<CollisionObject> collisionBodies)
    {
        Clear();

        foreach (var body in collisionBodies)
        {
            if (body.Enabled && body.HasColliders)
            {
                Add(body);
            }
        }
        
        register.Clean();
        staticRegister.Clean();
    }
    /// <summary>
    /// Clears all buckets and internal registers, releasing resources.
    /// </summary>
    public void Close()
    {
        Clear();
        register.Close();
        staticRegister.Close();
        buckets = [];
    }
    /// <summary>
    /// Queues a resize of the spatial hash bounds to the specified rectangle. The resize is applied on the next clear.
    /// </summary>
    /// <param name="targetBounds">The new bounds for the grid.</param>
    public void SetBounds(Rect targetBounds) 
    {
        newBounds = targetBounds;
        boundsResizeQueued = true;
    }

    /// <inheritdoc cref="IBounds.HasValidBounds"/>
    public bool HasValidBounds() => true;

    /// <summary>
    /// Changes the number of rows and columns in the grid. This operation clears all buckets.
    /// </summary>
    /// <param name="rows">The new number of rows.</param>
    /// <param name="cols">The new number of columns.</param>
    /// <remarks>
    /// Recalculates spacing, clears and removes all old buckets, and then creates new buckets according to <c>rows*cols</c>.
    /// </remarks>
    public void ChangeGrid(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        SetSpacing();
        BucketCount = rows * cols;
        buckets = new BroadphaseBucket[BucketCount];
        for (int i = 0; i < BucketCount; i++)
        {
            buckets[i] = new();
        }
    }


    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
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
    /// Gets all buckets that may contain colliders overlapping the given collider.
    /// </summary>
    /// <param name="collider">The collider to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Collider collider, ref List<BroadphaseBucket> candidateBuckets)
    {
        if (!collider.Enabled) return 0;
        int count = 0;
        
        if (register.TryGetEntry(collider, out var bucketIds) && bucketIds != null)
        {
            if (bucketIds.Count <= 0) return 0;
            foreach (int id in bucketIds)
            {
                var bucket = buckets[id];
                if (bucket.Count > 0)
                {
                    count++;
                    candidateBuckets.Add(buckets[id]);
                }
            }
            
            return count;
        }
        
        idHolder.Clear();
        GetCellIDs(collider, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }

    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given point.
    /// </summary>
    /// <param name="point">The world-space point to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Vector2 point, ref List<BroadphaseBucket> candidateBuckets)
    {
        idHolder.Clear();
        GetCellIDs(point, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }

    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given segment.
    /// </summary>
    /// <param name="segment">The segment to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets)
    {
        idHolder.Clear();
        GetCellIDs(segment, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }
    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given line.
    /// </summary>
    /// <param name="line">The line to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets)
    {
        idHolder.Clear();
        GetCellIDs(line, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }
    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given ray.
    /// </summary>
    /// <param name="ray">The ray to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets)
    {
        idHolder.Clear();
        GetCellIDs(ray, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }
    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given circle.
    /// </summary>
    /// <param name="circle">The circle to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets)
    {
        idHolder.Clear();
        GetCellIDs(circle, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }
    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given triangle.
    /// </summary>
    /// <param name="triangle">The triangle to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets)
    {
        idHolder.Clear();
        GetCellIDs(triangle, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }
    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets)
    {
        idHolder.Clear();
        GetCellIDs(rect, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }
    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given quad.
    /// </summary>
    /// <param name="quad">The quad to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets)
    {
        idHolder.Clear();
        GetCellIDs(quad, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }
    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given polygon.
    /// </summary>
    /// <param name="poly">The polygon to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets)
    {
        idHolder.Clear();
        GetCellIDs(poly, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }
    /// <summary>
    /// Gets all buckets that may contain colliders overlapping the given polyline.
    /// </summary>
    /// <param name="polyLine">The polyline to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets)
    {
        idHolder.Clear();
        GetCellIDs(polyLine, ref idHolder);
        return FillCandidateBuckets(idHolder, ref candidateBuckets);
    }
  
    /// <summary>
    /// Gets all unique colliders that may overlap the given collision object.
    /// </summary>
    /// <param name="collisionBody">The collision object to query.</param>
    /// <param name="candidates">A set to populate with unique colliders.</param>
    /// <returns>
    /// Returns the number of unique colliders added to the candidates set.
    /// </returns>
    public int GetUniqueCandidates(CollisionObject collisionBody, ref HashSet<Collider> candidates)
    {
        if (!collisionBody.HasColliders) return 0;
        var count = 0;
        foreach (var collider in collisionBody.Colliders)
        {
            count += GetUniqueCandidates(collider, ref candidates);
        }

        return count;
    }
    /// <summary>
    /// Gets all unique colliders that may overlap the given collider.
    /// </summary>
    /// <param name="collider">The collider to query.</param>
    /// <param name="candidates">A set to populate with unique colliders.</param>
    /// <returns>
    /// Returns the number of unique colliders added to the candidates set.
    /// </returns>
    public int GetUniqueCandidates(Collider collider, ref HashSet<Collider> candidates)
    {
        if (!collider.Enabled) return 0;
        if (register.TryGetEntry(collider, out var bucketIds) && bucketIds != null)
        {
            if (bucketIds.Count <= 0) return 0;
            int prevCount = candidates.Count;
            
            foreach (int id in bucketIds)
            {
                var bucket = buckets[id];
                if(bucket.Count > 0) candidates.UnionWith(bucket);
            }

            return candidates.Count - prevCount;
        }
        
        idHolder.Clear();
        GetCellIDs(collider, ref idHolder);
        return AccumulateUniqueCandidates(idHolder, ref candidates);
    }
    /// <summary>
    /// Gets all unique colliders that may overlap the given segment.
    /// </summary>
    /// <param name="segment">The segment to query.</param>
    /// <param name="candidates">A set to populate with unique colliders.</param>
    /// <returns>
    /// Returns the number of unique colliders added to the candidates set.
    /// </returns>
    public int GetUniqueCandidates(Segment segment, ref HashSet<Collider> candidates)
    {
        idHolder.Clear();
        GetCellIDs(segment, ref idHolder);
        return AccumulateUniqueCandidates(idHolder, ref candidates);
    }
    /// <summary>
    /// Gets all unique colliders that may overlap the given circle.
    /// </summary>
    /// <param name="circle">The circle to query.</param>
    /// <param name="candidates">A set to populate with unique colliders.</param>
    /// <returns>
    /// Returns the number of unique colliders added to the candidates set.
    /// </returns>
    public int GetUniqueCandidates(Circle circle, ref HashSet<Collider> candidates)
    {
        idHolder.Clear();
        GetCellIDs(circle, ref idHolder);
        return AccumulateUniqueCandidates(idHolder, ref candidates);
    }
    /// <summary>
    /// Gets all unique colliders that may overlap the given triangle.
    /// </summary>
    /// <param name="triangle">The triangle to query.</param>
    /// <param name="candidates">A set to populate with unique colliders.</param>
    /// <returns>
    /// Returns the number of unique colliders added to the candidates set.
    /// </returns>
    public int GetUniqueCandidates(Triangle triangle, ref HashSet<Collider> candidates)
    {
        idHolder.Clear();
        GetCellIDs(triangle, ref idHolder);
        return AccumulateUniqueCandidates(idHolder, ref candidates);
    }
    /// <summary>
    /// Gets all unique colliders that may overlap the given rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to query.</param>
    /// <param name="candidates">A set to populate with unique colliders.</param>
    /// <returns>
    /// Returns the number of unique colliders added to the candidates set.
    /// </returns>
    public int GetUniqueCandidates(Rect rect, ref HashSet<Collider> candidates)
    {
        idHolder.Clear();
        GetCellIDs(rect, ref idHolder);
        return AccumulateUniqueCandidates(idHolder, ref candidates);
    }
    /// <summary>
    /// Gets all unique colliders that may overlap the given quad.
    /// </summary>
    /// <param name="quad">The quad to query.</param>
    /// <param name="candidates">A set to populate with unique colliders.</param>
    /// <returns>
    /// Returns the number of unique colliders added to the candidates set.
    /// </returns>
    public int GetUniqueCandidates(Quad quad, ref HashSet<Collider> candidates)
    {
        idHolder.Clear();
        GetCellIDs(quad, ref idHolder);
        return AccumulateUniqueCandidates(idHolder, ref candidates);
    }
    /// <summary>
    /// Gets all unique colliders that may overlap the given polygon.
    /// </summary>
    /// <param name="poly">The polygon to query.</param>
    /// <param name="candidates">A set to populate with unique colliders.</param>
    /// <returns>
    /// Returns the number of unique colliders added to the candidates set.
    /// </returns>
    public int GetUniqueCandidates(Polygon poly, ref HashSet<Collider> candidates)
    {
        idHolder.Clear();
        GetCellIDs(poly, ref idHolder);
        return AccumulateUniqueCandidates(idHolder, ref candidates);
    }
    /// <summary>
    /// Gets all unique colliders that may overlap the given polyline.
    /// </summary>
    /// <param name="polyLine">The polyline to query.</param>
    /// <param name="candidates">A set to populate with unique colliders.</param>
    /// <returns>
    /// Returns the number of unique colliders added to the candidates set.
    /// </returns>
    public int GetUniqueCandidates(Polyline polyLine, ref HashSet<Collider> candidates)
    {
        idHolder.Clear();
        GetCellIDs(polyLine, ref idHolder);
        return AccumulateUniqueCandidates(idHolder, ref candidates);
    }
  
    /// <summary>
    /// Draws the spatial hash grid and filled cells for debugging purposes.
    /// </summary>
    /// <param name="border">The color for the grid lines.</param>
    /// <param name="fill">The color to fill non-empty cells.</param>
    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
        for (var i = 0; i < BucketCount; i++)
        {
            var coords = GetCoordinatesGrid(i);
            var rect = new Rect(Bounds.X + coords.x * SpacingX, Bounds.Y + coords.y * SpacingY, SpacingX, SpacingY);
            rect.DrawLines(2f, border);
            bool validId = GetCellId(coords.x, coords.y, out int id);
            if(!validId) continue;
            
            if (buckets[id].Count > 0)
            {
                rect.Draw(fill);
            }
        }
    }
    
    #endregion

    #region Private
   
    /// <summary>
    /// Adds all unique colliders from the specified bucket IDs to the candidates set.
    /// </summary>
    /// <param name="bucketIds">List of bucket indices to check.</param>
    /// <param name="candidates">Reference to the set of unique colliders to populate.</param>
    /// <returns>
    /// Returns the number of unique colliders added to the candidates set.
    /// </returns>
    private int AccumulateUniqueCandidates(HashSet<int> bucketIds, ref HashSet<Collider> candidates)
    {
        if (bucketIds.Count <= 0) return 0;
        var prevCount = candidates.Count;
        foreach (int id in bucketIds)
        {
            var bucket = buckets[id];
            if (bucket.Count > 0) candidates.UnionWith(bucket);
        }
        return candidates.Count - prevCount;
    }
    /// <summary>
    /// Adds all non-empty buckets from the specified bucket IDs to the candidateBuckets list.
    /// </summary>
    /// <param name="bucketIds">List of bucket indices to check.</param>
    /// <param name="candidateBuckets">Reference to the list of candidate buckets to populate.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    private int FillCandidateBuckets(HashSet<int> bucketIds, ref List<BroadphaseBucket> candidateBuckets)
    {
        if (bucketIds.Count <= 0) return 0;

        int count = 0;
        foreach (var id in bucketIds)
        {
            var bucket = buckets[id];
            if (bucket.Count > 0)
            {
                count++;
                candidateBuckets.Add(buckets[id]);
            }
        }

        return count;
    }

    
    /// <summary>
    /// Returns the (x, y) grid coordinates for a given bucket index.
    /// </summary>
    /// <param name="index">The bucket index.</param>
    /// <returns>Tuple of (x, y) grid coordinates.</returns>
    private (int x, int y) GetCoordinatesGrid(int index)
    {
        return (index % Cols, index / Cols);
        //return new Tuple<int x, int y>(index % cols, index / cols);
    }

    /// <summary>
    /// Returns the world-space coordinates of the top-left corner of a cell by index.
    /// </summary>
    /// <param name="index">The bucket index.</param>
    /// <returns>World-space coordinates as a Vector2.</returns>
    private Vector2 GetCoordinatesWorld(int index)
    {
        var coord = GetCoordinatesGrid(index);
        return new Vector2(Bounds.X + coord.x * SpacingX, Bounds.Y + coord.y * SpacingY);
    }
    /// <summary>
    /// Gets the rectangle representing a cell at the given grid coordinates.
    /// </summary>
    /// <param name="x">Grid x coordinate.</param>
    /// <param name="y">Grid y coordinate.</param>
    /// <returns>Rectangle of the cell in world space.</returns>
    private Rect GetCellRectangle(int x, int y)
    {
        return new Rect(Bounds.X + x * SpacingX, Bounds.Y + y * SpacingY, SpacingX, SpacingY);
    }
    /// <summary>
    /// Gets the rectangle representing a cell by its index.
    /// </summary>
    /// <param name="index">The bucket index.</param>
    /// <returns>Rectangle of the cell in world space.</returns>
    private Rect GetCellRectangle(int index)
    {
        return GetCellRectangle(index % Cols, index / Cols);
    }

    /// <summary>
    /// Calculates the cell ID (bucket index) for the given grid coordinates.
    /// </summary>
    /// <param name="x">Grid x coordinate.</param>
    /// <param name="y">Grid y coordinate.</param>
    /// <param name="id">The resulting cell ID if valid; otherwise, -1.</param>
    /// <returns>True if the coordinates are valid and the ID is set; otherwise, false.</returns>
    private bool GetCellId(int x, int y, out int id)
    {
        id = -1;
        if (x < 0 || x > Cols - 1 || y < 0 || y > Rows - 1) return false;
        id = x + y * Cols;
        return true;
    }
    /// <summary>
    /// Gets the grid cell coordinates for the given world-space coordinates,
    /// clamping the result to ensure it stays within the valid grid bounds.
    /// </summary>
    /// <param name="x">World x coordinate.</param>
    /// <param name="y">World y coordinate.</param>
    /// <returns>Tuple of (x, y) grid coordinates, clamped to the grid size.</returns>
    private (int x, int y) GetCellCoordinate(float x, float y)
    {
        int xi = Math.Clamp((int)Math.Floor((x - Bounds.X) / SpacingX), 0, Cols - 1); 
        int yi = Math.Clamp((int)Math.Floor((y - Bounds.Y) / SpacingY), 0, Rows - 1);
        return (xi, yi);
    }
    
    /// <summary>
    /// Adds all colliders from a collision object to the spatial hash.
    /// </summary>
    /// <param name="collisionBody">The collision object to add.</param>
    private void Add(CollisionObject collisionBody)
    {
        foreach (var collider in collisionBody.Colliders)
        {
            Add(collider, collisionBody.MotionType);
        }
    }
    
    /// <summary>
    /// Adds a collider to the spatial hash, updating the register and buckets.
    /// </summary>
    /// <param name="collider">The collider to add.</param>
    /// <param name="motionType">The motion type of the parent <see cref="CollisionObject"/> used for this <see cref="Collider"/>.
    /// <see cref="MotionType.Static"/> is used to cache the collider rect and ids in a separate static register for potential optimizations.
    /// </param>
    private void Add(Collider collider, MotionType motionType)
    {
        // The SpatialHash is cleared and filled every frame, so skipping disabled colliders here is safe.
        if (!collider.Enabled) return;
        
        HashSet<int>? staticColliderIds = null;
        if (motionType == MotionType.Static)
        {
            bool entryExists = staticRegister.AddEntry(collider, 1, out var staticSet, out _);
            if (entryExists)
            {
                if (staticSet.Count <= 0) return;
                
                var resultSet = register.AddEntry(collider, staticSet.Count);
                if(resultSet == null) return; //already added this frame
                foreach (int id in staticSet)
                {
                    resultSet.Add(id);
                    buckets[id].Add(collider);
                }
        
                return;
            }
            
            staticColliderIds = staticSet;
        }
        
        var ids = register.AddEntry(collider, 0);
        if(ids == null) return; //already added this frame
        
        var boundingRect = GetCellIDs(collider, ref ids);
        if (ids.Count <= 0) return; 

        if (staticColliderIds != null)
        {
            staticRegister.UpdateRect(collider, boundingRect);
        }
        
        foreach (int hash in ids)
        {
            staticColliderIds?.Add(hash);
            buckets[hash].Add(collider);
        }
    }
    
    /// <summary>
    /// Clears all buckets and applies any queued bounds resize.
    /// </summary>
    private void Clear()
    {
        for (var i = 0; i < BucketCount; i++)
        {
            buckets[i].Clear();
        }

        if (boundsResizeQueued)
        {
            boundsResizeQueued = false;
            Bounds = newBounds;
            SetSpacing();
        }
    }
    
    /// <summary>
    /// Populates the given <paramref name="idList"/> with the cell ID (bucket index) that contains the specified point.
    /// </summary>
    /// <param name="point">The world-space point to locate in the grid.</param>
    /// <param name="idList">The set to populate with the cell ID.</param>
    private void GetCellIDs(Vector2 point, ref HashSet<int> idList)
    {
        if (!Bounds.ContainsPoint(point)) return;
        
        var coordinate = GetCellCoordinate(point.X, point.Y);
        bool validId = GetCellId(coordinate.x, coordinate.y, out int id);
        if(!validId) return;
        idList.Add(id);
    }
    
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a segment overlaps.
    /// </summary>
    /// <param name="segment">The segment to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    /// <param name="testFullShape">Whether to test the exact shape against each bucket rect or just the bounding box of the shape.</param>
    /// <returns>
    /// Returns the bounding rectangle of the shape.
    /// </returns>
    private Rect GetCellIDs(Segment segment, ref HashSet<int> idList, bool testFullShape = true)
    {
        var boundingRect = segment.GetBoundingBox();
        if (!Bounds.OverlapShape(boundingRect)) return boundingRect;
            
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                bool validId = GetCellId(i, j, out int id);
                if(!validId) continue;
                
                if (testFullShape)
                {
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(segment)) idList.Add(id);
                }
                else
                {
                    idList.Add(id);
                }
            }
        }
        return boundingRect;
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a line overlaps.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    ///  <param name="testFullShape">Whether to test the exact shape against each bucket rect or just the bounding box of the shape.</param>
    /// <returns>
    /// Returns the bounding rectangle of the shape.
    /// </returns>
    private Rect GetCellIDs(Line line, ref HashSet<int> idList, bool testFullShape = true)
    {
        var boundingRect = line.GetBoundingBox();
        if (!Bounds.OverlapShape(boundingRect)) return boundingRect;
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                bool validId = GetCellId(i, j, out int id);
                if(!validId) continue;
                
                if (testFullShape)
                {
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(line)) idList.Add(id);
                }
                else
                {
                    idList.Add(id);
                }
            }
        }
        return boundingRect;
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a ray overlaps.
    /// </summary>
    /// <param name="ray">The ray to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    ///  <param name="testFullShape">Whether to test the exact shape against each bucket rect or just the bounding box of the shape.</param>
    /// <returns>
    /// Returns the bounding rectangle of the shape.
    /// </returns>
    private Rect GetCellIDs(Ray ray, ref HashSet<int> idList, bool testFullShape = true)
    {
        var boundingRect = ray.GetBoundingBox();
        if (!Bounds.OverlapShape(boundingRect)) return boundingRect;
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);
        
        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                bool validId = GetCellId(i, j, out int id);
                if(!validId) continue;
                
                if (testFullShape)
                {
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(ray)) idList.Add(id);
                }
                else
                {
                    idList.Add(id);
                }
            }
        }
        
        return boundingRect;
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a triangle overlaps.
    /// </summary>
    /// <param name="triangle">The triangle to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    /// <param name="testFullShape">Whether to test the exact shape against each bucket rect or just the bounding box of the shape.</param>
    /// <returns>
    /// Returns the bounding rectangle of the shape.
    /// </returns>
    private Rect GetCellIDs(Triangle triangle, ref HashSet<int> idList, bool testFullShape = true)
    {
        var boundingRect = triangle.GetBoundingBox();
        if (!Bounds.OverlapShape(boundingRect)) return boundingRect;
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                bool validId = GetCellId(i, j, out int id);
                if(!validId) continue;
                
                if (testFullShape)
                {
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(triangle)) idList.Add(id);
                }
                else
                {
                    idList.Add(id);
                }
            }
        }
        
        return boundingRect;
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a quad overlaps.
    /// </summary>
    /// <param name="quad">The quad to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    /// <param name="testFullShape">Whether to test the exact shape against each bucket rect or just the bounding box of the shape.</param>
    /// <returns>
    /// Returns the bounding rectangle of the shape.
    /// </returns>
    private Rect GetCellIDs(Quad quad, ref HashSet<int> idList, bool testFullShape = true)
    {
        var boundingRect = quad.GetBoundingBox();
        if (!Bounds.OverlapShape(boundingRect)) return boundingRect;
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                bool validId = GetCellId(i, j, out int id);
                if(!validId) continue;
                
                if (testFullShape)
                {
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(quad)) idList.Add(id);
                }
                else
                {
                    idList.Add(id);
                }
            }
        }
        
        return boundingRect;
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a circle overlaps.
    /// </summary>
    /// <param name="circle">The circle to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    /// <param name="testFullShape">Whether to test the exact shape against each bucket rect or just the bounding box of the shape.</param>
    /// <returns>
    /// Returns the bounding rectangle of the shape.
    /// </returns>
    private Rect GetCellIDs(Circle circle, ref HashSet<int> idList, bool testFullShape = true)
    {
        var boundingRect = circle.GetBoundingBox();
        if (!Bounds.OverlapShape(boundingRect)) return boundingRect;
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                bool validId = GetCellId(i, j, out int id);
                if(!validId) continue;
                
                if (testFullShape)
                {
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(circle)) idList.Add(id);
                }
                else
                {
                    idList.Add(id);
                }
            }
        }
        
        return boundingRect;
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a rect overlaps.
    /// </summary>
    /// <param name="rect">The rect to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    /// <returns>
    /// Returns the bounding rectangle of the shape.
    /// </returns>
    private Rect GetCellIDs(Rect rect, ref HashSet<int> idList)
    {
        if (!Bounds.OverlapShape(rect)) return rect;
        var topLeft = GetCellCoordinate(rect.X, rect.Y);
        var bottomRight = GetCellCoordinate(rect.X + rect.Width, rect.Y + rect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                bool validId = GetCellId(i, j, out int id);
                if(!validId) continue;
                
                var cellRect = GetCellRectangle(id);
                if(cellRect.OverlapShape(rect)) idList.Add(id);
            }
        }
        return rect;
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a poly overlaps.
    /// </summary>
    /// <param name="poly">The poly to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    /// <param name="testFullShape">Whether to test the exact shape against each bucket rect or just the bounding box of the shape.</param>
    /// <returns>
    /// Returns the bounding rectangle of the shape.
    /// </returns>
    private Rect GetCellIDs(Polygon poly, ref HashSet<int> idList, bool testFullShape = true)
    {
        var boundingRect = poly.GetBoundingBox();
        if (!Bounds.OverlapShape(boundingRect)) return boundingRect;
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                bool validId = GetCellId(i, j, out int id);
                if(!validId) continue;
                
                if (testFullShape)
                {
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(poly)) idList.Add(id);
                }
                else
                {
                    idList.Add(id);
                }
            }
        }
        return boundingRect;
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a polyline overlaps.
    /// </summary>
    /// <param name="polyLine">The polyline to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    /// <param name="testFullShape">Whether to test the exact shape against each bucket rect or just the bounding box of the shape.</param>
    /// <returns>
    /// Returns the bounding rectangle of the shape.
    /// </returns>
    private Rect GetCellIDs(Polyline polyLine, ref HashSet<int> idList, bool testFullShape = true)
    {
        var boundingRect = polyLine.GetBoundingBox();
        if (!Bounds.OverlapShape(boundingRect)) return boundingRect;
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                bool validId = GetCellId(i, j, out int id);
                if(!validId) continue;
                
                if (testFullShape)
                {
                    var cellRect = GetCellRectangle(id);
                    if(cellRect.OverlapShape(polyLine)) idList.Add(id);
                }
                else
                {
                    idList.Add(id);
                }
            }
        }
        return boundingRect;
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a collider overlaps.
    /// </summary>
    /// <param name="collider">The collider to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    /// <returns>
    /// Returns the bounding rectangle of the shape.
    /// </returns>
    private Rect GetCellIDs(Collider collider, ref HashSet<int> idList)
    {
        if (!collider.Enabled) return new();

        if (collider.BroadphaseType == BroadphaseType.Point)
        {
            var pos = collider.CurTransform.Position;
            GetCellIDs(pos, ref idList);
            return new(pos, new Size(1 , 1), AnchorPoint.Center);
        }
        
        var testFullShape = collider.BroadphaseType == BroadphaseType.FullShape;
        
        switch (collider.GetShapeType())
        {
            case ShapeType.Circle: return GetCellIDs(collider.GetCircleShape(), ref idList, testFullShape);
            case ShapeType.Segment: return GetCellIDs(collider.GetSegmentShape(), ref idList, testFullShape);
            case ShapeType.Line: return GetCellIDs(collider.GetLineShape(), ref idList, testFullShape);
            case ShapeType.Ray: return GetCellIDs(collider.GetRayShape(), ref idList, testFullShape);
            case ShapeType.Triangle: return GetCellIDs(collider.GetTriangleShape(), ref idList, testFullShape);
            case ShapeType.Rect: return GetCellIDs(collider.GetRectShape(), ref idList);
            case ShapeType.Quad: return GetCellIDs(collider.GetQuadShape(), ref idList, testFullShape);
            case ShapeType.Poly: return GetCellIDs(collider.GetPolygonShape(), ref idList, testFullShape);
            case ShapeType.PolyLine: return GetCellIDs(collider.GetPolylineShape(), ref idList, testFullShape);
        }

        return new();
    }
    
    /// <summary>
    /// Calculates and sets the spacing for each grid cell in the X and Y directions
    /// based on the current bounds and number of columns and rows.
    /// </summary>
    private void SetSpacing()
    {
        SpacingX = Bounds.Width / Cols;
        SpacingY = Bounds.Height / Rows;
    }
    #endregion
}