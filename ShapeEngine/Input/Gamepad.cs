using System.Diagnostics.CodeAnalysis;
using Raylib_CsLo;

namespace ShapeEngine.Input;

public class Gamepad
{
    public static readonly GamepadButton[] AllGamepadButtons = Enum.GetValues<GamepadButton>();
    public static readonly GamepadAxis[] AllGamepadAxis = Enum.GetValues<GamepadAxis>();
    public static readonly ShapeGamepadButton[] AllShapeGamepadButtons = Enum.GetValues<ShapeGamepadButton>();
    public static readonly ShapeGamepadAxis[] AllShapeGamepadAxis = Enum.GetValues<ShapeGamepadAxis>();
    
    public readonly int Index;
    
    public bool Available { get; private set; } = true;
    public bool Connected { get; private set; }

    public string Name { get; private set; } = "No Device";
    public int AxisCount { get; private set; } = 0;

    public bool WasUsed { get; private set; } = false;
    public readonly List<ShapeGamepadButton> UsedButtons = new();
    public readonly List<ShapeGamepadAxis> UsedAxis = new();
    
    public event Action? OnConnectionChanged;
    public event Action? OnAvailabilityChanged;
    
    public Gamepad(int index, bool connected)
    {
        Index = index;
        Connected = connected;
        if (Connected)
        {
            unsafe
            {
                Name = Raylib.GetGamepadName(index)->ToString();
            }

            AxisCount = Raylib.GetGamepadAxisCount(index);
        }
        
    }

    public void Update()
    {
        WasUsed = false;
        if(UsedButtons.Count > 0) UsedButtons.Clear();
        if(UsedAxis.Count > 0) UsedAxis.Clear();
        
        if (!Connected) return;
        
        var usedButtons = GetUsedGamepadButtons(Index);
        var usedAxis = GetUsedGamepadAxis(Index);
        WasUsed = usedButtons.Count > 0 || usedAxis.Count > 0;
        UsedButtons.AddRange(usedButtons);
        UsedAxis.AddRange(usedAxis);
    }
    
    public void Connect()
    {
        if (Connected) return;
        Connected = true;
        unsafe
        {
            Name = Raylib.GetGamepadName(Index)->ToString();
        }

        AxisCount = Raylib.GetGamepadAxisCount(Index);
        OnConnectionChanged?.Invoke();
    }
    public void Disconnect()
    {
        if (!Connected) return;
        Connected = false;
        OnConnectionChanged?.Invoke();
    }
    public bool Claim()
    {
        if (!Connected || !Available) return false;
        Available = false;
        OnAvailabilityChanged?.Invoke();
        return true;
    }
    public bool Free()
    {
        if (Available) return false;
        Available = true;
        OnAvailabilityChanged?.Invoke();
        return true;
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
    
    
}