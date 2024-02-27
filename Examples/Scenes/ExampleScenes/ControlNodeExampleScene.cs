using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Text;
using ShapeEngine.UI;

namespace Examples.Scenes.ExampleScenes
{
    internal class ControlNodeContainer : ControlNode
    {
        // public ControlNodeContainer()
        // {
        //     MouseFilter = MouseFilter.Pass;
        // }

        protected override void OnDraw()
        {
            Rect.DrawLines(2f, Colors.Light);
        }
    }
    internal class ControlNodeLabel : ControlNode
    {
        private static TextFont textFont = new(GAMELOOP.GetFont(FontIDs.JetBrains), 1f, Colors.Text);
        public string Text { get; set; }
        public ControlNodeLabel(string text, Vector2 anchor, Vector2 stretch)
        {
            this.Text = text;
            this.Anchor = anchor;
            this.Stretch = stretch;
        }

        // protected override void ActiveWasChanged(bool value)
        // {
        //     Console.WriteLine($"Active was changed to {value} / Active In Hierarchy: {IsActiveInHierarchy}");
        // }
        //
        // protected override void ParentActiveWasChanged(bool value)
        // {
        //     Console.WriteLine($"Parent Active was changed to {value} / Active In Hierarchy: {IsActiveInHierarchy}");
        // }

        protected override void OnDraw()
        {
            if (!IsActiveInHierarchy)
            {
                textFont.ColorRgba = Colors.Medium;
            }
            else
            {
                textFont.ColorRgba = Colors.Text;
            }
            
            textFont.DrawWord(Text, Rect, Anchor);   
        }
    }
    internal class ControlNodeTexBox : ControlNode
    {
        private static TextFont textFont = new(GAMELOOP.GetFont(FontIDs.JetBrains), 1f, Colors.Text);
        public string Text { get; set; }
        public ControlNodeTexBox(string text, Vector2 anchor, Vector2 stretch)
        {
            this.Text = text;
            this.Anchor = anchor;
            this.Stretch = stretch;
        }

        protected override void OnDraw()
        {
            textFont.DrawTextWrapWord(Text, Rect, Anchor);   
        }
    }
    internal class ControlNodeButton : ControlNode
    {
        public ControlNodeButton(string text, Vector2 anchor, Vector2 stretch)
        {
            this.Anchor = anchor;
            this.Stretch = stretch;
            var label = new ControlNodeLabel(text, new(0.5f), new(0.98f, 0.98f));
            AddChild(label);

            this.MouseFilter = MouseFilter.Stop;
            this.SelectionFilter = SelectFilter.All;
            this.InputFilter = InputFilter.All;
        }

        protected override bool GetPressedState()
        {
            // || MouseInside
            if (!Selected) return false;
            return Raylib.IsKeyDown(KeyboardKey.Space);
        }

        protected override bool GetMousePressedState()
        {
            if (!MouseInside) return false;
            return Raylib.IsMouseButtonDown(MouseButton.Left);
        }

        public override NavigationDirection GetNavigationDirection()
        {
            var hor = 0;
            var vert = 0;
            if (Raylib.IsKeyPressed(KeyboardKey.A)) hor = -1;
            else if (Raylib.IsKeyPressed(KeyboardKey.D)) hor = 1;
            
            if (Raylib.IsKeyPressed(KeyboardKey.W)) vert = -1;
            else if (Raylib.IsKeyPressed(KeyboardKey.S)) vert = 1;
            return new(hor, vert);
        }

        // private Vector2 mousePos = new();
        // protected override void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid)
        // {
        //     this.mousePos = mousePos;
        // }

        protected override void OnDraw()
        {
            if (!Active)
            {
                // Rect.Draw(Colors.Dark);
            }
            else
            {
                if (Pressed)
                {
                    Rect.Draw(Colors.Warm);
                }
                else if (Selected)
                {
                    Rect.Draw(Colors.Medium);
                }
                else
                {
                    Rect.Draw(Colors.Dark);
                }
            
                if (MouseInside)
                {
                    var amount = Rect.Size.Y * 0.25f;
                    var outside = Rect.ChangeSize(amount, new Vector2(0.5f, 0.5f));//  Rect.ScaleSize(1.2f, new Vector2(0.5f));
                    outside.DrawLines(2f, Colors.Medium);
                }
                
            }
            
        }
        
        
    }
    
    public class ControlNodeExampleScene : ExampleScene
    {
        // private readonly InputAction iaNextAlignement;
        private readonly ControlNodeNavigator navigator;
        private readonly ControlNodeContainer container;

        private readonly ControlNodeButton optionButton;
        // private readonly EventTester eventTester;
        public ControlNodeExampleScene() : base()
        {
            Title = "UI System 2.0 Example";

            container = new()
            {
                Anchor = new(0.5f),
                Stretch = new(0.98f, 0.8f)
            };
            navigator = new();

            var buttonContainer = new ControlNodeContainer
            {
                Anchor = new Vector2(0.5f),
                Stretch = new Vector2(0.5f, 0.5f)
            };

            var startButton = new ControlNodeButton("Start", new(0.5f, 0.1f), new Vector2(0.9f, 0.25f));
            optionButton = new ControlNodeButton("Options", new(0.5f, 0.5f), new Vector2(0.9f, 0.25f));
            var quitButton = new ControlNodeButton("Quit", new(0.5f, 0.9f), new Vector2(0.9f, 0.25f));

            
            
            buttonContainer.AddChild(startButton);
            buttonContainer.AddChild(optionButton);
            buttonContainer.AddChild(quitButton);

            container.AddChild(buttonContainer);

            var titleLabel = new ControlNodeLabel("UI System 2.0", new Vector2(0.5f, 0f), new Vector2(0.8f, 0.15f));
            container.AddChild(titleLabel);

            var infoBox = new ControlNodeTexBox(
                "This is an info box about the new ui system. " +
                "It improves many long standing issues, simplifies the usage while still being versatile. " +
                "It is loosely based on Godots Control system.", 
                new Vector2(0.02f, 0.98f), new Vector2(0.6f, 0.15f));
            container.AddChild(infoBox);

            var backButton = new ControlNodeButton("Back", new Vector2(0.95f, 0.95f), new Vector2(0.2f, 0.1f));
            container.AddChild(backButton);
            
            navigator.AddNode(container);

            // optionButton.Active = false;
            
            navigator.StartNavigation();

            
            startButton.OnPressedChanged += OnStartButtonPressedChanged;
            optionButton.OnPressedChanged += OnOptionButtonPressedChanged;
            quitButton.OnPressedChanged += OnQuitButtonPressedChanged;
            backButton.OnPressedChanged += OnBackButtonPressedChanged;
        }

        private void OnStartButtonPressedChanged(ControlNode node, bool value)
        {
            if(!value) navigator.StartNavigation();
        }
        private void OnOptionButtonPressedChanged(ControlNode node, bool value)
        {
            if(!value) node.Active = false;
        }
        private void OnQuitButtonPressedChanged(ControlNode node, bool value)
        {
            if(!value) navigator.EndNavigation();
        }
        private void OnBackButtonPressedChanged(ControlNode node, bool value)
        {
            if(!value) optionButton.Active = true;
        }
        
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            container.UpdateRect(ui.Area);
            container.Update(time.Delta, ui.MousePos);
            navigator.Update();
        }

        protected override void OnDrawGameUIExample(ScreenInfo ui)
        {
            container.Draw();
        }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
        }
    }

}
