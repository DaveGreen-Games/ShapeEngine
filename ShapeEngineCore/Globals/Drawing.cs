using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals
{
    public static class Drawing
    {
        public static List<Vector2> GeneratePolygon(int pointCount, Vector2 center, float minLength, float maxLength)
        {
            List<Vector2> points = new();
            float angleStep = PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = RNG.randF(minLength, maxLength);
                Vector2 p = Vec.Rotate(Vec.Right(), angleStep * i) * randLength;
                p += center;
                points.Add(p);
            }
            return points;
        }
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

        public static void DrawCircleSectorLinesEx(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float lineThickness, Color color, bool closed = true, float sideLength = 8f, bool reversed = false)
        {
            float startAngleRad = startAngleDeg * DEG2RAD;
            float endAngleRad = endAngleDeg * DEG2RAD;
            float anglePiece = MathF.Abs(endAngleDeg - startAngleDeg);
            int sides = GetCircleArcSideCount(radius, anglePiece, sideLength);
            anglePiece *= DEG2RAD;
            float angleStep = anglePiece / sides;
            if (reversed) angleStep *= -1f;
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
        public static void DrawCircleSectorLinesEx(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, Color color, bool closed = true, float sideLength = 8f, bool reversed = false)
        {
            DrawCircleSectorLinesEx(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, closed, sideLength, reversed);
        }
        public static void DrawCircleSectorLinesEx(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, Color color, bool closed = true, bool reversed = false)
        {
            float startAngleRad = startAngleDeg * DEG2RAD;
            float endAngleRad = endAngleDeg * DEG2RAD;
            float anglePiece = MathF.Abs(endAngleRad - startAngleRad);
            float angleStep = anglePiece / sides;
            if (reversed) angleStep *= -1f;
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
        public static void DrawCircleSectorLinesEx(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, Color color, bool closed = true, bool reversed = false)
        {
            DrawCircleSectorLinesEx(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed, reversed);
        }

        public static void DrawRingLinesEx(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float lineThickness, Color color, float sideLength = 8f)
        {
            DrawCircleSectorLinesEx(center, innerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);
            DrawCircleSectorLinesEx(center, outerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);

            Vector2 innerStart = center + Vec.Rotate(Vec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), startAngleDeg * DEG2RAD);
            Vector2 outerStart = center + Vec.Rotate(Vec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), startAngleDeg * DEG2RAD);
            DrawLineEx(innerStart, outerStart, lineThickness, color);

            Vector2 innerEnd = center + Vec.Rotate(Vec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), endAngleDeg * DEG2RAD);
            Vector2 outerEnd = center + Vec.Rotate(Vec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), endAngleDeg * DEG2RAD);
            DrawLineEx(innerEnd, outerEnd, lineThickness, color);
        }
        public static void DrawRingLinesEx(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, Color color, float sideLength = 8f)
        {
            DrawRingLinesEx(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, sideLength);
        }
        public static void DrawRingFilled(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, Color color, float sideLength = 10f)
        {
            float anglePiece = MathF.Abs(endAngleDeg - startAngleDeg);
            int sides = GetCircleArcSideCount(outerRadius, anglePiece, sideLength);
            DrawRing(center, innerRadius, outerRadius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), sides, YELLOW);
        }
        public static void DrawRingFilled(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, int sides, Color color)
        {
            DrawRing(center, innerRadius, outerRadius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), sides, YELLOW);
        }
        public static void DrawRingFilled(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, Color color, float sideLength = 10f)
        {
            DrawRingFilled(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, color, sideLength);
        }
        public static void DrawRingFilled(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, Color color)
        {
            DrawRingFilled(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, color);
        }

        public static void DrawRectangeLinesPro(Vector2 center, Vector2 size, float rotRad, float lineThickness, Color color)
        {
            Vector2 topLeft = center + Vec.Rotate(-size / 2, rotRad);
            Vector2 topRight = center + Vec.Rotate(new Vector2(size.X, -size.Y) / 2, rotRad);
            Vector2 bottomRight = center + Vec.Rotate(size / 2, rotRad);
            Vector2 bottomLeft = center + Vec.Rotate(new Vector2(-size.X, size.Y) / 2, rotRad);
            Vector2 leftExtension = Vec.Rotate(new Vector2(-lineThickness / 2, 0f), rotRad);
            Vector2 rightExtension = Vec.Rotate(new Vector2(lineThickness / 2, 0f), rotRad);
            DrawLineEx(topLeft + leftExtension, topRight + rightExtension, lineThickness, color);
            DrawLineEx(bottomLeft + leftExtension, bottomRight + rightExtension, lineThickness, color);
            DrawLineEx(topLeft, bottomLeft, lineThickness, color);
            DrawLineEx(topRight, bottomRight, lineThickness, color);
        }

        public static void DrawRect(Vector2 center, Vector2 size, Vector2 origin, float rotDeg, Color color)
        {
            Rectangle rect = new(center.X, center.Y, size.X, size.Y);
            Vector2 pivot = new(size.X * origin.X, size.Y * origin.Y);
            DrawRectanglePro(rect, pivot, rotDeg, color);

            //DrawRectangle((int)(center.X - size.X / 2), (int)(center.Y - size.Y / 2), (int)size.X, (int)size.Y, new(255, 0, 0, 100));
            //DrawCircleV(center, 10, YELLOW);
        }
        public static void DrawRect(Rectangle rect, Vector2 origin, float rotDeg, Color color)
        {
            Vector2 center = new(rect.x + rect.width / 2, rect.y + rect.height / 2);
            Vector2 size = new(rect.width, rect.height);
            DrawRect(center, size, origin, rotDeg, color);
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

        public static void DrawCircleOutlineBar(Vector2 center, float radius, float thickness, float f, Color color, bool reversed = false)
        {
            DrawCircleSectorLinesEx(center, radius, 0, 360 * f, thickness, color, false, 8f, reversed);
        }
        public static void DrawCircleOutlineBar(Vector2 center, float radius, float startOffsetDeg, float thickness, float f, Color color, bool reversed = false)
        {
            DrawCircleSectorLinesEx(center, radius, 0, 360 * f, startOffsetDeg, thickness, color, false, 8f, reversed);
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

        public static float TransformAngleDeg(float angleDeg) { return 450f - angleDeg; }
        public static float TransformAngleRad(float angleRad) { return 2.5f * PI - angleRad; }

    }
}
