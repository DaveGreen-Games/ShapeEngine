using ShapePersistent;

namespace ShapeEngineDemo.DataObjects
{
    public class AsteroidData : DataObject
    {
        public float velMin { get; set; } = 0f;
        public float velMax { get; set; } = 0f;
        public int spawnCount { get; set; } = 0;
        public string spawnName { get; set; } = "";
        public float size { get; set; } = 0f;
        public float health { get; set; } = 0f;

    }
}
