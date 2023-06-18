using Examples.Scenes;
using Raylib_CsLo;
using ShapeEngine.UI;
using System.Numerics;
using ShapeEngine.Lib;

namespace Examples.UIElements
{
    public class ExampleSelectionButton : UIElement
    {
        //public string Text { get; set; } = "Button";
        private Vector2 prevMousePos = new();
        public ExampleScene? Scene { get; private set; } = null;
        private Font font;
        public ExampleSelectionButton()
        {
            Hidden = true;
            DisabledSelection = true;
            font = GAMELOOP.FontDefault; // GAMELOOP.GetFont(GameloopExamples.FONT_IndieFlowerRegular);

        }

        public void SetScene(ExampleScene? newScene)
        {
            Scene = newScene;
            Hidden = newScene == null;
            DisabledSelection = Hidden;

        }
        protected override bool CheckMousePressed()
        {
            if (Hidden || Scene == null) return false;
            return IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT);
        }

        protected override bool CheckPressed()
        {
            if (Hidden || Scene == null) return false;
            return IsKeyReleased(KeyboardKey.KEY_SPACE) || IsKeyPressed(KeyboardKey.KEY_ENTER);
        }
        public override void Update(float dt, Vector2 mousePosUI)
        {
            Check(prevMousePos, mousePosUI, false, 5f);
            prevMousePos = mousePosUI;
        }
        public override void Draw()
        {
            if (Hidden || Scene == null) return;

            var r = GetRect();
            var text = Scene.Title;

            if (Selected)
            {
                //r.DrawLines(4f, GREEN);
                font.DrawText(text, r, 5f, new(0f), GREEN);
            }
            else if (Pressed)
            {
                //r.DrawLines(4f, YELLOW);
                font.DrawText(text, r, 5f, new(0f), YELLOW);
            }
            else
            {
                //r.DrawLines(4f, WHITE);
                font.DrawText(text, r, 5f, new(0f), WHITE);
            }


        }
        protected override void PressedChanged(bool pressed)
        {
            if (Hidden || Scene == null) return;
            if (pressed)
            {
                GAMELOOP.GoToScene(Scene);
            }
        }
    }

}
