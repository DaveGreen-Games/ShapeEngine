using System.Numerics;
using Lib;
using Raylib_CsLo;


namespace Demo
{
    public class Frame
    {
        List<Vector2> basePoints = new();
        List<Vector2> points = new();
        float lineThickness = 1f;
        float sizeFactor = 0f;
        Raylib_CsLo.Color color = WHITE;

        public Frame(float lineThickness, params Vector2[] points)
        {
            this.basePoints = points.ToList();
            this.points = points.ToList();
            this.lineThickness = lineThickness;
        }

        public void Update(Vector2 center, float angleRad, float sizeFactor, Raylib_CsLo.Color color, float dt)
        {
            this.sizeFactor = sizeFactor;
            this.color = color;

            for (int i = 0; i < basePoints.Count; i++)
            {
                points[i] = center + SVec.Rotate(basePoints[i] * sizeFactor, angleRad);
            }
        }
        public void Draw()
        {
            SDrawing.DrawPolygonLines(points, lineThickness * sizeFactor, color);
        }
    }
}
