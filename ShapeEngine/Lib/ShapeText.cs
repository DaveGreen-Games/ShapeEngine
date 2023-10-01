

using Raylib_CsLo;
using static System.Net.Mime.MediaTypeNames;

namespace ShapeEngine.Lib
{
    public struct TextBox
    {
        public string Text;
        public int CaretIndex;
        public bool Active;
        
        public TextBox(string text, int caretIndex, bool Active)
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
    
    public static class ShapeText
    {
        public static TextBox UpdateTextBox(this TextBox textBox, TextBoxKeys keys)
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
                    var info = ShapeText.TextDelete(textBox.Text, textBox.CaretIndex);
                    textBox.Text = info.text;
                    textBox.CaretIndex = info.caretIndex;
                }
                else if (IsKeyPressed(keys.KeyBackspace))
                {
                    var info = ShapeText.TextBackspace(textBox.Text, textBox.CaretIndex);
                    textBox.Text = info.text;
                    textBox.CaretIndex = info.caretIndex;
                }
                else if (IsKeyPressed(keys.KeyLeft))
                {
                    textBox.CaretIndex = ShapeText.DecreaseCaretIndex(textBox.CaretIndex, textBox.Text.Length);
                }
                else if (IsKeyPressed(keys.KeyRight))
                {
                    textBox.CaretIndex = ShapeText.IncreaseCaretIndex(textBox.CaretIndex, textBox.Text.Length);
                }
                else
                {
                    var info = ShapeText.GetTextInput(textBox.Text, textBox.CaretIndex);
                    textBox.Text = info.text;
                    textBox.CaretIndex = info.newCaretPosition;
                }
                return textBox;
            }

            
        }

        public static TextBox IncreaseCaretIndex(this TextBox textBox) { return ChangeCaretIndex(textBox, 1); }
        public static TextBox DecreaseCaretIndex(this TextBox textBox) { return ChangeCaretIndex(textBox, -1); }
        public static TextBox ChangeCaretIndex(this TextBox textBox, int amount)
        {
            textBox.CaretIndex += amount;
            if (textBox.CaretIndex < 0) textBox.CaretIndex = textBox.Text.Length;
            else if (textBox.CaretIndex > textBox.Text.Length) textBox.CaretIndex = 0;
            return textBox;
        }
        public static TextBox TextBackspace(this TextBox textBox)
        {
            if (textBox.CaretIndex < 0 || textBox.CaretIndex > textBox.Text.Length) return textBox;
            if (textBox.Text.Length <= 0) return textBox;

            if (textBox.CaretIndex > 0)
            {
                textBox.CaretIndex -= 1;
                textBox.Text = textBox.Text.Remove(textBox.CaretIndex, 1);
            }
            else
            {
                textBox.Text = textBox.Text.Remove(textBox.CaretIndex, 1);
            }
            return textBox;
        }
        public static TextBox TextDelete(this TextBox textBox)
        {
            if (textBox.CaretIndex < 0 || textBox.CaretIndex > textBox.Text.Length) return textBox;
            if (textBox.Text.Length <= 0) return textBox;

            if (textBox.CaretIndex < textBox.Text.Length)
            {
                textBox.Text = textBox.Text.Remove(textBox.CaretIndex, 1);
            }
            else
            {
                textBox.CaretIndex -= 1;
                textBox.Text = textBox.Text.Remove(textBox.CaretIndex, 1);
            }
            return textBox;
        }
        public static TextBox GetTextInput(this TextBox textBox)
        {
            List<Char> characters = textBox.Text.ToList();
            int unicode = Raylib.GetCharPressed();
            while (unicode != 0)
            {
                var c = (char)unicode;
                if (textBox.CaretIndex < 0 || textBox.CaretIndex >= characters.Count) characters.Add(c);
                else
                {
                    characters.Insert(textBox.CaretIndex, c);

                }
                textBox.CaretIndex++;
                unicode = Raylib.GetCharPressed();
            }
            textBox.Text = new string(characters.ToArray());
            return textBox;
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
