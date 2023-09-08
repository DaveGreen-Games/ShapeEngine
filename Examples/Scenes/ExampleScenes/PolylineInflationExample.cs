

using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    public class PolylineInflationExample : ExampleScene
    {
        ScreenTexture game;
        Polyline polyline = new();
        int dragIndex = -1;
        float offsetDelta = 0f;
        float lerpOffsetDelta = 0f;

        public PolylineInflationExample()
        {
            Title = "Polyline Inflation Example";
            game = GAMELOOP.Game;
        }
        public override void Reset()
        {
            polyline = new();
            dragIndex = -1;
            offsetDelta = 0f;
            lerpOffsetDelta = 0f;
        }
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);
            if (IsKeyPressed(KeyboardKey.KEY_R)) { polyline = new(); }

            if (GetMouseWheelMove() > 0)
            {
                offsetDelta += 10f;
            }
            else if (GetMouseWheelMove() < 0)
            {
                offsetDelta -= 10f;
            }

            lerpOffsetDelta = Lerp(lerpOffsetDelta, offsetDelta, dt * 2f);

            offsetDelta = Clamp(offsetDelta, 0f, 300f);
        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);
            Vector2 mousePos = mousePosUI;

            float vertexRadius = 8f;
            int pickedVertex = -1;

            bool isMouseOnLine = false; // polyline.OverlapShape(new Circle(mousePos, vertexRadius * 2f));
            var closest = polyline.GetClosestPoint(mousePos).Point;
            int closestIndex = polyline.GetClosestIndex(mousePos);
            bool drawClosest = true;


            for (int i = 0; i < polyline.Count; i++)
            {
                var p = polyline[i];
                float disSq = (mousePos - p).LengthSquared();
                if (pickedVertex == -1 && disSq < (vertexRadius * vertexRadius) * 2f)
                {
                    DrawCircleV(p, vertexRadius * 2f, GREEN);
                    pickedVertex = i;
                }
                else DrawCircleV(p, vertexRadius, GRAY);
                if (drawClosest)
                {
                    disSq = (closest - p).LengthSquared();
                    if (disSq < (vertexRadius * vertexRadius) * 4f)
                    {
                        drawClosest = false;
                    }
                }

            }

            if (drawClosest)
            {
                float disSq = (closest - mousePos).LengthSquared();

                float tresholdSq = 30 * 30;
                if (disSq > tresholdSq)
                {
                    drawClosest = false;
                }
                else isMouseOnLine = true;
            }


            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                if (pickedVertex == -1)
                {
                    if (isMouseOnLine)
                    {
                        polyline.Insert(closestIndex + 1, mousePos);
                    }
                    else polyline.Add(mousePos);

                }
                else
                {
                    dragIndex = pickedVertex;
                }
            }
            else if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dragIndex = -1;
            }
            else if (dragIndex == -1 && IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                if (pickedVertex > -1)
                {
                    polyline.RemoveAt(pickedVertex);
                }
            }

            if (dragIndex > -1) polyline[dragIndex] = mousePos;

            //polyline.Draw(4f, WHITE);
            var segments = polyline.GetEdges();
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                if (drawClosest)
                {
                    if (closestIndex == i) segment.Draw(4f, BLUE);
                    else segment.Draw(4f, WHITE);
                }
                else segment.Draw(4f, WHITE);



            }

            if (drawClosest) DrawCircleV(closest, vertexRadius, RED);

            if (lerpOffsetDelta > 10f)
            {
                var polygons = SClipper.Inflate(polyline, lerpOffsetDelta).ToPolygons();
                foreach (var polygon in polygons)
                {
                    polygon.DrawLines(3f, GOLD);
                }
            }

        }
    }

}
