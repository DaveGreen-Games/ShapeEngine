using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.UI;
using System.Numerics;
using ShapeEngine.StaticLib;
using Examples.Scenes.ExampleScenes;
using Examples.UIElements;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Persistent;
using ShapeEngine.StaticLib.Drawing;
using ShapeEngine.Text;

namespace Examples.Scenes
{
    public class TestSaveGame : SavegameObject
    {
        public int ID {get; set;}
        public string Text {get; set;}

        public TestSaveGame(int id, string text)
        {
            this.ID = id;
            this.Text = text;
        }
    }
    public class MainScene : Scene
    {
        private readonly List<ExampleScene> examples = new();
        private readonly ControlNodeNavigator navigator;
        private readonly ControlNodeContainer buttonContainer;
        private readonly TextFont titleFont;

        private float tabChangeMouseWheelLockTimer = 0f;
        private readonly InputActionLabel quitLabel;

        private readonly TextureSurface textureSurface;
        public MainScene()
        {
            examples.Add(new OutlineDrawingExample());
            examples.Add(new StripedShapeDrawingExample());
            examples.Add(new ShapeIntersectionExample());
            examples.Add(new CurveDataExample());
            examples.Add(new PhysicsExample());
            examples.Add(new EndlessSpaceCollision());
            examples.Add(new PolylineInflationExample());
            examples.Add(new AsteroidMiningExample());
            examples.Add(new GameObjectHandlerExample());
            examples.Add(new ScreenEffectsExample());
            examples.Add(new CameraGroupFollowExample());
            examples.Add(new ShipInputExample());
            examples.Add(new DataExample());
            
            examples.Add(new InputExample());
            examples.Add(new CameraExample());
            examples.Add(new CameraAreaDrawExample());
            examples.Add(new BouncyCircles());
            examples.Add(new DelaunayExample());
            
            examples.Add(new TextScalingExample());
            examples.Add(new TextWrapEmphasisExample());
            examples.Add(new TextBoxExample()); 
            examples.Add(new ControlNodeExampleScene()); 
            examples.Add(new PathfinderExample()); 
            examples.Add(new PathfinderExample2()); 
            
            buttonContainer = new ControlNodeContainer
            {
                Anchor = new AnchorPoint(0.05f, 0.92f),
                Stretch = new Vector2(0.45f, 0.75f)
            };
            buttonContainer.AlwaysKeepFilled = false;
            buttonContainer.Gap = new Vector2(0.025f, 0.025f);
            buttonContainer.DisplayIndex = 0;
            buttonContainer.NavigationStep = 1;
            buttonContainer.Grid = new(1, 12, false, false, false);
            
            for (var i = 0; i < examples.Count; i++)
            {
                var b = new ExampleSelectionButton();
                // b.OnSelectedChanged += OnButtonSelected;
                b.SetScene(examples[i]);
                buttonContainer.AddChild(b);
            }

            navigator = new ControlNodeNavigator();
            navigator.AddNode(buttonContainer);
            
            titleFont = new(GAMELOOP.FontDefault, 10f, Colors.Text);
            var action = GAMELOOP.InputActionUICancel;
            quitLabel = new(action, "Quit", GAMELOOP.FontDefault, Colors.PcWarm);
            
            textureSurface = new TextureSurface(2048, 2048);
            textureSurface.SetTextureFilter(TextureFilter.Trilinear);
            textureSurface.BeginDraw(ColorRgba.Clear);
            LineDrawingInfo stripedInfo = new(2f, ColorRgba.White, LineCapType.Capped, 6);
            textureSurface.Rect.DrawStriped(16f, 30f, stripedInfo);
            textureSurface.EndDraw();
        }
        
        
        private void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var cancelState = GAMELOOP.InputActionUICancel.Consume();
            if (cancelState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.Quit();
            }

            var resetState = GAMELOOP.InputActionReset.Consume();
            if (resetState is { Consumed: false, Pressed: true })
            {
                for (int i = 0; i < examples.Count; i++)
                {
                    examples[i].Reset();
                }
                GAMELOOP.Camera.Reset();
                GAMELOOP.ResetCamera();
            }

