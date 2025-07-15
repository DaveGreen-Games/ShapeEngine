
namespace ShapeEngine.Input;

//Q: should this system have lock / access tag system as well?
// Should it have a blacklist system for input types? Update could reset the blacklist every frame and the CreateInputState methods register the used input type and check if it already exists?
    
// !!!: Implement a InputType state class ?
// Has list of InputTypes and also saves prev state to create new state? Is the same system as InputAction, just way simpler...
// this is the same as InputAction.... therefore redundant and not necessary.

/// <summary>
/// Provides static access to input devices (keyboard, mouse, gamepads) and input state queries.
/// Handles device switching, input state retrieval, and device change events.
/// </summary>
public static class ShapeInput
{
    /// <summary>
    /// Gets the current input device type in use.
    /// </summary>
    public static InputDeviceType CurrentInputDeviceType { get; private set; }

    /// <summary>
    /// Gets the current input device type, but returns Keyboard if Mouse is active.
    /// </summary>
    public static InputDeviceType CurrentInputDeviceTypeNoMouse => FilterInputDevice(CurrentInputDeviceType, InputDeviceType.Mouse, InputDeviceType.Keyboard);
    
    /// <summary>
    /// Event triggered when the input device type changes.
    /// </summary>
    public static event Action<InputDeviceType, InputDeviceType>? OnInputDeviceChanged;
    
    /// <summary>
    /// The default global keyboard device instance.
    /// </summary>
    public static readonly KeyboardDevice DefaultKeyboardDevice;
    
    /// <summary>
    /// The default global mouse device instance.
    /// </summary>
    public static readonly MouseDevice DefaultMouseDevice;
    
    /// <summary>
    /// The default global gamepad device manager instance.
    /// </summary>
    public static readonly GamepadDeviceManager DefaultGamepadDeviceManager;
    
    /// <summary>
    /// The global keyboard device instance.
    /// </summary>
    public static KeyboardDevice ActiveKeyboardDevice { get; private set; }
    
    /// <summary>
    /// The global mouse device instance.
    /// </summary>
    public static MouseDevice ActiveMouseDevice { get; private set; }
    
    /// <summary>
    /// The global gamepad device manager instance.
    /// </summary>
    public static GamepadDeviceManager ActiveGamepadDeviceManager { get; private set; }
    
    /// <summary>
    /// The global input event handler instance.
    /// </summary>
    public static readonly InputEventHandler EventHandler;
    
    /// <summary>
    /// Indicates whether the input device selection cooldown is currently active.
    /// Returns true if the cooldown timer is greater than zero.
    /// </summary>
    public static bool InputDeviceSelectionCooldownActive => inputDeviceSelectionCooldownTimer > 0f;
    private static float inputDeviceSelectionCooldownTimer;

    static ShapeInput()
    {
        CurrentInputDeviceType = InputDeviceType.Keyboard;
        
        DefaultKeyboardDevice = new();
        DefaultMouseDevice = new();
        DefaultGamepadDeviceManager = new();
        
        ActiveKeyboardDevice = DefaultKeyboardDevice;
        ActiveMouseDevice = DefaultMouseDevice;
        ActiveGamepadDeviceManager = DefaultGamepadDeviceManager;
        
        ActiveKeyboardDevice.Activate();
        ActiveMouseDevice.Activate();
        ActiveGamepadDeviceManager.Activate();
        
        EventHandler = new(ActiveKeyboardDevice, ActiveMouseDevice, ActiveGamepadDeviceManager);
    }
    
    /// <summary>
    /// Changes the active mouse device.
    /// </summary>
    /// <param name="device">The new mouse device to set as active.</param>
    /// <returns>True if the device was changed; otherwise, false.</returns>
    public static bool ChangeActiveMouseDevice(MouseDevice device)
    {
        if (device == ActiveMouseDevice) return false;
        
        bool changed = EventHandler.ChangeActiveMouseDevice(device);
        if (!changed) return false;
        
        ActiveMouseDevice.Deactivate();
        ActiveMouseDevice = device;
        ActiveMouseDevice.Activate();
        
        return true;
    }
    
    /// <summary>
    /// Changes the active keyboard device.
    /// </summary>
    /// <param name="device">The new keyboard device to set as active.</param>
    /// <returns>True if the device was changed; otherwise, false.</returns>
    public static bool ChangeActiveKeybordDevice(KeyboardDevice device)
    {
        if (device == ActiveKeyboardDevice) return false;
        
        bool changed = EventHandler.ChangeActiveKeyboardDevice(device);
        if (!changed) return false;
        
        ActiveKeyboardDevice.Deactivate();
        ActiveKeyboardDevice = device;
        ActiveKeyboardDevice.Activate();
        
        return true;
    }
    
