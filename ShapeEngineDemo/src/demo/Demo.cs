using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals.Audio;
using ShapeEngineCore.Globals.Input;
using ShapeEngineCore.Globals.UI;
using ShapeEngineCore.Globals.Shaders;
using ShapeEngineCore.Globals.Timing;
using ShapeEngineCore.Globals.Persistent;
using Raylib_CsLo;
using ShapeEngineCore;

namespace ShapeEngineDemo
{
    public class Demo : GameLoop
    {
        public override void Start()
        {
            var colorData = DataHandler.GetSheet<ColorData>("colors");
            foreach (var palette in colorData)
            {
                Dictionary<string, Color> colors = new()
                {
                    {"bg1", PaletteHandler.HexToColor(palette.Value.bg1)},
                    {"bg2", PaletteHandler.HexToColor(palette.Value.bg2)},
                    {"flash", PaletteHandler.HexToColor(palette.Value.flash)},
                    {"special1", PaletteHandler.HexToColor(palette.Value.special1)},
                    {"special2", PaletteHandler.HexToColor(palette.Value.special2)},
                    {"text", PaletteHandler.HexToColor(palette.Value.text)},
                    {"header", PaletteHandler.HexToColor(palette.Value.header)},
                    {"player", PaletteHandler.HexToColor(palette.Value.player)},
                    {"neutral", PaletteHandler.HexToColor(palette.Value.neutral)},
                    {"enemy", PaletteHandler.HexToColor(palette.Value.enemy)},
                    {"armor", PaletteHandler.HexToColor(palette.Value.armor)},
                    {"acid", PaletteHandler.HexToColor(palette.Value.acid)},
                    {"shield", PaletteHandler.HexToColor(palette.Value.shield)},
                    {"radiation", PaletteHandler.HexToColor(palette.Value.radiation)},
                    {"energy", PaletteHandler.HexToColor(palette.Value.energy)},
                    {"darkmatter", PaletteHandler.HexToColor(palette.Value.darkMatter)},

                };
                PaletteHandler.AddPalette(palette.Key, colors);
            }

            PaletteHandler.ChangePalette("starter");

            ScreenHandler.Game.SetClearColor(PaletteHandler.C("bg1"));

            ShaderHandler.AddScreenShader("outline", "shaders/outline-shader.fs", false, -1);
            ShaderHandler.AddScreenShader("colorize", "shaders/colorize-shader.fs", false, 0);
            ShaderHandler.AddScreenShader("bloom", "shaders/bloom-shader.fs", false, 1);
            ShaderHandler.AddScreenShader("chrom", "shaders/chromatic-aberration-shader.fs", false, 2);
            ShaderHandler.AddScreenShader("crt", "shaders/crt-shader.fs", true, 3);
            ShaderHandler.AddScreenShader("grayscale", "shaders/grayscale-shader.fs", false, 4);
            ShaderHandler.AddScreenShader("pixelizer", "shaders/pixelizer-shader.fs", false, 5);
            ShaderHandler.AddScreenShader("blur", "shaders/blur-shader.fs", false, 6);

            //outline only works with transparent background!!
            ShaderHandler.SetScreenShaderValueVec("outline", "textureSize", new float[] { ScreenHandler.GameWidth(), ScreenHandler.GameHeight() }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
            ShaderHandler.SetScreenShaderValueFloat("outline", "outlineSize", 2.0f);
            ShaderHandler.SetScreenShaderValueVec("outline", "outlineColor", new float[] { 1.0f, 0.0f, 0.0f, 1.0f }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
            ShaderHandler.SetScreenShaderValueVec("bloom", "size", new float[] { ScreenHandler.GameWidth(), ScreenHandler.GameHeight() }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
            ShaderHandler.SetScreenShaderValueFloat("pixelizer", "renderWidth", ScreenHandler.GameWidth());
            ShaderHandler.SetScreenShaderValueFloat("pixelizer", "renderHeight", ScreenHandler.GameHeight());
            ShaderHandler.SetScreenShaderValueFloat("pixelizer", "pixelWidth", 1.0f);
            ShaderHandler.SetScreenShaderValueFloat("pixelizer", "pixelHeight", 1.0f);
            ShaderHandler.SetScreenShaderValueFloat("blur", "renderWidth", ScreenHandler.GameWidth());
            ShaderHandler.SetScreenShaderValueFloat("blur", "renderHeight", ScreenHandler.GameHeight());
            ShaderHandler.SetScreenShaderValueFloat("blur", "scale", 1.25f);

            UIHandler.AddFont("light", "fonts/teko-light.ttf", 200);
            UIHandler.AddFont("regular", "fonts/teko-regular.ttf", 200);
            UIHandler.AddFont("medium", "fonts/teko-medium.ttf", 200);
            UIHandler.AddFont("semibold", "fonts/teko-semibold.ttf", 200);
            UIHandler.SetDefaultFont("medium");

            AudioHandler.AddBus("music", 0.5f, "master");
            AudioHandler.AddBus("sound", 0.5f, "master");

            AudioHandler.AddSFX("button click", "audio/sfx/button-click01.wav", 0.25f, "sound");
            AudioHandler.AddSFX("button hover", "audio/sfx/button-hover01.wav", 0.5f, "sound");
            AudioHandler.AddSFX("boost", "audio/sfx/boost01.wav", 0.5f, "sound");
            AudioHandler.AddSFX("slow", "audio/sfx/slow02.wav", 0.5f, "sound");
            AudioHandler.AddSFX("player hurt", "audio/sfx/hurt01.wav", 0.5f, "sound");
            AudioHandler.AddSFX("player die", "audio/sfx/die01.wav", 0.5f, "sound");
            AudioHandler.AddSFX("player stun ended", "audio/sfx/stun01.wav", 0.5f, "sound");
            AudioHandler.AddSFX("player healed", "audio/sfx/healed01.wav", 0.5f, "sound");

            AudioHandler.AddSFX("player pwr down", "audio/sfx/pwrDown01.wav", 1.0f, "sound");
            AudioHandler.AddSFX("player pwr up", "audio/sfx/pwrUp05.wav", 1.0f, "sound");

            AudioHandler.AddSFX("projectile pierce", "audio/sfx/projectilePierce01.wav", 0.7f, "sound");
            AudioHandler.AddSFX("projectile bounce", "audio/sfx/projectileBounce03.wav", 0.6f, "sound");
            AudioHandler.AddSFX("projectile impact", "audio/sfx/projectileImpact01.wav", 0.8f, "sound");
            AudioHandler.AddSFX("projectile explosion", "audio/sfx/explosion01.wav", 1f, "sound");
            AudioHandler.AddSFX("projectile crit", "audio/sfx/projectileCrit01.wav", 0.6f, "sound");
            //AudioHandler.AddSFX("asteroid hurt", "audio/sfx/hurt02.wav", 0.5f, "sound");
            AudioHandler.AddSFX("asteroid die", "audio/sfx/die02.wav", 0.55f, "sound");
            AudioHandler.AddSFX("bullet", "audio/sfx/gun05.wav", 0.25f, "sound");

            AudioHandler.AddSong("neon-road-trip", "audio/music/neon-road-trip.wav", 0.05f, "music");
            AudioHandler.AddSong("space-invaders", "audio/music/space-invaders.wav", 0.05f, "music");

            AudioHandler.AddSong("behind-the-darkness", "audio/music/behind-the-darkness.ogg", 0.1f, "music");
            AudioHandler.AddSong("synthetic-whisper", "audio/music/synthetic-whisper.ogg", 0.1f, "music");
            AudioHandler.AddSong("underbeat", "audio/music/underbeat.ogg", 0.1f, "music");

            AudioHandler.AddPlaylist("menu", new() { "behind-the-darkness", "synthetic-whisper", "underbeat" });
            AudioHandler.AddPlaylist("game", new() { "neon-road-trip", "space-invaders" });
            AudioHandler.StartPlaylist("menu");

            AddScene("splash", new SplashScreen());
            AddScene("mainmenu", new MainMenu());

            //InputAction iaRestart = new("Restart", InputAction.Keys.R);
            InputAction iaQuit = new("Quit", InputAction.Keys.ESCAPE);
            InputAction iaFullscreen = new("Fullscreen", InputAction.Keys.F);
            InputAction rotateLeft = new("Rotate Left", 0, InputAction.Keys.A, InputAction.Keys.GP_BUTTON_LEFT_FACE_LEFT);
            InputAction rotateRight = new("Rotate Right", 0, InputAction.Keys.D, InputAction.Keys.GP_BUTTON_LEFT_FACE_RIGHT);
            InputAction rotate = new("Rotate", 0, 0.25f, InputAction.Keys.GP_AXIS_LEFT_X);
            InputAction boost = new("Boost", 0, InputAction.Keys.W, InputAction.Keys.GP_BUTTON_LEFT_FACE_UP, InputAction.Keys.GP_BUTTON_LEFT_TRIGGER_BOTTOM);
            InputAction slow = new("Slow", 0, InputAction.Keys.S, InputAction.Keys.GP_BUTTON_LEFT_FACE_DOWN, InputAction.Keys.GP_BUTTON_LEFT_TRIGGER_TOP);
            InputAction cycleGunSetup = new("Cycle Gun Setup", InputAction.Keys.ONE, InputAction.Keys.GP_BUTTON_RIGHT_FACE_UP);
            InputAction shootFixed = new("Shoot Fixed", InputAction.Keys.J, InputAction.Keys.SPACE, InputAction.Keys.GP_BUTTON_RIGHT_TRIGGER_BOTTOM);
            InputAction dropAimPoint = new("Drop Aim Point", "K", "RB",  InputAction.Keys.K, InputAction.Keys.GP_BUTTON_RIGHT_TRIGGER_TOP);


            InputAction healPlayerDebug = new("Heal Player", InputAction.Keys.H);
            InputAction spawnAsteroidDebug = new("Spawn Asteroid", InputAction.Keys.G);
            InputAction toggleDrawHelpersDebug = new("Toggle Draw Helpers", InputAction.Keys.EIGHT);
            InputAction toggleDrawCollidersDebug = new("Toggle Draw Colliders", InputAction.Keys.NINE);
            InputAction cycleZoomDebug = new("Cycle Zoom", InputAction.Keys.ZERO);
            InputMap inputMap = new("Default", 
                iaQuit, iaFullscreen, 
                rotateLeft, rotateRight, rotate, 
                boost, slow, 
                shootFixed, dropAimPoint,
                spawnAsteroidDebug, healPlayerDebug, toggleDrawCollidersDebug, toggleDrawHelpersDebug, cycleZoomDebug
                );
            InputMapHandler.AddInputMap(inputMap);
            InputMapHandler.AddDefaultUIInputsToMap("Default");
            InputMapHandler.SwitchToMap("Default");
            Action startscene = () => GoToScene("splash");
            TimerHandler.Add(2.0f, startscene);
        }
        //public override void PostDraw()
        //{
        //    Screen.DEBUG_DrawMonitorInfo(20, 20, 25);
        //}
        public override void HandleInput()
        {
            if (InputMapHandler.IsReleased("Fullscreen")) { ScreenHandler.ToggleFullscreen(); }

            if (DEBUGMODE)
            {
                if (InputMapHandler.IsReleased("Toggle Draw Helpers")) DEBUG_DrawHelpers = !DEBUG_DrawHelpers;
                if (InputMapHandler.IsReleased("Toggle Draw Colliders")) DEBUG_DrawColliders = !DEBUG_DrawColliders;
                if (InputMapHandler.IsReleased("Cycle Zoom"))
                {
                    ScreenHandler.Cam.ZoomBy(0.25f);
                    if (ScreenHandler.Cam.ZoomFactor > 2) ScreenHandler.Cam.ZoomFactor = 0.25f;
                }
            }

            //if (InputMapHandler.IsReleased("Switch Palette")) { ColorPalette.Next(); ScreenHandler.GetTexture("main").SetClearColor(ColorPalette.Cur.bg1); }

            //if (InputMapHandler.IsReleased("Restart")) Restart();

            //if (InputMapHandler.IsReleased("Next Monitor")) ScreenHandler.NextMonitor();

            //if (InputMapHandler.IsReleased("Vsync")) ScreenHandler.ToggleVsync();
        }
    }
}
