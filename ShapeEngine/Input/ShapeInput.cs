using System.Text;
using Raylib_CsLo;

namespace ShapeEngine.Input;

public class ShapeInput
{
    #region Members
    public static readonly uint AllAccessTag = 0;
    
    public bool Locked { get; private set; } = false;
    private readonly List<uint> lockWhitelist = new();
    private readonly List<uint> lockBlacklist = new();
    private readonly Dictionary<uint, InputAction> inputActions = new();
    #endregion
    
    //TODO last input device & last gamepad used should go here as well from shape loop!
    
    #region Lock System
    public void Lock()
    {
        Locked = true;
        lockWhitelist.Clear();
        lockBlacklist.Clear();
    }

    public void Lock(uint[] whitelist, uint[] blacklist)
    {
        Locked = true;
        lockWhitelist.Clear();
        lockBlacklist.Clear();
        if(whitelist.Length > 0) lockWhitelist.AddRange(whitelist);
        if(blacklist.Length > 0) lockWhitelist.AddRange(blacklist);
    }
    public void LockWhitelist(params uint[] whitelist)
    {
        Locked = true;
        lockWhitelist.Clear();
        lockBlacklist.Clear();
        if(whitelist.Length > 0) lockWhitelist.AddRange(whitelist);
        
    }
    public void LockBlacklist(params uint[] blacklist)
    {
        Locked = true;
        lockWhitelist.Clear();
        lockBlacklist.Clear();
        if(blacklist.Length > 0) lockWhitelist.AddRange(blacklist);
    }
    public void Unlock()
    {
        Locked = false;
        lockWhitelist.Clear();
        lockBlacklist.Clear();
    }
    public bool HasAccess(uint tag) => tag == AllAccessTag || (lockWhitelist.Contains(tag) && !lockBlacklist.Contains(tag));
    #endregion
    
    #region Input Actions
    public bool HasAction(uint id) => inputActions.ContainsKey(id);
    public uint AddAction(InputAction newAction)
    {
        var id = newAction.ID;
        if (HasAction(id)) inputActions[id] = newAction;
        else inputActions.Add(id, newAction);
        return id;
    }
    public void AddActions(params InputAction[] newActions)
    {
        foreach (var action in newActions)
        {
            AddAction(action);
        }
    }
    public bool RemoveAction(uint id) => inputActions.Remove(id);

    public InputState GetActionState(uint id)
    {
        if (!HasAction(id)) return new();
        var action = inputActions[id];
        return Locked && !HasAccess(action.AccessTag) ? new() : action.State;
    }
    public InputState ConsumeAction(uint id)
    {
        if (!HasAction(id)) return new();
        var action = inputActions[id];
        return Locked && !HasAccess(action.AccessTag) ? new() : action.Consume();
    }

    public InputAction? GetAction(uint id)
    {
        return !inputActions.ContainsKey(id) ? null : inputActions[id];
    }

    public void UpdateActionGamepad(int gamepad)
    {
        foreach (var input in inputActions.Values)
        {
            input.Gamepad = gamepad;
        }
    }
    public void UpdateActionGamepad(uint accessTag, int gamepad)
    {
        foreach (var input in inputActions.Values)
        {
            if(input.AccessTag != accessTag) continue;
            input.Gamepad = gamepad;
        }
    }

    public List<string> GetActionDescriptions(InputDevice inputDevice, bool shorthand,params uint[] actionIDs)
    {
        var actions = new List<InputAction>();
        foreach (var id in actionIDs)
        {
            var action = GetAction(id);
            if(action != null) actions.Add(action);
        }

        return GetActionDescriptions(inputDevice, shorthand, actions.ToArray());
    }
    public List<string> GetActionDescriptions(InputDevice inputDevice, bool shorthand, params InputAction[] actions)
    {
        var final = new List<string>();
        foreach (var action in actions)
        {
            var description = action.GetInputDescription(inputDevice, shorthand);
            
            final.Add(description);
        }

        return final;
    }

    
    #endregion

