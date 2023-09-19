
namespace ShapeEngine.Lib
{
    public static class SID
    {
        private static uint idCounter = 10;

        //public static uint NextID() { return idCounter++; }
        public static readonly uint INVALID_ID = 0;
        public static uint NextID { get { return idCounter++; } }
    }
}
