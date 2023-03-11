

using Raylib_CsLo;

namespace ShapeStats
{
    public interface IBuff
    {
        public string GetID();
        public bool IsEmpty();
        public bool DrawToUI();
        public (float totalBonus, float totalFlat) Get(params string[] tags);
        public void AddStack();
        public bool RemoveStack();
        public void Update(float dt);
        public void DrawUI(Rectangle r, Color barColor, Color bgColor, Color textColor);
    }

    public struct BuffValue
    {
        public float bonus = 0f;
        public float flat = 0f;
        public string id = "";
        public BuffValue(string id)
        {
            this.bonus = 0f;
            this.flat = 0f;
            this.id = id;
        }
        public BuffValue(string id, float bonus, float flat)
        {
            this.id = id;
            this.bonus = bonus;
            this.flat = flat;
        }
    }

    public class Buff : IBuff
    {
        private Dictionary<string, BuffValue> buffValues = new();
        private string id = "";
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
        public string GetID() { return id; }

        public Buff(string id, int maxStacks = -1, float duration = -1, params BuffValue[] buffValues)
        {
            this.id = id;
            this.MaxStacks = maxStacks;
            this.Duration = duration;
            foreach (var statChange in buffValues)
            {
                this.buffValues.Add(statChange.id, statChange);
            }
        }

        public (float totalBonus, float totalFlat) Get(params string[] tags)
        {
            float totalBonus = 0f;
            float totalFlat = 0f;
            if (IsEmpty()) return new(0f, 0f);

            foreach (var stat in buffValues.Values)
            {
                if (tags.Contains(stat.id))
                {
                    totalBonus += stat.bonus * CurStacks;
                    totalFlat += stat.flat * CurStacks;
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
