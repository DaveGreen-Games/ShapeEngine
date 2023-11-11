using System.IO.Pipes;
using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace Examples.Scenes.ExampleScenes
{
    internal class Star
    {
        Circle circle;
        public Star(Vector2 pos, float size)
        {
            circle = new(pos, size);
        }

        public Rect GetBoundingBox() => circle.GetBoundingBox();

        public void Draw()
        {
            Color color = DARKGRAY;
            if (circle.Radius > 2f && circle.Radius <= 3f) color = GRAY;
            else if (circle.Radius > 3f) color = WHITE;
            ShapeDrawing.DrawCircleFast(circle.Center, circle.Radius, color);
        }
        public void Draw(Color c) => ShapeDrawing.DrawCircleFast(circle.Center, circle.Radius, c);
    }
    internal class Comet
    {
        private const float MaxSize = 20f;
        private const float MinSize = 10f;
        private const float MaxSpeed = 150f;
        private const float MinSpeed = 10f;
        private static ChanceList<Color> colors = new((50, ORANGE), (30, YELLOW), (10, RED), (5, PURPLE), (1, LIME));
        private static ChanceList<float> speeds = new((10, MinSpeed), (30, MinSpeed * 2.5f), (50, MinSpeed * 4f), (20, MaxSpeed / 2), (10, MaxSpeed));
        Circle circle;
        Vector2 vel;
        Color color;
        float speed = 0f;
        public Comet(Vector2 pos)
        {
            this.circle = new(pos, ShapeRandom.randF(MinSize, MaxSize));
            this.speed = speeds.Next();
            this.vel = ShapeRandom.randVec2() * this.speed;
            this.color = colors.Next();

        }
        public void Update(float dt, Rect universe)
        {
            circle.Center += vel * dt;

            if (!universe.ContainsPoint(circle.Center))
            {
                circle.Center = -circle.Center;
            }
        }
        public bool CheckCollision(Circle ship)
        {
            return circle.OverlapShape(ship);
        }
        public float GetCollisionIntensity()
        {
            float speedF = ShapeUtils.GetFactor(speed, MinSpeed, MaxSpeed);
            float sizeF = ShapeUtils.GetFactor(circle.Radius, MinSize, MaxSize);
            return speedF * sizeF;
        }
        public void Draw()
        {
            circle.DrawLines(6f, color);
        }
    }

    
    internal class Slider
    {
        public float CurValue { get; private set; } = 0f;
        public string Title { get; set; } = "";
        private Rect background = new();
        private Rect fill = new();
        private Font font;
        private bool mouseInside = false;
        public Slider(float startValue, string title, Font font)
        {
            this.Title = title;
            this.CurValue = ShapeMath.Clamp(startValue, 0f, 1f);
            this.font = font;
        }
        
        public void Update(float dt, Rect r, Vector2 mousePos)
        {
            background = r; // ui.Area.ApplyMargins(0.025f, 0.6f, 0.1f, 0.85f);
            mouseInside = background.ContainsPoint(mousePos);
            if (mouseInside)
            {
                if (Input.GetActionState(GAMELOOP.InputActionUIAccept).Down)
                {
                    float intensity = background.GetWidthPointFactor(mousePos.X);
                    CurValue = intensity;
                    fill = background.SetSize(background.Size * new Vector2(intensity, 1f));
                }
                else fill = background.SetSize(background.Size * new Vector2(CurValue, 1f));
            }
            else fill = background.SetSize(background.Size * new Vector2(CurValue, 1f));
        }
        public void Draw()
        {
            background.DrawRounded(4f, 4, ExampleScene.ColorDarkB);
            fill.DrawRounded(4f, 4, ExampleScene.ColorMedium);
            int textValue = (int)(CurValue * 100);
            font.DrawText($"{Title} {textValue}", background, 1f, new Vector2(0.1f, 0.5f), mouseInside ? ExampleScene.ColorHighlight2 : ExampleScene.ColorHighlight3);
        }
    }
    internal class Ship : ICameraFollowTarget
    {
        public Circle Hull { get; private set; }
        private Vector2 movementDir;
        public float Speed = 500;

        private Color hullColor = BLUE;
        private Color outlineColor = RED;
        private Color cockpitColor = SKYBLUE;

        public InputAction iaMoveHor;
        public InputAction iaMoveVer;

        private void SetupInput()
        {
            var moveHorKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            // var moveHorGP =
                // new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT);//reverse modifier
            var moveHor2GP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, true);
            iaMoveHor = new(moveHorKB, moveHor2GP);
            
            var moveVerKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            // var moveVerGP =
                // new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN);//reverse modifier
            var moveVer2GP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.1f, ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, true);
            iaMoveVer = new(moveVerKB, moveVer2GP);
        }
        public Ship(Vector2 pos, float r)
        {
            Hull = new(pos, r);
            SetupInput();
        }
        public Ship(Vector2 pos, float r, Color hullColor, Color cockpitColor, Color outlineColor)
        {
            Hull = new(pos, r);
            this.hullColor = hullColor;
            this.cockpitColor = cockpitColor;
            this.outlineColor = outlineColor;
            SetupInput();
        }

        public string GetInputDescription(InputDevice inputDevice)
        {
            string hor = iaMoveHor.GetInputTypeDescription(inputDevice, true, 1, false, false);
            string ver = iaMoveVer.GetInputTypeDescription(inputDevice, true, 1, false, false);
            return $"Move Horizontal [{hor}] Vertical [{ver}]";
        }
        public void Reset(Vector2 pos, float r)
        {
            Hull = new(pos, r);
        } 
        
        public void Update(float dt, float cameraRotationDeg)
        {
            // int dirX = 0;
            // int dirY = 0;
            //
            //
            //
            // if (IsKeyDown(KeyboardKey.KEY_A))
            // {
            //     dirX = -1;
            // }
            // else if (IsKeyDown(KeyboardKey.KEY_D))
            // {
            //     dirX = 1;
            // }
            //
            // if (IsKeyDown(KeyboardKey.KEY_W))
            // {
            //     dirY = -1;
            // }
            // else if (IsKeyDown(KeyboardKey.KEY_S))
            // {
            //     dirY = 1;
            // }
            int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            
            iaMoveHor.Gamepad = gamepadIndex;
            iaMoveHor.Update(dt);
            
            iaMoveVer.Gamepad = gamepadIndex;
            iaMoveVer.Update(dt);
            
            Vector2 dir = new(iaMoveHor.State.AxisRaw, iaMoveVer.State.AxisRaw);
            float lsq = dir.LengthSquared();
            if (lsq > 0f)
            {
                movementDir = dir.Normalize();
                movementDir = movementDir.RotateDeg(-cameraRotationDeg);
                var movement = movementDir * Speed * dt;
                Hull = new Circle(Hull.Center + movement, Hull.Radius);
            }
            
        }
        public void Draw()
        {
            var rightThruster = movementDir.RotateDeg(-25);
            var leftThruster = movementDir.RotateDeg(25);
            DrawCircleV(Hull.Center - rightThruster * Hull.Radius, Hull.Radius / 6, outlineColor);
            DrawCircleV(Hull.Center - leftThruster * Hull.Radius, Hull.Radius / 6, outlineColor);
            Hull.Draw(hullColor);
            DrawCircleV(Hull.Center + movementDir * Hull.Radius * 0.66f, Hull.Radius * 0.33f, cockpitColor);

            Hull.DrawLines(4f, outlineColor);
        }
        
        public void FollowStarted()
        {
            
        }
        public void FollowEnded()
        {
            
        }
        public Vector2 GetCameraFollowPosition()
        {
            return Hull.Center;
        }
    }
    public class ScreenEffectsExample : ExampleScene
    {
        Font font;
        //Vector2 movementDir = new();
        Rect universe = new(new Vector2(0f), new Vector2(10000f), new Vector2(0.5f));
        List<Star> stars = new();
        List<Comet> comets = new();

        private Ship ship = new(new Vector2(0f), 30f);

        private Rect slider = new();
        private Rect sliderFill = new();

        private Slider intensitySlider;
        private Slider cameraFollowSlider;
        
        private ShapeCamera camera = new ShapeCamera();

        private InputAction iaShakeCamera;
        private InputAction iaRotateCamera;
        
        public ScreenEffectsExample()
        {
            Title = "Screen Effects Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
                
            GenerateStars(2500);
            GenerateComets(200);

            camera.Follower.BoundaryDis = new(100, 300);
            intensitySlider = new(1f, "Intensity", font);
            cameraFollowSlider = new(1f, "Camera Follow", font);
            SetSliderValues();

            var shakeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.F);
            var shakeGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            var shakeMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaShakeCamera = new(shakeKB, shakeGP, shakeMB);

            var rotateKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.Q, ShapeKeyboardButton.E);
            var rotateGB = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, false);
            var rotateMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL);
            iaRotateCamera = new(rotateKB, rotateGB, rotateMW);
            
            
        }

        private void SetSliderValues()
        {
            float intensity = intensitySlider.CurValue;
            GAMELOOP.ScreenEffectIntensity = intensity;
            camera.Intensity = intensity;
            camera.Follower.FollowSpeed = ShapeMath.LerpFloat(0.5f, 2f, cameraFollowSlider.CurValue) * ship.Speed;
        }
        private void GenerateStars(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                //Vector2 pos = SRNG.randVec2(0, 5000);
                Vector2 pos = universe.GetRandomPointInside();

                ChanceList<float> sizes = new((45, 1.5f), (25, 2f), (15, 2.5f), (10, 3f), (3, 3.5f), (2, 4f));
                float size = sizes.Next();
                Star star = new(pos, size);
                stars.Add(star);
            }
        }
        private void GenerateComets(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Vector2 pos = universe.GetRandomPointInside();
                Comet comet = new(pos);
                comets.Add(comet);
            }
        }
        public override void Activate(IScene oldScene)
        {
            GAMELOOP.Camera = camera;
            camera.Follower.SetTarget(ship);
            // GAMELOOP.UseMouseMovement = false;
        }

        public override void Deactivate()
        {
            GAMELOOP.ResetCamera();
            // GAMELOOP.UseMouseMovement = true;
        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }
        public override void Reset()
        {
            GAMELOOP.ScreenEffectIntensity = 1f;
            camera.Reset();
            ship.Reset(new Vector2(0), 30f);
            camera.Follower.SetTarget(ship);
            stars.Clear();
            comets.Clear();
            GenerateStars(2500);
            GenerateComets(200);

        }
        
        // private void HandleZoom(float dt)
        // {
        //     float zoomSpeed = 1f;
        //     int zoomDir = 0;
        //     if (IsKeyDown(KeyboardKey.KEY_Z)) zoomDir = -1;
        //     else if (IsKeyDown(KeyboardKey.KEY_X)) zoomDir = 1;
        //
        //     if (zoomDir != 0)
        //     {
        //         camera.Zoom(zoomDir * zoomSpeed * dt);
        //     }
        // }
        private void HandleRotation(float dt)
        {
            float rotDir = iaRotateCamera.State.AxisRaw;

            if (rotDir != 0)
            {
                camera.Rotate(rotDir * 90f * dt);
            }
        }
        private void ShakeCamera()
        {
            camera.Shake(ShapeRandom.randF(0.8f, 2f), new Vector2(100, 100), 0, 25, 0.75f);
        }
        
        
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            // HandleZoom(dt);
            HandleRotation(dt);

            if (iaShakeCamera.State.Pressed) ShakeCamera();

        }
        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            iaShakeCamera.Gamepad = gamepadIndex;
            iaShakeCamera.Update(dt);
            
            iaRotateCamera.Gamepad = gamepadIndex;
            iaRotateCamera.Update(dt);
            
            Rect slider = GAMELOOP.UIRects.GetRect("center").ApplyMargins(0.025f, 0.025f, 0.02f, 0.93f);
            Rect sliderLeft = slider.ApplyMargins(0f, 0.55f, 0f, 0f);
            Rect sliderRight = slider.ApplyMargins(0.55f, 0f, 0f, 0f);
            intensitySlider.Update(dt, sliderLeft, ui.MousePos);
            cameraFollowSlider.Update(dt, sliderRight, ui.MousePos);
            SetSliderValues();
            
            ship.Update(dt, camera.RotationDeg);
            
            for (int i = comets.Count - 1; i >= 0; i--)
            {
                Comet comet = comets[i];
                comet.Update(dt, universe);

                if (comet.CheckCollision(ship.Hull))
                {
                    float f = comet.GetCollisionIntensity();
                    camera.Shake(ShapeRandom.randF(0.4f, 0.6f), new Vector2(150, 150) * f, 0, 10, 0.75f);
                    comets.RemoveAt(i);

                    GAMELOOP.Flash(0.25f, new(255, 255, 255, 150), new(0,0,0,0));
                }
            }
        }
        protected override void DrawGameExample(ScreenInfo game)
        {
            foreach (var star in stars)
            {
                star.Draw();
            }

            foreach (var comet in comets)
            {
                comet.Draw();
            }
            
            ship.Draw();
            
            
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            intensitySlider.Draw();
            cameraFollowSlider.Draw();
            
        }
        protected override void DrawUIExample(ScreenInfo ui)
        {
            DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
            DrawCameraInfo(GAMELOOP.UIRects.GetRect("bottom right"));
            // Vector2 uiSize = ui.Area.Size;
            // Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            //
            // var pos = camera.Position;
            // int x = (int)pos.X;
            // int y = (int)pos.Y;
            // int rot = (int)camera.RotationDeg;
            // int zoom = (int)(ShapeUtils.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
            // string moveText = $"[W/A/S/D] Move ({x}/{y})";
            // string rotText = $"[Q/E] Rotate ({rot})";
            // string scaleText = $"[Y/X] Zoom ({zoom}%)";
            // //string transText = String.Format("[LMB] Offset ({0}/{1})", transX, transY);
            // string shakeText = "[Space] Shake Camera";
            // string infoText = $"{moveText} | {rotText} | {scaleText} | {shakeText}";
            // font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);

        }

        private void DrawCameraInfo(Rect rect)
        {
            var pos = camera.Position;
            var x = (int)pos.X;
            var y = (int)pos.Y;
            var rot = (int)camera.RotationDeg;
            var zoom = (int)(ShapeUtils.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
            
            string text = $"Pos {x}/{y} | Rot {rot} | Zoom {zoom}";
            font.DrawText(text, rect, 1f, new Vector2(0.5f, 0.5f), ColorHighlight3);
        }
        private void DrawInputDescription(Rect rect)
        {
            var curDevice = Input.CurrentInputDevice;
            string shakeCameraText = iaShakeCamera.GetInputTypeDescription(curDevice, true, 1, false);
            string rotateCameraText = iaRotateCamera.GetInputTypeDescription(curDevice, true, 1, false);
            string moveText = ship.GetInputDescription(Input.CurrentInputDeviceNoMouse);
            string text = $"{moveText} | Shake {shakeCameraText} | Rotate {rotateCameraText}";
            font.DrawText(text, rect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }

}
