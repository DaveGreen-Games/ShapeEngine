using ShapeEngine.Core.Structs;

namespace ShapeEngine.Input;

/// <summary>
/// Provides static access to input devices (keyboard, mouse, gamepads) and input state queries.
/// Handles device switching, input state retrieval, and device change events.
/// </summary>
public static class ShapeInput
{
    #region Members and Properties
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
    #endregion
    
    #region Constructor
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
    #endregion
    
    #region Input Device Handling

    /// <summary>
    /// Changes the active <see cref="MouseDevice"/>.
    /// </summary>
    /// <param name="device">The new <see cref="MouseDevice"/> to set as active.</param>
    /// <returns>True if the device was changed; otherwise, false.</returns>
    /// <remarks>
    /// This will call <see cref="MouseDevice.Deactivate"/> on the current active <see cref="MouseDevice"/> and it will call <see cref="MouseDevice.Activate"/> on the new <see cref="MouseDevice"/>.
    /// This will change the active state of both the current and the new <see cref="MouseDevice"/>.
    /// </remarks>
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
    /// Changes the active <see cref="KeyboardDevice"/>.
    /// </summary>
    /// <param name="device">The new <see cref="KeyboardDevice"/> to set as active.</param>
    /// <returns>True if the device was changed; otherwise, false.</returns>
    /// <remarks>
    /// This will call <see cref="KeyboardDevice.Deactivate"/> on the current active <see cref="KeyboardDevice"/> and it will call <see cref="KeyboardDevice.Activate"/> on the new <see cref="KeyboardDevice"/>.
    /// This will change the active state of both the current and the new <see cref="KeyboardDevice"/>.
    /// </remarks>
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
    /// Changes the active <see cref="GamepadDeviceManager"/>.
    /// </summary>
    /// <param name="deviceManager">The new <see cref="GamepadDeviceManager"/> to set as active.</param>
    /// <returns>True if the device manager was changed; otherwise, false.</returns>
    /// <remarks>
    /// This will call <see cref="GamepadDeviceManager.Deactivate"/> on the current active <see cref="GamepadDeviceManager"/> and it will call <see cref="GamepadDeviceManager.Activate"/> on the new <see cref="GamepadDeviceManager"/>.
    /// This will change the active state of both the current and the new <see cref="GamepadDeviceManager"/>.
    /// This will also deactivate all <see cref="GamepadDevice"/>s in the current <see cref="GamepadDeviceManager"/> and activate all <see cref="GamepadDevice"/>s in the new <see cref="GamepadDeviceManager"/>.
    /// </remarks>
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
    /// Applies the <see cref="InputDeviceUsageDetectionSettings"/> to all currently active input devices.
    /// </summary>
    /// <param name="settings">The new <see cref="InputDeviceUsageDetectionSettings"/> to apply.</param>
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
    #endregion
    
    #region Get InputState Methods

    /// <summary>
    /// Gets the input state for a keyboard button.
    /// </summary>
    /// <param name="button">The keyboard button to query.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed. Default is <see cref="AllAccessTag"/>.</param>
    /// <returns>The input state for the specified keyboard button.</returns>
    public static InputState GetInputState(this ShapeKeyboardButton button, uint accessTag = AllAccessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveKeyboardDevice.GetButtonState(button);
    }
    /// <summary>
    /// Gets the input state for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button to query.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed. Default is <see cref="AllAccessTag"/>.</param>
    /// <returns>The input state for the specified mouse button.</returns>
    public static InputState GetInputState(this ShapeMouseButton button, uint accessTag = AllAccessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.GetButtonState(button);
    }
    /// <summary>
    /// Gets the input state for a mouse axis.
    /// </summary>
    /// <param name="axis">The mouse axis to query.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed. Default is <see cref="AllAccessTag"/>.</param>
    /// <returns>The input state for the specified mouse axis.</returns>
    public static InputState GetInputState(this ShapeMouseAxis axis, uint accessTag = AllAccessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.GetAxisState(axis);
    }
    /// <summary>
    /// Gets the input state for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to query.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed. Default is <see cref="AllAccessTag"/>.</param>
    /// <returns>The input state for the specified mouse wheel axis.</returns>
    public static InputState GetInputState(this ShapeMouseWheelAxis axis, uint accessTag = AllAccessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.GetWheelAxisState(axis);
    }
    /// <summary>
    /// Gets the input state for a gamepad button on a specific gamepad.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.</param>
    /// <returns>The input state for the button.</returns>
    public static InputState GetInputState(this ShapeGamepadButton button, int gamepadIndex, uint accessTag = AllAccessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad?.GetButtonState(button) ?? new();
    }
    /// <summary>
    /// Gets the input state for a gamepad axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The gamepad axis.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.</param>
    /// <returns>The input state for the axis.</returns>
    public static InputState GetInputState(this ShapeGamepadAxis axis, int gamepadIndex, uint accessTag = AllAccessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
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
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeKeyboardButton button, out bool valid, uint accessTag = AllAccessTag)
    {
        valid = false;
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveKeyboardDevice.ConsumeButtonState(button, out valid);
    }

