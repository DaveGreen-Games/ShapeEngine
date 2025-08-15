using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangleDef;

public readonly partial struct Triangle
{
    /// <summary>
    /// Finds the closest point on the triangle's perimeter to any collider in the given collision object.
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
    /// Finds the closest point on this triangle's perimeter to the given collider.
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
    /// Finds the closest point on a triangle's perimeter to a specified point using static triangle vertices.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <param name="p">The point to find the closest point to.</param>
    /// <param name="disSquared">When this method returns, contains the squared distance to the closest point.</param>
    /// <returns>The closest point on the triangle's perimeter to the specified point.</returns>
    /// <remarks>
    /// This static method checks all three edges of the triangle and returns the closest point found.
    /// The squared distance is provided for performance reasons to avoid expensive square root calculations.
    /// </remarks>
    public static Vector2 GetClosestPointTrianglePoint(Vector2 a, Vector2 b, Vector2 c, Vector2 p, out float disSquared)
    {
        var min = Segment.GetClosestPointSegmentPoint(a, b, p, out disSquared);

        var cp = Segment.GetClosestPointSegmentPoint(b, c, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
        }

        cp = Segment.GetClosestPointSegmentPoint(c, a, p, out dis);
        if (dis < disSquared)
        {
            disSquared = dis;
            return cp;
        }

        return min;
    }

    /// <summary>
    /// Finds the closest point on this triangle's perimeter to a specified point.
    /// </summary>
    /// <param name="p">The point to find the closest point to.</param>
    /// <param name="disSquared">When this method returns, contains the squared distance to the closest point.</param>
    /// <returns>A intersection point containing the closest point and its surface normal.</returns>
    /// <remarks>
    /// This method checks all three edges of the triangle and returns the closest point with its corresponding
    /// surface normal. The normal points outward from the triangle's surface at the closest point.
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

        cp = Segment.GetClosestPointSegmentPoint(C, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - C;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    /// <summary>
    /// Finds the closest point on this triangle's perimeter to a specified point, including edge index information.
    /// </summary>
    /// <param name="p">The point to find the closest point to.</param>
    /// <param name="disSquared">When this method returns, contains the squared distance to the closest point.</param>
    /// <param name="index">When this method returns, contains the index of the edge containing the closest point (0=A-B, 1=B-C, 2=C-A).</param>
    /// <returns>A intersection point containing the closest point and its surface normal.</returns>
    /// <remarks>
    /// This method extends the basic closest point functionality by also providing the index of the edge
    /// that contains the closest point, which is useful for edge-specific operations and analysis.
    /// </remarks>
    public IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        var min = Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        var normal = B - A;
        index = 0;

        var cp = Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = C - B;
            index = 1;
        }

        cp = Segment.GetClosestPointSegmentPoint(C, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - C;
            index = 2;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    /// <summary>
    /// Finds the closest points between this triangle and a line.
    /// </summary>
    /// <param name="other">The line to find the closest points with.</param>
    /// <returns>A result containing the closest points on both shapes, the squared distance, and the triangle edge index.</returns>
    /// <remarks>
    /// This method finds the closest approach between the triangle's perimeter and an infinite line.
    /// The result includes collision points for both shapes with their respective surface normals.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Line other)
    {
        var closestResult = Segment.GetClosestPointSegmentLine(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var index = 0;

        var result = Segment.GetClosestPointSegmentLine(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.GetClosestPointSegmentLine(C, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            index);
    }

    /// <summary>
    /// Finds the closest points between this triangle and a ray.
    /// </summary>
    /// <param name="other">The ray to find the closest points with.</param>
    /// <returns>A result containing the closest points on both shapes, the squared distance, and the triangle edge index.</returns>
    /// <remarks>
    /// This method finds the closest approach between the triangle's perimeter and a ray (semi-infinite line).
    /// The result includes collision points for both shapes with their respective surface normals.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var closestResult = Segment.GetClosestPointSegmentRay(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var index = 0;

        var result = Segment.GetClosestPointSegmentRay(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.GetClosestPointSegmentRay(C, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            index);
    }

    /// <summary>
    /// Finds the closest points between this triangle and a line segment.
    /// </summary>
    /// <param name="other">The segment to find the closest points with.</param>
    /// <returns>A result containing the closest points on both shapes, the squared distance, and the triangle edge index.</returns>
    /// <remarks>
    /// This method finds the closest approach between the triangle's perimeter and a finite line segment.
    /// The result includes collision points for both shapes with their respective surface normals.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.Start, other.End, out float disSquared);
        var curNormal = B - A;
        var index = 0;

        var result = Segment.GetClosestPointSegmentSegment(B, C, other.Start, other.End, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.GetClosestPointSegmentSegment(C, A, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, other.Normal),
            disSquared,
            index);
    }

    /// <summary>
    /// Finds the closest points between this triangle and a circle.
    /// </summary>
    /// <param name="other">The circle to find the closest points with.</param>
    /// <returns>A result containing the closest points on both shapes, the squared distance, and the triangle edge index.</returns>
    /// <remarks>
    /// This method finds the closest approach between the triangle's perimeter and a circle's circumference.
    /// The result includes collision points for both shapes with their respective surface normals.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var closestResult = Segment.GetClosestPointSegmentCircle(A, B, other.Center, other.Radius, out float disSquared);
        var curNormal = B - A;
        var index = 0;
        var result = Segment.GetClosestPointSegmentCircle(B, C, other.Center, other.Radius, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }

        result = Segment.GetClosestPointSegmentCircle(C, A, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }

        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()),
            new(closestResult.other, (closestResult.other - other.Center).Normalize()),
            disSquared,
            index);
    }

    /// <summary>
    /// Finds the closest points between this triangle and another triangle.
    /// </summary>
    /// <param name="other">The other triangle to find the closest points with.</param>
    /// <returns>A result containing the closest points on both triangles, the squared distance, and the edge index for this triangle and the other triangle.</returns>
    /// <remarks>
    /// This method performs a comprehensive comparison between all edges of both triangles to find
    /// the closest approach points. This is useful for collision detection and proximity analysis.
    /// </remarks>
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

        result = Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(C, A, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
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
    /// Finds the closest points between this triangle and a quadrilateral.
    /// </summary>
    /// <param name="other">The quad to find the closest points with.</param>
    /// <returns>A result containing the closest points on both shapes, the squared distance, and edge indices.</returns>
    /// <remarks>
    /// This method compares the triangle's edges against all four edges of the quadrilateral
    /// to find the closest approach points between the two shapes.
    /// </remarks>
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

        result = Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(C, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(C, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
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
    /// Finds the closest points between this triangle and a rectangle.
    /// </summary>
    /// <param name="other">The rectangle to find the closest points with.</param>
    /// <returns>A result containing the closest points on both shapes, the squared distance, and edge indices.</returns>
    /// <remarks>
    /// This method compares the triangle's edges against all four edges of the rectangle
    /// to find the closest approach points between the two shapes.
    /// </remarks>
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

        result = Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }

        result = Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }

        result = Segment.GetClosestPointSegmentSegment(C, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.D - other.C;
        }

        result = Segment.GetClosestPointSegmentSegment(C, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
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
    /// Finds the closest points between this triangle and a polygon.
    /// </summary>
    /// <param name="other">The polygon to find the closest points with.</param>
    /// <returns>A result containing the closest points on both shapes, the squared distance, and edge indices.</returns>
    /// <remarks>
    /// This method compares the triangle's edges against all edges of the polygon
    /// to find the closest approach points. The polygon can have any number of vertices.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (other.Count < 3) return new();

        (Vector2 self, Vector2 other) closestResult = (Vector2.Zero, Vector2.Zero);
        var curSelfNormal = Vector2.Zero;
        var curOtherNormal = Vector2.Zero;
        var selfIndex = -1;
        var otherIndex = -1;
        var disSquared = -1f;

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

            result = Segment.GetClosestPointSegmentSegment(C, A, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - C;
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
    /// Finds the closest points between this triangle and a polyline.
    /// </summary>
    /// <param name="other">The polyline to find the closest points with.</param>
    /// <returns>A result containing the closest points on both shapes, the squared distance, and edge indices.</returns>
    /// <remarks>
    /// This method compares the triangle's edges against all segments of the polyline
    /// to find the closest approach points. Unlike polygons, polylines are not closed shapes.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (other.Count < 2) return new();

        (Vector2 self, Vector2 other) closestResult = (Vector2.Zero, Vector2.Zero);
        var curSelfNormal = Vector2.Zero;
        var curOtherNormal = Vector2.Zero;
        var selfIndex = -1;
        var otherIndex = -1;
        var disSquared = -1f;

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

            result = Segment.GetClosestPointSegmentSegment(C, A, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - C;
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
    /// Finds the closest points between this triangle and a collection of line segments.
    /// </summary>
    /// <param name="other">The segments collection to find the closest points with.</param>
    /// <returns>A result containing the closest points on both shapes, the squared distance, and edge indices.</returns>
    /// <remarks>
    /// This method compares the triangle's edges against all segments in the collection
    /// to find the closest approach points between any triangle edge and any segment.
    /// </remarks>
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
    /// Finds the triangle edge closest to a specified point.
    /// </summary>
    /// <param name="p">The point to find the closest edge to.</param>
    /// <param name="disSquared">When this method returns, contains the squared distance to the closest edge.</param>
    /// <returns>A tuple containing the closest edge as a segment and the closest point on that edge with its normal.</returns>
    /// <remarks>
    /// This method is useful when you need to know which specific edge of the triangle is closest to a point,
    /// along with the exact closest point on that edge and its surface normal.
    /// </remarks>
    public (Segment segment, IntersectionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        var closestSegment = SegmentAToB;
        var closestResult = closestSegment.GetClosestPoint(p, out disSquared);

        var currentSegment = SegmentBToC;
        var result = currentSegment.GetClosestPoint(p, out float dis);
        if (dis < disSquared)
        {
            closestSegment = currentSegment;
            disSquared = dis;
            closestResult = result;
        }

        currentSegment = SegmentCToA;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < disSquared)
        {
            closestSegment = currentSegment;
            disSquared = dis;
            closestResult = result;
        }

        return (closestSegment, closestResult);
    }

    /// <summary>
    /// Finds the triangle vertex closest to a specified point.
    /// </summary>
    /// <param name="p">The point to find the closest vertex to.</param>
    /// <param name="disSquared">When this method returns, contains the squared distance to the closest vertex.</param>
    /// <param name="index">When this method returns, contains the index of the closest vertex (0=A, 1=B, 2=C).</param>
    /// <returns>The position of the closest vertex.</returns>
    /// <remarks>
    /// This method compares distances to all three vertices and returns the closest one along with its index.
    /// This is useful for vertex-specific operations and snapping behaviors.
    /// </remarks>
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        var closest = A;
        disSquared = (A - p).LengthSquared();
        index = 0;
        float l = (B - p).LengthSquared();
        if (l < disSquared)
        {
            closest = B;
            disSquared = l;
            index = 1;
        }

        l = (C - p).LengthSquared();
        if (l < disSquared)
        {
            closest = C;
            disSquared = l;
            index = 2;
        }

        return closest;
    }

    /// <summary>
    /// Finds the triangle vertex furthest from a specified point.
    /// </summary>
    /// <param name="p">The point to find the furthest vertex from.</param>
    /// <param name="disSquared">When this method returns, contains the squared distance to the furthest vertex.</param>
    /// <param name="index">When this method returns, contains the index of the furthest vertex (0=A, 1=B, 2=C).</param>
    /// <returns>The position of the furthest vertex.</returns>
    /// <remarks>
    /// This method compares distances to all three vertices and returns the furthest one along with its index.
    /// This is useful for finding the vertex that provides maximum separation from a given point.
    /// </remarks>
    public Vector2 GetFurthestVertex(Vector2 p, out float disSquared, out int index)
    {
        var furthest = A;
        disSquared = (A - p).LengthSquared();
        index = 0;
        float l = (B - p).LengthSquared();
        if (l > disSquared)
        {
            furthest = B;
            disSquared = l;
            index = 1;
        }

        l = (C - p).LengthSquared();
        if (l > disSquared)
        {
            furthest = C;
            disSquared = l;
            index = 2;
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