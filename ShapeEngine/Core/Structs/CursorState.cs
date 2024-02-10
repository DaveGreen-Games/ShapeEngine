namespace ShapeEngine.Core.Structs;

internal readonly struct CursorState
{
    public readonly bool Visible;
    public readonly bool Enabled;
    public readonly bool OnScreen;

    public CursorState()
    {
        Visible = true;
        Enabled = true;
        OnScreen = true;
    }

    public CursorState(bool visible, bool enabled, bool onScreen)
    {
        Visible = visible;
        Enabled = enabled;
        OnScreen = onScreen;
    }
}