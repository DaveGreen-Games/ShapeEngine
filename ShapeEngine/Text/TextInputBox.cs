namespace ShapeEngine.Text;

/// <summary>
/// Provides a text input box with caret, text editing, and entry management functionality.
/// </summary>
/// <remarks>
/// Supports caret movement, text entry, deletion, and blinking behavior for UI text input scenarios.
/// </remarks>
public class TextInputBox
{
    #region Members

    /// <summary>
    /// Gets the current text to display, depending on whether the box is active.
    /// </summary>
    public string Text
    {
        get
        {
            if (Active)
            {
                return ActiveText.Length <= 0 ? EmptyText : ActiveText;
            }
            else
            {
                return EnteredText.Length <= 0 ? EmptyText : EnteredText;
            }
        }
    }
    /// <summary>
    /// The text currently being edited while the box is active.
    /// </summary>
    public string ActiveText { get; private set; } = string.Empty;
    /// <summary>
    /// The last entered text after finishing entry.
    /// </summary>
    public string EnteredText { get; private set; } = string.Empty;
    /// <summary>
    /// The text to display when no text is entered.
    /// </summary>
    public string EmptyText;
    /// <summary>
    /// The current caret index in the text.
    /// </summary>
    public int CaretIndex { get; private set; }
    /// <summary>
    /// Indicates whether the input box is currently active (being edited).
    /// </summary>
    public bool Active { get; private set; }
    /// <summary>
    /// Indicates whether the caret is currently visible (blinking logic).
    /// </summary>
    public bool CaretVisible => !caretBlinkActive;
    /// <summary>
    /// The interval in seconds for caret blinking.
    /// </summary>
    public float CaretBlinkInterval = 0.5f;
    /// <summary>
    /// The interval in seconds for caret movement and deletion.
    /// </summary>
    public float CaretMoveInterval = 0.1f; //move caret, delete, backspace

    private bool caretBlinkActive;
    private float caretBlinkTimer;
    private float caretMoveTimer;
    #endregion

    #region Basic
    /// <summary>
    /// Initializes a new instance of the <see cref="TextInputBox"/> class.
    /// </summary>
    /// <param name="emptyText">The text to display when no text is entered.</param>
    public TextInputBox(string emptyText)
    {
        this.EmptyText = emptyText;
        if (CaretBlinkInterval > 0f) caretBlinkTimer = CaretBlinkInterval;
        if (CaretMoveInterval > 0f) caretMoveTimer = CaretMoveInterval;
    }

    /// <summary>
    /// Updates the caret blink and move timers.
    /// </summary>
    /// <param name="dt">The time delta since the last update, in seconds.</param>
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

    /// <summary>
    /// Sets the entered text directly, without activating the input box.
    /// </summary>
    /// <param name="newEnteredText">The new entered text.</param>
    public void SetEnteredText(string newEnteredText)
    {
        EnteredText = newEnteredText;
    }
    /// <summary>
    /// Starts a new text entry, activating the input box.
    /// </summary>
    /// <returns>True if the entry was successfully started, false if already active.</returns>
    public bool StartEntry()
    {
        if (Active) return false;
        Active = true;
        ActiveText = EnteredText;
        return true;
    }
    /// <summary>
    /// Starts a new clean text entry, clearing any existing text.
    /// </summary>
    /// <returns>True if the clean entry was successfully started, false if already active.</returns>
    public bool StartEntryClean()
    {
        if (Active) return false;
        Active = true;
        ActiveText = string.Empty;
        EnteredText = string.Empty;
        CaretIndex = 0;
        return true;
    }
    /// <summary>
    /// Finishes the current text entry, saving the text and deactivating the input box.
    /// </summary>
    /// <returns>The text that was entered, or an empty string if no text was entered.</returns>
    public string FinishEntry()
    {
        if (!Active) return string.Empty;
        Active = false;
        EnteredText = ActiveText;
        ActiveText = string.Empty;
        CaretIndex = EnteredText.Length;
        return EnteredText;
    }
    /// <summary>
    /// Cancels the current text entry, discarding any changes and deactivating the input box.
    /// </summary>
    /// <returns>True if the entry was successfully canceled, false if no active entry to cancel.</returns>
    public bool CancelEntry()
    {
        if (!Active) return false;
        Active = false;
        ActiveText = string.Empty;
        CaretIndex = EnteredText.Length;
        return true;
    }
    
