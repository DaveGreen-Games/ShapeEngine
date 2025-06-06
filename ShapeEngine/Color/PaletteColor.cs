namespace ShapeEngine.Color;

/// <summary>
/// Represents a color in a palette, with a unique ID and a <see cref="ColorRgba"/> value.
/// </summary>
public class PaletteColor
{
    private static int idCounter;
    private static int GetNextId() => idCounter++;
    
    /// <summary>
    /// The RGBA color value of this palette color.
    /// </summary>
    public ColorRgba ColorRgba { get; internal set; }
    /// <summary>
    /// The unique identifier for this palette color.
    /// </summary>
    public readonly int ID;

    /// <summary>
    /// Initializes a new <see cref="PaletteColor"/> with a random color and a unique ID.
    /// </summary>
    public PaletteColor()
    {
        this.ID = GetNextId();
        ColorRgba = new(); //randomize
    }
    /// <summary>
    /// Initializes a new <see cref="PaletteColor"/> with the specified color and a unique ID.
    /// </summary>
    /// <param name="colorRgba">The color value to assign.</param>
    public PaletteColor(ColorRgba colorRgba)
    {
        this.ID = GetNextId();
        this.ColorRgba = colorRgba;
    }
    /// <summary>
    /// Initializes a new <see cref="PaletteColor"/> with the specified ID and color.
    /// </summary>
    /// <param name="id">The ID to assign to this palette color.</param>
    /// <param name="colorRgba">The color value to assign.</param>
    public PaletteColor(int id, ColorRgba colorRgba)
    {
        this.ID = id;
        this.ColorRgba = colorRgba;
    }
    
    /// <summary>
    /// Creates a copy of this <see cref="PaletteColor"/> with the same ID and color.
    /// </summary>
    /// <returns>A new <see cref="PaletteColor"/> instance with the same ID and color.</returns>
    public PaletteColor Clone() => new(this.ID, this.ColorRgba);

    /// <summary>
    /// Creates a copy of this <see cref="PaletteColor"/> with the same ID and a specified color.
    /// </summary>
    /// <param name="colorRgba">The color value for the clone.</param>
    /// <returns>A new <see cref="PaletteColor"/> instance with the same ID and the specified color.</returns>
    public PaletteColor Clone(ColorRgba colorRgba) => new(this.ID, colorRgba);
        
}