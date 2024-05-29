using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Stats;

public class Buff : IBuff
{
    protected readonly List<BuffEffect> Effects;
    public uint Id { get; private set; }
    public uint GetId() => Id;
    
    public Buff(uint id)
    {
        Id = id;
        Effects = new();
    }
    public Buff(uint id, params BuffEffect[] effects)
    {
        Id = id;
        this.Effects = new(effects.Length);
        this.Effects.AddRange(effects);
    }

    public virtual IBuff Clone() => new Buff(Id, Effects.ToArray());

    public void AddEffect(BuffEffect buffEffect)
    {
        Effects.Add(buffEffect);
    }
    
    public virtual void AddStacks(int amount) { }
    public virtual bool RemoveStacks(int amount) => true;
    public void ApplyTo(IStat stat)
    {
        if (Effects.Count <= 0) return;
        foreach (var effect in Effects)
        {
            if (stat.IsAffected(effect.Tag))
            {
                stat.Apply(GetCurBuffValue(effect));
            }
        }
    }
    public virtual void Update(float dt) { }
    public virtual void Draw(Rect rect) { }
    public virtual bool IsFinished() => false;
    public virtual void GetEffectTexts(ref List<string> result)
    {
        foreach (var effect in Effects)
        {
            var v = GetCurBuffValue(effect);
            result.Add(v.ToText());
        }
    }
    
    protected virtual BuffValue GetCurBuffValue(BuffEffect effect)
    {
        return new (effect.Bonus, effect.Flat);
    }
}