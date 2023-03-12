
namespace ShapeStats
{
    public interface IStat
    {
        public int[] Tags { get; set; }
        public void UpdateCur(float totalBonus, float totalFlat);
    }

    public class Stat : IStat
    {
        public event Action<Stat, float>? CurChanged;
        public float Base { get; private set; } = 0f;
        public float Cur { get; private set; } = 0f;
        public float F
        {
            get
            {
                if (Base <= 0f) return 0f;
                return Cur / Base;
            }
        }
        public int[] Tags { get; set; }
        public Stat(float baseValue, params int[] tags) { Base = baseValue; Cur = baseValue; Tags = tags; }
        public void SetBase(float value)
        {
            Base = value;
            Cur = value;
        }
        public void UpdateCur(float totalBonuses, float totalFlats)
        {
            float old = Cur;
            if (totalBonuses >= 0f)
            {
                Cur = (Base + totalFlats) * (1f + totalBonuses);
            }
            else
            {
                Cur = (Base + totalFlats) / (1f + MathF.Abs(totalBonuses));
            }

            if (Cur != old) CurChanged?.Invoke(this, old);
        }

    }
    
    public class StatInt : IStat
    {
        public event Action<StatInt, int>? CurChanged;
        public int Base { get; private set; } = 0;
        public int Cur { get; private set; } = 0;
        public float F
        {
            get
            {
                if (Base <= 0f) return 0f;
                return (float)Cur / (float)Base;
            }
        }
        public int[] Tags { get; set; }

        public StatInt(int baseValue, params int[] tags) { Base = baseValue; Cur = baseValue; this.Tags = tags; }
        public void SetBase(int value)
        {
            Base = value;
            Cur = value;
        }
        public void UpdateCur(float totalBonuses, float totalFlats)
        {
            int old = Cur;
            if (totalBonuses >= 0f)
            {
                float v = ((float)Base + totalFlats) * totalBonuses;
                Cur = (int)MathF.Ceiling(v);
            }
            else
            {
                float v = ((float)Base + totalFlats) / (1f + MathF.Abs(totalBonuses));
                Cur = (int)MathF.Ceiling(v);
            }

            if (Cur != old) CurChanged?.Invoke(this, old);
        }

    }

}
