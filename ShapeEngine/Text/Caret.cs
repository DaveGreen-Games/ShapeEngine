using System.Numerics;
using ShapeEngine.Color;

namespace ShapeEngine.Text;

public struct Caret
{
    public int Index;
    public float WidthRelative;
    public ShapeColor Color;

    public bool IsValid => Index >= 0 && WidthRelative > 0f;
    public Caret()
    {
        Index = -1;
        WidthRelative = 0f;
        Color = ShapeColor.White;
    }

    public Caret(int index, ShapeColor color, float relativeWidth = 0.05f)
    {
        this.Index = index;
        this.Color = color;
        this.WidthRelative = relativeWidth;
    }

    public void Draw(Vector2 top, float height)
    {
        var bottom = top + new Vector2(0f, height);
        DrawLineEx(top, bottom, WidthRelative * height, Color.ToRayColor());
    }
    // public void Draw(string text, Vector2 topLeft, Font font, float fontSize, float fontSpacing)
    // {
    //     if (Width <= 0f) return;
    //     
    //     string caretText = text.Substring(0, Index);
    //     var caretTextSize = TextBlock.GetTextSize(caretText, fontSize, fontSpacing, font);
    //
    //     var caretTop = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, 0f);
    //     var caretBottom = topLeft + new Vector2(caretTextSize.X + fontSpacing * 0.5f, fontSize);
    //     
    //     DrawLineEx(caretTop, caretBottom, Width, Color);
    // }
}