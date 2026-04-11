using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Random;
using ShapeEngine.ShapeClipper;
using ShapeEngine.StaticLib;
using Size = ShapeEngine.Core.Structs.Size;

namespace ShapeEngine.Geometry.PolygonDef;

/// <summary>
/// Represents a 2D polygon defined by a sequence of points in counter-clockwise order,
/// providing geometric operations, containment and intersection tests,
/// convex hull computation, triangulation, randomization, and shape manipulation utilities.
/// </summary>
/// <remarks>
/// Points should be provided in CCW order for correct geometric calculations.
/// Use <see cref="FixWindingOrder"/> or <see cref="MakeCounterClockwise"/> if your points are in clockwise (CW) order.
/// <list type="bullet">
/// <item>
/// A convex polygon is a polygon where all interior angles are less than 180°,
/// and every line segment between any two points inside the polygon lies entirely within the polygon.
/// In other words, no vertices "point inward."
/// </item>
/// <item>
///A concave polygon is a polygon that has at least one interior angle greater than 180°,
/// and at least one line segment between points inside the polygon passes outside the polygon.
/// This means it has at least one "caved-in" or inward-pointing vertex.
/// </item>
/// </list>
/// </remarks>
public partial class Polygon : Points, IEquatable<Polygon>, IShapeTypeProvider, IClosedShapeTypeProvider
{
    #region Helper Members
    
    private static IntersectionPoints intersectionPointsReference = new(6);
    private static Triangulation triangulationBuffer = new();
    
    private static Paths64 clipResultBuffer = new();
    private static Polygon clipPolygonBuffer = new();
    private static Paths64PooledBuffer clipPooledBuffer = new();
    
    private static List<WeightedItem<Triangle>> weightedTrianglesBuffer = new();
    private static List<Triangle> pickedTrianglesBuffer = new();
    private static Segments segmentsBuffer = new();
    
    #endregion
    
    #region Getters
    /// <summary>
    /// Creates a deep copy of the current polygon.
    /// </summary>
    /// <returns>A new <see cref="Polygon"/> instance with the same vertices.</returns>
    public override Polygon Copy() => new(this);
    
    /// <summary>
    /// Gets the centroid (center) of the polygon.
    /// </summary>
    public Vector2 Center => GetCentroid();

    /// <summary>
    /// Gets the circumcenter of the polygon.
    /// </summary>
    /// <remarks>
    /// Returns the center of the minimal bounding circle computed by <see cref="GetBoundingCircle"/>.
    /// For an empty polygon this will be the default <see cref="System.Numerics.Vector2"/> (0,0).
    /// </remarks>
    public Vector2 Circumcenter => GetBoundingCircle().Center;
    
    /// <summary>
    /// Gets the area of the polygon.
    /// </summary>
    public float Area => GetArea();
    
    /// <summary>
    /// Gets the perimeter (total edge length) of the polygon.
    /// </summary>
    public float Perimeter => GetPerimeter();
    
    /// <summary>
    /// Gets the diameter (maximum distance between any two vertices) of the polygon.
    /// </summary>
    public float Diameter => GetDiameter();
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes an empty polygon.
    /// </summary>
    public Polygon() { }
    
    /// <summary>
    /// Initializes a polygon with a specified capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity for the number of vertices.</param>
    public Polygon(int capacity) : base(capacity)
    {
        
    }
    
    /// <summary>
    /// Initializes a polygon from a collection of points.
    /// </summary>
    /// <param name="points">The points that define the polygon. Should be in CCW order.
    /// Use <see cref="FixWindingOrder"/> or <see cref="MakeCounterClockwise"/> if in CW order.</param>
    public Polygon(IEnumerable<Vector2> points) { AddRange(points); }
    
    /// <summary>
    /// Initializes a polygon from another <see cref="Points"/> instance.
    /// </summary>
    /// <param name="points">The points to copy. Should be in CCW order.</param>
    public Polygon(Points points) : base(points.Count) { AddRange(points); }
    
    /// <summary>
    /// Initializes a polygon by copying another polygon.
    /// </summary>
    /// <param name="poly">The polygon to copy.</param>
    public Polygon(Polygon poly) : base(poly.Count) { AddRange(poly); }
    
    /// <summary>
    /// Initializes a polygon from a polyline.
    /// </summary>
    /// <param name="polyLine">The polyline whose points will define the polygon.</param>
    public Polygon(Polyline polyLine) : base(polyLine.Count) { AddRange(polyLine); }
    #endregion

    #region Equals & Hashcode
    /// <summary>
    /// Determines whether the specified polygon is equal to the current polygon.
    /// </summary>
    /// <param name="other">The polygon to compare with the current polygon.</param>
    /// <returns>True if the polygons are equal; otherwise, false.</returns>
    /// <remarks>
    /// Equality is determined by comparing the number of vertices and the similarity of each corresponding vertex.
    /// </remarks>
    public bool Equals(Polygon? other) => base.Equals(other);

    /// <summary>
    /// Determines whether the specified polygon is equal to the current polygon using quantized comparison.
    /// </summary>
    /// <param name="other">The polygon to compare with the current polygon.</param>
    /// <param name="decimalPlaces">The number of decimal places used to quantize point coordinates before comparison.</param>
    /// <returns>True if the polygons are equal after quantization; otherwise, false.</returns>
    public bool Equals(Polygon? other, int decimalPlaces) => base.Equals(other, decimalPlaces);
    
    /// <summary>
    /// Returns a hash code for the polygon.
    /// </summary>
    /// <returns>A hash code for the current polygon.</returns>
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>
    /// Gets the closed shape type represented by this polygon.
    /// </summary>
    /// <returns><see cref="ClosedShapeType.Poly"/>.</returns>
    public ClosedShapeType GetClosedShapeType() => ClosedShapeType.Poly;

    /// <summary>
    /// Gets the general shape type represented by this polygon.
    /// </summary>
    /// <returns><see cref="ShapeType.Poly"/>.</returns>
    public ShapeType GetShapeType() => ShapeType.Poly;

