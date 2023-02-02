using System.Text.Json.Nodes;
using System.Text.Json;

namespace ShapePersistent
{
    public class DataResolver
    {
        public virtual DataObject? Resolve(string sheetName, JsonNode line)
        {
            return null;
        }
    }
}
