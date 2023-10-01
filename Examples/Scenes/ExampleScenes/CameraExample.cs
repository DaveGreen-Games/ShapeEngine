using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;

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
        Font font;
        ShapeCamera camera;
        Rect universe = new(new Vector2(0f), new Vector2(10000f), new Vector2(0.5f));

        List<Pillar> pillars = new();


        public CameraExample()
        {
            Title = "Camera Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            camera = GAMELOOP.Camera;
            //boundaryRect = new(new Vector2(0, -45), new Vector2(1800, 810), new Vector2(0.5f));

            for (int i = 0; i < 250; i++)
            {
                //Vector2 pos = SRNG.randVec2(0, 5000);
                Vector2 pos = universe.GetRandomPointInside();
                float size = ShapeRandom.randF(25, 100);
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
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }
        public override void Reset()
        {
            camera.Reset();
        }
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);


            HandleCameraPosition(dt);
            HandleCameraZoom(dt);
            HandleCameraRotation(dt);
            HandleCameraTranslation(dt, mousePosGame);
        }
        private void HandleCameraZoom(float dt)
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
        private void HandleCameraTranslation(float dt, Vector2 mousePos)
        {
            if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                camera.Position = mousePos;
            }
        }
        private void HandleCameraRotation(float dt)
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
        private void HandleCameraPosition(float dt)
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
        
        public override void DrawGame(ScreenInfo game)
        {
            base.DrawGame(game);
            foreach (var pillar in pillars)
            {
                pillar.Draw();
            }

            float f = camera.ZoomFactor;
            DrawCircleV(camera.Position, 8f * f, BLUE);
            ShapeDrawing.DrawCircleLines(camera.Position, 64 * f, 2f * f, BLUE);
            Segment hor = new(camera.Position - new Vector2(3000 * f, 0), camera.Position + new Vector2(3000 * f, 0));
            hor.Draw(2f * f, BLUE);
            Segment ver = new(camera.Position - new Vector2(0, 3000 * f), camera.Position + new Vector2(0, 3000 * f));
            ver.Draw(2f * f, BLUE);
        }
        public override void DrawUI(ScreenInfo ui)
        {
            base.DrawUI(ui);
            Vector2 uiSize = ui.Area.Size;
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            
            var pos = camera.Position;
            int x = (int)pos.X;
            int y = (int)pos.Y;
            int rot = (int)camera.BaseRotationDeg;
            int zoom = (int)(ShapeUtils.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
            int transX = (int)camera.Position.X;
            int transY = (int)camera.Position.Y;
            string moveText = $"[W/A/S/D] Move ({x}/{y})";
            string rotText = $"[Q/E] Rotate ({rot})";
            string scaleText = $"[Y/X] Zoom ({zoom}%)";
            string transText = $"[LMB] Offset ({transX}/{transY})";
            string infoText = $"{moveText} | {rotText} | {scaleText} | {transText}";
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

    }

}
