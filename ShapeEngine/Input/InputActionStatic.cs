using ShapeEngine.Core.Structs;

namespace ShapeEngine.Input;

//TODO: Move the lock system out of the InputAction class and to ShapeInput ? So that all input systems have an access tag and can be locked out of receiving input?
public partial class InputAction
{
    #region Members

    /// <summary>
    /// This access tag grants access regardless of the input system lock.
    /// </summary>
    public static readonly uint AllAccessTag = NextTag;

    /// <summary>
    /// The default access tag for actions.
    /// </summary>
    public static readonly uint DefaultAccessTag = NextTag;

    /// <summary>
    /// Indicates if the input system is currently locked.
    /// </summary>
    public static bool Locked { get; private set; }

    private static BitFlag lockWhitelist;
    private static BitFlag lockBlacklist;
    private static uint tagPowerCounter = 1;

    /// <summary>
    /// Gets the next available access tag.
    /// </summary>
    public static uint NextTag => BitFlag.GetFlagUint(tagPowerCounter++);

    #endregion

    #region Lock System

    /// <summary>
    /// Locks the input system, clearing all whitelists and blacklists.
    /// <remarks>
    /// Only <see cref="InputAction"/> with <see cref="AllAccessTag"/> will be able to trigger.
    /// </remarks>
    /// </summary>
    public static void Lock()
    {
        Locked = true;
        lockWhitelist = BitFlag.Clear();
        lockBlacklist = BitFlag.Clear();
    }

    /// <summary>
    /// Locks the input system with a specific whitelist and blacklist.
    /// <remarks>
    /// All <see cref="InputAction"/> with a tag contained in the whitelist will be able to trigger.
    /// All <see cref="InputAction"/> with a tag contained in the blacklist will not be able to trigger.
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
    /// All <see cref="InputAction"/> with a tag contained in the whitelist will be able to trigger.
    /// </remarks>
    /// </summary>
    /// <param name="whitelist">The whitelist of access tags.</param>
    public static void LockWhitelist(BitFlag whitelist)
    {
        Locked = true;
        lockWhitelist = whitelist;
        lockBlacklist = BitFlag.Clear();
    }

    /// <summary>
    /// Locks the input system with a specific blacklist.
    /// <remarks>
    /// All <see cref="InputAction"/> with a tag contained in the blacklist will not be able to trigger.
    /// </remarks>
    /// </summary>
    /// <param name="blacklist">The blacklist of access tags.</param>
    public static void LockBlacklist(BitFlag blacklist)
    {
        Locked = true;
        lockBlacklist = blacklist;
        lockWhitelist = BitFlag.Clear();
    }

    /// <summary>
    /// Unlocks the input system, clearing all whitelists and blacklists.
    /// </summary>
    public static void Unlock()
    {
        Locked = false;
        lockWhitelist = BitFlag.Clear();
        lockBlacklist = BitFlag.Clear();
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

    #region Input Actions

    /// <summary>
    /// Updates a set of input actions with the specified gamepad and time delta.
    /// </summary>
    /// <param name="dt">The time delta in seconds.</param>
    /// <param name="gamepad">The gamepad device to use.</param>
    /// <param name="actions">The input actions to update.</param>
    public static void UpdateActions(float dt, GamepadDevice? gamepad, params InputAction[] actions)
    {
        foreach (var action in actions)
        {
            action.Gamepad = gamepad;
            action.Update(dt);
        }
    }

    /// <summary>
    /// Updates a list of input actions with the specified gamepad and time delta.
    /// </summary>
    /// <param name="dt">The time delta in seconds.</param>
    /// <param name="gamepad">The gamepad device to use.</param>
    /// <param name="actions">The input actions to update.</param>
    public static void UpdateActions(float dt, GamepadDevice? gamepad, List<InputAction> actions)
    {
        foreach (var action in actions)
        {
            action.Gamepad = gamepad;
            action.Update(dt);
        }
    }

    /// <summary>
    /// Gets descriptions for a list of input actions.
    /// </summary>
    /// <param name="inputDeviceType">The device type to filter by.</param>
    /// <param name="shorthand">Whether to use shorthand names.</param>
    /// <param name="typesPerActionCount">The number of types per action.</param>
    /// <param name="actions">The input actions to describe.</param>
    /// <returns>A list of action descriptions.</returns>
    public static List<string> GetActionDescriptions(InputDeviceType inputDeviceType, bool shorthand, int typesPerActionCount, List<InputAction> actions)
    {
        var final = new List<string>();
        foreach (var action in actions)
        {
            var description = action.GetInputTypeDescription(inputDeviceType, shorthand, typesPerActionCount, true);

            final.Add(description);
        }

        return final;
    }

    /// <summary>
    /// Gets descriptions for a set of input actions.
    /// </summary>
    /// <param name="inputDeviceType">The device type to filter by.</param>
    /// <param name="shorthand">Whether to use shorthand names.</param>
    /// <param name="typesPerActionCount">The number of types per action.</param>
    /// <param name="actions">The input actions to describe.</param>
    /// <returns>A list of action descriptions.</returns>
    public static List<string> GetActionDescriptions(InputDeviceType inputDeviceType, bool shorthand, int typesPerActionCount, params InputAction[] actions)
    {
        var final = new List<string>();
        foreach (var action in actions)
        {
            var description = action.GetInputTypeDescription(inputDeviceType, shorthand, typesPerActionCount, true);

            final.Add(description);
        }

        return final;
    }

    #endregion

    #region Basic

    //TODO: ShapeInput should have GetState / ConsumeState for all buttons, axes, and wheel axes as well!
    
    /// <summary>
    /// Gets the input state for a keyboard button.
    /// </summary>
    /// <param name="button">The keyboard button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeKeyboardButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.ActiveKeyboardDevice.CreateInputState(button); // InputTypeKeyboardButton.GetState(button);
    }