    public void Update(float dt)
    {
        foreach (var input in inputActions.Values)
        {
            input.Update(dt);
        }
    }
    #region Basic
    public InputState GetState(ShapeKeyboardButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return InputTypeKeyboardButton.GetState(button);
    }
    public InputState GetState(ShapeMouseButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return InputTypeMouseButton.GetState(button);
    }
    public InputState GetState(ShapeGamepadButton button, uint accessTag, int gamepad, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return InputTypeGamepadButton.GetState(button, gamepad, deadzone);
    }
    public InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return InputTypeKeyboardButtonAxis.GetState(neg, pos);
    }
    public InputState GetState(ShapeMouseButton neg, ShapeMouseButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return InputTypeMouseButtonAxis.GetState(neg, pos);
    }
    public InputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, uint accessTag, int gamepad, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return InputTypeGamepadButtonAxis.GetState(neg, pos, gamepad, deadzone);
    }
    public InputState GetState(ShapeMouseWheelAxis axis, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return InputTypeMouseWheelAxis.GetState(axis);
    }
    public InputState GetState(ShapeGamepadAxis axis, uint accessTag, int gamepad, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return InputTypeGamepadAxis.GetState(axis, gamepad, deadzone);
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
    public static bool WasKeyboardUsed() => Raylib.GetKeyPressed() > 0;
    public static bool WasMouseUsed(float moveThreshold = 0.5f, float mouseWheelThreshold = 0.25f)
    {
        var mouseDelta = Raylib.GetMouseDelta();
        if (mouseDelta.LengthSquared() > moveThreshold * moveThreshold) return true;
        var mouseWheel = Raylib.GetMouseWheelMoveV();
        if (mouseWheel.LengthSquared() > mouseWheelThreshold * mouseWheelThreshold) return true;

        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_EXTRA)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_FORWARD)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_BACK)) return true;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_SIDE)) return true;

        return false;
    }
    public static int WasGamepadUsed(List<int> connectedGamepads, float deadzone = 0.05f)
    {
        var gamepadButton = WasGamepadButtonUsed(connectedGamepads);
        if (gamepadButton >= 0) return gamepadButton;
        //if (Raylib.GetGamepadButtonPressed() > 0) return true;
        foreach (int gamepad in connectedGamepads)
        {
            if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X)) > deadzone) return gamepad;
            if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y)) > deadzone) return gamepad;
            if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X)) > deadzone) return gamepad;
            if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y)) > deadzone) return gamepad;
            if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1f) / 2f > deadzone / 2) return gamepad;
            if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1f) / 2f > deadzone / 2) return gamepad;
        }

        return -1;
    }

    public static int WasGamepadButtonUsed(List<int> connectedGamepads)
    {
        var values = Enum.GetValues<GamepadButton>();
        foreach (var gamepad in connectedGamepads)
        {
            foreach (var b in  values)
            {
                if (Raylib.IsGamepadButtonDown(gamepad, b)) return gamepad;
            }
        }

        return -1;
        
        // GAMEPAD_BUTTON_UNKNOWN,
        // GAMEPAD_BUTTON_LEFT_FACE_UP,
        // GAMEPAD_BUTTON_LEFT_FACE_RIGHT,
        // GAMEPAD_BUTTON_LEFT_FACE_DOWN,
        // GAMEPAD_BUTTON_LEFT_FACE_LEFT,
        // GAMEPAD_BUTTON_RIGHT_FACE_UP,
        // GAMEPAD_BUTTON_RIGHT_FACE_RIGHT,
        // GAMEPAD_BUTTON_RIGHT_FACE_DOWN,
        // GAMEPAD_BUTTON_RIGHT_FACE_LEFT,
        // GAMEPAD_BUTTON_LEFT_TRIGGER_1,
        // GAMEPAD_BUTTON_LEFT_TRIGGER_2,
        // GAMEPAD_BUTTON_RIGHT_TRIGGER_1,
        // GAMEPAD_BUTTON_RIGHT_TRIGGER_2,
        // GAMEPAD_BUTTON_MIDDLE_LEFT,
        // GAMEPAD_BUTTON_MIDDLE,
        // GAMEPAD_BUTTON_MIDDLE_RIGHT,
        // GAMEPAD_BUTTON_LEFT_THUMB,
        // GAMEPAD_BUTTON_RIGHT_THUMB,
    }
    #endregion
}