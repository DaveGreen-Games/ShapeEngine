using System.Numerics;
using Raylib_CsLo;
using ShapeLib;

namespace ShapeCore
{

    internal class SpawnSegment
    {
        Rectangle rect;
        float area = 0f;
        public SpawnSegment(Rectangle rect)
        {
            this.rect = rect;
            area = rect.width * rect.height;
        }
        public SpawnSegment(Vector2 topLeft, Vector2 bottomRight)
        {
            rect = new(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
            area = rect.width * rect.height;
        }
        public float Area { get { return area; } }
        public Vector2 GetRandPos()
        {
            return SRNG.randVec2(rect);
        }

        public void Debug_Draw(Color color)
        {
            //DrawRectangleRec(rect, ColorPalette.ChangeAlpha(color, 100));
            //DrawRectangleRec(rect, DEBUG_HelperColor);
            DrawRectangleLinesEx(rect, 3f, color);
        }
    }
    public class SpawnArea
    {

        Rectangle inner;
        Rectangle outer;
        List<SpawnSegment> spawnSegments;
        List<SpawnSegment> sides;
        List<SpawnSegment> corners;

        public SpawnArea(Rectangle inner, Rectangle outer)
        {
            this.inner = inner;
            this.outer = outer;
            spawnSegments = ConstructSpawnSegments(inner, outer);
            sides = new() { spawnSegments[1], spawnSegments[3], spawnSegments[5], spawnSegments[7] };
            corners = new() { spawnSegments[0], spawnSegments[2], spawnSegments[4], spawnSegments[6] };
        }

        public Vector2 RandTop() { return spawnSegments[1].GetRandPos(); }
        public Vector2 RandRight() { return spawnSegments[3].GetRandPos(); }
        public Vector2 RandBottom() { return spawnSegments[5].GetRandPos(); }
        public Vector2 RandLeft() { return spawnSegments[7].GetRandPos(); }

        public Vector2 RandTopLeft() { return spawnSegments[0].GetRandPos(); }
        public Vector2 RandTopRight() { return spawnSegments[2].GetRandPos(); }
        public Vector2 RandBottomRight() { return spawnSegments[4].GetRandPos(); }
        public Vector2 RandBottomLeft() { return spawnSegments[6].GetRandPos(); }

        public Vector2 RandCorner() { return GetRandSegment(corners).GetRandPos(); }
        public Vector2 RandSide() { return GetRandSegment(sides).GetRandPos(); }
        public Vector2 Rand() { return GetRandSegment(spawnSegments).GetRandPos(); }
        private SpawnSegment GetRandSegment(List<SpawnSegment> segments)
        {
            float maxArea = 0f;
            foreach (var segment in segments)
            {
                maxArea += segment.Area;
            }
            float chosen = SRNG.randF(0, maxArea);
            float cur = 0f;
            for (int i = 0; i < segments.Count; i++)
            {
                cur += spawnSegments[i].Area;
                if (cur >= chosen)
                {
                    return segments[i];
                }

            }
            return segments[spawnSegments.Count - 1];
        }
        public Vector2 RandInner() { return SRNG.randVec2(inner); }
        public Vector2 RandOuter() { return SRNG.randVec2(outer); }

        public void Debug_DrawSegments()
        {
            //Color[] colors = new Color[] { RED, ORANGE, YELLOW, LIME, GREEN, BLUE, PURPLE, BEIGE };
            for (int i = 0; i < spawnSegments.Count; i++)
            {
                spawnSegments[i].Debug_Draw(DEBUG_SpawnAreaLines);
            }
        }
        private List<SpawnSegment> ConstructSpawnSegments(Rectangle inner, Rectangle outer)
        {
            List<SpawnSegment> spawnAreas = new();

            //topLeft
            Vector2 tl0 = new(outer.x, outer.y);
            Vector2 br0 = new(inner.x, inner.y);
            spawnAreas.Add(new(tl0, br0));

            //topCenter
            Vector2 tl1 = new(inner.x, outer.y);
            Vector2 br1 = new(inner.x + inner.width, inner.y);
            spawnAreas.Add(new(tl1, br1));

            //topRight
            Vector2 tl2 = new(inner.x + inner.width, outer.y);
            Vector2 br2 = new(outer.x + outer.width, inner.y);
            spawnAreas.Add(new(tl2, br2));

            //rightCenter
            Vector2 tl3 = br1;
            Vector2 br3 = new(outer.x + outer.width, inner.y + inner.height);
            spawnAreas.Add(new(tl3, br3));

            //bottomRight
            Vector2 tl4 = new(inner.x + inner.width, inner.y + inner.height);
            Vector2 br4 = new(outer.x + outer.width, outer.y + outer.height);
            spawnAreas.Add(new(tl4, br4));

            //bottomCenter
            Vector2 tl5 = new(inner.x, inner.y + inner.height);
            Vector2 br5 = new(inner.x + inner.width, outer.y + outer.height);
            spawnAreas.Add(new(tl5, br5));

            //bottomLeft
            Vector2 tl6 = new(outer.x, inner.y + inner.height);
            Vector2 br6 = new(inner.x, outer.y + outer.height);
            spawnAreas.Add(new(tl6, br6));

            //leftCenter
            Vector2 tl7 = new(outer.x, inner.y);
            Vector2 br7 = tl5;
            spawnAreas.Add(new(tl7, br7));

            return spawnAreas;
        }
    }
}
