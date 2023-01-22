using System.Numerics;
using Raylib_CsLo;
using ShapeUI;

namespace ShapeLib
{
    public static class SDrawing
    {


        public static void DrawTextBox(Rectangle rect, string emptyText, List<char> chars, float fontSpacing, Font font, Color textColor, bool drawCaret, int caretPosition, float caretWidth, Color caretColor, Vector2 alignement)
        {
            //fix alignement
            alignement = new(0, 0.5f);
            if(chars.Count <= 0)
            {
                SDrawing.DrawTextAligned(emptyText, rect, fontSpacing, textColor, font, alignement);
            }
            else
            {
                string text = String.Concat(chars);
                SDrawing.DrawTextAligned(text, rect, fontSpacing, textColor, font, alignement);

                if (drawCaret)
                {
                    float fontSize = UIHandler.CalculateDynamicFontSize(text, new Vector2(rect.width, rect.height), font, fontSpacing);
                    string caretText = String.Concat(chars.GetRange(0, caretPosition));
                    Vector2 caretTextSize = MeasureTextEx(font, caretText, fontSize, fontSpacing);

                    //Vector2 uiPos = SRect.GetRectPos(rect, alignement);
                    //Vector2 topLeft = uiPos - alignement * new Vector2(caretTextSize.X, 0f);
                    Vector2 topLeft = new(rect.x, rect.y);

                    Vector2 caretTop = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, 0f);
                    Vector2 caretBottom = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, rect.height);
                    DrawLineEx(caretTop, caretBottom, caretWidth, caretColor);
                }
            }
        }

        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, Vector2 textSize, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = UIHandler.CalculateDynamicFontSize(text, textSize, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = alignement * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, fontSpacing, color);

            // DrawRectangleLinesEx(new())
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, float textHeight, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = UIHandler.CalculateDynamicFontSize(textHeight, font);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = alignement * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, fontSpacing, color);
        }
        public static void DrawTextAlignedPro2(string text, Vector2 uiPos, float rotDeg, float textWidth, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = UIHandler.CalculateDynamicFontSize(text, textWidth, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = alignement * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, 1, color);
        }
        public static void DrawTextAlignedPro3(string text, Vector2 uiPos, float rotDeg, float fontSize, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = alignement * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, fontSpacing, color);
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, Vector2 textSize, float fontSpacing, Color color, Vector2 alignement)
        {
            DrawTextAlignedPro(text, uiPos, rotDeg, textSize, fontSpacing, color, UIHandler.GetFont(), alignement);
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, float textHeight, float fontSpacing, Color color, Vector2 alignement)
        {
            DrawTextAlignedPro(text, uiPos, rotDeg, textHeight, fontSpacing, color, UIHandler.GetFont(), alignement);
        }
        public static void DrawTextAlignedPro2(string text, Vector2 uiPos, float rotDeg, float textWidth, float fontSpacing, Color color, Vector2 alignement)
        {
            DrawTextAlignedPro2(text, uiPos, rotDeg, textWidth, fontSpacing, color, UIHandler.GetFont(), alignement);
        }
        public static void DrawTextAlignedPro3(string text, Vector2 uiPos, float rotDeg, float fontSize, float fontSpacing, Color color, Vector2 alignement)
        {
            DrawTextAlignedPro3(text, uiPos, rotDeg, fontSize, fontSpacing, color, UIHandler.GetFont(), alignement);
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, Vector2 textSize, float fontSpacing, Color color, string fontName, Vector2 alignement)
        {
            DrawTextAlignedPro(text, uiPos, rotDeg, textSize, fontSpacing, color,   UIHandler.GetFont(fontName), alignement);
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, float textHeight, float fontSpacing, Color color, string fontName, Vector2 alignement)
        {
            DrawTextAlignedPro(text, uiPos, rotDeg, textHeight, fontSpacing, color, UIHandler.GetFont(fontName), alignement);
        }
        public static void DrawTextAlignedPro2(string text, Vector2 uiPos, float rotDeg, float textWidth, float fontSpacing, Color color, string fontName, Vector2 alignement)
        {
            DrawTextAlignedPro2(text, uiPos, rotDeg, textWidth, fontSpacing, color, UIHandler.GetFont(fontName), alignement);
        }
        public static void DrawTextAlignedPro3(string text, Vector2 uiPos, float rotDeg, float fontSize, float fontSpacing, Color color, string fontName, Vector2 alignement)
        {
            DrawTextAlignedPro3(text, uiPos, rotDeg, fontSize, fontSpacing, color,  UIHandler.GetFont(fontName), alignement);
        }



        public static void DrawTextAligned(string text, Vector2 uiPos, Vector2 textSize, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = UIHandler.CalculateDynamicFontSize(text, textSize, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 topLeft = uiPos - alignement * fontDimensions;
            DrawTextEx(font, text, topLeft, fontSize, fontSpacing, color);
            //DrawRectangleLinesEx(new(topLeft.X, topLeft.Y, fontDimensions.X, fontDimensions.Y), 5f, WHITE);
        }
        public static void DrawTextAligned(string text, Rectangle rect, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            Vector2 textSize = SRect.GetRectSize(rect);
            Vector2 uiPos = SRect.GetRectPos(rect, alignement);
            float fontSize = UIHandler.CalculateDynamicFontSize(text, textSize, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 topLeft = uiPos - alignement * fontDimensions;
            DrawTextEx(font, text, topLeft, fontSize, fontSpacing, color);
        }
        public static void DrawTextAligned(string text, Vector2 uiPos, float textHeight, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = UIHandler.CalculateDynamicFontSize(textHeight, font);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            DrawTextEx(font, text, uiPos - alignement * fontDimensions, fontSize, fontSpacing, color);
        }
        public static void DrawTextAligned2(string text, Vector2 uiPos, float textWidth, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = UIHandler.CalculateDynamicFontSize(text, textWidth, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            DrawTextEx(font, text, uiPos - alignement * fontDimensions, fontSize, fontSpacing, color);
        }
        public static void DrawTextAligned3(string text, Vector2 uiPos, float fontSize, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 topLeft = uiPos - alignement * fontDimensions;
            DrawTextEx(font, text, topLeft, fontSize, fontSpacing, color);
            DrawRectangleLinesEx(new(topLeft.X, topLeft.Y, fontDimensions.X, fontDimensions.Y), 5f, WHITE);
        }


        public static void DrawTextAligned(string text, Vector2 uiPos, Vector2 textSize, float fontSpacing, Color color, Vector2 alignement)
        {
            DrawTextAligned(text, uiPos, textSize, fontSpacing, color, UIHandler.GetFont(), alignement);
        }
        public static void DrawTextAligned(string text, Vector2 uiPos, float textHeight, float fontSpacing, Color color, Vector2 alignement)
        {
            DrawTextAligned(text, uiPos, textHeight, fontSpacing, color, UIHandler.GetFont(), alignement);
        }
        public static void DrawTextAligned2(string text, Vector2 uiPos, float textWidth, float fontSpacing, Color color, Vector2 alignement)
        {
            DrawTextAligned2(text, uiPos, textWidth, fontSpacing, color, UIHandler.GetFont(), alignement);
        }
        public static void DrawTextAligned3(string text, Vector2 uiPos, float fontSize, float fontSpacing, Color color, Vector2 alignement)
        {
            DrawTextAligned3(text, uiPos, fontSize, fontSpacing, color, UIHandler.GetFont(), alignement);
        }
        public static void DrawTextAligned(string text, Rectangle textRect, float fontSpacing, Color color, Vector2 alignement)
        {
            DrawTextAligned(text, textRect, fontSpacing, color, UIHandler.GetFont(), alignement);
            //DrawTextAligned(text, new Vector2(textRect.X, textRect.Y), new Vector2(textRect.width, textRect.height), fontSpacing, color, UIHandler.GetFont(), alignement);
        }
        public static void DrawTextAligned(string text, Vector2 uiPos, Vector2 textSize, float fontSpacing, Color color, string fontName, Vector2 alignement)
        {
            DrawTextAligned(text, uiPos, textSize, fontSpacing, color, UIHandler.GetFont(fontName), alignement);
        }
        public static void DrawTextAligned(string text, Vector2 uiPos, float fontHeight, float fontSpacing, Color color, string fontName, Vector2 alignement)
        {
            DrawTextAligned(text, uiPos, fontHeight, fontSpacing, color, UIHandler.GetFont(fontName), alignement);
        }
        public static void DrawTextAligned2(string text, Vector2 uiPos, float fontWidth, float fontSpacing, Color color, string fontName, Vector2 alignement)
        {
            DrawTextAligned2(text, uiPos, fontWidth, fontSpacing, color, UIHandler.GetFont(fontName), alignement);
        }
        public static void DrawTextAligned3(string text, Vector2 uiPos, float fontSize, float fontSpacing, Color color, string fontName, Vector2 alignement)
        {
            DrawTextAligned3(text, uiPos, fontSize, fontSpacing, color, UIHandler.GetFont(fontName), alignement);
        }
        public static void DrawTextAligned(string text, Rectangle textRect, float fontSpacing, Color color, string fontName, Vector2 alignement)
        {
            DrawTextAligned(text, textRect, fontSpacing, color, UIHandler.GetFont(fontName), alignement);
            //DrawTextAligned(text, new Vector2(textRect.X, textRect.Y), new Vector2(textRect.width, textRect.height), fontSpacing, color, UIHandler.GetFont(fontName), alignement);
        }






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
            DrawCircleV(points[0], lineThickness * 0.5f, outlineColor);
            for (int i = 0; i < points.Count - 1; i++)
            {
                DrawCircleV(points[i + 1], lineThickness * 0.5f, outlineColor);
                DrawLineEx(points[i], points[i + 1], lineThickness, outlineColor);
            }
            DrawLineEx(points[points.Count - 1], points[0], lineThickness, outlineColor);
        }
        public static void DrawPolygon(List<Vector2> points, float lineThickness, Color outlineColor, Vector2 center)
        {
            DrawCircleV(center + points[0], lineThickness * 0.5f, outlineColor);
            for (int i = 0; i < points.Count - 1; i++)
            {
                DrawCircleV(center + points[i + 1], lineThickness * 0.5f, outlineColor);
                DrawLineEx(center + points[i], center + points[i + 1], lineThickness, outlineColor);
            }
            DrawLineEx(center + points[points.Count - 1], center + points[0], lineThickness, outlineColor);
        }
        public static void DrawPolygon(List<Vector2> points, Vector2 center, Color fillColor, float lineThickness, Color outlineColor, bool clockwise = true)
        {
            DrawPolygon(points, center, fillColor, clockwise);
            DrawPolygon(points, lineThickness, outlineColor);
        }


        public static void DrawGlowLine(Vector2 start, Vector2 end, float width, float endWidth, Color color, Color endColor, int steps)
        {
            float wStep = (endWidth - width) / steps;

            float rStep = (endColor.r - color.r) / steps;
            float gStep = (endColor.g - color.g) / steps;
            float bStep = (endColor.b - color.b) / steps;
            float aStep = (endColor.a - color.a) / steps;

            for (int i = steps; i >= 0; i--)
            {
                DrawLineEx
                (
                    start, end, width + wStep * i, 
                    new
                    (
                        (int)(color.r + rStep * i), 
                        (int)(color.g + gStep * i), 
                        (int)(color.b + bStep * i), 
                        (int)(color.a + aStep * i)
                    )
                );
            }
        }


        public static void DrawRectangleCheckeredLines(Vector2 pos, Vector2 size, Vector2 alignement, float spacing, float lineThickness, float angleDeg, Color lineColor, Color outlineColor, Color bgColor)
        {
            float maxDimension = MathF.Max(size.X, size.Y);
            Vector2 aVector = alignement * size;
            Vector2 center = pos - aVector + size / 2;
            float rotRad = angleDeg * DEG2RAD;

            var rect = SRect.ConstructRect(pos, size, alignement);
            Vector2 tl = new(rect.x, rect.y);
            Vector2 tr = new(rect.x + rect.width, rect.y);
            Vector2 bl = new(rect.x, rect.y + rect.height);
            Vector2 br = new(rect.x + rect.width, rect.y + rect.height);
            //List<(Vector2 start, Vector2 end)> segments = new()
            //{
            //    (tl, tr), (bl, br), (tl, bl), (tr, br)
            //};

            if (bgColor.a > 0) DrawRectangleRec(rect, bgColor);

            Vector2 cur = new(-spacing / 2, 0f);

            //safety for while loops
            int whileMaxCount = (int)(maxDimension / spacing) * 2;
            int whileCounter = 0;

            //left half of rectangle
            while (whileCounter < whileMaxCount)
            {
                Vector2 p = center + SVec.Rotate(cur, rotRad);
                Vector2 up = new(0f, -maxDimension * 2);//make sure that lines are going outside of the rectangle
                Vector2 down = new(0f, maxDimension * 2);
                Vector2 start = p + SVec.Rotate(up, rotRad);
                Vector2 end = p + SVec.Rotate(down, rotRad);
                List<(Vector2 p, Vector2 n)> intersections = ShapeCollision.SGeometry.IntersectionSegmentRect(start, end, tl, tr, br, bl).points;
                
                if (intersections.Count >= 2) DrawLineEx(intersections[0].p, intersections[1].p, lineThickness, lineColor);
                else break;
                cur.X -= spacing;
                whileCounter++;
            } 

            cur = new(spacing / 2, 0f);
            whileCounter = 0;
            //right half of rectangle
            while (whileCounter < whileMaxCount)
            {
                Vector2 p = center + SVec.Rotate(cur, rotRad);
                Vector2 up = new(0f, -maxDimension * 2);
                Vector2 down = new(0f, maxDimension * 2);
                Vector2 start = p + SVec.Rotate(up, rotRad);
                Vector2 end = p + SVec.Rotate(down, rotRad);
                List<(Vector2 p, Vector2 n)> intersections = ShapeCollision.SGeometry.IntersectionSegmentRect(start, end, tl, tr, br, bl).points;
                if (intersections.Count >= 2) DrawLineEx(intersections[0].p, intersections[1].p, lineThickness, lineColor);
                else break;
                cur.X += spacing;
                whileCounter++;
            }

            if (outlineColor.a > 0) SDrawing.DrawRectangeLinesPro(pos, size, alignement, new Vector2(0.5f, 0.5f), 0f, lineThickness, outlineColor);
        }
        public static void DrawCircleCheckeredLines(Vector2 pos, Vector2 alignement, float radius, float spacing, float lineThickness, float angleDeg, Color lineColor, Color bgColor)
        {

            float maxDimension = radius;
            Vector2 size = new Vector2(radius, radius) * 2f;
            Vector2 aVector = alignement * size;
            Vector2 center = pos - aVector + size / 2;
            float rotRad = angleDeg * DEG2RAD;

            if (bgColor.a > 0) DrawCircleV(center, radius, bgColor);

            Vector2 cur = new(-spacing / 2, 0f);
            while (cur.X > -maxDimension)
            {
                Vector2 p = center + SVec.Rotate(cur, rotRad);

                //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
                float angle = MathF.Acos(cur.X / radius);
                float y = radius * MathF.Sin(angle);

                Vector2 up = new(0f, -y);
                Vector2 down = new(0f, y);
                Vector2 start = p + SVec.Rotate(up, rotRad);
                Vector2 end = p + SVec.Rotate(down, rotRad);
                DrawLineEx(start, end, lineThickness, lineColor);
                cur.X -= spacing;
            }

            cur = new(spacing / 2, 0f);
            while (cur.X < maxDimension)
            {
                Vector2 p = center + SVec.Rotate(cur, rotRad);
                //float y = MathF.Sqrt((radius * radius) - (cur.X * cur.X));
                float angle = MathF.Acos(cur.X / radius);
                float y = radius * MathF.Sin(angle);

                Vector2 up = new(0f, -y);
                Vector2 down = new(0f, y);
                Vector2 start = p + SVec.Rotate(up, rotRad);
                Vector2 end = p + SVec.Rotate(down, rotRad);
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
                Vector2 sectorStart = center + SVec.Rotate(SVec.Right() * radius + new Vector2(lineThickness / 2, 0), startAngleRad);
                DrawLineEx(center, sectorStart, lineThickness, color);

                Vector2 sectorEnd = center + SVec.Rotate(SVec.Right() * radius + new Vector2(lineThickness / 2, 0), endAngleRad);
                DrawLineEx(center, sectorEnd, lineThickness, color);
            }
            for (int i = 0; i < sides; i++)
            {
                Vector2 start = center + SVec.Rotate(SVec.Right() * radius, startAngleRad + angleStep * i);
                Vector2 end = center + SVec.Rotate(SVec.Right() * radius, startAngleRad + angleStep * (i + 1));
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
                Vector2 sectorStart = center + SVec.Rotate(SVec.Right() * radius + new Vector2(lineThickness / 2, 0), startAngleRad);
                DrawLineEx(center, sectorStart, lineThickness, color);

                Vector2 sectorEnd = center + SVec.Rotate(SVec.Right() * radius + new Vector2(lineThickness / 2, 0), endAngleRad);
                DrawLineEx(center, sectorEnd, lineThickness, color);
            }
            for (int i = 0; i < sides; i++)
            {
                Vector2 start = center + SVec.Rotate(SVec.Right() * radius, startAngleRad + angleStep * i);
                Vector2 end = center + SVec.Rotate(SVec.Right() * radius, startAngleRad + angleStep * (i + 1));
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
            Vector2 innerStart = center + SVec.Rotate(SVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), startAngleRad);
            Vector2 outerStart = center + SVec.Rotate(SVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), startAngleRad);
            DrawLineEx(innerStart, outerStart, lineThickness, color);

            Vector2 innerEnd = center + SVec.Rotate(SVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), endAngleRad);
            Vector2 outerEnd = center + SVec.Rotate(SVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), endAngleRad);
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

        public static void DrawRectangeLinesPro(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 pivot, float rotDeg, float lineThickness, Color color)
        {
            Vector2 leftExtension = SVec.Rotate(new Vector2(-lineThickness / 2, 0f), rotDeg * DEG2RAD);
            Vector2 rightExtension = SVec.Rotate(new Vector2(lineThickness / 2, 0f), rotDeg * DEG2RAD);

            var rr = SRect.RotateRect(pos, size, alignement, pivot, rotDeg);
            DrawLineEx(rr.tl + leftExtension, rr.tr + rightExtension, lineThickness, color);
            DrawLineEx(rr.bl + leftExtension, rr.br + rightExtension, lineThickness, color);
            DrawLineEx(rr.tl, rr.bl, lineThickness, color);
            DrawLineEx(rr.tr, rr.br, lineThickness, color);
        }
        public static void DrawRectangle(Rectangle rect, Color color)
        {
            DrawRectangleRec(rect, color);
        }
        public static void DrawRectangle(Vector2 pos, Vector2 size, Vector2 alignement, Color color)
        {
            DrawRectangle(SRect.ConstructRect(pos, size, alignement), color);
        }

        public static void DrawRectangle(Rectangle rect, Vector2 pivot, float rotDeg, Color color)
        {
            var rr = SRect.RotateRect(rect, pivot, rotDeg);
            Raylib.DrawTriangle(rr.tl, rr.bl, rr.br, color);
            Raylib.DrawTriangle(rr.br, rr.tr, rr.tl, color);
        }

        public static void DrawRectangle(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 pivot, float rotDeg, Color color)
        {
            DrawRectangle(SRect.ConstructRect(pos, size, alignement), pivot, rotDeg, color);
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
            float newF = SUtils.RemapFloat(f, fMin, fMax, 0f, 1f);
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
                if (i == lines - 1) end = SVec.Lerp(start, end, newF);
                DrawLineEx(start, end, thickness, color);
            }
        }
        public static void DrawRectangleOutlineBar(Vector2 pos, Vector2 size, Vector2 alignement, float thickness, float f, Color color)
        {
            DrawRectangleOutlineBar(SRect.ConstructRect(pos, size, alignement), thickness, f, color);
        }


        public static void DrawRectangleOutlineBar(Rectangle rect, Vector2 pivot, float angleDeg, float thickness, float f, Color color)
        {
            var rr = SRect.RotateRect(rect, pivot, angleDeg);
            //Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
            //Vector2 thicknessOffsetY = new Vector2(0f, thickness);
            
            Vector2 leftExtension = SVec.Rotate(new Vector2(-thickness / 2, 0f), angleDeg * DEG2RAD);
            Vector2 rightExtension = SVec.Rotate(new Vector2(thickness / 2, 0f), angleDeg * DEG2RAD);

            Vector2 tl = rr.tl;
            Vector2 br = rr.br;
            Vector2 tr = rr.tr;
            Vector2 bl = rr.bl;

            int lines = (int)MathF.Ceiling(4 * Clamp(f, 0f, 1f));
            float fMin = 0.25f * (lines - 1);
            float fMax = fMin + 0.25f;
            float newF = SUtils.RemapFloat(f, fMin, fMax, 0f, 1f);
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
                if (i == lines - 1) end = SVec.Lerp(start, end, newF);
                DrawLineEx(start, end, thickness, color);
            }
        }
        public static void DrawRectangleOutlineBar(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 pivot, float angleDeg, float thickness, float f, Color color)
        {
            DrawRectangleOutlineBar(SRect.ConstructRect(pos, size, alignement), pivot, angleDeg, thickness, f, color);
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
            SDrawing.DrawRectangle(rect, bgColor);
            SDrawing.DrawRectangle(progressRect, barColor);
        }
        public static void DrawBar(Rectangle rect, Vector2 pivot, float angleDeg, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            f = 1.0f - f;
            UIMargins progressMargins = new(f * top, f * right, f * bottom, f * left);
            var progressRect = progressMargins.Apply(rect);
            SDrawing.DrawRectangle(rect, pivot, angleDeg, bgColor);
            SDrawing.DrawRectangle(progressRect, pivot, angleDeg, barColor);
        }
        
        public static void DrawBar(Vector2 pos, Vector2 size, Vector2 alignement, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            DrawBar(SRect.ConstructRect(pos, size, alignement), f, barColor, bgColor, left, right, top, bottom);
        }
        public static void DrawBar(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 pivot, float angleDeg, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            DrawBar(SRect.ConstructRect(pos, size, alignement), pivot, angleDeg, f, barColor, bgColor, left, right, top, bottom);
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
