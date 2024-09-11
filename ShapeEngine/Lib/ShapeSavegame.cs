using System.Text.Json;
using ShapeEngine.Core;

namespace ShapeEngine.Lib
{
    public static class ShapeSavegame
    {
        public static string ApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string ApplicationLocalDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public static string ApplicationCommonDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public static string ApplicationDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string ApplicationCommonDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
        
        public static string GetSpecialFolderPath(Environment.SpecialFolder folder) => Environment.GetFolderPath(folder);

        
        public static string CombinePath(params string[] paths) => Path.Combine(paths);
        public static string CombinePath(string path1, string path2) => Path.Combine(path1, path2);

        
        public static bool SaveText(string text, string absolutePath, string fileName)
        {
            if (absolutePath.Length <= 0 || fileName.Length <= 0) return false;
            Directory.CreateDirectory(absolutePath);
            File.WriteAllText(CombinePath(absolutePath, fileName), text);
            return true;
        }
        public static string LoadText(string absolutePath, string fileName)
        {
            string path = CombinePath(absolutePath, fileName);
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
            File.WriteAllText(CombinePath(absolutePath, fileName), data_string);
            //File.WriteAllText(absolutPath + "\\" + fileName, data_string);
            return true;
        }
        public static T? Load<T>(string absolutePath, string fileName)
        {
            //absolutPath = absolutPath + "\\" + fileName;
            string path = CombinePath(absolutePath, fileName);
            if (!File.Exists(path)) return default;

            var data_string = File.ReadAllText(path);

            return JsonSerializer.Deserialize<T>(data_string);
        }
    }
}
