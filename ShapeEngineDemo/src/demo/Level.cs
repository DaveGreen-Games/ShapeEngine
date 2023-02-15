using System.Numerics;
using Raylib_CsLo;
using ShapeCore;
using ShapeLib;
using ShapeScreen;
using ShapeUI;
using ShapeInput;
using ShapeTiming;
using ShapeCursor;
using ShapeEngineDemo.Bodies;
using ShapeColor;
using ShapeAchievements;

namespace ShapeEngineDemo
{
    internal class Star : GameObject
    {
        private Vector2 pos;
        private float r = 1f;
        private Color color;


        public Star(Rectangle spawnArea, float radius, Color color)
        {
            this.pos = SRNG.randVec2(spawnArea);
            this.r = radius;
            this.color = color;
            DrawOrder = SRNG.randF(-1f, 1f);
        }

        public override void Draw()
        {
            DrawCircleV(pos + AreaLayerOffset, r, color);
        }
        public override Rectangle GetBoundingBox()
        {
            return new(pos.X + AreaLayerOffset.X - r, pos.Y + AreaLayerOffset.Y - r, r * 2, r * 2);
        }

        public override Vector2 GetPosition() { return pos; }
        public override bool IsDead() { return false; }
    }

    internal class Planet : GameObject
    {
        private Vector2 pos;
        private float r = 1f;
        private Color color;
        List<(Vector2 center, float r, Color color)> circles = new();
        List<(Vector2 center, float r, float thickness, Color color)> rings = new();
        public Planet(Rectangle spawnArea, float radius, Color color)
        {
            this.pos = SRNG.randVec2(spawnArea);
            this.r = radius;
            this.color = color;
            DrawOrder = SRNG.randF(-1f, 1f);

            if (r > 6)
            {
                int randAmount = SRNG.randI(0, 4);
                for (int i = 0; i < randAmount; i++)
                {
                    var randR = SRNG.randF(1f, this.r / 2f);
                    var randPos = SRNG.randVec2(0, this.r - randR);
                    var randColor = SColor.ChangeHUE(color, SRNG.randI(-50, 50));
                    randColor = SColor.ChangeBrightness(randColor, SRNG.randF(-0.2f, -0.1f));
                    circles.Add((randPos, randR, randColor));
                }
            }

            if(SRNG.randF() < 0.1f)
            {
                int randAmount = SRNG.randI(1, 2);
                for (int i = 0; i < randAmount; i++)
                {
                    var randR = SRNG.randF(r * 1.2f, r * 2.5f);
                    var randThickness = SRNG.randF(1f, (randR - this.r) / 2);
                    var randColor = SColor.ChangeHUE(color, SRNG.randI(-50, 50)); 
                    rings.Add((new Vector2(0f), randR, randThickness, SColor.ChangeAlpha(randColor, (byte) SRNG.randI(75, 150))));
                }
            }
        }

        public override void Draw()
        {
            //Drawing.DrawCircleLines(pos, r, 1.0f, color, 2);
            DrawCircleV(pos + AreaLayerOffset, r, color);

            foreach (var circle in circles)
            {
                DrawCircleV(pos + AreaLayerOffset + circle.center, circle.r, circle.color);
            }

            foreach (var ring in rings)
            {
                SDrawing.DrawCircleLines(pos + AreaLayerOffset + ring.center, ring.r, ring.thickness, ring.color, 4f);
            }
        }
        public override Rectangle GetBoundingBox()
        {
            return new(pos.X + AreaLayerOffset.X - r, pos.Y + AreaLayerOffset.Y - r, r * 2, r * 2);
        }

        public override Vector2 GetPosition() { return pos; }
        public override bool IsDead() { return false; }
    }

    
    public class AreaBasic : Area
    {
        public AreaBasic(Rectangle area, int rows, int cols) : base(area, rows, cols)
        {
            AddLayer("stars very far", -100, 0.2f);
            AddLayer("stars far", -95, 0.15f);
            AddLayer("stars near", -90, 0.1f);
            AddLayer("planets very far", -85, 0.05f);
            AddLayer("planets far", -85, 0.03f);
            AddLayer("planets near", -80, 0.01f);
            AddLayer("asteroids", -5, 0);
            
            SpawnStars(90, new(0.7f, 0.8f), new(0.4f, 0.5f), "stars very far");
            SpawnStars(90, new(0.8f, 1.0f), new(0.5f, 0.6f), "stars far");
            SpawnStars(90, new(1.0f, 1.2f), new(0.6f, 0.7f), "stars near");
            SpawnPlanets(5, new(3, 4.5f), "planets very far", -0.9f);
            SpawnPlanets(3, new(5, 6.5f), "planets far", -0.7f);
            SpawnPlanets(2, new(7, 8.5f), "planets near", -0.5f);
            //this.playfield = new PlayfieldBasic(area, -10);
        }
        //
        public override void Draw()
        {
            DrawRectangleLinesEx(inner, 3f, Demo.PALETTES.C("neutral"));
            base.Draw();
            //DrawRectangleLinesEx(new Rectangle(0.0f, 0.0f, ScreenHandler.GameWidth(), ScreenHandler.GameHeight()), 6, ColorPalette.Cur.neutral);

        }

