namespace ShapeEngine.Stats;

/// <summary>
/// Represents a simple stat with a base value and modifiers that affect it.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>A base value and upper/lower bounds can be set.</item>
/// <item>Modifiers can be added, removed, and stacks of modifiers can be adjusted (reduced, increased).</item>
/// <item>Modifiers have an ID to identify them.</item>
/// <item>Modifiers can be permanent or timed. Timed modifiers that will run out are completely removed.</item>
/// <item>Adding stacks refreshes the modifier's remaining duration, if it is timed.</item>
/// <item>Removing stacks to 0 will remove the modifier.</item>
/// <item>Max stacks can be used to cap the stack number that can be reached. Timer will still be refreshed on timed modifiers.</item>
/// <item>There a flat, additive percentage, multiplicative percentage, override, min, and max modifiers.</item>
/// <item>Flat modifiers are summed up and added to the base value. Stacks apply.</item>
/// <item>Additive percentage modifiers are summed up and then multiplied with the current value. Stacks apply.</item>
/// <item>Each multiplicative modifier is multiplied with the current value seperatly for the amount of stacks it has.</item>
/// <item>If at least 1 override modifier is active the override value with the highest priority is used to set the current value directly. No flat or percentage modifiers are applied. Stacks are ignored.</item>
/// <item>The min & max modifiers with the highest priority are used to clamp the current value. Stacks are ignored.</item>
/// <item>The stats bounds are used (if set) to clamp the current value.</item>
/// </list>
/// </remarks>
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

        //TODO: Docs
        public int MaxStacks;

        //Todo: Docs - Highest wins, equal does not change, only affects override, min and max
        public int Priority;
    }

    #region Private Members
    
    /// <summary>
    /// Array of flat bonus modifiers.
    /// </summary>
    private Modifier[] flatModifiers;

    /// <summary>
    /// Array of additive multiplier modifiers.
    /// </summary>
    private Modifier[] additivePercentageModifiers;

    /// <summary>
    /// Array of multiplicative multiplier modifiers.
    /// </summary>
    private Modifier[] multiplicativePercentageModifiers;

    //TODO: Add docs
    private Modifier[] overrideModifiers;
    
    //TODO: Add docs
    private Modifier[] minModifiers;
    
    //TODO: Add docs
    private Modifier[] maxModifiers;
    
    /// <summary>
    /// Number of flat bonus modifiers currently applied.
    /// </summary>
    private int flatModifierCount;

    /// <summary>
    /// Number of additive multiplier modifiers currently applied.
    /// </summary>
    private int additivePercentageModifierCount;

    /// <summary>
    /// Number of multiplicative multiplier modifiers currently applied.
    /// </summary>
    private int multiplicativePercentageModifierCount;
    
    //TODO: Add docs
    private int overrideModifierCount;
    
    //TODO: Add docs
    private int minModifierCount;
    
    //TODO: Add docs
    private int maxModifierCount;

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

    //Todo: Docs
    private bool dirty;
    #endregion
    
    #region Public Members
    
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
            dirty = true;
            // Recalculate();
        }
    }

    /// <summary>
    /// Gets the current calculated value of the stat.
    /// </summary>
    public float CurrentValue
    {
        get
        {
            if(dirty)
            {
                Recalculate();
            }
            return currentValue;
        }
    }

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
    public int FlatModifierCount => flatModifierCount;

    /// <summary>
    /// Gets the number of additive multiplier modifiers currently applied.
    /// </summary>
    public int AdditivePercentageModifierCount => additivePercentageModifierCount;

    /// <summary>
    /// Gets the number of multiplicative multiplier modifiers currently applied.
    /// </summary>
    public int MultiplicativePercentageModifierCount => multiplicativePercentageModifierCount;

    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Creates a new simple stat with the specified base value and optional modifiers.
    /// </summary>
    /// <param name="baseValue">The base value of the stat.</param>
    /// <param name="name">The display name of the stat.</param>
    /// <param name="abbreviation">The abbreviated display name of the stat.</param>
    /// <param name="description">The description of the stat.</param>
    /// <param name="modifierCapacities">The initial capacity for modifiers.</param>
    public SimpleStat(
        float baseValue = 0f,
        string name = "",
        string abbreviation = "",
        string description = "",
        int modifierCapacities = 4)
    {
        this.baseValue = baseValue;
        Name = name;
        Abbreviation = abbreviation;
        Description = description;

        int capacity = modifierCapacities < 0 ? 0 : modifierCapacities;
        
        flatModifiers = new Modifier[capacity];
        additivePercentageModifiers = new Modifier[capacity];
        multiplicativePercentageModifiers = new Modifier[capacity];
        overrideModifiers = new Modifier[capacity];
        minModifiers = new Modifier[capacity];
        maxModifiers = new Modifier[capacity];

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
        dirty = other.dirty;

        Name = other.Name;
        Abbreviation = other.Abbreviation;
        Description = other.Description;

        flatModifierCount = other.flatModifierCount;
        additivePercentageModifierCount = other.additivePercentageModifierCount;
        multiplicativePercentageModifierCount = other.multiplicativePercentageModifierCount;
        overrideModifierCount = other.overrideModifierCount;
        minModifierCount = other.minModifierCount;
        maxModifierCount = other.maxModifierCount;

        flatModifiers = new Modifier[other.flatModifiers.Length];
        additivePercentageModifiers = new Modifier[other.additivePercentageModifiers.Length];
        multiplicativePercentageModifiers = new Modifier[other.multiplicativePercentageModifiers.Length];
        overrideModifiers = new Modifier[other.overrideModifiers.Length];
        minModifiers = new Modifier[other.minModifiers.Length];
        maxModifiers = new Modifier[other.maxModifiers.Length];

        CopyModifiers(other.flatModifiers, flatModifiers, flatModifierCount);
        CopyModifiers(other.additivePercentageModifiers, additivePercentageModifiers, additivePercentageModifierCount);
        CopyModifiers(other.multiplicativePercentageModifiers, multiplicativePercentageModifiers, multiplicativePercentageModifierCount);
        CopyModifiers(other.overrideModifiers, overrideModifiers, overrideModifierCount);
        CopyModifiers(other.minModifiers, minModifiers, minModifierCount);
        CopyModifiers(other.maxModifiers, maxModifiers, maxModifierCount);
    }

    #endregion
    
    #region Public Functions
    
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

        dirty = true;
        // Recalculate();
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

        dirty = true;
        // Recalculate();
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
            (min, max) = (max, min);
        }

        minValue = min;
        maxValue = max;
        hasMinValue = true;
        hasMaxValue = true;
        dirty = true;
        // Recalculate();
    }

    /// <summary>
    /// Clears the minimum value bound.
    /// </summary>
    public void ClearMin()
    {
        hasMinValue = false;
        dirty = true;
        // Recalculate();
    }

    /// <summary>
    /// Clears the maximum value bound.
    /// </summary>
    public void ClearMax()
    {
        hasMaxValue = false;
        dirty = true;
        // Recalculate();
    }

    /// <summary>
    /// Clears both minimum and maximum value bounds.
    /// </summary>
    public void ClearBounds()
    {
        hasMinValue = false;
        hasMaxValue = false;
        dirty = true;
        // Recalculate();
    }

    //TODO: Add Docs
    public bool HasModifier(int id, StatModifierKind kind)
    {
        switch (kind)
        {
            case StatModifierKind.Flat: return ContainsModifier(flatModifiers, flatModifierCount, id);
            case StatModifierKind.AdditivePercent: return ContainsModifier(additivePercentageModifiers, additivePercentageModifierCount, id);
            case StatModifierKind.MultiplicativePercent: return ContainsModifier(multiplicativePercentageModifiers, multiplicativePercentageModifierCount, id);
            case StatModifierKind.Override: return ContainsModifier(overrideModifiers, overrideModifierCount, id);
            case StatModifierKind.Min: return ContainsModifier(minModifiers, minModifierCount, id);
            case StatModifierKind.Max: return ContainsModifier(maxModifiers, maxModifierCount, id);
        }

        return false;
    }
    
    //TODO: Docs
    public bool AddModifier(int id, float value, StatModifierKind kind, float duration = PermanentDuration, int stacks = 1, int maxStacks = 1, int priority = 0)
    {
        switch (kind)
        {
            case StatModifierKind.Flat: return AddModifier(ref flatModifiers, ref flatModifierCount, id, value, duration, stacks, maxStacks, priority);
            case StatModifierKind.AdditivePercent: return AddModifier(ref additivePercentageModifiers, ref additivePercentageModifierCount, id, value, duration, stacks, maxStacks, priority);
            case StatModifierKind.MultiplicativePercent: return AddModifier(ref multiplicativePercentageModifiers, ref multiplicativePercentageModifierCount, id, value, duration, stacks, maxStacks, priority);
            case StatModifierKind.Override: return AddModifier(ref overrideModifiers, ref overrideModifierCount, id, value, duration, stacks, maxStacks, priority);
            case StatModifierKind.Min: return AddModifier(ref minModifiers, ref minModifierCount, id, value, duration, stacks, maxStacks, priority);
            case StatModifierKind.Max: return AddModifier(ref maxModifiers, ref maxModifierCount, id, value, duration, stacks, maxStacks, priority);
        }

        return false;
    }

    //TODO: Docs
    public bool RemoveModifier(int id, StatModifierKind kind)
    {
        switch (kind)
        {
            case StatModifierKind.Flat: return RemoveModifier(flatModifiers, ref flatModifierCount, id);
            case StatModifierKind.AdditivePercent: return RemoveModifier(additivePercentageModifiers, ref additivePercentageModifierCount, id);
            case StatModifierKind.MultiplicativePercent: return RemoveModifier(multiplicativePercentageModifiers, ref multiplicativePercentageModifierCount, id);
            case StatModifierKind.Override: return RemoveModifier(overrideModifiers, ref overrideModifierCount, id);
            case StatModifierKind.Min: return RemoveModifier(minModifiers, ref minModifierCount, id);
            case StatModifierKind.Max: return RemoveModifier(maxModifiers, ref maxModifierCount, id);
        }

        return false;
    }

    //TODO: Docs
    public bool AdjustModifierStacks(int id, StatModifierKind kind, int stacks)
    {
        if (stacks == 0) return false;

        switch (kind)
        {
            case StatModifierKind.Flat: return AdjustModifierStacks(flatModifiers, ref flatModifierCount, id, stacks);
            case StatModifierKind.AdditivePercent: return AdjustModifierStacks(additivePercentageModifiers, ref additivePercentageModifierCount, id, stacks);
            case StatModifierKind.MultiplicativePercent: return AdjustModifierStacks(multiplicativePercentageModifiers, ref multiplicativePercentageModifierCount, id, stacks);
            case StatModifierKind.Override: return AdjustModifierStacks(overrideModifiers, ref overrideModifierCount, id, stacks);
            case StatModifierKind.Min: return AdjustModifierStacks(minModifiers, ref minModifierCount, id, stacks);
            case StatModifierKind.Max: return AdjustModifierStacks(maxModifiers, ref maxModifierCount, id, stacks);
        }
        
        return false;
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

        var changed = UpdateModifiers(flatModifiers, ref flatModifierCount, dt);
        changed |= UpdateModifiers(additivePercentageModifiers, ref additivePercentageModifierCount, dt);
        changed |= UpdateModifiers(multiplicativePercentageModifiers, ref multiplicativePercentageModifierCount, dt);
        changed |= UpdateModifiers(overrideModifiers, ref overrideModifierCount, dt);
        changed |= UpdateModifiers(minModifiers, ref minModifierCount, dt);
        changed |= UpdateModifiers(maxModifiers, ref maxModifierCount, dt);
        
        if (changed)
        {
            dirty = true;
            // Recalculate();
        }
    }

    /// <summary>
    /// Resets all modifiers to their initial state (removes all modifiers).
    /// </summary>
    public void ResetAllModifiers()
    {
        flatModifierCount = 0;
        additivePercentageModifierCount = 0;
        multiplicativePercentageModifierCount = 0;
        overrideModifierCount = 0;
        minModifierCount = 0;
        maxModifierCount = 0;
        dirty = true;
        // Recalculate();
    }

    //TODO: Add docs
    public void Reset(StatModifierKind kind)
    {
        switch (kind)
        {
            case StatModifierKind.Flat:
                flatModifierCount = 0;
                dirty = true;
                // Recalculate();
                break;
            case StatModifierKind.AdditivePercent:
                additivePercentageModifierCount = 0;
                dirty = true;
                // Recalculate();
                break;
            case StatModifierKind.MultiplicativePercent:
                multiplicativePercentageModifierCount = 0;
                dirty = true;
                // Recalculate();
                break;
            case StatModifierKind.Override:
                overrideModifierCount = 0;
                dirty = true;
                // Recalculate();
                break;
            case StatModifierKind.Min:
                minModifierCount = 0;
                dirty = true;
                // Recalculate();
                break;
            case StatModifierKind.Max:
                maxModifierCount = 0;
                dirty = true;
                // Recalculate();
                break;
        }
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
        var value = ApplyOverrideValue(overrideModifiers, overrideModifierCount, baseValue, out bool overrideFound);

        if (!overrideFound)
        {
            //apply flat modifiers
            value += SumModifiers(flatModifiers, flatModifierCount);
        
            //apply additive percentage modifiers
            value *= 1f + SumModifiers(additivePercentageModifiers, additivePercentageModifierCount);

            //apply multiplicative percentage modifiers
            value = ApplyMultiplicativePercentageModifiers(multiplicativePercentageModifiers, multiplicativePercentageModifierCount, value);
        }

        //apply bound modifiers (min, max)
        value = ApplyModifierBounds(minModifiers, minModifierCount, maxModifiers, maxModifierCount, value);

        currentValue = ApplyBounds(value);
    }

    #endregion
    
    #region Private Functions
    //Todo: Docs
    private bool AddModifier(ref Modifier[] modifiers, ref int count, int id, float value, float duration, int stacks, int maxStacks, int priority)
    {
        if (stacks <= 0 || maxStacks <= 0)
        {
            return false;
        }

        if (stacks > maxStacks)
        {
            stacks = maxStacks;
        }
        
        //look if modifier already exists, 
        //if so, set the existing modifier's value and duration to the new values
        for (var i = 0; i < count; i++)
        {
            if (modifiers[i].Id != id)
            {
                continue;
            }

            modifiers[i].Value = value;
            modifiers[i].Stacks = stacks;
            modifiers[i].MaxStacks = maxStacks;
            modifiers[i].Duration = duration;
            modifiers[i].Remaining = duration;
            modifiers[i].Priority = priority;
            
            dirty = true;
            // Recalculate();
            return true;
        }

        //if modifier doesn't exist, add it to the array'
        EnsureCapacity(ref modifiers, count + 1);

        modifiers[count] = new Modifier
        {
            Id = id,
            Value = value,
            Duration = duration,
            Remaining = duration,
            Stacks = stacks,
            MaxStacks = maxStacks,
            Priority = priority,
        };

        count++;
        
        dirty = true;
        // Recalculate();
        return true;
    }
    
    //Todo: Docs
    private bool RemoveModifier(Modifier[] modifiers, ref int count, int id)
    {
        //find modifier
        for (var i = 0; i < count; i++)
        {
            if (modifiers[i].Id != id)
            {
                continue;
            }
            
            RemoveAt(modifiers, ref count, i);
            
            dirty = true;
            
            // Recalculate();
            return true;
        }

        return false;
    }
    
    //Todo: Docs
    private bool AdjustModifierStacks(Modifier[] modifiers, ref int count, int id, int stacks)
    {
        if (stacks == 0) return false;
        
        for (var i = 0; i < count; i++)
        {
            if (modifiers[i].Id != id)
            {
                continue;
            }
            
            if (stacks > 0)
            {
                modifiers[i].Stacks += stacks;
                
                //clamp to max stacks if max stacks is greater than 0
                var maxStacks = modifiers[i].MaxStacks;
                if (maxStacks > 0 && modifiers[i].Stacks > maxStacks)
                {
                    modifiers[i].Stacks = maxStacks;
                }

                //reset Remaining time if duration is greater than 0
                if (modifiers[i].Duration > 0)
                {
                    modifiers[i].Remaining = modifiers[i].Duration;
                }
            }
            else
            {
                //removing stacks does not reset timer!
                modifiers[i].Stacks -= stacks;
                if (modifiers[i].Stacks <= 0)
                {
                    RemoveAt(modifiers, ref count, i);
                }
                
            }
            
            dirty = true;
            // Recalculate();
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

            //if has duration and timer runs out, modifier is completely removed (not just a single stack!)
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

    //Todo: Docs
    private static float ApplyModifierBounds(Modifier[] minModifiers, int minModifierCount, Modifier[] maxModifiers, int maxModifierCount, float value)
    {
        if (minModifierCount <= 0 && maxModifierCount <= 0) return value;
        
        int curPriority = int.MinValue;
        float minModifierValue = float.MaxValue;
        float maxModifierValue = float.MinValue;
        bool minModifierFound = false;
        bool maxModifierFound = false;
        
        for (int i = 0; i < minModifierCount; i++)
        {
            var min = minModifiers[i].Value;
            var priority = minModifiers[i].Priority;
            if (priority > curPriority)
            {
                minModifierValue = min;
                curPriority = priority;
                minModifierFound = true;
            }
        }
        
        curPriority = int.MinValue;
        for (int i = 0; i < maxModifierCount; i++)
        {
            var max = maxModifiers[i].Value;
            var priority = maxModifiers[i].Priority;
            if (priority > curPriority)
            {
                maxModifierValue = max;
                curPriority = priority;
                maxModifierFound = true;
            }
        }

        if (maxModifierFound && minModifierFound)
        {
            if (maxModifierValue < minModifierValue)
            {
                (minModifierValue, maxModifierValue) = (maxModifierValue, minModifierValue);
            }
        }

        if (minModifierFound && value < minModifierValue)
        {
            value = minModifierValue;
        }

        if (maxModifierFound && value > maxModifierValue)
        {
            value = maxModifierValue;
        }

        return value;
    }

    //Todo: Docs
    private static float ApplyMultiplicativePercentageModifiers(Modifier[] modifiers, int count, float value)
    {
        for (var i = 0; i < count; i++)
        {
            var multiplier = 1f + modifiers[i].Value;

            for (var stack = 0; stack < modifiers[i].Stacks; stack++)
            {
                value *= multiplier;
            }
        }

        return value;
    }

    //Todo: Docs
    private static float ApplyOverrideValue(Modifier[] modifiers, int count, float value, out bool overrideFound)
    {
        overrideFound = false;
        
        if (count <= 0) return value;
        
        float overrideValue = 0;
        int curPriority = int.MinValue;
        
        for (var i = 0; i < count; i++)
        {
            var priority = modifiers[i].Priority;
            if (priority > curPriority)
            {
                overrideValue = modifiers[i].Value;
                curPriority = priority;
                overrideFound = true;
            }
        }

        if(overrideFound) return overrideValue;
        
        return value;
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
    
    //Todo: Docs
    private static bool ContainsModifier(Modifier[] modifiers, int count, int id)
    {
        if (count <= 0) return false;
        if (count == 1) return modifiers[0].Id == id;
        else
        {
            for (int i = 0; i < count; i++)
            {
                if(modifiers[i].Id == id) return true;
            }

            return false;
        }
    }
    #endregion
}
