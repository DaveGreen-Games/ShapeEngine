using Raylib_CsLo;
using ShapeEngine.Core;
using System.Numerics;

namespace ShapeEngine.Lib
{
    /*
        //added to sgeometry
        public static bool ContainsPoint(List<Vector2> poly, Vector2 p)
        {
            bool oddNodes = false;
            int num = poly.Count;
            int j = num - 1;
            for (int i = 0; i < num; i++)
            {
                var vi = poly[i];
                var vj = poly[j];
                if (vi.Y < p.Y && vj.Y >= p.Y || vj.Y < p.Y && vi.Y >= p.Y)
                {
                    if (vi.X + (p.Y - vi.Y) / (vj.Y - vi.Y) * (vj.X - vi.X) < p.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }
        public static bool ContainsPoly(List<Vector2> poly, List<Vector2> otherPoly)
        {
            for (int i = 0; i < otherPoly.Count; i++)
            {
                if (!ContainsPoint(poly, otherPoly[i]))
                {
                    return false;
                }
            }
            return true;
        }
        */
    /*
        public static bool IsClockwise(List<Vector2> poly) { return GetArea(poly) > 0f; }
        public static bool IsConvex(List<Vector2> poly)
        {
            int num = poly.Count;
            bool isPositive = false;

            for (int i = 0; i < num; i++)
            {
                int prevIndex = (i == 0) ? num -1 : i - 1;
                int nextIndex = (i == num - 1) ? 0 : i + 1;
                var d0 = poly[i] - poly[prevIndex];
                var d1 = poly[nextIndex] - poly[i];
                var newIsP = d0.Cross(d1) > 0f;
                if (i == 0) isPositive = true;
                else if (isPositive != newIsP) return false;
            }
            return true;
        }
        */
    /*
        public static Vector2 GetRandomPointOnEdge(List<Vector2> poly)
        {
            var edges = GetEdges(poly);
            var re = edges[SRNG.randI(edges.Count)];
            return re.start.Lerp(re.end, SRNG.randF());
        }
        public static Vector2 GetRandomPointFast(List<Vector2> poly)
        {
            //only work with convex polygons
            var edges = GetEdges(poly);
            var ea = SRNG.randCollection(edges, true);
            var eb = SRNG.randCollection(edges);

            var pa = ea.start.Lerp(ea.end, SRNG.randF());
            var pb = eb.start.Lerp(eb.end, SRNG.randF());
            return pa.Lerp(pb, SRNG.randF());
        }
        
        public static Vector2 GetRandomPoint(List<Vector2> poly)
        {
            //triangulate poly
            //pick triangle based on area as weight
            //pick random point in triangle
            return new();
        }
        */
    /*
    internal static class ToxiLibPolygon
    {

        public static void Add(List<Vector2> poly, Vector2 v)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                poly[i] += v;
            }
        }
        public static void Center(List<Vector2> poly, Vector2 newCenter)
        {
            var centroid = SPoly.GetCentroid(poly);
            var delta = newCenter - centroid;
            Add(poly, delta);
        }


        public static float GetCircumference(List<Vector2> poly)
        {
            float circ = 0;

            for (int i = 0; i < poly.Count; i++)
            {
                float dis = (poly[i] - poly[(i + 1) % poly.Count]).Length();
                circ += dis;
            }

            return circ;
        }
        public static List<Segment> GetEdges(List<Vector2> poly)
        {
            List<Segment> edges = new();
            for (int i = 0; i < poly.Count; i++)
            {
                var s = new Segment(poly[i], poly[(i + 1) % poly.Count]);
                edges.Add(s);
            }
            return edges;
        }
        
        
        /// <summary>
        /// Returns the vertex at the given index. This function follows Python
        /// convention, in that if the index is negative, it is considered relative
        /// to the list end. Therefore the vertex at index -1 is the last vertex in
        /// the list.
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Vector2 GetVertex(List<Vector2> poly, int index)
        {
            if (index < 0)
            {
                index += poly.Count;
            }
            return poly[index];
        }
        public static Circle GetBoundingCircle(List<Vector2> poly)
        {
            float maxD = 0f;
            int num = poly.Count;
            Vector2 origin = new();
            for (int i = 0; i < num; i++) { origin += poly[i]; }
            origin *= (1f / (float)num);
            for (int i = 0; i < num; i++)
            {
                float d = (origin - poly[i]).LengthSquared();
                if (d > maxD) maxD = d;
            }

            return new Circle(origin, MathF.Sqrt(maxD));
        }
        public static Vector2 GetCentroid(List<Vector2> poly)
        {
            Vector2 result = new();

            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 a = poly[i];
                Vector2 b = poly[(i + 1) % poly.Count];
                float factor = a.X * b.Y - b.X * a.Y;
                result.X += (a.X + b.X) * factor;
                result.Y += (a.Y + b.Y) * factor;
            }

            return result * (1f / (GetArea(poly) * 6f));
        }
        public static Rect GetBoundingRect(List<Vector2> poly)
        {
            Vector2 first = poly[0];
            Rect bounds = new Rect(first.X, first.Y, 0, 0);
            for (int i = 1; i < poly.Count; i++)
            {
                bounds.Enlarge(poly[i]);
            }
            return bounds;
        }
        
        
        
        



        public static void Add(Polygon poly, Vector2 v)
        {
            for (int i = 0; i < poly.points.Count; i++)
            {
                poly.points[i] += v;
            }
        }
        public static void Center(Polygon poly, Vector2 newCenter)
        {
            var delta = newCenter - poly.center;
            Add(poly, delta);
            poly.center = newCenter;
        }
    
    }
    */