        private void SpawnPlanet(float radius, string layer, float brightness = 0f)
        {
            Color color = SRNG.randColor(150, 220, 255);
            var planet = new Planet(inner, radius, SColor.ChangeBrightness(color, brightness));
            AddGameObject(planet, false, layer);

        }
        private void SpawnPlanets(int amount, Vector2 radiusRange, string layer, float brightness = 0f)
        {
            for (int i = 0; i < amount; i++)
            {
                SpawnPlanet(SRNG.randF(radiusRange.X, radiusRange.Y), layer, brightness);
            }
        }
        private void SpawnStar(float radius, Vector2 alphaRange, string layer)
        {
            Color color = WHITE;
            color.a = (byte)(255 * SRNG.randF(alphaRange.X, alphaRange.Y));
            var star = new Star(outer, radius, color);
            AddGameObject(star, false, layer);
        }
        private void SpawnStars(int amount, Vector2 radiusRange, Vector2 alphaRange, string layer)
        {
            for (int i = 0; i < amount; i++)
            {
                SpawnStar(SRNG.randF(radiusRange.X, radiusRange.Y), alphaRange, layer);
            }
        }
    }
    
    public class Level : Scene
    {
        private Area area;
        private Player player;
        private AsteroidSpawner asteroidSpawner;
        private Rectangle playArea;

        public Level()
        {
            Rectangle playArea = new Rectangle(0, 0, 1200, 1200);// Utils.ScaleRectangle(ScreenHandler.GameArea(), 1.5f);
            this.area = new AreaBasic(playArea, 10, 10);
            this.asteroidSpawner = new(this.area, 1f, 2f);
            this.playArea = playArea;

            

            ArmoryInfo armoryInfo = new("minigun", "bouncer", "basic");
            player = new(armoryInfo, "starter");
            ScreenHandler.CAMERA.SetTarget(player);
            //ScreenHandler.Cam.ZoomFactor = 0.35f;
        }

