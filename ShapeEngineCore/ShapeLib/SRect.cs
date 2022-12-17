using Raylib_CsLo;
using System.Numerics;

namespace ShapeLib
{
    public class SRect
    {
        public static Rectangle MultiplyRectangle(Rectangle rect, float factor)
        {
            return new Rectangle
                (
                    rect.x * factor,
                    rect.y * factor,
                    rect.width * factor,
                    rect.height * factor
                );
        }
        public static Rectangle MultiplyRectangle(Rectangle a, Vector2 factor)
        {
            return new Rectangle
                (
                    a.x * factor.X,
                    a.y * factor.Y,
                    a.width * factor.X,
                    a.height * factor.Y
                );
        }
        public static Rectangle MultiplyRectangle(Rectangle a, Rectangle b)
        {
            return new Rectangle
                (
                    a.x * b.x,
                    a.y * b.y,
                    a.width * b.width,
                    a.height * b.height
                );
        }
        
        public static Rectangle ScaleRectangle(Rectangle rect, float scale, Vector2 pivot)
        {
            float newWidth = rect.width * scale;
            float newHeight = rect.height * scale;
            return new Rectangle(rect.x - (newWidth - rect.width) * pivot.X, rect.y - (newHeight - rect.height) * pivot.Y, newWidth, newHeight);
        }
        public static Rectangle ScaleRectangle(Rectangle rect, float scale)
        {
            float newWidth = rect.width * scale;
            float newHeight = rect.height * scale;
            return new Rectangle(rect.x - (newWidth - rect.width) / 2, rect.y - (newHeight - rect.height) / 2, newWidth, newHeight);
        }


        public static Vector2 GetRectCorner(Rectangle r, int corner)
        {
            return GetRectCornersList(r)[corner % 4];
        }
        public static Vector2 GetRectCorner(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, int corner)
        {
            Rectangle rect = ConstructRect(rectPos, rectSize, rectAlignement);
            return GetRectCorner(rect, corner);
            //Vector2 tl = new(rect.x, rect.y);
            //Vector2 s = new(rect.width, rect.height);
            //switch (corner % 4)
            //{
            //    case 0: tl.X += s.X; break;   //tr
            //    case 1: tl += s; break;       //br
            //    case 2: tl.Y += s.Y; break;   //bl
            //    default: break;           //tl
            //}
            //return tl;
        }
        public static bool SeperateAxisRect(Vector2 axisStart, Vector2 axisEnd, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement)
        {
            Vector2 n = axisStart - axisEnd;
            Vector2 edgeAStart = GetRectCorner(rectPos, rectSize, rectAlignement, 0);
            Vector2 edgeAEnd = GetRectCorner(rectPos, rectSize, rectAlignement, 1);
            Vector2 edgeBStart = GetRectCorner(rectPos, rectSize, rectAlignement, 2);
            Vector2 edgeBEnd = GetRectCorner(rectPos, rectSize, rectAlignement, 3);

            RangeFloat edgeARange = ProjectSegment(edgeAStart, edgeAEnd, n);
            RangeFloat edgeBRange = ProjectSegment(edgeBStart, edgeBEnd, n);
            RangeFloat rProjection = RangeHull(edgeARange, edgeBRange);

            RangeFloat axisRange = ProjectSegment(axisStart, axisEnd, n);
            return !OverlappingRange(axisRange, rProjection);
        }

        public static Rectangle EnlargeRect(Rectangle r, Vector2 p)
        {
            return new
                (
                    MathF.Min(r.X, p.X),
                    MathF.Min(r.Y, p.Y),
                    MathF.Max(r.X + r.width, p.X),
                    MathF.Max(r.Y + r.height, p.Y)
                );
        }
        public static Rectangle EnlargeRect(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, Vector2 point)
        {
            Rectangle r = ConstructRect(rectPos, rectSize, rectAlignement);
            return EnlargeRect(r, point);

        }

        public static Vector2 ClampOnRect(Vector2 p, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement)
        {
            return ClampOnRect(p, ConstructRect(rectPos, rectSize, rectAlignement));
        }
        public static Vector2 ClampOnRect(Vector2 p, Rectangle rect)
        {
            return new
                (
                    RayMath.Clamp(p.X, rect.X, rect.X + rect.width),
                    RayMath.Clamp(p.Y, rect.Y, rect.Y + rect.height)
                );
        }
        public static Rectangle ConstructRect(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            Vector2 offset = size * alignement;
            Vector2 topLeft = pos - offset;
            return new
                (
                    topLeft.X,
                    topLeft.Y,
                    size.X,
                    size.Y
                );
        }

