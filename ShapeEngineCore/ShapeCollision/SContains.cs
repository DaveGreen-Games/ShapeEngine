using System.Numerics;
using ShapeLib;
using Raylib_CsLo;

namespace ShapeCollision
{
    public static class SContains
    {//CONTAINS
        //Circle - Point/Circle/Rect/Line
        public static bool Contains(Collider a, Collider b)
        {
            if (a is CircleCollider)
            {
                if (b is CircleCollider)
                {
                    return ContainsCircleCircle((CircleCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ContainsCircleSegment((CircleCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ContainsCircleRect((CircleCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ContainsCirclePoly((CircleCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ContainsCirclePoint((CircleCollider)a, b);
                }
            }
            else if (a is SegmentCollider)
            {
                return false;
                //if (b is CircleCollider)
                //{
                //    return OverlapSegmentCircle((SegmentCollider)a, (CircleCollider)b);
                //}
                //else if (b is SegmentCollider)
                //{
                //    return OverlapSegmentSegment((SegmentCollider)a, (SegmentCollider)b);
                //}
                //else if (b is RectCollider)
                //{
                //    return OverlapSegmentRect((SegmentCollider)a, (RectCollider)b);
                //}
                //else if (b is PolyCollider)
                //{
                //    return OverlapSegmentPoly((SegmentCollider)a, (PolyCollider)b);
                //}
                //else
                //{
                //    return OverlapSegmentPoint((SegmentCollider)a, b);
                //}
            }
            else if (a is RectCollider)
            {
                if (b is CircleCollider)
                {
                    return ContainsRectCircle((RectCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ContainsRectSegment((RectCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ContainsRectRect((RectCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ContainsRectPoly((RectCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ContainsRectPoint((RectCollider)a, b);
                }
            }
            else if (a is PolyCollider)
            {
                if (b is CircleCollider)
                {
                    return ContainsPolyCircle((PolyCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ContainsPolySegment((PolyCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ContainsPolyRect((PolyCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ContainsPolyPoly((PolyCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ContainsPolyPoint((PolyCollider)a, b);
                }
            }
            else
            {
                return false;
                //if (b is CircleCollider)
                //{
                //    return OverlapPointCircle(a, (CircleCollider)b);
                //}
                //else if (b is SegmentCollider)
                //{
                //    return OverlapPointSegment(a, (SegmentCollider)b);
                //}
                //else if (b is RectCollider)
                //{
                //    return OverlapPointRect(a, (RectCollider)b);
                //}
                //else if (b is PolyCollider)
                //{
                //    return OverlapPointPoly(a, (PolyCollider)b);
                //}
                //else
                //{
                //    return OverlapPointPoint(a, b);
                //}
            }
        }
        public static bool ContainsCirclePoint(Vector2 circlePos, float r, Vector2 point)
        {
            Vector2 dif = circlePos - point;
            return dif.LengthSquared() < r * r;
        }
        public static bool ContainsCirclePoint(CircleCollider circle, Collider point)
        {
            return ContainsCirclePoint(circle.Pos, circle.Radius, point.Pos);
        }
        public static bool ContainsCirclePoint(CircleCollider circle, Vector2 point)
        {
            return ContainsCirclePoint(circle.Pos, circle.Radius, point);
        }
        public static bool ContainsCircleCircle(Vector2 aPos, float aR, Vector2 bPos, float bR)
        {
            if (aR <= bR) return false;
            Vector2 dif = bPos - aPos;

            return dif.LengthSquared() + bR * bR < aR * aR;
        }
        public static bool ContainsCircleCircle(CircleCollider self, CircleCollider other)
        {
            return ContainsCircleCircle(self.Pos, self.Radius, other.Pos, other.Radius);
        }
        public static bool ContainsCircleCircle(CircleCollider circle, Vector2 pos, float radius)
        {
            return ContainsCircleCircle(circle.Pos, circle.Radius, pos, radius);
        }
        public static bool ContainsCircleRect(CircleCollider circle, RectCollider rect)
        {
            var corners = rect.GetCorners();
            if (!ContainsCirclePoint(circle, corners.tl)) return false;
            if (!ContainsCirclePoint(circle, corners.br)) return false;
            return true;
        }
        public static bool ContainsCircleSegment(CircleCollider circle, SegmentCollider segment)
        {
            if (!ContainsCirclePoint(circle, segment.Pos)) return false;
            if (!ContainsCirclePoint(circle, segment.End)) return false;
            return true;
        }

        public static bool ContainsCircleRect(CircleCollider circle, Rectangle rect, Vector2 pivot, float angleDeg)
        {
            return ContainsCircleRect(circle.Pos, circle.Radius, rect, pivot, angleDeg);
        }
        public static bool ContainsCircleRect(Vector2 circlePos, float radius, Rectangle rect, Vector2 pivot, float angleDeg)
        {
            var rr = SRect.RotateRectList(rect, pivot, angleDeg);
            foreach (var point in rr)
            {
                bool contains = ContainsCirclePoint(circlePos, radius, point);
                if (!contains) return false;
            }
            return true;
        }

        public static bool ContainsCirclePoly(CircleCollider circle, PolyCollider poly)
        {
            return ContainsCirclePoly(circle, poly.Shape);
        }
        public static bool ContainsCirclePoly(CircleCollider circle, List<Vector2> poly)
        {
            return ContainsCirclePoly(circle.Pos, circle.Radius, poly);
        }
        public static bool ContainsCirclePoly(Vector2 circlePos, float radius, List<Vector2> poly)
        {
            foreach (var point in poly)
            {
                bool contains = ContainsCirclePoint(circlePos, radius, point);
                if (!contains) return false;
            }
            return true;
        }


        //Rect - Rect/Point/Circle/Line
        public static bool ContainsRectRect(RectCollider a, RectCollider b)
        {
            return ContainsRectRect(a.Rect, b.Rect);
        }
        public static bool ContainsRectRect(Rectangle a, Rectangle b)
        {
            return a.X < b.X && a.Y < b.Y && a.X + a.width > b.X + b.width && a.Y + a.height > b.Y + b.height;
        }
        public static bool ContainsRectRect(Rectangle a, RectCollider b)
        {
            return ContainsRectRect(a, b.Rect);
        }
        public static bool ContainsRectPoint(RectCollider rect, Collider point)
        {
            return ContainsRectPoint(rect.Rect, point.Pos);
        }
        public static bool ContainsRectPoint(RectCollider rect, Vector2 point)
        {
            return ContainsRectPoint(rect.Rect, point);
        }
        public static bool ContainsRectPoint(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, Vector2 point)
        {
            return ContainsRectPoint(SRect.ConstructRect(rectPos, rectSize, rectAlignement), point);
        }
        public static bool ContainsRectPoint(Rectangle rect, Vector2 point)
        {
            return rect.X <= point.X && rect.Y <= point.Y && rect.X + rect.width >= point.X && rect.Y + rect.height >= point.Y;
        }
        public static bool ContainsRectPoint(Rectangle rect, Collider point)
        {
            return ContainsRectPoint(rect, point.Pos);
        }
        public static bool ContainsRectCircle(RectCollider rect, CircleCollider circle)
        {
            return ContainsRectCircle(rect.Rect, circle);
        }
        public static bool ContainsRectCircle(Rectangle rect, CircleCollider circle)
        {
            return ContainsRectCircle(rect, circle.Pos, circle.Radius);
        }
        public static bool ContainsRectCircle(RectCollider rect, Vector2 circlePos, float circleRadius)
        {
            return ContainsRectCircle(rect.Rect, circlePos, circleRadius);
        }
        public static bool ContainsRectCircle(Rectangle rect, Vector2 circlePos, float circleRadius)
        {
            return
                rect.X <= circlePos.X - circleRadius &&
                rect.Y <= circlePos.Y - circleRadius &&
                rect.X + rect.width >= circlePos.X + circleRadius &&
                rect.Y + rect.height >= circlePos.Y + circleRadius;
        }
        public static bool ContainsRectSegment(RectCollider rect, SegmentCollider segment)
        {
            if (!ContainsRectPoint(rect, segment.Pos)) return false;
            if (!ContainsRectPoint(rect, segment.End)) return false;
            return true;
        }
        public static bool ContainsRectSegment(Rectangle rect, SegmentCollider segment)
        {
            if (!ContainsRectPoint(rect, segment.Pos)) return false;
            if (!ContainsRectPoint(rect, segment.End)) return false;
            return true;
        }
        public static bool ContainsRectSegment(Rectangle rect, Vector2 start, Vector2 end)
        {
            if (!ContainsRectPoint(rect, start)) return false;
            if (!ContainsRectPoint(rect, end)) return false;
            return true;
        }

        public static bool ContainsRectPoint(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 point)
        {
            return ContainsPolyPoint(SRect.RotateRectList(rect, pivot, angleDeg), point);
        }
        public static bool ContainsRectCircle(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 circlePos, float radius)
        {
            return ContainsPolyCircle(SRect.RotateRectList(rect, pivot, angleDeg), circlePos, radius);
        }
        public static bool ContainsRectSegment(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 start, Vector2 end)
        {
            return ContainsPolySegment(SRect.RotateRectList(rect, pivot, angleDeg), start, end);
        }
        public static bool ContainsRectPoint(Rectangle rect, Vector2 pivot, float angleDeg, Collider point)
        {
            return ContainsPolyPoint(SRect.RotateRectList(rect, pivot, angleDeg), point);
        }
        public static bool ContainsRectCircle(Rectangle rect, Vector2 pivot, float angleDeg, CircleCollider circle)
        {
            return ContainsPolyCircle(SRect.RotateRectList(rect, pivot, angleDeg), circle);
        }
        public static bool ContainsRectSegment(Rectangle rect, Vector2 pivot, float angleDeg, SegmentCollider segment)
        {
            return ContainsPolySegment(SRect.RotateRectList(rect, pivot, angleDeg), segment);
        }
        public static bool ContainsRectPoly(RectCollider rect, List<Vector2> poly)
        {
            return ContainsRectPoly(rect.Rect, poly);
        }
        public static bool ContainsRectPoly(RectCollider rect, PolyCollider poly)
        {
            return ContainsRectPoly(rect, poly.Shape);
        }
        public static bool ContainsRectPoly(Rectangle rect, List<Vector2> poly)
        {

            if (poly.Count < 3) return false;
            for (int i = 0; i < poly.Count; i++)
            {
                if (!ContainsRectPoint(rect, poly[i])) return false;
            }
            return true;
        }
        public static bool ContainsRectPoly(Rectangle rect, Vector2 pivot, float angleDeg, List<Vector2> poly)
        {
            return ContainsPolyPoly(SRect.RotateRectList(rect, pivot, angleDeg), poly);
        }
        public static bool ContainsRectPoly(Vector2 pos, Vector2 size, Vector2 alignement, List<Vector2> poly)
        {
            return ContainsRectPoly(SRect.ConstructRect(pos, size, alignement), poly);
        }
        public static bool ContainsRectPoly(Vector2 pos, Vector2 size, Vector2 alignement, float angleDeg, List<Vector2> poly)
        {
            return ContainsPolyPoly(SRect.RotateRectList(pos, size, alignement, alignement, angleDeg), poly);
        }

        public static bool ContainsPolyPoint(List<Vector2> poly, Vector2 point)
        {
            if (poly.Count < 3) return false;
            int intersections = 0;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var info = SGeometry.IntersectRaySegmentInfo(point, new(1f, 0f), start, end);
                if (info.intersected) intersections += 1;
            }

            return !(intersections % 2 == 0);
        }
        public static bool ContainsPolyPoint(List<Vector2> poly, Collider point)
        {
            if (poly.Count < 3) return false;
            return ContainsPolyPoint(poly, point.Pos);
        }
        public static bool ContainsPolyPoint(PolyCollider poly, Collider point)
        {
            return ContainsPolyPoint(poly.Shape, point);
        }
        public static bool ContainsPolyCircle(List<Vector2> poly, Vector2 circlePos, float radius)
        {
            if (poly.Count < 3) return false;
            if (!ContainsPolyPoint(poly, circlePos)) return false;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var points = SGeometry.IntersectSegmentCircle(start, end, circlePos, radius);
                if (points.Count > 0) return false;
            }
            return true;
        }
        public static bool ContainsPolyCircle(List<Vector2> poly, CircleCollider circle)
        {
            return ContainsPolyCircle(poly, circle.Pos, circle.Radius);
        }
        public static bool ContainsPolyCircle(PolyCollider poly, CircleCollider circle)
        {
            return ContainsPolyCircle(poly.Shape, circle);
        }
        public static bool ContainsPolySegment(List<Vector2> poly, Vector2 segmentStart, Vector2 segmentEnd)
        {
            if (poly.Count < 3) return false;
            return ContainsPolyPoint(poly, segmentStart) && ContainsPolyPoint(poly, segmentEnd);
        }
        public static bool ContainsPolySegment(List<Vector2> poly, SegmentCollider segment)
        {
            if (poly.Count < 3) return false;
            return ContainsPolySegment(poly, segment.Pos, segment.End);
        }
        public static bool ContainsPolySegment(PolyCollider poly, SegmentCollider segment)
        {
            return ContainsPolySegment(poly.Shape, segment);
        }
        public static bool ContainsPolyRect(List<Vector2> poly, Rectangle rect)
        {
            if (poly.Count < 3) return false;
            var points = SRect.GetRectCornersList(rect);
            foreach (var point in points)
            {
                if (!ContainsPolyPoint(poly, point)) return false;
            }
            return true;
        }
        public static bool ContainsPolyRect(List<Vector2> poly, RectCollider rect)
        {
            if (poly.Count < 3) return false;
            return ContainsPolyRect(poly, rect.Rect);
        }
        public static bool ContainsPolyRect(List<Vector2> poly, Rectangle rect, Vector2 pivot, float rotDeg)
        {
            if (poly.Count < 3) return false;
            var points = SRect.RotateRectList(rect, pivot, rotDeg);
            foreach (var point in points)
            {
                if (!ContainsPolyPoint(poly, point)) return false;
            }
            return true;
        }
        public static bool ContainsPolyRect(PolyCollider poly, RectCollider rect)
        {
            return ContainsPolyRect(poly.Shape, rect);
        }
        public static bool ContainsPolyPoly(List<Vector2> a, List<Vector2> b)
        {
            if (a.Count < 3 || b.Count < 3) return false;
            foreach (var point in b)
            {
                if (!ContainsPolyPoint(a, point)) return false;
            }
            return true;
        }
        public static bool ContainsPolyPoly(PolyCollider a, PolyCollider b)
        {
            return ContainsPolyPoly(a.Shape, b.Shape);
        }

    }
}
