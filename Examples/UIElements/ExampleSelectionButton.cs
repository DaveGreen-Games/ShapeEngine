using System.Numerics;
using Examples.Scenes;
using Examples.Scenes.ExampleScenes;
using Raylib_cs;
using ShapeEngine.Core.Structs;
using ShapeEngine.UI;
using ShapeEngine.Lib;
using ShapeEngine.Text;

namespace Examples.UIElements
{
    public class ExampleSelectionButton : ControlNode
    {
        private float inputCooldownTimer = 0f;
        private const float inputCooldown = 0.1f;
        public ExampleScene? Scene { get; private set; } = null;
        private TextFont textFont;
        public bool Hidden => Scene == null;
        public ExampleSelectionButton()
        {
            // Hidden = true;
            // DisabledSelection = true;
            textFont = new TextFont(GAMELOOP.FontDefault, 5f, Colors.Text);
            MouseFilter = MouseFilter.Stop;
            SelectionFilter = SelectFilter.All;
            InputFilter = InputFilter.All;
            

        }

        public void SetScene(ExampleScene? newScene)
        {
            Scene = newScene;
            Active = Scene != null;
            Visible = Active;
        }
        
        protected override bool GetMousePressedState()
        {
            if (!MouseInside) return false;
            if (Scene == null) return false;
            var acceptState = GAMELOOP.InputActionUIAcceptMouse.Consume();
            return acceptState is { Consumed: false, Released: true };
        }

        protected override bool GetPressedState()
        {
            if (!Selected) return false;
            if (Scene == null) return false;
            var acceptState = GAMELOOP.InputActionUIAccept.Consume();
            return acceptState is { Consumed: false, Released: true };
        }

        public override Direction GetNavigationDirection()
        {
            if (inputCooldownTimer > 0f)
            {
                if (Raylib.IsKeyReleased(KeyboardKey.A) ||
                    Raylib.IsKeyReleased(KeyboardKey.D) ||
                    Raylib.IsKeyReleased(KeyboardKey.W) ||
                    Raylib.IsKeyReleased(KeyboardKey.S))
                {
                    inputCooldownTimer = 0f;
                }
                else return new();
            }
            
            var hor = 0;
            var vert = 0;
            if (Raylib.IsKeyDown(KeyboardKey.A)) hor = -1;
            else if (Raylib.IsKeyDown(KeyboardKey.D)) hor = 1;
            
            if (Raylib.IsKeyDown(KeyboardKey.W)) vert = -1;
            else if (Raylib.IsKeyDown(KeyboardKey.S)) vert = 1;
            return new(hor, vert);
        }

        protected override void SelectedWasChanged(bool value)
        {
            if (value)
            {
                inputCooldownTimer = inputCooldown;
                ContainerStretch =  1.25f;
            }
            else ContainerStretch = 1f;
        }
        protected override void PressedWasChanged(bool value)
        {
            if (Scene == null) return;
            if (value)
            {
                // Console.WriteLine($"Button Pressed - Scene {Scene.Title}");
                GAMELOOP.GoToScene(Scene);
                
            }
        }

        protected override void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid)
        {
            base.OnUpdate(dt, mousePos, mousePosValid);
            if (inputCooldownTimer > 0)
            {
                inputCooldownTimer -= dt;
            }
        }

        protected override void OnDraw()
        {
            if (!Active || Scene == null) return;
            
            var r = Rect;
            var text = Scene.Title;

            if (Selected)
            {
                textFont.ColorRgba = Colors.Highlight;
                textFont.DrawTextWrapNone(text, r, new(0f));
            }
            else if (Pressed)
            {
                textFont.ColorRgba = Colors.Special;
                textFont.DrawTextWrapNone(text, r, new(0f));
            }
            else
            {
                textFont.ColorRgba = Colors.Text;
                textFont.DrawTextWrapNone(text, r, new(0f));
            }
            
            if (MouseInside)
            {
                var amount = Rect.Size.Min() * 0.25f;
                var outside = Rect.ChangeSize(amount, new Vector2(0.5f, 0.5f));
                outside.DrawLines(2f, Colors.Medium);
            }
        }
        // protected override bool CheckMousePressed()
        // {
        //     if (Hidden || Scene == null) return false;
        //
        //     var acceptState = GAMELOOP.InputActionUIAcceptMouse.Consume();
        //     return acceptState is { Consumed: false, Released: true };
        //     //return IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT);
        // }
        //
        // protected override bool CheckPressed()
        // {
        //     if (Hidden || Scene == null) return false;
        //     var acceptState = GAMELOOP.InputActionUIAccept.Consume();// Input.ConsumeAction(GAMELOOP.InputActionUIAccept);
        //     return acceptState is { Consumed: false, Released: true };
        //     //return IsKeyReleased(KeyboardKey.KEY_SPACE) || IsKeyPressed(KeyboardKey.KEY_ENTER);
        // }
        //
        // public override void Update(float dt, Vector2 mousePosUI)
        // {
        //     Check(prevMousePos, mousePosUI, false, 5f);
        //     prevMousePos = mousePosUI;
        // }
        // public override void Draw()
        // {
        //     if (Hidden || Scene == null) return;
        //
        //     var r = GetRect();
        //     var text = Scene.Title;
        //
        //     if (Selected)
        //     {
        //         textFont.ColorRgba = Colors.Highlight;
        //         textFont.DrawTextWrapNone(text, r, new(0f));
        //         // font.DrawText(text, r, 5f, new(0f), ExampleScene.ColorHighlight2);
        //     }
        //     else if (Pressed)
        //     {
        //         textFont.ColorRgba = Colors.Special;
        //         textFont.DrawTextWrapNone(text, r, new(0f));
        //         // font.DrawText(text, r, 5f, new(0f), ExampleScene.ColorHighlight1);
        //     }
        //     else
        //     {
        //         textFont.ColorRgba = Colors.Text;
        //         textFont.DrawTextWrapNone(text, r, new(0f));
        //         // font.DrawText(text, r, 5f, new(0f), ExampleScene.ColorHighlight1);
        //     }
        //
        //
        // }
        
        // protected override void PressedChanged(bool pressed)
        // {
        //     if (Hidden || Scene == null) return;
        //     if (pressed)
        //     {
        //         GAMELOOP.GoToScene(Scene);
        //     }
        // }
    }

}
