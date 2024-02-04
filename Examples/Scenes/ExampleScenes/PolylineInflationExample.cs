

using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using Color = System.Drawing.Color;

namespace Examples.Scenes.ExampleScenes
{
    public class PolylineInflationExample : ExampleScene
    {
        private const float MaxOffset = 1000;
        Polyline polyline = new();
        int dragIndex = -1;
        float offsetDelta = 0f;
        float lerpOffsetDelta = 0f;
        private Font font;

        private InputAction createPoint;
        private InputAction deletePoint;
        private InputAction changeOffset;
        
        public PolylineInflationExample()
        {
            Title = "Polyline Inflation Example";
            font = GAMELOOP.GetFont(FontIDs.JetBrains);


            var createMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            var createGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            var createKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            createPoint = new(createMB, createGP, createKB);

            var deleteMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            var deleteGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            var deleteKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            deletePoint = new(deleteMB, deleteGP, deleteKB);

            var offsetMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f);
            var offsetKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.S, ShapeKeyboardButton.W);
            var offsetGP = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_DOWN, ShapeGamepadButton.LEFT_FACE_UP);
            changeOffset = new(offsetMW, offsetGP, offsetKB);
            
            textFont.FontSpacing = 1f;
            textFont.Color = Colors.Light;

        }
        public override void Reset()
        {
            polyline = new();
            dragIndex = -1;
            offsetDelta = 0f;
            lerpOffsetDelta = 0f;
        }
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            var gamepad = GAMELOOP.CurGamepad;
            createPoint.Gamepad = gamepad;
            createPoint.Update(dt);
            
            deletePoint.Gamepad = gamepad;
            deletePoint.Update(dt);
            
            changeOffset.Gamepad = gamepad;
            changeOffset.Update(dt);

            var offsetState = changeOffset.State;
            offsetDelta += offsetState.AxisRaw * dt * 250f;

            lerpOffsetDelta = Lerp(lerpOffsetDelta, offsetDelta, dt * 2f);

            offsetDelta = Clamp(offsetDelta, 0f, MaxOffset);
        }
        protected override void DrawGameExample(ScreenInfo game)
        {
            Vector2 mousePos = game.MousePos;

            float relativeSize = MathF.Min( game.Area.Size.Max() * 0.003f, 15f);

            float vertexRadius = relativeSize * 1.5f; // 8f;
            int pickedVertex = -1;

            bool isMouseOnLine = false; // polyline.OverlapShape(new Circle(mousePos, vertexRadius * 2f));
            var closest = polyline.GetClosestCollisionPoint(mousePos).Point;
            int closestIndex = polyline.GetClosestIndexOnEdge(mousePos);
            bool drawClosest = true;

            var createState = createPoint.State;
            var deleteState = deletePoint.State;

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


            if (createState.Pressed)// && createState.MultiTapCount > 0)// IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
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
            else if (createState.Released)// IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dragIndex = -1;
            }
            else if (dragIndex == -1 && deleteState.Pressed)// deleteState.HoldFinished)// IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                if (pickedVertex > -1)
                {
                    polyline.RemoveAt(pickedVertex);
                }
            }

            if (dragIndex > -1) polyline[dragIndex] = mousePos;

            var segments = polyline.GetEdges();
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                if (drawClosest)
                {
                    if (closestIndex == i)
                    {
                        // segment.Start.Draw(relativeSize / 2, BLUE);
                        segment.Draw(relativeSize, new(Color.CornflowerBlue), LineCapType.CappedExtended, 4);
                    }
                    else
                    {
                        // segment.Start.Draw(relativeSize / 2, WHITE);
                        segment.Draw(relativeSize, new(Color.AntiqueWhite), LineCapType.CappedExtended, 4);
                    }
                }
                else
                {
                    // segment.Start.Draw(relativeSize / 2, WHITE);
                    segment.Draw(relativeSize, new(Color.AntiqueWhite), LineCapType.CappedExtended, 4);
                }
            }

            if (drawClosest) DrawCircleV(closest, vertexRadius, RED);

            if (lerpOffsetDelta > 10f)
            {
                var polygons = ShapeClipper.Inflate(polyline, lerpOffsetDelta).ToPolygons();
                foreach (var polygon in polygons)
                {
                    polygon.DrawLines(relativeSize, new(Color.Goldenrod));
                }
            }
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            
        }
        protected override void DrawUIExample(ScreenInfo ui)
        {
            Vector2 uiSize = ui.Area.Size;

            // var device = input.CurrentInputDevice == InputDevice.Keyboard
                // ? InputDevice.Mouse
                // : input.CurrentInputDevice;
            var curDevice = ShapeInput.CurrentInputDeviceType;
            var create = createPoint. GetInputTypeDescription( curDevice, true, 1, false); 
            var delete = deletePoint. GetInputTypeDescription( curDevice, true, 1, false); 
            var offset = changeOffset.GetInputTypeDescription( curDevice , true, 1, false);
            
            //Rect infoRect = ui.Area.ApplyMargins(0.05f, 0.05f, 0.9f, 0.05f);
            // var bottomCenter = GAMELOOP.UIZones.BottomCenter;
            Rect bottomCenter = GAMELOOP.UIRects.GetRect("bottom center"); // Get("bottom").Get("center").GetRect(); // GAMELOOP.UIRects.GetChild("bottom").GetChild("center").Rect;
            string infoText =
                $"Add Point {create} | Remove Point {delete} | Inflate {offset} {MathF.Round(offsetDelta * 100) / 100}";

            textFont.Color = Colors.Light;
            textFont.DrawTextWrapNone(infoText, bottomCenter, new(0.5f));
            // font.DrawText(infoText, bottomCenter, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }

}
