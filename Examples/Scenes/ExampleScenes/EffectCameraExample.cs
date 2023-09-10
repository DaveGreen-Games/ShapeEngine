using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    internal class Star
    {
        Circle circle;
        public Star(Vector2 pos, float size)
        {
            circle = new(pos, size);
        }

        public void Draw()
        {
            Color color = DARKGRAY;
            if (circle.radius > 2f && circle.radius <= 3f) color = GRAY;
            else if (circle.radius > 3f) color = WHITE;
            SDrawing.DrawCircleFast(circle.center, circle.radius, color);
        }
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
            circle.center += vel * dt;

            if (!universe.IsPointInside(circle.center))
            {
                circle.center = -circle.center;
            }
        }
        public bool CheckCollision(Circle ship)
        {
            return circle.OverlapShape(ship);
        }
        public float GetCollisionIntensity()
        {
            float speedF = SUtils.GetFactor(speed, MinSpeed, MaxSpeed);
            float sizeF = SUtils.GetFactor(circle.radius, MinSize, MaxSize);
            return speedF * sizeF;
        }
        public void Draw()
        {
            circle.DrawLines(6f, color);
        }
    }
    public class EffectCameraExample : ExampleScene
    {
        //Rect boundaryRect;
        Font font;
        EffectCamera camera;
        Vector2 movementDir = new();
        Rect universe = new(new Vector2(0f), new Vector2(10000f), new Vector2(0.5f));
        List<Star> stars = new();
        List<Comet> comets = new();
        Circle ship = new(new Vector2(0f), 30f);

        public EffectCameraExample()
        {
            Title = "Effect Camera Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            camera = new EffectCamera(new(0f), GAMELOOP.Game.GetSize(), new(0.5f), 1f, 0f);
            
            //boundaryRect = new(new Vector2(0, -45), new Vector2(1800, 810), new Vector2(0.5f));

            
            for (int i = 0; i < 2500; i++)
            {
                //Vector2 pos = SRNG.randVec2(0, 5000);
                Vector2 pos = universe.GetRandomPoint();

                ChanceList<float> sizes = new((45, 1.5f), (25, 2f), (15, 2.5f), (10, 3f), (3, 3.5f), (2, 4f));
                float size = sizes.Next();
                Star star = new(pos, size);
                stars.Add(star);
            }
            for (int i = 0; i < 100; i++)
            {
                Vector2 pos = universe.GetRandomPoint();
                Comet comet = new(pos);
                comets.Add(comet);
            }
        }
        public override void Activate(IScene oldScene)
        {
            GAMELOOP.Game.SetCamera(camera);
        }

        public override void Deactivate()
        {
            GAMELOOP.Game.SetCamera(GAMELOOP.GameCam);
        }
        public override Area? GetCurArea()
        {
            return null;
        }
        public override void Reset()
        {
            camera.Position = new(0f);
            camera.ResetRotation();
            camera.ResetTranslation();
            camera.ResetZoom();
        }
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);


            HandleCameraTranslation(dt);
            HandleZoom(dt);
            HandleRotation(dt);

        }
        private void HandleZoom(float dt)
        {
            float zoomSpeed = 1f;
            int zoomDir = 0;
            if (IsKeyDown(KeyboardKey.KEY_Z)) zoomDir = -1;
            else if (IsKeyDown(KeyboardKey.KEY_X)) zoomDir = 1;

            if (zoomDir != 0)
            {
                camera.Zoom += zoomDir * zoomSpeed * dt;
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
                camera.RotationDeg += rotDir * rotSpeedDeg * dt;
            }
        }
        private void HandleCameraTranslation(float dt)
        {
            float speed = 500;
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
                movementDir = movementDir.RotateDeg(-camera.RotationDeg);
                Vector2 movement = movementDir * speed * dt;
                camera.Position += movement;
                //camera.Translation += movement;
            }
        }
        public override void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            base.Update(dt, mousePosScreen, game, ui);

            for (int i = comets.Count - 1; i >= 0; i--)
            {
                Comet comet = comets[i];
                comet.Update(dt, universe);

                if (comet.CheckCollision(ship))
                {
                    float f = comet.GetCollisionIntensity();
                    camera.Shake(SRNG.randF(0.4f, 0.6f), new Vector2(150, 150) * f, 0, 10, 0.75f);
                    comets.RemoveAt(i);
                }
            }
            ship.center = camera.Position;
        }

        public override void DrawGame(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.DrawGame(gameSize, mousePosGame);
            foreach (var star in stars)
            {
                star.Draw();
            }

            foreach (var comet in comets)
            {
                comet.Draw();
            }

            Vector2 rightThruster = movementDir.RotateDeg(-25);
            Vector2 leftThruster = movementDir.RotateDeg(25);
            DrawCircleV(ship.center - rightThruster * ship.radius, ship.radius / 6, RED);
            DrawCircleV(ship.center - leftThruster * ship.radius, ship.radius / 6, RED);
            ship.Draw(BLUE);
            DrawCircleV(ship.center + movementDir * ship.radius * 0.66f, ship.radius * 0.33f, SKYBLUE);

            ship.DrawLines(4f, RED);
        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);

            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));

            var pos = camera.Position;
            var wc = camera.GetCamera();

            string infoText = String.Format("[W/A/S/D] Move Camera | Camera Pos: {0} | Offset: {1}", pos, wc.target);
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

    }

}
