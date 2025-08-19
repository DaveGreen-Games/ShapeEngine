using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolylineDef;

public partial class Polyline
{
    /// <summary>
    /// Finds the closest point on a polyline (defined by a list of points) to a given point.
    /// </summary>
    /// <param name="points">The list of points defining the polyline. Must contain at least two points.</param>
    /// <param name="p">The point to which the closest point on the polyline is sought.</param>
    /// <param name="disSquared">Outputs the squared distance from <paramref name="p"/> to the closest point on the polyline.</param>
    /// <returns>The closest point on the polyline to <paramref name="p"/>.
    /// Returns <c>Vector2.Zero</c> if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline
    /// and finds the closest point on each segment to <paramref name="p"/>.
    /// The closest of these is returned.
    /// </remarks>
    public static Vector2 GetClosestPointPolylinePoint(List<Vector2> points, Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (points.Count < 2) return new();

        var first = points[0];
        var second = points[1];
        var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);

        for (var i = 1; i < points.Count - 1; i++)
        {
            var p1 = points[i];
            var p2 = points[i + 1];

            var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                closest = cp;
                disSquared = dis;
            }
        }

        return closest;
    }

    /// <summary>
    /// Finds the closest point on this polyline instance to a given point.
    /// </summary>
    /// <param name="p">The point to which the closest point on the polyline is sought.</param>
    /// <param name="disSquared">Outputs the squared distance from <paramref name="p"/> to the closest point on the polyline.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the closest point on the polyline.
    /// Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline
    /// and finds the closest point on each segment to <paramref name="p"/>.
    /// The closest of these is returned as a <see cref="IntersectionPoint"/>.
    /// </remarks>
    public new IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count < 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                closest = cp;
                disSquared = dis;
                normal = p2 - p1;
            }
        }

        return new(closest, normal.GetPerpendicularRight().Normalize());
    }

    /// <summary>
    /// Finds the closest point on this polyline instance to a given point,
    /// and returns the index of the closest segment.
    /// </summary>
    /// <param name="p">The point to which the closest point on the polyline is sought.</param>
    /// <param name="disSquared">Outputs the squared distance from <paramref name="p"/> to the closest point on the polyline.</param>
    /// <param name="index">Outputs the index of the segment containing the closest point.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the closest point on the polyline.
    /// Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline
    /// and finds the closest point on each segment to <paramref name="p"/>.
    /// The closest of these is returned as a <see cref="IntersectionPoint"/>, and the segment index is provided.
    /// </remarks>
    public new IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count < 2) return new();

        var first = this[0];
        var second = this[1];
        index = 0;
        var normal = second - first;
        var closest = Segment.GetClosestPointSegmentPoint(first, second, p, out disSquared);

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
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

    /// <summary>
    /// Finds the closest vertex (point) in the polyline to a given point.
    /// </summary>
    /// <param name="p">The point to which the closest vertex is sought.</param>
    /// <param name="disSquared">Outputs the squared distance from <paramref name="p"/> to the closest vertex.</param>
    /// <param name="index">Outputs the index of the closest vertex.</param>
    /// <returns>The closest vertex as a <see cref="Vector2"/>.
    /// Returns <c>Vector2.Zero</c> if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through all vertices and finds the one closest to <paramref name="p"/>.
    /// </remarks>
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count < 2) return new();

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

    /// <summary>
    /// Finds the furthest vertex (point) in the polyline from a given point.
    /// </summary>
    /// <param name="p">The point from which the furthest vertex is sought.</param>
    /// <param name="disSquared">Outputs the squared distance from <paramref name="p"/> to the furthest vertex.</param>
    /// <param name="index">Outputs the index of the furthest vertex.</param>
    /// <returns>The furthest vertex as a <see cref="Vector2"/>.
    /// Returns <c>Vector2.Zero</c> if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through all vertices and finds the one furthest from <paramref name="p"/>.
    /// </remarks>
    public Vector2 GetFurthestVertex(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count < 2) return new();

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
    
    /// <summary>
    /// Finds the closest point between this polyline and a given line.
    /// </summary>
    /// <param name="other">The <see cref="Line"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data.
    /// Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline
    /// and finds the closest point on each segment to the line.
    /// The closest of these is returned as a <see cref="ClosestPointResult"/>.
    /// </remarks>
    public new ClosestPointResult GetClosestPoint(Line other)
    {
        if (Count < 2) return new();
        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentLine(first, second, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            var cp = Segment.GetClosestPointSegmentLine(p1, p2, other.Point, other.Direction, out float dis);
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

    /// <summary>
    /// Finds the closest point between this polyline and a <see cref="Ray"/>.
    /// </summary>
    /// <param name="other">The <see cref="Ray"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data.
    /// Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline
    /// and finds the closest point on each segment to the ray.
    /// The closest of these is returned as a <see cref="ClosestPointResult"/>.
    /// </remarks>
    public new ClosestPointResult GetClosestPoint(Ray other)
    {
        if (Count < 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentRay(first, second, other.Point, other.Direction, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            var cp = Segment.GetClosestPointSegmentRay(p1, p2, other.Point, other.Direction, out float dis);
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

    /// <summary>
    /// Finds the closest point between this polyline and a <see cref="Segment"/>.
    /// </summary>
    /// <param name="other">The <see cref="Segment"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data.
    /// Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline
    /// and finds the closest point on each segment to the segment.
    /// The closest of these is returned as a <see cref="ClosestPointResult"/>.
    /// </remarks>
    public new ClosestPointResult GetClosestPoint(Segment other)
    {
        if (Count < 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentSegment(first, second, other.Start, other.End, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.Start, other.End, out float dis);
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

    /// <summary>
    /// Finds the closest point between this polyline and a <see cref="Circle"/>.
    /// </summary>
    /// <param name="other">The <see cref="Circle"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data.
    /// Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline
    /// and finds the closest point on each segment to the circle.
    /// The closest of these is returned as a <see cref="ClosestPointResult"/>.
    /// </remarks>
    public new ClosestPointResult GetClosestPoint(Circle other)
    {
        if (Count < 2) return new();

        var first = this[0];
        var second = this[1];
        var normal = second - first;
        var result = Segment.GetClosestPointSegmentCircle(first, second, other.Center, other.Radius, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            var cp = Segment.GetClosestPointSegmentCircle(p1, p2, other.Center, other.Radius, out float dis);
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
            new(result.other, (result.other - other.Center).Normalize()),
            disSquared,
            selfIndex
        );
    }

    /// <summary>
    /// Finds the closest point between this polyline and a <see cref="Triangle"/>.
    /// </summary>
    /// <param name="other">The <see cref="Triangle"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data.
    /// Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline and each edge of the triangle,
    /// finding the closest points between all segment pairs.
    /// The closest of these is returned as a <see cref="ClosestPointResult"/>.
    /// </remarks>
    public new ClosestPointResult GetClosestPoint(Triangle other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.B - other.A;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.C - other.B;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.A, out dis);
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

    /// <summary>
    /// Finds the closest point between this polyline and a <see cref="Quad"/>.
    /// </summary>
    /// <param name="other">The <see cref="Quad"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data.
    /// Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline and each edge of the quad,
    /// finding the closest points between all segment pairs.
    /// The closest of these is returned as a <see cref="ClosestPointResult"/>.
    /// </remarks>
    public new ClosestPointResult GetClosestPoint(Quad other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.B - other.A;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.C - other.B;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.D - other.C;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.D, other.A, out dis);
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

    /// <summary>
    /// Finds the closest point between this polyline and a <see cref="Rect"/>.
    /// </summary>
    /// <param name="other">The <see cref="Rect"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data.
    /// Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline and each edge of the rectangle,
    /// finding the closest points between all segment pairs.
    /// The closest of these is returned as a <see cref="ClosestPointResult"/>.
    /// </remarks>
    public new ClosestPointResult GetClosestPoint(Rect other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            var cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.A, other.B, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 0;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.B - other.A;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.B, other.C, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 1;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.C - other.B;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.C, other.D, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = 2;
                result = cp;
                disSquared = dis;
                selfNormal = p2 - p1;
                otherNormal = other.D - other.C;
            }

            cp = Segment.GetClosestPointSegmentSegment(p1, p2, other.D, other.A, out dis);
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

    /// <summary>
    /// Finds the closest point between this polyline and a <see cref="Polygon"/>.
    /// </summary>
    /// <param name="other">The <see cref="Polygon"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data.
    /// Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline and each edge of the polygon,
    /// finding the closest points between all segment pairs.
    /// The closest of these is returned as a <see cref="ClosestPointResult"/>.
    /// </remarks>
    public new ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            for (var j = 0; j < other.Count; j++)
            {
                var otherP1 = other[j];
                var otherP2 = other[(j + 1) % other.Count];
                var cp = Segment.GetClosestPointSegmentSegment(p1, p2, otherP1, otherP2, out float dis);
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

    /// <summary>
    /// Finds the closest point between this polyline and another <see cref="Polyline"/>.
    /// </summary>
    /// <param name="other">The <see cref="Polyline"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data.
    /// Returns a default value if either polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of both polylines,
    /// finding the closest points between all segment pairs.
    /// The closest of these is returned as a <see cref="ClosestPointResult"/>.
    /// </remarks>
    public new ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            for (var j = 0; j < other.Count - 1; j++)
            {
                var otherP1 = other[j];
                var otherP2 = other[j + 1];
                var cp = Segment.GetClosestPointSegmentSegment(p1, p2, otherP1, otherP2, out float dis);
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

    /// <summary>
    /// Finds the closest point between this polyline and another <see cref="Segments"/> collection.
    /// </summary>
    /// <param name="other">The <see cref="Segments"/> collection to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data. Returns a default value if the polyline has fewer than two points.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline and each segment of the <paramref name="other"/> collection,
    /// finding the closest points between all segment pairs.
    /// The closest of these is returned as a <see cref="ClosestPointResult"/>.
    /// </remarks>
    public new ClosestPointResult GetClosestPoint(Segments other)
    {
        if (Count < 2) return new();

        (Vector2 self, Vector2 other) result = (Vector2.Zero, Vector2.Zero);
        var selfNormal = Vector2.Zero;
        var otherNormal = Vector2.Zero;
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            for (var j = 0; j < other.Count; j++)
            {
                var otherSegment = other[j];
                var cp = Segment.GetClosestPointSegmentSegment(p1, p2, otherSegment.Start, otherSegment.End, out float dis);
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

    /// <summary>
    /// Finds the closest segment to a given point, and the corresponding point on the segment.
    /// </summary>
    /// <param name="p">The point to which the closest segment is sought.</param>
    /// <param name="disSquared">Outputs the squared distance from <paramref name="p"/> to the closest point on the segment.</param>
    /// <returns>A tuple containing the closest <see cref="Segment"/> and the closest point on that segment as a <see cref="IntersectionPoint"/>.</returns>
    /// <remarks>
    /// This method iterates through each segment of the polyline
    /// and finds the closest point on each segment to <paramref name="p"/>.
    /// The closest of these segments and points is returned.
    /// </remarks>
    public (Segment segment, IntersectionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count < 2) return (new(), new());

        var closestSegment = new Segment(this[0], this[1]);
        var closest = closestSegment.GetClosestPoint(p, out disSquared);

        for (var i = 1; i < Count - 1; i++)
        {
            var p1 = this[i];
            var p2 = this[i + 1];

            var cp = Segment.GetClosestPointSegmentPoint(p1, p2, p, out float dis);
            if (dis < disSquared)
            {
                var normal = (p2 - p1).GetPerpendicularRight().Normalize();
                closest = new(cp, normal);
                closestSegment = new Segment(p1, p2);
                disSquared = dis;
            }
        }

        return new(closestSegment, closest);
    }
    
    /// <summary>
    /// Finds the closest point on this shapes perimeter to the given <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to compare against.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing the closest point information for the shape.
    /// </returns>
    public new ClosestPointResult GetClosestPoint(IShape shape)
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
