using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
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
public class BroadphaseSpatialHash : IBounds, IBroadphase
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
    private BroadphaseBucket[] buckets;
    private readonly Dictionary<Collider, List<int>> register = new();
    private readonly HashSet<Collider> registerKeys = [];
    private bool boundsResizeQueued;
    private Rect newBounds;
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

        /*foreach (var collider in colliders)
        {
            Add(collider);
        }*/
        
        CleanRegister();
    }
    /// <summary>
    /// Clears all buckets and internal registers, releasing resources.
    /// </summary>
    public void Close()
    {
        Clear();
        register.Clear();
        buckets = Array.Empty<BroadphaseBucket>();  //new HashSet<ICollidable>[0];
    }
    /// <summary>
    /// Queues a resize of the spatial hash bounds to the specified rectangle. The resize is applied on the next clear.
    /// </summary>
    /// <param name="targetBounds">The new bounds for the grid.</param>
    public void ResizeBounds(Rect targetBounds) 
    {
        newBounds = targetBounds;
        boundsResizeQueued = true;
    }
    
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
    /// <param name="collidable">The collision object to query.</param>
    /// <param name="candidateBuckets">A list to populate with candidate buckets.</param>
    /// <returns>
    /// Returns the number of buckets added to the candidateBuckets list.
    /// </returns>
    public int GetCandidateBuckets(CollisionObject collidable, ref List<BroadphaseBucket> candidateBuckets)
    {
        var count = 0;
        
        foreach (var collider in collidable.Colliders)
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
        if (register.TryGetValue(collider, out var bucketIds))
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
        
        List<int> ids = [];
        GetCellIDs(collider, ref ids);
        return FillCandidateBuckets(ids, ref candidateBuckets);
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
        List<int> bucketIds = [];
        GetCellIDs(segment, ref bucketIds);
        
        return FillCandidateBuckets(bucketIds, ref candidateBuckets);
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
        List<int> bucketIds = [];
        GetCellIDs(line, ref bucketIds);
        
        return FillCandidateBuckets(bucketIds, ref candidateBuckets);
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
        List<int> bucketIds = [];
        GetCellIDs(ray, ref bucketIds);
        
        return FillCandidateBuckets(bucketIds, ref candidateBuckets);
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
        List<int> bucketIds = [];
        GetCellIDs(circle, ref bucketIds);
        
        return FillCandidateBuckets(bucketIds, ref candidateBuckets);
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
        List<int> bucketIds = [];
        GetCellIDs(triangle, ref bucketIds);
        
        return FillCandidateBuckets(bucketIds, ref candidateBuckets);
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
        List<int> bucketIds = [];
        GetCellIDs(rect, ref bucketIds);
        
        return FillCandidateBuckets(bucketIds, ref candidateBuckets);
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
        List<int> bucketIds = [];
        GetCellIDs(quad, ref bucketIds);
        
        return FillCandidateBuckets(bucketIds, ref candidateBuckets);
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
        List<int> bucketIds = [];
        GetCellIDs(poly, ref bucketIds);
        
        return FillCandidateBuckets(bucketIds, ref candidateBuckets);
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
        List<int> bucketIds = [];
        GetCellIDs(polyLine, ref bucketIds);
        
        return FillCandidateBuckets(bucketIds, ref candidateBuckets);
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
        if (register.TryGetValue(collider, out var bucketIds))
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
        
        List<int> ids = [];
        GetCellIDs(collider, ref ids);
        return AccumulateUniqueCandidates(ids, ref candidates);
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
        List<int> bucketIds = [];
        GetCellIDs(segment, ref bucketIds);
        
        return AccumulateUniqueCandidates(bucketIds, ref candidates);
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
        List<int> bucketIds = [];
        GetCellIDs(circle, ref bucketIds);
        
        return AccumulateUniqueCandidates(bucketIds, ref candidates);
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
        List<int> bucketIds = [];
        GetCellIDs(triangle, ref bucketIds);
        
        return AccumulateUniqueCandidates(bucketIds, ref candidates);
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
        List<int> bucketIds = [];
        GetCellIDs(rect, ref bucketIds);
        
        return AccumulateUniqueCandidates(bucketIds, ref candidates);
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
        List<int> bucketIds = [];
        GetCellIDs(quad, ref bucketIds);
        
        return AccumulateUniqueCandidates(bucketIds, ref candidates);
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
        List<int> bucketIds = [];
        GetCellIDs(poly, ref bucketIds);
        
        return AccumulateUniqueCandidates(bucketIds, ref candidates);
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
        List<int> bucketIds = [];
        GetCellIDs(polyLine, ref bucketIds);
        
        return AccumulateUniqueCandidates(bucketIds, ref candidates);
    }
  
    /// <summary>
    /// Draws the spatial hash grid and filled cells for debugging purposes.
    /// </summary>
    /// <param name="border">The color for the grid lines.</param>
    /// <param name="fill">The color to fill non-empty cells.</param>
    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
        for (int i = 0; i < BucketCount; i++)
        {
            var coords = GetCoordinatesGrid(i);
            var rect = new Rect(Bounds.X + coords.x * SpacingX, Bounds.Y + coords.y * SpacingY, SpacingX, SpacingY);
            rect.DrawLines(2f, border);
            int id = GetCellId(coords.x, coords.y);
            if (buckets[id].Count > 0)
            {
                rect.Draw(fill);
            }
            
            
            // var rect = new Rectangle(Bounds.X + coords.x * SpacingX, Bounds.Y + coords.y * SpacingY, SpacingX, SpacingY);

            // Raylib.DrawRectangleLinesEx(rect, 1, border.ToRayColor());
            // int id = GetCellID(coords.x, coords.y);
            // if (buckets[id].Count > 0)
            // {
            //     Raylib.DrawRectangleRec(rect, fill.ToRayColor());
            // }

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
    private int AccumulateUniqueCandidates(List<int> bucketIds, ref HashSet<Collider> candidates)
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
    private int FillCandidateBuckets(List<int> bucketIds, ref List<BroadphaseBucket> candidateBuckets)
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
        return new Vector2(coord.x * SpacingX, coord.y * SpacingY);
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
    /// Gets the cell id (bucket index) for the given grid coordinates.
    /// </summary>
    /// <param name="x">Grid x coordinate.</param>
    /// <param name="y">Grid y coordinate.</param>
    /// <returns>Bucket index.</returns>
    private int GetCellId(int x, int y)
    {
        return x + y * Cols;
    }

    /// <summary>
    /// Gets the cell id (bucket index) for the given world-space coordinates.
    /// </summary>
    /// <param name="x">World x coordinate.</param>
    /// <param name="y">World y coordinate.</param>
    /// <returns>Bucket index.</returns>
    private int GetCellId(float x, float y)
    {
        int xi = Math.Clamp((int)Math.Floor((x - Bounds.X) / SpacingX), 0, Cols - 1);
        int yi = Math.Clamp((int)Math.Floor((y - Bounds.Y) / SpacingY), 0, Rows - 1);
        return GetCellId(xi, yi);
    }
    /// <summary>
    /// Gets the grid cell coordinates for the given world-space coordinates.
    /// </summary>
    /// <param name="x">World x coordinate.</param>
    /// <param name="y">World y coordinate.</param>
    /// <returns>Tuple of (x, y) grid coordinates.</returns>
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
            Add(collider);
        }
    }
   
    /// <summary>
    /// Adds a collider to the spatial hash, updating the register and buckets.
    /// </summary>
    /// <param name="collider">The collider to add.</param>
    private void Add(Collider collider)
    {
        // The SpatialHash is cleared and filled every frame, so skipping disabled colliders here is safe.
        if (!collider.Enabled) return;
            
        List<int> ids;
        if (register.TryGetValue(collider, out var value))
        {
            ids = value;
            ids.Clear();
        }
        else
        {
            ids = new List<int>();
            register.Add(collider, ids);
            
        }
        GetCellIDs(collider, ref ids);
        if (ids.Count <= 0) return;
        registerKeys.Remove(collider);
        foreach (int hash in ids)
        {
            buckets[hash].Add(collider);
        }
    }

    /// <summary>
    /// Cleans the register by removing unreferenced colliders and updating the register keys set.
    /// </summary>
    private void CleanRegister()
    {
        foreach (var collider in registerKeys)
        {
            register.Remove(collider);
        }

        registerKeys.Clear();
        registerKeys.UnionWith(register.Keys);
        // registerKeys = register.Keys.ToHashSet();
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
    /// Populates a list with the cell IDs (bucket indices) that a segment overlaps.
    /// </summary>
    /// <param name="segment">The segment to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    private void GetCellIDs(Segment segment, ref List<int> idList)
    {
        var boundingRect = segment.GetBoundingBox();
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                int id = GetCellId(i, j);
                var cellRect = GetCellRectangle(id);
                if(cellRect.OverlapShape(segment)) idList.Add(id);
            }
        }
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a line overlaps.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    private void GetCellIDs(Line line, ref List<int> idList)
    {
        var boundingRect = line.GetBoundingBox();
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                int id = GetCellId(i, j);
                var cellRect = GetCellRectangle(id);
                if(cellRect.OverlapShape(line)) idList.Add(id);
            }
        }
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a ray overlaps.
    /// </summary>
    /// <param name="ray">The ray to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    private void GetCellIDs(Ray ray, ref List<int> idList)
    {
        var boundingRect = ray.GetBoundingBox();
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                int id = GetCellId(i, j);
                var cellRect = GetCellRectangle(id);
                if(cellRect.OverlapShape(ray)) idList.Add(id);
            }
        }
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a triangle overlaps.
    /// </summary>
    /// <param name="triangle">The triangle to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    private void GetCellIDs(Triangle triangle, ref List<int> idList)
    {
        var boundingRect = triangle.GetBoundingBox();
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                int id = GetCellId(i, j);
                var cellRect = GetCellRectangle(id);
                if(cellRect.OverlapShape(triangle)) idList.Add(id);
            }
        }
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a quad overlaps.
    /// </summary>
    /// <param name="quad">The quad to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    private void GetCellIDs(Quad quad, ref List<int> idList)
    {
        var boundingRect = quad.GetBoundingBox();
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                int id = GetCellId(i, j);
                var cellRect = GetCellRectangle(id);
                if(cellRect.OverlapShape(quad)) idList.Add(id);
            }
        }
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a circle overlaps.
    /// </summary>
    /// <param name="circle">The circle to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    private void GetCellIDs(Circle circle, ref List<int> idList)
    {
        var boundingRect = circle.GetBoundingBox();
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                int id = GetCellId(i, j);
                var cellRect = GetCellRectangle(id);
                if(cellRect.OverlapShape(circle)) idList.Add(id);
            }
        }
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a rect overlaps.
    /// </summary>
    /// <param name="rect">The rect to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    private void GetCellIDs(Rect rect, ref List<int> idList)
    {
        var topLeft = GetCellCoordinate(rect.X, rect.Y);
        var bottomRight = GetCellCoordinate(rect.X + rect.Width, rect.Y + rect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                int id = GetCellId(i, j);
                var cellRect = GetCellRectangle(id);
                if(cellRect.OverlapShape(rect)) idList.Add(id);
            }
        }
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a poly overlaps.
    /// </summary>
    /// <param name="poly">The poly to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    private void GetCellIDs(Polygon poly, ref List<int> idList)
    {
        var boundingRect = poly.GetBoundingBox();
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                int id = GetCellId(i, j);
                var cellRect = GetCellRectangle(id);
                if(cellRect.OverlapShape(poly)) idList.Add(id);
            }
        }
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a polyline overlaps.
    /// </summary>
    /// <param name="polyLine">The polyline to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    private void GetCellIDs(Polyline polyLine, ref List<int> idList)
    {
        var boundingRect = polyLine.GetBoundingBox();
        var topLeft = GetCellCoordinate(boundingRect.X, boundingRect.Y);
        var bottomRight = GetCellCoordinate(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height);

        for (int j = topLeft.y; j <= bottomRight.y; j++)
        {
            for (int i = topLeft.x; i <= bottomRight.x; i++)
            {
                int id = GetCellId(i, j);
                var cellRect = GetCellRectangle(id);
                if(cellRect.OverlapShape(polyLine)) idList.Add(id);
            }
        }
    }
    /// <summary>
    /// Populates a list with the cell IDs (bucket indices) that a collider overlaps.
    /// </summary>
    /// <param name="collider">The collider to check.</param>
    /// <param name="idList">The list to populate with cell IDs.</param>
    private void GetCellIDs(Collider collider, ref List<int> idList)
    {
        if (!collider.Enabled) return;
        
        switch (collider.GetShapeType())
        {
            case ShapeType.Circle: GetCellIDs(collider.GetCircleShape(), ref idList); 
                break;
            case ShapeType.Segment: GetCellIDs(collider.GetSegmentShape(), ref idList); 
                break;
            case ShapeType.Line: GetCellIDs(collider.GetLineShape(), ref idList); 
                break;
            case ShapeType.Ray: GetCellIDs(collider.GetRayShape(), ref idList); 
                break;
            case ShapeType.Triangle: GetCellIDs(collider.GetTriangleShape(), ref idList); 
                break;
            case ShapeType.Rect: GetCellIDs(collider.GetRectShape(), ref idList); 
                break;
            case ShapeType.Quad: GetCellIDs(collider.GetQuadShape(), ref idList); 
                break;
            case ShapeType.Poly: GetCellIDs(collider.GetPolygonShape(), ref idList); 
                break;
            case ShapeType.PolyLine: GetCellIDs(collider.GetPolylineShape(), ref idList); 
                break;
        }
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