    /// <summary>
    /// Gets the input state for a mouse button.
    /// </summary>
    /// <param name="button">The mouse button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeMouseButton button, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.ActiveMouseDevice.CreateInputState(button); // InputTypeMouseButton.GetState(button);
    }

    /// <summary>
    /// Gets the input state for a gamepad button.
    /// </summary>
    /// <param name="button">The gamepad button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <param name="deadzone">The deadzone threshold.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeGamepadButton button, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ShapeInput.ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.CreateInputState(button, deadzone); //  InputTypeGamepadButton.GetState(button, gamepad, deadzone);
    }

    /// <summary>
    /// Gets the input state for a keyboard button axis.
    /// </summary>
    /// <param name="neg">The negative direction button.</param>
    /// <param name="pos">The positive direction button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeKeyboardButton neg, ShapeKeyboardButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.ActiveKeyboardDevice.CreateInputState(neg, pos); // InputTypeKeyboardButtonAxis.GetState(neg, pos);
    }

    /// <summary>
    /// Gets the input state for a mouse button axis.
    /// </summary>
    /// <param name="neg">The negative direction button.</param>
    /// <param name="pos">The positive direction button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeMouseButton neg, ShapeMouseButton pos, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.ActiveMouseDevice.CreateInputState(neg, pos); // InputTypeMouseButtonAxis.GetState(neg, pos);
    }

    /// <summary>
    /// Gets the input state for a gamepad button axis.
    /// </summary>
    /// <param name="neg">The negative direction button.</param>
    /// <param name="pos">The positive direction button.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <param name="deadzone">The deadzone threshold.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeGamepadButton neg, ShapeGamepadButton pos, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ShapeInput.ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.CreateInputState(neg, pos, deadzone);
        // return InputTypeGamepadButtonAxis.GetState(neg, pos, gamepad, deadzone);
    }

    /// <summary>
    /// Gets the input state for a mouse wheel axis.
    /// </summary>
    /// <param name="axis">The mouse wheel axis.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <param name="deadzone">The deadzone threshold.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeMouseWheelAxis axis, uint accessTag, float deadzone = 1f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        return ShapeInput.ActiveMouseDevice.CreateInputState(axis, deadzone); // InputTypeMouseWheelAxis.GetState(axis);
    }

    /// <summary>
    /// Gets the input state for a gamepad axis.
    /// </summary>
    /// <param name="axis">The gamepad axis.</param>
    /// <param name="accessTag">The access tag.</param>
    /// <param name="gamepadIndex">The gamepad index.</param>
    /// <param name="deadzone">The deadzone threshold.</param>
    /// <returns>The input state.</returns>
    public static InputState GetState(ShapeGamepadAxis axis, uint accessTag, int gamepadIndex, float deadzone = 0.2f)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        var gamepad = ShapeInput.ActiveGamepadDeviceManager.GetGamepad(gamepadIndex);
        return gamepad == null ? new() : gamepad.CreateInputState(axis, deadzone);
        // return InputTypeGamepadAxis.GetState(axis, gamepad, deadzone);
    }

    #endregion
}