using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals.UI;

namespace ShapeEngineCore.Globals
{
    public static class Drawing
    {

        public static void DrawGrid(Rectangle rect, int lines, float lineThickness, Color color)
        {
            float width = rect.width;
            float height = rect.height;
            float hGap = width / lines;
            float vGap = height / lines;

            Vector2 tl = new Vector2(rect.x, rect.y);
            Vector2 tr = tl + new Vector2(width, 0);
            Vector2 bl = tl + new Vector2(0, height);

            for (int l = 0; l < lines; l++)
            {
                Vector2 xOffset = new Vector2(hGap, 0f) * l;
                Vector2 yOffset = new Vector2(0f, vGap) * l;
                DrawLineEx(tl + xOffset, bl + xOffset, lineThickness, color);
                DrawLineEx(tl + yOffset, tr + yOffset, lineThickness, color);
            }
        }
        //public static void DrawGrid(Rectangle rect, int lines, float lineThickness, Color color, Vector2 highlightPos, Color highlightColor)
        //{
        //    float width = rect.width;
        //    float height = rect.height;
        //    float hGap = width / lines;
        //    float vGap = height / lines;
        //
        //    Vector2 tl = new Vector2(rect.x, rect.y);
        //    Vector2 tr = tl + new Vector2(width, 0);
        //    Vector2 bl = tl + new Vector2(0, height);
        //
        //    for (int l = 0; l < lines; l++)
        //    {
        //        Vector2 xOffset = new Vector2(hGap, 0f) * l;
        //        Vector2 yOffset = new Vector2(0f, vGap) * l;
        //        DrawLineEx(tl + xOffset, bl + xOffset, lineThickness, color);
        //        DrawLineEx(tl + yOffset, tr + yOffset, lineThickness, color);
        //    }
        //
        //    int x = (int)(highlightPos.X / hGap);
        //    int y = (int)(highlightPos.Y / vGap);
        //    Rectangle highlightRect = new(tl.X + x * hGap, tl.Y + y * vGap, hGap, vGap);
        //    DrawRectangleLinesEx(highlightRect, lineThickness, highlightColor);
        //}
        public static void DrawPolygon(List<Vector2> points, Vector2 center, Color fillColor, bool clockwise = true)
        {
            if (clockwise)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    DrawTriangle(points[i], center, points[i + 1], fillColor);
                }
                DrawTriangle(points[points.Count - 1], center, points[0], fillColor);
            }
            else
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    DrawTriangle(points[i], points[i + 1], center, fillColor);
                }
                DrawTriangle(points[points.Count - 1], points[0], center, fillColor);
            }


        }
        public static void DrawPolygon(List<Vector2> points, float lineThickness, Color outlineColor)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                DrawLineEx(points[i], points[i + 1], lineThickness, outlineColor);
            }
            DrawLineEx(points[points.Count - 1], points[0], lineThickness, outlineColor);
        }
        public static void DrawPolygon(List<Vector2> points, float lineThickness, Color outlineColor, Vector2 center)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                DrawLineEx(center + points[i], center + points[i + 1], lineThickness, outlineColor);
            }
            DrawLineEx(center + points[points.Count - 1], center + points[0], lineThickness, outlineColor);
        }
        public static void DrawPolygon(List<Vector2> points, Vector2 center, Color fillColor, float lineThickness, Color outlineColor, bool clockwise = true)
        {
            DrawPolygon(points, center, fillColor, clockwise);
            DrawPolygon(points, lineThickness, outlineColor);
        }

        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, int sides, Color color)
        {
            DrawPolyLinesEx(center, sides, radius, 0f, lineThickness, color);
        }
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, float rotDeg, int sides, Color color)
        {
            DrawPolyLinesEx(center, sides, radius, rotDeg, lineThickness, color);
        }
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, Color color, float sideLength = 8f)
        {
            int sides = GetCircleSideCount(radius, sideLength);
            DrawPolyLinesEx(center, sides, radius, 0f, lineThickness, color);
        }
        public static void DrawCircleSector(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int segments, Color color)
        {
            Raylib.DrawCircleSector(center, radius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), segments, color);
        }
        public static void DrawCircleSectorLinesEx(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float lineThickness, Color color, bool closed = true, float sideLength = 8f)
        {
            float startAngleRad = startAngleDeg * DEG2RAD;
            float endAngleRad = endAngleDeg * DEG2RAD;
            float anglePiece = endAngleRad - startAngleRad;
            int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePiece * RAD2DEG), sideLength);
            float angleStep = anglePiece / sides;
            if (closed)
            {
                Vector2 sectorStart = center + Vec.Rotate(Vec.Right() * radius + new Vector2(lineThickness / 2, 0), startAngleRad);
                DrawLineEx(center, sectorStart, lineThickness, color);

                Vector2 sectorEnd = center + Vec.Rotate(Vec.Right() * radius + new Vector2(lineThickness / 2, 0), endAngleRad);
                DrawLineEx(center, sectorEnd, lineThickness, color);
            }
            for (int i = 0; i < sides; i++)
            {
                Vector2 start = center + Vec.Rotate(Vec.Right() * radius, startAngleRad + angleStep * i);
                Vector2 end = center + Vec.Rotate(Vec.Right() * radius, startAngleRad + angleStep * (i + 1));
                DrawLineEx(start, end, lineThickness, color);
            }
        }
        public static void DrawCircleSectorLinesEx(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, Color color, bool closed = true, float sideLength = 8f)
        {
            DrawCircleSectorLinesEx(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, closed, sideLength); ;
        }
        public static void DrawCircleSectorLinesEx(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, Color color, bool closed = true)
        {
            float startAngleRad = startAngleDeg * DEG2RAD;
            float endAngleRad = endAngleDeg * DEG2RAD;
            float anglePiece = endAngleDeg - startAngleRad;
            float angleStep = MathF.Abs(anglePiece) / sides;
            if (closed)
            {
                Vector2 sectorStart = center + Vec.Rotate(Vec.Right() * radius + new Vector2(lineThickness / 2, 0), startAngleRad);
                DrawLineEx(center, sectorStart, lineThickness, color);

                Vector2 sectorEnd = center + Vec.Rotate(Vec.Right() * radius + new Vector2(lineThickness / 2, 0), endAngleRad);
                DrawLineEx(center, sectorEnd, lineThickness, color);
            }
            for (int i = 0; i < sides; i++)
            {
                Vector2 start = center + Vec.Rotate(Vec.Right() * radius, startAngleRad + angleStep * i);
                Vector2 end = center + Vec.Rotate(Vec.Right() * radius, startAngleRad + angleStep * (i + 1));
                DrawLineEx(start, end, lineThickness, color);
            }
        }
        public static void DrawCircleSectorLinesEx(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, Color color, bool closed = true)
        {
            DrawCircleSectorLinesEx(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed);
        }

        public static void DrawRingLinesEx(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float lineThickness, Color color, float sideLength = 8f)
        {
            DrawCircleSectorLinesEx(center, innerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);
            DrawCircleSectorLinesEx(center, outerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);

            float startAngleRad = startAngleDeg * DEG2RAD;
            float endAngleRad = endAngleDeg * DEG2RAD;
            Vector2 innerStart = center + Vec.Rotate(Vec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), startAngleRad);
            Vector2 outerStart = center + Vec.Rotate(Vec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), startAngleRad);
            DrawLineEx(innerStart, outerStart, lineThickness, color);

            Vector2 innerEnd = center + Vec.Rotate(Vec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), endAngleRad);
            Vector2 outerEnd = center + Vec.Rotate(Vec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), endAngleRad);
            DrawLineEx(innerEnd, outerEnd, lineThickness, color);
        }
        public static void DrawRingLinesEx(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, Color color, float sideLength = 8f)
        {
            DrawRingLinesEx(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, sideLength);
        }
        public static void DrawRingFilled(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, Color color, float sideLength = 10f)
        {
            float start = TransformAngleDeg(startAngleDeg);
            float end = TransformAngleDeg(endAngleDeg);
            float anglePiece = end - start;
            int sides = GetCircleArcSideCount(outerRadius, MathF.Abs(anglePiece), sideLength);
            DrawRing(center, innerRadius, outerRadius, start, end, sides, color);
        }
        public static void DrawRingFilled(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, int sides, Color color)
        {
            DrawRing(center, innerRadius, outerRadius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), sides, color);
        }
        public static void DrawRingFilled(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, Color color, float sideLength = 10f)
        {
            DrawRingFilled(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, color, sideLength);
        }
        public static void DrawRingFilled(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, Color color)
        {
            DrawRingFilled(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, color);
        }

        //public static void DrawRectangeLinesPro(Vector2 center, Vector2 size, float rotRad, float lineThickness, Color color)
        //{
        //    Vector2 topLeft = center + Vec.Rotate(-size / 2, rotRad);
        //    Vector2 topRight = center + Vec.Rotate(new Vector2(size.X, -size.Y) / 2, rotRad);
        //    Vector2 bottomRight = center + Vec.Rotate(size / 2, rotRad);
        //    Vector2 bottomLeft = center + Vec.Rotate(new Vector2(-size.X, size.Y) / 2, rotRad);
        //    Vector2 leftExtension = Vec.Rotate(new Vector2(-lineThickness / 2, 0f), rotRad);
        //    Vector2 rightExtension = Vec.Rotate(new Vector2(lineThickness / 2, 0f), rotRad);
        //    DrawLineEx(topLeft + leftExtension, topRight + rightExtension, lineThickness, color);
        //    DrawLineEx(bottomLeft + leftExtension, bottomRight + rightExtension, lineThickness, color);
        //    DrawLineEx(topLeft, bottomLeft, lineThickness, color);
        //    DrawLineEx(topRight, bottomRight, lineThickness, color);
        //}
        public static void DrawRectangeLinesPro(Vector2 pos, Vector2 size, Vector2 pivot, float rotRad, float lineThickness, Color color)
        {
            Vector2 av = pivot;
            Vector2 topLeft = pos + Vec.Rotate(-size * av, rotRad);
            Vector2 topRight = pos + Vec.Rotate(new Vector2(size.X, 0f) - size * av, rotRad);
            Vector2 bottomRight = pos + Vec.Rotate(new Vector2(size.X, size.Y) - size * av, rotRad);
            Vector2 bottomLeft = pos + Vec.Rotate(new Vector2(0f, size.Y) - size * av, rotRad);
            //Vector2 leftExtension = Vec.Rotate(new Vector2(-lineThickness / 2, 0f), rotRad);
            //Vector2 rightExtension = Vec.Rotate(new Vector2(lineThickness / 2, 0f), rotRad);
            DrawLineEx(topLeft, topRight, lineThickness, color);
            DrawLineEx(bottomLeft, bottomRight, lineThickness, color);
            DrawLineEx(topLeft, bottomLeft, lineThickness, color);
            DrawLineEx(topRight, bottomRight, lineThickness, color);
        }
        public static void DrawRectangeLinesPro(Vector2 pos, Vector2 size, Alignement pivot, float rotRad, float lineThickness, Color color)
        {
            DrawRectangeLinesPro(pos, size, UIHandler.GetAlignementVector(pivot), rotRad, lineThickness, color);
        }
        /// <summary>
        /// Draw a rectangle with optional rotation.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="pivot">Value from 0 (top left) to 1 (bottom right)</param>
        /// <param name="rotDeg"></param>
        /// <param name="color"></param>
        public static void DrawRectangle(Vector2 center, Vector2 size, Vector2 pivot, float rotDeg, Color color)
        {
            //pivot -= new Vector2(0.5f, 0.5f);
            Rectangle rect = new(center.X, center.Y, size.X, size.Y);
            Vector2 rotPivot = new(size.X * pivot.X, size.Y * pivot.Y);
            DrawRectanglePro(rect, rotPivot, rotDeg, color);
        }
        public static void DrawRectangle(Rectangle rect, Vector2 pivot, float rotDeg, Color color)
        {
            Vector2 center = new(rect.x + rect.width / 2, rect.y + rect.height / 2);
            Vector2 size = new(rect.width, rect.height);
            DrawRectangle(center, size, pivot, rotDeg, color);
        }
        public static void DrawRectangle(Rectangle rect, Alignement pivot, float rotDeg, Color color)
        {
            Drawing.DrawRectangle(rect, UIHandler.GetAlignementVector(pivot), rotDeg, color);
        }
        public static void DrawRectangle(Vector2 center, Vector2 size, Alignement pivot, float rotDeg, Color color)
        {
            Drawing.DrawRectangle(center, size, UIHandler.GetAlignementVector(pivot), rotDeg, color);
        }

        public static void DrawRectangleOutlineBar(Vector2 center, Vector2 size, float thickness, float f, Color color)
        {
            Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
            Vector2 thicknessOffsetY = new Vector2(0f, thickness);
            Vector2 tl = center - size / 2;
            Vector2 br = center + size / 2;
            Vector2 tr = center + new Vector2(size.X, -size.Y) / 2;
            Vector2 bl = center + new Vector2(-size.X, size.Y) / 2;

            int lines = (int)MathF.Ceiling(4 * Clamp(f, 0f, 1f));
            float fMin = 0.25f * (lines - 1);
            float fMax = fMin + 0.25f;
            float newF = Utils.RemapFloat(f, fMin, fMax, 0f, 1f);
            for (int i = 0; i < lines; i++)
            {
                Vector2 end;
                Vector2 start;
                if (i == 0)
                {
                    start = tl - thicknessOffsetX / 2;
                    end = tr - thicknessOffsetX / 2;
                }
                else if (i == 1)
                {
                    start = tr - thicknessOffsetY / 2;
                    end = br - thicknessOffsetY / 2;
                }
                else if (i == 2)
                {
                    start = br + thicknessOffsetX / 2;
                    end = bl + thicknessOffsetX / 2;
                }
                else
                {
                    start = bl + thicknessOffsetY / 2;
                    end = tl + thicknessOffsetY / 2;
                }

                //last line
                if (i == lines - 1) end = Vec.Lerp(start, end, newF);
                DrawLineEx(start, end, thickness, color);
            }
        }
        public static void DrawRectangleOutlineBar(Vector2 center, Vector2 size, float angleRad, float thickness, float f, Color color)
        {
            Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
            Vector2 thicknessOffsetY = new Vector2(0f, thickness);
            Vector2 tl = -size / 2;
            Vector2 br = size / 2;
            Vector2 tr = new Vector2(size.X, -size.Y) / 2;
            Vector2 bl = new Vector2(-size.X, size.Y) / 2;

            int lines = (int)MathF.Ceiling(4 * Clamp(f, 0f, 1f));
            float fMin = 0.25f * (lines - 1);
            float fMax = fMin + 0.25f;
            float newF = Utils.RemapFloat(f, fMin, fMax, 0f, 1f);
            for (int i = 0; i < lines; i++)
            {
                Vector2 end;
                Vector2 start;
                if (i == 0)
                {
                    start = tl - thicknessOffsetX / 2;
                    end = tr - thicknessOffsetX / 2;
                }
                else if (i == 1)
                {
                    start = tr - thicknessOffsetY / 2;
                    end = br - thicknessOffsetY / 2;
                }
                else if (i == 2)
                {
                    start = br + thicknessOffsetX / 2;
                    end = bl + thicknessOffsetX / 2;
                }
                else
                {
                    start = bl + thicknessOffsetY / 2;
                    end = tl + thicknessOffsetY / 2;
                }

                //last line
                if (i == lines - 1) end = Vec.Lerp(start, end, newF);
                DrawLineEx(center + Vec.Rotate(start, angleRad), center + Vec.Rotate(end, angleRad), thickness, color);
            }
        }

        public static void DrawCircleOutlineBar(Vector2 center, float radius, float thickness, float f, Color color)
        {
            DrawCircleSectorLinesEx(center, radius, 0, 360 * f, thickness, color, false, 8f);
        }
        public static void DrawCircleOutlineBar(Vector2 center, float radius, float startOffsetDeg, float thickness, float f, Color color)
        {
            DrawCircleSectorLinesEx(center, radius, 0, 360 * f, startOffsetDeg, thickness, color, false, 8f);
        }

        public static int GetCircleSideCount(float radius, float maxLength = 10f)
        {
            float circumference = 2.0f * PI * radius;
            return (int)MathF.Max(circumference / maxLength, 1);
        }
        public static int GetCircleArcSideCount(float radius, float angleDeg, float maxLength = 10f)
        {
            float circumference = 2.0f * PI * radius * (angleDeg / 360f);
            return (int)MathF.Max(circumference / maxLength, 1);
        }



        public static void DrawBar(Vector2 topLeft, Vector2 size, float f, Color barColor, Color bgColor, BarType barType = BarType.LEFTRIGHT)
        {
            Rectangle barRect = new Rectangle(topLeft.X, topLeft.Y, size.X, size.Y);
            DrawBar(barRect, f, barColor, bgColor, barType);
        }
        public static void DrawBar(Rectangle barRect, float f, Color barColor, Color bgColor, BarType barType = BarType.LEFTRIGHT)
        {
            Rectangle original = barRect;
            Rectangle rect = original;
            switch (barType)
            {
                case BarType.LEFTRIGHT:
                    rect.width *= f;
                    break;
                case BarType.RIGHTLEFT:
                    rect.X += rect.width * (1.0f - f);
                    rect.width *= f;
                    break;
                case BarType.TOPBOTTOM:
                    rect.height *= f;
                    break;
                case BarType.BOTTOMTOP:
                    rect.Y += rect.height * (1.0f - f);
                    rect.height *= f;
                    break;
                default:
                    rect.width *= f;
                    break;
            }
            DrawRectangleRec(original, bgColor);
            DrawRectangleRec(rect, barColor);
        }
        public static void DrawBar(Vector2 topLeft, Vector2 size, float f, Color barColor, Color bgColor, Color outlineColor, float outlineSize, BarType barType = BarType.LEFTRIGHT)
        {
            Rectangle barRect = new Rectangle(topLeft.X, topLeft.Y, size.X, size.Y);
            DrawBar(barRect, f, barColor, bgColor, outlineColor, outlineSize, barType);
        }
        public static void DrawBar(Rectangle barRect, float f, Color barColor, Color bgColor, Color outlineColor, float outlineSize, BarType barType = BarType.LEFTRIGHT)
        {
            Rectangle original = barRect;
            Rectangle rect = original;
            switch (barType)
            {
                case BarType.LEFTRIGHT:
                    rect.width *= f;
                    break;
                case BarType.RIGHTLEFT:
                    rect.X += rect.width * (1.0f - f);
                    break;
                case BarType.TOPBOTTOM:
                    rect.height *= f;
                    break;
                case BarType.BOTTOMTOP:
                    rect.Y += rect.height * (1.0f - f);
                    break;
                default:
                    rect.width *= f;
                    break;
            }
            DrawRectangleRec(original, bgColor);
            DrawRectangleRec(rect, barColor);
            if (outlineSize > 0f) DrawRectangleLinesEx(original, outlineSize, outlineColor);
        }



        public static float TransformAngleDeg(float angleDeg) { return 450f - angleDeg; }
        public static float TransformAngleRad(float angleRad) { return 2.5f * PI - angleRad; }

    }
}
