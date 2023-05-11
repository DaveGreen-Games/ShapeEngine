//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace ShapePersistent
{
    //public class Sheet <T>
    //{
    //    public string name { get; set; } = "";
    //    public T[] lines { get; set; } = new T[0];
    //}
    //public class DContainer<T> : IDataContainer where T : IDataObject
    //{
    //    public string Name { get; set; } = "";
    //    private List<T> data = new();
    //
    //    public List<IDataObject> GetData() { return data.Cast<IDataObject>().ToList(); }
    //
    //    public string GetName(){return Name;}
    //
    //    public IDataObject GetRandom()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    public interface IDataContainer
    {
        public string GetName();
        public List<IDataObject> GetData();
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
        public JNode[] GetArray(string arrayKey)
        {
            if (!Valid) return Array.Empty<JNode>();   
            if(node.ContainsKey(arrayKey))
            {
                var arr = node[arrayKey]?.AsArray();
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
        
        public List<IDataContainer> ParseToList(string arrayKey, Func<JNode, IDataContainer> parser)
        {
            List<IDataContainer> result = new();
            var arr = GetArray(arrayKey);
            foreach (var item in arr)
            {
                result.Add(parser(item));
            }
            return result;
        }
        public Dictionary<string, IDataContainer> ParseToDict(string arrayKey, Func<JNode, IDataContainer> parser)
        {
            Dictionary<string, IDataContainer> result = new();
            var arr = GetArray(arrayKey);
            foreach (var item in arr)
            {
                var c = parser(item);
                result.Add(c.GetName(), c);
            }
            return result;
        }
        
        public static List<T> SerializeArrayToList<T>(JNode[] nodes)
        {
            List<T> result = new List<T>();
            foreach (var node in nodes)
            {
                var r = node.Deserialize<T>();
                if(r != null) result.Add(r);
            }
            return result;
        }
        public static Dictionary<string, T> SerializeArrayToDictionary<T>(JNode[] nodes) where T : IDataObject
        {
            Dictionary<string, T> result = new();
            foreach (var node in nodes)
            {
                var r = node.Deserialize<T>();
                if (r != null)
                {
                    result.Add(r.GetName(), r);
                }
            }
            return result;
        }
        
        public List<T> SerializeArrayToList<T>(string arrayKey)
        {
            List<T> result = new();

            if (node.ContainsKey(arrayKey))
            {
                var arr = node[arrayKey]?.AsArray();
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
        public Dictionary<string, T> SerializeArrayToDict<T>(string arrayKey) where T : IDataObject
        {
            Dictionary<string, T> result = new();

            if (node.ContainsKey(arrayKey))
            {
                var arr = node[arrayKey]?.AsArray();
                if (arr != null)
                {
                    foreach (var item in arr)
                    {
                        if (item == null) continue;

                        var obj = item.AsObject();
                        if (obj != null)
                        {
                            var t = obj.Deserialize<T>();
                            if(t != null)result.Add(t.GetName(), t);
                        }
                    }
                }
            }
            return result;
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
        
        
        
        
        
        //public Dictionary<string, T> GetSheet<T>(string key, string idPropertyName)
        //{
        //    Dictionary<string, T> result = new();
        //
        //    if (node.ContainsKey(key))
        //    {
        //        var arr = node[key]?.AsArray();
        //        if(arr != null)
        //        {
        //            foreach (var item in arr)
        //            {
        //                if(item == null) continue;
        //                
        //                var obj = item.AsObject();
        //                if(obj != null)
        //                {
        //                    if (obj.ContainsKey(idPropertyName))
        //                    {
        //                        var id = obj[idPropertyName]?.ToString();
        //                        var t = obj.Deserialize<T>();
        //                        if(id != null && t != null) result.Add(id, t);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return result;
        //}
    }
   
    public class JManager
    {

        public JManager()
        {
        }


    }
}
