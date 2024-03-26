using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Shapes;
using Clipper2Lib;

namespace ShapeEngine.Lib
{
    public class Polygons : List<Polygon>
    {
        public Polygons() { }
        public Polygons(int capacity) : base(capacity) { }
        public Polygons(params Polygon[] polygons) { AddRange(polygons); }
        public Polygons(IEnumerable<Polygon> polygons) { AddRange(polygons); }
    }
    public class Polylines : List<Polyline>
    {
        public Polylines() { }
        public Polylines(params Polyline[] polylines) { AddRange(polylines); }
        public Polylines(IEnumerable<Polyline> polylines) { AddRange(polylines); }
    }
    public static class ShapeClipper
    {
        public static PathsD ClipRect(this Rect rect, Polygon poly, int precision = 2, bool convexOnly = false)
        {
            return Clipper.ExecuteRectClip(rect.ToClipperRect(), poly.ToClipperPath(), precision, convexOnly);
        }
        //public static PathsD Union(this Polygons polygons, FillRule fillRule = FillRule.NonZero) 
        //{
        //    if (polygons.Count <= 0) return new();
        //    else if(polygons.Count == 1) return polygons[0].ToClipperPaths();
        //    else
        //    {
        //        var main = polygons[0];
        //        polygons.RemoveAt(0);
        //        return Union(main, polygons, fillRule);
        //    
        //    }
        //
        //    //return Clipper.Union(polygons.ToClipperPaths(), fillRule); 
        //}
        public static PathsD UnionMany(this Polygon a, Polygons other, FillRule fillRule = FillRule.NonZero)
        {
            
            return Clipper.Union(a.ToClipperPaths(), other.ToClipperPaths(), fillRule);
        }
        public static PathsD Union(this Polygon a, Polygon b, FillRule fillRule = FillRule.NonZero) { return Clipper.Union(ToClipperPaths(a), ToClipperPaths(b), fillRule); }

        // public static void UnionSelf(Polygon a, Polygon b, FillRule fillRule = FillRule.NonZero)
        // {
        //     var result = Clipper.Union(ToClipperPaths(a), ToClipperPaths(b), fillRule);
        //     if (result.Count > 0)
        //     {
        //         a.Clear();
        //         foreach (var p in result[0])
        //         {
        //             a.Add(p.ToVec2());
        //         }
        //     }
        //     
        //
        // }
        
        public static PathsD Intersect(this Polygon subject, Polygon clip, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            return Clipper.Intersect(ToClipperPaths(subject), ToClipperPaths(clip), fillRule, precision);
        }
        public static PathsD Intersect(this Polygon clip, Polygons subjects, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            var result = new PathsD();
            foreach (var subject in subjects)
            {
                result.AddRange(Clipper.Intersect(subject.ToClipperPaths(), clip.ToClipperPaths(), fillRule, precision));
            }
            return result;
        }
        public static PathsD IntersectMany(this Polygon subject, Polygons clips, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            var cur = subject.ToClipperPaths();
            foreach (var clip in clips)
            {
                cur = Clipper.Intersect(cur, clip.ToClipperPaths(), fillRule, precision);
            }
            return cur;
        }

        public static PathsD Difference(this Polygon subject, Polygon clip, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            return Clipper.Difference(ToClipperPaths(subject), ToClipperPaths(clip), fillRule, precision);
        }
        
        public static PathsD Difference(this Polygon clip, Polygons subjects, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            var result = new PathsD();
            foreach (var subject in subjects)
            {
                result.AddRange(Clipper.Difference(subject.ToClipperPaths(), clip.ToClipperPaths(), fillRule, precision));
            }
            return result;
        }
        public static PathsD DifferenceMany(this Polygon subject, Polygons clips, FillRule fillRule = FillRule.NonZero, int precision = 2)
        {
            var cur = subject.ToClipperPaths();
            foreach (var clip in clips)
            {
                cur = Clipper.Difference(cur, clip.ToClipperPaths(), fillRule, precision);
            }
            return cur;
        }
        

        
        public static bool IsHole(this PathD path) { return !Clipper.IsPositive(path); }
        public static bool IsHole(this Polygon p) { return IsHole(p.ToClipperPath()); }
        