    /// <summary>
    /// Determines whether the specified object is equal to the current polygon.
    /// </summary>
    /// <param name="obj">The object to compare with the current polygon.</param>
    /// <returns>True if the object is a <see cref="Polygon"/> and is equal to the current polygon; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Polygon other && Equals(other);

    /// <summary>
    /// Determines whether two polygons are equal.
    /// </summary>
    /// <param name="left">The first polygon to compare.</param>
    /// <param name="right">The second polygon to compare.</param>
    /// <returns>True if the polygons are equal; otherwise, false.</returns>
    public static bool operator ==(Polygon? left, Polygon? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two polygons are not equal.
    /// </summary>
    /// <param name="left">The first polygon to compare.</param>
    /// <param name="right">The second polygon to compare.</param>
    /// <returns>True if the polygons are not equal; otherwise, false.</returns>
    public static bool operator !=(Polygon? left, Polygon? right)
    {
        return !(left == right);
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Gets the segment (edge) at the specified index.
    /// </summary>
    /// <param name="index">The index of the segment. Wraps around if out of bounds.</param>
    /// <returns>The segment defined by two consecutive vertices.</returns>
    public Segment GetSegment(int index)
    {
        if (index < 0) return new Segment();
        if (Count < 2) return new Segment();
        var first = index % Count;
        var second = (index + 1) % Count;
        return new Segment(this[first], this[second]);
    }
    #endregion
    
    #region Vertices

    /// <summary>
    /// Ensures the polygon's winding order is counter-clockwise (CCW).
    /// </summary>
    /// <remarks>
    /// If the polygon is in clockwise (CW) order, it will be reversed.
    /// </remarks>
    public void FixWindingOrder()
    {
        if (IsClockwise())
        {
            Reverse();
        }
    }
    
    /// <summary>
    /// Converts the polygon's winding order to clockwise (CW).
    /// </summary>
    /// <remarks>
    /// If the polygon is in counter-clockwise (CCW) order, it will be reversed.
    /// </remarks>
    public void MakeClockwise()
    {
        if (IsClockwise()) return;
        Reverse();
    }
    
    /// <summary>
    /// Converts the polygon's winding order to counter-clockwise (CCW).
    /// </summary>
    /// <remarks>
    /// If the polygon is in clockwise (CW) order, it will be reversed.
    /// </remarks>
    public void MakeCounterClockwise()
    {
        if (!IsClockwise()) return;
        Reverse();
    }
    
    /// <summary>
    /// Reduces the number of vertices in the polygon to the specified count.
    /// </summary>
    /// <param name="newCount">The desired number of vertices. If less than 3, the polygon is cleared.</param>
    public void ReduceVertexCount(int newCount)
    {
        if (newCount < 3) Clear();//no points left to form a polygon

        while (Count > newCount)
        {
            float minD = 0f;
            int shortestID = 0;
            for (int i = 0; i < Count; i++)
            {
                float d = (this[i] - this[(i + 1) % Count]).LengthSquared();
                if (d > minD)
                {
                    minD = d;
                    shortestID = i;
                }
            }
            RemoveAt(shortestID);
        }

    }
    
    /// <summary>
    /// Reduces the number of vertices by a factor.
    /// </summary>
    /// <param name="factor">The fraction of vertices to remove <c>(0-1)</c>.</param>
    public void ReduceVertexCount(float factor)
    {
        ReduceVertexCount(Count - (int)(Count * factor));
    }
    
    /// <summary>
    /// Increases the number of vertices in the polygon to the specified count by subdividing the longest edges.
    /// </summary>
    /// <param name="newCount">The desired number of vertices. No action if less than or equal to the current count.</param>
    public void IncreaseVertexCount(int newCount)
    {
        if (newCount <= Count) return;

        while (Count < newCount)
        {
            float maxD = 0f;
            int longestID = 0;
            for (int i = 0; i < Count; i++)
            {
                float d = (this[i] - this[(i + 1) % Count]).LengthSquared();
                if (d > maxD)
                {
                    maxD = d;
                    longestID = i;
                }
            }
            Vector2 m = (this[longestID] + this[(longestID + 1) % Count]) * 0.5f;
            this.Insert(longestID + 1, m);
        }
    }
    
    /// <summary>
    /// Gets the vertex at the specified index, wrapping around if necessary.
    /// </summary>
    /// <param name="index">The index of the vertex.</param>
    /// <returns>The vertex at the specified index.</returns>
    public Vector2 GetVertex(int index) => this[ShapeMath.WrapIndex(Count, index)];

    /// <summary>
    /// Removes vertices that are approximately collinear with their neighboring vertices.
    /// </summary>
    /// <param name="angleThresholdDegrees">
    /// The threshold angle in degrees used to determine collinearity. If the angle between the
    /// vectors (prev -> cur) and (cur -> next) is smaller than this threshold the current vertex
    /// is considered collinear and removed.
    /// </param>
    /// <remarks>
    /// - No action is taken when the polygon has fewer than three vertices.
    /// - Uses <see cref="ShapeVec.IsColinearAngle(Vector2, Vector2, Vector2, float)"/> to test collinearity.
    /// - Builds a temporary <see cref="Points"/> result and replaces the polygon's vertices
    ///   after filtering to preserve iteration stability.
    /// </remarks>
    public void RemoveCollinearVertices(float angleThresholdDegrees = 5f)
    {
        if (Count < 3) return;
        Points result = [];
        for (var i = 0; i < Count; i++)
        {
            var prev = Game.GetItem(this, i - 1);
            var cur = this[i];
            var next = Game.GetItem(this, i + 1);

            if(ShapeVec.IsColinearAngle(prev, cur, next, angleThresholdDegrees)) continue;
            result.Add(cur);
        }
        Clear();
        AddRange(result);
    }
    
    /// <summary>
    /// Removes vertices that are effectively duplicates by comparing each vertex to its next neighbor.
    /// If the squared distance between two consecutive vertices is less than or equal to
    /// <paramref name="toleranceSquared"/>, the current vertex is omitted.
    /// No action is performed when the polygon has fewer than three vertices.
    /// </summary>
    /// <param name="toleranceSquared">Squared distance threshold used to detect duplicate vertices (default: 0.001f).</param>
    public void RemoveDuplicates(float toleranceSquared = 0.001f)
    {
        if (Count < 3) return;
        Points result = [];

        for (var i = 0; i < Count; i++)
        {
            var cur = this[i];
            var next = Game.GetItem(this, i + 1);
            if ((cur - next).LengthSquared() > toleranceSquared) result.Add(cur);
        }
        Clear();
        AddRange(result);
    }

    /// <summary>
    /// Smooths the polygon by moving each vertex towards the average of its neighbors and the centroid.
    /// </summary>
    /// <param name="amount">The smoothing factor (0-1).</param>
    /// <param name="baseWeight">The weight applied to the centroid direction.</param>
    public void Smooth(float amount, float baseWeight)
    {
        if (Count < 3) return;
        Points result = [];
        var centroid = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var cur = this[i];
            var prev = this[ShapeMath.WrapIndex(Count, i - 1)];
            var next = this[ShapeMath.WrapIndex(Count, i + 1)];
            var dir = (prev - cur) + (next - cur) + ((cur - centroid) * baseWeight);
            result.Add(cur + dir * amount);
        }

        Clear();
        AddRange(result);
    }
    
    /// <summary>
    /// Creates and returns a new polygon that is a copy of this polygon with vertices
    /// that are approximately collinear removed.
    /// </summary>
    /// <param name="angleThresholdDegrees">
    /// The threshold angle in degrees used to determine collinearity. If the angle between
    /// the vectors (prev -> cur) and (cur -> next) is smaller than this threshold the current
    /// vertex is considered collinear and omitted from the returned polygon. Default is 5 degrees.
    /// </param>
    /// <returns>
    /// A new <see cref="Polygon"/> containing the filtered vertices, or <c>null</c> if this
    /// polygon has fewer than three vertices (no meaningful polygon can be produced).
    /// </returns>
    public Polygon? RemoveCollinearVerticesCopy(float angleThresholdDegrees = 5f)
    {
        if (Count < 3) return null;
        Polygon result = [];
        for (var i = 0; i < Count; i++)
        {
            var prev = Game.GetItem(this, i - 1);
            var cur = this[i];
            var next = Game.GetItem(this, i + 1);

            if(ShapeVec.IsColinearAngle(prev, cur, next, angleThresholdDegrees)) continue;
            result.Add(cur);
        }

        return result;
    }
    
    /// <summary>
    /// Creates and returns a new <see cref="Polygon"/> with consecutive duplicate vertices removed.
    /// Each vertex is compared to its next neighbor; if the squared distance between them is less than
    /// or equal to <paramref name="toleranceSquared"/>, the current vertex is omitted from the result.
    /// </summary>
    /// <param name="toleranceSquared">
    /// Squared distance threshold used to detect duplicate vertices. Vertices closer than or equal to
    /// this threshold are treated as duplicates. Default is <c>0.001f</c>.
    /// </param>
    /// <returns>
    /// A new <see cref="Polygon"/> containing the filtered vertices, or <c>null</c> if this polygon
    /// has fewer than three vertices (no meaningful polygon can be produced).
    /// </returns>
    public Polygon? RemoveDuplicatesCopy(float toleranceSquared = 0.001f)
    {
        if (Count < 3) return null;
        Polygon result = [];

        for (var i = 0; i < Count; i++)
        {
            var cur = this[i];
            var next = Game.GetItem(this, i + 1);
            if ((cur - next).LengthSquared() > toleranceSquared) result.Add(cur);
        }

        return result;
    }
    
    /// <summary>
    /// Creates and returns a smoothed copy of this polygon.
    /// Each vertex is moved towards the average of its neighboring vertices and towards the polygon centroid
    /// according to the provided smoothing parameters.
    /// </summary>
    /// <param name="amount">Smoothing factor in the range [0,1]. 0 means no change, 1 applies the full computed displacement.</param>
    /// <param name="baseWeight">Weight applied to the centroid contribution when computing the smoothing direction.</param>
    /// <returns>
    /// A new <see cref="Polygon"/> containing the smoothed vertices, or <c>null</c> if this polygon has fewer than three vertices.
    /// </returns>
    public Polygon? SmoothCopy(float amount, float baseWeight)
    {
        if (Count < 3) return null;
        Polygon result = [];
        var centroid = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var cur = this[i];
            var prev = this[ShapeMath.WrapIndex(Count, i - 1)];
            var next = this[ShapeMath.WrapIndex(Count, i + 1)];
            var dir = (prev - cur) + (next - cur) + ((cur - centroid) * baseWeight);
            result.Add(cur + dir * amount);
        }

        return result;
    }
    
