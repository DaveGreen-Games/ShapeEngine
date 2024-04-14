namespace ShapeEngine.Text;

public class TextInputBox
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
    public TextInputBox(string emptyText)
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