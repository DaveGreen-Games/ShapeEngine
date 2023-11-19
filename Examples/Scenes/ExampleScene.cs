using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using Examples.UIElements;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.UI;

namespace Examples.Scenes
{
    public class ExampleScene : IScene
    {

        public static Color ColorDark = ShapeColor.HexToColor("0A131F");
        public static Color ColorDarkB = ShapeColor.HexToColor("#121F2B");
        public static Color ColorMedium = ShapeColor.HexToColor("1F3847");
        public static Color ColorLight = ShapeColor.HexToColor("B6E0E2");
        public static Color ColorHighlight1 = ShapeColor.HexToColor("E5F6DF");
        public static Color ColorHighlight2 = ShapeColor.HexToColor("E94957");
        public static Color ColorHighlight3 = ShapeColor.HexToColor("#FCA311");
        public static Color ColorRustyRed = ShapeColor.HexToColor("#DE3C4B");

        public string Title { get; protected set; } = "Title Goes Here";
        public string Description { get; protected set; } = "No Description Yet.";

        protected Font titleFont = GAMELOOP.FontDefault;

        //protected readonly ShapeInput input = GAMELOOP.Input;
        private InputActionLabel backLabel;

        public ExampleScene()
        {
            var action = GAMELOOP.InputActionUICancel;
            backLabel = new(action, "BACK", GAMELOOP.FontDefault, ExampleScene.ColorHighlight3);
        }
        public virtual void Reset() { }

        public virtual void OnPauseChanged(bool paused){}
        
        private void HandleZoom(float dt)
        {
            float zoomSpeed = 1f;
            float zoomDir = 0;
            
            var zoomState = Input.ConsumeAction(GAMELOOP.InputActionZoom);
            if (!zoomState.Consumed)
            {
                zoomDir = -zoomState.AxisRaw;
            }
            
            if (zoomDir != 0)
            {
                GAMELOOP.Camera.Zoom(zoomDir * zoomSpeed * dt);
            }
        }
        
        protected void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var cancelState = Input.ConsumeAction(GAMELOOP.InputActionUICancel);
            if (cancelState is { Consumed: false, Pressed: true })
            {
                if(GAMELOOP.Paused) GAMELOOP.Paused = false;
                GAMELOOP.GoToMainScene();
            }
            
            var pausedState = Input.ConsumeAction(GAMELOOP.InputActionPause);
            if (pausedState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.Paused = !GAMELOOP.Paused;
            }

            if (GAMELOOP.Paused) return;
            
            
            var resetState = Input.ConsumeAction(GAMELOOP.InputActionReset);
            if (resetState is { Consumed: false, Pressed: true })
            {
                Reset();
            }
            
            HandleZoom(dt);
            // float zoomIncrement = 0.05f;
            // var zoomInState = input.ConsumeAction(GameloopExamples.InputZoomInID);
            // if (zoomInState is { Consumed: false, Pressed: true })
            // {
            //     GAMELOOP.Camera.Zoom(-zoomIncrement);
            // }
            //
            // var zoomOutState = input.ConsumeAction(GameloopExamples.InputZoomOutID);
            // if (zoomOutState is { Consumed: false, Pressed: true })
            // {
            //     GAMELOOP.Camera.Zoom(zoomIncrement);
            // }
        }

        public void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            HandleInput(dt, game.MousePos, ui.MousePos);

