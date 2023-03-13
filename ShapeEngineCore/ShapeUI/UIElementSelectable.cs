using System.Numerics;
using Raylib_CsLo;
using ShapeInput;
using ShapeAudio;

namespace ShapeUI
{

    public struct UISelectionColors
    {
        public Color baseColor = DARKBLUE;
        public Color hoveredColor = SKYBLUE;
        public Color selectedColor = WHITE;
        public Color pressedColor = WHITE;
        public Color disabledColor = DARKGRAY;

        public UISelectionColors() { }
        public UISelectionColors(Color baseColor, Color hoveredColor, Color selectedColor, Color pressedColor, Color disabledColor)
        {
            this.baseColor = baseColor;
            this.hoveredColor = hoveredColor;
            this.selectedColor = selectedColor;
            this.pressedColor = pressedColor;
            this.disabledColor = disabledColor;
        }
    }

    public class UIElementSelectable : UIElement
    {
        protected bool hovered = false;
        protected bool selected = false;
        protected bool disabled = false;

        protected bool pressed = false;
        protected bool clicked = false;

        protected bool mousePressed = false;
        protected bool mouseClicked = false;

        protected UINeighbors neighbors = new();
        protected string shortcut = "";


        public bool Clicked() { return clicked || mouseClicked; }
        public bool Pressed() { return pressed || mousePressed; }
        protected virtual bool CheckPressed()
        {
            return selected && InputHandler.IsDown(UIHandler.playerSlot, UIHandler.inputSelect);
        }
        protected virtual bool CheckClicked()
        {
            return selected && InputHandler.IsReleased(UIHandler.playerSlot, UIHandler.inputSelect);
        }
        protected virtual bool CheckMousePressed()
        {
            return hovered && InputHandler.IsDown(UIHandler.playerSlot, UIHandler.inputSelectMouse);
        }
        protected virtual bool CheckMouseClicked()
        {
            return hovered && InputHandler.IsReleased(UIHandler.playerSlot, UIHandler.inputSelectMouse);
        }
        protected virtual bool IsShortcutDown()
        {
            if (shortcut == "") return false;
            return InputHandler.IsDown(UIHandler.playerSlot, shortcut);
        }
        protected virtual bool IsShortcutReleased()
        {
            if (shortcut == "") return false;
            return InputHandler.IsReleased(UIHandler.playerSlot, shortcut);
        }

        public virtual bool IsAutomaticDetectionDirectionEnabled(UINeighbors.NeighborDirection dir) { return true; }
        
        public void AddShortcut(string newShortcut)
        {
            shortcut = newShortcut;
        }
        public void RemoveShortcut()
        {
            shortcut = "";
        }
        public void SetNeighbor(UIElementSelectable neighbor, UINeighbors.NeighborDirection dir) { neighbors.SetNeighbor(neighbor, dir); }
        public bool IsSelected() { return selected; }
        public void Disable() { disabled = true; }
        public void Enable() { disabled = false; }
        public bool IsDisabled() { return disabled; }
        public void Select()
        {
            if (selected) return;
            selected = true;
            SelectedChanged(true);
            //AudioHandler.PlaySFX("button hover");
        }
        public void Deselect()
        {
            if (!selected) return;
            selected = false;
            SelectedChanged(false);
        }
        public override void Update(float dt, Vector2 mousePosUI)
        {
            clicked = false;
            mouseClicked = false;
            if (disabled) return;
            
            bool shortcutPressed = IsShortcutDown();
            bool shortcutReleased = IsShortcutReleased();
            
            var prevPressed = pressed;
            var prevMousePressed = mousePressed;

            if (shortcutPressed || shortcutReleased)
            {
                clicked = shortcutReleased;
                pressed = shortcutPressed;
            }
            else
            {
                var prevHovered = hovered;
                hovered = IsPointInside(mousePosUI);
                if (hovered && !prevHovered)
                {
                    HoveredChanged(true);
                    PlayHoveredSound();
                }
                else if (!hovered && prevHovered) { HoveredChanged(false); }
               
                if (hovered)
                {
                    if (mousePressed) mouseClicked = CheckMouseClicked();
                    mousePressed = CheckMousePressed();
                }
                else
                {
                    mousePressed = false;
                }

                if (selected)
                {
                    if (pressed) clicked = CheckClicked();
                    pressed = CheckPressed();
                }
                else
                {
                    pressed = false;
                }
            }

            bool pressedChanged = pressed && !prevPressed;
            bool mousePressedChanged = mousePressed && !prevMousePressed;

            if (pressedChanged || mousePressedChanged)
            {
                if(pressedChanged) PressedChanged(true);
                if(mousePressedChanged) MousePressedChanged(true);

                PlayClickedSound();
            }
            else
            {
                if (prevPressed) PressedChanged(false);
                if (prevMousePressed) MousePressedChanged(false);

            }

            if (clicked) WasClicked();
            if (mouseClicked) WasMouseClicked();
        }


