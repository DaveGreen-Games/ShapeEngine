using System.Text.Json.Nodes;
using System.Text.Json;
using Newtonsoft.Json;
using ShapeLib;
//using ShapeEngineCore.Demo.DataObjects;

namespace ShapePersistent
{

    public class DataContainer
    {
        public virtual void Close()
        {

        }
    }

    public class DataContainerCDB : DataContainer
    {
        private readonly Dictionary<string, Dictionary<string, DataObject>> data = new();


        public DataContainerCDB(string dataFilePath, DataResolver resolver, params string[] sheetNames)
        {
            if (dataFilePath == "" || sheetNames.Length <= 0) return;

            var dataString = ResourceManager.LoadJsonDataFromRaylib(dataFilePath);
            var dataNode = JsonNode.Parse(dataString);
            if (dataNode == null) return;

            foreach (string sheetName in sheetNames)
            {
                data.Add(sheetName, new());
                var lines = GetDataSheetLines(dataNode, sheetName);
                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        if (line == null) continue;
                        var result = resolver.Resolve(sheetName, line);
                        if (result != null && !Contains(sheetName, result.name))
                        {
                            data[sheetName].Add(result.name, result);
                        }
                    }
                }
            }
        }

        public DataContainerCDB(DataResolver resolver, string dataString, params string[] sheetNames)
        {
            if (dataString == "" || sheetNames.Length <= 0) return;

            var dataNode = JsonNode.Parse(dataString);
            if (dataNode == null) return;

            foreach (string sheetName in sheetNames)
            {
                data.Add(sheetName, new());
                var lines = GetDataSheetLines(dataNode, sheetName);
                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        if (line == null) continue;
                        var result = resolver.Resolve(sheetName, line);
                        if (result != null && !Contains(sheetName, result.name))
                        {
                            data[sheetName].Add(result.name, result);
                        }
                    }
                }
            }
        }


        public bool Contains(string sheet)
        {
            return data.ContainsKey(sheet);
        }
        public bool Contains(string sheet, string name)
        {
            if (!Contains(sheet)) return false;
            return data[sheet].ContainsKey(name);
        }
        public Dictionary<string, T> GetSheet<T>(string sheetName) where T : DataObject
        {
            if (!data.ContainsKey(sheetName)) return new();
            Dictionary<string, T> lines = new();
            foreach (var line in data[sheetName])
            {
                var value = line.Value as T;
                if (value != null) lines.Add(line.Key, value);
            }
            return lines;
        }
        public string GetRandomElementName(string sheet)
        {
            if (!Contains(sheet)) return "";
            var s = data[sheet];
            if (s.Count <= 0) return "";
            int index = SRNG.randI(0, s.Count);
            return s.Keys.ToList()[index];
        }
        public T? GetRandom<T>(string sheet) where T : DataObject
        {
            if (!Contains(sheet)) return default;
            var s = data[sheet];
            if (s.Count <= 0) return default;
            int index = SRNG.randI(0, s.Count);
            string id = s.Keys.ToList()[index];
            return s[id] as T;
        }
        public T? Get<T>(string sheet, string name) where T : DataObject
        {
            if (!Contains(sheet, name)) return default;
            return data[sheet][name] as T;
        }
        public DataObject? Get(string sheet, string name)
        {
            if (!Contains(sheet, name)) return null;
            return data[sheet][name];
        }

        
        public static Dictionary<string, T> LoadSheetFromFile<T>(string dataPath, string sheetName) where T : DataObject
        {
            string dataString = File.ReadAllText(dataPath);// LoadFileText(dataPath);
            var dataNode = JsonNode.Parse(dataString);
            if (dataNode == null) return new();
            var lines = GetDataSheetLines(dataNode, sheetName);
            if (lines == null) return new();
            Dictionary<string, T> dict = new();
            foreach (var line in lines)
            {
                var result = line.Deserialize<T>();
                if (result == null) continue;
                dict.Add(result.name, result);
            }
            return dict;
        }
        public static T? LoadLineFromFile<T>(string dataPath, string sheetName, string lineName) where T : DataObject
        {
            var lines = LoadSheetFromFile<T>(dataPath, sheetName);
            if (lines == null) return null;
            if (lines.ContainsKey(lineName)) return lines[lineName];
            else return null;
        }


        public override void Close()
        {
            foreach (var d in data.Values)
            {
                d.Clear();
            }
            data.Clear();
        }


        public static JsonArray? GetDataSheets(JsonNode root)
        {
            var nodeObject = root.AsObject();
            if (nodeObject == null) return null;
            if (nodeObject.ContainsKey("sheets"))
            {
                var sheets = nodeObject["sheets"];
                if (sheets == null) return null;
                return sheets.AsArray();
            }
            return null;
        }
        public static JsonObject? GetDataSheet(JsonNode root, string sheetName)
        {
            var sheets = GetDataSheets(root);
            if (sheets == null) return null;
            foreach (var s in sheets)
            {
                if (s == null) continue;
                var sObject = s.AsObject();
                if (sObject == null || !sObject.ContainsKey("name")) continue;
                var sName = sObject["name"];
                if (sName == null) continue;
                if (sName.GetValue<string>() == sheetName) return sObject;
            }
            return null;
        }
        public static JsonArray? GetDataSheetLines(JsonNode root, string sheetName)
        {
            var sheet = GetDataSheet(root, sheetName);
            if (sheet == null) return null;
            if (sheet.ContainsKey("lines"))
            {
                var lines = sheet["lines"];
                if (lines == null) return null;
                return lines.AsArray();
            }
            return null;
        }

    }


    public class DataHandler
    {
        private Dictionary<string, DataContainer> dataContainers = new();

        public DataHandler() { }
        /// <summary>
        /// Checks if the DataHandler contains a data container.
        /// </summary>
        /// <param name="name">The name of the data container.</param>
        /// <returns></returns>
        public bool ContainsDataContainer(string name = "default")
        {
            return dataContainers.ContainsKey(name);
        }
        /// <summary>
        /// Get a data container if it exists.
        /// </summary>
        /// <param name="name">The name of the data container.</param>
        /// <returns></returns>
        public DataContainer? GetDataContainer(string name = "default")
        {
            if (!ContainsDataContainer(name)) return null;
            return dataContainers[name];
        }
        /// <summary>
        /// Get a generic data container if it exists.
        /// </summary>
        /// <param name="name">The name of the data container.</param>
        /// <returns></returns>
        public T? GetDataContainer<T>(string name = "default") where T : DataContainer
        {
            if (!ContainsDataContainer(name)) return null;
            var container = dataContainers[name];
            if (container is T) return (T)dataContainers[name];
            else return null; 
        }
        /// <summary>
        /// Get a Castle DB container if it exists.
        /// </summary>
        /// <param name="name">The name of the Castle DB container.</param>
        /// <returns></returns>
        public DataContainerCDB? GetCDBContainer(string name = "default")
        {
            if (!ContainsDataContainer(name)) return null;
            var container = dataContainers[name];
            if (container is DataContainerCDB) return (DataContainerCDB)dataContainers[name];
            else return null; 
        }

        /// <summary>
        /// Get an element (DataObject) from the specified sheet in the specified container.
        /// </summary>
        public T? GetCDBElement<T>(string sheetName, string elementName, string containerName = "default") where T : DataObject
        {
            if (!ContainsDataContainer(containerName)) return default;
            DataContainerCDB container = (DataContainerCDB) dataContainers[containerName];
            return container.Get<T>(sheetName, elementName);
        }
        /// <summary>
        /// Get a random element (DataObject) from the specified sheet in the specified container.
        /// </summary>
        public T? GetCDBRandomElement<T>(string sheetName, string containerName = "default") where T : DataObject
        {
            if (!ContainsDataContainer(containerName)) return default;
            DataContainerCDB container = (DataContainerCDB)dataContainers[containerName];
            return container.GetRandom<T>(sheetName);
        }

        /// <summary>
        /// Get a random element name from the specified sheet in the specified container.
        /// </summary>
        public string GetCDBRandomElementName(string sheetName, string containerName = "default")
        {
            if (!ContainsDataContainer(containerName)) return "";
            DataContainerCDB container = (DataContainerCDB)dataContainers[containerName];
            return container.GetRandomElementName(sheetName);
        }
        /// <summary>
        /// Add a new data container. If name already exists the old container is overwritten with the new one.
        /// </summary>
        /// <param name="name">The name of the data container.</param>
        /// <param name="dataContainer">The data container to add.</param>
        public void AddDataContainer(DataContainer dataContainer, string name = "default")
        {
            if (ContainsDataContainer(name))
            {
                dataContainers[name].Close();
                dataContainers[name] = dataContainer;
            }
            else dataContainers.Add(name, dataContainer);
        }


        public void Close()
        {
            foreach (var container in dataContainers.Values)
            {
                container.Close();
            }
            dataContainers.Clear();
        }






        /// <summary>
        /// Load a Json File from disk and return it as string.
        /// </summary>
        /// <param name="path">The path to the json file. Can either be relative to the project or absolut.</param>
        /// <returns></returns>
        public static string LoadJsonFile(string path)
        {
            return File.ReadAllText(path);
        }
        /// <summary>
        /// Load a Json File from the resource manager.
        /// </summary>
        /// <param name="fileName">The filename without extension to look for.</param>
        /// <returns></returns>
        public static string GetJsonFile(string fileName, ResourceManager res)
        {
            return res.LoadJsonData(fileName);
        }

        public static void PopulateObject(string json, object target)
        {
            JsonConvert.PopulateObject(json, target);
        }
        public static string Serialize(object? value)
        {
            return JsonConvert.SerializeObject(value);
        }
        public static T? Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}