

using Raylib_CsLo;

namespace ShapeStats
{
    public interface IBuff
    {
        public int GetID();
        public bool IsEmpty();
        public bool DrawToUI();
        public (float totalBonus, float totalFlat) Get(params int[] tags);
        public void AddStack();
        public bool RemoveStack();
        public void Update(float dt);
        public void DrawUI(Rectangle r, Color barColor, Color bgColor, Color textColor);
    }

    public struct BuffValue
    {
        public float bonus = 0f;
        public float flat = 0f;
        public int id = -1;
        public BuffValue(int id)
        {
            this.bonus = 0f;
            this.flat = 0f;
            this.id = id;
        }
        public BuffValue(int id, float bonus, float flat)
        {
            this.id = id;
            this.bonus = bonus;
            this.flat = flat;
        }
    }

    public class Buff : IBuff
    {
        private Dictionary<int, BuffValue> buffValues = new();
        private int id = -1;
        public int MaxStacks { get; private set; } = -1;
        public int CurStacks { get; private set; } = 1;
        public float Duration { get; private set; } = -1f;
        public float Timer { get; private set; } = 0f;
        public float TimerF
        {
            get
            {
                if (Duration <= 0f) return 0f;
                return 1f - (Timer / Duration);
            }
        }
        public float StackF
        {
            get
            {
                if (MaxStacks <= 0) return 0f;
                return (float)CurStacks / (float)MaxStacks;
            }
        }
        public string Name { get; set; } = "";
        public string Abbreviation { get; set; } = "";
        public bool clearAllStacksOnDurationEnd = false;
        public bool IsEmpty() { return CurStacks <= 0; }
        public bool DrawToUI() { return Abbreviation != ""; }
        public int GetID() { return id; }

        public Buff(int id, int maxStacks = -1, float duration = -1, params BuffValue[] buffValues)
        {
            this.id = id;
            this.MaxStacks = maxStacks;
            this.Duration = duration;
            foreach (var buffValue in buffValues)
            {
                this.buffValues.Add(buffValue.id, buffValue);
            }
        }

        public (float totalBonus, float totalFlat) Get(params int[] tags)
        {
            float totalBonus = 0f;
            float totalFlat = 0f;
            if (IsEmpty()) return new(0f, 0f);

            foreach (var buffValue in buffValues.Values)
            {
                if (tags.Contains(buffValue.id))
                {
                    totalBonus += buffValue.bonus * CurStacks;
                    totalFlat += buffValue.flat * CurStacks;
                }
            }
            return (totalBonus, totalFlat);
        }
        public void AddStack()
        {
            if (CurStacks < MaxStacks || MaxStacks < 0) CurStacks += 1;
            if (Duration > 0) Timer = Duration;
        }
        public bool RemoveStack()
        {
            CurStacks -= 1;
            if (CurStacks <= 0) return true;

            return false;
        }
        public void Update(float dt)
        {
            if (IsEmpty()) return;
            if (Duration > 0f)
            {
                Timer -= dt;
                if (Timer <= 0f)
                {
                    if (clearAllStacksOnDurationEnd) CurStacks = 0;
                    else
                    {
                        CurStacks -= 1;
                        if (CurStacks > 0)
                        {
                            Timer = Duration;
                        }
                    }
                }
            }
        }

        public void DrawUI(Rectangle r, Color barColor, Color bgColor, Color textColor) { }
    }

}
