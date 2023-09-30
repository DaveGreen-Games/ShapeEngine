using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using System.Net.NetworkInformation;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.UI;
using ShapeEngine.Core.Shapes;

namespace Examples.Scenes.ExampleScenes
{
    public class CameraAreaDrawExample : ExampleScene
    {
        Font font;
        Vector2 movementDir = new();
        Rect universe = new(new Vector2(0f), new Vector2(10000f), new Vector2(0.5f));
        List<Star> stars = new();
        private List<Star> drawStars = new();
        
        private Ship ship = new(new Vector2(0f), 30f);

        private ShapeCamera camera = new ShapeCamera();
        public CameraAreaDrawExample()
        {
            Title = "Camera Area Draw Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
                
            GenerateStars(SRNG.randI(15000, 30000));
            camera.Follower.BoundaryDis = 45f;
            camera.Follower.FollowSmoothness = 15f;
        }

        private void GenerateStars(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Vector2 pos = universe.GetRandomPointInside();

                //ChanceList<float> sizes = new((45, 1.5f), (25, 2f), (15, 2.5f), (10, 3f), (3, 3.5f), (2, 4f));
                float size = SRNG.randF(1.5f, 3f);// sizes.Next();
                Star star = new(pos, size);
                stars.Add(star);
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
            GenerateStars(SRNG.randI(15000, 30000));

        }
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            HandleZoom(dt);

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
        public override void Update(float dt, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(dt, game, ui);

            ship.Update(dt, camera.RotationDeg);

            drawStars.Clear();
            Rect cameraArea = game.Area;
            foreach (var star in stars)
            {
                if(cameraArea.OverlapShape(star.GetBoundingBox())) drawStars.Add(star);
            }
        }

        public override void DrawGame(ScreenInfo game)
        {
            base.DrawGame(game);
            foreach (var star in drawStars)
            {
                star.Draw(new Color(20, 150, 150, 200));
            }
            ship.Draw();
        }
        public override void DrawUI(ScreenInfo ui)
        {
            base.DrawUI(ui);
            Vector2 uiSize = ui.Area.Size;
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            string infoText = $"Zoom [Y X] | Move [W A S D] | Total Stars {stars.Count} | Drawn Stars {drawStars.Count} | Camera Size {camera.Area.Size.Round()}";
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            // var pos = camera.Position;
            // int x = (int)pos.X;
            // int y = (int)pos.Y;
            // int rot = (int)camera.RotationDeg;
            // int zoom = (int)(SUtils.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
            // string moveText = $"[W/A/S/D] Move ({x}/{y})";
            // string rotText = $"[Q/E] Rotate ({rot})";
            // string scaleText = $"[Y/X] Zoom ({zoom}%)";
            // //string transText = String.Format("[LMB] Offset ({0}/{1})", transX, transY);
            // string shakeText = "[Space] Shake Camera";
            // string infoText = $"{moveText} | {rotText} | {scaleText} | {shakeText}";
            // font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            
        }

    }

}
