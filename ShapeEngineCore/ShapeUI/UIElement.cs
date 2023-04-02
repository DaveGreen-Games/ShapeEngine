using Raylib_CsLo;
using System.Numerics;

namespace ShapeUI
{
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




    public class UIMargins
    {
        public float top = 0f;
        public float right = 0f;
        public float bottom = 0f;
        public float left = 0f;

        public UIMargins() { }
        public UIMargins(float top, float right, float bottom, float left)
        {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }
        public UIMargins(Vector2 horizontal, Vector2 vertical)
        {
            this.left = horizontal.X;
            this.right = horizontal.Y;
            this.top = vertical.X;
            this.bottom = vertical.Y;
        }

        public Rectangle Apply(Rectangle rect)
        {
            Vector2 tl = new(rect.x, rect.y);
            Vector2 size = new(rect.width, rect.height);
            Vector2 br = tl + size;

            tl.X += size.X * left;
            tl.Y += size.Y * top;
            br.X -= size.X * right;
            br.Y -= size.Y * bottom;

            Vector2 finalTopLeft = new (MathF.Min(tl.X, br.X), MathF.Min(tl.Y, br.Y));
            Vector2 finalBottomRight = new(MathF.Max(tl.X, br.X), MathF.Max(tl.Y, br.Y));
            return new
                (
                    finalTopLeft.X,
                    finalTopLeft.Y,
                    finalBottomRight.X - finalTopLeft.X,
                    finalBottomRight.Y - finalTopLeft.Y
                );
        }
    }
    public class UINavigator
    {
        private HashSet<UIElement2> register = new();
        
        public UIElement2? StartElement { get; protected set; } = null;
        public UIElement2? Selected { get; protected set; } = null;
        public float InputInterval { get; set; } = 0.25f;
        public bool InputDisabled { get; set; } = false;
        public UINeighbors2.NeighborDirection LastInputDirection { get; protected set; } = UINeighbors2.NeighborDirection.NONE;
        
        private float dirInputTimer = -1f;


        public UINavigator(params UIElement2[] elements)
        {
            RegisterElements(elements);
        }
        
        
        public void RegisterElements(params UIElement2[] elements)
        {
            if (elements.Length > 0)
            {
                if(Selected != null)
                {
                    Selected.Deselect();
                    Selected = null;
                }
                
                register.Clear();
                
                foreach (var element in elements) { element.Deselect(); }
                
                StartElement = elements[0];
                StartElement.Select();
                Selected = StartElement;

                register = elements.ToHashSet();
            }
            else
            {
                if (Selected != null)
                {
                    Selected.Deselect();
                    Selected = null;
                }
                StartElement = null;
                register.Clear();
            }
            
        }
        public void Reset()
        {
            if (register.Count > 0)
            {
                foreach (var element in register) { element.Deselect(); }
                if (StartElement != null)
                {
                    StartElement.Select();
                    Selected = StartElement;
                }
                else
                {

                    StartElement = register.First();
                    StartElement.Select();
                    Selected = StartElement;
                }
            }
        }
        public void Close()
        {
            register.Clear();
            Selected = null;
        }
        public void Update(float dt, UINeighbors2.NeighborDirection inputDirection)
        {
            if (Selected != null && !InputDisabled)
            {
                UIElement2? newSelected = null;

                if (dirInputTimer > 0f)
                {
                    if (LastInputDirection == inputDirection)
                    {
                        dirInputTimer -= dt;
                        if (dirInputTimer <= 0f) dirInputTimer = 0f;
                    }
                    else
                    {
                        dirInputTimer = -1f;
                        LastInputDirection = UINeighbors2.NeighborDirection.NONE;
                    }
                }

                if (inputDirection == UINeighbors2.NeighborDirection.TOP || (LastInputDirection == UINeighbors2.NeighborDirection.TOP && dirInputTimer == 0f))
                {
                    LastInputDirection = UINeighbors2.NeighborDirection.TOP;
                    newSelected = CheckDirection(Selected, inputDirection);
                    if (InputInterval > 0f) dirInputTimer = InputInterval - dt;
                    //OnDirectionInput?.Invoke(lastDir, Selected, newSelected);
                }
                else if (inputDirection == UINeighbors2.NeighborDirection.RIGHT || (LastInputDirection == UINeighbors2.NeighborDirection.RIGHT && dirInputTimer == 0f))
                {
                    LastInputDirection = UINeighbors2.NeighborDirection.RIGHT;
                    newSelected = CheckDirection(Selected, inputDirection);
                    if (InputInterval > 0f) dirInputTimer = InputInterval - dt;
                    //OnDirectionInput?.Invoke(lastDir, Selected, newSelected);
                }
                else if (inputDirection == UINeighbors2.NeighborDirection.BOTTOM || (LastInputDirection == UINeighbors2.NeighborDirection.BOTTOM && dirInputTimer == 0f))
                {
                    LastInputDirection = UINeighbors2.NeighborDirection.BOTTOM;
                    newSelected = CheckDirection(Selected, inputDirection);
                    if (InputInterval > 0f) dirInputTimer = InputInterval - dt;
                    //OnDirectionInput?.Invoke(lastDir, Selected, newSelected);
                }
                else if (inputDirection == UINeighbors2.NeighborDirection.LEFT || (LastInputDirection == UINeighbors2.NeighborDirection.LEFT && dirInputTimer == 0f))
                {
                    LastInputDirection = UINeighbors2.NeighborDirection.LEFT;
                    newSelected = CheckDirection(Selected, inputDirection);
                    if (InputInterval > 0f) dirInputTimer = InputInterval - dt;
                    //OnDirectionInput?.Invoke(lastDir, Selected, newSelected);
                }

                if (newSelected != null)
                {
                    Selected = newSelected;
                }
            }
        }


