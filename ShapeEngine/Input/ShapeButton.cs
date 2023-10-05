namespace ShapeEngine.Input;

public class ShapeButton
{
    public IShapeInputType InputType { get; private set; }
    
    
    public ShapeButton(IShapeInputType inputType)
    {
        this.InputType = inputType;
    }
    public ShapeButton(ShapeKeyboardButtonInput keyboardButton)
    {
        InputType = keyboardButton;
    }
    public ShapeButton(ShapeMouseButtonInput mouseButton)
    {
        InputType = mouseButton;
    }
    public ShapeButton(ShapeGamepadButtonInput gamepadButton)
    {
        InputType = gamepadButton;
        //GamepadIndex = gamepadIndex;
    }
    public ShapeButton(ShapeKeyboardButton button)
    {
        InputType = new ShapeKeyboardButtonInput(button);
    }
    public ShapeButton(ShapeMouseButton button)
    {
        InputType = new ShapeMouseButtonInput(button);
    }
    public ShapeButton(ShapeGamepadButton button, float deadzone = 0.2f)
    {
        InputType = new ShapeGamepadButtonInput(button, deadzone);
        //GamepadIndex = gamepadIndex;
    }

    public ShapeButton(ShapeKeyboardButtonAxisInput keyboardButtonAxisInput)
    {
        InputType = keyboardButtonAxisInput;
    }
    public ShapeButton(ShapeMouseButtonAxisInput mouseButtonAxisInput)
    {
        InputType = mouseButtonAxisInput;
    }
    public ShapeButton(ShapeGamepadButtonAxisInput gamepadButtonAxisInput)
    {
        InputType = gamepadButtonAxisInput;
        //GamepadIndex = gamepadIndex;
    }
    public ShapeButton(ShapeKeyboardButton neg, ShapeKeyboardButton pos)
    {
        InputType = new ShapeKeyboardButtonAxisInput(neg, pos);
    }
    public ShapeButton(ShapeMouseButton neg, ShapeMouseButton pos)
    {
        InputType = new ShapeMouseButtonAxisInput(neg, pos);
    }
    public ShapeButton(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.2f)
    {
        InputType = new ShapeGamepadButtonAxisInput(neg, pos, deadzone);
        //GamepadIndex = gamepadIndex;
    }
    public ShapeButton(ShapeMouseWheelAxisInput mouseWheelAxisInput)
    {
        InputType = mouseWheelAxisInput;
    }
    public ShapeButton(ShapeGamepadAxisInput gamepadAxisInput)
    {
        InputType = gamepadAxisInput;
        //GamepadIndex = gamepadIndex;
    }
    public ShapeButton(ShapeMouseWheelAxis mouseWheelAxis)
    {
        InputType = new ShapeMouseWheelAxisInput(mouseWheelAxis);
    }
    public ShapeButton(ShapeGamepadAxis gamepadAxis, float deadzone = 0.2f)
    {
        InputType = new ShapeGamepadAxisInput(gamepadAxis, deadzone);
        //GamepadIndex = gamepadIndex;
    }
    private ShapeButton(ShapeButton button)
    {
        InputType = button.InputType.Copy();
    }

   
    
    public ShapeButton Copy() => new ShapeButton(this);
    public void Update(float dt, int gamepadIndex) => InputType.Update(dt, gamepadIndex);
}