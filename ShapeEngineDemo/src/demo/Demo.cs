using ShapeCore;
using ShapeScreen;
using ShapeAudio;
using ShapeInput;
using ShapeUI;
using ShapeTiming;
using ShapePersistent;
using Raylib_CsLo;
using ShapeColor;
using ShapeEngineDemo.DataObjects;
using ShapeAchievements;
using System.Numerics;
using ShapeLib;
using ShapeCursor;

namespace ShapeEngineDemo
{

    public class Demo : GameLoop
    {
        private Image icon;
        private int curResIndex = 0;
        private int curFrameRateLimitIndex = 0;

        public static ResourceManager RESOURCES = new("", "resources.txt");
        public static SavegameHandler SAVEGAME = new("solobytegames", "shape-engine-demo");
        public static DataHandler DATA = new();
        public static FontHandler FONT = new();
        public static CursorHandler CURSOR = new(true);
        public static DelegateTimerHandler TIMER = new();
        public static AchievementHandler ACHIEVEMENTS = new();
        public static PaletteHandler PALETTES = new();
        public const int SHADER_CRT = 0;
        public const int BUS_SOUND = 1;
        public const int BUS_MUSIC = 2;


        public const int FONT_Light = 0;
        public const int FONT_Regular = 1;
        public const int FONT_Medium = 2;
        public const int FONT_SemiBold = 3;
        public const int FONT_Huge = 4;


