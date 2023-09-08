using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    internal class Pillar
    {
        Rect outline = new();
        Rect center = new();
        public Pillar(Vector2 pos, float size)
        {
            outline = new(pos, new Vector2(size), new Vector2(0.5f));
            center = outline.ScaleSize(0.5f, new Vector2(0.5f));
        }

        public void Draw()
        {
            outline.DrawLines(4f, RED);
            center.Draw(RED);
        }
    }
    public class CameraExample : ExampleScene
    {
        Rect boundaryRect;
        Font font;
        BasicCamera camera;


        List<Pillar> pillars = new();


        public CameraExample()
        {
            Title = "Camera Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            camera = GAMELOOP.GameCam;
            boundaryRect = new(new Vector2(0, -45), new Vector2(1800, 810), new Vector2(0.5f));

            for (int i = 0; i < 150; i++)
            {
                Vector2 pos = SRNG.randVec2(0, 5000);
                float size = SRNG.randF(25, 100);
                Pillar p = new(pos, size);
                pillars.Add(p);
            }
        }
        public override void Activate(IScene oldScene)
        {
            
        }

        public override void Deactivate()
        {

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
                Vector2 movement = new Vector2(dirX, dirY).Normalize() * speed * dt;
                camera.Position += movement;
                //camera.Translation += movement;
            }
        }
        public override void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            base.Update(dt, mousePosScreen, game, ui);
        }

        public override void DrawGame(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.DrawGame(gameSize, mousePosGame);
            foreach (var pillar in pillars)
            {
                pillar.Draw();
            }
        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);

            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            
            var pos = camera.Position;
            var wc = camera.GetCamera();
            
            string infoText = String.Format("[W/A/S/D] Move Camera | Camera Pos: {0} | Offset: {1}", pos , wc.target);
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

    }

}
