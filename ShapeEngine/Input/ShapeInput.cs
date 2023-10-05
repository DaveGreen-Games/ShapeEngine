using System.Numerics;
using System.Text;
using Raylib_CsLo;

namespace ShapeEngine.Input;

public static class ShapeInput
{
    #region Members
    
    public static readonly uint AllAccessTag = 0;
    public static bool Locked { get; private set; } = false;
    private static readonly List<uint> lockExceptionTags = new();
    private static readonly Dictionary<uint, ShapeInputAction> inputActions = new();

    private static readonly Dictionary<ShapeKeyboardButton, ShapeInputState> keyboardButtonStates = new();
    private static readonly Dictionary<ShapeMouseButton, ShapeInputState> mouseButtonStates = new();
    private static readonly Dictionary<ShapeGamepadButton, ShapeInputState> gamepadButtonStates = new();
    private static readonly Dictionary<ShapeMouseWheelAxis, ShapeInputState> mouseWheelAxisStates = new();
    private static readonly Dictionary<ShapeGamepadAxis, ShapeInputState> gamepadAxisStates = new();
    private static readonly Dictionary<int, ShapeInputState> keyboardButtonAxisStates = new();
    private static readonly Dictionary<int, ShapeInputState> mouseButtonAxisStates = new();
    private static readonly Dictionary<int, ShapeInputState> gamepadButtonAxisStates = new();
    #endregion
    
    #region Lock System
    public static void Lock()
    {
        Locked = true;
        lockExceptionTags.Clear();
    }
    public static void Lock(params uint[] exceptionTags)
    {
        Locked = true;
        lockExceptionTags.Clear();
        if(exceptionTags.Length > 0) lockExceptionTags.AddRange(exceptionTags);
    }
    public static void Unlock()
    {
        Locked = false;
        lockExceptionTags.Clear();
    }
    public static bool HasAccess(uint tag) => tag == AllAccessTag || lockExceptionTags.Contains(tag);
    #endregion
    
    #region Input Actions
    public static bool HasAction(uint id) => inputActions.ContainsKey(id);
    public static uint AddAction(ShapeInputAction newAction)
    {
        var id = newAction.ID;
        if (HasAction(id)) inputActions[id] = newAction;
        else inputActions.Add(id, newAction);
        return id;
    }
    public static bool RemoveAction(uint id) => inputActions.Remove(id);

    public static ShapeInputState GetActionState(uint id)
    {
        if (!HasAction(id)) return new();
        var action = inputActions[id];
        return Locked && !HasAccess(action.AccessTag) ? new() : action.State;
    }
    public static ShapeInputState ConsumeAction(uint id)
    {
        if (!HasAction(id)) return new();
        var action = inputActions[id];
        return Locked && !HasAccess(action.AccessTag) ? new() : action.Consume();
    }

    public static ShapeInputAction? GetAction(uint id)
    {
        return !inputActions.ContainsKey(id) ? null : inputActions[id];
    }
    #endregion

    #region Basic
    public static ShapeInputState GetState(ShapeKeyboardButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        if (!keyboardButtonStates.ContainsKey(button))
        {
            var state = ShapeKeyboardButtonInput.GetState(button);
            keyboardButtonStates.Add(button, state);
            return state;
        }

        var previousState = keyboardButtonStates[button];
        var newState = ShapeKeyboardButtonInput.GetState(button, previousState);
        keyboardButtonStates[button] = newState;
        return newState;
    }
    public static ShapeInputState GetState(ShapeMouseButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        if (!mouseButtonStates.ContainsKey(button))
        {
            var state = ShapeMouseButtonInput.GetState(button);
            mouseButtonStates.Add(button, state);
            return state;
        }

        var previousState = mouseButtonStates[button];
        var newState = ShapeMouseButtonInput.GetState(button, previousState);
        mouseButtonStates[button] = newState;
        return newState;
    }
    public static ShapeInputState GetState(ShapeGamepadButton button, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        if (!gamepadButtonStates.ContainsKey(button))
        {
            var state = ShapeGamepadButtonInput.GetState(button, gamepadIndex, deadzone);
            gamepadButtonStates.Add(button, state);
            return state;
        }

        var previousState = gamepadButtonStates[button];
        var newState = ShapeGamepadButtonInput.GetState(button, previousState, gamepadIndex, deadzone);
        gamepadButtonStates[button] = newState;
        return newState;
    }
    public static ShapeInputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        int hashCode = HashCode.Combine((int)neg, (int)pos);
        if (!keyboardButtonAxisStates.ContainsKey(hashCode))
        {
            var state = ShapeKeyboardButtonAxisInput.GetState(neg, pos);
            keyboardButtonAxisStates.Add(hashCode, state);
            return state;
        }

