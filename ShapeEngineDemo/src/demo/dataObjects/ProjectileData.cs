using ShapeEngineCore.Globals.Persistent;

namespace ShapeEngineDemo.DataObjects
{
    public class ProjectileData : DataObject
    {
        public float lifetime { get; set; } = 0f;
        public float dmg { get; set; } = 0f;
        public float critChance { get; set; } = 0f;
        public float critBonus { get; set; } = 0f;
        public float speed { get; set; } = 0f;
        public float accuracy { get; set; } = 0f;
        public float size { get; set; } = 0f;
        public float expRadius { get; set; } = 0f;
        public float forceradius { get; set; } = 0f;

    }
}
