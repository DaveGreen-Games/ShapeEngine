namespace ShapeEngine.Color;

/// <summary>
/// Represents a palette of colors, supporting color scheme application and color management.
/// Each color is stored as a <see cref="PaletteColor"/> with a unique ID (int).
/// You can apply a <see cref="ColorScheme"/> or a list of <see cref="PaletteColor"/> to this <see cref="ColorPalette"/>,
/// which sets every <see cref="PaletteColor"/> contained in this palette to a matching <see cref="PaletteColor"/> in the scheme or list.
/// </summary>
public class ColorPalette
{
    /// <summary>
    /// Event triggered when a color scheme is applied.
    /// </summary>
    public event Action<ColorScheme>? OnColorSchemeApplied;
    /// <summary>
    /// The currently applied color scheme, if any.
    /// </summary>
    public ColorScheme? CurrentColorScheme { get; private set; }

    private readonly Dictionary<int, PaletteColor> colors = new();
        
    #region Constructors
    /// <summary>
    /// Initializes a new empty <see cref="ColorPalette"/>.
    /// </summary>
    public ColorPalette()
    {
            
    }
    /// <summary>
    /// Initializes a new <see cref="ColorPalette"/> with a specified number of random colors.
    /// </summary>
    /// <param name="colorCount">The number of colors to add to the palette.</param>
    public ColorPalette(int colorCount)
    {
        for (int i = 0; i < colorCount; i++)
        {
            AddColor();
        }
    }
    /// <summary>
    /// Initializes a new <see cref="ColorPalette"/> from a collection of <see cref="ColorRgba"/> colors.
    /// </summary>
    /// <param name="colors">The collection of colors to add.</param>
    public ColorPalette(IEnumerable<ColorRgba> colors)
    {
        foreach (var color in colors)
        {
            AddColor(color);
        }
    }
    /// <summary>
    /// Initializes a new <see cref="ColorPalette"/> from a collection of <see cref="PaletteColor"/> objects.
    /// </summary>
    /// <param name="colors">The collection of palette colors to add.</param>
    public ColorPalette(IEnumerable<PaletteColor> colors)
    {
        foreach (var color in colors)
        {
            AddColor(color);
        }
    }
    /// <summary>
    /// Initializes a new <see cref="ColorPalette"/> from an array of <see cref="PaletteColor"/> objects.
    /// </summary>
    /// <param name="colors">The palette colors to add.</param>
    public ColorPalette(params PaletteColor[] colors)
    {
        foreach (var color in colors)
        {
            AddColor(color);
        }
    }
    /// <summary>
    /// Initializes a new <see cref="ColorPalette"/> from an array of <see cref="ColorRgba"/> colors.
    /// </summary>
    /// <param name="colors">The colors to add.</param>
    public ColorPalette(params ColorRgba[] colors)
    {
        foreach (var color in colors)
        {
            AddColor(color);
        }
    }
    /// <summary>
    /// Initializes a new <see cref="ColorPalette"/> from a <see cref="ColorScheme"/>.
    /// </summary>
    /// <param name="colorScheme">The color scheme to use for the palette.</param>
    public ColorPalette(ColorScheme colorScheme)
    {
        foreach (var color in colorScheme)
        {
            AddColor(color.Clone());
        }
    }
    #endregion
        
