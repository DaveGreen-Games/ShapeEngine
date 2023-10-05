using System.Text;

namespace ShapeEngine.Input;

public class ShapeGamepadButtonAxisInput : IShapeInputType
{
    private readonly ShapeGamepadButton neg;
    private readonly ShapeGamepadButton pos;
    private readonly float deadzone;
    private ShapeInputState state = new();

    public ShapeGamepadButtonAxisInput(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
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

    public void Update(float dt, int gamepadIndex)
    {
        state = GetState(neg, pos, state, gamepadIndex, deadzone);
    }
    public ShapeInputState GetState() => state;

    public IShapeInputType Copy() => new ShapeGamepadButtonAxisInput(neg, pos, deadzone);

    private static float GetAxis(ShapeGamepadButton neg, ShapeGamepadButton pos, int gamepadIndex, float deadzone = 0.2f)
    {
        float vNegative = GetValue(neg, gamepadIndex, deadzone);
        float vPositive = GetValue(pos, gamepadIndex, deadzone);
        return vPositive - vNegative;
    }
    private static float GetValue(ShapeGamepadButton button, int gamepadIndex, float deadzone = 0.2f)
    {
        if (gamepadIndex < 0) return 0f;

        int id = (int)button;
        if (id >= 30 && id <= 33)
        {
            id -= 30;
            float value = GetGamepadAxisMovement(gamepadIndex, id);
            if (MathF.Abs(value) < deadzone) return 0f;
            if (value > 0f) return value;
            
            return 0f;
        }
        
        if (id >= 40 && id <= 43)
        {
            id -= 40;
            float value = GetGamepadAxisMovement(gamepadIndex, id);
            if (MathF.Abs(value) < deadzone) return 0f;
            if (value < 0) return MathF.Abs(value);
            
            return 0f;
        }
        
        return IsGamepadButtonDown(gamepadIndex, id) ? 1f : 0f;
    }
    public static ShapeInputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, int gamepadIndex, float deadzone = 0.2f)
    {
        float axis = GetAxis(neg, pos, gamepadIndex, deadzone);
        bool down = axis != 0f;
        return new(down, !down, axis, -1);
    }
    public static ShapeInputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos,
        ShapeInputState previousState, int gamepadIndex, float deadzone = 0.2f)
    {
        return new(previousState, GetState(neg, pos, gamepadIndex, deadzone));
    }
    
}