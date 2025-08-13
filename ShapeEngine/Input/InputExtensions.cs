using System.Text;
using ShapeEngine.Core.GameDef;

namespace ShapeEngine.Input;

/// <summary>
/// Provides extension methods for input-related types, such as device type filtering,
/// input state retrieval, consumption, creation, and display name formatting.
/// </summary>
public static class InputExtensions
{
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
    
    #region Get InputState Methods

    /// <summary>
    /// Gets the input state for a keyboard button.
    /// </summary>
    /// <param name="button">The keyboard button to query.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed. Default is <see cref="InputSystem.AllAccessTag"/>.</param>
    /// <returns>The input state for the specified keyboard button.</returns>
    public static InputState GetInputState(this ShapeKeyboardButton button, uint accessTag = InputSystem.AllAccessTag)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Keyboard.GetButtonState(button);
    }
    /// <summary>
    /// Gets the input state for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button to query.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed. Default is <see cref="InputSystem.AllAccessTag"/>.</param>
    /// <returns>The input state for the specified mouse button.</returns>
    public static InputState GetInputState(this ShapeMouseButton button, uint accessTag = InputSystem.AllAccessTag)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Mouse.GetButtonState(button);
    }
    /// <summary>
    /// Gets the input state for a mouse axis.
    /// </summary>
    /// <param name="axis">The mouse axis to query.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed. Default is <see cref="InputSystem.AllAccessTag"/>.</param>
    /// <returns>The input state for the specified mouse axis.</returns>
    public static InputState GetInputState(this ShapeMouseAxis axis, uint accessTag = InputSystem.AllAccessTag)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Mouse.GetAxisState(axis);
    }
    /// <summary>
    /// Gets the input state for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to query.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed. Default is <see cref="InputSystem.AllAccessTag"/>.</param>
    /// <returns>The input state for the specified mouse wheel axis.</returns>
    public static InputState GetInputState(this ShapeMouseWheelAxis axis, uint accessTag = InputSystem.AllAccessTag)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Mouse.GetWheelAxisState(axis);
    }
    /// <summary>
    /// Gets the input state for a gamepad button on a specific gamepad.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="InputSystem.Locked"/> is set to <c>true</c>.</param>
    /// <returns>The input state for the button.</returns>
    public static InputState GetInputState(this ShapeGamepadButton button, int gamepadIndex, uint accessTag = InputSystem.AllAccessTag)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        var gamepad = Game.Instance.Input.GamepadManager.GetGamepad(gamepadIndex);
        return gamepad?.GetButtonState(button) ?? new();
    }
    /// <summary>
    /// Gets the input state for a gamepad joystick axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The joystick axis to query.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="accessTag">
    /// The access tag used to check if the input request has access and can be consumed. Default is <see cref="InputSystem.AllAccessTag"/>.
    /// Access is only checked when <see cref="InputSystem.Locked"/> is set to <c>true</c>.
    /// </param>
    /// <returns>The input state for the specified joystick axis.</returns>
    public static InputState GetInputState(this ShapeGamepadJoyAxis axis, int gamepadIndex, uint accessTag = InputSystem.AllAccessTag)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        var gamepad = Game.Instance.Input.GamepadManager.GetGamepad(gamepadIndex);
        return gamepad?.GetAxisState(axis) ?? new();
    }
    /// <summary>
    /// Gets the input state for a gamepad trigger axis (left or right trigger) on a specific gamepad.
    /// </summary>
    /// <param name="axis">The trigger axis to query (e.g., left or right trigger).</param>
    /// <param name="gamepadIndex">The index of the gamepad to query.</param>
    /// <param name="accessTag">
    /// The access tag used to check if the input request has access and can be consumed. Default is <see cref="InputSystem.AllAccessTag"/>.
    /// Access is only checked when <see cref="InputSystem.Locked"/> is set to <c>true</c>.
    /// </param>
    /// <returns>The input state for the specified trigger axis on the specified gamepad.</returns>
    public static InputState GetInputState(this ShapeGamepadTriggerAxis axis, int gamepadIndex, uint accessTag = InputSystem.AllAccessTag)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        var gamepad = Game.Instance.Input.GamepadManager.GetGamepad(gamepadIndex);
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
    /// Access is only checked when <see cref="InputSystem.Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeKeyboardButton button, out bool valid, uint accessTag = InputSystem.AllAccessTag)
    {
        valid = false;
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Keyboard.ConsumeButtonState(button, out valid);
    }

    /// <summary>
    /// Consumes the input state for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button to consume.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="InputSystem.Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeMouseButton button, out bool valid, uint accessTag = InputSystem.AllAccessTag)
    {
        valid = false;
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Mouse.ConsumeButtonState(button, out valid);
    }

    /// <summary>
    /// Consumes the input state for a mouse axis.
    /// </summary>
    /// <param name="axis">The mouse axis to consume.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="InputSystem.Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeMouseAxis axis, out bool valid, uint accessTag = InputSystem.AllAccessTag)
    {
        valid = false;
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Mouse.ConsumeAxisState(axis, out valid);
    }

    /// <summary>
    /// Consumes the input state for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The mouse wheel axis to consume.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="InputSystem.Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeMouseWheelAxis axis, out bool valid, uint accessTag = InputSystem.AllAccessTag)
    {
        valid = false;
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Mouse.ConsumeWheelAxisState(axis, out valid);
    }

    /// <summary>
    /// Consumes the input state for a gamepad button on a specific gamepad.
    /// </summary>
    /// <param name="button">The gamepad button to consume.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">The access tag used to check if the input request has access and can be consumed.
    /// Access is only checked when <see cref="InputSystem.Locked"/> is set to <c>true</c>.</param>
    /// <returns>The consumed input state.</returns>
    public static InputState ConsumeInputState(this ShapeGamepadButton button, int gamepadIndex, out bool valid, uint accessTag = InputSystem.AllAccessTag)
    {
        valid = false;
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        var gamepad = Game.Instance.Input.GamepadManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.ConsumeButtonState(button, out valid);
    }
    /// <summary>
    /// Consumes the input state for a gamepad joystick axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The joystick axis to consume.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">
    /// The access tag used to check if the input request has access and can be consumed. Default is <see cref="InputSystem.AllAccessTag"/>.
    /// Access is only checked when <see cref="InputSystem.Locked"/> is set to <c>true</c>.
    /// </param>
    /// <returns>The consumed input state for the specified joystick axis.</returns>
    public static InputState ConsumeInputState(this ShapeGamepadJoyAxis axis, int gamepadIndex, out bool valid, uint accessTag = InputSystem.AllAccessTag)
    {
        valid = false;
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        var gamepad = Game.Instance.Input.GamepadManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.ConsumeAxisState(axis, out valid);
    }
    /// <summary>
    /// Consumes the input state for a gamepad trigger axis (left or right trigger) on a specific gamepad.
    /// </summary>
    /// <param name="axis">The trigger axis to consume (e.g., left or right trigger).</param>
    /// <param name="gamepadIndex">The index of the gamepad to query.</param>
    /// <param name="valid">True if the state was valid and consumed; otherwise, false.</param>
    /// <param name="accessTag">
    /// The access tag used to check if the input request has access and can be consumed. Default is <see cref="InputSystem.AllAccessTag"/>.
    /// Access is only checked when <see cref="InputSystem.Locked"/> is set to <c>true</c>.
    /// </param>
    /// <returns>The consumed input state for the specified trigger axis on the specified gamepad.</returns>
    public static InputState ConsumeInputState(this ShapeGamepadTriggerAxis axis, int gamepadIndex, out bool valid, uint accessTag = InputSystem.AllAccessTag)
    {
        valid = false;
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        var gamepad = Game.Instance.Input.GamepadManager.GetGamepad(gamepadIndex);
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
    public static InputState CreateInputState(this ShapeKeyboardButton button, uint accessTag, ModifierKeySet? modifierKeySet = null)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return modifierKeySet == null ? Game.Instance.Input.Keyboard.CreateInputState(button) : Game.Instance.Input.Keyboard.CreateInputState(button, modifierKeySet);
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for a keyboard button axis (negative and positive).
    /// </summary>
    /// <param name="neg">The negative keyboard button.</param>
    /// <param name="pos">The positive keyboard button.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(this ShapeKeyboardButton neg, ShapeKeyboardButton pos, uint accessTag, ModifierKeySet? modifierKeySet = null)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return modifierKeySet == null ? Game.Instance.Input.Keyboard.CreateInputState(neg, pos) : Game.Instance.Input.Keyboard.CreateInputState(neg, pos, modifierKeySet);
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
    public static InputState CreateInputState(this ShapeMouseButton button, uint accessTag, 
        float moveDeadzone = InputSettings.MouseSettings.DefaultMouseMoveThreshold,  
        float wheelDeadzone = InputSettings.MouseSettings.DefaultMouseWheelThreshold,  
        ModifierKeySet? modifierKeySet = null)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Mouse.CreateInputState(button, moveDeadzone, wheelDeadzone, modifierKeySet);
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
    public static InputState CreateInputState(this ShapeMouseButton neg, ShapeMouseButton pos, uint accessTag, 
        float moveDeadzone = InputSettings.MouseSettings.DefaultMouseMoveThreshold,  
        float wheelDeadzone = InputSettings.MouseSettings.DefaultMouseWheelThreshold,  
        ModifierKeySet? modifierKeySet = null)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Mouse.CreateInputState(neg, pos, moveDeadzone, wheelDeadzone, modifierKeySet);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The mouse wheel axis.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="deadzone">Optional deadzone for mouse wheel detection.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(this ShapeMouseWheelAxis axis, uint accessTag, 
        float deadzone = InputSettings.MouseSettings.DefaultMouseWheelThreshold, ModifierKeySet? modifierKeySet = null)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Mouse.CreateInputState(axis, deadzone, modifierKeySet);
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a mouse axis.
    /// </summary>
    /// <param name="axis">The mouse axis.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="deadzone">Optional deadzone for mouse movement detection.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(this ShapeMouseAxis axis, uint accessTag, 
        float deadzone = InputSettings.MouseSettings.DefaultMouseMoveThreshold, ModifierKeySet? modifierKeySet = null)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        return Game.Instance.Input.Mouse.CreateInputState(axis, deadzone, modifierKeySet);
    }


    #endregion

    #region Gamepad
    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad button on a specific gamepad.
    /// </summary>
    /// <param name="button">The gamepad button to create the input state for.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="axisDeadzone">The deadzone value for axis sensitivity.</param>
    /// <param name="triggerDeadzone">The deadzone value for trigger sensitivity.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(this ShapeGamepadButton button, uint accessTag, int gamepadIndex, 
        float axisDeadzone = InputSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        var gamepad = Game.Instance.Input.GamepadManager.GetGamepad(gamepadIndex);
        return gamepad?.CreateInputState(button, axisDeadzone, triggerDeadzone, modifierKeySet) ?? new();
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad button axis (negative and positive) on a specific gamepad.
    /// </summary>
    /// <param name="neg">The negative direction gamepad button.</param>
    /// <param name="pos">The positive direction gamepad button.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="axisDeadzone">The deadzone value for axis sensitivity.</param>
    /// <param name="triggerDeadzone">The deadzone value for trigger sensitivity.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(this ShapeGamepadButton neg, ShapeGamepadButton pos, uint accessTag, int gamepadIndex, 
        float axisDeadzone = InputSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        float triggerDeadzone = InputSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        ModifierKeySet? modifierKeySet = null)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        var gamepad = Game.Instance.Input.GamepadManager.GetGamepad(gamepadIndex);
        
        return gamepad?.CreateInputState(neg, pos, axisDeadzone, triggerDeadzone, modifierKeySet) ?? new();
    }
    
    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad joystick axis on a specific gamepad.
    /// </summary>
    /// <param name="axis">The joystick axis to create the input state for.</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="deadzone">The deadzone value for axis sensitivity.</param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(this ShapeGamepadJoyAxis axis, uint accessTag, int gamepadIndex, 
        float deadzone = InputSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        bool inverted = false,
        ModifierKeySet? modifierKeySet = null)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        var gamepad = Game.Instance.Input.GamepadManager.GetGamepad(gamepadIndex);
        
        return gamepad?.CreateInputState(axis, deadzone, inverted, modifierKeySet) ?? new();
    }
    /// <summary>
    /// Creates an <see cref="InputState"/> for a gamepad trigger axis (left or right trigger) on a specific gamepad.
    /// </summary>
    /// <param name="axis">The trigger axis to create the input state for (e.g., left or right trigger).</param>
    /// <param name="accessTag">The access tag for input access control.</param>
    /// <param name="gamepadIndex">The index of the gamepad.</param>
    /// <param name="deadzone">The deadzone value for trigger sensitivity.</param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input state creation.</param>
    /// <returns>The created <see cref="InputState"/>.</returns>
    public static InputState CreateInputState(this ShapeGamepadTriggerAxis axis, uint accessTag, int gamepadIndex, 
        float deadzone = InputSettings.GamepadSettings.DefaultTriggerAxisThreshold, bool inverted = false,
        ModifierKeySet? modifierKeySet = null)
    {
        if (InputSystem.Locked && !InputSystem.HasAccess(accessTag)) return new();
        var gamepad = Game.Instance.Input.GamepadManager.GetGamepad(gamepadIndex);
        
        return gamepad?.CreateInputState(axis, deadzone, inverted, modifierKeySet) ?? new();
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
    public static IInputType CreateInputType(this ShapeMouseButton button, float deadzone = InputSettings.MouseSettings.DefaultMouseThreshold, 
        ModifierKeySet? modifierKeySet = null) => new InputTypeMouseButton(button, deadzone,  modifierKeySet);

    /// <summary>
    /// Creates an input type for a mouse button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = InputSettings.MouseSettings.DefaultMouseThreshold, 
        ModifierKeySet? modifierKeySet = null) => new InputTypeMouseButtonAxis(neg, pos, deadzone,  modifierKeySet);

    /// <summary>
    /// Creates an input type for a mouse wheel axis.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseWheelAxis axis, float deadzone = InputSettings.MouseSettings.DefaultMouseWheelThreshold, 
        ModifierKeySet? modifierKeySet = null) => new InputTypeMouseWheelAxis(axis, deadzone,  modifierKeySet);

    /// <summary>
    /// Creates an input type for a mouse axis.
    /// </summary>
    public static IInputType CreateInputType(this ShapeMouseAxis axis, float deadzone = InputSettings.MouseSettings.DefaultMouseMoveThreshold, 
        ModifierKeySet? modifierKeySet = null) => new InputTypeMouseAxis(axis, deadzone,  modifierKeySet);

    #endregion
    
    #region Gamepad
    /// <summary>
    /// Creates an input type for a gamepad button.
    /// </summary>
    public static IInputType CreateInputType(this ShapeGamepadButton button, float deadzone = InputSettings.GamepadSettings.DefaultAxisThreshold, 
        ModifierKeySet? modifierKeySet = null) => new InputTypeGamepadButton(button, deadzone,  modifierKeySet);

    /// <summary>
    /// Creates an input type for a gamepad button axis (negative and positive).
    /// </summary>
    public static IInputType CreateInputType(this ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = InputSettings.GamepadSettings.DefaultAxisThreshold, 
        ModifierKeySet? modifierKeySet = null) => new InputTypeGamepadButtonAxis(neg, pos, deadzone,  modifierKeySet);
    /// <summary>
    /// Creates an input type for a gamepad joystick axis.
    /// </summary>
    /// <param name="axis">The gamepad joystick axis.</param>
    /// <param name="deadzone">The deadzone value for axis sensitivity.</param>
    /// <param name="inverted">Whether to invert the axis value. Default is false.</param>
    /// <param name="modifierKeySet">Optional modifier key set for input type creation.</param>
    /// <returns>An <see cref="IInputType"/> representing the gamepad joystick axis input.</returns>
    public static IInputType CreateInputType(this ShapeGamepadJoyAxis axis, float deadzone = InputSettings.GamepadSettings.DefaultJoyAxisThreshold, 
        bool inverted = false, ModifierKeySet? modifierKeySet = null) => new InputTypeGamepadJoyAxis(axis, deadzone, inverted, modifierKeySet);
    /// <summary>
    /// Creates an input type for a gamepad trigger axis (left or right trigger).
    /// </summary>
    /// <param name="axis">The gamepad trigger axis (e.g., left or right trigger).</param>
    /// <param name="deadzone">The deadzone value for trigger sensitivity.</param>
    /// <param name="inverted">Whether the trigger axis input should be inverted. (From <c>[0 - 1]</c> to <c>[1 - 0]</c></param>
    /// <param name="modifierKeySet">Optional modifier key set for input type creation.</param>
    /// <returns>An <see cref="IInputType"/> representing the gamepad trigger axis input.</returns>
    public static IInputType CreateInputType(this ShapeGamepadTriggerAxis axis, float deadzone = InputSettings.GamepadSettings.DefaultTriggerAxisThreshold, 
        bool inverted = false, ModifierKeySet? modifierKeySet = null) => new InputTypeGamepadTriggerAxis(axis, deadzone, inverted, modifierKeySet);

    #endregion
    
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
            case ShapeGamepadButton.LEFT_THUMB: return shortHand ? "LsClick" : "GP Button Left Stick Click";
            case ShapeGamepadButton.RIGHT_THUMB: return shortHand ? "RsClick" : "GP Button Right Stick Click";
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