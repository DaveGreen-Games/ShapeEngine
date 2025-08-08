using System.Text;
using Raylib_cs;

namespace ShapeEngine.Input;

/// <summary>
/// Represents a keyboard input device, providing access to keyboard buttons, state tracking,
/// character input, and utility methods for keyboard input.
/// </summary>
public sealed class KeyboardDevice : InputDevice
{
    /// <summary>
    /// All available Raylib keyboard keys.
    /// </summary>
    public static readonly KeyboardKey[] AllKeyboardKeys = Enum.GetValues<KeyboardKey>();
    /// <summary>
    /// All available Shape keyboard buttons.
    /// </summary>
    public static readonly ShapeKeyboardButton[] AllShapeKeyboardButtons = Enum.GetValues<ShapeKeyboardButton>();

    /// <summary>
    /// Gets the usage detection settings for the keyboard input device.
    /// </summary>
    public InputDeviceUsageDetectionSettings.KeyboardSettings UsageDetectionSettings { get; private set; } = new();
    // internal event Action<InputDevice, InputDeviceUsageDetectionSettings>? OnInputDeviceChangeSettingsChanged;
    
    /// <summary>
    /// <para>
    /// The process priority of this keyboard device instance.
    /// </para>
    /// <para>
    /// Change this value to change the order in which this device is processed in <see cref="ShapeInput"/>.
    /// Lower priorities are processed first.
    /// </para>
    /// </summary>
    /// <remarks>
    /// A unique value based on the order of instantiation is assigned per default.
    /// </remarks>
    public uint DeviceProcessPriority = processPriorityCounter++;
    
    private bool wasUsed;
    private bool wasUsedRaw;
    private bool isLocked;
    
    private int pressedCount;
    private float pressedCountDurationTimer;
    private float usedDurationTimer;
    
    /// <summary>
    /// Tracks whether the device was selected in the previous update cycle.
    /// Used for input usage detection logic.
    /// If a device was selected in the previous update cycle,
    /// the device stays the current selected device as long as at least one button is held down.
    /// </summary>
    private bool wasSelected = false;
    
    
    /// <summary>
    /// List of characters entered since the last update.
    /// </summary>
    public readonly List<char> UsedCharacters = [];
    /// <summary>
    /// List of keyboard buttons pressed since the last update.
    /// </summary>
    public readonly List<ShapeKeyboardButton> PressedButtons = [];
    
    /// <summary>
    /// List of keyboard buttons released since the last update.
    /// </summary>
    public readonly List<ShapeKeyboardButton> ReleasedButtons = [];
    
    /// <summary>
    /// List of keyboard buttons currently held down.
    /// </summary>
    public readonly List<ShapeKeyboardButton> HeldDownButtons = [];
    
    private readonly Dictionary<ShapeKeyboardButton, InputState> buttonStates = new(AllShapeKeyboardButtons.Length);
    
    /// <summary>
    /// Event triggered when a keyboard button is pressed.
    /// </summary>
    public event Action<ShapeKeyboardButton>? OnButtonPressed;
    /// <summary>
    /// Event triggered when a keyboard button is released.
    /// </summary>
    public event Action<ShapeKeyboardButton>? OnButtonReleased;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardDevice"/> class.
    /// </summary>
    internal KeyboardDevice()
    {
        foreach (var button in AllShapeKeyboardButtons)
        {
            buttonStates.Add(button, new());
        }
    }

    /// <inheritdoc cref="InputDevice.GetDeviceProcessPriority"/>
    public override uint GetDeviceProcessPriority() => DeviceProcessPriority;

    /// <summary>
    /// Gets the type of this input device, which is always <see cref="InputDeviceType.Keyboard"/>.
    /// </summary>
    public override InputDeviceType GetDeviceType() => InputDeviceType.Keyboard;
    
    /// <summary>
    /// Returns whether the keyboard device is currently locked.
    /// Locking can be used to temporarily disable input processing.
    /// This does not affect whether the device is active or not.
    /// </summary>
    public override bool IsLocked() => isLocked;

    /// <summary>
    /// Locks the keyboard device, preventing input from being registered.
    /// </summary>
    public override void Lock()
    {
        if (isLocked) return;
        isLocked = true;
        
        ResetState();
    }

    /// <summary>
    /// Unlocks the keyboard device, allowing input to be registered.
    /// </summary>
    public override void Unlock()
    {
        if (!isLocked) return;
        isLocked = false;
    }
    
