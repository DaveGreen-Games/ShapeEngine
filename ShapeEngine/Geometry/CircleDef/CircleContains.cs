using System.Numerics;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

public readonly partial struct Circle
{
    /// <summary>
    /// Determines whether the circle contains a given point.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <returns><c>true</c> if the point is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsPoint(Vector2 p) => ContainsCirclePoint(Center, Radius, p);


    /// <summary>
    /// Determines whether the circle contains two given points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns><c>true</c> if both points are inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(Vector2 a, Vector2 b) => ContainsCirclePoints(Center, Radius, a, b);

    /// <summary>
    /// Determines whether the circle contains three given points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="c">The third point.</param>
    /// <returns><c>true</c> if all points are inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(Vector2 a, Vector2 b, Vector2 c) => ContainsCirclePoints(Center, Radius, a, b, c);

    /// <summary>
    /// Determines whether the circle contains four given points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="c">The third point.</param>
    /// <param name="d">The fourth point.</param>
    /// <returns><c>true</c> if all points are inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => ContainsCirclePoints(Center, Radius, a, b, c, d);

    /// <summary>
    /// Determines whether the circle contains a list of points.
    /// </summary>
    /// <param name="points">The list of points to check.</param>
    /// <returns><c>true</c> if all points are inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(List<Vector2> points) => ContainsCirclePoints(Center, Radius, points);

    /// <summary>
    /// Determines whether a point [<paramref name="p"/>] is within a sector of the circle,
    /// defined by a center angle [<paramref name="rotationRad"/>] and a sector angle [<paramref name="sectorAngleRad"/>] (in radians).
    /// The sector is centered at <paramref name="rotationRad"/> and spans <paramref name="sectorAngleRad"/>.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <param name="rotationRad">The center angle of the sector in radians.</param>
    /// <param name="sectorAngleRad">The angle of the sector in radians.</param>
    /// <returns><c>true</c> if the point is within the sector; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// <paramref name="rotationRad"/> is used to calculate the direction of the sector.
    /// <paramref name="sectorAngleRad"/> defines the sector centered around the calculated direction.
    /// If the angle between the sector direction and the direction from the circle <see cref="Center"/> to <paramref name="p"/> has a smaller
    /// absolute angle than <paramref name="sectorAngleRad"/>,
    /// the function returns true, otherwise false.
    /// </remarks>
    public bool ContainsPointSector(Vector2 p, float rotationRad, float sectorAngleRad)
    {
        if (sectorAngleRad <= 0f) return false;
        rotationRad = ShapeMath.WrapAngleRad(rotationRad);

        var dir = ShapeVec.VecFromAngleRad(rotationRad);
        var a = dir.AngleRad(p - Center);
        return MathF.Abs(a) < sectorAngleRad * 0.5f;
    }

    /// <summary>
    /// Determines whether a point [<paramref name="p"/>] is within a sector of the circle,
    /// defined by a direction vector [<paramref name="dir"/>] and a sector angle [<paramref name="sectorAngleRad"/>] (in radians).
    /// The sector is centered along <paramref name="dir"/> and spans <paramref name="sectorAngleRad"/>.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <param name="dir">The direction vector representing the center of the sector.</param>
    /// <param name="sectorAngleRad">The angle of the sector in radians.</param>
    /// <returns><c>true</c> if the point is within the sector; otherwise, <c>false</c>.</returns>
    /// /// <remarks>
    /// <paramref name="sectorAngleRad"/> defines the sector centered around the <paramref name="dir"/>.
    /// If the angle between <paramref name="dir"/> and the direction from the circle <see cref="Center"/> to <paramref name="p"/> has a smaller
    /// absolute angle than <paramref name="sectorAngleRad"/>,
    /// the function returns true, otherwise false.
    /// </remarks>
    public bool ContainsPointSector(Vector2 p, Vector2 dir, float sectorAngleRad)
    {
        if (sectorAngleRad <= 0f) return false;
        if (dir.X == 0f && dir.Y == 0f) return false;
        if (!ContainsPoint(p)) return false;

        var a = dir.AngleRad(p - Center);
        return MathF.Abs(a) < sectorAngleRad * 0.5f;
    }

    /// <summary>
    /// Determines whether the circle contains a collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object to check.</param>
    /// <returns><c>true</c> if the collision object is inside the circle;
    /// otherwise, <c>false</c>.</returns>
    public bool ContainsCollisionObject(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return false;
        foreach (var collider in collisionObject.Colliders)
        {
            if (!ContainsCollider(collider)) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the circle contains a collider.
    /// </summary>
    /// <param name="collider">The collider to check.</param>
    /// <returns><c>true</c> if the collider is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsCollider(Collider collider)
    {
        if (!collider.Enabled) return false;
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

    /// <summary>
    /// Determines whether the circle contains a shape.
    /// </summary>
    /// <param name="segment">The segment to check.</param>
    /// <returns><c>true</c> if the segment is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Segment segment)
    {
        return ContainsCircleSegment(Center, Radius, segment.Start, segment.End);
    }

    /// <summary>
    /// Determines whether the circle contains another circle.
    /// </summary>
    /// <param name="circle">The circle to check.</param>
    /// <returns><c>true</c> if the other circle is inside the current circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Circle circle)
    {
        return ContainsCircleCircle(Center, Radius, circle.Center, circle.Radius);
    }

    /// <summary>
    /// Determines whether the circle contains a rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to check.</param>
    /// <returns><c>true</c> if the rectangle is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Rect rect)
    {
        return ContainsCircleRect(Center, Radius, rect.A, rect.B, rect.C, rect.D);
    }

    /// <summary>
    /// Determines whether the circle contains a triangle.
    /// </summary>
    /// <param name="triangle">The triangle to check.</param>
    /// <returns><c>true</c> if the triangle is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Triangle triangle)
    {
        return ContainsCircleTriangle(Center, Radius, triangle.A, triangle.B, triangle.C);
    }

    /// <summary>
    /// Determines whether the circle contains a quad.
    /// </summary>
    /// <param name="quad">The quad to check.</param>
    /// <returns><c>true</c> if the quad is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Quad quad)
    {
        return ContainsCircleQuad(Center, Radius, quad.A, quad.B, quad.C, quad.D);
    }

    /// <summary>
    /// Determines whether the circle contains a polyline.
    /// </summary>
    /// <param name="polyline">The polyline to check.</param>
    /// <returns><c>true</c> if the polyline is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Polyline polyline)
    {
        return ContainsCirclePolyline(Center, Radius, polyline);
    }

    /// <summary>
    /// Determines whether the circle contains a polygon.
    /// </summary>
    /// <param name="polygon">The polygon to check.</param>
    /// <returns><c>true</c> if the polygon is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Polygon polygon)
    {
        return ContainsCirclePolygon(Center, Radius, polygon);
    }

    /// <summary>
    /// Determines whether the circle contains a set of points.
    /// </summary>
    /// <param name="points">The points to check.</param>
    /// <returns><c>true</c> if all points are inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Points points)
    {
        return ContainsCirclePoints(Center, Radius, points);
    }

    /// <summary>
    /// Determines whether this shape contains the specified <see cref="IShape"/>.
    /// </summary>
    /// <param name="shape">The shape to check.</param>
    /// <returns><c>true</c> if the shape is inside this shape; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(IShape shape)
    {
        return shape.GetShapeType() switch
        {
            ShapeType.Circle => ContainsShape(shape.GetCircleShape()),
            ShapeType.Segment => ContainsShape(shape.GetSegmentShape()),
            ShapeType.Triangle => ContainsShape(shape.GetTriangleShape()),
            ShapeType.Rect => ContainsShape(shape.GetRectShape()),
            ShapeType.Quad => ContainsShape(shape.GetQuadShape()),
            ShapeType.Poly => ContainsShape(shape.GetPolygonShape()),
            ShapeType.PolyLine => ContainsShape(shape.GetPolylineShape()),
            _ => false
        };
    }
}