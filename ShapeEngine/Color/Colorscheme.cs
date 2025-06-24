namespace ShapeEngine.Color;

/// <summary>
/// Represents a color scheme, which is a list of <see cref="PaletteColor"/> objects with a unique ID.
/// </summary>
public class ColorScheme : List<PaletteColor>
{
    private static int idCounter;
    private static int GetNextId() => idCounter++;

    /// <summary>
    /// The unique identifier for this color scheme.
    /// </summary>
    public readonly int Id;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorScheme"/> class with a unique ID.
    /// </summary>
    public ColorScheme()
    {
        Id = GetNextId();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorScheme"/> class with a specified ID.
    /// </summary>
    /// <param name="id">The ID to assign to this color scheme.</param>
    public ColorScheme(int id)
    {
        Id = id;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorScheme"/> class from a collection of <see cref="PaletteColor"/> objects.
    /// </summary>
    /// <param name="colors">The colors to include in the scheme.</param>
    public ColorScheme(IEnumerable<PaletteColor> colors) : base(colors)
    {
        Id = GetNextId();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorScheme"/> class from an array of <see cref="PaletteColor"/> objects.
    /// </summary>
    /// <param name="colors">The colors to include in the scheme.</param>
    public ColorScheme(params PaletteColor[] colors) : base(colors)
    {
        Id = GetNextId();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorScheme"/> class with a specified ID and a collection of <see cref="PaletteColor"/> objects.
    /// </summary>
    /// <param name="id">The ID to assign to this color scheme.</param>
    /// <param name="colors">The colors to include in the scheme.</param>
    public ColorScheme(int id, IEnumerable<PaletteColor> colors) : base(colors)
    {
        Id = id;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorScheme"/> class with a specified ID and an array of <see cref="PaletteColor"/> objects.
    /// </summary>
    /// <param name="id">The ID to assign to this color scheme.</param>
    /// <param name="colors">The colors to include in the scheme.</param>
    public ColorScheme(int id, params PaletteColor[] colors) : base(colors)
    {
        Id = id;
    }

    /// <summary>
    /// Generates a <see cref="ColorScheme"/> from arrays of integer color values and color IDs.
    /// </summary>
    /// <param name="colors">An array of integer color values (hexadecimal).</param>
    /// <param name="colorIDs">An array of color IDs to assign to the palette colors.</param>
    /// <returns>A new <see cref="ColorScheme"/> containing the generated colors.</returns>
    public static ColorScheme Generate(int[] colors, params int[] colorIDs)
    {
        if (colors.Length <= 0 || colorIDs.Length <= 0) return new();
        List<PaletteColor> container = new();
        int size = colors.Length;
        if (colorIDs.Length < size) size = colorIDs.Length;
        for (int i = 0; i < size; i++)
        {
            var paletteColor = new PaletteColor(colorIDs[i], ColorRgba.FromHex(colors[i]));
            container.Add(paletteColor);
        }
        return new(container);
    }

    /// <summary>
    /// Generates a <see cref="ColorScheme"/> from arrays of hexadecimal color strings and color IDs.
    /// </summary>
    /// <param name="hexColors">An array of hexadecimal color strings.</param>
    /// <param name="colorIDs">An array of color IDs to assign to the palette colors.</param>
    /// <returns>A new <see cref="ColorScheme"/> containing the generated colors.</returns>
    public static ColorScheme Generate(string[] hexColors, params int[] colorIDs)
    {
        if (hexColors.Length <= 0 || colorIDs.Length <= 0) return new();
        List<PaletteColor> container = new();
        int size = hexColors.Length;
        if (colorIDs.Length < size) size = colorIDs.Length;
        for (int i = 0; i < size; i++)
        {
            var paletteColor = new PaletteColor(colorIDs[i], ColorRgba.FromHex(hexColors[i]));
            container.Add(paletteColor);
        }
        return new(container);
    }
        
}