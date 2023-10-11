namespace ShapeEngine.Input;

public readonly struct InputName
{
    public readonly string Name;
    public readonly InputDevice InputDevice;

    public InputName(string name, InputDevice inputDevice)
    {
        Name = name;
        InputDevice = inputDevice;
    }
}