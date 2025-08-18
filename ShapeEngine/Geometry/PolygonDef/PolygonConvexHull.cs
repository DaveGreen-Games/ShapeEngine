using System.Numerics;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;


public partial class Polygon
{
    // Convex Hull Algorithms
    // This class implements the Jarvis March (Gift Wrapping) algorithm to find the convex hull of a set of points.
    // Reference: https://github.com/allfii/ConvexHull/tree/master
    
    // Alternative algorithms for convex hull computation:
    // - Graham scan: https://en.wikipedia.org/wiki/Graham_scan
    // - Chan's algorithm: https://en.wikipedia.org/wiki/Chan%27s_algorithm
    
    // Gift Wrapping algorithm resources:
    // - Coding Train video: https://www.youtube.com/watch?v=YNyULRrydVI
    // - Wikipedia: https://en.wikipedia.org/wiki/Gift_wrapping_algorithm
    
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

    /// <summary>
    /// Finds the convex hull of a set of points using the Jarvis March algorithm.
    /// </summary>
    /// <param name="points">The list of points to compute the convex hull for.</param>
    /// <returns>A <see cref="Polygon"/> representing the convex hull.</returns>
    private static Polygon? ConvexHull_JarvisMarch(List<Vector2> points)
    {
        if (points.Count < 3) return null; // Polygon must have at least 3 points
        var hull = new Polygon();
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

        // return new Polygon(hull);
        return hull;
    }
    /// <summary>
    /// Finds the convex hull of a list of points.
    /// </summary>
    /// <param name="points">The list of points.</param>
    /// <returns>A <see cref="Polygon"/> representing the convex hull.</returns>
    public static Polygon? FindConvexHull(List<Vector2> points)
    {
        if (points.Count < 3) return null; // Polygon must have at least 3 points
        return ConvexHull_JarvisMarch(points);
    }
    /// <summary>
    /// Finds the convex hull of a set of points.
    /// </summary>
    /// <param name="points">The points as a <see cref="Points"/> collection.</param>
    /// <returns>A <see cref="Polygon"/> representing the convex hull.</returns>
    public static Polygon? FindConvexHull(Points points)
    {
        if (points.Count < 3) return null; // Polygon must have at least 3 points
        return ConvexHull_JarvisMarch(points);
    }
    /// <summary>
    /// Finds the convex hull of a set of points.
    /// </summary>
    /// <param name="points">The points as an array of <see cref="Vector2"/>.</param>
    /// <returns>A <see cref="Polygon"/> representing the convex hull.</returns>
    public static Polygon? FindConvexHull(params Vector2[] points)
    {
        if (points.Length < 3) return null; // Polygon must have at least 3 points
        return ConvexHull_JarvisMarch(points.ToList());
    }
    /// <summary>
    /// Finds the convex hull of a polygon's points.
    /// </summary>
    /// <param name="points">The polygon whose points are used.</param>
    /// <returns>A <see cref="Polygon"/> representing the convex hull.</returns>
    public static Polygon? FindConvexHull(Polygon points)
    {
        if (points.Count < 3) return null; // Polygon must have at least 3 points
        return ConvexHull_JarvisMarch(points);
    }

    /// <summary>
    /// Finds the convex hull of multiple polygons by combining all their points.
    /// </summary>
    /// <param name="shapes">The polygons to combine.</param>
    /// <returns>A <see cref="Polygon"/> representing the convex hull of all points.</returns>
    public static Polygon? FindConvexHull(params Polygon[] shapes)
    {
        List<Vector2>? allPoints = null;
        foreach (var shape in shapes)
        {
            if(shape.Count < 3) continue; // Skip polygons with less than 3 points

            allPoints ??= [];
            allPoints.AddRange(shape);
        }

        return allPoints == null ? null : ConvexHull_JarvisMarch(allPoints);
    }
}