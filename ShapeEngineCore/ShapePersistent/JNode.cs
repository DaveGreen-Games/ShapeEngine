using System.Text.Json.Nodes;
using System.Text.Json;
using ShapeLib;

namespace ShapePersistent
{
    //public abstract class JDataObject : IDataObject
    //{
    //    public abstract string GetName();
    //}

    //public class JArray : List<JNode>
    //{
    //    
    //}
    
    public interface IDataObject
    {
        public string GetName();
    }
    public interface IDataContainer
    {
        public string GetName();
        public List<IDataObject> GetData();
        public List<T> GetData<T>() { return GetData().Cast<T>().ToList(); }
        public IDataObject? GetRandomEntry();
        public T? GetRandomEntry<T>();
        public IDataObject? GetEntry(string name);
        public T? GetEntry<T>(string name);
    }
    public class JDataContainer : IDataContainer
    {
        public string Name { get; set; } = "";
        //public List<JDataObject> data { get; protected set; } = new();
        //public virtual List<T> GetData<T>() { return data.Cast<T>().ToList(); }
        protected Dictionary<string, IDataObject> data = new();

        public JDataContainer() { }
        public JDataContainer(params IDataObject[] data)
        {
            foreach (var entry in data)
            {
                if (this.data.ContainsKey(entry.GetName())) continue;
                this.data.Add(entry.GetName(), entry);
            }
        }
        public JDataContainer(List<IDataObject> data)
        {
            foreach (var entry in data)
            {
                if (this.data.ContainsKey(entry.GetName())) continue;
                this.data.Add(entry.GetName(), entry);
            }
        }
        public string GetName() { return Name; }
        
        public IDataObject? GetRandomEntry()
        {
            if (data.Count <= 0) return null;
            int randIndex = SRNG.randI(data.Count);
            return data.ElementAt(randIndex).Value;
        }
        public T? GetRandomEntry<T>()
        {
            if (data.Count <= 0) return default(T);
            int randIndex = SRNG.randI(data.Count);
            if (data.ElementAt(randIndex).Value is T t) return t;
            return default(T);
        }

        public IDataObject? GetEntry(string name)
        {
            if (data.ContainsKey(name)) return data[name];
            else return null;
        }
        public T? GetEntry<T>(string name)
        {
            if (data.ContainsKey(name))
            {
                if (data[name] is T t) return t;
            }
            return default(T);
        }

        public List<IDataObject> GetData() { return data.Values.ToList(); }
        public List<T> GetData<T>() { return data.Values.Cast<T>().ToList(); }
        
        //public IDataObject? GetRandomEntry()
        //{
        //    var data = GetData();
        //    if (data.Count <= 0) return null;
        //    int randIndex = SRNG.randI(data.Count);
        //    return data[randIndex];
        //}
        //public T? GetRandomEntry<T>()
        //{
        //    var data = GetData();
        //    if (data.Count <= 0) return default(T);
        //    int randIndex = SRNG.randI(data.Count);
        //    if (data[randIndex] is T t) return t;
        //    return default(T);
        //}
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
        
        public List<T> ParseToList<T>(string arrayKey, Func<JNode, T> parser) where T : IDataContainer
        {
            List<T> result = new();
            var arr = GetArray(arrayKey);
            foreach (var item in arr)
            {
                result.Add(parser(item));
            }
            return result;
        }
        public Dictionary<string, T> ParseToDict<T>(string arrayKey, Func<JNode, T> parser) where T : IDataContainer
        {
            Dictionary<string, T> result = new();
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
        public static Dictionary<string, T> SerializeArrayToDict<T>(JNode[] nodes) where T : IDataObject
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
   
}