        public virtual void PlayHoveredSound()
        {
            //AudioHandler.PlaySFX("button hover");
        }
        public virtual void PlayClickedSound()
        {
            //AudioHandler.PlaySFX("button click");
        }
        public virtual void WasClicked() { }
        public virtual void WasMouseClicked() { }
        public virtual void MousePressedChanged(bool pressed) { }
        public virtual void PressedChanged(bool pressed) { }
        public virtual void HoveredChanged(bool hovered) { }
        public virtual void SelectedChanged(bool selected) { }



        
        public UIElementSelectable? CheckDirection(UINeighbors.NeighborDirection dir, List<UIElementSelectable> register)
        {
            var neighbor = GoToNeighbor(dir);
            if (neighbor != null) return neighbor;
            else if (IsAutomaticDetectionDirectionEnabled(dir))
            {
                var closest = FindNeighbor(dir, register);
                if (closest != null)
                {
                    Deselect();
                    closest.Select();
                    return closest;
                }
            }
            return null;
        }
        private UIElementSelectable? FindNeighbor(UINeighbors.NeighborDirection dir, List<UIElementSelectable> register)
        {
            UIElementSelectable current = this;
            if (register == null || register.Count <= 0) return null;
            List<UIElementSelectable> neighbors = register.FindAll(e => e != current && !e.IsDisabled());// && e.IsAutomaticDetectionDirectionEnabled(dir));
            if (neighbors.Count <= 0) return null;
            if (neighbors.Count == 1)
            {
                if (current.CheckNeighborDistance(neighbors[0], dir) < float.PositiveInfinity) return neighbors[0];
                else return null;
            }
            int closestIndex = -1;
            float closestDistance = float.PositiveInfinity;
            for (int i = 0; i < neighbors.Count; i++)
            {
                float dis = current.CheckNeighborDistance(neighbors[i], dir);
                if (dis < closestDistance)
                {
                    closestDistance = dis;
                    closestIndex = i;
                }
            }

            if (closestIndex < 0 || closestIndex >= neighbors.Count) return null;
            return neighbors[closestIndex];
        }
        private Vector2 GetDirectionPosition(UINeighbors.NeighborDirection dir)
        {
            Rectangle self = GetRect(new(0f));
            switch (dir)
            {
                case UINeighbors.NeighborDirection.TOP:
                    return new(self.X + self.width / 2, self.Y + self.height);//bottom
                case UINeighbors.NeighborDirection.RIGHT:
                    return new(self.X, self.Y + self.height / 2); //left
                case UINeighbors.NeighborDirection.BOTTOM:
                    return new(self.X + self.width / 2, self.Y);//top
                case UINeighbors.NeighborDirection.LEFT:
                    return new(self.X + self.width, self.Y + self.height / 2);//right
                default: return new(self.X + self.width / 2, self.Y + self.height / 2); //center
            }
        }
        private float CheckNeighborDistance(UIElementSelectable neighbor, UINeighbors.NeighborDirection dir)
        {
            if (neighbor == null) return float.PositiveInfinity;
            Vector2 pos = GetDirectionPosition(dir);
            Vector2 otherPos = neighbor.GetDirectionPosition(dir);
            switch (dir)
            {
                case UINeighbors.NeighborDirection.TOP:
                    if (pos.Y - otherPos.Y > 0)//neighbor is really on top
                    {
                        return (otherPos - pos).LengthSquared();
                    }
                    return float.PositiveInfinity;
                case UINeighbors.NeighborDirection.RIGHT:
                    if (otherPos.X - pos.X > 0)
                    {
                        return (otherPos - pos).LengthSquared();
                    }
                    return float.PositiveInfinity;
                case UINeighbors.NeighborDirection.BOTTOM:
                    if (otherPos.Y - pos.Y > 0)
                    {
                        return (otherPos - pos).LengthSquared();
                    }
                    return float.PositiveInfinity;
                case UINeighbors.NeighborDirection.LEFT:
                    if (pos.X - otherPos.X > 0)
                    {
                        return (otherPos - pos).LengthSquared();
                    }
                    return float.PositiveInfinity;
                default:
                    return float.PositiveInfinity;
            }

        }
        private UIElementSelectable? GoToNeighbor(UINeighbors.NeighborDirection dir) { return neighbors.SelectNeighbor(dir, this); }


    }
}
