
using System.Numerics;
using Raylib_CsLo;
using ShapeCore;
using ShapeUI;

namespace ShapeLib
{
    public static class SDrawing
    {

        #region Pixel
        public static void DrawPixel(Vector2 pos, Color color) => Raylib.DrawPixelV(pos, color); 
        public static void DrawPixel(float x, float y, Color color) => Raylib.DrawPixelV(new(x, y), color);
        #endregion

        #region Line
        public static void Draw(this Segment line, float thickness, Color color) => Raylib.DrawLineEx(line.start, line.end, thickness, color);
        public static void DrawLine(float x1, float y1, float x2, float y2, float thickness, Color color) => Raylib.DrawLineEx(new(x1, y1), new(x2, y2), thickness, color);
        public static void DrawLine(Vector2 start, Vector2 end, float thickness, Color color) => Raylib.DrawLineEx(start, end, thickness, color);
        public static void DrawLines(List<Segment> lines, float thickness, Color color, bool smoothJoints) 
        {
            if(lines.Count <= 0) return;
            foreach (var line in lines)
            {
                if(smoothJoints)Raylib.DrawCircleV(line.start, thickness / 2, color);
                line.Draw(thickness, color);
            }
            if (smoothJoints) Raylib.DrawCircleV(lines[lines.Count - 1].end, thickness / 2, color);
        }
        public static void DrawLines(List<Segment> lines, float thickness, List<Color> colors, bool smoothJoints)
        {
            if (lines.Count <= 0 || colors.Count <= 0) return;
            for (int i = 0; i < lines.Count; i++)
            {
                Color c = colors[i % colors.Count];
                if (smoothJoints) Raylib.DrawCircleV(lines[i].start, thickness / 2, c);
                lines[i].Draw(thickness, c);
            }
            if (smoothJoints) Raylib.DrawCircleV(lines[lines.Count - 1].end, thickness / 2, colors[(lines.Count - 1) % colors.Count]);
        }
        public static void DrawLines(List<Vector2> lines, float thickness, Color color, bool smoothJoints)
        {
            if (lines.Count < 2) return;
            for(int i = 0; i < lines.Count - 1; i++)
            {
                if (smoothJoints) Raylib.DrawCircleV(lines[i], thickness / 2, color);
                Draw(new Segment(lines[i], lines[i+1]), thickness, color);
            }
            if (smoothJoints) Raylib.DrawCircleV(lines[lines.Count - 1], thickness / 2, color);
        }
        public static void DrawLines(List<Vector2> lines, float thickness, List<Color> colors, bool smoothJoints)
        {
            if (lines.Count < 2 || colors.Count <= 0) return;
            for (int i = 0; i < lines.Count - 1; i++)
            {
                Color c = colors[i % colors.Count];
                if (smoothJoints) Raylib.DrawCircleV(lines[i], thickness / 2, c);
                Draw(new Segment(lines[i], lines[i + 1]), thickness, c);
            }
            if (smoothJoints) Raylib.DrawCircleV(lines[lines.Count - 1], thickness / 2, colors[(lines.Count - 1) % colors.Count]);
        }
       
        public static List<Segment> CreateLightningLine(this Segment line, int segments = 10, float maxSway = 80f)
        {
            List<Segment> result = new();
            Vector2 w = line.end - line.start;
            Vector2 dir = SVec.Normalize(w);
            Vector2 n = new Vector2(dir.Y, -dir.X);
            float length = w.Length();

            float prevDisplacement = 0;
            Vector2 cur = line.start;
            //result.Add(start);

            float segmentLength = length / segments;
            float remainingLength = length;
            List<Vector2> accumulator = new()
            {
                line.start
            };
            while (remainingLength > 0f)
            {
                float randSegmentLength = SRNG.randF() * segmentLength;
                remainingLength -= randSegmentLength;
                if (remainingLength <= 0f)
                {
                    if(accumulator.Count == 1)
                    {
                        result.Add(new(accumulator[0], line.end));
                    }
                    else
                    {
                        result.Add(new(result[result.Count - 1].end, line.end));
                    }
                    break;
                }
                float scale = randSegmentLength / segmentLength;
                float displacement = SRNG.randF(-maxSway, maxSway);
                displacement -= (displacement - prevDisplacement) * (1 - scale);
                cur = cur + dir * randSegmentLength;
                Vector2 p = cur + displacement * n;
                accumulator.Add(p);
                if(accumulator.Count == 2)
                {
                    result.Add(new(accumulator[0], accumulator[1]));
                    accumulator.Clear();
                }
                prevDisplacement = displacement;
            }
            return result;
        }
        public static List<Segment> CreateLightningLine(this Segment line, float segmentLength = 5f, float maxSway = 80f)
        {
            List<Segment> result = new();
            Vector2 w = line.end - line.start;
            Vector2 dir = SVec.Normalize(w);
            Vector2 n = new Vector2(dir.Y, -dir.X);
            float length = w.Length();

            float prevDisplacement = 0;
            Vector2 cur = line.start;
            List<Vector2> accumulator = new()
            {
                line.start
            };
            float remainingLength = length;
            while (remainingLength > 0f)
            {
                float randSegmentLength = SRNG.randF() * segmentLength;
                remainingLength -= randSegmentLength;
                if (remainingLength <= 0f)
                {
                    if (accumulator.Count == 1)
                    {
                        result.Add(new(accumulator[0], line.end));
                    }
                    else
                    {
                        result.Add(new(result[result.Count - 1].end, line.end));
                    }
                    break;
                }
                float scale = randSegmentLength / segmentLength;
                float displacement = SRNG.randF(-maxSway, maxSway);
                displacement -= (displacement - prevDisplacement) * (1 - scale);
                cur = cur + dir * randSegmentLength;
                Vector2 p = cur + displacement * n;
                accumulator.Add(p);
                if (accumulator.Count == 2)
                {
                    result.Add(new(accumulator[0], accumulator[1]));
                    accumulator.Clear();
                }
                prevDisplacement = displacement;
            }
            return result;
        }
        public static List<Vector2> CreateLightningLine(Vector2 start, Vector2 end, int segments = 10, float maxSway = 80f)
        {
            List<Vector2> result = new();
            Vector2 w = end - start;
            Vector2 dir = SVec.Normalize(w);
            Vector2 n = new Vector2(dir.Y, -dir.X);
            float length = w.Length();

            float prevDisplacement = 0;
            Vector2 cur = start;
            result.Add(start);

            float segmentLength = length / segments;
            float remainingLength = length;
            while (remainingLength > 0f)
            {
                float randSegmentLength = SRNG.randF() * segmentLength;
                remainingLength -= randSegmentLength;
                if (remainingLength <= 0f)
                {
                    result.Add(end);
                    break;
                }
                float scale = randSegmentLength / segmentLength;
                float displacement = SRNG.randF(-maxSway, maxSway);
                displacement -= (displacement - prevDisplacement) * (1 - scale);
                cur = cur + dir * randSegmentLength;
                Vector2 p = cur + displacement * n;
                result.Add(p);
                prevDisplacement = displacement;
            }
            return result;
        }
        public static List<Vector2> CreateLightningLine(Vector2 start, Vector2 end, float segmentLength = 5f, float maxSway = 80f)
        {
            List<Vector2> result = new();
            Vector2 w = end - start;
            Vector2 dir = SVec.Normalize(w);
            Vector2 n = new Vector2(dir.Y, -dir.X);
            float length = w.Length();

            float prevDisplacement = 0;
            Vector2 cur = start;
            result.Add(start);
            float remainingLength = length;
            while (remainingLength > 0f)
            {
                float randSegmentLength = SRNG.randF() * segmentLength;
                remainingLength -= randSegmentLength;
                if (remainingLength <= 0f)
                {
                    result.Add(end);
                    break;
                }
                float scale = randSegmentLength / segmentLength;
                float displacement = SRNG.randF(-maxSway, maxSway);
                displacement -= (displacement - prevDisplacement) * (1 - scale);
                cur = cur + dir * randSegmentLength;
                Vector2 p = cur + displacement * n;
                result.Add(p);
                prevDisplacement = displacement;
            }
            return result;
        }
        public static void DrawLineDotted(this Segment line, int gaps, float thickness, Color color, bool roundedLineEdges = false)
        {
            DrawLineDotted(line.start, line.end, gaps, thickness, color, roundedLineEdges);
        }
        public static void DrawLineDotted(this Segment line, int gaps, float gapSizeF, float thickness, Color color, bool roundedLineEdges = false)
        {
            DrawLineDotted(line.start, line.end, gaps, gapSizeF, thickness, color, roundedLineEdges);
        }
        public static void DrawLineDotted(Vector2 start, Vector2 end, int gaps, float thickness, Color color, bool roundedLineEdges = false)
        {
            if (gaps <= 0) DrawLineEx(start, end, thickness, color);
            else
            {
                Vector2 w = end - start;
                float l = w.Length();
                Vector2 dir = w / l;
                int totalGaps = gaps * 2 + 1;
                float size = l / totalGaps;
                Vector2 offset = dir * size;

                Vector2 cur = start;
                for (int i = 0; i < totalGaps; i++)
                {
                    if (i % 2 == 0)
                    {
                        Vector2 next = cur + offset;
                        if (roundedLineEdges)
                        {
                            DrawCircleV(cur, thickness * 0.5f, color);
                            DrawCircleV(next, thickness * 0.5f, color);
                        }
                        DrawLineEx(cur, next, thickness, color);
                        cur = next;

                    }
                    else
                    {
                        cur = cur + offset; //gap
                    }
                }
            }
        }
        public static void DrawLineDotted(Vector2 start, Vector2 end, int gaps, float gapSizeF, float thickness, Color color, bool roundedLineEdges = false)
        {
            if (gaps <= 0) DrawLineEx(start, end, thickness, color);
            else
            {
                Vector2 w = end - start;
                float l = w.Length();
                Vector2 dir = w / l;

                float totalGapSize = l * gapSizeF;
                float remaining = l - totalGapSize;
                float gapSize = totalGapSize / gaps;
                float size = remaining / (gaps + 1);

                Vector2 gapOffset = dir * gapSize;
                Vector2 offset = dir * size;

                int totalGaps = gaps * 2 + 1;
                Vector2 cur = start;
                for (int i = 0; i < totalGaps; i++)
                {
                    if (i % 2 == 0)
                    {
                        Vector2 next = cur + offset;
                        if (roundedLineEdges)
                        {
                            DrawCircleV(cur, thickness * 0.5f, color);
                            DrawCircleV(next, thickness * 0.5f, color);
                        }
                        DrawLineEx(cur, next, thickness, color);
                        cur = next;
                    }
                    else
                    {
                        cur = cur + gapOffset; //gap
                    }
                }
            }
        }
        public static void DrawLineGlow(this Segment line, float width, float endWidth, Color color, Color endColor, int steps)
        {
            DrawLineGlow(line.start, line.end, width, endWidth, color, endColor, steps);
        }
        public static void DrawLinesGlow(List<Segment> lines, float width, float endWidth, Color color, Color endColor, int steps)
        {
            foreach (var line in lines)
            {
                DrawLineGlow(line, width, endWidth, color, endColor, steps);
            }
        }
        public static void DrawLineGlow(Vector2 start, Vector2 end, float width, float endWidth, Color color, Color endColor, int steps)
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
        public static void DrawLinesGlow(List<Vector2> points, float width, float endWidth, Color color, Color endColor, int steps)
        {
            if (points.Count < 2) return;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 cur = points[i];
                Vector2 next = points[i + 1];
                DrawLineGlow(cur, next, width, endWidth, color, endColor, steps);
            }
        }

