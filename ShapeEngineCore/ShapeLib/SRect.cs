//using Raylib_CsLo;
using ShapeCore;
using System.Numerics;

namespace ShapeLib
{
    public static class SRect
    {
        public static Rect Lerp(this Rect from, Rect to, float f)
        {
            return
                new
                (
                    SUtils.LerpFloat(from.x, to.x, f),
                    SUtils.LerpFloat(from.y, to.y, f),
                    SUtils.LerpFloat(from.width, to.width, f),
                    SUtils.LerpFloat(from.height, to.height, f)
                );
        }
        public static List<Rect> AlignRectsHorizontal(this Rect rect, int count, float gapRelative = 0f, float maxElementSizeRel = 1f)
        {
            List<Rect> rects = new();
            Vector2 startPos = new(rect.x, rect.y);
            int gaps = count - 1;

            float totalWidth = rect.width;
            float gapSize = totalWidth * gapRelative;
            float elementWidth = (totalWidth - gaps * gapSize) / count;
            Vector2 offset = new(0f, 0f);
            for (int i = 0; i < count; i++)
            {
                Vector2 size = new(elementWidth, rect.height);
                Vector2 maxSize = maxElementSizeRel * new Vector2(rect.width, rect.height);
                if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
                if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
                Rect r = new(startPos + offset, size, new(0f));
                rects.Add(r);
                offset += new Vector2(gapSize + elementWidth, 0f);
            }
            return rects;
        }
        public static List<Rect> AlignRectsVertical(this Rect rect, int count, float gapRelative = 0f, float maxElementSizeRel = 1f)
        {
            List<Rect> rects = new();
            Vector2 startPos = new(rect.x, rect.y);
            int gaps = count - 1;

            float totalHeight = rect.height;
            float gapSize = totalHeight * gapRelative;
            float elementHeight = (totalHeight - gaps * gapSize) / count;
            Vector2 offset = new(0f, 0f);
            for (int i = 0; i < count; i++)
            {
                Vector2 size = new(rect.width, elementHeight);
                Vector2 maxSize = maxElementSizeRel * new Vector2(rect.width, rect.height);
                if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
                if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
                Rect r = new(startPos + offset, size, new(0f));
                rects.Add(r);
                offset += new Vector2(0, gapSize + size.Y);
            }
            return rects;
        }
        public static List<Rect> AlignRectsGrid(this Rect rect, int columns, int rows, int count, float hGapRelative = 0f, float vGapRelative = 0f, bool leftToRight = true)
        {
            List<Rect> rects = new();
            Vector2 startPos = new(rect.x, rect.y);

            int hGaps = columns - 1;
            float totalWidth = rect.width;
            float hGapSize = totalWidth * hGapRelative;
            float elementWidth = (totalWidth - hGaps * hGapSize) / columns;
            Vector2 hGap = new(hGapSize + elementWidth, 0);

            int vGaps = rows - 1;
            float totalHeight = rect.height;
            float vGapSize = totalHeight * vGapRelative;
            float elementHeight = (totalHeight - vGaps * vGapSize) / rows;
            Vector2 vGap = new(0, vGapSize + elementHeight);

            Vector2 elementSize = new(elementWidth, elementHeight);

            for (int i = 0; i < count; i++)
            {
                var coords = SUtils.TransformIndexToCoordinates(i, rows, columns, leftToRight);
                Rect r = new(startPos + hGap * coords.col + vGap * coords.row, elementSize, new(0f));
                rects.Add(r);
            }
            return rects;
        }



        public static (bool collided, Vector2 hitPoint, Vector2 n, Vector2 newPos) CollidePlayfield(this Rect playfieldRect, Vector2 objPos, float objRadius)
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

            if (objPos.Y + objRadius > playfieldRect.y + playfieldRect.height)
            {
                hitPoint = new(objPos.X, playfieldRect.y + playfieldRect.height);
                newPos.Y = hitPoint.Y - objRadius;
                n = new(0, -1);
                collided = true;
            }
            else if (objPos.Y - objRadius < playfieldRect.y)
            {
                hitPoint = new(objPos.X, playfieldRect.y);
                newPos.Y = hitPoint.Y + objRadius;
                n = new(0, 1);
                collided = true;
            }

