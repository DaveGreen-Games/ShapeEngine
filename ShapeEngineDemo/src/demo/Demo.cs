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
using Windows.UI.ViewManagement;

namespace ShapeEngineDemo
{
    public class CursorUI : ICursor
    {
        protected float sizeRelative = 20f;
        protected uint colorID;
        public uint ID { get;}
        public CursorUI(uint id, float size, uint colorID)
        {
            this.sizeRelative = size;
            this.colorID = colorID;
            this.ID = id;
        }
    
        public void Draw(Vector2 uiSize, Vector2 mousePos)
        {
            Vector2 tip = mousePos;
            Vector2 right = mousePos + SVec.Rotate(SVec.Right() * sizeRelative * uiSize.X, 90.0f * DEG2RAD);
            Vector2 left = mousePos + SVec.Rotate(SVec.Right() * sizeRelative * uiSize.X, 135.0f * DEG2RAD);
            DrawTriangle(tip, left, right, Demo.PALETTES.C(colorID));
    
        }
    }
    public class CursorGame : ICursor
    {
        protected float sizeRelative = 20f;
        protected uint colorID;
        public uint ID { get; }
        public CursorGame(uint id, float size, uint colorID)
        {
            this.sizeRelative = size;
            this.colorID = colorID;
            this.ID = id;
        }

        public void Draw(Vector2 uiSize, Vector2 mousePos)
        {
            Color color = Demo.PALETTES.C(colorID);
            float size = uiSize.X * sizeRelative;
            DrawLineEx(mousePos + new Vector2(-size / 2, -size / 2), mousePos + new Vector2(-size / 4, -size / 2), 4.0f, color);
            DrawLineEx(mousePos + new Vector2(-size / 2, -size / 2), mousePos + new Vector2(-size / 2, -size / 4), 4.0f, color);
    
            DrawLineEx(mousePos + new Vector2(size / 2, -size / 2), mousePos + new Vector2(size / 4, -size / 2), 4.0f, color);
            DrawLineEx(mousePos + new Vector2(size / 2, -size / 2), mousePos + new Vector2(size / 2, -size / 4), 4.0f, color);
    
            DrawLineEx(mousePos + new Vector2(-size / 2, size / 2), mousePos + new Vector2(-size / 4, size / 2), 4.0f, color);
            DrawLineEx(mousePos + new Vector2(-size / 2, size / 2), mousePos + new Vector2(-size / 2, size / 4), 4.0f, color);
    
            DrawLineEx(mousePos + new Vector2(size / 2, size / 2), mousePos + new Vector2(size / 4, size / 2), 4.0f, color);
            DrawLineEx(mousePos + new Vector2(size / 2, size / 2), mousePos + new Vector2(size / 2, size / 4), 4.0f, color);
    
        }
    }

    public class Demo : GameLoop
    {
        private Image icon;
        private int curResIndex = 0;
        private int curFrameRateLimitIndex = 0;

        public static AudioDevice AUDIO = new();
        public static ResourceManager RESOURCES = new("", "resources.txt");
        public static SavegameHandler SAVEGAME = new("solobytegames", "shape-engine-demo");
        public static DataHandler DATA = new();
        public static FontHandler FONT = new();
        public static CursorHandler CURSOR = new(true);
        public static DelegateTimerHandler TIMER = new();
        public static AchievementHandler ACHIEVEMENTS = new();
        public static PaletteHandler PALETTES = new();
        public static InputMap INPUT = new(0);
        public static Vector2 MousePos { get; private set; } = new(0f);
        public static Vector2 MousePosUI { get; private set; } = new(0f);
        public static Vector2 MousePosGame { get; private set; } = new(0f);

        public static readonly uint SHADER_CRT = SID.NextID;


        public static readonly uint FONT_Light = SID.NextID;
        public static readonly uint FONT_Regular = SID.NextID;
        public static readonly uint FONT_Medium = SID.NextID;
        public static readonly uint FONT_SemiBold = SID.NextID;
        public static readonly uint FONT_Huge = SID.NextID;

