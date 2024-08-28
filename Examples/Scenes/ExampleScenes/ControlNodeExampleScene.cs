using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Text;
using ShapeEngine.UI;

namespace Examples.Scenes.ExampleScenes
{
    internal class ControlNodeTestContainer : ControlNodeContainer
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
        public ControlNodeLabel(string text, AnchorPoint anchor, Vector2 stretch)
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
        public ControlNodeTexBox(string text, AnchorPoint anchor, Vector2 stretch)
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
        private float inputCooldownTimer = 0f;
        private const float inputCooldown = 0.1f;
        public ControlNodeButton(string text, AnchorPoint anchor, Vector2 stretch)
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
            if (!Selected) return false;
            var acceptState = GAMELOOP.InputActionUIAccept.Consume();
            return acceptState is { Consumed: false, Pressed: true };
            
            // if (!Selected) return false;
            // return Raylib.IsKeyDown(KeyboardKey.Space);
        }

        protected override bool GetMousePressedState()
        {
            if (!MouseInside) return false;
            var acceptState = GAMELOOP.InputActionUIAcceptMouse.Consume();
            return acceptState is { Consumed: false, Pressed: true };
            
            // if (!MouseInside) return false;
            // return Raylib.IsMouseButtonDown(MouseButton.Left);
        }

        public override Direction GetNavigationDirection()
        {
            var upState = GAMELOOP.InputActionUIUp.Consume();
            var downState = GAMELOOP.InputActionUIDown.Consume();
            var rightState = GAMELOOP.InputActionUIRight.Consume();
            var leftState = GAMELOOP.InputActionUILeft.Consume();
            
            if (inputCooldownTimer > 0f)
            {
                if (upState is { Consumed: false, Released: true } ||
                    downState is { Consumed: false, Released: true } ||
                    rightState is { Consumed: false, Released: true } ||
                    leftState is { Consumed: false, Released: true })
                {
                    inputCooldownTimer = 0f;
                }
                else return new();
            }
            
            var hor = 0;
            var vert = 0;
            if (leftState is { Consumed: false, Down: true }) hor = -1;
            else if (rightState is { Consumed: false, Down: true }) hor = 1;
            
            if (upState is { Consumed: false, Down: true }) vert = -1;
            else if (downState is { Consumed: false, Down: true }) vert = 1;
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
                    var amount = Rect.Size.Min() * 0.25f;
                    var outside = Rect.ChangeSize(amount, new AnchorPoint(0.5f, 0.5f));//  Rect.ScaleSize(1.2f, new Vector2(0.5f));
                    outside.DrawLines(2f, Colors.Medium);
                }
                
            }
            
        }

        protected override void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid)
        {
            if (inputCooldownTimer > 0)
            {
                inputCooldownTimer -= dt;
            }
        }
    }
    
    
    
    
    public class ControlNodeExampleScene : ExampleScene
    {
        // private readonly InputAction iaNextAlignement;
        private readonly ControlNodeNavigator navigator;
        private readonly ControlNodeTestContainer container;

        private readonly ControlNodeButton optionButton;

        private readonly ControlNodeContainer buttonContainer;
        // private readonly EventTester eventTester;

        private readonly InputAction cycleGridStyles;
        private readonly InputAction resetGridStyles;

        private int curGridStyle = 0;

        private static readonly List<Func<Grid>> GridStyles = new()
        {
            () => new(5, 5, false, false, false),
            () => new(5, 1, true, false, false),
            () => new(1, 7, false, false, false),
            () => new(5, 8, false, true, false),
            () => new(3, 3, true, true, true)
        };
        private static readonly List<string> GridStyleNames = new()
        {
            "5x5 - Default",
            "5x1 - Horizontal Reversed",
            "1x7 - Default",
            "5x8 - Vertical Reversed",
            "3x3 - Horizontal & Vertical Reversed, Top To Bottom Order"
        };
        public ControlNodeExampleScene() : base()
        {
            Title = "UI System 2.0 Example";

            container = new()
            {
                Anchor = new(0.5f),
                Stretch = new(0.98f, 0.7f)
            };
            container.MouseFilter = MouseFilter.Pass;
            
            navigator = new();

            buttonContainer = new ControlNodeContainer
            {
                Anchor = new AnchorPoint(0.5f),
                Stretch = new Vector2(0.8f, 0.6f)
            };
            
            // var startButton = new ControlNodeButton("Nav Start", new(0.5f, 0.5f), new Vector2(0.98f, 0.95f));
            // optionButton = new ControlNodeButton("Active Test", new(0.5f, 0.5f), new Vector2(0.98f, 0.95f));
            // var quitButton = new ControlNodeButton("Nav End", new(0.5f, 0.5f), new Vector2(0.98f, 0.95f));

            // buttonContainer.AddChild(startButton);
            // buttonContainer.AddChild(optionButton);
            // buttonContainer.AddChild(quitButton);
            
            for (var i = 0; i < 64; i++)
            {
                var button = new ControlNodeButton($"B{i+1}", new(0.5f, 0.5f), new Vector2(0.98f, 0.95f));
                buttonContainer.AddChild(button);
                button.OnPressedChanged += OnGenericButtonPressed;
            }
            
            // buttonContainer.Type = ControlNodeContainer.ContainerType.Grid;
            buttonContainer.Gap = new Vector2(0.025f, 0.025f);
            buttonContainer.DisplayIndex = 0;
            buttonContainer.NavigationStep = 1;
            buttonContainer.Grid = GridStyles[0](); //  new(5, 5, false, true, false);}
            
            container.AddChild(buttonContainer);

            var titleLabel = new ControlNodeLabel("UI System 2.0", new AnchorPoint(0.5f, 0f), new Vector2(0.8f, 0.15f));
            container.AddChild(titleLabel);

            var infoBox = new ControlNodeTexBox(
                "This is an info box about the new ui system. " +
                "It improves many long standing issues, simplifies the usage while still being versatile. " +
                "It is loosely based on Godots Control system.", 
                new AnchorPoint(0.02f, 0.98f), new Vector2(0.6f, 0.15f));
            container.AddChild(infoBox);

            var resetButton = new ControlNodeButton("Reset", new AnchorPoint(0.95f, 0.95f), new Vector2(0.2f, 0.1f));
            container.AddChild(resetButton);
            
            navigator.AddNode(container);
            navigator.StartNavigation();

            
            // startButton.OnPressedChanged += OnStartButtonPressedChanged;
            // optionButton.OnPressedChanged += OnOptionButtonPressedChanged;
            // quitButton.OnPressedChanged += OnQuitButtonPressedChanged;
            resetButton.OnPressedChanged += OnResetButtonPressedChanged;


            var cycleGridStylesKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var cycleGridStylesGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            cycleGridStyles = new InputAction(cycleGridStylesKB, cycleGridStylesGp);
            
            var resetGridStylesKB = new InputTypeKeyboardButton(ShapeKeyboardButton.R);
            var resetGridStylesGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            resetGridStyles = new InputAction(resetGridStylesKB, resetGridStylesGp);
        }

        // private void OnStartButtonPressedChanged(ControlNode node, bool value)
        // {
        //     if(!value) navigator.StartNavigation();
        // }
        // private void OnOptionButtonPressedChanged(ControlNode node, bool value)
        // {
        //     // if (!value) node.Visible = false;
        //     if (!value) node.Active = false;
        // }
        // private void OnQuitButtonPressedChanged(ControlNode node, bool value)
        // {
        //     if(!value) navigator.EndNavigation();
        // }

        private void OnGenericButtonPressed(ControlNode node, bool value)
        {
            Console.WriteLine($"---------------------------Generic Button Pressed Changed {value} | {container.ChildCount}");
            if (!value)
            {
                var prev = buttonContainer.GetPreviousChild(node);
                if (prev != null)
                {
                    prev.Active = false;
                    Console.WriteLine("---------------------------Prev set active false");
                }
                
                var next = buttonContainer.GetNextChild(node);
                if (next != null)
                {
                    next.Active = true;
                    Console.WriteLine("---------------------------Next set active true");
                }
            }
        }
        private void OnResetButtonPressedChanged(ControlNode node, bool value)
        {
            if (!value)
            {
                foreach (var child in buttonContainer.GetChildrenEnumerable)
                {
                    child.Active = true;
                }
            }
            
            // if (!value) optionButton.Active = true;
        }
        
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {

            if (container.MouseInside)
            {
                
                if (ShapeMouseWheelAxis.VERTICAL.GetInputState().AxisRaw > 0)
                {
                    navigator.SelectPrevious(buttonContainer.Grid);
                }
                
                else if (ShapeMouseWheelAxis.VERTICAL.GetInputState().AxisRaw < 0)
                {
                    navigator.SelectNext(buttonContainer.Grid);
                }
            }
            
            var gamepad = GAMELOOP.CurGamepad;
            
            cycleGridStyles.Gamepad = gamepad;
            cycleGridStyles.Update(time.Delta);
            
            resetGridStyles.Gamepad = gamepad;
            resetGridStyles.Update(time.Delta);

            if (cycleGridStyles.State.Pressed)
            {
                curGridStyle++;
                if (curGridStyle >= GridStyles.Count) curGridStyle = 0;
                buttonContainer.Grid = GridStyles[curGridStyle]();
            }

            if (resetGridStyles.State.Pressed)
            {
                curGridStyle = 0;
                buttonContainer.Grid = GridStyles[curGridStyle]();
            }
            
            // buttonContainer.Grid = new(5, 5, false, false, false);
            // buttonContainer.Grid = new(5, 1, true, false, false);
            // buttonContainer.Grid = new(1, 7, false, false, false);
            // buttonContainer.Grid = new(5, 8, false, true, false);
            // buttonContainer.Grid = new(3, 3, true, true, true);
            
            
            container.UpdateRect(ui.Area);
            container.Update(time.Delta, ui.MousePos);
            navigator.Update();
        }

        protected override void OnDrawGameUIExample(ScreenInfo gameUi)
        {
            container.Draw();
        }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
        }
        
        private void DrawInputDescription(Rect rect)
        {
            var rects = rect.SplitV(0.35f);
            var curDevice = ShapeInput.CurrentInputDeviceTypeNoMouse;
            string cycleText = cycleGridStyles.GetInputTypeDescription(curDevice, true, 1, false);
            string resetText = resetGridStyles.GetInputTypeDescription(curDevice, true, 1, false);
            string styleText = GridStyleNames[curGridStyle];
            
            
            string textTop = $"Cycle Grid Layouts {cycleText} | Reset Layout {resetText}";
            string textBottom = $"Current Style: {styleText}";
            
            textFont.FontSpacing = 1f;
            
            textFont.ColorRgba = Colors.Medium;
            textFont.DrawTextWrapNone(textTop, rects.top, new(0.5f));
            
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(textBottom, rects.bottom, new(0.5f));
        }
    }

}
