
namespace ShapeEngine.Lib
{
    public static class ShapeID
    {
        private static IdCounter counter = new();
        public static uint InvalidId => IdCounter.InvalidId;
        public static uint NextID => counter.NextId;
        
        public static void AdvanceTo(uint id) => counter.AdvanceTo(id);

        public static void Reset() => counter.Reset();
    }

    public class IdCounter
    {
        public static readonly uint InvalidId = 0;
        private uint count = 10;
        public uint NextId => count++;

        public void AdvanceTo(uint id)
        {
            if(id >= count) count = id + 1;
        }

        public void Reset() => count = 10;
    }
}
