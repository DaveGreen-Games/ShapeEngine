using System.Numerics;
using System.Text;
using Raylib_CsLo;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Lib;

namespace Examples.UIElements;

public class InputActionLabel
{
    public Font Font;
    public Color Color;
    public InputAction InputAction;
    public string Title;
    public InputActionLabel(InputAction inputAction, string title, Font font, Color color)
    {
        this.InputAction = inputAction;
        this.Font = font;
        this.Color = color;
        this.Title = title;
    }
    public void Draw(Rect r, Vector2 textAlignement, InputDevice curInputDevice, float fontSpacing = 1f)
    {
        string text = InputAction.GetInputTypeDescription(curInputDevice, true, 1, false);
        StringBuilder b = new(text.Length + Title.Length + 3);
        b.Append(Title);
        b.Append(' ');
        //b.Append('[');
        b.Append(text);
        //b.Append(']');
        Font.DrawText(b.ToString(), r, fontSpacing, textAlignement, Color);
    }
}