    /// <summary>
    /// Consumes the input state for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button to consume.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeMouseButton button, out bool valid, uint accessTag = AllAccessTag)
    {
        valid = false;
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.ConsumeButtonState(button, out valid);
    }

    /// <summary>
    /// Consumes the input state for a mouse axis.
    /// </summary>
    /// <param name="axis">The mouse axis to consume.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeMouseAxis axis, out bool valid, uint accessTag = AllAccessTag)
    {
        valid = false;
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.ConsumeAxisState(axis, out valid);
    }

    /// <summary>
    /// Consumes the input state for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to consume.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeMouseWheelAxis axis, out bool valid, uint accessTag = AllAccessTag)
    {
        valid = false;
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.ConsumeWheelAxisState(axis, out valid);
    }

    /// <summary>
    /// Consumes the input state for a gamepad button on a specific gamepad.
    /// </summary>
    /// <param name="button">The gamepad button to consume.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeGamepadButton button, int gamepadIndex, out bool valid, uint accessTag = AllAccessTag)
    {
        valid = false;
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.ConsumeButtonState(button, out valid);
    }

    /// <summary>
    /// Consumes the input state for a gamepad axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The gamepad axis to consume.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeGamepadAxis axis, int gamepadIndex, out bool valid, uint accessTag = AllAccessTag)
    {
        valid = false;
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.ConsumeAxisState(axis, out valid);
    }
    
    #endregion
    
