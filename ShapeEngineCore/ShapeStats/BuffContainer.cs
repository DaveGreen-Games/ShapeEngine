

namespace ShapeStats
{
    public class BuffContainer
    {
        private Dictionary<int, IBuff> buffs = new();
        
        public List<IBuff> GetBuffs() { return buffs.Values.ToList(); }
        public List<IBuff> GetBuffs(Predicate<IBuff> match) { return GetBuffs().FindAll(match); }
        public List<IBuff> GetUIBuffs() { return GetBuffs(b => b.DrawToUI()); }
        
        public void AddBuff(IBuff buff)
        {
            if (buffs.ContainsKey(buff.GetID())) buffs[buff.GetID()].AddStack();
            else buffs.Add(buff.GetID(), buff);
        }
        public void RemoveBuff(int id)
        {
            if (buffs.ContainsKey(id))
            {
                buffs[id].RemoveStack();
                if (buffs[id].IsEmpty()) buffs.Remove(id);
            }
        }
        public void RemoveBuff(IBuff buff)
        {
            if (buffs.ContainsKey(buff.GetID()))
            {
                buffs[buff.GetID()].RemoveStack();
                if (buffs[buff.GetID()].IsEmpty()) buffs.Remove(buff.GetID());
            }
        }
       
        public void Update(float dt)
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                var buff = buffs.ElementAt(i).Value;
                buff.Update(dt);
                if (buff.IsEmpty())
                {
                    buffs.Remove(buff.GetID());
                }
            }
        }
        
        public void Apply(params IStat[] stats)
        {
            if (buffs.Count <= 0) return;
            foreach (var stat in stats)
            {
                ApplyStat(stat);
            }
        }
        public void Apply(IStat stat)
        {
            if (buffs.Count <= 0) return;
            ApplyStat(stat);
        }
        
        private void ApplyStat(IStat stat)
        {
            float totalFlat = 0f;
            float totalBonus = 0f;
            foreach (var buff in buffs.Values)
            {
                var info = buff.Get(stat.Tags);
                totalBonus += info.totalBonus;
                totalFlat += info.totalFlat;
            }

            stat.UpdateCur(totalBonus, totalFlat);
        }
    }

}
