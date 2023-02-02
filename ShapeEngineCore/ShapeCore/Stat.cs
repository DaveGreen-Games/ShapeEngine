using System.Linq;

namespace ShapeCore
{
    internal class StatValue
    {
        public float value = 0f;
        float duration = 0f;
        float timer = 0f;
        public StatValue(float value, float duration = -1f)
        {
            this.value = value;
            Start(duration);
        }
        public float GetRemaining() { return timer; }
        public float GetDuration() { return duration; }
        //public float Get() { return value; }
        public void Add(float amount) { value += amount; }
        public void Start(float duration) { this.duration = duration; timer = duration; }
        public void Restart() { if (duration > 0f) timer = duration; }
        public bool Update(float dt)
        {
            if (timer == 0f) return true;
            else if (timer > 0f)
            {
                timer -= dt;
                if (timer <= 0f) { timer = 0f; return true; }
            }

            return false;
        }
    }
    public class StatSimple
    {
        private float baseValue = 0f;
        private float flatTotal = 0f;
        private float bonusTotal = 1f;
        private List<StatValue> timedBonuses = new();
        private List<StatValue> timedFlats = new();

        public StatSimple(float baseValue) { this.baseValue = baseValue; }

        public void SetBase(float value) { baseValue = value; }
        public float Base { get { return baseValue; } }
        public float Cur
        {
            get { return (baseValue + flatTotal) * bonusTotal; }
            private set { }
        }

        public bool Update(float dt)
        {
            bool changed = false;
            if (timedBonuses.Count > 0)
            {
                for (int i = timedBonuses.Count - 1; i >= 0; i--)
                {
                    if (timedBonuses[i].Update(dt))
                    {
                        bonusTotal -= timedBonuses[i].value;
                        timedBonuses.RemoveAt(i);
                        if (!changed) changed = true;
                    }
                }
            }

            if (timedFlats.Count > 0)
            {
                for (int i = timedFlats.Count - 1; i >= 0; i--)
                {
                    if (timedFlats[i].Update(dt))
                    {
                        flatTotal -= timedFlats[i].value;
                        timedFlats.RemoveAt(i);
                        if (!changed) changed = true;
                    }
                }
            }
            return changed;
        }

        public float GetF()
        {
            if (Base == 0f) return 0f;
            return Cur / baseValue;
        }
        public void Set(StatSimple other)
        {
            baseValue = other.baseValue;
            SetBonuses(other);
        }
        public void SetBonuses(StatSimple other)
        {
            flatTotal += other.flatTotal;
            bonusTotal += other.bonusTotal - 1f;

            foreach (var bonus in other.timedBonuses)
            {
                AddBonus(bonus.value, bonus.GetRemaining());
            }
            foreach (var flat in other.timedFlats)
            {
                AddFlat(flat.value, flat.GetRemaining());
            }
        }

        public void AddBonuses(params float[] bonuses)
        {
            foreach (var bonus in bonuses)
            {
                bonusTotal += bonus;
            }
        }
        public void AddFlats(params float[] flats)
        {
            foreach (var flat in flats)
            {
                flatTotal += flat;
            }
        }
        public void AddBonus(float bonus, float duration = -1f)
        {
            bonusTotal += bonus;
            if (duration > 0f)
            {
                timedBonuses.Add(new(bonus, duration));
            }
        }
        public void AddFlat(float flat, float duration = -1f)
        {
            flatTotal += flat;
            if (duration > 0f)
            {
                timedFlats.Add(new(flat, duration));
            }
        }
        public void RemoveBonus(float bonus)
        {
            bonusTotal -= bonus;
        }
        public void RemoveFlat(float flat)
        {
            flatTotal -= flat;
        }
        public void ResetBonus() { bonusTotal = 1f; timedBonuses.Clear(); }
        public void ResetFlat() { flatTotal = 0f; timedFlats.Clear(); }
        public void Reset() { ResetBonus(); ResetFlat(); }
    }
    public class Stat
    {
        float baseValue = 0f;
        float total = 0f;

        Dictionary<string, StatValue> bonuses = new();
        Dictionary<string, StatValue> flatBonues = new();

        public Stat(float baseValue)
        {
            this.baseValue = baseValue;
        }

        public float GetBase() { return baseValue; }
        public float GetCur() { return total; }

