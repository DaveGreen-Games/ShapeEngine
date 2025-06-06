using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Screen;

/// <summary>
/// Represents information about a monitor, including its name, dimensions, refresh rate, index, and position.
/// </summary>
public struct MonitorInfo
{
    /// <summary>
    /// Indicates whether the monitor is available.
    /// </summary>
    public bool Available = false;

    /// <summary>
    /// The name of the monitor.
    /// </summary>
    public string Name = "";

    /// <summary>
    /// The dimensions (width and height) of the monitor.
    /// </summary>
    public Dimensions Dimensions = new();

    /// <summary>
    /// The refresh rate of the monitor.
    /// </summary>
    public int Refreshrate = -1;

    /// <summary>
    /// The index of the monitor.
    /// </summary>
    public int Index = -1;

    /// <summary>
    /// The position of the monitor in screen coordinates.
    /// </summary>
    public Vector2 Position = new();

    /// <summary>
    /// Gets the width of the monitor.
    /// </summary>
    public int Width { get { return Dimensions.Width; } }

    /// <summary>
    /// Gets the height of the monitor.
    /// </summary>
    public int Height { get { return Dimensions.Height; } }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitorInfo"/> struct with default values.
    /// </summary>
    public MonitorInfo()
    {
        this.Available = false;
        this.Name = "";
        this.Dimensions = new(-1, -1);
        this.Refreshrate = -1;
        this.Index = -1;
        this.Position = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitorInfo"/> struct with the specified values.
    /// </summary>
    /// <param name="name">The name of the monitor.</param>
    /// <param name="w">The width of the monitor.</param>
    /// <param name="h">The height of the monitor.</param>
    /// <param name="pos">The position of the monitor.</param>
    /// <param name="refreshrate">The refresh rate of the monitor.</param>
    /// <param name="index">The index of the monitor.</param>
    public MonitorInfo(string name, int w, int h, Vector2 pos, int refreshrate, int index)
    {
        this.Available = true;
        this.Name = name;
        this.Dimensions = new(w, h);
        this.Refreshrate = refreshrate;
        this.Index = index;
        this.Position = pos;
    }

    /// <summary>
    /// Writes debug information about the monitor to the console.
    /// </summary>
    public void WriteDebugInfo()
    {
        Console.WriteLine("---Monitor Info---");
        Console.WriteLine($"{Name}: x({Dimensions.Width}) y({Dimensions.Height}) i({Index}) rr({Refreshrate}) ");
        Console.WriteLine("------------------");
    }
}