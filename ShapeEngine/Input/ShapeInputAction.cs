using ShapeEngine.Lib;

namespace ShapeEngine.Input;

public class ShapeInputAction
{
    public uint ID { get; private set; }
    public uint AccessTag { get; private set; } = ShapeInput.AllAccessTag;
    public int GamepadIndex { get; set; } = -1;
    private bool consumed = false;
    public ShapeInputState State { get; private set; } = new();
    public ShapeInputState Consume()
    {
        if (consumed) return new();
        consumed = true;
        return State;
    }
    
    public readonly List<ShapeButton> Inputs = new();

    public ShapeInputAction()
    {
        ID = ShapeID.NextID;
    }
    public ShapeInputAction(uint accessTag, uint id)
    {
        ID = id;
        AccessTag = accessTag;
    }
    public ShapeInputAction(uint accessTag, int gamepadIndex)
    {
        ID = ShapeID.NextID;
        GamepadIndex = gamepadIndex;
        AccessTag = accessTag;
    }
    public ShapeInputAction(uint accessTag, uint id, int gamepadIndex)
    {
        ID = id;
        GamepadIndex = gamepadIndex;
        AccessTag = accessTag;
    }
    public ShapeInputAction(params ShapeButton[] buttons)
    {
        ID = ShapeID.NextID;
        Inputs.AddRange(buttons);
    }
    public ShapeInputAction(uint accessTag, uint id, params ShapeButton[] buttons)
    {
        ID = id;
        AccessTag = accessTag;
        Inputs.AddRange(buttons);
    }
    public ShapeInputAction(int gamepadIndex, params ShapeButton[] buttons)
    {
        ID = ShapeID.NextID;
        Inputs.AddRange(buttons);
        GamepadIndex = gamepadIndex;
    }
    public ShapeInputAction(uint accessTag, int gamepadIndex, params ShapeButton[] buttons)
    {
        ID = ShapeID.NextID;
        AccessTag = accessTag;
        GamepadIndex = gamepadIndex;
        Inputs.AddRange(buttons);
    }
    public ShapeInputAction(uint accessTag, uint id, int gamepadIndex, params ShapeButton[] buttons)
    {
        ID = id;
        AccessTag = accessTag;
        GamepadIndex = gamepadIndex;
        Inputs.AddRange(buttons);
    }
    
    public void Update(float dt)
    {
        consumed = false;
        ShapeInputState current = new();
        foreach (var input in Inputs)
        {
            input.Update(dt, GamepadIndex);
            current = current.Accumulate(input.InputType.GetState());
        }

        State = new(State, current);
    }
}