        var previousState = keyboardButtonAxisStates[hashCode];
        var newState = ShapeKeyboardButtonAxisInput.GetState(neg, pos, previousState);
        keyboardButtonAxisStates[hashCode] = newState;
        return newState;
    }
    public static ShapeInputState GetState(ShapeMouseButton neg, ShapeMouseButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        int hashCode = HashCode.Combine((int)neg, (int)pos);
        if (!mouseButtonAxisStates.ContainsKey(hashCode))
        {
            var state = ShapeMouseButtonAxisInput.GetState(neg, pos);
            mouseButtonAxisStates.Add(hashCode, state);
            return state;
        }

        var previousState = mouseButtonAxisStates[hashCode];
        var newState = ShapeMouseButtonAxisInput.GetState(neg, pos, previousState);
        mouseButtonAxisStates[hashCode] = newState;
        return newState;
    }
    public static ShapeInputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        int hashCode = HashCode.Combine((int)neg, (int)pos);
        if (!gamepadButtonAxisStates.ContainsKey(hashCode))
        {
            var state = ShapeGamepadButtonAxisInput.GetState(neg, pos, gamepadIndex, deadzone);
            gamepadButtonAxisStates.Add(hashCode, state);
            return state;
        }

        var previousState = gamepadButtonAxisStates[hashCode];
        var newState = ShapeGamepadButtonAxisInput.GetState(neg, pos, previousState, gamepadIndex, deadzone);
        gamepadButtonAxisStates[hashCode] = newState;
        return newState;
    }
    public static ShapeInputState GetState(ShapeMouseWheelAxis axis, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        if (!mouseWheelAxisStates.ContainsKey(axis))
        {
            var state = ShapeMouseWheelAxisInput.GetState(axis);
            mouseWheelAxisStates.Add(axis, state);
            return state;
        }

        var previousState = mouseWheelAxisStates[axis];
        var newState = ShapeMouseWheelAxisInput.GetState(axis, previousState);
        mouseWheelAxisStates[axis] = newState;
        return newState;
    }
    public static ShapeInputState GetState(ShapeGamepadAxis axis, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        if (!gamepadAxisStates.ContainsKey(axis))
        {
            var state = ShapeGamepadAxisInput.GetState(axis, gamepadIndex, deadzone);
            gamepadAxisStates.Add(axis, state);
            return state;
        }

        var previousState = gamepadAxisStates[axis];
        var newState = ShapeGamepadAxisInput.GetState(axis, previousState, gamepadIndex, deadzone);
        gamepadAxisStates[axis] = newState;
        return newState;
    }
    
    public static List<char> GetKeyboardStreamChar()
    {
        if (Locked) return new();
        int unicode = Raylib.GetCharPressed();
        List<char> chars = new();
        while (unicode != 0)
        {
            var c = (char)unicode;
            chars.Add(c);

            unicode = Raylib.GetCharPressed();
        }
        return chars;
    }
    public static List<char> GetKeyboardStreamChar(uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        int unicode = Raylib.GetCharPressed();
        List<char> chars = new();
        while (unicode != 0)
        {
            var c = (char)unicode;
            chars.Add(c);

            unicode = Raylib.GetCharPressed();
        }
        return chars;
    }
    public static string GetKeyboardStream(uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return "";
        int unicode = Raylib.GetCharPressed();
        List<char> chars = new();
        while (unicode != 0)
        {
            var c = (char)unicode;
            chars.Add(c);

            unicode = Raylib.GetCharPressed();
        }

        StringBuilder b = new(chars.Count);
        b.Append(chars);
        return b.ToString();
    }
    public static string GetKeyboardStream(string curText, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return "";
        var chars = GetKeyboardStreamChar(accessTag);
        var b = new StringBuilder(chars.Count + curText.Length);
        b.Append(curText);
        b.Append(chars);
        return b.ToString();
    }
    #endregion

    #region Input Used
    public static bool WasKeyboardUsed() => Raylib.GetKeyPressed() < 0;
    public static bool WasMouseUsed(float moveThreshold = 5f, float mouseWheelThreshold = 1f)
    {
        var mouseDelta = Raylib.GetMouseDelta();
        if (mouseDelta.LengthSquared() > moveThreshold * moveThreshold) return true;
        var mouseWheel = Raylib.GetMouseWheelMoveV();
        if (mouseWheel.X > mouseWheelThreshold || mouseWheel.Y > mouseWheelThreshold) return true;

        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_EXTRA)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_FORWARD)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_BACK)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_SIDE)) return true;

        return false;
    }
    public static bool WasGamepadUsed(List<int> connectedGamepads, float deadzone = 0.2f)
    {
        if (Raylib.GetGamepadButtonPressed() < 0) return true;
        foreach (int gamepad in connectedGamepads)
        {
            if (Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X) > deadzone) return true;
            if (Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y) > deadzone) return true;
            if (Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X) > deadzone) return true;
            if (Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y) > deadzone) return true;
            if (Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) > deadzone) return true;
            if (Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) > deadzone) return true;
        }

        return false;
    }
    #endregion
}

// public class Gamepad
// {
//     public int Index { get; internal set; } = -1;
//     public bool Claimed { get; internal set; } = false;
// }

public enum InputDevice
{
    Keyboard = 1,
    Mouse = 2,
    Gamepad = 3
}