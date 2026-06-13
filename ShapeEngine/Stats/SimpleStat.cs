namespace ShapeEngine.Stats;

/// <summary>
/// Represents a simple stat with a base value and modifiers that affect it.
/// </summary>
public class SimpleStat
{
    /// <summary>
    /// Duration value that represents a permanent modifier (never expires).
    /// </summary>
    public const float PermanentDuration = -1f;

    /// <summary>
    /// Represents a modifier applied to a stat.
    /// </summary>
    private struct Modifier
    {
        /// <summary>
        /// Unique identifier for the modifier.
        /// </summary>
        public int Id;

        /// <summary>
        /// The modifier value.
        /// </summary>
        public float Value;

        /// <summary>
        /// The total duration of the modifier.
        /// </summary>
        public float Duration;

        /// <summary>
        /// The remaining duration of the modifier.
        /// </summary>
        public float Remaining;

        /// <summary>
        /// The number of times the modifier is stacked.
        /// </summary>
        public int Stacks;
    }

    /// <summary>
    /// Array of flat bonus modifiers.
    /// </summary>
    private Modifier[] flatBonuses;

    /// <summary>
    /// Array of additive multiplier modifiers.
    /// </summary>
    private Modifier[] additiveMultipliers;

    /// <summary>
    /// Array of multiplicative multiplier modifiers.
    /// </summary>
    private Modifier[] multiplicativeMultipliers;

    /// <summary>
    /// Number of flat bonus modifiers currently applied.
    /// </summary>
    private int flatBonusCount;

    /// <summary>
    /// Number of additive multiplier modifiers currently applied.
    /// </summary>
    private int additiveMultiplierCount;

    /// <summary>
    /// Number of multiplicative multiplier modifiers currently applied.
    /// </summary>
    private int multiplicativeMultiplierCount;

    /// <summary>
    /// The base value of the stat.
    /// </summary>
    private float baseValue;

    /// <summary>
    /// The current calculated value of the stat after applying all modifiers and bounds.
    /// </summary>
    private float currentValue;

    /// <summary>
    /// The minimum value bound for the stat (if set).
    /// </summary>
    private float minValue;

    /// <summary>
    /// The maximum value bound for the stat (if set).
    /// </summary>
    private float maxValue;

    /// <summary>
    /// True if the stat has a minimum value bound set.
    /// </summary>
    private bool hasMinValue;

    /// <summary>
    /// True if the stat has a maximum value bound set.
    /// </summary>
    private bool hasMaxValue;

    /// <summary>
    /// The display name of the stat.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The abbreviated display name of the stat.
    /// </summary>
    public string Abbreviation { get; set; }

    /// <summary>
    /// The description of the stat.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the base value of the stat. Setting this will recalculate the current value.
    /// </summary>
    public float BaseValue
    {
        
        get => baseValue;
        
        set
        {
            baseValue = value;
            Recalculate();
        }
    }

    /// <summary>
    /// Gets the current calculated value of the stat.
    /// </summary>
    public float CurrentValue => currentValue;

    /// <summary>
    /// Gets a value indicating whether the stat has a minimum value bound set.
    /// </summary>
    public bool HasMinValue => hasMinValue;

    /// <summary>
    /// Gets a value indicating whether the stat has a maximum value bound set.
    /// </summary>
    public bool HasMaxValue => hasMaxValue;

    /// <summary>
    /// Gets the minimum value bound of the stat.
    /// </summary>
    public float MinValue => minValue;

    /// <summary>
    /// Gets the maximum value bound of the stat.
    /// </summary>
    public float MaxValue => maxValue;

    /// <summary>
    /// Gets the number of flat bonus modifiers currently applied.
    /// </summary>
    public int FlatBonusCount => flatBonusCount;

    /// <summary>
    /// Gets the number of additive multiplier modifiers currently applied.
    /// </summary>
    public int AdditiveMultiplierCount => additiveMultiplierCount;

    /// <summary>
    /// Gets the number of multiplicative multiplier modifiers currently applied.
    /// </summary>
    public int MultiplicativeMultiplierCount => multiplicativeMultiplierCount;

