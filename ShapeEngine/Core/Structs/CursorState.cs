namespace ShapeEngine.Core.Structs;

public readonly struct CursorState
{
    public readonly bool Hidden;
    public readonly bool Locked;
    public readonly bool OnScreen;

    public CursorState()
    {
        Hidden = false;
        Locked = false;
        OnScreen = true;
    }

    public CursorState(bool hidden, bool locked, bool onScreen)
    {
        Hidden = hidden;
        Locked = locked;
        OnScreen = onScreen;
    }
}