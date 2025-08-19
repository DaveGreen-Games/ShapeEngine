using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;
using Game = ShapeEngine.Core.GameDef.Game;
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
///A convex polygon is a polygon where all interior angles are less than 180°,
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
public partial class Polygon : Points, IEquatable<Polygon>
{
    private static IntersectionPoints intersectionPointsReference = new(4);
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
    
    private Polygon? compoundHelperPolygon = null;
    
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
    public bool Equals(Polygon? other)
    {
        if (other == null) return false;
        
        if (Count != other.Count) return false;
        for (var i = 0; i < Count; i++)
        {
            if (!this[i].IsSimilar(other[i])) return false;
            //if (this[i] != other[i]) return false;
        }
        return true;
    }
    /// <summary>
    /// Returns a hash code for the polygon.
    /// </summary>
    /// <returns>A hash code for the current polygon.</returns>
    public override int GetHashCode() => Game.GetHashCode(this);
    
    /// <summary>
    /// Determines whether the specified object is equal to the current polygon.
    /// </summary>
    /// <param name="obj">The object to compare with the current polygon.</param>
    /// <returns>True if the object is a <see cref="Polygon"/> and is equal to the current polygon; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Polygon);
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
    /// Removes colinear vertices from the polygon.
    /// </summary>
    /// <remarks>
    /// Colinear vertices are those that lie on a straight line with their neighbors.
    /// </remarks>
    public void RemoveColinearVertices()
    {
        if (Count < 3) return;
        Points result = [];
        for (var i = 0; i < Count; i++)
        {
            var cur = this[i];
            var prev = Game.GetItem(this, i - 1);
            var next = Game.GetItem(this, i + 1);

            var prevCur = prev - cur;
            var nextCur = next - cur;
            if (prevCur.Cross(nextCur) != 0f) result.Add(cur);
        }
        Clear();
        AddRange(result);
    }
    /// <summary>
    /// Removes duplicate vertices from the polygon.
    /// </summary>
    /// <param name="toleranceSquared">The squared distance below which two vertices are considered duplicates.
    /// Default is 0.001.</param>
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
    public Triangle GetBoundingTriangle(float margin = 3f) => Polygon.GetBoundingTriangle(this, margin);
    /// <summary>
    /// Triangulates the polygon into a set of triangles.
    /// </summary>
    /// <returns>A <see cref="Triangulation"/> containing the triangles.</returns>
    /// <remarks>
    /// Uses an ear-clipping algorithm. The polygon must be simple (non-self-intersecting).
    /// </remarks>
    public Triangulation Triangulate()
    {
        if (Count < 3) return new();
        if (Count == 3) return new() { new(this[0], this[1], this[2]) };

        Triangulation triangles = new();
        List<Vector2> vertices = new();
        vertices.AddRange(this);
        List<int> validIndices = new();
        for (int i = 0; i < vertices.Count; i++)
        {
            validIndices.Add(i);
        }
        while (vertices.Count > 3)
        {
            if (validIndices.Count <= 0) 
                break;

            int i = validIndices[Rng.Instance.RandI(0, validIndices.Count)];
            var a = vertices[i];
            var b = Game.GetItem(vertices, i + 1);
            var c = Game.GetItem(vertices, i - 1);

            var ba = b - a;
            var ca = c - a;
            float cross = ba.Cross(ca);
            if (cross >= 0f)//makes sure that ear is not self intersecting
            {
                validIndices.Remove(i);
                continue;
            }

            Triangle t = new(a, b, c);

            bool isValid = true;
            foreach (var p in this)
            {
                if (p == a || p == b || p == c) continue;
                if (t.ContainsPoint(p))
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                triangles.Add(t);
                vertices.RemoveAt(i);

                validIndices.Clear();
                for (int j = 0; j < vertices.Count; j++)
                {
                    validIndices.Add(j);
                }
                //break;
            }
        }


        triangles.Add(new(vertices[0], vertices[1], vertices[2]));


        return triangles;
    }
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
    /// Gets the minimal bounding circle of the polygon.
    /// </summary>
    /// <returns>The bounding <see cref="Circle"/>.</returns>
    public Circle GetBoundingCircle()
    {
        float maxD = 0f;
        int num = this.Count;
        Vector2 origin = new();
        for (int i = 0; i < num; i++) { origin += this[i]; }
        origin = origin / num;
        //origin *= (1f / (float)num);
        for (int i = 0; i < num; i++)
        {
            float d = (origin - this[i]).LengthSquared();
            if (d > maxD) maxD = d;
        }

        return new Circle(origin, MathF.Sqrt(maxD));
    }
    /// <summary>
    /// Gets the axis-aligned bounding box of the polygon.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public Rect GetBoundingBox()
    {
        if (Count < 2) return new();
        var start = this[0];
        Rect r = new(start.X, start.Y, 0, 0);

        foreach (var p in this)
        {
            r = r.Enlarge(p);// ShapeRect.Enlarge(r, p);
        }
        return r;
    }
    /// <summary>
    /// Returns the convex hull of the polygon as a new polygon.
    /// </summary>
    /// <returns>A convex polygon containing all the original points.</returns>
    public Polygon? ToConvex() => Polygon.FindConvexHull(this);
    #endregion

