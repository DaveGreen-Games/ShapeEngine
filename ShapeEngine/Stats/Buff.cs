using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Stats;

/// <summary>
/// Represents a basic buff that can apply one or more effects to a stat.
/// </summary>
/// <remarks>
/// Buffs can be extended for stacking, timing, or other advanced behaviors.
/// </remarks>
public class Buff : IBuff
{
    /// <summary>
    /// The list of effects this buff applies.
    /// </summary>
    protected readonly List<BuffEffect> Effects;
    /// <summary>
    /// The unique identifier for this buff.
    /// </summary>
    public uint Id { get; private set; }
    /// <summary>
    /// Gets the unique identifier for this buff.
    /// </summary>
    /// <returns>The buff's unique ID.</returns>
    public uint GetId() => Id;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Buff"/> class with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier for the buff.</param>
    public Buff(uint id)
    {
        Id = id;
        Effects = new();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Buff"/> class with the specified ID and effects.
    /// </summary>
    /// <param name="id">The unique identifier for the buff.</param>
    /// <param name="effects">The effects to apply with this buff.</param>
    public Buff(uint id, params BuffEffect[] effects)
    {
        Id = id;
        this.Effects = new(effects.Length);
        this.Effects.AddRange(effects);
    }

    /// <summary>
    /// Creates a copy of this buff.
    /// </summary>
    /// <returns>A new <see cref="IBuff"/> instance with the same properties and effects.</returns>
    public virtual IBuff Clone() => new Buff(Id, Effects.ToArray());

    /// <summary>
    /// Adds an effect to this buff.
    /// </summary>
    /// <param name="buffEffect">The effect to add.</param>
    public void AddEffect(BuffEffect buffEffect)
    {
        Effects.Add(buffEffect);
    }
    
    /// <summary>
    /// Adds stacks to this buff.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="amount">The number of stacks to add.</param>
    public virtual void AddStacks(int amount) { }
    /// <summary>
    /// Removes stacks from this buff.
    /// Default implementation does nothing and always returns true.
    /// </summary>
    /// <param name="amount">The number of stacks to remove.</param>
    /// <returns>True if the buff should be removed; otherwise, false.</returns>
    public virtual bool RemoveStacks(int amount) => true;
    /// <summary>
    /// Applies this buff's effects to the specified stat.
    /// </summary>
    /// <param name="stat">The stat to apply effects to.</param>
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
    /// <summary>
    /// Updates the buff's state.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="dt">The time delta since the last update.</param>
    public virtual void Update(float dt) { }
    /// <summary>
    /// Draws the buff in the specified rectangle.
    /// Default implementation does nothing.
    /// </summary>
    /// <param name="rect">The rectangle to draw in.</param>
    public virtual void Draw(Rect rect) { }
    /// <summary>
    /// Determines whether the buff is finished and should be removed.
    /// </summary>
    /// <returns>True if the buff is finished; otherwise, false.</returns>
    public virtual bool IsFinished() => false;
    /// <summary>
    /// Gets the textual descriptions of all effects for UI or debugging.
    /// </summary>
    /// <param name="result">A list to which effect texts will be added.</param>
    public virtual void GetEffectTexts(ref List<string> result)
    {
        foreach (var effect in Effects)
        {
            var v = GetCurBuffValue(effect);
            result.Add(v.ToText());
        }
    }
    
    /// <summary>
    /// Gets the current value of a buff effect. Can be overridden for time/stack-based buffs.
    /// </summary>
    /// <param name="effect">The effect to evaluate.</param>
    /// <returns>The current <see cref="BuffValue"/> for the effect.</returns>
    protected virtual BuffValue GetCurBuffValue(BuffEffect effect)
    {
        return new (effect.Bonus, effect.Flat);
    }
}