        public override void Activate(Scene? oldScene)
        {
            ScreenHandler.GAME.Flash(0.25f, new(0, 0, 0, 255), new(0, 0, 0, 0));
            //Action action = () => ScreenHandler.Cam.Shake(0.25f, new(75.0f, 75.0f), 1, 0, 0.75f);
            //TimerHandler.Add(0.25f, action);
            //AudioHandler.PlaySFX("explosion");
            Demo.CURSOR.Switch("game");
            GAMELOOP.backgroundColor = Demo.PALETTES.C("bg1");
            //AudioHandler.SwitchPlaylist("game");
        }
        public override void Deactivate(Scene? newScene)
        {
            if (newScene == null) return;
            ScreenHandler.GAME.Flash(0.25f, new(0, 0, 0, 255), new(0, 0, 0, 255));
            Action action = () => GAMELOOP.SwitchScene(this, newScene);
            ScreenHandler.CAMERA.ResetZoom();
            Demo.TIMER.Add(0.25f, action);

        }
        public override Area? GetCurArea()
        {
            return area;
        }
        public override void Start()
        {
            if (area == null) return;
            area.AddGameObject(player, true);
            area.Start();
            asteroidSpawner.Start();

            //Vector2 pos = new(RNG.randF(0, ScreenHandler.GameWidth()), RNG.randF(0, ScreenHandler.GameHeight()));
            //float r = RNG.randF(100, 200);
            //Attractor attractor = new(pos, r, -500, 0);
            //area.AddGameObject(attractor);
        }
        public override void HandleInput()
        {
            if (InputHandler.IsReleased(0, "Pause")) TogglePause();
            
            if (InputHandler.IsDown(0, "Slow Time")) GAMELOOP.Slow(0.25f, 1.0f);
            else if (InputHandler.IsReleased(0, "Slow Time")) GAMELOOP.EndSlow();
            
            if (InputHandler.IsReleased(0, "UI Cancel")) GAMELOOP.GoToScene("mainmenu");
            
            if (!IsPaused() && InputHandler.IsReleased(0, "Spawn Asteroid")) SpawnAsteroidDebug();

            if (EDITORMODE)
            {
                if (InputHandler.IsReleased(0, "Toggle Draw Helpers")) DEBUG_DRAWHELPERS = !DEBUG_DRAWHELPERS;
                if (InputHandler.IsReleased(0, "Toggle Draw Colliders")) DEBUG_DRAWCOLLIDERS = !DEBUG_DRAWCOLLIDERS;
                if (InputHandler.IsReleased(0, "Cycle Zoom"))
                {
                    ScreenHandler.CAMERA.ZoomBy(0.25f);
                    if (ScreenHandler.CAMERA.ZoomFactor > 2) ScreenHandler.CAMERA.ZoomFactor = 0.25f;
                }

                //if (Raylib.IsKeyReleased(KeyboardKey.KEY_P)) TogglePause();
            }
        }
        public override void Update(float dt)
        {
            //if (IsPaused()) return;
            if (area == null) return;
            ScreenHandler.UpdateCamera(dt);

            if(player != null && !player.IsDead()) area.UpdateLayerParallaxe(player.GetPos());
            area.Update(dt);
            asteroidSpawner.Update(dt);



            
            //testRotationDeg += dt * 90f;
            //if (testRotationDeg > 360) testRotationDeg = 0f;
        }
        public override void Draw()
        {
            if (area == null) return;
            //Drawing.DrawGrid(Utils.ScaleRectangle(playArea, 2), 15, 2f, new(255, 255, 255, 120));
            area.Draw();
            asteroidSpawner.Draw();
        }
        public override void DrawUI(Vector2 uiSize, Vector2 stretchFactor)
        {
            SDrawing.DrawTextAlignedPro(String.Format("{0}", GetFPS()), uiSize * new Vector2(0.01f, 0.03f), -5f, uiSize * new Vector2(0.10f, 0.05f), 2f, Demo.PALETTES.C("special1"), Demo.FONT.GetFont(), new(0,0.5f));
            if (area == null) return;
            area.DrawUI(uiSize, stretchFactor);

            Vector2 textSize = uiSize * new Vector2(0.25f, 0.04f);
            SDrawing.DrawTextAlignedPro(String.Format("Objs {0}", area.GetGameObjects().Count), uiSize * new Vector2(0.01f, 0.1f), 0f, textSize, 2f, Demo.PALETTES.C("text"), Demo.FONT.GetFont(), new(0, 0.5f));
            SDrawing.DrawTextAlignedPro(String.Format("{0}", InputHandler.GetCurInputType()), uiSize * new Vector2(0.01f, 0.13f), 0f, textSize, 2f, Demo.PALETTES.C("text"), Demo.FONT.GetFont(), new(0, 0.5f));
            SDrawing.DrawTextAlignedPro(String.Format("GP {0}/{1}", InputHandler.CUR_GAMEPAD, InputHandler.GetConnectedGamepadCount()), uiSize * new Vector2(0.01f, 0.16f), 0f, textSize, 2f, Demo.PALETTES.C("text"), Demo.FONT.GetFont(), new(0, 0.5f));
            SDrawing.DrawTextAlignedPro(String.Format("Used {0}", InputHandler.gamepadUsed), uiSize * new Vector2(0.01f, 0.19f), 0f, textSize, 2f, Demo.PALETTES.C("text"), Demo.FONT.GetFont(), new(0, 0.5f));
            
            SDrawing.DrawTextAlignedPro(String.Format("Kills {0}", Demo.ACHIEVEMENTS.GetStatValue("asteroidKills")), uiSize * new Vector2(0.01f, 0.25f), 0f, textSize, 2f, Demo.PALETTES.C("text"), Demo.FONT.GetFont(), new(0, 0.5f));
            
            
            SDrawing.DrawTextAlignedPro("Debug Keys [8, 9, 0]", uiSize * new Vector2(0.5f, 0.98f), 0f, textSize, 2f, Demo.PALETTES.C("text"), Demo.FONT.GetFont(), new(0.5f, 1));
            

            SDrawing.DrawTextAlignedPro("Slow Time [ALT]", uiSize * new Vector2(0.99f, 0.03f), 0f, textSize, 2f, Demo.PALETTES.C("text"), Demo.FONT.GetFont(), new(1, 0.5f));
            SDrawing.DrawTextAlignedPro("Pause [P]", uiSize * new Vector2(0.99f, 0.07f), 0f, textSize, 2f, Demo.PALETTES.C("text"), Demo.FONT.GetFont(), new(1, 0.5f));

            if (IsPaused())
            {
                var pos = GAMELOOP.UISize();
                SDrawing.DrawTextAlignedPro("PAUSED", uiSize * new Vector2(0.5f, 0.3f), 0f, uiSize * new Vector2(0.5f, 0.25f), 5f, Demo.PALETTES.C("header"), Demo.FONT.GetFont(), new(0.5f));
            }


           

        }
        public override void Close()
        {
            if (area != null) area.Close();
        }

        private void SpawnAsteroidDebug()
        {
            Asteroid a = new(GAMELOOP.MOUSE_POS_GAME, "xlarge");
            area.AddGameObject(a);
        }
    }
}