        public static List<Vector2> RotateRectList(Rectangle rect, Vector2 pivot, float angleDeg)
        {
            float rotRad = angleDeg * RayMath.DEG2RAD;
            Vector2 size = new Vector2(rect.width, rect.height);
            Vector2 pivotPoint = new Vector2(rect.x, rect.y) + size * pivot;

            Vector2 tl = new Vector2(rect.X, rect.Y);
            Vector2 br = tl + size;
            Vector2 tr = tl + new Vector2(rect.width, 0);
            Vector2 bl = tl + new Vector2(0, rect.height);

            Vector2 topLeft = pivotPoint + SVec.Rotate(tl - pivotPoint, rotRad);
            Vector2 topRight = pivotPoint + SVec.Rotate(tr - pivotPoint, rotRad);
            Vector2 bottomRight = pivotPoint + SVec.Rotate(br - pivotPoint, rotRad);
            Vector2 bottomLeft = pivotPoint + SVec.Rotate(bl - pivotPoint, rotRad);

            return new() { topLeft, topRight, bottomRight, bottomLeft };
        }
        public static List<Vector2> RotateRectList(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 pivot, float angleDeg)
        {
            return RotateRectList(ConstructRect(pos, size, alignement), pivot, angleDeg);
        }

        public static (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) RotateRect(Rectangle rect, Vector2 pivot, float angleDeg)
        {
            float rotRad = angleDeg * RayMath.DEG2RAD;
            Vector2 size = new Vector2(rect.width, rect.height);
            Vector2 pivotPoint = new Vector2(rect.x, rect.y) + size * pivot;

            Vector2 tl = new Vector2(rect.X, rect.Y);
            Vector2 br = tl + size;
            Vector2 tr = tl + new Vector2(rect.width, 0);
            Vector2 bl = tl + new Vector2(0, rect.height);

            Vector2 topLeft = pivotPoint + SVec.Rotate(tl - pivotPoint, rotRad);
            Vector2 topRight = pivotPoint + SVec.Rotate(tr - pivotPoint, rotRad);
            Vector2 bottomRight = pivotPoint + SVec.Rotate(br - pivotPoint, rotRad);
            Vector2 bottomLeft = pivotPoint + SVec.Rotate(bl - pivotPoint, rotRad);

            return new(topLeft, topRight, bottomRight, bottomLeft);
        }
        public static (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) RotateRect(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 pivot, float angleDeg)
        {
            return RotateRect(ConstructRect(pos, size, alignement), pivot, angleDeg);
        }

        public static (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetRectCorners(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            return GetRectCorners(ConstructRect(pos, size, alignement));
        }
        public static (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetRectCorners(Rectangle rect)
        {
            Vector2 tl = new(rect.x, rect.y);
            Vector2 tr = new(rect.x + rect.width, rect.y);
            Vector2 bl = new(rect.x, rect.y + rect.height);
            Vector2 br = new(rect.x + rect.width, rect.y + rect.height);
            return (tl, tr, br, bl);
        }
        public static List<Vector2> GetRectCornersList(Rectangle rect)
        {
            Vector2 tl = new(rect.x, rect.y);
            Vector2 tr = new(rect.x + rect.width, rect.y);
            Vector2 bl = new(rect.x, rect.y + rect.height);
            Vector2 br = new(rect.x + rect.width, rect.y + rect.height);
            return new() { tl, tr, br, bl };
        }
        public static List<(Vector2 start, Vector2 end)> GetRectSegments(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            return GetRectSegments(ConstructRect(pos, size, alignement));
        }
        public static List<(Vector2 start, Vector2 end)> GetRectSegments(Rectangle rect)
        {
            var c = GetRectCorners(rect);
            return GetRectSegments(c.tl, c.tr, c.br, c.bl);
        }
        public static List<(Vector2 start, Vector2 end)> GetRectSegments(Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl)
        {
            List<(Vector2 start, Vector2 end)> segments = new()
            {
                (tl, tr), (bl, br), (tl, bl), (tr, br)
            };

            return segments;
        }






        public static bool OverlappingRange(float minA, float maxA, float minB, float maxB)
        {
            if (maxA < minA)
            {
                float temp = minA;
                minA = maxA;
                maxA = temp;
            }
            if (maxB < minB)
            {
                float temp = minB;
                minB = maxB;
                maxB = temp;
            }
            //return minA < maxB && maxA > minB;
            return minB <= maxA && minA <= maxB;
        }
        public static bool OverlappingRange(RangeFloat a, RangeFloat b)
        {
            return OverlappingRange(a.min, a.max, b.min, b.max);
        }
        private static RangeFloat RangeHull(RangeFloat a, RangeFloat b)
        {
            return new
                (
                    a.min < b.min ? a.min : b.min,
                    a.max > b.max ? a.max : b.max
                );
        }
        public static RangeFloat ProjectSegment(Vector2 aPos, Vector2 aEnd, Vector2 onto)
        {
            Vector2 unitOnto = Vector2.Normalize(onto);
            RangeFloat r = new(SVec.Dot(unitOnto, aPos), SVec.Dot(unitOnto, aEnd));
            return r;
        }
        public static bool SegmentOnOneSide(Vector2 axisPos, Vector2 axisDir, Vector2 segmentPos, Vector2 segmentEnd)
        {
            Vector2 d1 = segmentPos - axisPos;
            Vector2 d2 = segmentEnd - axisPos;
            Vector2 n = SVec.Rotate90CCW(axisDir);// new(-axisDir.Y, axisDir.X);
            return Vector2.Dot(n, d1) * Vector2.Dot(n, d2) > 0.0f;
        }

    }
}