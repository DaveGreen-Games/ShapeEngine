namespace ShapeEngine.Stats;

/// <summary>
/// Contains compact example scenarios for the stat system.
/// </summary>
public static class StatExamples
{
    /// <summary>
    /// Demonstrates an incremental game production stat with flat upgrades and multiplicative prestige.
    /// </summary>
    /// <returns>The calculated production value.</returns>
    public static float IncrementalProduction()
    {
        StatId production = 1;
        var stats = new StatSet();
        stats.AddStat(new Stat(production, 10f, "Production"));
        stats.AddSource(new StatModifierSource(100, StatModifierSourceType.Upgrade, "Conveyor Upgrade", modifiers: StatModifier.Flat(production, 5f)));
        stats.AddSource(new StatModifierSource(101, StatModifierSourceType.Passive, "Prestige", modifiers: StatModifier.MultiplicativePercent(production, 1f)));
        return stats.GetValue(production);
    }

    /// <summary>
    /// Demonstrates a survivor movement stat with a timed slow and temporary haste.
    /// </summary>
    /// <returns>The movement value after both effects are applied.</returns>
    public static float SurvivorMoveSpeed()
    {
        StatId moveSpeed = 2;
        var stats = new StatSet();
        stats.AddStat(new Stat(moveSpeed, 100f, "Move Speed", minValue: 0f));
        stats.AddSource(new TimedStatModifierSource(200, StatModifierSourceType.Debuff, 4f, "Slow", modifiers: StatModifier.AdditivePercent(moveSpeed, -0.3f)));
        stats.AddSource(new TimedStatModifierSource(201, StatModifierSourceType.Buff, 2f, "Haste", modifiers: StatModifier.MultiplicativePercent(moveSpeed, 0.2f)));
        return stats.GetValue(moveSpeed);
    }

    /// <summary>
    /// Demonstrates ARPG-style loot damage with flat, additive, and multiplicative layers.
    /// </summary>
    /// <returns>The calculated damage value.</returns>
    public static float LootDamage()
    {
        StatId damage = 3;
        var stats = new StatSet();
        stats.AddStat(new Stat(damage, 20f, "Damage", minValue: 0f));
        stats.AddSource(new StatModifierSource(300, StatModifierSourceType.Equipment, "Sword Affix", modifiers: StatModifier.Flat(damage, 12f)));
        stats.AddSource(new StatModifierSource(301, StatModifierSourceType.Equipment, "Damage Roll", modifiers: StatModifier.AdditivePercent(damage, 0.5f)));
        stats.AddSource(new StatModifierSource(302, StatModifierSourceType.Equipment, "Unique Bonus", modifiers: StatModifier.MultiplicativePercent(damage, 0.25f)));
        return stats.GetValue(damage);
    }

    /// <summary>
    /// Demonstrates a stackable timed buff with max stacks and duration refresh on reapply.
    /// </summary>
    /// <returns>The calculated attack speed value at max stacks.</returns>
    public static float StackableHaste()
    {
        StatId attackSpeed = 4;
        var stats = new StatSet();
        stats.AddStat(new Stat(attackSpeed, 1f, "Attack Speed", minValue: 0f));
        stats.AddSource(new StackableStatModifierSource(
            400,
            StatModifierSourceType.Buff,
            3f,
            3,
            name: "Haste",
            modifiers: StatModifier.AdditivePercent(attackSpeed, 0.1f)));
        stats.AddStacks(400, 4);
        return stats.GetValue(attackSpeed);
    }

    /// <summary>
    /// Runs small example-driven checks that cover the intended calculation and source behavior.
    /// </summary>
    /// <returns>True if all checks pass.</returns>
    public static bool Verify()
    {
        const float tolerance = 0.0001f;

        static bool Close(float a, float b) => MathF.Abs(a - b) <= tolerance;

        StatId test = 10;
        var stats = new StatSet();
        stats.AddStat(new Stat(test, 10f, "Test", minValue: 0f, maxValue: 100f));
        stats.AddSource(new StatModifierSource(1, StatModifierSourceType.Upgrade, modifiers: StatModifier.Flat(test, 5f)));
        stats.AddSource(new StatModifierSource(2, StatModifierSourceType.Passive, modifiers: StatModifier.AdditivePercent(test, 1f)));
        stats.AddSource(new StatModifierSource(3, StatModifierSourceType.Passive, modifiers: StatModifier.MultiplicativePercent(test, 0.5f)));
        if (!Close(stats.GetValue(test), 45f)) return false;

        stats.AddSource(new StatModifierSource(4, StatModifierSourceType.Custom, modifiers: StatModifier.Max(test, 40f)));
        if (!Close(stats.GetValue(test), 40f)) return false;

        stats.AddSource(new StatModifierSource(5, StatModifierSourceType.Custom, modifiers: StatModifier.Override(test, 7f, priority: 1)));
        if (!Close(stats.GetValue(test), 7f)) return false;

        stats.RemoveSource(5);
        if (!Close(stats.GetValue(test), 40f)) return false;

        stats.AddSource(new TimedStatModifierSource(6, StatModifierSourceType.Buff, 1f, modifiers: StatModifier.Flat(test, 10f)));
        stats.Update(2f);
        if (stats.TryGetSource(6, out _)) return false;

        stats.AddSource(new StackableStatModifierSource(7, StatModifierSourceType.Buff, 5f, 3, modifiers: StatModifier.Flat(test, 2f)));
        if (stats.AddStacks(7, 10) != 2) return false;
        if (!stats.TryGetSource(7, out var stackable) || stackable.Stacks != 3) return false;
        var remaining = stackable.RemainingDuration;
        stats.Update(1f);
        stats.AddSource(new StackableStatModifierSource(7, StatModifierSourceType.Buff, 5f, 3, modifiers: StatModifier.Flat(test, 2f)));
        if (!stats.TryGetSource(7, out stackable) || stackable.RemainingDuration <= remaining - 1f) return false;

        return true;
    }
}