        //public static void DrawLine(this Vector2 start, Vector2 end, float lineThickness, Color color) { Raylib.DrawLineEx(start, end, lineThickness, color); }
        //public static void DrawLines(List<Vector2> points, float lineThickness, Color color)
        //{
        //    if(points.Count < 2) return;
        //    for (int i = 0; i < points.Count - 1; i++)
        //    {
        //        DrawLine(points[i], points[i+1], lineThickness, color); 
        //    }
        //}
        //public static List<Vector2> CreateLightningLine(Vector2 start, Vector2 end, float segmentLength = 5f, float maxSway = 80f)
        //{
        //    List<Vector2> result = new();
        //    Vector2 tangent = end - start;
        //    Vector2 normal = Vector2.Normalize(new Vector2(tangent.Y, -tangent.X));
        //    float length = tangent.Length();
        //    List<float> positions = new() { 0 };
        //    for (int i = 0; i < length / segmentLength; i++)
        //        positions.Add(SRNG.randF());
        //    positions.Sort();
        //    float Sway = maxSway;
        //    float Jaggedness = 1 / Sway;
        //    float prevDisplacement = 0;
        //    result.Add(start);
        //    for (int i = 1; i < positions.Count; i++)
        //    {
        //        float pos = positions[i];
        //        // used to prevent sharp angles by ensuring very close positions also have small perpendicular variation. 
        //        float scale = (length * Jaggedness) * (pos - positions[i - 1]);
        //        // defines an envelope. Points near the middle of the bolt can be further from the central line. 
        //        float envelope = pos > 0.95f ? 20 * (1 - pos) : 1;
        //        float displacement = SRNG.randF(-Sway, Sway);
        //        displacement -= (displacement - prevDisplacement) * (1 - scale);
        //        displacement *= envelope;
        //        Vector2 point = start + pos * tangent + displacement * normal;
        //        result.Add(point);
        //        prevDisplacement = displacement;
        //    }
        //    result.Add(end); 
        //    return result;
        //}
        //public static void DrawLineLightning(Vector2 start, Vector2 end, float lineThickness, Color color, float segmentLength = 5f, float maxSway = 80)
        //{
        //    Vector2 tangent = end - start;
        //    Vector2 normal = Vector2.Normalize(new Vector2(tangent.Y, -tangent.X));
        //    float length = tangent.Length();
        //    List<float> positions = new() { 0 };
        //    for (int i = 0; i < length / segmentLength; i++)
        //        positions.Add(SRNG.randF());
        //    positions.Sort();
        //    float Sway = maxSway;
        //    float Jaggedness = 1 / Sway;
        //    Vector2 prevPoint = start;
        //    float prevDisplacement = 0;
        //    for (int i = 1; i < positions.Count; i++)
        //    {
        //        float pos = positions[i];
        //        // used to prevent sharp angles by ensuring very close positions also have small perpendicular variation. 
        //        float scale = (length * Jaggedness) * (pos - positions[i - 1]);
        //        // defines an envelope. Points near the middle of the bolt can be further from the central line. 
        //        float envelope = pos > 0.95f ? 20 * (1 - pos) : 1;
        //        float displacement = SRNG.randF(-Sway, Sway);
        //        displacement -= (displacement - prevDisplacement) * (1 - scale);
        //        displacement *= envelope;
        //        Vector2 point = start + pos * tangent + displacement * normal;
        //        DrawLineEx(prevPoint, point, lineThickness, color);
        //        prevPoint = point;
        //        prevDisplacement = displacement;
        //    }
        //    DrawLineEx(prevPoint, end, lineThickness, color);
        //    DrawCircleV(start, lineThickness * 2, color);
        //    DrawCircleV(end, lineThickness * 2, color);
        //    //results.Add(new Line(prevPoint, dest, thickness));
        //}
        //public static void DrawLineLightning(Vector2 start, Vector2 end, float maxAngleDeg, Vector2 sizeFRange, float lineThickness, Color color)
        //{
        //    
        //    Vector2 w = (end - start);
        //    float l = w.Length();
        //    Vector2 dir = w / l;
        //    float maxAngleRad = maxAngleDeg * SUtils.DEGTORAD;
        //
        //    float minSize = sizeFRange.X * l;
        //    float maxSize = sizeFRange.Y * l;
        //
        //    DrawCircleV(start, lineThickness * 4, RED);
        //    DrawCircleV(end, lineThickness * 4, RED);
        //    DrawCircleV(end, minSize, new(255, 0, 0, 50));
        //    DrawLineEx(start, end, lineThickness * 2, RED);
        //    Vector2 cur = start;
        //    Vector2 curGuide = cur;
        //    float remainingLength = l;
        //    while(true)
        //    {
        //        float randSize = SRNG.randF(minSize, maxSize);
        //        remainingLength -= randSize;
        //        if(remainingLength <= minSize)
        //        {
        //            DrawLineEx(cur, end, lineThickness, color);
        //            break;
        //        }
        //        Vector2 randP = curGuide + dir * randSize;
        //        DrawCircleV(randP, lineThickness * 4, RED);
        //        Vector2 next = SVec.Rotate(randP, SRNG.randF(-maxAngleRad, maxAngleRad));
        //        //Vector2 randDisplacement = SVec.Rotate(randP, SRNG.randF(-maxAngleRad, maxAngleRad));
        //        //Vector2 next = cur + randDisplacement;
        //        DrawLineEx(cur, next, lineThickness, color);
        //        cur = next;
        //        curGuide = randP;
        //    }
        //    //DrawLineEx(cur, end, lineThickness, color);
        //}
        //public static void DrawLines(List<Vector2> points, float lineThickness, Color color, bool smoothJoints = false)
        //{
        //    if (points.Count < 2) return;
        //    if (smoothJoints) DrawCircleV(points[0], lineThickness / 2, color);
        //    for (int i = 0; i < points.Count - 1; i++)
        //    {
        //        Vector2 cur = points[i];
        //        Vector2 next = points[i + 1];
        //        DrawCircleV(next, lineThickness / 2, color);
        //        DrawLineEx(cur, next, lineThickness, color);
        //    }
        //}

