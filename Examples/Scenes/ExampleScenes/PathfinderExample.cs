

using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Input;
using ShapeEngine.StaticLib;
using ShapeEngine.Pathfinding;
using Path = ShapeEngine.Pathfinding.Path;


namespace Examples.Scenes.ExampleScenes;



internal class PathfinderFlag
{
    private Circle circle;
    // private bool dragging = false;
    // public bool IsDragging => dragging;
    // public bool mouseInside = false;
    public Vector2 Position
    {
        get => circle.Center;
        set => circle = circle.SetPosition(value);
    }
    public PathfinderFlag(Vector2 pos, float r)
    {
        circle = new(pos, r);
    }

    // public void Update(float dt, Vector2 mousePos)
    // {
    //     var action = 
    //         ShapeInput.CurrentInputDeviceType == InputDeviceType.Gamepad || 
    //         ShapeInput.CurrentInputDeviceType == InputDeviceType.Keyboard
    //             ? GAMELOOP.InputActionUIAccept
    //             : GAMELOOP.InputActionUIAcceptMouse;
    //     
    //     
    //     mouseInside = circle.ContainsPoint(mousePos);
    //     if (mouseInside)
    //     {
    //         if (action.State.Pressed)
    //         {
    //             dragging = true;
    //         }
    //     }
    //
    //     if (dragging)
    //     {
    //         circle = new Circle(mousePos, circle.Radius);
    //         if (action.State.Released) dragging = false;
    //     }
    // }

