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

namespace Examples.Scenes.ExampleScenes
{
    public class ShipInputExample : ExampleScene
    {
        Font font;
        Rect universe = new(new Vector2(0f), new Vector2(10000f), new Vector2(0.5f));
        List<Star> stars = new();
        List<Comet> comets = new();

        private Ship ship = new(new Vector2(0f), 30f);

        private Rect slider = new();
        private Rect sliderFill = new();

        private Slider intensitySlider;
        private Slider cameraFollowSlider;
        
        private ShapeCamera camera = new ShapeCamera();
        public ShipInputExample()
        {
            Title = "Ship Input Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
                
            GenerateStars(2500);
            GenerateComets(200);

            camera.Follower.BoundaryDis = new(100, 300);
            intensitySlider = new(1f, "Intensity", font);
            cameraFollowSlider = new(1f, "Camera Follow", font);
            SetSliderValues();
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
        private void ShakeCamera()
        {
            camera.Shake(ShapeRandom.randF(0.8f, 2f), new Vector2(100, 100), 0, 25, 0.75f);
        }
        
        
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            HandleZoom(dt);
            HandleRotation(dt);

            if (IsKeyPressed(KeyboardKey.KEY_SPACE)) ShakeCamera();

        }
        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
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
            Vector2 uiSize = ui.Area.Size;
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));

            var pos = camera.Position;
            int x = (int)pos.X;
            int y = (int)pos.Y;
            int rot = (int)camera.RotationDeg;
            int zoom = (int)(ShapeUtils.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
            string moveText = $"[W/A/S/D] Move ({x}/{y})";
            string rotText = $"[Q/E] Rotate ({rot})";
            string scaleText = $"[Y/X] Zoom ({zoom}%)";
            //string transText = String.Format("[LMB] Offset ({0}/{1})", transX, transY);
            string shakeText = "[Space] Shake Camera";
            string infoText = $"{moveText} | {rotText} | {scaleText} | {shakeText}";
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);

        }
    }

}
