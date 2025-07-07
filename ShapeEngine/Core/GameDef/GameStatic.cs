using System.Runtime.InteropServices;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.GameDef;

public partial class Game
{
    /// <summary>
    /// Gets the singleton instance of the current game.
    /// </summary>
    /// <remarks>
    /// This property is initialized automatically in the constructor.
    /// You need to create a game instance before accessing this property.
    /// If it is accessed before being set, a NullReferenceException will be thrown.
    /// You should only ever create one game instance per application,
    /// but in case you create multiple, this property will always point to the last created game instance.
    /// </remarks>
    public static Game CurrentGameInstance { get; private set; } = null!;
    
    /// <summary>
    /// Gets the current directory where the application is running.
    /// </summary>
    /// <remarks>
    /// This property returns the base directory of the current application domain.
    /// It is equivalent to the value returned by <see cref="System.IO.Directory.GetCurrentDirectory"/> method.
    /// </remarks>
    public static readonly string CURRENT_DIRECTORY = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// Gets the current operating system platform.
    /// </summary>
    /// <remarks>
    /// The value is determined at runtime and can be one of the following:
    /// <list type="bullet">
    /// <item><description>OSPlatform.Windows</description></item>
    /// <item><description>OSPlatform.Linux</description></item>
    /// <item><description>OSPlatform.OSX</description></item>
    /// <item><description>OSPlatform.FreeBSD</description></item>
    /// </list>
    /// </remarks>
    public static OSPlatform OS_PLATFORM { get; private set; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatform.Windows :
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX :
        OSPlatform.FreeBSD;

    /// <summary>
    /// A static property that indicates whether the application is currently in debug mode.
    /// </summary>
    public static bool DebugMode { get; private set; } = false;

    /// <summary>
    /// A static property that indicates whether the application is currently in release mode.
    /// </summary>
    public static bool ReleaseMode { get; private set; } = true;

    /// <summary>
    /// A static method that checks if the current operating system is Windows.
    /// </summary>
    /// <returns>True if the operating system is Windows, otherwise false.</returns>
    public static bool IsWindows() => OS_PLATFORM == OSPlatform.Windows;

    /// <summary>
    /// A static method that checks if the current operating system is Linux.
    /// </summary>
    /// <returns>True if the operating system is Linux, otherwise false.</returns>
    public static bool IsLinux() => OS_PLATFORM == OSPlatform.Linux;

    /// <summary>
    /// A static method that checks if the current operating system is macOS.
    /// </summary>
    /// <returns>True if the operating system is macOS, otherwise false.</returns>
    public static bool IsOSX() => OS_PLATFORM == OSPlatform.OSX;

    /// <summary>
    /// Checks if the current operating system is macOS and if the application is running in an app bundle.
    /// </summary>
    /// <returns>
    /// Returns true if the operating system is macOS and the application is running in an app bundle.
    /// Returns false otherwise.
    /// </returns>
    public static bool OSXIsRunningInAppBundle()
    {
        if (!IsOSX()) return false;

        string exeDir = AppContext.BaseDirectory.Replace('\\', '/');
        return exeDir.Contains(".app/Contents/MacOS/");
    }

    /// <summary>
    /// Compares two lists for equality. The lists must contain elements of the same type that implement the IEquatable interface.
    /// </summary>
    /// <typeparam name="T">The type of elements in the lists. Must implement the IEquatable interface.</typeparam>
    /// <param name="a">The first list to compare.</param>
    /// <param name="b">The second list to compare.</param>
    /// <returns>
    /// Returns true if both lists are not null, have the same count, and all corresponding elements in the lists are equal. 
    /// Returns false otherwise.
    /// </returns>
    public static bool IsEqual<T>(List<T>? a, List<T>? b) where T : IEquatable<T>
    {
        if (a == null || b == null) return false;
        if (a.Count != b.Count) return false;
        for (var i = 0; i < a.Count; i++)
        {
            if (!a[i].Equals(b[i])) return false;
        }

        return true;
    }

    /// <summary>
    /// Computes and returns the hash code for a generic collection of elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection of elements to compute the hash code for.</param>
    /// <returns>The computed hash code for the collection.</returns>
    public static int GetHashCode<T>(IEnumerable<T> collection)
    {
        HashCode hash = new();
        foreach (var element in collection)
        {
            hash.Add(element);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Gets an item from a collection at the specified index, wrapping around if the index is out of range.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="collection">The collection to get the item from.</param>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item at the specified index, wrapped around if necessary.</returns>
    public static T GetItem<T>(List<T> collection, int index)
    {
        int i = ShapeMath.WrapIndex(collection.Count, index);
        return collection[i];
    }

}