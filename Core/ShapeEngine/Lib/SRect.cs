using ShapeEngine.Core;
using System.Numerics;

namespace ShapeEngine.Lib
{
    public static class SRect
    {
        #region UI
        public static List<Rect> GetAlignedRectsHorizontal(this Rect rect, int count, float gapRelative = 0f, float maxElementSizeRel = 1f)
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
        public static List<Rect> GetAlignedRectsVertical(this Rect rect, int count, float gapRelative = 0f, float maxElementSizeRel = 1f)
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
        public static List<Rect> GetAlignedRectsGrid(this Rect rect, int columns, int rows, int count, float hGapRelative = 0f, float vGapRelative = 0f, bool leftToRight = true)
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
        #endregion

        #region Collision
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
        #endregion

        #region Misc
        
        /// <summary>
        /// Construct 9 rects out of an outer and inner rect.
        /// </summary>
        /// <param name="inner">The inner rect. Has to be inside of the outer rect.</param>
        /// <param name="outer">The outer rect. Has to be bigger than the inner rect.</param>
        /// <returns>A list of rectangle in the order [TL,TC,TR,LC,C,RC,BL,BC,BR].</returns>
        public static List<Rect> GetNineTiles(Rect inner, Rect outer)
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

        /// <summary>
        /// Points are ordered in ccw order starting with top left. (tl, bl, br, tr)
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="pivot"></param>
        /// <param name="angleDeg"></param>
        /// <returns></returns>
        public static Polygon RotateList(this Rect rect, Vector2 pivot, float angleDeg)
        {
            return SPoly.Rotate(rect.ToPolygon(), pivot, angleDeg);

            //float rotRad = angleDeg * SUtils.DEGTORAD;
            //Vector2 size = new Vector2(rect.width, rect.height);
            //Vector2 pivotPoint = new Vector2(rect.x, rect.y) + size * pivot;
            //
            //Vector2 tl = new Vector2(rect.x, rect.y);
            //Vector2 br = tl + size;
            //Vector2 tr = tl + new Vector2(rect.width, 0);
            //Vector2 bl = tl + new Vector2(0, rect.height);
            //
            //Vector2 topLeft = pivotPoint + SVec.Rotate(tl - pivotPoint, rotRad);
            //Vector2 topRight = pivotPoint + SVec.Rotate(tr - pivotPoint, rotRad);
            //Vector2 bottomRight = pivotPoint + SVec.Rotate(br - pivotPoint, rotRad);
            //Vector2 bottomLeft = pivotPoint + SVec.Rotate(bl - pivotPoint, rotRad);
            //
            //return new() { topLeft, topRight, bottomRight, bottomLeft };
        }
        public static (Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr) Rotate(this Rect rect, Vector2 pivot, float angleDeg)
        {
            var rotated = SPoly.Rotate(rect.ToPolygon(), pivot, angleDeg);
            return new(rotated[0], rotated[1], rotated[2], rotated[3]);

            //float rotRad = angleDeg * SUtils.DEGTORAD;
            //Vector2 size = new Vector2(rect.width, rect.height);
            //Vector2 pivotPoint = new Vector2(rect.x, rect.y) + size * pivot;
            //
            //Vector2 tl = new Vector2(rect.x, rect.y);
            //Vector2 br = tl + size;
            //Vector2 tr = tl + new Vector2(rect.width, 0);
            //Vector2 bl = tl + new Vector2(0, rect.height);
            //
            //Vector2 topLeft = pivotPoint + SVec.Rotate(tl - pivotPoint, rotRad);
            //Vector2 topRight = pivotPoint + SVec.Rotate(tr - pivotPoint, rotRad);
            //Vector2 bottomRight = pivotPoint + SVec.Rotate(br - pivotPoint, rotRad);
            //Vector2 bottomLeft = pivotPoint + SVec.Rotate(bl - pivotPoint, rotRad);
            //
            //return new(topLeft, topRight, bottomRight, bottomLeft);
        }
        /// <summary>
        /// Returns the segments of a rect in ccw order. (tl -> bl, bl -> br, br -> tr, tr -> tl)
        /// </summary>
        /// <param name="tl"></param>
        /// <param name="bl"></param>
        /// <param name="br"></param>
        /// <param name="tr"></param>
        /// <returns></returns>
        public static Segments GetEdges(Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr)
        {
            Segments segments = new()
            {
                new(tl, bl), new(bl, br), new(br, tr), new(tr, tl)
            };

            return segments;
        }
        #endregion

