namespace ShapeEngine.Input;

/// <summary>
/// Handles input events and manages listeners for keyboard, mouse, and gamepad input events.
/// </summary>
public class InputEventHandler
{
    /// <summary>
    /// Callback for receiving input events.
    /// </summary>
    /// <param name="inputEvent">The input event received.</param>
    /// <returns>True if the event should propagate; otherwise, false.</returns>
    public delegate bool InputEventCallback(InputEvent inputEvent);
    
    private readonly Dictionary<InputEventCallback, uint> listeners = new();
    private readonly List<InputEventCallback> sortedListeners = new();
    
    /// <summary>
    /// Initializes a new instance of <see cref="InputEventHandler"/> and subscribes to device events.
    /// </summary>
    /// <param name="keyboard">The keyboard device.</param>
    /// <param name="mouse">The mouse device.</param>
    /// <param name="gamepadManager">The gamepad device manager.</param>
    public InputEventHandler(KeyboardDevice keyboard, MouseDevice mouse, GamepadDeviceManager gamepadManager)
    {
        keyboard.OnButtonPressed += OnKeyboardButtonPressed;
        keyboard.OnButtonReleased += OnKeyboardButtonReleased;
        mouse.OnButtonPressed += OnMouseButtonPressed;
        mouse.OnButtonReleased += OnMouseButtonReleased;
        gamepadManager.OnGamepadButtonPressed += OnGamepadButtonPressed;
        gamepadManager.OnGamepadButtonReleased += OnGamepadButtonReleased;
    }

    /// <summary>
    /// Register a callback to be notified when a button was pressed/ released on a keyboard, mouse, or gamepad.
    /// </summary>
    /// <param name="callback">The callback for input events.</param>
    /// <param name="order">Lower order comes first.</param>
    /// <returns>Returns if adding was successful.</returns>
    public bool AddListener(InputEventCallback callback, uint order)
    {
        if (!listeners.TryAdd(callback, order)) return false;
        SortListeners();
        return true;
    }

    /// <summary>
    /// Remove the specified callback listener.
    /// </summary>
    /// <param name="callback">The callback to remove.</param>
    /// <returns>Returns if removal was successful.</returns>
    public bool RemoveListener(InputEventCallback callback)
    {
        var removed = listeners.Remove(callback);
        if(removed) SortListeners();
        return removed;
    }

    /// <summary>
    /// Invokes all registered listeners for the given input event in order.
    /// </summary>
    /// <param name="inputEvent">The input event to propagate.</param>
    private void OnInputEventTriggered(InputEvent inputEvent)
    {
        foreach (var listener in sortedListeners)
        {
            var propagate = listener.Invoke(inputEvent);
            if (!propagate) return;
        }
    }

    /// <summary>
    /// Sorts listeners by their order value.
    /// </summary>
    private void SortListeners()
    {
        sortedListeners.Clear();
        var result = listeners.OrderBy(x => x.Value);
        foreach (var kvp in result)
        {
            sortedListeners.Add(kvp.Key);
        }
    }
    
    private void OnKeyboardButtonPressed(ShapeKeyboardButton button) => OnInputEventTriggered(new InputEvent(button));
    private void OnKeyboardButtonReleased(ShapeKeyboardButton button) => OnInputEventTriggered(new InputEvent(button));
    private void OnMouseButtonPressed(ShapeMouseButton button) => OnInputEventTriggered(new InputEvent(button));
    private void OnMouseButtonReleased(ShapeMouseButton button) => OnInputEventTriggered(new InputEvent(button));
    private void OnGamepadButtonPressed(GamepadDevice gamepad, ShapeGamepadButton button) => OnInputEventTriggered(new InputEvent(gamepad, button));
    private void OnGamepadButtonReleased(GamepadDevice gamepad, ShapeGamepadButton button) => OnInputEventTriggered(new InputEvent(gamepad, button));
}