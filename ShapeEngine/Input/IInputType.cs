namespace ShapeEngine.Input;

public interface IInputType
{
    public IInputType Copy();
    public float GetDeadzone();
    public void SetDeadzone(float value);
    /*public float GetSensitivity();
    /// <summary>
    /// How fast the axis goes to max value (-1/1) when input is detected.
    /// </summary>
    /// <param name="seconds">How many seconds it takes.</param>
    public void SetSensitivity(float seconds);
    public float GetGravitiy();
    /// <summary>
    /// How fast the axis goes back to 0 when no input is detected anymore.
    /// </summary>
    /// <param name="seconds">How many seconds it takes.</param>
    public void SetGravitiy(float seconds);*/
    public InputState GetState(int gamepad = -1);
    public InputState GetState(InputState prev, int gamepad = -1);
    public string GetName(bool shorthand = true);
    public InputDevice GetInputDevice();
}