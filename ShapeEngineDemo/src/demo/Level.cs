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
            color.a = (byte)(125 * this.r);
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
                    var randColor = Utils.ShiftHue(color, RNG.randI(-50, 50));
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
                    var randColor = Utils.ShiftHue(color, RNG.randI(-50, 50)); 
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
            AddLayer("stars fixed", -100, 0);
            AddLayer("stars far", -95, 0.02f);
            AddLayer("stars near", -90, 0.04f);
            AddLayer("planets far", -85, 0.08f);
            AddLayer("planets near", -80, 0.1f);
            AddLayer("asteroids", -5, 0);
            
            SpawnStars(RNG.randI(150, 250));
            SpawnPlanets(RNG.randI(2, 6));
            this.playfield = new(area, 3f, PaletteHandler.C("neutral"), -10);
        }

        public override void Draw()
        {
            base.Draw();
            //DrawRectangleLinesEx(new Rectangle(0.0f, 0.0f, ScreenHandler.GameWidth(), ScreenHandler.GameHeight()), 6, ColorPalette.Cur.neutral);

        }
        public override void DrawUI()
        {
            base.DrawUI();
            //UIHandler.DrawTextAligned(String.Format("Objects {0}", gameObjects.Count), new(GAMELOOP.UICenter().X, GAMELOOP.UISize().Y), UIHandler.GetFontSizeScaled(FontSize.SMALL), UIHandler.Scale(5), ColorPalette.Cur.text, Alignement.BOTTOMCENTER);
        }

        //public override void Update(float dt)
        //{
        //    base.Update(dt);
        //    if(RNG.randF() < 0.1f) SpawnStar();
        //}

        private void SpawnPlanet()
        {
            float randRadius = RNG.randF(3f, 8f);
            var planet = new Planet(inner, randRadius, RNG.randColor(150, 220, 255));


            if (randRadius < 5) AddGameObject(planet, false, "planets far");
            else AddGameObject(planet, false, "planets near");

        }
        private void SpawnPlanets(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                SpawnPlanet();
            }
        }
        private void SpawnStar()
        {
            //Vector2 pos = RNG.randVec2(outer);
            //float radius = RNG.randF(0.25f, 1.25f);
            //float lifetime = -1f; // RNG.randF(15, 30);
            //Color color = WHITE;
            //color.a = (byte)(100 * radius);
            //CircleParticle p = new(pos, 0f, color, radius, lifetime);
            //AddGameObject(p, false);

            float randRadius = RNG.randF(0.25f, 1.25f);
            var star = new Star(outer, randRadius, WHITE);
            
            if(randRadius < 0.5f) AddGameObject(star, false, "stars fixed");
            if(randRadius < 0.8f) AddGameObject(star, false, "stars far");
            else AddGameObject(star, false, "stars near");
        }
        private void SpawnStars(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                SpawnStar();
            }
        }
    }
    
    public class Level : Scene
    {
        private Area area;
        private Player player;
        private AsteroidSpawner asteroidSpawner;
        public Level()
        {
            Rectangle playArea = Utils.ScaleRectangle(ScreenHandler.GameArea(), 1.5f);
            this.area = new AreaBasic(playArea, 20, 20);
            this.asteroidSpawner = new(this.area, 1f, 2f);

            

            ArmoryInfo armoryInfo = new("minigun", "bouncer", "basic");
            player = new(armoryInfo, "starter");
            ScreenHandler.Cam.SetTarget(player);
            //ScreenHandler.Cam.ZoomFactor = 0.35f;
        }

        public override void Activate(Scene? oldScene)
        {
            ScreenHandler.Game.Flash(0.25f, new(0, 0, 0, 255), new(0, 0, 0, 0));
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
            ScreenHandler.Game.Flash(0.25f, new(0, 0, 0, 255), new(0, 0, 0, 255));
            Action action = () => GAMELOOP.SwitchScene(this, newScene);
            ScreenHandler.Cam.ResetZoom();
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
        }
        public override void Draw()
        {
            if (area == null) return;
            area.Draw();
            asteroidSpawner.Draw();
        }
        public override void DrawUI()
        {
            UIHandler.DrawTextAlignedPro(String.Format("{0}", GetFPS()), new Vector2(30, 60), -5f, FontSize.XLARGE, 5, PaletteHandler.C("special1"), Alignement.LEFTCENTER);
            if (area == null) return;
            area.DrawUI();
            UIHandler.DrawTextAlignedPro(String.Format("Objs {0}", area.GetGameObjects().Count), new(30, 200), 0f, FontSize.LARGE, 5, PaletteHandler.C("text"), Alignement.LEFTCENTER);
            UIHandler.DrawTextAlignedPro(String.Format("{0}", InputHandler.GetCurInputType()), new(30, 260), 0f, FontSize.LARGE, 5, PaletteHandler.C("text"), Alignement.LEFTCENTER);
            UIHandler.DrawTextAlignedPro(String.Format("GP {0}/{1}", InputHandler.CUR_GAMEPAD, InputHandler.GetConnectedGamepadCount()), new(30, 320), 0f, FontSize.LARGE, 5, PaletteHandler.C("text"), Alignement.LEFTCENTER);
            UIHandler.DrawTextAlignedPro(String.Format("Used {0}", InputHandler.gamepadUsed), new(30, 380), 0f, FontSize.LARGE, 5, PaletteHandler.C("text"), Alignement.LEFTCENTER);
            UIHandler.DrawTextAlignedPro("Debug Keys [8, 9, 0]", new Vector2(ScreenHandler.UIWidth() / 2, ScreenHandler.UIHeight() - 20), 0f, FontSize.LARGE, 5f, PaletteHandler.C("text"), Alignement.BOTTOMCENTER);

            UIHandler.DrawTextAlignedPro("Slow Time [ALT]", new Vector2(ScreenHandler.UIWidth() - 30, 60), 0f, FontSize.LARGE, 5f, PaletteHandler.C("text"), Alignement.RIGHTCENTER);
            UIHandler.DrawTextAlignedPro("Pause [P]", new Vector2(ScreenHandler.UIWidth() - 30, 150), 0f, FontSize.LARGE, 5f, PaletteHandler.C("text"), Alignement.RIGHTCENTER);

            if (IsPaused())
            {
                var pos = GAMELOOP.UISize();
                UIHandler.DrawTextAlignedPro("PAUSED", new Vector2(pos.X/2, pos.Y * 0.25f), 0f, FontSize.HEADER_L, 5f, PaletteHandler.C("header"), Alignement.CENTER);
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