            var maximizedState = GAMELOOP.InputActionMaximize.Consume();
            if (maximizedState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.Window.ToggleMaximizeWindow();
            }
            var minimizeState = GAMELOOP.InputActionMinimize.Consume();
            if (minimizeState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.Window.ToggleMinimizeWindow();
            }
            var fullscreenState = GAMELOOP.InputActionFullscreen.Consume();
            if (fullscreenState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.Window.ToggleBorderlessFullscreen();
            }

            var prevTabState = GAMELOOP.InputActionUIPrevTab.Consume();
            if (prevTabState is { Consumed: false, Pressed: true })
            {
                if (prevTabState.InputDeviceType == InputDeviceType.Mouse)
                {
                    if (tabChangeMouseWheelLockTimer <= 0)
                    {
                        PrevPage();
                        tabChangeMouseWheelLockTimer = 0.5f;
                    }
                }
                else
                {
                    PrevPage();
                    tabChangeMouseWheelLockTimer = 0f;
                }
                //PrevPage();
            }

            var nextTabState = GAMELOOP.InputActionUINextTab.Consume();
            if (nextTabState is { Consumed: false, Pressed: true })
            { 
                if (nextTabState.InputDeviceType == InputDeviceType.Mouse)
                {
                    if (tabChangeMouseWheelLockTimer <= 0)
                    {
                        NextPage();
                        tabChangeMouseWheelLockTimer = 0.5f;
                    }
                }
                else
                {
                    NextPage();
                    tabChangeMouseWheelLockTimer = 0f;
                }
                // NextPage();
            }

            // var uiDownState = GAMELOOP.InputActionUIDown.Consume();
            // if (uiDownState is { Consumed: false, Pressed: true })
            // { 
            //     // NextButton();
            // }
            //
            // var uiUpState = GAMELOOP.InputActionUIUp.Consume();
            // if (uiUpState is { Consumed: false, Pressed: true })
            // { 
            //     // PrevButton();
            // }

            var nextMonitorState = GAMELOOP.InputActionNextMonitor.Consume();
            if (nextMonitorState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.Window.NextMonitor();
            }
        }
        protected override void OnUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            if (tabChangeMouseWheelLockTimer > 0f)
            {
                tabChangeMouseWheelLockTimer -= time.Delta;
                if (tabChangeMouseWheelLockTimer <= 0f) tabChangeMouseWheelLockTimer = 0f;
            }
            HandleInput(time.Delta, game.MousePos, ui.MousePos);
            
