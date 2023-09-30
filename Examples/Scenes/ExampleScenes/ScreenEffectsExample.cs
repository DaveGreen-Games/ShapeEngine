using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;

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
            SDrawing.DrawCircleFast(circle.Center, circle.Radius, color);
        }
        public void Draw(Color c) => SDrawing.DrawCircleFast(circle.Center, circle.Radius, c);
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
            this.circle = new(pos, SRNG.randF(MinSize, MaxSize));
            this.speed = speeds.Next();
            this.vel = SRNG.randVec2() * this.speed;
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
            float speedF = SUtils.GetFactor(speed, MinSpeed, MaxSpeed);
            float sizeF = SUtils.GetFactor(circle.Radius, MinSize, MaxSize);
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

        public Slider(float startValue, string title, Font font)
        {
            this.Title = title;
            this.CurValue = SUtils.Clamp(startValue, 0f, 1f);
            this.font = font;
        }
        
        public void Update(float dt, Rect r, Vector2 mousePos)
        {
            background = r; // ui.Area.ApplyMargins(0.025f, 0.6f, 0.1f, 0.85f);
            if (background.ContainsPoint(mousePos))
            {
                if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
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
            font.DrawText($"{Title} {textValue}", background, 1f, new Vector2(0.1f, 0.5f), ExampleScene.ColorHighlight3);
        }
    }
    internal class Ship : ICameraFollowTarget
    {
        public Circle Hull { get; private set; }
        private Vector2 movementDir;
        private float speed = 500;
        
        public Ship(Vector2 pos, float r)
        {
            Hull = new(pos, r);
        }

        public void Reset(Vector2 pos, float r)
        {
            Hull = new(pos, r);
        } 
        
        public void Update(float dt, float cameraRotationDeg)
        {
            int dirX = 0;
            int dirY = 0;

            if (IsKeyDown(KeyboardKey.KEY_A))
            {
                dirX = -1;
            }
            else if (IsKeyDown(KeyboardKey.KEY_D))
            {
                dirX = 1;
            }

            if (IsKeyDown(KeyboardKey.KEY_W))
            {
                dirY = -1;
            }
            else if (IsKeyDown(KeyboardKey.KEY_S))
            {
                dirY = 1;
            }
            
            if (dirX != 0 || dirY != 0)
            {
                movementDir = new Vector2(dirX, dirY).Normalize();
                movementDir = movementDir.RotateDeg(-cameraRotationDeg);
                Vector2 movement = movementDir * speed * dt;
                Hull = new Circle(Hull.Center + movement, Hull.Radius);
            }
            
        }
        public void Draw()
        {
            Vector2 rightThruster = movementDir.RotateDeg(-25);
            Vector2 leftThruster = movementDir.RotateDeg(25);
            DrawCircleV(Hull.Center - rightThruster * Hull.Radius, Hull.Radius / 6, RED);
            DrawCircleV(Hull.Center - leftThruster * Hull.Radius, Hull.Radius / 6, RED);
            Hull.Draw(BLUE);
            DrawCircleV(Hull.Center + movementDir * Hull.Radius * 0.66f, Hull.Radius * 0.33f, SKYBLUE);

            Hull.DrawLines(4f, RED);
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
        Vector2 movementDir = new();
        Rect universe = new(new Vector2(0f), new Vector2(10000f), new Vector2(0.5f));
        List<Star> stars = new();
        List<Comet> comets = new();

        private Ship ship = new(new Vector2(0f), 30f);

        private Rect slider = new();
        private Rect sliderFill = new();

        private Slider intensitySlider;
        private Slider cameraFollowSlider;
        
        private ShapeCamera camera = new ShapeCamera();
        public ScreenEffectsExample()
        {
            Title = "Screen Effects Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
                
            GenerateStars(2500);
            GenerateComets(200);

            camera.Follower.BoundaryDis = 45f;

            intensitySlider = new(1f, "Intensity", font);
            cameraFollowSlider = new(1f, "Camera Follow", font);
            SetSliderValues();
        }

        private void SetSliderValues()
        {
            float intensity = intensitySlider.CurValue;
            GAMELOOP.ScreenEffectIntensity = intensity;
            camera.Intensity = intensity;
            camera.Follower.FollowSmoothness = SUtils.LerpFloat(0.5f, 4f, cameraFollowSlider.CurValue);
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
        }

        public override void Deactivate()
        {
            GAMELOOP.ResetCamera();
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
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            HandleZoom(dt);
            HandleRotation(dt);

            if (IsKeyPressed(KeyboardKey.KEY_SPACE)) ShakeCamera();

        }
        private void HandleZoom(float dt)
        {
            float zoomSpeed = 1f;
            int zoomDir = 0;
            if (IsKeyDown(KeyboardKey.KEY_Z)) zoomDir = -1;
            else if (IsKeyDown(KeyboardKey.KEY_X)) zoomDir = 1;

            if (zoomDir != 0)
            {
                camera.Zoom(zoomDir * zoomSpeed * dt);
            }
        }
        private void HandleRotation(float dt)
        {
            float rotSpeedDeg = 90f;
            int rotDir = 0;
            if (IsKeyDown(KeyboardKey.KEY_Q)) rotDir = -1;
            else if (IsKeyDown(KeyboardKey.KEY_E)) rotDir = 1;

            if (rotDir != 0)
            {
                camera.Rotate(rotDir * rotSpeedDeg * dt);
            }
        }
        // private void HandleCameraTranslation(float dt)
        // {
        //     float speed = 500;
        //     int dirX = 0;
        //     int dirY = 0;
        //
        //     if (IsKeyDown(KeyboardKey.KEY_A))
        //     {
        //         dirX = -1;
        //     }
        //     else if (IsKeyDown(KeyboardKey.KEY_D))
        //     {
        //         dirX = 1;
        //     }
        //
        //     if (IsKeyDown(KeyboardKey.KEY_W))
        //     {
        //         dirY = -1;
        //     }
        //     else if (IsKeyDown(KeyboardKey.KEY_S))
        //     {
        //         dirY = 1;
        //     }
        //     if (dirX != 0 || dirY != 0)
        //     {
        //         movementDir = new Vector2(dirX, dirY).Normalize();
        //         movementDir = movementDir.RotateDeg(-camera.RotationDeg);
        //         Vector2 movement = movementDir * speed * dt;
        //         camera.Position += movement;
        //         //camera.Translation += movement;
        //     }
        // }
        private void ShakeCamera()
        {
            camera.Shake(SRNG.randF(0.8f, 2f), new Vector2(100, 100), 0, 25, 0.75f);
        }
        public override void Update(float dt, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(dt, game, ui);

            intensitySlider.Update(dt, ui.Area.ApplyMargins(0.025f, 0.6f, 0.1f, 0.85f), ui.MousePos);
            cameraFollowSlider.Update(dt, ui.Area.ApplyMargins(0.025f, 0.6f, 0.16f, 0.79f), ui.MousePos);
            SetSliderValues();
            
            ship.Update(dt, camera.RotationDeg);
            
            for (int i = comets.Count - 1; i >= 0; i--)
            {
                Comet comet = comets[i];
                comet.Update(dt, universe);

                if (comet.CheckCollision(ship.Hull))
                {
                    float f = comet.GetCollisionIntensity();
                    camera.Shake(SRNG.randF(0.4f, 0.6f), new Vector2(150, 150) * f, 0, 10, 0.75f);
                    comets.RemoveAt(i);

                    GAMELOOP.Flash(0.25f, new(255, 255, 255, 150), new(0,0,0,0));
                }
            }
        }

        public override void DrawGame(ScreenInfo game)
        {
            base.DrawGame(game);
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
        public override void DrawUI(ScreenInfo ui)
        {
            base.DrawUI(ui);
            Vector2 uiSize = ui.Area.Size;
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));

            var pos = camera.Position;
            int x = (int)pos.X;
            int y = (int)pos.Y;
            int rot = (int)camera.RotationDeg;
            int zoom = (int)(SUtils.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
            string moveText = $"[W/A/S/D] Move ({x}/{y})";
            string rotText = $"[Q/E] Rotate ({rot})";
            string scaleText = $"[Y/X] Zoom ({zoom}%)";
            //string transText = String.Format("[LMB] Offset ({0}/{1})", transX, transY);
            string shakeText = "[Space] Shake Camera";
            string infoText = $"{moveText} | {rotText} | {scaleText} | {shakeText}";
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            
            intensitySlider.Draw();
            cameraFollowSlider.Draw();
        }

    }

}
