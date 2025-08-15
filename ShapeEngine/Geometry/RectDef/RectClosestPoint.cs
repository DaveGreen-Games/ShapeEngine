using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    /// <summary>
    /// Finds the closest point on the rect's perimeter to any collider in the given collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object containing one or more colliders to compare against.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the nearest collider.
    /// </returns>
    public ClosestPointResult GetClosestPoint(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return new();
        var closestPoint = new ClosestPointResult();
        foreach (var collider in collisionObject.Colliders)
        {
            var result = GetClosestPoint(collider);
            if(!result.Valid) continue;
            if (!closestPoint.Valid) closestPoint = result;
            else
            {
                if (result.DistanceSquared < closestPoint.DistanceSquared) closestPoint = result;
            }
        }
        return closestPoint;
    }
  
    /// <summary>
    /// Finds the closest point on this rect's perimeter to the given collider.
    /// </summary>
    /// <param name="collider">The collider to compare against.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the collider.
    /// </returns>
    public ClosestPointResult GetClosestPoint(Collider collider)
    {
        if (!collider.Enabled) return new();
        switch (collider.GetShapeType())
        {
            case ShapeType.Line: return GetClosestPoint(collider.GetLineShape());
            case ShapeType.Ray: return GetClosestPoint(collider.GetRayShape());
            case ShapeType.Circle: return GetClosestPoint(collider.GetCircleShape());
            case ShapeType.Segment: return GetClosestPoint(collider.GetSegmentShape());
            case ShapeType.Triangle: return GetClosestPoint(collider.GetTriangleShape());
            case ShapeType.Rect: return GetClosestPoint(collider.GetRectShape());
            case ShapeType.Quad: return GetClosestPoint(collider.GetQuadShape());
            case ShapeType.Poly: return GetClosestPoint(collider.GetPolygonShape());
            case ShapeType.PolyLine: return GetClosestPoint(collider.GetPolylineShape());
        }

        return new();
    }
    
    
    /// <summary>
    /// Returns the closest point on the rectangle (defined by four corners) to a given point.
    /// </summary>
    /// <param name="a">Top-left corner of the rectangle.</param>
    /// <param name="b">Bottom-left corner of the rectangle.</param>
    /// <param name="c">Bottom-right corner of the rectangle.</param>
    /// <param name="d">Top-right corner of the rectangle.</param>
    /// <param name="p">The point to find the closest point to.</param>
    /// <param name="disSquared">The squared distance from <paramref name="p"/> to the closest point on the rectangle.</param>
    /// <returns>The closest point on the rectangle to <paramref name="p"/>.</returns>
    /// <remarks>
    /// This method checks all four edges of the rectangle and returns the closest point on any edge.
    /// </remarks>
    public static Vector2 GetClosestPointRectPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 p, out float disSquared)
    {
        var min = Segment.GetClosestPointSegmentPoint(a, b, p, out float minDisSq);

        var cp = Segment.GetClosestPointSegmentPoint(b, c, p, out float dis);
        if (dis < minDisSq)
        {
            min = cp;
            minDisSq = dis;
        }

        cp = Segment.GetClosestPointSegmentPoint(c, d, p, out dis);
        if (dis < minDisSq)
        {
            min = cp;
            minDisSq = dis;
        }

        cp = Segment.GetClosestPointSegmentPoint(d, a, p, out dis);
        if (dis < minDisSq)
        {
            min = cp;
            minDisSq = dis;
        }

        disSquared = minDisSq;
        return min;
    }

    /// <summary>
    /// Returns the closest point on this rectangle to a given point, along with the outward normal at that point.
    /// </summary>
    /// <param name="p">The point to find the closest point to.</param>
    /// <param name="disSquared">The squared distance from <paramref name="p"/> to the closest point on the rectangle.</param>
    /// <returns>A <see cref="IntersectionPoint"/> containing the closest point and the outward normal.</returns>
    /// <remarks>
    /// The normal is perpendicular to the edge where the closest point lies, pointing outward from the rectangle.
    /// </remarks>
    public IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        var min = Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        var normal = B - A;

        var cp = Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = C - B;
        }

        cp = Segment.GetClosestPointSegmentPoint(C, D, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = D - C;
        }

        cp = Segment.GetClosestPointSegmentPoint(D, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - D;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    /// <summary>
    /// Returns the closest point on this rectangle to a given point, along with the outward normal and the edge index.
    /// </summary>
    /// <param name="p">The point to find the closest point to.</param>
    /// <param name="disSquared">The squared distance from <paramref name="p"/> to the closest point on the rectangle.</param>
    /// <param name="index">The index of the edge (0 = AB, 1 = BC, 2 = CD, 3 = DA) where the closest point lies.</param>
    /// <returns>A <see cref="IntersectionPoint"/> containing the closest point and the outward normal.</returns>
    /// <remarks>
    /// The normal is perpendicular to the edge where the closest point lies, pointing outward from the rectangle.
    /// </remarks>
    public IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        var min = Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        index = 0;
        var normal = B - A;

        var cp = Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 1;
            normal = C - B;
        }

        cp = Segment.GetClosestPointSegmentPoint(C, D, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 2;
            normal = D - C;
        }

        cp = Segment.GetClosestPointSegmentPoint(D, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 3;
            normal = A - D;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    /// <summary>
    /// Returns the closest point and normal between this rectangle and a line.
    /// </summary>
    /// <param name="other">The line to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points, normals, squared distance, and edge index.</returns>
    public ClosestPointResult GetClosestPoint(Line other)
    {
        var closestResult = Segment.GetClosestPointSegmentLine(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;

        var result = Segment.GetClosestPointSegmentLine(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.GetClosestPointSegmentLine(C, D, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }

        result = Segment.GetClosestPointSegmentLine(D, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Returns the closest point and normal between this rectangle and a ray.
    /// </summary>
    /// <param name="other">The ray to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points, normals, squared distance, and edge index.</returns>
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var closestResult = Segment.GetClosestPointSegmentRay(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;
        var result = Segment.GetClosestPointSegmentRay(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.GetClosestPointSegmentRay(C, D, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }

        result = Segment.GetClosestPointSegmentRay(D, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Returns the closest point and normal between this rectangle and a segment.
    /// </summary>
    /// <param name="other">The segment to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points, normals, squared distance, and edge index.</returns>
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.Start, other.End, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;

        var result = Segment.GetClosestPointSegmentSegment(B, C, other.Start, other.End, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Returns the closest point and normal between this rectangle and a circle.
    /// </summary>
    /// <param name="other">The circle to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points, normals, squared distance, and edge index.</returns>
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var closestResult = Segment.GetClosestPointSegmentCircle(A, B, other.Center, other.Radius, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;

        var result = Segment.GetClosestPointSegmentCircle(B, C, other.Center, other.Radius, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.GetClosestPointSegmentCircle(C, D, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }

        result = Segment.GetClosestPointSegmentCircle(D, A, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, (closestResult.other - other.Center).Normalize()),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Returns the closest point and normal between this rectangle and a triangle.
    /// </summary>
    /// <param name="other">The triangle to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points, normals, squared distance, and edge indices.</returns>
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;

        var result = Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(A, B, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.A - other.C;
        }

        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Returns the closest point and normal between this rectangle and a quad.
    /// </summary>
    /// <param name="other">The quad to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points, normals, squared distance, and edge indices.</returns>
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;

        var result = Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(A, B, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(A, B, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.D;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.D;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.D;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.A - other.D;
        }

        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Returns the closest point and normal between this rectangle and another rectangle.
    /// </summary>
    /// <param name="other">The rectangle to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points, normals, squared distance, and edge indices.</returns>
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;

        var result = Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(A, B, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(A, B, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.D;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(B, C, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.D;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(C, D, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.D;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(D, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.A - other.D;
        }

        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    /// <summary>
    /// Returns the closest point and normal between this rectangle and a polygon.
    /// </summary>
    /// <param name="other">The polygon to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points, normals, squared distance, and edge indices.</returns>
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (other.Count < 3) return new();

        (Vector2 self, Vector2 other) closestResult = (Vector2.Zero, Vector2.Zero);
        var curSelfNormal = Vector2.Zero;
        var curOtherNormal = Vector2.Zero;
        float disSquared = -1;
        var selfIndex = -1;
        var otherIndex = -1;
        for (var i = 0; i < other.Count; i++)
        {
            var p1 = other[i];
            var p2 = other[(i + 1) % other.Count];

            var result = Segment.GetClosestPointSegmentSegment(A, B, p1, p2, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 0;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = B - A;
            }

            result = Segment.GetClosestPointSegmentSegment(B, C, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 1;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = C - B;
            }

            result = Segment.GetClosestPointSegmentSegment(C, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = D - C;
            }

            result = Segment.GetClosestPointSegmentSegment(A, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 3;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - D;
            }
        }


        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    /// <summary>
    /// Returns the closest point and normal between this rectangle and a polyline.
    /// </summary>
    /// <param name="other">The polyline to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points, normals, squared distance, and edge indices.</returns>
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (other.Count < 2) return new();

        (Vector2 self, Vector2 other) closestResult = (Vector2.Zero, Vector2.Zero);
        var curSelfNormal = Vector2.Zero;
        var curOtherNormal = Vector2.Zero;
        var disSquared = -1f;
        var selfIndex = -1;
        var otherIndex = -1;
        for (var i = 0; i < other.Count - 1; i++)
        {
            var p1 = other[i];
            var p2 = other[i + 1];

            var result = Segment.GetClosestPointSegmentSegment(A, B, p1, p2, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 0;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = B - A;
            }

            result = Segment.GetClosestPointSegmentSegment(B, C, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 1;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = C - B;
            }

            result = Segment.GetClosestPointSegmentSegment(C, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = D - C;
            }

            result = Segment.GetClosestPointSegmentSegment(A, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 3;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - D;
            }
        }

        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Returns the closest point and normal between this rectangle and a set of segments.
    /// </summary>
    /// <param name="other">The segments to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points, normals, squared distance, and edge indices.</returns>
    public ClosestPointResult GetClosestPoint(Segments other)
    {
        if (other.Count <= 0) return new();

        ClosestPointResult closestResult = new();

        for (var i = 0; i < other.Count; i++)
        {
            var segment = other[i];
            var result = GetClosestPoint(segment);

            if (!closestResult.Valid || result.IsCloser(closestResult))
            {
                closestResult = result;
            }
        }

        return closestResult;
    }

    /// <summary>
    /// Returns the closest segment and its closest point to a given point.
    /// </summary>
    /// <param name="p">The point to compare against.</param>
    /// <param name="disSquared">The squared distance from <paramref name="p"/> to the closest segment.</param>
    /// <returns>A tuple containing the closest <see cref="Segment"/> and its closest <see cref="IntersectionPoint"/>.</returns>
    public (Segment segment, IntersectionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        var closestSegment = TopSegment;
        var closestResult = closestSegment.GetClosestPoint(p, out float minDisSquared);

        var currentSegment = LeftSegment;
        var result = currentSegment.GetClosestPoint(p, out float dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }

        currentSegment = BottomSegment;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }

        currentSegment = RightSegment;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }

        disSquared = minDisSquared;
        return (closestSegment, closestResult);
    }

    /// <summary>
    /// Returns the closest vertex of the rectangle to a given point.
    /// </summary>
    /// <param name="p">The point to compare against.</param>
    /// <param name="disSquared">The squared distance from <paramref name="p"/> to the closest vertex.</param>
    /// <param name="index">The index of the closest vertex (0 = TopLeft, 1 = BottomLeft, 2 = BottomRight, 3 = TopRight).</param>
    /// <returns>The closest <see cref="Vector2"/> vertex.</returns>
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        var closest = TopLeft;
        disSquared = (TopLeft - p).LengthSquared();
        index = 0;

        float l = (BottomLeft - p).LengthSquared();
        if (l < disSquared)
        {
            closest = BottomLeft;
            disSquared = l;
            index = 1;
        }

        l = (BottomRight - p).LengthSquared();
        if (l < disSquared)
        {
            closest = BottomRight;
            disSquared = l;
            index = 2;
        }

        l = (TopRight - p).LengthSquared();
        if (l < disSquared)
        {
            disSquared = l;
            closest = TopRight;
            index = 3;
        }

        return closest;
    }

    /// <summary>
    /// Returns the furthest vertex of the rectangle from a given point.
    /// </summary>
    /// <param name="p">The point to compare against.</param>
    /// <param name="disSquared">The squared distance from <paramref name="p"/> to the furthest vertex.</param>
    /// <param name="index">The index of the furthest vertex (0 = TopLeft, 1 = BottomLeft, 2 = BottomRight, 3 = TopRight).</param>
    /// <returns>The furthest <see cref="Vector2"/> vertex.</returns>
    public Vector2 GetFurthestVertex(Vector2 p, out float disSquared, out int index)
    {
        var furthest = TopLeft;
        disSquared = (TopLeft - p).LengthSquared();
        index = 0;

        float l = (BottomLeft - p).LengthSquared();
        if (l > disSquared)
        {
            furthest = BottomLeft;
            disSquared = l;
            index = 1;
        }

        l = (BottomRight - p).LengthSquared();
        if (l > disSquared)
        {
            furthest = BottomRight;
            disSquared = l;
            index = 2;
        }

        l = (TopRight - p).LengthSquared();
        if (l > disSquared)
        {
            disSquared = l;
            furthest = TopRight;
            index = 3;
        }

        return furthest;
    }
    
    /// <summary>
    /// Finds the closest point on this shapes perimeter to the given <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to compare against.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the shape.
    /// </returns>
    public ClosestPointResult GetClosestPoint(IShape shape)
    {
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => GetClosestPoint(shape.GetCircleShape()),
            ShapeType.Segment => GetClosestPoint(shape.GetSegmentShape()),
            ShapeType.Ray => GetClosestPoint(shape.GetRayShape()),
            ShapeType.Line => GetClosestPoint(shape.GetLineShape()),
            ShapeType.Triangle => GetClosestPoint(shape.GetTriangleShape()),
            ShapeType.Rect => GetClosestPoint(shape.GetRectShape()),
            ShapeType.Quad => GetClosestPoint(shape.GetQuadShape()),
            ShapeType.Poly => GetClosestPoint(shape.GetPolygonShape()),
            ShapeType.PolyLine => GetClosestPoint(shape.GetPolylineShape()),
            _ => new()
        };
    }
}