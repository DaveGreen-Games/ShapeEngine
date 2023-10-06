namespace ShapeEngine.Input;

public interface IInputType
{
    public IInputType Copy();
    public float GetDeadzone();
    public void SetDeadzone(float value);
    public InputState GetState(int gamepad = -1);
    public InputState GetState(InputState prev, int gamepad = -1);
    public string GetName(bool shorthand = true);
    public InputDevice GetInputDevice();
}