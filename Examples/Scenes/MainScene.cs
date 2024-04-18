using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.UI;
using System.Numerics;
using ShapeEngine.Lib;
using Examples.Scenes.ExampleScenes;
using Examples.UIElements;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Text;

namespace Examples.Scenes
{
    public class MainScene : Scene
    {
        private readonly List<ExampleScene> examples = new();
        private readonly ControlNodeNavigator navigator;
        private readonly ControlNodeContainer buttonContainer;
        private readonly TextFont titleFont;

        private float tabChangeMouseWheelLockTimer = 0f;
        private readonly InputActionLabel quitLabel;
        public MainScene()
        {
            examples.Add(new ShapesExample());
            examples.Add(new ProjectedShapesExample());
            examples.Add(new EndlessSpaceCollision());
            examples.Add(new PolylineInflationExample());
            examples.Add(new AsteroidMiningExample());
            examples.Add(new GameObjectHandlerExample());
            examples.Add(new ScreenEffectsExample());
            examples.Add(new CameraGroupFollowExample());
            examples.Add(new ShipInputExample());
            
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
                Anchor = new Vector2(0.05f, 0.92f),
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
                // GAMELOOP.Maximized = !GAMELOOP.Maximized;
                GAMELOOP.Window.DisplayState = GAMELOOP.Window.DisplayState == WindowDisplayState.Maximized ? WindowDisplayState.Normal : WindowDisplayState.Maximized;
            }
            var minimizeState = GAMELOOP.InputActionMinimize.Consume();
            if (minimizeState is { Consumed: false, Pressed: true })
            { 
                // GAMELOOP.Maximized = !GAMELOOP.Maximized;
                GAMELOOP.Window.DisplayState = GAMELOOP.Window.DisplayState == WindowDisplayState.Minimized ? WindowDisplayState.Normal : WindowDisplayState.Minimized;
            }
            var fullscreenState = GAMELOOP.InputActionFullscreen.Consume();
            if (fullscreenState is { Consumed: false, Pressed: true })
            { 
                // GAMELOOP.Fullscreen = !GAMELOOP.Fullscreen;
                GAMELOOP.Window.DisplayState = GAMELOOP.Window.DisplayState == WindowDisplayState.Fullscreen ? WindowDisplayState.Normal : WindowDisplayState.Fullscreen;
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

            var uiDownState = GAMELOOP.InputActionUIDown.Consume();
            if (uiDownState is { Consumed: false, Pressed: true })
            { 
                // NextButton();
            }

            var uiUpState = GAMELOOP.InputActionUIUp.Consume();
            if (uiUpState is { Consumed: false, Pressed: true })
            { 
                // PrevButton();
            }

            var nextMonitorState = GAMELOOP.InputActionNextMonitor.Consume();
            if (nextMonitorState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.Window.NextMonitor();
            }
        }
        protected override void OnUpdateGame(GameTime time, ScreenInfo game, ScreenInfo ui)
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
            var uiSize = ui.Area.Size.ToVector2();
            buttonContainer.Draw();

            var text = "Shape Engine Examples";
            var titleRect = new Rect(uiSize * new Vector2(0.5f, 0.01f), uiSize.ToSize() * new Size(0.75f, 0.09f), new Vector2(0.5f, 0f));
            titleFont.FontSpacing = 10f;
            titleFont.ColorRgba = Colors.Text;
            titleFont.DrawTextWrapNone(text, titleRect, new(0.5f));
            int pages = buttonContainer.MaxPages; // GetMaxPages();
            int curPage = buttonContainer.CurPage;
            string prevName = GAMELOOP.InputActionUIPrevTab.GetInputTypeDescription(ShapeInput.CurrentInputDeviceType, true, 1, false);
            string nextName = GAMELOOP.InputActionUINextTab.GetInputTypeDescription(ShapeInput.CurrentInputDeviceType, true, 1, false);
            
            string pagesText = $"{prevName} <- Page #{buttonContainer.DisplayIndex} - {curPage}/{pages} -> {nextName}";
            var pageRect = new Rect(uiSize * new Vector2(0.01f, 0.12f), uiSize.ToSize() * new Vector2(0.3f, 0.06f), new Vector2(0f, 0f));
            titleFont.FontSpacing = 4f;
            titleFont.ColorRgba = Colors.Highlight;
            titleFont.DrawTextWrapNone(pagesText, pageRect, new(0f, 0.5f));

