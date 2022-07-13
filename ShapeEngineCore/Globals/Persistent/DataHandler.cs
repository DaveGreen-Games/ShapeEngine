using System.Text.Json.Nodes;
using System.Text.Json;
using Raylib_CsLo;
//using ShapeEngineCore.Demo.DataObjects;

namespace ShapeEngineCore.Globals.Persistent
{
    public class DataHandler
    {
        public static readonly Dictionary<string, Dictionary<string, DataObject>> data = new();
        
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

        public static Dictionary<string, T> LoadSheet<T>(string dataPath, string sheetName) where T : DataObject
        {
            string dataString = LoadFileText(dataPath);
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
        public static T? LoadLine<T>(string dataPath, string sheetName, string lineName) where T : DataObject
        {
            var lines = LoadSheet<T>(dataPath, sheetName);
            if (lines == null) return null;
            if (lines.ContainsKey(lineName)) return lines[lineName];
            else return null;
        }

        public static void Initialize(string dataPath, DataResolver resolver, params string[] sheetNames)
        {
            if (dataPath == "" || sheetNames.Length <= 0) return;
            
            var dataString = LoadFileText(dataPath);
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

        public static void Close()
        {
            //asteroidData.Clear();
            foreach (var d in data.Values)
            {
                d.Clear();
            }
            //dataString = "";
            //dataNode = null;
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