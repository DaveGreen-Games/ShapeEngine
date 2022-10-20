using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals.Input;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals.Audio;
using Microsoft.Win32;

namespace ShapeEngineCore.Globals.UI
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
        protected bool pressed = false;
        protected bool clicked = false;
        protected bool disabled = false;
        protected UINeighbors neighbors = new();
        protected string shortcut = "";


        protected string inputLeft = "UI Left";
        protected string inputUp = "UI Up";
        protected string inputRight = "UI Right";
        protected string inputDown = "UI Down";
        protected string inputSelect = "UI Select";
        protected string inputSelectMouse = "UI Select Mouse";
        protected string inputCancel = "UI Cancel";
        protected string inputCancelMouse = "UI Cancel Mouse";

        public bool Clicked() { return clicked; }
        protected virtual bool CheckPressed()
        {
            return hovered && InputHandler.IsDown(-1, inputSelectMouse) || selected && InputHandler.IsDown(-1, inputSelect);
        }
        protected virtual bool CheckClicked()
        {
            return hovered && InputHandler.IsReleased(-1, inputSelectMouse) || selected && InputHandler.IsReleased(-1, inputSelect);
        }
        protected bool IsShortcutDown()
        {
            if (shortcut == "") return false;
            return InputHandler.IsDown(-1, shortcut);
            //if (keyboardShortcuts.Count <= 0 && mouseShortcuts.Count <= 0) return false;
            //foreach (var button in keyboardShortcuts)
            //{
            //    if (IsKeyDown(button)) return true;
            //}
            //foreach (var button in mouseShortcuts)
            //{
            //    if (IsMouseButtonDown(button)) return true;
            //}
            //return false;
        }
        protected bool IsShortcutReleased()
        {
            if (shortcut == "") return false;
            return InputHandler.IsReleased(-1, shortcut);
            //if (keyboardShortcuts.Count <= 0 && mouseShortcuts.Count <= 0) return false;
            //foreach (var button in keyboardShortcuts)
            //{
            //    if (IsKeyReleased(button)) return true;
            //}
            //foreach (var button in mouseShortcuts)
            //{
            //    if (IsMouseButtonReleased(button)) return true;
            //}
            //return false;
        }

        public virtual bool IsAutomaticDetectionDirectionEnabled(UINeighbors.NeighborDirection dir) { return true; }
        //public void AddShortcut(KeyboardKey key)
        //{
        //    if (keyboardShortcuts.Contains(key)) return;
        //    keyboardShortcuts.Add(key);
        //}
        public void AddShortcut(string newShortcut)
        {
            //if (mouseShortcuts.Contains(button)) return;
            //mouseShortcuts.Add(button);
            shortcut = newShortcut;
        }
        public void RemoveShortcut()
        {
            shortcut = "";
        }
        //public void RemoveShortcut(MouseButton button)
        //{
        //    mouseShortcuts.Remove(button);
        //}
        //public Rectangle GetRect() { return rect; }


        
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
            AudioHandler.PlaySFX("button hover");
        }
        public void Deselect()
        {
            if (!selected) return;
            selected = false;
            SelectedChanged(false);
        }
        public override void Update(float dt, Vector2 mousePosUI)
        {
            //if (Screen.HasMonitorChanged())
            //{
            //    MonitorHasChanged();
            //}

            clicked = false;
            if (disabled) return;

            bool shortcutPressed = IsShortcutDown();
            bool shortcutReleased = IsShortcutReleased();
            var prevPressed = pressed;
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
                    AudioHandler.PlaySFX("button hover");
                }
                else if (!hovered && prevHovered) { HoveredChanged(false); }
                if (hovered || selected)
                {
                    if (pressed) clicked = CheckClicked();
                    pressed = CheckPressed();
                }
                else
                {
                    pressed = false;

                }
            }
            if (pressed && !prevPressed)
            {
                PressedChanged(true);
                AudioHandler.PlaySFX("button click");
            }
            else if (!pressed && prevPressed) { PressedChanged(false); }

            if (clicked) WasClicked();
        }

        public virtual void WasClicked() { }
        public virtual void PressedChanged(bool pressed) { }
        public virtual void HoveredChanged(bool hovered) { }
        public virtual void SelectedChanged(bool selected) { }



        public UIElementSelectable? CheckDirectionInput(List<UIElementSelectable> register)
        {
            UIElementSelectable? selected = null;
            if (InputHandler.IsPressed(-1, inputUp))
            {
                selected = CheckDirection(UINeighbors.NeighborDirection.TOP, register);
            }
            else if (InputHandler.IsPressed(-1, inputRight))
            {
                selected = CheckDirection(UINeighbors.NeighborDirection.RIGHT, register);
            }
            else if (InputHandler.IsPressed(-1, inputDown))
            {
                selected = CheckDirection(UINeighbors.NeighborDirection.BOTTOM, register);
            }
            else if (InputHandler.IsPressed(-1, inputLeft))
            {
                selected = CheckDirection(UINeighbors.NeighborDirection.LEFT, register);
            }
            //else return this; // no input detected therefore element stays selected


            return selected;
        }
        private UIElementSelectable? CheckDirection(UINeighbors.NeighborDirection dir, List<UIElementSelectable> register)
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
        public Vector2 GetDirectionPosition(UINeighbors.NeighborDirection dir)
        {
            Rectangle self = GetRect();
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