    #region Random
    /// <summary>
    /// Gets a random point inside the polygon using triangulation.
    /// </summary>
    /// <returns>A random point inside the polygon.</returns>
    public Vector2 GetRandomPointInside()
    {
        var triangles = Triangulate();
        List<WeightedItem<Triangle>> items = new();
        foreach (var t in triangles)
        {
            items.Add(new(t, (int)t.GetArea()));
        }
        var item = Rng.Instance.PickRandomItem(items.ToArray());
        return item.GetRandomPointInside();
    }
    /// <summary>
    /// Gets a set of random points inside the polygon.
    /// </summary>
    /// <param name="amount">The number of random points to generate.</param>
    /// <returns>A set of random points inside the polygon.</returns>
    public Points GetRandomPointsInside(int amount)
    {
        var triangles = Triangulate();
        var items = new WeightedItem<Triangle>[triangles.Count];
        for (var i = 0; i < items.Length; i++)
        {
            var t = triangles[i];
            items[i] = new(t, (int)t.GetArea());
        }


        var pickedTriangles = Rng.Instance.PickRandomItems(amount, items);
        Points randomPoints = [];
        foreach (var tri in pickedTriangles) randomPoints.Add(tri.GetRandomPointInside());

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
    public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
    /// <summary>
    /// Gets a random point on the polygon's edge.
    /// </summary>
    /// <returns>A random point on an edge.</returns>
    public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
    /// <summary>
    /// Gets a set of random points on the polygon's edges.
    /// </summary>
    /// <param name="amount">The number of random points to generate.</param>
    /// <returns>A set of random points on the edges.</returns>
    public Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);
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

    
    #endregion
    
    #region Cutout & Compound
    private bool GetPolygonShape(IShape shape, ref Polygon result)
    {
        switch (shape.GetShapeType())
        {
            default:
            case ShapeType.None: 
            case ShapeType.Ray:
            case ShapeType.Line:
            case ShapeType.Segment:
            case ShapeType.PolyLine:
                return false;
            case ShapeType.Circle:
                return shape.GetCircleShape().ToPolygon(ref result);
            case ShapeType.Triangle:
                shape.GetTriangleShape().ToPolygon(ref result);
                return true;
            case ShapeType.Quad:
                shape.GetQuadShape().ToPolygon(ref result);
                return true;
            case ShapeType.Rect:
                shape.GetRectShape().ToPolygon(ref result);
                return true;
            case ShapeType.Poly:
                return shape.GetPolygonShape().ToPolygon(ref result);
        }
    }
    
    /// <summary>
    /// Adds a compound shape to the polygon by converting the given <see cref="IShape"/> to a polygon and merging it.
    /// </summary>
    /// <param name="shape">The shape to add.</param>
    /// <returns>True if the shape was successfully merged; otherwise, false.</returns>
    public bool AddCompoundShape(IShape shape)
    {
        compoundHelperPolygon ??= [];
        var valid = GetPolygonShape(shape, ref compoundHelperPolygon);
        if (!valid || compoundHelperPolygon.Count <= 2) return false;
        return MergeShapeSelf(compoundHelperPolygon);
    
    }
    
