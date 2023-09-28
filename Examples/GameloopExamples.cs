using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Lib;
using ShapeEngine.Persistent;
using ShapeEngine.Core;
using Examples.Scenes;
using ShapeEngine.Screen;

namespace Examples
{
    
    public class GameloopExamples : ShapeLoop
    {
        //public BasicCamera GameCam { get; private set; }
        public Font FontDefault { get; private set; }


        private Dictionary<int, Font> fonts = new();
        private List<string> fontNames = new();
        private MainScene? mainScene = null;
        //private Shader shader;
        
        //public GameloopExamples() : base(new(960 * 2, 540 * 2), new(1920, 1080))
        //{
        //    GameCam = new BasicCamera(new(0f), Game.GetSize(), new(0.5f), 1f, 0f);
        //}
        public GameloopExamples() : base(new(1920, 1080))
        {
            
        }
        protected override void LoadContent()
        {
            //Game.SetCamera(GameCam);

            //shader = ContentLoader.LoadFragmentShader("Resources/Shaders/CRTShader.fs");
            
            //fonts.Add(FontIDs.AbelRegular, ContentLoader.LoadFont("fonts/Abel-Regular.ttf", 200));
            fonts.Add(FontIDs.GruppoRegular, ContentLoader.LoadFont("Resources/Fonts/Gruppo-Regular.ttf", 100));
            fonts.Add(FontIDs.IndieFlowerRegular, ContentLoader.LoadFont("Resources/Fonts/IndieFlower-Regular.ttf", 100));
            fonts.Add(FontIDs.OrbitRegular, ContentLoader.LoadFont("Resources/Fonts/Orbit-Regular.ttf", 100));
            fonts.Add(FontIDs.OrbitronBold, ContentLoader.LoadFont("Resources/Fonts/Orbitron-Bold.ttf", 100));
            fonts.Add(FontIDs.OrbitronRegular, ContentLoader.LoadFont("Resources/Fonts/Orbitron-Regular.ttf", 100));
            fonts.Add(FontIDs.PromptLightItalic, ContentLoader.LoadFont("Resources/Fonts/Prompt-LightItalic.ttf", 100));
            fonts.Add(FontIDs.PromptRegular, ContentLoader.LoadFont("Resources/Fonts/Prompt-Regular.ttf", 100));
            fonts.Add(FontIDs.PromptThin, ContentLoader.LoadFont("Resources/Fonts/Prompt-Thin.ttf", 100));
            fonts.Add(FontIDs.TekoMedium, ContentLoader.LoadFont("Resources/Fonts/Teko-Medium.ttf", 100));
            fonts.Add(FontIDs.JetBrains, ContentLoader.LoadFont("Resources/Fonts/JetBrainsMono.ttf", 100));
            
            //fontNames.Add("Abel Regular");
            fontNames.Add("Gruppo Regular");
            fontNames.Add("Indie Flower Regular");
            fontNames.Add("Orbit Regular");
            fontNames.Add("Orbitron Bold");
            fontNames.Add("Orbitron Regular");
            fontNames.Add("Prompt Light Italic");
            fontNames.Add("Prompt Regular");
            fontNames.Add("Prompt Thin");
            fontNames.Add("Teko Medium");
            fontNames.Add("Jet Brains Mono");

            FontDefault = GetFont(FontIDs.JetBrains);
            this.VSync = false;
            this.FrameRateLimit = 60;
        }
        protected override void UnloadContent()
        {
            //ContentLoader.UnloadShader(shader);
            ContentLoader.UnloadFonts(fonts.Values);
        }
        protected override void BeginRun()
        {
            mainScene = new MainScene();
            GoToScene(mainScene);
        }

        protected override void WindowSizeChanged(DimensionConversionFactors conversionFactors)
        {
            
        }

        //protected override void HandleInput(float dt)
        //{
        //    if (IsKeyPressed(KeyboardKey.KEY_F))
        //    {
        //        GAMELOOP.ToggleWindowMaximize();
        //    }
        //    base.HandleInput(dt);
        //}
        protected override void Update(float dt)
        {
            UpdateScene();
        }

        protected override void DrawGame(ScreenInfo game)
        {
            DrawGameScene();
        }

        protected override void DrawUI(ScreenInfo ui)
        {
            DrawUIScene();
        }

        public int GetFontCount() { return fonts.Count; }
        public Font GetFont(int id) { return fonts[id]; }
        public string GetFontName(int id) { return fontNames[id]; }
        public Font GetRandomFont()
        {
            Font? randFont = SRNG.randCollection<Font>(fonts.Values.ToList(), false);
            return randFont != null ? (Font)randFont : FontDefault;
        }
        public void GoToMainScene()
        {
            if (mainScene == null) return;
            if (CurScene == mainScene) return;
            GoToScene(mainScene);
        }
    }

}