    public struct FractureInfo
    {
        public Polygons NewShapes;
        public Polygons Cutouts;
        public Triangulation Pieces;

        public FractureInfo(Polygons newShapes, Polygons cutouts, Triangulation pieces)
        {
            this.NewShapes = newShapes;
            this.Cutouts = cutouts;
            this.Pieces = pieces;
        }
    }
    public class FractureHelper
    {
        public float MinArea { get; set; }
        public float MaxArea { get; set; }
        public float KeepChance { get; set; }
        public float NarrowValue{ get; set; }

        //public float DivisionChance { get; set; } = 0.5f;
        //public int MinDivisionCount { get; set; } = 3;
        //public int MaxDivisionCount { get; set; } = 9;

        public FractureHelper(float minArea, float maxArea, float keepChance = 0.5f, float narrowValue = 0.2f)
        {
            this.MinArea = minArea;
            this.MaxArea = maxArea;
            this.KeepChance = keepChance;
            this.NarrowValue = narrowValue;
        }
        
        public FractureInfo Fracture(Polygon shape, Polygon cutShape)
        {
            var cutOuts = SClipper.Intersect(shape, cutShape).ToPolygons(true);
            var newShapes = SClipper.Difference(shape, cutShape).ToPolygons(true);
            Triangulation pieces = new();
            foreach (var cutOut in cutOuts)
            {
                var fracturePieces = cutOut.Triangulate().Subdivide(MinArea, MaxArea, KeepChance, NarrowValue);
                pieces.AddRange(fracturePieces);
            }

            return new(newShapes, cutOuts, pieces);
        }
    }

    

    public static class SPoly
    {


