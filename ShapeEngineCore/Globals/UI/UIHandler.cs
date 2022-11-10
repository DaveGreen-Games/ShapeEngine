using Raylib_CsLo;
using ShapeEngineCore.Globals.Input;
using ShapeEngineCore.Globals.Screen;
using System.Numerics;
using ShapeEngineCore.Globals.Persistent;
using System.Runtime.CompilerServices;

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
    //public enum FontSize //good idea?
    //{
    //    TINY = 30,
    //    SMALL = 35,
    //    MEDIUM = 40,
    //    LARGE = 50,
    //    XLARGE = 75,
    //    HUGE = 90,
    //    HEADER_S = 120,
    //    HEADER_M = 160,
    //    HEADER_L = 200,
    //    HEADER_XL = 250,
    //    HEADER_XXL = 350
    //}

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

        private static Dictionary<string, float> fontSizes = new();

        static float dirInputTimer = -1f;
        static float dirInputInterval = 0.25f;
        static UINeighbors.NeighborDirection lastDir = UINeighbors.NeighborDirection.NONE;

        public static string inputLeft = "UI Left";
        public static string inputUp = "UI Up";
        public static string inputRight = "UI Right";
        public static string inputDown = "UI Down";
        public static string inputSelect = "UI Select";
        public static string inputSelectMouse = "UI Select Mouse";
        public static int playerSlot = -1;


        public delegate void DirectionInput(UINeighbors.NeighborDirection dir, UIElement? selected, UIElement? nextSelected);
        public static event DirectionInput? OnDirectionInput;

        public delegate void SelectedItemUnregistered(UIElement selected);
        public static event SelectedItemUnregistered? OnSelectedItemUnregistered;

        public static void Initialize()
        {
            defaultFont = GetFontDefault();
        }

        public static void SetDirInputInterval(float newInterval) { dirInputInterval = newInterval; }

        public static void AddFontSize(string name, float size)
        {
            if (fontSizes.ContainsKey(name)) fontSizes[name] = size;
            else fontSizes.Add(name, size);
        }
        public static void RemoveFontSize(string name)
        {
            fontSizes.Remove(name);
        }

        public static float GetFontSize(string name)
        {
            if (!fontSizes.ContainsKey(name)) return -1;
            else return fontSizes[name];
        }
        public static void AddFont(string name, string fileName, int fontSize = 100)
        {
            if (fileName == "" || fonts.ContainsKey(name)) return;
            Font font = ResourceManager.LoadFont(fileName, fontSize);
            
            SetTextureFilter(font.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
            fonts.Add(name, font);
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
            if (selected != null)
            {
                UIElementSelectable? newSelected = null;

                if (dirInputTimer > 0f)
                {
                    string input = GetDirInput(lastDir);
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

                if (InputHandler.IsPressed(playerSlot, inputUp) || (lastDir == UINeighbors.NeighborDirection.TOP && dirInputTimer == 0f))
                {
                    newSelected = selected.CheckDirection(UINeighbors.NeighborDirection.TOP, register);
                    lastDir = UINeighbors.NeighborDirection.TOP;
                    if(dirInputInterval > 0f) dirInputTimer = dirInputInterval - dt;
                    OnDirectionInput?.Invoke(lastDir, selected, newSelected);
                }
                else if (InputHandler.IsPressed(playerSlot, inputRight) || (lastDir == UINeighbors.NeighborDirection.RIGHT && dirInputTimer == 0f))
                {
                    newSelected = selected.CheckDirection(UINeighbors.NeighborDirection.RIGHT, register);
                    lastDir = UINeighbors.NeighborDirection.RIGHT;
                    if (dirInputInterval > 0f) dirInputTimer = dirInputInterval - dt;
                    OnDirectionInput?.Invoke(lastDir, selected, newSelected);
                }
                else if (InputHandler.IsPressed(playerSlot, inputDown) || (lastDir == UINeighbors.NeighborDirection.BOTTOM && dirInputTimer == 0f))
                {
                    newSelected = selected.CheckDirection(UINeighbors.NeighborDirection.BOTTOM, register);
                    lastDir = UINeighbors.NeighborDirection.BOTTOM;
                    if (dirInputInterval > 0f) dirInputTimer = dirInputInterval - dt;
                    OnDirectionInput?.Invoke(lastDir, selected, newSelected);
                }
                else if (InputHandler.IsPressed(playerSlot, inputLeft) || (lastDir == UINeighbors.NeighborDirection.LEFT && dirInputTimer == 0f))
                {
                    newSelected = selected.CheckDirection(UINeighbors.NeighborDirection.LEFT, register);
                    lastDir = UINeighbors.NeighborDirection.LEFT;
                    if (dirInputInterval > 0f) dirInputTimer = dirInputInterval - dt;
                    OnDirectionInput?.Invoke(lastDir, selected, newSelected);
                }



                if (newSelected != null) selected = newSelected;
            }
        }

        //private bool IsDown(UINeighbors.NeighborDirection dir)
        //{
        //    string input = GetDirInput(dir);
        //    if (input != "")
        //    {
        //        return InputHandler.IsDown(playerSlot, input);
        //    }
        //    return false;
        //}
        //private bool IsPressed(UINeighbors.NeighborDirection dir)
        //{
        //    string input = GetDirInput(dir);
        //    if (input != "")
        //    {
        //        return InputHandler.IsPressed(playerSlot, input);
        //    }
        //    return false;
        //}
        private static string GetDirInput(UINeighbors.NeighborDirection dir)
        {
            switch (dir)
            {
                case UINeighbors.NeighborDirection.NONE: return "";
                case UINeighbors.NeighborDirection.TOP: return inputUp;
                case UINeighbors.NeighborDirection.RIGHT: return inputRight;
                case UINeighbors.NeighborDirection.BOTTOM: return inputDown;
                case UINeighbors.NeighborDirection.LEFT: return inputLeft;
                default: return "";
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
        public static float CalculateDynamicFontSize(string text, Vector2 size, float fontSpacing = 1f, string fontName = "")
        {
            float baseSize = GetFont(fontName).baseSize;
            //float scalingFactor = size.Y / baseSize;
            //Vector2 textSize = MeasureTextEx(GetFont(), text, baseSize * scalingFactor, fontSpacing);
            //float correctionFactor = MathF.Min(size.X / textSize.X, 1f) ;
            //return (baseSize * scalingFactor) * correctionFactor;// * (size.X / textSize.X);

            return GetFontScalingFactor(text, size, fontSpacing, fontName) * baseSize;
        }
        
        public static float CalculateDynamicFontSize(string text, Vector2 size, Font font, float fontSpacing = 1f)
        {
            float baseSize = font.baseSize;

            return GetFontScalingFactor(text, size, font, fontSpacing) * baseSize;
        }
        public static float CalculateDynamicFontSize(float height,Font font)
        {
            float baseSize = font.baseSize;

            return GetFontScalingFactor(height, font) * baseSize;
        }
        public static float CalculateDynamicFontSize(float height, string fontName = "")
        {
            return CalculateDynamicFontSize(height, GetFont(fontName));
        }
        public static float CalculateDynamicFontSize(string text, float width, float fontSpacing = 1f, string fontName = "")
        {
            return CalculateDynamicFontSize(text, width, GetFont(fontName), fontSpacing);
        }
        public static float CalculateDynamicFontSize(string text, float width, Font font, float fontSpacing = 1f)
        {
            float baseSize = font.baseSize;

            return GetFontScalingFactor(text, width, font, fontSpacing) * baseSize;
        }
        //public static float GetFontScalingFactor(float fontSize, string fontName = "") { return fontSize / (float)GetFont(fontName).baseSize; }
        public static float GetFontScalingFactor(float height, string fontName = "") { return GetFontScalingFactor(height, GetFont(fontName)); }
        public static float GetFontScalingFactor(float height, Font font)
        {
            float baseSize = font.baseSize;
            return height / baseSize;
        }
        public static float GetFontScalingFactor(string text, Vector2 size, float fontSpacing = 1, string fontName = "")
        {
            float baseSize = GetFont(fontName).baseSize;
            float scalingFactor = size.Y / baseSize;
            Vector2 textSize = MeasureTextEx(GetFont(), text, baseSize * scalingFactor, fontSpacing);
            float correctionFactor = MathF.Min(size.X / textSize.X, 1f);
            return scalingFactor * correctionFactor;
        }
        public static float GetFontScalingFactor(string text, Vector2 size, Font font, float fontSpacing = 1)
        {
            float baseSize = font.baseSize;
            float scalingFactor = size.Y / baseSize;
            Vector2 textSize = MeasureTextEx(GetFont(), text, baseSize * scalingFactor, fontSpacing);
            float correctionFactor = MathF.Min(size.X / textSize.X, 1f);
            return scalingFactor * correctionFactor;
        }
        public static float GetFontScalingFactor(string text, float width, float fontSpacing = 1, string fontName = "")
        {
            float baseSize = GetFont(fontName).baseSize;
            Vector2 textSize = MeasureTextEx(GetFont(), text, baseSize, fontSpacing);
            float scalingFactor = width / textSize.X;
            return scalingFactor;
        }
        public static float GetFontScalingFactor(string text, float width, Font font, float fontSpacing = 1)
        {
            float baseSize = font.baseSize;
            Vector2 textSize = MeasureTextEx(GetFont(), text, baseSize, fontSpacing);
            float scalingFactor = width / textSize.X;
            return scalingFactor;
        }
        
        public static Vector2 GetTextSize(string text, float fontSize, float fontSpacing, string fontName = "")
        {
            return MeasureTextEx(GetFont(fontName), text, fontSize, fontSpacing);
        }

        //public static Rectangle AdjustRectangleToScreen(Rectangle rect, Vector2 stretchFactor, float areaSideFactor)
        //{
        //    return new Rectangle
        //        (
        //            rect.x * stretchFactor.X,
        //            rect.y * stretchFactor.Y,
        //            rect.width * areaSideFactor,
        //            rect.height * areaSideFactor
        //        );
        //}
       


        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, Vector2 textSize, float fontSpacing, Color color, Font font, Alignement alignement = Alignement.CENTER)
        {
            float fontSize = CalculateDynamicFontSize(text, textSize, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = GetAlignementVector(alignement) * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, fontSpacing, color);

           // DrawRectangleLinesEx(new())
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, float textHeight, float fontSpacing, Color color, Font font, Alignement alignement = Alignement.CENTER)
        {
            float fontSize = CalculateDynamicFontSize(textHeight, font);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = GetAlignementVector(alignement) * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, fontSpacing, color);
        }
        public static void DrawTextAlignedPro2(string text, Vector2 uiPos, float rotDeg, float textWidth, float fontSpacing, Color color, Font font, Alignement alignement = Alignement.CENTER)
        {
            float fontSize = CalculateDynamicFontSize(text, textWidth, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = GetAlignementVector(alignement) * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, 1, color);
        }
        public static void DrawTextAlignedPro3(string text, Vector2 uiPos, float rotDeg, float fontSize, float fontSpacing, Color color, Font font, Alignement alignement = Alignement.CENTER)
        {
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 originOffset = GetAlignementVector(alignement) * fontDimensions;
            DrawTextPro(font, text, uiPos, originOffset, rotDeg, fontSize, fontSpacing, color);
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, Vector2 textSize, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro(text, uiPos, rotDeg, textSize, fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, float textHeight, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro(text, uiPos, rotDeg, textHeight, fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAlignedPro2(string text, Vector2 uiPos, float rotDeg, float textWidth, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro2(text, uiPos, rotDeg, textWidth, fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAlignedPro3(string text, Vector2 uiPos, float rotDeg, float fontSize, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro3(text, uiPos, rotDeg, fontSize, fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, Vector2 textSize, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro(text, uiPos, rotDeg, textSize, fontSpacing, color, GetFont(fontName), alignement);
        }
        public static void DrawTextAlignedPro(string text, Vector2 uiPos, float rotDeg, float textHeight, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro(text, uiPos, rotDeg, textHeight, fontSpacing, color, GetFont(fontName), alignement);
        }
        public static void DrawTextAlignedPro2(string text, Vector2 uiPos, float rotDeg, float textWidth, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro2(text, uiPos, rotDeg, textWidth, fontSpacing, color, GetFont(fontName), alignement);
        }
        public static void DrawTextAlignedPro3(string text, Vector2 uiPos, float rotDeg, float fontSize, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAlignedPro3(text, uiPos, rotDeg, fontSize, fontSpacing, color, GetFont(fontName), alignement);
        }
        //public static void DrawTextAlignedPro(string text, Vector2 pos, float rot, FontSize fontSize, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        //{
        //    DrawTextAlignedPro(text, pos, rot, GetFontSizeScaled(fontSize), Scale(fontSpacing), color, GetFont(), alignement);
        //}
        //public static void DrawTextAlignedPro(string text, Vector2 pos, float rot, FontSize fontSize, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        //{
        //    DrawTextAlignedPro(text, pos, rot, GetFontSizeScaled(fontSize), Scale(fontSpacing), color, GetFont(fontName), alignement);
        //}
        public static void DrawTextAligned(string text, Vector2 uiPos, Vector2 textSize, float fontSpacing, Color color, Font font, Alignement alignement = Alignement.CENTER)
        {
            float fontSize = CalculateDynamicFontSize(text, textSize, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 topLeft = uiPos - GetAlignementVector(alignement) * fontDimensions;
            DrawTextEx(font, text, topLeft, fontSize, fontSpacing, color);
            //DrawRectangleLinesEx(new(topLeft.X, topLeft.Y, fontDimensions.X, fontDimensions.Y), 5f, WHITE);
        }
        public static void DrawTextAligned(string text, Vector2 uiPos, float textHeight, float fontSpacing, Color color, Font font, Alignement alignement = Alignement.CENTER)
        {
            float fontSize = CalculateDynamicFontSize(textHeight, font);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            DrawTextEx(font, text, uiPos - GetAlignementVector(alignement) * fontDimensions, fontSize, fontSpacing, color);
        }
        public static void DrawTextAligned2(string text, Vector2 uiPos, float textWidth, float fontSpacing, Color color, Font font, Alignement alignement = Alignement.CENTER)
        {
            float fontSize = CalculateDynamicFontSize(text, textWidth, font, fontSpacing);
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            DrawTextEx(font, text, uiPos - GetAlignementVector(alignement) * fontDimensions, fontSize, fontSpacing, color);
        }
        public static void DrawTextAligned3(string text, Vector2 uiPos, float fontSize, float fontSpacing, Color color, Font font, Alignement alignement = Alignement.CENTER)
        {
            Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            Vector2 topLeft = uiPos - GetAlignementVector(alignement) * fontDimensions;
            DrawTextEx(font, text, topLeft, fontSize, fontSpacing, color);
            DrawRectangleLinesEx(new(topLeft.X, topLeft.Y, fontDimensions.X, fontDimensions.Y), 5f, WHITE);
        }


        public static void DrawTextAligned(string text, Vector2 uiPos, Vector2 textSize, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned(text, uiPos, textSize, fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAligned(string text, Vector2 uiPos, float textHeight, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned(text, uiPos, textHeight, fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAligned2(string text, Vector2 uiPos, float textWidth, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned2(text, uiPos, textWidth, fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAligned3(string text, Vector2 uiPos, float fontSize, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned3(text, uiPos, fontSize, fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAligned(string text, Rectangle textRect, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned(text, new Vector2(textRect.X, textRect.Y), new Vector2(textRect.width, textRect.height), fontSpacing, color, GetFont(), alignement);
        }
        public static void DrawTextAligned(string text, Vector2 uiPos, Vector2 textSize, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned(text, uiPos, textSize, fontSpacing, color, GetFont(fontName), alignement);
        }
        public static void DrawTextAligned(string text, Vector2 uiPos, float fontHeight, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned(text, uiPos, fontHeight, fontSpacing, color, GetFont(fontName), alignement);
        }
        public static void DrawTextAligned2(string text, Vector2 uiPos, float fontWidth, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned2(text, uiPos, fontWidth, fontSpacing, color, GetFont(fontName), alignement);
        }
        public static void DrawTextAligned3(string text, Vector2 uiPos, float fontSize, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned3(text, uiPos, fontSize, fontSpacing, color, GetFont(fontName), alignement);
        }
        public static void DrawTextAligned(string text, Rectangle textRect, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        {
            DrawTextAligned(text, new Vector2(textRect.X, textRect.Y), new Vector2(textRect.width, textRect.height), fontSpacing, color, GetFont(fontName), alignement);
        }
        //public static void DrawTextAligned(string text, Vector2 pos, FontSize fontSize, float fontSpacing, Color color, Alignement alignement = Alignement.CENTER)
        //{
        //    DrawTextAligned(text, pos, GetFontSizeScaled(fontSize), Scale(fontSpacing), color, GetFont(), alignement);
        //}
        //public static void DrawTextAligned(string text, Vector2 pos, FontSize fontSize, float fontSpacing, Color color, string fontName, Alignement alignement = Alignement.CENTER)
        //{
        //    DrawTextAligned(text, pos, GetFontSizeScaled(fontSize), Scale(fontSpacing), color, GetFont(fontName), alignement);
        //}

        
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

