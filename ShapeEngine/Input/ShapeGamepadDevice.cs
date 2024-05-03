using System.Text;
using Raylib_cs;

namespace ShapeEngine.Input;

public sealed class ShapeGamepadDevice : ShapeInputDevice
{
    private readonly struct AxisRange
    {
        public readonly float Minimum;
        public readonly float Maximum;

        public float TotalRange => MathF.Abs(Maximum - Minimum);
        public AxisRange(float minimum, float maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public AxisRange UpdateRange(float newValue)
        {
            if (newValue < Minimum) return new(newValue, Maximum);
            if (newValue > Maximum) return new(Minimum, newValue);
            return new(Minimum, Maximum);
        }
    }
    
    
    public static readonly GamepadButton[] AllGamepadButtons = Enum.GetValues<GamepadButton>();
    public static readonly GamepadAxis[] AllGamepadAxis = Enum.GetValues<GamepadAxis>();
    
    public static readonly ShapeGamepadButton[] AllShapeGamepadButtons = Enum.GetValues<ShapeGamepadButton>();
    public static readonly ShapeGamepadAxis[] AllShapeGamepadAxis = Enum.GetValues<ShapeGamepadAxis>();
    
    
    private readonly Dictionary<ShapeGamepadButton, InputState> buttonStates = new (AllShapeGamepadButtons.Length);
    private readonly Dictionary<ShapeGamepadAxis, InputState> axisStates = new (AllShapeGamepadAxis.Length);
    

    public readonly int Index;
    
    public bool Available { get; private set; } = true;
    public bool Connected { get; private set; }

    public string Name { get; private set; } = "No Device";
    public int AxisCount { get; private set; } = 0;

    private bool isLocked = false;
    private bool wasUsed  = false;
    
    public readonly List<ShapeGamepadButton> UsedButtons = new();
    public readonly List<ShapeGamepadAxis> UsedAxis = new();
    
    public event Action? OnConnectionChanged;
    public event Action? OnAvailabilityChanged;

    // private bool triggerFix = false;
    private readonly Dictionary<ShapeGamepadAxis, float> axisZeroCalibration = new();
    // public Func<float, float>? AxisValueCalibrator = null; 
    private Dictionary<ShapeGamepadAxis, AxisRange> axisRanges = new();
    
    public ShapeGamepadDevice(int index, bool connected)
    {
        Index = index;
        
        Connected = connected;
        if (Connected)
        {
            unsafe
            {
                Name = Raylib.GetGamepadName(index)->ToString();
            }

            AxisCount = Raylib.GetGamepadAxisCount(index);
            
            Calibrate();
        }
        
        foreach (var button in AllShapeGamepadButtons)
        {
            buttonStates.Add(button, new());
        }
        foreach (var axis in AllShapeGamepadAxis)
        {
            axisStates.Add(axis, new());
        }
        
    }

    public InputState GetButtonState(ShapeGamepadButton button) => buttonStates[button];
    public InputState GetAxisState(ShapeGamepadAxis axis) => axisStates[axis];
    
    public bool WasUsed() => wasUsed;
    public bool IsLocked() => isLocked;

    public void Lock()
    {
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }
    public void Update()
    {
        UpdateButtonStates();
        UpdateAxisStates();
        
        wasUsed = false;
        if(UsedButtons.Count > 0) UsedButtons.Clear();
        if(UsedAxis.Count > 0) UsedAxis.Clear();
        
        if (!Connected || isLocked) return;
        
        var usedButtons = GetUsedGamepadButtons();
        var usedAxis = GetUsedGamepadAxis(0.25f);
        wasUsed = usedButtons.Count > 0 || usedAxis.Count > 0;
        if(usedButtons.Count > 0) UsedButtons.AddRange(usedButtons);
        if(usedAxis.Count > 0) UsedAxis.AddRange(usedAxis);
    }
    
    public void Connect()
    {
        if (Connected) return;
        Connected = true;
        unsafe
        {
            Name = Raylib.GetGamepadName(Index)->ToString();
        }

        AxisCount = Raylib.GetGamepadAxisCount(Index);
        OnConnectionChanged?.Invoke();
        
        axisRanges.Clear();
        
        Calibrate();
    }
    public void Disconnect()
    {
        if (!Connected) return;
        Connected = false;
        OnConnectionChanged?.Invoke();
    }
    public bool Claim()
    {
        if (!Connected || !Available) return false;
        Available = false;
        OnAvailabilityChanged?.Invoke();
        return true;
    }
    public bool Free()
    {
        if (Available) return false;
        Available = true;
        OnAvailabilityChanged?.Invoke();
        return true;
    }

    public void Calibrate()
    {
        float leftX = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.LeftX);
        float leftY = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.LeftY);
        
