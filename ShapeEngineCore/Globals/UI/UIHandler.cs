using Raylib_CsLo;
using ShapeEngineCore.Globals.Input;
using ShapeEngineCore.Globals.Screen;
using System.Numerics;

namespace ShapeEngineCore.Globals.UI
{
    public enum Alignement
    {
        TOPLEFT = 0,
        TOPCENTER = 1,
        TOPRIGHT = 2,
        RIGHTCENTER = 3,
        BOTTOMRIGHT = 4,
        BOTTOMCENTER = 5,
        BOTTOMLEFT = 6,
        LEFTCENTER = 7,
        CENTER = 8,
    }
    public enum FontSize //good idea?
    {
        TINY = 30,
        SMALL = 35,
        MEDIUM = 40,
        LARGE = 50,
        XLARGE = 75,
        HUGE = 90,
        HEADER_S = 120,
        HEADER_M = 160,
        HEADER_L = 200,
        HEADER_XL = 250,
        HEADER_XXL = 350
    }

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
        private static Dictionary<string, Font> fonts = new Dictionary<string, Font>();
        private static Font defaultFont = GetFontDefault();


        public static void Initialize()
        {
            defaultFont = GetFontDefault();
        }

        public static float GetFontSizeScaled(FontSize fontSize)
        {
            return GetFontSizeScaled((float)fontSize);
        }
        public static float GetFontSizeScaled(int fontSize)
        {
            return GetFontSizeScaled(fontSize);
        }
        public static float GetFontSizeScaled(float fontSize)
        {
            return fontSize * ScreenHandler.UI_FACTOR;
        }

        public static Vector2 Scale(Vector2 v) { return v * ScreenHandler.UI_FACTOR; }
        public static float Scale(float f) { return f * ScreenHandler.UI_FACTOR; }
        public static int Scale(int i) { return (int)(i * ScreenHandler.UI_FACTOR); }

        public static float CalculateDynamicFontSize(string text, float size, float fontSizeBase = -1f, float fontSpacingBase = 1f)
        {
            if (fontSizeBase <= 0f) fontSizeBase = size;
            Vector2 textSize = GetTextSize(text, fontSizeBase, fontSpacingBase);
            float factor = size / textSize.X;
            return fontSizeBase * factor;
        }
        public static void AddFont(string name, string path, int fontSize = 100)
        {
            if (path == "" || fonts.ContainsKey(name)) return;
            unsafe
            {
                Font font = LoadFontEx(path, fontSize, (int*)0, 300);
                SetTextureFilter(font.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
                fonts.Add(name, font);
            }
        }

        public static Font GetFont(string name = "")
        {
            if (name == "" || !fonts.ContainsKey(name)) return defaultFont;
            return fonts[name];
        }
        public static void SetDefaultFont(string name)
        {
            if (!fonts.ContainsKey(name)) return;
            defaultFont = fonts[name];
        }

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

            if (selected != null)
            {
                if (InputMapHandler.IsPressed("UI Up"))
                {
                    CheckDirection(UINeighbors.NeighborDirection.TOP);
                }
                else if (InputMapHandler.IsPressed("UI Right"))
                {
                    CheckDirection(UINeighbors.NeighborDirection.RIGHT);
                }
                else if (InputMapHandler.IsPressed("UI Down"))
                {
                    CheckDirection(UINeighbors.NeighborDirection.BOTTOM);
                }
                else if (InputMapHandler.IsPressed("UI Left"))
                {
                    CheckDirection(UINeighbors.NeighborDirection.LEFT);
                }
            }

        }


        public static Vector2 GetAlignementVector(Alignement alignement)
        {
            switch (alignement)
            {
                case Alignement.TOPLEFT: return new(0.0f, 0.0f);
                case Alignement.TOPCENTER: return new(0.5f, 0.0f);
                case Alignement.TOPRIGHT: return new(1.0f, 0.0f);
                case Alignement.RIGHTCENTER: return new(1.0f, 0.5f);
                case Alignement.BOTTOMRIGHT: return new(1.0f, 1.0f);
                case Alignement.BOTTOMCENTER: return new(0.5f, 1.0f);
                case Alignement.BOTTOMLEFT: return new(0.0f, 1.0f);
                case Alignement.LEFTCENTER: return new(0.0f, 0.5f);
                case Alignement.CENTER: return new(0.5f, 0.5f);
                default: return new(0.5f, 0.5f);
            }
        }