            if (GAMELOOP.Paused) return;
            HandleInputExample(dt, game.MousePos, ui.MousePos);
            UpdateExample(dt, deltaSlow, game, ui);
        }
        public void DrawGame(ScreenInfo game)
        {
            DrawGameExample(game);
        }
        public void DrawGameUI(ScreenInfo ui)
        {
            if (GAMELOOP.Paused)
            {
                ui.Area.Draw(ColorDark.ChangeAlpha((byte)150));
                
                var pausedRect = ui.Area.ApplyMargins(0.05f, 0.05f, 0.15f, 0.55f);
                titleFont.DrawText("PAUSED", pausedRect, 30f, new(0.5f), ColorRustyRed);
                
                return;
                
            }
            
            DrawGameUIExample(ui);
            
            //Vector2 uiSize = ui.Area.Size;
            // Segment s = new(uiSize * new Vector2(0f, 0.07f), uiSize * new Vector2(1f, 0.07f));
            // GAMELOOP.UIZones.TopLeft.CombineWith(GAMELOOP.UIZones.TopCenter).CombineWith(GAMELOOP.UIZones.TopRight).BottomSegment.Draw(2f, ColorLight);
            //GAMELOOP.RectTop.BottomSegment.Draw(2f, ColorLight);
            var topLine = GAMELOOP.UIRects.GetRectSingle("top").BottomSegment;
            topLine.Draw(2f, ColorLight);

            var topCenterRect = GAMELOOP.UIRects.GetRect("top center"); // Get("top").Get("center").GetRect();
            titleFont.DrawText(Title, topCenterRect, 10f, new(0.5f), ColorLight);

            // string backText = "Back [ESC]";
            //Rect backRect = new Rect(uiSize * new Vector2(0.02f, 0.06f), uiSize * new Vector2(0.3f, 0.04f), new Vector2(0f, 1f));
            // titleFont.DrawText(backText, backRect, 4f, new Vector2(0f, 0.5f), ColorHighlight2);
            //var backRect = ui.Area.ApplyMargins(0.012f, 0.85f, 0.012f, 0.95f);
            // var curInputDevice = input.CurrentInputDevice == InputDevice.Mouse
            //     ? InputDevice.Keyboard
            //     : input.CurrentInputDevice;
            // backLabel.Color = ExampleScene.ColorMedium;
            // backLabel.Draw(GAMELOOP.RectTopLeft.SplitH(0.75f).top, new(0f, 0f), curInputDevice, 4);
            
            // string fpsText = $"Fps: {Raylib.GetFPS()}";
            // Rect fpsRect = new Rect(uiSize * new Vector2(0.98f, 0.06f), uiSize * new Vector2(0.3f, 0.04f), new Vector2(1f, 1f));
            // titleFont.DrawText(fpsText, fpsRect, 4f, new Vector2(1f, 0.5f), ColorHighlight2);
            
            //DrawInputDeviceInfo(GAMELOOP.RectBottomLeft);
        }
        public void DrawUI(ScreenInfo ui)
        {
            var backRect = ui.Area.ApplyMargins(0.012f, 0.85f, 0.012f, 0.95f);
            var curInputDevice = Input.CurrentInputDeviceNoMouse;
            var backLabelRect = GAMELOOP.UIRects.GetRect("top left top"); // GetRect("top", "left", "top"); // Get("top").Get("left").Get("top").GetRect();
            backLabel.Draw(backLabelRect, new(0f, 0f), curInputDevice, 4);
            
            if (GAMELOOP.Paused) return;

            var deviceRect = GAMELOOP.UIRects.GetRect("bottom left"); // GetRect("bottom", "left"); // Get("bottom").Get("left").GetRect();
            DrawInputDeviceInfo(deviceRect);
            
            DrawUIExample(ui);
            
        }
        
        protected virtual void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI) { }
        protected virtual void UpdateExample(float dt, float slowDelta, ScreenInfo game, ScreenInfo ui) { }
        protected virtual void DrawGameUIExample(ScreenInfo ui) { }
        protected virtual void DrawUIExample(ScreenInfo ui) { }
        protected virtual void DrawGameExample(ScreenInfo game) { }
        
        public virtual void OnWindowSizeChanged(DimensionConversionFactors conversionFactors)
        {
            // var cancelState = GAMELOOP.Input.ConsumeAction(GameloopExamples.InputUICancelID);
            // if (cancelState is { Consumed: false, Pressed: true })
            // {
            //     GAMELOOP.Quit();
            // }
        }
        public virtual void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos){}
        public virtual void OnMonitorChanged(MonitorInfo newMonitor){}
        public virtual void OnGamepadConnected(Gamepad gamepad){}
        public virtual void OnGamepadDisconnected(Gamepad gamepad){}
        public virtual void OnInputDeviceChanged(InputDevice prevDevice, InputDevice curDevice){}
        public virtual void OnWindowMaximizeChanged(bool maximized){}
        public virtual void OnWindowFullscreenChanged(bool fullscreen){}
        public virtual void OnPausedChanged(bool newPaused){}
        public virtual void OnCursorEnteredScreen(){}
        public virtual void OnCursorHiddenChanged(bool hidden){}
        public virtual void OnCursorLeftScreen(){}
        public virtual void OnCursorLockChanged(bool locked){}
        public virtual void OnWindowFocusChanged(bool focused){}

        protected void DrawCross(Vector2 center, float length)
        {
            Color c = ColorLight.ChangeAlpha((byte)125);
            Segment hor = new Segment(center - new Vector2(length / 2, 0f), center + new Vector2(length / 2, 0f));
            Segment ver = new Segment(center - new Vector2(0f, length / 2), center + new Vector2(0f, length / 2));
            hor.Draw(2f, c);
            ver.Draw(2f, c);
        }
        private void DrawInputDeviceInfo(Rect rect)
        {
            var infoRect = rect;//.ApplyMargins(0.01f, 0.75f, 0.08f, 0.84f);
            var split = infoRect.SplitV(2);
            var deviceRect = split[0];
            var gamepadRect = split[1];

            var deviceText = Input.GetCurInputDeviceGenericName(); // ShapeInput.GetInputDeviceGenericName(Input.CurrentInputDevice);
            // var deviceRect = rect.ApplyMargins(0.01f, 0.7f, 0.1f, 0.85f);
            titleFont.DrawText(deviceText, deviceRect, 1f, new Vector2(0.01f, 0.5f), ColorHighlight3);
            
            string gamepadText = "No Gamepad Connected";
            if (GAMELOOP.CurGamepad != null)
            {
                var gamepadIndex = GAMELOOP.CurGamepad.Index;
                gamepadText = $"Gamepad [{gamepadIndex}] Connected";
            }
            
            // var textRect = rect.ApplyMargins(0f, 0.5f, 0.1f, 0.85f);
            titleFont.DrawText(gamepadText, gamepadRect, 1f, new Vector2(0.01f, 0.5f), GAMELOOP.CurGamepad != null ? ColorHighlight3 : ColorMedium);
        }
        
        public virtual GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }

        // public void SetInput(ShapeInput input)
        // {
        //     //this.input = input;
        // }

        public virtual void Activate(IScene oldScene)
        {
            GAMELOOP.Camera.Reset();
        }
        public virtual void Deactivate()
        {
            GAMELOOP.Camera.Reset();
            GAMELOOP.ResetCamera();
        }
        public virtual void Close()
        {
            
        }

    }

}