        #endregion

        #region Circle
        public static void Draw(this Circle c, Color color) => Raylib.DrawCircleV(c.center, c.radius, color);
        public static void DrawLines(this Circle c, float lineThickness, int sides, Color color) => DrawPolyLinesEx(c.center, sides, c.radius, 0f, lineThickness, color);
        public static void DrawLines(this Circle c, float lineThickness, float rotDeg, int sides, Color color) => DrawPolyLinesEx(c.center, sides, c.radius, rotDeg, lineThickness, color);
        public static void DrawLines(this Circle c, float lineThickness, Color color, float sideLength = 8f)
        {
            int sides = GetCircleSideCount(c.radius, sideLength);
            DrawPolyLinesEx(c.center, sides, c.radius, 0f, lineThickness, color);
        }
        
        public static void DrawCircle(Vector2 center, float radius, Color color) => Raylib.DrawCircleV(center, radius, color);
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, int sides, Color color) => DrawPolyLinesEx(center, sides, radius, 0f, lineThickness, color);
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, float rotDeg, int sides, Color color) => DrawPolyLinesEx(center, sides, radius, rotDeg, lineThickness, color);
        public static void DrawCircleLines(Vector2 center, float radius, float lineThickness, Color color, float sideLength = 8f)
        {
            int sides = GetCircleSideCount(radius, sideLength);
            DrawPolyLinesEx(center, sides, radius, 0f, lineThickness, color);
        }

        public static void DrawCircleSector(this Circle c, float startAngleDeg, float endAngleDeg, int segments, Color color)
        {
            Raylib.DrawCircleSector(c.center, c.radius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), segments, color);
        }
        public static void DrawCircleSector(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int segments, Color color)
        {
            Raylib.DrawCircleSector(center, radius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), segments, color);
        }
        
        public static void DrawCircleSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float lineThickness, Color color, bool closed = true, float sideLength = 8f)
        {
            DrawCircleSectorLines(c.center, c.radius, startAngleDeg, endAngleDeg, lineThickness, color, closed, sideLength);
        }
        public static void DrawCircleSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, Color color, bool closed = true, float sideLength = 8f)
        {
            DrawCircleSectorLines(c.center, c.radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, closed, sideLength); ;
        }
        public static void DrawCircleSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, Color color, bool closed = true)
        {
            DrawCircleSectorLines(c.center, c.radius, startAngleDeg, endAngleDeg, sides, lineThickness, color, closed);
        }
        public static void DrawCircleSectorLines(this Circle c, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, Color color, bool closed = true)
        {
            DrawCircleSectorLines(c.center, c.radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed);
        }

        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float lineThickness, Color color, bool closed = true, float sideLength = 8f)
        {
            float startAngleRad = startAngleDeg * SUtils.DEGTORAD;
            float endAngleRad = endAngleDeg * SUtils.DEGTORAD;
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
        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, Color color, bool closed = true, float sideLength = 8f)
        {
            DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, closed, sideLength); ;
        }
        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, Color color, bool closed = true)
        {
            float startAngleRad = startAngleDeg * SUtils.DEGTORAD;
            float endAngleRad = endAngleDeg * SUtils.DEGTORAD;
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
        public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, float lineThickness, Color color, bool closed = true)
        {
            DrawCircleSectorLines(center, radius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, lineThickness, color, closed);
        }

        public static void DrawCircleLinesDotted(Vector2 center, float radius, int sidesPerGap, float lineThickness, Color color, float sideLength = 8f, bool roundedLineEdges = false)
        {
            float anglePieceRad = 360 * SUtils.DEGTORAD;
            int sides = GetCircleArcSideCount(radius, MathF.Abs(anglePieceRad * RAD2DEG), sideLength);
            float angleStep = anglePieceRad / sides;

            //int totalGaps = gaps * 2 + 1;
            //float circum = 2f * PI * radius;
            //float size = circum / totalGaps;
            float size = sideLength * sidesPerGap;
            float remainingSize = size;
            bool gap = false;
            for (int i = 0; i < sides; i++)
            {
                if (!gap)
                {
                    Vector2 start = center + SVec.Rotate(SVec.Right() * radius, angleStep * i);
                    Vector2 end = center + SVec.Rotate(SVec.Right() * radius, angleStep * (i + 1));
                    if (roundedLineEdges)
                    {
                        DrawCircleV(start, lineThickness * 0.5f, color);
                        DrawCircleV(end, lineThickness * 0.5f, color);
                    }
                    DrawLineEx(start, end, lineThickness, color);
                }

                remainingSize -= sideLength;
                if (remainingSize <= 0f)
                {
                    gap = !gap;
                    remainingSize = size;
                }
            }
        }
        public static void DrawCircleCheckeredLines(Vector2 pos, Vector2 alignement, float radius, float spacing, float lineThickness, float angleDeg, Color lineColor, Color bgColor)
        {

            float maxDimension = radius;
            Vector2 size = new Vector2(radius, radius) * 2f;
            Vector2 aVector = alignement * size;
            Vector2 center = pos - aVector + size / 2;
            float rotRad = angleDeg * SUtils.DEGTORAD;

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

        #endregion

        #region Ring
        public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float lineThickness, Color color, float sideLength = 8f)
        {
            DrawCircleLines(center, innerRadius, lineThickness, color, sideLength);
            DrawCircleLines(center, outerRadius, lineThickness, color, sideLength);
        }
        public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float lineThickness, Color color, float sideLength = 8f)
        {
            DrawCircleSectorLines(center, innerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);
            DrawCircleSectorLines(center, outerRadius, startAngleDeg, endAngleDeg, lineThickness, color, false, sideLength);

            float startAngleRad = startAngleDeg * SUtils.DEGTORAD;
            float endAngleRad = endAngleDeg * SUtils.DEGTORAD;
            Vector2 innerStart = center + SVec.Rotate(SVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), startAngleRad);
            Vector2 outerStart = center + SVec.Rotate(SVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), startAngleRad);
            DrawLineEx(innerStart, outerStart, lineThickness, color);

            Vector2 innerEnd = center + SVec.Rotate(SVec.Right() * innerRadius - new Vector2(lineThickness / 2, 0), endAngleRad);
            Vector2 outerEnd = center + SVec.Rotate(SVec.Right() * outerRadius + new Vector2(lineThickness / 2, 0), endAngleRad);
            DrawLineEx(innerEnd, outerEnd, lineThickness, color);
        }
        public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, float lineThickness, Color color, float sideLength = 8f)
        {
            DrawRingLines(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, lineThickness, color, sideLength);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, Color color, float sideLength = 8f)
        {
            DrawRing(center, innerRadius, outerRadius, 0, 360, color, sideLength);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, int sides, Color color)
        {
            DrawRing(center, innerRadius, outerRadius, 0, 360, sides, color);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, Color color, float sideLength = 10f)
        {
            float start = TransformAngleDeg(startAngleDeg);
            float end = TransformAngleDeg(endAngleDeg);
            float anglePiece = end - start;
            int sides = GetCircleArcSideCount(outerRadius, MathF.Abs(anglePiece), sideLength);
            Raylib.DrawRing(center, innerRadius, outerRadius, start, end, sides, color);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, int sides, Color color)
        {
            Raylib.DrawRing(center, innerRadius, outerRadius, TransformAngleDeg(startAngleDeg), TransformAngleDeg(endAngleDeg), sides, color);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, Color color, float sideLength = 10f)
        {
            DrawRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, color, sideLength);
        }
        public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngleDeg, float endAngleDeg, float rotOffsetDeg, int sides, Color color)
        {
            DrawRing(center, innerRadius, outerRadius, startAngleDeg + rotOffsetDeg, endAngleDeg + rotOffsetDeg, sides, color);
        }

        #endregion

        #region Rectangle
        public static void DrawGrid(Rect r, int lines, float lineThickness, Color color)
        {
            //float hGap = r.width / lines;
            //float vGap = r.height / lines;
            Vector2 xOffset = new Vector2(r.width / lines, 0f);// * i;
            Vector2 yOffset = new Vector2(0f, r.height / lines);// * i;
            
            Vector2 tl = r.TopLeft;
            Vector2 tr = tl + new Vector2(r.width, 0);
            Vector2 bl = tl + new Vector2(0, r.height);

            for (int i = 0; i < lines; i++)
            {
                DrawLineEx(tl + xOffset * i, bl + xOffset * i, lineThickness, color);
                DrawLineEx(tl + yOffset * i, tr + yOffset * i, lineThickness, color);
            }
        }

        public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, Color color) => DrawRectangleRec(new Rect(topLeft, bottomRight).Rectangle, color);
        public static void DrawRect(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, Color color) => Draw(new Rect(topLeft, bottomRight),pivot, rotDeg, color);
        public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, float lineThickness, Color color) => DrawLines(new Rect(topLeft, bottomRight),lineThickness,color);
        public static void DrawRectLines(Vector2 topLeft, Vector2 bottomRight, Vector2 pivot, float rotDeg, float lineThickness, Color color, bool rounded = false)
        {
            DrawLines(new Rect(topLeft, bottomRight), pivot, rotDeg, lineThickness, color);

        }

        public static void Draw(this Rect rect, Color color) => DrawRectangleRec(rect.Rectangle, color);
        public static void Draw(this Rect rect, Vector2 pivot, float rotDeg, Color color)
        {
            var rr = rect.Rotate(pivot, rotDeg); // SRect.RotateRect(rect, pivot, rotDeg);
            Raylib.DrawTriangle(rr.tl, rr.bl, rr.br, color);
            Raylib.DrawTriangle(rr.br, rr.tr, rr.tl, color);
        }
        public static void DrawLines(this Rect rect, float lineThickness, Color color) => Raylib.DrawRectangleLinesEx(rect.Rectangle, lineThickness, color);
        public static void DrawLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, Color color, bool rounded = false)
        {
            var rr = SRect.Rotate(rect, pivot, rotDeg);

            if (rounded)
            {
                DrawCircle(rr.tl, lineThickness / 2, color);
                DrawCircle(rr.tr, lineThickness / 2, color);
                DrawCircle(rr.br, lineThickness / 2, color);
                DrawCircle(rr.bl, lineThickness / 2, color);

                DrawLineEx(rr.tl, rr.tr, lineThickness, color);
                DrawLineEx(rr.bl, rr.br, lineThickness, color);
                DrawLineEx(rr.tl, rr.bl, lineThickness, color);
                DrawLineEx(rr.tr, rr.br, lineThickness, color);
            }
            else
            {
                Vector2 leftExtension = SVec.Rotate(new Vector2(-lineThickness / 2, 0f), rotDeg * SUtils.DEGTORAD);
                Vector2 rightExtension = SVec.Rotate(new Vector2(lineThickness / 2, 0f), rotDeg * SUtils.DEGTORAD);
               
                DrawLineEx(rr.tl + leftExtension, rr.tr + rightExtension, lineThickness, color);
                DrawLineEx(rr.bl + leftExtension, rr.br + rightExtension, lineThickness, color);
                DrawLineEx(rr.tl, rr.bl, lineThickness, color);
                DrawLineEx(rr.tr, rr.br, lineThickness, color);
            }
        }

        public static void DrawRectRounded(this Rect rect, float roundness, int segments, Color color) => Raylib.DrawRectangleRounded(rect.Rectangle, roundness, segments, color);
        public static void DrawRectRoundedLines(this Rect rect, float roundness, float lineThickness, int segments, Color color) => Raylib.DrawRectangleRoundedLines(rect.Rectangle, roundness, segments, lineThickness, color);

        public static void DrawRectSlantedCorners(this Rect rect, Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var points = GetRectSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
            DrawPolygonConvex(points, rect.Center, color);
        }
        public static void DrawRectSlantedCorners(this Rect rect, Vector2 pivot, float rotDeg, Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var points = SPoly.Rotate(GetRectSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner), pivot, rotDeg * SUtils.DEGTORAD);
            DrawPolygonConvex(points, rect.Center, color);
        }
        public static void DrawRectSlantedCornersLines(this Rect rect, float lineThickness, Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var points = GetRectSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner);
            DrawPolygonLines(points, lineThickness, color);
        }
        public static void DrawRectSlantedCornersLines(this Rect rect, Vector2 pivot, float rotDeg, float lineThickness, Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            var points = SPoly.Rotate(GetRectSlantedCornerPoints(rect, tlCorner, trCorner, brCorner, blCorner), pivot, rotDeg * SUtils.DEGTORAD);
            DrawPolygonLines(points, lineThickness, color);
        }

        /// <summary>
        /// Get the points to draw a rectangle with slanted corners.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="tlCorner"></param>
        /// <param name="trCorner"></param>
        /// <param name="brCorner"></param>
        /// <param name="blCorner"></param>
        /// <returns>Returns points in ccw order.</returns>
        public static Polygon GetRectSlantedCornerPoints(this Rect rect, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            Vector2 tl = rect.TopLeft;
            Vector2 tr = rect.TopRight;
            Vector2 br = rect.BottomRight;
            Vector2 bl = rect.BottomLeft;
            Polygon points = new();
            if (tlCorner > 0f && tlCorner < 1f)
            {
                points.Add(tl + new Vector2(MathF.Min(tlCorner, rect.width), 0f));
                points.Add(tl + new Vector2(0f, MathF.Min(tlCorner, rect.height)));
            }
            if (blCorner > 0f && blCorner < 1f)
            {
                points.Add(bl - new Vector2(0f, MathF.Min(tlCorner, rect.height)));
                points.Add(bl + new Vector2(MathF.Min(tlCorner, rect.width), 0f));
            }
            if (brCorner > 0f && brCorner < 1f)
            {
                points.Add(br - new Vector2(MathF.Min(tlCorner, rect.width), 0f));
                points.Add(br - new Vector2(0f, MathF.Min(tlCorner, rect.height)));
            }
            if (trCorner > 0f && trCorner < 1f)
            {
                points.Add(tr + new Vector2(0f, MathF.Min(tlCorner, rect.height)));
                points.Add(tr - new Vector2(MathF.Min(tlCorner, rect.width), 0f));
            }
            return points;
        }
        /// <summary>
        /// Get the points to draw a rectangle with slanted corners. The corner values are the percentage of the width/height of the rectange the should be used for the slant.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="tlCorner">Should be bewteen 0 - 1</param>
        /// <param name="trCorner">Should be bewteen 0 - 1</param>
        /// <param name="brCorner">Should be bewteen 0 - 1</param>
        /// <param name="blCorner">Should be bewteen 0 - 1</param>
        /// <returns>Returns points in ccw order.</returns>
        public static Polygon GetRectSlantedCornerPointsRelative(this Rect rect, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            Vector2 tl = rect.TopLeft;
            Vector2 tr = rect.TopRight;
            Vector2 br = rect.BottomRight;
            Vector2 bl = rect.BottomLeft;
            Polygon points = new();
            if (tlCorner > 0f && tlCorner < 1f)
            {
                points.Add(tl + new Vector2(tlCorner * rect.width, 0f));
                points.Add(tl + new Vector2(0f, tlCorner * rect.height));
            }
            if (blCorner > 0f && blCorner < 1f)
            {
                points.Add(bl - new Vector2(0f, tlCorner * rect.height));
                points.Add(bl + new Vector2(tlCorner * rect.width, 0f));
            }
            if (brCorner > 0f && brCorner < 1f)
            {
                points.Add(br - new Vector2(tlCorner * rect.width, 0f));
                points.Add(br - new Vector2(0f, tlCorner * rect.height));
            }
            if (trCorner > 0f && trCorner < 1f)
            {
                points.Add(tr + new Vector2(0f, tlCorner * rect.height));
                points.Add(tr - new Vector2(tlCorner * rect.width, 0f));
            }
            return points;
        }
        
        public static void DrawRectCorners(this Rect rect, float lineThickness, Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            Vector2 tl = rect.TopLeft;
            Vector2 tr = rect.TopRight;
            Vector2 br = rect.BottomRight;
            Vector2 bl = rect.BottomLeft;

            if (tlCorner > 0f && tlCorner < 1f)
            {
                DrawCircle(tl, lineThickness / 2, color);
                DrawLine(tl, tl + new Vector2(MathF.Min(tlCorner, rect.width), 0f), lineThickness, color);
                DrawLine(tl, tl + new Vector2(0f, MathF.Min(tlCorner, rect.height)), lineThickness, color);
            }
            if (trCorner > 0f && trCorner < 1f)
            {
                DrawCircle(tr, lineThickness / 2, color);
                DrawLine(tr, tr - new Vector2(MathF.Min(tlCorner, rect.width), 0f), lineThickness, color);
                DrawLine(tr, tr + new Vector2(0f, MathF.Min(tlCorner, rect.height)), lineThickness, color);
            }
            if (brCorner > 0f && brCorner < 1f)
            {
                DrawCircle(br, lineThickness / 2, color);
                DrawLine(br, br - new Vector2(MathF.Min(tlCorner, rect.width), 0f), lineThickness, color);
                DrawLine(br, br - new Vector2(0f, MathF.Min(tlCorner, rect.height)), lineThickness, color);
            }
            if (blCorner > 0f && blCorner < 1f)
            {
                DrawCircle(bl, lineThickness / 2, color);
                DrawLine(bl, bl + new Vector2(MathF.Min(tlCorner, rect.width), 0f), lineThickness, color);
                DrawLine(bl, bl - new Vector2(0f, MathF.Min(tlCorner, rect.height)), lineThickness, color);
            }
        }
        public static void DrawRectCorners(this Rect rect, float lineThickness, Color color, float cornerLength) => DrawRectCorners(rect, lineThickness, color, cornerLength, cornerLength, cornerLength, cornerLength);
        public static void DrawRectCornersRelative(this Rect rect, float lineThickness, Color color, float tlCorner, float trCorner, float brCorner, float blCorner)
        {
            Vector2 tl = rect.TopLeft;
            Vector2 tr = rect.TopRight;
            Vector2 br = rect.BottomRight;
            Vector2 bl = rect.BottomLeft;

            if (tlCorner > 0f && tlCorner < 1f)
            {
                DrawCircle(tl, lineThickness / 2, color);
                DrawLine(tl, tl + new Vector2(tlCorner * rect.width, 0f), lineThickness, color);
                DrawLine(tl, tl + new Vector2(0f, tlCorner * rect.height), lineThickness, color);
            }
            if (trCorner > 0f && trCorner < 1f)
            {
                DrawCircle(tr, lineThickness / 2, color);
                DrawLine(tr, tr - new Vector2(tlCorner * rect.width, 0f), lineThickness, color);
                DrawLine(tr, tr + new Vector2(0f, tlCorner * rect.height), lineThickness, color);
            }
            if (brCorner > 0f && brCorner < 1f)
            {
                DrawCircle(br, lineThickness / 2, color);
                DrawLine(br, br - new Vector2(tlCorner * rect.width, 0f), lineThickness, color);
                DrawLine(br, br - new Vector2(0f, tlCorner * rect.height), lineThickness, color);
            }
            if (blCorner > 0f && blCorner < 1f)
            {
                DrawCircle(bl, lineThickness / 2, color);
                DrawLine(bl, bl + new Vector2(tlCorner * rect.width, 0f), lineThickness, color);
                DrawLine(bl, bl - new Vector2(0f, tlCorner * rect.height), lineThickness, color);
            }
        }
        public static void DrawRectCornersRelative(this Rect rect, float lineThickness, Color color, float cornerLengthFactor) => DrawRectCornersRelative(rect, lineThickness, color, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor, cornerLengthFactor);
        
        public static void DrawRectCheckered(this Rect rect, float spacing, float lineThickness, float angleDeg, Color lineColor, Color outlineColor, Color bgColor)
        {
            Vector2 size = new Vector2(rect.width, rect.height);
            Vector2 center = new Vector2(rect.x, rect.y) + size / 2;
            float maxDimension = MathF.Max(size.X, size.Y);
            float rotRad = angleDeg * SUtils.DEGTORAD;

            Vector2 tl = new(rect.x, rect.y);
            Vector2 tr = new(rect.x + rect.width, rect.y);
            Vector2 bl = new(rect.x, rect.y + rect.height);
            Vector2 br = new(rect.x + rect.width, rect.y + rect.height);

            if (bgColor.a > 0) DrawRectangleRec(rect.Rectangle, bgColor);

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
                List<(Vector2 p, Vector2 n)> intersections = SGeometry.IntersectShape(new Segment(start, end), rect).points; // SGeometry.IntersectionSegmentRect(center, start, end, tl, tr, br, bl).points;

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
                List<(Vector2 p, Vector2 n)> intersections = SGeometry.IntersectShape(new Segment(start, end), rect).points; //SGeometry.IntersectionSegmentRect(center, start, end, tl, tr, br, bl).points;
                if (intersections.Count >= 2) DrawLineEx(intersections[0].p, intersections[1].p, lineThickness, lineColor);
                else break;
                cur.X += spacing;
                whileCounter++;
            }

            if (outlineColor.a > 0) DrawLines(rect, new Vector2(0.5f, 0.5f), 0f, lineThickness, outlineColor);
        }

        public static void DrawRectDotted(this Rect rect, int gapsPerSide, float lineThickness, Color color, bool cornersRounded = false, bool roundedLineEdges = false)
        {
            if (cornersRounded)
            {
                var corners = SRect.GetCorners(rect);
                float r = lineThickness * 0.5f;
                DrawCircleV(corners.tl, r, color);
                DrawCircleV(corners.tr, r, color);
                DrawCircleV(corners.br, r, color);
                DrawCircleV(corners.bl, r, color);
            }


            var segments = rect.GetEdges();// SRect.GetEdges(rect);
            foreach (var s in segments)
            {
                DrawLineDotted(s.start, s.end, gapsPerSide, lineThickness, color, roundedLineEdges);
            }
        }
        public static void DrawRectDotted(this Rect rect, int gapsPerSide, float gapSizeF, float lineThickness, Color color, bool cornersRounded = false, bool roundedLineEdges = false)
        {
            if (cornersRounded)
            {
                var corners = SRect.GetCorners(rect);
                float r = lineThickness * 0.5f;
                DrawCircleV(corners.tl, r, color);
                DrawCircleV(corners.tr, r, color);
                DrawCircleV(corners.br, r, color);
                DrawCircleV(corners.bl, r, color);
            }


            var segments = rect.GetEdges(); // SRect.GetEdges(rect);
            foreach (var s in segments)
            {
                DrawLineDotted(s.start, s.end, gapsPerSide, gapSizeF, lineThickness, color, roundedLineEdges);
            }
        }


        //public static void DrawRectCheckered(Vector2 pos, Vector2 size, Vector2 alignement, float spacing, float lineThickness, float angleDeg, Color lineColor, Color outlineColor, Color bgColor)
        //{
        //    Rect rect = new(pos, size, alignement);
        //    DrawRectCheckered(rect, spacing, lineThickness, angleDeg, lineColor, outlineColor, bgColor);
        //}
        /*
        public static void DrawRectangleCornered(Rect rect, float lineThickness, Color outlineColor, Color fillColor, Vector2 topF, Vector2 rightF, Vector2 bottomF, Vector2 leftF)
        {
            var points = GetRectangleCorneredPoints(rect, topF, rightF, bottomF, leftF);
            Vector2 center = new(rect.x + rect.width / 2, rect.y + rect.height / 2);
            DrawPolygon(points, center, fillColor, true);
            DrawPolygon(points, lineThickness, outlineColor);
        }
        public static void DrawRectangleCorneredLine(Rect rect, float lineThickness, Color color, Vector2 topF, Vector2 rightF, Vector2 bottomF, Vector2 leftF)
        {
            DrawPolygon(GetRectangleCorneredPoints(rect, topF, rightF, bottomF, leftF), lineThickness, color);
        }
        public static void DrawRectangleCorneredFilled(Rect rect, Color color, Vector2 topF, Vector2 rightF, Vector2 bottomF, Vector2 leftF)
        {
            Vector2 center = new(rect.x + rect.width / 2, rect.y + rect.height / 2);
            DrawPolygon(GetRectangleCorneredPoints(rect, topF, rightF, bottomF, leftF), center, color, true);
        }
        */
        /*
        private static List<Vector2> GetRectangleCorneredPoints(Rect rect, Vector2 topF, Vector2 rightF, Vector2 bottomF, Vector2 leftF)
        {
            List<Vector2> poly = new();

            Vector2 tl = new(rect.x, rect.y);
            Vector2 tr = new(rect.x + rect.width, rect.y);
            Vector2 br = new(rect.x + rect.width, rect.y + rect.height);
            Vector2 bl = new(rect.x, rect.y + rect.height);


            if (topF.X <= 0f && leftF.Y <= 0f)
            {
                poly.Add(tl);
            }
            else
            {
                poly.Add(tl + new Vector2(0f, rect.height * leftF.Y));
                poly.Add(tl + new Vector2(rect.width * topF.X, 0f));
            }

            if (topF.Y <= 0f && rightF.X <= 0f)
            {
                poly.Add(tr);
            }
            else
            {
                poly.Add(tr - new Vector2(rect.width * topF.Y, 0f));
                poly.Add(tr + new Vector2(0f, rect.height * rightF.X));
            }

            if (rightF.Y <= 0f && bottomF.X <= 0f)
            {
                poly.Add(br);
            }
            else
            {
                poly.Add(br - new Vector2(0f, rect.height * rightF.Y));
                poly.Add(br - new Vector2(rect.width * bottomF.X, 0f));
            }

            if (bottomF.Y <= 0f && leftF.X <= 0f)
            {
                poly.Add(bl);
            }
            else
            {
                poly.Add(bl + new Vector2(rect.width * bottomF.Y, 0f));
                poly.Add(bl - new Vector2(0f, rect.height * leftF.X));
            }

            return poly;
        }
        */
        /*
        public static void DrawRectangleCorners(Rect rect, float lineThickness, Color color, Vector2 topF, Vector2 rightF, Vector2 bottomF, Vector2 leftF)
        {
            Vector2 tl = new(rect.x, rect.y);
            Vector2 tr = new(rect.x + rect.width, rect.y);
            Vector2 br = new(rect.x + rect.width, rect.y + rect.height);
            Vector2 bl = new(rect.x, rect.y + rect.height);


            if (topF.X > 0 || leftF.Y > 0) DrawCircleV(tl, lineThickness / 2, color);
            if (topF.Y > 0 || rightF.X > 0) DrawCircleV(tr, lineThickness / 2, color);
            if (bottomF.X > 0 || rightF.Y > 0) DrawCircleV(br, lineThickness / 2, color);
            if (bottomF.Y > 0 || leftF.X > 0) DrawCircleV(bl, lineThickness / 2, color);

            if (topF.X > 0 && topF.X <= 1f) DrawLineEx(tl, tl + new Vector2(rect.width * topF.X, 0f), lineThickness, color);
            else if (topF.X > 1f) DrawLineEx(tl, tl + new Vector2(topF.X, 0f), lineThickness, color);

            if (topF.Y > 0 && topF.Y <= 1f) DrawLineEx(tr, tr - new Vector2(rect.width * topF.Y, 0f), lineThickness, color);
            else if (topF.Y > 1f) DrawLineEx(tr, tr - new Vector2(topF.Y, 0f), lineThickness, color);

            if (bottomF.X > 0 && bottomF.X <= 1f) DrawLineEx(br, br - new Vector2(rect.width * bottomF.X, 0f), lineThickness, color);
            else if (bottomF.X > 1f) DrawLineEx(br, br - new Vector2(bottomF.X, 0f), lineThickness, color);

            if (bottomF.Y > 0 && bottomF.Y <= 1f) DrawLineEx(bl, bl + new Vector2(rect.width * bottomF.Y, 0f), lineThickness, color);
            else if (bottomF.Y > 1f) DrawLineEx(bl, bl + new Vector2(bottomF.Y, 0f), lineThickness, color);

            if (rightF.X > 0 && rightF.X <= 1f) DrawLineEx(tr, tr + new Vector2(0f, rect.height * rightF.X), lineThickness, color);
            else if (rightF.X > 1f) DrawLineEx(tr, tr + new Vector2(0f, rightF.X), lineThickness, color);

            if (rightF.Y > 0 && rightF.Y <= 1f) DrawLineEx(br, br - new Vector2(0f, rect.height * rightF.Y), lineThickness, color);
            else if (rightF.Y > 1f) DrawLineEx(br, br - new Vector2(0f, rightF.Y), lineThickness, color);

            if (leftF.X > 0 && leftF.X <= 1f) DrawLineEx(bl, bl - new Vector2(0f, rect.height * leftF.X), lineThickness, color);
            else if (leftF.X > 1f) DrawLineEx(bl, bl - new Vector2(0f, leftF.X), lineThickness, color);

            if (leftF.Y > 0 && leftF.Y <= 1f) DrawLineEx(tl, tl + new Vector2(0f, rect.height * leftF.Y), lineThickness, color);
            else if (leftF.Y > 1f) DrawLineEx(tl, tl + new Vector2(0f, leftF.Y), lineThickness, color);

        }
        public static void DrawRectangleCorners(Rect rect, float lineThickness, Color color, float length)
        {
            DrawRectangleCorners(rect, lineThickness, color, new(length), new(length), new(length), new(length));
        }
        public static void DrawRectangleCorners(Vector2 pos, Vector2 size, Vector2 alignement, float lineThickness, Color color, float length)
        {
            DrawRectangleCorners(new(pos, size, alignement), lineThickness, color, length);
        }
        public static void DrawRectangleCorners(Vector2 pos, Vector2 size, Vector2 alignement, float lineThickness, Color color, Vector2 topF, Vector2 rightF, Vector2 bottomF, Vector2 leftF)
        {
            DrawRectangleCorners(new(pos, size, alignement), lineThickness, color, topF, rightF, bottomF, leftF);
        }
        */
        /*
        public static void DrawRect(Vector2 pos, Vector2 size, Vector2 alignement, Color color)
        {
            DrawRect(new(pos, size, alignement), color);
        }
        public static void DrawRect(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 pivot, float rotDeg, float lineThickness, Color color)
        {
            Vector2 leftExtension = SVec.Rotate(new Vector2(-lineThickness / 2, 0f), rotDeg * SUtils.DEGTORAD);
            Vector2 rightExtension = SVec.Rotate(new Vector2(lineThickness / 2, 0f), rotDeg * SUtils.DEGTORAD);

            var rr = SRect.RotateRect(pos, size, alignement, pivot, rotDeg);
            DrawLineEx(rr.tl + leftExtension, rr.tr + rightExtension, lineThickness, color);
            DrawLineEx(rr.bl + leftExtension, rr.br + rightExtension, lineThickness, color);
            DrawLineEx(rr.tl, rr.bl, lineThickness, color);
            DrawLineEx(rr.tr, rr.br, lineThickness, color);
        }
        public static void DrawRect(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 pivot, float rotDeg, Color color)
        {
            DrawRect(new(pos, size, alignement), pivot, rotDeg, color);
        }
        */

        #endregion

        #region Triangle
        public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Color color) => Raylib.DrawTriangle(a, b, c, color);
        public static void DrawTriangleLines(Vector2 a, Vector2 b, Vector2 c, float lineThickness, Color color, bool smoothJoints = true)
        {
            var lines = new Triangle(a,b,c).GetEdges();
            DrawLines(lines, lineThickness, color, smoothJoints);
        }

        public static void Draw(this Triangle t, Color color) => Raylib.DrawTriangle(t.a, t.b, t.c, color);
        public static void DrawLines(this Triangle t, float lineThickness, Color color, bool smoothJoints = true)
        {
            var lines = t.GetEdges();
            DrawLines(lines, lineThickness, color, smoothJoints);
        }
        
        public static void DrawTriangles(List<Triangle> triangles, Color color)
        {
            foreach (var t in triangles)
            {
                t.Draw(color);
            }
        }

        #endregion

        #region Polygon
        public static void DrawPolygonConvex(this Polygon poly, Color color, bool clockwise = false) { DrawPolygonConvex(poly, poly.GetCentroid(), color, clockwise); }
        public static void DrawPolygonConvex(this Polygon poly, Vector2 center, Color color, bool clockwise = false)
        {
            if (clockwise)
            {
                for (int i = 0; i < poly.Count - 1; i++)
                {
                    Raylib.DrawTriangle(poly[i], center, poly[i + 1], color);
                }
                Raylib.DrawTriangle(poly[poly.Count - 1], center, poly[0], color);
            }
            else
            {
                for (int i = 0; i < poly.Count - 1; i++)
                {
                    Raylib.DrawTriangle(poly[i], poly[i + 1], center, color);
                }
                Raylib.DrawTriangle(poly[poly.Count - 1], poly[0], center, color);
            }
        }
        public static void DrawPolygonConvex(this Polygon relativePoly, Vector2 pos, float scale, float rotDeg, Color color, bool clockwise = false)
        {
            if (clockwise)
            {
                for (int i = 0; i < relativePoly.Count - 1; i++)
                {
                    Vector2 a = pos + SVec.Rotate(relativePoly[i] * scale, rotDeg * SUtils.DEGTORAD);
                    Vector2 b = pos;
                    Vector2 c = pos + SVec.Rotate(relativePoly[i + 1] * scale, rotDeg * SUtils.DEGTORAD);
                    Raylib.DrawTriangle(a, b, c, color);
                }

                Vector2 aFinal = pos + SVec.Rotate(relativePoly[relativePoly.Count - 1] * scale, rotDeg * SUtils.DEGTORAD);
                Vector2 bFinal = pos;
                Vector2 cFinal = pos + SVec.Rotate(relativePoly[0] * scale, rotDeg * SUtils.DEGTORAD);
                Raylib.DrawTriangle(aFinal, bFinal, cFinal, color);
            }
            else
            {
                for (int i = 0; i < relativePoly.Count - 1; i++)
                {
                    Vector2 a = pos + SVec.Rotate(relativePoly[i] * scale, rotDeg * SUtils.DEGTORAD);
                    Vector2 b = pos + SVec.Rotate(relativePoly[i + 1] * scale, rotDeg * SUtils.DEGTORAD);
                    Vector2 c = pos;
                    Raylib.DrawTriangle(a, b, c, color);
                }

                Vector2 aFinal = pos + SVec.Rotate(relativePoly[relativePoly.Count - 1] * scale, rotDeg * SUtils.DEGTORAD);
                Vector2 bFinal = pos + SVec.Rotate(relativePoly[0] * scale, rotDeg * SUtils.DEGTORAD);
                Vector2 cFinal = pos;
                Raylib.DrawTriangle(aFinal, bFinal, cFinal, color);
            }
        }
        
        public static void DrawPolygon(this Polygon poly, Color color)
        {
            var triangles = poly.Triangulate();
            foreach (var t in triangles) t.Draw(color);
        }

        public static void DrawPolygonLines(this Polygon poly, float lineThickness, Color color)
        {
            for (int i = 0; i < poly.Count - 1; i++)
            {
                DrawCircleV(poly[i], lineThickness * 0.5f, color);
                DrawLineEx(poly[i], poly[i + 1], lineThickness, color);
            }
            DrawCircleV(poly[poly.Count - 1], lineThickness * 0.5f, color);
            DrawLineEx(poly[poly.Count - 1], poly[0], lineThickness, color);
        }
        public static void DrawPolygonLines(this Polygon poly, Vector2 pos, Vector2 size, float rotDeg, float lineThickness, Color outlineColor)
        {
            for (int i = 0; i < poly.Count - 1; i++)
            {
                Vector2 p1 = pos + SVec.Rotate(poly[i] * size, rotDeg * SUtils.DEGTORAD);
                Vector2 p2 = pos + SVec.Rotate(poly[i + 1] * size, rotDeg * SUtils.DEGTORAD);
                DrawCircleV(p1, lineThickness * 0.5f, outlineColor);
                DrawLineEx(p1, p2, lineThickness, outlineColor);
            }
            DrawCircleV(pos + SVec.Rotate(poly[poly.Count - 1] * size, rotDeg * SUtils.DEGTORAD), lineThickness * 0.5f, outlineColor);
            DrawLineEx(pos + SVec.Rotate(poly[poly.Count - 1] * size, rotDeg * SUtils.DEGTORAD), pos + SVec.Rotate(poly[0] * size, rotDeg * SUtils.DEGTORAD), lineThickness, outlineColor);
        }
        
        public static void DrawPolygonCornered(this Polygon poly, float lineThickness, Color color, float cornerLength)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 prev = poly[(i-1)%poly.Count];
                Vector2 cur = poly[i];
                Vector2 next = poly[(i+1)%poly.Count];
                DrawLine(cur, cur + next.Normalize() * cornerLength, lineThickness, color);
                DrawLine(cur, cur + prev.Normalize() * cornerLength, lineThickness, color);
            }
        }
        public static void DrawPolygonCornered(this Polygon poly, float lineThickness, Color color, List<float> cornerLengths)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                float cornerLength = cornerLengths[i%cornerLengths.Count];
                Vector2 prev = poly[(i - 1) % poly.Count];
                Vector2 cur = poly[i];
                Vector2 next = poly[(i + 1) % poly.Count];
                DrawLine(cur, cur + next.Normalize() * cornerLength, lineThickness, color);
                DrawLine(cur, cur + prev.Normalize() * cornerLength, lineThickness, color);
            }
        }
        public static void DrawPolygonCorneredRelative(this Polygon poly, float lineThickness, Color color, float cornerF)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 prev = poly[(i - 1) % poly.Count];
                Vector2 cur = poly[i];
                Vector2 next = poly[(i + 1) % poly.Count];
                DrawLine(cur, cur.Lerp(next, cornerF), lineThickness, color);
                DrawLine(cur, cur.Lerp(prev, cornerF), lineThickness, color);
            }
        }
        public static void DrawPolygonCorneredRelative(this Polygon poly, float lineThickness, Color color, List<float> cornerFactors)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                float cornerF = cornerFactors[i%cornerFactors.Count];
                Vector2 prev = poly[(i - 1) % poly.Count];
                Vector2 cur = poly[i];
                Vector2 next = poly[(i + 1) % poly.Count];
                DrawLine(cur, cur.Lerp(next, cornerF), lineThickness, color);
                DrawLine(cur, cur.Lerp(prev, cornerF), lineThickness, color);
            }
        }


        #endregion

        #region Text
        //todo overhaul
        public static void DrawTextBox(Rect rect, string emptyText, List<char> chars, float fontSpacing, Font font, Color textColor, bool drawCaret, int caretPosition, float caretWidth, Color caretColor, Vector2 textAlignement)
        {
            //fix alignement
            //alignement = new(0, 0.5f);
            if (chars.Count <= 0)
            {
                SDrawing.DrawText(emptyText, rect, fontSpacing, textColor, font, textAlignement);
            }
            else
            {
                string text = String.Concat(chars);
                SDrawing.DrawText(text, rect, fontSpacing, textColor, font, textAlignement);

                if (drawCaret)
                {
                    float fontSize = FontHandler.CalculateDynamicFontSize(text, new Vector2(rect.width, rect.height), font, fontSpacing);
                    Vector2 textSize = MeasureTextEx(font, text, fontSize, fontSpacing);
                    Vector2 uiPos = rect.GetPoint(textAlignement);
                    Vector2 topLeft = uiPos - textAlignement * textSize;
                    //Vector2 topLeft = new(rect.x, rect.y);

                    string caretText = String.Concat(chars.GetRange(0, caretPosition));
                    Vector2 caretTextSize = MeasureTextEx(font, caretText, fontSize, fontSpacing);

                    Vector2 caretTop = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, 0f);
                    Vector2 caretBottom = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, rect.height);
                    DrawLineEx(caretTop, caretBottom, caretWidth, caretColor);
                }
            }
        }


        public static void DrawText(this string text, Rect rect, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            Vector2 textSize = rect.Size;
            Vector2 uiPos = rect.GetPoint(alignement);
            float fontSize = FontHandler.CalculateDynamicFontSize(text, textSize, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 topLeft = uiPos - alignement * fontDimensions;
            DrawTextEx(font, text, topLeft, fontSize, fontSpacing, color);
        }
        public static void DrawText(this string text, Rect rect, float rotDeg, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            Vector2 textSize = rect.Size;
            Vector2 uiPos = rect.GetPoint(alignement);
            float fontSize = FontHandler.CalculateDynamicFontSize(text, textSize, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = alignement * fontDimensions;
            Vector2 topLeft = uiPos - alignement * fontDimensions;
            DrawTextPro(font, text, topLeft, originOffset, rotDeg, fontSize, fontSpacing, color);
        }
        public static void DrawText(this string text, Vector2 uiPos, float fontSize, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 topLeft = uiPos - alignement * fontDimensions;
            DrawTextEx(font, text, topLeft, fontSize, fontSpacing, color);
            DrawRectangleLinesEx(new(topLeft.X, topLeft.Y, fontDimensions.X, fontDimensions.Y), 5f, WHITE);
        }
        public static void DrawText(this string text, Vector2 uiPos, float rotDeg, float fontSize, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = alignement * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, fontSpacing, color);
        }
        public static void DrawText(List<string> texts, Rect rect, float fontSpacing, List<Color> colors, Font font, Vector2 alignement)
        {
            string text = "";
            foreach (var t in texts)
            {
                text += t;
            }
            Vector2 textSize = rect.Size;
            Vector2 uiPos = rect.GetPoint(alignement);
            float fontSize = FontHandler.CalculateDynamicFontSize(text, textSize, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 curPos = uiPos - alignement * fontDimensions;
            for (int i = 0; i < texts.Count; i++)
            {
                string curText = texts[i];
                float w = MeasureTextEx(font, curText, fontSize, fontSpacing).X;
                DrawTextEx(font, curText, curPos, fontSize, fontSpacing, colors[i % colors.Count]);
                curPos += new Vector2(w, 0f);
            }
        }
        
        
        /*
        public static void DrawTextAligned(string text, Vector2 uiPos, Vector2 textSize, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = FontHandler.CalculateDynamicFontSize(text, textSize, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 topLeft = uiPos - alignement * fontDimensions;
            DrawTextEx(font, text, topLeft, fontSize, fontSpacing, color);
            //DrawRectangleLinesEx(new(topLeft.X, topLeft.Y, fontDimensions.X, fontDimensions.Y), 5f, WHITE);
        }
        public static void DrawTextAligned(string text, Vector2 uiPos, float textHeight, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = FontHandler.CalculateDynamicFontSize(textHeight, font);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            DrawTextEx(font, text, uiPos - alignement * fontDimensions, fontSize, fontSpacing, color);
        }
        public static void DrawTextAligned2(string text, Vector2 uiPos, float textWidth, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = FontHandler.CalculateDynamicFontSize(text, textWidth, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            DrawTextEx(font, text, uiPos - alignement * fontDimensions, fontSize, fontSpacing, color);
        }
        */
        /*
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, Vector2 textSize, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = FontHandler.CalculateDynamicFontSize(text, textSize, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = alignement * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, fontSpacing, color);

            // DrawRectangleLinesEx(new())
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, float textHeight, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = FontHandler.CalculateDynamicFontSize(textHeight, font);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = alignement * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, fontSpacing, color);
        }
        public static void DrawTextAlignedPro2(string text, Vector2 uiPos, float rotDeg, float textWidth, float fontSpacing, Color color, Font font, Vector2 alignement)
        {
            float fontSize = FontHandler.CalculateDynamicFontSize(text, textWidth, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = alignement * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, 1, color);
        }
        */

        #endregion

        #region UI
        public static void DrawOutlineBar(this Rect rect, float thickness, float f, Color color)
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
        public static void DrawOutlineBar(this Rect rect, Vector2 pivot, float angleDeg, float thickness, float f, Color color)
        {
            var rr = SRect.Rotate(rect, pivot, angleDeg);
            //Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
            //Vector2 thicknessOffsetY = new Vector2(0f, thickness);

            Vector2 leftExtension = SVec.Rotate(new Vector2(-thickness / 2, 0f), angleDeg * SUtils.DEGTORAD);
            Vector2 rightExtension = SVec.Rotate(new Vector2(thickness / 2, 0f), angleDeg * SUtils.DEGTORAD);

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

        public static void DrawOutlineBar(this Circle c, float thickness, float f, Color color)
        {
            DrawCircleSectorLines(c.center, c.radius, 0, 360 * f, thickness, color, false, 8f);
        }
        public static void DrawOutlineBar(this Circle c, float startOffsetDeg, float thickness, float f, Color color)
        {
            DrawCircleSectorLines(c.center, c.radius, 0, 360 * f, startOffsetDeg, thickness, color, false, 8f);
        }


        public static void DrawBar(this Rect rect, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            f = 1.0f - f;
            UIMargins progressMargins = new(f * top, f * right, f * bottom, f * left);
            var progressRect = progressMargins.Apply(rect);
            SDrawing.Draw(rect, bgColor);
            SDrawing.Draw(progressRect, barColor);
        }
        public static void DrawBar(this Rect rect, Vector2 pivot, float angleDeg, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            f = 1.0f - f;
            UIMargins progressMargins = new(f * top, f * right, f * bottom, f * left);
            var progressRect = progressMargins.Apply(rect);
            SDrawing.Draw(rect, pivot, angleDeg, bgColor);
            SDrawing.Draw(progressRect, pivot, angleDeg, barColor);
        }

        /*
        public static void DrawBar(Vector2 pos, Vector2 size, Vector2 alignement, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            DrawBar(new(pos, size, alignement), f, barColor, bgColor, left, right, top, bottom);
        }
        public static void DrawBar(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 pivot, float angleDeg, float f, Color barColor, Color bgColor, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
        {
            DrawBar(new(pos, size, alignement), pivot, angleDeg, f, barColor, bgColor, left, right, top, bottom);
        }
        */
        //public static void DrawRectangleOutlineBar(Vector2 pos, Vector2 size, Vector2 alignement, float thickness, float f, Color color)
        //{
        //    DrawRectangleOutlineBar(new(pos, size, alignement), thickness, f, color);
        //}
        //public static void DrawRectangleOutlineBar(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 pivot, float angleDeg, float thickness, float f, Color color)
        //{
        //    DrawRectangleOutlineBar(new(pos, size, alignement), pivot, angleDeg, thickness, f, color);
        //}
        #endregion

    }
}

/*
        public static void Draw(this Polygon p, Color color, bool clockwise = false) => DrawPolygon(p, p.GetCentroid(), color, clockwise);
        public static void Draw(this Polygon p, Vector2 pos, float scale, float rotDeg, Color color, bool clockwise = false) => DrawPolygon(p, p.GetCentroid() + pos, scale, rotDeg, color, clockwise);

        public static void DrawLines(this Polygon p, float lineThickness, Color color) => DrawPolygonLines(p, lineThickness, color);
        public static void DrawLines(this Polygon p, Vector2 pos, Vector2 size, float rotDeg, float lineThickness, Color color) => DrawPolygonLines(p, p.GetCentroid() + pos, size, rotDeg, lineThickness, color);

        public static void DrawPolygonCornered(this Polygon p, float lineThickness, Color color, float cornerLength) => DrawPolygonCornered(p, lineThickness, color, cornerLength);
        public static void DrawPolygonCornered(this Polygon p, float lineThickness, Color color, List<float> cornerLengths) => DrawPolygonCornered(p, lineThickness, color, cornerLengths);
        public static void DrawPolygonCorneredRelative(this Polygon p, float lineThickness, Color color, float cornerF) => DrawPolygonCorneredRelative(p, lineThickness, color, cornerF);
        public static void DrawPolygonCorneredRelative(this Polygon p, float lineThickness, Color color, List<float> cornerFactors) => DrawPolygonCorneredRelative(p, lineThickness, color, cornerFactors);
        */
/*
        public static void DrawPolygonCentered(List<Vector2> points, Vector2 center, Color fillColor, bool clockwise = true)
        {
            if (clockwise)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    Raylib.DrawTriangle(center + points[i], center, center + points[i + 1], fillColor);
                }
                Raylib.DrawTriangle(center + points[points.Count - 1], center, center + points[0], fillColor);
            }
            else
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    Raylib.DrawTriangle(center + points[i], center + points[i + 1], center, fillColor);
                }
                Raylib.DrawTriangle(center + points[points.Count - 1], center + points[0], center, fillColor);
            }
        }
        */
/*
        public static void DrawPolygon(List<Vector2> points, Vector2 center, Color fillColor, float lineThickness, Color outlineColor, bool clockwise = true)
        {
            DrawPolygon(points, center, fillColor, clockwise);
            DrawPolygonLines(points, lineThickness, outlineColor, center);
        }
        public static void DrawPolygonCentered(List<Vector2> points, Vector2 center, Color fillColor, float lineThickness, Color outlineColor, bool clockwise = true)
        {
            DrawPolygonCentered(points, center, fillColor, clockwise);
            DrawPolygonLines(points, lineThickness, outlineColor, center);
        }
        public static void DrawPolygonCentered(List<Vector2> points, Vector2 center, float scale, float rotDeg, Color fillColor, float lineThickness, Color outlineColor, bool clockwise = true)
        {
            DrawPolygon(points, center, scale, rotDeg, fillColor, clockwise);
            DrawPolygonLines(points, lineThickness, outlineColor, center, scale, rotDeg);
        }
        */
/*
        public static void DrawPolygonLines(List<Vector2> points, float lineThickness, Color color, Vector2 center)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                DrawCircleV(center + points[i], lineThickness * 0.5f, color);
                DrawLineEx(center + points[i], center + points[i + 1], lineThickness, color);
            }
            DrawCircleV(center + points[points.Count - 1], lineThickness * 0.5f, color);
            DrawLineEx(center + points[points.Count - 1], center + points[0], lineThickness, color);
        }
        public static void DrawPolygonLines(List<Vector2> points, float lineThickness, Color color, Vector2 center, float scale)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                DrawCircleV(center + points[i] * scale, lineThickness * 0.5f * scale, color);
                DrawLineEx(center + points[i] * scale, center + points[i + 1] * scale, lineThickness * scale, color);
            }
            DrawCircleV(center + points[points.Count - 1] * scale, lineThickness * 0.5f * scale, color);
            DrawLineEx(center + points[points.Count - 1] * scale, center + points[0] * scale, lineThickness * scale, color);
        }
        public static void DrawPolygonLines(List<Vector2> points, float lineThickness, Color color, Vector2 center, float scale, float rotDeg)
        {
            float lt = lineThickness * scale;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 p1 = center + SVec.Rotate(points[i] * scale, rotDeg * SUtils.DEGTORAD);
                Vector2 p2 = center + SVec.Rotate(points[i + 1] * scale, rotDeg * SUtils.DEGTORAD);
                DrawCircleV(p1, lt * 0.5f, color);
                DrawLineEx(p1, p2, lt, color);
            }
            DrawCircleV(center + SVec.Rotate(points[points.Count - 1] * scale, rotDeg * SUtils.DEGTORAD), lt * 0.5f, color);
            DrawLineEx(center + SVec.Rotate(points[points.Count - 1] * scale, rotDeg * SUtils.DEGTORAD), center + SVec.Rotate(points[0] * scale, rotDeg * SUtils.DEGTORAD), lt, color);
        }
        */