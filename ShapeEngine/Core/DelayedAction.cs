using ShapeEngine.Timing;

namespace ShapeEngine.Core;

internal class DelayedAction : ISequenceable
{
    private readonly Action action;
    private float timer;

    public DelayedAction(float delay, Action action)
    {
        if (delay <= 0)
        {
            this.timer = 0f;
            this.action = action;
            this.action();
        }
        else
        {
            this.timer = delay;
            this.action = action;
        }
    }

    private DelayedAction(DelayedAction action)
    {
        this.timer = action.timer;
        this.action = action.action;
    }

    public ISequenceable Copy() => new DelayedAction(this);
    public bool Update(float dt)
    {
        if (timer <= 0f) return true;
        else
        {
            timer -= dt;
            if (timer > 0f) return false;
            this.action.Invoke();
            return true;
        }
    }
}