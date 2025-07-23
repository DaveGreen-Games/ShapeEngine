using ShapeEngine.StaticLib;
using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;

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
        private readonly InputActionTree inputActionTree;
        
        private bool collisionSegmentValid = false;
        private Segment collisionSegment = new(); // new(new(-192, -450), new(466, 750));
        
        public PolylineInflationExample()
        {
            Title = "Polyline Inflation Example";
            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            InputActionSettings defaultSettings = new();

            var modifierKeySetGpReversed = new ModifierKeySet(ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            
            var createMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            var createGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            var createKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            createPoint = new(defaultSettings,createMB, createGP, createKB);

            var deleteMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            var deleteGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            var deleteKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            deletePoint = new(defaultSettings,deleteMB, deleteGP, deleteKB);

            var offsetMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f);
            var offsetKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.S, ShapeKeyboardButton.W);
            var offsetGP = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_DOWN, ShapeGamepadButton.LEFT_FACE_UP, 0.1f, modifierKeySetGpReversed);
            changeOffset = new(defaultSettings,offsetMW, offsetGP, offsetKB);
            
            inputActionTree = [ 
                createPoint,
                deletePoint,
                changeOffset
            ];
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;

        }

        protected override void OnActivate(Scene oldScene)
        {
            GAMELOOP.InputActionZoom.Active = false;
        }

        protected override void OnDeactivate()
        {
            GAMELOOP.InputActionZoom.Active = true;
        }

        public override void Reset()
        {
            polyline = new();
            dragIndex = -1;
            offsetDelta = 0f;
            lerpOffsetDelta = 0f;
            collisionSegment = new();
            collisionSegmentValid = false;
        }
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame,Vector2 mousePosGameUi,  Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosGameUi, mousePosUI);
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            var gamepad = GAMELOOP.CurGamepad;
            
            inputActionTree.CurrentGamepad = gamepad;
            inputActionTree.Update(dt);

            var offsetState = changeOffset.State;
            offsetDelta += offsetState.AxisRaw * dt * 250f;

            lerpOffsetDelta = ShapeMath.LerpFloat(lerpOffsetDelta, offsetDelta, dt * 2f);

            offsetDelta = ShapeMath.Clamp(offsetDelta, 0f, MaxOffset);


            if (ShapeInput.ActiveKeyboardDevice.IsDown(ShapeKeyboardButton.H))
            {
                collisionSegmentValid = true;
                collisionSegment = collisionSegment.SetStart(mousePosGame);
            }

            if (ShapeInput.ActiveKeyboardDevice.IsDown(ShapeKeyboardButton.G))
            {
                collisionSegmentValid = true;
                collisionSegment = collisionSegment.SetEnd(mousePosGame);
            }
            

        }
        protected override void OnDrawGameExample(ScreenInfo game)
        {
            Vector2 mousePos = game.MousePos;

            float relativeSize = MathF.Min( game.Area.Size.Max() * 0.003f, 15f);

            float vertexRadius = relativeSize * 1.5f; // 8f;
            int pickedVertex = -1;

            bool isMouseOnLine = false; // polyline.OverlapShape(new Circle(mousePos, vertexRadius * 2f));
            var closest = polyline.GetClosestPoint(mousePos, out float closestDistanceSquared, out int closestIndex);
            bool drawClosest = true;

            var createState = createPoint.State;
            var deleteState = deletePoint.State;
            
            for (int i = 0; i < polyline.Count; i++)
            {
                var p = polyline[i];
                float disSq = (mousePos - p).LengthSquared();
                if (pickedVertex == -1 && disSq < (vertexRadius * vertexRadius) * 2f)
                {
                    CircleDrawing.DrawCircle(p, vertexRadius * 2f, Colors.Highlight);
                    pickedVertex = i;
                }
                else CircleDrawing.DrawCircle(p, vertexRadius, Colors.Medium);
                if (drawClosest)
                {
                    disSq = (closest.Point - p).LengthSquared();
                    if (disSq < (vertexRadius * vertexRadius) * 4f)
                    {
                        drawClosest = false;
                    }
                }

            }

            if (drawClosest)
            {

                float tresholdSq = 30 * 30;
                if (closestDistanceSquared > tresholdSq)
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
                        segment.Draw(relativeSize, Colors.Cold, LineCapType.CappedExtended, 4);
                    }
                    else
                    {
                        // segment.Start.Draw(relativeSize / 2, WHITE);
                        segment.Draw(relativeSize, Colors.Light, LineCapType.CappedExtended, 4);
                    }
                }
                else
                {
                    // segment.Start.Draw(relativeSize / 2, WHITE);
                    segment.Draw(relativeSize, Colors.Light, LineCapType.CappedExtended, 4);
                }
            }

            if (drawClosest) CircleDrawing.DrawCircle(closest.Point, vertexRadius, Colors.Warm);

            Polygons? inflatedPolygons = null;
            if (lerpOffsetDelta > 10f)
            {
                inflatedPolygons = ShapeClipper.Inflate(polyline, lerpOffsetDelta).ToPolygons();
                foreach (var polygon in inflatedPolygons)
                {
                    polygon.DrawLines(relativeSize, Colors.Special);
                }
            }


            if (collisionSegmentValid)
            {
                var intersectionHappend = false;
                if (inflatedPolygons != null)
                {
                    foreach (var polygon in inflatedPolygons)
                    {
                        var intersectionPoints = collisionSegment.IntersectShape(polygon);
                        if (intersectionPoints != null && intersectionPoints.Count > 0)
                        {
                            foreach (var p in intersectionPoints)
                            {
                                if (p.Valid)
                                {
                                    if(!intersectionHappend) intersectionHappend = true;
                                    p.Point.Draw(8f, Colors.Warm, 10);
                                    Segment normal = new(p.Point, p.Point + p.Normal * 50f);
                                    normal.Draw(4f, Colors.Warm);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var intersectionPoints = collisionSegment.IntersectShape(polyline);// polyline.IntersectShape(collisionSegment);
                    if (intersectionPoints != null && intersectionPoints.Count > 0)
                    {
                        foreach (var p in intersectionPoints)
                        {
                            if (p.Valid)
                            {
                                if(!intersectionHappend) intersectionHappend = true;
                                p.Point.Draw(8f, Colors.Warm, 10);
                                Segment normal = new(p.Point, p.Point + p.Normal * 50f);
                                normal.Draw(4f, Colors.Warm);
                            }
                        }
                    }
                }
                
                if(intersectionHappend) collisionSegment.Draw(4f, Colors.Highlight, LineCapType.None);
                else collisionSegment.Draw(4f,Colors.Light, LineCapType.None);
            }
        }
        protected override void OnDrawGameUIExample(ScreenInfo gameUi)
        {
            
        }
        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            // Vector2 uiSize = ui.Area.Size;

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

            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(infoText, bottomCenter, new(0.5f));
            // font.DrawText(infoText, bottomCenter, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }

}
