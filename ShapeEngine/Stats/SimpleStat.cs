namespace ShapeEngine.Stats;

//TODO: Review and make sure everything is in order.
public class SimpleStat
{
    public const float PermanentDuration = -1f;

    private struct Modifier
    {
        public int Id;
        public float Value;
        public float Duration;
        public float Remaining;
        public int Stacks;
    }

    private Modifier[] flatBonuses;
    private Modifier[] additiveMultipliers;
    private Modifier[] multiplicativeMultipliers;

    private int flatBonusCount;
    private int additiveMultiplierCount;
    private int multiplicativeMultiplierCount;

    private float baseValue;
    private float currentValue;
    private float minValue;
    private float maxValue;
    private bool hasMinValue;
    private bool hasMaxValue;

    public string Name { get; set; }
    public string Abbreviation { get; set; }
    public string Description { get; set; }

    public float BaseValue
    {
        get => baseValue;
        set
        {
            baseValue = value;
            Recalculate();
        }
    }

    public float CurrentValue => currentValue;

    public bool HasMinValue => hasMinValue;
    public bool HasMaxValue => hasMaxValue;
    public float MinValue => minValue;
    public float MaxValue => maxValue;

    public int FlatBonusCount => flatBonusCount;
    public int AdditiveMultiplierCount => additiveMultiplierCount;
    public int MultiplicativeMultiplierCount => multiplicativeMultiplierCount;

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

    public void ClearMin()
    {
        hasMinValue = false;
        Recalculate();
    }

    public void ClearMax()
    {
        hasMaxValue = false;
        Recalculate();
    }

    public void ClearBounds()
    {
        hasMinValue = false;
        hasMaxValue = false;
        Recalculate();
    }

    public void AddFlat(float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref flatBonuses, ref flatBonusCount, 0, value, duration, stacks, false);
    }

    public void AddFlat(int id, float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref flatBonuses, ref flatBonusCount, id, value, duration, stacks, true);
    }

    public void AddAdditiveMultiplier(float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref additiveMultipliers, ref additiveMultiplierCount, 0, value, duration, stacks, false);
    }

    public void AddAdditiveMultiplier(int id, float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref additiveMultipliers, ref additiveMultiplierCount, id, value, duration, stacks, true);
    }

    public void AddMultiplicativeMultiplier(float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref multiplicativeMultipliers, ref multiplicativeMultiplierCount, 0, value, duration, stacks, false);
    }

    public void AddMultiplicativeMultiplier(int id, float value, float duration = PermanentDuration, int stacks = 1)
    {
        AddModifier(ref multiplicativeMultipliers, ref multiplicativeMultiplierCount, id, value, duration, stacks, true);
    }

    public bool RemoveFlat(int id)
    {
        return RemoveModifier(flatBonuses, ref flatBonusCount, id);
    }

    public bool RemoveAdditiveMultiplier(int id)
    {
        return RemoveModifier(additiveMultipliers, ref additiveMultiplierCount, id);
    }

    public bool RemoveMultiplicativeMultiplier(int id)
    {
        return RemoveModifier(multiplicativeMultipliers, ref multiplicativeMultiplierCount, id);
    }

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

    public void Reset()
    {
        flatBonusCount = 0;
        additiveMultiplierCount = 0;
        multiplicativeMultiplierCount = 0;
        Recalculate();
    }

    public SimpleStat Copy()
    {
        return new SimpleStat(this);
    }

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

    private static void RemoveAt(Modifier[] modifiers, ref int count, int index)
    {
        count--;

        if (index != count)
        {
            modifiers[index] = modifiers[count];
        }

        modifiers[count] = default;
    }

    private static float SumModifiers(Modifier[] modifiers, int count)
    {
        var sum = 0f;

        for (var i = 0; i < count; i++)
        {
            sum += modifiers[i].Value * modifiers[i].Stacks;
        }

        return sum;
    }

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

    private static void CopyModifiers(Modifier[] source, Modifier[] destination, int count)
    {
        for (var i = 0; i < count; i++)
        {
            destination[i] = source[i];
        }
    }
}
