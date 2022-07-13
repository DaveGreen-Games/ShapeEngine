using ShapeEngineCore.Globals.Persistent;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace ShapeEngineDemo.DataObjects
{
    public class DefaultDataResolver : DataResolver
    {
        public override DataObject? Resolve(string sheetName, JsonNode line)
        {
            switch (sheetName)
            {
                case "asteroids": return line.Deserialize<AsteroidData>();
                case "player": return line.Deserialize<PlayerData>();
                case "projectiles": return line.Deserialize<ProjectileData>();
                case "guns": return line.Deserialize<GunData>();
                case "engines": return line.Deserialize<EngineData>();
                case "colors": return line.Deserialize<ColorData>();
                default: return null;
            }
        }
    }
}
