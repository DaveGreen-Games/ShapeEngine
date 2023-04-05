using Raylib_CsLo;
using ShapeInput;
using ShapeLib;
using System.Numerics;

namespace ShapeUI
{
    public class UIElement
    {
        public event Action<UIElement>? WasSelected;

        protected Rectangle rect;
        protected Vector2 prevMousePos = new(0f);

        public float StretchFactor { get; set; } = 1f;
        public UIMargins Margins { get; set; } = new();
        public UINeighbors Neighbors { get; private set; } = new();
        public string Tooltip { get; set; } = "";

        public bool Released { get; protected set; } = false;
        public bool Selected { get; internal set; } = false;
        public bool Pressed { get; protected set; } = false;

        protected bool disabled = false;
        public bool Disabled 
        {
            get { return disabled; }
            set 
            {
                disabled = value;
                if(disabled && Selected)
                {
                    Selected = false;
                    //WasDeselected?.Invoke(this);
                }
            }
        }
        
        //protected bool hidden = false;
        //public bool Hidden 
        //{
        //    get { return hidden; }
        //    set
        //    {
        //        hidden = value;
        //        if(hidden && Selected)
        //        {
        //            Selected = false;
        //            //WasDeselected?.Invoke(this);
        //        }
        //    }
        //}
        
        protected bool selectable = false;
        public bool Selectable
        {
            get { return selectable; }
            set
            {
                selectable = value;
                if (!selectable && Selected)
                {
                    Selected = false;
                    //WasDeselected?.Invoke(this);
                }
            }
        }
        
        public float MouseTolerance { get; set; } = 5f;

        
        protected virtual bool CheckPressed() { return false; }
        protected virtual bool CheckShortcutPressed() { return false; }
        
        public void Select()
        {
            if (Selected) return;
            Selected = true;
            SelectedChanged(true);
        }
        public void Deselect()
        {
            if (!Selected) return;
            Selected = false;
            SelectedChanged(false);
        }

        public Rectangle GetRect() { return GetRect(new(0f)); }
        public Rectangle GetRect(Vector2 alignement)
        {
            if (alignement.X == 0f && alignement.Y == 0f) return Margins.Apply(rect);
            else
            {
                Vector2 topLeft = new Vector2(rect.X, rect.Y);
                Vector2 size = GetSize();
                Vector2 offset = size * alignement;
                return Margins.Apply(new
                    (
                        topLeft.X + offset.X,
                        topLeft.Y + offset.Y,
                        size.X,
                        size.Y
                    ));
            }
        }
        public Vector2 GetPos(Vector2 alignement)
        {
            Rectangle rect = GetRect(new(0f));
            Vector2 topLeft = new Vector2(rect.X, rect.Y);
            Vector2 offset = GetSize() * alignement;
            return topLeft + offset;
        }
        public Vector2 GetSize()
        {
            Rectangle rect = GetRect(new(0f));
            return new(rect.width, rect.height);
        }
        public void UpdateRect(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            Vector2 offset = size * alignement;
            rect = new(pos.X - offset.X, pos.Y - offset.Y, size.X, size.Y);
        }
        public void UpdateRect(Rectangle rect, Vector2 alignement)
        {
            UpdateRect(new Vector2(rect.x, rect.y), new Vector2(rect.width, rect.height), alignement);
        }




        public bool IsPointInside(Vector2 uiPos)
        {
            return CheckCollisionPointRec(uiPos, GetRect(new(0f)));
        }
        public virtual void Draw()
        {
            
        }
        public virtual void Update(float dt, Vector2 mousePosUI)
        {
            Released = false;
            if (!Disabled && Selectable)
            {
                if(IsPointInside(prevMousePos))
                {
                    float disSq = (prevMousePos - mousePosUI).LengthSquared();
                    if (disSq > MouseTolerance)
                    {
                        bool mouseInside = IsPointInside(mousePosUI);
                        if (mouseInside && !Selected)
                        {
                            Selected = true;
                            SelectedChanged(true);
                            WasSelected?.Invoke(this);
                        }
                        else if (!mouseInside && Selected)
                        {
                            Selected = false;
                            SelectedChanged(false);
                        }
                    }
                }
                
                bool prevPressed = Pressed;
                if (CheckShortcutPressed()) Pressed = true;
                else if (Selected) Pressed = CheckPressed();
                else Pressed = false;


                if (Pressed && !prevPressed) PressedChanged(true);
                else if (!Pressed && prevPressed) 
                { 
                    PressedChanged(false); 
                    Released = true;
                }
            }

            prevMousePos = mousePosUI;
        }
        //public virtual void DrawElement() { }
        
        public virtual void PressedChanged(bool pressed) { }
        public virtual void SelectedChanged(bool selected) { }
    }

    public class TestButton : UIElement
    {
        public string Text { get; set; } = "Button";
        private Font font;
        private int shortcutID = -1;
        private int pressedID = -1;
        public TestButton(string text, Font font, int pressedID, int shortcutID = -1) 
        { 
            this.Text = text; 
            this.font = font; 
            this.shortcutID = shortcutID;
            this.pressedID = pressedID;
            this.selectable = true;
        }

        protected override bool CheckPressed()
        {
            if (pressedID < 0) return false;
            else return InputHandler.IsDown(0, pressedID);
        }

