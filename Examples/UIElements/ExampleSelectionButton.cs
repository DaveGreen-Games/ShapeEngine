using Examples.Scenes;
using Raylib_CsLo;
using ShapeEngine.UI;
using System.Numerics;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Text;

namespace Examples.UIElements
{
    public class ExampleSelectionButton : UIElement
    {
        //public string Text { get; set; } = "Button";
        private Vector2 prevMousePos = new();
        public ExampleScene? Scene { get; private set; } = null;
        private TextFont textFont;
        public ExampleSelectionButton()
        {
            Hidden = true;
            DisabledSelection = true;
            textFont = new TextFont(GAMELOOP.FontDefault, 5f, Colors.Text);

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

            var acceptState = GAMELOOP.InputActionUIAcceptMouse.Consume();// Input.ConsumeAction(GAMELOOP.InputActionUIAcceptMouse);
            return acceptState is { Consumed: false, Released: true };
            //return IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT);
        }

        protected override bool CheckPressed()
        {
            if (Hidden || Scene == null) return false;
            var acceptState = GAMELOOP.InputActionUIAccept.Consume();// Input.ConsumeAction(GAMELOOP.InputActionUIAccept);
            return acceptState is { Consumed: false, Released: true };
            //return IsKeyReleased(KeyboardKey.KEY_SPACE) || IsKeyPressed(KeyboardKey.KEY_ENTER);
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
                textFont.ColorRgba = Colors.Highlight;
                textFont.DrawTextWrapNone(text, r, new(0f));
                // font.DrawText(text, r, 5f, new(0f), ExampleScene.ColorHighlight2);
            }
            else if (Pressed)
            {
                textFont.ColorRgba = Colors.Special;
                textFont.DrawTextWrapNone(text, r, new(0f));
                // font.DrawText(text, r, 5f, new(0f), ExampleScene.ColorHighlight1);
            }
            else
            {
                textFont.ColorRgba = Colors.Text;
                textFont.DrawTextWrapNone(text, r, new(0f));
                // font.DrawText(text, r, 5f, new(0f), ExampleScene.ColorHighlight1);
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
