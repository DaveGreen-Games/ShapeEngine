using System.Text.Json.Nodes;
using System.Text.Json;
using Newtonsoft.Json;
//using ShapeEngineCore.Demo.DataObjects;

namespace ShapeEngineCore.Globals.Persistent
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


        public DataContainerCDB(string dataFileName, DataResolver resolver, params string[] sheetNames)
        {
            if (dataFileName == "" || sheetNames.Length <= 0) return;

            var dataString = ResourceManager.LoadJsonData(dataFileName);
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

        public static Dictionary<string, T> LoadSheetFromResourceManager<T>(string dataFileName, string sheetName) where T : DataObject
        {
            string dataString = ResourceManager.LoadJsonData(dataFileName);
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
        public static T? LoadLineFromResourceManager<T>(string dataFileName, string sheetName, string lineName) where T : DataObject
        {
            var lines = LoadSheetFromResourceManager<T>(dataFileName, sheetName);
            if (lines == null) return null;
            if (lines.ContainsKey(lineName)) return lines[lineName];
            else return null;
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


        private static JsonArray? GetDataSheets(JsonNode root)
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
        private static JsonObject? GetDataSheet(JsonNode root, string sheetName)
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
        private static JsonArray? GetDataSheetLines(JsonNode root, string sheetName)
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


    public static class DataHandler
    {
        private static Dictionary<string, DataContainer> dataContainers = new();

        
        

        /// <summary>
        /// Checks if the DataHandler contains a data container.
        /// </summary>
        /// <param name="name">The name of the data container.</param>
        /// <returns></returns>
        public static bool ContainsDataContainer(string name = "default")
        {
            return dataContainers.ContainsKey(name);
        }
        /// <summary>
        /// Get a data container if it exists.
        /// </summary>
        /// <param name="name">The name of the data container.</param>
        /// <returns></returns>
        public static DataContainer? GetDataContainer(string name = "default")
        {
            if (!ContainsDataContainer(name)) return null;
            return dataContainers[name];
        }
        /// <summary>
        /// Get a generic data container if it exists.
        /// </summary>
        /// <param name="name">The name of the data container.</param>
        /// <returns></returns>
        public static T? GetDataContainer<T>(string name = "default") where T : DataContainer
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
        public static DataContainerCDB? GetCDBContainer(string name = "default")
        {
            if (!ContainsDataContainer(name)) return null;
            var container = dataContainers[name];
            if (container is DataContainerCDB) return (DataContainerCDB)dataContainers[name];
            else return null; 
        }


        /// <summary>
        /// Add a new data container. If name already exists the old container is overwritten with the new one.
        /// </summary>
        /// <param name="name">The name of the data container.</param>
        /// <param name="dataContainer">The data container to add.</param>
        public static void AddDataContainer(DataContainer dataContainer, string name = "default")
        {
            if (ContainsDataContainer(name))
            {
                dataContainers[name].Close();
                dataContainers[name] = dataContainer;
            }
            else dataContainers.Add(name, dataContainer);
        }


        public static void Close()
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
        public static string GetJsonFile(string fileName)
        {
            return ResourceManager.LoadJsonData(fileName);
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


        /*
        public static readonly Dictionary<string, Dictionary<string, DataObject>> data = new();
        
        public static void Initialize(string dataFileName, DataResolver resolver, params string[] sheetNames)
        {
            if (dataFileName == "" || sheetNames.Length <= 0) return;

            var dataString = ResourceManager.LoadJsonData(dataFileName);
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

        public static bool Contains(string sheet)
        {
            return data.ContainsKey(sheet);
        }
        public static bool Contains(string sheet, string name)
        {
            if (!Contains(sheet)) return false;
            return data[sheet].ContainsKey(name);
        }
        public static Dictionary<string, T> GetSheet<T>(string sheetName) where T : DataObject
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
        public static T? Get<T>(string sheet, string name) where T : DataObject
        {
            if (!Contains(sheet, name)) return default;
            return data[sheet][name] as T;
        }
        public static DataObject? Get(string sheet, string name)
        {
            if (!Contains(sheet, name)) return null;
            return data[sheet][name];
        }

        public static Dictionary<string, T> LoadSheetFromResourceManager<T>(string dataFileName, string sheetName) where T : DataObject
        {
            string dataString = ResourceManager.LoadJsonData(dataFileName);
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
        public static T? LoadLineFromResourceManager<T>(string dataFileName, string sheetName, string lineName) where T : DataObject
        {
            var lines = LoadSheetFromResourceManager<T>(dataFileName, sheetName);
            if (lines == null) return null;
            if (lines.ContainsKey(lineName)) return lines[lineName];
            else return null;
        }
        public static Dictionary<string, T> LoadSheetFromFile<T>(string dataPath, string sheetName) where T : DataObject
        {
            string dataString = File.ReadAllText(dataPath);// LoadFileText(dataPath);
            var dataNode = JsonNode.Parse(dataString);
            if(dataNode == null) return new();
            var lines = GetDataSheetLines(dataNode, sheetName);
            if(lines == null) return new();
            Dictionary<string, T> dict = new();
            foreach (var line in lines)
            {
                var result = line.Deserialize<T>();
                if(result == null) continue;
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

        

        public static void Close()
        {
            foreach (var d in data.Values)
            {
                d.Clear();
            }
        }
        private static JsonArray? GetDataSheets(JsonNode root)
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
        private static JsonObject? GetDataSheet(JsonNode root, string sheetName)
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
        private static JsonArray? GetDataSheetLines(JsonNode root, string sheetName)
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
        */
    }

}


/*

private static bool GetDataSheets(JsonDocument doc, out JsonElement.ArrayEnumerator sheets)
{
	var root = doc.RootElement;
	JsonElement sheet;
	if(root.TryGetProperty("sheets", out sheet))
    {
		sheets = sheet.EnumerateArray();
		return true;
    }

	sheets = new();
	return false;
}
private static bool GetDataSheet(JsonDocument doc, string sheetName, out JsonElement dataSheet)
{
	JsonElement.ArrayEnumerator sheets;
    if (!GetDataSheets(doc, out sheets))
    {
		dataSheet = new();
		return false;
    }

	JsonElement sheet;

    foreach (var s in sheets)
    {
		if (s.TryGetProperty("name", out sheet) && sheet.GetString() == sheetName)
        {
			dataSheet = s;
			return true;
        }
    }
	dataSheet = new();
	return false;
}
private static bool GetDataSheetLines(JsonDocument doc, string sheetName, out JsonElement.ArrayEnumerator dataLines)
{
	JsonElement dataSheet;
    if (!GetDataSheet(doc, sheetName, out dataSheet))
    {
		dataLines = new();
		return false;
    }

	JsonElement dataLineElement;
    if (dataSheet.TryGetProperty("lines", out dataLineElement))
    {
		dataLines = dataLineElement.EnumerateArray();
		return true;
    }

	dataLines = new();
	return false;
}


*/


/*
private static JsonElement.ArrayEnumerator GetDataSheets(JsonDocument doc)
        {
			var root = doc.RootElement;
			return root.GetProperty("sheets").EnumerateArray();
		}
		private static JsonElement GetDataSheet(JsonDocument doc, string sheetName)
        {
			JsonElement.ArrayEnumerator sheets = GetDataSheets(doc);

            foreach (var s in sheets)
            {
				if (s.GetProperty("name").GetString() == sheetName)
                {
					return s;
                }
            }
			return new();
        }
		private static JsonElement.ArrayEnumerator GetDataSheetLines(JsonDocument doc, string sheetName)
        {
			JsonElement dataSheet = GetDataSheet(doc, sheetName);
			return dataSheet.GetProperty("lines").EnumerateArray();
        }
*/

/*var sheets = doc.RootElement.GetProperty("sheets");
foreach (var sheet in sheets.EnumerateArray())
{
	string sheetName = sheet.GetProperty("name").GetString();
	if (sheetName == "entities")
	{
		var entities = sheet.GetProperty("lines");
		foreach (var entity in entities.EnumerateArray())
		{
			string name = entity.GetProperty("name").GetString();
			float minSpeed = (float)entity.GetProperty("minSpeed").GetDouble();
			float maxSpeed = (float)entity.GetProperty("maxSpeed").GetDouble();
			float radius = (float)entity.GetProperty("radius").GetDouble();

			EntityObject e = new(name, minSpeed, maxSpeed, radius);
			entityData.Add(e);
		}
	}
	else if (sheetName == "randomGen")
	{
		var values = sheet.GetProperty("lines");
		foreach (var value in values.EnumerateArray())
		{

		}
	}
}*/





/*

{
	"sheets": [
		{
			"name": "entities",
			"columns": [
				{
					"typeStr": "1",
					"name": "name",
					"display": null
				},
				{
					"typeStr": "4",
					"name": "minSpeed"
				},
				{
					"typeStr": "4",
					"name": "maxSpeed"
				},
				{
					"typeStr": "4",
					"name": "radius"
				}
			],
			"lines": [
				{
					"radius": 3,
					"name": "particle",
					"minSpeed": 10,
					"maxSpeed": 70
				},
				{
					"name": "bullet",
					"radius": 5,
					"minSpeed": 195,
					"maxSpeed": 205
				}
			],
			"separators": [],
			"props": {}
		},
		{
			"name": "randomGen",
			"columns": [
				{
					"typeStr": "1",
					"name": "name",
					"display": null
				},
				{
					"typeStr": "4",
					"name": "value"
				}
			],
			"lines": [
				{
					"name": "spawnRate",
					"value": 5
				},
				{
					"name": "difficulty",
					"value": 10
				}
			],
			"separators": [],
			"props": {}
		}
	],
	"customTypes": [],
	"compress": false
}

*/