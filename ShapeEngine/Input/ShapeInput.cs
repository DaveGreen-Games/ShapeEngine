using System.Text;
using Raylib_CsLo;
using ShapeEngine.Stats;

namespace ShapeEngine.Input;

//does shape loop have a static ShapeInput member or should shape input be static?
//I think shape input as static class makes more sense? most input stuff is global and for the current
//application...
public class ShapeInput
{
    #region Members
    public static readonly GamepadButton[] AllGamepadButtons = Enum.GetValues<GamepadButton>();
    public static readonly GamepadAxis[] AllGamepadAxis = Enum.GetValues<GamepadAxis>();
    public static readonly KeyboardKey[] AllKeyboardKeys = Enum.GetValues<KeyboardKey>();
    public static readonly MouseButton[] AllMouseButtons = Enum.GetValues<MouseButton>();
    
    public static readonly ShapeGamepadButton[] AllShapeGamepadButtons = Enum.GetValues<ShapeGamepadButton>();
    public static readonly ShapeGamepadAxis[] AllShapeGamepadAxis = Enum.GetValues<ShapeGamepadAxis>();
    public static readonly ShapeKeyboardButton[] AllShapeKeyboardButtons = Enum.GetValues<ShapeKeyboardButton>();
    public static readonly ShapeMouseButton[] AllShapeMouseButtons = Enum.GetValues<ShapeMouseButton>();
    
    public static readonly uint AllAccessTag = 0;
    
    public bool Locked { get; private set; } = false;
    private readonly List<uint> lockWhitelist = new();
    private readonly List<uint> lockBlacklist = new();
    //private static readonly Dictionary<uint, InputAction> inputActions = new();
    
    private readonly Gamepad[] gamepads = new Gamepad[8];
    private readonly List<int> connectedGamepadIndices = new();
    public InputDevice CurrentInputDevice { get; private set; } = InputDevice.Keyboard;

    public InputDevice CurrentInputDeviceNoMouse => FilterInputDevice(CurrentInputDevice, InputDevice.Mouse, InputDevice.Keyboard);

    public static InputDevice FilterInputDevice(InputDevice current, InputDevice replace, InputDevice with)
    {
        return current == replace
            ? with
            : current;
    }

    public int MaxGamepads => gamepads.Length;
    public Gamepad? LastUsedGamepad { get; private set; } = null;
    #endregion

    public event Action<Gamepad, bool>? OnGamepadConnectionChanged;
    public event Action<InputDevice, InputDevice>? OnInputDeviceChanged;