    #region Create InputState Methods
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a keyboard button.
    /// </summary>
    /// <param name="button">The keyboard button.</param>
    /// <param name="accessTag">The access tag for input access control. See <see cref="Locked"/> system for more info.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeKeyboardButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveKeyboardDevice.CreateInputState(button);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="accessTag">The access tag for input access control. See <see cref="Locked"/> system for more info.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeMouseButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.CreateInputState(button);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad button.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="accessTag">The access tag for input access control. See <see cref="Locked"/> system for more info.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="deadzone">The deadzone value for input sensitivity.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeGamepadButton button, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad?.CreateInputState(button, deadzone) ?? new();
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a keyboard button axis (negative and positive).
    /// </summary>
    /// <param name="neg">The negative keyboard button.</param>
    /// <param name="pos">The positive keyboard button.</param>
    /// <param name="accessTag">The access tag for input access control. See <see cref="Locked"/> system for more info.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveKeyboardDevice.CreateInputState(neg, pos);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a mouse button axis (negative and positive).
    /// </summary>
    /// <param name="neg">The negative mouse button.</param>
    /// <param name="pos">The positive mouse button.</param>
    /// <param name="accessTag">The access tag for input access control. See <see cref="Locked"/> system for more info.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.CreateInputState(neg, pos);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad button axis (negative and positive).
    /// </summary>
    /// <param name="neg">The negative gamepad button.</param>
    /// <param name="pos">The positive gamepad button.</param>
    /// <param name="accessTag">The access tag for input access control. See <see cref="Locked"/> system for more info.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="deadzone">The deadzone value for input sensitivity.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad?.CreateInputState(neg, pos, deadzone) ?? new();
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The mouse wheel axis.</param>
    /// <param name="accessTag">The access tag for input access control. See <see cref="Locked"/> system for more info.</param>
    /// <param name="deadzone">The deadzone value for input sensitivity.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeMouseWheelAxis axis, uint accessTag, float deadzone = 1f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.CreateInputState(axis, deadzone);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad axis.
    /// </summary>
    /// <param name="axis">The gamepad axis.</param>
    /// <param name="accessTag">The access tag for input access control. See <see cref="Locked"/> system for more info.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="deadzone">The deadzone value for input sensitivity.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeGamepadAxis axis, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad?.CreateInputState(axis, deadzone) ?? new();
    }

    #endregion
    
    #region Input Type Factory

    #region Keyboard
    /// <summary>
    /// Creates an input type for a keyboard button.
    /// </summary>
    public static IInputType CreateInputType(this ShapeKeyboardButton button, IModifierKey? modifierKey = null, ModifierKeyOperator modifierKeyOperator = ModifierKeyOperator.And)
    {
        return modifierKey == null ? new InputTypeKeyboardButton(button) : new InputTypeKeyboardButton(button, modifierKeyOperator, modifierKey);
    }
    /// <summary>
    /// Creates an input type for a keyboard button.
    /// </summary>
    public static IInputType CreateInputType(this ShapeKeyboardButton button,  ModifierKeyOperator modifierKeyOperator, params IModifierKey[] modifierKeys)
    {
        return  new InputTypeKeyboardButton(button, modifierKeyOperator, modifierKeys);
    }
    /// <summary>
    /// Creates an input type for a keyboard button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeKeyboardButton neg, ShapeKeyboardButton pos, IModifierKey? modifierKey = null, ModifierKeyOperator modifierKeyOperator = ModifierKeyOperator.And)
    {
        return modifierKey == null ? new InputTypeKeyboardButtonAxis(neg, pos) : new InputTypeKeyboardButtonAxis(neg, pos,  modifierKeyOperator, modifierKey);
    }
    /// <summary>
    /// Creates an input type for a keyboard button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeyOperator modifierKeyOperator, params IModifierKey[] modifierKeys)
    {
        return new InputTypeKeyboardButtonAxis(neg, pos,  modifierKeyOperator, modifierKeys);
    }
    #endregion
    
    #region Mouse
    /// <summary>
    /// Creates an input type for a mouse button.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseButton button, float deadzone = 0.2f, IModifierKey? modifierKey = null, ModifierKeyOperator modifierKeyOperator = ModifierKeyOperator.And)
    {
        return modifierKey == null ? new InputTypeMouseButton(button, deadzone) : new InputTypeMouseButton(button, deadzone,  modifierKeyOperator, modifierKey);
    }
    /// <summary>
    /// Creates an input type for a mouse button.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseButton button, float deadzone,  ModifierKeyOperator modifierKeyOperator, params IModifierKey[] modifierKeys)
    {
        return  new InputTypeMouseButton(button, deadzone, modifierKeyOperator, modifierKeys);
    }
    /// <summary>
    /// Creates an input type for a mouse button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0.2f, IModifierKey? modifierKey = null, ModifierKeyOperator modifierKeyOperator = ModifierKeyOperator.And)
    {
        return modifierKey == null ? new InputTypeMouseButtonAxis(neg, pos, deadzone) : new InputTypeMouseButtonAxis(neg, pos, deadzone,  modifierKeyOperator, modifierKey);
    }
    /// <summary>
    /// Creates an input type for a mouse button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierKeyOperator, params IModifierKey[] modifierKeys)
    {
        return new InputTypeMouseButtonAxis(neg, pos, deadzone, modifierKeyOperator, modifierKeys);
    }
    /// <summary>
    /// Creates an input type for a mouse wheel axis.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseWheelAxis axis, float deadzone = 0.2f, IModifierKey? modifierKey = null, ModifierKeyOperator modifierKeyOperator = ModifierKeyOperator.And)
    {
        return modifierKey == null ? new InputTypeMouseWheelAxis(axis, deadzone) : new InputTypeMouseWheelAxis(axis, deadzone,  modifierKeyOperator, modifierKey);
    }
    /// <summary>
    /// Creates an input type for a mouse axis.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseAxis axis, float deadzone = 0.2f, IModifierKey? modifierKey = null, ModifierKeyOperator modifierKeyOperator = ModifierKeyOperator.And)
    {
        return modifierKey == null ? new InputTypeMouseAxis(axis, deadzone) : new InputTypeMouseAxis(axis, deadzone,  modifierKeyOperator, modifierKey);
    }
    /// <summary>
    /// Creates an input type for a mouse wheel axis.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseWheelAxis axis, float deadzone, ModifierKeyOperator modifierKeyOperator, params IModifierKey[] modifierKeys)
    {
        return new InputTypeMouseWheelAxis(axis, deadzone,  modifierKeyOperator, modifierKeys);
    }
    /// <summary>
    /// Creates an input type for a mouse axis.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseAxis axis, float deadzone, ModifierKeyOperator modifierKeyOperator, params IModifierKey[] modifierKeys)
    {
        return new InputTypeMouseAxis(axis, deadzone,  modifierKeyOperator, modifierKeys);
    }
    #endregion
    
    #region Gamepad
    /// <summary>
    /// Creates an input type for a gamepad button.
    /// </summary>
    public static IInputType CreateInputType(this ShapeGamepadButton button, float deadzone = 0.2f, IModifierKey? modifierKey = null, ModifierKeyOperator modifierKeyOperator = ModifierKeyOperator.And)
    {
        return modifierKey == null ? new InputTypeGamepadButton(button, deadzone) : new InputTypeGamepadButton(button, deadzone,  modifierKeyOperator, modifierKey);
    }
    /// <summary>
    /// Creates an input type for a gamepad button.
    /// </summary>
    public static IInputType CreateInputType(this ShapeGamepadButton button, float deadzone,  ModifierKeyOperator modifierKeyOperator, params IModifierKey[] modifierKeys)
    {
        return  new InputTypeGamepadButton(button, deadzone, modifierKeyOperator, modifierKeys);
    }
    /// <summary>
    /// Creates an input type for a gamepad button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f, IModifierKey? modifierKey = null, ModifierKeyOperator modifierKeyOperator = ModifierKeyOperator.And)
    {
        return modifierKey == null ? new InputTypeGamepadButtonAxis(neg, pos, deadzone) : new InputTypeGamepadButtonAxis(neg, pos, deadzone,  modifierKeyOperator, modifierKey);
    }
    /// <summary>
    /// Creates an input type for a gamepad button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone, ModifierKeyOperator modifierKeyOperator, params IModifierKey[] modifierKeys)
    {
        return new InputTypeGamepadButtonAxis(neg, pos, deadzone, modifierKeyOperator, modifierKeys);
    }
    /// <summary>
    /// Creates an input type for a gamepad axis.
    /// </summary>
    public static IInputType CreateInputType(this ShapeGamepadAxis axis, float deadzone = 0.2f, IModifierKey? modifierKey = null, ModifierKeyOperator modifierKeyOperator = ModifierKeyOperator.And)
    {
        return modifierKey == null ? new InputTypeGamepadAxis(axis, deadzone) : new InputTypeGamepadAxis(axis, deadzone,  modifierKeyOperator, modifierKey);
    }
    /// <summary>
    /// Creates an input type for a gamepad axis.
    /// </summary>
    public static IInputType CreateInputType(this ShapeGamepadAxis axis, float deadzone,  ModifierKeyOperator modifierKeyOperator, params IModifierKey[] modifierKeys)
    {
        return new InputTypeGamepadAxis(axis, deadzone,  modifierKeyOperator, modifierKeys);
    }
    #endregion
    
    #endregion
    
    #region Lock System
    
    /// <summary>
    /// This access tag grants access regardless of the input system lock.
    /// </summary>
    public const uint AllAccessTag = 1; // 2^0 (2 to the power of 0)

    /// <summary>
    /// The default access tag for actions.
    /// </summary>
    public const uint DefaultAccessTag = 2; // 2^1 (2 to the power of 1)

    /// <summary>
    /// Indicates if the input system is currently locked.
    /// When set to <c>true</c>, <see cref="InputAction"/>s and other input requests will only be processed
    /// if they have the <see cref="AllAccessTag"/> or their access tag is contained in the lock whitelist.
    /// All <see cref="InputAction"/>s and input request with an access tag contained in the lock blacklist will not be processed.
    /// </summary>
    public static bool Locked { get; private set; }

    private static BitFlag lockWhitelist;
    private static BitFlag lockBlacklist;
    private static uint tagPowerCounter = 2; //0 and 1 are reserved for AllAccessTag and DefaultAccessTag respectively.

    /// <summary>
    /// Gets the next available access tag.
    /// <c>1</c> is reserved for <see cref="AllAccessTag"/>, <c>2</c> is reserved for <see cref="DefaultAccessTag"/>.
    /// Only power of 2 values are used for access tags.
    /// </summary>
    public static uint NextAccessTag => BitFlag.GetPowerOfTwo(tagPowerCounter++);
    
    /// <summary>
    /// Locks the input system, clearing all whitelists and blacklists.
    /// <remarks>
    /// Only <see cref="InputAction"/>s and input requests with <see cref="AllAccessTag"/> will be processed.
    /// </remarks>
    /// </summary>
    public static void Lock()
    {
        Locked = true;
        lockWhitelist = BitFlag.Empty;
        lockBlacklist = BitFlag.Empty;
    }

    /// <summary>
    /// Locks the input system with a specific whitelist and blacklist.
    /// <remarks>
    /// All <see cref="InputAction"/>s and other input requests with a tag contained in the whitelist or with the <see cref="AllAccessTag"/> will be processed.
    /// All <see cref="InputAction"/>s and other input requests with a tag contained in the blacklist will not be processed.
    /// </remarks>
    /// </summary>
    /// <param name="whitelist">The whitelist of access tags.</param>
    /// <param name="blacklist">The blacklist of access tags.</param>
    public static void Lock(BitFlag whitelist, BitFlag blacklist)
    {
        Locked = true;
        lockWhitelist = whitelist;
        lockBlacklist = blacklist;
    }

    /// <summary>
    /// Locks the input system with a specific whitelist.
    /// <remarks>
    /// All <see cref="InputAction"/>s and other input requests with a tag contained in the whitelist or with the <see cref="AllAccessTag"/> will be processed.
    /// </remarks>
    /// </summary>
    /// <param name="whitelist">The whitelist of access tags.</param>
    public static void LockWhitelist(BitFlag whitelist)
    {
        Locked = true;
        lockWhitelist = whitelist;
        lockBlacklist = BitFlag.Empty;
    }

    /// <summary>
    /// Locks the input system with a specific blacklist.
    /// <remarks>
    /// All <see cref="InputAction"/>s and other input requests with a tag contained in the blacklist will not be processed.
    /// </remarks>
    /// </summary>
    /// <param name="blacklist">The blacklist of access tags.</param>
    public static void LockBlacklist(BitFlag blacklist)
    {
        Locked = true;
        lockBlacklist = blacklist;
        lockWhitelist = BitFlag.Empty;
    }

    /// <summary>
    /// Unlocks the input system, clearing all whitelists and blacklists.
    /// </summary>
    public static void Unlock()
    {
        Locked = false;
        lockWhitelist = BitFlag.Empty;
        lockBlacklist = BitFlag.Empty;
    }

    /// <summary>
    /// Determines if the specified access tag has access.
    /// <remarks>
    /// <see cref="AllAccessTag"/> always returns true (has access).
    /// <list type="bullet">
    /// <item>If <c>tag</c> is contained in the current blacklist, this function will return false (no access).</item>
    /// <item>If <c>tag</c> is not contained in the current blacklist and <c>tag</c> is contained in the current whitelist,
    /// or the current whitelist is empty, this function will return true (has access).</item>
    /// </list>
    /// </remarks>
    /// </summary>
    /// <param name="tag">The access tag to check.</param>
    /// <returns>True if access is granted; otherwise, false.</returns>
    public static bool HasAccess(uint tag)
    {
        if (tag == AllAccessTag) return true;
        return (lockWhitelist.IsEmpty() || lockWhitelist.Has(tag)) && !lockBlacklist.Has(tag);
    }

    /// <summary>
    /// Determines if input is available for the specified access tag.
    /// <remarks>
    /// Always returns true if <see cref="Locked"/> is false.
    /// Otherwise returns <see cref="HasAccess(uint)"/> with the <c>tag</c> parameter.
    /// </remarks>
    /// </summary>
    /// <param name="tag">The access tag to check.</param>
    /// <returns>True if input is available; otherwise, false.</returns>
    public static bool IsInputAvailable(uint tag)
    {
        if (!Locked) return true;
        return HasAccess(tag);
    }

    /// <summary>
    /// Determines if the specified action has access.
    /// </summary>
    /// <param name="action">The input action to check.</param>
    /// <returns>True if access is granted; otherwise, false.</returns>
    public static bool HasAccess(InputAction action) => HasAccess(action.AccessTag);

    #endregion
}