    /// <summary>
    /// Writes a copy of this polygon into <paramref name="result"/> with approximately collinear vertices removed.
    /// </summary>
    /// <param name="result">The destination polygon that will be cleared and populated with the filtered vertices.</param>
    /// <param name="angleThresholdDegrees">
    /// The threshold angle in degrees used to determine collinearity. If the angle between
    /// the vectors (prev -> cur) and (cur -> next) is smaller than this threshold the current
    /// vertex is considered collinear and omitted from the copied polygon. Default is 5 degrees.
    /// </param>
    /// <returns><c>true</c> if this polygon has at least three vertices and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    public bool RemoveCollinearVerticesCopy(Polygon result, float angleThresholdDegrees = 5f)
    {
        if (Count < 3) return false;
        result.Clear();
        result.EnsureCapacity(Count);
        for (var i = 0; i < Count; i++)
        {
            var prev = Game.GetItem(this, i - 1);
            var cur = this[i];
            var next = Game.GetItem(this, i + 1);

            if(ShapeVec.IsColinearAngle(prev, cur, next, angleThresholdDegrees)) continue;
            result.Add(cur);
        }

        return true;
    }
    
    /// <summary>
    /// Writes a copy of this polygon into <paramref name="result"/> with consecutive duplicate vertices removed.
    /// Each vertex is compared to its next neighbor; if the squared distance between them is less than
    /// or equal to <paramref name="toleranceSquared"/>, the current vertex is omitted from the result.
    /// </summary>
    /// <param name="result">The destination polygon that will be cleared and populated with the filtered vertices.</param>
    /// <param name="toleranceSquared">
    /// Squared distance threshold used to detect duplicate vertices. Vertices closer than or equal to
    /// this threshold are treated as duplicates. Default is <c>0.001f</c>.
    /// </param>
    /// <returns><c>true</c> if this polygon has at least three vertices and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    public bool RemoveDuplicatesCopy(Polygon result, float toleranceSquared = 0.001f)
    {
        if (Count < 3) return false;
        
        result.Clear();
        result.EnsureCapacity(Count);

        for (var i = 0; i < Count; i++)
        {
            var cur = this[i];
            var next = Game.GetItem(this, i + 1);
            if ((cur - next).LengthSquared() > toleranceSquared) result.Add(cur);
        }

        return true;
    }
    
