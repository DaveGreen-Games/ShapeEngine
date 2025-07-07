namespace ShapeEngine.Core;

public partial class Game
{
    /// <summary>
    /// Use the writeAction to write to the text file.
    /// </summary>
    /// <param name="path">The path were the file should be. A new one is created if it does not exist.</param>
    /// <param name="fileName">The name of the file. Needs a valid extension.</param>
    /// <param name="writeAction">The function that is called with the active StreamWriter. Use Write/ WriteLine functions to write.</param>
    /// <exception cref="ArgumentException">Filename has no valid extension.</exception>
    public static void WriteToFile(string path, string fileName, Action<StreamWriter> writeAction)
    {
        if (!Path.HasExtension(fileName))
        {
            throw new ArgumentException("File name must have a valid extension.");
        }

        try
        {
            var fullPath = Path.Combine(path, fileName);
            using (var writer = new StreamWriter(fullPath))
            {
                writeAction(writer);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
    }

    /// <summary>
    /// Use the readAction to read from the file.
    /// </summary>
    /// <param name="path">The path were the file should be. A new one is created if it does not exist.</param>
    /// <param name="fileName">The name of the file. Needs a valid extension.</param>
    /// <param name="readAction">The function that is called with the active StreamReader. Use Read/ ReadLine functions to read.</param>
    /// <exception cref="ArgumentException">Filename has no valid extension.</exception>
    public static void ReadFromFile(string path, string fileName, Action<StreamReader> readAction)
    {
        if (!Path.HasExtension(fileName))
        {
            throw new ArgumentException("File name must have a valid extension.");
        }

        var fullPath = Path.Combine(path, fileName);
        try
        {
            // Open the text file using a StreamReader.
            using (StreamReader sr = new StreamReader(fullPath))
            {
                readAction(sr);
            }
        }
        catch (Exception e)
        {
            // Print any errors to the console.
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
    }

    /// <summary>
    /// Attempts to parse a string value into the specified enum type.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse the string value into.</typeparam>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">When this method returns, contains the parsed enum value if the parsing succeeded, or the default value if parsing failed.</param>
    /// <returns>True if the string was successfully parsed into an enum value; otherwise, false.</returns>
    public static bool TryParseEnum<TEnum>(string value, out TEnum result) where TEnum : struct
    {
        if (typeof(TEnum).IsEnum)
        {
            if (Enum.TryParse(value, true, out TEnum parsedValue))
            {
                result = parsedValue;
                return true;
            }
        }

        result = default(TEnum);
        return false;
    }

}