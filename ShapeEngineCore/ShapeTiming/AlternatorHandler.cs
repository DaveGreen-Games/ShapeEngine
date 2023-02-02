

namespace ShapeTiming
{
    //alternator handler
    public class AlternatorHandler
    {
        private Dictionary<string, Alternator> alternators = new();

        public AlternatorHandler() { }
        public AlternatorHandler(Dictionary<string, Alternator> alternators)
        {
            this.alternators = alternators;
        }
        public AlternatorHandler(params (string name, Alternator alternator)[] alternators)
        {
            foreach (var item in alternators)
            {
                if (this.alternators.ContainsKey(item.name)) continue;
                this.alternators.Add(item.name, item.alternator);
            }
        }

        public void Add(string name, Alternator alternator)
        {
            if (Has(name)) alternators[name] = alternator;
            else alternators.Add(name, alternator);
        }
        public bool Remove(string name)
        {
            return alternators.Remove(name);
        }
        
        public bool StartAlternator(string name)
        {
            if (!Has(name)) return false;
            alternators[name].Start();
            return true;
        }
        public bool StartAlternator(string name, int state)
        {
            if (!Has(name)) return false;
            alternators[name].Start(state);
            return true;
        }
        public bool StartAlternator(string name, string state)
        {
            if (!Has(name)) return false;
            alternators[name].Start(state);
            return true;
        }

        public bool StopAlternator(string name)
        {
            if (!Has(name)) return false;
            alternators[name].Stop();
            return true;
        }
        public Alternator? Get(string name)
        {
            if(!Has(name)) return null;
            return alternators[name];
        }
        public bool Has(string name) { return alternators.ContainsKey(name); }

        
        public void Update(float dt)
        {
            foreach (var alternator in alternators.Values)
            {
                alternator.Update(dt);
            }
        }

        public void Clear()
        {
            alternators.Clear();
        }
        public void Close() { Clear(); }

    }
}