    public ShapeInput()
    {
        GamepadSetup();
    }
    public void Update()
    {
        CheckGamepadConnections();
        CheckInputDevice();
    }
    public string GetCurInputDeviceGenericName()
    {
        return
            CurrentInputDevice == InputDevice.Gamepad ? "Gamepad" :
            CurrentInputDevice == InputDevice.Keyboard ? "Keyboard" : "Mouse";
    }
    public string GetInputDeviceGenericName(InputDevice device)
    {
        return
            device == InputDevice.Gamepad ? "Gamepad" :
            device == InputDevice.Keyboard ? "Keyboard" : "Mouse";
    }
    
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
        if(blacklist.Length > 0) lockBlacklist.AddRange(blacklist);
    }
    public void Unlock()
    {
        Locked = false;
        lockWhitelist.Clear();
        lockBlacklist.Clear();
    }
    public bool HasAccess(uint tag) => tag == AllAccessTag || (lockWhitelist.Contains(tag) && !lockBlacklist.Contains(tag));
    public bool HasAccess(InputAction action) => HasAccess(action.AccessTag);
    #endregion
    
    #region Input Actions
    public InputState GetActionState(InputAction action)
    {
        return Locked && !HasAccess(action.AccessTag) ? new() : action.State;
    }
    public InputState ConsumeAction(InputAction action)
    {
        return Locked && !HasAccess(action.AccessTag) ? new() : action.Consume();
    }
    public void UpdateActions(float dt, int gamepad, params InputAction[] actions)
    {
        foreach (var action in actions)
        {
            action.Gamepad = gamepad;
            action.Update(dt);
        }
    }
    public void UpdateActions(float dt, int gamepad, List<InputAction> actions)
    {
        foreach (var action in actions)
        {
            action.Gamepad = gamepad;
            action.Update(dt);
        }
    }
    public List<string> GetActionDescriptions(InputDevice inputDevice, bool shorthand, int typesPerActionCount, List<InputAction> actions)
    {
        var final = new List<string>();
        foreach (var action in actions)
        {
            var description = action.GetInputTypeDescription(inputDevice, shorthand, typesPerActionCount, true);
            
            final.Add(description);
        }

        return final;
    }
    public List<string> GetActionDescriptions(InputDevice inputDevice, bool shorthand, int typesPerActionCount, params InputAction[] actions)
    {
        var final = new List<string>();
        foreach (var action in actions)
        {
            var description = action.GetInputTypeDescription(inputDevice, shorthand, typesPerActionCount, true);
            
            final.Add(description);
        }

        return final;
    }

    
    #endregion

    #region Gamepad
    public bool HasGamepad(int index) => index >= 0 && index < gamepads.Length;
    public bool IsGamepadConnected(int index) => HasGamepad(index) && gamepads[index].Connected;
    public Gamepad? GetGamepad(int index)
    {
        if (!HasGamepad(index)) return null;
        return gamepads[index];
    }
    public Gamepad? RequestGamepad(int preferredIndex = -1)
    {
        var preferredGamepad = GetGamepad(preferredIndex);
        if (preferredGamepad is { Connected: true, Available: true })
        {
            preferredGamepad.Claim();
            return preferredGamepad;
        }

        foreach (var gamepad in gamepads)
        {
            if (gamepad is { Connected: true, Available: true })
            {
                gamepad.Claim();
                return gamepad;
            }
        }
        return null;
    }
    // public Gamepad? RequestConnectedGamepad(int preferredIndex = -1)
    // {
    //     var preferredGamepad = GetGamepad(preferredIndex);
    //     if (preferredGamepad is { Connected: true})
    //     {
    //         preferredGamepad.Claim();
    //         return preferredGamepad;
    //     }
    //
    //     foreach (var gamepad in gamepads)
    //     {
    //         if (gamepad is { Connected: true})
    //         {
    //             gamepad.Claim();
    //             return gamepad;
    //         }
    //     }
    //     return null;
    // }
    public void ReturnGamepad(int index) => GetGamepad(index)?.Free();
    public void ReturnGamepad(Gamepad gamepad) => GetGamepad(gamepad.Index)?.Free();

    #endregion
    
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

    #region Private

    private void CheckInputDevice()
    {
        var prevInputDevice = CurrentInputDevice;
        if (CurrentInputDevice == InputDevice.Keyboard)
        {
            if (ShapeInput.WasMouseUsed()) CurrentInputDevice = InputDevice.Mouse;
            else
            {
                var index = ShapeInput.WasGamepadUsed(connectedGamepadIndices);
                if (index >= 0)
                {
                    CurrentInputDevice = InputDevice.Gamepad;
                    LastUsedGamepad = GetGamepad(index);
                }
            }
            //else if (ShapeInput.WasGamepadUsed(connectedGamepadIndices)) CurrentInputDevice = InputDevice.Gamepad;
        }
        else if (CurrentInputDevice == InputDevice.Mouse)
        {
            if (ShapeInput.WasKeyboardUsed()) CurrentInputDevice = InputDevice.Keyboard;
            else
            {
                var index = ShapeInput.WasGamepadUsed(connectedGamepadIndices);
                if (index >= 0)
                {
                    CurrentInputDevice = InputDevice.Gamepad;
                    LastUsedGamepad = GetGamepad(index);
                }
            }
            //else if (ShapeInput.WasGamepadUsed(connectedGamepadIndices)) CurrentInputDevice = InputDevice.Gamepad;
        }
        else //gamepad
        {
            if (ShapeInput.WasMouseUsed()) CurrentInputDevice = InputDevice.Mouse;
            else if (ShapeInput.WasKeyboardUsed()) CurrentInputDevice = InputDevice.Keyboard;
        }

        if (CurrentInputDevice != prevInputDevice)
        {
            OnInputDeviceChanged?.Invoke(prevInputDevice, CurrentInputDevice);
        }
    }
    private void CheckGamepadConnections()
    {
        connectedGamepadIndices.Clear();
        for (var i = 0; i < gamepads.Length; i++)
        {
            var gamepad = gamepads[i];
            if (Raylib.IsGamepadAvailable(i))
            {
                if (!gamepad.Connected)
                {
                    gamepad.Connect();
                    OnGamepadConnectionChanged?.Invoke(gamepad, true);
                }
                connectedGamepadIndices.Add(i);
            }
            else
            {
                if (gamepad.Connected)
                {
                    gamepad.Disconnect();
                    OnGamepadConnectionChanged?.Invoke(gamepad, false);
                }
            }
        }
        
    }
    private void GamepadSetup()
    {
        for (var i = 0; i < gamepads.Length; i++)
        {
            gamepads[i] = new Gamepad(i, Raylib.IsGamepadAvailable(i));
        }
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
        foreach (var gamepad in connectedGamepads)
        {
            if (WasGamepadUsed(gamepad, deadzone)) return gamepad;
        }
        // var gamepadFromButton = WasGamepadButtonUsed(connectedGamepads);
        // if (gamepadFromButton >= 0) return gamepadFromButton;
        // //if (Raylib.GetGamepadButtonPressed() > 0) return true;
        // foreach (int gamepad in connectedGamepads)
        // {
        //     if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X)) > deadzone) return gamepad;
        //     if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y)) > deadzone) return gamepad;
        //     if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X)) > deadzone) return gamepad;
        //     if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y)) > deadzone) return gamepad;
        //     if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1f) / 2f > deadzone / 2) return gamepad;
        //     if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1f) / 2f > deadzone / 2) return gamepad;
        // }

        return -1;
    }
    public static int WasGamepadButtonUsed(List<int> connectedGamepads)
    {
        foreach (var gamepad in connectedGamepads)
        {
            if (WasGamepadButtonUsed(gamepad)) return gamepad;
            // foreach (var b in  AllGamepadButtons)
            // {
            //     if (Raylib.IsGamepadButtonDown(gamepad, b)) return gamepad;
            // }
        }

        return -1;
    }
    public static int WasGamepadAxisUsed(List<int> connectedGamepads, float deadzone = 0.05f)
    {
        foreach (var gamepad in connectedGamepads)
        {
            if (WasGamepadAxisUsed(gamepad, deadzone)) return gamepad;
        }
        // if (MathF.Abs(Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X)) > deadzone) return true;
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y)) > deadzone) return true;
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X)) > deadzone) return true;
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y)) > deadzone) return true;
        // if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1f) / 2f > deadzone / 2) return true;
        // if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1f) / 2f > deadzone / 2) return true;

        return -1;
    }
    public static bool WasGamepadUsed(int gamepad, float deadzone = 0.05f)
    {
        return WasGamepadButtonUsed(gamepad) || WasGamepadAxisUsed(gamepad, deadzone);
    }
    public static bool WasGamepadButtonUsed(int gamepad)
    {
        foreach (var b in  AllGamepadButtons)
        {
            if (Raylib.IsGamepadButtonDown(gamepad, b)) return true;
        }

        return false;
    }
    public static bool WasGamepadAxisUsed(int gamepad, float deadzone = 0.05f)
    {
        if (MathF.Abs(Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X)) > deadzone) return true;
        if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y)) > deadzone) return true;
        if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X)) > deadzone) return true;
        if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y)) > deadzone) return true;
        if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1f) / 2f > deadzone / 2) return true;
        if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1f) / 2f > deadzone / 2) return true;

        return false;
    }
    public static List<int> GetUsedGamepads(List<int> connectedGamepads, float deadzone = 0.05f)
    {
        var usedGamepads = new List<int>();
        foreach (var gamepad in connectedGamepads)
        {
            if(WasGamepadUsed(gamepad, deadzone)) usedGamepads.Add(gamepad);
        }

        return usedGamepads;
    }
    public static List<ShapeGamepadButton> GetUsedGamepadButtons(int gamepad)
    {
        var usedButtons = new List<ShapeGamepadButton>();
        var values = Enum.GetValues<ShapeGamepadButton>();
        foreach (var b in  values)
        {
            if (InputTypeGamepadButton.IsDown(b, gamepad, 0))
            {
                usedButtons.Add(b);
            }
        }
        return usedButtons;
    }
    public static List<ShapeGamepadAxis> GetUsedGamepadAxis(int gamepad, float deadzone = 0.2f)
    {
        var usedAxis = new List<ShapeGamepadAxis>();
        
        if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X)) > deadzone) usedAxis.Add(ShapeGamepadAxis.LEFT_X);
        if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y)) > deadzone) usedAxis.Add(ShapeGamepadAxis.LEFT_Y);
        if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X)) > deadzone) usedAxis.Add(ShapeGamepadAxis.RIGHT_X);
        if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y)) > deadzone) usedAxis.Add(ShapeGamepadAxis.RIGHT_Y);
        if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1f) / 2f > deadzone / 2) usedAxis.Add(ShapeGamepadAxis.LEFT_TRIGGER);
        if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1f) / 2f > deadzone / 2) usedAxis.Add(ShapeGamepadAxis.RIGHT_TRIGGER);

        return usedAxis;
    }
    
    #endregion
}