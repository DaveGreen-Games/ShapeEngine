
using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Shapes;

public readonly struct Circle : IEquatable<Circle>
{
    #region Members
    public readonly Vector2 Center;
    public readonly float Radius;
    #endregion

    #region Getter Setter
    public float Diameter => Radius * 2f;
    public Vector2 Top => Center + new Vector2(0, -Radius);
    public Vector2 Right => Center + new Vector2(Radius, 0);
    public Vector2 Bottom => Center + new Vector2(0, Radius);
    public Vector2 Left => Center + new Vector2(-Radius, 0);
    public Vector2 TopLeft => Center + new Vector2(-Radius, -Radius);
    public Vector2 BottomLeft => Center + new Vector2(-Radius, Radius);
    public Vector2 BottomRight => Center + new Vector2(Radius, Radius);
    public Vector2 TopRight => Center + new Vector2(Radius, -Radius);
    #endregion
    
    #region Constructors
    public Circle(Vector2 center, float radius) { this.Center = center; this.Radius = radius; }
    public Circle(float x, float y, float radius) { this.Center = new(x, y); this.Radius = radius; }
    public Circle(Circle c, float radius) { Center = c.Center; Radius = radius;}
    public Circle(Circle c, Vector2 center) { Center = center; Radius = c.Radius; }
    public Circle(Rect r) { Center = r.Center; Radius = MathF.Max(r.Width, r.Height); }
    public Circle(Transform2D transform) { Center = transform.Position; Radius = transform.ScaledSize.Radius; }
    #endregion

    #region Equality & Hashcode
    public bool Equals(Circle other)
    {
        return Center == other.Center && ShapeMath.EqualsF(Radius, other.Radius);// Radius == other.Radius;
    }
    public readonly override int GetHashCode() => HashCode.Combine(Center, Radius);

    public static bool operator ==(Circle left, Circle right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(Circle left, Circle right)
    {
        return !(left == right);
    }
    public override bool Equals(object? obj)
    {
        if (obj is Circle c) return Equals(c);
        return false;
    }
    #endregion

    #region Math

    public Points? GetProjectedShapePoints(Vector2 v, int pointCount = 8)
    {
        if (pointCount < 4 || v.LengthSquared() <= 0f) return null;
        float angleStep = (MathF.PI * 2f) / pointCount;
        Points points = new(pointCount * 2);
        for (var i = 0; i < pointCount; i++)
        {
            var p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            points.Add(p);
            points.Add(p + v);
        }
        return points;
    }

    public Polygon? ProjectShape(Vector2 v, int pointCount = 8)
    {
        if (pointCount < 4 || v.LengthSquared() <= 0f) return null;
        float angleStep = (MathF.PI * 2f) / pointCount;
        Points points = new(pointCount * 2);
        for (var i = 0; i < pointCount; i++)
        {
            var p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            points.Add(p);
            points.Add(p + v);
        }
        return Polygon.FindConvexHull(points);
    }
    
    public Circle Floor() { return new(Center.Floor(), MathF.Floor(Radius)); }
    public Circle Ceiling() { return new(Center.Ceiling(), MathF.Ceiling(Radius)); }
    public Circle Round() { return new(Center.Round(), MathF.Round(Radius)); }
    public Circle Truncate() { return new(Center.Truncate(), MathF.Truncate(Radius)); }
    
    public float GetArea() { return MathF.PI * Radius * Radius; }
    public float GetCircumference() { return MathF.PI * Radius * 2f; }
    public static float GetCircumference(float radius) { return MathF.PI * radius * 2f; }
    public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }

    #endregion

    #region Points & Vertext

    public Vector2 GetVertex(float angleRad, float angleStepRad, int index)
    {
        return Center + new Vector2(Radius, 0f).Rotate(angleRad + angleStepRad * index);
    }
    
    public Vector2 GetPoint(float angleRad, float f) { return Center + new Vector2(Radius * f, 0f).Rotate(angleRad); }
    public Vector2 GetRandomPoint()
    {
        float randAngle = Rng.Instance.RandAngleRad();
        var randDir = ShapeVec.VecFromAngleRad(randAngle);
        return Center + randDir * Rng.Instance.RandF(0, Radius);
    }
    public Points GetRandomPoints(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPoint());
        }
        return points;
    }
    public Vector2 GetRandomVertex() { return Rng.Instance.RandCollection(GetVertices(), false); }
    public Segment GetRandomEdge() { return Rng.Instance.RandCollection(GetEdges(), false); }
    public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
    public Points GetRandomPointsOnEdge(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPointOnEdge());
        }
        return points;
    }


    #endregion

    #region Shapes

    public Segments GetEdges(int pointCount = 16)
    {
        float angleStep = (MathF.PI * 2f) / pointCount;
        Segments segments = new();
        for (int i = 0; i < pointCount; i++)
        {
            var start = Center + new Vector2(Radius, 0f).Rotate(-angleStep * i);
            var end = Center + new Vector2(Radius, 0f).Rotate(-angleStep * ((i + 1) % pointCount));

            segments.Add(new Segment(start, end));
        }
        return segments;
    }
    public Points GetVertices(int count = 16)
    {
        float angleStep = (MathF.PI * 2f) / count;
        Points points = new();
        for (int i = 0; i < count; i++)
        {
            Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            points.Add(p);
        }
        return points;
    }
    public Polygon ToPolygon(int pointCount = 16)
    {
        float angleStep = (MathF.PI * 2f) / pointCount;
        Polygon poly = new();
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            poly.Add(p);
        }
        return poly;
    }
    public Polyline ToPolyline(int pointCount = 16)
    {
        float angleStep = (MathF.PI * 2f) / pointCount;
        Polyline polyLine = new();
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            polyLine.Add(p);
        }
        return polyLine;
    }
    public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
    public Rect GetBoundingBox() { return new Rect(Center, new Size(Radius, Radius) * 2f, new(0.5f)); }

    public Circle Combine(Circle other)
    {
        return new
        (
            (Center + other.Center) / 2,
            Radius + other.Radius
        );
    }
    
    #endregion
    
    #region Corners
    
    public (Vector2 top, Vector2 right, Vector2 bottom, Vector2 left) GetCorners()
    {
        var top = Center + new Vector2(0, -Radius);
        var right = Center + new Vector2(Radius, 0);
        var bottom = Center + new Vector2(0, Radius);
        var left = Center + new Vector2(-Radius, 0);
        return (top, right, bottom, left);
    }
    public List<Vector2> GetCornersList()
    {
        var top = Center + new Vector2(0, -Radius);
        var right = Center + new Vector2(Radius, 0);
        var bottom = Center + new Vector2(0, Radius);
        var left = Center + new Vector2(-Radius, 0);
        return new() { top, right, bottom, left };
    }
    public (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetRectCorners()
    {
        var tl = Center + new Vector2(-Radius, -Radius);
        var tr = Center + new Vector2(Radius, -Radius);
        var br = Center + new Vector2(Radius, Radius);
        var bl = Center + new Vector2(-Radius, Radius);
        return (tl, tr, br, bl);
    }
    public List<Vector2> GetRectCornersList()
    {
        var tl = Center + new Vector2(-Radius, -Radius);
        var tr = Center + new Vector2(Radius, -Radius);
        var br = Center + new Vector2(Radius, Radius);
        var bl = Center + new Vector2(-Radius, Radius);
        return new() {tl, tr, br, bl};
    }
    #endregion

    #region Transform

    public Circle ScaleRadius(float scale) => new(Center, Radius * scale);
    public Circle ChangeRadius(float amount) => new(Center, Radius + amount);
    public Circle SetRadius(float radius) => new(Center, radius);


    public Circle ChangeRotation(float rotationRad, Vector2 pivot)
    {
        var w = Center - pivot;
        var rotated = w.Rotate(rotationRad);
        return new(pivot + rotated, Radius);
    }
    
    
    public Circle ChangePosition(Vector2 offset) => this + offset;
    public Circle ChangePosition(float x, float y) => this + new Vector2(x, y);
    public Circle SetPosition(Vector2 position) => new Circle(position, Radius);
    
    
    
    // public Transform2D GetOffset() => new Transform2D(Center, 0f, new Size(Radius, Radius), new Vector2(1f, 1f));
    
    /// <summary>
    /// Moves the circle by offset.Position
    /// Changes the radius of the moved circle by transform.ScaledSize.Radius!
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public Circle ApplyOffset(Transform2D offset)
    {
        var newCircle = ChangePosition(offset.Position);
        return newCircle.ChangeRadius(offset.ScaledSize.Radius);
    }

    /// <summary>
    /// Moves the circle to transform.Position
    /// Set the radius of the moved circle to ScaledSize.Radius.
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public Circle SetTransform(Transform2D transform)
    {
        var newCircle = SetPosition(transform.Position);
        return newCircle.SetRadius(transform.ScaledSize.Radius);
    }


    #endregion
    
    #region Operators

    public static Circle operator +(Circle left, Circle right)
    {
        return new
            (
                left.Center + right.Center,
                left.Radius + right.Radius
            );
    }
    public static Circle operator -(Circle left, Circle right)
    {
        return new
        (
            left.Center - right.Center,
            left.Radius - right.Radius
        );
    }
    public static Circle operator *(Circle left, Circle right)
    {
        return new
        (
            left.Center * right.Center,
            left.Radius * right.Radius
        );
    }
    public static Circle operator /(Circle left, Circle right)
    {
        return new
        (
            left.Center / right.Center,
            left.Radius / right.Radius
        );
    }
    public static Circle operator +(Circle left, Vector2 right)
    {
        return new
        (
            left.Center + right,
            left.Radius
        );
    }
    public static Circle operator -(Circle left, Vector2 right)
    {
        return new
        (
            left.Center - right,
            left.Radius
        );
    }
    public static Circle operator *(Circle left, Vector2 right)
    {
        return new
        (
            left.Center * right,
            left.Radius
        );
    }
    public static Circle operator /(Circle left, Vector2 right)
    {
        return new
        (
            left.Center / right,
            left.Radius
        );
    }
    public static Circle operator +(Circle left, float right)
    {
        return new
        (
            left.Center,
            left.Radius + right
        );
    }
    public static Circle operator -(Circle left, float right)
    {
        return new
        (
            left.Center,
            left.Radius - right
        );
    }
    public static Circle operator *(Circle left, float right)
    {
        return new
        (
            left.Center,
            left.Radius * right
        );
    }
    public static Circle operator /(Circle left, float right)
    {
        return new
        (
            left.Center,
            left.Radius / right
        );
    }
    #endregion

    #region Contains

    public bool ContainsPoint(Vector2 p) => (Center - p).LengthSquared() <= Radius * Radius;
    public bool ContainsPointSector(Vector2 p, float rotationRad, float sectorAngleRad)
    {
        if(sectorAngleRad <= 0f) return false;
        rotationRad = ShapeMath.WrapAngleRad(rotationRad);

        var dir = ShapeVec.VecFromAngleRad(rotationRad);
        var a = dir.AngleRad(p - Center);
        return MathF.Abs(a) < sectorAngleRad * 0.5f;
    }
    public bool ContainsPointSector(Vector2 p, Vector2 dir, float sectorAngleRad)
    {
        if(sectorAngleRad <= 0f) return false;
        if(dir.X == 0f && dir.Y == 0f) return false;
        if (!ContainsPoint(p)) return false;
        
        var a = dir.AngleRad(p - Center);
        return MathF.Abs(a) < sectorAngleRad * 0.5f;
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
        }

        return false;
    }
    public bool ContainsShape(Segment segment)
    {
        return ContainsPoint(segment.Start) && ContainsPoint(segment.End);
    }
    public bool ContainsShape(Circle circle)
    {
        float rDif = Radius - circle.Radius;
        if(rDif <= 0) return false;

        float disSquared = (Center - circle.Center).LengthSquared();
        return disSquared < rDif * rDif;
    }
    public bool ContainsShape(Rect rect)
    {
        return ContainsPoint(rect.TopLeft) &&
            ContainsPoint(rect.BottomLeft) &&
            ContainsPoint(rect.BottomRight) &&
            ContainsPoint(rect.TopRight);
    }
    public bool ContainsShape(Triangle triangle)
    {
        return ContainsPoint(triangle.A) &&
            ContainsPoint(triangle.B) &&
            ContainsPoint(triangle.C);
    }
    public bool ContainsShape(Quad quad)
    {
        return ContainsPoint(quad.A) &&
               ContainsPoint(quad.B) &&
               ContainsPoint(quad.C) &&
               ContainsPoint(quad.D);
    }
    public bool ContainsShape(Points points)
    {
        if (points.Count <= 0) return false;
        foreach (var p in points)
        {
            if (!ContainsPoint(p)) return false;
        }
        return true;
    }
    #endregion
    
    #region Closest Point
    public static Vector2 GetClosestPointCirclePoint(Vector2 center, float radius, Vector2 p, out float disSquared)
    {
        var dir = (p - center).Normalize();
        var closestPoint = center + dir * radius;
        disSquared = (closestPoint - p).LengthSquared();
        return closestPoint;
    }
    public static (Vector2 self, Vector2 other) GetClosestPointCircleSegment(Vector2 circleCenter, float circleRadius, Vector2 segmentStart, Vector2 segmentEnd, out float disSquared)
    {
        var d1 = segmentEnd - segmentStart;

        var toCenter = circleCenter - segmentStart;
        float projectionLength = Vector2.Dot(toCenter, d1) / d1.LengthSquared();
        projectionLength = Math.Clamp(projectionLength, 0.0f, 1.0f);
        var closestPointOnSegment = segmentStart + projectionLength * d1;

        var offset = Vector2.Normalize(closestPointOnSegment - circleCenter) * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnCircle - closestPointOnSegment).LengthSquared();
        return (closestPointOnCircle, closestPointOnSegment);
    }
    public static (Vector2 self, Vector2 other) GetClosestPointCircleLine(Vector2 circleCenter, float circleRadius, Vector2 linePoint, Vector2 lineDirection, out float disSquared)
    {
        var d1 = lineDirection.Normalize();

        var toCenter = circleCenter - linePoint;
        float projectionLength = Vector2.Dot(toCenter, d1);
        var closestPointOnLine = linePoint + projectionLength * d1;

        var offset = (closestPointOnLine - circleCenter).Normalize() * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnLine - closestPointOnCircle).LengthSquared();
        return (closestPointOnCircle, closestPointOnLine);
    }
    public static (Vector2 self, Vector2 other) GetClosestPointCircleRay(Vector2 circleCenter, float circleRadius, Vector2 rayPoint, Vector2 rayDirection, out float disSquared)
    {
        var d1 = rayDirection.Normalize();

        var toCenter = circleCenter - rayPoint;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        var closestPointOnRay = rayPoint + projectionLength * d1;

        var offset = (closestPointOnRay - circleCenter).Normalize() * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnRay - closestPointOnCircle).LengthSquared();
        return (closestPointOnCircle, closestPointOnRay);
    }
    public static (Vector2 self, Vector2 other) GetClosestPointCircleCircle(Vector2 circle1Center, float circle1Radius, Vector2 circle2Center, float circle2Radius, out float disSquared)
    {
        var w = circle1Center - circle2Center;
        var dir = w.Normalize();
        var a = circle1Center - dir * circle1Radius;
        var b = circle2Center + dir * circle2Radius;
        disSquared = (a - b).LengthSquared();
        return (a, b);
    }
   
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        var dir = (p - Center).Normalize();
        var closestPoint = Center + dir * Radius;
        var normal = (closestPoint - Center).Normalize();
        disSquared = (closestPoint - p).LengthSquared();
        return new(closestPoint, normal);
    }
    public ClosestPointResult GetClosestPoint(Line other)
    {
        var d1 = other.Direction;

        var toCenter = Center - other.Point;
        float projectionLength = Vector2.Dot(toCenter, d1);
        var closestPointOnLine = other.Point + projectionLength * d1;

        var offset = (closestPointOnLine - Center).Normalize() * Radius;
        var closestPointOnCircle = Center + offset;
        float disSquared = (closestPointOnLine - closestPointOnCircle).LengthSquared();
        var circleNormal = (closestPointOnCircle - Center).Normalize();
        return new(
            new(closestPointOnCircle, circleNormal), 
            new(closestPointOnLine, other.Normal),
            disSquared);
    }
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var d1 = other.Direction;

        var toCenter = Center - other.Point;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        var closestPointOnRay = other.Point + projectionLength * d1;

        var offset = (closestPointOnRay - Center).Normalize() * Radius;
        var closestPointOnCircle = Center + offset;
        var circleNormal = (closestPointOnCircle - Center).Normalize();
        float disSquared = (closestPointOnRay - closestPointOnCircle).LengthSquared();
        return new(
            new(closestPointOnCircle, circleNormal), 
            new(closestPointOnRay, other.Normal),
            disSquared);
    }
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var d1 = other.End - other.Start;

        var toCenter = Center - other.Start;
        float projectionLength = Vector2.Dot(toCenter, d1) / d1.LengthSquared();
        projectionLength = Math.Clamp(projectionLength, 0.0f, 1.0f);
        var closestPointOnSegment = other.Start + projectionLength * d1;

        var offset = Vector2.Normalize(closestPointOnSegment - Center) * Radius;
        var closestPointOnCircle = Center + offset;
        var circleNormal = (closestPointOnCircle - Center).Normalize();
        float disSquared = (closestPointOnCircle - closestPointOnSegment).LengthSquared();
        return new(
            new(closestPointOnCircle, circleNormal), 
            new(closestPointOnSegment, other.Normal),
            disSquared);
    }
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var w = Center - other.Center;
        var dir = w.Normalize();
        var a = Center - dir * Radius;
        var aNormal = (a - Center).Normalize();
        var b = other.Center + dir * other.Radius;
        var bNormal = (b - other.Center).Normalize();
        float disSquared = (a - b).LengthSquared();
        return new(
            new(a, aNormal), 
            new(b, bNormal),
            disSquared);
    }
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        var closestResult = GetClosestPointCircleSegment(Center, Radius, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointCircleSegment(Center, Radius, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointCircleSegment(Center, Radius, other.C, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.C).GetPerpendicularRight().Normalize();
            var circleNormal = (result.self - Center).Normalize();
            return new(
                new(result.self, circleNormal), 
                new(result.self, normal),
                disSquared,
                -1,
                2);
        }
        
        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        var closestResult = GetClosestPointCircleSegment(Center, Radius, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointCircleSegment(Center, Radius, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointCircleSegment(Center, Radius, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }
        
        result = GetClosestPointCircleSegment(Center, Radius, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            var circleNormal = (result.self - Center).Normalize();
            return new(
                new(result.self, circleNormal), 
                new(result.self, normal),
                disSquared,
                -1,
                3);
        }

        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        var closestResult = GetClosestPointCircleSegment(Center, Radius, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointCircleSegment(Center, Radius, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointCircleSegment(Center, Radius, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }
        
        result = GetClosestPointCircleSegment(Center, Radius, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            var circleNormal = (result.self - Center).Normalize();
            return new(
                new(result.self, circleNormal), 
                new(result.self, normal),
                disSquared,
                -1,
                3);
        }
        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (other.Count < 3) return new();
        
        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count; i++)
        {
            p1 = other[i];
            p2 = other[(i + 1) % other.Count];
            var result = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                otherNormal = (p2 - p1);
            }
        }
        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (other.Count < 2) return new();
        
        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count - 1; i++)
        {
            p1 = other[i];
            p2 = other[i + 1];
            var result = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                otherNormal = (p2 - p1);
            }
        }
        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Segments segments)
    {
        if (segments.Count <= 0) return new();
        
        var curSegment = segments[0];
        var closestResult = GetClosestPoint(curSegment);
        var otherIndex = 0;
        for (var i = 1; i < segments.Count; i++)
        {
            curSegment = segments[i];
            var result = GetClosestPoint(curSegment);

            if (result.IsCloser(closestResult))
            {
                otherIndex = i;
                closestResult = result;
            }
        }
        return closestResult.SetOtherSegmentIndex(otherIndex);
    }

    public Vector2 GetClosestVertex(Vector2 p, out float disSquared)
    {
        var vertex = Center + (p - Center).Normalize() * Radius;
        disSquared = (vertex - p).LengthSquared();
        return vertex;
    }
    #endregion
    
    #region Static
    public static Circle Combine(params Circle[] circles)
    {
        if (circles.Length <= 0) return new();
        Vector2 combinedCenter = new();
        float totalRadius = 0f;
        for (int i = 0; i < circles.Length; i++)
        {
            var circle = circles[i];
            combinedCenter += circle.Center;
            totalRadius += circle.Radius;
        }
        return new(combinedCenter / circles.Length, totalRadius);
    }

    public static bool OverlapCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
    {
        if (aRadius <= 0.0f && bRadius > 0.0f) return ContainsCirclePoint(bPos, bRadius, aPos);
        if (bRadius <= 0.0f && aRadius > 0.0f) return ContainsCirclePoint(aPos, aRadius, bPos);
        if (aRadius <= 0.0f && bRadius <= 0.0f) return aPos == bPos;

        float rSum = aRadius + bRadius;
        return (aPos - bPos).LengthSquared() < rSum * rSum;
    }
    public static bool ContainsCirclePoint(Vector2 cPos, float cRadius, Vector2 p) => (cPos - p).LengthSquared() <= cRadius * cRadius;
    public static bool OverlapCircleSegment(Vector2 cPos, float cRadius, Vector2 segStart, Vector2 segEnd)
    {
        if (cRadius <= 0.0f) return Segment.IsPointOnSegment(cPos, segStart, segEnd);
        if (ContainsCirclePoint(cPos, cRadius, segStart)) return true;
        // if (ContainsCirclePoint(cPos, cRadius, segEnd)) return true;

        var d = segEnd - segStart;
        var lc = cPos - segStart;
        var p = lc.Project(d);
        var nearest = segStart + p;

        return
            ContainsCirclePoint(cPos, cRadius, nearest) &&
            p.LengthSquared() <= d.LengthSquared() &&
            Vector2.Dot(p, d) >= 0.0f;
    }
    public static bool OverlapCircleLine(Vector2 cPos, float cRadius, Vector2 linePos, Vector2 lineDir)
    {
        var lc = cPos - linePos;
        var p = lc.Project(lineDir);
        var nearest = linePos + p;
        return ContainsCirclePoint(cPos, cRadius, nearest);
    }
    public static bool OverlapCircleRay(Vector2 cPos, float cRadius, Vector2 rayPos, Vector2 rayDir)
    {
        var w = cPos - rayPos;
        float p = w.X * rayDir.Y - w.Y * rayDir.X;
        if (p < -cRadius || p > cRadius) return false;
        float t = w.X * rayDir.X + w.Y * rayDir.Y;
        if (t < 0.0f)
        {
            float d = w.LengthSquared();
            if (d > cRadius * cRadius) return false;
        }
        return true;
    }
    
    public static bool OverlapCircleTriangle(Vector2 center, float radius, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Triangle.ContainsPoint(a, b, c, center)) return true;
        
        if( OverlapCircleSegment(center, radius,  a, b) ) return true;
        if( OverlapCircleSegment(center, radius,  b, c) ) return true;
        if( OverlapCircleSegment(center, radius,  c, a) ) return true;

        return false;
    }
    public static bool OverlapCircleQuad(Vector2 center, float radius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.ContainsPoint(a, b, c, d,  center)) return true;
        
        if( OverlapCircleSegment(center, radius,  a, b) ) return true;
        if( OverlapCircleSegment(center, radius,  b, c) ) return true;
        if( OverlapCircleSegment(center, radius,  c, d) ) return true;
        if( OverlapCircleSegment(center, radius,  d, a) ) return true;
        
        return false;
    }
    public static bool OverlapCircleRect(Vector2 center, float radius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapCircleQuad(center, radius, a, b, c, d);
    }
    public static bool OverlapCirclePolygon(Vector2 center, float radius, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        if (Circle.ContainsCirclePoint(center, radius, points[0])) return true;

        var oddNodes = false;
        for (var i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            if(OverlapCircleSegment(center, radius, p1, p2)) return true;
            if(Polygon.ContainsPointCheck(p1, p2, center)) oddNodes = !oddNodes;
        }
        return oddNodes;
    }
    public static bool OverlapCirclePolyline(Vector2 center, float radius, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            if(OverlapCircleSegment(center, radius, points[i], points[i + 1])) return true;
        }
        return false;
    }
    public static bool OverlapCircleSegments(Vector2 center, float radius, List<Segment> segments)
    {
        if (segments.Count <= 0) return false;

        foreach (var seg in segments)
        {
            if( OverlapCircleSegment(center, radius, seg.Start, seg.End) ) return true;
        }
        return false;
    }

    
    
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius) { return IntersectCircleCircle(aPos.X, aPos.Y, aRadius, bPos.X, bPos.Y, bRadius); }
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleCircle(float cx0, float cy0, float radius0, float cx1, float cy1, float radius1)
    {
        // Find the distance between the centers.
        float dx = cx0 - cx1;
        float dy = cy0 - cy1;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        // See how many solutions there are.
        if (dist > radius0 + radius1)
        {
            // No solutions, the circles are too far apart.
            return (new(), new());
        }
        if (dist < Math.Abs(radius0 - radius1))
        {
            // No solutions, one circle contains the other.
            return (new(), new());
        }
        if ((dist == 0) && ShapeMath.EqualsF(radius0, radius1))// (radius0 == radius1))
        {
            // No solutions, the circles coincide.
            return (new(), new());
        }
        
        // Find a and h.
        double a = (radius0 * radius0 - radius1 * radius1 + dist * dist) / (2 * dist);
        double h = Math.Sqrt(radius0 * radius0 - a * a);

        // Find P2.
        double cx2 = cx0 + a * (cx1 - cx0) / dist;
        double cy2 = cy0 + a * (cy1 - cy0) / dist;

        // Get the points P3.
        var intersection1 = new Vector2(
            (float)(cx2 + h * (cy1 - cy0) / dist),
            (float)(cy2 - h * (cx1 - cx0) / dist));
        var intersection2 = new Vector2(
            (float)(cx2 - h * (cy1 - cy0) / dist),
            (float)(cy2 + h * (cx1 - cx0) / dist));

        // See if we have 1 or 2 solutions.
        if (ShapeMath.EqualsF((float)dist, radius0 + radius1))
        {
            var n = intersection1 - new Vector2(cx1, cy1);
            var cp = new CollisionPoint(intersection1, n.Normalize());
            return (cp, new());
        }
            
        var n1 = intersection1 - new Vector2(cx1, cy1);
        var cp1 = new CollisionPoint(intersection1, n1.Normalize());
            
        var n2 = intersection2 - new Vector2(cx1, cy1);
        var cp2 = new CollisionPoint(intersection2, n2.Normalize());
        return (cp1, cp2);

    }
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleSegment(Vector2 circlePos, float circleRadius, Vector2 start, Vector2 end) 
    {
        return IntersectCircleSegment(
            circlePos.X, circlePos.Y, circleRadius,
            start.X, start.Y,
            end.X, end.Y); 
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleRay(Vector2 circleCenter, float circleRadius, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal) 
    {
        var toCircle = circleCenter - rayPoint;
        float projectionLength = Vector2.Dot(toCircle, rayDirection);
        var closestPoint = rayPoint + projectionLength * rayDirection;
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        if (distanceToCenter < circleRadius)
        {
            var offset = (float)Math.Sqrt(circleRadius * circleRadius - distanceToCenter * distanceToCenter);
            var intersection1 = closestPoint - offset * rayDirection;
            var intersection2 = closestPoint + offset * rayDirection;

            CollisionPoint a = new();
            CollisionPoint b = new();
            if (Vector2.Dot(intersection1 - rayPoint, rayDirection) >= 0)
            {
                a = new(intersection1, rayNormal);
            }

            if (Vector2.Dot(intersection2 - rayPoint, rayDirection) >= 0)
            {
                b = new(intersection2, rayNormal);
                
            }
            return (a, b);
        }
        
        if (Math.Abs(distanceToCenter - circleRadius) < 1e-10)
        {
            if (Vector2.Dot(closestPoint - rayPoint, rayDirection) >= 0)
            {
                var cp = new CollisionPoint(closestPoint, rayNormal);
                return (cp, new());
            }
        }
        
        return (new(), new());
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleLine(Vector2 circleCenter, float circleRadius, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        // Normalize the direction vector
        lineDirection = lineDirection.Normalize();

        // Vector from the line point to the circle center
        var toCircle = circleCenter - linePoint;
        
        // Projection of toCircle onto the line direction to find the closest approach
        float projectionLength = Vector2.Dot(toCircle, lineDirection);

        // Closest point on the line to the circle center
        var closestPoint = linePoint + projectionLength * lineDirection;

        // Distance from the closest point to the circle center
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        // Check if the line intersects the circle
        if (distanceToCenter < circleRadius)
        {
            // Calculate the distance from the closest point to the intersection points
            var offset = (float)Math.Sqrt(circleRadius * circleRadius - distanceToCenter * distanceToCenter);

            // Intersection points
            var intersection1 = closestPoint - offset * lineDirection;
            var intersection2 = closestPoint + offset * lineDirection;

            // Normals at the intersection points
            var p1 = new CollisionPoint(intersection1, lineNormal);
            var p2 = new CollisionPoint(intersection2, lineNormal);
            return (p1, p2);
        }
        
        if (Math.Abs(distanceToCenter - circleRadius) < 1e-10)
        {
            var p = new CollisionPoint(closestPoint,lineNormal);
            return (p, new());
        }

        return (new(), new());
    }
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleSegment(float circleX, float circleY, float circleRadius, float segStartX, float segStartY, float segEndX, float segEndY)
    {
        float difX = segEndX - segStartX;
        float difY = segEndY - segStartY;
        if ((difX == 0) && (difY == 0)) return (new(), new());

        float dl = (difX * difX + difY * difY);
        float t = ((circleX - segStartX) * difX + (circleY - segStartY) * difY) / dl;

        // point on a line nearest to circle center
        float nearestX = segStartX + t * difX;
        float nearestY = segStartY + t * difY;

        float dist = (new Vector2(nearestX, nearestY) - new Vector2(circleX, circleY)).Length(); // point_dist(nearestX, nearestY, cX, cY);

        if (ShapeMath.EqualsF(dist, circleRadius))
        {
            // line segment touches circle; one intersection point
            float iX = nearestX;
            float iY = nearestY;

            if (t >= 0f && t <= 1f)
            {
                // intersection point is not actually within line segment
                var p = new Vector2(iX, iY);
                var n = Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false); // p - new Vector2(circleX, circleY);
                var cp = new CollisionPoint(p, n);
                return (cp, new());
            }
            return (new(), new());
        }
        if (dist < circleRadius)
        {
            // List<Vector2>? intersectionPoints = null;
            CollisionPoint a = new();
            CollisionPoint b = new();
            // two possible intersection points

            float dt = MathF.Sqrt(circleRadius * circleRadius - dist * dist) / MathF.Sqrt(dl);

            // intersection point nearest to A
            float t1 = t - dt;
            float i1X = segStartX + t1 * difX;
            float i1Y = segStartY + t1 * difY;
            if (t1 >= 0f && t1 <= 1f)
            {
                // intersection point is actually within line segment
                // intersectionPoints ??= new();
                // intersectionPoints.Add(new Vector2(i1X, i1Y));
                
                var p = new Vector2(i1X, i1Y);
                // var n = p - new Vector2(circleX, circleY);
                var n = Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false); 
                a = new CollisionPoint(p, n);
            }

            // intersection point farthest from A
            float t2 = t + dt;
            float i2X = segStartX + t2 * difX;
            float i2Y = segStartY + t2 * difY;
            if (t2 >= 0f && t2 <= 1f)
            {
                // intersection point is actually within line segment
                // intersectionPoints ??= new();
                // intersectionPoints.Add(new Vector2(i2X, i2Y));
                var p = new Vector2(i2X, i2Y);
                // var n = p - new Vector2(circleX, circleY);
                var n = Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false); 
                b = new CollisionPoint(p, n);
            }

            return (a, b);
        }

        return (new(), new());
    }
    #endregion

    #region Overlap
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
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
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

    public bool OverlapShape(Segments segments)
    {
        foreach (var seg in segments)
        {
            if(OverlapCircleSegment(Center, Radius, seg.Start, seg.End)) return true;
            // if (seg.OverlapShape(this)) return true;
        }
        return false;
    }
    public bool OverlapShape(Segment s) => OverlapCircleSegment(Center, Radius, s.Start, s.End);
    public bool OverlapShape(Line l) => Line.OverlapLineCircle(l.Point, l.Direction, Center, Radius);
    public bool OverlapShape(Ray r) => Ray.OverlapRayCircle(r.Point, r.Direction, Center, Radius);
    public bool OverlapShape(Circle b) => OverlapCircleCircle(Center, Radius, b.Center, b.Radius);
    public bool OverlapShape(Triangle t)
    {
        if (ContainsPoint(t.A)) return true;
        if (t.ContainsPoint(Center)) return true;

        if (Segment.OverlapSegmentCircle(t.A, t.B, Center, Radius)) return true;
        if (Segment.OverlapSegmentCircle(t.B, t.C, Center, Radius)) return true;
        return Segment.OverlapSegmentCircle(t.C, t.A, Center, Radius);
    }
    public bool OverlapShape(Quad q)
    {
        if (ContainsPoint(q.A)) return true;
        if (q.ContainsPoint(Center)) return true;
    
        if (Segment.OverlapSegmentCircle(q.A, q.B, Center, Radius)) return true;
        if (Segment.OverlapSegmentCircle(q.B, q.C, Center, Radius)) return true;
        if (Segment.OverlapSegmentCircle(q.C, q.D, Center, Radius)) return true;
        return Segment.OverlapSegmentCircle(q.D, q.A, Center, Radius);
    }
    public bool OverlapShape(Rect r)
    {
        if (Radius <= 0.0f) return r.ContainsPoint(Center);
        return ContainsPoint(r.ClampOnRect(Center));
    }
    public bool OverlapShape(Polygon poly)
    {
        if (poly.Count < 3) return false;
        if (ContainsPoint(poly[0])) return true;
        
        var oddNodes = false;
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            if (Circle.OverlapCircleSegment(Center, Radius, start, end)) return true;
            if (Polygon.ContainsPointCheck(start, end, Center)) oddNodes = !oddNodes;
        }
        return oddNodes;
    }
    public bool OverlapShape(Polyline pl)
    {
        if (pl.Count <= 0) return false;
        if (pl.Count == 1) return ContainsPoint(pl[0]);

        if (ContainsPoint(pl[0])) return true;
        
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var start = pl[i];
            var end = pl[(i + 1) % pl.Count];
            if (OverlapCircleSegment(Center, Radius, start, end)) return true;
        }

        return false;
    }

    public bool OverlapSegment(Vector2 start, Vector2 end) => OverlapCircleSegment(Center, Radius, start, end);
    public bool OverlapLine(Vector2 linePos, Vector2 lineDir) => OverlapCircleLine(Center, Radius, linePos, lineDir);
    public bool OverlapRay(Vector2 rayPos, Vector2 rayDir) => OverlapCircleRay(Center, Radius, rayPos, rayDir);
    public bool OverlapCircle(Vector2 center, float radius) => OverlapCircleCircle(Center, Radius, center, radius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapCircleTriangle(Center, Radius, a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapCircleQuad(Center, Radius, a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapCircleQuad(Center, Radius, a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapCirclePolygon(Center, Radius, points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapCirclePolyline(Center, Radius, points);
    public bool OverlapSegments(List<Segment> segments) => OverlapCircleSegments(Center, Radius, segments);
    #endregion

    #region Intersect
    public  CollisionPoints? Intersect(Collider collider)
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
    public  CollisionPoints? IntersectShape(Circle c)
    {
        var result = IntersectCircleCircle(Center, Radius, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            var points = new CollisionPoints();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;

    }
    public  CollisionPoints? IntersectShape(Ray r)
    {
        var result = IntersectCircleRay(Center, Radius, r.Point, r.Direction, r.Normal);
        if (result.a.Valid || result.b.Valid)
        {
            var points = new CollisionPoints();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;
    }
    public  CollisionPoints? IntersectShape(Line l)
    {
        var result = IntersectCircleLine(Center, Radius, l.Point, l.Direction, l.Normal);
        
        if (result.a.Valid || result.b.Valid)
        {
            var points = new CollisionPoints();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;
    }
    public  CollisionPoints? IntersectShape(Segment s)
    {
        var result = IntersectCircleSegment(Center, Radius, s.Start, s.End);
        if (result.a.Valid || result.b.Valid)
        {
            var points = new CollisionPoints();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;
    }
    public  CollisionPoints? IntersectShape(Triangle t)
    {
        CollisionPoints? points = null;
        var result = IntersectCircleSegment(Center, Radius, t.A, t.B);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        result = IntersectCircleSegment(Center, Radius, t.B, t.C);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, t.C, t.A);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }

        return points;
    }
    public  CollisionPoints? IntersectShape(Rect r)
    {
        CollisionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        
        var result = IntersectCircleSegment(Center, Radius, a, b);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        var c = r.BottomRight;
        result = IntersectCircleSegment(Center, Radius, b, c);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        var d = r.TopRight;
        result = IntersectCircleSegment(Center, Radius, c, d);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, d, a);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        return points;
    }
    public  CollisionPoints? IntersectShape(Quad q)
    {
        CollisionPoints? points = null;
        
        var result = IntersectCircleSegment(Center, Radius, q.A, q.B);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        result = IntersectCircleSegment(Center, Radius, q.B, q.C);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        result = IntersectCircleSegment(Center, Radius, q.C, q.D);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, q.D, q.A);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        return points;
    }
    public  CollisionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3) return null;
        
        CollisionPoints? points = null;

        for (var i = 0; i < p.Count; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, p[i], p[(i + 1) % p.Count]);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if(result.a.Valid) points.Add(result.a);
                if(result.b.Valid) points.Add(result.b);
            }
            
        }
        return points;
    }
    public  CollisionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2) return null;
        
        CollisionPoints? points = null;

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, pl[i], pl[(i + 1) % pl.Count]);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if(result.a.Valid) points.Add(result.a);
                if(result.b.Valid) points.Add(result.b);
            }
            
        }
        return points;
    }

    public CollisionPoints? IntersectShape(Segments shape)
    {
        CollisionPoints? points = null;
        foreach (var seg in shape)
        {
            var result = IntersectCircleSegment(Center, Radius, seg.Start, seg.End);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if(result.a.Valid) points.Add(result.a);
                if(result.b.Valid) points.Add(result.b);
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
    public int IntersectShape(Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleRay(Center, Radius, r.Point, r.Direction, r.Normal);
        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }
            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }
        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }
        
        if (result.b.Valid)
        {
            points.Add(result.b);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleLine(Center, Radius, l.Point, l.Direction, l.Normal);
        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }
            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }
        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }
        
        if (result.b.Valid)
        {
            points.Add(result.b);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleSegment(Center, Radius, s.Start, s.End);
        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }
            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }
        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }
        
        if (result.b.Valid)
        {
            points.Add(result.b);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleCircle(Center, Radius, c.Center, c.Radius);
        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }
            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }
        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }
        
        if (result.b.Valid)
        {
            points.Add(result.b);
            return 1;
        }

        return 0;
    }
    public int IntersectShape(Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleSegment(Center, Radius, t.A, t.B);
        var count = 0;
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
        
        result = IntersectCircleSegment(Center, Radius, t.B, t.C);
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

        result = IntersectCircleSegment(Center, Radius, t.C, t.A);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            count++;
        }

        return count;
    }
    public int IntersectShape(Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        
        var result = IntersectCircleSegment(Center, Radius, q.A, q.B);
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
        
        result = IntersectCircleSegment(Center, Radius, q.B, q.C);
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
        
        result = IntersectCircleSegment(Center, Radius, q.C, q.D);
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

        result = IntersectCircleSegment(Center, Radius, q.D, q.A);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            count++;
        }
        return count;
    }
    public int IntersectShape(Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        
        var result = IntersectCircleSegment(Center, Radius, a, b);
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
        
        var c = r.BottomRight;
        result = IntersectCircleSegment(Center, Radius, b, c);
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
        
        var d = r.TopRight;
        result = IntersectCircleSegment(Center, Radius, c, d);
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

        result = IntersectCircleSegment(Center, Radius, d, a);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            count++;
        }
        return count;
    }
    public int IntersectShape(Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;

        var count = 0;

        for (var i = 0; i < p.Count; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, p[i], p[(i + 1) % p.Count]);
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
    public int IntersectShape(Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;

        var count = 0;

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, pl[i], pl[(i + 1) % pl.Count]);
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
    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        foreach (var seg in shape)
        {
            var result = IntersectCircleSegment(Center, Radius, seg.Start, seg.End);
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
   
    
    #endregion
}