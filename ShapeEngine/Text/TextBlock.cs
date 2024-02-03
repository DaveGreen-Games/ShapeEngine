using System.Numerics;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Text;

public class TextBlock
{
    #region Members
    public readonly List<TextEmphasis> Emphases = new();
    
    public TextFont TextFont;
    public Caret Caret = new();
    public bool UseEmphasis = true;
    #endregion

    #region Main
    public TextBlock(TextFont textFont)
    {
        this.TextFont = textFont;
    }

    public bool HasEmphasis() => UseEmphasis && Emphases.Count > 0;
    public void Draw(string text, Rect rect, Vector2 alignement, TextWrapType textWrapType = TextWrapType.None)
    {
        if(textWrapType == TextWrapType.None)
        {
            TextFont.DrawTextWrapNone(text, rect ,alignement, Caret, Emphases);
        }
        else if (textWrapType == TextWrapType.Char)
        {
            TextFont.DrawTextWrapChar(text, rect, alignement, Caret, Emphases);
        }
        else
        {
            TextFont.DrawTextWrapWord(text, rect, alignement, Caret, Emphases);
        }
        
    }
    #endregion
}



// public class TextLabel
// {
//     internal class Word
//     {
//         public Rect Rect;
//         public string Text;
//         public Emphasis? Emphasis = null;
//         public Raylib_CsLo.Color Color;
//         
//         
//     }
//     
//     
//     //dirty system?
//     //mouse pos / mouse selection emphasis / mouse caret color
//     //function to move caret to mouse position or to specific index
//     //event & virtual function when word was selected by mouse (mouse hovers over it)
//     //maybe a word selection mode where user can move between words instead of moving the caret?
//
//     //- Label (collection of words based on text, rect, color, emphases, font spacing, line spacing)
//     //  - Word (text + rect + emphasis)
//
//     private string text;
//     public string Text
//     {
//         get
//         { 
//             return text;
//         }
//         set
//         {
//             text = value;
//         }
//     }
//     
//     private Rect rect;
//     public Rect Rect
//     {
//         get
//         { 
//             return rect;
//         }
//         set
//         {
//             rect = value;
//         }
//     }
//
//     
//     
//     public TextLabel()
//     {
//         
//     }
// }

