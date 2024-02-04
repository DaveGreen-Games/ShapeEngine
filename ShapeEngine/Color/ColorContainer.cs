namespace ShapeEngine.Color;

public class ColorContainer
{
    private readonly PaletteColor[] colors;

    public ColorContainer(params PaletteColor[] colors)
    {
        this.colors = colors;
    }
    public ColorContainer(IEnumerable<PaletteColor> colors)
    {
        this.colors = colors.ToArray();
    }
    public ColorContainer(Dictionary<int, ShapeColor> colors)
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
            target.Color = source.Color;
            return;
        }
    }
    public void Apply(IColorPalette palette)
    {
        var targetColors = palette.GetColors();
        foreach (var c in targetColors)
        {
            ApplyColor(c);
        }
    }
        
    public static ColorContainer Generate(int[] colors, params int[] colorIDs)
    {
        if (colors.Length <= 0 || colorIDs.Length <= 0) return new();
        List<PaletteColor> container = new();
        int size = colors.Length;
        if (colorIDs.Length < size) size = colorIDs.Length;
        for (int i = 0; i < size; i++)
        {
            var paletteColor = new PaletteColor(colorIDs[i], ShapeColor.FromHex(colors[i]));
            container.Add(paletteColor);
        }
        return new(container);
    }
    public static ColorContainer Generate(string[] hexColors, params int[] colorIDs)
    {
        if (hexColors.Length <= 0 || colorIDs.Length <= 0) return new();
        List<PaletteColor> container = new();
        int size = hexColors.Length;
        if (colorIDs.Length < size) size = colorIDs.Length;
        for (int i = 0; i < size; i++)
        {
            var paletteColor = new PaletteColor(colorIDs[i], ShapeColor.FromHex(hexColors[i]));
            container.Add(paletteColor);
        }
        return new(container);
    }
}