            Segment s = new(uiSize * new Vector2(0f, 0.22f), uiSize * new Vector2(1f, 0.22f));
            s.Draw(MathF.Max(4f * GAMELOOP.DevelopmentToScreen.AreaFactor, 0.5f), Colors.Light);

            var backRect = new Rect(uiSize * new Vector2(0.01f, 0.17f), uiSize.ToSize() * new Size(0.2f, 0.04f), new Vector2(0f, 0f));
            var curInputDevice = ShapeInput.CurrentInputDeviceTypeNoMouse;
            
            quitLabel.Draw(backRect, new Vector2(0f), curInputDevice);

            var infoArea = ui.Area.ApplyMargins(0.7f, 0.025f, 0.14f, 0.79f);
            var infoAreaRects = infoArea.SplitV(0.5f);
            float p = GAMELOOP.Window.GetScreenPercentage();
            int pi = (int)MathF.Round(p * 100);
            
            titleFont.FontSpacing = 1f;
            titleFont.ColorRgba = Colors.Medium;
            titleFont.DrawTextWrapNone($"Window Focused: {Raylib.IsWindowFocused()} | [{pi}%]", infoAreaRects.top, new Vector2(1f, 1f));
            titleFont.DrawTextWrapNone($"Cursor On Screen: {GameWindow.IsMouseOnScreen}", infoAreaRects.bottom, new Vector2(1f, 1f));
            
            var centerRight = GAMELOOP.UIRects.GetRect("center right");
            var inputInfoRect = centerRight.ApplyMargins(0.25f, -0.025f, 0.15f, 0.55f);
            DrawInputInfoBox(inputInfoRect);
        }
        
        
        public override void Activate(Scene oldScene)
        {
            GAMELOOP.Window.SwitchCursor(new SimpleCursorUI());
            navigator.StartNavigation();
        }
        public override void Deactivate()
        {
            GAMELOOP.Window.SwitchCursor(new SimpleCursorGameUI());
            navigator.EndNavigation();
        }

        public override void Close()
        {
            
        }
        
        
        private void NextPage() => buttonContainer.NextPage(true);
        private void PrevPage() => buttonContainer.PrevPage(true);
        private void DrawInputInfoBox(Rect area)
        {
            var curInputDevice = ShapeInput.CurrentInputDeviceType;
            if (curInputDevice == InputDeviceType.Mouse) curInputDevice = InputDeviceType.Keyboard;

            string fullscreenInputTypeName = GAMELOOP.InputActionFullscreen.GetInputTypeDescription(curInputDevice, true, 1, false);
            var fullscreenInfo = $"Fullscreen {fullscreenInputTypeName}";
            
            string crtInputTypeNamesPlus = GAMELOOP.InputActionCRTPlus.GetInputTypeDescription(curInputDevice, true, 1, false, false);
            string crtInputTypeNamesMinus = GAMELOOP.InputActionCRTMinus.GetInputTypeDescription(curInputDevice, true, 1, false, false);
            var crtInfo = $"CRT Shader [{crtInputTypeNamesPlus}|{crtInputTypeNamesMinus}]";
            
            string zoomInputTypeName = GAMELOOP.InputActionZoom.GetInputTypeDescription(ShapeInput.CurrentInputDeviceType, true, 1, false);
            var zoomInfo = $"Zoom Example {zoomInputTypeName}";
            
            string pauseInputTypeName = GAMELOOP.InputActionPause.GetInputTypeDescription(curInputDevice, true, 1, false);
            var pauseInfo = $"Pause Example {pauseInputTypeName}";
            
            string resetInputTypeName = GAMELOOP.InputActionReset.GetInputTypeDescription(curInputDevice, true, 1, false);
            var resetInfo = $"Reset Example {resetInputTypeName}";

            var rects = area.SplitV(5);

            var color = Colors.Medium;
            var alignement = new Vector2(1f, 0.05f);
            titleFont.FontSpacing = 1f;
            titleFont.ColorRgba = color;
            titleFont.DrawTextWrapNone(fullscreenInfo, rects[0], alignement);
            titleFont.DrawTextWrapNone(crtInfo, rects[1], alignement);
            titleFont.DrawTextWrapNone(resetInfo, rects[2], alignement);
            titleFont.DrawTextWrapNone(zoomInfo, rects[3], alignement);
            titleFont.DrawTextWrapNone(pauseInfo, rects[4], alignement);
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
                titleFont.DrawTextWrapNone(infoText, rect, new Vector2(0.95f, 0.5f));
            }
        }
    }

}
