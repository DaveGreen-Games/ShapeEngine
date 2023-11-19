using System.Formats.Tar;
using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.UI;
using System.Numerics;
using ShapeEngine.Lib;
using Examples.Scenes.ExampleScenes;
using Examples.UIElements;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Screen;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace Examples.Scenes
{
    // public class UIBox
    // {
    //     public Rect Rect;
    //     //private List<UIBox> children = new();
    //
    //     public virtual void Update(float dt, Rect parentRect)
    //     {
    //
    //     }
    //     public virtual void Draw()
    //     {
    //         
    //     }
    // }
    
    //Info box
    //-General-
    //Reset
    //Fullscreen
    
    //-Scene-
    //Zoom
    //CRT Shader
    
    
    public class MainScene : IScene
    {
        //should display names/description of all examples
        //left half lists all examples vertical / right half displays short info

        private int curPageIndex = 0;
        private List<ExampleScene> examples = new();
        private List<ExampleSelectionButton> buttons = new();
        private UIElement curButton;
        private Font titleFont;

        private float tabChangeMouseWheelLockTimer = 0f;
        private InputActionLabel quitLabel;
        public MainScene()
        {
            for (var i = 0; i < 10; i++)
            {
                ExampleSelectionButton b = new ExampleSelectionButton();
                buttons.Add(b);
                b.WasSelected += OnButtonSelected;
            }
            curButton = buttons[0];
            examples.Add(new PolylineInflationExample());
            examples.Add(new AsteroidMiningExample());
            examples.Add(new GameObjectHandlerExample());
            examples.Add(new CameraExample());
            examples.Add(new CameraAreaDrawExample());
            examples.Add(new ScreenEffectsExample());
            examples.Add(new DelaunayExample());
            examples.Add(new BouncyCircles());
            examples.Add(new InputExample());
            examples.Add(new ShipInputExample());
            
            examples.Add(new TextScalingExample());
            examples.Add(new TextWrapEmphasisExample());
            examples.Add(new WordEmphasisDynamicExample());
            examples.Add(new TextBoxExample()); 
            
            //examples.Add(new TextEmphasisExample());
            //examples.Add(new WordEmphasisStaticExample());
            //examples.Add(new TextWrapExample());
            //examples.Add(new TextRotationExample());
            //examples.Add(new PolylineCollisionExample());
            //examples.Add(new CCDExample());
            
            titleFont = GAMELOOP.FontDefault; // GAMELOOP.GetFont(GameloopExamples.FONT_IndieFlowerRegular);

            SetupButtons();

            var action = GAMELOOP.InputActionUICancel;// GAMELOOP.Input.GetAction(GameloopExamples.InputUICancelID);
            quitLabel = new(action, "Quit", GAMELOOP.FontDefault, ExampleScene.ColorHighlight3);
        }
        
        public void OnWindowSizeChanged(DimensionConversionFactors conversionFactors)
        {
            
        }

        public void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos)
        {
        }

        public void OnMonitorChanged(MonitorInfo newMonitor)
        {
        }

        public void OnGamepadConnected(Gamepad gamepad)
        {
        }

        public void OnGamepadDisconnected(Gamepad gamepad)
        {
        }

        public void OnInputDeviceChanged(InputDevice prevDevice, InputDevice curDevice)
        {
        }

        public void OnPauseChanged(bool paused){}
        
        public void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var cancelState = Input.ConsumeAction(GAMELOOP.InputActionUICancel);
            if (cancelState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.Quit();
            }

            var resetState = Input.ConsumeAction(GAMELOOP.InputActionReset);
            if (resetState is { Consumed: false, Pressed: true })
            {
                for (int i = 0; i < examples.Count; i++)
                {
                    examples[i].Reset();
                }
                GAMELOOP.Camera.Reset();
                GAMELOOP.ResetCamera();
            }

            var maximizedState = Input.ConsumeAction(GAMELOOP.InputActionMaximize);
            if (maximizedState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.Maximized = !GAMELOOP.Maximized;
            }
            var fullscreenState = Input.ConsumeAction(GAMELOOP.InputActionFullscreen);
            if (fullscreenState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.Fullscreen = !GAMELOOP.Fullscreen;
            }
            
            var prevTabState = Input.ConsumeAction(GAMELOOP.InputActionUIPrevTab);
            if (prevTabState is { Consumed: false, Pressed: true })
            {
                if (prevTabState.InputDevice == InputDevice.Mouse)
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
            var nextTabState = Input.ConsumeAction(GAMELOOP.InputActionUINextTab);
            if (nextTabState is { Consumed: false, Pressed: true })
            { 
                if (nextTabState.InputDevice == InputDevice.Mouse)
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

            var uiDownState = Input.ConsumeAction(GAMELOOP.InputActionUIDown);
            if (uiDownState is { Consumed: false, Pressed: true })
            { 
                NextButton();
            }
            var uiUpState = Input.ConsumeAction(GAMELOOP.InputActionUIUp);
            if (uiUpState is { Consumed: false, Pressed: true })
            { 
                PrevButton();
            }
            
            var nextMonitorState = Input.ConsumeAction(GAMELOOP.InputActionNextMonitor);
            if (nextMonitorState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.NextMonitor();
            }
        }

        public void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            if (tabChangeMouseWheelLockTimer > 0f)
            {
                tabChangeMouseWheelLockTimer -= dt;
                if (tabChangeMouseWheelLockTimer <= 0f) tabChangeMouseWheelLockTimer = 0f;
            }
            HandleInput(dt, game.MousePos, ui.MousePos);
            foreach (var b in buttons)
            {
                b.Update(dt, ui.MousePos);
            }
        }
        
        public void DrawGameUI(ScreenInfo ui)
        {
            
           
        }

        public void DrawUI(ScreenInfo ui)
        {
            var uiSize = ui.Area.Size;
            var start = uiSize * new Vector2(0.02f, 0.25f);
            var size = uiSize * new Vector2(0.45f, 0.05f);
            var gap = uiSize * new Vector2(0f, 0.07f);
            for (int i = 0; i < buttons.Count; i++)
            {
                var b = buttons[i];
                b.UpdateRect(start + gap * i, size, new Vector2(0f));
                b.Draw();
            }


            var text = "Shape Engine Examples";
            var titleRect = new Rect(uiSize * new Vector2(0.5f, 0.01f), uiSize * new Vector2(0.75f, 0.09f), new Vector2(0.5f, 0f));
            titleFont.DrawText(text, titleRect, 10, new(0.5f), ExampleScene.ColorLight);

            int pages = GetMaxPages();
            string prevName = GAMELOOP.InputActionUIPrevTab.GetInputTypeDescription(Input.CurrentInputDevice, true, 1, false);
            string nextName = GAMELOOP.InputActionUINextTab.GetInputTypeDescription(Input.CurrentInputDevice, true, 1, false);
            
            string pagesText = pages <= 1 ? "Page 1/1" : $"{prevName} <- Page #{curPageIndex + 1}/{pages} -> {nextName}";
            var pageRect = new Rect(uiSize * new Vector2(0.01f, 0.12f), uiSize * new Vector2(0.3f, 0.06f), new Vector2(0f, 0f));
            titleFont.DrawText(pagesText, pageRect, 4f, new(0f, 0.5f), ExampleScene.ColorHighlight2);

            Segment s = new(uiSize * new Vector2(0f, 0.22f), uiSize * new Vector2(1f, 0.22f));
            s.Draw(MathF.Max(4f * GAMELOOP.DevelopmentToScreen.AreaFactor, 0.5f), ExampleScene.ColorLight);

            var backRect = new Rect(uiSize * new Vector2(0.01f, 0.17f), uiSize * new Vector2(0.2f, 0.04f), new Vector2(0f, 0f));
            var curInputDevice = Input.CurrentInputDeviceNoMouse;
            quitLabel.Draw(backRect, new Vector2(0f), curInputDevice, 1f);

            var r = ui.Area.ApplyMargins(0.75f, 0.025f, 0.17f, 0.79f);
            titleFont.DrawText($"Cursor On Screen: {ShapeLoop.CursorOnScreen}", r, 1f, new Vector2(1f, 1f), ExampleScene.ColorHighlight2);

            var centerRight = GAMELOOP.UIRects.GetRect("center right");
            var inputInfoRect = centerRight.ApplyMargins(0.25f, -0.025f, 0.15f, 0.55f);
            DrawInputInfoBox(inputInfoRect);
        }

        
        private void OnButtonSelected(UIElement button)
        {
            if (curButton != button)
            {
                GAMELOOP.Cursor.TriggerEffect("scale");
                curButton.Deselect();
                curButton = button;
            }
        }
        private int GetCurButtonIndex()
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                var b = buttons[i];
                if (b.Selected) return i;
            }
            return -1;
        }
        private int GetVisibleButtonCount()
        {
            var count = 0;
            foreach (var b in buttons)
            {
                if (!b.Hidden) count++;
            }
            return count;
        }
        private int GetMaxPages()
        {
            if (buttons.Count <= 0 || examples.Count <= 0) return 1;
            int pages = (examples.Count - 1) / buttons.Count;
            if (pages < 1) return 1;
            else return pages + 1;
        }
        private void SetupButtons()
        {
            int startIndex = curPageIndex * buttons.Count;
            int endIndex = startIndex + buttons.Count;
            var buttonIndex = 0;
            for (int i = curPageIndex * buttons.Count; i < endIndex; i++)
            {
                var b = buttons[buttonIndex];
                buttonIndex++;
                if (i < examples.Count) b.SetScene(examples[i]);
                else b.SetScene(null);
            }

            buttons[0].Select();
        }
        private void NextButton()
        {
            int curButtonIndex = GetCurButtonIndex();
            if (curButtonIndex < 0) return;
            curButtonIndex++;
            int buttonCount = GetVisibleButtonCount();
            if (curButtonIndex >= buttonCount) curButtonIndex = 0;
            buttons[curButtonIndex].Select();
        }
        private void PrevButton()
        {
            int curButtonIndex = GetCurButtonIndex();
            if (curButtonIndex < 0) return;
            curButtonIndex--;
            int buttonCount = GetVisibleButtonCount();
            if (curButtonIndex < 0) curButtonIndex = buttonCount - 1;
            buttons[curButtonIndex].Select();
        }
        private void NextPage()
        {
            int maxPages = GetMaxPages();
            if (maxPages <= 1) return;

            curPageIndex++;
            if (curPageIndex >= maxPages) curPageIndex = 0;
            SetupButtons();
        }
        private void PrevPage()
        {
            int maxPages = GetMaxPages();
            if (maxPages <= 1) return;

            curPageIndex--;
            if (curPageIndex < 0) curPageIndex = maxPages - 1;
            SetupButtons();
        }

        private void DrawInputInfoBox(Rect area)
        {
            //area.DrawLinesDotted(3, 1f, ExampleScene.ColorMedium, LineCapType.Capped, 3);
            //area = area.ApplyMargins(0.05f, 0.05f, 0.01f, 0.01f);
            var curInputDevice = Input.CurrentInputDevice;
            if (curInputDevice == InputDevice.Mouse) curInputDevice = InputDevice.Keyboard;

            string fullscreenInputTypeName = GAMELOOP.InputActionFullscreen.GetInputTypeDescription(curInputDevice, true, 1, false);
            var fullscreenInfo = $"Fullscreen {fullscreenInputTypeName}";
            
            string crtInputTypeNamesPlus = GAMELOOP.InputActionCRTPlus.GetInputTypeDescription(curInputDevice, true, 1, false, false);
            string crtInputTypeNamesMinus = GAMELOOP.InputActionCRTMinus.GetInputTypeDescription(curInputDevice, true, 1, false, false);
            var crtInfo = $"CRT Shader [{crtInputTypeNamesPlus}|{crtInputTypeNamesMinus}]";
            
            string zoomInputTypeName = GAMELOOP.InputActionZoom.GetInputTypeDescription(ShapeLoop.Input.CurrentInputDevice, true, 1, false);
            var zoomInfo = $"Zoom Example {zoomInputTypeName}";
            
            string pauseInputTypeName = GAMELOOP.InputActionPause.GetInputTypeDescription(curInputDevice, true, 1, false);
            var pauseInfo = $"Pause Example {pauseInputTypeName}";
            
            string resetInputTypeName = GAMELOOP.InputActionReset.GetInputTypeDescription(curInputDevice, true, 1, false);
            var resetInfo = $"Reset Example {resetInputTypeName}";

            var rects = area.SplitV(5);

            var color = ExampleScene.ColorMedium;
            var alignement = new Vector2(1f, 0.05f);
            titleFont.DrawText(fullscreenInfo, rects[0], 1f, alignement, color);
            titleFont.DrawText(crtInfo, rects[1], 1f, alignement, color);
            titleFont.DrawText(resetInfo, rects[2], 1f, alignement, color);
            titleFont.DrawText(zoomInfo, rects[3], 1f, alignement, color);
            titleFont.DrawText(pauseInfo, rects[4], 1f, alignement, color);
        }
        
        private void DrawScreenInfoDebug(Rect uiArea)
        {
            var rightHalf = uiArea.ApplyMargins(0.6f, 0.025f, 0.25f, 0.025f);
            //rightHalf.DrawLines(2f, RED);

            List<string> infos = new();
            
            int monitor = Raylib.GetCurrentMonitor();
            infos.Add($"[{monitor}] Monitor Size: {Raylib.GetMonitorWidth(monitor)}|{Raylib.GetMonitorHeight(monitor)}");
            infos.Add($"Window(Screen) Size: {Raylib.GetScreenWidth()}|{Raylib.GetScreenHeight()}");
            infos.Add($"Render Size: {Raylib.GetRenderWidth()}|{Raylib.GetRenderHeight()}");
            infos.Add($"Scale DPI: {Raylib.GetWindowScaleDPI().X}|{Raylib.GetWindowScaleDPI().Y}");
            infos.Add($"HIGH Dpi: {Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_HIGHDPI)}");

            var rects = rightHalf.SplitV(infos.Count);

            for (var i = 0; i < infos.Count; i++)
            {
                string infoText = infos[i];
                var rect = rects[i];
                titleFont.DrawText(infoText, rect, 1f, new Vector2(0.95f, 0.5f), WHITE);
            }
        }
        
        public GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }
        public void Activate(IScene oldScene)
        {
            
            GAMELOOP.SwitchCursor(new SimpleCursorUI());
        }

        public void Deactivate()
        {
            GAMELOOP.SwitchCursor(new SimpleCursorGameUI());
        }

        public void Close()
        {
            
        }

        

        public void DrawGame(ScreenInfo game)
        {
            
        }
    }

}
