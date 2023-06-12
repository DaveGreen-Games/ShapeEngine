using ShapeEngine;

namespace Demo
{
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
            if (key != "")
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