    /// <summary>
    /// Creates a new simple stat with the specified base value and optional modifiers.
    /// </summary>
    /// <param name="baseValue">The base value of the stat.</param>
    /// <param name="name">The display name of the stat.</param>
    /// <param name="abbreviation">The abbreviated display name of the stat.</param>
    /// <param name="description">The description of the stat.</param>
    /// <param name="flatBonusCapacity">The initial capacity for flat bonus modifiers (default 4).</param>
    /// <param name="additiveMultiplierCapacity">The initial capacity for additive multiplier modifiers (default 4).</param>
    /// <param name="multiplicativeMultiplierCapacity">The initial capacity for multiplicative multiplier modifiers (default 4).</param>
    public SimpleStat(
        float baseValue = 0f,
        string name = "",
        string abbreviation = "",
        string description = "",
        int flatBonusCapacity = 4,
        int additiveMultiplierCapacity = 4,
        int multiplicativeMultiplierCapacity = 4)
    {
        this.baseValue = baseValue;
        Name = name;
        Abbreviation = abbreviation;
        Description = description;

        flatBonuses = new Modifier[flatBonusCapacity < 0 ? 0 : flatBonusCapacity];
        additiveMultipliers = new Modifier[additiveMultiplierCapacity < 0 ? 0 : additiveMultiplierCapacity];
        multiplicativeMultipliers = new Modifier[multiplicativeMultiplierCapacity < 0 ? 0 : multiplicativeMultiplierCapacity];

        currentValue = ApplyBounds(baseValue);
    }

    /// <summary>
    /// Creates a copy of this simple stat.
    /// </summary>
    private SimpleStat(SimpleStat other)
    {
        baseValue = other.baseValue;
        currentValue = other.currentValue;
        minValue = other.minValue;
        maxValue = other.maxValue;
        hasMinValue = other.hasMinValue;
        hasMaxValue = other.hasMaxValue;

        Name = other.Name;
        Abbreviation = other.Abbreviation;
        Description = other.Description;

        flatBonusCount = other.flatBonusCount;
        additiveMultiplierCount = other.additiveMultiplierCount;
        multiplicativeMultiplierCount = other.multiplicativeMultiplierCount;

        flatBonuses = new Modifier[other.flatBonuses.Length];
        additiveMultipliers = new Modifier[other.additiveMultipliers.Length];
        multiplicativeMultipliers = new Modifier[other.multiplicativeMultipliers.Length];

        CopyModifiers(other.flatBonuses, flatBonuses, flatBonusCount);
        CopyModifiers(other.additiveMultipliers, additiveMultipliers, additiveMultiplierCount);
        CopyModifiers(other.multiplicativeMultipliers, multiplicativeMultipliers, multiplicativeMultiplierCount);
    }

    /// <summary>
    /// Sets the minimum value bound for the stat. If the current maximum is lower than this value, the maximum is also updated to match.
    /// </summary>
    /// <param name="value">The minimum value to set.</param>
    public void SetMin(float value)
    {
        minValue = value;
        hasMinValue = true;

        if (hasMaxValue && maxValue < minValue)
        {
            maxValue = minValue;
        }

        Recalculate();
    }

    /// <summary>
    /// Sets the maximum value bound for the stat. If the current minimum is higher than this value, the minimum is also updated to match.
    /// </summary>
    /// <param name="value">The maximum value to set.</param>
    public void SetMax(float value)
    {
        maxValue = value;
        hasMaxValue = true;

        if (hasMinValue && minValue > maxValue)
        {
            minValue = maxValue;
        }

        Recalculate();
    }

    /// <summary>
    /// Sets both minimum and maximum value bounds for the stat.
    /// </summary>
    /// <param name="min">The minimum value bound.</param>
    /// <param name="max">The maximum value bound.</param>
    public void SetBounds(float min, float max)
    {
        if (max < min)
        {
            max = min;
        }

        minValue = min;
        maxValue = max;
        hasMinValue = true;
        hasMaxValue = true;
        Recalculate();
    }

    /// <summary>
    /// Clears the minimum value bound.
    /// </summary>
    public void ClearMin()
    {
        hasMinValue = false;
        Recalculate();
    }

