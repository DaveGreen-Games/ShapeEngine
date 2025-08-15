using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PointsDef;

public partial class Points
{
    /// <summary>
    /// Finds the point in this collection that is closest to the specified position.
    /// </summary>
    /// <param name="p">The position to compare against.</param>
    /// <param name="disSquared">The squared distance from the closest point to <paramref name="p"/>.</param>
    /// <returns>The closest <see cref="Vector2"/> in the collection, or a default value if the collection is empty.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="Vector2"/> and sets <paramref name="disSquared"/> to -1.
    /// </remarks>
    public Vector2 GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        if (Count <= 0) return new();

        if (Count == 1)
        {
            disSquared = (this[0] - p).LengthSquared();
            return this[0];
        }


        var closestPoint = this[0];
        var minDisSq = (closestPoint - p).LengthSquared();

        for (var i = 1; i < Count; i++)
        {
            var disSq = (this[i] - p).LengthSquared();
            if (disSq >= minDisSq) continue;
            minDisSq = disSq;
            closestPoint = this[i];
        }

        return closestPoint;
    }

    /// <summary>
    /// Finds the point in this collection that is closest to the specified position, and returns its index and squared distance.
    /// </summary>
    /// <param name="p">The position to compare against.</param>
    /// <param name="disSquared">The squared distance from the closest point to <paramref name="p"/>.</param>
    /// <param name="index">The index of the closest point in the collection.</param>
    /// <returns>The closest <see cref="Vector2"/> in the collection, or a default value if the collection is empty.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="Vector2"/>, sets <paramref name="disSquared"/> to -1, and <paramref name="index"/> to -1.
    /// </remarks>
    public Vector2 GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        disSquared = -1;
        index = -1;
        if (Count <= 0) return new();

        var closest = this[0];
        index = 0;
        disSquared = (closest - p).LengthSquared();

        for (var i = 1; i < Count; i++)
        {
            var next = this[i];
            var dis = (next - p).LengthSquared();
            if (dis < disSquared)
            {
                index = i;
                closest = next;
                disSquared = dis;
            }
        }

        return closest;
    }

    /// <summary>
    /// Finds the closest point between any of the points to any collider in the given collision object.
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
    /// Finds the closest point between any of the points to the given collider.
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
    /// Finds the closest point in this collection to the specified <see cref="Line"/>.
    /// </summary>
    /// <param name="other">The <see cref="Line"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the line.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Line other)
    {
        if (Count <= 0) return new();
        var closest = this[0];
        var pointOnOther = Line.GetClosestPointLinePoint(other.Point, other.Direction, closest, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var next = this[i];

            var result = Line.GetClosestPointLinePoint(other.Point, other.Direction, next, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                closest = next;
                pointOnOther = result;
                disSquared = dis;
            }
        }

        var dir = closest - pointOnOther;
        var dot = Vector2.Dot(dir, other.Normal);
        Vector2 otherNormal;
        if (dot >= 0) otherNormal = other.Normal;
        else otherNormal = -other.Normal;

        return new(
            new(closest, -dir.Normalize()),
            new(pointOnOther, otherNormal),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Ray"/>.
    /// </summary>
    /// <param name="other">The <see cref="Ray"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the ray.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        if (Count <= 0) return new();
        var closest = this[0];
        var pointOnOther = Ray.GetClosestPointRayPoint(other.Point, other.Direction, closest, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var next = this[i];

            var result = Ray.GetClosestPointRayPoint(other.Point, other.Direction, next, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                closest = next;
                pointOnOther = result;
                disSquared = dis;
            }
        }

        var dir = closest - pointOnOther;
        var dot = Vector2.Dot(dir, other.Normal);
        Vector2 otherNormal;
        if (dot >= 0) otherNormal = other.Normal;
        else otherNormal = -other.Normal;

        return new(
            new(closest, -dir.Normalize()),
            new(pointOnOther, otherNormal),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Segment"/>.
    /// </summary>
    /// <param name="other">The <see cref="Segment"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the segment.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        if (Count <= 0) return new();
        var closest = this[0];
        var pointOnOther = Segment.GetClosestPointSegmentPoint(other.Start, other.End, closest, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var next = this[i];

            var result = Segment.GetClosestPointSegmentPoint(other.Start, other.End, next, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                closest = next;
                pointOnOther = result;
                disSquared = dis;
            }
        }

        var dir = closest - pointOnOther;
        var dot = Vector2.Dot(dir, other.Normal);
        Vector2 otherNormal;
        if (dot >= 0) otherNormal = other.Normal;
        else otherNormal = -other.Normal;

        return new(
            new(closest, -dir.Normalize()),
            new(pointOnOther, otherNormal),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Circle"/>.
    /// </summary>
    /// <param name="other">The <see cref="Circle"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the circle.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        if (Count <= 0) return new();
        var closest = this[0];
        var pointOnOther = Circle.GetClosestPointCirclePoint(other.Center, other.Radius, closest, out float disSquared);
        var selfIndex = 0;

        for (var i = 1; i < Count; i++)
        {
            var next = this[i];

            var result = Circle.GetClosestPointCirclePoint(other.Center, other.Radius, next, out float dis);
            if (dis < disSquared)
            {
                selfIndex = i;
                closest = next;
                pointOnOther = result;
                disSquared = dis;
            }
        }

        var dir = closest - pointOnOther;
        var otherNormal = dir.Normalize();

        return new(
            new(closest, -otherNormal),
            new(pointOnOther, otherNormal),
            disSquared,
            selfIndex);
    }

    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Triangle"/>.
    /// </summary>
    /// <param name="other">The <see cref="Triangle"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the triangle.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        if (Count <= 0) return new();

        var closestOther = new IntersectionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var point = this[i];

            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()),
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Quad"/>.
    /// </summary>
    /// <param name="other">The <see cref="Quad"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the quad.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        if (Count <= 0) return new();

        var closestOther = new IntersectionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var point = this[i];

            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()),
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Rect"/>.
    /// </summary>
    /// <param name="other">The <see cref="Rect"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the rectangle.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        if (Count <= 0) return new();

        var closestOther = new IntersectionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var point = this[i];

            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()),
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Polygon"/>.
    /// </summary>
    /// <param name="other">The <see cref="Polygon"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the polygon.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (Count <= 0) return new();

        var closestOther = new IntersectionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var point = this[i];

            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()),
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Finds the closest point in this collection to the specified <see cref="Polyline"/>.
    /// </summary>
    /// <param name="other">The <see cref="Polyline"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the polyline.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="ClosestPointResult"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (Count <= 0) return new();

        var closestOther = new IntersectionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var point = this[i];

            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()),
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
    }

    /// <summary>
    /// Finds the closest point in this collection to the specified shape.
    /// </summary>
    /// <param name="other">The <see cref="ClosestPointResult"/> to compare against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest point in this collection and the corresponding point on the segments.</returns>
    /// <remarks>
    /// If the collection is empty, returns a default <see cref="Segments"/>.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(Segments other)
    {
        if (Count <= 0) return new();

        var closestOther = new IntersectionPoint();
        int selfIndex = -1;
        int otherIndex = -1;
        float disSquared = -1f;

        for (var i = 0; i < Count; i++)
        {
            var point = this[i];

            var cp = other.GetClosestPoint(point, out float dis, out int index);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = i;
                otherIndex = index;
                closestOther = cp;
                disSquared = dis;
            }
        }

        var selfPoint = this[selfIndex];
        return new(
            new(selfPoint, (closestOther.Point - selfPoint).Normalize()),
            closestOther,
            disSquared,
            selfIndex,
            otherIndex);
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