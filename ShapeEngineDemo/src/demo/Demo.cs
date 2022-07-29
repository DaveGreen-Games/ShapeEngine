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
        private Image icon;

        public override void Start()
        {
            //WINDOW ICON
            icon = ResourceManager.LoadImage("shape-engine-icon-bg");
            SetWindowIcon(icon);


            //DATA CONTAINER INIT
            DataContainerCDB dataContainer = new("test-properties", new ShapeEngineDemo.DataObjects.DefaultDataResolver(), "asteroids", "player", "guns", "projectiles", "colors", "engines");
            DataHandler.AddDataContainer(dataContainer);

            
            //COLOR PALETTES
            var colorData = DataHandler.GetCDBContainer().GetSheet<ColorData>("colors");
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


            //Set the clear color for game screen texture
            ScreenHandler.Game.SetClearColor(PaletteHandler.C("bg1"));



            //SHADERS - Does not work right now because Raylib-CsLo LoadShaderFromMemory does not work correctly...
            ShaderHandler.AddScreenShader("outline", "outline-shader", false, -1);
            ShaderHandler.AddScreenShader("colorize", "colorize-shader", false, 0);
            ShaderHandler.AddScreenShader("bloom", "bloom-shader", false, 1);
            ShaderHandler.AddScreenShader("chrom", "chromatic-aberration-shader", false, 2);
            ShaderHandler.AddScreenShader("crt", "crt-shader", true, 3);
            ShaderHandler.AddScreenShader("grayscale", "grayscale-shader", false, 4);
            ShaderHandler.AddScreenShader("pixelizer", "pixelizer-shader", false, 5);
            ShaderHandler.AddScreenShader("blur", "blur-shader", false, 6);

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


            //FONTS
            UIHandler.AddFont("light", "teko-light", 200);
            UIHandler.AddFont("regular", "teko-regular", 200);
            UIHandler.AddFont("medium", "teko-medium", 200);
            UIHandler.AddFont("semibold", "teko-semibold", 200);
            UIHandler.SetDefaultFont("medium");


            //AUDIO BUSES
            AudioHandler.AddBus("music", 0.5f, "master");
            AudioHandler.AddBus("sound", 0.5f, "master");


            //SOUNDS
            AudioHandler.AddSFX("button click", "button-click01", 0.25f, "sound");
            AudioHandler.AddSFX("button hover", "button-hover01", 0.5f, "sound");
            AudioHandler.AddSFX("boost", "boost01", 0.5f, "sound");
            AudioHandler.AddSFX("slow", "slow02", 0.5f, "sound");
            AudioHandler.AddSFX("player hurt", "hurt01", 0.5f, "sound");
            AudioHandler.AddSFX("player die", "die01", 0.5f, "sound");
            AudioHandler.AddSFX("player stun ended", "stun01", 0.5f, "sound");
            AudioHandler.AddSFX("player healed", "healed01", 0.5f, "sound");

            AudioHandler.AddSFX("player pwr down", "pwrDown01", 1.0f, "sound");
            AudioHandler.AddSFX("player pwr up", "pwrUp05", 1.0f, "sound");

            AudioHandler.AddSFX("projectile pierce", "projectilePierce01", 0.7f, "sound");
            AudioHandler.AddSFX("projectile bounce", "projectileBounce03", 0.6f, "sound");
            AudioHandler.AddSFX("projectile impact", "projectileImpact01", 0.8f, "sound");
            AudioHandler.AddSFX("projectile explosion", "explosion01", 1f, "sound");
            AudioHandler.AddSFX("projectile crit", "projectileCrit01", 0.6f, "sound");
            AudioHandler.AddSFX("asteroid die", "die02", 0.55f, "sound");
            AudioHandler.AddSFX("bullet", "gun05", 0.25f, "sound");



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
            InputAction boost = new("Boost", InputAction.Keys.W, InputAction.Keys.GP_BUTTON_LEFT_FACE_UP, InputAction.Keys.GP_BUTTON_LEFT_TRIGGER_BOTTOM);
            InputAction slow = new("Slow", InputAction.Keys.S, InputAction.Keys.GP_BUTTON_LEFT_FACE_DOWN, InputAction.Keys.GP_BUTTON_LEFT_TRIGGER_TOP);
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
            InputHandler.AddInputMap(inputMap, true);
            InputHandler.SwitchToMap("Default", 0);


            //SPAWN SPLASH SCREEN
            Action startscene = () => GoToScene("splash");
            TimerHandler.Add(2.0f, startscene);
        }
        
        public override void HandleInput()
        {
            if (InputHandler.IsReleased(0, "Fullscreen")) { ScreenHandler.ToggleFullscreen(); }

            if (DEBUGMODE)
            {
                if (InputHandler.IsReleased(0, "Toggle Draw Helpers")) DEBUG_DrawHelpers = !DEBUG_DrawHelpers;
                if (InputHandler.IsReleased(0, "Toggle Draw Colliders")) DEBUG_DrawColliders = !DEBUG_DrawColliders;
                if (InputHandler.IsReleased(0, "Cycle Zoom"))
                {
                    ScreenHandler.Cam.ZoomBy(0.25f);
                    if (ScreenHandler.Cam.ZoomFactor > 2) ScreenHandler.Cam.ZoomFactor = 0.25f;
                }
            }
        }

        public override void End()
        {
            base.End();
            UnloadImage(icon);
        }
    }
}
