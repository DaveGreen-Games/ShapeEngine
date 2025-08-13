using ShapeEngine.Core.Structs;

namespace ShapeEngine.Input;


/// <summary>
/// Represents the main input system for managing input devices, input actions, and access control.
/// Handles device updates, action trees, and provides locking mechanisms for input processing.
/// </summary>
public sealed class InputSystem
{
    #region Members and Properties
    /// <summary>
    /// A global collection of <see cref="InputActionTree"/> instances.
    /// Each tree manages a hierarchy of input actions and their bindings.
    /// </summary>
    public readonly InputActionTreeGroup ActiveInputActionTreeGroup = [];
    /// <summary>
    /// Gets the current input settings.
    /// </summary>
    public InputSettings InputSettings { get; private set; }
    /// <summary>
    /// Gets the current input device type in use.
    /// </summary>
    public InputDeviceType CurrentInputDeviceType { get; private set; }

    /// <summary>
    /// Gets the current input device type, but returns Keyboard if Mouse is active.
    /// </summary>
    public InputDeviceType CurrentInputDeviceTypeNoMouse => CurrentInputDeviceType.FilterInputDevice(InputDeviceType.Mouse, InputDeviceType.Keyboard);
    
    /// <summary>
    /// Event triggered when the input device type changes.
    /// </summary>
    public event Action<InputDeviceType, InputDeviceType>? OnInputDeviceChanged;
    
    
    /// <summary>
    /// Current <see cref="InputAction"/> based device type detected by <see cref="ActiveInputActionTreeGroup"/>.
    /// The first non-None <see cref="InputDeviceType"/> detected during the update of the <see cref="ActiveInputActionTreeGroup"/> is used.
    /// </summary>
    /// <remarks>
    /// For an <see cref="InputAction"/>s <see cref="IInputType"/>s <see cref="InputDeviceType"/> to be considered the following conditions must be met:
    /// <list type="bullet">
    /// <item>The <see cref="InputAction"/> must be part of an <see cref="InputActionTree"/> within the <see cref="ActiveInputActionTreeGroup"/>.</item>
    /// <item>The <see cref="InputAction"/> must be active.</item>
    /// <item>The <see cref="IInputType"/>'s <see cref="InputState"/> must be down.</item>
    /// <item>The <see cref="IInputType"/> must not be blocked (when <see cref="InputAction.BlocksInput"/> is <c>true</c>).</item>
    /// </list>
    /// </remarks>
    public InputDeviceType CurrentInputActionDeviceType { get; private set; }
    
    /// <summary>
    /// Event triggered when <see cref="CurrentInputActionDeviceType"/> changes.
    /// </summary>
    /// <remarks>
    /// The event provides the previous and new input action device types as parameters.
    /// </remarks>
    public event Action<InputDeviceType, InputDeviceType>? OnInputActionDeviceTypeChanged;
    
    /// <summary>
    /// The global keyboard device instance.
    /// </summary>
    public KeyboardDevice Keyboard { get; private set; }
    
    /// <summary>
    /// The global mouse device instance.
    /// </summary>
    public MouseDevice Mouse { get; private set; }
    
    /// <summary>
    /// The global gamepad device manager instance.
    /// </summary>
    public GamepadDeviceManager GamepadManager { get; private set; }
    
    /// <summary>
    /// Indicates whether the input device selection cooldown is currently active.
    /// Returns true if the cooldown timer is greater than zero.
    /// </summary>
    public bool InputDeviceSelectionCooldownActive => inputDeviceSelectionCooldownTimer > 0f;
    private float inputDeviceSelectionCooldownTimer;
    
    /// <summary>
    /// Indicates whether the input action device selection cooldown is currently active.
    /// Returns true if the cooldown timer is greater than zero.
    /// </summary>
    public bool InputActionDeviceSelectionCooldownActive => inputActionDeviceSelectionCooldownTimer > 0f;
    private float inputActionDeviceSelectionCooldownTimer;
    private readonly SortedSet<InputDevice> sortedInputDevices = [];
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InputSystem"/> class with the specified input settings.
    /// </summary>
    /// <param name="settings">The input settings to use for initialization.</param>
    internal InputSystem(InputSettings settings)
    {
        CurrentInputDeviceType = InputDeviceType.Keyboard;
        CurrentInputActionDeviceType = InputDeviceType.None;
        
        InputSettings = settings;
        
        Keyboard = new(settings.Keyboard);
        GamepadManager = new(settings.MaxGamepadCount, settings.Gamepad);
        Mouse = new(settings.Mouse);
    }
    #endregion
    
    #region Input Device Handling
    /// <summary>
    /// Applies the <see cref="InputSettings"/> to all attached input devices.
    /// </summary>
    /// <param name="settings">The new <see cref="InputSettings"/> to apply.</param>
    /// <remarks>
    /// Settings are applied to <see cref="Mouse"/>,
    /// <see cref="Keyboard"/>,
    /// and <see cref="GamepadManager"/> that applies the settings to all <see cref="GamepadDevice"/>s.
    /// </remarks>
    public void ApplyInputDeviceChangeSettings(InputSettings settings)
    {
        InputSettings = settings;
        Mouse.ApplyInputDeviceChangeSettings(settings);
        Keyboard.ApplyInputDeviceChangeSettings(settings);
        GamepadManager.ApplyInputDeviceChangeSettings(settings);
    }
    