        public static (Polygons newShapes, Polygons cutOuts) Cut(this Polygon poly, Polygon cutShape) 
        {
            var cutOuts = SClipper.Intersect(poly, cutShape).ToPolygons(true);
            var newShapes = SClipper.Difference(poly, cutShape).ToPolygons(true);

            return (newShapes, cutOuts);
        }
        public static (Polygons newShapes, Polygons cutOuts) CutMany(this Polygon poly, Polygons cutShapes) 
        {
            var cutOuts = SClipper.IntersectMany(poly, cutShapes).ToPolygons(true);
            var newShapes = SClipper.DifferenceMany(poly, cutShapes).ToPolygons(true);
            return (newShapes, cutOuts);
        }
        public static (Polygons newShapes, Polygons overlaps) Combine(this Polygon poly, Polygon other) 
        {
            var overlaps = SClipper.Intersect(poly, other).ToPolygons(true);
            var newShapes = SClipper.Union(poly, other).ToPolygons(true);
            return (newShapes, overlaps);
        }
        public static (Polygons newShapes, Polygons overlaps) Combine(this Polygon poly, Polygons others) 
        { 
            var overlaps = SClipper.IntersectMany(poly, others).ToPolygons(true);
            var newShapes = SClipper.UnionMany(poly, others).ToPolygons(true);
            return (newShapes, overlaps);
        }
        public static (Polygons newShapes, Polygons cutOuts) CutSimple(this Polygon poly, Vector2 cutPos, float minCutRadius, float maxCutRadius, int pointCount = 16)
        {
            var cut = Generate(cutPos, pointCount, minCutRadius, maxCutRadius);
            return poly.Cut(cut);
        }
        public static (Polygons newShapes, Polygons cutOuts) CutSimple(this Polygon poly, Segment cutLine, float minSectionLength = 0.025f, float maxSectionLength = 0.1f, float minMagnitude = 0.05f, float maxMagnitude = 0.25f)
        {
            var cut = Generate(cutLine, minMagnitude, maxMagnitude, minSectionLength, maxSectionLength);
            return poly.Cut(cut);
        }
        


        /// <summary>
        /// Triangulates a set of points. Only works with non self intersecting shapes.
        /// </summary>
        /// <param name="points">The points to triangulate. Can be any set of points. (polygons as well) </param>
        /// <returns></returns>
        public static Triangulation TriangulateDelaunay(IEnumerable<Vector2> points)
        {
            Triangle supraTriangle = GetBoundingTriangle(points, 2f);
            return TriangulateDelaunay(points, supraTriangle);
        }