    /// <summary>
    /// Changes the active gamepad device manager.
    /// </summary>
    /// <param name="deviceManager">The new gamepad device manager to set as active.</param>
    /// <returns>True if the device manager was changed; otherwise, false.</returns>
    public static bool ChangeActiveGamepadDeviceManager(GamepadDeviceManager deviceManager)
    {
        if (deviceManager == ActiveGamepadDeviceManager) return false;
        
        bool changed = EventHandler.ChangeActiveGamepadDeviceManager(deviceManager);
        if (!changed) return false;
        
        ActiveGamepadDeviceManager.Deactivate();
        ActiveGamepadDeviceManager = deviceManager;
        ActiveGamepadDeviceManager.Activate();
        
        return true;
    }
    
    /// <summary>
    /// Applies the <see cref="InputDeviceUsageDetectionSettings"/> to all input devices.
    /// </summary>
    /// <param name="settings">The new input device change settings to apply.</param>
    /// <remarks>
    /// Settings are applied to <see cref="ActiveMouseDevice"/>,
    /// <see cref="ActiveKeyboardDevice"/>,
    /// and <see cref="ActiveGamepadDeviceManager"/> that applies the settings to all <see cref="GamepadDevice"/>s.
    /// </remarks>
    public static void ApplyInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings)
    {
        // InputDeviceUsageDetectionSettings = settings;
        ActiveMouseDevice.ApplyInputDeviceChangeSettings(settings);
        ActiveKeyboardDevice.ApplyInputDeviceChangeSettings(settings);
        ActiveGamepadDeviceManager.ApplyInputDeviceChangeSettings(settings);
    }
   
    /// <summary>
    /// Updates all input devices and checks for input device changes.
    /// </summary>
    internal static void Update(float dt)
    {
        if (InputDeviceSelectionCooldownActive)
        {
            inputDeviceSelectionCooldownTimer -= dt;
            if (inputDeviceSelectionCooldownTimer <= 0f)
            {
                inputDeviceSelectionCooldownTimer = 0f;
            }
        }
        
        var selectionCooldownActive = InputDeviceSelectionCooldownActive;
        var usedInputDevice = InputDeviceType.None;
        // Prevents input device switching if another device was already used this frame.
        // For example, keyboard usage can block gamepad and mouse from becoming the active device.
        var wasOtherDeviceUsed = ActiveKeyboardDevice.Update(dt, false);
        if (wasOtherDeviceUsed)
        {
            usedInputDevice = InputDeviceType.Keyboard;   
        }
        wasOtherDeviceUsed = ActiveGamepadDeviceManager.Update(dt, wasOtherDeviceUsed);
        if (usedInputDevice == InputDeviceType.None && wasOtherDeviceUsed)
        {
            usedInputDevice = InputDeviceType.Gamepad;
        }
        wasOtherDeviceUsed = ActiveMouseDevice.Update(dt, wasOtherDeviceUsed);
        if (usedInputDevice == InputDeviceType.None && wasOtherDeviceUsed)
        {
            usedInputDevice = InputDeviceType.Mouse;
        }

        if (usedInputDevice != InputDeviceType.None)
        {
            if(!selectionCooldownActive && usedInputDevice != CurrentInputDeviceType)
            {
                var prevInputDevice = CurrentInputDeviceType;
                CurrentInputDeviceType = usedInputDevice;
                OnInputDeviceChanged?.Invoke(prevInputDevice, CurrentInputDeviceType);
                
                float deviceCooldown;
                
                if (usedInputDevice == InputDeviceType.Keyboard) deviceCooldown = ActiveKeyboardDevice.UsageDetectionSettings.SelectionCooldownDuration;
                else if (usedInputDevice == InputDeviceType.Gamepad) deviceCooldown = ActiveGamepadDeviceManager.UsageDetectionSettings.SelectionCooldownDuration;
                else deviceCooldown = ActiveMouseDevice.UsageDetectionSettings.SelectionCooldownDuration;
                
                if (deviceCooldown > 0f)
                {
                    inputDeviceSelectionCooldownTimer = deviceCooldown;
                }
            }
        }
    }
    
    #region Get InputState Methods
    /// <summary>
    /// Gets the input state for a keyboard button.
    /// </summary>
    public static InputState GetInputState(this ShapeKeyboardButton button) => ActiveKeyboardDevice.GetButtonState(button);
    /// <summary>
    /// Gets the input state for a mouse button.
    /// </summary>
    public static InputState GetInputState(this ShapeMouseButton button) => ActiveMouseDevice.GetButtonState(button);
    /// <summary>
    /// Gets the input state for a mouse axis.
    /// </summary>
    public static InputState GetInputState(this ShapeMouseAxis axis) => ActiveMouseDevice.GetAxisState(axis);
    /// <summary>
    /// Gets the input state for a mouse wheel axis.
    /// </summary>
    public static InputState GetInputState(this ShapeMouseWheelAxis axis) => ActiveMouseDevice.GetWheelAxisState(axis);
    /// <summary>
    /// Gets the input state for a gamepad button on a specific gamepad.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <returns>The input state for the button.</returns>
    public static InputState GetInputState(this ShapeGamepadButton button, int gamepadIndex)
    {
       var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);

       return gamepad?.GetButtonState(button) ?? new();
    }
    /// <summary>
    /// Gets the input state for a gamepad axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The gamepad axis.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <returns>The input state for the axis.</returns>
    public static InputState GetInputState(this ShapeGamepadAxis axis, int gamepadIndex)
    {
       var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);

       return gamepad?.GetAxisState(axis) ?? new();
    }
    #endregion

    #region Consume InputState Methods
   
    /// <summary>
    /// Consumes the input state for a keyboard button.
    /// </summary>
    /// <param name="button">The keyboard button to consume.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeKeyboardButton button, out bool valid) => ActiveKeyboardDevice.ConsumeButtonState(button, out valid);

    /// <summary>
    /// Consumes the input state for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button to consume.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeMouseButton button, out bool valid) => ActiveMouseDevice.ConsumeButtonState(button, out valid);

    /// <summary>
    /// Consumes the input state for a mouse axis.
    /// </summary>
    /// <param name="axis">The mouse axis to consume.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeMouseAxis axis, out bool valid) => ActiveMouseDevice.ConsumeAxisState(axis, out valid);

    /// <summary>
    /// Consumes the input state for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to consume.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeMouseWheelAxis axis, out bool valid) => ActiveMouseDevice.ConsumeWheelAxisState(axis, out valid);

    /// <summary>
    /// Consumes the input state for a gamepad button on a specific gamepad.
    /// </summary>
    /// <param name="button">The gamepad button to consume.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeGamepadButton button, int gamepadIndex, out bool valid)
    {
       var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
       valid = false;
       return gamepad == null ? new() : gamepad.ConsumeButtonState(button, out valid);
    }

    /// <summary>
    /// Consumes the input state for a gamepad axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The gamepad axis to consume.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeGamepadAxis axis, int gamepadIndex, out bool valid)
    {
       var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
       valid = false;
       return gamepad == null ? new() : gamepad.ConsumeAxisState(axis, out valid);
    }
    #endregion
    
    #region InputDeviceType

    /// <summary>
    /// Returns <paramref name="with"/> if <paramref name="current"/> equals <paramref name="replace"/>, otherwise returns <paramref name="current"/>.
    /// </summary>
    /// <param name="current">Current input device type.</param>
    /// <param name="replace">Device type to replace.</param>
    /// <param name="with">Device type to use as replacement.</param>
    /// <returns>The filtered input device type.</returns>
    public static InputDeviceType FilterInputDevice(InputDeviceType current, InputDeviceType replace, InputDeviceType with)
    {
        return current == replace
            ? with
            : current;
    }

    /// <summary>
    /// Gets the generic name of the current input device type.
    /// </summary>
    /// <returns>"Gamepad", "Keyboard", or "Mouse".</returns>
    public static string GetCurInputDeviceGenericName()
    {
        return
            CurrentInputDeviceType == InputDeviceType.Gamepad ? "Gamepad" :
            CurrentInputDeviceType == InputDeviceType.Keyboard ? "Keyboard" : "Mouse";
    }
    /// <summary>
    /// Gets the generic name for a specified input device type.
    /// </summary>
    /// <param name="deviceType">The input device type.</param>
    /// <returns>"Gamepad", "Keyboard", or "Mouse".</returns>
    public static string GetInputDeviceGenericName(InputDeviceType deviceType)
    {
        return
            deviceType == InputDeviceType.Gamepad ? "Gamepad" :
            deviceType == InputDeviceType.Keyboard ? "Keyboard" : "Mouse";
    }
    
    #endregion

}

