using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Text;
using Color = System.Drawing.Color;

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
            var color = new ColorRgba(System.Drawing.Color.DarkGray);
            if (circle.Radius > 2f && circle.Radius <= 3f) color = new(System.Drawing.Color.LightGray);
            else if (circle.Radius > 3f) color = new(System.Drawing.Color.AntiqueWhite);
            ShapeDrawing.DrawCircleFast(circle.Center, circle.Radius, color);
        }
        public void Draw(ColorRgba c) => ShapeDrawing.DrawCircleFast(circle.Center, circle.Radius, c);
    }
    internal class Comet
    {
        private const float MaxSize = 20f;
        private const float MinSize = 10f;
        private const float MaxSpeed = 150f;
        private const float MinSpeed = 10f;
        private static ChanceList<ColorRgba> colors = new((50, new(Color.Orange)), (30, new(Color.Goldenrod)), (10, new(Color.IndianRed)), (5, new(Color.MediumPurple)), (1, new(Color.ForestGreen)));
        private static ChanceList<float> speeds = new((10, MinSpeed), (30, MinSpeed * 2.5f), (50, MinSpeed * 4f), (20, MaxSpeed / 2), (10, MaxSpeed));
        Circle circle;
        Vector2 vel;
        ColorRgba colorRgba;
        float speed = 0f;
        public Comet(Vector2 pos)
        {
            this.circle = new(pos, ShapeRandom.RandF(MinSize, MaxSize));
            this.speed = speeds.Next();
            this.vel = ShapeRandom.RandVec2() * this.speed;
            this.colorRgba = colors.Next();

        }
        public void Update(float dt, Rect universe)
        {
            circle += vel * dt; // circle.Center += vel * dt;

            if (!universe.ContainsPoint(circle.Center))
            {
                circle -= circle.Center; // circle.Center = -circle.Center;
            }
        }
        public bool CheckCollision(Circle ship)
        {
            return circle.OverlapShape(ship);
        }
        public float GetCollisionIntensity()
        {
            float speedF = ShapeMath.GetFactor(speed, MinSpeed, MaxSpeed);
            float sizeF = ShapeMath.GetFactor(circle.Radius, MinSize, MaxSize);
            return speedF * sizeF;
        }
        public void Draw()
        {
            circle.DrawLines(6f, colorRgba);
        }
    }

    
    internal class Slider
    {
        public float CurValue { get; private set; } = 0f;
        public string Title { get; set; } = "";
        private Rect background = new();
        private Rect fill = new();
        private TextFont font;
        private bool mouseInside = false;
        public Slider(float startValue, string title, Font font)
        {
            this.Title = title;
            this.CurValue = ShapeMath.Clamp(startValue, 0f, 1f);
            this.font = new(font, 1f, ColorRgba.White);
        }

        public void SetValue(float newValue)
        {
            CurValue = ShapeMath.Clamp(newValue, 0f, 1f);
        }
        public void Update(float dt, Rect r, Vector2 mousePos)
        {
            background = r; // ui.Area.ApplyMargins(0.025f, 0.6f, 0.1f, 0.85f);
            mouseInside = background.ContainsPoint(mousePos);
            if (mouseInside)
            {
                if (GAMELOOP.InputActionUIAccept.State.Down || GAMELOOP.InputActionUIAcceptMouse.State.Down)
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
            background.DrawRounded(4f, 4, Colors.Dark);
            fill.DrawRounded(4f, 4, Colors.Medium);
            
            int textValue = (int)(CurValue * 100);
            font.ColorRgba = mouseInside ? Colors.Highlight: Colors.Special;
            font.DrawTextWrapNone($"{Title} {textValue}", background, new Vector2(0.1f, 0.5f));
            // font.DrawText(, background, 1f, new Vector2(0.1f, 0.5f), mouseInside ? ExampleScene.ColorHighlight2 : ExampleScene.ColorHighlight3);
        }
    }
    internal class Ship : ICameraFollowTarget
    {
        public Circle Hull { get; private set; }
        private Vector2 movementDir;
        public float Speed = 500;

        private PaletteColor hullColor = Colors.PcCold; // new(System.Drawing.Color.SteelBlue);
        private PaletteColor outlineColor = Colors.PcLight; //new(System.Drawing.Color.IndianRed);
        private PaletteColor cockpitColor = Colors.PcWarm; // new(System.Drawing.Color.DodgerBlue);

        private InputAction iaMoveHor;
        private InputAction iaMoveVer;
        
        
        private void SetupInput()
        {
            var moveHorKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            // var moveHorGP =
                // new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT);//reverse modifier
            var moveHor2GP = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            var moveHorMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveHor = new(moveHorKB, moveHor2GP, moveHorMW);
            
            var moveVerKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            // var moveVerGP =
                // new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN);//reverse modifier
            var moveVer2GP = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            var moveVerMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveVer = new(moveVerKB, moveVer2GP, moveVerMW);
        }
        public Ship(Vector2 pos, float r)
        {
            Hull = new(pos, r);
            SetupInput();
        }
        public Ship(Vector2 pos, float r, PaletteColor hullColor, PaletteColor cockpitColor, PaletteColor outlineColor)
        {
            Hull = new(pos, r);
            this.hullColor = hullColor;
            this.cockpitColor = cockpitColor;
            this.outlineColor = outlineColor;
            SetupInput();
        }

        public string GetInputDescription(InputDeviceType inputDeviceType)
        {
            string hor = iaMoveHor.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
            string ver = iaMoveVer.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
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
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            
            iaMoveHor.Gamepad = GAMELOOP.CurGamepad;
            iaMoveHor.Update(dt);
            
            iaMoveVer.Gamepad = GAMELOOP.CurGamepad;
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
            ShapeDrawing.DrawCircle(Hull.Center - rightThruster * Hull.Radius, Hull.Radius / 6, outlineColor.ColorRgba, 12);
            ShapeDrawing.DrawCircle(Hull.Center - leftThruster * Hull.Radius, Hull.Radius / 6, outlineColor.ColorRgba, 12);
            Hull.Draw(hullColor.ColorRgba);
            ShapeDrawing.DrawCircle(Hull.Center + movementDir * Hull.Radius * 0.66f, Hull.Radius * 0.33f, cockpitColor.ColorRgba, 12);

            Hull.DrawLines(4f, outlineColor.ColorRgba);
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
        private readonly Font font;
        private readonly Rect universe = new(new Vector2(0f), new Size(10000f), new Vector2(0.5f));
        private readonly List<Star> stars = new();
        private readonly List<Comet> comets = new();

        private readonly Ship ship = new(new Vector2(0f), 30f);

        private Rect slider = new();
        private Rect sliderFill = new();

        private readonly Slider intensitySlider;
        private readonly Slider cameraFollowSlider;

        private readonly ShapeCamera camera;

        private readonly InputAction iaShakeCamera;
        private readonly InputAction iaRotateCamera;
        private readonly InputAction iaToggleDrawCameraFollowBoundary;
        private bool drawCameraFollowBoundary = false;

        private readonly CameraFollowerSingle follower;
        
        public ScreenEffectsExample()
        {
            Title = "Screen Effects Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
                
            GenerateStars(2500);
            GenerateComets(200);

            camera = new();
            intensitySlider = new(0.5f, "Intensity", font);
            cameraFollowSlider = new(0.5f, "Camera Follow", font);
            follower = new(0, 100, 500);
            camera.Follower = follower;
            
            UpdateFollower(GAMELOOP.UIScreenInfo.Area.Size.Min());
            SetSliderValues();

            var shakeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.G);
            var shakeGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            var shakeMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaShakeCamera = new(shakeKB, shakeGP, shakeMB);


            var rotateKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.Q, ShapeKeyboardButton.E);
            // var rotateGB = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad);
            var rotateGB = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad);
            var rotateMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouse);
            iaRotateCamera = new(rotateKB, rotateGB, rotateMW);

            var toggleDrawKB = new InputTypeKeyboardButton(ShapeKeyboardButton.T);
            var toggleDrawGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            iaToggleDrawCameraFollowBoundary = new(toggleDrawKB, toggleDrawGP);
        }

        private void SetSliderValues()
        {
            float intensity = intensitySlider.CurValue;
            GAMELOOP.ScreenEffectIntensity = intensity;
            camera.Intensity = intensity;
        }
        private void GenerateStars(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
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
                var pos = universe.GetRandomPointInside();
                Comet comet = new(pos);
                comets.Add(comet);
            }
        }
        public override void Activate(Scene oldScene)
        {
            GAMELOOP.Camera = camera;
            UpdateFollower(GAMELOOP.UIScreenInfo.Area.Size.Min());
            
            follower.SetTarget(ship);

            GAMELOOP.MouseControlEnabled = false;
            // follower.Activate();
        }

        public override void Deactivate()
        {
            GAMELOOP.ResetCamera();
            GAMELOOP.MouseControlEnabled = true;
            // follower.Deactivate();
        }
        public override void Reset()
        {
            intensitySlider.SetValue(0.5f);
            cameraFollowSlider.SetValue(0.5f);
            SetSliderValues();
            GAMELOOP.ScreenEffectIntensity = 1f;
            camera.Reset();
            
            ship.Reset(new Vector2(0), 30f);
            
            follower.Reset();
            follower.SetTarget(ship);
            
            UpdateFollower(GAMELOOP.UIScreenInfo.Area.Size.Min());
            
            stars.Clear();
            comets.Clear();
            GenerateStars(2500);
            GenerateComets(200);
            drawCameraFollowBoundary = false;
        }
        
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
            camera.Shake(ShapeRandom.RandF(0.8f, 2f), new Vector2(100, 100), 0, 25, 0.75f);
        }
        
        
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            var gamepad = GAMELOOP.CurGamepad;

            GAMELOOP.MouseControlEnabled = gamepad?.IsDown(ShapeGamepadAxis.RIGHT_TRIGGER, 0.1f) ?? true;
            
            
            iaShakeCamera.Gamepad = gamepad;
            iaShakeCamera.Update(dt);
            
            iaRotateCamera.Gamepad = gamepad;
            iaRotateCamera.Update(dt);

            iaToggleDrawCameraFollowBoundary.Gamepad = gamepad;
            iaToggleDrawCameraFollowBoundary.Update(dt);

            if (iaToggleDrawCameraFollowBoundary.State.Pressed)
            {
                drawCameraFollowBoundary = !drawCameraFollowBoundary;
            }
            
            HandleRotation(dt);

            if (iaShakeCamera.State.Pressed) ShakeCamera();

        }

        private void UpdateFollower(float size)
        {
            var sliderF = cameraFollowSlider.CurValue;
            var minBoundary = 0.12f * size; // ShapeMath.LerpFloat(0.2f, 0.1f, sliderF) * size;
            var maxBoundary = ShapeMath.LerpFloat(0.55f, 0.15f, sliderF) * size;
            var boundary = new Vector2(minBoundary, maxBoundary) * camera.ZoomFactor;
            follower.Speed = ship.Speed;// * Lerp(0.5f, 2f, sliderF);
            follower.BoundaryDis = new(boundary);
        }
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            UpdateFollower(ui.Area.Size.Min());
            //follower.Update(dt,camera);
            
            var slider = GAMELOOP.UIRects.GetRect("center").ApplyMargins(0.025f, 0.025f, 0.02f, 0.93f);
            var sliderLeft = slider.ApplyMargins(0f, 0.55f, 0f, 0f);
            var sliderRight = slider.ApplyMargins(0.55f, 0f, 0f, 0f);
            intensitySlider.Update(time.Delta, sliderLeft, ui.MousePos);
            cameraFollowSlider.Update(time.Delta, sliderRight, ui.MousePos);
            SetSliderValues();
            
            ship.Update(time.Delta, camera.RotationDeg);
            
            for (int i = comets.Count - 1; i >= 0; i--)
            {
                Comet comet = comets[i];
                comet.Update(time.Delta, universe);

                if (comet.CheckCollision(ship.Hull))
                {
                    float f = comet.GetCollisionIntensity();
                    camera.Shake(ShapeRandom.RandF(0.4f, 0.6f), new Vector2(150, 150) * f, 0, 10, 0.75f);
                    comets.RemoveAt(i);

                    GAMELOOP.Flash(0.25f, new(255, 255, 255, 150), new(0,0,0,0));
                }
            }
        }
        protected override void OnDrawGameExample(ScreenInfo game)
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

            if (drawCameraFollowBoundary)
            {
                float thickness = 2f * camera.ZoomFactor;
                var boundarySize = follower.BoundaryDis.ToVector2();
                var boundaryCenter = camera.BasePosition;

                if (boundarySize.X > 0)
                {
                    var innerBoundary = new Circle(boundaryCenter, boundarySize.X);
                    var innerColor = Colors.Highlight.ChangeAlpha((byte)150);
                    innerBoundary.DrawLines(thickness, innerColor);
                    
                }

                if (boundarySize.Y > 0)
                {
                    var outerBoundary = new Circle(boundaryCenter, boundarySize.Y);
                    var outerColor = Colors.Special.ChangeAlpha((byte)150);
                    outerBoundary.DrawLines(thickness, outerColor);
                }
            }
        }
        protected override void OnDrawGameUIExample(ScreenInfo ui)
        {
            intensitySlider.Draw();
            cameraFollowSlider.Draw();
            
        }
        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
            DrawCameraInfo(GAMELOOP.UIRects.GetRect("bottom right"));

        }

        private void DrawCameraInfo(Rect rect)
        {
            var pos = camera.BasePosition;
            var x = (int)pos.X;
            var y = (int)pos.Y;
            var rot = (int)camera.RotationDeg;
            var zoom = (int)(ShapeMath.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
            
            string text = $"Pos {x}/{y} | Rot {rot} | Zoom {zoom}";
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone(text, rect, new(0.5f));
            
            // font.DrawText(text, rect, 1f, new Vector2(0.5f, 0.5f), ColorHighlight3);
        }
        private void DrawInputDescription(Rect rect)
        {
            var rects = rect.SplitV(0.35f);
            var curDevice = ShapeInput.CurrentInputDeviceType;
            string shakeCameraText = iaShakeCamera.GetInputTypeDescription(curDevice, true, 1, false);
            string rotateCameraText = iaRotateCamera.GetInputTypeDescription(curDevice, true, 1, false);
            string toggleDrawText = iaToggleDrawCameraFollowBoundary.GetInputTypeDescription(ShapeInput.CurrentInputDeviceTypeNoMouse, true, 1, false);
            string moveText = ship.GetInputDescription(curDevice);
            string onText = drawCameraFollowBoundary ? "ON" : "OFF";
            string textTop = $"Draw Camera Follow Boundary {onText} - Toggle {toggleDrawText}";
            string textBottom = $"{moveText} | Shake {shakeCameraText} | Rotate {rotateCameraText}";
            
            textFont.FontSpacing = 1f;
            
            textFont.ColorRgba = Colors.Medium;
            textFont.DrawTextWrapNone(textTop, rects.top, new(0.5f));
            
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(textBottom, rects.bottom, new(0.5f));
            
            // font.DrawText(textTop, rects.top, 1f, new Vector2(0.5f, 0.5f), ColorMedium);
            // font.DrawText(textBottom, rects.bottom, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }

}
