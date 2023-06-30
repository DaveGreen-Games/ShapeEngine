

using Raylib_CsLo;
using static System.Net.Mime.MediaTypeNames;

namespace ShapeEngine.Lib
{
    public struct TextBoxInfo
    {
        public string Text;
        public int CaretIndex;
        public bool Active;
        

        public TextBoxInfo(string text, int caretIndex, bool Active)
        {
            this.Text = text;
            this.CaretIndex = caretIndex;
            this.Active = Active;
        }
    }
    public struct TextBoxKeys
    {
        public KeyboardKey KeyActivate = KeyboardKey.KEY_ENTER;
        public KeyboardKey KeyFinish = KeyboardKey.KEY_ENTER;
        public KeyboardKey KeyCancel = KeyboardKey.KEY_ESCAPE;
        public KeyboardKey KeyRight = KeyboardKey.KEY_RIGHT;
        public KeyboardKey KeyLeft = KeyboardKey.KEY_LEFT;
        public KeyboardKey KeyDelete = KeyboardKey.KEY_DELETE;
        public KeyboardKey KeyBackspace = KeyboardKey.KEY_BACKSPACE;
        public TextBoxKeys() { }
    }
    
    public static class SText
    {
        public static TextBoxInfo UpdateTextBoxInfo(this TextBoxInfo textBox, TextBoxKeys keys)
        {
            if (!textBox.Active)
            {
                if (IsKeyPressed(keys.KeyActivate))
                {
                    textBox.Active = true;
                }
                return textBox;
            }
            else
            {
                if (IsKeyPressed(keys.KeyCancel))
                {
                    textBox.Active = false;
                    textBox.Text = string.Empty;
                    textBox.CaretIndex = 0;
                }
                else if (IsKeyPressed(keys.KeyFinish))
                {
                    textBox.Active = false;
                }
                else if (IsKeyPressed(keys.KeyDelete))
                {
                    var info = SText.TextDelete(textBox.Text, textBox.CaretIndex);
                    textBox.Text = info.text;
                    textBox.CaretIndex = info.caretIndex;
                }
                else if (IsKeyPressed(keys.KeyBackspace))
                {
                    var info = SText.TextBackspace(textBox.Text, textBox.CaretIndex);
                    textBox.Text = info.text;
                    textBox.CaretIndex = info.caretIndex;
                }
                else if (IsKeyPressed(keys.KeyLeft))
                {
                    textBox.CaretIndex = SText.DecreaseCaretIndex(textBox.CaretIndex, textBox.Text.Length);
                }
                else if (IsKeyPressed(keys.KeyRight))
                {
                    textBox.CaretIndex = SText.IncreaseCaretIndex(textBox.CaretIndex, textBox.Text.Length);
                }
                else
                {
                    var info = SText.GetTextInput(textBox.Text, textBox.CaretIndex);
                    textBox.Text = info.text;
                    textBox.CaretIndex = info.newCaretPosition;
                }
                return textBox;
            }

            
        }

        public static int IncreaseCaretIndex(int caretIndex, int textLength) { return ChangeCaretIndex(caretIndex, 1, textLength); }
        public static int DecreaseCaretIndex(int caretIndex, int textLength) { return ChangeCaretIndex(caretIndex, -1, textLength); }
        public static int ChangeCaretIndex(int caretIndex, int amount, int textLength)
        {
            caretIndex += amount;
            if (caretIndex < 0) caretIndex = textLength;
            else if (caretIndex > textLength) caretIndex = 0;
            return caretIndex;
        }

        public static (string text, int caretIndex) TextBackspace(string text, int caretIndex)
        {
            if (caretIndex < 0 || caretIndex > text.Length) return (text, 0);
            if (text.Length <= 0) return (text, 0);

            if (caretIndex > 0)
            {
                return (text.Remove(caretIndex - 1, 1), caretIndex - 1);

            }
            else
            {
                return (text.Remove(caretIndex, 1), caretIndex);
            }
        }
        public static (string text, int caretIndex) TextDelete(string text, int caretIndex)
        {
            if (caretIndex < 0 || caretIndex > text.Length) return (text, 0);
            if (text.Length <= 0) return (text, 0);
            if (caretIndex < text.Length)
            {
                return (text.Remove(caretIndex, 1), caretIndex);
            }
            else
            {
                return (text.Remove(caretIndex - 1, 1), caretIndex - 1);
            }
        }

        public static (string text, int newCaretPosition) GetTextInput(string curText, int caretIndex)
        {
            List<Char> characters = curText.ToList();
            int unicode = Raylib.GetCharPressed();
            while (unicode != 0)
            {
                var c = (char)unicode;
                if (caretIndex < 0 || caretIndex >= characters.Count) characters.Add(c);
                else
                {
                    characters.Insert(caretIndex, c);

                }
                caretIndex++;
                unicode = Raylib.GetCharPressed();
            }
            return (new string(characters.ToArray()), caretIndex);
        }
        public static string GetTextInput(string curText)
        {
            if (IsKeyPressed(KeyboardKey.KEY_BACKSPACE)) curText = curText.Remove(curText.Length - 1);
            int unicode = Raylib.GetCharPressed();
            while (unicode != 0)
            {
                var c = (char)unicode;
                curText += c;

                unicode = Raylib.GetCharPressed();
            }
            return curText;
        }

    }
}
