using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals.UI;
using ShapeEngineCore.Globals.Audio;
using ShapeEngineCore.Globals.Input;
using ShapeEngineCore.Globals.Timing;
using ShapeEngineCore.Globals.Cursor;
using ShapeEngineDemo.Bodies;
using Windows.Devices.PointOfService;
using Windows.Devices.Usb;

namespace ShapeEngineDemo
{
    internal class Star : GameObject
    {
        private Vector2 pos;
        private float r = 1f;
        private Color color;


        public Star(Rectangle spawnArea, float radius, Color color)
        {
            this.pos = RNG.randVec2(spawnArea);
            this.r = radius;
            this.color = color;
            DrawOrder = RNG.randF(-1f, 1f);
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
            this.pos = RNG.randVec2(spawnArea);
            this.r = radius;
            this.color = color;
            DrawOrder = RNG.randF(-1f, 1f);

            if (r > 6)
            {
                int randAmount = RNG.randI(0, 4);
                for (int i = 0; i < randAmount; i++)
                {
                    var randR = RNG.randF(1f, this.r / 2f);
                    var randPos = RNG.randVec2(0, this.r - randR);
                    var randColor = Utils.ChangeHUE(color, RNG.randI(-50, 50));
                    randColor = Utils.ChangeBrightness(randColor, RNG.randF(-0.2f, -0.1f));
                    circles.Add((randPos, randR, randColor));
                }
            }

            if(RNG.randF() < 0.1f)
            {
                int randAmount = RNG.randI(1, 2);
                for (int i = 0; i < randAmount; i++)
                {
                    var randR = RNG.randF(r * 1.2f, r * 2.5f);
                    var randThickness = RNG.randF(1f, (randR - this.r) / 2);
                    var randColor = Utils.ChangeHUE(color, RNG.randI(-50, 50)); 
                    rings.Add((new Vector2(0f), randR, randThickness, Utils.ChangeAlpha(randColor, (byte) RNG.randI(75, 150))));
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
                Drawing.DrawCircleLines(pos + AreaLayerOffset + ring.center, ring.r, ring.thickness, ring.color, 4f);
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
            //for (int i = 0; i < 5; i++)
            //{
            //    string layer = String.Format("stars {0}", i);
            //    AddLayer(layer, -100 + 2 * i, 0.9f - 0.1f * i);
            //    SpawnStars(60, new Vector2(0.7f, 0.8f) + new Vector2(0.06f, 0.06f) * i, new Vector2(0.4f, 0.5f) + new Vector2(0.04f, 0.04f) * i, layer);
            //}


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
            this.playfield = new(area, 3f, PaletteHandler.C("neutral"), -10);
        }

        public override void Draw()
        {
            base.Draw();
            //DrawRectangleLinesEx(new Rectangle(0.0f, 0.0f, ScreenHandler.GameWidth(), ScreenHandler.GameHeight()), 6, ColorPalette.Cur.neutral);

        }
        //public override void DrawUI(Vector2 uiSize, Vector2 stretchFactor)
        //{
        //    base.DrawUI(devRes, stretchFactor);
        //    //UIHandler.DrawTextAligned(String.Format("Objects {0}", gameObjects.Count), new(GAMELOOP.UICenter().X, GAMELOOP.UISize().Y), UIHandler.GetFontSizeScaled(FontSize.SMALL), UIHandler.Scale(5), ColorPalette.Cur.text, Alignement.BOTTOMCENTER);
        //}

        //public override void Update(float dt)
        //{
        //    base.Update(dt);
        //    if(RNG.randF() < 0.1f) SpawnStar();
        //}

        private void SpawnPlanet(float radius, string layer, float brightness = 0f)
        {
            Color color = RNG.randColor(150, 220, 255);
            var planet = new Planet(inner, radius, Utils.ChangeBrightness(color, brightness));
            AddGameObject(planet, false, layer);

        }
        private void SpawnPlanets(int amount, Vector2 radiusRange, string layer, float brightness = 0f)
        {
            for (int i = 0; i < amount; i++)
            {
                SpawnPlanet(RNG.randF(radiusRange.X, radiusRange.Y), layer, brightness);
            }
        }
        private void SpawnStar(float radius, Vector2 alphaRange, string layer)
        {
            Color color = WHITE;
            color.a = (byte)(255 * RNG.randF(alphaRange.X, alphaRange.Y));
            var star = new Star(outer, radius, color);
            AddGameObject(star, false, layer);
        }
        private void SpawnStars(int amount, Vector2 radiusRange, Vector2 alphaRange, string layer)
        {
            for (int i = 0; i < amount; i++)
            {
                SpawnStar(RNG.randF(radiusRange.X, radiusRange.Y), alphaRange, layer);
            }
        }
    }
    
    public class Level : Scene
    {
        private Area area;
        private Player player;
        private AsteroidSpawner asteroidSpawner;
        private Rectangle playArea;

        //private Panel testPanel = new(Alignement.LEFTCENTER);
        //private float testRotationDeg = 0f;
        //private Label testLabel = new("TEST", Alignement.RIGHTCENTER);

        public Level()
        {
            Rectangle playArea = new Rectangle(0, 0, 800, 800);// Utils.ScaleRectangle(ScreenHandler.GameArea(), 1.5f);
            this.area = new AreaBasic(playArea, 20, 20);
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
            CursorHandler.Switch("game");
            GAMELOOP.backgroundColor = PaletteHandler.C("bg1");
            //AudioHandler.SwitchPlaylist("game");
        }
        public override void Deactivate(Scene? newScene)
        {
            if (newScene == null) return;
            ScreenHandler.GAME.Flash(0.25f, new(0, 0, 0, 255), new(0, 0, 0, 255));
            Action action = () => GAMELOOP.SwitchScene(this, newScene);
            ScreenHandler.CAMERA.ResetZoom();
            TimerHandler.Add(0.25f, action);

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
        public override void HandleInput(float dt)
        {
            if (InputHandler.IsReleased(0, "Pause")) TogglePause();
            
            if (InputHandler.IsDown(0, "Slow Time")) GAMELOOP.Slow(0.25f, 1.0f);
            else if (InputHandler.IsReleased(0, "Slow Time")) GAMELOOP.EndSlow();
            
            if (InputHandler.IsReleased(0, "UI Cancel")) GAMELOOP.GoToScene("mainmenu");
            
            if (!IsPaused() && InputHandler.IsReleased(0, "Spawn Asteroid")) SpawnAsteroidDebug();
        }
        public override void Update(float dt)
        {
            if (IsPaused()) return;
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
            UIHandler.DrawTextAlignedPro(String.Format("{0}", GetFPS()), uiSize * new Vector2(0.01f, 0.03f), -5f, uiSize * new Vector2(0.10f, 0.05f), 2f, PaletteHandler.C("special1"), Alignement.LEFTCENTER);
            if (area == null) return;
            area.DrawUI(uiSize, stretchFactor);

            Vector2 textSize = uiSize * new Vector2(0.25f, 0.04f);
            UIHandler.DrawTextAlignedPro(String.Format("Objs {0}", area.GetGameObjects().Count), uiSize * new Vector2(0.01f, 0.1f), 0f, textSize, 2f, PaletteHandler.C("text"), Alignement.LEFTCENTER);
            UIHandler.DrawTextAlignedPro(String.Format("{0}", InputHandler.GetCurInputType()), uiSize * new Vector2(0.01f, 0.13f), 0f, textSize, 2f, PaletteHandler.C("text"), Alignement.LEFTCENTER);
            UIHandler.DrawTextAlignedPro(String.Format("GP {0}/{1}", InputHandler.CUR_GAMEPAD, InputHandler.GetConnectedGamepadCount()), uiSize * new Vector2(0.01f, 0.16f), 0f, textSize, 2f, PaletteHandler.C("text"), Alignement.LEFTCENTER);
            UIHandler.DrawTextAlignedPro(String.Format("Used {0}", InputHandler.gamepadUsed), uiSize * new Vector2(0.01f, 0.19f), 0f, textSize, 2f, PaletteHandler.C("text"), Alignement.LEFTCENTER);
            
            
            UIHandler.DrawTextAlignedPro("Debug Keys [8, 9, 0]", uiSize * new Vector2(0.5f, 0.98f), 0f, textSize, 2f, PaletteHandler.C("text"), Alignement.BOTTOMCENTER);
            

            UIHandler.DrawTextAlignedPro("Slow Time [ALT]", uiSize * new Vector2(0.99f, 0.03f), 0f, textSize, 2f, PaletteHandler.C("text"), Alignement.RIGHTCENTER);
            UIHandler.DrawTextAlignedPro("Pause [P]", uiSize * new Vector2(0.99f, 0.07f), 0f, textSize, 2f, PaletteHandler.C("text"), Alignement.RIGHTCENTER);

            if (IsPaused())
            {
                var pos = GAMELOOP.UISize();
                UIHandler.DrawTextAlignedPro("PAUSED", uiSize * new Vector2(0.5f, 0.3f), 0f, uiSize * new Vector2(0.5f, 0.25f), 5f, PaletteHandler.C("header"), Alignement.CENTER);
            }


            //Drawing.DrawRectangeLinesPro(uiSize * new Vector2(0.5f, 0.5f), uiSize * new Vector2(0.25f, 0.1f), Alignement.CENTER, testRotationDeg * DEG2RAD, 3f, WHITE);
            //DrawCircleV(uiSize * new Vector2(0.5f, 0.5f), 5f, GOLD);
            //testPanel.SetRotation(testRotationDeg);
            //testPanel.SetOutlineThickness(3f);
            //testPanel.SetColors(RED, WHITE);
            //testPanel.UpdateRect(uiSize * new Vector2(0.5f, 0.5f), uiSize * new Vector2(0.25f, 0.1f), Alignement.LEFTCENTER);
            //testPanel.Draw(uiSize, stretchFactor);
            //
            //testLabel.SetRotation(testRotationDeg);
            //testLabel.SetOutlineThickness(2f);
            //testLabel.SetColors(BLUE, WHITE);
            //testLabel.SetTextColor(GRAY);
            //testLabel.UpdateRect(uiSize * new Vector2(0.5f, 0.65f), uiSize * new Vector2(0.25f, 0.1f), Alignement.RIGHTCENTER);
            //testLabel.Draw(uiSize, stretchFactor);
            //
            //DrawCircleV(uiSize * new Vector2(0.5f, 0.5f), 5f, GOLD);
            //DrawCircleV(uiSize * new Vector2(0.5f, 0.65f), 5f, GREEN);
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


/*
        private Bullet SpawnBullet()
        {
            return new(GAMELOOP.MOUSE_POS_GAME, 3.0f, RNG.randVec2(195, 205));
        }
        private List<Particle> GenerateParticles(int amount)
        {
            List<Particle> particles = new();
            for (int i = 0; i < amount; i++)
            {
                float randR = RNG.randF(1.0f, 3.0f);
                Rectangle area = ScreenHandler.GameArea();
                Vector2 randPos = new(RNG.randF(area.X + randR, area.width - randR), RNG.randF(area.y + randR, area.height - randR));
                Vector2 randVel = RNG.randVec2(10.0f, 50.0f);
                var particle = new Particle(randPos, randVel, randR, 0.0f);
                particles.Add(particle);
            }
            return particles;
        }
        private List<Particle> GenerateParticles(int amount, Vector2 center, float r)
        {
            List<Particle> particles = new();
            for (int i = 0; i < amount; i++)
            {
                float randR = RNG.randF(1.0f, 3.0f);
                Vector2 randPos = center + RNG.randVec2(0.0f, r);
                Vector2 randVel = RNG.randVec2(10.0f, 70.0f);
                var particle = new Particle(randPos, randVel, randR, 0.0f);
                particles.Add(particle);
            }
            return particles;
        }
        */
