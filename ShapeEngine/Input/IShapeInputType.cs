namespace ShapeEngine.Input;

public interface IShapeInputType
{
    public IShapeInputType Copy();
    public float GetDeadzone();
    public void SetDeadzone(float value);
    public ShapeInputState GetState(int gamepad = -1);
    public ShapeInputState GetState(ShapeInputState prev, int gamepad = -1);
    public string GetName(bool shorthand = true);
    public InputDevice GetInputDevice();
}