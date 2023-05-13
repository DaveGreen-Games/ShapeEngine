using System.Text.Json;

namespace ShapeLib
{
    public static class SSavegame
    {
        public static string APPLICATION_DATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// Constructs a path out of all the folders with a backslash between them. Does not start with backslash but ends with a backslash.
        /// </summary>
        /// <param name="folders">The folder names for constructing a path.</param>
        /// <returns></returns>
        public static string ConstructPath(params string[] folders)
        {
            string path = "";
            foreach (string folder in folders)
            {
                if (path.EndsWith("\\"))
                {
                    path += folder;
                }
                else path += String.Format("{0}\\", folder);
            }
            return path;
        }
        public static string AddBackslash(string path) { return path + "\\"; }
        public static bool SaveText(string text, string absolutePath, string fileName)
        {
            if (absolutePath.Length <= 0 || fileName.Length <= 0) return false;
            Directory.CreateDirectory(absolutePath);
            File.WriteAllText(ConstructPath(absolutePath, fileName), text);
            return true;
        }
        public static string LoadText(string absolutePath, string fileName)
        {
            string path = ConstructPath(absolutePath, fileName);
            if (!File.Exists(path)) return String.Empty;
            return File.ReadAllText(path);
        }
        public static bool Save<T>(T data, string absolutePath, string fileName)
        {
            if (data == null) return false;
            if (absolutePath.Length <= 0 || fileName.Length <= 0) return false;
            Directory.CreateDirectory(absolutePath);

            string data_string = JsonSerializer.Serialize(data);
            if (data_string.Length <= 0) return false;
            File.WriteAllText(ConstructPath(absolutePath, fileName), data_string);
            //File.WriteAllText(absolutPath + "\\" + fileName, data_string);
            return true;
        }
        public static T? Load<T>(string absolutePath, string fileName)
        {
            //absolutPath = absolutPath + "\\" + fileName;
            string path = ConstructPath(absolutePath, fileName);
            if (!File.Exists(path)) return default;

            var data_string = File.ReadAllText(path);

            return JsonSerializer.Deserialize<T>(data_string);
        }
    }
}