        public static readonly uint CURSOR_UI = SID.NextID;
        public static readonly uint CURSOR_Game = SID.NextID;
        public static readonly Dictionary<string, uint> PALETTE_IDs = new();
        public override void Start()
        {
            
            
            //WINDOW ICON
            icon = RESOURCES.LoadImage("resources/gfx/shape-engine-icon-bg.png");
            SetWindowIcon(icon);
            
            
            //DATA CONTAINER INIT
            var dataString = RESOURCES.LoadJsonData("resources/data/test-properties.json");
            DataContainerCDB dataContainer = new(new ShapeEngineDemo.DataObjects.DefaultDataResolver(), dataString, "asteroids", "player", "guns", "projectiles", "colors", "engines");
            DATA.AddDataContainer(dataContainer);

            Dictionary<string, uint> paletteIDs = new();
            //COLOR PALETTES
            var colorData = DATA.GetCDBContainer().GetSheet<ColorData>("colors");
            foreach (var palette in colorData)
            {
                uint id = SID.NextID;
                PALETTE_IDs.Add(palette.Key, id);
                Dictionary<uint, Color> colors = new()
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
                PALETTES.AddPalette(id, colors);
            }
            PALETTES.ChangePalette(PALETTE_IDs["starter"]);
            
            
            //Set the clear color for game screen texture
            //ScreenHandler.Game.SetClearColor(new Color(0, 0, 0, 0)); // PaletteHandler.C("bg1"));

            //var outline = RESOURCES.LoadFragmentShader("resources/shaders/outline-shader.fs");
            //var colorize = RESOURCES.LoadFragmentShader("resources/shaders/colorize-shader.fs");
            //var bloom = RESOURCES.LoadFragmentShader("resources/shaders/bloom-shader.fs");
            //var chrom = RESOURCES.LoadFragmentShader("resources/shaders/chromatic-aberration-shader.fs");
            //var grayscale = RESOURCES.LoadFragmentShader("resources/shaders/grayscale-shader.fs");
            //var pixelizer = RESOURCES.LoadFragmentShader("resources/shaders/pixelizer-shader.fs");
            //var blur = RESOURCES.LoadFragmentShader("resources/shaders/blur-shader.fs");
            //SHADERS - Does not work right now because Raylib-CsLo LoadShaderFromMemory does not work correctly...
            //ScreenHandler.SHADERS.AddScreenShader("outline", outline, false, -1);
            //ScreenHandler.SHADERS.AddScreenShader("colorize", colorize, false, 0);
            //ScreenHandler.SHADERS.AddScreenShader("bloom", bloom, false, 1);
            //ScreenHandler.SHADERS.AddScreenShader("chrom", chrom, false, 2);
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
            var crt = RESOURCES.LoadFragmentShader("resources/shaders/crt-shader.fs");
            GraphicsDevice.Shader.AddScreenShader(SHADER_CRT, crt, true, 3);
            GraphicsDevice.Shader.SetScreenShaderValueFloat(SHADER_CRT, "renderWidth", GraphicsDevice.CurWindowSize.width);
            GraphicsDevice.Shader.SetScreenShaderValueFloat(SHADER_CRT, "renderHeight", GraphicsDevice.CurWindowSize.height);
            
            
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
            AUDIO.BusAdd(SoundIDs.BUS_MUSIC, 0.5f);
            AUDIO.BusAdd(SoundIDs.BUS_SOUND, 0.5f);

            
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
            AUDIO.SFXAdd(SoundIDs.UI_Click, buttonClick, 0.25f,                     1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.UI_Hover, buttonHover, 0.5f,                      1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PLAYER_Boost, boost, 0.5f,                        1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PLAYER_Slow, slow, 0.5f,                          1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PLAYER_Hurt, playerHurt, 0.5f,                    1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PLAYER_Die, playerDie, 0.5f,                      1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PLAYER_StunEnded, playerStunEnded, 0.5f,          1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PLAYER_Healed, playerHealed, 0.5f,                1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PLAYER_PowerDown, playerPwrDown, 1.0f,            1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PLAYER_PowerUp, playerPwrUp, 1.0f,                1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PROJECTILE_Pierce, projectilePierce, 0.7f,        1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PROJECTILE_Bounce, projectileBounce, 0.6f,        1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PROJECTILE_Impact, projectileImpact, 0.8f,        1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PROJECTILE_Explosion, projectileExplosion, 1f,    1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PROJECTILE_Crit, projectileCrit, 0.6f,            1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.PROJECTILE_Shoot, bullet, 0.25f,                  1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);
            AUDIO.SFXAdd(SoundIDs.ASTEROID_Die, asteroidDie, 0.55f,                 1f, AudioDevice.BUS_MASTER, SoundIDs.BUS_SOUND);




            //INPUT
            InputAction quit =                      new(InputIDs.OPTIONS_Quit, IInputType.Create(SKeyboardButton.ESCAPE));//, IInputType.Create(SGamepadButton.MIDDLE_RIGHT));
            InputAction rotateLeft =                new(InputIDs.PLAYER_RotateLeft, IInputType.Create(SKeyboardButton.A), IInputType.Create(SGamepadButton.LEFT_STICK_LEFT));
            InputAction rotateRight =               new(InputIDs.PLAYER_RotateRight, IInputType.Create(SKeyboardButton.D), IInputType.Create(SGamepadButton.LEFT_STICK_RIGHT));
            InputAction rotate =                    new(InputIDs.PLAYER_Rotate, IInputType.Create(SKeyboardButton.A, SKeyboardButton.D), IInputType.Create(SGamepadAxis.LEFT_X, 0.25f));
            InputAction boostInput =                new(InputIDs.PLAYER_Boost, IInputType.Create(SKeyboardButton.W), IInputType.Create(SGamepadButton.RIGHT_TRIGGER_TOP));
            InputAction slowInput =                 new(InputIDs.PLAYER_Slow, IInputType.Create(SKeyboardButton.S), IInputType.Create(SGamepadButton.LEFT_TRIGGER_TOP));
            InputAction cycleGunSetup =             new(InputIDs.PLAYER_CycleGuns, IInputType.Create(SMouseButton.RIGHT), IInputType.Create(SKeyboardButton.Q), IInputType.Create(SGamepadButton.RIGHT_FACE_UP));
            InputAction shootFixed =                new(InputIDs.PLAYER_Shoot, IInputType.Create(SMouseButton.LEFT), IInputType.Create(SKeyboardButton.SPACE), IInputType.Create(SGamepadButton.RIGHT_TRIGGER_BOTTOM));
            InputAction dropAimPoint =              new(InputIDs.PLAYER_DropAimPoint, IInputType.Create(SKeyboardButton.E), IInputType.Create(SGamepadButton.RIGHT_FACE_LEFT));

            InputAction fullscreen =                new(InputIDs.OPTIONS_Fullscreen, IInputType.Create(SKeyboardButton.F), IInputType.Create(SGamepadButton.MIDDLE_LEFT));
            InputAction cycleResolutionsDebug =     new(InputIDs.OPTIONS_CycleRes, IInputType.Create(SKeyboardButton.UP));
            InputAction nextMonitorDebug =          new(InputIDs.OPTIONS_NextMonitor, IInputType.Create(SKeyboardButton.RIGHT));
            InputAction toggleVsyncDebug =          new(InputIDs.OPTIONS_Vsync, IInputType.Create(SKeyboardButton.DOWN));
            InputAction cycleFramerateLimitDebug =  new(InputIDs.OPTIONS_CycleFrameRateLimit, IInputType.Create(SKeyboardButton.LEFT));

            InputAction pause =                     new(InputIDs.GAME_Pause, IInputType.Create(SKeyboardButton.P), IInputType.Create(SGamepadButton.MIDDLE_RIGHT));
            InputAction slowTime =                  new(InputIDs.GAME_SlowTime, IInputType.Create(SKeyboardButton.LEFT_ALT), IInputType.Create(SGamepadButton.LEFT_THUMB));
            
            InputAction healPlayerDebug =           new(InputIDs.DEBUG_HealPlayer, IInputType.Create(SKeyboardButton.H));
            InputAction spawnAsteroidDebug =        new(InputIDs.DEBUG_SpawnAsteroid, IInputType.Create(SKeyboardButton.G));
            InputAction toggleDrawHelpersDebug =    new(InputIDs.DEBUG_ToggleDrawHelpers, IInputType.Create(SKeyboardButton.EIGHT));
            InputAction toggleDrawCollidersDebug =  new(InputIDs.DEBUG_ToggleDrawColliders, IInputType.Create(SKeyboardButton.NINE));
            InputAction cycleZoomDebug =            new(InputIDs.DEBUG_CycleZoom, IInputType.Create(SKeyboardButton.ZERO));

            InputAction uiPressed = new(InputIDs.UI_Pressed, IInputType.Create(SKeyboardButton.SPACE), IInputType.Create(SGamepadButton.RIGHT_FACE_DOWN));
            InputAction uiMousePressed = new(InputIDs.UI_MousePressed, IInputType.Create(SMouseButton.LEFT));
            InputAction uiCancel = new(InputIDs.UI_Cancel, IInputType.Create(SKeyboardButton.ESCAPE), IInputType.Create(SGamepadButton.RIGHT_FACE_RIGHT));
            InputAction uiDown = new(InputIDs.UI_Down, IInputType.Create(SKeyboardButton.S), IInputType.Create(SGamepadButton.LEFT_FACE_DOWN));
            InputAction uiUp = new(InputIDs.UI_Up, IInputType.Create(SKeyboardButton.W), IInputType.Create(SGamepadButton.LEFT_FACE_UP));
            InputAction uiLeft = new(InputIDs.UI_Left, IInputType.Create(SKeyboardButton.A), IInputType.Create(SGamepadButton.LEFT_FACE_LEFT));
            InputAction uiRight = new(InputIDs.UI_Right, IInputType.Create(SKeyboardButton.D), IInputType.Create(SGamepadButton.LEFT_FACE_RIGHT));

            //ui
            INPUT.AddGroup(InputIDs.GROUP_UI, uiPressed, uiMousePressed, uiCancel, uiDown, uiUp, uiLeft, uiRight);
            INPUT.AddGroup(InputIDs.GROUP_Settings, cycleResolutionsDebug, nextMonitorDebug, cycleFramerateLimitDebug, toggleVsyncDebug, fullscreen);
            INPUT.AddGroup(InputIDs.GROUP_Debug, spawnAsteroidDebug, healPlayerDebug, toggleDrawCollidersDebug, toggleDrawHelpersDebug, cycleZoomDebug);
            INPUT.AddGroup(InputIDs.GROUP_Player, rotateLeft, rotateRight, rotate, boostInput, slowInput, shootFixed, dropAimPoint);
            INPUT.AddGroup(InputIDs.GROUP_Level, quit, pause, slowTime);

            INPUT.SetGroupDisabled(InputIDs.GROUP_Player, true);
            INPUT.SetGroupDisabled(InputIDs.GROUP_Level, true);
            INPUT.SetGroupDisabled(InputIDs.GROUP_Debug, true);

            GraphicsDevice.OnWindowSizeChanged += OnWindowSizeChanged;
            GraphicsDevice.Camera.BaseZoom = 1.5f;

            
            //Achievements
            AchievementStat asteroidKills = new("asteroidKills", "Asteroid Kills", 0);
            ACHIEVEMENTS.AddStat(asteroidKills);


            Achievement asteroidKiller = new("asteroidKiller", "Asteroid Killer", true, asteroidKills, 0, 100, 20);
            Achievement asteroidDestroyer = new("asteroidDestroyer", "Asteroid Destroyer", false, asteroidKills, 100, 250, 50);
            Achievement asteroidAnnihilator = new("asteroidAnnihilator", "Asteroid Annihilator", false, asteroidKills, 250, 1000, 250);


            ACHIEVEMENTS.AddAchievement(asteroidKiller);
            ACHIEVEMENTS.AddAchievement(asteroidDestroyer);
            ACHIEVEMENTS.AddAchievement(asteroidAnnihilator);


            CURSOR.Add(new CursorUI(CURSOR_UI, 0.02f, ColorIDs.Player));
            CURSOR.Add(new CursorGame(CURSOR_Game, 0.02f, ColorIDs.Player));
            CURSOR.Switch(CURSOR_UI);
            CURSOR.Hide();

            INPUT.OnInputTypeChanged += OnInputTypeChanged;
            if (INPUT.IsGamepad) CURSOR.Hide();
            else CURSOR.Show();

            AddScene("splash", new SplashScreen());
            AddScene("mainmenu", new MainMenu());
            
            Action startscene = () => GoToScene("splash");
            TIMER.Add(2.0f, startscene);
        }
        

