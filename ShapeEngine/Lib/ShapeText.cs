using Raylib_CsLo;

namespace ShapeEngine.Lib
{
    //struct for Font, FontSize, FontColor
    //=> can calculate dynamic font size
    //=> static shape text class has a Member for that struct that can be set to be used in all text drawing functions
    //=> text drawing functions only need text, rect & emphasis anymore
    //=>
    
    
    public class ShapeTextBox
    {
        #region Members

        public string Text
        {
            get
            {
                if (Active)
                {
                    // if (ActiveText.Length <= 0)
                    // {
                    //     return EnteredText.Length <= 0 ? EmptyText : EnteredText;
                    // }
                    // else
                    // {
                    //     return ActiveText;
                    // }
                    return ActiveText.Length <= 0 ? EmptyText : ActiveText;
                }
                else
                {
                    return EnteredText.Length <= 0 ? EmptyText : EnteredText;
                }
            }
        }
        
        public string ActiveText { get; private set; } = string.Empty;
        public string EnteredText { get; private set; } = string.Empty;
        public string EmptyText;
        
        public int CaretIndex { get; private set; } = 0;
        public bool Active { get; private set; } = false;

        public bool CaretVisible => !caretBlinkActive;// || caretMoveTimer > 0f;
        public float CaretBlinkInterval = 0.5f;
        public float CaretMoveInterval = 0.1f; //move caret, delete, backspace

        private bool caretBlinkActive = false;
        private float caretBlinkTimer;
        private float caretMoveTimer;
        #endregion

        #region Basic
        public ShapeTextBox(string emptyText)
        {
            this.EmptyText = emptyText;
            if (CaretBlinkInterval > 0f) caretBlinkTimer = CaretBlinkInterval;
            if (CaretMoveInterval > 0f) caretMoveTimer = CaretMoveInterval;
        }

        public void Update(float dt)
        {
            if (CaretBlinkInterval < 0f)
            {
                caretBlinkActive = false;
                caretBlinkTimer = 0f;
            }
            else
            {
                if (caretBlinkTimer > 0f)
                {
                    caretBlinkTimer -= dt;
                    if (caretBlinkTimer <= 0f)
                    {
                        caretBlinkTimer = CaretBlinkInterval;
                        caretBlinkActive = !caretBlinkActive;
                    }
                }
            }

            if (caretMoveTimer > 0f)
            {
                caretMoveTimer -= dt;
                if (caretMoveTimer <= 0f) caretMoveTimer = 0f;
            }
            else caretMoveTimer = -1f;
        }
        #endregion
        
        #region Start / End / Delete Entry

        public void SetEnteredText(string newEnteredText)
        {
            EnteredText = newEnteredText;
        }
        public bool StartEntry()
        {
            if (Active) return false;
            Active = true;
            ActiveText = EnteredText;
            return true;
        }
        public bool StartEntryClean()
        {
            if (Active) return false;
            Active = true;
            ActiveText = string.Empty;
            EnteredText = string.Empty;
            CaretIndex = 0;
            return true;
        }
        public string FinishEntry()
        {
            if (!Active) return string.Empty;
            Active = false;
            EnteredText = ActiveText;
            ActiveText = string.Empty;
            CaretIndex = EnteredText.Length;
            return EnteredText;
        }
        public bool CancelEntry()
        {
            if (!Active) return false;
            Active = false;
            ActiveText = string.Empty;
            CaretIndex = EnteredText.Length;
            return true;
        }
        
        public bool DeleteEntry()
        {
            if (!Active) return false;
            ActiveText = string.Empty;
            CaretIndex = 0;
            return true;
        }
        #endregion