    /// <summary>
    /// Updates all input devices and checks for input device changes.
    /// </summary>
    internal void Update(float dt)
    {
        if (InputDeviceSelectionCooldownActive)
        {
            inputDeviceSelectionCooldownTimer -= dt;
            if (inputDeviceSelectionCooldownTimer <= 0f)
            {
                inputDeviceSelectionCooldownTimer = 0f;
            }
        }
        
        if (InputActionDeviceSelectionCooldownActive)
        {
            inputActionDeviceSelectionCooldownTimer -= dt;
            if (inputActionDeviceSelectionCooldownTimer <= 0f)
            {
                inputActionDeviceSelectionCooldownTimer = 0f;
            }
        }

        GamepadManager.ClearUsedGamepads();
        // Re-add active devices to the set each frame to reflect any changes in device instances or their priority.
        sortedInputDevices.Clear(); 
        sortedInputDevices.Add(Keyboard);
        sortedInputDevices.Add(Mouse);
        foreach (var gamepad in GamepadManager.GetConnectedGamepads())
        {
            sortedInputDevices.Add(gamepad);
        }
        
        var usedInputDevice = InputDeviceType.None;
        var wasOtherDeviceUsed = false;
        var deviceTypeLocked = false;
        foreach (var inputDevice in sortedInputDevices)
        {
            var deviceType = inputDevice.GetDeviceType();
            if (deviceType == CurrentInputDeviceType)
            {
                if (inputDevice.Update(dt, false))
                {
                    if(inputDevice is GamepadDevice gamepad) GamepadManager.GamepadWasUsed(gamepad);
                    usedInputDevice = CurrentInputDeviceType;
                    deviceTypeLocked = true;
                    wasOtherDeviceUsed = true;
                }
            }
            else
            {
                var prevUsed = wasOtherDeviceUsed;
                var deviceUsed = inputDevice.Update(dt, wasOtherDeviceUsed);
                if (deviceUsed)
                {
                    if (inputDevice is GamepadDevice gamepad) GamepadManager.GamepadWasUsed(gamepad);
                }
                
                if(deviceUsed) wasOtherDeviceUsed = deviceUsed;
                
                if(!deviceTypeLocked && wasOtherDeviceUsed && !prevUsed) usedInputDevice = inputDevice.GetDeviceType();
            }
            
        }
        
        if (usedInputDevice != InputDeviceType.None && usedInputDevice != CurrentInputDeviceType)
        {
            if(!InputDeviceSelectionCooldownActive)
            {
                var prevInputDevice = CurrentInputDeviceType;
                CurrentInputDeviceType = usedInputDevice;
                OnInputDeviceChanged?.Invoke(prevInputDevice, CurrentInputDeviceType);
                
                float deviceCooldown;
                
                if (usedInputDevice == InputDeviceType.Keyboard) deviceCooldown = InputSettings.Keyboard.SelectionCooldownDuration;
                else if (usedInputDevice == InputDeviceType.Gamepad) deviceCooldown = InputSettings.Gamepad.SelectionCooldownDuration;
                else deviceCooldown = InputSettings.Mouse.SelectionCooldownDuration;
                
                if (deviceCooldown > 0f)
                {
                    inputDeviceSelectionCooldownTimer = deviceCooldown;
                }
            }
        }
        
        var usedInputActionDevice = ActiveInputActionTreeGroup.Update(dt);
        if (usedInputActionDevice != InputDeviceType.None)
        {
            if(!InputActionDeviceSelectionCooldownActive && usedInputActionDevice != CurrentInputActionDeviceType)
            {
                var prevInputActionDevice = CurrentInputActionDeviceType;
                CurrentInputActionDeviceType = usedInputActionDevice;
                OnInputActionDeviceTypeChanged?.Invoke(prevInputActionDevice, CurrentInputActionDeviceType);
                
                float deviceCooldown;
                
                if (usedInputActionDevice == InputDeviceType.Keyboard) deviceCooldown = InputSettings.Keyboard.SelectionCooldownDuration;
                else if (usedInputActionDevice == InputDeviceType.Gamepad) deviceCooldown = InputSettings.Gamepad.SelectionCooldownDuration;
                else deviceCooldown = InputSettings.Mouse.SelectionCooldownDuration;
                
                if (deviceCooldown > 0f)
                {
                    inputActionDeviceSelectionCooldownTimer = deviceCooldown;
                }
            }
        }

        
    }

    internal void EndFrame()
    {
        // InputAction.ClearInputTypeBlocklist();
    }
    #endregion
    
    #region Lock System
    
    /// <summary>
    /// This access tag grants access regardless of the input system lock.
    /// </summary>
    public const uint AllAccessTag = 1; // 2^0 (2 to the power of 0)

    /// <summary>
    /// The default access tag for actions.
    /// </summary>
    public const uint DefaultAccessTag = 2; // 2^1 (2 to the power of 1)

