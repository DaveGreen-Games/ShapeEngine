using ShapePersistent;

namespace ShapeEngineDemo.DataObjects
{
    public struct FrameInfo
    {
        public float x { get; set; }
        public float y { get; set; }
        public float lineThickness { get; set; }
    }
    public struct HardpointInfo
    {
        public float x { get; set; }
        public float y { get; set; }
        public float rotOffset { get; set; }
        public int type { get; set; }
        public int size { get; set; }
        public int turretMode { get; set; }

    }
    public struct ThrusterInfo
    {
        public float x { get; set; }
        public float y { get; set; }
        public float size { get; set; }
        public float pSize { get; set; }
        public int min { get; set; }
        public int max { get; set; }
    }
    public class PlayerData : DataObject
    {
        public float health { get; set; } = 0f;
        public float stunRes { get; set; } = 1f;
        public float speed { get; set; } = 0f;
        public float rotSpeed { get; set; } = 0f;
        public float size { get; set; } = 0f;
        public float colDmg { get; set; } = 0f;
        public float targetingRange { get; set; } = 0f;
        public float pinDuration { get; set; } = 0f;
        public float pinCooldown { get; set; } = 0f;
        public FrameInfo[] frame { get; set; } = new FrameInfo[0];
        public HardpointInfo[] hardpoints { get; set; } = new HardpointInfo[0];
        public ThrusterInfo[] thrusters { get; set; } = new ThrusterInfo[0];

    }
}