            return (collided, hitPoint, n, newPos);
        }
        public static (bool outOfBounds, Vector2 newPos) WrapAroundPlayfield(this Rect playfieldRect, Vector2 objPos, float objRadius)
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

            if (objPos.Y + objRadius > playfieldRect.y + playfieldRect.height)
            {
                newPos = new(objPos.X, playfieldRect.y);
                outOfBounds = true;
            }
            else if (objPos.Y - objRadius < playfieldRect.y)
            {
                newPos = new(objPos.X, playfieldRect.y + playfieldRect.height);
                outOfBounds = true;
            }

            return (outOfBounds, newPos);
        }
        
        
        public static Rect AlignRect(this Rect rect, Vector2 alignement)
        {
            Vector2 topLeft = new Vector2(rect.x, rect.y);
            Vector2 size = new(rect.width, rect.height);
            Vector2 offset = size * alignement;
            return new
                (
                    topLeft.X + offset.X,
                    topLeft.Y + offset.Y,
                    size.X,
                    size.Y
                );
        }
        public static Rect ApplyMargins(this Rect rect, float left, float right, float top, float bottom)
        {
            Vector2 tl = new(rect.x, rect.y);
            Vector2 size = new(rect.width, rect.height);
            Vector2 br = tl + size;

            tl.X += size.X * left;
            tl.Y += size.Y * top;
            br.X -= size.X * right;
            br.Y -= size.Y * bottom;

            Vector2 finalTopLeft = new(MathF.Min(tl.X, br.X), MathF.Min(tl.Y, br.Y));
            Vector2 finalBottomRight = new(MathF.Max(tl.X, br.X), MathF.Max(tl.Y, br.Y));
            return new
                (
                    finalTopLeft.X,
                    finalTopLeft.Y,
                    finalBottomRight.X - finalTopLeft.X,
                    finalBottomRight.Y - finalTopLeft.Y
                );
        }
        
        
        /// <summary>
        /// Construct 9 rects out of an outer and inner rect.
        /// </summary>
        /// <param name="inner">The inner rect. Has to be inside of the outer rect.</param>
        /// <param name="outer">The outer rect. Has to be bigger than the inner rect.</param>
        /// <returns>A list of rectangle in the order [TL,TC,TR,LC,C,RC,BL,BC,BR].</returns>
        public static List<Rect> GetNineTiles(this Rect inner, Rect outer)
        {
            List<Rect> tiles = new();

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
            
            tiles.Add(new(tl0, br0));//topLeft
            tiles.Add(new(tl1, br1));//topCenter
            tiles.Add(new(tl2, br2));//topRight
            tiles.Add(new(tl7, br7));//leftCenter
            tiles.Add(inner);
            tiles.Add(new(tl3, br3));//rightCenter
            tiles.Add(new(tl6, br6));//bottomLeft
            tiles.Add(new(tl5, br5));//bottomCenter
            tiles.Add(new(tl4, br4));//bottomRight

            return tiles;
        }


        public static Rect MultiplyRectangle(this Rect rect, float factor)
        {
            return new Rect
                (
                    rect.x * factor,
                    rect.y * factor,
                    rect.width * factor,
                    rect.height * factor
                );
        }
        public static Rect MultiplyRectangle(this Rect a, Vector2 factor)
        {
            return new Rect
                (
                    a.x * factor.X,
                    a.y * factor.Y,
                    a.width * factor.X,
                    a.height * factor.Y
                );
        }
        public static Rect MultiplyRectangle(this Rect a, Rect b)
        {
            return new Rect
                (
                    a.x * b.x,
                    a.y * b.y,
                    a.width * b.width,
                    a.height * b.height
                );
        }
        
        public static Rect ScaleRectangle(this Rect rect, float scale, Vector2 pivot)
        {
            float newWidth = rect.width * scale;
            float newHeight = rect.height * scale;
            return new Rect(rect.x - (newWidth - rect.width) * pivot.X, rect.y - (newHeight - rect.height) * pivot.Y, newWidth, newHeight);
        }
        public static Rect ScaleRectangle(this Rect rect, float scale)
        {
            float newWidth = rect.width * scale;
            float newHeight = rect.height * scale;
            return new Rect(rect.x - (newWidth - rect.width) / 2, rect.y - (newHeight - rect.height) / 2, newWidth, newHeight);
        }


        public static Vector2 GetRectCorner(this Rect r, int corner)
        {
            return GetRectCornersList(r)[corner % 4];
        }
        public static Vector2 GetRectCorner(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, int corner)
        {
            Rect rect = new(rectPos, rectSize, rectAlignement);
            return GetRectCorner(rect, corner);
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

        public static Rect EnlargeRect(this Rect r, Vector2 p)
        {
            Vector2 tl = new
                (
                    MathF.Min(r.x, p.X),
                    MathF.Min(r.y, p.Y)
                );
            Vector2 br = new
                (
                    MathF.Max(r.x + r.width, p.X),
                    MathF.Max(r.y + r.height, p.Y)
                );
            return new
                (
                  tl.X,
                  tl.Y,
                  br.X - tl.X,
                  br.Y - tl.Y
                    
                );
        }
        public static Rect EnlargeRect(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, Vector2 point)
        {
            Rect r = new(rectPos, rectSize, rectAlignement);
            return EnlargeRect(r, point);

        }

        public static Vector2 ClampOnRect(Vector2 p, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement)
        {
            return ClampOnRect(p, new(rectPos, rectSize, rectAlignement));
        }
        public static Vector2 ClampOnRect(Vector2 p, Rect rect)
        {
            return new
                (
                    SUtils.Clamp(p.X, rect.x, rect.x + rect.width),
                    SUtils.Clamp(p.Y, rect.y, rect.y + rect.height)
                );
        }

        public static Rect Clamp(this Rect r, Rect bounds)
        {
            Vector2 tl = ClampOnRect(r.TopLeft, bounds);
            Vector2 br = ClampOnRect(r.BottomRight, bounds);
            return new(tl, br);
        }
        public static Rect Clamp(this Rect r, Vector2 min, Vector2 max)
        {
            return Clamp(r, new(min, max));
        }
       
        public static List<Vector2> RotateRectList(this Rect rect, Vector2 pivot, float angleDeg)
        {
            float rotRad = angleDeg * SUtils.DEGTORAD;
            Vector2 size = new Vector2(rect.width, rect.height);
            Vector2 pivotPoint = new Vector2(rect.x, rect.y) + size * pivot;

            Vector2 tl = new Vector2(rect.x, rect.y);
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
            return RotateRectList(new(pos, size, alignement), pivot, angleDeg);
        }

        public static (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) RotateRect(this Rect rect, Vector2 pivot, float angleDeg)
        {
            float rotRad = angleDeg * SUtils.DEGTORAD;
            Vector2 size = new Vector2(rect.width, rect.height);
            Vector2 pivotPoint = new Vector2(rect.x, rect.y) + size * pivot;

            Vector2 tl = new Vector2(rect.x, rect.y);
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
            return RotateRect(new(pos, size, alignement), pivot, angleDeg);
        }

        public static (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetRectCorners(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            return GetRectCorners(new(pos, size, alignement));
        }
        public static (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetRectCorners(this Rect rect)
        {
            Vector2 tl = new(rect.x, rect.y);
            Vector2 tr = new(rect.x + rect.width, rect.y);
            Vector2 bl = new(rect.x, rect.y + rect.height);
            Vector2 br = new(rect.x + rect.width, rect.y + rect.height);
            return (tl, tr, br, bl);
        }
        public static List<Vector2> GetRectCornersList(this Rect rect)
        {
            Vector2 tl = new(rect.x, rect.y);
            Vector2 tr = new(rect.x + rect.width, rect.y);
            Vector2 bl = new(rect.x, rect.y + rect.height);
            Vector2 br = new(rect.x + rect.width, rect.y + rect.height);
            return new() { tl, tr, br, bl };
        }
        public static List<(Vector2 start, Vector2 end)> GetRectSegments(this Rect rect)
        {
            var c = GetRectCorners(rect);
            return GetRectSegments(c.tl, c.tr, c.br, c.bl);
        }
        public static List<(Vector2 start, Vector2 end)> GetRectSegments(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            return GetRectSegments(new(pos, size, alignement));
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