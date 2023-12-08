using System.Numerics;
using System.Text;

namespace ShapeEngine.Input;

public class InputTypeMouseButtonAxis : IInputType
{
    private readonly ShapeMouseButton neg;
    private readonly ShapeMouseButton pos;
    private float deadzone;
    private readonly IModifierKey[] modifierKeys;
    private readonly ModifierKeyOperator modifierOperator;
    public InputTypeMouseButtonAxis(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone = 0f)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierKeys = Array.Empty<IModifierKey>();
        this.modifierOperator = ModifierKeyOperator.And;
    }
    public InputTypeMouseButtonAxis(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = modifierKeys;
    }
    public InputTypeMouseButtonAxis(ShapeMouseButton neg, ShapeMouseButton pos, float deadzone, ModifierKeyOperator modifierOperator, IModifierKey modifierKey)
    {
        this.neg = neg;
        this.pos = pos;
        this.deadzone = deadzone;
        this.modifierOperator = modifierOperator;
        this.modifierKeys = new[]{ modifierKey };
    }
    public float GetDeadzone() => deadzone;

    public void SetDeadzone(float value) => deadzone = value;

    public virtual string GetName(bool shorthand = true)
    {
        StringBuilder sb = new();
        IModifierKey.GetModifierKeyNames(sb, modifierKeys, modifierOperator, shorthand);
        string negName = ShapeMouseDevice.GetButtonName(neg, shorthand);
        string posName = ShapeMouseDevice.GetButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
        
        
    }
    public InputState GetState(ShapeGamepadDevice? gamepad = null)
    {
        // if (gamepad != null) return new();
        return ShapeInput.MouseDevice.GetState(neg, pos, deadzone, modifierOperator, modifierKeys);
    }

    public InputState GetState(InputState prev, ShapeGamepadDevice? gamepad = null)
    {
        // if (gamepad != null) return new();
        return ShapeInput.MouseDevice.GetState(neg, pos, prev, deadzone, modifierOperator, modifierKeys);
    }
    public InputDeviceType GetInputDevice() => InputDeviceType.Mouse;
    public IInputType Copy() => new InputTypeMouseButtonAxis(neg, pos);

    

}