//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace ShapePersistent
{

    public sealed class JNode
    {
        private JsonObject node;
        public bool Valid { get; private set; } = false;


        public JNode() { this.node = new JsonObject(); }
        public JNode(string json)
        {
            var obj = JsonNode.Parse(json)?.AsObject();
            if(obj != null)
            {
                Valid = true;
                node = obj;
            }
            else node = new JsonObject();
        }
        internal JNode(JsonNode? node)
        {
            var obj = node?.AsObject();
            if (obj != null)
            {
                Valid = true;
                this.node = obj;
            }
            else this.node = new JsonObject();
        }

        public string GetJsonString() { return node.ToJsonString(); }
        public bool ContainsKey(string key) { return Valid && node.ContainsKey(key); }

        public T? Deserialize<T>() { return node.Deserialize<T>(); }
        public T? GetProperty<T>(string key)
        {
            if (node.ContainsKey(key))
            {
                var n = node[key];
                return n.Deserialize<T>();
            }
            return default(T);
        }
        public JNode GetNode(string key)
        {
            if (node.ContainsKey(key))
            {
                var n = node[key];
                if(n != null) return new JNode(n);
            }
            return new JNode();
        }
        public HashSet<JNode> GetArray(string key)
        {
            if (!Valid) return new();   
            if(node.ContainsKey(key))
            {
                var arr = node[key]?.AsArray();
                if(arr != null)
                {
                    HashSet<JNode> result = new();
                    foreach (var item in arr)
                    {
                        var jn = new JNode(item);
                        if (jn.Valid) result.Add(jn);
                    }
                    return result;
                }
            }
            return new();
        }
        
    }

    //public class JArray
    //{
    //    private HashSet<JNode> entries = new();
    //    public bool Valid { get; private set; } = false;
    //    
    //    public JArray() { }
    //    internal JArray(JsonNode node)
    //    {
    //        var arr = node.AsArray();
    //        foreach(var item in arr)
    //        {
    //            var jn = new JNode(item);
    //            if(jn.Valid) entries.Add(jn);
    //        }
    //        if(entries.Count > 0) Valid = true;
    //    }
    //
    //
    //
    //    //private JsonArray arr;
    //    //public bool Valid { get; private set; } = false;
    //    //
    //    //public JArray() { arr = new JsonArray(); }
    //    //internal JArray(JsonNode node)
    //    //{
    //    //    arr = node.AsArray();
    //    //    Valid = true;
    //    //}
    //    //
    //    //public JNode Get(string property, string value)
    //    //{
    //    //
    //    //}
    //}

    //public class JNode
    //{
    //    private JsonNode node;
    //    private JNode(JsonNode node)
    //    {
    //        this.node = node;
    //    }
    //
    //    public static JNode? Create(string json)
    //    {
    //        var dataNode = JsonNode.Parse(json);
    //        if (dataNode == null) return null;
    //        else return new JNode(dataNode);
    //    }
    //}
    
    public class Player { }
    public class JManager
    {

        public JManager()
        {
            string json = "asoihfdahsfhkjo";
            var node = new JNode(json);
            var arr = node.GetArray("sheets");
            foreach (var item in arr)
            {
                if(item.ContainsKey("name") && item.GetProperty<string>("name") == "player")
                {
                    var lines = item.GetArray("lines");
                    Dictionary<string, Player> playerData = new();
                    foreach (var line in lines)
                    {
                        if (line.ContainsKey("name"))
                        {
                            var name = line.GetProperty<string>("name");
                            var player = line.Deserialize<Player>();
                            if(player != null && name != null)
                            {
                                playerData.Add(name, player);
                            }
                        }
                    }
                }
            }
        }


    }
}
