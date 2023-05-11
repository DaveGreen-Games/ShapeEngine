//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace ShapePersistent
{
    public interface IDataContainer
    {
        public IDataObject GetRandom();
    }
    public interface IDataObject
    {
        public string GetName();
        
    }

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

        public JNode GetNode(string key)
        {
            if (node.ContainsKey(key))
            {
                var n = node[key];
                if(n != null) return new JNode(n);
            }
            return new JNode();
        }
        public JNode[] GetArray(string key)
        {
            if (!Valid) return Array.Empty<JNode>();   
            if(node.ContainsKey(key))
            {
                var arr = node[key]?.AsArray();
                if(arr != null)
                {
                    
                    List<JNode> result = new();
                    foreach (var item in arr)
                    {
                        var jn = new JNode(item);
                        if (jn.Valid) result.Add(jn);
                    }
                    return result.ToArray();
                }
            }
            return Array.Empty<JNode>();
        }
        
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
        
        public List<T> GetSheet<T>(string key)
        {
            List<T> result = new();

            if (node.ContainsKey(key))
            {
                var arr = node[key]?.AsArray();
                if (arr != null)
                {
                    foreach (var item in arr)
                    {
                        if (item == null) continue;

                        var obj = item.AsObject();
                        if (obj != null)
                        {
                            var t = obj.Deserialize<T>();
                            if (t != null) result.Add(t);
                        }
                    }
                }
            }
            return result;
        }
        public Dictionary<string, T> GetSheet<T>(string key, string idPropertyName)
        {
            Dictionary<string, T> result = new();

            if (node.ContainsKey(key))
            {
                var arr = node[key]?.AsArray();
                if(arr != null)
                {
                    foreach (var item in arr)
                    {
                        if(item == null) continue;
                        
                        var obj = item.AsObject();
                        if(obj != null)
                        {
                            if (obj.ContainsKey(idPropertyName))
                            {
                                var id = obj[idPropertyName]?.ToString();
                                var t = obj.Deserialize<T>();
                                if(id != null && t != null) result.Add(id, t);
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
   
    public class JManager
    {

        public JManager()
        {
            
        }


    }
}
