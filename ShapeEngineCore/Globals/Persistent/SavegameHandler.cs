using System.Text.Json;
//using System.IO;

namespace ShapeEngineCore.Globals.Persistent
{
    public static class SavegameHandler
    {

        public static string MAIN_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/solobytegames/template-raylib/";
        public static string SAVEGAME_PATH = MAIN_PATH + "savegames/";

        public static bool Save<T>(T data, string path, string fileName)
        {
            if (data == null) return false;
            if (path.Length <= 0 || fileName.Length <= 0) return false;
            Directory.CreateDirectory(path);

            string data_string = JsonSerializer.Serialize(data);//, new JsonSerializerOptions( new JsonSerializerDefaults() ) );
            if (data_string.Length <= 0) return false;

            File.WriteAllText(path + fileName, data_string);
            return true;
        }

        public static T? Load<T>(string path, string fileName)
        {
            path = path + fileName;
            if (!File.Exists(path)) return default;

            var data_string = File.ReadAllText(path);


            return JsonSerializer.Deserialize<T>(data_string);
        }

    }
}
