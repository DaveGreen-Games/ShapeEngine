using System.Text;
using Raylib_CsLo;

namespace ShapeEngine.Input;

public class ShapeInput
{
    #region Members
    public static readonly uint AllAccessTag = 0;
    
    public bool Locked { get; private set; } = false;
    private readonly List<uint> lockExceptionTags = new();
    private readonly Dictionary<uint, ShapeInputAction> inputActions = new();
    #endregion
    
    #region Lock System
    public void Lock()
    {
        Locked = true;
        lockExceptionTags.Clear();
    }
    public void Lock(params uint[] exceptionTags)
    {
        Locked = true;
        lockExceptionTags.Clear();
        if(exceptionTags.Length > 0) lockExceptionTags.AddRange(exceptionTags);
    }
    public void Unlock()
    {
        Locked = false;
        lockExceptionTags.Clear();
    }
    public bool HasAccess(uint tag) => tag == AllAccessTag || lockExceptionTags.Contains(tag);
    #endregion
    
    #region Input Actions
    public bool HasAction(uint id) => inputActions.ContainsKey(id);
    public uint AddAction(ShapeInputAction newAction)
    {
        var id = newAction.ID;
        if (HasAction(id)) inputActions[id] = newAction;
        else inputActions.Add(id, newAction);
        return id;
    }
    public bool RemoveAction(uint id) => inputActions.Remove(id);

    public ShapeInputState GetActionState(uint id)
    {
        if (!HasAction(id)) return new();
        var action = inputActions[id];
        return Locked && !HasAccess(action.AccessTag) ? new() : action.State;
    }
    public ShapeInputState ConsumeAction(uint id)
    {
        if (!HasAction(id)) return new();
        var action = inputActions[id];
        return Locked && !HasAccess(action.AccessTag) ? new() : action.Consume();
    }

    public ShapeInputAction? GetAction(uint id)
    {
        return !inputActions.ContainsKey(id) ? null : inputActions[id];
    }
    #endregion

    #region Basic
    public ShapeInputState GetState(ShapeKeyboardButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeKeyboardButtonInput.GetState(button);
    }
    public ShapeInputState GetState(ShapeMouseButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeMouseButtonInput.GetState(button);
    }
    public ShapeInputState GetState(ShapeGamepadButton button, uint accessTag, int gamepad, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeGamepadButtonInput.GetState(button, gamepad, deadzone);
    }
    public ShapeInputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeKeyboardButtonAxisInput.GetState(neg, pos);
    }
    public ShapeInputState GetState(ShapeMouseButton neg, ShapeMouseButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeMouseButtonAxisInput.GetState(neg, pos);
    }
    public ShapeInputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, uint accessTag, int gamepad, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeGamepadButtonAxisInput.GetState(neg, pos, gamepad, deadzone);
    }
    public ShapeInputState GetState(ShapeMouseWheelAxis axis, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeMouseWheelAxisInput.GetState(axis);
    }
    public ShapeInputState GetState(ShapeGamepadAxis axis, uint accessTag, int gamepad, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeGamepadAxisInput.GetState(axis, gamepad, deadzone);
    }
    
    public List<char> GetKeyboardStreamChar()
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
    public List<char> GetKeyboardStreamChar(uint accessTag)
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
    public string GetKeyboardStream(uint accessTag)
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
    public string GetKeyboardStream(string curText, uint accessTag)
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

public enum InputDevice
{
    Keyboard = 1,
    Mouse = 2,
    Gamepad = 3
}