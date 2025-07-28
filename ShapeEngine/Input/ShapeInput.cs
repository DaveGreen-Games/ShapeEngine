using System.Text;
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
    /// A global collection of <see cref="InputActionTree"/> instances.
    /// Each tree manages a hierarchy of input actions and their bindings.
    /// </summary>
    public static readonly InputActionTreeGroup ActiveInputActionTreeGroup = [];
    
    
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
    /// Current <see cref="InputAction"/> based device type detected by <see cref="ActiveInputActionTreeGroup"/>.
    /// The first non-None <see cref="InputDeviceType"/> detected during the update of the <see cref="ActiveInputActionTreeGroup"/> is used.
    /// </summary>
    /// <remarks>
    /// For an <see cref="InputAction"/>s <see cref="IInputType"/>s <see cref="InputDeviceType"/> to be considered the following conditions must be met:
    /// <list type="bullet">
    /// <item>The <see cref="InputAction"/> must be part of an <see cref="InputActionTree"/> within the <see cref="ActiveInputActionTreeGroup"/>.</item>
    /// <item>The <see cref="InputAction"/> must be active.</item>
    /// <item>The <see cref="IInputType"/>'s <see cref="InputState"/> must be down.</item>
    /// <item>The <see cref="IInputType"/> must not be blocked (when <see cref="InputAction.BlocksInput"/> is <c>true</c>).</item>
    /// </list>
    /// </remarks>
    public static InputDeviceType CurrentInputActionDeviceType { get; private set; }
    
    /// <summary>
    /// Event triggered when <see cref="CurrentInputActionDeviceType"/> changes.
    /// </summary>
    /// <remarks>
    /// The event provides the previous and new input action device types as parameters.
    /// </remarks>
    public static event Action<InputDeviceType, InputDeviceType>? OnInputActionDeviceTypeChanged;
    
    
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
    
    /// <summary>
    /// Indicates whether the input action device selection cooldown is currently active.
    /// Returns true if the cooldown timer is greater than zero.
    /// </summary>
    public static bool InputActionDeviceSelectionCooldownActive => inputActionDeviceSelectionCooldownTimer > 0f;
    private static float inputActionDeviceSelectionCooldownTimer;
    private static readonly SortedSet<InputDeviceBase> sortedInputDevices = [];
    #endregion
    
    #region Constructor
    static ShapeInput()
    {
        CurrentInputDeviceType = InputDeviceType.Keyboard;
        CurrentInputActionDeviceType = InputDeviceType.None;
        DefaultKeyboardDevice = new();
        DefaultGamepadDeviceManager = new();
        DefaultMouseDevice = new();
        
        ActiveKeyboardDevice = DefaultKeyboardDevice;
        ActiveGamepadDeviceManager = DefaultGamepadDeviceManager;
        ActiveMouseDevice = DefaultMouseDevice;
        
        ActiveKeyboardDevice.Activate();
        ActiveGamepadDeviceManager.Activate();
        ActiveMouseDevice.Activate();
        
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
    public static InputDeviceType FilterInputDevice(this InputDeviceType current, InputDeviceType replace, InputDeviceType with)
    {
        return current == replace
            ? with
            : current;
    }
    
    /// <summary>
    /// Returns the generic name for the specified <see cref="InputDeviceType"/>.
    /// Returns "None" for <see cref="InputDeviceType.None"/>, "Gamepad" for <see cref="InputDeviceType.Gamepad"/>,
    /// "Keyboard" for <see cref="InputDeviceType.Keyboard"/>, and "Mouse" for all other types.
    /// </summary>
    /// <param name="inputDeviceType">The input device type to get the generic name for.</param>
    /// <returns>The generic name as a string.</returns>
    public static string GetInputDeviceTypeGenericName(this InputDeviceType inputDeviceType)
    {
        if (inputDeviceType == InputDeviceType.None) return "None";
        return inputDeviceType switch
        {
            InputDeviceType.Gamepad => "Gamepad",
            InputDeviceType.Keyboard => "Keyboard",
            _ => "Mouse"
        };
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
        
        if (InputActionDeviceSelectionCooldownActive)
        {
            inputActionDeviceSelectionCooldownTimer -= dt;
            if (inputActionDeviceSelectionCooldownTimer <= 0f)
            {
                inputActionDeviceSelectionCooldownTimer = 0f;
            }
        }

        // Re-add active devices to the set each frame to reflect any changes in device instances or their priority.
        sortedInputDevices.Clear(); 
        sortedInputDevices.Add(ActiveKeyboardDevice);
        sortedInputDevices.Add(ActiveMouseDevice);
        sortedInputDevices.Add(ActiveGamepadDeviceManager);
        
        var usedInputDevice = InputDeviceType.None;
        var wasOtherDeviceUsed = false;
        foreach (var inputDevice in sortedInputDevices)
        {
            var prevUsed = wasOtherDeviceUsed;
            wasOtherDeviceUsed = inputDevice.Update(dt, wasOtherDeviceUsed);
            if(wasOtherDeviceUsed && !prevUsed) usedInputDevice = inputDevice.GetDeviceType();
        }
        
        if (usedInputDevice != InputDeviceType.None)
        {
            if(!InputDeviceSelectionCooldownActive && usedInputDevice != CurrentInputDeviceType)
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
        
        var usedInputActionDevice = ActiveInputActionTreeGroup.Update(dt);
        if (usedInputActionDevice != InputDeviceType.None)
        {
            if(!InputActionDeviceSelectionCooldownActive && usedInputActionDevice != CurrentInputActionDeviceType)
            {
                var prevInputActionDevice = CurrentInputActionDeviceType;
                CurrentInputActionDeviceType = usedInputActionDevice;
                OnInputActionDeviceTypeChanged?.Invoke(prevInputActionDevice, CurrentInputActionDeviceType);
                
                float deviceCooldown;
                
                if (usedInputActionDevice == InputDeviceType.Keyboard) deviceCooldown = ActiveKeyboardDevice.UsageDetectionSettings.SelectionCooldownDuration;
                else if (usedInputActionDevice == InputDeviceType.Gamepad) deviceCooldown = ActiveGamepadDeviceManager.UsageDetectionSettings.SelectionCooldownDuration;
                else deviceCooldown = ActiveMouseDevice.UsageDetectionSettings.SelectionCooldownDuration;
                
                if (deviceCooldown > 0f)
                {
                    inputActionDeviceSelectionCooldownTimer = deviceCooldown;
                }
            }
        }

        
    }

    internal static void EndFrame()
    {
        // InputAction.ClearInputTypeBlocklist();
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
    /// Gets the input state for a gamepad joystick axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The joystick axis to query.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="accessTag">
    /// The access tag used to check if the input request has access and can be consumed. Default is <see cref="AllAccessTag"/>.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.
    /// </param>
    /// <returns>The input state for the specified joystick axis.</returns>
    public static InputState GetInputState(this ShapeGamepadJoyAxis axis, int gamepadIndex, uint accessTag = AllAccessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad?.GetAxisState(axis) ?? new();
    }
    /// <summary>
    /// Gets the input state for a gamepad trigger axis (left or right trigger) on a specific gamepad.
    /// </summary>
    /// <param name="axis">The trigger axis to query (e.g., left or right trigger).</param>
    /// <param name="gamepadIndex">The index of the gamepad to query.</param>
    /// <param name="accessTag">
    /// The access tag used to check if the input request has access and can be consumed. Default is <see cref="AllAccessTag"/>.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.
    /// </param>
    /// <returns>The input state for the specified trigger axis on the specified gamepad.</returns>
    public static InputState GetInputState(this ShapeGamepadTriggerAxis axis, int gamepadIndex, uint accessTag = AllAccessTag)
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
    /// Consumes the input state for a gamepad joystick axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The joystick axis to consume.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">
    /// The access tag used to check if the input request has access and can be consumed. Default is <see cref="AllAccessTag"/>.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.
    /// </param>
    /// <returns>The consumed input state for the specified joystick axis.</returns>
    public static InputState ConsumeInputState(this ShapeGamepadJoyAxis axis, int gamepadIndex, out bool valid, uint accessTag = AllAccessTag)
    {
        valid = false;
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.ConsumeAxisState(axis, out valid);
    }
    /// <summary>
    /// Consumes the input state for a gamepad trigger axis (left or right trigger) on a specific gamepad.
    /// </summary>
    /// <param name="axis">The trigger axis to consume (e.g., left or right trigger).</param>
    /// <param name="gamepadIndex">The index of the gamepad to query.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">
    /// The access tag used to check if the input request has access and can be consumed. Default is <see cref="AllAccessTag"/>.
    /// Access is only checked when <see cref="Locked"/> is set to <c>true</c>.
    /// </param>
    /// <returns>The consumed input state for the specified trigger axis on the specified gamepad.</returns>
    public static InputState ConsumeInputState(this ShapeGamepadTriggerAxis axis, int gamepadIndex, out bool valid, uint accessTag = AllAccessTag)
    {
        valid = false;
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.ConsumeAxisState(axis, out valid);
    }
    #endregion
    
    #region Create InputState Methods
    #region Keyboard
    /// <summary>
    /// Creates an <see cref="InputState"/> for a keyboard button.
    /// </summary>
    /// <param name="button">The keyboard button.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeKeyboardButton button, uint accessTag, ModifierKeySet? modifierKeySet = null)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return modifierKeySet == null ? ActiveKeyboardDevice.CreateInputState(button) : ActiveKeyboardDevice.CreateInputState(button, modifierKeySet);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for a keyboard button axis (negative and positive).
    /// </summary>
    /// <param name="neg">The negative keyboard button.</param>
    /// <param name="pos">The positive keyboard button.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, uint accessTag, ModifierKeySet? modifierKeySet = null)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return modifierKeySet == null ? ActiveKeyboardDevice.CreateInputState(neg, pos) : ActiveKeyboardDevice.CreateInputState(neg, pos, modifierKeySet);
    }
    #endregion

    #region Mouse
    /// <summary>
    /// Creates an <see cref="InputState"/> for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="moveDeadzone">Optional deadzone for mouse movement detection.</param>
    /// <param name="wheelDeadzone">Optional deadzone for mouse wheel detection.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeMouseButton button, uint accessTag, 
        float moveDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold,  
        float wheelDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseWheelThreshold,  
        ModifierKeySet? modifierKeySet = null)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.CreateInputState(button, moveDeadzone, wheelDeadzone, modifierKeySet);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a mouse button axis (negative and positive).
    /// </summary>
    /// <param name="neg">The negative mouse button.</param>
    /// <param name="pos">The positive mouse button.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="moveDeadzone">Optional deadzone for mouse movement detection.</param>
    /// <param name="wheelDeadzone">Optional deadzone for mouse wheel detection.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeMouseButton neg, ShapeMouseButton pos, uint accessTag, 
        float moveDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold,  
        float wheelDeadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseWheelThreshold,  
        ModifierKeySet? modifierKeySet = null)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.CreateInputState(neg, pos, moveDeadzone, wheelDeadzone, modifierKeySet);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The mouse wheel axis.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="deadzone">Optional deadzone for mouse wheel detection.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeMouseWheelAxis axis, uint accessTag, 
        float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseWheelThreshold, ModifierKeySet? modifierKeySet = null)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.CreateInputState(axis, deadzone, modifierKeySet);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a mouse axis.
    /// </summary>
    /// <param name="axis">The mouse axis.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="deadzone">Optional deadzone for mouse movement detection.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeMouseAxis axis, uint accessTag, 
        float deadzone = InputDeviceUsageDetectionSettings.MouseSettings.DefaultMouseMoveThreshold, ModifierKeySet? modifierKeySet = null)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ActiveMouseDevice.CreateInputState(axis, deadzone, modifierKeySet);
    }

    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad button on a specific gamepad.
    /// </summary>
    /// <param name="button">The gamepad button to create the input state for.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="axisDeadzone">The deadzone value for axis sensitivity. Default is 0.1f.</param>
    /// <param name="triggerDeadzone">The deadzone value for trigger sensitivity. Default is 0.1f.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeGamepadButton button, uint accessTag, int gamepadIndex, 
        float axisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad?.CreateInputState(button, axisDeadzone, triggerDeadzone, modifierKeySet) ?? new();
    }
    #endregion

    #region Gamepad
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad button axis (negative and positive) on a specific gamepad.
    /// </summary>
    /// <param name="neg">The negative direction gamepad button.</param>
    /// <param name="pos">The positive direction gamepad button.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="axisDeadzone">The deadzone value for axis sensitivity. Default is 0.1f.</param>
    /// <param name="triggerDeadzone">The deadzone value for trigger sensitivity. Default is 0.1f.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, uint accessTag, int gamepadIndex, 
        float axisDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        
        return gamepad?.CreateInputState(neg, pos, axisDeadzone, triggerDeadzone, modifierKeySet) ?? new();
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad joystick axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The joystick axis to create the input state for.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="deadzone">The deadzone value for axis sensitivity. Default is 0.1f.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeGamepadJoyAxis axis, uint accessTag, int gamepadIndex, 
        float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        
        return gamepad?.CreateInputState(axis, deadzone, modifierKeySet) ?? new();
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad trigger axis (left or right trigger) on a specific gamepad.
    /// </summary>
    /// <param name="axis">The trigger axis to create the input state for (e.g., left or right trigger).</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="deadzone">The deadzone value for trigger sensitivity. Default is 0.1f.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(ShapeGamepadTriggerAxis axis, uint accessTag, int gamepadIndex, 
        float deadzone = InputDeviceUsageDetectionSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        
        return gamepad?.CreateInputState(axis, deadzone, modifierKeySet) ?? new();
    }
    #endregion
    
    #endregion
    
    #region Input Type Factory

    #region Keyboard
    /// <summary>
    /// Creates an input type for a keyboard button.
    /// </summary>
    public static IInputType CreateInputType(this ShapeKeyboardButton button, ModifierKeySet? modifierKeySet = null) => new InputTypeKeyboardButton(button, modifierKeySet);

    /// <summary>
    /// Creates an input type for a keyboard button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeKeyboardButton neg, ShapeKeyboardButton pos, ModifierKeySet? modifierKeySet = null) => new InputTypeKeyboardButtonAxis(neg, pos,  modifierKeySet);

    #endregion
    
    #region Mouse
    /// <summary>
    /// Creates an input type for a mouse button.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseButton button, float deadzone = 0.2f, ModifierKeySet? modifierKeySet = null) => new InputTypeMouseButton(button, deadzone,  modifierKeySet);

    /// <summary>
    /// Creates an input type for a mouse button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0.2f, ModifierKeySet? modifierKeySet = null) => new InputTypeMouseButtonAxis(neg, pos, deadzone,  modifierKeySet);

    /// <summary>
    /// Creates an input type for a mouse wheel axis.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseWheelAxis axis, float deadzone = 0.2f, ModifierKeySet? modifierKeySet = null) => new InputTypeMouseWheelAxis(axis, deadzone,  modifierKeySet);

    /// <summary>
    /// Creates an input type for a mouse axis.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseAxis axis, float deadzone = 0.2f, ModifierKeySet? modifierKeySet = null) => new InputTypeMouseAxis(axis, deadzone,  modifierKeySet);

    #endregion
    
    #region Gamepad
    /// <summary>
    /// Creates an input type for a gamepad button.
    /// </summary>
    public static IInputType CreateInputType(this ShapeGamepadButton button, float deadzone = 0.1f, ModifierKeySet? modifierKeySet = null) => new InputTypeGamepadButton(button, deadzone,  modifierKeySet);

    /// <summary>
    /// Creates an input type for a gamepad button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.1f, ModifierKeySet? modifierKeySet = null) => new InputTypeGamepadButtonAxis(neg, pos, deadzone,  modifierKeySet);
    /// <summary>
    /// Creates an input type for a gamepad joystick axis.
    /// </summary>
    /// <param name="axis">The gamepad joystick axis.</param>
    /// <param name="deadzone">The deadzone value for axis sensitivity. Default is 0.1f.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input type creation.</param>
    /// <returns>An <see cref="IInputType"/> representing the gamepad joystick axis input.</returns>
    public static IInputType CreateInputType(this ShapeGamepadJoyAxis axis, float deadzone = 0.1f, ModifierKeySet? modifierKeySet = null) => new InputTypeGamepadJoyAxis(axis, deadzone,  modifierKeySet);
    /// <summary>
    /// Creates an input type for a gamepad trigger axis (left or right trigger).
    /// </summary>
    /// <param name="axis">The gamepad trigger axis (e.g., left or right trigger).</param>
    /// <param name="deadzone">The deadzone value for trigger sensitivity. Default is 0.1f.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input type creation.</param>
    /// <returns>An <see cref="IInputType"/> representing the gamepad trigger axis input.</returns>
    public static IInputType CreateInputType(this ShapeGamepadTriggerAxis axis, float deadzone = 0.1f, ModifierKeySet? modifierKeySet = null) => new InputTypeGamepadTriggerAxis(axis, deadzone,  modifierKeySet);

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
    private static int tagPowerCounter = 2; //0 and 1 are reserved for AllAccessTag and DefaultAccessTag respectively.

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

    #region Button / Axis Names

    #region Keyboard
    /// <summary>
    /// Gets the display name for a keyboard button.
    /// </summary>
    /// <param name="button">The keyboard button to get the name for.</param>
    /// <param name="shortHand">Whether to use shorthand notation for the button name.</param>
    /// <returns>The button name as a string.</returns>
    public static string GetButtonName(this ShapeKeyboardButton button, bool shortHand = true)
    {
        return button switch
        {
            ShapeKeyboardButton.APOSTROPHE => shortHand ? "´" : "Apostrophe",
            ShapeKeyboardButton.COMMA => shortHand ? "," : "Comma",
            ShapeKeyboardButton.MINUS => shortHand ? "-" : "Minus",
            ShapeKeyboardButton.PERIOD => shortHand ? "." : "Period",
            ShapeKeyboardButton.SLASH => shortHand ? "/" : "Slash",
            ShapeKeyboardButton.ZERO => shortHand ? "0" : "Zero",
            ShapeKeyboardButton.ONE => shortHand ? "1" : "One",
            ShapeKeyboardButton.TWO => shortHand ? "2" : "Two",
            ShapeKeyboardButton.THREE => shortHand ? "3" : "Three",
            ShapeKeyboardButton.FOUR => shortHand ? "4" : "Four",
            ShapeKeyboardButton.FIVE => shortHand ? "5" : "Five",
            ShapeKeyboardButton.SIX => shortHand ? "6" : "Six",
            ShapeKeyboardButton.SEVEN => shortHand ? "7" : "Seven",
            ShapeKeyboardButton.EIGHT => shortHand ? "8" : "Eight",
            ShapeKeyboardButton.NINE => shortHand ? "9" : "Nine",
            ShapeKeyboardButton.SEMICOLON => shortHand ? ";" : "Semi Colon",
            ShapeKeyboardButton.EQUAL => shortHand ? "=" : "Equal",
            ShapeKeyboardButton.A => "A",
            ShapeKeyboardButton.B => "B",
            ShapeKeyboardButton.C => "C",
            ShapeKeyboardButton.D => "D",
            ShapeKeyboardButton.E => "E",
            ShapeKeyboardButton.F => "F",
            ShapeKeyboardButton.G => "G",
            ShapeKeyboardButton.H => "H",
            ShapeKeyboardButton.I => "I",
            ShapeKeyboardButton.J => "J",
            ShapeKeyboardButton.K => "K",
            ShapeKeyboardButton.L => "L",
            ShapeKeyboardButton.M => "M",
            ShapeKeyboardButton.N => "N",
            ShapeKeyboardButton.O => "O",
            ShapeKeyboardButton.P => "P",
            ShapeKeyboardButton.Q => "Q",
            ShapeKeyboardButton.R => "R",
            ShapeKeyboardButton.S => "S",
            ShapeKeyboardButton.T => "T",
            ShapeKeyboardButton.U => "U",
            ShapeKeyboardButton.V => "V",
            ShapeKeyboardButton.W => "W",
            ShapeKeyboardButton.X => "X",
            ShapeKeyboardButton.Y => "Y",
            ShapeKeyboardButton.Z => "Z",
            ShapeKeyboardButton.LEFT_BRACKET => shortHand ? "[" : "Left Bracket",
            ShapeKeyboardButton.BACKSLASH => shortHand ? "\\" : "Backslash",
            ShapeKeyboardButton.RIGHT_BRACKET => shortHand ? "]" : "Right Bracket",
            ShapeKeyboardButton.GRAVE => shortHand ? "`" : "Grave",
            ShapeKeyboardButton.SPACE => shortHand ? "Spc" : "Space",
            ShapeKeyboardButton.ESCAPE => shortHand ? "Esc" : "Escape",
            ShapeKeyboardButton.ENTER => shortHand ? "Ent" : "Enter",
            ShapeKeyboardButton.TAB => shortHand ? "Tab" : "Tabulator",
            ShapeKeyboardButton.BACKSPACE => shortHand ? "BSpc" : "Backspace",
            ShapeKeyboardButton.INSERT => shortHand ? "Ins" : "Insert",
            ShapeKeyboardButton.DELETE => shortHand ? "Del" : "Delete",
            ShapeKeyboardButton.RIGHT => shortHand ? "Rgt" : "Right",
            ShapeKeyboardButton.LEFT => shortHand ? "Lft" : "Left",
            ShapeKeyboardButton.DOWN => shortHand ? "Dwn" : "Down",
            ShapeKeyboardButton.UP => "Up",
            ShapeKeyboardButton.PAGE_UP => shortHand ? "PUp" : "Page Up",
            ShapeKeyboardButton.PAGE_DOWN => shortHand ? "PDwn" : "Page Down",
            ShapeKeyboardButton.HOME => shortHand ? "Hom" : "Home",
            ShapeKeyboardButton.END => shortHand ? "End" : "End Key",
            ShapeKeyboardButton.CAPS_LOCK => shortHand ? "CpsL" : "Caps Lock",
            ShapeKeyboardButton.SCROLL_LOCK => shortHand ? "ScrL" : "Scroll Lock",
            ShapeKeyboardButton.NUM_LOCK => shortHand ? "NumL" : "Num Lock",
            ShapeKeyboardButton.PRINT_SCREEN => shortHand ? "Prnt" : "Print Screen",
            ShapeKeyboardButton.PAUSE => shortHand ? "Pse" : "Pause",
            ShapeKeyboardButton.F1 => shortHand ? "F1" : "Function 1",
            ShapeKeyboardButton.F2 => shortHand ? "F2" : "Function 2",
            ShapeKeyboardButton.F3 => shortHand ? "F3" : "Function 3",
            ShapeKeyboardButton.F4 => shortHand ? "F4" : "Function 4",
            ShapeKeyboardButton.F5 => shortHand ? "F5" : "Function 5",
            ShapeKeyboardButton.F6 => shortHand ? "F6" : "Function 6",
            ShapeKeyboardButton.F7 => shortHand ? "F7" : "Function 7",
            ShapeKeyboardButton.F8 => shortHand ? "F8" : "Function 8",
            ShapeKeyboardButton.F9 => shortHand ? "F9" : "Function 9",
            ShapeKeyboardButton.F10 => shortHand ? "F10" : "Function 10",
            ShapeKeyboardButton.F11 => shortHand ? "F11" : "Function 11",
            ShapeKeyboardButton.F12 => shortHand ? "F12" : "Function 12",
            ShapeKeyboardButton.LEFT_SHIFT => shortHand ? "LShift" : "Left Shift",
            ShapeKeyboardButton.LEFT_CONTROL => shortHand ? "LCtrl" : "Left Control",
            ShapeKeyboardButton.LEFT_ALT => shortHand ? "LAlt" : "Left Alt",
            ShapeKeyboardButton.LEFT_SUPER => shortHand ? "LSuper" : "Left Super",
            ShapeKeyboardButton.RIGHT_SHIFT => shortHand ? "RShift" : "Right Shift",
            ShapeKeyboardButton.RIGHT_CONTROL => shortHand ? "RCtrl" : "Right Control",
            ShapeKeyboardButton.RIGHT_ALT => shortHand ? "RAlt" : "Right Alt",
            ShapeKeyboardButton.RIGHT_SUPER => shortHand ? "RSuper" : "Right Super",
            ShapeKeyboardButton.KB_MENU => shortHand ? "KBMenu" : "KB Menu",
            ShapeKeyboardButton.KP_0 => shortHand ? "KP0" : "Keypad 0",
            ShapeKeyboardButton.KP_1 => shortHand ? "KP1" : "Keypad 1",
            ShapeKeyboardButton.KP_2 => shortHand ? "KP2" : "Keypad 2",
            ShapeKeyboardButton.KP_3 => shortHand ? "KP3" : "Keypad 3",
            ShapeKeyboardButton.KP_4 => shortHand ? "KP4" : "Keypad 4",
            ShapeKeyboardButton.KP_5 => shortHand ? "KP5" : "Keypad 5",
            ShapeKeyboardButton.KP_6 => shortHand ? "KP6" : "Keypad 6",
            ShapeKeyboardButton.KP_7 => shortHand ? "KP7" : "Keypad 7",
            ShapeKeyboardButton.KP_8 => shortHand ? "KP8" : "Keypad 8",
            ShapeKeyboardButton.KP_9 => shortHand ? "KP9" : "Keypad 9",
            ShapeKeyboardButton.KP_DECIMAL => shortHand ? "KPDec" : "Keypad Decimal",
            ShapeKeyboardButton.KP_DIVIDE => shortHand ? "KPDiv" : "Keypad Divide",
            ShapeKeyboardButton.KP_MULTIPLY => shortHand ? "KPMul" : "Keypad Multiply",
            ShapeKeyboardButton.KP_SUBTRACT => shortHand ? "KPSub" : "Keypad Subtract",
            ShapeKeyboardButton.KP_ADD => shortHand ? "KPAdd" : "Keypad Add",
            ShapeKeyboardButton.KP_ENTER => shortHand ? "KPEnt" : "Keypad Enter",
            ShapeKeyboardButton.KP_EQUAL => shortHand ? "KPEqual" : "Keypad Equal",
            ShapeKeyboardButton.VOLUME_UP => shortHand ? "Vol+" : "Volume Up",
            ShapeKeyboardButton.VOLUME_DOWN => shortHand ? "Vol-" : "Volume Down",
            ShapeKeyboardButton.BACK => shortHand ? "Bck" : "Back",
            ShapeKeyboardButton.NULL => shortHand ? "Nll" : "Null",
            ShapeKeyboardButton.MENU => shortHand ? "Mnu" : "Menu",
            _ => "No Key"
        };
    }
    
    /// <summary>
    /// Gets the display name for a button axis (negative and positive button pair).
    /// </summary>
    /// <remarks>
    /// Format: "NegButtonName|PosButtonName" (e\.g\., "A|D" or "Left|Right"\)
    /// </remarks>
    /// <param name="neg">Negative direction button.</param>
    /// <param name="pos">Positive direction button.</param>
    /// <param name="shorthand">Whether to use shorthand notation.</param>
    /// <returns>The button axis name.</returns>
    public static string GetButtonAxisName(this ShapeKeyboardButton neg, ShapeKeyboardButton pos, bool shorthand = true)
    {
        StringBuilder sb = new();
        
        string negName = neg.GetButtonName(shorthand);
        string posName = pos.GetButtonName(shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
    }
    #endregion

    #region Mouse
    
    /// <summary>
    /// Gets the display name for a mouse axis.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <param name="shortHand">Whether to use shorthand notation.</param>
    /// <returns>The axis name.</returns>
    public static string GetAxisName(this ShapeMouseAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeMouseAxis.HORIZONTAL: return shortHand ? "Mx" : "Mouse Horizontal";
            case ShapeMouseAxis.VERTICAL: return shortHand ? "My" : "Mouse Vertical";
            default: return "No Key";
        }
    }
    /// <summary>
    /// Gets the display name for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The wheel axis.</param>
    /// <param name="shortHand">Whether to use shorthand notation.</param>
    /// <returns>The wheel axis name.</returns>
    public static string GetWheelAxisName(this ShapeMouseWheelAxis axis, bool shortHand = true)
    {
        return axis switch
        {
            ShapeMouseWheelAxis.HORIZONTAL => shortHand ? "MWx" : "Mouse Wheel Horizontal",
            ShapeMouseWheelAxis.VERTICAL => shortHand ? "MWy" : "Mouse Wheel Vertical",
            _ => "No Key"
        };
    }
    /// <summary>
    /// Gets the display name for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="shortHand">Whether to use shorthand notation.</param>
    /// <returns>The button name.</returns>
    public static string GetButtonName(this ShapeMouseButton button, bool shortHand = true)
    {
        return button switch
        {
            ShapeMouseButton.LEFT => shortHand ? "LMB" : "Left Mouse Button",
            ShapeMouseButton.RIGHT => shortHand ? "RMB" : "Right Mouse Button",
            ShapeMouseButton.MIDDLE => shortHand ? "MMB" : "Middle Mouse Button",
            ShapeMouseButton.SIDE => shortHand ? "SMB" : "Side Mouse Button",
            ShapeMouseButton.EXTRA => shortHand ? "EMB" : "Extra Mouse Button",
            ShapeMouseButton.FORWARD => shortHand ? "FMB" : "Forward Mouse Button",
            ShapeMouseButton.BACK => shortHand ? "BMB" : "Back Mouse Button",
            ShapeMouseButton.MW_UP => shortHand ? "MW U" : "Mouse Wheel Up",
            ShapeMouseButton.MW_DOWN => shortHand ? "MW D" : "Mouse Wheel Down",
            ShapeMouseButton.MW_LEFT => shortHand ? "MW L" : "Mouse Wheel Left",
            ShapeMouseButton.MW_RIGHT => shortHand ? "MW R" : "Mouse Wheel Right",
            ShapeMouseButton.UP_AXIS => shortHand ? "M Up" : "Mouse Up",
            ShapeMouseButton.DOWN_AXIS => shortHand ? "M Dwn" : "Mouse Down",
            ShapeMouseButton.LEFT_AXIS => shortHand ? "M Lft" : "Mouse Left",
            ShapeMouseButton.RIGHT_AXIS => shortHand ? "M Rgt" : "Mouse Right",
            _ => "No Key"
        };
    }
    /// <summary>
    /// Gets the display name for a mouse button axis (negative and positive button pair).
    /// </summary>
    /// <param name="neg">Negative direction mouse button.</param>
    /// <param name="pos">Positive direction mouse button.</param>
    /// <param name="shorthand">Whether to use shorthand notation.</param>
    /// <returns>The button axis name as a string.</returns>
    public static string GetButtonAxisName(ShapeMouseButton neg, ShapeMouseButton pos, bool shorthand = true)
    {
        StringBuilder sb = new();
        
        string negName = neg.GetButtonName(shorthand);
        string posName = pos.GetButtonName(shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
    }

    #endregion
    
    #region Gamepad
    /// <summary>
    /// Gets the display name for a gamepad button.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="shortHand">Whether to use shorthand notation.</param>
    /// <returns>The button name.</returns>
    public static string GetButtonName(this ShapeGamepadButton button, bool shortHand = true)
    {
        switch (button)
        {
            case ShapeGamepadButton.UNKNOWN: return shortHand ? "Unknown" : "GP Button Unknown";
            case ShapeGamepadButton.LEFT_FACE_UP: return shortHand ? "Up" : "GP Button Up";
            case ShapeGamepadButton.LEFT_FACE_RIGHT: return shortHand ? "Right" : "GP Button Right";
            case ShapeGamepadButton.LEFT_FACE_DOWN: return shortHand ? "Down" : "GP Button Down";
            case ShapeGamepadButton.LEFT_FACE_LEFT: return shortHand ? "Left" : "GP Button Left";
            case ShapeGamepadButton.RIGHT_FACE_UP: return shortHand ? "Y" : "GP Button Y";
            case ShapeGamepadButton.RIGHT_FACE_RIGHT: return shortHand ? "B" : "GP Button B";
            case ShapeGamepadButton.RIGHT_FACE_DOWN: return shortHand ? "A" : "GP Button A";
            case ShapeGamepadButton.RIGHT_FACE_LEFT: return shortHand ? "X" : "GP Button X";
            case ShapeGamepadButton.LEFT_TRIGGER_TOP: return shortHand ? "LB" : "GP Button Left Bumper";
            case ShapeGamepadButton.LEFT_TRIGGER_BOTTOM: return shortHand ? "LT" : "GP Button Left Trigger";
            case ShapeGamepadButton.RIGHT_TRIGGER_TOP: return shortHand ? "RB" : "GP Button Right Bumper";
            case ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM: return shortHand ? "RT" : "GP Button Right Trigger";
            case ShapeGamepadButton.MIDDLE_LEFT: return shortHand ? "Select" : "GP Button Select";
            case ShapeGamepadButton.MIDDLE: return shortHand ? "Home" : "GP Button Home";
            case ShapeGamepadButton.MIDDLE_RIGHT: return shortHand ? "Start" : "GP Button Start";
            case ShapeGamepadButton.LEFT_THUMB: return shortHand ? "LClick" : "GP Button Left Stick Click";
            case ShapeGamepadButton.RIGHT_THUMB: return shortHand ? "RClick" : "GP Button Right Stick Click";
            case ShapeGamepadButton.LEFT_STICK_RIGHT: return shortHand ? "LS R" : "Left Stick Right";
            case ShapeGamepadButton.LEFT_STICK_LEFT: return shortHand ? "LS L" : "Left Stick Left";
            case ShapeGamepadButton.LEFT_STICK_DOWN: return shortHand ? "LS D" : "Left Stick Down";
            case ShapeGamepadButton.LEFT_STICK_UP: return shortHand ? "LS U" : "Left Stick Up";
            case ShapeGamepadButton.RIGHT_STICK_RIGHT: return shortHand ? "RS R" : "Right Stick Right";
            case ShapeGamepadButton.RIGHT_STICK_LEFT: return shortHand ? "RS L" : "Right Stick Left";
            case ShapeGamepadButton.RIGHT_STICK_DOWN: return shortHand ? "RS D" : "Right Stick Down";
            case ShapeGamepadButton.RIGHT_STICK_UP: return shortHand ? "RS U" : "Right Stick Up";
            default: return "No Key";
        }
    }
    
    /// <summary>
    /// Gets the display name for a gamepad joystick axis.
    /// </summary>
    /// <param name="axis">The joystick axis to get the name for.</param>
    /// <param name="shortHand">Whether to use shorthand notation.</param>
    /// <returns>The axis name as a string.</returns>
    public static string GetAxisName(this ShapeGamepadJoyAxis axis, bool shortHand = true)
    {
        return axis switch
        {
            ShapeGamepadJoyAxis.LEFT_X => shortHand ? "LSx" : "GP Axis Left X",
            ShapeGamepadJoyAxis.LEFT_Y => shortHand ? "LSy" : "GP Axis Left Y",
            ShapeGamepadJoyAxis.RIGHT_X => shortHand ? "RSx" : "GP Axis Right X",
            ShapeGamepadJoyAxis.RIGHT_Y => shortHand ? "RSy" : "GP Axis Right Y",
            _ => "No Key"
        };
    }
    /// <summary>
    /// Gets the display name for a gamepad trigger axis.
    /// </summary>
    /// <param name="axis">The trigger axis to get the name for.</param>
    /// <param name="shortHand">Whether to use shorthand notation.</param>
    /// <returns>The trigger axis name as a string.</returns>
    public static string GetAxisName(this ShapeGamepadTriggerAxis axis, bool shortHand = true)
    {
        return axis switch
        {
            ShapeGamepadTriggerAxis.LEFT => shortHand ? "LT" : "GP Axis Left Trigger",
            ShapeGamepadTriggerAxis.RIGHT => shortHand ? "RT" : "GP Axis Right Trigger",
            _ => "No Key"
        };
    }

    /// <summary>
    /// Gets the display name for a button axis (negative and positive button pair).
    /// </summary>
    /// /// <remarks>
    /// Format: "NegButtonName|PosButtonName" (e\.g\., "A|D" or "Left|Right"\)
    /// </remarks>
    /// <param name="neg">Negative direction button.</param>
    /// <param name="pos">Positive direction button.</param>
    /// <param name="shorthand">Whether to use shorthand notation.</param>
    /// <returns>The button axis name.</returns>
    public static string GetButtonAxisName(this ShapeGamepadButton neg, ShapeGamepadButton pos, bool shorthand = true)
    {
        StringBuilder sb = new();
        
        string negName = GetButtonName(neg, shorthand);
        string posName = GetButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
    }
    #endregion

    #endregion
}