        public static PathsD RemoveAllHoles(this PathsD paths) { paths.RemoveAll((p) => { return IsHole(p); }); return paths; }
        public static Polygons RemoveAllHoles(this Polygons polygons) { polygons.RemoveAll((p) => { return IsHole(p); }); return polygons; }
        
        public static PathsD GetAllHoles(this PathsD paths) { paths.RemoveAll((p) => { return !IsHole(p); }); return paths; }
        public static Polygons GetAllHoles(this Polygons polygons) { polygons.RemoveAll((p) => { return !IsHole(p); }); return polygons; }
        
        public static PathsD RemoveAllHolesCopy(this PathsD paths)
        {
            var result = new PathsD();
            foreach (var p in paths)
            {
                if(!IsHole(p)) result.Add(p);
            }
            return result;
        }
        public static Polygons RemoveAllHolesCopy(this Polygons polygons)
        {
            var result = new Polygons();
            foreach (var p in polygons)
            {
                if (!IsHole(p)) result.Add(p);
            }
            return result;
        }
       
        public static PathsD GetAllHolesCopy(this PathsD paths)
        {
            var result = new PathsD();
            foreach (var p in paths)
            {
                if (IsHole(p)) result.Add(p);
            }
            return result;
        }
        public static Polygons GetAllHolesCopy(this Polygons polygons)
        {
            var result = new Polygons();
            foreach (var p in polygons)
            {
                if (IsHole(p)) result.Add(p);
            }
            return result;
        }

        public static PathsD Inflate(this Polyline polyline, float delta, JoinType joinType = JoinType.Square, EndType endType = EndType.Square, float miterLimit = 2f, int precision = 2)
        {
            return Clipper.InflatePaths(polyline.ToClipperPaths(), delta, joinType, endType, miterLimit, precision);
        }
        public static PathsD Inflate(this Polygon poly, float delta, JoinType joinType = JoinType.Square, EndType endType = EndType.Polygon, float miterLimit = 2f, int precision = 2)
        {
            return Clipper.InflatePaths(poly.ToClipperPaths(), delta, joinType, endType, miterLimit, precision);
        }
        public static PathsD Inflate(this Polygons polygons, float delta, JoinType joinType = JoinType.Square, EndType endType = EndType.Polygon, float miterLimit = 2f, int precision = 2)
        {
            return Clipper.InflatePaths(polygons.ToClipperPaths(), delta, joinType, endType, miterLimit, precision);
        }
        
        public static PathD Simplify(this Polygon poly, float epsilon, bool isOpen = false) { return Clipper.SimplifyPath(poly.ToClipperPath(), epsilon, isOpen); }
        public static PathsD Simplify(this Polygons poly, float epsilon, bool isOpen = false) { return Clipper.SimplifyPaths(poly.ToClipperPaths(), epsilon, isOpen); }
        
        /// <summary>
        /// Uses RamerDouglasPeucker algorithm. Only works on closed polygons
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static PathD SimplifyRDP(this Polygon poly, float epsilon) { return Clipper.RamerDouglasPeucker(poly.ToClipperPath(), epsilon); }
        /// <summary>
        /// Uses RamerDouglasPeucker algorithm. Only works on closed polygons
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static PathsD SimplifyRDP(this Polygons poly, float epsilon) { return Clipper.SimplifyPaths(poly.ToClipperPaths(), epsilon); }

        public static PointInPolygonResult IsPointInsideClipper(this Polygon poly, Vector2 p) { return Clipper.PointInPolygon(p.ToClipperPoint(), poly.ToClipperPath()); }
        public static bool IsPointInside(this Polygon poly, Vector2 p) { return IsPointInsideClipper(poly, p) != PointInPolygonResult.IsOutside; }

