using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
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

        public static void DrawRectangleCheckeredLines(Vector2 pos, Vector2 size, Alignement alignement, float spacing, float lineThickness, float angleDeg, Color lineColor, Color outlineColor, Color bgColor)
        {
            float maxDimension = MathF.Max(size.X, size.Y);
            Vector2 aVector = UIHandler.GetAlignementVector(alignement) * size;
            Vector2 center = pos - aVector + size / 2;
            float rotRad = angleDeg * DEG2RAD;

            

            var rect = Utils.ConstructRectangle(pos, size, alignement);
            Vector2 tl = new(rect.x, rect.y);
            Vector2 tr = new(rect.x + rect.width, rect.y);
            Vector2 bl = new(rect.x, rect.y + rect.height);
            Vector2 br = new(rect.x + rect.width, rect.y + rect.height);
            List<(Vector2 start, Vector2 end)> segments = new()
            {
                (tl, tr), (bl, br), (tl, bl), (tr, br)
            };

            if (bgColor.a > 0) DrawRectangleRec(rect, bgColor);

            Vector2 cur = new(-spacing / 2, 0f);
            int whileMaxCount = (int)(maxDimension / spacing) * 10;
            int whileCounter = 0;

            while (whileCounter < whileMaxCount)
            {
                Vector2 p = center + Vec.Rotate(cur, rotRad);
                //if (!SimpleCollision.Overlap.OverlapPointRect(p, tl, size)) break;
                Vector2 up = new(0f, -maxDimension * 2);
                Vector2 down = new(0f, maxDimension * 2);
                Vector2 start = p + Vec.Rotate(up, rotRad);
                Vector2 end = p + Vec.Rotate(down, rotRad);
                List<Vector2> intersections = new();
                foreach (var seg in segments)
                {
                    if (intersections.Count >= 2) continue;
                    var result = SimpleCollision.Collision.IntersectLineSegment(start, end, seg.start, seg.end);
                    if (result.intersection)
                    {
                        intersections.Add(result.intersectPoint);
                    }
                }
                if (intersections.Count >= 2) DrawLineEx(intersections[0], intersections[1], lineThickness, lineColor);
                else break; // DrawLineEx(start, end, lineThickness, lineColor);
                cur.X -= spacing;
                whileCounter++;
            } 

            cur = new(spacing / 2, 0f);
            whileCounter = 0;
            while (whileCounter < whileMaxCount)
            {
                Vector2 p = center + Vec.Rotate(cur, rotRad);
                //if (!SimpleCollision.Overlap.OverlapPointRect(p, tl, size)) break;
                Vector2 up = new(0f, -maxDimension * 2);
                Vector2 down = new(0f, maxDimension * 2);
                Vector2 start = p + Vec.Rotate(up, rotRad);
                Vector2 end = p + Vec.Rotate(down, rotRad);
                List<Vector2> intersections = new();
                foreach (var seg in segments)
                {
                    if (intersections.Count >= 2) continue;
                    var result = SimpleCollision.Collision.IntersectLineSegment(start, end, seg.start, seg.end);
                    if (result.intersection)
                    {
                        intersections.Add(result.intersectPoint);
                    }
                }
                if (intersections.Count >= 2) DrawLineEx(intersections[0], intersections[1], lineThickness, lineColor);
                else break; // DrawLineEx(start, end, lineThickness, lineColor);
                cur.X += spacing;
                whileCounter++;
            }

            if (outlineColor.a > 0) Drawing.DrawRectangeLinesPro(pos, size, alignement, Alignement.CENTER, 0f, lineThickness, outlineColor);
        }
        public static void DrawCircleCheckeredLines(Vector2 pos, Alignement alignement, float radius, float spacing, float lineThickness, float angleDeg, Color lineColor, Color bgColor)
        {

            float maxDimension = radius;
            Vector2 size = new Vector2(radius, radius) * 2f;
            Vector2 aVector = UIHandler.GetAlignementVector(alignement) * size;
            Vector2 center = pos - aVector + size / 2;
            float rotRad = angleDeg * DEG2RAD;

            if (bgColor.a > 0) DrawCircleV(center, radius, bgColor);

            Vector2 cur = new(-spacing / 2, 0f);
            while (cur.X > -maxDimension)
            {
                Vector2 p = center + Vec.Rotate(cur, rotRad);

                //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
                float angle = MathF.Acos(cur.X / radius);
                float y = radius * MathF.Sin(angle);

                Vector2 up = new(0f, -y);
                Vector2 down = new(0f, y);
                Vector2 start = p + Vec.Rotate(up, rotRad);
                Vector2 end = p + Vec.Rotate(down, rotRad);
                DrawLineEx(start, end, lineThickness, lineColor);
                cur.X -= spacing;
            }

            cur = new(spacing / 2, 0f);
            while (cur.X < maxDimension)
            {
                Vector2 p = center + Vec.Rotate(cur, rotRad);
                //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
                float angle = MathF.Acos(cur.X / radius);
                float y = radius * MathF.Sin(angle);

                Vector2 up = new(0f, -y);
                Vector2 down = new(0f, y);
                Vector2 start = p + Vec.Rotate(up, rotRad);
                Vector2 end = p + Vec.Rotate(down, rotRad);
                DrawLineEx(start, end, lineThickness, lineColor);
                cur.X += spacing;
            }

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

        public static void DrawRectangeLinesPro(Vector2 pos, Vector2 size, Alignement alignement, Vector2 pivot, float rotDeg, float lineThickness, Color color)
        {
            Vector2 leftExtension = Vec.Rotate(new Vector2(-lineThickness / 2, 0f), rotDeg * DEG2RAD);
            Vector2 rightExtension = Vec.Rotate(new Vector2(lineThickness / 2, 0f), rotDeg * DEG2RAD);

            var rr = Utils.RotateRectangle(pos, size, alignement, pivot, rotDeg);
            DrawLineEx(rr.topLeft + leftExtension, rr.topRight + rightExtension, lineThickness, color);
            DrawLineEx(rr.bottomLeft + leftExtension, rr.bottomRight + rightExtension, lineThickness, color);
            DrawLineEx(rr.topLeft, rr.bottomLeft, lineThickness, color);
            DrawLineEx(rr.topRight, rr.bottomRight, lineThickness, color);
        }
        public static void DrawRectangeLinesPro(Vector2 pos, Vector2 size, Alignement alignement, Alignement pivot, float rotDeg, float lineThickness, Color color)
        {
            DrawRectangeLinesPro(pos, size, alignement, UIHandler.GetAlignementVector(pivot), rotDeg, lineThickness, color);
        }

        public static void DrawRectangle(Rectangle rect, Color color)
        {
            DrawRectangleRec(rect, color);
        }
        public static void DrawRectangle(Vector2 pos, Vector2 size, Alignement alignement, Color color)
        {
            DrawRectangle(Utils.ConstructRectangle(pos, size, alignement), color);
        }

        public static void DrawRectangle(Rectangle rect, Vector2 pivot, float rotDeg, Color color)
        {
            var rr = Utils.RotateRectangle(rect, pivot, rotDeg);
            Raylib.DrawTriangle(rr.topLeft, rr.bottomLeft, rr.bottomRight, color);
            Raylib.DrawTriangle(rr.bottomRight, rr.topRight, rr.topLeft, color);
        }

        public static void DrawRectangle(Vector2 pos, Vector2 size, Alignement alignement, Vector2 pivot, float rotDeg, Color color)
        {
            DrawRectangle(Utils.ConstructRectangle(pos, size, alignement), pivot, rotDeg, color);
        }

        public static void DrawRectangle(Rectangle rect, Alignement pivot, float rotDeg, Color color)
        {
            DrawRectangle(rect, UIHandler.GetAlignementVector(pivot), rotDeg, color);
        }

        public static void DrawRectangle(Vector2 pos, Vector2 size, Alignement alignement, Alignement pivot, float rotDeg, Color color)
        {
            DrawRectangle(Utils.ConstructRectangle(pos, size, alignement), UIHandler.GetAlignementVector(pivot), rotDeg, color);
        }
        


        public static void DrawRectangleOutlineBar(Rectangle rect, float thickness, float f, Color color)
        {
            Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
            Vector2 thicknessOffsetY = new Vector2(0f, thickness);

            Vector2 tl = new(rect.x, rect.y);
            Vector2 br = tl + new Vector2(rect.width, rect.height); ;
            Vector2 tr = tl + new Vector2(rect.width, 0);
            Vector2 bl = tl + new Vector2(0, rect.height);

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
        public static void DrawRectangleOutlineBar(Vector2 pos, Vector2 size, Alignement alignement, float thickness, float f, Color color)
        {
            DrawRectangleOutlineBar(Utils.ConstructRectangle(pos, size, alignement), thickness, f, color);
        }


        public static void DrawRectangleOutlineBar(Rectangle rect, Vector2 pivot, float angleDeg, float thickness, float f, Color color)
        {
            var rr = Utils.RotateRectangle(rect, pivot, angleDeg);
            //Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
            //Vector2 thicknessOffsetY = new Vector2(0f, thickness);
            
            Vector2 leftExtension = Vec.Rotate(new Vector2(-thickness / 2, 0f), angleDeg * DEG2RAD);
            Vector2 rightExtension = Vec.Rotate(new Vector2(thickness / 2, 0f), angleDeg * DEG2RAD);

            Vector2 tl = rr.topLeft;
            Vector2 br = rr.bottomRight;
            Vector2 tr = rr.topRight;
            Vector2 bl = rr.bottomLeft;

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
                    start = tl + leftExtension;
                    end = tr + rightExtension;
                }
                else if (i == 1)
                {
                    start = tr;
                    end = br;
                }
                else if (i == 2)
                {
                    start = br + rightExtension;
                    end = bl + leftExtension;
                }
                else
                {
                    start = bl;
                    end = tl;
                }

                //last line
                if (i == lines - 1) end = Vec.Lerp(start, end, newF);
                DrawLineEx(start, end, thickness, color);
            }
        }
        public static void DrawRectangleOutlineBar(Vector2 pos, Vector2 size, Alignement alignement, Vector2 pivot, float angleDeg, float thickness, float f, Color color)
        {
            DrawRectangleOutlineBar(Utils.ConstructRectangle(pos, size, alignement), pivot, angleDeg, thickness, f, color);
        }
        public static void DrawRectangleOutlineBar(Vector2 pos, Vector2 size, Alignement alignement, Alignement pivot, float angleDeg, float thickness, float f, Color color)
        {
            DrawRectangleOutlineBar(Utils.ConstructRectangle(pos, size, alignement), UIHandler.GetAlignementVector(pivot), angleDeg, thickness, f, color);
        }
        public static void DrawRectangleOutlineBar(Rectangle rect, Alignement pivot, float angleDeg, float thickness, float f, Color color)
        {
            DrawRectangleOutlineBar(rect, UIHandler.GetAlignementVector(pivot), angleDeg, thickness, f, color);
        }
        
        
        public static void DrawCircleOutlineBar(Vector2 center, float radius, float thickness, float f, Color color)
        {
            DrawCircleSectorLinesEx(center, radius, 0, 360 * f, thickness, color, false, 8f);
        }
        public static void DrawCircleOutlineBar(Vector2 center, float radius, float startOffsetDeg, float thickness, float f, Color color)
        {
            DrawCircleSectorLinesEx(center, radius, 0, 360 * f, startOffsetDeg, thickness, color, false, 8f);
        }

        
        public static void DrawBar(Rectangle rect, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            f = 1.0f - f;
            UIMargins progressMargins = new(f * top, f * right, f * bottom, f * left);
            var progressRect = progressMargins.Apply(rect);
            Drawing.DrawRectangle(rect, bgColor);
            Drawing.DrawRectangle(progressRect, barColor);
        }
        public static void DrawBar(Vector2 pos, Vector2 size, Alignement alignement, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            DrawBar(Utils.ConstructRectangle(pos, size, alignement), f, barColor, bgColor, left, right, top, bottom);
        }

        public static void DrawBar(Rectangle rect, Vector2 pivot, float angleDeg, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            f = 1.0f - f;
            UIMargins progressMargins = new(f * top, f * right, f * bottom, f * left);
            var progressRect = progressMargins.Apply(rect);
            Drawing.DrawRectangle(rect, pivot, angleDeg, bgColor);
            Drawing.DrawRectangle(progressRect, pivot, angleDeg, barColor);
        }
        public static void DrawBar(Vector2 pos, Vector2 size, Alignement alignement, Vector2 pivot, float angleDeg, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            DrawBar(Utils.ConstructRectangle(pos, size, alignement), pivot, angleDeg, f, barColor, bgColor, left, right, top, bottom);
        }
        public static void DrawBar(Rectangle rect, Alignement pivot, float angleDeg, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            DrawBar(rect, UIHandler.GetAlignementVector(pivot), angleDeg, f, barColor, bgColor, left, right, top, bottom);
        }
        public static void DrawBar(Vector2 pos, Vector2 size, Alignement alignement, Alignement pivot, float angleDeg, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            DrawBar(Utils.ConstructRectangle(pos, size, alignement), UIHandler.GetAlignementVector(pivot), angleDeg, f, barColor, bgColor, left, right, top, bottom);
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
