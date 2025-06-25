using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Screen;
using System.Numerics;
using Examples.UIElements;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.Input;
using ShapeEngine.StaticLib.Drawing;
using ShapeEngine.Text;


namespace Examples.Scenes
{
    public class ExampleScene : Scene
    {
        public string Title { get; protected set; } = "Title Goes Here";
        public string Description { get; protected set; } = "No Description Yet.";

        protected TextFont titleFont = new(GAMELOOP.FontDefault, 1f, Colors.Highlight);
        protected TextFont textFont = new(GAMELOOP.GetFont(FontIDs.JetBrains), 1f, Colors.Text);

        protected bool drawInputDeviceInfo = true;
        protected bool drawTitle = true;
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
        protected void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
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
            // var pausedState = GAMELOOP.InputActionPause.Consume();
            // if (pausedState is { Consumed: false, Pressed: true })
            // {
            //     GAMELOOP.Paused = !GAMELOOP.Paused;
            // }
            

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

        protected override void OnUpdate(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            HandleInput(time.Delta, game.MousePos, gameUi.MousePos, ui.MousePos);

            if (GAMELOOP.Paused) return;
            OnHandleInputExample(time.Delta, game.MousePos, gameUi.MousePos, ui.MousePos);
            OnUpdateExample(time, game, gameUi, ui);
        }
        protected override void OnDrawGame(ScreenInfo game)
        {
            OnDrawGameExample(game);
        }
        protected override void OnDrawGameUI(ScreenInfo gameUi)
        {
            if (GAMELOOP.Paused)
            {
                // ui.Area.Draw(ColorDark.ChangeAlpha((byte)150));
                gameUi.Area.Draw(Colors.Dark);
                var pausedRect = gameUi.Area.ApplyMargins(0.05f, 0.05f, 0.15f, 0.55f);
                titleFont.LineSpacing = 30f;
                titleFont.ColorRgba = Colors.Special;
                titleFont.DrawTextWrapNone("PAUSED", pausedRect, new(0.5f));
                // titleFont.DrawText("PAUSED", pausedRect, 30f, new(0.5f), ColorRustyRed);
                
                return;
                
            }
            
            OnDrawGameUIExample(gameUi);

            
            
        }
        protected override void OnDrawUI(ScreenInfo ui)
        {
            // var backRect = ui.Area.ApplyMargins(0.012f, 0.85f, 0.012f, 0.95f);
            var curInputDevice = ShapeInput.CurrentInputDeviceTypeNoMouse;

            var rectNode = GAMELOOP.UIRects.GetChild("top left");
            if (rectNode != null)
            {
                var backLabelRect = rectNode.Rect;
                if (rectNode.MouseInside)
                {
                    backLabelRect.DrawLines(2f, Colors.Medium);
                    
                    var acceptState = GAMELOOP.InputActionUIAccept.Consume();
                    var acceptMouseState = GAMELOOP.InputActionUIAcceptMouse.Consume();
                    
                    // if (GAMELOOP.InputActionUIAccept.State.Pressed || GAMELOOP.InputActionUIAcceptMouse.State.Pressed)// ShapeInput.MouseDevice.GetButtonState(ShapeMouseButton.LEFT).Pressed)
                    if (acceptState is {Consumed:false, Pressed:true} || acceptMouseState is {Consumed:false, Pressed:true})// ShapeInput.MouseDevice.GetButtonState(ShapeMouseButton.LEFT).Pressed)
                    {
                        if (IsCancelAllowed())
                        {
                            if(GAMELOOP.Paused) GAMELOOP.Paused = false;
                            GAMELOOP.GoToMainScene();
                        }
                    }
                }
                // var backLabelRect = GAMELOOP.UIRects.GetRect("top left");
                backLabel.Draw(backLabelRect, new(0f, 0f), curInputDevice);
            }
            
            if (GAMELOOP.Paused) return;
            
            if (drawTitle)
            {
                var topLine = GAMELOOP.UIRects.GetRect("top").BottomSegment;
                topLine.Draw(2f, Colors.Light);

                var topCenterRect = GAMELOOP.UIRects.GetRect("top center"); // Get("top").Get("center").GetRect();
                titleFont.LineSpacing = 10f;
                titleFont.ColorRgba = Colors.Highlight;
                titleFont.DrawTextWrapNone(Title, topCenterRect, new(0.5f));
            }

            var deviceRect = GAMELOOP.UIRects.GetRect("bottom left"); // GetRect("bottom", "left"); // Get("bottom").Get("left").GetRect();
            DrawInputDeviceInfo(deviceRect);
            
            OnDrawUIExample(ui);
        }
        
        protected virtual void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI) { }
        protected virtual void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui) { }
        protected virtual void OnDrawGameUIExample(ScreenInfo gameUi) { }
        protected virtual void OnDrawUIExample(ScreenInfo ui) { }
        protected virtual void OnDrawGameExample(ScreenInfo game) { }
        
        
        private void DrawInputDeviceInfo(Rect rect)
        {
            if (!drawInputDeviceInfo) return;
            var infoRect = rect;
            var split = infoRect.SplitV(2);
            var deviceRect = split[0];
            var gamepadRect = split[1];

            var deviceText = ShapeInput.GetCurInputDeviceGenericName();
            titleFont.LineSpacing = 1f;
            titleFont.ColorRgba = Colors.Medium;
            titleFont.DrawTextWrapNone(deviceText, deviceRect, new AnchorPoint(0.01f, 0.5f));
            // titleFont.DrawText(deviceText, deviceRect, 1f, new Vector2(0.01f, 0.5f), ColorHighlight3);
            
            string gamepadText = "No Gamepad Connected";
            if (GAMELOOP.CurGamepad != null)
            {
                var gamepadIndex = GAMELOOP.CurGamepad.Index;
                gamepadText = $"Gamepad [{gamepadIndex}] Connected";
            }
            
            titleFont.LineSpacing = 1f;
            titleFont.ColorRgba = GAMELOOP.CurGamepad != null ? Colors.Highlight : Colors.Medium;
            titleFont.DrawTextWrapNone(gamepadText, gamepadRect, new AnchorPoint(0.01f, 0.5f));
            // titleFont.DrawText(gamepadText, gamepadRect, 1f, new Vector2(0.01f, 0.5f), GAMELOOP.CurGamepad != null ? ColorHighlight3 : ColorMedium);
        }

        protected override void OnActivate(Scene oldScene)
        {
            GAMELOOP.Camera.Reset();
        }

        protected override void OnDeactivate()
        {
            GAMELOOP.Camera.Reset();
            GAMELOOP.ResetCamera();
        }



        public static Vector2 CalculateMouseMovementDirection(Vector2 mousePos, ShapeCamera camera)
        {
            var dir = mousePos - camera.BasePosition;

            float minDis = 100 * camera.ZoomFactor;
            float maxDis = 350 * camera.ZoomFactor;
            float minDisSq = minDis * minDis;
            float maxDisSq = maxDis * maxDis;
            var lsq = dir.LengthSquared();
            if (lsq <= minDisSq) dir = new();
            else if (lsq >= maxDisSq) dir = dir.Normalize();
            else
            {
                var f = (lsq - minDisSq) / (maxDisSq - minDisSq);
                dir = dir.Normalize() * f;
            }

            return dir;
        }

    }

}
