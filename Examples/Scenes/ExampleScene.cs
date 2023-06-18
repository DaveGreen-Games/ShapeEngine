using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using System.Numerics;

namespace Examples.Scenes
{
    public class ExampleScene : Scene
    {
        public string Title { get; protected set; } = "Title Goes Here";
        public string Description { get; protected set; } = "No Description Yet.";

        protected Font titleFont;

        public ExampleScene()
        {
            titleFont = GAMELOOP.FontDefault;// GAMELOOP.GetFont(GameloopExamples.FONT_IndieFlowerRegular);
        }

        public virtual void Reset() { }

        public override void HandleInput(float dt)
        {
            if (IsKeyPressed(KeyboardKey.KEY_R)) Reset();
            if (IsKeyPressed(KeyboardKey.KEY_ESCAPE)) GAMELOOP.GoToMainScene();
        }

        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            Segment s = new(uiSize * new Vector2(0f, 0.07f), uiSize * new Vector2(1f, 0.07f));
            s.Draw(2f, WHITE);

            Rect r = new Rect(uiSize * new Vector2(0.5f, 0.02f), uiSize * new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0f));
            titleFont.DrawText(Title, r, 10f, new(0.5f), WHITE);

            string backText = "Back [ESC]";
            Rect backRect = new Rect(uiSize * new Vector2(0.02f, 0.02f), uiSize * new Vector2(0.25f, 0.03f), new Vector2(0f));
            titleFont.DrawText(backText, backRect, 4f, new Vector2(0.5f), RED);
        }
    }

}