    /// <summary>
    /// Writes a smoothed copy of this polygon into <paramref name="result"/>.
    /// Each vertex is moved towards the average of its neighboring vertices and towards the polygon centroid
    /// according to the provided smoothing parameters.
    /// </summary>
    /// <param name="result">The destination polygon that will be cleared and populated with the smoothed vertices.</param>
    /// <param name="amount">Smoothing factor in the range [0,1]. 0 means no change, 1 applies the full computed displacement.</param>
    /// <param name="baseWeight">Weight applied to the centroid contribution when computing the smoothing direction.</param>
    /// <returns><c>true</c> if this polygon has at least three vertices and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    public bool SmoothCopy(Polygon result, float amount, float baseWeight)
    {
        if (Count < 3) return false;
        result.Clear();
        result.EnsureCapacity(Count);
        var centroid = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var cur = this[i];
            var prev = this[ShapeMath.WrapIndex(Count, i - 1)];
            var next = this[ShapeMath.WrapIndex(Count, i + 1)];
            var dir = (prev - cur) + (next - cur) + ((cur - centroid) * baseWeight);
            result.Add(cur + dir * amount);
        }

        return true;
    }
    #endregion

    #region Shape
    /// <summary>
    /// Converts the polygon to a relative shape centered at its centroid
    /// and normalized by its maximum distance from the centroid.
    /// </summary>
    /// <returns>A tuple containing the transformation and the relative polygon.</returns>
    public (Transform2D transform, Polygon shape) ToRelative()
    {
        var pos = GetCentroid();
        var maxLengthSq = 0f;
        for (int i = 0; i < this.Count; i++)
        {
            var lsq = (this[i] - pos).LengthSquared();
            if (maxLengthSq < lsq) maxLengthSq = lsq;
        }

        var size = MathF.Sqrt(maxLengthSq);
        var relativeShape = new Polygon(Count);
        for (int i = 0; i < this.Count; i++)
        {
            var w = this[i] - pos;
            relativeShape.Add(w / size); //transforms it to range 0 - 1
        }

        return (new Transform2D(pos, 0f, new Size(size, 0f), 1f), relativeShape);
    }
   
    /// <summary>
    /// Converts the polygon's points to relative coordinates using a given transform.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <returns>A set of points in relative coordinates.</returns>
    public Points ToRelativePoints(Transform2D transform)
    {
        var points = new Points(Count);
        for (var i = 0; i < Count; i++)
        {
            var p = transform.RevertPosition(this[i]);
            points.Add(p);
        }

        return points;
    }

    /// <summary>
    /// Converts the polygon to a new polygon in relative coordinates using a given transform.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <returns>A new polygon in relative coordinates.</returns>
    public Polygon ToRelativePolygon(Transform2D transform)
    {
        var points = new Polygon(Count);
        for (int i = 0; i < Count; i++)
        {
            var p = transform.RevertPosition(this[i]);
            points.Add(p);
        }

        return points;
    }
    
    /// <summary>
    /// Converts the polygon's points to a list of relative coordinates using a given transform.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <returns>A list of points in relative coordinates.</returns>
    public List<Vector2> ToRelative(Transform2D transform)
    {
        var points = new List<Vector2>(Count);
        for (int i = 0; i < Count; i++)
        {
            var p = transform.RevertPosition(this[i]);
            points.Add(p);
        }

        return points;
    }
    
    /// <summary>
    /// Gets the minimal bounding triangle of the polygon.
    /// </summary>
    /// <param name="margin">Optional margin to expand the bounding triangle. Default is 3.</param>
    /// <returns>The bounding triangle.</returns>
    public Triangle GetBoundingTriangle(float margin = 3f) => GetPointCloudBoundingTriangle(margin);
    
    /// <summary>
    /// Returns the edges (segments) of the polygon.
    /// </summary>
    /// <returns>A <see cref="Segments"/> collection representing the polygon's edges.</returns>
    /// <remarks>
    /// If the points are in CCW order, the normals face outward.
    /// </remarks>
    public Segments GetEdges()
    {
        if (Count <= 1) return new();
        if (Count == 2) return new() { new(this[0], this[1]) };
        Segments segments = new(Count);
        for (int i = 0; i < Count; i++)
        {
            segments.Add(new(this[i], this[(i + 1) % Count]));
        }
        return segments;
    }
    
    /// <summary>
    /// Computes and returns the normal vectors for the polygon edges.
    /// </summary>
    /// <returns>
    /// A <see cref="List{Vector2}"/> containing one normal per edge:
    /// - If the polygon has 0 or 1 vertex an empty list is returned.
    /// - If the polygon has 2 vertices a single normal for the segment is returned.
    /// - For 3+ vertices returns normals for every edge (edge from vertex i to i+1).
    /// </returns>
    /// <remarks>
    /// Normals are computed using <see cref="Segment.GetNormal(Vector2, Vector2, bool)"/> with the third parameter set to <c>false</c>.
    /// If the polygon points are in counter-clockwise (CCW) order the normals will face outward.
    /// </remarks>
    public List<Vector2> GetEdgeNormals()
    {
        switch (Count)
        {
            case <= 1:
                return [];
            case 2:
                return [Segment.GetNormal(this[0], this[1], false)];
        }

        var normals = new List<Vector2>(Count);
        for (var i = 0; i < Count; i++)
        {
            normals.Add(Segment.GetNormal(this[i], this[(i + 1) % Count], false));
        }
        return normals;
    }
    
    /// <summary>
    /// Returns a simple (non-minimal) enclosing circle for the polygon.
    /// You should always use <see cref="GetBoundingCircle"/> unless performance is critical.
    /// </summary>
    /// <remarks>
    /// This method computes an approximate bounding circle using the polygon's centroid
    /// and the maximum distance from that centroid to any vertex.
    /// It is faster (by a very small amount) than
    /// computing the minimal enclosing circle but may produce a larger-than-necessary radius.
    /// Use <see cref="GetBoundingCircle"/> for the minimal enclosing circle. 
    /// </remarks>
    /// <returns>
    /// A <see cref="Circle"/> centered at the polygon centroid with radius equal to the
    /// maximum distance from the centroid to any vertex. For an empty polygon an empty
    /// <see cref="Circle"/> is returned.
    /// </returns>
    public Circle GetBoundingCircleSimple()
    {
        if(Count <= 0) return new Circle();
        var maxD = 0f;
        int num = Count;
        Vector2 origin = new();
        for (int i = 0; i < num; i++) { origin += this[i]; }
        origin /= num;
        for (int i = 0; i < num; i++)
        {
            float d = (origin - this[i]).LengthSquared();
            if (d > maxD) maxD = d;
        }

        return new Circle(origin, MathF.Sqrt(maxD));
    }
   
    /// <summary>
    /// Gets the minimal bounding circle of the polygon using Welzl's algorithm.
    /// </summary>
    /// <returns>The minimal bounding <see cref="Circle"/>.</returns>
    public Circle GetBoundingCircle()
    {
        if (Count == 0) return new Circle();
        if (Count == 1) return new Circle(this[0], 0f);
        if (Count == 2)
        {
            var center = (this[0] + this[1]) * 0.5f;
            var radius = (this[1] - this[0]).Length() * 0.5f;
            return new Circle(center, radius);
        }
    
        var points = new List<Vector2>(this);
        var boundary = new List<Vector2>();
        return WelzlHelper(points, boundary, points.Count);
    }
    
    /// <summary>
    /// Gets the axis-aligned bounding box of the polygon.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public Rect GetBoundingBox()
    {
        if (Count == 0) return new Rect();
        if (Count == 1) return new Rect(this[0].X, this[0].Y, 0, 0);
    
        float minX = this[0].X;
        float maxX = this[0].X;
        float minY = this[0].Y;
        float maxY = this[0].Y;
    
        for (int i = 1; i < Count; i++)
        {
            var p = this[i];
            if (p.X < minX) minX = p.X;
            else if (p.X > maxX) maxX = p.X;
            
            if (p.Y < minY) minY = p.Y;
            else if (p.Y > maxY) maxY = p.Y;
        }
    
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }
    
    /// <summary>
    /// Returns the convex hull of the polygon as a new polygon.
    /// </summary>
    /// <returns>A convex polygon containing all the original points.</returns>
    public Polygon? ToConvex()
    {
        var result = new Polygon();
        FindConvexHull(result);
        return result.Count >= 3 ? result : null;
    }
    
    /// <summary>
    /// Converts this polygon's vertices into relative coordinates using the supplied transform and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination list that will be cleared and populated with the relative coordinates.</param>
    /// <param name="transform">The transform whose inverse position mapping is applied to each polygon vertex.</param>
    public void ToRelative(List<Vector2> result, Transform2D transform)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        for (int i = 0; i < Count; i++)
        {
            var p = transform.RevertPosition(this[i]);
            result.Add(p);
        }
    }
    
    /// <summary>
    /// Appends the edge normal vectors of this polygon to the provided list.
    /// </summary>
    /// <param name="result">A <see cref="List{Vector2}"/> that will receive the computed normals. Existing contents are preserved and the normals are appended.</param>
    /// <returns>
    /// The number of normals added:
    /// - 0 when the polygon has 0 or 1 vertex (nothing is added).
    /// - 1 when the polygon has exactly 2 vertices (a single segment normal is added).
    /// - <c>Count</c> when the polygon has 3 or more vertices (one normal per edge).
    /// </returns>
    /// <remarks>
    /// Normals are computed using <see cref="Segment.GetNormal(Vector2, Vector2, bool)"/> with the third parameter set to <c>false</c>.
    /// When the polygon vertices are in counter-clockwise (CCW) order the normals will face outward.
    /// </remarks>
    public int GetEdgeNormals(ref List<Vector2> result)
    {
        switch (Count)
        {
            case <= 1:
                return 0;
            case 2:
                result.Add(Segment.GetNormal(this[0], this[1], false));
                return 1;
        }
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(Segment.GetNormal(this[i], this[(i + 1) % Count], false));
        }
        return Count;
    }
    
    /// <summary>
    /// Computes the convex hull of this polygon and writes it into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination polygon that will receive the convex hull vertices.</param>
    /// <remarks>
    /// This method delegates to <see cref="Points.FindConvexHull(List{Vector2})"/> using the polygon instance as the destination collection.
    /// </remarks>
    public void ToConvex(Polygon result)
    {
        FindConvexHull(result);
    }

    /// <summary>
    /// Computes the direction vector of each polygon edge and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination list that will be cleared and populated with one direction vector per edge.</param>
    /// <param name="normalized"><c>true</c> to normalize each direction vector; otherwise, full edge displacement vectors are written.</param>
    /// <returns><c>true</c> if the polygon has at least two vertices and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    public bool GetEdgeDirections(List<Vector2> result, bool normalized = false)
    {
        if (Count <= 1) return false;
        result.Clear();
        if (Count == 2)
        {
            result.Add(this[1] - this[0]);
            return true;
        }
        for (var i = 0; i < Count; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];
            var a = end - start;
            result.Add(normalized ? a.Normalize() : a);
        }

        return true;
    }
    
    /// <summary>
    /// Writes the polygon edges into <paramref name="result"/> as segments.
    /// </summary>
    /// <param name="result">The destination segment collection that will be cleared and populated with the polygon edges.</param>
    /// <returns><c>true</c> if the polygon has at least two vertices and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// When the polygon has exactly two vertices, a single segment is produced. For three or more vertices, the closing segment from the last vertex back to the first is included.
    /// </remarks>
    public bool GetEdges(Segments result)
    {
        if (Count <= 1) return false;
        
        result.Clear();
        result.EnsureCapacity(Count);
        
        if (Count == 2)
        {
            var segment = new Segment(this[0], this[1]);
            result.Add(segment);
            return true;
        }
        
        for (int i = 0; i < Count; i++)
        {
            result.Add(new(this[i], this[(i + 1) % Count]));
        }
        
        return true;
    }
    
    #endregion

    #region Random
    /// <summary>
    /// Gets a random point inside the polygon using triangulation.
    /// </summary>
    /// <returns>A random point inside the polygon.</returns>
    public Vector2 GetRandomPointInside()
    {
        Triangulate(triangulationBuffer);
        
        weightedTrianglesBuffer.Clear();
        weightedTrianglesBuffer.EnsureCapacity(triangulationBuffer.Count);
        
        foreach (var t in triangulationBuffer)
        {
            weightedTrianglesBuffer.Add(new(t, (int)t.GetArea()));
        }
        
        var item = Rng.Instance.PickRandomItem(weightedTrianglesBuffer);
        return item.GetRandomPointInside();
    }
    
    /// <summary>
    /// Gets a set of random points inside the polygon.
    /// </summary>
    /// <param name="amount">The number of random points to generate.</param>
    /// <returns>A set of random points inside the polygon.</returns>
    public Points GetRandomPointsInside(int amount)
    {
        Triangulate(triangulationBuffer);
        
        weightedTrianglesBuffer.Clear();
        weightedTrianglesBuffer.EnsureCapacity(triangulationBuffer.Count);
        
        foreach (var t in triangulationBuffer)
        {
            weightedTrianglesBuffer.Add(new(t, (int)t.GetArea()));
        }
        
        Rng.Instance.PickRandomItems(pickedTrianglesBuffer, amount, weightedTrianglesBuffer);
        Points randomPoints = new(amount);
        foreach (var tri in pickedTrianglesBuffer)
        {
            randomPoints.Add(tri.GetRandomPointInside());
        }

        return randomPoints;
    }
    
    /// <summary>
    /// Gets a random vertex from the polygon.
    /// </summary>
    /// <returns>A random vertex.</returns>
    public Vector2 GetRandomVertex() { return Rng.Instance.RandCollection(this); }
    
    /// <summary>
    /// Gets a random edge (segment) from the polygon.
    /// </summary>
    /// <returns>A random edge.</returns>
    public Segment GetRandomEdge()
    {
        GetEdges(segmentsBuffer);
        return segmentsBuffer.GetRandomSegment();
    }

    /// <summary>
    /// Gets a random point on the polygon's edge.
    /// </summary>
    /// <returns>A random point on an edge.</returns>
    public Vector2 GetRandomPointOnEdge()
    {
        return GetRandomEdge().GetRandomPoint();
    }

    /// <summary>
    /// Gets a set of random points on the polygon's edges.
    /// </summary>
    /// <param name="amount">The number of random points to generate.</param>
    /// <returns>A set of random points on the edges.</returns>
    public Points GetRandomPointsOnEdge(int amount)
    {
        GetEdges(segmentsBuffer);
        var points = new Points();
        segmentsBuffer.GetRandomPoints(amount, points);
        return points;
    }

    /// <summary>
    /// Gets a random point inside the convex hull of the polygon by interpolating between random edges.
    /// </summary>
    /// <returns>A random point inside the convex hull.</returns>
    public Vector2 GetRandomPointConvex()
    {
        var edges = GetEdges();
        var ea = Rng.Instance.RandCollection(edges, true);
        var eb = Rng.Instance.RandCollection(edges);

        var pa = ea.Start.Lerp(ea.End, Rng.Instance.RandF());
        var pb = eb.Start.Lerp(eb.End, Rng.Instance.RandF());
        return pa.Lerp(pb, Rng.Instance.RandF());
    }
    
    /// <summary>
    /// Generates random points inside the polygon and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the generated points.</param>
    /// <param name="amount">The number of random points to generate.</param>
    /// <remarks>
    /// This method triangulates the polygon, randomly selects triangles weighted by their area, and samples one random interior point from each selected triangle.
    /// </remarks>
    public void GetRandomPointsInside(Points result, int amount)
    {
        Triangulate(triangulationBuffer);
        
        weightedTrianglesBuffer.Clear();
        weightedTrianglesBuffer.EnsureCapacity(triangulationBuffer.Count);
        
        foreach (var t in triangulationBuffer)
        {
            weightedTrianglesBuffer.Add(new(t, (int)t.GetArea()));
        }
        
        Rng.Instance.PickRandomItems(pickedTrianglesBuffer, amount, weightedTrianglesBuffer);
        
        result.Clear();
        result.EnsureCapacity(pickedTrianglesBuffer.Count);
        
        foreach (var tri in pickedTrianglesBuffer)
        {
            result.Add(tri.GetRandomPointInside());
        }
    }
    
    /// <summary>
    /// Generates random points on the polygon's edges and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the generated edge points.</param>
    /// <param name="amount">The number of random points to generate.</param>
    /// <remarks>
    /// Edge selection is delegated to <see cref="Segments.GetRandomPoints(int, Points)"/> after building the polygon edge list.
    /// </remarks>
    public void GetRandomPointsOnEdge(Points result, int amount)
    {
        GetEdges(segmentsBuffer);
        segmentsBuffer.GetRandomPoints(amount, result);
    }
    #endregion
    
    #region Static Methods
    
    /// <summary>
    /// Transforms the supplied relative points and appends them to <paramref name="result"/> as polygon vertices.
    /// </summary>
    /// <param name="relative">The relative points defining the shape.</param>
    /// <param name="transform">The transform applied to each point before it is added to <paramref name="result"/>.</param>
    /// <param name="result">The destination polygon that receives the transformed vertices.</param>
    /// <returns><c>true</c> if <paramref name="relative"/> contains at least three points and the transformed vertices were added; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not clear <paramref name="result"/> before adding vertices.
    /// </remarks>
    public static bool GetShape(Points relative, Transform2D transform, Polygon result)
    {
        if (relative.Count < 3) return false;
        for (var i = 0; i < relative.Count; i++)
        {
            result.Add(transform.ApplyTransformTo(relative[i]));
        }

        return true;
    }

    /// <summary>
    /// Generates vertices for a random polygon around the origin and appends them to <paramref name="result"/>.
    /// </summary>
    /// <param name="pointCount">Number of points (vertices) in the polygon. Must be at least 3.</param>
    /// <param name="minLength">Minimum distance from the origin for each point.</param>
    /// <param name="maxLength">Maximum distance from the origin for each point.</param>
    /// <param name="result">The destination polygon that receives the generated vertices.</param>
    /// <returns><c>true</c> if the polygon parameters are valid and vertices were generated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The generated points are evenly distributed by angle and randomized by radial distance. If <paramref name="minLength"/> is greater than <paramref name="maxLength"/>, the values are swapped. This method does not clear <paramref name="result"/> before adding vertices.
    /// </remarks>
    public static bool GenerateRelative(int pointCount, float minLength, float maxLength, Polygon result)
    {
        if (pointCount < 3) return false;
        if (Math.Abs(minLength - maxLength) < ShapeMath.Epsilon) return false;
        if (minLength > maxLength)
        {
            //swap
            (minLength, maxLength) = (maxLength, minLength);
        }
        
        float angleStep = ShapeMath.PI * 2.0f / pointCount;

        for (var i = 0; i < pointCount; i++)
        {
            float randLength = Rng.Instance.RandF(minLength, maxLength);
            var p = ShapeVec.Right().Rotate(-angleStep * i) * randLength;
            result.Add(p);
        }

        return true;
    }

    /// <summary>
    /// Generates vertices for a random polygon centered at <paramref name="center"/> and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="center">The center position of the polygon.</param>
    /// <param name="pointCount">Number of points (vertices) in the polygon. Must be at least 3.</param>
    /// <param name="minLength">Minimum distance from the center for each point.</param>
    /// <param name="maxLength">Maximum distance from the center for each point.</param>
    /// <param name="result">The destination polygon that will be cleared and populated with the generated vertices.</param>
    /// <returns><c>true</c> if the polygon parameters are valid and vertices were generated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The generated points are evenly distributed by angle and randomized by radial distance. If <paramref name="minLength"/> is greater than <paramref name="maxLength"/>, the values are swapped.
    /// </remarks>
    public static bool Generate(Vector2 center, int pointCount, float minLength, float maxLength, Polygon result)
    {
        if (pointCount < 3) return false;
        if (Math.Abs(minLength - maxLength) < ShapeMath.Epsilon) return false;
        if (minLength > maxLength)
        {
            //swap
            (minLength, maxLength) = (maxLength, minLength);
        }
        float angleStep = ShapeMath.PI * 2.0f / pointCount;
        result.Clear();
        for (int i = 0; i < pointCount; i++)
        {
            float randLength = Rng.Instance.RandF(minLength, maxLength);
            var p = ShapeVec.Right().Rotate(-angleStep * i) * randLength;
            p += center;
            result.Add(p);
        }

        return true;
    }

    /// <summary>
    /// Generates a polygonal shape around the specified <paramref name="segment"/> and appends its vertices to <paramref name="result"/>.
    /// </summary>
    /// <param name="segment">The segment to build a polygon around.</param>
    /// <param name="result">The destination polygon that receives the generated vertices.</param>
    /// <param name="magMin">The minimum perpendicular offset magnitude used for intermediate points on either side of the segment.</param>
    /// <param name="magMax">The maximum perpendicular offset magnitude used for intermediate points on either side of the segment.</param>
    /// <param name="minSectionLength">The minimum step size along the segment, expressed as a fraction of the segment length.</param>
    /// <param name="maxSectionLength">The maximum step size along the segment, expressed as a fraction of the segment length.</param>
    /// <returns><c>true</c> if the segment and generation parameters are valid and vertices were added; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The generated vertex order starts at <see cref="Segment.Start"/>, progresses toward <see cref="Segment.End"/> along one side of the segment, then returns along the opposite side. If the minimum values are greater than the corresponding maximum values, the pairs are swapped. This method does not clear <paramref name="result"/> before adding vertices.
    /// </remarks>
    public static bool Generate(Segment segment, Polygon result, float magMin = 0.1f, float magMax = 0.25f, float minSectionLength = 0.025f, float maxSectionLength = 0.1f)
    {
        if (segment.LengthSquared <= 0) return false;
        if (magMin <= 0 || magMax <= 0) return false;
        if (minSectionLength <= 0 || maxSectionLength <= 0) return false;
        if (magMin > magMax)
        {
            (magMin, magMax) = (magMax, magMin);
        }

        if (minSectionLength > maxSectionLength)
        {
            (minSectionLength, maxSectionLength) = (maxSectionLength, minSectionLength);       
        }
        result.Add(segment.Start);
        var dir = segment.Dir;
        var dirRight = dir.GetPerpendicularRight();
        var dirLeft = dir.GetPerpendicularLeft();
        float len = segment.Length;
        float minSectionLengthSq = (minSectionLength * len) * (minSectionLength * len);
        var cur = segment.Start;
        while (true)
        {
            cur += dir * Rng.Instance.RandF(minSectionLength, maxSectionLength) * len;
            if ((cur - segment.End).LengthSquared() < minSectionLengthSq) break;
            result.Add(cur + dirRight * Rng.Instance.RandF(magMin, magMax));
        }

        cur = segment.End;
        result.Add(cur);
        while (true)
        {
            cur -= dir * Rng.Instance.RandF(minSectionLength, maxSectionLength) * len;
            if ((cur - segment.Start).LengthSquared() < minSectionLengthSq) break;
            result.Add(cur + dirLeft * Rng.Instance.RandF(magMin, magMax));
        }

        return true;
    }
    
    
    #endregion
    
    #region Private
    internal static bool ContainsPointCheck(Vector2 a, Vector2 b, Vector2 pointToCheck)
    {
        if (a.Y < pointToCheck.Y && b.Y >= pointToCheck.Y || b.Y < pointToCheck.Y && a.Y >= pointToCheck.Y)
        {
            if (a.X + (pointToCheck.Y - a.Y) / (b.Y - a.Y) * (b.X - a.X) < pointToCheck.X)
            {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Recursively computes the minimal enclosing circle for a set of points using Welzl's algorithm.
    /// </summary>
    /// <param name="points">List of input points. Only the first <paramref name="n"/> elements are considered in this recursion.</param>
    /// <param name="boundary">Auxiliary list of boundary (support) points that must lie on the circle. Its size will be 0..3.</param>
    /// <param name="n">Number of points from <paramref name="points"/> to consider in this recursion.</param>
    /// <returns>The minimal enclosing <see cref="Circle"/> for the considered points given the current boundary.</returns>
    /// <remarks>
    /// The algorithm randomly selects a point from the first <paramref name="n"/> points, computes the minimal circle
    /// for the remaining points, and if the selected point lies outside that circle, it is added to the boundary
    /// and the algorithm recurses. Termination occurs when <paramref name="n"/> is 0 or the boundary contains 3 points.
    /// </remarks>
    private Circle WelzlHelper(List<Vector2> points, List<Vector2> boundary, int n)
    {
        if (n == 0 || boundary.Count == 3)
        {
            return GetCircleFromBoundary(boundary);
        }
    
        int idx = Rng.Instance.RandI(0, n);
        var p = points[idx];
        
        (points[idx], points[n - 1]) = (points[n - 1], points[idx]);
        
        var circle = WelzlHelper(points, boundary, n - 1);
        
        if (circle.ContainsPoint(p))
        {
            return circle;
        }
        
        boundary.Add(p);
        circle = WelzlHelper(points, boundary, n - 1);
        boundary.RemoveAt(boundary.Count - 1);
        
        return circle;
    }
    
    /// <summary>
    /// Constructs the minimal circle determined by the given boundary (support) points.
    /// </summary>
    /// <param name="boundary">A list of support points (0..3) which must lie on the resulting circle.</param>
    /// <returns>
    /// A <see cref="Circle"/> that is the minimal circle passing through the provided boundary points:
    /// - If <c>boundary.Count == 0</c> returns an empty/default circle.
    /// - If <c>boundary.Count == 1</c> returns a circle centered at the single point with radius 0.
    /// - If <c>boundary.Count == 2</c> returns the circle with the two points as endpoints of a diameter.
    /// - If <c>boundary.Count == 3</c> returns the circumcircle; if the three points are (nearly) collinear,
    ///   the function falls back to a circle using the largest pair as diameter.
    /// </returns>
    /// <remarks>
    /// This helper is used by Welzl's algorithm to compute the minimal enclosing circle for a set of points.
    /// Numerical stability is considered: when the determinant is very small the points are treated as collinear.
    /// </remarks>
    private Circle GetCircleFromBoundary(List<Vector2> boundary)
    {
        if (boundary.Count == 0) return new Circle();
        if (boundary.Count == 1) return new Circle(boundary[0], 0f);
        if (boundary.Count == 2)
        {
            var center1 = (boundary[0] + boundary[1]) * 0.5f;
            var radius1 = (boundary[1] - boundary[0]).Length() * 0.5f;
            return new Circle(center1, radius1);
        }
        
        var a = boundary[0];
        var b = boundary[1];
        var c = boundary[2];
        
        var d = 2f * (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));
        if (MathF.Abs(d) < 0.0001f)//check for collinearity
        {
            var center2 = (a + b) * 0.5f;
            // var radius2 = MathF.Max((b - a).Length(), (c - a).Length()) * 0.5f;
            var radius2 = MathF.Max(MathF.Max((b - a).Length(), (c - a).Length()), (c - b).Length()) * 0.5f;
            return new Circle(center2, radius2);
        }
        
        var aSq = a.LengthSquared();
        var bSq = b.LengthSquared();
        var cSq = c.LengthSquared();
        
        var ux = (aSq * (b.Y - c.Y) + bSq * (c.Y - a.Y) + cSq * (a.Y - b.Y)) / d;
        var uy = (aSq * (c.X - b.X) + bSq * (a.X - c.X) + cSq * (b.X - a.X)) / d;
        
        var center3 = new Vector2(ux, uy);
        var radius3 = (center3 - a).Length();
        
        return new Circle(center3, radius3);
    }
    #endregion
}
