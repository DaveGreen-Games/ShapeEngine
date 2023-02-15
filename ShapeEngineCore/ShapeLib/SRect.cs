using Raylib_CsLo;
using System.Numerics;

namespace ShapeLib
{
    public class SRect
    {
        public static (bool collided, Vector2 hitPoint, Vector2 n, Vector2 newPos) CollidePlayfield(Rectangle playfieldRect, Vector2 objPos, float objRadius)
        {
            bool collided = false;
            Vector2 hitPoint = objPos;
            Vector2 n = new(0f, 0f);
            Vector2 newPos = objPos;
            if (objPos.X + objRadius > playfieldRect.x + playfieldRect.width)
            {
                hitPoint = new(playfieldRect.x + playfieldRect.width, objPos.Y);
                newPos.X = hitPoint.X - objRadius;
                n = new(-1, 0);
                collided = true;
            }
            else if (objPos.X - objRadius < playfieldRect.x)
            {
                hitPoint = new(playfieldRect.x, objPos.Y);
                newPos.X = hitPoint.X + objRadius;
                n = new(1, 0);
                collided = true;
            }

            if (objPos.Y + objRadius > playfieldRect.Y + playfieldRect.height)
            {
                hitPoint = new(objPos.X, playfieldRect.Y + playfieldRect.height);
                newPos.Y = hitPoint.Y - objRadius;
                n = new(0, -1);
                collided = true;
            }
            else if (objPos.Y - objRadius < playfieldRect.y)
            {
                hitPoint = new(objPos.X, playfieldRect.Y);
                newPos.Y = hitPoint.Y + objRadius;
                n = new(0, 1);
                collided = true;
            }

            return (collided, hitPoint, n, newPos);
        }
        public static (bool outOfBounds, Vector2 newPos) WrapAroundPlayfield(Rectangle playfieldRect, Vector2 objPos, float objRadius)
        {
            bool outOfBounds = false;
            Vector2 newPos = objPos;
            if (objPos.X + objRadius > playfieldRect.x + playfieldRect.width)
            {
                newPos = new(playfieldRect.x, objPos.Y);
                outOfBounds = true;
            }
            else if (objPos.X - objRadius < playfieldRect.x)
            {
                newPos = new(playfieldRect.x + playfieldRect.width, objPos.Y);
                outOfBounds = true;
            }

            if (objPos.Y + objRadius > playfieldRect.Y + playfieldRect.height)
            {
                newPos = new(objPos.X, playfieldRect.Y);
                outOfBounds = true;
            }
            else if (objPos.Y - objRadius < playfieldRect.y)
            {
                newPos = new(objPos.X, playfieldRect.Y + playfieldRect.height);
                outOfBounds = true;
            }

            return (outOfBounds, newPos);
        }

        /// <summary>
        /// Construct 9 rects out of an outer and inner rect.
        /// </summary>
        /// <param name="inner">The inner rect. Has to be inside of the outer rect.</param>
        /// <param name="outer">The outer rect. Has to be bigger than the inner rect.</param>
        /// <returns>A list of rectangle in the order [TL,TC,TR,LC,C,RC,BL,BC,BR].</returns>
        public static List<Rectangle> GetNineTiles(Rectangle inner, Rectangle outer)
        {
            List<Rectangle> tiles = new();

            //topLeft
            Vector2 tl0 = new(outer.x, outer.y);
            Vector2 br0 = new(inner.x, inner.y);
            
            //topCenter
            Vector2 tl1 = new(inner.x, outer.y);
            Vector2 br1 = new(inner.x + inner.width, inner.y);
            
            //topRight
            Vector2 tl2 = new(inner.x + inner.width, outer.y);
            Vector2 br2 = new(outer.x + outer.width, inner.y);
           
            //rightCenter
            Vector2 tl3 = br1;
            Vector2 br3 = new(outer.x + outer.width, inner.y + inner.height);
            
            //bottomRight
            Vector2 tl4 = new(inner.x + inner.width, inner.y + inner.height);
            Vector2 br4 = new(outer.x + outer.width, outer.y + outer.height);
            
            //bottomCenter
            Vector2 tl5 = new(inner.x, inner.y + inner.height);
            Vector2 br5 = new(inner.x + inner.width, outer.y + outer.height);
            
            //bottomLeft
            Vector2 tl6 = new(outer.x, inner.y + inner.height);
            Vector2 br6 = new(inner.x, outer.y + outer.height);
            
            //leftCenter
            Vector2 tl7 = new(outer.x, inner.y);
            Vector2 br7 = tl5;
            
            tiles.Add(ConstructRect(tl0, br0));//topLeft
            tiles.Add(ConstructRect(tl1, br1));//topCenter
            tiles.Add(ConstructRect(tl2, br2));//topRight
            tiles.Add(ConstructRect(tl7, br7));//leftCenter
            tiles.Add(inner);
            tiles.Add(ConstructRect(tl3, br3));//rightCenter
            tiles.Add(ConstructRect(tl6, br6));//bottomLeft
            tiles.Add(ConstructRect(tl5, br5));//bottomCenter
            tiles.Add(ConstructRect(tl4, br4));//bottomRight

            return tiles;
        }


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
            Vector2 tl = new
                (
                    MathF.Min(r.X, p.X),
                    MathF.Min(r.Y, p.Y)
                );
            Vector2 br = new
                (
                    MathF.Max(r.X + r.width, p.X),
                    MathF.Max(r.Y + r.height, p.Y)
                );
            return new
                (
                  tl.X,
                  tl.Y,
                  br.X - tl.X,
                  br.Y - tl.Y
                    
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

        public static Rectangle ConstructRect(Vector2 topLeft, Vector2 bottomRight)
        {
            return new(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
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

        public static Vector2 GetRectPos(Rectangle rect, Vector2 alignement)
        {
            Vector2 topLeft = new Vector2(rect.X, rect.Y);
            Vector2 offset = GetRectSize(rect) * alignement;
            return topLeft + offset;
        }
        public static Vector2 GetRectSize(Rectangle rect)
        {
            return new(rect.width, rect.height);
        }
    }
}