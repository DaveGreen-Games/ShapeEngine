namespace ShapeEngine.Random;

/// <summary>
/// Represents a list of values with weighted chances for random selection.
/// </summary>
/// <typeparam name="T">The type of values in the list.</typeparam>
/// <remarks>
/// The list is populated with each value repeated according to its specified amount,
/// representing its weight in the selection process.
/// Each time a random value is picked,
/// it is removed from the list, ensuring that every value is selected exactly as many times
/// as its weight before the list is refilled with the original entries.
/// This guarantees proportional selection: for example, if value A appears 10 times and value B appears 5 times,
/// then over 15 picks, value A will always be chosen 10 times and value B 5 times, in a randomized order.
/// </remarks>
public class ChanceList<T>
{
    private List<T> values = new();
    private (int amount, T value)[] entries;

    private Rng rng;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChanceList{T}"/> class with the specified entries and a random seed.
    /// </summary>
    /// <param name="entries">An array of tuples specifying the amount and value for each entry.</param>
    /// <remarks>
    /// The Amount specifies the number of times the value will be added to the list.
    /// Therefore, the amount represents the weight of the item.
    /// </remarks>
    public ChanceList(params (int amount, T value)[] entries)
    {
        this.rng = new Rng();
        this.entries = entries;
        Generate();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ChanceList{T}"/> class with the specified seed and entries.
    /// </summary>
    /// <param name="seed">The seed for the random number generator.</param>
    /// <param name="entries">An array of tuples specifying the amount and value for each entry.</param>
    /// <remarks>
    /// The Amount specifies the number of times the value will be added to the list.
    /// Therefore, the amount represents the weight of the item.
    /// </remarks>
    public ChanceList(int seed, params (int amount, T value)[] entries)
    {
        this.rng = new Rng(seed);
        this.entries = entries;
        Generate();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChanceList{T}"/> class with the specified entries and a random seed.
    /// </summary>
    /// <param name="entries">An enumerable of tuples specifying the amount and value for each entry.</param>
    /// <remarks>
    /// The Amount specifies the number of times the value will be added to the list.
    /// Therefore, the amount represents the weight of the item.
    /// </remarks>
    public ChanceList(IEnumerable<(int amount, T value)> entries)
    {
        this.rng = new Rng();
        this.entries = entries.ToArray();
        Generate();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ChanceList{T}"/> class with the specified seed and entries.
    /// </summary>
    /// <param name="seed">The seed for the random number generator.</param>
    /// <param name="entries">An enumerable of tuples specifying the amount and value for each entry.</param>
    /// <remarks>
    /// The Amount specifies the number of times the value will be added to the list.
    /// Therefore, the amount represents the weight of the item.
    /// </remarks>
    public ChanceList(int seed, IEnumerable<(int amount, T value)> entries)
    {
        this.rng = new Rng(seed);
        this.entries = entries.ToArray();
        Generate();
    }
    
    
    /// <summary>
    /// Clears and refills the list of values based on the original entries.
    /// </summary>
    public void Refill()
    {
        values.Clear();
        Generate();
    }

    /// <summary>
    /// Returns a random value matching the given predicate, or the default value if none match.
    /// </summary>
    /// <param name="match">Predicate to filter values.</param>
    /// <returns>A random value matching the predicate, or default if none found.</returns>
    public T? Next(Predicate<T> match)
    {
        if (values.Count <= 0) Generate();
        var filtered = values.FindAll(match);
        if (filtered.Count <= 0) return default;
        int index = rng.RandI(0, filtered.Count);
        T value = filtered[index];
        //filtered.RemoveAt(index);
        return value;
    }

    /// <summary>
    /// Returns a list of random values, with the number of values chosen randomly between min and max.
    /// </summary>
    /// <param name="min">Minimum number of values to return.</param>
    /// <param name="max">Maximum number of values to return.</param>
    /// <returns>A list of randomly selected values.</returns>
    public List<T> Next(int min, int max)
    {
        return Next(rng.RandI(min, max));
    }
    /// <summary>
    /// Returns a list of the specified number of random values.
    /// </summary>
    /// <param name="amount">The number of values to return.</param>
    /// <returns>A list of randomly selected values.</returns>
    public List<T> Next(int amount)
    {
        List<T> picked = new();
        for (var i = 0; i < amount; i++)
        {
            picked.Add(Next());
        }
        return picked;
    }
    /// <summary>
    /// Returns a single random value and removes it from the list.
    /// </summary>
    /// <returns>A randomly selected value.</returns>
    public T Next()
    {
        if (values.Count <= 0) Generate();

        int index = rng.RandI(0, values.Count);
        T value = values[index];
        values.RemoveAt(index);
        return value;
    }
    /// <summary>
    /// Generates the internal list of values based on the entries.
    /// </summary>
    private void Generate()
    {
        foreach (var entry in entries)
        {
            for (int i = 0; i < entry.amount; i++)
            {
                values.Add(entry.value);
            }
        }
    }
}