        float rightX = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.RightX);
        float rightY = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.RightY);
        
        float triggerRight = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.LeftTrigger);
        float triggerLeft = Raylib.GetGamepadAxisMovement(Index, GamepadAxis.RightTrigger);

        axisZeroCalibration[ShapeGamepadAxis.LEFT_X] = leftX;
        axisZeroCalibration[ShapeGamepadAxis.LEFT_Y] = leftY;
        
        axisZeroCalibration[ShapeGamepadAxis.RIGHT_X] = rightX;
        axisZeroCalibration[ShapeGamepadAxis.RIGHT_Y] = rightY;
        
        axisZeroCalibration[ShapeGamepadAxis.RIGHT_TRIGGER] = triggerRight;
        axisZeroCalibration[ShapeGamepadAxis.LEFT_TRIGGER] = triggerLeft;

    }
    
    private void UpdateButtonStates()
    {
        foreach (var state in buttonStates)
        {
            var button = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(button);
            buttonStates[button] = new InputState(prevState, curState);

        }
    }

    private void UpdateAxisStates()
    {
        foreach (var state in axisStates)
        {
            var button = state.Key;
            var prevState = state.Value;
            var curState = CreateInputState(button);
            axisStates[button] = new InputState(prevState, curState);

        }
    }

    #region Button

    public bool IsModifierActive(ShapeGamepadButton modifierKey, bool reverseModifier)
    {
        return IsDown(modifierKey) != reverseModifier;
    }
    public bool IsDown(ShapeGamepadButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(button, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    public bool IsDown(ShapeGamepadButton button, float deadzone = 0.1f)
    {
        return GetValue(button, deadzone) != 0f;
    }

    public float GetValue(ShapeGamepadButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, this)) return 0f;
        return GetValue(button, deadzone);
    }
    public float GetValue(ShapeGamepadButton button, float deadzone = 0.1f)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        
        var id = (int)button;
        if (id >= 30 && id <= 33)
        {
            id -= 30;
            float value = GetValue((ShapeGamepadAxis)id, deadzone);// GetGamepadAxisMovement(Index, id);
            if (MathF.Abs(value) < deadzone) value = 0f;
            return value;
        }
        
        if (id >= 40 && id <= 43)
        {
            id -= 40;
            float value = GetValue((ShapeGamepadAxis)id, deadzone);// GetGamepadAxisMovement(Index, id);
            if (MathF.Abs(value) < deadzone) value = 0f;
            return value;
        }
        
        return Raylib.IsGamepadButtonDown(Index, (GamepadButton)id) ? 1f : 0f;
    }
    
    public InputState CreateInputState(ShapeGamepadButton button, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        bool down = IsDown(button, deadzone, modifierOperator, modifierKeys);
        return new(down, !down, down ? 1f : 0f, Index, InputDeviceType.Gamepad);
    }
    public InputState CreateInputState(ShapeGamepadButton button, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, CreateInputState(button, deadzone, modifierOperator, modifierKeys));
    }
    public InputState CreateInputState(ShapeGamepadButton button, float deadzone = 0.1f)
    {
        bool down = IsDown(button, deadzone);
        return new(down, !down, down ? 1f : 0f, Index, InputDeviceType.Gamepad);
    }
    public InputState CreateInputState(ShapeGamepadButton button, InputState previousState, float deadzone = 0.1f)
    {
        return new(previousState, CreateInputState(button, deadzone));
    }
    public static string GetButtonName(ShapeGamepadButton button, bool shortHand = true)
    {
        switch (button)
        {
            case ShapeGamepadButton.UNKNOWN: return shortHand ? "Unknown" : "GP Button Unknown";
            case ShapeGamepadButton.LEFT_FACE_UP: return shortHand ? "Up" : "GP Button Up";
            case ShapeGamepadButton.LEFT_FACE_RIGHT: return shortHand ? "Right" : "GP Button Right";
            case ShapeGamepadButton.LEFT_FACE_DOWN: return shortHand ? "Down" : "GP Button Down";
            case ShapeGamepadButton.LEFT_FACE_LEFT: return shortHand ? "Left" : "GP Button Left";
            case ShapeGamepadButton.RIGHT_FACE_UP: return shortHand ? "Y" : "GP Button Y";
            case ShapeGamepadButton.RIGHT_FACE_RIGHT: return shortHand ? "B" : "GP Button B";
            case ShapeGamepadButton.RIGHT_FACE_DOWN: return shortHand ? "A" : "GP Button A";
            case ShapeGamepadButton.RIGHT_FACE_LEFT: return shortHand ? "X" : "GP Button X";
            case ShapeGamepadButton.LEFT_TRIGGER_TOP: return shortHand ? "LB" : "GP Button Left Bumper";
            case ShapeGamepadButton.LEFT_TRIGGER_BOTTOM: return shortHand ? "LT" : "GP Button Left Trigger";
            case ShapeGamepadButton.RIGHT_TRIGGER_TOP: return shortHand ? "RB" : "GP Button Right Bumper";
            case ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM: return shortHand ? "RT" : "GP Button Right Trigger";
            case ShapeGamepadButton.MIDDLE_LEFT: return shortHand ? "Select" : "GP Button Select";
            case ShapeGamepadButton.MIDDLE: return shortHand ? "Home" : "GP Button Home";
            case ShapeGamepadButton.MIDDLE_RIGHT: return shortHand ? "Start" : "GP Button Start";
            case ShapeGamepadButton.LEFT_THUMB: return shortHand ? "LClick" : "GP Button Left Stick Click";
            case ShapeGamepadButton.RIGHT_THUMB: return shortHand ? "RClick" : "GP Button Right Stick Click";
            case ShapeGamepadButton.LEFT_STICK_RIGHT: return shortHand ? "LS R" : "Left Stick Right";
            case ShapeGamepadButton.LEFT_STICK_LEFT: return shortHand ? "LS L" : "Left Stick Left";
            case ShapeGamepadButton.LEFT_STICK_DOWN: return shortHand ? "LS D" : "Left Stick Down";
            case ShapeGamepadButton.LEFT_STICK_UP: return shortHand ? "LS U" : "Left Stick Up";
            case ShapeGamepadButton.RIGHT_STICK_RIGHT: return shortHand ? "RS R" : "Right Stick Right";
            case ShapeGamepadButton.RIGHT_STICK_LEFT: return shortHand ? "RS L" : "Right Stick Left";
            case ShapeGamepadButton.RIGHT_STICK_DOWN: return shortHand ? "RS D" : "Right Stick Down";
            case ShapeGamepadButton.RIGHT_STICK_UP: return shortHand ? "RS U" : "Right Stick Up";
            default: return "No Key";
        }
    }


    #endregion
    
    #region Axis
    public bool IsDown(ShapeGamepadAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(axis, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    public bool IsDown(ShapeGamepadAxis axis, float deadzone)
    {
        return GetValue(axis, deadzone) != 0f;
    }

    public float GetValue(ShapeGamepadAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, this)) return 0f;

        return GetValue(axis, deadzone);
    }
    public float GetValue(ShapeGamepadAxis axis, float deadzone)
    {
        // if (Index < 0 || isLocked || !Connected) return 0f;
        // float value = GetGamepadAxisMovement(Index, (int)axis);
        // if (axis is ShapeGamepadAxis.LEFT_TRIGGER or ShapeGamepadAxis.RIGHT_TRIGGER)
        // {
        //     value = (value + 1f) / 2f;
        // }
        // return MathF.Abs(value) < deadzone ? 0f : value;
        
        if (!Connected || Index < 0 || isLocked) return 0f;
        var value = GetValue(axis);
        return MathF.Abs(value) < deadzone ? 0f : value;
    }
    public float GetValue(ShapeGamepadAxis axis)
    {
        if (!Connected || Index < 0 || isLocked) return 0f;
        
        float value = Raylib.GetGamepadAxisMovement(Index, (GamepadAxis)axis);
        // if(axis == ShapeGamepadAxis.RIGHT_TRIGGER) Console.WriteLine($"Axis Movement {value} | Calibration Value: {axisCalibrationValues[axis]} | Final value: {value - axisCalibrationValues[axis]}");

        var calibrationValue = axisZeroCalibration[axis];
        value -= calibrationValue;
        value = CalibrateAxis(value, axis);
        
        // if (calibrationValue != 0f)
        // {
        //     value -= axisZeroCalibration[axis];
        //     if (axis == ShapeGamepadAxis.LEFT_TRIGGER || axis == ShapeGamepadAxis.RIGHT_TRIGGER) value /= 2f;
        // }
        
        return value;
        
        
    }

    private float CalibrateAxis(float value, ShapeGamepadAxis axis)
    {
        if (axisRanges.TryGetValue(axis, out var range))
        {
            axisRanges[axis] = range.UpdateRange(value);
        }
        else
        {
            axisRanges[axis] = new(value, value);
        }
        
        if (axis is ShapeGamepadAxis.LEFT_TRIGGER or ShapeGamepadAxis.RIGHT_TRIGGER)
        {
            var axisRange = axisRanges[axis];
            if (axisRange.TotalRange > 1)
            {
                value /= axisRange.TotalRange;
            }
            
        }
        
        return value;
    }
    public InputState CreateInputState(ShapeGamepadAxis axis, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axisValue = GetValue(axis, deadzone, modifierOperator, modifierKeys);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, Index, InputDeviceType.Gamepad);
    }
    public InputState CreateInputState(ShapeGamepadAxis axis, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, CreateInputState(axis, deadzone, modifierOperator, modifierKeys));
    }
    public InputState CreateInputState(ShapeGamepadAxis axis, float deadzone = 0.1f)
    {
        float axisValue = GetValue(axis, deadzone);
        bool down = axisValue != 0f;
        return new(down, !down, axisValue, Index, InputDeviceType.Gamepad);
    }
    public InputState CreateInputState(ShapeGamepadAxis axis, InputState previousState, float deadzone = 0.1f)
    {
        return new(previousState, CreateInputState(axis, deadzone));
    }
    public static string GetAxisName(ShapeGamepadAxis axis, bool shortHand = true)
    {
        switch (axis)
        {
            case ShapeGamepadAxis.LEFT_X: return shortHand ? "LSx" : "GP Axis Left X";
            case ShapeGamepadAxis.LEFT_Y: return shortHand ? "LSy" : "GP Axis Left Y";
            case ShapeGamepadAxis.RIGHT_X: return shortHand ? "RSx" : "GP Axis Right X";
            case ShapeGamepadAxis.RIGHT_Y: return shortHand ? "RSy" : "GP Axis Right Y";
            case ShapeGamepadAxis.RIGHT_TRIGGER: return shortHand ? "RT" : "GP Axis Right Trigger";
            case ShapeGamepadAxis.LEFT_TRIGGER: return shortHand ? "LT" : "GP Axis Left Trigger";
            default: return "No Key";
        }
    }


    #endregion

    #region Button Axis
    public static string GetButtonAxisName(ShapeGamepadButton neg, ShapeGamepadButton pos, bool shorthand = true)
    {
        StringBuilder sb = new();
        
        string negName = GetButtonName(neg, shorthand);
        string posName = GetButtonName(pos, shorthand);
        sb.Append(negName);
        sb.Append('|');
        sb.Append(posName);
        return sb.ToString();
    }
    
    public bool IsDown(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return GetValue(neg, pos, deadzone, modifierOperator, modifierKeys) != 0f;
    }
    public bool IsDown(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.1f)
    {
        return GetValue(neg, pos, deadzone) != 0f;
    }

    public float GetValue(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        if (!IModifierKey.IsActive(modifierOperator, modifierKeys, this)) return 0f;
        return GetValue(neg, pos, deadzone);
    }
    public float GetValue(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.1f)
    {
        if (Index < 0 || isLocked || !Connected) return 0f;
        float vNegative = GetValue(neg, deadzone);
        float vPositive = GetValue(pos, deadzone);
        return vPositive - vNegative;
    }
    
    public InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        float axis = GetValue(neg, pos, deadzone, modifierOperator, modifierKeys);
        bool down = axis != 0f;
        return new(down, !down, axis, Index, InputDeviceType.Gamepad);
    }
    public InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, InputState previousState, float deadzone, ModifierKeyOperator modifierOperator, params IModifierKey[] modifierKeys)
    {
        return new(previousState, CreateInputState(neg, pos, deadzone, modifierOperator, modifierKeys));
    }
    public InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, float deadzone = 0.1f)
    {
        float axis = GetValue(neg, pos, deadzone);
        bool down = axis != 0f;
        return new(down, !down, axis, Index, InputDeviceType.Gamepad);
    }
    public InputState CreateInputState(ShapeGamepadButton neg, ShapeGamepadButton pos, InputState previousState, float deadzone = 0.2f)
    {
        return new(previousState, CreateInputState(neg, pos, deadzone));
    }

    #endregion

    #region Used

    // public bool WasGamepadUsed(float deadzone = 0f)
    // {
    //     if (!Connected || Index < 0) return false;
    //     return WasGamepadButtonUsed() || WasGamepadAxisUsed(deadzone);
    // }
    private bool WasGamepadButtonUsed()
    {
        if (!Connected || Index < 0 || isLocked) return false;
        foreach (var b in  AllGamepadButtons)
        {
            if (Raylib.IsGamepadButtonDown(Index, b)) return true;
        }

        return false;
    }
    private bool WasGamepadAxisUsed(ShapeGamepadAxis axis, float deadzone = 0.25f)
    {
        if (!Connected || Index < 0 || isLocked) return false;
        return GetValue(axis, deadzone) != 0f;
    }
    private bool WasGamepadAxisUsed(float deadzone = 0.25f)
    {
        if (!Connected || Index < 0 || isLocked) return false;
        
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_X, deadzone)) return true;
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_Y, deadzone)) return true;
        
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_X, deadzone)) return true;
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_Y, deadzone)) return true;
        
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_TRIGGER, deadzone)) return true;
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_TRIGGER, deadzone)) return true;
        // if (MathF.Abs(Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X)) > deadzone) return true;
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y)) > deadzone) return true;
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X)) > deadzone) return true;
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y)) > deadzone) return true;
        // if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1f) / 2f > deadzone / 2) return true;
        // if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1f) / 2f > deadzone / 2) return true;

        return false;
    }
    
    private List<ShapeGamepadButton> GetUsedGamepadButtons(float deadzone = 0.25f)
    {
        var usedButtons = new List<ShapeGamepadButton>();
        if (!Connected || Index < 0 || isLocked) return usedButtons;
        var values = ShapeGamepadDevice.AllShapeGamepadButtons;// Enum.GetValues<ShapeGamepadButton>();
        foreach (var b in  values)
        {
            if (IsDown(b, deadzone))
            {
                usedButtons.Add(b);
            }
        }
        return usedButtons;
    }
    private List<ShapeGamepadAxis> GetUsedGamepadAxis(float deadzone = 0.25f)
    {
        var usedAxis = new List<ShapeGamepadAxis>();
        if (!Connected || Index < 0 || isLocked) return usedAxis;
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_X, deadzone)) usedAxis.Add(ShapeGamepadAxis.LEFT_X);
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_Y, deadzone)) usedAxis.Add(ShapeGamepadAxis.LEFT_Y);
        
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_X, deadzone)) usedAxis.Add(ShapeGamepadAxis.RIGHT_X);
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_Y, deadzone)) usedAxis.Add(ShapeGamepadAxis.RIGHT_Y);
        
        if (WasGamepadAxisUsed(ShapeGamepadAxis.LEFT_TRIGGER, deadzone)) usedAxis.Add(ShapeGamepadAxis.LEFT_TRIGGER);
        if (WasGamepadAxisUsed(ShapeGamepadAxis.RIGHT_TRIGGER, deadzone)) usedAxis.Add(ShapeGamepadAxis.RIGHT_TRIGGER);
        
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X)) > deadzone) usedAxis.Add(ShapeGamepadAxis.LEFT_X);
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y)) > deadzone) usedAxis.Add(ShapeGamepadAxis.LEFT_Y);
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X)) > deadzone) usedAxis.Add(ShapeGamepadAxis.RIGHT_X);
        // if (MathF.Abs( Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y)) > deadzone) usedAxis.Add(ShapeGamepadAxis.RIGHT_Y);
        // if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1f) / 2f > deadzone / 2) usedAxis.Add(ShapeGamepadAxis.LEFT_TRIGGER);
        // if ((Raylib.GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1f) / 2f > deadzone / 2) usedAxis.Add(ShapeGamepadAxis.RIGHT_TRIGGER);

        return usedAxis;
    }
    #endregion
    
}