        public override void Start()
        {
            
            
            //WINDOW ICON
            icon = RESOURCES.LoadImage("resources/gfx/shape-engine-icon-bg.png");
            SetWindowIcon(icon);
            
            
            //DATA CONTAINER INIT
            var dataString = RESOURCES.LoadJsonData("resources/data/test-properties.json");
            DataContainerCDB dataContainer = new(new ShapeEngineDemo.DataObjects.DefaultDataResolver(), dataString, "asteroids", "player", "guns", "projectiles", "colors", "engines");
            DATA.AddDataContainer(dataContainer);
            
            
            //COLOR PALETTES
            var colorData = DATA.GetCDBContainer().GetSheet<ColorData>("colors");
            foreach (var palette in colorData)
            {
                Dictionary<int, Color> colors = new()
                {
                    {ColorIDs.Background1, PaletteHandler.HexToColor(palette.Value.bg1)},
                    {ColorIDs.Background2, PaletteHandler.HexToColor(palette.Value.bg2)},
                    {ColorIDs.Flash, PaletteHandler.HexToColor(palette.Value.flash)},
                    {ColorIDs.Special1, PaletteHandler.HexToColor(palette.Value.special1)},
                    {ColorIDs.Special2, PaletteHandler.HexToColor(palette.Value.special2)},
                    {ColorIDs.Text, PaletteHandler.HexToColor(palette.Value.text)},
                    {ColorIDs.Header, PaletteHandler.HexToColor(palette.Value.header)},
                    {ColorIDs.Player, PaletteHandler.HexToColor(palette.Value.player)},
                    {ColorIDs.Neutral, PaletteHandler.HexToColor(palette.Value.neutral)},
                    {ColorIDs.Enemy, PaletteHandler.HexToColor(palette.Value.enemy)},
                    {ColorIDs.Armor, PaletteHandler.HexToColor(palette.Value.armor)},
                    {ColorIDs.Acid, PaletteHandler.HexToColor(palette.Value.acid)},
                    {ColorIDs.Shield, PaletteHandler.HexToColor(palette.Value.shield)},
                    {ColorIDs.Radiation, PaletteHandler.HexToColor(palette.Value.radiation)},
                    {ColorIDs.Energy, PaletteHandler.HexToColor(palette.Value.energy)},
                    {ColorIDs.DarkMatter, PaletteHandler.HexToColor(palette.Value.darkMatter)},

                };
                PALETTES.AddPalette(palette.Key, colors);
            }
            PALETTES.ChangePalette("starter");
            
            
            //Set the clear color for game screen texture
            //ScreenHandler.Game.SetClearColor(new Color(0, 0, 0, 0)); // PaletteHandler.C("bg1"));

            //var outline = RESOURCES.LoadFragmentShader("resources/shaders/outline-shader.fs");
            //var colorize = RESOURCES.LoadFragmentShader("resources/shaders/colorize-shader.fs");
            //var bloom = RESOURCES.LoadFragmentShader("resources/shaders/bloom-shader.fs");
            //var chrom = RESOURCES.LoadFragmentShader("resources/shaders/chromatic-aberration-shader.fs");
            var crt = RESOURCES.LoadFragmentShader("resources/shaders/crt-shader.fs");
            //var grayscale = RESOURCES.LoadFragmentShader("resources/shaders/grayscale-shader.fs");
            //var pixelizer = RESOURCES.LoadFragmentShader("resources/shaders/pixelizer-shader.fs");
            //var blur = RESOURCES.LoadFragmentShader("resources/shaders/blur-shader.fs");
            //SHADERS - Does not work right now because Raylib-CsLo LoadShaderFromMemory does not work correctly...
            //ScreenHandler.SHADERS.AddScreenShader("outline", outline, false, -1);
            //ScreenHandler.SHADERS.AddScreenShader("colorize", colorize, false, 0);
            //ScreenHandler.SHADERS.AddScreenShader("bloom", bloom, false, 1);
            //ScreenHandler.SHADERS.AddScreenShader("chrom", chrom, false, 2);
            ScreenHandler.SHADERS.AddScreenShader(SHADER_CRT, crt, true, 3);
            //ScreenHandler.SHADERS.AddScreenShader("grayscale", grayscale, false, 4);
            //ScreenHandler.SHADERS.AddScreenShader("pixelizer", pixelizer, false, 5);
            //ScreenHandler.SHADERS.AddScreenShader("blur", blur, false, 6);

            //outline only works with transparent background!!
            //ScreenHandler.SHADERS.SetScreenShaderValueVec("outline", "textureSize", new float[] { ScreenHandler.GameWidth(), ScreenHandler.GameHeight() }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("outline", "outlineSize", 2.0f);
            //ScreenHandler.SHADERS.SetScreenShaderValueVec("outline", "outlineColor", new float[] { 1.0f, 0.0f, 0.0f, 1.0f }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
            //ScreenHandler.SHADERS.SetScreenShaderValueVec("chrom", "amount", new float[] { 1.2f, 1.2f }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
            //ScreenHandler.SHADERS.SetScreenShaderValueVec("bloom", "size", new float[] { ScreenHandler.CUR_WINDOW_SIZE.width, ScreenHandler.CUR_WINDOW_SIZE.height }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("bloom", "samples", 5f);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("bloom", "quality", 2.5f);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("pixelizer", "renderWidth", ScreenHandler.CUR_WINDOW_SIZE.width);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("pixelizer", "renderHeight", ScreenHandler.CUR_WINDOW_SIZE.height);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("pixelizer", "pixelWidth", 1.0f);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("pixelizer", "pixelHeight", 1.0f);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("blur", "renderWidth", ScreenHandler.CUR_WINDOW_SIZE.width);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("blur", "renderHeight", ScreenHandler.CUR_WINDOW_SIZE.height);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("blur", "scale", 1.25f);
            ScreenHandler.SHADERS.SetScreenShaderValueFloat(SHADER_CRT, "renderWidth", ScreenHandler.CUR_WINDOW_SIZE.width);
            ScreenHandler.SHADERS.SetScreenShaderValueFloat(SHADER_CRT, "renderHeight", ScreenHandler.CUR_WINDOW_SIZE.height);
            
            
            var light = RESOURCES.LoadFont("resources/fonts/teko-light.ttf", 200);
            var regular = RESOURCES.LoadFont("resources/fonts/teko-regular.ttf", 200);
            var medium = RESOURCES.LoadFont("resources/fonts/teko-medium.ttf", 200);
            var semibold = RESOURCES.LoadFont("resources/fonts/teko-semibold.ttf", 200);
            var huge = RESOURCES.LoadFont("resources/fonts/teko-semibold.ttf", 600);
            //FONTS

            var tf = TextureFilter.TEXTURE_FILTER_BILINEAR;
            int fs = 200;
            FONT.AddFont(FONT_Light, light, fs, tf);
            FONT.AddFont(FONT_Regular, regular, fs, tf);
            FONT.AddFont(FONT_Medium, medium, fs, tf);
            FONT.AddFont(FONT_SemiBold, semibold, fs, tf);
            FONT.AddFont(FONT_Huge, huge, 500, tf);
            FONT.SetDefaultFont(FONT_Medium);
            

            //AUDIO BUSES
            AudioHandler.AddBus(BUS_MUSIC, 0.5f, AudioHandler.BUS_MASTER);
            AudioHandler.AddBus(BUS_SOUND, 0.5f, AudioHandler.BUS_MASTER);

            
            var buttonClick = RESOURCES.LoadSound("resources/audio/sfx/button-click01.wav");
            var buttonHover = RESOURCES.LoadSound("resources/audio/sfx/button-hover01.wav");
            var boost = RESOURCES.LoadSound("resources/audio/sfx/boost01.wav");
            var slow = RESOURCES.LoadSound("resources/audio/sfx/slow02.wav");
            var playerHurt = RESOURCES.LoadSound("resources/audio/sfx/hurt01.wav");
            var playerDie = RESOURCES.LoadSound("resources/audio/sfx/die01.wav");
            var playerStunEnded = RESOURCES.LoadSound("resources/audio/sfx/stun01.wav");
            var playerHealed = RESOURCES.LoadSound("resources/audio/sfx/healed01.wav");
            var playerPwrDown = RESOURCES.LoadSound("resources/audio/sfx/pwrDown01.wav");
            var playerPwrUp = RESOURCES.LoadSound("resources/audio/sfx/pwrUp05.wav");
            var projectilePierce = RESOURCES.LoadSound("resources/audio/sfx/projectilePierce01.wav");
            var projectileBounce = RESOURCES.LoadSound("resources/audio/sfx/projectileBounce03.wav");
            var projectileImpact = RESOURCES.LoadSound("resources/audio/sfx/projectileImpact01.wav");
            var projectileExplosion = RESOURCES.LoadSound("resources/audio/sfx/explosion01.wav");
            var projectileCrit = RESOURCES.LoadSound("resources/audio/sfx/projectileCrit01.wav");
            var asteroidDie = RESOURCES.LoadSound("resources/audio/sfx/die02.wav");
            var bullet = RESOURCES.LoadSound("resources/audio/sfx/gun05.wav");


           


            //SOUNDS
            AudioHandler.AddSFX(SoundIDs.UI_Click, buttonClick, 0.25f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.UI_Hover, buttonHover, 0.5f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PLAYER_Boost, boost, 0.5f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PLAYER_Slow, slow, 0.5f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PLAYER_Hurt, playerHurt, 0.5f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PLAYER_Die, playerDie, 0.5f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PLAYER_StunEnded, playerStunEnded, 0.5f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PLAYER_Healed, playerHealed, 0.5f, BUS_SOUND);

            AudioHandler.AddSFX(SoundIDs.PLAYER_PowerDown, playerPwrDown, 1.0f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PLAYER_PowerUp, playerPwrUp, 1.0f, BUS_SOUND);

            AudioHandler.AddSFX(SoundIDs.PROJECTILE_Pierce, projectilePierce, 0.7f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PROJECTILE_Bounce, projectileBounce, 0.6f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PROJECTILE_Impact, projectileImpact, 0.8f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PROJECTILE_Explosion, projectileExplosion, 1f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PROJECTILE_Crit, projectileCrit, 0.6f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.PROJECTILE_Shoot, bullet, 0.25f, BUS_SOUND);
            AudioHandler.AddSFX(SoundIDs.ASTEROID_Die, asteroidDie, 0.55f, BUS_SOUND);
            


            //MUSIC EXAMPLE--------------
            //AudioHandler.AddSong("menu-song1", "song1", 0.5f, "music");
            //AudioHandler.AddSong("menu-song2", "song2", 0.35f, "music");
            //AudioHandler.AddSong("game-song1", "song3", 0.4f, "music");
            //AudioHandler.AddSong("game-song2", "song4", 0.5f, "music");
            //
            //AudioHandler.AddPlaylist("menu", new() { "menu-song1", "menu-song2" });
            //AudioHandler.AddPlaylist("game", new() { "game-song1", "game-song2" });
            //AudioHandler.StartPlaylist("menu");
            //----------------------------------


            AddScene("splash", new SplashScreen());
            AddScene("mainmenu", new MainMenu());


            //INPUT
            InputAction iaQuit = new("Quit", InputAction.Keys.ESCAPE);
            InputAction iaFullscreen = new("Fullscreen", InputAction.Keys.F);
            InputAction rotateLeft = new("Rotate Left", InputAction.Keys.A, InputAction.Keys.GP_BUTTON_LEFT_FACE_LEFT);
            InputAction rotateRight = new("Rotate Right", InputAction.Keys.D, InputAction.Keys.GP_BUTTON_LEFT_FACE_RIGHT);
            InputAction rotate = new("Rotate", 0.25f, InputAction.Keys.GP_AXIS_LEFT_X);
            InputAction boostInput = new("Boost", InputAction.Keys.W, InputAction.Keys.GP_BUTTON_LEFT_FACE_UP, InputAction.Keys.GP_BUTTON_LEFT_TRIGGER_BOTTOM);
            InputAction slowInput = new("Slow", InputAction.Keys.S, InputAction.Keys.GP_BUTTON_LEFT_FACE_DOWN, InputAction.Keys.GP_BUTTON_LEFT_TRIGGER_TOP);
            InputAction cycleGunSetup = new("Cycle Gun Setup", InputAction.Keys.ONE, InputAction.Keys.GP_BUTTON_RIGHT_FACE_UP);
            InputAction shootFixed = new("Shoot Fixed", InputAction.Keys.J, InputAction.Keys.SPACE, InputAction.Keys.GP_BUTTON_RIGHT_TRIGGER_BOTTOM, InputAction.Keys.MB_LEFT);
            InputAction dropAimPoint = new("Drop Aim Point",  InputAction.Keys.K, InputAction.Keys.GP_BUTTON_RIGHT_TRIGGER_TOP);

            InputAction cycleResolutionsDebug = new("Cycle Res", InputAction.Keys.RIGHT);
            InputAction nextMonitorDebug = new("Next Monitor", InputAction.Keys.LEFT);
            InputAction toggleVsyncDebug = new("Vsync", InputAction.Keys.V);
            InputAction cycleFramerateLimitDebug = new("Cycle Framerate Limit", InputAction.Keys.UP);

            InputAction pause = new("Pause", InputAction.Keys.P);
            InputAction slowTime = new("Slow Time", InputAction.Keys.LEFT_ALT);
            
            InputAction healPlayerDebug = new("Heal Player",InputAction.Keys.H);
            InputAction spawnAsteroidDebug = new("Spawn Asteroid", InputAction.Keys.G);
            InputAction toggleDrawHelpersDebug = new("Toggle Draw Helpers", InputAction.Keys.EIGHT);
            InputAction toggleDrawCollidersDebug = new("Toggle Draw Colliders", InputAction.Keys.NINE);
            InputAction cycleZoomDebug = new("Cycle Zoom", InputAction.Keys.ZERO);


            InputMap inputMap = new("Default", 
                iaQuit, iaFullscreen, 
                rotateLeft, rotateRight, rotate, 
                boostInput, slowInput, 
                shootFixed, dropAimPoint,
                pause, slowTime,
                spawnAsteroidDebug, healPlayerDebug, toggleDrawCollidersDebug, toggleDrawHelpersDebug, cycleZoomDebug, 
                cycleResolutionsDebug, nextMonitorDebug, cycleFramerateLimitDebug, toggleVsyncDebug
                );
            inputMap.AddActions(InputHandler.UI_Default_InputActions.Values.ToList());
            InputHandler.AddInputMap(inputMap);
            InputHandler.SwitchToMap("Default", 0);


            ScreenHandler.OnWindowSizeChanged += OnWindowSizeChanged;
            ScreenHandler.CAMERA.BaseZoom = 1.5f;

            
            //Achievements
            AchievementStat asteroidKills = new("asteroidKills", "Asteroid Kills", 0);
            ACHIEVEMENTS.AddStat(asteroidKills);


            Achievement asteroidKiller = new("asteroidKiller", "Asteroid Killer", true, asteroidKills, 0, 100, 20);
            Achievement asteroidDestroyer = new("asteroidDestroyer", "Asteroid Destroyer", false, asteroidKills, 100, 250, 50);
            Achievement asteroidAnnihilator = new("asteroidAnnihilator", "Asteroid Annihilator", false, asteroidKills, 250, 1000, 250);


            ACHIEVEMENTS.AddAchievement(asteroidKiller);
            ACHIEVEMENTS.AddAchievement(asteroidDestroyer);
            ACHIEVEMENTS.AddAchievement(asteroidAnnihilator);


            CURSOR.Add("ui", new CursorBasic(0.02f, RED));
            CURSOR.Add("game", new CursorGame(0.02f, RED));
            CURSOR.Switch("ui");
            CURSOR.Hide();

            InputHandler.OnInputChanged += OnInputTypeChanged;

            //ScreenHandler.SetFrameRateLimit(180);
            //SPAWN SPLASH SCREEN
            Action startscene = () => GoToScene("splash");
            TIMER.Add(2.0f, startscene);
        }
        