            buttonContainer.UpdateRect(ui.Area);
            buttonContainer.Update(time.Delta, ui.MousePos);
            navigator.Update();
            // foreach (var b in buttons)
            // {
            //     b.Update(time.Delta, ui.MousePos);
            // }
        }
        protected override void OnDrawUI(ScreenInfo ui)
        {
            var source = textureSurface.Rect.ScaleSize(new Vector2(1f, 0.22f), AnchorPoint.TopCenter);
            var destination = ui.Area.ScaleSize(new Vector2(1f, 0.22f), AnchorPoint.TopCenter);
            textureSurface.Draw(source, destination, Colors.Dark);
            
            GAMELOOP.DrawFpsBox();
            
            var uiSize = ui.Area.Size.ToVector2();
            buttonContainer.Draw();

            var text = "Shape Engine Examples";
            var titleRect = new Rect(uiSize * new Vector2(0.5f, 0.01f), uiSize.ToSize() * new Size(0.75f, 0.09f), new AnchorPoint(0.5f, 0f));
            titleFont.FontSpacing = 10f;
            titleFont.ColorRgba = Colors.Text;
            titleFont.DrawTextWrapNone(text, titleRect, new(0.5f));
            int pages = buttonContainer.MaxPages; // GetMaxPages();
            int curPage = buttonContainer.CurPage;
            string prevName = GAMELOOP.InputActionUIPrevTab.GetInputTypeDescription(ShapeInput.CurrentInputDeviceType, true, 1, false);
            string nextName = GAMELOOP.InputActionUINextTab.GetInputTypeDescription(ShapeInput.CurrentInputDeviceType, true, 1, false);
            
            string pagesText = $"{prevName} <- Page #{curPage}/{pages} -> {nextName}";
            var pageRect = new Rect(uiSize * new Vector2(0.01f, 0.12f), uiSize.ToSize() * new Vector2(0.3f, 0.06f), new AnchorPoint(0f, 0f));
            titleFont.FontSpacing = 4f;
            titleFont.ColorRgba = Colors.Highlight;
            titleFont.DrawTextWrapNone(pagesText, pageRect, new(0f, 0.5f));

            Segment s = new(uiSize * new Vector2(0f, 0.22f), uiSize * new Vector2(1f, 0.22f));
            float thickness = ui.Area.Height * 0.0025f;
            s.Draw(thickness, Colors.Light);

            var backRect = new Rect(uiSize * new Vector2(0.01f, 0.17f), uiSize.ToSize() * new Size(0.2f, 0.04f), new AnchorPoint(0f, 0f));
            var curInputDevice = ShapeInput.CurrentInputDeviceTypeNoMouse;
            
            quitLabel.Draw(backRect, new AnchorPoint(0f), curInputDevice);

            var infoArea = ui.Area.ApplyMargins(0.7f, 0.025f, 0.14f, 0.79f);
            var infoAreaRects = infoArea.SplitV(0.5f);
            float p = GAMELOOP.Window.GetScreenPercentage();
            int pi = (int)MathF.Round(p * 100);
            
            titleFont.FontSpacing = 1f;
            titleFont.ColorRgba = Colors.Medium;
            titleFont.DrawTextWrapNone($"Window Focused: {GameWindow.Instance.IsWindowFocused} | [{pi}%]", infoAreaRects.top, new AnchorPoint(1f, 1f));
            titleFont.DrawTextWrapNone($"Cursor On Screen: {GameWindow.Instance.MouseOnScreen}", infoAreaRects.bottom, new AnchorPoint(1f, 1f));


            var inputInfoRect = ui.Area.ApplyMargins(0.75f, 0.01f, 0.75f, 0.01f);
            // var centerRight = GAMELOOP.UIRects.GetRect("center right");//.Union(GAMELOOP.UIRects.GetRect("bottom right"));
            // var inputInfoRect = centerRight.ApplyMargins(-2f, 0f, 0.65f, 0f);
            DrawInputInfoBox(inputInfoRect);


            // var binaryDrawerRect = ui.Area.ApplyMargins(0.05f, 0.05f, 0.32f, 0.32f);
            // binaryDrawerRect.Draw(Colors.Medium);
            // BinaryDrawerTester.BinaryDrawer3x5Standard.Draw("8439567102", binaryDrawerRect.ApplyMargins(0.025f));
            
            // ShapeDrawing.DrawArrow3(ui.Area.Center, ui.MousePos, 0.25f, 0.25f, new LineDrawingInfo(4f, Colors.Warm, LineCapType.CappedExtended, 8), ColorRgba.Clear);
        }


        protected override void OnActivate(Scene oldScene)
        {
            // GAMELOOP.Window.SwitchCursor(new SimpleCursorUI());
            navigator.StartNavigation();
        }

        protected override void OnDeactivate()
        {
            // GAMELOOP.Window.SwitchCursor(new SimpleCursorGameUI());
            navigator.EndNavigation();
        }

        protected override void OnClose()
        {
            foreach (var example in examples)
            {
                example.Close();
            }
            
            textureSurface.Unload();
        }
        
        
        private void NextPage() => buttonContainer.NextPage(true);
        private void PrevPage() => buttonContainer.PrevPage(true);
        private void DrawInputInfoBox(Rect area)
        {
            area.Draw(Colors.Dark);
            var sourceRect = new Rect(Vector2.Zero, area.Size * 2);
            textureSurface.Draw(sourceRect, area, Colors.Highlight.ChangeBrightness(-0.75f));
            area.DrawLines(2f, Colors.PcMedium.ColorRgba);

            var margin = area.Size.Min() * 0.025f;
            area = area.ApplyMarginsAbsolute(margin, margin, margin, margin);
            
            var curInputDevice = ShapeInput.CurrentInputDeviceType;
            if (curInputDevice == InputDeviceType.Mouse) curInputDevice = InputDeviceType.Keyboard;

            string fullscreenInputTypeName = GAMELOOP.InputActionFullscreen.GetInputTypeDescription(curInputDevice, true, 1, false);
            var fullscreenInfo = $"Fullscreen {fullscreenInputTypeName}";

            string cycleShaderInputTypeDescription = GAMELOOP.InputActionCycleShaders.GetInputTypeDescription(InputDeviceType.Keyboard, true, 1, false);
            var cycleShaderInfo = $"Cycle Shaders {cycleShaderInputTypeDescription}";
            
            // string crtInputTypeNamesPlus = GAMELOOP.InputActionCRTPlus.GetInputTypeDescription(InputDeviceType.Keyboard, true, 1, false, false);
            // string crtInputTypeNamesMinus = GAMELOOP.InputActionCRTMinus.GetInputTypeDescription(InputDeviceType.Keyboard, true, 1, false, false);
            // var crtInfo = $"Shader [{crtInputTypeNamesPlus}|{crtInputTypeNamesMinus}]";
            
            string zoomInputTypeName = GAMELOOP.InputActionZoom.GetInputTypeDescription(ShapeInput.CurrentInputDeviceType, true, 1, false);
            var zoomInfo = $"Zoom {zoomInputTypeName}";
            
            // string pauseInputTypeName = GAMELOOP.InputActionPause.GetInputTypeDescription(curInputDevice, true, 1, false);
            // var pauseInfo = $"Pause {pauseInputTypeName}";
            string palleteInputTypeName = GAMELOOP.InputActionCyclePalette.GetInputTypeDescription(curInputDevice, true, 1, false);
            var pauseInfo = $"Palette {palleteInputTypeName}";
            
            string resetInputTypeName = GAMELOOP.InputActionReset.GetInputTypeDescription(curInputDevice, true, 1, false);
            var resetInfo = $"Reset {resetInputTypeName}";

            var rects = curInputDevice == InputDeviceType.Gamepad ? area.SplitV(6) : area.SplitV(5);

            var color = Colors.Medium;
            var alignement = new AnchorPoint(1f, 0.05f);
            titleFont.FontSpacing = 1f;
            titleFont.ColorRgba = color;
            titleFont.DrawTextWrapNone(fullscreenInfo, rects[0], alignement);
            titleFont.DrawTextWrapNone(cycleShaderInfo, rects[1], alignement);
            titleFont.DrawTextWrapNone(resetInfo, rects[2], alignement);
            titleFont.DrawTextWrapNone(zoomInfo, rects[3], alignement);
            titleFont.DrawTextWrapNone(pauseInfo, rects[4], alignement);
            
            if (curInputDevice == InputDeviceType.Gamepad)
            {
                string mouseMovementModifierName = GameloopExamples.ModifierKeyGamepad.GetName(true);// GAMELOOP.InputActionReset.GetInputTypeDescription(curInputDevice, true, 1, false);
                var mouseMovementInfo = $"Mouse [{mouseMovementModifierName} + LS]";
                titleFont.DrawTextWrapNone(mouseMovementInfo, rects[5], alignement);
            }
            
        }
        
        private void DrawScreenInfoDebug(Rect uiArea)
        {
            var rightHalf = uiArea.ApplyMargins(0.6f, 0.025f, 0.25f, 0.025f);
            List<string> infos = new();
            
            int monitor = Raylib.GetCurrentMonitor();
            infos.Add($"[{monitor}] Monitor Size: {Raylib.GetMonitorWidth(monitor)}|{Raylib.GetMonitorHeight(monitor)}");
            infos.Add($"Window(Screen) Size: {Raylib.GetScreenWidth()}|{Raylib.GetScreenHeight()}");
            infos.Add($"Render Size: {Raylib.GetRenderWidth()}|{Raylib.GetRenderHeight()}");
            infos.Add($"Scale DPI: {Raylib.GetWindowScaleDPI().X}|{Raylib.GetWindowScaleDPI().Y}");
            infos.Add($"HIGH Dpi: {Raylib.IsWindowState(ConfigFlags.HighDpiWindow)}");

            var rects = rightHalf.SplitV(infos.Count);

            for (var i = 0; i < infos.Count; i++)
            {
                string infoText = infos[i];
                var rect = rects[i];
                titleFont.FontSpacing = 1f;
                titleFont.ColorRgba = ColorRgba.White;
                titleFont.DrawTextWrapNone(infoText, rect, new AnchorPoint(0.95f, 0.5f));
            }
        }
    }

}
