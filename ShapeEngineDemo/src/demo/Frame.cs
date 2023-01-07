using System.Numerics;
using ShapeLib;
using Raylib_CsLo;


namespace ShapeEngineDemo
{
    public class Frame
    {
        List<Vector2> basePoints = new();
        List<Vector2> points = new();
        float lineThickness = 1f;
        float sizeFactor = 0f;
        Color color = WHITE;

        public Frame(float lineThickness, params Vector2[] points)
        {
            this.basePoints = points.ToList();
            this.points = points.ToList();
            this.lineThickness = lineThickness;
        }

        public void Update(Vector2 center, float angleRad, float sizeFactor, Color color, float dt)
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
            SDrawing.DrawPolygon(points, lineThickness * sizeFactor, color);
        }
    }
}
