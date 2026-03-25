using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    //TODO: Fix Docs
    /// <summary>
    /// Intersects a ray with this polygon and returns all segments of the ray that lie inside the polygon.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray. Must not be zero.</param>
    /// <returns>A list of segments inside the polygon, or null if there are fewer than 3 vertices or no valid intersections.</returns>
    /// <remarks>Segments are sorted by distance from the ray origin.</remarks>
    public bool CutRayWithPolygon(Vector2 rayPoint, Vector2 rayDirection, List<Segment> result)
    {
        if (Count < 3) return false;
        if (rayDirection.X == 0 && rayDirection.Y == 0) return false;

        rayDirection = rayDirection.Normalize();
        var intersectionPoints = IntersectPolygonRay(this, rayPoint, rayDirection);
        if (intersectionPoints == null || intersectionPoints.Count < 2) return false;

        intersectionPoints.SortClosestFirst(rayPoint);

        result.Clear();
        for (int i = 0; i < intersectionPoints.Count - 1; i += 2)
        {
            var segmentStart = intersectionPoints[i].Point;
            var segmentEnd = intersectionPoints[i + 1].Point;
            var segment = new Segment(segmentStart, segmentEnd);
            result.Add(segment);
        }

        return true;
    }

    /// <summary>
    /// Intersects a ray with this polygon and stores all segments of the ray that lie inside the polygon in the provided result list.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray. Must not be zero.</param>
    /// <param name="result">A list to store the resulting segments inside the polygon.</param>
    /// <returns>The number of segments added to the result list.</returns>
    /// <remarks>Segments are sorted by distance from the ray origin.</remarks>
    public int CutRayWithPolygon(Vector2 rayPoint, Vector2 rayDirection, ref List<Segment> result)
    {
        if (Count < 3) return 0;
        if (rayDirection.X == 0 && rayDirection.Y == 0) return 0;

        rayDirection = rayDirection.Normalize();
        var intersectionPoints = IntersectPolygonRay(this, rayPoint, rayDirection, ref intersectionPointsReference);
        if (intersectionPoints < 2) return 0;

        int count = result.Count;
        intersectionPointsReference.SortClosestFirst(rayPoint);

        for (int i = 0; i < intersectionPointsReference.Count - 1; i += 2)
        {
            var segmentStart = intersectionPointsReference[i].Point;
            var segmentEnd = intersectionPointsReference[i + 1].Point;
            var segment = new Segment(segmentStart, segmentEnd);
            result.Add(segment);
        }

        intersectionPointsReference.Clear();
        return result.Count - count;
    }
    
    //TODO: Fix Docs
    /// <summary>
    /// Creates a polygon from a set of relative points and a transform.
    /// </summary>
    /// <param name="relative">The relative points defining the shape.</param>
    /// <param name="transform">The transform to apply to each point.</param>
    /// <returns>A polygon with transformed points, or an empty polygon if fewer than 3 points.</returns>
    public static bool GetShape(Points relative, Transform2D transform, Polygon result)
    {
        if (relative.Count < 3) return false;
        for (var i = 0; i < relative.Count; i++)
        {
            result.Add(transform.ApplyTransformTo(relative[i]));
        }

        return true;
    }

    //TODO: Fix Docs
    /// <summary>
    /// Generates a random polygon in relative space.
    /// </summary>
    /// <param name="pointCount">Number of points (vertices) in the polygon. Must be at least 3.</param>
    /// <param name="minLength">Minimum distance from the origin for each point.</param>
    /// <param name="maxLength">Maximum distance from the origin for each point.</param>
    /// <returns>A randomly generated polygon.</returns>
    public static bool GenerateRelative(int pointCount, float minLength, float maxLength, Polygon result)
    {
        if (pointCount < 3) return false;
        if (Math.Abs(minLength - maxLength) < ShapeMath.Epsilon) return false;
        if (minLength > maxLength)
        {
            //swap
            (minLength, maxLength) = (maxLength, minLength);
        }
        
        float angleStep = ShapeMath.PI * 2.0f / pointCount;

        for (var i = 0; i < pointCount; i++)
        {
            float randLength = Rng.Instance.RandF(minLength, maxLength);
            var p = ShapeVec.Right().Rotate(-angleStep * i) * randLength;
            result.Add(p);
        }

        return true;
    }

    //TODO: Fix Docs
    /// <summary>
    /// Generates a random polygon centered at a given position.
    /// </summary>
    /// <param name="center">The center position of the polygon.</param>
    /// <param name="pointCount">Number of points (vertices) in the polygon. Must be at least 3.</param>
    /// <param name="minLength">Minimum distance from the center for each point.</param>
    /// <param name="maxLength">Maximum distance from the center for each point.</param>
    /// <returns>A randomly generated polygon centered at the specified position.</returns>
    public static bool Generate(Vector2 center, int pointCount, float minLength, float maxLength, Polygon result)
    {
        if (pointCount < 3) return false;
        if (Math.Abs(minLength - maxLength) < ShapeMath.Epsilon) return false;
        if (minLength > maxLength)
        {
            //swap
            (minLength, maxLength) = (maxLength, minLength);
        }
        float angleStep = ShapeMath.PI * 2.0f / pointCount;
        result.Clear();
        for (int i = 0; i < pointCount; i++)
        {
            float randLength = Rng.Instance.RandF(minLength, maxLength);
            var p = ShapeVec.Right().Rotate(-angleStep * i) * randLength;
            p += center;
            result.Add(p);
        }

        return true;
    }

    //TODO: Fix Docs
    /// <summary>
    /// Generates a polygon around the given segment. Points are generated ccw around the segment beginning with the segment start.
    /// </summary>
    /// <param name="segment">The segment to build a polygon around.</param>
    /// <param name="magMin">The minimum perpendicular magnitude factor for generating a point. (0-1)</param>
    /// <param name="magMax">The maximum perpendicular magnitude factor for generating a point. (0-1)</param>
    /// <param name="minSectionLength">The minimum factor of the length between points along the line.(0-1)</param>
    /// <param name="maxSectionLength">The maximum factor of the length between points along the line.(0-1)</param>
    /// <returns>Returns the generated polygon.</returns>
    public static bool Generate(Segment segment, Polygon result, float magMin = 0.1f, float magMax = 0.25f, float minSectionLength = 0.025f, float maxSectionLength = 0.1f)
    {
        if (segment.LengthSquared <= 0) return false;
        if (magMin <= 0 || magMax <= 0) return false;
        if (minSectionLength <= 0 || maxSectionLength <= 0) return false;
        if (magMin > magMax)
        {
            (magMin, magMax) = (magMax, magMin);
        }

        if (minSectionLength > maxSectionLength)
        {
            (minSectionLength, maxSectionLength) = (maxSectionLength, minSectionLength);       
        }
        result.Add(segment.Start);
        var dir = segment.Dir;
        var dirRight = dir.GetPerpendicularRight();
        var dirLeft = dir.GetPerpendicularLeft();
        float len = segment.Length;
        float minSectionLengthSq = (minSectionLength * len) * (minSectionLength * len);
        var cur = segment.Start;
        while (true)
        {
            cur += dir * Rng.Instance.RandF(minSectionLength, maxSectionLength) * len;
            if ((cur - segment.End).LengthSquared() < minSectionLengthSq) break;
            result.Add(cur + dirRight * Rng.Instance.RandF(magMin, magMax));
        }

        cur = segment.End;
        result.Add(cur);
        while (true)
        {
            cur -= dir * Rng.Instance.RandF(minSectionLength, maxSectionLength) * len;
            if ((cur - segment.Start).LengthSquared() < minSectionLengthSq) break;
            result.Add(cur + dirLeft * Rng.Instance.RandF(magMin, magMax));
        }

        return true;
    }
    
    
    #region Helper
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
    
    // private static Segments GetUniqueSegmentsDelaunay(Segments segments)
    // {
    //     Segments uniqueEdges = new();
    //     for (int i = segments.Count - 1; i >= 0; i--)
    //     {
    //         var edge = segments[i];
    //         if (IsSimilar(segments, edge))
    //         {
    //             uniqueEdges.Add(edge);
    //         }
    //     }
    //
    //     return uniqueEdges;
    // }

    private static bool IsSimilar(Segments segments, Segment seg)
    {
        var counter = 0;
        foreach (var segment in segments)
        {
            if (segment.IsSimilar(seg)) counter++;
            if (counter > 1) return false;
        }

        return true;
    }
    #endregion


    //TODO: Move this to quad and make private / internal or find a way to use new ClipperImmediate2d class instead of those functions!
    
    #region Inset Convex
    /// <summary>
    /// Normal convenience function: allocates and returns a new list.
    /// Thread-safe.
    /// </summary>
    public static List<Vector2> InsetConvex(IReadOnlyList<Vector2> poly, float inset, float eps = 1e-6f)
    {
        if (poly == null) throw new ArgumentNullException(nameof(poly));
        // Allocate output (garbage-producing variant)
        var result = new List<Vector2>(Math.Max(4, poly.Count));
        // Use low-gc core with temporary buffers allocated here (still allocations, but contained)
        var tmp = new List<Vector2>(Math.Max(4, poly.Count));
        InsetConvex(poly, inset, result, tmp, eps);
        return result;
    }

    /// <summary>
    /// Lowest-garbage variant:
    /// - Writes output into 'result' (cleared first).
    /// - Uses 'temp' as scratch (cleared first).
    /// - Thread-safe because caller owns buffers.
    ///
    /// After the first time the lists grow to sufficient capacity, this produces zero GC.
    /// </summary>
    public static bool InsetConvex(IReadOnlyList<Vector2> poly, float inset, List<Vector2> result, List<Vector2> temp, float eps = 1e-6f)
    {
        if (poly == null) throw new ArgumentNullException(nameof(poly));
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (temp == null) throw new ArgumentNullException(nameof(temp));

        int n = poly.Count;
        if (n < 3) return false;
        if (inset < 0) return false;
        
        result.Clear();
        temp.Clear();

        if (inset <= eps)
        {
            EnsureCapacity(result, n);
            for (int i = 0; i < n; i++) result.Add(poly[i]);
            return true;
        }

        bool ccw = SignedArea2(poly) > 0f;

        float scale = EstimateScale(poly);
        float big = MathF.Max(1f, scale) * 10000f;

        // We'll clip a big box by each inset half-plane.
        // Use result/temp alternately as buffers 'a' and 'b'.
        List<Vector2> a = result;
        List<Vector2> b = temp;

        a.Clear();
        EnsureCapacity(a, 4);
        a.Add(new Vector2(-big, -big));
        a.Add(new Vector2( big, -big));
        a.Add(new Vector2( big,  big));
        a.Add(new Vector2(-big,  big));

        b.Clear();

        for (int i = 0; i < n; i++)
        {
            Vector2 p0 = poly[i];
            Vector2 p1 = poly[(i + 1) % n];
            Vector2 e = p1 - p0;

            if (e.LengthSquared() <= eps * eps)
                continue;

            Vector2 leftN = NormalizeSafe(new Vector2(-e.Y, e.X), eps);
            Vector2 inwardN = ccw ? leftN : -leftN;
            float c = Vector2.Dot(inwardN, p0) + inset;

            ClipByHalfPlane(input: a, output: b, n: inwardN, c: c, eps: eps);

            // swap buffers (no allocations)
            var t = a; a = b; b = t;
            b.Clear();

            if (a.Count == 0)
            {
                // Collapsed -> single point
                a.Add(PolygonAreaCentroid(poly, eps));
                break;
            }
        }

        // Degenerate/tiny outcomes -> collapse to single point.
        if (a.Count >= 3)
        {
            float a2 = MathF.Abs(SignedArea2(a));
            if (a2 <= eps)
            {
                Vector2 p = CentroidOfVertices(a);
                a.Clear();
                a.Add(p);
            }
        }
        else if (a.Count == 2)
        {
            Vector2 mid = 0.5f * (a[0] + a[1]);
            a.Clear();
            a.Add(mid);
        }
        else if (a.Count == 0)
        {
            a.Add(PolygonAreaCentroid(poly, eps));
        }

        // Preserve original winding (only meaningful for polygons with 3+ vertices)
        if (a.Count >= 3)
        {
            bool outCcw = SignedArea2(a) > 0f;
            if (outCcw != ccw) a.Reverse();
        }

        // If final buffer is temp, copy it back into result.
        // (This copy is still allocation-free; it's just element copies.)
        if (!ReferenceEquals(a, result))
        {
            result.Clear();
            EnsureCapacity(result, a.Count);
            for (int i = 0; i < a.Count; i++) result.Add(a[i]);
        }

        return true;
    }
    
    // ------------------- Clipping core -------------------

    // Clips 'input' by half-plane dot(n, p) >= c into 'output'
    private static void ClipByHalfPlane(List<Vector2> input, List<Vector2> output, Vector2 n, float c, float eps)
    {
        output.Clear();
        int count = input.Count;
        if (count == 0) return;

        Vector2 prev = input[count - 1];
        float prevD = Vector2.Dot(n, prev) - c;
        bool prevInside = prevD >= -eps;

        for (int i = 0; i < count; i++)
        {
            Vector2 curr = input[i];
            float currD = Vector2.Dot(n, curr) - c;
            bool currInside = currD >= -eps;

            if (currInside)
            {
                if (!prevInside)
                    output.Add(IntersectSegmentWithLine(prev, curr, n, c, eps));
                output.Add(curr);
            }
            else if (prevInside)
            {
                output.Add(IntersectSegmentWithLine(prev, curr, n, c, eps));
            }

            prev = curr;
            prevInside = currInside;
        }

        Cleanup(output, eps);
    }

    private static Vector2 IntersectSegmentWithLine(Vector2 p0, Vector2 p1, Vector2 n, float c, float eps)
    {
        Vector2 d = p1 - p0;
        float denom = Vector2.Dot(n, d);

        if (MathF.Abs(denom) <= eps)
            return p0; // nearly parallel fallback

        float t = (c - Vector2.Dot(n, p0)) / denom;
        t = Clamp01(t);
        return p0 + d * t;
    }

    private static void Cleanup(List<Vector2> poly, float eps)
    {
        if (poly.Count == 0) return;
        float eps2 = eps * eps;

        // Remove consecutive near-duplicates
        for (int i = poly.Count - 1; i > 0; i--)
        {
            if ((poly[i] - poly[i - 1]).LengthSquared() <= eps2)
                poly.RemoveAt(i);
        }

        // Remove closing near-duplicate
        if (poly.Count > 1 && (poly[0] - poly[poly.Count - 1]).LengthSquared() <= eps2)
            poly.RemoveAt(poly.Count - 1);
    }

    // ------------------- Geometry helpers -------------------

    private static float SignedArea2(IReadOnlyList<Vector2> poly)
    {
        double sum = 0;
        for (int i = 0, n = poly.Count; i < n; i++)
        {
            Vector2 a = poly[i];
            Vector2 b = poly[(i + 1) % n];
            sum += (double)a.X * b.Y - (double)a.Y * b.X;
        }
        return (float)sum; // 2*area
    }

    private static float EstimateScale(IReadOnlyList<Vector2> poly)
    {
        float minX = poly[0].X, maxX = poly[0].X;
        float minY = poly[0].Y, maxY = poly[0].Y;

        for (int i = 1; i < poly.Count; i++)
        {
            var p = poly[i];
            if (p.X < minX) minX = p.X;
            if (p.X > maxX) maxX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.Y > maxY) maxY = p.Y;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static Vector2 NormalizeSafe(Vector2 v, float eps)
    {
        float len = v.Length();
        if (len <= eps) return Vector2.Zero;
        return v / len;
    }

    private static float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);

    private static void EnsureCapacity(List<Vector2> list, int needed)
    {
        if (list.Capacity < needed) list.Capacity = needed;
    }

    private static Vector2 CentroidOfVertices(IReadOnlyList<Vector2> poly)
    {
        Vector2 c = Vector2.Zero;
        for (int i = 0; i < poly.Count; i++) c += poly[i];
        return c / Math.Max(1, poly.Count);
    }

    // Area-weighted centroid (guaranteed inside for convex polygons)
    private static Vector2 PolygonAreaCentroid(IReadOnlyList<Vector2> poly, float eps)
    {
        double a2 = 0;     // 2*area
        double cx6a = 0;   // 6A*Cx
        double cy6a = 0;   // 6A*Cy

        int n = poly.Count;
        for (int i = 0; i < n; i++)
        {
            Vector2 p = poly[i];
            Vector2 q = poly[(i + 1) % n];
            double cross = (double)p.X * q.Y - (double)q.X * p.Y;

            a2 += cross;
            cx6a += (p.X + q.X) * cross;
            cy6a += (p.Y + q.Y) * cross;
        }

        if (Math.Abs(a2) <= eps)
            return CentroidOfVertices(poly);

        double A = a2 / 2.0;
        double cx = cx6a / (6.0 * A);
        double cy = cy6a / (6.0 * A);
        return new Vector2((float)cx, (float)cy);
    }
    #endregion
    
    #region Triangulate Convex Outline
    public static bool TriangulateConvexOutline(IReadOnlyList<Vector2> outerCCW, IReadOnlyList<Vector2> innerCCW, List<Vector2> vertices, List<int> indices, float eps = 1e-6f)
    {
        int nO = outerCCW.Count;
        int nI = innerCCW.Count;

        if (nO < 3 || nI < 3)
        {
            Console.WriteLine("[TriangulateConvexOutlineYDown] Warning: outer/inner must have at least 3 points.");
            return false;
        }

        vertices.Clear();
        indices.Clear();

        // Pack vertices as [outer..., inner...]
        int outerBase = 0;
        EnsureCapacity(vertices, nO + nI);
        for (int i = 0; i < nO; i++) vertices.Add(outerCCW[i]);
        int innerBase = vertices.Count;
        for (int i = 0; i < nI; i++) vertices.Add(innerCCW[i]);

        // Screen-space CCW (Y-down) means SignedArea2 (math Y-up) will be NEGATIVE.
        // We'll just detect and if either is not in the expected orientation, warn and proceed anyway.
        // If winding is wrong, triangles may be flipped; caller can fix their input order.
        float outerArea2 = SignedArea2_MathYUp(outerCCW);
        float innerArea2 = SignedArea2_MathYUp(innerCCW);
        if (outerArea2 >= 0 || innerArea2 >= 0)
        {
            Console.WriteLine("[TriangulateConvexOutlineYDown] Warning: expected CCW in Y-down. (SignedArea2 should be < 0 in math-Y-up test). Proceeding anyway.");
        }

        // Start at rightmost points (max X, tie max Y) on both loops.
        int iO = IndexOfRightmost(outerCCW);
        int iI = IndexOfRightmost(innerCCW);

        // We will advance exactly nO + nI times, producing that many triangles.
        // This is the correct triangle count for a convex ring: nO + nI triangles.
        // (Think of it as triangulating a convex polygon with a convex hole -> nO + nI triangles.)
        int stepsO = 0, stepsI = 0;

        // Pre-ensure index capacity (avoid growth GC in hot loops)
        EnsureCapacity(indices, (nO + nI) * 3);

        // Helper local to map loop index -> vertex buffer index
        int O(int idx) => outerBase + idx;
        int I(int idx) => innerBase + idx;

        for (int guard = 0; guard < nO + nI; guard++)
        {
            int nextO = (iO + 1) % nO;
            int nextI = (iI + 1) % nI;

            Vector2 o = outerCCW[iO];
            Vector2 oN = outerCCW[nextO];
            Vector2 i = innerCCW[iI];
            Vector2 iN = innerCCW[nextI];

            Vector2 dO = oN - o;
            Vector2 dI = iN - i;

            // Decide which boundary to advance.
            // We need an ordering of edge directions around the loop.
            // In Y-down coordinates, the sign of cross is flipped relative to math Y-up.
            // We'll compute cross in math sense (X right, Y up) by negating Y to get Y-up vectors.
            // Equivalent: crossYDown(a,b) = -crossMath(a,b).
            float crossMath = Cross(new Vector2(dO.X, -dO.Y), new Vector2(dI.X, -dI.Y));

            bool advanceOuter;
            if (MathF.Abs(crossMath) <= eps)
            {
                // Nearly parallel; advance shorter edge for stability
                advanceOuter = dO.LengthSquared() <= dI.LengthSquared();
            }
            else
            {
                // If dO comes "before" dI in CCW (Y-down), advance outer.
                // Because we converted to math-Y-up via (x, -y), we can use crossMath > 0 as CCW ordering there.
                advanceOuter = crossMath > 0;
            }

            if (advanceOuter)
            {
                if (stepsO >= nO) advanceOuter = false; // outer already fully advanced
            }
            else
            {
                if (stepsI >= nI) advanceOuter = true;  // inner already fully advanced
            }

            if (advanceOuter)
            {
                // Emit triangle between inner current and outer edge (iI, iO, nextO)
                // Must be CCW in Y-down. We'll enforce by checking signed area in Y-down and swapping if needed.
                int a = I(iI);
                int b = O(iO);
                int c = O(nextO);
                AddTriangleCCW_YDown(vertices, indices, a, b, c, eps);

                iO = nextO;
                stepsO++;
            }
            else
            {
                // Emit triangle between outer current and inner edge (iO, nextI, iI) or similar.
                int a = O(iO);
                int b = I(nextI);
                int c = I(iI);
                AddTriangleCCW_YDown(vertices, indices, a, b, c, eps);

                iI = nextI;
                stepsI++;
            }
        }

        // Validate we produced a multiple of 3 indices
        if ((indices.Count % 3) != 0)
        {
            Console.WriteLine("[TriangulateConvexOutlineYDown] Warning: produced non-multiple-of-3 indices. Clearing.");
            indices.Clear();
            return false;
        }

        return true;
    }

    // --- Triangle winding enforcement (Y-down CCW) ---

    private static void AddTriangleCCW_YDown(List<Vector2> v, List<int> idx, int i0, int i1, int i2, float eps)
    {
        Vector2 a = v[i0];
        Vector2 b = v[i1];
        Vector2 c = v[i2];

        // In Y-down, "CCW" corresponds to negative cross in math coordinates.
        // Compute signed area *2 in Y-down directly:
        // crossYDown = crossMath( (b-a) with Y flipped? ) but easiest:
        // Use math cross with actual coordinates and then negate.
        float crossMath = Cross(b - a, c - a);
        float crossYDown = -crossMath;

        if (crossYDown <= eps)
        {
            // swap to flip winding
            idx.Add(i0);
            idx.Add(i2);
            idx.Add(i1);
        }
        else
        {
            idx.Add(i0);
            idx.Add(i1);
            idx.Add(i2);
        }
    }

    // --- Helpers ---

    private static int IndexOfRightmost(IReadOnlyList<Vector2> poly)
    {
        int best = 0;
        for (int i = 1; i < poly.Count; i++)
        {
            var p = poly[i];
            var b = poly[best];
            if (p.X > b.X || (p.X == b.X && p.Y > b.Y))
                best = i;
        }
        return best;
    }

    // Signed area *2 in standard math coords (Y-up).
    private static float SignedArea2_MathYUp(IReadOnlyList<Vector2> poly)
    {
        double sum = 0;
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 a = poly[i];
            Vector2 b = poly[(i + 1) % poly.Count];
            sum += (double)a.X * b.Y - (double)a.Y * b.X;
        }
        return (float)sum;
    }

    private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

    private static void EnsureCapacity<T>(List<T> list, int needed)
    {
        if (list.Capacity < needed) list.Capacity = needed;
    }
    #endregion
}