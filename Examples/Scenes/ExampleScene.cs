using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using Examples.UIElements;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Text;


namespace Examples.Scenes
{
    public class ExampleScene : Scene
    {
        public string Title { get; protected set; } = "Title Goes Here";
        public string Description { get; protected set; } = "No Description Yet.";

        protected TextFont titleFont = new(GAMELOOP.FontDefault, 1f, Colors.Highlight);
        protected TextFont textFont = new(GAMELOOP.GetFont(FontIDs.JetBrains), 1f, Colors.Text);

        //protected readonly ShapeInput input = GAMELOOP.Input;
        private InputActionLabel backLabel;

        public ExampleScene()
        {
            var action = GAMELOOP.InputActionUICancel;
            backLabel = new(action, "BACK", GAMELOOP.FontDefault, Colors.PcWarm, 4f);
        }
        public virtual void Reset() { }

        
        private void HandleZoom(float dt)
        {
            float zoomSpeed = 1f;
            float zoomDir = 0;

            var zoomState = GAMELOOP.InputActionZoom.Consume();
            if (!zoomState.Consumed)
            {
                zoomDir = -zoomState.AxisRaw;
            }
            
            if (zoomDir != 0)
            {
                GAMELOOP.Camera.Zoom(zoomDir * zoomSpeed * dt);
            }
        }

        protected virtual bool IsCancelAllowed() => true;
        protected void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var cancelState = GAMELOOP.InputActionUICancel.Consume();
            if (cancelState is { Consumed: false, Pressed: true })
            {
                if (IsCancelAllowed())
                {
                    if(GAMELOOP.Paused) GAMELOOP.Paused = false;
                    GAMELOOP.GoToMainScene();
                }
                
            }

            var pausedState = GAMELOOP.InputActionPause.Consume();
            if (pausedState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.Paused = !GAMELOOP.Paused;
            }

            if (GAMELOOP.Paused) return;


            var resetState = GAMELOOP.InputActionReset.Consume();
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

        protected override void OnUpdateGame(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            HandleInput(time.Delta, game.MousePos, ui.MousePos);

            if (GAMELOOP.Paused) return;
            OnHandleInputExample(time.Delta, game.MousePos, ui.MousePos);
            OnUpdateExample(time, game, ui);
        }
        protected override void OnDrawGame(ScreenInfo game)
        {
            OnDrawGameExample(game);
        }
        protected override void OnDrawGameUI(ScreenInfo ui)
        {
            if (GAMELOOP.Paused)
            {
                // ui.Area.Draw(ColorDark.ChangeAlpha((byte)150));
                ui.Area.Draw(Colors.Dark);
                var pausedRect = ui.Area.ApplyMargins(0.05f, 0.05f, 0.15f, 0.55f);
                titleFont.LineSpacing = 30f;
                titleFont.ColorRgba = Colors.Special;
                titleFont.DrawTextWrapNone("PAUSED", pausedRect, new(0.5f));
                // titleFont.DrawText("PAUSED", pausedRect, 30f, new(0.5f), ColorRustyRed);
                
                return;
                
            }
            
            OnDrawGameUIExample(ui);
            var topLine = GAMELOOP.UIRects.GetRectSingle("top").BottomSegment;
            topLine.Draw(2f, Colors.Light);

            var topCenterRect = GAMELOOP.UIRects.GetRect("top center"); // Get("top").Get("center").GetRect();
            titleFont.LineSpacing = 10f;
            titleFont.ColorRgba = Colors.Highlight;
            titleFont.DrawTextWrapNone(Title, topCenterRect, new(0.5f));
        }
        protected override void OnDrawUI(ScreenInfo ui)
        {
            // var backRect = ui.Area.ApplyMargins(0.012f, 0.85f, 0.012f, 0.95f);
            var curInputDevice = ShapeInput.CurrentInputDeviceTypeNoMouse;
            var backLabelRect = GAMELOOP.UIRects.GetRect("top left top"); // GetRect("top", "left", "top"); // Get("top").Get("left").Get("top").GetRect();
            backLabel.Draw(backLabelRect, new(0f, 0f), curInputDevice);
            
            if (GAMELOOP.Paused) return;

            var deviceRect = GAMELOOP.UIRects.GetRect("bottom left"); // GetRect("bottom", "left"); // Get("bottom").Get("left").GetRect();
            DrawInputDeviceInfo(deviceRect);
            
            OnDrawUIExample(ui);
        }
        
        protected virtual void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI) { }
        protected virtual void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo ui) { }
        protected virtual void OnDrawGameUIExample(ScreenInfo ui) { }
        protected virtual void OnDrawUIExample(ScreenInfo ui) { }
        protected virtual void OnDrawGameExample(ScreenInfo game) { }
        
        
        private void DrawInputDeviceInfo(Rect rect)
        {
            var infoRect = rect;
            var split = infoRect.SplitV(2);
            var deviceRect = split[0];
            var gamepadRect = split[1];

            var deviceText = ShapeInput.GetCurInputDeviceGenericName();
            titleFont.LineSpacing = 1f;
            titleFont.ColorRgba = Colors.Medium;
            titleFont.DrawTextWrapNone(deviceText, deviceRect, new Vector2(0.01f, 0.5f));
            // titleFont.DrawText(deviceText, deviceRect, 1f, new Vector2(0.01f, 0.5f), ColorHighlight3);
            
            string gamepadText = "No Gamepad Connected";
            if (GAMELOOP.CurGamepad != null)
            {
                var gamepadIndex = GAMELOOP.CurGamepad.Index;
                gamepadText = $"Gamepad [{gamepadIndex}] Connected";
            }
            
            titleFont.LineSpacing = 1f;
            titleFont.ColorRgba = GAMELOOP.CurGamepad != null ? Colors.Highlight : Colors.Medium;
            titleFont.DrawTextWrapNone(gamepadText, gamepadRect, new Vector2(0.01f, 0.5f));
            // titleFont.DrawText(gamepadText, gamepadRect, 1f, new Vector2(0.01f, 0.5f), GAMELOOP.CurGamepad != null ? ColorHighlight3 : ColorMedium);
        }
        
        public override void Activate(Scene oldScene)
        {
            GAMELOOP.Camera.Reset();
        }
        public override void Deactivate()
        {
            GAMELOOP.Camera.Reset();
            GAMELOOP.ResetCamera();
        }

    }

}