        /// <summary>
        /// Triangulates a set of points. Only works with non self intersecting shapes.
        /// </summary>
        /// <param name="points">The points to triangulate. Can be any set of points. (polygons as well) </param>
        /// <param name="supraTriangle">The triangle that encapsulates all the points.</param>
        /// <returns></returns>
        public static Triangulation TriangulateDelaunay(IEnumerable<Vector2> points, Triangle supraTriangle)
        {
            Triangulation triangles = new();

            triangles.Add(supraTriangle);

            foreach (var p in points)
            {
                Triangulation badTriangles = new();

                //Identify 'bad triangles'
                for (int triIndex = triangles.Count - 1; triIndex >= 0; triIndex--)
                {
                    Triangle triangle = triangles[triIndex];

                    //A 'bad triangle' is defined as a triangle who's CircumCentre contains the current point
                    var circumCircle = triangle.GetCircumCircle();
                    float distSq = Vector2.DistanceSquared(p, circumCircle.center);
                    if (distSq < circumCircle.radius * circumCircle.radius)
                    {
                        badTriangles.Add(triangle);
                        triangles.RemoveAt(triIndex);
                    }
                }

                Segments allEdges = new();
                foreach (var badTriangle in badTriangles) { allEdges.AddRange(badTriangle.GetEdges()); }

                Segments uniqueEdges = allEdges.GetUniqueSegments();
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


        /*
        /// <summary>
        /// Subdivide the triangulation until all triangles are smaller than min area.
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="minArea"></param>
        /// <returns></returns>
        public static Triangulation Subdivide(this Triangulation triangles, float minArea)
        {
            Triangulation subdivision = new();
            Triangulation final = new();
            foreach (var tri in triangles)
            {
                var area = tri.GetArea();
                if (minArea >= area) final.Add(tri);
                else subdivision.AddRange(tri.Triangulate());
            }

            if (subdivision.Count > 0) final.AddRange(Subdivide(subdivision, minArea));

            return final;
        }
        */

        /*
        /// <summary>
        /// Triangulates and subdivides the triangulation until the area of triangles reaches the min area limit.
        /// </summary>
        /// <param name="areaThresholdFactor">Used to calculate the min area limit. The threshold factor is multiplied with the total area of the polygon to recieve the min area limit.</param>
        /// <returns></returns>
        public Triangulation Fracture(float areaThresholdFactor = 0f)
        {
            var triangulation = Triangulate();
            if (areaThresholdFactor <= 0f || areaThresholdFactor >= 1f) return triangulation;

            float totalArea = triangulation.GetArea();
            float minArea = totalArea * areaThresholdFactor;

            ////var1
            //Triangulation final = new();
            //foreach (var tri in triangulation)
            //{
            //    final.AddRange(Subdivide(tri, minArea));
            //}
            //return final;

            //var2
            return triangulation.Subdivide(minArea); // SPoly.Subdivide(triangulation, minArea);
        }
        */





        /// <summary>
        /// Get a rect that encapsulates all points.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Rect GetBoundingBox(IEnumerable<Vector2> points)
        {
            if (points.Count() < 2) return new();
            Vector2 start = points.First();
            Rect r = new(start.X, start.Y, 0, 0);

            foreach (var p in points)
            {
                r = SRect.Enlarge(r, p);
            }
            return r;
        }
        
        /// <summary>
        /// Get a triangle the encapsulates all points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="marginFactor"> A factor for scaling the final triangle.</param>
        /// <returns></returns>
        public static Triangle GetBoundingTriangle(IEnumerable<Vector2> points, float marginFactor = 1f)
        {
            var bounds = GetBoundingBox(points);
            float dMax = SVec.Max(bounds.Size) * marginFactor; // SVec.Max(bounds.BottomRight - bounds.BottomLeft) + margin; //  Mathf.Max(bounds.maxX - bounds.minX, bounds.maxY - bounds.minY) * Margin;
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


            return new Triangle(a, b, c);
        }
        

        public static Polygon GetShape(this Polygon relative, Vector2 pos, float rotRad, Vector2 scale)
        {
            if (relative.Count < 3) return new();
            Polygon shape = new();
            for (int i = 0; i < relative.Count; i++)
            {
                shape.Add(pos + SVec.Rotate(relative[i], rotRad) * scale);
            }
            return shape;
        }
        public static Polygon Center(this Polygon p, Vector2 newCenter)
        {
            var centroid = p.GetCentroid();
            var delta = newCenter - centroid;
            return Move(p, delta);
        }
        public static Polygon Move(this Polygon p, Vector2 translation)
        {
            Polygon result = new();
            for (int i = 0; i < p.Count; i++)
            {
                result.Add(p[i] + translation);
            }
            return result;
        }
        public static Polygon Rotate(this Polygon p, Vector2 pivot, float rotRad)
        {
            if (p.Count < 3) return new();
            Polygon rotated = new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 w = p[i] - pivot;
                rotated.Add(pivot + w.Rotate(rotRad));
            }
            return rotated;
        }
        public static Polygon Scale(this Polygon p, float scale)
        {
            Polygon shape = new();
            for (int i = 0; i < p.Count; i++)
            {
                shape.Add(p[i] * scale);
            }
            return shape;
        }
        public static Polygon Scale(this Polygon p, Vector2 pivot, float scale)
        {
            if (p.Count < 3) return new();
            Polygon scaled = new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 w = p[i] - pivot;
                scaled.Add(pivot + w * scale);
            }
            return scaled;
        }
        public static Polygon Scale(this Polygon p, Vector2 pivot, Vector2 scale)
        {
            if (p.Count < 3) return new();
            Polygon scaled = new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 w = p[i] - pivot;
                scaled.Add(pivot + w * scale);
            }
            return scaled;
        }
        public static Polygon ScaleUniform(this Polygon p, float distance)
        {
            Polygon shape = new();
            for (int i = 0; i < p.Count; i++)
            {
                shape.Add(SVec.ScaleUniform(p[i], distance));
            }
            return shape;
        }
        