    public void Draw(ColorRgba color)
    {
        // if (mouseInside)
        // {
        //     Circle c = new(circle.Center, circle.Radius * 2f);
        //     c.Draw(color);
        // }
        // else circle.Draw(color);
        circle.Draw(color.ChangeBrightness(-0.05f));
        // circle.DrawLines(circle.Radius * 0.25f, Colors.PcMedium.ColorRgba);
        CircleDrawing.DrawCircleLines(circle.Center, circle.Radius * 1.5f, circle.Radius * 0.15f, Colors.PcText.ColorRgba, 4f);
        // circle.DrawLines(6f, color.ChangeBrightness(-0.5f));
    }
}

    public class PathfinderExample : ExampleScene
    {
        private enum TerrainType
        {
            Default = 0,
            Easy = 1,
            Hard = 2,
            Block = 3
        }
        
        private readonly InputAction iaMoveCameraH;
        private readonly InputAction iaMoveCameraV;

        private readonly InputAction iaCycleTerrainType;
        private readonly InputAction iaZoning;
        private readonly InputAction iaCalculatePath;

        private readonly InputAction iaPositionStartFlag;
        private readonly InputAction iaPositionEndFlag;
        private readonly InputAction iaPortal;
        
        private readonly InputActionTree inputActionTree;

        private readonly Pathfinder pathfinder;

        private bool rectStarted = false;
        private Vector2 rectStartPos = new();

        private PathfinderFlag startFlag;
        private PathfinderFlag endFlag;
        private Path? path = null;
        // private List<PathfinderFlag> endFlags = new();
        // private List<Path> paths = new();

        private Circle? connectionCircleA = null;
        private Circle? connectionCircleB = null;


        private TerrainType currentTerrainType = TerrainType.Default;
        
        
        public PathfinderExample()
        {
            Title = "Pathfinder Example";

            InputActionSettings defaultSettings = new();
            
            var cameraHorizontalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var cameraHorizontalGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad);
            // var cameraHorizontalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            // var cameraHorizontalMW = new InputTypeMouseAxis(ShapeMouseAxis.HORIZONTAL, 0.05f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouse); // new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraH = new(defaultSettings,cameraHorizontalKB, cameraHorizontalGP);
            
            var cameraVerticalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var cameraVerticalGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad);
            // var cameraVerticalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            // var cameraVerticalMW  = new InputTypeMouseAxis(ShapeMouseAxis.VERTICAL, 0.05f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouse); // = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraV = new(defaultSettings,cameraVerticalKB, cameraVerticalGP);


            var cycleTerrainTypeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
            var cycleTerrainTypeGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            var cycleTerrainTypeMb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaCycleTerrainType = new(defaultSettings,cycleTerrainTypeKb, cycleTerrainTypeGp, cycleTerrainTypeMb);

            var zoningKb = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var zoningGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            var zoningMb = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            iaZoning = new(defaultSettings,zoningKb, zoningGp, zoningMb);

            var calculatePathKb = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var calculatePathGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            var calculatePathMb = new InputTypeMouseButton(ShapeMouseButton.MIDDLE);
            iaCalculatePath = new(defaultSettings,calculatePathKb, calculatePathGp, calculatePathMb);


            var positionStartFlagKb = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var positionStartFlagGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            // var positionStartFlagMb = new InputTypeMouseButton(ShapeMouseButton.MW_UP, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaPositionStartFlag = new(defaultSettings,positionStartFlagKb, positionStartFlagGp);
            
            var positionEndFlagKb = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
            var positionEndFlagGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            // var positionEndFlagMb = new InputTypeMouseButton(ShapeMouseButton.MW_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaPositionEndFlag = new(defaultSettings,positionEndFlagKb, positionEndFlagGp);
            
            var portalKb = new InputTypeKeyboardButton(ShapeKeyboardButton.G);
            var portalGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
            // var portalMb = new InputTypeMouseButton(ShapeMouseButton.SIDE);
            iaPortal = new(defaultSettings,portalKb, portalGp);
            
            inputActionTree =
            [
                iaMoveCameraH, iaMoveCameraV,

                iaCycleTerrainType, iaZoning, iaCalculatePath, iaPositionStartFlag, iaPositionEndFlag, iaPortal
            ];

            Rect bounds = new(new(0f), new(8000, 8000), new(0.5f));
            pathfinder = new(bounds, 100, 100);

            startFlag = new(new Vector2(-250), 32);

            
            
            SetupEndFlags();
            
        }

        private void SetupEndFlags()
        {
            // for (int i = 0; i < 1; i++)
            // {
            //     var randPos = pathfinder.Bounds.GetRandomPointInside();
            //     var flag = new PathfinderFlag(randPos, 32);
            //     endFlags.Add(flag);
            // }
            var randPos = pathfinder.Bounds.GetRandomPointInside();
            endFlag = new PathfinderFlag(randPos, 32);
        }
        
        public override void Reset()
        {
            pathfinder.ResetNodes();
            path = null;
            SetupEndFlags();
        }

        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi,  Vector2 mousePosUI)
        {
            var gamepad = GAMELOOP.CurGamepad;
            inputActionTree.CurrentGamepad = gamepad;
            inputActionTree.Update(dt);

            if (ShapeInput.CurrentInputDeviceType == InputDeviceType.Mouse)
            {
                if (ShapeKeyboardButton.LEFT_SHIFT.GetInputState().Down)
                {
                    var dir = ExampleScene.CalculateMouseMovementDirection(GAMELOOP.GameScreenInfo.MousePos, GAMELOOP.Camera);
                    var cam = GAMELOOP.Camera;
                    var f = cam.ZoomFactor;
                    cam.BasePosition += dir * 500 * dt * f;
                }
                
            }
            else
            {
                var moveCameraH = iaMoveCameraH.State.AxisRaw;
                var moveCameraV = iaMoveCameraV.State.AxisRaw;
                var moveCameraDir = new Vector2(moveCameraH, moveCameraV);
                var cam = GAMELOOP.Camera;
                var f = cam.ZoomFactor;
                cam.BasePosition += moveCameraDir * 500 * dt * f;
            }
            
            


            if (iaPositionStartFlag.State.Pressed) startFlag.Position = mousePosGame;
            if (iaPositionEndFlag.State.Pressed) endFlag.Position = mousePosGame;
            
            
            
            // startFlag.Update(dt, mousePosGame);
            // endFlag.Update(dt, mousePosGame);
            // bool dragging = startFlag.IsDragging;
            // foreach (var flag in endFlags)
            // {
            //     flag.Update(dt, mousePosGame);
            //     dragging |= flag.IsDragging;
            // }

            if (iaCycleTerrainType.State.Pressed)
            {
                CycleTerrainType();
            }
            
            if (iaZoning.State.Pressed)
            {
                rectStarted = true;
                rectStartPos = mousePosGame;
            }

            if (iaZoning.State.Released)
            {
                rectStarted = false;
                var r = new Rect(rectStartPos, mousePosGame);
                switch (currentTerrainType)
                {
                    case TerrainType.Default: pathfinder.ApplyNodeValue(r, new (NodeValueType.Reset));
                        break;
                    case TerrainType.Easy: pathfinder.ApplyNodeValue(r, new (5f, NodeValueType.ResetThenSet));
                        break;
                    case TerrainType.Hard: pathfinder.ApplyNodeValue(r, new (-5f, NodeValueType.ResetThenSet));
                        break;
                    case TerrainType.Block: pathfinder.ApplyNodeValue(r, new (NodeValueType.ResetThenBlock));
                        break;
                }
            }

            if (iaCalculatePath.State.Pressed)
            {
                path = pathfinder.GetPath(startFlag.Position, endFlag.Position, 0);
                // paths.Clear();
                // for (int i = 0; i < endFlags.Count; i++)
                // {
                //     var flag = endFlags[i];
                //     var path = pathfinder.GetPath(startFlag.Position, flag.Position, 0);
                //     if(path != null) paths.Add(path);
                // }
            }

            // if (Raylib.IsKeyPressed(KeyboardKey.H))
            // {
            //     var flag = new PathfinderFlag(mousePosGame, 32);
            //     endFlags.Add(flag);
            // }

            if (iaPortal.State.Pressed)
            {
                if (connectionCircleA != null && connectionCircleB != null)
                {
                    var a = (Circle)connectionCircleA;
                    var b = (Circle)connectionCircleB;
                    pathfinder.RemoveConnections(a.GetBoundingBox(), b.GetBoundingBox(), false);

                    connectionCircleA = null; // new Circle(mousePosGame, 64);
                    connectionCircleB = null;
                }
                else if (connectionCircleA != null && connectionCircleB == null)
                {
                    connectionCircleB = new Circle(mousePosGame, 64);
                    var a = (Circle)connectionCircleA;
                    var b = (Circle)connectionCircleB;
                    pathfinder.AddConnections(a.GetBoundingBox(), b.GetBoundingBox(), false);
                }
                else
                {
                    connectionCircleA = new Circle(mousePosGame, 64);
                }
            }
        }

        private void CycleTerrainType()
        {
            int index = (int)currentTerrainType + 1;
            if (index > (int)TerrainType.Block)
            {
                currentTerrainType = TerrainType.Default;
            }

            else currentTerrainType = (TerrainType)index;

        }

        private string GetTerrainTypeName()
        {
            switch (currentTerrainType)
            {
                case TerrainType.Default: return "Default";
                case TerrainType.Easy: return "Easy"; 
                case TerrainType.Hard: return "Hard"; 
                case TerrainType.Block: return "Block";
                default: return "None";
            }
        }

        // protected override void OnPreDrawGame(ScreenInfo game)
        // {
        //     
        // }

        protected override void OnDrawGameExample(ScreenInfo game)
        {
            var cBounds = Colors.PcLight.ColorRgba;
            var cBlocked = Colors.PcWarm.ColorRgba.ChangeBrightness(-0.1f);
            var cDefault = Colors.PcDark.ColorRgba;// new ColorRgba(Color.DarkSlateGray).ChangeBrightness(-0.3f);
            var cDesirable = Colors.PcCold.ColorRgba.ChangeBrightness(-0.25f);
            var cUndesirable = Colors.PcSpecial.ColorRgba.ChangeBrightness(-0.25f);
            pathfinder.DrawDebug(cBounds, cDefault, cBlocked, cDesirable, cUndesirable, 0);

            if (rectStarted)
            {
                var r = new Rect(rectStartPos, game.MousePos);
                var c = Colors.PcMedium.ColorRgba.SetAlpha(200);// new ColorRgba(Color.Gold).SetAlpha(200);
                r.Draw(c);
            }

            if (path != null)
            {
                foreach (var rect in path.Rects)
                {
                    rect.ScaleSize(0.3f, new AnchorPoint(0.5f)).Draw(Colors.PcText.ColorRgba);
                }
            }

            // if (paths.Count > 0)
            // {
            //     foreach (var path in paths)
            //     {
            //         foreach (var rect in path.Rects)
            //         {
            //             rect.ScaleSize(0.3f, new Vector2(0.5f)).Draw(new ColorRgba(Color.DodgerBlue));
            //         }
            //     }
            //     
            // }
            
            startFlag.Draw(Colors.PcHighlight.ColorRgba);
            endFlag.Draw(Colors.PcWarm.ColorRgba);
            // foreach (var flag in endFlags)
            // {
            //     flag.Draw(new ColorRgba(Color.Fuchsia));
            // }
            

            connectionCircleA?.DrawLines(8f, Colors.PcHighlight.ColorRgba);
            connectionCircleB?.DrawLines(8f, Colors.PcWarm.ColorRgba);
            if (connectionCircleA != null && connectionCircleB != null)
            {
                var a = (Circle)connectionCircleA;
                var b = (Circle)connectionCircleB;
                Segment s = new(a.Center, b.Center);
                s.Draw(6f, Colors.PcLight.ColorRgba);
                s.Start.Draw(12f, Colors.PcLight.ColorRgba);
                s.End.Draw(12f, Colors.PcLight.ColorRgba);
            }
        }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            
            var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
            DrawInputText(bottomCenter);
            
            var bottomRight = GAMELOOP.UIRects.GetRect("bottom right");
            var rects = bottomRight.SplitV(0.5f);
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone("Cell Count", rects.top, new(0.5f, 0f));
            
            textFont.DrawTextWrapNone($"{pathfinder.Grid.Count}", rects.bottom, new(0.5f));
        }

        private void DrawInputText(Rect rect)
        {

            var split = rect.SplitV(0.35f);
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            
            string moveCameraH = curInputDeviceAll == InputDeviceType.Mouse ? "[LShift + Mx]" : iaMoveCameraH.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string moveCameraV = curInputDeviceAll == InputDeviceType.Mouse ? "[LShift + My]" : iaMoveCameraV.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string zoomCamera = GAMELOOP.InputActionZoom.GetInputTypeDescription(curInputDeviceAll, true, 1, false);

        // private readonly InputAction iaCycleTerrainType;
        // private readonly InputAction iaZoning;
        // private readonly InputAction iaCalculatePath;
        //
        // private readonly InputAction iaPositionStartFlag;
        // private readonly InputAction iaPositionEndFlag;
            
            string cycleText = iaCycleTerrainType.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string zoningText = iaZoning.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string calculateText = iaCalculatePath.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string positionStartFlagText = iaPositionStartFlag.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string positionEndFlagText = iaPositionEndFlag.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string portalText = iaPortal.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);

            string currentTerrainTypeText = GetTerrainTypeName();
            
            string textTop = $"Camera Move {moveCameraH}{moveCameraV} | Zoom {zoomCamera}";
            string textBottom = $"Terrain ({currentTerrainTypeText}) Cycle: {cycleText} Zone: {zoningText} | Flags {positionStartFlagText}{positionEndFlagText} | Path {calculateText} | Portal {portalText}";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(textTop, split.top, new(0.5f));
            
            textFont.ColorRgba = Colors.Special;
            textFont.DrawTextWrapNone(textBottom, split.bottom, new(0.5f));
        }
    }