    /// <summary>
    /// Adds a compound shape and its children from a <see cref="ShapeContainer"/> to the polygon.
    /// </summary>
    /// <param name="shape">The shape container to add.</param>
    /// <returns>The number of shapes successfully merged. (Parent Shape + Children Shapes) </returns>
    public int AddCompoundShape(ShapeContainer shape)
    {
        int count = 0;
        if (AddCompoundShape((IShape)shape)) count++;
        
        foreach (var child in shape.GetChildrenCopy())
        {
            if(AddCompoundShape((IShape)child))
            {
                count++;
            }
        }
    
        return count;
    }
    
    /// <summary>
    /// Adds a <see cref="Circle"/> shape to the polygon by converting it to a polygon with the specified number of points.
    /// </summary>
    /// <param name="shape">The circle to add.</param>
    /// <param name="pointCount">The number of points to use for the polygon approximation. Default is 16.</param>
    /// <returns>True if the shape was successfully merged; otherwise, false.</returns>
    public bool AddCompoundShape(Circle shape, int pointCount = 16)
    {
        compoundHelperPolygon ??= [];
        shape.ToPolygon(ref compoundHelperPolygon, pointCount);
        if(compoundHelperPolygon.Count <= 2) return false;
        return MergeShapeSelf(compoundHelperPolygon);
    }
    
    /// <summary>
    /// Adds a <see cref="Triangle"/> shape to the polygon by converting it to a polygon.
    /// </summary>
    /// <param name="shape">The triangle to add.</param>
    /// <returns>True if the shape was successfully merged; otherwise, false.</returns>
    public bool AddCompoundShape(Triangle shape)
    {
        compoundHelperPolygon ??= [];
        shape.ToPolygon(ref compoundHelperPolygon);
        if(compoundHelperPolygon.Count <= 2) return false;
        return MergeShapeSelf(compoundHelperPolygon);
    }
    
    /// <summary>
    /// Adds a <see cref="Quad"/> shape to the polygon by converting it to a polygon.
    /// </summary>
    /// <param name="shape">The quad to add.</param>
    /// <returns>True if the shape was successfully merged; otherwise, false.</returns>
    public bool AddCompoundShape(Quad shape)
    {
        compoundHelperPolygon ??= [];
        shape.ToPolygon(ref compoundHelperPolygon);
        if(compoundHelperPolygon.Count <= 2) return false;
        return MergeShapeSelf(compoundHelperPolygon);
    }
    
    /// <summary>
    /// Adds a <see cref="Rect"/> shape to the polygon by converting it to a polygon.
    /// </summary>
    /// <param name="shape">The rectangle to add.</param>
    /// <returns>True if the shape was successfully merged; otherwise, false.</returns>
    public bool AddCompoundShape(Rect shape)
    {
        compoundHelperPolygon ??= [];
        shape.ToPolygon(ref compoundHelperPolygon);
        if(compoundHelperPolygon.Count <= 2) return false;
        return MergeShapeSelf(compoundHelperPolygon);
    }
    
    /// <summary>
    /// Adds another <see cref="Polygon"/> shape to the polygon by merging it.
    /// </summary>
    /// <param name="shape">The polygon to add.</param>
    /// <returns>True if the shape was successfully merged; otherwise, false.</returns>
    public bool AddCompoundShape(Polygon shape)
    {
        if (shape.Count <= 2) return false;
        return MergeShapeSelf(shape);
    }
   
    
    /// <summary>
    /// Cuts out the given <see cref="IShape"/> from the polygon by converting it to a polygon and performing a cut operation.
    /// </summary>
    /// <param name="shape">The shape to cut out.</param>
    /// <returns>True if the cutout was successful; otherwise, false.</returns>
    /// <remarks>
    /// Does not support cutting out holes.
    /// If the shape for cutting is completely contained within the polygon, it will not be cut out!
    /// </remarks>
    public bool AddCutoutShape(IShape shape)
    {
        compoundHelperPolygon ??= [];
        bool valid = GetPolygonShape(shape, ref compoundHelperPolygon);
        if (!valid || compoundHelperPolygon.Count <= 2) return false;
        return CutShapeSelf(compoundHelperPolygon);
    }
    