    /// <summary>
    /// Clears the maximum value bound.
    /// </summary>
    public void ClearMax()
    {
        hasMaxValue = false;
        Recalculate();
    }

    /// <summary>
    /// Clears both minimum and maximum value bounds.
    /// </summary>
    public void ClearBounds()
    {
        hasMinValue = false;
        hasMaxValue = false;
        Recalculate();
    }

    /// <summary>
    /// Adds a flat modifier to the stat.
    /// </summary>
    /// <param name="value">The flat value to add.</param>
    /// <param name="duration">The duration of the modifier. Use <see cref="PermanentDuration"/> for a permanent modifier.</param>
    /// <param name="stacks">The number of times to stack the modifier.</param>
    public void AddFlat(float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref flatBonuses, ref flatBonusCount, 0, value, duration, stacks, false);
    }

    /// <summary>
    /// Adds a flat modifier to the stat with a specific id.
    /// </summary>
    /// <param name="id">The unique identifier for the modifier.</param>
    /// <param name="value">The flat value to add.</param>
    /// <param name="duration">The duration of the modifier. Use <see cref="PermanentDuration"/> for a permanent modifier.</param>
    /// <param name="stacks">The number of times to stack the modifier.</param>
    public void AddFlat(int id, float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref flatBonuses, ref flatBonusCount, id, value, duration, stacks, true);
    }

    /// <summary>
    /// Adds an additive percentage modifier to the stat.
    /// </summary>
    /// <param name="value">The percentage multiplier to add (e.g., 0.25 for +25%).</param>
    /// <param name="duration">The duration of the modifier. Use <see cref="PermanentDuration"/> for a permanent modifier.</param>
    /// <param name="stacks">The number of times to stack the modifier.</param>
    public void AddAdditiveMultiplier(float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref additiveMultipliers, ref additiveMultiplierCount, 0, value, duration, stacks, false);
    }

    /// <summary>
    /// Adds an additive percentage modifier to the stat with a specific id.
    /// </summary>
    /// <param name="id">The unique identifier for the modifier.</param>
    /// <param name="value">The percentage multiplier to add (e.g., 0.25 for +25%).</param>
    /// <param name="duration">The duration of the modifier. Use <see cref="PermanentDuration"/> for a permanent modifier.</param>
    /// <param name="stacks">The number of times to stack the modifier.</param>
    public void AddAdditiveMultiplier(int id, float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref additiveMultipliers, ref additiveMultiplierCount, id, value, duration, stacks, true);
    }

    /// <summary>
    /// Adds a multiplicative percentage modifier to the stat.
    /// </summary>
    /// <param name="value">The multiplier to add (e.g., 0.25 for x1.25).</param>
    /// <param name="duration">The duration of the modifier. Use <see cref="PermanentDuration"/> for a permanent modifier.</param>
    /// <param name="stacks">The number of times to stack the modifier.</param>
    public void AddMultiplicativeMultiplier(float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref multiplicativeMultipliers, ref multiplicativeMultiplierCount, 0, value, duration, stacks, false);
    }

    /// <summary>
    /// Adds a multiplicative percentage modifier to the stat with a specific id.
    /// </summary>
    /// <param name="id">The unique identifier for the modifier.</param>
    /// <param name="value">The multiplier to add (e.g., 0.25 for x1.25).</param>
    /// <param name="duration">The duration of the modifier. Use <see cref="PermanentDuration"/> for a permanent modifier.</param>
    /// <param name="stacks">The number of times to stack the modifier.</param>
    public void AddMultiplicativeMultiplier(int id, float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref multiplicativeMultipliers, ref multiplicativeMultiplierCount, id, value, duration, stacks, true);
    }

    /// <summary>
    /// Removes a flat modifier by id if it exists.
    /// </summary>
    /// <param name="id">The unique identifier of the modifier to remove.</param>
    /// <returns>True if a modifier with the given id was removed.</returns>
    public bool RemoveFlat(int id)
    {
        return RemoveModifier(flatBonuses, ref flatBonusCount, id);
    }

    /// <summary>
    /// Removes an additive multiplier by id if it exists.
    /// </summary>
    /// <param name="id">The unique identifier of the modifier to remove.</param>
    /// <returns>True if a modifier with the given id was removed.</returns>
    public bool RemoveAdditiveMultiplier(int id)
    {
        return RemoveModifier(additiveMultipliers, ref additiveMultiplierCount, id);
    }

    /// <summary>
    /// Removes a multiplicative multiplier by id if it exists.
    /// </summary>
    /// <param name="id">The unique identifier of the modifier to remove.</param>
    /// <returns>True if a modifier with the given id was removed.</returns>
    public bool RemoveMultiplicativeMultiplier(int id)
    {
        return RemoveModifier(multiplicativeMultipliers, ref multiplicativeMultiplierCount, id);
    }

    /// <summary>
    /// Updates timed modifiers by reducing their remaining duration.
    /// </summary>
    /// <param name="dt">The time delta since the last update.</param>
    public void Update(float dt)
    {
        if (dt <= 0f)
        {
            return;
        }

        var changed = UpdateModifiers(flatBonuses, ref flatBonusCount, dt);
        changed |= UpdateModifiers(additiveMultipliers, ref additiveMultiplierCount, dt);
        changed |= UpdateModifiers(multiplicativeMultipliers, ref multiplicativeMultiplierCount, dt);

        if (changed)
        {
            Recalculate();
        }
    }

    /// <summary>
    /// Resets all modifiers to their initial state (removes all modifiers).
    /// </summary>
    public void Reset()
    {
        flatBonusCount = 0;
        additiveMultiplierCount = 0;
        multiplicativeMultiplierCount = 0;
        Recalculate();
    }

    /// <summary>
    /// Creates a copy of this simple stat.
    /// </summary>
    /// <returns>A new SimpleStat instance with the same base value and modifiers.</returns>
    public SimpleStat Copy()
    {
        return new SimpleStat(this);
    }

    /// <summary>
    /// Recalculates the current value of the stat based on the base value and all modifiers.
    /// </summary>
    public void Recalculate()
    {
        var value = baseValue + SumModifiers(flatBonuses, flatBonusCount);
        value *= 1f + SumModifiers(additiveMultipliers, additiveMultiplierCount);

        for (var i = 0; i < multiplicativeMultiplierCount; i++)
        {
            var multiplier = 1f + multiplicativeMultipliers[i].Value;

            for (var stack = 0; stack < multiplicativeMultipliers[i].Stacks; stack++)
            {
                value *= multiplier;
            }
        }

        currentValue = ApplyBounds(value);
    }

    /// <summary>
    /// Adds or updates a modifier in the specified modifier array.
    /// </summary>
    /// <param name="modifiers">The array to add the modifier to.</param>
    /// <param name="count">The current count of modifiers in the array.</param>
    /// <param name="id">The unique identifier for the modifier. Use 0 for auto-generated id.</param>
    /// <param name="value">The value of the modifier.</param>
    /// <param name="duration">The duration of the modifier.</param>
    /// <param name="stacks">The number of times to stack the modifier.</param>
    /// <param name="useId">If true, tries to find and update an existing modifier with the given id instead of adding a new one.</param>
    private void AddModifier(ref Modifier[] modifiers, ref int count, int id, float value, float duration, int stacks, bool useId)
    {
        if (stacks <= 0)
        {
            return;
        }

        if (useId)
        {
            for (var i = 0; i < count; i++)
            {
                if (modifiers[i].Id != id)
                {
                    continue;
                }

                modifiers[i].Value = value;
                modifiers[i].Stacks += stacks;

                if (duration > 0f)
                {
                    modifiers[i].Duration = duration;
                    modifiers[i].Remaining = duration;
                }
                else if (modifiers[i].Duration > 0f)
                {
                    modifiers[i].Remaining = modifiers[i].Duration;
                }

                Recalculate();
                return;
            }
        }

        EnsureCapacity(ref modifiers, count + 1);

        modifiers[count] = new Modifier
        {
            Id = id,
            Value = value,
            Duration = duration,
            Remaining = duration,
            Stacks = stacks
        };

        count++;
        Recalculate();
    }

    /// <summary>
    /// Removes a modifier from the specified array by id if it exists.
    /// </summary>
    /// <param name="modifiers">The array to search for the modifier.</param>
    /// <param name="count">The count of modifiers in the array.</param>
    /// <param name="id">The unique identifier of the modifier to remove.</param>
    /// <returns>True if a modifier with the given id was found and removed.</returns>
    private bool RemoveModifier(Modifier[] modifiers, ref int count, int id)
    {
        for (var i = 0; i < count; i++)
        {
            if (modifiers[i].Id != id)
            {
                continue;
            }

            RemoveAt(modifiers, ref count, i);
            Recalculate();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Updates all timed modifiers by reducing their remaining duration by dt seconds.
    /// </summary>
    /// <param name="modifiers">The array of modifiers to update.</param>
    /// <param name="count">The count of active modifiers in the array.</param>
    /// <param name="dt">The time delta since the last update.</param>
    /// <returns>True if any modifiers expired or changed.</returns>
    private static bool UpdateModifiers(Modifier[] modifiers, ref int count, float dt)
    {
        var changed = false;

        for (var i = count - 1; i >= 0; i--)
        {
            if (modifiers[i].Duration <= 0f)
            {
                continue;
            }

            modifiers[i].Remaining -= dt;

            if (modifiers[i].Remaining > 0f)
            {
                continue;
            }

            RemoveAt(modifiers, ref count, i);
            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// Removes the modifier at the specified index from the array.
    /// </summary>
    /// <param name="modifiers">The array to remove from.</param>
    /// <param name="count">The count of active modifiers in the array.</param>
    /// <param name="index">The index of the modifier to remove.</param>
    private static void RemoveAt(Modifier[] modifiers, ref int count, int index)
    {
        count--;

        if (index != count)
        {
            modifiers[index] = modifiers[count];
        }

        modifiers[count] = default;
    }

    /// <summary>
    /// Calculates the sum of all modifiers in the array.
    /// </summary>
    /// <param name="modifiers">The array of modifiers.</param>
    /// <param name="count">The count of active modifiers in the array.</param>
    /// <returns>The sum of all active modifiers.</returns>
    private static float SumModifiers(Modifier[] modifiers, int count)
    {
        var sum = 0f;

        for (var i = 0; i < count; i++)
        {
            sum += modifiers[i].Value * modifiers[i].Stacks;
        }

        return sum;
    }

    /// <summary>
    /// Applies the minimum and maximum value bounds to the given value.
    /// </summary>
    /// <param name="value">The value to apply bounds to.</param>
    /// <returns>The value clamped within the bounds (or unchanged if no bounds are set).</returns>
    private float ApplyBounds(float value)
    {
        if (hasMinValue && value < minValue)
        {
            value = minValue;
        }

        if (hasMaxValue && value > maxValue)
        {
            value = maxValue;
        }

        return value;
    }

    /// <summary>
    /// Ensures the modifier array has sufficient capacity.
    /// </summary>
    /// <param name="modifiers">The modifier array to resize if necessary.</param>
    /// <param name="capacity">The required capacity.</param>
    private static void EnsureCapacity(ref Modifier[] modifiers, int capacity)
    {
        if (modifiers.Length >= capacity)
        {
            return;
        }

        var newCapacity = modifiers.Length == 0 ? 4 : modifiers.Length * 2;

        while (newCapacity < capacity)
        {
            newCapacity *= 2;
        }

        var newModifiers = new Modifier[newCapacity];
        CopyModifiers(modifiers, newModifiers, modifiers.Length);
        modifiers = newModifiers;
    }

    /// <summary>
    /// Copies the first count modifiers from the source array to the destination array.
    /// </summary>
    /// <param name="source">The source array to copy from.</param>
    /// <param name="destination">The destination array to copy to.</param>
    /// <param name="count">The number of modifiers to copy.</param>
    private static void CopyModifiers(Modifier[] source, Modifier[] destination, int count)
    {
        for (var i = 0; i < count; i++)
        {
            destination[i] = source[i];
        }
    }
}
