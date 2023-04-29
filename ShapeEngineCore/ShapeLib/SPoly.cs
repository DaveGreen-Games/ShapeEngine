using Raylib_CsLo;
using ShapeCore;
using System.Numerics;

namespace ShapeLib
{
    public static class SPoly
    {
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
        public static List<Vector2> GetShape(List<Vector2> relativePoints, Vector2 pos, float rotRad, Vector2 scale)
        {
            if (relativePoints.Count < 3) return new();
            List<Vector2> shape = new();
            for (int i = 0; i < relativePoints.Count; i++)
            {
                shape.Add(pos + SVec.Rotate(relativePoints[i], rotRad) * scale);
            }
            return shape;
        }
        
        public static Rect GetBoundingBox(List<Vector2> points)
        {
            if (points.Count < 2) return new();
            Vector2 start = points[0];
            Rect r = new(start.X, start.Y, 0, 0);

            foreach (var p in points)
            {
                r = SRect.Enlarge(r, p);
            }
            return r;
        }
        public static Vector2 GetCentroid(List<Vector2> points)
        {
            if (points.Count <= 0) return new(0f);
            Vector2 total = new(0f);
            foreach (Vector2 p in points) { total += p; }
            return total / points.Count;
        }
        public static List<Vector2> Move(List<Vector2> points, Vector2 translation)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] += translation;
            }
            return points;
        }
        public static List<Vector2> Rotate(List<Vector2> points, Vector2 pivot, float rotRad)
        {
            if (points.Count < 3) return new();
            List<Vector2> rotated = new();
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 w = points[i] - pivot;
                rotated.Add(pivot + w.Rotate(rotRad));
            }
            return rotated;
        }
        public static List<Vector2> Scale(List<Vector2> points, float scale)
        {
            var shape = new List<Vector2>();
            for (int i = 0; i < points.Count; i++)
            {
                shape.Add(points[i] * scale);
            }
            return shape;
        }
        public static List<Vector2> Scale(List<Vector2> points, Vector2 pivot, float scale)
        {
            if (points.Count < 3) return new();
            List<Vector2> scaled = new();
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 w = points[i] - pivot;
                scaled.Add(pivot + w * scale);
            }
            return scaled;
        }
        public static List<Vector2> ScaleUniform(List<Vector2> points, float distance)
        {
            var shape = new List<Vector2>();
            for (int i = 0; i < points.Count; i++)
            {
                shape.Add(SVec.ScaleUniform(points[i], distance));
            }
            return shape;
        }
        public static List<Segment> GetSegments(List<Vector2> points)
        {
            if (points.Count <= 1) return new();
            else if (points.Count == 2)
            {
                return new() { new(points[0], points[1]) };
            }
            List<Segment> segments = new();
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[(i + 1) % points.Count];
                segments.Add(new(start, end));
            }
            return segments;
        }
        public static List<Vector2> GetSegmentAxis(List<Vector2> points, bool normalized = false)
        {
            if (points.Count <= 1) return new();
            else if (points.Count == 2)
            {
                return new() { points[1] - points[0] };
            }
            List<Vector2> axis = new();
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[(i + 1) % points.Count];
                Vector2 a = end - start;
                axis.Add(normalized ? SVec.Normalize(a) : a);
            }
            return axis;
        }
        public static List<Vector2> GetSegmentAxis(List<Segment> segments, bool normalized = false)
        {
            List<Vector2> axis = new();
            foreach (var seg in segments)
            {
                axis.Add(normalized ? seg.Dir : seg.Displacement);
            }
            return axis;
        }
        public static float GetArea(List<Vector2> points, Vector2 center)
        {
            if (points.Count < 3) return 0f;
            var triangles = Triangulate(points, center);
            float totalArea = 0f;
            foreach (var t in triangles)
            {
                totalArea += t.GetArea();
            }
            return totalArea;
        }
        public static float GetCircumference(List<Vector2> points)
        {
            return MathF.Sqrt(GetCircumferenceSquared(points));
        }
        public static float GetCircumferenceSquared(List<Vector2> points)
        {
            if (points.Count < 3) return 0f;
            float lengthSq = 0f;
            points.Add(points[0]);
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 w = points[i + 1] - points[i];
                lengthSq += w.LengthSquared();
            }
            return lengthSq;
        }
        public static List<Triangle> Triangulate(List<Vector2> points, Vector2 center)
        {
            if (points.Count < 3) return new();
            List<Triangle> triangles = new();
            points.Add(points[0]);
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 a = points[i];
                Vector2 b = center;
                Vector2 c = points[i + 1];
                triangles.Add(new(a, b, c));
            }
            return triangles;
        }
        public static List<Vector2> GeneratePoints(Vector2 center, int pointCount, float minLength, float maxLength)
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
        public static List<Vector2> GeneratePoints(int pointCount, float minLength, float maxLength)
        {
            List<Vector2> points = new();
            float angleStep = RayMath.PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = SRNG.randF(minLength, maxLength);
                Vector2 p = SVec.Rotate(SVec.Right(), angleStep * i) * randLength;
                points.Add(p);
            }
            return points;
        }
        
        
        public static Polygon GetShape(this Polygon p, Vector2 pos, float rotRad, Vector2 scale) { return new(GetShape(p.points, pos, rotRad, scale), pos); }
        
        public static Vector2 GetCentroid(this Polygon p) { return GetCentroid(p.points); }
        public static Rect GetBoundingBox(this Polygon p) { return GetBoundingBox(p.points); }
        public static Polygon Move(this Polygon p, Vector2 translation) { return new(Move(p.points, translation), p.center + translation); }
        public static Polygon Rotate(this Polygon p, Vector2 pivot, float rotRad) { return new(Rotate(p.points, pivot, rotRad), p.center); }
        public static Polygon Scale(this Polygon p, float scale) { return new(Scale(p.points, scale), p.center); }
        public static Polygon Scale(this Polygon p, Vector2 pivot, float scale) { return new(Scale(p.points, pivot, scale)); }
        public static Polygon ScaleUniform(this Polygon p, float distance) { return new(ScaleUniform(p.points, distance), p.center); }
        public static List<Segment> GetSegments(this Polygon p) { return GetSegments(p.points); }
        //public static SegmentShape GetSegmentsShape(this Polygon p) { return new(GetSegments(p.points), p.center); }
        public static List<Vector2> GetSegmentAxis(this Polygon p, bool normalized = false) { return GetSegmentAxis(p.points, normalized); }
        public static List<Triangle> Triangulate(this Polygon p) { return Triangulate(p.points, p.center); }
        public static float GetArea(this Polygon p) { return GetArea(p.points, p.center); }
        public static float GetCircumference(this Polygon p) { return GetCircumference(p.points); }
        public static float GetCircumferenceSquared(this Polygon p) { return GetCircumferenceSquared(p.points); }
        
        public static Polygon GeneratePointsPolygon(Vector2 center, int pointCount, float minLength, float maxLength) { return new Polygon(GeneratePoints(center, pointCount, minLength, maxLength), center); }
        public static Polygon GeneratePointsPolygon(int pointCount, float minLength, float maxLength) { return new(GeneratePoints(pointCount, minLength, maxLength), new Vector2(0f)); }
        


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
    }
}
