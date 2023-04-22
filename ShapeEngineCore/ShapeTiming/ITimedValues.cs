

namespace ShapeTiming
{
    public interface ITimedValues
    {
        public float Total { get; }
        public uint Add(float factor, float duration = -1);
        public bool Remove(uint id);
        public void Clear();
        public void Update(float dt);

    }

}
