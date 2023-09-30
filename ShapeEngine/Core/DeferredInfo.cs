namespace ShapeEngine.Core;

internal class DeferredInfo
{
    private readonly Action action;
    private int frames;
    public DeferredInfo(Action action, int frames)
    {
        this.action = action;
        this.frames = frames;
    }

    public bool Call()
    {
        if (frames <= 0)
        {
            action.Invoke();
            return true;
        }
        else
        {
            frames--;
            return false;
        }
    }

}