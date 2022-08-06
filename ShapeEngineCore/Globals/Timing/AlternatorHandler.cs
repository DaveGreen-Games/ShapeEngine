

namespace ShapeEngineCore.Globals.Timing
{
    public static class AlternatorHandler
    {
        public static AlternatorContainer container = new();


        public static void Add(string name, Alternator alternator)
        {
            container.Add(name, alternator);
        }
        public static bool Remove(string name)
        {
            return container.Remove(name);
        }
        public static bool StartAlternator(string name)
        {
            return container.StartAlternator(name);
        }
        public static bool StartAlternator(string name, int state)
        {
            return container.StartAlternator(name, state);
        }
        public static bool StartAlternator(string name, string state)
        {
            return container.StartAlternator(name, state);
        }
        public static bool StopAlternator(string name)
        {
            return container.StopAlternator(name);
        }
        public static Alternator? Get(string name)
        {
            return container.Get(name);
        }
        public static bool Has(string name) { return container.Has(name); }

        public static void Update(float dt)
        {
            container.Update(dt);
        }
    }
}
