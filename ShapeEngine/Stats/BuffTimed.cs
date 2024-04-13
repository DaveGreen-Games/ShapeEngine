namespace ShapeEngine.Stats;

public class BuffTimed : Buff
{
    public float Duration { get; private set; }
    public float Timer { get; protected set; }
    public float TimerF
    {
        get
        {
            if (Duration <= 0f) return 0f;
            return 1f - (Timer / Duration);
        }
    }
    public bool Degrading = false;

    public BuffTimed(uint id, float duration, bool degrading) : base(id)
    {
        Duration = duration;
        if (this.Duration > 0f) Timer = this.Duration;
        else Timer = 0f;
        Degrading = degrading;
    }
    public BuffTimed(uint id, float duration, bool degrading, params BuffEffect[] effects) : base(id, effects)
    {
        Duration = duration;
        if (this.Duration > 0f) Timer = this.Duration;
        else Timer = 0f;
        Degrading = degrading;
    }

    public override IBuff Clone() => new BuffTimed(Id, Duration, Degrading, Effects.ToArray());

    public override void AddStacks(int amount)
    {
        if (Duration > 0) Timer = Duration;
    }
   
    public override void Update(float dt)
    {
        if (Duration > 0f)
        {
            Timer -= dt;
        }
    }
    public override bool IsFinished() => Duration > 0f && Timer <= 0f;
    
    
    protected override BuffValue GetCurBuffValue(BuffEffect effect)
    {
        float f = 1f;
        if (Degrading && Duration > 0) f = TimerF;
    
        return new (effect.Bonus * f, effect.Flat * f);
    }
}