        #region Math
        public static Vector2 GetPoint(this Rect r, Vector2 alignement)
        {
            Vector2 offset = r.Size * alignement;
            return r.TopLeft + offset;
        }
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
        public static Rect Align(this Rect r, Vector2 alignement) { return new(r.TopLeft, r.Size, alignement); }
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
        public static Rect ScaleSize(this Rect r, float scale, Vector2 alignement) { return new(r.GetPoint(alignement), r.Size * scale, alignement); }
        public static Rect ScaleSize(this Rect r, Vector2 scale, Vector2 alignement) { return new(r.GetPoint(alignement), r.Size * scale, alignement); }
        public static Rect ChangeSize(this Rect r, float amount, Vector2 alignement) { return new(r.GetPoint(alignement), new(r.width + amount, r.height + amount), alignement); }
        public static Rect ChangeSize(this Rect r, Vector2 amount, Vector2 alignement) { return new(r.GetPoint(alignement), r.Size + amount, alignement); }
        public static Rect Move(this Rect r, Vector2 amount) { return new( r.TopLeft + amount, r.Size, new(0f)); }
        public static Rect Enlarge(this Rect r, Vector2 p)
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
            return new(tl, br);
        }
        public static Vector2 ClampOnRect(this Rect r, Vector2 p)
        {
            return new
                (
                    SUtils.Clamp(p.X, r.x, r.x + r.width),
                    SUtils.Clamp(p.Y, r.y, r.y + r.height)
                );
        }
        /// <summary>
        /// Checks if the top left point is further up & left than the bottom right point and returns the correct points if necessary.
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <returns></returns>
        public static (Vector2 topLeft, Vector2 bottomRight) Fix(Vector2 topLeft, Vector2 bottomRight)
        {
            Vector2 tl = topLeft;// r.TopLeft;
            Vector2 br = bottomRight;// r.BottomRight;
            
            Vector2 newTopLeft = new
                (
                    MathF.Min(tl.X, br.X),
                    MathF.Min(tl.Y, br.Y)
                );
            Vector2 newBottomRight = new
                (
                    MathF.Max(tl.X, br.X),
                    MathF.Max(tl.Y, br.Y)
                );


            return (newTopLeft, newBottomRight);
        }
        public static Rect Clamp(this Rect r, Rect bounds)
        {
            Vector2 tl = ClampOnRect(bounds, r.TopLeft);
            Vector2 br = ClampOnRect(bounds, r.BottomRight);
            return new(tl, br);
        }
        public static Rect Clamp(this Rect r, Vector2 min, Vector2 max) { return Clamp(r, new Rect(min, max)); }
        #endregion

        #region Corners
        /// <summary>
        /// Corners a numbered in ccw order starting from the top left. (tl, bl, br, tr)
        /// </summary>
        /// <param name="r"></param>
        /// <param name="corner">Corner Index from 0 to 3</param>
        /// <returns></returns>
        public static Vector2 GetCorner(this Rect r, int corner) { return r.ToPolygon()[corner % 4]; }
        public static (Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr) GetCorners(this Rect rect) { return (rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight); }
        /// <summary>
        /// Points are ordered in ccw order starting from the top left. (tl, bl, br, tr)
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Polygon GetPointsRelative(this Rect rect, Vector2 pos)
        {
            var points = rect.ToPolygon(); //GetPoints(rect);
            for (int i = 0; i < points.Count; i++)
            {
                points[i] -= pos;
            }
            return points;
        }
        
        #endregion


        //public static Polygon GetPointsPolygon(this Rect rect) { return new() { rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight }; }
        ///// <summary>
        ///// Points are ordered in ccw order starting from the top left. (tl, bl, br, tr)
        ///// </summary>
        ///// <param name="rect"></param>
        ///// <returns></returns>
        //public static Polygon GetPoints(this Rect rect) { return new() { rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight }; }



        public static bool SeperateAxis(this Rect r, Vector2 axisStart, Vector2 axisEnd)
        {
            Vector2 n = axisStart - axisEnd;
            var corners = r.ToPolygon();
            Vector2 edgeAStart =    corners[0]; //r.GetRectCorner(0); //GetRectCorner(rectPos, rectSize, rectAlignement, 0);
            Vector2 edgeAEnd =      corners[1]; //r.GetRectCorner(1);// GetRectCorner(rectPos, rectSize, rectAlignement, 1);
            Vector2 edgeBStart =    corners[2]; //r.GetRectCorner(2); //GetRectCorner(rectPos, rectSize, rectAlignement, 2);
            Vector2 edgeBEnd =      corners[3]; //r.GetRectCorner(3); //GetRectCorner(rectPos, rectSize, rectAlignement, 3);

            RangeFloat edgeARange = ProjectSegment(edgeAStart, edgeAEnd, n);
            RangeFloat edgeBRange = ProjectSegment(edgeBStart, edgeBEnd, n);
            RangeFloat rProjection = RangeHull(edgeARange, edgeBRange);

            RangeFloat axisRange = ProjectSegment(axisStart, axisEnd, n);
            return !OverlappingRange(axisRange, rProjection);
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



//public static Vector2 GetAlignementPos(this Rect rect, Vector2 alignement)
//{
//    Vector2 offset = rect.Size * alignement;
//    return rect.TopLeft + offset;
//}
//public static Rect Align(this Rect rect, Vector2 alignement)
//{
//    Vector2 topLeft = new Vector2(rect.x, rect.y);
//    Vector2 size = new(rect.width, rect.height);
//    //return new(topLeft, size, alignement);
//    Vector2 offset = size * alignement;
//    return new
//        (
//            topLeft.X + offset.X,
//            topLeft.Y + offset.Y,
//            size.X,
//            size.Y
//        );
//}
/*
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
*/
/*
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
*/