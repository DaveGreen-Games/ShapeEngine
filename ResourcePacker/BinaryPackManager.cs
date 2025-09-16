using System.Diagnostics;
using System.IO.Compression;

namespace ResourcePacker;

public class BinaryPackManager
{
    #region Binary Packer
    //TODO: Optimized versions:
    // - Batching should be standard
    // - Parallel should be the second option
    // - UnpackFromFile, UnpackFromFileParallel
    // - UnpackFromFileToMemory
    // - CreateIndexFromFile, LoadDataFromFile
    
    //TODO: make pack functions also store an index for fast random access
    // store files packed in the first line
    // store index length in the second line
    // store index in the third line
    // store rest of data
    public static bool Pack(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Directory not found: {sourceDirectoryPath}");
            return false;
        }
        
        if (!Path.HasExtension(outputFilePath))
        {
            Console.WriteLine($"Output resource path must have a valid extension: {outputFilePath}");
            return false;
        }
    
        var resxDir = Path.GetDirectoryName(outputFilePath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return false;
        }
    
        if (!Directory.Exists(resxDir))
        {
            Console.WriteLine($"Created output directory: {resxDir}");
            Directory.CreateDirectory(resxDir);
        }
        
        var debugWatch = Stopwatch.StartNew();
        
        var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        int total = files.Length, current = 0;
        long totalBytesPacked = 0;
        int totalFilesPacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>(total) : [];
    
        Console.WriteLine($"Binary File packing started with {total} files.");
        
        using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        //Create placeholder for writing total files packed after packing is done
        fileStream.Write(new byte[4], 0, 4); 
        
        //Q: store index length next?
        //Q: store lookup index next?
        
