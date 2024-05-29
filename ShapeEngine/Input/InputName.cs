namespace ShapeEngine.Input;

public readonly struct InputName
{
    public readonly string Name;
    public readonly InputDeviceType InputDeviceType;

    public InputName(string name, InputDeviceType inputDeviceType)
    {
        Name = name;
        InputDeviceType = inputDeviceType;
    }
}