        protected UIElement2? CheckDirection(UIElement2 current, UINeighbors2.NeighborDirection dir)
        {
            var neighbor = current.Neighbors.GetNeighbor(dir); 
            if (neighbor != null) return neighbor;
            else
            {
                var closest = FindNeighbor(current, dir);
                if (closest != null)
                {
                    current.Deselect();
                    closest.Select();
                    return closest;
                }
            }
            return null;
        }
        protected UIElement2? FindNeighbor(UIElement2 current, UINeighbors2.NeighborDirection dir)
        {
            if (register == null || register.Count <= 0) return null;
            List<UIElement2> neighbors = register.ToList().FindAll(e => e != current && !e.Disabled);
            if (neighbors.Count <= 0) return null;
            if (neighbors.Count == 1)
            {
                if (CheckNeighborDistance(current, neighbors[0], dir) < float.PositiveInfinity) return neighbors[0];
                else return null;
            }
            int closestIndex = -1;
            float closestDistance = float.PositiveInfinity;
            for (int i = 0; i < neighbors.Count; i++)
            {
                float dis = CheckNeighborDistance(current, neighbors[i], dir);
                if (dis < closestDistance)
                {
                    closestDistance = dis;
                    closestIndex = i;
                }
            }

            if (closestIndex < 0 || closestIndex >= neighbors.Count) return null;
            return neighbors[closestIndex];
        }
        protected float CheckNeighborDistance(UIElement2 current, UIElement2 neighbor, UINeighbors2.NeighborDirection dir)
        {
            if (neighbor == null) return float.PositiveInfinity;
            Vector2 pos = GetDirectionPosition(current, dir);
            Vector2 otherPos = GetDirectionPosition(neighbor, dir);
            switch (dir)
            {
                case UINeighbors2.NeighborDirection.TOP:
                    if (pos.Y - otherPos.Y > 0)//neighbor is really on top
                    {
                        return (otherPos - pos).LengthSquared();
                    }
                    return float.PositiveInfinity;
                case UINeighbors2.NeighborDirection.RIGHT:
                    if (otherPos.X - pos.X > 0)
                    {
                        return (otherPos - pos).LengthSquared();
                    }
                    return float.PositiveInfinity;
                case UINeighbors2.NeighborDirection.BOTTOM:
                    if (otherPos.Y - pos.Y > 0)
                    {
                        return (otherPos - pos).LengthSquared();
                    }
                    return float.PositiveInfinity;
                case UINeighbors2.NeighborDirection.LEFT:
                    if (pos.X - otherPos.X > 0)
                    {
                        return (otherPos - pos).LengthSquared();
                    }
                    return float.PositiveInfinity;
                default:
                    return float.PositiveInfinity;
            }

        }
        protected Vector2 GetDirectionPosition(UIElement2 element, UINeighbors2.NeighborDirection dir)
        {
            Rectangle self = element.GetRect(new(0f));
            switch (dir)
            {
                case UINeighbors2.NeighborDirection.TOP:
                    return new(self.X + self.width / 2, self.Y + self.height);//bottom
                case UINeighbors2.NeighborDirection.RIGHT:
                    return new(self.X, self.Y + self.height / 2); //left
                case UINeighbors2.NeighborDirection.BOTTOM:
                    return new(self.X + self.width / 2, self.Y);//top
                case UINeighbors2.NeighborDirection.LEFT:
                    return new(self.X + self.width, self.Y + self.height / 2);//right
                default: return new(self.X + self.width / 2, self.Y + self.height / 2); //center
            }
        }
        
        //public event Action<UINeighbors2.NeighborDirection, UIElement, UIElement> SelectedChanged;