        public static Vector2 GetTextSize(string text, float fontSize, float fontSpacing)
        {
            return MeasureTextEx(GetFont(), text, fontSize, fontSpacing);
        }
        //rot is in degrees
        public static void DrawTextAlignedPro(string text, Vector2 pos, float rot, float fontSize, float fontSpacing, Color color, Font font, Alignement alignement = Alignement.CENTER)
        {
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = GetAlignementVector(alignement) * fontDimensions;
            Vector2 textPosition = pos;// - Vec.Rotate(originOffset, rot);
            DrawTextPro(font, text, textPosition, originOffset, rot, fontSize, fontSpacing, color);
        }
        public static void DrawTextAlignedPro(string text, Vector2 pos, float rot, float fontSize, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro(text, pos, rot, fontSize, fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAlignedPro(string text, Vector2 pos, float rot, float fontSize, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro(text, pos, rot, fontSize, fontSpacing, color, GetFont(fontName), alignement);
        }
        public static void DrawTextAlignedPro(string text, Vector2 pos, float rot, FontSize fontSize, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro(text, pos, rot, GetFontSizeScaled(fontSize), Scale(fontSpacing), color, GetFont(), alignement);
        }
        public static void DrawTextAlignedPro(string text, Vector2 pos, float rot, FontSize fontSize, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro(text, pos, rot, GetFontSizeScaled(fontSize), Scale(fontSpacing), color, GetFont(fontName), alignement);
        }
        public static void DrawTextAligned(string text, Vector2 pos, float fontSize, float fontSpacing, Color color, Font font, Alignement alignement = Alignement.CENTER)
        {
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            DrawTextEx(font, text, pos - GetAlignementVector(alignement) * fontDimensions, fontSize, fontSpacing, color);
        }
        public static void DrawTextAligned(string text, Vector2 pos, float fontSize, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned(text, pos, fontSize, fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAligned(string text, Vector2 pos, float fontSize, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned(text, pos, fontSize, fontSpacing, color, GetFont(fontName), alignement);
        }

        public static void DrawTextAligned(string text, Vector2 pos, FontSize fontSize, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned(text, pos, GetFontSizeScaled(fontSize), Scale(fontSpacing), color, GetFont(), alignement);
        }
        public static void DrawTextAligned(string text, Vector2 pos, FontSize fontSize, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned(text, pos, GetFontSizeScaled(fontSize), Scale(fontSpacing), color, GetFont(fontName), alignement);
        }

        public static void DrawBar(Vector2 topLeft, Vector2 size, float f, Color barColor, Color bgColor, BarType barType = BarType.LEFTRIGHT)
        {
            Rectangle barRect = new(topLeft.X, topLeft.Y, size.X, size.Y);
            DrawBar(barRect, f, barColor, bgColor, barType);
        }
        public static void DrawBar(Rectangle rect, float f, Color barColor, Color bgColor, BarType barType = BarType.LEFTRIGHT)
        {
            Rectangle original = rect;
            switch (barType)
            {
                case BarType.LEFTRIGHT:
                    rect.width *= f;
                    break;
                case BarType.RIGHTLEFT:
                    rect.X += rect.width * (1.0f - f);
                    rect.width *= f;
                    break;
                case BarType.TOPBOTTOM:
                    rect.height *= f;
                    break;
                case BarType.BOTTOMTOP:
                    rect.Y += rect.height * (1.0f - f);
                    rect.height *= f;
                    break;
                default:
                    rect.width *= f;
                    break;
            }
            DrawRectangleRec(original, bgColor);
            DrawRectangleRec(rect, barColor);
        }
        public static void DrawBar(Vector2 topLeft, Vector2 size, float f, Color barColor, Color bgColor, Color outlineColor, float outlineSize, BarType barType = BarType.LEFTRIGHT)
        {
            Rectangle barRect = new(topLeft.X, topLeft.Y, size.X, size.Y);
            DrawBar(barRect, f, barColor, bgColor, outlineColor, outlineSize, barType);
        }
        public static void DrawBar(Rectangle rect, float f, Color barColor, Color bgColor, Color outlineColor, float outlineSize, BarType barType = BarType.LEFTRIGHT)
        {
            Rectangle original = rect;
            switch (barType)
            {
                case BarType.LEFTRIGHT:
                    rect.width *= f;
                    break;
                case BarType.RIGHTLEFT:
                    rect.X += rect.width * (1.0f - f);
                    break;
                case BarType.TOPBOTTOM:
                    rect.height *= f;
                    break;
                case BarType.BOTTOMTOP:
                    rect.Y += rect.height * (1.0f - f);
                    break;
                default:
                    rect.width *= f;
                    break;
            }
            DrawRectangleRec(original, bgColor);
            DrawRectangleRec(rect, barColor);
            if (outlineSize > 0f) DrawRectangleLinesEx(original, outlineSize, outlineColor);
        }
        public static void Close()
        {
            foreach (Font font in fonts.Values)
            {
                UnloadFont(font);
            }
            fonts.Clear();
            register.Clear();
            selected = null;
        }


        private static void CheckDirection(UINeighbors.NeighborDirection dir)
        {
            if (selected == null) return;
            var neighbor = selected.GoToNeighbor(dir);
            if (neighbor != null) selected = neighbor;
            else if (selected.IsAutomaticDetectionDirectionEnabled(dir))
            {
                var closest = FindNeighbor(selected, dir);
                if (closest != null)
                {
                    selected.Deselect();
                    selected = closest;
                    selected.Select();
                }
            }
        }
        private static UIElementSelectable? FindNeighbor(UIElementSelectable current, UINeighbors.NeighborDirection dir)
        {
            if (current == null) return null;
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

