
namespace ShapeEngine.Input;
//TODO: rename all input classes / structs that have a Shape prefix! (ShapeKeyboardDevice to KeyboardDevice for instance)
//some things need shape prefix, like ShapeGamepadButton (raylib uses GamepadButton)

/// <summary>
/// Provides static access to input devices (keyboard, mouse, gamepads) and input state queries.
/// Handles device switching, input state retrieval, and device change events.
/// </summary>
public static class ShapeInput
{
    /// <summary>
    /// Gets the current input device type in use.
    /// </summary>
    public static InputDeviceType CurrentInputDeviceType { get; private set; } = InputDeviceType.Keyboard;

    /// <summary>
    /// Gets the current input device type, but returns Keyboard if Mouse is active.
    /// </summary>
    public static InputDeviceType CurrentInputDeviceTypeNoMouse => FilterInputDevice(CurrentInputDeviceType, InputDeviceType.Mouse, InputDeviceType.Keyboard);
    
    /// <summary>
    /// Event triggered when the input device type changes.
    /// </summary>
    public static event Action<InputDeviceType, InputDeviceType>? OnInputDeviceChanged;
    
    /// <summary>
    /// The global keyboard device instance.
    /// </summary>
    public static readonly KeyboardDevice KeyboardDevice = new();
    /// <summary>
    /// The global mouse device instance.
    /// </summary>
    public static readonly MouseDevice MouseDevice = new();
    /// <summary>
    /// The global gamepad device manager instance.
    /// </summary>
    public static readonly GamepadDeviceManager GamepadDeviceManager = new();
    /// <summary>
    /// The global input event handler instance.
    /// </summary>
    public static readonly InputEventHandler EventHandler = new(KeyboardDevice, MouseDevice, GamepadDeviceManager);


    private static float inputDeviceSelectionCooldownTimer = 0f;
    /// <summary>
    /// Indicates whether the input device selection cooldown is currently active.
    /// Returns true if the cooldown timer is greater than zero.
    /// </summary>
    public static bool InputDeviceSelectionCooldownActive => inputDeviceSelectionCooldownTimer > 0f;
    
    // /// <summary>
    // /// Gets or sets the global input device change settings.
    // /// </summary>
    // public static InputDeviceUsageDetectionSettings InputDeviceUsageDetectionSettings { get; private set; } = new();
    
