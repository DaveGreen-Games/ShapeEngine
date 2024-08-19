namespace ShapeEngine.Color;

public class ColorPalette
{
    public event Action<ColorScheme>? OnColorSchemeApplied;
    public ColorScheme? CurrentColorScheme { get; private set; } = null;

    private Dictionary<int, PaletteColor> colors = new();
        
    #region Constructors
    public ColorPalette()
    {
            
    }
    public ColorPalette(int colorCount)
    {
        for (int i = 0; i < colorCount; i++)
        {
            AddColor();
        }
    }
    public ColorPalette(IEnumerable<ColorRgba> colors)
    {
        foreach (var color in colors)
        {
            AddColor(color);
        }
    }
    public ColorPalette(IEnumerable<PaletteColor> colors)
    {
        foreach (var color in colors)
        {
            AddColor(color);
        }
    }
    public ColorPalette(params PaletteColor[] colors)
    {
        foreach (var color in colors)
        {
            AddColor(color);
        }
    }
    public ColorPalette(params ColorRgba[] colors)
    {
        foreach (var color in colors)
        {
            AddColor(color);
        }
    }
    public ColorPalette(ColorScheme colorScheme)
    {
        foreach (var color in colorScheme)
        {
            AddColor(color.Clone());
        }
    }
    #endregion
        
    #region Public methods
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
    public bool AddColor(PaletteColor color)
    {
        var added = colors.TryAdd(color.ID, color);
        if(added) ColorWasAdded(color);
        return added;
    }
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
        
    public int GetColorCount() => colors.Count;
    public bool HasColor(int colorId) => colors.ContainsKey(colorId);
    public PaletteColor? GetColor(int colorId) => colors.GetValueOrDefault(colorId);
    public bool RemoveColor(int colorId)
    {
        if (colors.Remove(colorId, out var pc))
        {
            ColorWasRemoved(pc);
            return true;
        }

        return false;
    }

    public List<PaletteColor>? GetPaletteColors() => colors.Count <= 0 ? null : colors.Values.ToList();
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

    public ColorPalette Clone()
    {
        var copy = GetPaletteColorsCopy();
        if(copy == null) return new ColorPalette();
        return new ColorPalette(copy);
    }
        
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

    protected virtual void ColorWasAdded(PaletteColor color) { }
    protected virtual void ColorWasRemoved(PaletteColor color) { }
    protected virtual void ColorSchemeWasApplied(ColorScheme colorScheme) { }

    #endregion
}