        using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal, leaveOpen: true))
        {
            foreach (var file in files)
            {
                current++;
                if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || current == total)
                {
                    ResourcePackManager.PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }
        
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
                {
                    if (debug) debugMessages.Add($"File skipped due to extension: {Path.GetFileName(file)}");
                    continue;
                }
        
                var relativePath = Path.GetRelativePath(sourceDirectoryPath, file);
                var pathBytes = System.Text.Encoding.UTF8.GetBytes(relativePath);
                var data = File.ReadAllBytes(file);
    
                totalFilesPacked++;
                totalBytesPacked += data.Length;
                // Write header: [path length][path][data length][data]
                gzipStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
                gzipStream.Write(pathBytes, 0, pathBytes.Length);
                gzipStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                gzipStream.Write(data, 0, data.Length);
        
                if (debug) debugMessages.Add($"Packed {relativePath} ({data.Length} bytes)");
            }
        }
        
        //write to placeholder at the start of the file
        fileStream.Position = 0;
        fileStream.Write(BitConverter.GetBytes(totalFilesPacked), 0, 4);
        
        if (debugMessages.Count > 0) foreach (var msg in debugMessages) Console.WriteLine(msg);
        Console.WriteLine($"Packing finished. {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds} seconds. Total bytes packed: {totalBytesPacked}");
        return true;
    }
    public static bool PackParallel(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Directory not found: {sourceDirectoryPath}");
            return false;
        }
    
        if (!Path.HasExtension(outputFilePath))
        {
            Console.WriteLine($"Output resource path must have a valid extension: {outputFilePath}");
            return false;
        }
    
        var resxDir = Path.GetDirectoryName(outputFilePath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return false;
        }
    
        if (!Directory.Exists(resxDir))
        {
            Console.WriteLine($"Created output directory: {resxDir}");
            Directory.CreateDirectory(resxDir);
        }
    
        var debugWatch = Stopwatch.StartNew();
        var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        int total = files.Length, current = 0;
        long totalBytesPacked = 0;
        int totalFilesPacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>(total) : [];
    
        Console.WriteLine($"Parallel Binary File packing started with {total} files.");
        
        // Parallel read and buffer file data
        var fileDataList = new List<(string RelativePath, byte[] Data)>(files.Length);
        Parallel.ForEach(files, file =>
        {
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
                return;
    
            var relativePath = Path.GetRelativePath(sourceDirectoryPath, file);
            var data = File.ReadAllBytes(file);
            lock (fileDataList)
            {
                fileDataList.Add((relativePath, data));
            }
        });
    
        using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        fileStream.Write(new byte[4], 0, 4); // Placeholder for total files
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal, leaveOpen: true);
    
        foreach (var (relativePath, data) in fileDataList)
        {
            current++;
            if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || current == total)
            {
                ResourcePackManager.PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }
    
            var pathBytes = System.Text.Encoding.UTF8.GetBytes(relativePath);
            gzipStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
            gzipStream.Write(pathBytes, 0, pathBytes.Length);
            gzipStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
            gzipStream.Write(data, 0, data.Length);
    
            totalFilesPacked++;
            totalBytesPacked += data.Length;
            if (debug) debugMessages.Add($"Packed {relativePath} ({data.Length} bytes)");
        }
    
        // Write total files packed at the start
        fileStream.Position = 0;
        fileStream.Write(BitConverter.GetBytes(totalFilesPacked), 0, 4);
    
        if (debugMessages.Count > 0) foreach (var msg in debugMessages) Console.WriteLine(msg);
        Console.WriteLine($"Packing finished. {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds} seconds. Total bytes packed: {totalBytesPacked}");
        return true;
    }
    public static bool PackBatching(string outputFilePath, string sourceDirectoryPath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!Directory.Exists(sourceDirectoryPath))
        {
            Console.WriteLine($"Directory not found: {sourceDirectoryPath}");
            return false;
        }
    
        if (!Path.HasExtension(outputFilePath))
        {
            Console.WriteLine($"Output resource path must have a valid extension: {outputFilePath}");
            return false;
        }
    
        var resxDir = Path.GetDirectoryName(outputFilePath);
        if (resxDir == null)
        {
            Console.WriteLine($"Directory not found: {resxDir}");
            return false;
        }
    
        if (!Directory.Exists(resxDir))
        {
            Console.WriteLine($"Created output directory: {resxDir}");
            Directory.CreateDirectory(resxDir);
        }
    
        var debugWatch = Stopwatch.StartNew();
        var files = Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories);
        int total = files.Length, current = 0;
        long totalBytesPacked = 0;
        int totalFilesPacked = 0;
        var sw = Stopwatch.StartNew();
        var debugMessages = debug ? new List<string>(total) : [];
        int batchSize = 512;
    
        Console.WriteLine($"Binary File packing started with {total} files. Batch Size: {batchSize}");
        
        using var fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        fileStream.Write(new byte[4], 0, 4); // Placeholder for total files
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal, leaveOpen: true);
    
        for (int i = 0; i < files.Length; i += batchSize)
        {
            var batch = files.Skip(i).Take(batchSize);
            var fileDataList = new List<(string RelativePath, byte[] Data)>();
    
            foreach (var file in batch)
            {
                if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(file)))
                    continue;
    
                var relativePath = Path.GetRelativePath(sourceDirectoryPath, file);
                var data = File.ReadAllBytes(file);
                fileDataList.Add((relativePath, data));
            }
    
            foreach (var (relativePath, data) in fileDataList)
            {
                current++;
                if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || current == total)
                {
                    ResourcePackManager.PrintProgressBar(current, total, debugWatch.Elapsed.TotalSeconds);
                    sw.Restart();
                }
    
                var pathBytes = System.Text.Encoding.UTF8.GetBytes(relativePath);
                gzipStream.Write(BitConverter.GetBytes(pathBytes.Length), 0, 4);
                gzipStream.Write(pathBytes, 0, pathBytes.Length);
                gzipStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                gzipStream.Write(data, 0, data.Length);
    
                totalFilesPacked++;
                totalBytesPacked += data.Length;
                if (debug) debugMessages.Add($"Packed {relativePath} ({data.Length} bytes)");
            }
        }
    
        fileStream.Position = 0;
        fileStream.Write(BitConverter.GetBytes(totalFilesPacked), 0, 4);
    
        if (debugMessages.Count > 0) foreach (var msg in debugMessages) Console.WriteLine(msg);
        Console.WriteLine($"Packing finished. {totalFilesPacked} files packed to {outputFilePath} in {debugWatch.Elapsed.TotalSeconds} seconds. Total bytes packed: {totalBytesPacked}");
        return true;
    }
    
    
    public static bool Unpack(string outputDirectoryPath, string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Packed file not found: {sourceFilePath}");
            return false;
        }
        if (!Directory.Exists(outputDirectoryPath))
        {
            Directory.CreateDirectory(outputDirectoryPath);
            Console.WriteLine($"Created output directory: {outputDirectoryPath}");
        }

        var debugWatch = Stopwatch.StartNew();
        
        var debugMessages = debug ? new List<string>() : [];
        
        using var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
        var totalFilesBytes = new byte[4];
        fileStream.ReadExactly(totalFilesBytes, 0, 4);
        int totalFiles = BitConverter.ToInt32(totalFilesBytes, 0);
        Console.WriteLine("Total files to unpack: " + totalFiles);
        
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        
        long totalBytesUnpacked = 0;
        int unpackedFiles = 0;
        int currentFile = 0;
        var sw = Stopwatch.StartNew();

        bool failed = false;
        while (currentFile < totalFiles)
        {
            var pathLenBytes = new byte[4];
            gzipStream.ReadExactly(pathLenBytes, 0, 4);
            int pathLen = BitConverter.ToInt32(pathLenBytes, 0);

            var pathBytes = new byte[pathLen];
            gzipStream.ReadExactly(pathBytes, 0, pathLen);
            string relativePath = System.Text.Encoding.UTF8.GetString(pathBytes);

            var dataLenBytes = new byte[4];
            gzipStream.ReadExactly(dataLenBytes, 0, 4);
            int dataLen = BitConverter.ToInt32(dataLenBytes, 0);

            var data = new byte[dataLen];
            gzipStream.ReadExactly(data, 0, dataLen);

            totalBytesUnpacked += dataLen;
            currentFile++;
            if (sw.Elapsed.TotalMilliseconds >= ResourcePackManager.ProgressIntervalMilliseconds || currentFile == totalFiles)
            {
                ResourcePackManager.PrintProgressBar(currentFile, totalFiles, debugWatch.Elapsed.TotalSeconds);
                sw.Restart();
            }
            //this needs to come after reading data to avoid breaking the stream
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                if (debug) debugMessages.Add($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                continue;
            }

            var filePath = Path.Combine(outputDirectoryPath, relativePath);
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath != null && !Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            File.WriteAllBytes(filePath, data);
            unpackedFiles++;
            if (debug) debugMessages.Add($"Unpacked {relativePath} ({dataLen} bytes)");
        }

        if (debugMessages.Count > 0)
        {
            foreach (var msg in debugMessages)
            {
                Console.WriteLine(msg);
            }
        }

        if (failed)
        {
            Console.WriteLine($"Unpacking terminated due to errors after {debugWatch.Elapsed.TotalSeconds:F2} seconds.");
            return false;
        }
        
        Console.WriteLine($"Unpacking finished. {unpackedFiles} files unpacked to {outputDirectoryPath} in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return true;
    }
    
    #endregion

    #region External Unpacking
    
    //TODO: add async versions
    
    public static Dictionary<string, byte[]> UnpackFromFile(string sourceFilePath, List<string>? extensionExceptions = null, bool debug = false)
    {
        var result = new Dictionary<string, byte[]>();

        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Packed file not found: {sourceFilePath}");
            return result;
        }

        var debugWatch = Stopwatch.StartNew();
        long totalBytesUnpacked = 0;
        int unpackedFiles = 0;

        using var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
        var totalFilesBytes = new byte[4];
        fileStream.ReadExactly(totalFilesBytes, 0, 4);
        int totalFiles = BitConverter.ToInt32(totalFilesBytes, 0);

        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

        for (int i = 0; i < totalFiles; i++)
        {
            var pathLenBytes = new byte[4];
            gzipStream.ReadExactly(pathLenBytes, 0, 4);
            int pathLen = BitConverter.ToInt32(pathLenBytes, 0);

            var pathBytes = new byte[pathLen];
            gzipStream.ReadExactly(pathBytes, 0, pathLen);
            string relativePath = System.Text.Encoding.UTF8.GetString(pathBytes);

            var dataLenBytes = new byte[4];
            gzipStream.ReadExactly(dataLenBytes, 0, 4);
            int dataLen = BitConverter.ToInt32(dataLenBytes, 0);

            var data = new byte[dataLen];
            gzipStream.ReadExactly(data, 0, dataLen);

            //this needs to come after reading data to avoid breaking the stream
            if (extensionExceptions is { Count: > 0 } && extensionExceptions.Contains(Path.GetExtension(relativePath)))
            {
                if (debug) Console.WriteLine($"File skipped due to extension: {Path.GetFileName(relativePath)}");
                continue;
            }

            result[relativePath] = data;
            totalBytesUnpacked += dataLen;
            unpackedFiles++;
            if (debug) Console.WriteLine($"Unpacked {relativePath} ({dataLen} bytes)");
        }

        Console.WriteLine($"Unpacking from packed file {sourceFilePath} finished. {unpackedFiles} files unpacked in {debugWatch.Elapsed.TotalSeconds:F2} seconds. Total bytes unpacked: {totalBytesUnpacked}");
        return result;
    }
    #endregion
}