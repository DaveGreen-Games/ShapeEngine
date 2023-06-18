using Raylib_CsLo;
using ShapeEngine.Lib;
using ShapeEngine.Persistent;
using ShapeEngine;
using Examples.Scenes;

namespace Examples
{
    public class GameloopExamples : GameLoopScene
    {
        public Font FontDefault { get; private set; }



        private Dictionary<int, Font> fonts = new();
        private List<string> fontNames = new();
        private MainScene? mainScene = null;

        public GameloopExamples() : base(960, 540, 1920, 1080)
        {

        }
        protected override void LoadContent()
        {
            //fonts.Add(FontIDs.AbelRegular, ContentLoader.LoadFont("fonts/Abel-Regular.ttf", 200));
            fonts.Add(FontIDs.GruppoRegular, ContentLoader.LoadFont("fonts/Gruppo-Regular.ttf", 200));
            fonts.Add(FontIDs.IndieFlowerRegular, ContentLoader.LoadFont("fonts/IndieFlower-Regular.ttf", 200));
            fonts.Add(FontIDs.OrbitRegular, ContentLoader.LoadFont("fonts/Orbit-Regular.ttf", 200));
            fonts.Add(FontIDs.OrbitronBold, ContentLoader.LoadFont("fonts/Orbitron-Bold.ttf", 200));
            fonts.Add(FontIDs.OrbitronRegular, ContentLoader.LoadFont("fonts/Orbitron-Regular.ttf", 200));
            fonts.Add(FontIDs.PromptLightItalic, ContentLoader.LoadFont("fonts/Prompt-LightItalic.ttf", 200));
            fonts.Add(FontIDs.PromptRegular, ContentLoader.LoadFont("fonts/Prompt-Regular.ttf", 200));
            fonts.Add(FontIDs.PromptThin, ContentLoader.LoadFont("fonts/Prompt-Thin.ttf", 200));
            fonts.Add(FontIDs.TekoMedium, ContentLoader.LoadFont("fonts/Teko-Medium.ttf", 200));

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

            FontDefault = GetFont(FontIDs.IndieFlowerRegular);

        }
        protected override void UnloadContent()
        {
            ContentLoader.UnloadFonts(fonts.Values);
        }
        protected override void BeginRun()
        {
            mainScene = new MainScene();
            GoToScene(mainScene);
        }

        protected override void HandleInput(float dt)
        {
            if (IsKeyPressed(KeyboardKey.KEY_F))
            {
                GAMELOOP.ToggleWindowMaximize();
            }
            base.HandleInput(dt);
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