    /// <inheritdoc cref="InputDevice.ApplyInputDeviceChangeSettings"/>
    internal override void ApplyInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings)
    {
        UsageDetectionSettings = settings.Keyboard;
        // OnInputDeviceChangeSettingsChanged?.Invoke(this, settings);
    }
    // internal void OverrideInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings)
    // {
    //     UsageDetectionSettings = settings.Keyboard;
    // }
    /// <inheritdoc cref="InputDevice.WasUsed"/>
    public override bool WasUsed() => wasUsed;
    
    /// <inheritdoc cref="InputDevice.WasUsedRaw"/>
    public override bool WasUsedRaw() => wasUsedRaw;
    
    /// <inheritdoc cref="InputDevice.Update"/>
    public override bool Update(float dt, bool wasOtherDeviceUsed)
    {
        PressedButtons.Clear();
        ReleasedButtons.Clear();
        HeldDownButtons.Clear();
        UsedCharacters.Clear();
        
        UpdateButtonStates();
        
        if (isLocked)
        {
            wasUsed = false;
            wasUsedRaw = false;
            return false;
        }
        
        var unicode = Raylib.GetCharPressed();
        while (unicode > 0)
        {
            UsedCharacters.Add((char)unicode);
            unicode = Raylib.GetCharPressed();
        }
        
        WasKeyboardUsed(dt, wasOtherDeviceUsed, out wasUsed, out wasUsedRaw);
        return wasUsed && !wasOtherDeviceUsed;//safety precaution
    }

    private void ResetState()
    {
        usedDurationTimer = 0f;
        pressedCount = 0;
        pressedCountDurationTimer = 0f;
        
        wasUsed = false;
        wasUsedRaw = false;
        wasUsedRaw = false;
        
        PressedButtons.Clear();
        ReleasedButtons.Clear();
        HeldDownButtons.Clear();
    }
    
    /// <summary>
    /// Gets the current input state for the specified keyboard button.
    /// </summary>
    public InputState GetButtonState(ShapeKeyboardButton button) => buttonStates[button];
    
    /// <summary>
    /// Consumes the input state for the specified keyboard button, marking it as consumed.
    /// Returns the consumed state, or null if already consumed.
    /// </summary>
    /// <param name="button">The keyboard button to consume.</param>
    /// <param name="valid">True if the state was not already consumed; otherwise, false.</param>
    public InputState ConsumeButtonState(ShapeKeyboardButton button, out bool valid)
    {
        valid = false;
        var state = buttonStates[button];
        if (state.Consumed) return state;

        valid = true;
        buttonStates[button] = state.Consume();
        return state;
    }
    
    /// <summary>
    /// Gets the list of characters entered since the last update.
    /// </summary>
    /// <returns>List of entered characters.</returns>
    public List<char> GetStreamChar()
    {
        if (isLocked) return new List<char>();
        return UsedCharacters.ToList();
    }
    /// <summary>
    /// Gets the string of characters entered since the last update.
    /// </summary>
    /// <returns>String of entered characters.</returns>
    public string GetStream()
    {
        if (isLocked) return string.Empty;
        return new string(UsedCharacters.ToArray());
    }
    /// <summary>
    /// Appends the entered characters to the current text.
    /// </summary>
    /// <param name="curText">Current text.</param>
    /// <returns>Updated text with entered characters appended.</returns>
    public string GetStream(string curText)
    {
        if (isLocked) return curText;
        var b = new StringBuilder(UsedCharacters.Count + curText.Length);
        b.Append(curText);
        b.Append( new string(UsedCharacters.ToArray()) );
        return b.ToString();
    }
    /// <summary>
    /// Inserts entered characters into the current text at the specified caret index.
    /// </summary>
    /// <param name="curText">Current text.</param>
    /// <param name="caretIndex">Caret index for insertion.</param>
    /// <returns>Tuple of updated text and new caret index.</returns>
    public (string text, int caretIndex) GetStream(string curText, int caretIndex)
    {
        if (isLocked) return (curText, caretIndex);
        var characters = curText.ToList();
        for (int i = 0; i < UsedCharacters.Count; i++)
        {
            var c = characters[i];
            if (caretIndex < 0 || caretIndex >= characters.Count) characters.Add(c);
            else
            {
                characters.Insert(caretIndex, c);

            }
            caretIndex++;
        }
        return (new string(characters.ToArray()), caretIndex);
    }

    /// <summary>
    /// Determines if the keyboard was used based on button presses.
    /// </summary>
    private void WasKeyboardUsed(float dt, bool wasOtherDeviceUsed, out bool used, out bool usedRaw)
    {
        used = false;
        usedRaw = false;
        
        if (isLocked) return;
        
        usedRaw = PressedButtons.Count > 0;
        
        
        if (wasOtherDeviceUsed)
        {
            usedDurationTimer = 0f;
            pressedCount = 0;
            pressedCountDurationTimer = 0f;
            wasSelected = false;
            return;
        }
        
        if (!UsageDetectionSettings.Detection) return;
        
        //if device was selected in the previous update cycle, it will be automatically considered used if at least one button is held down
        if (wasSelected)
        {
            if (HeldDownButtons.Count > 0)
            {
                used = true;
                return;
            }
            
            //no button was held down
            wasSelected = false;
        }
            
        if (UsageDetectionSettings.SelectionButtons is { Count: > 0 })
        {
            if (UsageDetectionSettings.ExceptionButtons is { Count: > 0 })
            {
                foreach (var button in UsageDetectionSettings.SelectionButtons)
                {
                    if (UsageDetectionSettings.ExceptionButtons.Contains(button)) continue;
                    if (Raylib.IsKeyDown((KeyboardKey)button)) //shortcut -> IsDown would call the same thing
                    {
                        used = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (var button in UsageDetectionSettings.SelectionButtons)
                {
                    if (Raylib.IsKeyDown((KeyboardKey)button)) //shortcut -> IsDown would call the same thing
                    {
                        used = true;
                        break;
                    }
                }
            }
        }
        else
        {
            var pressCountEnabled = UsageDetectionSettings.PressCountEnabled;
            var usedDurationEnabled = UsageDetectionSettings.UsedDurationEnabled;
            
            if (pressCountEnabled)
            {
                pressedCountDurationTimer += dt;
                if (pressedCountDurationTimer >= UsageDetectionSettings.MinPressInterval)
                {
                    pressedCountDurationTimer -= UsageDetectionSettings.MinPressInterval;
                    pressedCount = 0;
                }
            }

            if (usedDurationEnabled)
            {
                // Checks if any held down button is not in the exception list (or if the exception list is null)
                if (HeldDownButtons.Any(b => !UsageDetectionSettings.ExceptionButtons.Contains(b)))
                {
                    usedDurationTimer += dt;
                    if (usedDurationTimer > UsageDetectionSettings.MinUsedDuration)
                    {
                        used = true;
                        usedDurationTimer -= UsageDetectionSettings.MinUsedDuration;
                        pressedCount = 0;
                        pressedCountDurationTimer = 0f;
                    }
                }
                else if (usedDurationTimer > 0f) usedDurationTimer = 0f;
            }
            
            //if used is true, it means that used duration timer has been triggered and pressed count state was reset and it should not be checked this frame again
            if (!used && pressCountEnabled && PressedButtons.Any(b => !UsageDetectionSettings.ExceptionButtons.Contains(b)))
            {
                pressedCount++;
                if (pressedCount >= UsageDetectionSettings.MinPressCount)
                {
                    used = true;
                    pressedCountDurationTimer = 0f;
                    usedDurationTimer = 0f;
                    pressedCount = 0;
                }
            }
        }

        // if the device was used this frame it will be pre-selected for the next frame
        if (used)
        {
            wasSelected = true;
        }
    } 
    
    /// <summary>
    /// Updates the states of all keyboard buttons.
    /// </summary>
    private void UpdateButtonStates()
    {
        foreach (var state in buttonStates)
        {
            var button = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(button);
            var nextState = new InputState(prevState, curState);
            buttonStates[button] = nextState;
            
            if(nextState.Down) HeldDownButtons.Add(button);

            if (nextState.Pressed)
            {
                PressedButtons.Add(button);
                OnButtonPressed?.Invoke(button);
            }
            else if (nextState.Released)
            {
                ReleasedButtons.Add(button);
                OnButtonReleased?.Invoke(button);
            }
        }
    }
    
    #region Button
    /// <summary>
    /// Gets the value of the specified keyboard button, considering the optional modifier key set.
    /// Returns 1.0f if the button is down and modifiers (if any) are active; otherwise, returns 0.0f.
    /// </summary>
    /// <param name="button">The keyboard button to check.</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active.</param>
    /// <returns>1.0f if the button is down and modifiers are active; otherwise, 0.0f.</returns>
    public float GetValue(ShapeKeyboardButton button, ModifierKeySet? modifierKeySet = null)
    {
        if (isLocked) return 0f;
        if (modifierKeySet != null && !modifierKeySet.IsActive()) return 0f;
        return Raylib.IsKeyDown((KeyboardKey)button) ? 1f : 0f;
    }
    
    /// <summary>
    /// Determines whether the specified keyboard button is currently down,
    /// considering the optional modifier key set.
    /// </summary>
    /// <param name="button">The keyboard button to check.</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active.</param>
    /// <returns>True if the button is down and modifiers are active; otherwise, false.</returns>
    public bool IsDown(ShapeKeyboardButton button, ModifierKeySet? modifierKeySet = null)
    {
        return GetValue(button, modifierKeySet) != 0f;
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified keyboard button,
    /// considering the optional modifier key set.
    /// </summary>
    /// <param name="button">The keyboard button to check.</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active.</param>
    /// <returns>The input state for the button.</returns>
    public InputState CreateInputState(ShapeKeyboardButton button, ModifierKeySet? modifierKeySet = null)
    {
        bool down = IsDown(button, modifierKeySet);
        return new(down, !down, down ? 1f : 0f, -1, InputDeviceType.Keyboard);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified keyboard button,
    /// based on the previous state and considering the optional modifier key set.
    /// </summary>
    /// <param name="button">The keyboard button to check.</param>
    /// <param name="previousState">The previous input state.</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active.</param>
    /// <returns>The new input state for the button.</returns>
    public InputState CreateInputState(ShapeKeyboardButton button, InputState previousState, ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(button, modifierKeySet));
    }
    #endregion
    
    #region Button Axis
    
    /// <summary>
    /// Gets the axis value based on two keyboard buttons (negative and positive),
    /// considering the optional modifier key set.
    /// Returns 1.0f if the positive button is down, -1.0f if the negative button is down,
    /// or 0.0f if neither or both are down.
    /// </summary>
    /// <param name="neg">The negative direction keyboard button.</param>
    /// <param name="pos">The positive direction keyboard button.</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active.</param>
    /// <returns>The axis value: 1.0f, -1.0f, or 0.0f.</returns>
    public float GetValue(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeySet? modifierKeySet = null)
    {
        if (isLocked) return 0f;
        if (modifierKeySet != null && !modifierKeySet.IsActive()) return 0f;
        float vNegative = Raylib.IsKeyDown((KeyboardKey)neg) ? 1f : 0f;
        float vPositive = Raylib.IsKeyDown((KeyboardKey)pos) ? 1f : 0f;
        return vPositive - vNegative;
    }
    
    /// <summary>
    /// Determines whether either the negative or positive keyboard button is currently down,
    /// considering the optional modifier key set.
    /// </summary>
    /// <param name="neg">The negative direction keyboard button.</param>
    /// <param name="pos">The positive direction keyboard button.</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active.</param>
    /// <returns>True if either button is down and modifiers are active; otherwise, false.</returns>
    public bool IsDown(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeySet? modifierKeySet = null)
    {
        return GetValue(neg, pos, modifierKeySet) != 0f;
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified negative and positive keyboard buttons,
    /// considering the optional modifier key set.
    /// </summary>
    /// <param name="neg">The negative direction keyboard button.</param>
    /// <param name="pos">The positive direction keyboard button.</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active.</param>
    /// <returns>The input state for the button axis.</returns>
    public InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeySet? modifierKeySet = null)
    {
        float axis = GetValue(neg, pos, modifierKeySet);
        bool down = axis != 0f;
        return new(down, !down, axis, -1, InputDeviceType.Keyboard);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for the specified negative and positive keyboard buttons,
    /// based on the previous state and considering the optional modifier key set.
    /// </summary>
    /// <param name="neg">The negative direction keyboard button.</param>
    /// <param name="pos">The positive direction keyboard button.</param>
    /// <param name="previousState">The previous input state.</param>
    /// <param name="modifierKeySet">Optional modifier key set that must be active.</param>
    /// <returns>The new input state for the button axis.</returns>
    public InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, InputState previousState, ModifierKeySet? modifierKeySet = null)
    {
        return new(previousState, CreateInputState(neg, pos, modifierKeySet));
    }
    #endregion
}