        //public delegate void DirectionInput(UINeighbors.NeighborDirection dir, UIElement? selected, UIElement? nextSelected);
        //public static event DirectionInput? OnDirectionInput;
        //public static event Action<UIElement>? OnSelectedItemUnregistered;
        //public UINavigator() { }
        //public void ClearRegister() { register.Clear(); }
        //public bool RegisterUIElement(UIElement2 element)
        //{
        //    return register.Add(element);
        //}
        //public bool UnregisterUIElement(UIElement2 element)
        //{
        //    bool removed = register.Remove(element);
        //    if (selected != null && selected == element)
        //    {
        //        DeselectUIElement();
        //        ///OnSelectedItemUnregistered?.Invoke(element);
        //    }
        //    else if (element.IsSelected())
        //    {
        //        element.Deselect();
        //        //OnSelectedItemUnregistered?.Invoke(element);
        //    }
        //    return removed;
        //}

        //public bool DeselectUIElement()
        //{
        //    if (Selected != null)
        //    {
        //        Selected.Deselect();
        //        Selected = null;
        //        return true;
        //    }
        //    return false;
        //}
        //public bool SelectUIElement(UIElement2 element)
        //{
        //    if (element != null && element.IsDisabled()) return false;
        //    if (Selected != null) Selected.Deselect();
        //    Selected = element;
        //    if (Selected != null) Selected.Select();
        //    return true;
        //}
    }
    public class UINeighbors2
    {
        public enum NeighborDirection { NONE = -1, TOP = 0, RIGHT = 1, BOTTOM = 2, LEFT = 3 };
        private UIElement2? top = null;
        private UIElement2? right = null;
        private UIElement2? bottom = null;
        private UIElement2? left = null;

        public UINeighbors2() { }
        //public UINeighbors2(UIElement2 top, UIElement2 right, UIElement2 bottom, UIElement2 left)
        //{
        //    this.top = top;
        //    this.right = right;
        //    this.bottom = bottom;
        //    this.left = left;
        //}


        //public UIElementSelectable? SelectNeighbor(NeighborDirection dir, UIElementSelectable current)
        //{
        //    if (!HasNeighbor(dir)) return null;
        //    var neighbor = GetNeighbor(dir);
        //    if (neighbor.IsDisabled()) return null;
        //    if (current != null) current.Deselect();
        //    neighbor.Select();
        //
        //    return neighbor;
        //}
        public bool HasNeighbor(NeighborDirection dir)
        {
            switch (dir)
            {
                case NeighborDirection.TOP: return top != null;
                case NeighborDirection.RIGHT: return right != null;
                case NeighborDirection.BOTTOM: return bottom != null;
                case NeighborDirection.LEFT: return left != null;
                default: return false;
            }
        }
        public UIElement2? GetNeighbor(NeighborDirection dir)
        {
            switch (dir)
            {
                case NeighborDirection.TOP: return top;
                case NeighborDirection.RIGHT: return right;
                case NeighborDirection.BOTTOM: return bottom;
                case NeighborDirection.LEFT: return left;
                default: return null;
            }
        }
        public void SetNeighbor(UIElement2 neighbor, NeighborDirection dir)
        {
            switch (dir)
            {
                case NeighborDirection.TOP:
                    top = neighbor;
                    break;
                case NeighborDirection.RIGHT:
                    right = neighbor;
                    break;
                case NeighborDirection.BOTTOM:
                    bottom = neighbor;
                    break;
                case NeighborDirection.LEFT:
                    left = neighbor;
                    break;
                default:
                    break;
            }
        }
    }
    public class UIElement2
    {
        protected Rectangle rect;
        protected Vector2 prevMousePos = new(0f);

        public float StretchFactor { get; set; } = 1f;
        public UIMargins Margins { get; set; } = new();
        public UINeighbors2 Neighbors { get; private set; } = new();
        public string Tooltip { get; set; } = "";

        public bool Selected { get; protected set; } = false;
        public bool Pressed { get; protected set; } = false;
        public bool Disabled { get; set; } = false;
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
        public virtual void Update(float dt, Vector2 mousePosUI)
        {
            if (!Disabled)
            {

                float disSq = (prevMousePos - mousePosUI).LengthSquared();
                if (disSq > MouseTolerance)
                {
                    bool mouseInside = IsPointInside(mousePosUI);
                    if (mouseInside && !Selected)
                    {
                        Selected = true;
                        SelectedChanged(true);
                    }
                    else if (!mouseInside && Selected)
                    {
                        Selected = false;
                        SelectedChanged(false);
                    }
                }

                bool prevPressed = Pressed;
                bool sp = CheckShortcutPressed();
                if (sp) Pressed = sp;
                else if (Selected) Pressed = CheckPressed();
                else Pressed = false;


                if (Pressed && !prevPressed) PressedChanged(true);
                else if (!Pressed && prevPressed) PressedChanged(false);
            }

            prevMousePos = mousePosUI;
        }
        public virtual void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {

        }


        public virtual void PressedChanged(bool pressed) { }
        public virtual void SelectedChanged(bool selected) { }
    }

}

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