        public override void PostDrawUI(Vector2 uiSize, Vector2 stretchFactor)
        {
            CURSOR.Draw(uiSize, MOUSE_POS_UI);
            Rectangle r = SRect.ConstructRect(uiSize * new Vector2(0.97f), uiSize * new Vector2(0.2f, 0.08f), new(1, 1));
            ACHIEVEMENTS.Draw(FONT.GetFont(Demo.FONT_Medium), r, GRAY, WHITE, BLUE, YELLOW);
            
        }
        public override void PreUpdate(float dt)
        {
            TIMER.Update(dt);
            ACHIEVEMENTS.Update(dt);
        }

        public override void PreHandleInput()
        {
            if (InputHandler.IsReleased(0, "Fullscreen")) { ScreenHandler.ToggleFullscreen(); }
            if (InputHandler.IsReleased(0, "Next Monitor")) { ScreenHandler.NextMonitor(); }
            if (InputHandler.IsReleased(0, "Vsync")) { ScreenHandler.ToggleVsync(); }
            if (InputHandler.IsReleased(0, "Cycle Framerate Limit"))
            {
                List<int> frameRateLimits = new List<int>() { 30, 60, 72, 90, 120, 144, 180, 240 };
                curFrameRateLimitIndex += 1;
                if (curFrameRateLimitIndex >= frameRateLimits.Count) curFrameRateLimitIndex = 0;
                ScreenHandler.SetFrameRateLimit(frameRateLimits[curFrameRateLimitIndex]);

            }
            if (InputHandler.IsReleased(0, "Cycle Res") && !ScreenHandler.IsFullscreen())
            {
                List<(int width, int height)> supportedResolutions = new()
                {
                    //16:9
                    (1280, 720), (1366, 768), (1920, 1080), (2560, 1440),

                    //16:10
                    (1280, 800), (1440, 900), (1680, 1050), (1920, 1200),

                    //4:3
                    (640, 480), (800, 600), (1024, 768),

                    //21:9
                    (1280, 540), (1600,675), (2560, 1080), (3440, 1440)
                };
                var monitor = ScreenHandler.MONITOR_HANDLER.CurMonitor();
                int width = monitor.width;
                int height = monitor.height;
                List<(int width, int height)> resolutions = supportedResolutions.FindAll(((int width, int height) res) => res.width <= width && res.height <= height);
                curResIndex += 1;
                if (curResIndex >= resolutions.Count) curResIndex = 0;
                var res = resolutions[curResIndex];
                ScreenHandler.ResizeWindow(res.width, res.height);

            }
        }

        private void OnWindowSizeChanged(int w, int h)
        {
            ScreenHandler.SHADERS.SetScreenShaderValueFloat(SHADER_CRT, "renderWidth", w);
            ScreenHandler.SHADERS.SetScreenShaderValueFloat(SHADER_CRT, "renderHeight", h);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("pixelizer", "renderWidth", w);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("pixelizer", "renderHeight", h);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("blur", "renderWidth", w);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("blur", "renderHeight", h);
            //ScreenHandler.SHADERS.SetScreenShaderValueVec("bloom", "size", new float[] { w, h }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        private static void OnInputTypeChanged(InputType newInputType)
        {
            if (newInputType == InputType.KEYBOARD_MOUSE) { CURSOR.Show(); }
            else { CURSOR.Hide(); }
        }
        public override void End()
        {
            ScreenHandler.OnWindowSizeChanged -= OnWindowSizeChanged;
            InputHandler.OnInputChanged -= OnInputTypeChanged;
            RESOURCES.Close();
            DATA.Close();
            FONT.Close();
            CURSOR.Close();
            TIMER.Close();
            ACHIEVEMENTS.Close();
            PALETTES.Close();
            base.End();
            UnloadImage(icon);
        }
    }
}