        public static void CenterSelf(this Polygon p, Vector2 newCenter)
        {
            var centroid = p.GetCentroid();
            var delta = newCenter - centroid;
            MoveSelf(p, delta);
        }
        public static void MoveSelf(this Polygon p, Vector2 translation)
        {
            for (int i = 0; i < p.Count; i++)
            {
                p[i] += translation;
            }
            //return path;
        }
        public static void RotateSelf(this Polygon p, Vector2 pivot, float rotRad)
        {
            if (p.Count < 3) return;// new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 w = p[i] - pivot;
                p[i] = pivot + w.Rotate(rotRad);
            }
            //return path;
        }
        public static void RotateSelf(this Polygon p, float rotRad)
        {
            if (p.Count < 3) return;// new();
            for (int i = 0; i < p.Count; i++)
            {
                p[i] = p[i].Rotate(rotRad);
            }
            //return path;
        }
        public static void ScaleSelf(this Polygon p, float scale)
        {
            for (int i = 0; i < p.Count; i++)
            {
                p[i] *= scale;
            }
            //return path;
        }
        public static void ScaleSelf(this Polygon p, Vector2 pivot, float scale)
        {
            if (p.Count < 3) return;// new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 w = p[i] - pivot;
                p[i] = pivot + w * scale;
            }
            //return path;
        }
        public static void ScaleSelf(this Polygon p, Vector2 pivot, Vector2 scale)
        {
            if (p.Count < 3) return;// new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 w = p[i] - pivot;
                p[i] = pivot + w * scale;
            }
            //return path;
        }
        public static void ScaleUniformSelf(this Polygon p, float distance)
        {
            for (int i = 0; i < p.Count; i++)
            {
                p[i] = SVec.ScaleUniform(p[i], distance);
            }
            //return path;
        }

        public static Polygon RemoveColinearVertices(this Polygon p)
        {
            Polygon result = new();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 cur = p[i];
                Vector2 prev = SUtils.GetItem(p, i - 1);
                Vector2 next = SUtils.GetItem(p, i + 1);

                Vector2 prevCur = prev - cur;
                Vector2 nextCur = next - cur;
                if (prevCur.Cross(nextCur) != 0f) result.Add(cur);
            }
            return result;
        }
        public static Polygon RemoveDuplicates(this Polygon p, float toleranceSquared = 0.001f) 
        {
            Polygon result = new();

            for (int i = 0; i < p.Count; i++)
            {
                Vector2 cur = p[i];
                Vector2 next = SUtils.GetItem(p, i + 1);
                if((cur - next).LengthSquared() > toleranceSquared) result.Add(cur);
            }

            return result;


            //for (int i = path.Count - 1; i >= 0; i--)
            //{
            //    Vector2 cur = path[i];
            //    int nextIndex = i - 1;
            //    if (nextIndex < 0) nextIndex = path.Count - 1;
            //    Vector2 next = path[nextIndex];
            //    if ((cur - next).LengthSquared() < toleranceSquared) path.RemoveAt(i);
            //}
        }
        public static Polygon Smooth(this Polygon p, float amount, float baseWeight)
        {
            if (p.Count < 3) return new();
            Polygon smoothed = new();
            Vector2 centroid = p.GetCentroid();
            for (int i = 0; i < p.Count; i++)
            {
                Vector2 cur = p[i];
                Vector2 prev = p[SUtils.WrapIndex(p.Count, i - 1)];
                Vector2 next = p[SUtils.WrapIndex(p.Count, i + 1)];
                Vector2 dir = (prev - cur) + (next - cur) + ((cur - centroid) * baseWeight);
                smoothed.Add(cur + dir * amount);
            }
            return smoothed;
        }

        public static List<Vector2> GetSegmentAxis(this Polygon p, bool normalized = false)
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
                axis.Add(normalized ? SVec.Normalize(a) : a);
            }
            return axis;
        }
        public static List<Vector2> GetSegmentAxis(Segments edges, bool normalized = false)
        {
            List<Vector2> axis = new();
            foreach (var seg in edges)
            {
                axis.Add(normalized ? seg.Dir : seg.Displacement);
            }
            return axis;
        }
        
        public static Polygon Generate(Vector2 center, int pointCount, float minLength, float maxLength)
        {
            Polygon points = new();
            float angleStep = RayMath.PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = SRNG.randF(minLength, maxLength);
                Vector2 p = SVec.Rotate(SVec.Right(), -angleStep * i) * randLength;
                p += center;
                points.Add(p);
            }
            return points;
        }
        public static Polygon Generate(int pointCount, float minLength, float maxLength)
        {
            Polygon points = new();
            float angleStep = RayMath.PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = SRNG.randF(minLength, maxLength);
                Vector2 p = SVec.Rotate(SVec.Right(), -angleStep * i) * randLength;
                points.Add(p);
            }
            return points;
        }

        /*public static Polygon Generate(Segment segment, float magMin, float magMax, int pointCount)
        {
            List<float> factors = new();
            for (int i = 0; i < pointCount; i++) { factors.Add(SRNG.randF()); }
            factors.Sort();
            
            List<Vector2> points = new();
            var dir = segment.Dir;
            var l = segment.Length;
            foreach (var f in factors)
            {
                points.Add(dir * f * l);
            }

            Polygon poly = new() { segment.start };
            var rightDir = dir.GetPerpendicularRight();
            foreach (var p in points)
            {
                poly.Add(p + rightDir * SRNG.randF(magMin, magMax));
            }
            poly.Add(segment.end);
            var leftDir = dir.GetPerpendicularLeft();
            points.Reverse();
            foreach (var p in points)
            {
                poly.Add(p + leftDir * SRNG.randF(magMin, magMax));
            }
            return poly;
        }*/
        
        /// <summary>
        /// Generates a polygon around the given segment. Points are generated ccw around the segment beginning with the segment start.
        /// </summary>
        /// <param name="segment">The segment to build a polygon around.</param>
        /// <param name="magMin">The minimum perpendicular magnitude factor for generating a point. (0-1)</param>
        /// <param name="magMax">The maximum perpendicular magnitude factor for generating a point. (0-1)</param>
        /// <param name="minSectionLength">The minimum factor of the length between points along the line.(0-1)</param>
        /// <param name="maxSectionLength">The maximum factor of the length between points along the line.(0-1)</param>
        /// <returns>Returns the a generated polygon.</returns>
        public static Polygon Generate(Segment segment, float magMin = 0.1f, float magMax = 0.25f, float minSectionLength = 0.025f, float maxSectionLength = 0.1f)
        {
            Polygon poly = new() { segment.Start };
            var dir = segment.Dir;
            var dirRight = dir.GetPerpendicularRight();
            var dirLeft = dir.GetPerpendicularLeft();
            float len = segment.Length;
            float minSectionLengthSq = (minSectionLength * len) * (minSectionLength * len);
            Vector2 cur = segment.Start;
            while(true)
            {
                cur += dir * SRNG.randF(minSectionLength, maxSectionLength) * len;
                if ((cur - segment.End).LengthSquared() < minSectionLengthSq) break;
                poly.Add(cur + dirRight * SRNG.randF(magMin, magMax));
            }
            cur = segment.End;
            poly.Add(cur);
            while (true)
            {
                cur -= dir * SRNG.randF(minSectionLength, maxSectionLength) * len;
                if ((cur - segment.Start).LengthSquared() < minSectionLengthSq) break;
                poly.Add(cur + dirLeft * SRNG.randF(magMin, magMax));
            }
            return poly;
        }
        /// <summary>
        /// Computes the length of this polygon's apothem. This will only be valid if
        /// the polygon is regular. More info: http://en.wikipedia.org/wiki/Apothem
        /// </summary>
        /// <param name="p"></param>
        /// <returns>Return the length of the apothem.</returns>
        public static float GetApothem(this Polygon p)
        {
            return (p.GetCentroid() - (p[0].Lerp(p[1], 0.5f))).Length();
        }

        
        public static Vector2 GetRandomPointConvex(this Polygon p)
        {
            //only work with convex polygons
            var edges = p.GetEdges();
            var ea = SRNG.randCollection(edges, true);
            var eb = SRNG.randCollection(edges);

            var pa = ea.Start.Lerp(ea.End, SRNG.randF());
            var pb = eb.Start.Lerp(eb.End, SRNG.randF());
            return pa.Lerp(pb, SRNG.randF());
        }
    
    }
}
//public static float GetAreaSigned(this Polygon p)
        //{
        //    float area = 0f;
        //
        //    for (int i = 0; i < p.Count; i++)
        //    {
        //        Vector2 a = p[i];
        //        Vector2 b = p[(i + 1) % p.Count];
        //        area += a.X * b.Y;
        //        area -= a.Y * b.X;
        //    }
        //
        //    return area * 0.5f;
        //}

