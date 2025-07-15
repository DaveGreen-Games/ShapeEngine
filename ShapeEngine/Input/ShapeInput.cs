
namespace ShapeEngine.Input;

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
