namespace ShapeEngine.Core;

/// <summary>
/// Represents a deferred action that will be executed after a specified number of frames.
/// </summary>
internal class DeferredInfo
{
    /// <summary>
    /// The action to be executed after the delay.
    /// </summary>
    private readonly Action action;

    /// <summary>
    /// The number of frames to wait before executing the action.
    /// </summary>
    private int frames;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredInfo"/> class.
    /// </summary>
    /// <param name="action">The action to execute after the delay.</param>
    /// <param name="frames">The number of frames to wait before execution.</param>
    public DeferredInfo(Action action, int frames)
    {
        this.action = action;
        this.frames = frames;
    }

    /// <summary>
    /// Decrements the frame counter and invokes the action if the delay has elapsed.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the action was invoked; otherwise, <c>false</c>.
    /// </returns>
    public bool Call()
    {
        if (frames <= 0)
        {
            action.Invoke();
            return true;
        }
        
        frames--;
        return false;
    }

}