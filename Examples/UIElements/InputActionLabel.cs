using System.Numerics;
using System.Text;
using Raylib_CsLo;
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
    public InputActionLabel(InputAction inputAction, string title, Font font, Color color, float fontSpacing = 1f)
    {
        this.InputAction = inputAction;
        this.textFont = new(font, fontSpacing, color);
        this.Title = title;
    }
    public void Draw(Rect r, Vector2 textAlignement, InputDeviceType curInputDeviceType)
    {
        string text = InputAction.GetInputTypeDescription(curInputDeviceType, true, 1, false);
        StringBuilder b = new(text.Length + Title.Length + 3);
        b.Append(Title);
        b.Append(' ');
        b.Append(text);
        textFont.DrawTextWrapNone(b.ToString(), r, textAlignement);
        // Font.DrawText(b.ToString(), r, fontSpacing, textAlignement, Color);
    }
}