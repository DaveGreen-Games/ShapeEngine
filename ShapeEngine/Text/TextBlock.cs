// using ShapeEngine.Core.Shapes;
//
// namespace ShapeEngine.Text;
//
//
//
// public class TextPiece
// {
//     public Rect Rect;
//     public string Text;
//     public Emphasis? Emphasis = null;
//
//     public TextPiece(Rect rect, string text, Emphasis? emphasis = null)
//     {
//         Rect = rect;
//         Text = text;
//         Emphasis = emphasis;
//     }
//
//     public void UpdateRect(Rect newRect) => Rect = newRect;
// }
//
// public class TextBlock
// {
//     private List<TextPiece> textPieces;
//     private TextEmphasis emphasis;
//     private TextFont originalFont;
//     private TextFont scaledFont;
//     private string originalText;
//     private Rect curRect;
//
//     public TextBlock(Rect rect, string text, TextFont textFont, TextEmphasis textEmphasis)
//     {
//         originalText = text;
//         curRect = rect;
//         emphasis = textEmphasis;
//         originalFont = textFont;
//         scaledFont = CalculateScaledFont();
//         
//         var words = text.Split(" ");
//         foreach (var word in words)
//         {
//             var size = scaledFont.GetTextSize(word);
//             var piece = new TextPiece(new(), word, null);
//         }
//         
//     }
//
//     public void UpdateRect(Rect newRect)
//     {
//         curRect = newRect;
//         scaledFont = CalculateScaledFont();
//         
//     }
//
//     private void UpdateTextPieces()
//     {
//         
//     }
//     private TextFont CalculateScaledFont() => originalFont.ScaleDynamic(originalText, curRect.Size);
// }
