namespace ShapeEngine.Achievements;

/// <summary>
/// Represents a statistic used for tracking achievement progress.
/// </summary>
public class AchievementStat
{
    /// <summary>
    /// The API name of the stat.
    /// </summary>
    public string apiName;
    /// <summary>
    /// The display name of the stat.
    /// </summary>
    public string displayName;

    /// <summary>
    /// The current value of the stat.
    /// </summary>
    public int value;
    /// <summary>
    /// The default value of the stat.
    /// </summary>
    public int defaultValue;
    /// <summary>
    /// The maximum allowed value for the stat.
    /// </summary>
    public int maxValue;
    /// <summary>
    /// The minimum allowed value for the stat.
    /// </summary>
    public int minValue;


    /// <summary>
    /// Occurs when the stat value changes.
    /// </summary>
    public event Action<int, int>? OnValueChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementStat"/> class.
    /// </summary>
    /// <param name="apiName">The API name of the stat.</param>
    /// <param name="displayName">The display name of the stat.</param>
    /// <param name="defaultValue">The default value of the stat.</param>
    /// <param name="maxValue">The maximum allowed value.</param>
    /// <param name="minValue">The minimum allowed value.</param>
    public AchievementStat(string apiName, string displayName, int defaultValue, int maxValue = int.MaxValue, int minValue = int.MinValue)
    {
        this.apiName = apiName;
        this.displayName = displayName;
        this.maxValue = maxValue;
        this.minValue = minValue;
        this.defaultValue = defaultValue;
        if (this.defaultValue < minValue) this.defaultValue = minValue;
        else if (this.defaultValue > maxValue) this.defaultValue = maxValue;
        this.value = this.defaultValue;
    }

    /// <summary>
    /// Resets the stat to its default value.
    /// </summary>
    public void Reset() { SetStat(defaultValue); }

    /// <summary>
    /// Changes the stat value by a specified amount, clamping within min and max values.
    /// Triggers <see cref="OnValueChanged"/> if the value changes.
    /// </summary>
    /// <param name="change">The amount to change the stat by.</param>
    public void ChangeStat(int change)
    {
        if (change == 0) return;

        int oldValue = value;
        value += change;
        if (value < minValue) value = minValue;
        else if (value > maxValue) value = maxValue;

        if (oldValue != value) OnValueChanged?.Invoke(oldValue, value);
    }

    /// <summary>
    /// Sets the stat to a specific value.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    public void SetStat(int newValue) { ChangeStat(newValue - value); }

}