/*
 
   #region Input State Methods
   
   #region Create InputState Methods
   
   #region Gamepad
   /// <summary>
   /// Creates an <see cref="InputState"/> for a gamepad button with custom deadzones and modifier keys.
   /// </summary>
   /// <param name="button">The gamepad button.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="axisDeadzone">Deadzone for axes.</param>
   /// <param name="triggerDeadzone">Deadzone for triggers.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadButton button, int gamepadIndex, float axisDeadzone, float triggerDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(button, axisDeadzone, triggerDeadzone, modifierOperator, modifierKeys) ?? new();

   /// <summary>
   /// Creates an <see cref="InputState"/> for a gamepad button with previous state, custom deadzones, and modifier keys.
   /// </summary>
   /// <param name="button">The gamepad button.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="axisDeadzone">Deadzone for axes.</param>
   /// <param name="triggerDeadzone">Deadzone for triggers.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadButton button, int gamepadIndex, InputState previousState, float axisDeadzone, float triggerDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(button, previousState, axisDeadzone, triggerDeadzone, modifierOperator, modifierKeys) ?? new();

   /// <summary>
   /// Creates an <see cref="InputState"/> for a gamepad button with optional deadzones.
   /// </summary>
   /// <param name="button">The gamepad button.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="axisDeadzone">Deadzone for axes (default 0.1f).</param>
   /// <param name="triggerDeadzone">Deadzone for triggers (default 0.1f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadButton button, int gamepadIndex, float axisDeadzone = 0.1f, float triggerDeadzone = 0.1f) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(button, axisDeadzone, triggerDeadzone) ?? new();

   /// <summary>
   /// Creates an <see cref="InputState"/> for a gamepad button with previous state and optional deadzones.
   /// </summary>
   /// <param name="button">The gamepad button.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="axisDeadzone">Deadzone for axes (default 0.1f).</param>
   /// <param name="triggerDeadzone">Deadzone for triggers (default 0.1f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadButton button, int gamepadIndex, InputState previousState, float axisDeadzone = 0.1f, float triggerDeadzone = 0.1f) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(button, previousState, axisDeadzone, triggerDeadzone) ?? new();


   /// <summary>
   /// Creates an <see cref="InputState"/> for a gamepad axis with custom deadzones and modifier keys.
   /// </summary>
   /// <param name="axis">The gamepad axis.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="axisDeadzone">Deadzone for axes.</param>
   /// <param name="triggerDeadzone">Deadzone for triggers.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadAxis axis, int gamepadIndex, float axisDeadzone, float triggerDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(axis, axisDeadzone, triggerDeadzone, modifierOperator, modifierKeys) ?? new();

   /// <summary>
   /// Creates an <see cref="InputState"/> for a gamepad axis with previous state, custom deadzones, and modifier keys.
   /// </summary>
   /// <param name="axis">The gamepad axis.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="axisDeadzone">Deadzone for axes.</param>
   /// <param name="triggerDeadzone">Deadzone for triggers.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadAxis axis, int gamepadIndex, InputState previousState, float axisDeadzone, float triggerDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(axis, previousState, axisDeadzone, triggerDeadzone, modifierOperator, modifierKeys) ?? new();

   /// <summary>
   /// Creates an <see cref="InputState"/> for a gamepad axis with optional deadzones.
   /// </summary>
   /// <param name="axis">The gamepad axis.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="axisDeadzone">Deadzone for axes (default 0.1f).</param>
   /// <param name="triggerDeadzone">Deadzone for triggers (default 0.1f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadAxis axis, int gamepadIndex, float axisDeadzone = 0.1f, float triggerDeadzone = 0.1f) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(axis, axisDeadzone, triggerDeadzone) ?? new();

   /// <summary>
   /// Creates an <see cref="InputState"/> for a gamepad axis with previous state and optional deadzones.
   /// </summary>
   /// <param name="axis">The gamepad axis.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="axisDeadzone">Deadzone for axes (default 0.1f).</param>
   /// <param name="triggerDeadzone">Deadzone for triggers (default 0.1f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadAxis axis, int gamepadIndex, InputState previousState, float axisDeadzone = 0.1f, float triggerDeadzone = 0.1f) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(axis, previousState, axisDeadzone, triggerDeadzone) ?? new();

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of gamepad buttons representing a negative and positive axis, with custom deadzones and modifier keys.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="axisDeadzone">Deadzone for axes.</param>
   /// <param name="triggerDeadzone">Deadzone for triggers.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, int gamepadIndex, float axisDeadzone, float triggerDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(neg, pos, axisDeadzone, triggerDeadzone, modifierOperator, modifierKeys) ?? new();

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of gamepad buttons representing a negative and positive axis, with previous state, custom deadzones, and modifier keys.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="axisDeadzone">Deadzone for axes.</param>
   /// <param name="triggerDeadzone">Deadzone for triggers.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, int gamepadIndex, InputState previousState, float axisDeadzone, float triggerDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(neg, pos, previousState, axisDeadzone, triggerDeadzone, modifierOperator, modifierKeys) ?? new();

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of gamepad buttons representing a negative and positive axis, with optional deadzones.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="axisDeadzone">Deadzone for axes (default 0.1f).</param>
   /// <param name="triggerDeadzone">Deadzone for triggers (default 0.1f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, int gamepadIndex, float axisDeadzone = 0.1f, float triggerDeadzone = 0.1f) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(neg, pos, axisDeadzone, triggerDeadzone) ?? new();

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of gamepad buttons representing a negative and positive axis, with previous state and optional deadzones.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="gamepadIndex">The gamepad index.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="axisDeadzone">Deadzone for axes (default 0.1f).</param>
   /// <param name="triggerDeadzone">Deadzone for triggers (default 0.1f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, int gamepadIndex, InputState previousState, float axisDeadzone = 0.1f, float triggerDeadzone = 0.1f) => ActiveGamepadDeviceManager.GetGamepad(gamepadIndex)?.CreateInputState(neg, pos, previousState, axisDeadzone, triggerDeadzone) ?? new();
   #endregion

   #region Keyboard

   /// <summary>
   /// Creates an <see cref="InputState"/> for a keyboard button.
   /// </summary>
   /// <param name="button">The keyboard button.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeKeyboardButton button) => ActiveKeyboardDevice.CreateInputState(button);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a keyboard button with modifier keys.
   /// </summary>
   /// <param name="button">The keyboard button.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeKeyboardButton button, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveKeyboardDevice.CreateInputState(button, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a keyboard button with previous state and modifier keys.
   /// </summary>
   /// <param name="button">The keyboard button.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeKeyboardButton button, InputState previousState, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveKeyboardDevice.CreateInputState(button, previousState, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a keyboard button with previous state.
   /// </summary>
   /// <param name="button">The keyboard button.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeKeyboardButton button, InputState previousState) => ActiveKeyboardDevice.CreateInputState(button, previousState);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of keyboard buttons representing a negative and positive axis, with modifier keys.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveKeyboardDevice.CreateInputState(neg, pos, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of keyboard buttons representing a negative and positive axis, with previous state and modifier keys.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, InputState previousState, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveKeyboardDevice.CreateInputState(neg, pos, previousState, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of keyboard buttons representing a negative and positive axis.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos) => ActiveKeyboardDevice.CreateInputState(neg, pos);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of keyboard buttons representing a negative and positive axis, with previous state.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, InputState previousState) => ActiveKeyboardDevice.CreateInputState(neg, pos, previousState);

   #endregion

   #region Mouse

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse axis with custom deadzone and modifier keys.
   /// </summary>
   /// <param name="axis">The mouse axis.</param>
   /// <param name="deadzone">Deadzone for the axis.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveMouseDevice.CreateInputState(axis, deadzone, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse axis with previous state, custom deadzone, and modifier keys.
   /// </summary>
   /// <param name="axis">The mouse axis.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="deadzone">Deadzone for the axis.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseAxis axis, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveMouseDevice.CreateInputState(axis, previousState, deadzone, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse axis with optional deadzone.
   /// </summary>
   /// <param name="axis">The mouse axis.</param>
   /// <param name="deadzone">Deadzone for the axis (default 0.5f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseAxis axis, float deadzone = 0.5f) => ActiveMouseDevice.CreateInputState(axis, deadzone);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse axis with previous state and optional deadzone.
   /// </summary>
   /// <param name="axis">The mouse axis.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="deadzone">Deadzone for the axis (default 0.5f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseAxis axis, InputState previousState, float deadzone = 0.5f) => ActiveMouseDevice.CreateInputState(axis, previousState, deadzone);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse wheel axis with custom deadzone and modifier keys.
   /// </summary>
   /// <param name="axis">The mouse wheel axis.</param>
   /// <param name="deadzone">Deadzone for the axis.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveMouseDevice.CreateInputState(axis, deadzone, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse wheel axis with previous state, custom deadzone, and modifier keys.
   /// </summary>
   /// <param name="axis">The mouse wheel axis.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="deadzone">Deadzone for the axis.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveMouseDevice.CreateInputState(axis, previousState, deadzone, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse wheel axis with optional deadzone.
   /// </summary>
   /// <param name="axis">The mouse wheel axis.</param>
   /// <param name="deadzone">Deadzone for the axis (default 0.2f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseWheelAxis axis, float deadzone = 0.2f) => ActiveMouseDevice.CreateInputState(axis, deadzone);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse wheel axis with previous state and optional deadzone.
   /// </summary>
   /// <param name="axis">The mouse wheel axis.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="deadzone">Deadzone for the axis (default 0.2f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseWheelAxis axis, InputState previousState, float deadzone = 0.2f) => ActiveMouseDevice.CreateInputState(axis, previousState, deadzone);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse button with custom move and wheel deadzones and modifier keys.
   /// </summary>
   /// <param name="button">The mouse button.</param>
   /// <param name="mouseMoveDeadzone">Deadzone for mouse movement.</param>
   /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseButton button, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveMouseDevice.CreateInputState(button, mouseMoveDeadzone, mouseWheelDeadzone, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse button with previous state, custom move and wheel deadzones, and modifier keys.
   /// </summary>
   /// <param name="button">The mouse button.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="mouseMoveDeadzone">Deadzone for mouse movement.</param>
   /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseButton button, InputState previousState, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveMouseDevice.CreateInputState(button, previousState, mouseMoveDeadzone, mouseWheelDeadzone, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse button with optional move and wheel deadzones.
   /// </summary>
   /// <param name="button">The mouse button.</param>
   /// <param name="mouseMoveDeadzone">Deadzone for mouse movement (default 0f).</param>
   /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel (default 0f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseButton button, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f) => ActiveMouseDevice.CreateInputState(button, mouseMoveDeadzone, mouseWheelDeadzone);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a mouse button with previous state and optional move and wheel deadzones.
   /// </summary>
   /// <param name="button">The mouse button.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="mouseMoveDeadzone">Deadzone for mouse movement (default 0f).</param>
   /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel (default 0f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseButton button, InputState previousState, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f) => ActiveMouseDevice.CreateInputState(button, previousState, mouseMoveDeadzone, mouseWheelDeadzone);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of mouse buttons representing a negative and positive axis, with custom move and wheel deadzones and modifier keys.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="mouseMoveDeadzone">Deadzone for mouse movement.</param>
   /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveMouseDevice.CreateInputState(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of mouse buttons representing a negative and positive axis, with previous state, custom move and wheel deadzones, and modifier keys.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="mouseMoveDeadzone">Deadzone for mouse movement.</param>
   /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel.</param>
   /// <param name="modifierOperator">Operator for modifier keys.</param>
   /// <param name="modifierKeys">Modifier keys.</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, InputState previousState, float mouseMoveDeadzone, float mouseWheelDeadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys) => ActiveMouseDevice.CreateInputState(neg, pos, previousState, mouseMoveDeadzone, mouseWheelDeadzone, modifierOperator, modifierKeys);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of mouse buttons representing a negative and positive axis, with optional move and wheel deadzones.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="mouseMoveDeadzone">Deadzone for mouse movement (default 0f).</param>
   /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel (default 0f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f) => ActiveMouseDevice.CreateInputState(neg, pos, mouseMoveDeadzone, mouseWheelDeadzone);

   /// <summary>
   /// Creates an <see cref="InputState"/> for a pair of mouse buttons representing a negative and positive axis, with previous state and optional move and wheel deadzones.
   /// </summary>
   /// <param name="neg">Negative direction button.</param>
   /// <param name="pos">Positive direction button.</param>
   /// <param name="previousState">Previous input state.</param>
   /// <param name="mouseMoveDeadzone">Deadzone for mouse movement (default 0f).</param>
   /// <param name="mouseWheelDeadzone">Deadzone for mouse wheel (default 0f).</param>
   /// <returns>The created input state.</returns>
   public static InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, InputState previousState, float mouseMoveDeadzone = 0f, float mouseWheelDeadzone = 0f) => ActiveMouseDevice.CreateInputState(neg, pos, previousState, mouseMoveDeadzone, mouseWheelDeadzone);

   #endregion
   
   #endregion
   
   #endregion
 
 */