        public override void EndDrawUI(Vector2 uiSize)
        {
            CURSOR.Draw(uiSize, MousePosUI);
            Rectangle r = SRect.ConstructRect(uiSize * new Vector2(0.97f), uiSize * new Vector2(0.2f, 0.08f), new(1, 1));
            ACHIEVEMENTS.Draw(FONT.GetFont(Demo.FONT_Medium), r, GRAY, WHITE, BLUE, YELLOW);
            
        }
        public override void BeginUpdate(float dt)
        {
            TIMER.Update(dt);
            ACHIEVEMENTS.Update(dt);
            AUDIO.Update(dt, GraphicsDevice.Camera.RawPos);
            INPUT.Update(dt);
            MousePos = INPUT.MousePos;
            MousePosUI = GraphicsDevice.UITexture.ScalePositionV(MousePos);
            MousePosGame = GraphicsDevice.TransformPositionToGame(MousePosUI);
        }

        public override void BeginHandleInput()
        {

            if (INPUT.GetActionState(InputIDs.OPTIONS_Fullscreen).released) { GraphicsDevice.ToggleFullscreen(); }
            if (INPUT.GetActionState(InputIDs.OPTIONS_NextMonitor).released) { GraphicsDevice.NextMonitor(); }
            if (INPUT.GetActionState(InputIDs.OPTIONS_Vsync).released) { GraphicsDevice.ToggleVsync(); }
            if (INPUT.GetActionState(InputIDs.OPTIONS_CycleFrameRateLimit).released)
            {
                List<int> frameRateLimits = new List<int>() { 30, 60, 72, 90, 120, 144, 180, 240 };
                curFrameRateLimitIndex += 1;
                if (curFrameRateLimitIndex >= frameRateLimits.Count) curFrameRateLimitIndex = 0;
                GraphicsDevice.SetFrameRateLimit(frameRateLimits[curFrameRateLimitIndex]);

            }
            if (INPUT.GetActionState(InputIDs.OPTIONS_CycleRes).released && !GraphicsDevice.IsFullscreen())
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
                var monitor = GraphicsDevice.Monitor.CurMonitor();
                int width = monitor.width;
                int height = monitor.height;
                List<(int width, int height)> resolutions = supportedResolutions.FindAll(((int width, int height) res) => res.width <= width && res.height <= height);
                curResIndex += 1;
                if (curResIndex >= resolutions.Count) curResIndex = 0;
                var res = resolutions[curResIndex];
                GraphicsDevice.ResizeWindow(res.width, res.height);

            }
        }

        private void OnWindowSizeChanged(int w, int h)
        {
            GraphicsDevice.Shader.SetScreenShaderValueFloat(SHADER_CRT, "renderWidth", w);
            GraphicsDevice.Shader.SetScreenShaderValueFloat(SHADER_CRT, "renderHeight", h);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("pixelizer", "renderWidth", w);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("pixelizer", "renderHeight", h);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("blur", "renderWidth", w);
            //ScreenHandler.SHADERS.SetScreenShaderValueFloat("blur", "renderHeight", h);
            //ScreenHandler.SHADERS.SetScreenShaderValueVec("bloom", "size", new float[] { w, h }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        private static void OnInputTypeChanged()
        {
            if (INPUT.IsGamepad) CURSOR.Hide();
            else CURSOR.Show();
        }
        public override void End()
        {
            GraphicsDevice.OnWindowSizeChanged -= OnWindowSizeChanged;
            INPUT.OnInputTypeChanged -= OnInputTypeChanged;
            RESOURCES.Close();
            AUDIO.Close();
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
