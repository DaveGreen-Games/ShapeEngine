namespace ShapeEngine.Core.Structs;

public readonly struct WindowState
{
    // public readonly WindowType WindowType;
    public readonly bool Minimized;
    public readonly bool Maximized;
    public readonly bool Fullscreen;
    public readonly bool Hidden;
    public readonly bool Focused;

    public WindowState()
    {
        // WindowType = WindowType.Default;
        Minimized = false;
        Maximized = false;
        Fullscreen = false;
        Hidden = false;
        Focused = true;
    }

    public WindowState(bool minimized, bool maximized, bool fullscreen, bool hidden, bool focused)
    {
        Minimized = minimized;
        Maximized = maximized;
        Fullscreen = fullscreen;
        Hidden = hidden;
        Focused = focused;
    }
    // public WindowState(WindowType windowType, bool focused)
    // {
    //     WindowType = windowType;
    //     // if (windowState == WindowState.Hidden || windowState == WindowState.Minimized)
    //     // {
    //     //     Focused = false;
    //     // }
    //     // else Focused = focused;
    //     Focused = focused;
    // }
}