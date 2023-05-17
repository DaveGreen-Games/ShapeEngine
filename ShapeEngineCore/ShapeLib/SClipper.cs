using System.Numerics;
using ShapeCore;
using Clipper2Lib;




namespace ShapeLib
{
    public static class SClipper
    {
        public static PathsD ClipRect(Rect rect, Polygon poly, int precision = 2, bool convexOnly = false)
        {
            return Clipper.ExecuteRectClip(rect.ToClipperRect(), poly.ToClipperPath(), precision, convexOnly);
        }
        public static PathsD Union(FillRule fillRule, params Polygon[] polygons) { return Union(polygons, fillRule); }
        public static PathsD Union(params Polygon[] polygons) { return Union(polygons, FillRule.NonZero); }
        public static PathsD Union(IEnumerable<Polygon> polygons, FillRule fillRule = FillRule.NonZero) { return Clipper.Union(polygons.ToClipperPaths(), fillRule); }
        public static PathsD Union(Polygon a, Polygon b, FillRule fillRule = FillRule.NonZero) { return Clipper.Union(ToClipperPaths(a), ToClipperPaths(b), fillRule); }
        
        public static PathsD Intersect(Polygon subject, Polygon clip, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            return Clipper.Intersect(ToClipperPaths(subject), ToClipperPaths(clip), fillRule, precision);
        }
        public static PathsD Intersect(Polygon clip, FillRule fillRule, params Polygon[] subjects)
        {
            var result = new PathsD();
            foreach (var subject in subjects)
            {
                result.AddRange(Clipper.Intersect(subject.ToClipperPaths(), clip.ToClipperPaths(), fillRule));
            }
            return result;
        }
        public static PathsD Intersect(Polygon clip, IEnumerable<Polygon> subjects, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            var result = new PathsD();
            foreach (var subject in subjects)
            {
                result.AddRange(Clipper.Intersect(subject.ToClipperPaths(), clip.ToClipperPaths(), fillRule, precision));
            }
            return result;
        }
        public static PathsD IntersectMany(Polygon subject, FillRule fillRule, params Polygon[] clips)
        {
            var cur = subject.ToClipperPaths();
            foreach (var clip in clips)
            {
                cur = Clipper.Intersect(cur, clip.ToClipperPaths(), fillRule, 2);
            }
            return cur;
        }
        public static PathsD IntersectMany(Polygon subject, IEnumerable<Polygon> clips, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            var cur = subject.ToClipperPaths();
            foreach (var clip in clips)
            {
                cur = Clipper.Intersect(cur, clip.ToClipperPaths(), fillRule, precision);
            }
            return cur;
        }

        public static PathsD Difference(Polygon subject, Polygon clip, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            return Clipper.Difference(ToClipperPaths(subject), ToClipperPaths(clip), fillRule, precision);
        }
        public static PathsD Difference(Polygon clip, FillRule fillRule, params Polygon[] subjects)
        {
            var result = new PathsD();
            foreach (var subject in subjects)
            {
                result.AddRange(Clipper.Difference(subject.ToClipperPaths(), clip.ToClipperPaths(), fillRule));
            }
            return result;
        }
        public static PathsD Difference(Polygon clip, IEnumerable<Polygon> subjects, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            var result = new PathsD();
            foreach (var subject in subjects)
            {
                result.AddRange(Clipper.Difference(subject.ToClipperPaths(), clip.ToClipperPaths(), fillRule, precision));
            }
            return result;
        }
        public static PathsD DifferenceMany(Polygon subject, FillRule fillRule, params Polygon[] clips)
        {
            var cur = subject.ToClipperPaths();
            foreach (var clip in clips)
            {
                cur = Clipper.Difference(cur, clip.ToClipperPaths(), fillRule, 2);
            }
            return cur;
        }
        public static PathsD DifferenceMany(Polygon subject, IEnumerable<Polygon> clips, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            var cur = subject.ToClipperPaths();
            foreach (var clip in clips)
            {
                cur = Clipper.Difference(cur, clip.ToClipperPaths(), fillRule, precision);
            }
            return cur;
        }

        //TODO IMPLEMENT
        //Clipper.InflatePaths
        //Clipper.IsPositive
        //Clipper.OffsetPath
        //Clipper.PointInPolygon
        //Clipper.ScalePathD
        //Clipper.ScalePathsD
        //Clipper.SimplifyPath
        //Clipper.SimplifyPaths
        //Clipper.StripDuplicates
        //Clipper.StripNearDuplicates
        //Clipper.TrimCollinear


        //what does those funcs do?
        //Clipper.MinkowskiDiff
        //Clipper.MinkowskiSum
        //Clipper.RamerDouglasPeucker //simplifies path


        public static Polygon CreateEllipse(Vector2 center, float radiusX, float radiusY = 0f, int steps = 0) { return Clipper.Ellipse(center.ToClipperPoint(), radiusX, radiusY, steps).ToPolygon(); }

        public static Vector2 ToVec2(this PointD p) { return new((float)p.x, (float)p.y); }
        public static PointD ToClipperPoint(this Vector2 v) { return new(v.X, v.Y); }
        public static RectD ToClipperRect(this Rect r) { return new RectD(r.x, r.y, r.x + r.width, r.y + r.height); }
        public static Rect ToRect(this RectD r) { return new Rect((float)r.left, (float)r.top, (float)r.Width, (float)r.Height); }
        public static Polygon ToPolygon(this PathD path)
        {
            var poly = new Polygon();
            foreach (var point in path)
            {
                poly.Add(point.ToVec2());
            }
            return poly;
        }
        public static List<Polygon> ToPolygons(this PathsD paths)
        {
            var polygons = new List<Polygon>();
            foreach (var path in paths)
            {
                polygons.Add(path.ToPolygon());
            }
            return polygons;
        }
        public static PathD ToClipperPath(this Polygon poly)
        {
            var path = new PathD();
            foreach (var vertex in poly)
            {
                path.Add(vertex.ToClipperPoint());
            }
            return path;
        }
        public static PathsD ToClipperPaths(this Polygon poly) { return new PathsD() { poly.ToClipperPath() }; }
        public static PathsD ToClipperPaths(params Polygon[] polygons) { return polygons.ToClipperPaths(); }
        public static PathsD ToClipperPaths(this IEnumerable<Polygon> polygons)
        {
            var result = new PathsD();
            foreach(var polygon in polygons)
            {
                result.Add(polygon.ToClipperPath());
            }
            return result;
        }
        
        //public static List<PathsD> ToClipperPathsList(this IEnumerable<Polygon> polygons)
        //{
        //    var result = new List<PathsD>();
        //    foreach (var polygon in polygons)
        //    {
        //        result.Add(polygon.ToClipperPaths());
        //    }
        //    return result;
        //}


    }
}
