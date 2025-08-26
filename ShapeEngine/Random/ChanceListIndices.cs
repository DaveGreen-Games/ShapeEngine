namespace ShapeEngine.Random;

public class ChanceListIndices
{
    private readonly List<int> values = [];
    private readonly (int amount, int index)[] entries;
    private readonly Rng rng;

    public ChanceListIndices(params (int amount, int index)[] entries)
    {
        this.rng = new Rng();
        this.entries = entries;
        Generate();
    }

    public ChanceListIndices(int seed, params (int amount, int index)[] entries)
    {
        this.rng = new Rng(seed);
        this.entries = entries;
        Generate();
    }

    public ChanceListIndices(IEnumerable<(int amount, int index)> entries)
    {
        this.rng = new Rng();
        this.entries = entries.ToArray();
        Generate();
    }

    public ChanceListIndices(int seed, IEnumerable<(int amount, int index)> entries)
    {
        this.rng = new Rng(seed);
        this.entries = entries.ToArray();
        Generate();
    }

    public void Refill()
    {
        values.Clear();
        Generate();
    }

    public int? Next(Predicate<int> match)
    {
        if (values.Count <= 0) Generate();
        var filtered = values.FindAll(match);
        if (filtered.Count <= 0) return null;
        int index = rng.RandI(0, filtered.Count);
        int value = filtered[index];
        return value;
    }

    public List<int> Next(int min, int max)
    {
        return Next(rng.RandI(min, max));
    }

    public List<int> Next(int amount)
    {
        List<int> picked = [];
        for (var i = 0; i < amount; i++)
        {
            picked.Add(Next());
        }
        return picked;
    }

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