/*
        public static List<Vector2> GetShape(List<Vector2> points, Vector2 pos, float rotRad, Vector2 scale)
        {
            List<Vector2> poly = new();
            for (int i = 0; i < points.Count; i++)
            {
                poly.Add(pos + SVec.Rotate(points[i], rotRad) * scale);
            }
            return poly;
        }
        public static Rect GetBoundingBox(List<Vector2> poly)
        {
            if (poly.Count < 2) return new();
            Vector2 start = poly[0];
            Rect r = new(start.X, start.Y, 0, 0);

            foreach (var p in poly)
            {
                r = SRect.EnlargeRect(r, p);
            }
            return r;
        }

        public static List<Vector2> Scale(List<Vector2> poly, float scale)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < poly.Count; i++)
            {
                points.Add(poly[i] * scale);
            }
            return points;
        }

        public static List<Vector2> ScaleUniform(List<Vector2> poly, float distance)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < poly.Count; i++)
            {
                points.Add(SVec.ScaleUniform(poly[i], distance));
            }
            return points;
        }

        public static List<Vector2> GeneratePoints(int pointCount, Vector2 center, float minLength, float maxLength)
        {
            List<Vector2> points = new();
            float angleStep = RayMath.PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = SRNG.randF(minLength, maxLength);
                Vector2 p = SVec.Rotate(SVec.Right(), angleStep * i) * randLength;
                p += center;
                points.Add(p);
            }
            return points;
        }

        public static List<(Vector2 start, Vector2 end)> GetSegments(List<Vector2> poly)
        {
            if (poly.Count <= 1) return new();
            else if (poly.Count == 2)
            {
                return new() { (poly[0], poly[1]) };
            }
            List<(Vector2 start, Vector2 end)> segments = new();
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i+1) % poly.Count];
                segments.Add((start, end));
            }
            return segments;
        }
    
        public static List<Vector2> GetSegmentAxis(List<Vector2> poly, bool normalized = false)
        {
            if (poly.Count <= 1) return new();
            else if(poly.Count == 2)
            {
                return new() { poly[1] - poly[0] };
            }
            List<Vector2> axis = new();
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                Vector2 a = end - start;
                axis.Add(normalized ? SVec.Normalize(a) : a);
            }
            return axis;
        }
        */
