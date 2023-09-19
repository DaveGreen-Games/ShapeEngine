﻿
using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core
{
    /// <summary>
    /// Points shoud be in CCW order.
    /// </summary>
    public class Polygon : Points, IShape, IEquatable<Polygon>
    {
        #region Constructors
        public Polygon() { }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="points"></param>
        public Polygon(params Vector2[] points) { AddRange(points); }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="points"></param>
        public Polygon(IEnumerable<Vector2> points) { AddRange(points); }
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="points"></param>
        public Polygon(IShape shape) { AddRange(shape.ToPolygon()); }
        public Polygon(Polygon poly) { AddRange(poly); }
        public Polygon(Polyline polyLine) { AddRange(polyLine); }
        #endregion

        #region Equals & Hashcode
        public bool Equals(Polygon? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] != other[i]) return false;
            }
            return true;
        }
        public override int GetHashCode() { return SUtils.GetHashCode(this); }
        #endregion

        #region Getter Setter
        public bool FlippedNormals { get; set; } = false;
        #endregion

        #region Public
        public bool ContainsShape(Segment other)
        {
            return ContainsPoint(other.Start) && ContainsPoint(other.End);
        }
        public bool ContainsShape(Circle other)
        {
            var points = other.GetVertices(8);
            return ContainsShape(points);
        }
        public bool ContainsShape(Rect other)
        {
            return ContainsPoint(other.TopLeft) &&
                ContainsPoint(other.BottomLeft) &&
                ContainsPoint(other.BottomRight) &&
                ContainsPoint(other.TopRight);
        }
        public bool ContainsShape(Triangle other)
        {
            return ContainsPoint(other.A) &&
                ContainsPoint(other.B) &&
                ContainsPoint(other.C);
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

        public void FixWindingOrder() { if (this.IsClockwise()) this.Reverse(); }
        public void ReduceVertexCount(int newCount)
        {
            if (newCount < 3) Clear();//no points left to form a polygon

            while (Count > newCount)
            {
                float minD = 0f;
                int shortestID = 0;
                for (int i = 0; i < Count; i++)
                {
                    float d = (this[i] - this[(i + 1) % Count]).LengthSquared();
                    if (d > minD)
                    {
                        minD = d;
                        shortestID = i;
                    }
                }
                RemoveAt(shortestID);
            }

        }
        public void ReduceVertexCount(float factor) { ReduceVertexCount(Count - (int)Count * factor); }
        public void IncreaseVertexCount(int newCount)
        {
            if (newCount <= Count) return;

            while (Count < newCount)
            {
                float maxD = 0f;
                int longestID = 0;
                for (int i = 0; i < Count; i++)
                {
                    float d = (this[i] - this[(i + 1) % Count]).LengthSquared();
                    if (d > maxD)
                    {
                        maxD = d;
                        longestID = i;
                    }
                }
                Vector2 m = (this[longestID] + this[(longestID + 1) % Count]) * 0.5f;
                this.Insert(longestID + 1, m);
            }
        }
        public Vector2 GetVertex(int index)
        {
            return this[SUtils.WrapIndex(Count, index)];
        }

        //public void Floor() { Points.Floor(this); }
        //public void Ceiling() { Points.Ceiling(this); }
        //public void Truncate() { Points.Truncate(this); }
        //public void Round() { Points.Round(this); }

        /// <summary>
        /// Computes the length of this polygon's apothem. This will only be valid if
        /// the polygon is regular. More info: http://en.wikipedia.org/wiki/Apothem
        /// </summary>
        /// <param name="p"></param>
        /// <returns>Return the length of the apothem.</returns>
        public float GetApothem()
        {
            return (this.GetCentroid() - (this[0].Lerp(this[1], 0.5f))).Length();
        }
        public Vector2 GetRandomPointConvex()
        {
            var edges = GetEdges();
            var ea = SRNG.randCollection(edges, true);
            var eb = SRNG.randCollection(edges);

            var pa = ea.Start.Lerp(ea.End, SRNG.randF());
            var pb = eb.Start.Lerp(eb.End, SRNG.randF());
            return pa.Lerp(pb, SRNG.randF());
        }

        public void Center(Vector2 newCenter)
        {
            var centroid = GetCentroid();
            var delta = newCenter - centroid;
            Move(delta);
        }
        public void Move(Vector2 translation)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] += translation;
            }
            //return path;
        }
        public void Rotate(Vector2 pivot, float rotRad)
        {
            if (Count < 3) return;
            for (int i = 0; i < Count; i++)
            {
                Vector2 w = this[i] - pivot;
                this[i] = pivot + w.Rotate(rotRad);
            }
            //return path;
        }
        public void Rotate(float rotRad)
        {
            if (Count < 3) return;// new();
            for (int i = 0; i < Count; i++)
            {
                this[i] = this[i].Rotate(rotRad);
            }
            //return path;
        }
        public void Scale(float scale)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] *= scale;
            }
            //return path;
        }
        public void Scale(Vector2 pivot, float scale)
        {
            if (Count < 3) return;
            for (int i = 0; i < Count; i++)
            {
                Vector2 w = this[i] - pivot;
                this[i] = pivot + w * scale;
            }
        }
        public void Scale(Vector2 pivot, Vector2 scale)
        {
            if (Count < 3) return;// new();
            for (int i = 0; i < Count; i++)
            {
                Vector2 w = this[i] - pivot;
                this[i] = pivot + w * scale;
            }
            //return path;
        }
        public void ScaleUniform(float distance)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] = SVec.ScaleUniform(this[i], distance);
            }
        }

        public void RemoveColinearVertices()
        {
            if (Count < 3) return;
            Points result = new();
            for (int i = 0; i < Count; i++)
            {
                Vector2 cur = this[i];
                Vector2 prev = SUtils.GetItem(this, i - 1);
                Vector2 next = SUtils.GetItem(this, i + 1);

                Vector2 prevCur = prev - cur;
                Vector2 nextCur = next - cur;
                if (prevCur.Cross(nextCur) != 0f) result.Add(cur);
            }
            Clear();
            AddRange(result);
        }
        public void RemoveDuplicates(float toleranceSquared = 0.001f)
        {
            if (Count < 3) return;
            Points result = new();

            for (int i = 0; i < Count; i++)
            {
                Vector2 cur = this[i];
                Vector2 next = SUtils.GetItem(this, i + 1);
                if ((cur - next).LengthSquared() > toleranceSquared) result.Add(cur);
            }
            Clear();
            AddRange(result);
        }
        public void Smooth(float amount, float baseWeight)
        {
            if (Count < 3) return;
            Points result = new();
            Vector2 centroid = GetCentroid();
            for (int i = 0; i < Count; i++)
            {
                Vector2 cur = this[i];
                Vector2 prev = this[SUtils.WrapIndex(Count, i - 1)];
                Vector2 next = this[SUtils.WrapIndex(Count, i + 1)];
                Vector2 dir = (prev - cur) + (next - cur) + ((cur - centroid) * baseWeight);
                result.Add(cur + dir * amount);
            }

            Clear();
            AddRange(result);
        }

        public (Polygons newShapes, Polygons cutOuts) Cut(Polygon cutShape)
        {
            var cutOuts = SClipper.Intersect(this, cutShape).ToPolygons(true);
            var newShapes = SClipper.Difference(this, cutShape).ToPolygons(true);

            return (newShapes, cutOuts);
        }
        public (Polygons newShapes, Polygons cutOuts) CutMany(Polygons cutShapes)
        {
            var cutOuts = SClipper.IntersectMany(this, cutShapes).ToPolygons(true);
            var newShapes = SClipper.DifferenceMany(this, cutShapes).ToPolygons(true);
            return (newShapes, cutOuts);
        }
        public (Polygons newShapes, Polygons overlaps) Combine(Polygon other)
        {
            var overlaps = SClipper.Intersect(this, other).ToPolygons(true);
            var newShapes = SClipper.Union(this, other).ToPolygons(true);
            return (newShapes, overlaps);
        }
        public (Polygons newShapes, Polygons overlaps) Combine(Polygons others)
        {
            var overlaps = SClipper.IntersectMany(this, others).ToPolygons(true);
            var newShapes = SClipper.UnionMany(this, others).ToPolygons(true);
            return (newShapes, overlaps);
        }
        public (Polygons newShapes, Polygons cutOuts) CutSimple(Vector2 cutPos, float minCutRadius, float maxCutRadius, int pointCount = 16)
        {
            var cut = Generate(cutPos, pointCount, minCutRadius, maxCutRadius);
            return this.Cut(cut);
        }
        public (Polygons newShapes, Polygons cutOuts) CutSimple(Segment cutLine, float minSectionLength = 0.025f, float maxSectionLength = 0.1f, float minMagnitude = 0.05f, float maxMagnitude = 0.25f)
        {
            var cut = Generate(cutLine, minMagnitude, maxMagnitude, minSectionLength, maxSectionLength);
            return this.Cut(cut);
        }

        #endregion

        #region Static
        public static bool IsPointInPoly(Vector2 point, Polygon poly)
        {
            
            bool oddNodes = false;
            int num = poly.Count;
            int j = num - 1;
            for (int i = 0; i < num; i++)
            {
                var vi = poly[i];
                var vj = poly[j];
                if (vi.Y < point.Y && vj.Y >= point.Y || vj.Y < point.Y && vi.Y >= point.Y)
                {
                    if (vi.X + (point.Y - vi.Y) / (vj.Y - vi.Y) * (vj.X - vi.X) < point.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
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
                    float distSq = Vector2.DistanceSquared(p, circumCircle.Center);
                    if (distSq < circumCircle.Radius * circumCircle.Radius)
                    {
                        badTriangles.Add(triangle);
                        triangles.RemoveAt(triIndex);
                    }
                }

                Segments allEdges = new();
                foreach (var badTriangle in badTriangles) { allEdges.AddRange(badTriangle.GetEdges()); }

                Segments uniqueEdges = GetUniqueSegmentsDelauney(allEdges);
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
        private static Segments GetUniqueSegmentsDelauney(Segments segments)
        {
            Segments uniqueEdges = new();
            for (int i = segments.Count - 1; i >= 0; i--)
            {
                var edge = segments[i];
                if (IsSimilar(segments, edge))
                {
                    uniqueEdges.Add(edge);
                }
            }
            return uniqueEdges;
        }
        private static bool IsSimilar(Segments segments, Segment seg)
        {
            int counter = 0;
            foreach (var segment in segments)
            {
                if (segment.IsSimilar(seg)) counter++;
                if (counter > 1) return false;
            }
            return true;
        }



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
        
        
        public static List<Vector2> GetSegmentAxis(Polygon p, bool normalized = false)
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

        public static Polygon GetShape(Points relative, Vector2 pos, float rotRad, Vector2 scale)
        {
            if (relative.Count < 3) return new();
            Polygon shape = new();
            for (int i = 0; i < relative.Count; i++)
            {
                shape.Add(pos + SVec.Rotate(relative[i], rotRad) * scale);
            }
            return shape;
        }
        public static Points GenerateRelative(int pointCount, float minLength, float maxLength)
        {
            Points points = new();
            float angleStep = RayMath.PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = SRNG.randF(minLength, maxLength);
                Vector2 p = SVec.Rotate(SVec.Right(), -angleStep * i) * randLength;
                points.Add(p);
            }
            return points;
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
            while (true)
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

        public static Polygon Center(Polygon p, Vector2 newCenter)
        {
            var centroid = p.GetCentroid();
            var delta = newCenter - centroid;
            return Move(p, delta);
        }
        public static Polygon Move(Polygon p, Vector2 translation)
        {
            Polygon result = new();
            for (int i = 0; i < p.Count; i++)
            {
                result.Add(p[i] + translation);
            }
            return result;
        }
        public static Polygon Rotate(Polygon p, Vector2 pivot, float rotRad)
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
        public static Polygon Scale(Polygon p, float scale)
        {
            Polygon shape = new();
            for (int i = 0; i < p.Count; i++)
            {
                shape.Add(p[i] * scale);
            }
            return shape;
        }
        public static Polygon Scale(Polygon p, Vector2 pivot, float scale)
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
        public static Polygon Scale(Polygon p, Vector2 pivot, Vector2 scale)
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
        public static Polygon ScaleUniform(Polygon p, float distance)
        {
            Polygon shape = new();
            for (int i = 0; i < p.Count; i++)
            {
                shape.Add(SVec.ScaleUniform(p[i], distance));
            }
            return shape;
        }
        #endregion

        #region IShape
        public Vector2 GetCentroid()
        {
            //return GetCentroidMean();
            Vector2 result = new();
            
            for (int i = 0; i < Count; i++)
            {
                Vector2 a = this[i];
                Vector2 b = this[(i + 1) % Count];
                //float factor = a.X * b.Y - b.X * a.Y; //clockwise 
                float factor = a.Y * b.X - a.X * b.Y; //counter clockwise
                result.X += (a.X + b.X) * factor;
                result.Y += (a.Y + b.Y) * factor;
            }
            
            return result * (1f / (GetArea() * 6f));
        }
        public Vector2 GetCentroidMean()
        {
            if (Count <= 0) return new(0f);
            Vector2 total = new(0f);
            foreach (Vector2 p in this) { total += p; }
            return total / Count;
        }
        public Triangulation Triangulate()
        {
            if (Count < 3) return new();
            else if (Count == 3) return new() { new(this[0], this[1], this[2]) };

            Triangulation triangles = new();
            List<Vector2> vertices = new();
            vertices.AddRange(this);
            List<int> validIndices = new();
            for (int i = 0; i < vertices.Count; i++)
            {
                validIndices.Add(i);
            }
            while (vertices.Count > 3)
            {
                if (validIndices.Count <= 0) 
                    break;

                int i = validIndices[SRNG.randI(0, validIndices.Count)];
                Vector2 a = vertices[i];
                Vector2 b = SUtils.GetItem(vertices, i + 1);
                Vector2 c = SUtils.GetItem(vertices, i - 1);

                Vector2 ba = b - a;
                Vector2 ca = c - a;
                float cross = ba.Cross(ca);
                if (cross >= 0f)//makes sure that ear is not self intersecting
                {
                    validIndices.Remove(i);
                    continue;
                }

                Triangle t = new(a, b, c);

                bool isValid = true;
                foreach (var p in this)
                {
                    if (p == a || p == b || p == c) continue;
                    if (t.ContainsPoint(p))
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    triangles.Add(t);
                    vertices.RemoveAt(i);

                    validIndices.Clear();
                    for (int j = 0; j < vertices.Count; j++)
                    {
                        validIndices.Add(j);
                    }
                    //break;
                }
            }


            triangles.Add(new(vertices[0], vertices[1], vertices[2]));


            return triangles;
        }

        //private Triangulation Subdivide(Triangle triangle, float minArea)
        //{
        //    var area = triangle.GetArea();
        //    Triangulation final = new();
        //    if (minArea > area / 3)
        //    {
        //        final.Add(triangle);
        //    }
        //    else
        //    {
        //        var triangulation = triangle.Triangulate();
        //        foreach (var tri in triangulation)
        //        {
        //            final.AddRange(Subdivide(tri, minArea));
        //        }
        //    }
        //    return final;
        //}
        
        ///// <summary>
        ///// Triangulate this polygon. 
        ///// </summary>
        ///// <param name="minArea">The minimum area a triangle must have to be further subdivided. Does not affect the initial triangulation.</param>
        ///// <param name="subdivisions">A subdivision triangulates all triangles from the previous triangulation. (Do not go big!) </param>
        ///// <returns></returns>
        //public Triangulation Fracture(float minArea = -1, int subdivisions = 0)
        //{
        //    var triangulation = Triangulate();
        //    if (subdivisions <= 0) return triangulation;
        //    else
        //    {
        //        return Subdivide(triangulation, subdivisions, minArea);
        //    }
        //}
        //private Triangulation Subdivide(Triangulation triangles, int remaining, float minArea = -1)
        //{
        //    if(remaining <= 0) return triangles;
        //    Triangulation subdivision = new();
        //    foreach (var tri in triangles)
        //    {
        //        var area = tri.GetArea();
        //        //tri.GetRandomPoint()
        //        if(minArea <= 0 || tri.GetArea() >= minArea) subdivision.AddRange(tri.Triangulate());
        //        else subdivision.Add(tri);
        //
        //    }
        //    return Subdivide(subdivision, remaining - 1, minArea);
        //}
        /*
        //only works with simple polygons that is why the ear clipper algorithm is used.
        public Triangulation Fracture(int fractureComplexity = 0)//fix delauny triangulation
        {
            if (fractureComplexity <= 0) return SPoly.TriangulateDelaunay(this);

            List<Vector2> points = new();
            points.AddRange(this);
            points.AddRange(GetRandomPoints(fractureComplexity));
            return SPoly.TriangulateDelaunay(points);
        }*/

        /// <summary>
        /// Return the segments of the polygon. If the points are in ccw winding order the normals face outward when InsideNormals = false 
        /// and face inside otherwise.
        /// </summary>
        /// <returns></returns>
        public Segments GetEdges()
        {
            if (Count <= 1) return new();
            else if (Count == 2)
            {
                Vector2 A = this[0];
                Vector2 B = this[1];

                return new() { new(A, B, FlippedNormals) };
            }
            Segments segments = new();
            for (int i = 0; i < Count; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
                segments.Add(new(start, end, FlippedNormals));
            }
            return segments;
        }
        public Circle GetBoundingCircle()
        {
            float maxD = 0f;
            int num = this.Count;
            Vector2 origin = new();
            for (int i = 0; i < num; i++) { origin += this[i]; }
            origin = origin / num;
            //origin *= (1f / (float)num);
            for (int i = 0; i < num; i++)
            {
                float d = (origin - this[i]).LengthSquared();
                if (d > maxD) maxD = d;
            }

            return new Circle(origin, MathF.Sqrt(maxD));
        }
        public Rect GetBoundingBox()
        {
            if (Count < 2) return new();
            Vector2 start = this[0];
            Rect r = new(start.X, start.Y, 0, 0);

            foreach (var p in this)
            {
                r = SRect.Enlarge(r, p);
            }
            return r;
        }
        public Triangle GetBoundingTriangle(float margin = 3f) { return Polygon.GetBoundingTriangle(this, margin); }
        public float GetCircumference() { return MathF.Sqrt(GetCircumferenceSquared()); }
        public float GetCircumferenceSquared()
        {
            if (this.Count < 3) return 0f;
            float lengthSq = 0f;
            for (int i = 0; i < Count; i++)
            {
                Vector2 w = this[(i + 1)%Count] - this[i];
                lengthSq += w.LengthSquared();
            }
            return lengthSq;
        }
        public float GetArea() { return MathF.Abs(GetAreaSigned()); }
        public bool IsClockwise() { return GetAreaSigned() > 0f; }
        public bool IsConvex()
        {
            int num = this.Count;
            bool isPositive = false;

            for (int i = 0; i < num; i++)
            {
                int prevIndex = (i == 0) ? num - 1 : i - 1;
                int nextIndex = (i == num - 1) ? 0 : i + 1;
                var d0 = this[i] - this[prevIndex];
                var d1 = this[nextIndex] - this[i];
                var newIsP = d0.Cross(d1) > 0f;
                if (i == 0) isPositive = true;
                else if (isPositive != newIsP) return false;
            }
            return true;
        }

        public Points GetVertices() { return new(this); }
        //public Polygon ToPolygon() { return new( this ); }
        //public Polyline ToPolyline() { return new(this); }


        public int GetClosestIndex(Vector2 p)
        {
            //if (Count <= 0) return -1;
            //if (Count == 1) return 0;
            //
            //float minD = float.PositiveInfinity;
            //var edges = GetEdges();
            //int closestIndex = -1;
            //for (int i = 0; i < edges.Count; i++)
            //{
            //    Vector2 c = edges[i].GetClosestPoint(p).Point;
            //    float d = (c - p).LengthSquared();
            //    if (d < minD)
            //    {
            //        closestIndex = i;
            //        minD = d;
            //    }
            //}
            //return closestIndex;

            if (Count <= 0) return -1;
            if (Count == 1) return 0;

            float minD = float.PositiveInfinity;
            int closestIndex = -1;

            for (int i = 0; i < Count; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
                Segment edge = new Segment(start, end);

                Vector2 closest = edge.GetClosestPoint(p).Point;
                float d = (closest - p).LengthSquared();
                if (d < minD)
                {
                    closestIndex = i;
                    minD = d;
                }
            }
            return closestIndex;
        }
        public CollisionPoint GetClosestPoint(Vector2 p)
        {
            float minD = float.PositiveInfinity;
            var edges = GetEdges();
            CollisionPoint closest = new();

            for (int i = 0; i < edges.Count; i++)
            {
                CollisionPoint c = edges[i].GetClosestPoint(p);
                float d = (c.Point - p).LengthSquared();
                if (d < minD)
                {
                    closest = c;
                    minD = d;
                }
            }
            return closest;
        }
        public Vector2 GetClosestVertex(Vector2 p)
        {
            //float minD = float.PositiveInfinity;
            //Vector2 closest = new();
            //for (int i = 0; i < Count; i++)
            //{
            //    float d = (this[i] - p).LengthSquared();
            //    if (d < minD)
            //    {
            //        closest = this[i];
            //        minD = d;
            //    }
            //}
            //return closest;

            if (Count < 2) return new();
            float minD = float.PositiveInfinity;
            Vector2 closestPoint = new();

            for (int i = 0; i < Count; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
                Segment edge = new Segment(start, end);

                Vector2 closest = edge.GetClosestPoint(p).Point;
                float d = (closest - p).LengthSquared();
                if (d < minD)
                {
                    closestPoint = closest;
                    minD = d;
                }
            }
            return closestPoint;
        }
        public Segment GetClosestSegment(Vector2 p)
        {
            if (Count < 2) return new();
            float minD = float.PositiveInfinity;
            Segment closestSegment = new();

            for (int i = 0; i < Count; i++)
            {
                Vector2 start = this[i];
                Vector2 end = this[(i + 1) % Count];
                Segment edge = new Segment(start, end);
                
                Vector2 closest = edge.GetClosestPoint(p).Point;
                float d = (closest - p).LengthSquared();
                if (d < minD)
                {
                    closestSegment = edge;
                    minD = d;
                }
            }
            return closestSegment;
        }

        public bool ContainsPoint(Vector2 p) { return IsPointInPoly(p, this); }
        public Vector2 GetRandomPoint()
        {
            var triangles = Triangulate();
            List<WeightedItem<Triangle>> items = new();
            foreach (var t in triangles)
            {
                items.Add(new(t, (int)t.GetArea()));
            }
            var item = SRNG.PickRandomItem(items.ToArray());
            return item.GetRandomPoint();
        }
        public Points GetRandomPoints(int amount)
        {
            var triangles = Triangulate();
            WeightedItem<Triangle>[] items = new WeightedItem<Triangle>[triangles.Count];
            for (int i = 0; i < items.Length; i++)
            {
                var t = triangles[i];
                items[i] = new(t, (int)t.GetArea());
            }


            List<Triangle> pickedTriangles = SRNG.PickRandomItems(amount, items);
            Points randomPoints = new();
            foreach (var tri in pickedTriangles) randomPoints.Add(tri.GetRandomPoint());

            return randomPoints;
        }
        public Vector2 GetRandomVertex() { return SRNG.randCollection(this, false); }
        public Segment GetRandomEdge()
        {
            var edges = GetEdges();
            List<WeightedItem<Segment>> items = new(edges.Count);
            foreach (var edge in edges)
            {
                items.Add(new(edge, (int)edge.LengthSquared));
            }
            return SRNG.PickRandomItem(items.ToArray());
            //return SRNG.randCollection(GetEdges(), false); 
        }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
        public Points GetRandomPointsOnEdge(int amount)
        {
            List<WeightedItem<Segment>> items = new(amount);
            var edges = GetEdges();
            foreach (var edge in edges)
            {
                items.Add(new(edge, (int)edge.LengthSquared));
            }
            var pickedEdges = SRNG.PickRandomItems(amount, items.ToArray());
            var randomPoints = new Points();
            foreach (var edge in pickedEdges)
            {
                randomPoints.Add(edge.GetRandomPoint());
            }
            return randomPoints;
        }

        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => SDrawing.DrawLines(this, linethickness, color);
        #endregion

        #region Private
        private float GetAreaSigned()
        {
            float totalArea = 0f;

            for (int i = 0; i < this.Count; i++)
            {
                Vector2 a = this[i];
                Vector2 b = this[(i + 1) % this.Count];

                float dy = (a.Y + b.Y) / 2f;
                float dx = b.X - a.X;

                float area = dy * dx;
                totalArea += area;
            }

            return totalArea;
        }
        #endregion

        #region Overlap
        //public static bool OverlapShape(this Polygon poly, Segments segments)
        //{
        //    foreach (var seg in segments)
        //    {
        //        if (poly.OverlapShape(seg)) return true;
        //    }
        //    return false;
        //}
        public bool OverlapShape(Segment s)
        {
            if (Count < 3) return false;
            if (ContainsPoint(s.Start)) return true;
            if (ContainsPoint(s.End)) return true;
            for (int i = 0; i < Count; i++)
            {
                Vector2 start = Get(i); // this[i];
                Vector2 end = Get(i + 1); //this[(i + 1) % Count];
                var segment = new Segment(start, end);
                if (segment.OverlapShape(s)) return true;
            }
            return false;
        }
        public bool OverlapShape(Circle c)
        {
            if (Count < 3) return false;
            if (ContainsPoint(c.Center)) return true;
            foreach (var p in this)
            {
                if (c.ContainsPoint(p)) return true;
            }
            for (int i = 0; i < Count; i++)
            {
                Vector2 start = Get(i); // this[i];
                Vector2 end = Get(i + 1); // this[(i + 1) % Count];
                if (c.OverlapShape(new Segment(start, end))) return true;
            }
            return false;
        }
        public bool OverlapShape(Triangle t) { return OverlapShape(t.ToPolygon()); }
        public bool OverlapShape(Rect r)
        {
            if (Count < 3) return false;
            var corners = r.ToPolygon();
            foreach (var c in corners)
            {
                if (ContainsPoint(c)) return true;
            }
            foreach (var p in this)
            {
                if (r.ContainsPoint(p)) return true;
            }

            for (int i = 0; i < Count; i++)
            {
                Vector2 start = Get(i); // poly[i];
                Vector2 end = Get(i + 1); // poly[(i + 1) % poly.Count];
                if (r.OverlapShape(new Segment(start, end))) return true;
            }
            return false;
        }
        public bool OverlapShape(Polygon b)
        {

            if (Count < 3 || b.Count < 3) return false;
            //return a.IntersectShape(b).Count > 0;

            Segments segmentsB = new();
            for (int j = 0; j < b.Count; j++)
            {
                Vector2 startB = b.Get(j); // b[j];
                if (ContainsPoint(startB)) return true;
                Vector2 endB = b.Get(j + 1);// b[(j + 1) % b.Count];
                Segment segB = new(startB, endB);
                segmentsB.Add(segB);
            }

            for (int i = 0; i < Count; i++)
            {
                Vector2 startA = Get(i); //  a[i];
                if (b.ContainsPoint(startA)) return true;
                Vector2 endA = Get(i + 1); // a[(i + 1) % a.Count];
                Segment segA = new(startA, endA);
                if (segA.OverlapShape(segmentsB)) return true;
            }
            return false;
        }
        public bool OverlapShape(Polyline pl) { return pl.OverlapShape(this); }



        #endregion

        #region Intersect
        public CollisionPoints IntersectShape(Segment s) { return GetEdges().IntersectShape(s); }
        public CollisionPoints IntersectShape(Circle c) { return GetEdges().IntersectShape(c); }
        public CollisionPoints IntersectShape(Triangle t) { return GetEdges().IntersectShape(t.GetEdges()); }
        public CollisionPoints IntersectShape(Rect r) { return GetEdges().IntersectShape(r.GetEdges()); }
        public CollisionPoints IntersectShape(Polygon b) { return GetEdges().IntersectShape(b.GetEdges()); }
        public CollisionPoints IntersectShape(Polyline pl) { return GetEdges().IntersectShape(pl.GetEdges()); }

        #endregion


        //public Vector2 GetReferencePoint() { return GetCentroid(); }
        //public SegmentShape GetSegmentShape() { return new(GetEdges(), this.GetCentroid()); }
    }
}
