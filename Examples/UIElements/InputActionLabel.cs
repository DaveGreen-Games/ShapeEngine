using System.Numerics;
using System.Text;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Text;
using Raylib_cs;
using ShapeEngine.Core.Structs;

namespace Examples.UIElements;

public class InputActionLabel
{
    private TextFont textFont;
    public InputAction InputAction;
    public string Title;
    public PaletteColor color;
    public InputActionLabel(InputAction inputAction, string title, Font font, PaletteColor color, float fontSpacing = 1f)
    {
        this.color = color;
        this.InputAction = inputAction;
        this.textFont = new(font, fontSpacing, color.ColorRgba);
        this.Title = title;
    }
    public void Draw(Rect r, AnchorPoint textAlignement, InputDeviceType curInputDeviceType)
    {
        textFont.ColorRgba = color.ColorRgba;
        
        string text = InputAction.GetInputTypeDescription(curInputDeviceType, true, 1, false);
        StringBuilder b = new(text.Length + Title.Length + 3);
        b.Append(Title);
        b.Append(' ');
        b.Append(text);
        textFont.DrawTextWrapNone(b.ToString(), r, textAlignement);
        // Font.DrawText(b.ToString(), r, fontSpacing, textAlignement, Color);
    }
}