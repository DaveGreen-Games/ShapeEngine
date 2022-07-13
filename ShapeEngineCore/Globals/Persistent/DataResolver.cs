using System.Text.Json.Nodes;
using System.Text.Json;

namespace ShapeEngineCore.Globals.Persistent
{
    public class DataResolver
    {
        public virtual DataObject? Resolve(string sheetName, JsonNode line)
        {
            return null;
        }
    }
}
