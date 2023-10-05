namespace ShapeEngine.Input;

public class ShapeGamepadAxisInput : IShapeInputType
{
    private readonly ShapeGamepadAxis axis;
    private readonly float deadzone;
    private ShapeInputState state = new();

    public ShapeGamepadAxisInput(ShapeGamepadAxis axis, float deadzone = 0.2f)
    {
        this.axis = axis; 
        this.deadzone = deadzone;
    }

    public string GetName(bool shorthand = true) => GetGamepadAxisName(axis, shorthand);
    public void Update(float dt, int gamepadIndex)
    {
        state = GetState(axis, state, gamepadIndex, deadzone);
    }
    public ShapeInputState GetState() => state;
    public IShapeInputType Copy() => new ShapeGamepadAxisInput(axis);

    private static float GetValue(ShapeGamepadAxis axis, int gamepadIndex, float deadzone = 0.2f)
    {
        float value = GetGamepadAxisMovement(gamepadIndex, (int)axis);
        if (MathF.Abs(value) < deadzone) return 0f;
        return value;
    }
    public static ShapeInputState GetState(ShapeGamepadAxis axis, int gamepadIndex, float deadzone = 0.2f)
    {
        float axisValue = GetValue(axis, gamepadIndex, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, -1);
    }
    public static ShapeInputState GetState(ShapeGamepadAxis axis, ShapeInputState previousState, int gamepadIndex,
        float deadzone = 0.2f)
    {
        return new(previousState, GetState(axis, gamepadIndex, deadzone));
    }
    public static string GetGamepadAxisName(ShapeGamepadAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeGamepadAxis.LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
            case ShapeGamepadAxis.LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
            case ShapeGamepadAxis.RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
            case ShapeGamepadAxis.RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
            case ShapeGamepadAxis.RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
            case ShapeGamepadAxis.LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
            default: return "No Key";
        }
    }

}