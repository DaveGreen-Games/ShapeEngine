using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Core;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;
using Size = ShapeEngine.Core.Structs.Size;

namespace ShapeEngine.Geometry.Polygon;

/// <summary>
/// Points shoud be in CCW order.
/// </summary>
public class Polygon : Points, IEquatable<Polygon>
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
    public Polygon(Points points) : base(points.Count) { AddRange(points); }
    
    public Polygon(Polygon poly) : base(poly.Count) { AddRange(poly); }
    public Polygon(Polyline.Polyline polyLine) : base(polyLine.Count) { AddRange(polyLine); }
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
    public Segment.Segment GetSegment(int index)
    {
        if (index < 0) return new Segment.Segment();
        if (Count < 2) return new Segment.Segment();
        var first = index % Count;
        var second = (index + 1) % Count;
        return new Segment.Segment(this[first], this[second]);
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
        Points result = new();
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
        Points result = new();

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
        Points result = new();
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

    public Points ToRelativePoints(Transform2D transform)
    {
        var points = new Points(Count);
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


    public Triangle.Triangle GetBoundingTriangle(float margin = 3f) => Polygon.GetBoundingTriangle(this, margin);

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

            Triangle.Triangle t = new(a, b, c);

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
    public Segments.Segments GetEdges()
    {
        if (Count <= 1) return new();
        if (Count == 2) return new() { new(this[0], this[1]) };
        Segments.Segments segments = new(Count);
        for (int i = 0; i < Count; i++)
        {
            segments.Add(new(this[i], this[(i + 1) % Count]));
        }
        return segments;
    }
    public Circle.Circle GetBoundingCircle()
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

        return new Circle.Circle(origin, MathF.Sqrt(maxD));
    }
    
    public Rect.Rect GetBoundingBox()
    {
        if (Count < 2) return new();
        var start = this[0];
        Rect.Rect r = new(start.X, start.Y, 0, 0);

        foreach (var p in this)
        {
            r = r.Enlarge(p);// ShapeRect.Enlarge(r, p);
        }
        return r;
    }
    public Polygon ToConvex() => Polygon.FindConvexHull(this);

    #endregion
    
    #region Math

    public Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        
        var points = new Points(Count);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }
        return points;
    }

    public Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        
        var points = new Points(Count);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }
        
        return Polygon.FindConvexHull(points);
    }
    public Vector2 GetCentroid()
    {
        if (Count <= 0) return new();
        if (Count == 1) return this[0];
        if (Count == 2) return (this[0] + this[1]) / 2;
        if (Count == 3) return (this[0] + this[1] + this[2]) / 3;
        
        var centroid = new Vector2();
        var area = 0f;
        for (int i = Count - 1; i >= 0; i--)
        {
            var a = this[i];
            var index = ShapeMath.WrapIndex(Count, i - 1);
            var b = this[index];
            float cross = a.X * b.Y - b.X * a.Y; //clockwise 
            area += cross;
            centroid += (a + b) * cross;
        }

        area *= 0.5f;
        return centroid / (area * 6);

        //return GetCentroidMean();
        // Vector2 result = new();

        // for (int i = 0; i < Count; i++)
        // {
        // var a = this[i];
        // var b = this[(i + 1) % Count];
        //// float factor = a.X * b.Y - b.X * a.Y; //clockwise 
        // float factor = a.Y * b.X - a.X * b.Y; //counter clockwise
        // result.X += (a.X + b.X) * factor;
        // result.Y += (a.Y + b.Y) * factor;
        // }

        // return result * (1f / (GetArea() * 6f));
    }

    public float GetPerimeter()
    {
        if (this.Count < 3) return 0f;
        float length = 0f;
        for (int i = 0; i < Count; i++)
        {
            Vector2 w = this[(i + 1)%Count] - this[i];
            length += w.Length();
        }
        return length;
    }
    public float GetPerimeterSquared()
    {
        if (Count < 3) return 0f;
        var lengthSq = 0f;
        for (var i = 0; i < Count; i++)
        {
            var w = this[(i + 1)%Count] - this[i];
            lengthSq += w.LengthSquared();
        }
        return lengthSq;
    }
    private float GetDiameterSquared()
    {
        if (Count <= 2) return 0;
        var center = GetCentroid();
        var maxDisSquared = -1f;
        for (int i = 0; i < Count; i++)
        {
            var p = this[0];
            var disSquared = (p - center).LengthSquared();
            if (maxDisSquared < 0 || disSquared > maxDisSquared)
            {
                maxDisSquared = disSquared;
            }
                
        }

        return maxDisSquared;
    }
    private float GetDiameter()
    {
        if (Count <= 2) return 0;
        return MathF.Sqrt(GetDiameterSquared());
    }
    public float GetArea()
    {
        if (Count < 3) return 0f;
        var area = 0f;
        for (int i = Count - 1; i >= 0; i--)//backwards to be clockwise
        {
            var a = this[i];
            var index = ShapeMath.WrapIndex(Count, i - 1);
            var b = this[index];
            float cross = a.X * b.Y - b.X * a.Y; //clockwise 
            area += cross;
        }

        return area/ 2f;
    }
    public bool IsClockwise() => GetArea() < 0f;

    public bool IsConvex()
    {
        int num = this.Count;
        bool isPositive = false;

        for (int i = 0; i < num; i++)
        {
            int prevIndex = (i == 0) ? num - 1 : i - 1;
            int nextIndex = (i == num - 1) ? 0 : i + 1;
            var d0 = this[i] - this[prevIndex];
            var d1 = this[nextIndex] - this[i];
            var newIsP = d0.Cross(d1) > 0f;
            if (i == 0) isPositive = true;
            else if (isPositive != newIsP) return false;
        }
        return true;
    }

    public Points ToPoints()
    {
        return new(this);
    }
    public Vector2 GetCentroidMean()
    {
        if (Count <= 0) return new(0f);
        if (Count == 1) return this[0];
        if (Count == 2) return (this[0] + this[1]) / 2;
        if (Count == 3) return (this[0] + this[1] + this[2]) / 3;
        var total = new Vector2(0f);
        foreach (var p in this) { total += p; }
        return total / Count;
    }
    /// <summary>
    /// Computes the length of this polygon's apothem. This will only be valid if
    /// the polygon is regular. More info: http://en.wikipedia.org/wiki/Apothem
    /// </summary>
    /// <returns>Return the length of the apothem.</returns>
    public float GetApothem() => (this.GetCentroid() - (this[0].Lerp(this[1], 0.5f))).Length();

    #endregion
    
    #region Transform
    public void SetPosition(Vector2 newPosition)
    {
        var centroid = GetCentroid();
        var delta = newPosition - centroid;
        ChangePosition(delta);
    }
    public void ChangeRotation(float rotRad)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }
    public void SetRotation(float angleRad)
    {
        if (Count < 3) return;

        var origin = GetCentroid();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }
    public void ScaleSize(float scale)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }
    public void ChangeSize(float amount)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
        
    }
    public void SetSize(float size)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }

    }

    
    public Polygon? SetPositionCopy(Vector2 newPosition)
    {
        if (Count < 3) return null;
        var centroid = GetCentroid();
        var delta = newPosition - centroid;
        return ChangePositionCopy(delta);
    }
    public new Polygon? ChangePositionCopy(Vector2 offset)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        for (int i = 0; i < Count; i++)
        {
            newPolygon.Add(this[i] + offset);
        }

        return newPolygon;
    }
    public new Polygon? ChangeRotationCopy(float rotRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.Rotate(rotRad));
        }

        return newPolygon;
    }

    public Polygon? ChangeRotationCopy(float rotRad)
    {
        if (Count < 3) return null;
        return ChangeRotationCopy(rotRad, GetCentroid());
    }

    public new Polygon? SetRotationCopy(float angleRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }
    public Polygon? SetRotationCopy(float angleRad)
    {
        if (Count < 3) return null;

        var origin = GetCentroid();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }
    public Polygon? ScaleSizeCopy(float scale)
    {
        if (Count < 3) return null;
        return ScaleSizeCopy(scale, GetCentroid());
    }
    public new Polygon? ScaleSizeCopy(float scale, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add( origin + w * scale);
        }

        return newPolygon;
    }
    public new Polygon? ScaleSizeCopy(Vector2 scale, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w * scale);
        }

        return newPolygon;
    }
    public new Polygon? ChangeSizeCopy(float amount, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.ChangeLength(amount));
        }

        return newPolygon;

    }
    public Polygon? ChangeSizeCopy(float amount)
    {
        if (Count < 3) return null;
        return ChangeSizeCopy(amount, GetCentroid());

    }

    public new Polygon? SetSizeCopy(float size, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.SetLength(size));
        }

        return newPolygon;
    }
    public Polygon? SetSizeCopy(float size)
    {
        if (Count < 3) return null;
        return SetSizeCopy(size, GetCentroid());

    }

   
    public new Polygon? SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = SetPositionCopy(transform.Position);
        if (newPolygon == null) return null;
        newPolygon.SetRotation(transform.RotationRad, origin);
        newPolygon.SetSize(transform.ScaledSize.Length, origin);
        return newPolygon;
    }
    
    public new Polygon? ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        if (Count < 3) return null;
        
        var newPolygon = ChangePositionCopy(offset.Position);
        if (newPolygon == null) return null;
        newPolygon.ChangeRotation(offset.RotationRad, origin);
        newPolygon.ChangeSize(offset.ScaledSize.Length, origin);
        return newPolygon;
    }
    
    #endregion
    
    #region Clipping

    public void UnionShapeSelf(Polygon b, FillRule fillRule = FillRule.NonZero)
    {
        var result = Clipper.Union(this.ToClipperPaths(), b.ToClipperPaths(), fillRule);
        if (result.Count > 0)
        {
            this.Clear();
            foreach (var p in result[0])
            {
                this.Add(p.ToVec2());
            }
        }
        

    }
    
    public bool MergeShapeSelf(Polygon other, float distanceThreshold)
    {
        var result = GetClosestPoint(other);
        if (result.Valid && result.DistanceSquared < distanceThreshold * distanceThreshold)
        {
            var fillShape = Polygon.Generate(result.Self.Point, 7, distanceThreshold, distanceThreshold * 2);
            UnionShapeSelf(fillShape);
            UnionShapeSelf(other);
        }

        return false;
    }
    public Polygon? MergeShape(Polygon other, float distanceThreshold)
    {
        var result = GetClosestPoint(other);
        if (result.Valid && result.DistanceSquared < distanceThreshold * distanceThreshold)
        {
            var fillShape = Polygon.Generate(result.Self.Point, 7, distanceThreshold, distanceThreshold * 2);
            var clip = ShapeClipper.Union(this, fillShape);
            if (clip.Count > 0)
            {
                clip = ShapeClipper.Union(clip[0].ToPolygon(), other);
                if (clip.Count > 0) return clip[0].ToPolygon();
            }
        }

        return null;
    }
    public (Polygons newShapes, Polygons cutOuts) CutShape(Polygon cutShape)
    {
        var cutOuts = ShapeClipper.Intersect(this, cutShape).ToPolygons(true);
        var newShapes = ShapeClipper.Difference(this, cutShape).ToPolygons(true);

        return (newShapes, cutOuts);
    }
    public (Polygons newShapes, Polygons cutOuts) CutShapeMany(Polygons cutShapes)
    {
        var cutOuts = ShapeClipper.IntersectMany(this, cutShapes).ToPolygons(true);
        var newShapes = ShapeClipper.DifferenceMany(this, cutShapes).ToPolygons(true);
        return (newShapes, cutOuts);
    }

    
    public (Polygons newShapes, Polygons overlaps) CombineShape(Polygon other)
    {
        var overlaps = ShapeClipper.Intersect(this, other).ToPolygons(true);
        var newShapes = ShapeClipper.Union(this, other).ToPolygons(true);
        return (newShapes, overlaps);
    }
    public (Polygons newShapes, Polygons overlaps) CombineShape(Polygons others)
    {
        var overlaps = ShapeClipper.IntersectMany(this, others).ToPolygons(true);
        var newShapes = ShapeClipper.UnionMany(this, others).ToPolygons(true);
        return (newShapes, overlaps);
    }
    public (Polygons newShapes, Polygons cutOuts) CutShapeSimple(Vector2 cutPos, float minCutRadius, float maxCutRadius, int pointCount = 16)
    {
        var cut = Generate(cutPos, pointCount, minCutRadius, maxCutRadius);
        return this.CutShape(cut);
    }
    public (Polygons newShapes, Polygons cutOuts) CutShapeSimple(Segment.Segment cutLine, float minSectionLength = 0.025f, float maxSectionLength = 0.1f, float minMagnitude = 0.05f, float maxMagnitude = 0.25f)
    {
        var cut = Generate(cutLine, minMagnitude, maxMagnitude, minSectionLength, maxSectionLength);
        return this.CutShape(cut);
    }
    
    public Polygons? Split(Vector2 point, Vector2 direction)
    {
        var line = new Line.Line(point, direction);
        return Split(line);
    }
    public Polygons? Split(Line.Line line)
    {
        var w = Center - line.Point;
        var l = w.Length();
        if (l < Diameter * 2f) l = Diameter * 2f;
        else l *= 2f;
        
        var segment = line.ToSegment(l);
        return Split(segment);
    }
    public Polygons? Split(Segment.Segment segment)
    {
        var result = this.Difference(segment);
        if (result.Count <= 0) return null;
        return result.ToPolygons();
    }
    public Polygons? Split(Segments.Segments segments)
    {
        var result = this.DifferenceMany(segments);
        if (result.Count <= 0) return null;
        return result.ToPolygons();
    }

    /// <summary>
    /// Generates a facture line in the direction of dir and splits the polygon with it.
    /// </summary>
    /// <param name="dir">The direction for the fracture.</param>
    /// <param name="maxOffsetPercentage">Max distance each point can be offset along the fracture line.
    /// Value Range 0-1.
    /// Relative to segment length of the fracture line.</param>
    /// <param name="fractureLineComplexity">How many points should be generated for the fracture line.</param>
    /// <returns></returns>
    public Polygons? FractureSplit(Vector2 dir, float maxOffsetPercentage,  int fractureLineComplexity)
    {
        if(fractureLineComplexity < 1 || dir.LengthSquared() <= 0f) return null;
        
        var center = Center;
        var result = IntersectShape(new Line.Line(center, dir));
        if (result == null || result.Count < 2) return null;

        var start = result[0].Point;
        var end = result[1].Point;
        if (result.Count > 2)
        {
            end = result.GetFurthestCollisionPoint(start).Point;
        }
        
        var fractureLine = GenerateFractureLine(start, end, maxOffsetPercentage, fractureLineComplexity);
        if(fractureLine == null) return null;
        return Split(fractureLine);
    }
    
    /// <summary>
    /// Generate a fracture line from start to end.
    /// </summary>
    /// <param name="start">Start of the fracture line.</param>
    /// <param name="end">End of the fracture line.</param>
    /// <param name="maxOffsetPercentage">How far each point can be offset from the main line.
    /// Value Range 0-1.
    /// Relative to the segment length.</param>
    /// <param name="linePoints">How many points should be generated. Final segment count = segmentPoints + 1.</param>
    /// <returns></returns>
    public Segments.Segments? GenerateFractureLine(Vector2 start, Vector2 end, float maxOffsetPercentage, int linePoints)
    {
        if (linePoints < 1) return null;
        if (maxOffsetPercentage < 0f) return [new Segment.Segment(start, end)];
        
        var w = end - start;
        var disSquared = w.LengthSquared();
        if (disSquared <= 0f) return null;
        
        var l = MathF.Sqrt(disSquared);
        var segmentLength = l / (linePoints + 1);
        var dir = w / l;
        var p = dir.GetPerpendicularLeft();
        var result = new Segments.Segments();

        var curStart = start;
        var curLinePoint = start;
        for (int i = 0; i < linePoints; i++)
        {
            var point = curStart + dir * segmentLength;
            curStart = point;
            
            var offsetLength = Rng.Instance.RandF(0f, segmentLength * maxOffsetPercentage);
            var offset = p * offsetLength;
            if (Rng.Instance.Chance(0.5f))
            {
                offset = -p * offsetLength;
            }

     
            var nextLinePoint = point + offset;

            var segment = new Segment.Segment(curLinePoint, nextLinePoint);
            curLinePoint = nextLinePoint;
            result.Add(segment);
        }
        result.Add(new Segment.Segment(curLinePoint, end));
        
        return result;
    }
    
    #endregion
    
    #region Random

    public Vector2 GetRandomPointInside()
    {
        var triangles = Triangulate();
        List<WeightedItem<Triangle.Triangle>> items = new();
        foreach (var t in triangles)
        {
            items.Add(new(t, (int)t.GetArea()));
        }
        var item = Rng.Instance.PickRandomItem(items.ToArray());
        return item.GetRandomPointInside();
    }
    public Points GetRandomPointsInside(int amount)
    {
        var triangles = Triangulate();
        WeightedItem<Triangle.Triangle>[] items = new WeightedItem<Triangle.Triangle>[triangles.Count];
        for (int i = 0; i < items.Length; i++)
        {
            var t = triangles[i];
            items[i] = new(t, (int)t.GetArea());
        }


        List<Triangle.Triangle> pickedTriangles = Rng.Instance.PickRandomItems(amount, items);
        Points randomPoints = new();
        foreach (var tri in pickedTriangles) randomPoints.Add(tri.GetRandomPointInside());

        return randomPoints;
    }
    public Vector2 GetRandomVertex() { return Rng.Instance.RandCollection(this); }
    public Segment.Segment GetRandomEdge() => GetEdges().GetRandomSegment();
    public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
    public Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);
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

    #region Static
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
    
    //Variant 2
    // private static bool IsPointInsidePolygon(Vector2 point, List<Vector2> polygon)
    // {
    //     bool inside = false;
    //     for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
    //     {
    //         if ((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
    //             (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
    //         {
    //             inside = !inside;
    //         }
    //     }
    //     return inside;
    // }
    
    /// <summary>
    /// Triangulates a set of points. Only works with non self intersecting shapes.
    /// </summary>
    /// <param name="points">The points to triangulate. Can be any set of points. (polygons as well) </param>
    /// <returns></returns>
    public static Triangulation TriangulateDelaunay(IEnumerable<Vector2> points)
    {
        var enumerable = points.ToList();
        var supraTriangle = GetBoundingTriangle(enumerable, 2f);
        return TriangulateDelaunay(enumerable, supraTriangle);
    }
    /// <summary>
    /// Triangulates a set of points. Only works with non self intersecting shapes.
    /// </summary>
    /// <param name="points">The points to triangulate. Can be any set of points. (polygons as well) </param>
    /// <param name="supraTriangle">The triangle that encapsulates all the points.</param>
    /// <returns></returns>
    public static Triangulation TriangulateDelaunay(IEnumerable<Vector2> points, Triangle.Triangle supraTriangle)
    {
        Triangulation triangles = new() { supraTriangle };

        foreach (var p in points)
        {
            Triangulation badTriangles = new();

            //Identify 'bad triangles'
            for (int triIndex = triangles.Count - 1; triIndex >= 0; triIndex--)
            {
                Triangle.Triangle triangle = triangles[triIndex];

                //A 'bad triangle' is defined as a triangle who's CircumCentre contains the current point
                var circumCircle = triangle.GetCircumCircle();
                float distSq = Vector2.DistanceSquared(p, circumCircle.Center);
                if (distSq < circumCircle.Radius * circumCircle.Radius)
                {
                    badTriangles.Add(triangle);
                    triangles.RemoveAt(triIndex);
                }
            }

            Segments.Segments allEdges = new();
            foreach (var badTriangle in badTriangles) { allEdges.AddRange(badTriangle.GetEdges()); }

            Segments.Segments uniqueEdges = GetUniqueSegmentsDelaunay(allEdges);
            //Create new triangles
            for (int i = 0; i < uniqueEdges.Count; i++)
            {
                var edge = uniqueEdges[i];
                triangles.Add(new(p, edge));
            }
        }

        //Remove all triangles that share a vertex with the supra triangle to recieve the final triangulation
        for (int i = triangles.Count - 1; i >= 0; i--)
        {
            var t = triangles[i];
            if (t.SharesVertex(supraTriangle)) triangles.RemoveAt(i);
        }


        return triangles;
    }
    private static Segments.Segments GetUniqueSegmentsDelaunay(Segments.Segments segments)
    {
        Segments.Segments uniqueEdges = new();
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            var edge = segments[i];
            if (IsSimilar(segments, edge))
            {
                uniqueEdges.Add(edge);
            }
        }
        return uniqueEdges;
    }
    private static bool IsSimilar(Segments.Segments segments, Segment.Segment seg)
    {
        var counter = 0;
        foreach (var segment in segments)
        {
            if (segment.IsSimilar(seg)) counter++;
            if (counter > 1) return false;
        }
        return true;
    }
    

    /// <summary>
    /// Get a rect that encapsulates all points.
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static Rect.Rect GetBoundingBox(IEnumerable<Vector2> points)
    {
        var enumerable = points as Vector2[] ?? points.ToArray();
        if (enumerable.Length < 2) return new();
        var start = enumerable.First();
        Rect.Rect r = new(start.X, start.Y, 0, 0);

        foreach (var p in enumerable)
        {
            r = r.Enlarge(p);
        }
        return r;
    }
    /// <summary>
    /// Get a triangle the encapsulates all points.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="marginFactor"> A factor for scaling the final triangle.</param>
    /// <returns></returns>
    public static Triangle.Triangle GetBoundingTriangle(IEnumerable<Vector2> points, float marginFactor = 1f)
    {
        var bounds = GetBoundingBox(points);
        float dMax = bounds.Size.Max() * marginFactor; // SVec.Max(bounds.BottomRight - bounds.BottomLeft) + margin; //  Mathf.Max(bounds.maxX - bounds.minX, bounds.maxY - bounds.minY) * Margin;
        Vector2 center = bounds.Center;

        ////The float 0.866 is an arbitrary value determined for optimum supra triangle conditions.
        //float x1 = center.X - 0.866f * dMax;
        //float x2 = center.X + 0.866f * dMax;
        //float x3 = center.X;
        //
        //float y1 = center.Y - 0.5f * dMax;
        //float y2 = center.Y - 0.5f * dMax;
        //float y3 = center.Y + dMax;
        //
        //Vector2 a = new(x1, y1);
        //Vector2 b = new(x2, y2);
        //Vector2 c = new(x3, y3);

        Vector2 a = new Vector2(center.X, bounds.BottomLeft.Y + dMax);
        Vector2 b = new Vector2(center.X - dMax * 1.25f, bounds.TopLeft.Y - dMax / 4);
        Vector2 c = new Vector2(center.X + dMax * 1.25f, bounds.TopLeft.Y - dMax / 4);


        return new Triangle.Triangle(a, b, c);
    }
    
    
    public static List<Vector2> GetSegmentAxis(Polygon p, bool normalized = false)
    {
        if (p.Count <= 1) return new();
        else if (p.Count == 2)
        {
            return new() { p[1] - p[0] };
        }
        List<Vector2> axis = new();
        for (int i = 0; i < p.Count; i++)
        {
            Vector2 start = p[i];
            Vector2 end = p[(i + 1) % p.Count];
            Vector2 a = end - start;
            axis.Add(normalized ? ShapeVec.Normalize(a) : a);
        }
        return axis;
    }
    public static List<Vector2> GetSegmentAxis(Segments.Segments edges, bool normalized = false)
    {
        List<Vector2> axis = new();
        foreach (var seg in edges)
        {
            axis.Add(normalized ? seg.Dir : seg.Displacement);
        }
        return axis;
    }

    
    
    public static Polygon GetShape(Points relative, Transform2D transform)
    {
        if (relative.Count < 3) return new();
        Polygon shape = new();
        for (int i = 0; i < relative.Count; i++)
        {
            shape.Add(transform.ApplyTransformTo(relative[i]));
            // shape.Add(pos + ShapeVec.Rotate(relative[i], rotRad) * scale);
        }
        return shape;
    }
    public static Polygon GenerateRelative(int pointCount, float minLength, float maxLength)
    {
        Polygon poly = new();
        float angleStep = ShapeMath.PI * 2.0f / pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            float randLength = Rng.Instance.RandF(minLength, maxLength);
            var p = ShapeVec.Rotate(ShapeVec.Right(), -angleStep * i) * randLength;
            poly.Add(p);
        }
        return poly;
    }
    
    public static Polygon Generate(Vector2 center, int pointCount, float minLength, float maxLength)
    {
        Polygon points = new();
        float angleStep = ShapeMath.PI * 2.0f / pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            float randLength = Rng.Instance.RandF(minLength, maxLength);
            Vector2 p = ShapeVec.Rotate(ShapeVec.Right(), -angleStep * i) * randLength;
            p += center;
            points.Add(p);
        }
        return points;
    }
    /// <summary>
    /// Generates a polygon around the given segment. Points are generated ccw around the segment beginning with the segment start.
    /// </summary>
    /// <param name="segment">The segment to build a polygon around.</param>
    /// <param name="magMin">The minimum perpendicular magnitude factor for generating a point. (0-1)</param>
    /// <param name="magMax">The maximum perpendicular magnitude factor for generating a point. (0-1)</param>
    /// <param name="minSectionLength">The minimum factor of the length between points along the line.(0-1)</param>
    /// <param name="maxSectionLength">The maximum factor of the length between points along the line.(0-1)</param>
    /// <returns>Returns the a generated polygon.</returns>
    public static Polygon Generate(Segment.Segment segment, float magMin = 0.1f, float magMax = 0.25f, float minSectionLength = 0.025f, float maxSectionLength = 0.1f)
    {
        Polygon poly = new() { segment.Start };
        var dir = segment.Dir;
        var dirRight = dir.GetPerpendicularRight();
        var dirLeft = dir.GetPerpendicularLeft();
        float len = segment.Length;
        float minSectionLengthSq = (minSectionLength * len) * (minSectionLength * len);
        Vector2 cur = segment.Start;
        while (true)
        {
            cur += dir * Rng.Instance.RandF(minSectionLength, maxSectionLength) * len;
            if ((cur - segment.End).LengthSquared() < minSectionLengthSq) break;
            poly.Add(cur + dirRight * Rng.Instance.RandF(magMin, magMax));
        }
        cur = segment.End;
        poly.Add(cur);
        while (true)
        {
            cur -= dir * Rng.Instance.RandF(minSectionLength, maxSectionLength) * len;
            if ((cur - segment.Start).LengthSquared() < minSectionLengthSq) break;
            poly.Add(cur + dirLeft * Rng.Instance.RandF(magMin, magMax));
        }
        return poly;
    }

    
    #endregion

    #region Closest Point
    public static Vector2 GetClosestPointPolygonPoint(List<Vector2> points, Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (points.Count <= 2) return new();
        
        var first = points[0];
        var second = points[1];
        var closest = Segment.Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);
        
        for (var i = 1; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                closest = cp;
                disSquared = dis;
            }
        
        }
        return closest;
    }
    
    public new CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 2) return new();
        
        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var closest = Segment.Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);
        
        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                closest = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        
        }
        return new(closest, normal.GetPerpendicularRight().Normalize());
    }
    public new CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 2) return new();
        
        var first = this[0];
        var second = this[1];
        index = 0;
        var normal = second - first;
        var closest = Segment.Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);
        
        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                index = i;
                normal = p2 - p1;
                closest = cp;
                disSquared = dis;
            }
        
        }
        return new(closest, normal.GetPerpendicularRight().Normalize());
    }
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 2) return new();
        
        index = 0;
        var closest = this[index];
        disSquared = (closest - p).LengthSquared();
        
        for (var i = 1; i < Count; i++)
        {
            var cp = this[i];
            var dis = (cp - p).LengthSquared();
            if (dis < disSquared)
            {
                index = i;
                closest = cp;
                disSquared = dis;
            }
        }
        return closest;
    }
    public Vector2 GetFurthestVertex(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 2) return new();
        
        index = 0;
        var furthest = this[index];
        disSquared = (furthest - p).LengthSquared();
        
        for (var i = 1; i < Count; i++)
        {
            var cp = this[i];
            var dis = (cp - p).LengthSquared();
            if (dis > disSquared)
            {
                index = i;
                furthest = cp;
                disSquared = dis;
            }
        }
        return furthest;
    }

    public new ClosestPointResult GetClosestPoint(Line.Line other)
    {
        if (Count <= 2) return new();
        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.Segment.GetClosestPointSegmentLine(first, second, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentLine(p1, p2, other.Point, other.Direction, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        
        }

        return new(
            new(result.self, normal.GetPerpendicularRight().Normalize()), 
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public new ClosestPointResult GetClosestPoint(Ray.Ray other)
    {
        if (Count <= 2) return new();
        
        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.Segment.GetClosestPointSegmentRay(first, second, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentRay(p1, p2, other.Point, other.Direction, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        
        }

        return new(
            new(result.self, normal.GetPerpendicularRight().Normalize()), 
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public new ClosestPointResult GetClosestPoint(Segment.Segment other)
    {
        if (Count <= 2) return new();
        
        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.Segment.GetClosestPointSegmentSegment(first, second, other.Start, other.End, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.Start, other.End, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        
        }

        return new (
            new(result.self, normal.GetPerpendicularRight().Normalize()), 
            new(result.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public new ClosestPointResult GetClosestPoint(Circle.Circle other)
    {
        if (Count <= 2) return new();
        
        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.Segment.GetClosestPointSegmentCircle(first, second, other.Center, other.Radius, out float disSquared);
        var selfIndex = 0;
        
        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentCircle(p1, p2, other.Center, other.Radius, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                result = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        }

        return new (
            new(result.self, normal.GetPerpendicularRight().Normalize()), 
            new(result.other, (result.other - other.Center).Normalize()),
            disSquared,
            selfIndex
        );
    }
    public new ClosestPointResult GetClosestPoint(Triangle.Triangle other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.B - other.A;
            }
            
            cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.C - other.B;
            }
            
            cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.A - other.C;
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public new ClosestPointResult GetClosestPoint(Quad.Quad other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.B - other.A;
            }
            
            cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.C - other.B;
            }
            
            cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.D - other.C;
            }
            
            cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.D, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 3;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.A - other.D;
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public new ClosestPointResult GetClosestPoint(Rect.Rect other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.B - other.A;
            }
            
            cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.C - other.B;
            }
            
            cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.D - other.C;
            }
            
            cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, other.D, other.A, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 3;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.A - other.D;
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public new ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            for (var j = 0; j < other.Count; j++)
            {
                var otherP1 = other[j];
                var otherP2 = other[(j + 1) % other.Count];
                var cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, otherP1, otherP2, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = otherP2 - otherP1;
                }
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public new ClosestPointResult GetClosestPoint(Polyline.Polyline other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            for (var j = 0; j < other.Count - 1; j++)
            {
                var otherP1 = other[j];
                var otherP2 = other[j + 1];
                var cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, otherP1, otherP2, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = otherP2 - otherP1;
                }
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public new ClosestPointResult GetClosestPoint(Segments.Segments other)
    {
        if (Count <= 2) return new();
        
        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;
        
        for (var i = 0; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];

            for (var j = 0; j < other.Count; j++)
            {
                var otherSegment = other[j];
                var cp = Segment.Segment.GetClosestPointSegmentSegment(p1, p2, otherSegment.Start, otherSegment.End, out float dis);
                if (dis < disSquared || disSquared < 0)
                {
                    selfIndex = i;
                    otherIndex = j;
                    result = cp;
                    disSquared = dis;
                    selfNormal = p2 - p1;
                    otherNormal = otherSegment.Normal;
                }
            }
        
        }

        return new(
            new(result.self, selfNormal.GetPerpendicularRight().Normalize()), 
            new(result.other, otherNormal),
            disSquared,
            selfIndex,
            otherIndex);
    }

    public (Segment.Segment segment, CollisionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {

        disSquared = -1;
        if (Count <= 2) return (new(), new());
        
        var closestSegment = new Segment.Segment(this[0], this[1]);
        var closest = closestSegment.GetClosestPoint(p, out disSquared);
        
        for (var i = 1; i < Count; i++)
        {
            var p1 = this[i];
            var p2 = this[(i + 1) % Count];
            
            var cp = Segment.Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                var normal = (p2 - p1).GetPerpendicularRight().Normalize();
                closest = new(cp, normal);
                closestSegment = new Segment.Segment(p1, p2);
                disSquared = dis;
            }
        
        }
        
        return new(closestSegment, closest);
    }
    
    
    #endregion
    
    #region Contains
    public static bool ContainsPoint(List<Vector2> polygon, Vector2 p)
    {
        var oddNodes = false;
        int num = polygon.Count;
        int j = num - 1;
        for (int i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if (ContainsPointCheck(vi, vj, p)) oddNodes = !oddNodes;
            j = i;
        }

        return oddNodes;
    }
    public static bool ContainsPoints(List<Vector2> polygon, Vector2 a, Vector2 b)
    {
        var oddNodesA = false;
        var oddNodesB = false;
        int num = polygon.Count;
        int j = num - 1;
        for (var i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if(ContainsPointCheck(vi, vj, a)) oddNodesA = !oddNodesA;
            if(ContainsPointCheck(vi, vj, b)) oddNodesB = !oddNodesB;
            
            j = i;
        }

        return oddNodesA && oddNodesB;
    }
    public static bool ContainsPoints(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c)
    {
        var oddNodesA = false;
        var oddNodesB = false;
        var oddNodesC = false;
        int num = polygon.Count;
        int j = num - 1;
        for (int i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if(ContainsPointCheck(vi, vj, a)) oddNodesA = !oddNodesA;
            if(ContainsPointCheck(vi, vj, b)) oddNodesB = !oddNodesB;
            if(ContainsPointCheck(vi, vj, c)) oddNodesC = !oddNodesC;
            
            j = i;
        }

        return oddNodesA && oddNodesB && oddNodesC;
    }
    public static bool ContainsPoints(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        var oddNodesA = false;
        var oddNodesB = false;
        var oddNodesC = false;
        var oddNodesD = false;
        int num = polygon.Count;
        int j = num - 1;
        for (int i = 0; i < num; i++)
        {
            var vi = polygon[i];
            var vj = polygon[j];
            if(ContainsPointCheck(vi, vj, a)) oddNodesA = !oddNodesA;
            if(ContainsPointCheck(vi, vj, b)) oddNodesB = !oddNodesB;
            if(ContainsPointCheck(vi, vj, c)) oddNodesC = !oddNodesC;
            if(ContainsPointCheck(vi, vj, d)) oddNodesD = !oddNodesD;
            
            j = i;
        }

        return oddNodesA && oddNodesB && oddNodesC && oddNodesD;
    }
    public static bool ContainsPoints(List<Vector2> polygon, List<Vector2> points)
    {
        if (polygon.Count <= 0 || points.Count <= 0) return false;
        foreach (var p in points)
        {
            if (!ContainsPoint(polygon, p)) return false;
        }
        return true;
    }
    //clean up
    public static bool ContainsPolygonSegment(List<Vector2> polygon, Vector2 segmentStart, Vector2 segmentEnd)
    {
        if (!ContainsPoints(polygon, segmentStart, segmentEnd)) return false;
        
        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            if (Segment.Segment.IntersectSegmentSegment(segmentStart, segmentEnd, polyStart, polyEnd).Valid)
            {
                return false;
            }
        }

        return true;
    }
    public static bool ContainsPolygonCircle(List<Vector2> polygon, Vector2 circleCenter, float circleRadius)
    {
        if (!ContainsPoint(polygon, circleCenter)) return false;
        
        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            var result = Segment.Segment.IntersectSegmentCircle(polyStart, polyEnd, circleCenter, circleRadius);
            if (result.a.Valid || result.b.Valid)
            {
                return false;
            }
        }

        return true;
    }
    public static bool ContainsPolygonTriangle(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c)
    {
        if (!ContainsPoints(polygon, a, b, c)) return false;
        
        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            if (Segment.Segment.IntersectSegmentSegment(polyStart, polyEnd, a, b).Valid)
            {
                return false;
            }
            if (Segment.Segment.IntersectSegmentSegment(polyStart, polyEnd, b, c).Valid)
            {
                return false;
            }
            if (Segment.Segment.IntersectSegmentSegment(polyStart, polyEnd, c, a).Valid)
            {
                return false;
            }
        }

        return true;
    }
    public static bool ContainsPolygonQuad(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (!ContainsPoints(polygon, a, b, c, d)) return false;
        
        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            if (Segment.Segment.IntersectSegmentSegment(polyStart, polyEnd, a, b).Valid)
            {
                return false;
            }
            if (Segment.Segment.IntersectSegmentSegment(polyStart, polyEnd, b, c).Valid)
            {
                return false;
            }
            if (Segment.Segment.IntersectSegmentSegment(polyStart, polyEnd, c, d).Valid)
            {
                return false;
            }
            if (Segment.Segment.IntersectSegmentSegment(polyStart, polyEnd, d, a).Valid)
            {
                return false;
            }
        }

        return true;
    }
    public static bool ContainsPolygonRect(List<Vector2> polygon, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsPolygonQuad(polygon, a, b, c, d);
    }
    public static bool ContainsPolygonPolyline(List<Vector2> polygon, List<Vector2> polyline)
    {
        if (!ContainsPoints(polygon, polyline)) return false;
        
        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            for (int j = 0; j < polyline.Count - 1; j++)
            {
                var polylineStart = polyline[j];
                var polylineEnd = polyline[j + 1];
                
                if (Segment.Segment.IntersectSegmentSegment(polyStart, polyEnd, polylineStart, polylineEnd).Valid)
                {
                    return false;
                }
            }
        }

        return true;
    }
    public static bool ContainsPolygonPolygon(List<Vector2> polygon, List<Vector2> other)
    {
        if (!ContainsPoints(polygon, other)) return false;
        
        for (int i = 0; i < polygon.Count; i++)
        {
            var polyStart = polygon[i];
            var polyEnd = polygon[(i + 1) % polygon.Count];
            for (int j = 0; j < other.Count; j++)
            {
                var polylineStart = other[j];
                var polylineEnd = other[(j + 1) % other.Count];
                
                if (Segment.Segment.IntersectSegmentSegment(polyStart, polyEnd, polylineStart, polylineEnd).Valid)
                {
                    return false;
                }
            }
        }

        return true;
    }
    
    
    public bool ContainsCollisionObject(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return false;
        foreach (var collider in collisionObject.Colliders)
        {
            if (!ContainsCollider(collider)) return false;
        }

        return true;
    }
    public bool ContainsCollider(Collider collider)
    {
        switch (collider.GetShapeType())
        {
            case ShapeType.Circle: return ContainsShape(collider.GetCircleShape());
            case ShapeType.Segment: return ContainsShape(collider.GetSegmentShape());
            case ShapeType.Triangle: return ContainsShape(collider.GetTriangleShape());
            case ShapeType.Quad: return ContainsShape(collider.GetQuadShape());
            case ShapeType.Rect: return ContainsShape(collider.GetRectShape());
            case ShapeType.Poly: return ContainsShape(collider.GetPolygonShape());
            case ShapeType.PolyLine: return ContainsShape(collider.GetPolylineShape());
            
            case ShapeType.Line:
            case ShapeType.Ray:
            case ShapeType.None:
            default:
                break;
        }

        return false;
    }
    public bool ContainsShape(Segment.Segment segment) => ContainsPolygonSegment(this, segment.Start, segment.End);
    public bool ContainsShape(Circle.Circle circle) => ContainsPolygonCircle(this, circle.Center, circle.Radius);
    public bool ContainsShape(Rect.Rect rect) => ContainsPolygonRect(this, rect.A, rect.B, rect.C, rect.D);
    public bool ContainsShape(Triangle.Triangle triangle) => ContainsPolygonTriangle(this, triangle.A, triangle.B, triangle.C);
    public bool ContainsShape(Quad.Quad quad) => ContainsPolygonQuad(this, quad.A, quad.B, quad.C, quad.D);
    public bool ContainsShape(Polyline.Polyline polyline) => ContainsPolygonPolyline(this, polyline);
    public bool ContainsShape(Polygon polygon) => ContainsPolygonPolygon(this, polygon);
    
    public bool ContainsPoint(Vector2 p)
    {
        return ContainsPoint(this, p);
    }
    public bool ContainsSegment(Vector2 segmentStart, Vector2 segmentEnd) => ContainsPolygonSegment(this, segmentStart, segmentEnd);
    public bool ContainsTriangle(Vector2 a, Vector2 b, Vector2 c) => ContainsPolygonTriangle(this, a, b, c);
    public bool ContainsQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => ContainsPolygonQuad(this, a, b, c, d);
    
    public bool ContainsPoints(Vector2 a, Vector2 b)
    {
        return ContainsPoints(this, a, b);
    }
    public bool ContainsPoints(Vector2 a, Vector2 b, Vector2 c)
    {
        return ContainsPoints(this, a, b, c);
    }
    public bool ContainsPoints(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsPoints(this, a, b, c, d);
    }
    public bool ContainsPoints(Points points)
    {
        return ContainsPoints(this, points);
    }
    
    #endregion
    
    #region Overlap
    public static bool OverlapPolygonSegment(List<Vector2> points, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.Segment.OverlapSegmentPolygon(segmentStart, segmentEnd, points);
    }
    public static bool OverlapPolygonLine(List<Vector2> points, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.Line.OverlapLinePolygon(linePoint, lineDirection, points);
    }
    public static bool OverlapPolygonRay(List<Vector2> points, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.Ray.OverlapRayPolygon(rayPoint, rayDirection, points);
    }
    public static bool OverlapPolygonCircle(List<Vector2> points, Vector2 circleCenter, float circleRadius)
    {
        return Circle.Circle.OverlapCirclePolygon(circleCenter, circleRadius, points);
    }
    public static bool OverlapPolygonTriangle(List<Vector2> points, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.Triangle.OverlapTrianglePolygon(ta, tb, tc, points);

    }
    public static bool OverlapPolygonQuad(List<Vector2> points, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.Quad.OverlapQuadPolygon(qa, qb, qc, qd, points);
    }
    public static bool OverlapPolygonRect(List<Vector2> points, Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.Quad.OverlapQuadPolygon(ra, rb, rc, rd, points);
    }
    public static bool OverlapPolygonPolygon(List<Vector2> points1, List<Vector2> points2)
    {
        if (points1.Count < 3 || points2.Count < 3) return false;
        
        var oddNodes1 = false;
        var oddNodes2 = false;
        var containsPoints2CheckFinished = false;

        var pointToCeck1 = points1[0];
        var pointToCeck2 = points2[0];
        
        for (var i = 0; i < points1.Count; i++)
        {
            var start1 = points1[i];
            var end1 = points1[(i + 1) % points1.Count];
            
            for (var j = 0; j < points2.Count; j++)
            {
                var start2 = points2[j];
                var end2 = points2[(j + 1) % points2.Count];
                if (Segment.Segment.OverlapSegmentSegment(start1, end1, start2, end2)) return true;
                
                if (containsPoints2CheckFinished) continue;
                if(Polygon.ContainsPointCheck(start2, end2, pointToCeck1)) oddNodes2 = !oddNodes2;
            }

            if (!containsPoints2CheckFinished)
            {
                if (oddNodes2) return true;
                containsPoints2CheckFinished = true;
            }
           
            if(Polygon.ContainsPointCheck(start1, end1, pointToCeck2)) oddNodes1 = !oddNodes1;
        }

        return oddNodes1 || oddNodes2;
    }
    public static bool OverlapPolygonPolyline(List<Vector2> points1, List<Vector2> points2)
    {
        if (points1.Count < 3 || points2.Count < 2) return false;
        
        var oddNodes = false;
        var pointToCeck = points2[0];

        
        for (var i = 0; i < points1.Count; i++)
        {
            var start1 = points1[i];
            var end1 = points1[(i + 1) % points1.Count];
            
            for (var j = 0; j < points2.Count - 1; j++)
            {
                var start2 = points2[j];
                var end2 = points2[j + 1];
                if (Segment.Segment.OverlapSegmentSegment(start1, end1, start2, end2)) return true;
            }

            if(Polygon.ContainsPointCheck(start1, end1, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }
    public static bool OverlapPolygonSegments(List<Vector2> points, List<Segment.Segment> segments)
    {
        if (points.Count < 3 || segments.Count <= 0) return false;
        
        var oddNodes = false;
        var pointToCeck = segments[0].Start;

        
        for (var i = 0; i < points.Count; i++)
        {
            var start = points[i];
            var end = points[(i + 1) % points.Count];

            foreach (var seg in segments)
            {
                if (Segment.Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }

            if(Polygon.ContainsPointCheck(start, end, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }

    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapPolygonSegment(this, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapPolygonLine(this, linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapPolygonRay(this, rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapPolygonCircle(this, circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapPolygonTriangle(this, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolygonQuad(this, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapPolygonQuad(this, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapPolygonPolygon(this, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapPolygonPolyline(this, points);
    public bool OverlapSegments(List<Segment.Segment> segments) => OverlapPolygonSegments(this, segments);
    
    public bool OverlapShape(Line.Line line) => OverlapPolygonLine(this, line.Point, line.Direction);
    public bool OverlapShape(Ray.Ray ray) => OverlapPolygonRay(this, ray.Point, ray.Direction);

    
    public bool Overlap(Collider collider)
    {
        if (!collider.Enabled) return false;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return OverlapShape(c);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return OverlapShape(s);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return OverlapShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return OverlapShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return OverlapShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return OverlapShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return OverlapShape(pl);
        }

        return false;
    }
    public bool OverlapShape(Segment.Segment s) => s.OverlapShape(this);
    public bool OverlapShape(Circle.Circle c) => c.OverlapShape(this);
    public bool OverlapShape(Triangle.Triangle t) => t.OverlapShape(this);
    public bool OverlapShape(Rect.Rect r) => r.OverlapShape(this);
    public bool OverlapShape(Quad.Quad q) => q.OverlapShape(this);
    public bool OverlapShape(Polygon b)
    {
        if (Count < 3 || b.Count < 3) return false;
        
        var oddNodesThis = false;
        var oddNodesB = false;
        var containsPointBCheckFinished = false;

        var pointToCeckThis = this[0];
        var pointToCeckB = b[0];
        
        for (var i = 0; i < Count; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];
            
            for (int j = 0; j < b.Count; j++)
            {
                var bStart = b[j];
                var bEnd = b[(j + 1) % b.Count];
                if (Segment.Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
                
                if (containsPointBCheckFinished) continue;
                if(Polygon.ContainsPointCheck(bStart, bEnd, pointToCeckThis)) oddNodesB = !oddNodesB;
            }

            if (!containsPointBCheckFinished)
            {
                if (oddNodesB) return true;
                containsPointBCheckFinished = true;
            }
           
            if(Polygon.ContainsPointCheck(start, end, pointToCeckB)) oddNodesThis = !oddNodesThis;
        }

        return oddNodesThis || oddNodesB;
    }
    public bool OverlapShape(Polyline.Polyline pl)
    {
        if (Count < 3 || pl.Count < 2) return false;
        
        var oddNodes = false;
        var pointToCeck = pl[0];

        
        for (var i = 0; i < Count; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];
            
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var bStart = pl[j];
                var bEnd = pl[j + 1];
                if (Segment.Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
            }

            if(Polygon.ContainsPointCheck(start, end, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }
    public bool OverlapShape(Segments.Segments segments)
    {
        if (Count < 3 || segments.Count <= 0) return false;
        
        var oddNodes = false;
        var pointToCeck = segments[0].Start;

        
        for (var i = 0; i < Count; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];

            foreach (var seg in segments)
            {
                if (Segment.Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
            }

            if(Polygon.ContainsPointCheck(start, end, pointToCeck)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }


    #endregion

    #region Intersect
    public static CollisionPoints? IntersectPolygonRay(List<Vector2> polygon, Vector2 rayPoint, Vector2 rayDirection)
    {
        if (polygon.Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < polygon.Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentRay(polygon[i], polygon[(i + 1) % polygon.Count], rayPoint, rayDirection);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }
    public static int IntersectPolygonRay(List<Vector2> polygon, Vector2 rayPoint, Vector2 rayDirection, ref CollisionPoints result)
    {
        if (polygon.Count < 3) return 0;
        int count = result.Count;
        for (var i = 0; i < polygon.Count; i++)
        {
            var point = Segment.Segment.IntersectSegmentRay(polygon[i], polygon[(i + 1) % polygon.Count], rayPoint, rayDirection);
            if (point.Valid)
            {
                result.Add(point);
            }
            
        }
        return result.Count - count;
    }
    
    /// <summary>
    /// This function intersects a ray with a polygon and returns all segments that lie inside the polygon.
    /// </summary>
    /// <param name="rayPoint"></param>
    /// <param name="rayDirection"></param>
    /// <returns></returns>
    public List<Segment.Segment>? CutRayWithPolygon(Vector2 rayPoint, Vector2 rayDirection)
    {
        if(Count < 3) return null;
        if(rayDirection.X == 0 && rayDirection.Y == 0) return null;
        
        rayDirection = rayDirection.Normalize();
        var intersectionPoints = IntersectPolygonRay(this, rayPoint, rayDirection);
        if(intersectionPoints == null || intersectionPoints.Count < 2) return null;

        intersectionPoints.SortClosestFirst(rayPoint);

        var segments = new List<Segment.Segment>();
        for (int i = 0; i < intersectionPoints.Count - 1; i+=2)
        {
            var segmentStart = intersectionPoints[i].Point;
            var segmentEnd = intersectionPoints[i + 1].Point;
            var segment = new Segment.Segment(segmentStart, segmentEnd);
            segments.Add(segment);
        }
        return segments;
    }
    /// <summary>
    /// This function intersects a ray with a polygon and returns all segments that lie inside the polygon.
    /// </summary>
    /// <param name="rayPoint"></param>
    /// <param name="rayDirection"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public int CutRayWithPolygon(Vector2 rayPoint, Vector2 rayDirection, ref List<Segment.Segment> result)
    {
        if(Count < 3) return 0;
        if(rayDirection.X == 0 && rayDirection.Y == 0) return 0;
        
        rayDirection = rayDirection.Normalize();
        var intersectionPoints = IntersectPolygonRay(this, rayPoint, rayDirection, ref collisionPointsReference);
        if(intersectionPoints < 2) return 0;

        int count = result.Count;
        collisionPointsReference.SortClosestFirst(rayPoint);

        for (int i = 0; i < collisionPointsReference.Count - 1; i+=2)
        {
            var segmentStart = collisionPointsReference[i].Point;
            var segmentEnd = collisionPointsReference[i + 1].Point;
            var segment = new Segment.Segment(segmentStart, segmentEnd);
            result.Add(segment);
        }
        collisionPointsReference.Clear();
        return result.Count - count;
    }
    
    public CollisionPoints? Intersect(Collider collider)
    {
        if (!collider.Enabled) return null;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl);
        }

        return null;
    }
    
    public CollisionPoints? IntersectShape(Ray.Ray r)
    {
        if (Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentRay(this[i], this[(i + 1) % Count], r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }

    public CollisionPoints? IntersectShape(Line.Line l)
    {
        if (Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentLine(this[i], this[(i + 1) % Count], l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Segment.Segment s)
    {
        if (Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],s.Start, s.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }
    
    public CollisionPoints? IntersectShape(Circle.Circle c)
    {
        if (Count < 3) return null;
        
        CollisionPoints? points = null;
        
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentCircle(this[i], this[(i + 1) % Count], c.Center, c.Radius);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if(result.a.Valid) points.Add(result.a);
                if(result.b.Valid) points.Add(result.b);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Triangle.Triangle t)
    {
        if (Count < 3) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], t.A, t.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], t.B, t.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], t.C, t.A);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Rect.Rect r)
    {
        if (Count < 3) return null;

        CollisionPoints? points = null;

        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;
        
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], a, b);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], b, c);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], c, d);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], d, a);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Quad.Quad q)
    {
        if (Count < 3) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],q.A, q.B);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.B, q.C);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],q.C, q.D);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],q.D, q.A);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Polygon p)
    {
        if (Count < 3 || p.Count < 3) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            for (var j = 0; j < p.Count; j++)
            {
                var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],p[j], p[(j + 1) % p.Count]);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Polyline.Polyline pl)
    {
        if (Count < 3 || pl.Count < 2) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],pl[j], pl[(j + 1) % pl.Count]);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Segments.Segments segments)
    {
        if (Count < 3 || segments.Count <= 0) return null;
        CollisionPoints? points = null;
        for (var i = 0; i < Count; i++)
        {
            foreach (var seg in segments)
            {
                var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],seg.Start, seg.End);
                if (result.Valid)
                {
                    points ??= new();
                    points.Add(result);
                }
            }
        }
        return points;
    }

    public int Intersect(Collider collider, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!collider.Enabled) return 0;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape, ref points);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l, ref points);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s, ref points);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int IntersectShape(Ray.Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentRay(this[i], this[(i + 1) % Count],r.Point, r.Direction, r.Normal);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    public int IntersectShape(Line.Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentLine(this[i], this[(i + 1) % Count],l.Point, l.Direction, l.Normal);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    public int IntersectShape(Segment.Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],s.Start, s.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    
    public int IntersectShape(Circle.Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;
        
        var count = 0;
        
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentCircle(this[i], this[(i + 1) % Count], c.Center, c.Radius);
            if (result.a.Valid)
            {
                points.Add(result.a);
                if (returnAfterFirstValid) return 1;
                count++;
            }

            if (result.b.Valid)
            {
                points.Add(result.b);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    public int IntersectShape(Triangle.Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], t.A, t.B);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], t.B, t.C);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], t.C, t.A);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    public int IntersectShape(Quad.Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],q.A, q.B);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.B, q.C);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],q.C, q.D);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],q.D, q.A);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    public int IntersectShape(Rect.Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3) return 0;

        var count = 0;

        var a = r.TopLeft;
        var b = r.BottomLeft;
        var c = r.BottomRight;
        var d = r.TopRight;
        
        for (var i = 0; i < Count; i++)
        {
            var result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], a, b);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], b, c);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], c, d);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            result = Segment.Segment.IntersectSegmentSegment( this[i], this[(i + 1) % Count], d, a);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3 || p.Count < 3) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            for (var j = 0; j < p.Count; j++)
            {
                var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],p[j], p[(j + 1) % p.Count]);
                if (result.Valid)
                {
                    points.Add(result);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
        }
        return count;
    }
    public int IntersectShape(Polyline.Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3 || pl.Count < 2) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            for (var j = 0; j < pl.Count - 1; j++)
            {
                var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],pl[j], pl[(j + 1) % pl.Count]);
                if (result.Valid)
                {
                    points.Add(result);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
        }
        return count;
    }
    public int IntersectShape(Segments.Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (Count < 3 || shape.Count <= 0) return 0;
        var count = 0;
        for (var i = 0; i < Count; i++)
        {
            foreach (var seg in shape)
            {
                var result = Segment.Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],seg.Start, seg.End);
                if (result.Valid)
                {
                    points.Add(result);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
        }
        return count;
    }
   
    #endregion

    #region Convex Hull
    //ALternative algorithms
        //https://en.wikipedia.org/wiki/Graham_scan
        //https://en.wikipedia.org/wiki/Chan%27s_algorithm
        
    //GiftWrapping
    //https://www.youtube.com/watch?v=YNyULRrydVI -> coding train
    //https://en.wikipedia.org/wiki/Gift_wrapping_algorithm -> wiki
    public static Polygon FindConvexHull(List<Vector2> points) => ConvexHull_JarvisMarch(points);
    public static Polygon FindConvexHull(Points points) => ConvexHull_JarvisMarch(points);
    public static Polygon FindConvexHull(params Vector2[] points) => ConvexHull_JarvisMarch(points.ToList());
    public static Polygon FindConvexHull(Polygon points) => ConvexHull_JarvisMarch(points);
    public static Polygon FindConvexHull(params Polygon[] shapes)
    {
        var allPoints = new List<Vector2>();
        foreach (var shape in shapes)
        {
            allPoints.AddRange(shape);
        }
        return ConvexHull_JarvisMarch(allPoints);
    }
    
    #endregion
    
    #region Jarvis March Algorithm (Find Convex Hull)

    //SOURCE https://github.com/allfii/ConvexHull/tree/master
    
    private static int Turn_JarvisMarch(Vector2 p, Vector2 q, Vector2 r)
    {
        return ((q.X - p.X) * (r.Y - p.Y) - (r.X - p.X) * (q.Y - p.Y)).CompareTo(0);
        // return ((q.getX() - p.getX()) * (r.getY() - p.getY()) - (r.getX() - p.getX()) * (q.getY() - p.getY())).CompareTo(0);
    }
    private static Vector2 NextHullPoint_JarvisMarch(List<Vector2> points, Vector2 p)
    {
        // const int TurnLeft = 1;
        const int turnRight = -1;
        const int turnNone = 0;
        var q = p;
        int t;
        foreach (var r in points)
        {
            t = Turn_JarvisMarch(p, q, r);
            if (t == turnRight || t == turnNone && p.DistanceSquared(r) > p.DistanceSquared(q)) // dist(p, r) > dist(p, q))
                q = r;
        }
        return q;
    }
    private static Polygon ConvexHull_JarvisMarch(List<Vector2> points)
    {
        var hull = new List<Vector2>();
        foreach (var p in points)
        {
            if (hull.Count == 0)
                hull.Add(p);
            else
            {
                if (hull[0].X > p.X)
                    hull[0] = p;
                else if (ShapeMath.EqualsF(hull[0].X, p.X))
                    if (hull[0].Y > p.Y)
                        hull[0] = p;
            }
        }
        var counter = 0;
        while (counter < hull.Count)
        {
            var q = NextHullPoint_JarvisMarch(points, hull[counter]);
            if (q != hull[0])
            {
                hull.Add(q);
            }
            counter++;
        }
        return new Polygon(hull);
    }
    #endregion
}


