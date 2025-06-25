using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Circle;

public readonly partial struct Circle
{
    #region Static

    /// <summary>
    /// Calculates the intersection points between two circles.
    /// </summary>
    /// <param name="aPos">The center of the first circle.</param>
    /// <param name="aRadius">The radius of the first circle.</param>
    /// <param name="bPos">The center of the second circle.</param>
    /// <param name="bRadius">The radius of the second circle.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
    {
        return IntersectCircleCircle(aPos.X, aPos.Y, aRadius, bPos.X, bPos.Y, bRadius);
    }

    /// <summary>
    /// Calculates the intersection points between two circles using scalar values.
    /// </summary>
    /// <param name="cx0">The x-coordinate of the first circle's center.</param>
    /// <param name="cy0">The y-coordinate of the first circle's center.</param>
    /// <param name="radius0">The radius of the first circle.</param>
    /// <param name="cx1">The x-coordinate of the second circle's center.</param>
    /// <param name="cy1">The y-coordinate of the second circle's center.</param>
    /// <param name="radius1">The radius of the second circle.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
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

        if ((dist == 0) && ShapeMath.EqualsF(radius0, radius1)) // (radius0 == radius1))
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

    /// <summary>
    /// Calculates the intersection points between a circle and a segment.
    /// </summary>
    /// <param name="circlePos">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="end">The end point of the segment.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleSegment(Vector2 circlePos, float circleRadius, Vector2 start, Vector2 end)
    {
        return IntersectCircleSegment(
            circlePos.X, circlePos.Y, circleRadius,
            start.X, start.Y,
            end.X, end.Y);
    }

    /// <summary>
    /// Calculates the intersection points between a circle and a ray.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="rayNormal">The normal vector of the ray.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleRay(Vector2 circleCenter, float circleRadius, Vector2 rayPoint, Vector2 rayDirection,
        Vector2 rayNormal)
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

    /// <summary>
    /// Calculates the intersection points between a circle and a line.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="lineNormal">The normal vector of the line.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleLine(Vector2 circleCenter, float circleRadius, Vector2 linePoint, Vector2 lineDirection,
        Vector2 lineNormal)
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
            var p = new CollisionPoint(closestPoint, lineNormal);
            return (p, new());
        }

        return (new(), new());
    }

    /// <summary>
    /// Calculates the intersection points between a circle and a segment using scalar values.
    /// </summary>
    /// <param name="circleX">The x-coordinate of the circle's center.</param>
    /// <param name="circleY">The y-coordinate of the circle's center.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="segStartX">The x-coordinate of the segment's start point.</param>
    /// <param name="segStartY">The y-coordinate of the segment's start point.</param>
    /// <param name="segEndX">The x-coordinate of the segment's end point.</param>
    /// <param name="segEndY">The y-coordinate of the segment's end point.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleSegment(float circleX, float circleY, float circleRadius, float segStartX,
        float segStartY, float segEndX, float segEndY)
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
                var n = Segment.Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false); // p - new Vector2(circleX, circleY);
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
                var n = Segment.Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false);
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
                var n = Segment.Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false);
                b = new CollisionPoint(p, n);
            }

            return (a, b);
        }

        return (new(), new());
    }

    #endregion
}