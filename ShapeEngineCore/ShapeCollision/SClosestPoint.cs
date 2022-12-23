using ShapeLib;
using System.Numerics;
using Raylib_CsLo;

namespace ShapeCollision
{
    public static class SClosestPoint
    {
        //CLOSEST POINT
        //circle - circle
        public static Vector2 ClosestPoint(Collider a, Collider b)
        {
            if (a is CircleCollider)
            {
                if (b is CircleCollider)
                {
                    return ClosestPointCircleCircle((CircleCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ClosestPointCircleSegment((CircleCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ClosestPointCircleRect((CircleCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ClosestPointCirclePoly((CircleCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ClosestPointCirclePoint((CircleCollider)a, b);
                }
            }
            else if (a is SegmentCollider)
            {
                if (b is CircleCollider)
                {
                    return ClosestPointSegmentCircle((SegmentCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ClosestPointSegmentSegment((SegmentCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ClosestPointSegmentRect((SegmentCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ClosestPointSegmentPoly((SegmentCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ClosestPointSegmentPoint((SegmentCollider)a, b);
                }
            }
            else if (a is RectCollider)
            {
                if (b is CircleCollider)
                {
                    return ClosestPointRectCircle((RectCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ClosestPointRectSegment((RectCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ClosestPointRectRect((RectCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ClosestPointRectPoly((RectCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ClosestPointRectPoint((RectCollider)a, b);
                }
            }
            else if (a is PolyCollider)
            {
                if (b is CircleCollider)
                {
                    return ClosestPointPolyCircle((PolyCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ClosestPointPolySegment((PolyCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ClosestPointPolyRect((PolyCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ClosestPointPolyPoly((PolyCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ClosestPointPolyPoint((PolyCollider)a, b);
                }
            }
            else
            {
                return a.Pos;
            }
        }

        //figure out way for multiple closest point to select the right one -> reference for closest of multiple points

        public static float SqDistPointSegment(Vector2 segA, Vector2 segB, Vector2 c)
        {
            Vector2 ab = segB - segA;
            Vector2 ac = c - segA;
            Vector2 bc = c - segB;
            float e = SVec.Dot(ac, ab);
            // Handle cases where c projects outside ab
            if (e <= 0.0f) return SVec.Dot(ac, ac);
            float f = SVec.Dot(ab, ab);
            if (e >= f) return SVec.Dot(bc, bc);
            // Handle cases where c projects onto ab
            return SVec.Dot(ac, ac) - (e * e) / f;
        }
        public static float SqDistPointRect(Vector2 p, Rectangle rect)
        {
            float sqDist = 0f;

            float xMin = rect.x;
            float xMax = rect.x + rect.width;
            float yMin = rect.y;
            float yMax = rect.y + rect.height;

            if (p.X < xMin) sqDist += (xMin - p.X) * (xMin - p.X);
            if (p.X > xMax) sqDist += (p.X - xMax) * (p.X - xMax);

            if (p.Y < rect.y) sqDist += (yMin - p.Y) * (yMin - p.Y);
            if (p.Y > rect.y) sqDist += (p.Y - yMax) * (p.Y - yMax);

            return sqDist;
        }
        
        public static Vector2 ClosestPointCircleCircle(CircleCollider self, CircleCollider other)
        {
            return self.Pos + Vector2.Normalize(other.Pos - self.Pos) * self.Radius;
        }
        public static Vector2 ClosestPointCircleCircle(CircleCollider self, Vector2 otherPos)
        {
            return self.Pos + Vector2.Normalize(otherPos - self.Pos) * self.Radius;
        }
        public static Vector2 ClosestPointCircleCircle(Vector2 pos, float r, CircleCollider other)
        {
            return pos + Vector2.Normalize(other.Pos - pos) * r;
        }
        public static Vector2 ClosestPointCircleCircle(Vector2 selfPos, float selfR, Vector2 otherPos)
        {
            return selfPos + Vector2.Normalize(otherPos - selfPos) * selfR;
        }
        public static Vector2 ClosestPointCircleSegment(CircleCollider c, SegmentCollider s)
        {
            Vector2 p = ClosestPointSegmentCircle(s, c);

            Vector2 dir = p - c.Pos;
            return c.Pos + Vector2.Normalize(dir) * c.Radius;
        }
        public static Vector2 ClosestPointCircleRect(CircleCollider c, RectCollider r)
        {
            Vector2 p = ClosestPointRectCircle(r, c);
            Vector2 dir = p - c.Pos;
            return c.Pos + Vector2.Normalize(dir) * c.Radius;
        }
        public static Vector2 ClosestPointCirclePoly(CircleCollider c, PolyCollider poly)
        {
            Vector2 p = ClosestPointPolyCircle(poly, c);
            Vector2 dir = p - c.Pos;
            return c.Pos + Vector2.Normalize(dir) * c.Radius;
        }
        public static Vector2 ClosestPointCirclePoint(CircleCollider c, Collider point)
        {
            return ClosestPointPointCircle(point, c);
        }

        //point - line
        public static Vector2 ClosestPointLineCircle(Vector2 linePos, Vector2 lineDir, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = circlePos - linePos;
            float p = w.X * lineDir.Y - w.Y * lineDir.X;
            if (p < -circleRadius || p > circleRadius)
            {
                float t = lineDir.X * w.X + lineDir.Y * w.Y;
                return linePos + lineDir * t;
            }
            else
            {
                float qb = w.X * lineDir.X + w.Y * lineDir.Y;
                float qc = w.LengthSquared() - circleRadius * circleRadius;
                float qd = qb * qb - qc;
                float t = qb - MathF.Sqrt(qd);
                return linePos + lineDir * t;
            }
        }
        public static Vector2 ClosestPointLineCircle(Vector2 linePos, Vector2 lineDir, CircleCollider circle)
        {
            return ClosestPointLineCircle(linePos, lineDir, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPointPointLine(Vector2 point, Vector2 linePos, Vector2 lineDir)
        {
            Vector2 displacement = point - linePos;
            float t = lineDir.X * displacement.X + lineDir.Y * displacement.Y;
            return SVec.ProjectionPoint(linePos, lineDir, t);
        }
        public static Vector2 ClosestPointPointLine(Collider point, Vector2 linePos, Vector2 lineDir)
        {
            return ClosestPointPointLine(point.Pos, linePos, lineDir);
        }

        //point - ray
        public static Vector2 ClosestPointRayCircle(Vector2 rayPos, Vector2 rayDir, CircleCollider circle)
        {
            return ClosestPointRayCircle(rayPos, rayDir, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPointRayCircle(Vector2 rayPos, Vector2 rayDir, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = circlePos - rayPos;
            float p = w.X * rayDir.Y - w.Y * rayDir.X;
            float t1 = w.X * rayDir.X + w.Y * rayDir.Y;
            if (p < -circleRadius || p > circleRadius)
            {
                if (t1 < 0.0f) return rayPos;
                else return rayPos + rayDir * t1;
            }
            else
            {
                if (t1 < -circleRadius) return rayPos;
                else if (t1 < circleRadius)
                {
                    float qb = w.X * rayDir.X + w.Y * rayDir.Y;
                    float qc = w.LengthSquared() - circleRadius * circleRadius;
                    float qd = qb * qb - qc;
                    float t2 = qb - MathF.Sqrt(qd);
                    return rayPos + rayDir * t2;
                }
                else return rayPos + rayDir * t1;
            }
        }
        public static Vector2 ClosestPointPointRay(Vector2 point, Vector2 rayPos, Vector2 rayDir)
        {
            Vector2 displacement = point - rayPos;
            float t = rayDir.X * displacement.X + rayDir.Y * displacement.Y;
            return t < 0 ? rayPos : SVec.ProjectionPoint(rayPos, rayDir, t);
        }
        public static Vector2 ClosestPointPointRay(Collider point, Vector2 rayPos, Vector2 rayDir)
        {
            return ClosestPointPointRay(point.Pos, rayPos, rayDir);
        }

        //point - circle
        public static Vector2 ClosestPointPointCircle(Vector2 circlaAPos, float circlaARadius, Vector2 circleBPos, float circleBRadius)
        {
            return ClosestPointPointCircle(circlaAPos, circleBPos, circleBRadius);
        }
        public static Vector2 ClosestPointPointCircle(Vector2 point, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = point - circlePos;
            float t = circleRadius / w.Length();
            return circlePos + w * t;
        }
        public static Vector2 ClosestPointPointCircle(CircleCollider a, Vector2 circlePos, float circleRadius)
        {
            return ClosestPointPointCircle(a.Pos, circlePos, circleRadius);
        }
        public static Vector2 ClosestPointPointCircle(CircleCollider a, CircleCollider b)
        {
            return ClosestPointPointCircle(a.Pos, b.Pos, b.Radius);
        }
        public static Vector2 ClosestPointPointCircle(Collider point, CircleCollider circle)
        {
            return ClosestPointPointCircle(point.Pos, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPointPointCircle(Vector2 point, CircleCollider circle)
        {
            return ClosestPointPointCircle(point, circle.Pos, circle.Radius);
        }

        //point - segment
        public static Vector2 ClosestPointSegmentPoint(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 point)
        {
            Vector2 displacment = point - segmentPos;
            float t = SVec.ProjectionTime(displacment, segmentDir * segmentLength);
            if (t < 0.0f) return segmentPos;
            else if (t > 1.0f) return segmentPos + segmentDir * segmentLength;
            else return SVec.ProjectionPoint(segmentPos, segmentDir * segmentLength, t);
        }
        public static Vector2 ClosestPointSegmentPoint(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
        {
            Vector2 segmentDir = segmentEnd - segmentStart;
            float segmentLength = segmentDir.Length();
            return ClosestPointSegmentPoint(segmentStart, segmentDir / segmentLength, segmentLength, point);

        }
        public static Vector2 ClosestPointSegmentPoint(SegmentCollider segment, Vector2 point)
        {
            return ClosestPointSegmentPoint(segment.Pos, segment.Dir, segment.Length, point);
        }
        public static Vector2 ClosestPointSegmentPoint(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Collider point)
        {
            return ClosestPointSegmentPoint(segmentPos, segmentDir, segmentLength, point.Pos);
        }
        public static Vector2 ClosestPointSegmentPoint(SegmentCollider segment, Collider point)
        {
            return ClosestPointSegmentPoint(segment.Pos, segment.Dir, segment.Length, point.Pos);
        }

        //segment - circle
        public static Vector2 ClosestPointSegmentCircle(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 circlePos, float circleRadius)
        {
            return ClosestPointSegmentPoint(segmentPos, segmentDir, segmentLength, circlePos);

            //Vector2 sv = segmentDir * segmentLength;
            //Vector2 w = circlePos - segmentPos;
            //float p = w.X * segmentDir.Y - w.Y * segmentDir.X;
            //float qa = sv.LengthSquared();
            //float t1 = (w.X * sv.X + w.Y * sv.Y) / qa;
            //if (p < -circleRadius || p > circleRadius)
            //{
            //    if (t1 < 0.0f) return segmentPos;
            //    else if (t1 > 1.0f) return segmentPos + sv;
            //    else return segmentPos + sv * t1;
            //}
            //else
            //{
            //    float qb = w.X * sv.X + w.Y * sv.Y;
            //    float qc = w.LengthSquared() - circleRadius * circleRbadius;
            //    float qd = qb * qb - qc * qa;
            //    float t2 = (qb + MathF.Sqrt(qd)) / qa;
            //    if (t2 < 0.0f) return segmentPos;
            //    else if (t2 < 1.0f) return segmentPos + sv * t2;
            //    else
            //    {
            //        float t3 = (qb - MathF.Sqrt(qd)) / qa;
            //        if (t3 < 1.0f) return segmentPos + sv * t3;
            //        else return segmentPos + sv;
            //    }
            //}
        }
        public static Vector2 ClosestPointSegmentCircle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 circlePos, float circleRadius)
        {
            Vector2 segmentDir = segmentEnd - segmentStart;
            float segmentLength = segmentDir.Length();

            return ClosestPointSegmentCircle(segmentStart, segmentDir / segmentLength, segmentLength, circlePos, circleRadius);
        }
        public static Vector2 ClosestPointSegmentCircle(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, CircleCollider circle)
        {
            return ClosestPointSegmentCircle(segmentPos, segmentDir, segmentLength, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPointSegmentCircle(SegmentCollider segment, Vector2 circlePos, float circleRadius)
        {
            return ClosestPointSegmentCircle(segment.Pos, segment.Dir, segment.Length, circlePos, circleRadius);
        }
        public static Vector2 ClosestPointSegmentCircle(SegmentCollider segment, CircleCollider circle)
        {
            return ClosestPointSegmentCircle(segment.Pos, segment.Dir, segment.Length, circle.Pos, circle.Radius);
        }


        public static Vector2 ClosestPointSegmentSegment(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            var info = SGeometry.IntersectSegmentSegmentInfo(aStart, aEnd, bStart, bEnd);
            if (info.intersected) return info.intersectPoint;


            Vector2 b1 = ClosestPointSegmentPoint(aStart, aEnd, bStart);
            Vector2 a1 = ClosestPointSegmentPoint(bStart, bEnd, b1);
            float disSq1 = (b1 - a1).LengthSquared();
            Vector2 b2 = ClosestPointSegmentPoint(aStart, aEnd, bEnd);
            Vector2 a2 = ClosestPointSegmentPoint(bStart, bEnd, b2);
            float disSq2 = (b2 - a2).LengthSquared();

            return disSq1 <= disSq2 ? b1 : b2;
        }
        public static (Vector2 p, float disSq) ClosestPointSegmentSegment2(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            var info = SGeometry.IntersectSegmentSegmentInfo(aStart, aEnd, bStart, bEnd);
            if (info.intersected) return (info.intersectPoint, 0f);

            Vector2 b1 = ClosestPointSegmentPoint(aStart, aEnd, bStart);
            Vector2 a1 = ClosestPointSegmentPoint(bStart, bEnd, b1);
            float disSq1 = (b1 - a1).LengthSquared();
            Vector2 b2 = ClosestPointSegmentPoint(aStart, aEnd, bEnd);
            Vector2 a2 = ClosestPointSegmentPoint(bStart, bEnd, b2);
            float disSq2 = (b2 - a2).LengthSquared();

            return disSq1 <= disSq2 ? (b1, disSq1) : (b2, disSq2);
        }
        public static Vector2 ClosestPointSegmentSegment(SegmentCollider a, SegmentCollider b)
        {
            return ClosestPointSegmentSegment(a.Pos, a.End, b.Pos, b.End);
        }
        public static Vector2 ClosestPointSegmentRect(Vector2 segmentStart, Vector2 segmentEnd, Rectangle rect)
        {
            List<Vector2> closestPoints = new();
            var segments = SRect.GetRectSegments(rect);
            float minDisSq = float.PositiveInfinity;
            foreach (var seg in segments)
            {
                var info = ClosestPointSegmentSegment2(segmentStart, segmentEnd, seg.start, seg.end);
                if (info.disSq < minDisSq)
                {
                    closestPoints.Clear();
                    closestPoints.Add(info.p);
                    minDisSq = info.disSq;
                }
                else if (info.disSq == minDisSq)
                {
                    closestPoints.Add(info.p);
                }
            }
            return closestPoints[0];
        }
        public static (Vector2 p, float disSq) ClosestPointSegmentRect2(Vector2 segmentStart, Vector2 segmentEnd, Rectangle rect)
        {
            List<(Vector2 p, float disSq)> closestPoints = new();
            var segments = SRect.GetRectSegments(rect);
            float minDisSq = float.PositiveInfinity;
            foreach (var seg in segments)
            {
                var info = ClosestPointSegmentSegment2(segmentStart, segmentEnd, seg.start, seg.end);
                if (info.disSq < minDisSq)
                {
                    closestPoints.Clear();
                    closestPoints.Add(info);
                    minDisSq = info.disSq;
                }
                else if (info.disSq == minDisSq)
                {
                    closestPoints.Add(info);
                }
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointSegmentRect(SegmentCollider s, RectCollider r)
        {
            return ClosestPointSegmentRect(s.Pos, s.End, r.Rect);
        }
        public static Vector2 ClosestPointSegmentPoly(SegmentCollider s, PolyCollider pc)
        {
            List<Vector2> poly = pc.Shape;
            List<Vector2> closestPoints = new();
            Vector2 segmentStart = s.Pos;
            Vector2 segmentEnd = s.End;
            float minDisSq = float.PositiveInfinity;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var info = ClosestPointSegmentSegment2(segmentStart, segmentEnd, start, end);
                if (info.disSq < minDisSq)
                {
                    minDisSq = info.disSq;
                    closestPoints.Clear();
                    closestPoints.Add(info.p);
                }
                else if (info.disSq == minDisSq) closestPoints.Add(info.p);
            }
            return closestPoints[0];
        }

        public static Vector2 ClosestPointRectPoint(Rectangle rect, Vector2 point)
        {
            if (SContains.ContainsRectPoint(rect, point))
            {
                float difX = point.X - (rect.x + rect.width / 2);
                float difY = point.Y - (rect.y + rect.height / 2);
                if (MathF.Abs(difX) >= MathF.Abs(difY))//inside
                {
                    if (difX <= 0)
                    {
                        return new(rect.x, point.Y);
                    }
                    else
                    {
                        return new(rect.x + rect.width, point.Y);
                    }
                }
                else
                {
                    if (difY <= 0)
                    {
                        return new(point.X, rect.y);
                    }
                    else
                    {
                        return new(point.X, rect.y + rect.height);
                    }
                }
            }
            else // outside
            {
                float x = 0f;
                float y = 0f;
                x = Clamp(point.X, rect.x, rect.x + rect.width);
                y = Clamp(point.Y, rect.y, rect.y + rect.height);
                return new(x, y);
            }
        }
        public static Vector2 ClosestPointRectPoint(RectCollider r, Collider point)
        {
            return ClosestPointRectPoint(r.Rect, point.Pos);
        }
        public static Vector2 ClosestPointRectCircle(Rectangle rect, Vector2 circlePos, float radius)
        {
            return ClosestPointRectPoint(rect, circlePos);
            //var segments = SRect.GetRectSegments(rect);
            //float minDisSq = float.PositiveInfinity;
            //Vector2 closestPoint = circlePos;
            //foreach (var seg in segments)
            //{
            //    var p = ClosestPointSegmentCircle(seg.start, seg.end, circlePos, radius);
            //    float disSq = (circlePos - p).LengthSquared();
            //    if (disSq < minDisSq)
            //    {
            //        minDisSq = disSq;
            //        closestPoint = p;
            //    }
            //}
            //return closestPoint;
        }
        public static Vector2 ClosestPointRectCircle(RectCollider r, CircleCollider c)
        {
            return ClosestPointRectCircle(r.Rect, c.Pos, c.Radius);
        }
        public static Vector2 ClosestPointRectSegment(Rectangle rect, Vector2 segmentStart, Vector2 segmentEnd)
        {
            List<Vector2> closestPoints = new();
            var segments = SRect.GetRectSegments(rect);
            float minDisSq = float.PositiveInfinity;
            foreach (var seg in segments)
            {
                var info = ClosestPointSegmentSegment2(seg.start, seg.end, segmentStart, segmentEnd);
                if (info.disSq < minDisSq)
                {
                    closestPoints.Clear();
                    closestPoints.Add(info.p);
                    minDisSq = info.disSq;
                }
                else if (info.disSq == minDisSq)
                {
                    closestPoints.Add(info.p);
                }
            }
            return closestPoints[0];
        }
        public static (Vector2 p, float disSq) ClosestPointRectSegment2(Rectangle rect, Vector2 segmentStart, Vector2 segmentEnd)
        {
            List<(Vector2 point, float dis)> closestPoints = new();
            var segments = SRect.GetRectSegments(rect);
            float minDisSq = float.PositiveInfinity;
            foreach (var seg in segments)
            {
                var info = ClosestPointSegmentSegment2(seg.start, seg.end, segmentStart, segmentEnd);
                if (info.disSq < minDisSq)
                {
                    closestPoints.Clear();
                    closestPoints.Add(info);
                    minDisSq = info.disSq;
                }
                else if (info.disSq == minDisSq)
                {
                    closestPoints.Add(info);
                }
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointRectSegment(RectCollider r, SegmentCollider s)
        {
            return ClosestPointRectSegment(r.Rect, s.Pos, s.End);
        }
        public static Vector2 ClosestPointRectRect(Rectangle a, Rectangle b)
        {
            Vector2 aPos = new(a.x + a.width / 2, a.y + a.height / 2);
            Vector2 bPos = new(b.x + b.width / 2, b.y + b.height / 2);
            float aR = (aPos - new Vector2(a.x, a.y)).Length();
            //float bR = (bPos - new Vector2(b.x, b.y)).Length();
            Vector2 cp = ClosestPointCircleCircle(aPos, aR, bPos);
            return ClosestPointRectPoint(a, cp);
            //return ClosestPointRectCircle(a, bPos, bR);
        }
        public static Vector2 ClosestPointRectRect(RectCollider a, RectCollider b)
        {
            return ClosestPointRectRect(a.Rect, b.Rect);
        }
        public static Vector2 ClosestPointRectPoint(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 point)
        {
            return ClosestPointRectPoint(SRect.ConstructRect(pos, size, alignement), point);
        }
        public static Vector2 ClosestPointRectCircle(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 circlePos, float radius)
        {
            return ClosestPointRectCircle(SRect.ConstructRect(pos, size, alignement), circlePos, radius);
        }

        public static Vector2 ClosestPointRectSegment(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 segmentStart, Vector2 segmentEnd)
        {
            return ClosestPointRectSegment(SRect.ConstructRect(pos, size, alignement), segmentStart, segmentEnd);
        }
        public static Vector2 ClosestPointRectRect(Vector2 pos, Vector2 size, Vector2 alignement, Rectangle rect)
        {
            return ClosestPointRectRect(SRect.ConstructRect(pos, size, alignement), rect);
        }
        public static Vector2 ClosestPointRectRect(Vector2 aPos, Vector2 aSize, Vector2 aAlignement, Vector2 bPos, Vector2 bSize, Vector2 bAlignement)
        {
            return ClosestPointRectRect(SRect.ConstructRect(aPos, aSize, aAlignement), SRect.ConstructRect(bPos, bSize, bAlignement));
        }
        public static Vector2 ClosestPointRectPoint(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 point)
        {
            var poly = SRect.RotateRectList(rect, pivot, angleDeg);
            return ClosestPointPolyPoint(poly, point);
        }
        public static Vector2 ClosestPointRectCircle(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 circlePos, float radius)
        {
            var poly = SRect.RotateRectList(rect, pivot, angleDeg);
            return ClosestPointPolyCircle(poly, circlePos, radius);
        }
        public static Vector2 ClosestPointRectSegment(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 segmentStart, Vector2 segmentEnd)
        {
            var poly = SRect.RotateRectList(rect, pivot, angleDeg);
            return ClosestPointPolySegment(poly, segmentStart, segmentEnd);
        }
        public static Vector2 ClosestPointRectRect(Rectangle rect, Vector2 pivot, float angleDeg, Rectangle b)
        {
            var poly = SRect.RotateRectList(rect, pivot, angleDeg);
            return ClosestPointPolyRect(poly, b);
        }
        public static Vector2 ClosestPointRectRect(Rectangle a, Vector2 aPivot, float aAngleDeg, Rectangle b, Vector2 bPivot, float bAngleDeg)
        {
            var polyA = SRect.RotateRectList(a, aPivot, aAngleDeg);
            var polyB = SRect.RotateRectList(b, bPivot, bAngleDeg);
            return ClosestPointPolyPoly(polyA, polyB);
        }
        public static Vector2 ClosestPointRectPoly(Rectangle rect, List<Vector2> poly)
        {
            Vector2 rectPos = new(rect.x, rect.y);
            if (poly.Count < 2) return rectPos;
            else if (poly.Count < 3) return ClosestPointRectSegment(rect, poly[0], poly[1]);
            List<Vector2> closestPoints = new();
            float minDisSq = float.PositiveInfinity;

            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var info = ClosestPointRectSegment2(rect, start, end);
                if (info.disSq < minDisSq)
                {
                    minDisSq = info.disSq;
                    closestPoints.Add(info.p);
                }
                else if (info.disSq == minDisSq)
                {
                    closestPoints.Add(info.p);
                }
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointRectPoly(RectCollider r, PolyCollider poly)
        {
            return ClosestPointRectPoly(r.Rect, poly.Shape);
        }

        public static Vector2 ClosestPointPolyPoint(PolyCollider poly, Collider point)
        {
            return ClosestPointPolyPoint(poly.Shape, point.Pos);
        }
        public static Vector2 ClosestPointPolyPoint(List<Vector2> poly, Vector2 point)
        {
            float minDisSq = float.PositiveInfinity;
            Vector2 closestPoint = point;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var p = ClosestPointSegmentPoint(start, end, point);
                float disSq = (point - p).LengthSquared();
                if (disSq < minDisSq)
                {
                    minDisSq = disSq;
                    closestPoint = p;
                }
            }
            return closestPoint;
        }
        public static Vector2 ClosestPointPolyCircle(List<Vector2> poly, Vector2 circlePos, float radius)
        {
            return ClosestPointPolyPoint(poly, circlePos);
            //float minDisSq = float.PositiveInfinity;
            //Vector2 closestPoint = circlePos;
            //for (int i = 0; i < poly.Count; i++)
            //{
            //    Vector2 start = poly[i];
            //    Vector2 end = poly[(i + 1) % poly.Count];
            //    var p = ClosestPointSegmentCircle(start, end, circlePos, radius);
            //    float disSq = (circlePos - p).LengthSquared();
            //    if (disSq < minDisSq)
            //    {
            //        minDisSq = disSq;
            //        closestPoint = p;
            //    }
            //}
            //return closestPoint;
        }
        public static Vector2 ClosestPointPolyCircle(PolyCollider poly, CircleCollider c)
        {
            return ClosestPointPolyCircle(poly.Shape, c.Pos, c.Radius);
        }
        public static Vector2 ClosestPointPolySegment(List<Vector2> poly, Vector2 segmentStart, Vector2 segmentEnd)
        {
            List<Vector2> closestPoints = new();
            float minDisSq = float.PositiveInfinity;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var info = ClosestPointSegmentSegment2(start, end, segmentStart, segmentEnd);
                if (info.disSq < minDisSq)
                {
                    minDisSq = info.disSq;
                    closestPoints.Clear();
                    closestPoints.Add(info.p);
                }
                else if (info.disSq == minDisSq) closestPoints.Add(info.p);
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointPolySegment(PolyCollider poly, SegmentCollider s)
        {
            return ClosestPointPolySegment(poly.Shape, s.Pos, s.End);
        }
        public static Vector2 ClosestPointPolyRect(List<Vector2> poly, Rectangle rect)
        {
            List<Vector2> closestPoints = new();
            float minDisSq = float.PositiveInfinity;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var info = ClosestPointSegmentRect2(start, end, rect);
                if (info.disSq < minDisSq)
                {
                    minDisSq = info.disSq;
                    closestPoints.Clear();
                    closestPoints.Add(info.p);
                }
                else if (info.disSq == minDisSq) closestPoints.Add(info.p);
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointPolyRect(PolyCollider poly, RectCollider r)
        {
            return ClosestPointPolyRect(poly.Shape, r.Rect);
        }
        public static Vector2 ClosestPointPolyPoly(List<Vector2> a, List<Vector2> b)
        {
            if (a.Count < 3 || b.Count < 3) return new(0f);
            List<Vector2> closestPoints = new();
            float minDisSq = float.PositiveInfinity;
            for (int i = 0; i < a.Count; i++)
            {
                Vector2 aStart = a[i];
                Vector2 aEnd = a[(i + 1) % a.Count];
                for (int j = 0; j < b.Count; j++)
                {
                    Vector2 bStart = b[j];
                    Vector2 bEnd = b[(j + 1) % b.Count];
                    var info = ClosestPointSegmentSegment2(aStart, aEnd, bStart, bEnd);
                    if (info.disSq < minDisSq)
                    {
                        minDisSq = info.disSq;
                        closestPoints.Clear();
                        closestPoints.Add(info.p);
                    }
                    else if (info.disSq == minDisSq) closestPoints.Add(info.p);
                }
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointPolyPoly(PolyCollider a, PolyCollider b)
        {
            return ClosestPointPolyPoly(a.Shape, b.Shape);
        }

    }
}