//public static float GetArea(List<Vector2> points, Vector2 center)
        //{
        //    if (points.Count < 3) return 0f;
        //    var triangles = Triangulate(points, center);
        //    float totalArea = 0f;
        //    foreach (var t in triangles)
        //    {
        //        totalArea += t.GetArea();
        //    }
        //    return totalArea;
        //}
        //public static List<Triangle> Triangulate(List<Vector2> points, Vector2 center)
        //{
        //    if (points.Count < 3) return new();
        //    List<Triangle> triangles = new();
        //    points.Add(points[0]);
        //    for (int i = 0; i < points.Count - 1; i++)
        //    {
        //        Vector2 a = points[i];
        //        Vector2 b = center;
        //        Vector2 c = points[i + 1];
        //        triangles.Add(new(a, b, c));
        //    }
        //    return triangles;
        //}

        //public static List<Triangle> Triangulate(this Polygon path)
        //{
        //    if (path.Count < 3) return new();
        //    else if (path.Count == 3) return new() { new(path[0], path[1], path[2]) };
        //    
        //    List<Triangle> triangles = new();
        //    List<Vector2> vertices = new();
        //    vertices.AddRange(path);
        //    while(vertices.Count > 3)
        //    {
        //        for (int i = 0; i < vertices.Count; i++)
        //        {
        //            Vector2 a = vertices[i];
        //            Vector2 b = SUtils.GetItem(vertices, i + 1);
        //            Vector2 c = SUtils.GetItem(vertices, i - 1);
        //
        //            Vector2 ba = b - a;
        //            Vector2 ca = c - a;
        //
        //            if (ba.Cross(ca) < 0f) continue;
        //
        //            Triangle t = new(a, b, c);
        //
        //            bool isValid = true;
        //            foreach (var p in path)
        //            {
        //                if (p == a || p == b || p == c) continue;
        //                if (t.IsPointInside(p))
        //                {
        //                    isValid = false;
        //                    break;
        //                }
        //            }
        //
        //            if (isValid)
        //            {
        //                triangles.Add(t);
        //                vertices.RemoveAt(i);
        //                break;
        //            }
        //        }
        //    }
        //    triangles.Add(new(vertices[0], vertices[1], vertices[2]));
        //
        //
        //    return triangles;
        //}

        /*
        public static Polygon GetShape(this Polygon p, Vector2 pos, float rotRad, Vector2 scale) { return new(GetShape(p.points, pos, rotRad, scale), pos); }
        public static Vector2 GetCentroid(this Polygon p) { return GetCentroid(p.points); }
        public static Polygon Move(this Polygon p, Vector2 translation) { return new(Move(p.points, translation), p.center + translation); }
        public static Polygon Rotate(this Polygon p, Vector2 pivot, float rotRad) { return new(Rotate(p.points, pivot, rotRad), p.center); }
        public static Polygon Scale(this Polygon p, float scale) { return new(Scale(p.points, scale), p.center); }
        public static Polygon Scale(this Polygon p, Vector2 pivot, float scale) { return new(Scale(p.points, pivot, scale)); }
        public static Polygon ScaleUniform(this Polygon p, float distance) { return new(ScaleUniform(p.points, distance), p.center); }
        public static List<Vector2> GetSegmentAxis(this Polygon p, bool normalized = false) { return GetSegmentAxis(p.points, normalized); }
        public static List<Triangle> Triangulate(this Polygon p) { return Triangulate(p.points); }
        
        public static Polygon GeneratePointsPolygon(Vector2 center, int pointCount, float minLength, float maxLength) { return new Polygon(GeneratePoints(center, pointCount, minLength, maxLength), center); }
        public static Polygon GeneratePointsPolygon(int pointCount, float minLength, float maxLength) { return new(GeneratePoints(pointCount, minLength, maxLength), new Vector2(0f)); }
        */


        //public static Rect GetBoundingBox(this Polygon p) { return GetBoundingBox(p.points); }
        //public static List<Segment> GetSegments(this Polygon p) { return GetEdges(p.points); }
        //public static SegmentShape GetSegmentsShape(this Polygon p) { return new(GetSegments(p.points), p.center); }
        //public static float GetArea(this Polygon p) { return GetArea(p.points); }
        //public static float GetCircumference(this Polygon p) { return GetCircumference(p.points); }
        //public static float GetCircumferenceSquared(this Polygon p) { return GetCircumferenceSquared(p.points); }
/*
        public static List<Vector2> GetShape(this Polygon p)
        {
            return GetShape(p.points, p.pos, p.rotRad, p.scale);
        }
        public static Rect GetBoundingBox(this Polygon p)
        {
            return GetBoundingBox(p.GetShape());
        }
        public static List<Line> GetSegments(this Polygon p)
        {
            return GetSegments(p.GetShape());
        }
        public static List<Vector2> GetSegmentAxis(this Polygon p, bool normalized = false)
        {
            return GetSegmentAxis(p.GetShape(), normalized);
        }
        public static List<Triangle> Triangulate(this Polygon p) { return Triangulate(p.GetShape(), p.pos); }
        public static float GetArea(this Polygon p) { return GetArea(p.GetShape(), p.pos); }
        public static float GetCircumference(this Polygon p) { return GetCircumference(p.GetShape()); }
        public static float GetCircumferenceSquared(this Polygon p) { return GetCircumferenceSquared(p.GetShape()); }
        */