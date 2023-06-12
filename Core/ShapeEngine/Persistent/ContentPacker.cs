using System.IO.Compression;

namespace ShapeEngine.Persistent
{
    public struct ContentInfo
    {
        public string extension;
        public byte[] data;

        public ContentInfo(string extension, byte[] data) { this.extension = extension; this.data = data; }
    }
   
    /// <summary>
    /// Can be used to pack resources in a folder structur into a single txt file. Every resource consists of 2 lines the resulting txt file.
    /// The first line is the name with extension the second line is data as encoded string.
    /// </summary>
    public static class ContentPacker
    {
        
        /// <summary>
        /// Pack a folder structure of various content types into a single txt file.
        /// </summary>
        /// <param name="sourcePath">The path to the folder that should be packed. Goes trough all subfolders as well.</param>
        /// <param name="outputPath">The path where the resulting txt file should be saved.</param>
        /// <param name="outputFilename">The name of the resulting txt file.</param>
        public static void Pack(string sourcePath, string outputPath, string outputFilename = "resources.txt")
        {
            string[] files = Directory.GetFiles(sourcePath, "", SearchOption.AllDirectories);
            List<string> lines = new List<string>();
            foreach (var file in files)
            {
                lines.Add(Path.GetFileName(file));
                var d = File.ReadAllBytes(file);
                lines.Add(Convert.ToBase64String(Compress(d)));
            }
            File.WriteAllLines(outputPath + outputFilename, lines);
        }
        
        /// <summary>
        /// Load and unpack a txt file generate by the Pack function.
        /// </summary>
        /// <param name="path">The path to the packed txt file.</param>
        /// <param name="fileName">The name of the packed txt file.</param>
        /// <returns></returns>
        public static Dictionary<string, ContentInfo> Unpack(string path, string fileName = "resources.txt")
        {
            //if (editorMode) return new Dictionary<string, ResourceInfo>() { };

            Dictionary<string, ContentInfo> result = new();
            var lines = File.ReadAllLines(path + fileName);
            for (int i = 0; i < lines.Length; i += 2)
            {
                string filenName = lines[i];
                string name = Path.GetFileNameWithoutExtension(filenName);
                string extension = Path.GetExtension(filenName);
                string dataText = lines[i + 1];
                var data = Convert.FromBase64String(dataText);
                result.Add(name, new(extension, Decompress(data)));
            }
            return result;
        }
        
        private static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
        private static byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

    }

}
