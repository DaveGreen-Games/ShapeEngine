using ShapeLib;

namespace ShapeTiming
{
    public class TimedFactors : ITimedValues
    {
        internal class TimedFloat
        {
            public float Timer = 0f;
            public float Value = 0f;

            public TimedFloat(float duration, float value)
            {
                this.Timer = duration;
                this.Value = value;
            }
        }

        private Dictionary<uint, TimedFloat> factors = new();
        public float Total { get; protected set; } = 1f;
        public uint Add(float factor, float duration = -1)
        {
            if (factor < 0) return 0;
            uint id = SID.NextID;
            if (factors.ContainsKey(id))
            {
                factors[id] = new(duration, factor);
            }
            else
            {
                factors.Add(id, new(duration, factor));
            }
            return id;
        }
        public bool Remove(uint id)
        {
            if (!factors.ContainsKey(id)) return false;
            factors.Remove(id);
            return true;
        }
        public void Clear() { factors.Clear(); }
        public void Update(float dt)
        {
            float accumualted = 1f;
            var keys = factors.Keys.ToList();
            for (int i = keys.Count - 1; i >= 0; i--)
            {
                var entry = factors[keys[i]];
                entry.Timer -= dt;
                if (entry.Timer <= 0f)
                {
                    factors.Remove(keys[i]);
                }
                else
                {
                    accumualted *= entry.Value;
                }
            }

            Total = accumualted;
        }

    }

}