    /// <summary>
    /// Cuts out the parent and child shapes from a <see cref="ShapeContainer"/> from the polygon.
    /// </summary>
    /// <param name="shape">The shape container whose shapes will be cut out.</param>
    /// <returns>The number of shapes successfully cut out.</returns>
    /// <remarks>
    /// Does not support cutting out holes.
    /// If the shape for cutting is completely contained within the polygon, it will not be cut out!
    /// </remarks>
    public int AddCutoutShape(ShapeContainer shape)
    {
        var count = 0;
        if (AddCutoutShape((IShape)shape)) count++;
        
        foreach (var child in shape.GetChildrenCopy())
        {
            if(AddCutoutShape((IShape)child))
            {
                count++;
            }
        }
    
        return count;
    }
    
    /// <summary>
    /// Cuts out a <see cref="Circle"/> shape from the polygon by converting it to a polygon with the specified number of points.
    /// </summary>
    /// <param name="shape">The circle to cut out.</param>
    /// <param name="pointCount">The number of points to use for the polygon approximation. Default is 16.</param>
    /// <returns>True if the cutout was successful; otherwise, false.</returns>
    /// <remarks>
    /// Does not support cutting out holes.
    /// If the shape for cutting is completely contained within the polygon, it will not be cut out!
    /// </remarks>
    public bool AddCutoutShape(Circle shape, int pointCount = 16)
    {
        compoundHelperPolygon ??= [];
        shape.ToPolygon(ref compoundHelperPolygon, pointCount);
        if(compoundHelperPolygon.Count <= 2) return false;
        return CutShapeSelf(compoundHelperPolygon);
    }
    
    /// <summary>
    /// Cuts out a <see cref="Triangle"/> shape from the polygon by converting it to a polygon.
    /// </summary>
    /// <param name="shape">The triangle to cut out.</param>
    /// <returns>True if the cutout was successful; otherwise, false.</returns>
    /// <remarks>
    /// Does not support cutting out holes.
    /// If the shape for cutting is completely contained within the polygon, it will not be cut out!
    /// </remarks>
    public bool AddCutoutShape(Triangle shape)
    {
        compoundHelperPolygon ??= [];
        shape.ToPolygon(ref compoundHelperPolygon);
        if(compoundHelperPolygon.Count <= 2) return false;
        return CutShapeSelf(compoundHelperPolygon);
    }
    
    /// <summary>
    /// Cuts out a <see cref="Quad"/> shape from the polygon by converting it to a polygon.
    /// </summary>
    /// <param name="shape">The quad to cut out.</param>
    /// <returns>True if the cutout was successful; otherwise, false.</returns>
    /// <remarks>
    /// Does not support cutting out holes.
    /// If the shape for cutting is completely contained within the polygon, it will not be cut out!
    /// </remarks>
    public bool AddCutoutShape(Quad shape)
    {
        compoundHelperPolygon ??= [];
        shape.ToPolygon(ref compoundHelperPolygon);
        if(compoundHelperPolygon.Count <= 2) return false;
        return CutShapeSelf(compoundHelperPolygon);
    }
    
    /// <summary>
    /// Cuts out a <see cref="Rect"/> shape from the polygon by converting it to a polygon.
    /// </summary>
    /// <param name="shape">The rectangle to cut out.</param>
    /// <returns>True if the cutout was successful; otherwise, false.</returns>
    /// <remarks>
    /// Does not support cutting out holes.
    /// If the shape for cutting is completely contained within the polygon, it will not be cut out!
    /// </remarks>
    public bool AddCutoutShape(Rect shape)
    {
        compoundHelperPolygon ??= [];
        shape.ToPolygon(ref compoundHelperPolygon);
        if(compoundHelperPolygon.Count <= 2) return false;
        return CutShapeSelf(compoundHelperPolygon);
    }
    
    /// <summary>
    /// Cuts out another <see cref="Polygon"/> shape from the polygon.
    /// </summary>
    /// <param name="shape">The polygon to cut out.</param>
    /// <returns>True if the cutout was successful; otherwise, false.</returns>
    /// <remarks>
    /// Does not support cutting out holes.
    /// If the shape for cutting is completely contained within the polygon, it will not be cut out!
    /// </remarks>
    public bool AddCutoutShape(Polygon shape)
    {
        if (shape.Count <= 2) return false;
        return CutShapeSelf(shape);
    }
    
    #endregion
}
