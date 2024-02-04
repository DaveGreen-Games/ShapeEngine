using System.Numerics;
using System.Text;
using Raylib_CsLo;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Text;

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
        this.textFont = new(font, fontSpacing, color.Color);
        this.Title = title;
    }
    public void Draw(Rect r, Vector2 textAlignement, InputDeviceType curInputDeviceType)
    {
        textFont.Color = color.Color;
        
        string text = InputAction.GetInputTypeDescription(curInputDeviceType, true, 1, false);
        StringBuilder b = new(text.Length + Title.Length + 3);
        b.Append(Title);
        b.Append(' ');
        b.Append(text);
        textFont.DrawTextWrapNone(b.ToString(), r, textAlignement);
        // Font.DrawText(b.ToString(), r, fontSpacing, textAlignement, Color);
    }
}