    /// <summary>
    /// Applies the <see cref="InputDeviceUsageDetectionSettings"/> to all input devices.
    /// </summary>
    /// <param name="settings">The new input device change settings to apply.</param>
    /// <remarks>
    /// Settings are applied to <see cref="MouseDevice"/>,
    /// <see cref="KeyboardDevice"/>,
    /// and <see cref="GamepadDeviceManager"/> that applies the settings to all <see cref="GamepadDevice"/>s.
    /// </remarks>
    public static void ApplyInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings)
    {
        // InputDeviceUsageDetectionSettings = settings;
        MouseDevice.ApplyInputDeviceChangeSettings(settings);
        KeyboardDevice.ApplyInputDeviceChangeSettings(settings);
        GamepadDeviceManager.ApplyInputDeviceChangeSettings(settings);
    }
    
    /// <summary>
    /// Gets the input state for a keyboard button.
    /// </summary>
    public static InputState GetInputState(this ShapeKeyboardButton button) => KeyboardDevice.GetButtonState(button);
    /// <summary>
    /// Gets the input state for a mouse button.
    /// </summary>
    public static InputState GetInputState(this ShapeMouseButton button) => MouseDevice.GetButtonState(button);
    /// <summary>
    /// Gets the input state for a mouse axis.
    /// </summary>
    public static InputState GetInputState(this ShapeMouseAxis axis) => MouseDevice.GetAxisState(axis);
    /// <summary>
    /// Gets the input state for a mouse wheel axis.
    /// </summary>
    public static InputState GetInputState(this ShapeMouseWheelAxis axis) => MouseDevice.GetWheelAxisState(axis);
    /// <summary>
    /// Gets the input state for a gamepad button on a specific gamepad.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <returns>The input state for the button.</returns>
    public static InputState GetInputState(this ShapeGamepadButton button, int gamepadIndex)
    {
        var gamepad = GamepadDeviceManager.GetGamepad(gamepadIndex);

        if (gamepad == null) return new();

        return gamepad.GetButtonState(button);
    }
    /// <summary>
    /// Gets the input state for a gamepad axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The gamepad axis.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <returns>The input state for the axis.</returns>
    public static InputState GetInputState(this ShapeGamepadAxis axis, int gamepadIndex)
    {
        var gamepad = GamepadDeviceManager.GetGamepad(gamepadIndex);

        if (gamepad == null) return new();

        return gamepad.GetAxisState(axis);
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
        var wasOtherDeviceUsed = KeyboardDevice.Update(dt, false);
        if (wasOtherDeviceUsed)
        {
            usedInputDevice = InputDeviceType.Keyboard;   
        }
        wasOtherDeviceUsed = GamepadDeviceManager.Update(dt, wasOtherDeviceUsed);
        if (usedInputDevice == InputDeviceType.None && wasOtherDeviceUsed)
        {
            usedInputDevice = InputDeviceType.Gamepad;
        }
        wasOtherDeviceUsed = MouseDevice.Update(dt, wasOtherDeviceUsed);
        if (usedInputDevice == InputDeviceType.None && wasOtherDeviceUsed)
        {
            usedInputDevice = InputDeviceType.Mouse;
        }

        if (usedInputDevice != InputDeviceType.None)
        {
            //Update cooldown timer if device was used again
            if (selectionCooldownActive)
            {
                if (usedInputDevice == CurrentInputDeviceType)
                {
                    var deviceCooldown = 0f;
                
                    if (usedInputDevice == InputDeviceType.Keyboard) deviceCooldown = KeyboardDevice.UsageDetectionSettings.SelectionCooldownDuration;
                    else if (usedInputDevice == InputDeviceType.Gamepad) deviceCooldown = GamepadDeviceManager.UsageDetectionSettings.SelectionCooldownDuration;
                    else deviceCooldown = MouseDevice.UsageDetectionSettings.SelectionCooldownDuration;
               
                    inputDeviceSelectionCooldownTimer = deviceCooldown > 0f ? deviceCooldown : inputDeviceSelectionCooldownTimer;
                }
            }
            
            //set new cooldown timer if device was used
            else
            {
                var deviceCooldown = 0f;
                
                if (usedInputDevice == InputDeviceType.Keyboard) deviceCooldown = KeyboardDevice.UsageDetectionSettings.SelectionCooldownDuration;
                else if (usedInputDevice == InputDeviceType.Gamepad) deviceCooldown = GamepadDeviceManager.UsageDetectionSettings.SelectionCooldownDuration;
                else deviceCooldown = MouseDevice.UsageDetectionSettings.SelectionCooldownDuration;
               
                float cooldown = deviceCooldown > 0f ? deviceCooldown : 0f;
                
                //Update CurInputDeviceType
                var prevInputDevice = CurrentInputDeviceType;
                if (prevInputDevice != usedInputDevice)
                {
                    CurrentInputDeviceType = usedInputDevice;
                    OnInputDeviceChanged?.Invoke(prevInputDevice, CurrentInputDeviceType);
                }
                
                //Set Cooldown
                if (cooldown > 0f)
                {
                    inputDeviceSelectionCooldownTimer = cooldown;
                }
            }
        }
    }

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
    
    // /// <summary>
    // /// Checks for input device changes based on recent device usage and updates <see cref="CurrentInputDeviceType"/>.
    // /// Triggers <see cref="OnInputDeviceChanged"/> if the device type changes.
    // /// </summary>
    // private static void CheckInputDevice()
    // {
    //     var prevInputDevice = CurrentInputDeviceType;
    //     if (CurrentInputDeviceType == InputDeviceType.Keyboard)
    //     {
    //         if (MouseDevice.WasUsed()) CurrentInputDeviceType = InputDeviceType.Mouse;
    //         else
    //         {
    //             if (GamepadDeviceManager.LastUsedGamepads.Count > 0)
    //             {
    //                 CurrentInputDeviceType = InputDeviceType.Gamepad;
    //             }
    //         }
    //     }
    //     else if (CurrentInputDeviceType == InputDeviceType.Mouse)
    //     {
    //         if (KeyboardDevice.WasUsed()) CurrentInputDeviceType = InputDeviceType.Keyboard;
    //         else
    //         {
    //             if (GamepadDeviceManager.LastUsedGamepads.Count > 0)
    //             {
    //                 CurrentInputDeviceType = InputDeviceType.Gamepad;
    //             }
    //         }
    //     }
    //     else //gamepad
    //     {
    //         if (MouseDevice.WasUsed()) CurrentInputDeviceType = InputDeviceType.Mouse;
    //         else if (KeyboardDevice.WasUsed()) CurrentInputDeviceType = InputDeviceType.Keyboard;
    //     }
    //
    //     if (CurrentInputDeviceType != prevInputDevice)
    //     {
    //         OnInputDeviceChanged?.Invoke(prevInputDevice, CurrentInputDeviceType);
    //     }
    // }
    #endregion

}