    /// <summary>
    /// Deletes the current entry, clearing the text and resetting the caret.
    /// </summary>
    /// <returns>True if the entry was successfully deleted, false if no active entry to delete.</returns>
    public bool DeleteEntry()
    {
        if (!Active) return false;
        ActiveText = string.Empty;
        CaretIndex = 0;
        return true;
    }
    #endregion

    #region Delete Character
    /// <summary>
    /// Deletes characters from the start of the active text.
    /// </summary>
    /// <param name="amount">The number of characters to delete.</param>
    /// <returns>True if characters were deleted, false if not active or invalid amount.</returns>
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
    /// <summary>
    /// Ends the character deletion operation, stopping any timers.
    /// </summary>
    public void DeleteCharacterEnd()
    {
        caretMoveTimer = -1f;
    }
    /// <summary>
    /// Deletes characters or performs backspace operation in the active text.
    /// </summary>
    /// <param name="amount">The number of characters to delete (positive) or backspace (negative).</param>
    /// <returns>True if characters were deleted or backspaced, false if not active or invalid amount.</returns>
    public bool DeleteCharacter(int amount)
    {
        if (amount == 0 || !Active || ActiveText.Length <= 0) return false;

        if (amount > 0)//delete
        {
            if (CaretIndex >= ActiveText.Length) return false;

            ActiveText = ActiveText.Remove(CaretIndex, amount);
        }
        else //backspace
        {
            if (CaretIndex <= 0) return false;

            MoveCaret(amount);
            ActiveText = ActiveText.Remove(CaretIndex, amount * -1);
        }

        return true;
    }

    #endregion

    #region Add
    /// <summary>
    /// Adds a list of Unicode characters at the current caret position.
    /// </summary>
    /// <param name="unicodeCharacters">The list of Unicode character codes to add.</param>
    /// <returns>True if characters were added, false if not active or empty <see cref="unicodeCharacters"/> list.</returns>
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
    /// <summary>
    /// Adds a list of characters at the current caret position.
    /// </summary>
    /// <param name="characters">The list of characters to add.</param>
    /// <returns>True if characters were added, false if not active or empty <see cref="characters"/> list.</returns>
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
    /// <summary>
    /// Adds a single Unicode character at the current caret position.
    /// </summary>
    /// <param name="unicode">The Unicode character code to add.</param>
    /// <returns>True if the character was added, false if not active.</returns>
    public bool AddCharacter(int unicode)
    {
        return AddCharacter((char)unicode);
    }
    /// <summary>
    /// Adds a single character at the current caret position.
    /// </summary>
    /// <param name="c">The character to add.</param>
    /// <returns>True if the character was added, false if not active.</returns>
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

    /// <summary>
    /// Moves the caret by a certain number of spaces, with optional wrapping.
    /// </summary>
    /// <param name="spaces">The number of spaces to move the caret.</param>
    /// <param name="wrapAround">Whether to wrap around the text ends.</param>
    /// <returns>The new caret index.</returns>
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
    /// <summary>
    /// Ends the caret movement operation, stopping any timers.
    /// </summary>
    public void MoveCaretEnd()
    {
        caretMoveTimer = -1f;
    }
    
    /// <summary>
    /// Moves the caret by a certain number of spaces, with optional wrapping.
    /// </summary>
    /// <param name="spaces">The number of spaces to move the caret.</param>
    /// <param name="wrapAround">Whether to wrap around the text ends.</param>
    /// <returns>The new caret index.</returns>
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