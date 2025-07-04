﻿using System.Numerics;
using Examples.Scenes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.StripedDrawingDef;
using ShapeEngine.UI;
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


        private float pressDelayTimer = 0f;
        private const float PressDelay = 0.1f;
        
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
            if (!Selected) return new();
            if (Scene == null) return new();
            
            var downState = GAMELOOP.InputActionUIDown.Consume();
            var upState = GAMELOOP.InputActionUIUp.Consume();
            // var leftState = GAMELOOP.InputActionUILeft.Consume();
            // var rightState = GAMELOOP.InputActionUIRight.Consume();
            // Console.WriteLine($"Button {Scene.Title} - Down: {downState.Consumed}, Up: {upState.Consumed}, Left: {leftState.Consumed}, Right: {rightState.Consumed}");
            
            if (inputCooldownTimer > 0f)
            {
                if (downState is {Consumed:false, Released:true} ||
                    upState is {Consumed:false, Released:true})
                    // leftState is {Consumed:false, Released:true} ||
                    // rightState is {Consumed:false, Released:true})
                {
                    inputCooldownTimer = 0f;
                }
                else return new();
            }
            
            // var hor = 0;
            var vert = 0;
            // if (leftState is {Consumed:false, Down:true}) hor = -1;
            // else if (rightState is {Consumed:false, Down:true}) hor = 1;
            
            if (upState is {Consumed:false, Down:true}) vert = -1;
            else if (downState is {Consumed:false, Down:true}) vert = 1;
            return new(0, vert);
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
                // GAMELOOP.GoToScene(Scene);
                if (PressDelay > 0f)
                {
                    pressDelayTimer = PressDelay;
                }
                else
                {
                    GAMELOOP.GoToScene(Scene);
                }
                
            }
        }

        protected override void OnUpdate(float dt, Vector2 mousePos, bool mousePosValid)
        {
            base.OnUpdate(dt, mousePos, mousePosValid);
            if (inputCooldownTimer > 0)
            {
                inputCooldownTimer -= dt;
            }

            if (pressDelayTimer > 0)
            {
                pressDelayTimer -= dt;
                if (pressDelayTimer <= 0f)
                {
                    if (Scene != null) GAMELOOP.GoToScene(Scene);
                    pressDelayTimer = 0f;
                }
            }
        }

        protected override void OnDraw()
        {
            if (!Active || Scene == null) return;
            
            var r = Rect;
            var text = Scene.Title;

            if (MouseInside)
            {
                var amount = Rect.Size.Min() * 0.25f;
                var outside = Rect.ChangeSize(amount, new AnchorPoint(0.5f, 0.5f));
                // outside.DrawLines(2f, Colors.Medium);
                
                var lineThickness = outside.Size.Min() * 0.02f;
                var spacing = lineThickness * 12f;
                var info = new LineDrawingInfo(lineThickness, Colors.Dark, LineCapType.Capped, 6);
                outside.DrawStriped(spacing, 35f, info);
            }
            
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
            
            
        }
    }

}
