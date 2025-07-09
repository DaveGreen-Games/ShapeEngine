namespace ShapeEngine.Input;

public readonly struct InputDeviceUsageDetectionSettings
{
    public readonly struct MouseSettings
    {
        public static readonly MouseSettings Default = new();
        public MouseSettings()
        {
            MouseDetection = true;
            MoveThreshold = 0.5f;
            MouseWheelThreshold = 0.25f;
            MouseMinPressCount = 2;
            MouseMinPressInterval = 1f;
            MouseMinUsedDuration = 0.5f;
        }
        
        /// <summary>
        /// Gets a value indicating whether press count detection is enabled for the mouse.
        /// Detection is enabled when <c>MouseMinPressCount</c> is greater than 1 and <c>MouseMinPressInterval</c> is greater than 0.
        /// </summary>
        public bool PressCountEnabled => MouseMinPressCount > 1 && MouseMinPressInterval > 0f;
        
        /// <summary>
        /// Indicates whether used duration detection is enabled (requires <c>MouseMinUsedDuration</c> to be greater than zero).
        /// </summary>
        public bool UsedDurationEnabled => MouseMinUsedDuration > 0f;
        
        /// <summary>
        /// Indicates whether the mouse move threshold is enabled (greater than 0).
        /// </summary>
        public bool IsMouseMoveThresholdEnabled => MoveThreshold > 0f;
        
        /// <summary>
        /// Indicates whether the mouse wheel threshold is enabled (greater than 0).
        /// </summary>
        public bool IsMouseWheelThresholdEnabled => MouseWheelThreshold > 0f;
        
        /// <summary>
        /// Indicates whether the input device can be changed to mouse.
        /// </summary>
        /// <remarks>
        /// <c>Default is true</c>
        /// </remarks>
        public readonly bool MouseDetection;
        /// <summary>
        /// The minimum movement threshold to consider the mouse as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5f.</c>
        /// </remarks>
        public readonly float MoveThreshold;
        /// <summary>
        /// The minimum mouse wheel movement threshold to consider the mouse as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.25f.</c>
        /// </remarks>
        public readonly float MouseWheelThreshold;

        /// <summary>
        /// The minimum number of mouse button presses required, within the specified interval, 
        /// to consider the mouse device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 2 presses within 1 second.</c>
        /// </remarks>
        public readonly int MouseMinPressCount;
    
        /// <summary>
        /// The time interval (in seconds) from the first mouse button press within which 
        /// <c>MouseMinPressCount</c> must be reached for the mouse device to be considered "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 1 second.</c>
        /// </remarks>
        public readonly float MouseMinPressInterval;
    
        /// <summary>
        /// The minimum duration (in seconds) of consecutive mouse use (movement or holding at least one button down)
        /// required to consider the mouse device as "used".
        /// </summary>
        /// <remarks>
        /// <c>Default is 0.5 seconds</c>
        /// </remarks>
        public readonly float MouseMinUsedDuration;
    }
    public readonly struct KeyboardSettings
    {
        public static readonly KeyboardSettings Default = new();
    }
    public readonly struct GamepadSettings
    {
        public static readonly GamepadSettings Default = new();
    }
    
    
    public static readonly InputDeviceUsageDetectionSettings Default = new();

    public readonly MouseSettings Mouse;
    public readonly KeyboardSettings Keyboard;
    public readonly GamepadSettings Gamepad;
    
    public InputDeviceUsageDetectionSettings()
    {
        Mouse = MouseSettings.Default;
        Keyboard = KeyboardSettings.Default;
        Gamepad = GamepadSettings.Default;
    }
    
    public InputDeviceUsageDetectionSettings(MouseSettings mouse, KeyboardSettings keyboard, GamepadSettings gamepad)
    {
        Mouse = mouse;
        Keyboard = keyboard;
        Gamepad = gamepad;
    }
    
}