    #region Public methods
    /// <summary>
    /// Adds a new random color to the palette.
    /// </summary>
    /// <returns>The ID of the added color, or -1 if the color could not be added.</returns>
    public int AddColor()
    {
        var pc = new PaletteColor();
        if (colors.TryAdd(pc.ID, pc))
        {
            ColorWasAdded(pc);
            return pc.ID;
        }
        return -1;
    }
    /// <summary>
    /// Adds a <see cref="PaletteColor"/> to the palette.
    /// </summary>
    /// <param name="color">The palette color to add.</param>
    /// <returns>True if the color was added; otherwise, false.</returns>
    public bool AddColor(PaletteColor color)
    {
        var added = colors.TryAdd(color.ID, color);
        if(added) ColorWasAdded(color);
        return added;
    }
    /// <summary>
    /// Adds a <see cref="ColorRgba"/> to the palette.
    /// </summary>
    /// <param name="color">The color to add.</param>
    /// <returns>The ID of the added color, or -1 if the color could not be added.</returns>
    public int AddColor(ColorRgba color)
    {
        var pc = new PaletteColor(color);
        if (colors.TryAdd(pc.ID, pc))
        {
            ColorWasAdded(pc);
            return pc.ID;
        }
        return -1;
    }
    /// <summary>
    /// Gets the number of colors in the palette.
    /// </summary>
    /// <returns>The number of colors in the palette.</returns>
    public int GetColorCount() => colors.Count;
    /// <summary>
    /// Determines whether the palette contains a color with the specified ID.
    /// </summary>
    /// <param name="colorId">The ID of the color to check.</param>
    /// <returns>True if the color exists; otherwise, false.</returns>
    public bool HasColor(int colorId) => colors.ContainsKey(colorId);
    /// <summary>
    /// Gets the <see cref="PaletteColor"/> with the specified ID.
    /// </summary>
    /// <param name="colorId">The ID of the color to retrieve.</param>
    /// <returns>The <see cref="PaletteColor"/> if found; otherwise, null.</returns>
    public PaletteColor? GetColor(int colorId) => colors.GetValueOrDefault(colorId);
    /// <summary>
    /// Removes the color with the specified ID from the palette.
    /// </summary>
    /// <param name="colorId">The ID of the color to remove.</param>
    /// <returns>True if the color was removed; otherwise, false.</returns>
    public bool RemoveColor(int colorId)
    {
        if (colors.Remove(colorId, out var pc))
        {
            ColorWasRemoved(pc);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets a list of all <see cref="PaletteColor"/> objects in the palette.
    /// </summary>
    /// <returns>A list of palette colors, or null if the palette is empty.</returns>
    public List<PaletteColor>? GetPaletteColors() => colors.Count <= 0 ? null : colors.Values.ToList();

    /// <summary>
    /// Gets a copy of all <see cref="PaletteColor"/> objects in the palette.
    /// </summary>
    /// <returns>A list of cloned palette colors, or null if the palette is empty.</returns>
    public List<PaletteColor>? GetPaletteColorsCopy()
    {
        if (colors.Count <= 0) return null;
        var copy = new List<PaletteColor>(colors.Count);
        foreach (var color in colors.Values)
        {
            copy.Add(color.Clone());
        }

        return copy;
    }
    /// <summary>
    /// Gets a list of all <see cref="ColorRgba"/> colors in the palette.
    /// </summary>
    /// <returns>A list of colors, or null if the palette is empty.</returns>
    public List<ColorRgba>? GetColors()
    {
        if (colors.Count <= 0) return null;
            
        var result = new List<ColorRgba>();
        foreach (var color in colors.Values)
        {
            result.Add(color.ColorRgba);
        }
        return result;
    }
    /// <summary>
    /// Gets a copy of the current color scheme as a <see cref="ColorScheme"/>.
    /// </summary>
    /// <returns>A copy of the color scheme, or null if the palette is empty.</returns>
    public ColorScheme? GetColorSchemeCopy()
    {
        if(colors.Count <= 0) return null;
        var result = new ColorScheme();
        foreach (var color in colors.Values)
        {
            result.Add(color.Clone());
        }

        return result;
    }

    /// <summary>
    /// Creates a deep copy of this <see cref="ColorPalette"/>.
    /// </summary>
    /// <returns>A new <see cref="ColorPalette"/> with copied colors.</returns>
    public ColorPalette Clone()
    {
        var copy = GetPaletteColorsCopy();
        if(copy == null) return new ColorPalette();
        return new ColorPalette(copy);
    }
    /// <summary>
    /// Applies a color scheme to the palette using a list of <see cref="PaletteColor"/>.
    /// </summary>
    /// <param name="scheme">The color scheme to apply.</param>
    /// <returns>True if the scheme was applied; otherwise, false.</returns>
    public bool ApplyColorScheme(List<PaletteColor> scheme)
    {
        if (colors.Count <= 0 || scheme.Count <= 0) return false;

        CurrentColorScheme = new ColorScheme(scheme);
            
        foreach (var color in scheme)
        {
            if (colors.TryGetValue(color.ID, out var paletteColor))
            {
                paletteColor.ColorRgba = color.ColorRgba;
            }
        }
        
        ColorSchemeWasApplied(CurrentColorScheme);
        OnColorSchemeApplied?.Invoke(CurrentColorScheme);
            
        return true;
    }
    /// <summary>
    /// Applies a <see cref="ColorScheme"/> to the palette.
    /// </summary>
    /// <param name="scheme">The color scheme to apply.</param>
    /// <returns>True if the scheme was applied; otherwise, false.</returns>
    public bool ApplyColorScheme(ColorScheme scheme)
    {
        if (colors.Count <= 0 || scheme.Count <= 0) return false;
            
        CurrentColorScheme = scheme;
            
        foreach (var color in scheme)
        {
            if (colors.TryGetValue(color.ID, out var paletteColor))
            {
                paletteColor.ColorRgba = color.ColorRgba;
            }
        }
        
        ColorSchemeWasApplied(scheme);
        OnColorSchemeApplied?.Invoke(scheme);
            
        return true;
    }
    #endregion

    #region Protected Virtual

    /// <summary>
    /// Called when a color is added to the palette.
    /// </summary>
    /// <param name="color">The color that was added.</param>
    protected virtual void ColorWasAdded(PaletteColor color) { }
    /// <summary>
    /// Called when a color is removed from the palette.
    /// </summary>
    /// <param name="color">The color that was removed.</param>
    protected virtual void ColorWasRemoved(PaletteColor color) { }
    /// <summary>
    /// Called when a color scheme is applied to the palette.
    /// </summary>
    /// <param name="colorScheme">The color scheme that was applied.</param>
    protected virtual void ColorSchemeWasApplied(ColorScheme colorScheme) { }

    #endregion
}