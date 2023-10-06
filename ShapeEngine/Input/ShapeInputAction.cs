using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class ShapeInputAction
{
    public uint ID { get; private set; }
    public uint AccessTag { get; private set; } = ShapeInput.AllAccessTag;
    
    public int Gamepad = -1;
    
    public ShapeInputState State { get; private set; } = new();
    public ShapeInputState Consume()
    {
        var returnValue = State;
        State = State.Consume();
        return returnValue;
    }
    
    public readonly List<IShapeInputType> Inputs = new();

    public ShapeInputAction()
    {
        ID = ShapeID.NextID;
    }
    public ShapeInputAction(uint accessTag)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
    }
    public ShapeInputAction(uint accessTag, int gamepad)
    {
        ID = ShapeID.NextID;
        Gamepad = gamepad;
        AccessTag = accessTag;
    }
    public ShapeInputAction(uint accessTag, uint id)
    {
        ID = id;
        AccessTag = accessTag;
    }
    public ShapeInputAction(uint accessTag, uint id, int gamepad)
    {
        ID = id;
        AccessTag = accessTag;
        Gamepad = gamepad;
    }
    public ShapeInputAction(params IShapeInputType[] inputTypes)
    {
        ID = ShapeID.NextID;
        Inputs.AddRange(inputTypes);
    }
    public ShapeInputAction(uint accessTag, params IShapeInputType[] inputTypes)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
        Inputs.AddRange(inputTypes);
    }
    public ShapeInputAction(uint accessTag, uint id, params IShapeInputType[] inputTypes)
    {
        ID = id;
        AccessTag = accessTag;
        Inputs.AddRange(inputTypes);
    }
    public ShapeInputAction(uint accessTag, uint id, int gamepad, params IShapeInputType[] inputTypes)
    {
        ID = id;
        AccessTag = accessTag;
        Gamepad = gamepad;
        Inputs.AddRange(inputTypes);
    }

    public void Update(float dt)
    {
        ShapeInputState current = new();
        foreach (var input in Inputs)
        {
            var state = input.GetState(Gamepad); // input.Update(dt, Gamepad);
            current = current.Accumulate(state);
        }
        State = new(State, current);
    }
    
    
    public static IShapeInputType CreateInputType(ShapeKeyboardButton button) => new ShapeKeyboardButtonInput(button);
    public static IShapeInputType CreateInputType(ShapeMouseButton button) => new ShapeMouseButtonInput(button);
    public static IShapeInputType CreateInputType(ShapeGamepadButton button, float deadzone = 0.2f) => new ShapeGamepadButtonInput(button, deadzone);
    public static IShapeInputType CreateInputType(ShapeKeyboardButton neg, ShapeKeyboardButton pos) => new ShapeKeyboardButtonAxisInput(neg, pos);
    public static IShapeInputType CreateInputType(ShapeMouseButton neg, ShapeMouseButton pos) => new ShapeMouseButtonAxisInput(neg, pos);
    public static IShapeInputType CreateInputType(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f) => new ShapeGamepadButtonAxisInput(neg, pos, deadzone);
    public static IShapeInputType CreateInputType(ShapeMouseWheelAxis mouseWheelAxis) => new ShapeMouseWheelAxisInput(mouseWheelAxis);
    public static IShapeInputType CreateInputType(ShapeGamepadAxis gamepadAxis, float deadzone = 0.2f) => new ShapeGamepadAxisInput(gamepadAxis, deadzone);
}