
namespace ShapeEngine.Input;

public static class ShapeInput
{
    public static InputDeviceType CurrentInputDeviceType { get; private set; } = InputDeviceType.Keyboard;

    public static InputDeviceType CurrentInputDeviceTypeNoMouse => FilterInputDevice(CurrentInputDeviceType, InputDeviceType.Mouse, InputDeviceType.Keyboard);
    
    public static event Action<InputDeviceType, InputDeviceType>? OnInputDeviceChanged;
    
    
    public static readonly ShapeKeyboardDevice KeyboardDevice = new();
    public static readonly ShapeMouseDevice MouseDevice = new();
    public static readonly ShapeGamepadDeviceManager GamepadDeviceManager = new();
    
    
    internal static void Update()
    {
        KeyboardDevice.Update();
        MouseDevice.Update();
        GamepadDeviceManager.Update();
        CheckInputDevice();
    }

    

    #region InputDeviceType

    public static InputDeviceType FilterInputDevice(InputDeviceType current, InputDeviceType replace, InputDeviceType with)
    {
        return current == replace
            ? with
            : current;
    }

    public static string GetCurInputDeviceGenericName()
    {
        return
            CurrentInputDeviceType == InputDeviceType.Gamepad ? "Gamepad" :
            CurrentInputDeviceType == InputDeviceType.Keyboard ? "Keyboard" : "Mouse";
    }
    public static string GetInputDeviceGenericName(InputDeviceType deviceType)
    {
        return
            deviceType == InputDeviceType.Gamepad ? "Gamepad" :
            deviceType == InputDeviceType.Keyboard ? "Keyboard" : "Mouse";
    }
    
    private static void CheckInputDevice()
    {
        var prevInputDevice = CurrentInputDeviceType;
        if (CurrentInputDeviceType == InputDeviceType.Keyboard)
        {
            if (MouseDevice.WasUsed()) CurrentInputDeviceType = InputDeviceType.Mouse;
            else
            {
                if (GamepadDeviceManager.LastUsedGamepads.Count > 0)
                {
                    CurrentInputDeviceType = InputDeviceType.Gamepad;
                }
            }
        }
        else if (CurrentInputDeviceType == InputDeviceType.Mouse)
        {
            if (KeyboardDevice.WasUsed()) CurrentInputDeviceType = InputDeviceType.Keyboard;
            else
            {
                if (GamepadDeviceManager.LastUsedGamepads.Count > 0)
                {
                    CurrentInputDeviceType = InputDeviceType.Gamepad;
                }
            }
        }
        else //gamepad
        {
            if (MouseDevice.WasUsed()) CurrentInputDeviceType = InputDeviceType.Mouse;
            else if (KeyboardDevice.WasUsed()) CurrentInputDeviceType = InputDeviceType.Keyboard;
        }

        if (CurrentInputDeviceType != prevInputDevice)
        {
            OnInputDeviceChanged?.Invoke(prevInputDevice, CurrentInputDeviceType);
        }
    }
    #endregion

}