        public static PathD TrimCollinear(this Polygon poly, int precision, bool isOpen = false) { return Clipper.TrimCollinear(poly.ToClipperPath(), precision, isOpen); }
        public static PathD StripDuplicates(this Polygon poly, float minEdgeLengthSquared, bool isOpen = false) { return Clipper.StripNearDuplicates(poly.ToClipperPath(), minEdgeLengthSquared, isOpen); }

        public static PathsD MinkowskiDiff(this Polygon poly, Polygon path, bool isClosed = false) { return Clipper.MinkowskiDiff(poly.ToClipperPath(), path.ToClipperPath(), isClosed); }
        public static PathsD MinkowskiSum(this Polygon poly, Polygon path, bool isClosed = false) { return Clipper.MinkowskiSum(poly.ToClipperPath(), path.ToClipperPath(), isClosed); }

        public static PathsD MinkowskiDiffOrigin(this Polygon poly, Polygon path, bool isClosed = false)
        {
            var pathCopy = path.SetPositionCopy(new(0f));
            if (pathCopy == null) return new();
            return Clipper.MinkowskiDiff(poly.ToClipperPath(), pathCopy.ToClipperPath(), isClosed);
        }

        public static PathsD MinkowskiSumOrigin(this Polygon poly, Polygon path, bool isClosed = false)
        {
            var pathCopy = path.SetPositionCopy(new(0f));
            if (pathCopy == null) return new();
            return Clipper.MinkowskiSum(poly.ToClipperPath(), pathCopy.ToClipperPath(), isClosed);
        }

        public static Polygon CreateEllipse(Vector2 center, float radiusX, float radiusY = 0f, int steps = 0) { return Clipper.Ellipse(center.ToClipperPoint(), radiusX, radiusY, steps).ToPolygon(); }

        public static Vector2 ToVec2(this PointD p) { return new((float)p.x, -(float)p.y); }//flip of y necessary -> clipper up y is positve - raylib is negative
        public static PointD ToClipperPoint(this Vector2 v) { return new(v.X, -v.Y); }
        public static RectD ToClipperRect(this Rect r) { return new RectD(r.X, -r.Y-r.Height, r.X + r.Width, -r.Y); }
        public static Rect ToRect(this RectD r) { return new Rect((float)r.left, (float)(-r.top-r.Height), (float)r.Width, (float)r.Height); }
        public static Polygon ToPolygon(this PathD path)
        {
            var poly = new Polygon();
            foreach (var point in path)
            {
                poly.Add(point.ToVec2());
            }
            return poly;
        }
        public static Polygons ToPolygons(this PathsD paths, bool removeHoles = false)
        {
            var polygons = new Polygons();
            foreach (var path in paths)
            {
                if (!removeHoles || !IsHole(path))
                {
                    polygons.Add(path.ToPolygon());
                }
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

        public static Polyline ToPolyline(this PathD path)
        {
            var polyline = new Polyline();
            foreach (var point in path)
            {
                polyline.Add(point.ToVec2());
            }
            return polyline;
        }
        public static Polylines ToPolylines(this PathsD paths, bool removeHoles = false)
        {
            var polylines = new Polylines();
            foreach (var path in paths)
            {
                if (!removeHoles || !IsHole(path))
                {
                    polylines.Add(path.ToPolyline());
                }
            }
            return polylines;
        }
        public static PathD ToClipperPath(this Polyline polyline)
        {
            var path = new PathD();
            foreach (var vertex in polyline)
            {
                path.Add(vertex.ToClipperPoint());
            }
            return path;
        }
        public static PathsD ToClipperPaths(this Polyline polyline) { return new PathsD() { polyline.ToClipperPath() }; }
        public static PathsD ToClipperPaths(params Polyline[] polylines) { return polylines.ToClipperPaths(); }
        public static PathsD ToClipperPaths(this IEnumerable<Polyline> polylines)
        {
            var result = new PathsD();
            foreach (var polyline in polylines)
            {
                result.Add(polyline.ToClipperPath());
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
