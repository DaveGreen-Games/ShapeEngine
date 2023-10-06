namespace ShapeEngine.Input;

public interface IShapeInputType
{
    public IShapeInputType Copy();
    //public ShapeInputState Update(float dt, int gamepadIndex);
    //public ShapeInputState Consume();
    public ShapeInputState GetState(int gamepad = -1);
    public ShapeInputState GetState(ShapeInputState prev, int gamepad = -1);
    public string GetName(bool shorthand = true);
    public InputDevice GetInputDevice();

    //public ShapeInputState GetState();
}