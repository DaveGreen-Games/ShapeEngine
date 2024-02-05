namespace ShapeEngine.Color;

public class Colorscheme
{
    private readonly PaletteColor[] colors;

    public Colorscheme()
    {
        colors = Array.Empty<PaletteColor>();
    }
    public Colorscheme(params PaletteColor[] colors)
    {
        this.colors = colors;
    }
    public Colorscheme(params ColorRgba[] colors)
    {
        var newColors = new PaletteColor[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            newColors[i] = new(i, colors[i]);
        }
        this.colors = newColors;
    }
    public Colorscheme(IEnumerable<ColorRgba> colors)
    {
        var newColors = new List<PaletteColor>();
        int index = 0;
        foreach (var c in colors)
        {
            newColors.Add(new(index, c));
            index++;
        }
        this.colors = newColors.ToArray();
    }
    public Colorscheme(IEnumerable<PaletteColor> colors)
    {
        this.colors = colors.ToArray();
    }
    public Colorscheme(Dictionary<int, ColorRgba> colors)
    {
        this.colors = new PaletteColor[colors.Count];
        int index = 0;
        foreach (var kvp in colors)
        {
            this.colors[index] = new PaletteColor(kvp.Key, kvp.Value);
            index++;
        }
    }

    private void ApplyColor(PaletteColor target)
    {
        foreach (var source in colors)
        {
            if (source.ID != target.ID) continue;
            target.ColorRgba = source.ColorRgba;
            return;
        }
    }
    public void Apply(IColorPalette palette)
    {
        if (colors.Length <= 0) return;
        var targetColors = palette.GetColors();
        foreach (var c in targetColors)
        {
            ApplyColor(c);
        }
    }
        
    public static Colorscheme Generate(int[] colors, params int[] colorIDs)
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
    public static Colorscheme Generate(string[] hexColors, params int[] colorIDs)
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