        protected override bool CheckShortcutPressed()
        {
            if (shortcutID < 0) return false;
            else return InputHandler.IsDown(0, shortcutID);
        }
        public override void Draw()
        {
            Rectangle r = GetRect();
            
            if (Disabled)
            {
                DrawRectangleRec(r, DARKGRAY);
                SDrawing.DrawTextAligned(Text, r, 1f, RED, font, new(0.5f));
            }
            else
            {
                //string t = Text;
                //if (CheckShortcutPressed()) t = "Shortcut";
                Color buttonColor = GRAY;
                Color textColor = DARKGREEN;
                if (Pressed)
                {
                    buttonColor = WHITE;
                    textColor = BLACK;
                }
                else if(Selected)
                {
                    buttonColor = LIGHTGRAY;
                    textColor = LIME;
                }
                DrawRectangleRec(r, buttonColor);
                SDrawing.DrawTextAligned(Text, r, 1f, textColor, font, new(0.5f));
            }
        }
    }
}


/*
    public class UIElement
    {
        protected Rectangle rect;

        protected float stretchFactor = 1f; //used for containers
        public UIMargins Margins { get; set; } = new();


        //public void SetMargins(float left = 0, float right = 0, float top = 0, float bottom = 0)
        //{
        //    Margins = new(top, right, bottom, left);
        //}
        //public UIMargins GetMargins() { return Margins; }
        public void SetStretchFactor(float newFactor) { stretchFactor = newFactor; }
        public float GetStretchFactor() { return stretchFactor; }
        public Rectangle GetRect(Vector2 alignement) 
        { 
            if(alignement.X == 0f && alignement.Y == 0f) return Margins.Apply(rect);
            else
            {
                Vector2 topLeft = new Vector2(rect.X, rect.Y);
                Vector2 size = GetSize();
                Vector2 offset = size * alignement;
                return Margins.Apply(new
                    (
                        topLeft.X + offset.X,
                        topLeft.Y + offset.Y,
                        size.X,
                        size.Y
                    ));
            }
        }
        

        public virtual void UpdateRect(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            //Vector2 align = UIHandler.GetAlignementVector(alignement);
            Vector2 offset = size * alignement;
            rect = new(pos.X - offset.X, pos.Y - offset.Y, size.X, size.Y);
        }
        public virtual void UpdateRect(Rectangle rect, Vector2 alignement) 
        { 
            UpdateRect(new Vector2(rect.x, rect.y), new Vector2(rect.width, rect.height), alignement);
        }
        public virtual float GetWidth() { return GetRect(new(0f)).width; }
        public virtual float GetHeight() { return GetRect(new(0f)).height; }
        public virtual Vector2 GetPos(Vector2 alignement) 
        {
            Rectangle rect = GetRect(new(0f));
            Vector2 topLeft = new Vector2(rect.X, rect.Y);
            Vector2 offset = GetSize() * alignement;
            return topLeft + offset;
        }
        public virtual Vector2 GetSize() 
        {
            Rectangle rect = GetRect(new(0f));
            return new(rect.width, rect.height); 
        }

        public bool IsPointInside(Vector2 uiPos)
        {
            return CheckCollisionPointRec(uiPos, GetRect(new(0f))); // GetScaledRect());
        }
        public virtual void Update(float dt, Vector2 mousePosUI)
        {

        }
        public virtual void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {

        }

    }
    */

/*
    public class UIElementSelectable2 : UIElement
    {
        protected bool hovered = false;
        protected bool selected = false;
        protected bool disabled = false;

        protected bool pressed = false;
        protected bool clicked = false;

        protected bool mousePressed = false;
        protected bool mouseClicked = false;

        protected UINeighbors neighbors = new();
        protected int shortcut = -1;


        public bool Clicked() { return clicked || mouseClicked; }
        public bool Pressed() { return pressed || mousePressed; }
        protected virtual bool CheckPressed()
        {
            return selected && InputHandler.IsDown(UIHandler.playerSlot, UIHandler.InputSelect);
        }
        protected virtual bool CheckClicked()
        {
            return selected && InputHandler.IsReleased(UIHandler.playerSlot, UIHandler.InputSelect);
        }
        protected virtual bool CheckMousePressed()
        {
            return hovered && InputHandler.IsDown(UIHandler.playerSlot, UIHandler.InputSelectMouse);
        }
        protected virtual bool CheckMouseClicked()
        {
            return hovered && InputHandler.IsReleased(UIHandler.playerSlot, UIHandler.InputSelectMouse);
        }
        protected virtual bool IsShortcutDown()
        {
            if (shortcut == -1) return false;
            return InputHandler.IsDown(UIHandler.playerSlot, shortcut);
        }
        protected virtual bool IsShortcutReleased()
        {
            if (shortcut == -1) return false;
            return InputHandler.IsReleased(UIHandler.playerSlot, shortcut);
        }

        public virtual bool IsAutomaticDetectionDirectionEnabled(UINeighbors.NeighborDirection dir) { return true; }

        public void AddShortcut(int shortCutID)
        {
            shortcut = shortCutID;
        }
        public void RemoveShortcut()
        {
            shortcut = -1;
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
                if (pressedChanged) PressedChanged(true);
                if (mousePressedChanged) MousePressedChanged(true);

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
        public virtual void WasMouseClicked() { }
        public virtual void MousePressedChanged(bool pressed) { }
        public virtual void HoveredChanged(bool hovered) { }

        public virtual void WasClicked() { }
        public virtual void PressedChanged(bool pressed) { }
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
    */