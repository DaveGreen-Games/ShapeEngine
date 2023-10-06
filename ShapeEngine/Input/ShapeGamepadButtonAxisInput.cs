using System.Text;

namespace ShapeEngine.Input;

public class ShapeGamepadButtonAxisInput : IShapeInputType
{
    private readonly ShapeGamepadButton neg;
    private readonly ShapeGamepadButton pos;
    public float Deadzone { get; set; }

    public ShapeGamepadButtonAxisInput(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f)
    {
        this.neg = neg;
        this.pos = pos;
        this.Deadzone = deadzone;
    }

    public string GetName(bool shorthand = true)
    {
        string negName = ShapeGamepadButtonInput.GetGamepadButtonName(neg, shorthand);
        string posName = ShapeGamepadButtonInput.GetGamepadButtonName(pos, shorthand);
        StringBuilder b = new(negName.Length + posName.Length + 4);
        b.Append(negName);
        b.Append(" <> ");
        b.Append(posName);
        return b.ToString();
    }

    public ShapeInputState GetState(int gamepad = -1)
    {
        return GetState(neg, pos, gamepad, Deadzone);
    }

    public ShapeInputState GetState(ShapeInputState prev, int gamepad = -1)
    {
        return GetState(neg, pos, prev, gamepad, Deadzone);
    }

    public InputDevice GetInputDevice() => InputDevice.Gamepad;

    public IShapeInputType Copy() => new ShapeGamepadButtonAxisInput(neg, pos, Deadzone);

    private static float GetAxis(ShapeGamepadButton neg, ShapeGamepadButton pos, int gamepad, float deadzone = 0.2f)
    {
        float vNegative = GetValue(neg, gamepad, deadzone);
        float vPositive = GetValue(pos, gamepad, deadzone);
        return vPositive - vNegative;
    }
    private static float GetValue(ShapeGamepadButton button, int gamepad, float deadzone = 0.2f)
    {
        if (gamepad < 0) return 0f;

        int id = (int)button;
        if (id >= 30 && id <= 33)
        {
            id -= 30;
            float value = GetGamepadAxisMovement(gamepad, id);
            if (MathF.Abs(value) < deadzone) return 0f;
            if (value > 0f) return value;
            
            return 0f;
        }
        
        if (id >= 40 && id <= 43)
        {
            id -= 40;
            float value = GetGamepadAxisMovement(gamepad, id);
            if (MathF.Abs(value) < deadzone) return 0f;
            if (value < 0) return MathF.Abs(value);
            
            return 0f;
        }
        
        return IsGamepadButtonDown(gamepad, id) ? 1f : 0f;
    }
    public static ShapeInputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, int gamepad, float deadzone = 0.2f)
    {
        float axis = GetAxis(neg, pos, gamepad, deadzone);
        bool down = axis != 0f;
        return new(down, !down, axis, gamepad);
    }
    public static ShapeInputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos,
        ShapeInputState previousState, int gamepad, float deadzone = 0.2f)
    {
        return new(previousState, GetState(neg, pos, gamepad, deadzone));
    }
    
}