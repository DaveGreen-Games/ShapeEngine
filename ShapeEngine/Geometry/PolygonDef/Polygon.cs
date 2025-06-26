using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;
using Size = ShapeEngine.Core.Structs.Size;

namespace ShapeEngine.Geometry.PolygonDef;

/// <summary>
/// Points shoud be in CCW order.
/// </summary>
public partial class Polygon : PointsDef.Points, IEquatable<Polygon>
{
    private static CollisionPoints collisionPointsReference = new(4);
    public override Polygon Copy() => new(this);
    public Vector2 Center => GetCentroid();
    public float Area => GetArea();
    public float Perimeter => GetPerimeter();
    public float Diameter => GetDiameter();
    
    #region Constructors
    public Polygon() { }
    public Polygon(int capacity) : base(capacity)
    {
        
    }
    
    /// <summary>
    /// Points should be in CCW order. Use Reverse if they are in CW order.
    /// </summary>
    /// <param name="points"></param>
    public Polygon(IEnumerable<Vector2> points) { AddRange(points); }
    public Polygon(PointsDef.Points points) : base(points.Count) { AddRange(points); }
    
    public Polygon(Polygon poly) : base(poly.Count) { AddRange(poly); }
    public Polygon(Polyline polyLine) : base(polyLine.Count) { AddRange(polyLine); }
    #endregion

    #region Equals & Hashcode
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

    public override int GetHashCode() => Game.GetHashCode(this);
    #endregion

    #region Public Functions
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

    public void FixWindingOrder()
    {
        if (IsClockwise())
        {
            Reverse();
        }
    }
    public void MakeClockwise()
    {
        if (IsClockwise()) return;
        Reverse();
    }
    public void MakeCounterClockwise()
    {
        if (!IsClockwise()) return;
        Reverse();
    }
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

    public void ReduceVertexCount(float factor)
    {
        ReduceVertexCount(Count - (int)(Count * factor));
    }
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
    public Vector2 GetVertex(int index) => this[ShapeMath.WrapIndex(Count, index)];

    public void RemoveColinearVertices()
    {
        if (Count < 3) return;
        PointsDef.Points result = new();
        for (int i = 0; i < Count; i++)
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
    public void RemoveDuplicates(float toleranceSquared = 0.001f)
    {
        if (Count < 3) return;
        PointsDef.Points result = new();

        for (var i = 0; i < Count; i++)
        {
            var cur = this[i];
            var next = Game.GetItem(this, i + 1);
            if ((cur - next).LengthSquared() > toleranceSquared) result.Add(cur);
        }
        Clear();
        AddRange(result);
    }
    public void Smooth(float amount, float baseWeight)
    {
        if (Count < 3) return;
        PointsDef.Points result = new();
        var centroid = GetCentroid();
        for (int i = 0; i < Count; i++)
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

    public PointsDef.Points ToRelativePoints(Transform2D transform)
    {
        var points = new PointsDef.Points(Count);
        for (int i = 0; i < Count; i++)
        {
            var p = transform.RevertPosition(this[i]);
            points.Add(p);
        }

        return points;
    }
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
    
    public Triangle GetBoundingTriangle(float margin = 3f) => Polygon.GetBoundingTriangle(this, margin);
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
    /// Return the segments of the polygon. If the points are in ccw winding order the normals face outward when InsideNormals = false 
    /// and face inside otherwise.
    /// </summary>
    /// <returns></returns>
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
    public Polygon ToConvex() => Polygon.FindConvexHull(this);
    #endregion

    #region Random

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
    public PointsDef.Points GetRandomPointsInside(int amount)
    {
        var triangles = Triangulate();
        WeightedItem<Triangle>[] items = new WeightedItem<Triangle>[triangles.Count];
        for (int i = 0; i < items.Length; i++)
        {
            var t = triangles[i];
            items[i] = new(t, (int)t.GetArea());
        }


        List<Triangle> pickedTriangles = Rng.Instance.PickRandomItems(amount, items);
        PointsDef.Points randomPoints = new();
        foreach (var tri in pickedTriangles) randomPoints.Add(tri.GetRandomPointInside());

        return randomPoints;
    }
    public Vector2 GetRandomVertex() { return Rng.Instance.RandCollection(this); }
    public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
    public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
    public PointsDef.Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);
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
}


