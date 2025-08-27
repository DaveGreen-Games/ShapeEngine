namespace ShapeEngine.Random;

/// <summary>
/// Represents a list of indices with weighted chances for selection, allowing random picking based on specified weights.
/// </summary>
/// <remarks>
/// The class uses a tuple <c>(int amount, int index)</c> to define how many times each index appears in the internal list, effectively setting its selection probability.
/// Useful for scenarios where you need to randomly select indices with custom likelihoods.
/// The ChanceList system guarantees that each index is picked according to its weight until all indices are exhausted, at which point the list can be refilled.
/// (An index with weight 10 out of a total weight of 100 will have been picked exactly 10 times after 100 picks.)
/// </remarks>
public class ChanceListIndices
{
    private readonly List<int> values = [];
    private readonly (int amount, int index)[] entries;
    private readonly Rng rng;

    /// <summary>
    /// Initializes a new instance of <see cref="ChanceListIndices"/> using a variable number of weighted index entries.
    /// </summary>
    /// <param name="entries">
    /// An array of tuples where <c>amount</c> specifies how many times <c>index</c> appears in the list.
    /// Higher <c>amount</c> increases the chance of <c>index</c> being picked.
    /// </param>
    /// <remarks>
    /// Uses a default random seed.
    /// </remarks>
    public ChanceListIndices(params (int amount, int index)[] entries)
    {
        this.rng = new Rng();
        this.entries = entries;
        Generate();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ChanceListIndices"/> with a specific random seed and weighted index entries.
    /// </summary>
    /// <param name="seed">The seed for the random number generator, ensuring reproducible results.</param>
    /// <param name="entries">
    /// An array of tuples where <c>amount</c> specifies how many times <c>index</c> appears in the list.
    /// </param>
    /// <remarks>
    /// Use this constructor for deterministic randomization.
    /// </remarks>
    public ChanceListIndices(int seed, params (int amount, int index)[] entries)
    {
        this.rng = new Rng(seed);
        this.entries = entries;
        Generate();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ChanceListIndices"/> using an enumerable collection of weighted index entries.
    /// </summary>
    /// <param name="entries">
    /// An enumerable of tuples where <c>amount</c> specifies how many times <c>index</c> appears in the list.
    /// </param>
    /// <remarks>
    /// Uses a default random seed.
    /// </remarks>
    public ChanceListIndices(IEnumerable<(int amount, int index)> entries)
    {
        this.rng = new Rng();
        this.entries = entries.ToArray();
        Generate();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ChanceListIndices"/> with a specific random seed and an enumerable collection of weighted index entries.
    /// </summary>
    /// <param name="seed">The seed for the random number generator, ensuring reproducible results.</param>
    /// <param name="entries">
    /// An enumerable of tuples where <c>amount</c> specifies how many times <c>index</c> appears in the list.
    /// </param>
    /// <remarks>
    /// Use this constructor for deterministic randomization.
    /// </remarks>
    public ChanceListIndices(int seed, IEnumerable<(int amount, int index)> entries)
    {
        this.rng = new Rng(seed);
        this.entries = entries.ToArray();
        Generate();
    }

    /// <summary>
    /// Refill the internal list of values based on the initial weighted entries.
    /// </summary>
    public void Refill()
    {
        values.Clear();
        Generate();
    }

    /// <summary>
    /// Gets the next index from the list that matches the given criteria.
    /// </summary>
    /// <param name="match">The criteria that the index must match.</param>
    /// <returns>
    /// The next matching index, or null if no match is found.
    /// </returns>
    public int? Next(Predicate<int> match)
    {
        if (values.Count <= 0) Generate();
        var filtered = values.FindAll(match);
        if (filtered.Count <= 0) return null;
        int index = rng.RandI(0, filtered.Count);
        int value = filtered[index];
        return value;
    }

    /// <summary>
    /// Gets a list of indices within the specified range.
    /// </summary>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (exclusive).</param>
    /// <returns>A list of indices within the specified range.</returns>
    public List<int> Next(int min, int max)
    {
        return Next(rng.RandI(min, max));
    }

    /// <summary>
    /// Picks a specified number of random indices from the list.
    /// </summary>
    /// <param name="amount">The number of indices to pick.</param>
    /// <returns>A list of picked indices.</returns>
    public List<int> Next(int amount)
    {
        List<int> picked = [];
        for (var i = 0; i < amount; i++)
        {
            picked.Add(Next());
        }
        return picked;
    }

    /// <summary>
    /// Picks the next random index from the list.
    /// </summary>
    /// <returns>The next random index.</returns>
    public int Next()
    {
        if (values.Count <= 0) Generate();

        int randIndex = rng.RandI(0, values.Count);
        int value = values[randIndex];
        values.RemoveAt(randIndex);
        return value;
    }

    private void Generate()
    {
        foreach (var entry in entries)
        {
            for (int i = 0; i < entry.amount; i++)
            {
                values.Add(entry.index);
            }
        }
    }
}