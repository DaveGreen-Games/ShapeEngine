namespace ShapeEngine.Input;

public class InputEventHandler
{
    /// <summary>
    /// Callback for receiving input events. Should return if input event should propagate further down.
    /// </summary>
    public delegate bool InputEventCallback(InputEvent inputEvent);
    
    private readonly Dictionary<InputEventCallback, uint> listeners = new();
    private readonly List<InputEventCallback> sortedListeners = new();
    
    public InputEventHandler(ShapeKeyboardDevice keyboard, ShapeMouseDevice mouse, ShapeGamepadDeviceManager gamepadManager)
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
    /// <param name="priority">Lower Priorities are called first.</param>
    /// <returns>Returns if adding was successfull.</returns>
    public bool AddListener(InputEventCallback callback, uint priority)
    {
        if (!listeners.TryAdd(callback, priority)) return false;
        SortListeners();
        return true;
    }

    /// <summary>
    /// Remove the specified callback listener.
    /// </summary>
    /// <param name="callback">The callback to removed.</param>
    /// <returns>Returns if removal was successfull.</returns>
    public bool RemoveListener(InputEventCallback callback)
    {
        var removed = listeners.Remove(callback);
        if(removed) SortListeners();
        return removed;
    }

    private void OnInputEventTriggered(InputEvent inputEvent)
    {
        foreach (var listener in sortedListeners)
        {
            var propagate = listener.Invoke(inputEvent);
            if (!propagate) return;
        }
    }

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
    private void OnGamepadButtonPressed(ShapeGamepadDevice gamepad, ShapeGamepadButton button) => OnInputEventTriggered(new InputEvent(gamepad, button));
    private void OnGamepadButtonReleased(ShapeGamepadDevice gamepad, ShapeGamepadButton button) => OnInputEventTriggered(new InputEvent(gamepad, button));
}