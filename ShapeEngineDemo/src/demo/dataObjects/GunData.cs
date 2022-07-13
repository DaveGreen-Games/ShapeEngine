using ShapeEngineCore.Globals.Persistent;

namespace ShapeEngineDemo.DataObjects
{
    public class GunData : DataObject
    {
        public string bullet { get; set; } = "";
        public float bps { get; set; } = 0f;
        public bool semiAuto { get; set; } = false;
        public float ammoCost { get; set; } = 0f;
        public float accuracy { get; set; } = 0f;
        public int pellets { get; set; } = 0;
        public int burstCount { get; set; } = 0;
        public float burstDelay { get; set; } = 0f;
        public float speedVar { get; set; } = 0f;
        public string sound { get; set; } = "";
        public string effect { get; set; } = "";

    }
}