        public void Update(float dt)
        {
            if (bonuses.Count > 0)
            {
                var removeBonuses = bonuses.Where(kvp => kvp.Value.Update(dt));
                foreach (var kvp in removeBonuses)
                {
                    bonuses.Remove(kvp.Key);
                }
            }

            if (flatBonues.Count > 0)
            {
                var removeFlats = flatBonues.Where(kvp => kvp.Value.Update(dt));
                foreach (var kvp in removeFlats)
                {
                    flatBonues.Remove(kvp.Key);
                }
            }
        }
        public void AddBonuses(params (string name, float value)[] bonuses)
        {
            foreach (var bonus in bonuses)
            {
                if (this.bonuses.ContainsKey(bonus.name)) this.bonuses[bonus.name] = new(bonus.value, -1f);// this.bonuses[bonus.name].Add(bonus.value);
                else this.bonuses.Add(bonus.name, new(bonus.value, -1f));
            }
            UpdateTotal();
        }
        public void AddFlats(params (string name, float value)[] flats)
        {
            foreach (var flat in flats)
            {
                if (flatBonues.ContainsKey(flat.name)) flatBonues[flat.name] = new(flat.value, -1f);// flatBonues[flat.name].Add(flat.value);
                else flatBonues.Add(flat.name, new(flat.value, -1f));
            }
            UpdateTotal();
        }
        public void AddBonus(string name, float bonus, float duration = -1f)
        {
            if (bonuses.ContainsKey(name)) { bonuses[name] = new(bonus, duration); }// bonuses[name].Add(bonus); }
            else { bonuses.Add(name, new(bonus, duration)); }

            UpdateTotal();
        }
        public void AddFlat(string name, float flat, float duration = -1f)
        {
            if (flatBonues.ContainsKey(name)) flatBonues[name] = new(flat, duration); // flatBonues[name] += flat;
            else flatBonues.Add(name, new(flat, duration));

            UpdateTotal();
        }
        public void RemoveBonuses(params string[] names)
        {
            foreach (var name in names)
            {
                if (!bonuses.ContainsKey(name)) continue;
                bonuses.Remove(name);
            }
            UpdateTotal();
        }
        public void RemoveFlats(params string[] names)
        {
            foreach (var name in names)
            {
                if (!flatBonues.ContainsKey(name)) continue;
                flatBonues.Remove(name);
            }

            UpdateTotal();
        }
        public void RemoveBonus(string name)
        {
            if (!bonuses.ContainsKey(name)) return;
            bonuses.Remove(name);

            UpdateTotal();
        }
        public void RemoveFlat(string name)
        {
            if (!flatBonues.ContainsKey(name)) return;
            flatBonues.Remove(name);

            UpdateTotal();
        }
        public void ClearBonuses()
        {
            bonuses.Clear();
            UpdateTotal();
        }
        public void ClearFlat()
        {
            flatBonues.Clear();
            UpdateTotal();
        }
        public void Clear()
        {
            bonuses.Clear();
            flatBonues.Clear();
            total = baseValue;
        }

        private void UpdateTotal()
        {
            total = baseValue;
            foreach (var flat in flatBonues.Values)
            {
                total += flat.value;
            }

            float totalBonus = 1f;
            foreach (var bonus in bonuses.Values)
            {
                totalBonus += bonus.value;
            }

            total *= totalBonus;
        }
    }


    public class StatHandler
    {
        private Dictionary<string, StatSimple> stats = new();

        public delegate void StatChanged(string statName);
        public event StatChanged? OnStatChanged;

        public StatHandler() { }
        public StatHandler(params (string name, float value)[] add)
        {
            AddStats(add);
        }


        public void Update(float dt)
        {
            foreach (var stat in stats)
            {
                if (stat.Value.Update(dt))
                {
                    FireStatChanged(stat.Key);
                }
            }
        }

