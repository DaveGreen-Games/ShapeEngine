

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

    public class StatContainer
    {
        public Dictionary<string, StatBonuses> stats = new();
        //public event Action<StatBonuses>? StatChanged;
        
        public float ApplyBonuses(float baseValue, params string[] statIDs)
        {
            float bonusTotal = 1f;
            float flatTotal = 0f;

            foreach (var id in statIDs)
            {
                if (stats.ContainsKey(id))
                {
                    var stat = stats[id];
                    bonusTotal += stat.BonusTotal;
                    flatTotal += stat.FlatTotal;
                }
            }

            return (baseValue + flatTotal) * bonusTotal;
        }
        public int ApplyBonuses(int baseValue, params string[] statIDs)
        {
            float bonusTotal = 1f;
            float flatTotal = 0f;

            foreach (var id in statIDs)
            {
                if (stats.ContainsKey(id))
                {
                    var stat = stats[id];
                    bonusTotal += stat.BonusTotal;
                    flatTotal += stat.FlatTotal;
                }
            }
            float v = ((float)baseValue + flatTotal) * bonusTotal;
            return (int)MathF.Ceiling(v);
        }
        
        public void ApplyBonusesToStat(StatF baseStat, params string[] statIDs)
        {
            float bonusTotal = 1f;
            float flatTotal = 0f;

            foreach (var id in statIDs)
            {
                if (stats.ContainsKey(id))
                {
                    var stat = stats[id];
                    bonusTotal += stat.BonusTotal;
                    flatTotal += stat.FlatTotal;
                }
            }
            baseStat.UpdateCur(bonusTotal, flatTotal);
        }
        public void ApplyBonusesToStat(StatI baseStat, params string[] statIDs)
        {
            float bonusTotal = 1f;
            float flatTotal = 0f;

            foreach (var id in statIDs)
            {
                if (stats.ContainsKey(id))
                {
                    var stat = stats[id];
                    bonusTotal += stat.BonusTotal;
                    flatTotal += stat.FlatTotal;
                }
            }
            baseStat.UpdateCur(bonusTotal, flatTotal);
        }

        public void ApplyBonusesToStats(params StatF[] stats)
        {
            foreach (var stat in stats)
            {
                ApplyBonusesToStat(stat, stat.StatBonusIDS);
            }
        }
        public void ApplyBonusesToStats(params StatI[] stats)
        {
            foreach (var stat in stats)
            {
                ApplyBonusesToStat(stat, stat.StatBonusIDS);
            }
        }
        
        public void Update(float dt)
        {
            foreach (var stat in stats.Values)
            {
                stat.Update(dt);
            }
        }

        public void Add(StatBonuses stat)
        {
            if (!stats.ContainsKey(stat.ID))
            {
                stats.Add(stat.ID, stat);
                //stat.Changed += OnStatChanged;
            }
        }
        public void Remove(StatBonuses stat)
        {
            if (stats.ContainsKey(stat.ID))
            {
                //stat.Changed -= OnStatChanged;
                stats.Remove(stat.ID);
            }
        }

        public void Add(params string[] ids)
        {
            foreach (var id in ids)
            {
                Add(id);
            }
        }
        public void Add(string id)
        {
            if (!stats.ContainsKey(id))
            {
                var stat = new StatBonuses(id);
                //stat.Changed += OnStatChanged;
                stats.Add(id, stat);
            }
        }
        public void Remove(string id)
        {
            if (stats.ContainsKey(id))
            {
                //stats[id].Changed -= OnStatChanged;
                stats.Remove(id);
            }
        }

        public void ChangeStatBonus(string id, float bonus, float duration = -1, bool remove = false)
        {
            if (stats.ContainsKey(id))
            {
                if (remove)
                {
                    stats[id].RemoveBonus(bonus);
                }
                else stats[id].AddBonus(bonus, duration);
            }
        }
        public void ChangeStatFlat(string id, float flat, float duration = -1, bool remove = false)
        {
            if (stats.ContainsKey(id))
            {
                if (remove)
                {
                    stats[id].RemoveFlat(flat);
                }
                else stats[id].AddFlat(flat, duration);
            }
        }

        //private void OnStatChanged(StatBonuses stat) { StatChanged?.Invoke(stat); }
    }
    public class StatBonuses
    {
        private float flatTotal = 0f;
        private float bonusTotal = 1f;
        private List<StatValue> timedBonuses = new();
        private List<StatValue> timedFlats = new();

        public string ID { get; set; } = "";

        public event Action<StatBonuses>? Changed;

        public StatBonuses(string id) { this.ID = id; }
        
       
        public float BonusTotal { get { return bonusTotal - 1f; } }
        public float FlatTotal { get { return flatTotal; } }
        
        public float ApplyBonuses(float baseValue)
        {
           return (baseValue + flatTotal) * bonusTotal;
        }
        public int ApplyBonuses(int baseValue)
        {
            float v = ((float)baseValue + flatTotal) * bonusTotal;
            return (int)MathF.Ceiling(v);
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

            if (changed) Changed?.Invoke(this);

            return changed;
        }

        public void Set(StatBonuses other)
        {
            flatTotal = other.flatTotal;
            bonusTotal = other.bonusTotal;
            foreach (var bonus in other.timedBonuses)
            {
                AddBonus(bonus.value, bonus.GetRemaining());
            }
            foreach (var flat in other.timedFlats)
            {
                AddFlat(flat.value, flat.GetRemaining());
            }
            Changed?.Invoke(this);
        }
        public void Add(StatBonuses other)
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

            Changed?.Invoke(this);
        }

        public void AddBonuses(params float[] bonuses)
        {
            foreach (var bonus in bonuses)
            {
                bonusTotal += bonus;
            }

            Changed?.Invoke(this);
        }
        public void AddFlats(params float[] flats)
        {
            foreach (var flat in flats)
            {
                flatTotal += flat;
            }
            Changed?.Invoke(this);
        }
        
        public void AddBonus(float bonus, float duration = -1f)
        {
            bonusTotal += bonus;
            if (duration > 0f)
            {
                timedBonuses.Add(new(bonus, duration));
            }
            Changed?.Invoke(this);
        }
        public void AddFlat(float flat, float duration = -1f)
        {
            flatTotal += flat;
            if (duration > 0f)
            {
                timedFlats.Add(new(flat, duration));
            }
            Changed?.Invoke(this);
        }
        public void RemoveBonus(float bonus)
        {
            bonusTotal -= bonus;
            Changed?.Invoke(this);
        }
        public void RemoveFlat(float flat)
        {
            flatTotal -= flat;
            Changed?.Invoke(this);
        }
        public void ResetBonus() { bonusTotal = 1f; timedBonuses.Clear(); Changed?.Invoke(this); }
        public void ResetFlat() { flatTotal = 0f; timedFlats.Clear(); Changed?.Invoke(this); }
        public void Reset()
        {
            bonusTotal = 1f;
            timedBonuses.Clear();

            flatTotal = 0f;
            timedFlats.Clear();

            Changed?.Invoke(this);
        }
    }
    public class StatI
    {
        public event Action<StatI, int>? CurChanged;
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
        public string[] StatBonusIDS { get; set; }
        public StatI(int baseValue, params string[] statBonusIds) { Base = baseValue; Cur = baseValue; StatBonusIDS = statBonusIds; }
        public void SetBase(int value)
        {
            Base = value;
            Cur = value;
        }
        public void UpdateCur(float totalBonuses, float totalFlats)
        {
            int old = Cur;
            float v = ((float)Base + totalFlats) * totalBonuses;
            Cur = (int)MathF.Ceiling(v);
            if (Cur != old) CurChanged?.Invoke(this, old);
        }

    }
    public class StatF
    {
        public event Action<StatF, float>? CurChanged;
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
        public string[] StatBonusIDS { get; set; }
        public StatF(float baseValue, params string[] statBonusIds) { Base = baseValue; Cur = baseValue; StatBonusIDS = statBonusIds; }
        public void SetBase(float value)
        {
            Base = value;
            Cur = value;
        }
        public void UpdateCur(float totalBonuses, float totalFlats)
        {
            float old = Cur;
            Cur = (Base + totalFlats) * totalBonuses;
            if (Cur != old) CurChanged?.Invoke(this, old);
        }
        
    }

    //StatF/ StatI have event for changed and stat does not have an event anymore!!!

    public class StatSimple
    {
        private float baseValue = 0f;
        private float flatTotal = 0f;
        private float bonusTotal = 1f;
        private List<StatValue> timedBonuses = new();
        private List<StatValue> timedFlats = new();

        public string ID { get; set; } = "";

        public event Action<StatSimple>? Changed;
        
        public StatSimple(float baseValue) { this.baseValue = baseValue; }
        public StatSimple(float baseValue, string id) { this.baseValue = baseValue; this.ID = id;}
        public void SetBase(float value) 
        {
            float prev = baseValue;
            baseValue = value;
            if (prev != baseValue) Changed?.Invoke(this);
        }
        public float Base { get { return baseValue; } }
        public float Cur
        {
            get { return (baseValue + flatTotal) * bonusTotal; }
            private set { }
        }
        public float BonusTotal { get { return bonusTotal; } }
        public float FlatTotal { get { return flatTotal; } }
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

            if (changed) Changed?.Invoke(this);

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

            Changed?.Invoke(this);
        }

        public void AddBonuses(params float[] bonuses)
        {
            foreach (var bonus in bonuses)
            {
                bonusTotal += bonus;
            }

            Changed?.Invoke(this);
        }
        public void AddFlats(params float[] flats)
        {
            foreach (var flat in flats)
            {
                flatTotal += flat;
            }
            Changed?.Invoke(this);
        }
        public void AddBonus(float bonus, float duration = -1f)
        {
            bonusTotal += bonus;
            if (duration > 0f)
            {
                timedBonuses.Add(new(bonus, duration));
            }
            Changed?.Invoke(this);
        }
        public void AddFlat(float flat, float duration = -1f)
        {
            flatTotal += flat;
            if (duration > 0f)
            {
                timedFlats.Add(new(flat, duration));
            }
            Changed?.Invoke(this);
        }
        public void RemoveBonus(float bonus)
        {
            bonusTotal -= bonus;
            Changed?.Invoke(this);
        }
        public void RemoveFlat(float flat)
        {
            flatTotal -= flat;
            Changed?.Invoke(this);
        }
        public void ResetBonus() { bonusTotal = 1f; timedBonuses.Clear(); Changed?.Invoke(this); }
        public void ResetFlat() { flatTotal = 0f; timedFlats.Clear(); Changed?.Invoke(this); }
        public void Reset() 
        {
            bonusTotal = 1f; 
            timedBonuses.Clear();
            
            flatTotal = 0f; 
            timedFlats.Clear(); 
            
            Changed?.Invoke(this);
        }
    }
    public class StatNamed
    {
        private float prevTotal = 0f;
        private float baseValue = 0f;
        private float flatTotal = 0f;
        private float bonusTotal = 1f;
        private float total = 0f;

        private Dictionary<string, StatValue> bonuses = new();
        private Dictionary<string, StatValue> flatBonues = new();
        public string ID { get; set; } = "";

        public event Action<StatNamed>? Changed;
        public StatNamed(float baseValue)
        {
            this.baseValue = baseValue;
        }
        public StatNamed(float baseValue, string id)
        {
            this.baseValue = baseValue;
            this.ID = id;
        }
        public float BonusTotal { get { return bonusTotal; } }
        public float FlatTotal { get { return flatTotal; } }
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
            UpdateTotal();
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
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            total = baseValue;
            flatTotal = 0f;
            bonusTotal = 1f;
            foreach (var flat in flatBonues.Values)
            {
                flatTotal += flat.value;
                total += flat.value;
            }

            float totalBonus = 1f;
            foreach (var bonus in bonuses.Values)
            {
                bonusTotal += bonus.value;
                totalBonus += bonus.value;
            }

            total *= totalBonus;

            if(total != prevTotal) Changed?.Invoke(this);

            prevTotal = total;
        }
    }


}
