

namespace ShapeEngine.Random
{
    public class ChanceList<T>
    {
        private List<T> values = new();
        private (int amount, T value)[] entries;

        private Rng rng;

        public ChanceList(params (int amount, T value)[] entries)
        {
            this.rng = new Rng();
            this.entries = entries;
            Generate();
        }
        public ChanceList(int seed, params (int amount, T value)[] entries)
        {
            this.rng = new Rng(seed);
            this.entries = entries;
            Generate();
        }

        public void Refill()
        {
            values.Clear();
            Generate();
        }

        public T? Next(Predicate<T> match)
        {
            if (values.Count <= 0) Generate();
            List<T> filtered = values.FindAll(match);
            if (filtered.Count <= 0) return default;
            int index = rng.RandI(0, filtered.Count);
            T value = filtered[index];
            //filtered.RemoveAt(index);
            return value;
        }

        public List<T> Next(int min, int max)
        {
            return Next(rng.RandI(min, max));
        }
        public List<T> Next(int amount)
        {
            List<T> picked = new();
            for (int i = 0; i < amount; i++)
            {
                picked.Add(Next());
            }
            return picked;
        }
        public T Next()
        {
            if (values.Count <= 0) Generate();

            int index = rng.RandI(0, values.Count);
            T value = values[index];
            values.RemoveAt(index);
            return value;
        }
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

}
