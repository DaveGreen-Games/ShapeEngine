using ShapeEngineCore.Globals.Persistent;

namespace ShapeEngineDemo.DataObjects
{
    public class EngineData : DataObject
    {
        public float energy { get; set; } = 0f;
        public float eReplenish { get; set; } = 0f;
        public float cooldown { get; set; } = 0f;
        public float boostF { get; set; } = 0f;
        public float boostCost { get; set; } = 0f;
        public float slowF { get; set; } = 0f;
        public float slowCost { get; set; } = 0f;
    }
}