    /// <summary>
    /// Indicates if the input system is currently locked.
    /// When set to <c>true</c>, <see cref="InputAction"/>s and other input requests will only be processed
    /// if they have the <see cref="AllAccessTag"/> or their access tag is contained in the lock whitelist.
    /// All <see cref="InputAction"/>s and input request with an access tag contained in the lock blacklist will not be processed.
    /// </summary>
    public static bool Locked { get; private set; }

    private static BitFlag lockWhitelist;
    private static BitFlag lockBlacklist;
    private static int tagPowerCounter = 2; //0 and 1 are reserved for AllAccessTag and DefaultAccessTag respectively.

    /// <summary>
    /// Gets the next available access tag.
    /// <c>1</c> is reserved for <see cref="AllAccessTag"/>, <c>2</c> is reserved for <see cref="DefaultAccessTag"/>.
    /// Only power of 2 values are used for access tags.
    /// </summary>
    public static uint NextAccessTag => BitFlag.GetPowerOfTwo(tagPowerCounter++);
    
    /// <summary>
    /// Locks the input system, clearing all whitelists and blacklists.
    /// <remarks>
    /// Only <see cref="InputAction"/>s and input requests with <see cref="AllAccessTag"/> will be processed.
    /// </remarks>
    /// </summary>
    public static void Lock()
    {
        Locked = true;
        lockWhitelist = BitFlag.Empty;
        lockBlacklist = BitFlag.Empty;
    }

    /// <summary>
    /// Locks the input system with a specific whitelist and blacklist.
    /// <remarks>
    /// All <see cref="InputAction"/>s and other input requests with a tag contained in the whitelist or with the <see cref="AllAccessTag"/> will be processed.
    /// All <see cref="InputAction"/>s and other input requests with a tag contained in the blacklist will not be processed.
    /// </remarks>
    /// </summary>
    /// <param name="whitelist">The whitelist of access tags.</param>
    /// <param name="blacklist">The blacklist of access tags.</param>
    public static void Lock(BitFlag whitelist, BitFlag blacklist)
    {
        Locked = true;
        lockWhitelist = whitelist;
        lockBlacklist = blacklist;
    }

    /// <summary>
    /// Locks the input system with a specific whitelist.
    /// <remarks>
    /// All <see cref="InputAction"/>s and other input requests with a tag contained in the whitelist or with the <see cref="AllAccessTag"/> will be processed.
    /// </remarks>
    /// </summary>
    /// <param name="whitelist">The whitelist of access tags.</param>
    public static void LockWhitelist(BitFlag whitelist)
    {
        Locked = true;
        lockWhitelist = whitelist;
        lockBlacklist = BitFlag.Empty;
    }

    /// <summary>
    /// Locks the input system with a specific blacklist.
    /// <remarks>
    /// All <see cref="InputAction"/>s and other input requests with a tag contained in the blacklist will not be processed.
    /// </remarks>
    /// </summary>
    /// <param name="blacklist">The blacklist of access tags.</param>
    public static void LockBlacklist(BitFlag blacklist)
    {
        Locked = true;
        lockBlacklist = blacklist;
        lockWhitelist = BitFlag.Empty;
    }

    /// <summary>
    /// Unlocks the input system, clearing all whitelists and blacklists.
    /// </summary>
    public static void Unlock()
    {
        Locked = false;
        lockWhitelist = BitFlag.Empty;
        lockBlacklist = BitFlag.Empty;
    }

    /// <summary>
    /// Determines if the specified access tag has access.
    /// <remarks>
    /// <see cref="AllAccessTag"/> always returns true (has access).
    /// <list type="bullet">
    /// <item>If <c>tag</c> is contained in the current blacklist, this function will return false (no access).</item>
    /// <item>If <c>tag</c> is not contained in the current blacklist and <c>tag</c> is contained in the current whitelist,
    /// or the current whitelist is empty, this function will return true (has access).</item>
    /// </list>
    /// </remarks>
    /// </summary>
    /// <param name="tag">The access tag to check.</param>
    /// <returns>True if access is granted; otherwise, false.</returns>
    public static bool HasAccess(uint tag)
    {
        if (tag == AllAccessTag) return true;
        return (lockWhitelist.IsEmpty() || lockWhitelist.Has(tag)) && !lockBlacklist.Has(tag);
    }

    /// <summary>
    /// Determines if input is available for the specified access tag.
    /// <remarks>
    /// Always returns true if <see cref="Locked"/> is false.
    /// Otherwise returns <see cref="HasAccess(uint)"/> with the <c>tag</c> parameter.
    /// </remarks>
    /// </summary>
    /// <param name="tag">The access tag to check.</param>
    /// <returns>True if input is available; otherwise, false.</returns>
    public static bool IsInputAvailable(uint tag)
    {
        if (!Locked) return true;
        return HasAccess(tag);
    }

    /// <summary>
    /// Determines if the specified action has access.
    /// </summary>
    /// <param name="action">The input action to check.</param>
    /// <returns>True if access is granted; otherwise, false.</returns>
    public static bool HasAccess(InputAction action) => HasAccess(action.AccessTag);

    #endregion
}