        #region Delete Character
        public bool DeleteCharacterStart(int amount)
        {
            if (amount == 0 || !Active || ActiveText.Length <= 0 || caretMoveTimer > 0) return false;
            
            if (CaretMoveInterval > 0)
            {
                if (CaretBlinkInterval > 0f)
                {
                    caretBlinkTimer = CaretBlinkInterval * 2f;
                    caretBlinkActive = false;
                }
                
                if(caretMoveTimer < 0f) caretMoveTimer = CaretMoveInterval * 4;
                else caretMoveTimer = CaretMoveInterval;
            }

            return DeleteCharacter(amount);
        }
        public void DeleteCharacterEnd()
        {
            caretMoveTimer = -1f;
        }
        public bool DeleteCharacter(int amount)
        {
            if (amount == 0 || !Active || ActiveText.Length <= 0) return false;

            if (amount > 0)//delete
            {
                if (CaretIndex >= ActiveText.Length) return false;

                ActiveText = ActiveText.Remove(CaretIndex, amount);
                return true;
            }
            else //backspace
            {
                if (CaretIndex <= 0) return false;

                MoveCaret(amount, false);
                ActiveText = ActiveText.Remove(CaretIndex, amount * -1);
                return true;
            }
        }

        #endregion

        #region Add
        public bool AddCharacters(List<int> unicodeCharacters)
        {
            if (!Active) return false;
            if (unicodeCharacters.Count <= 0) return false;
            
            var curCharacters = ActiveText.ToList();

            foreach (var c in unicodeCharacters)
            {
                if (CaretIndex >= ActiveText.Length) curCharacters.Add((char)c);
                else curCharacters.Insert(CaretIndex, (char)c);
                CaretIndex++;
            }

            ActiveText =  new string(curCharacters.ToArray());
            return true;
        }
        public bool AddCharacters(List<char> characters)
        {
            if (!Active) return false;
            if (characters.Count <= 0) return false;
            
            var curCharacters = ActiveText.ToList();

            foreach (var c in characters)
            {
                if (CaretIndex >= ActiveText.Length) curCharacters.Add(c);
                else curCharacters.Insert(CaretIndex, c);
                CaretIndex++;
            }

            ActiveText = new string(curCharacters.ToArray());
            return true;
        }
        public bool AddCharacter(int unicode)
        {
            return AddCharacter((char)unicode);
        }
        public bool AddCharacter(char c)
        {
            if (!Active) return false;
            
            var characters = ActiveText.ToList();
            
            if (CaretIndex >= ActiveText.Length) characters.Add(c);
            else characters.Insert(CaretIndex, c);

            CaretIndex++;
            
            ActiveText =  new string(characters.ToArray());
            return true;
        }
        #endregion

        #region Caret

        public int MoveCaretStart(int spaces, bool wrapAround = false)
        {
            if (!Active) return CaretIndex;
            if (caretMoveTimer > 0) return CaretIndex;
            
            if (CaretMoveInterval > 0)
            {
                if (CaretBlinkInterval > 0f)
                {
                    caretBlinkTimer = CaretBlinkInterval * 2f;
                    caretBlinkActive = false;
                }
                
                if(caretMoveTimer < 0f) caretMoveTimer = CaretMoveInterval * 4;
                else caretMoveTimer = CaretMoveInterval;
            }

            return MoveCaret(spaces, wrapAround);
        }
        public void MoveCaretEnd()
        {
            caretMoveTimer = -1f;
        }
        
        public int MoveCaret(int spaces, bool wrapAround = false)
        {
            if (!Active) return CaretIndex;
            CaretIndex += spaces;
            if (wrapAround)
            {
                if (CaretIndex < 0) CaretIndex = ActiveText.Length;
                else if (CaretIndex > ActiveText.Length) CaretIndex = 0;
            }
            else
            {
                if (CaretIndex < 0) CaretIndex = 0;
                else if (CaretIndex > ActiveText.Length) CaretIndex = ActiveText.Length;
            }
            
            return CaretIndex;
        }
        
        #endregion
    }
    
    
    
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
        /// <summary>
        /// Automatically scales all text that is drawn with functions from ShapeText.
        /// 0.5 means text is draw at half the size, 2 means that text is drawn at twice the size.
        /// </summary>
        public static float FontSizeModifier = 1f;
        
        
        
        
        
        
        
        
        
        
        
        
        
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