        public Dictionary<string, StatSimple> GetAllStats() { return stats; }
        public Dictionary<string, StatSimple> GetAllStats(params string[] filters)
        {
            return stats.Where(kvp => filters.Contains(kvp.Key)).ToDictionary(k => k.Key, e => e.Value);
        }
        public bool Has(string statName) { return stats.ContainsKey(statName); }
        public void Set(params (string name, StatSimple stat)[] stats)
        {
            foreach (var item in stats)
            {
                Set(item.name, item.stat);
            }
        }
        public void Set(Dictionary<string, StatSimple> stats)
        {
            foreach (var item in stats)
            {
                Set(item.Key, item.Value);
            }
        }
        public void Set(string name, StatSimple stat)
        {
            if (!Has(name)) return;
            stats[name].Set(stat);
        }
        public void SetBonuses(params (string name, StatSimple stat)[] stats)
        {
            foreach (var item in stats)
            {
                SetBonuses(item.name, item.stat);
            }
        }
        public void SetBonuses(string name, StatSimple stat)
        {
            if (!Has(name)) return;
            stats[name].SetBonuses(stat);
        }
        public void SetBonuses(Dictionary<string, StatSimple> stats)
        {
            foreach (var item in stats)
            {
                SetBonuses(item.Key, item.Value);
            }
        }

        public float Get(string name)
        {
            if (!Has(name)) return 0f;
            return stats[name].Cur;
        }
        public StatSimple? GetStat(string name)
        {
            if (!Has(name)) return null;
            return stats[name];
        }

        public void SetBase(string name, float value)
        {
            if (!Has(name)) return;
            stats[name].SetBase(value);
        }
        public void SetStat(string name, float value)
        {
            if (!Has(name)) return;
            stats[name].SetBase(value);
            FireStatChanged(name);
        }
        public void AddStats(params (string name, float value)[] add)
        {
            foreach (var stat in add)
            {
                AddStat(stat.name, stat.value);
            }
        }
        public void AddStat(string name, float value)
        {
            if (Has(name)) stats[name].SetBase(value);
            else stats.Add(name, new(value));
        }
        public void AddStat(string name, StatSimple stat)
        {
            if (Has(name)) stats[name] = stat;
            else stats.Add(name, stat);
        }
        public void AddStats(params (string name, StatSimple stat)[] add)
        {
            foreach (var param in add)
            {
                AddStat(param.name, param.stat);
            }
        }
        public void RemoveStat(string name) { stats.Remove(name); }
        public void RemoveStat(StatSimple stat)
        {
            string key = GetStatKey(stat);
            if(key != "")
            {
                stats.Remove(key);
            }
        }
        public string GetStatKey(StatSimple stat)
        {
            foreach (var kvp in stats)
            {
                if (kvp.Value == stat)
                {
                    return kvp.Key;
                }
            }
            return "";
        }
        public void AddBonuses(string statName, params float[] bonuses)
        {
            if (!Has(statName)) return;
            stats[statName].AddBonuses(bonuses);
            FireStatChanged(statName);
        }
        public void AddBonuses(params (string name, float bonus, float duration)[] bonuses)
        {
            foreach (var item in bonuses)
            {
                AddBonus(item.name, item.bonus, item.duration);
            }
        }
        public void AddBonuses(params (string name, float bonus)[] bonuses)
        {
            foreach (var item in bonuses)
            {
                AddBonus(item.name, item.bonus);
            }
        }
        public void AddBonus(string statName, float value, float duration = -1f)
        {
            if (!Has(statName)) return;
            stats[statName].AddBonus(value, duration);
            FireStatChanged(statName);
        }
        public void AddFlats(string statName, params float[] flats)
        {
            if (!Has(statName)) return;
            stats[statName].AddFlats(flats);
            FireStatChanged(statName);
        }
        public void AddFlats(params (string name, float flat, float duration)[] flats)
        {
            foreach (var item in flats)
            {
                AddFlat(item.name, item.flat, item.duration);
            }
        }
        public void AddFlats(params (string name, float flat)[] flats)
        {
            foreach (var item in flats)
            {
                AddFlat(item.name, item.flat);
            }
        }
        public void AddFlat(string statName, float value, float duration = -1f)
        {
            if (!Has(statName)) return;
            stats[statName].AddFlat(value, duration);
            FireStatChanged(statName);
        }
        public void RemoveBonus(string statName, float value)
        {
            if (!Has(statName)) return;
            stats[statName].RemoveBonus(value);
            FireStatChanged(statName);
        }
        public void RemoveFlat(string statName, float value)
        {
            if (!Has(statName)) return;
            stats[statName].RemoveFlat(value);
            FireStatChanged(statName);
        }

        public void Reset()
        {
            foreach (var stat in stats.Values)
            {
                stat.Reset();
            }
        }
        private void FireStatChanged(string statName) { OnStatChanged?.Invoke(statName); }
    }
}
