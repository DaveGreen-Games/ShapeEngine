
using ShapeInput;

namespace ShapeUI
{
    public enum BarType
    {
        LEFTRIGHT = 0,
        RIGHTLEFT = 1,
        TOPBOTTOM = 2,
        BOTTOMTOP = 3
    }
    

    public static class UIHandler
    {
        private static List<UIElementSelectable> register = new();
        private static UIElementSelectable? selected = null;
        

        private static float dirInputTimer = -1f;
        private static float dirInputInterval = 0.25f;
        private static UINeighbors.NeighborDirection lastDir = UINeighbors.NeighborDirection.NONE;

        public static int InputLeft = InputHandler.UI_Left;
        public static int InputUp = InputHandler.UI_Up;
        public static int InputRight = InputHandler.UI_Right;
        public static int InputDown = InputHandler.UI_Down;
        public static int InputSelect = InputHandler.UI_Select;
        public static int InputSelectMouse = InputHandler.UI_SelectMouse;
        public static int playerSlot = -1;


        public delegate void DirectionInput(UINeighbors.NeighborDirection dir, UIElement? selected, UIElement? nextSelected);
        public static event DirectionInput? OnDirectionInput;

        //public delegate void SelectedItemUnregistered(UIElement selected);
        public static event Action<UIElement>? OnSelectedItemUnregistered;

        public static bool InputDisabled { get; set; } = false;
        public static void SetDirInputInterval(float newInterval) { dirInputInterval = newInterval; }
        public static void ClearRegister() { register.Clear(); }
        public static void RegisterUIElement(UIElementSelectable element)
        {
            if (register.Contains(element)) return;
            register.Add(element);
        }
        public static void UnregisterUIElement(UIElementSelectable element)
        {
            if (register == null || register.Count <= 0) return;
            register.Remove(element);
            if (selected != null && selected == element)
            {
                DeselectUIElement();
                OnSelectedItemUnregistered?.Invoke(element);
            }
            else if (element.IsSelected())
            {
                element.Deselect();
                OnSelectedItemUnregistered?.Invoke(element);
            }
        }
        public static bool DeselectUIElement()
        {
            if(selected != null)
            {
                selected.Deselect();
                selected = null;
                return true;
            }
            return false;
        }
        public static bool SelectUIElement(UIElementSelectable element)
        {
            if (element != null && element.IsDisabled()) return false;
            if (selected != null) selected.Deselect();
            selected = element;
            if (selected != null) selected.Select();
            return true;
        }
        public static void Update(float dt)
        {
            if (selected != null && !InputDisabled)
            {
                UIElementSelectable? newSelected = null;

                if (dirInputTimer > 0f)
                {
                    int input = GetDirInput(lastDir);
                    if (InputHandler.IsDown(playerSlot, input))
                    {
                        dirInputTimer -= dt;
                        if (dirInputTimer <= 0f) dirInputTimer = 0f;
                    }
                    else
                    {
                        dirInputTimer = -1f;
                        lastDir = UINeighbors.NeighborDirection.NONE;
                    }
                }

                if (InputHandler.IsPressed(playerSlot, InputUp) || (lastDir == UINeighbors.NeighborDirection.TOP && dirInputTimer == 0f))
                {
                    newSelected = selected.CheckDirection(UINeighbors.NeighborDirection.TOP, register);
                    lastDir = UINeighbors.NeighborDirection.TOP;
                    if(dirInputInterval > 0f) dirInputTimer = dirInputInterval - dt;
                    OnDirectionInput?.Invoke(lastDir, selected, newSelected);
                }
                else if (InputHandler.IsPressed(playerSlot, InputRight) || (lastDir == UINeighbors.NeighborDirection.RIGHT && dirInputTimer == 0f))
                {
                    newSelected = selected.CheckDirection(UINeighbors.NeighborDirection.RIGHT, register);
                    lastDir = UINeighbors.NeighborDirection.RIGHT;
                    if (dirInputInterval > 0f) dirInputTimer = dirInputInterval - dt;
                    OnDirectionInput?.Invoke(lastDir, selected, newSelected);
                }
                else if (InputHandler.IsPressed(playerSlot, InputDown) || (lastDir == UINeighbors.NeighborDirection.BOTTOM && dirInputTimer == 0f))
                {
                    newSelected = selected.CheckDirection(UINeighbors.NeighborDirection.BOTTOM, register);
                    lastDir = UINeighbors.NeighborDirection.BOTTOM;
                    if (dirInputInterval > 0f) dirInputTimer = dirInputInterval - dt;
                    OnDirectionInput?.Invoke(lastDir, selected, newSelected);
                }
                else if (InputHandler.IsPressed(playerSlot, InputLeft) || (lastDir == UINeighbors.NeighborDirection.LEFT && dirInputTimer == 0f))
                {
                    newSelected = selected.CheckDirection(UINeighbors.NeighborDirection.LEFT, register);
                    lastDir = UINeighbors.NeighborDirection.LEFT;
                    if (dirInputInterval > 0f) dirInputTimer = dirInputInterval - dt;
                    OnDirectionInput?.Invoke(lastDir, selected, newSelected);
                }



                if (newSelected != null) selected = newSelected;
            }
        }
        public static void Close()
        {
            register.Clear();
            selected = null;
        }
        private static int GetDirInput(UINeighbors.NeighborDirection dir)
        {
            switch (dir)
            {
                case UINeighbors.NeighborDirection.NONE: return -1;
                case UINeighbors.NeighborDirection.TOP: return InputUp;
                case UINeighbors.NeighborDirection.RIGHT: return InputRight;
                case UINeighbors.NeighborDirection.BOTTOM: return InputDown;
                case UINeighbors.NeighborDirection.LEFT: return InputLeft;
                default: return -1;
            }
        }
    }

}




/*public class ButtonStyle
{
    public virtual void DrawDefault(Rectangle rect) { DrawRectangleRec(rect, GRAY); }
    public virtual void DrawPressed(Rectangle rect)
    {
        DrawRectangleRec(rect, GREEN);
    }
    public virtual void DrawHovered(Rectangle rect)
    {
        DrawRectangleRec(rect, WHITE);
    }
}
public static class UI
{
    private static Vector2 mousePos = new();

    public static void UpdateMousePos(Vector2 newPos)
    {
        mousePos.X = newPos.X;
        mousePos.Y = newPos.Y;
    }
    public static void UpdateMousePos(int x, int y)
    {
        mousePos.X = x;
        mousePos.Y = y;
    }
    public static bool Button(Rectangle rec, ButtonStyle style)
    {
        bool inside = CheckCollisionPointRec(mousePos, rec);
        bool clicked = false;
        if (inside)
        {
            if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                style.DrawPressed(rec);
            }
            else if(IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            {
                clicked = true;
                style.DrawHovered(rec);
            }
            else
            {
                style.DrawHovered(rec);
            }
        }
        else
        {
            style.DrawDefault(rec);
        }


        return clicked;
    }
}
*/

