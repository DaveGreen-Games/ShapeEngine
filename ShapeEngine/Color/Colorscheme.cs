
namespace ShapeEngine.Color;

public class ColorScheme : List<PaletteColor>
{
    private static int IdCounter = 0;
    private static int GetNextId() => IdCounter++;

    public readonly int Id;

    public ColorScheme() : base()
    {
        Id = GetNextId();
    }
    public ColorScheme(int id) : base()
    {
        Id = id;
    }
    public ColorScheme(IEnumerable<PaletteColor> colors) : base(colors)
    {
        Id = GetNextId();
    }
    public ColorScheme(params PaletteColor[] colors) : base(colors)
    {
        Id = GetNextId();
    }
    public ColorScheme(int id, IEnumerable<PaletteColor> colors) : base(colors)
    {
        Id = id;
    }
    public ColorScheme(int id, params PaletteColor[] colors) : base(colors)
    {
        Id = id;
    }
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


// public class Colorscheme
// {
//     // private readonly PaletteColor[] colors;
//     private readonly Dictionary<int, PaletteColor>? colors;
//
//     private Colorscheme()
//     {
//         // colors = new Dictionary<int, PaletteColor>(); // Array.Empty<PaletteColor>();
//         colors = null;
//     }
//     public Colorscheme(params PaletteColor[] paletteColors)
//     {
//         if (paletteColors.Length <= 0)
//         {
//             colors = null;
//             return;
//         }
//         
//         this.colors = new();
//         foreach (var color in paletteColors)
//         {
//             colors.TryAdd(color.ID, color.Clone());
//         }
//     }
//     public Colorscheme(params ColorRgba[] paletteColors)
//     {
//         if (paletteColors.Length <= 0)
//         {
//             colors = null;
//             return;
//         }
//         
//         this.colors = new();
//         for (int i = 0; i < paletteColors.Length; i++)
//         {
//             var color = paletteColors[i];
//             colors.TryAdd(i, new PaletteColor(i, color));
//         }
//         // foreach (var color in paletteColors)
//         // {
//         //     var pc = new PaletteColor(color);
//         //     colors.TryAdd(pc.ID, pc);
//         // }
//     }
//     public Colorscheme(IEnumerable<ColorRgba> paletteColors)
//     {
//         int index = 0;
//         foreach (var color in paletteColors)
//         {
//             colors ??= new();
//             colors.TryAdd(index, new PaletteColor(index, color));
//             index++;
//         }
//     }
//     public Colorscheme(IEnumerable<PaletteColor> paletteColors)
//     {
//         foreach (var color in paletteColors)
//         {
//             colors ??= new();
//             colors.TryAdd(color.ID, color.Clone());
//         }
//     }
//     public Colorscheme(Dictionary<int, ColorRgba> paletteColors)
//     {
//         if (paletteColors.Count <= 0)
//         {
//             colors = null;
//             return;
//         }
//
//         
//         this.colors = new();
//         foreach (var color in paletteColors)
//         {
//             this.colors.Add(color.Key, new PaletteColor(color.Key, color.Value));
//         }
//         // this.colors = new PaletteColor[colors.Count];
//         // int index = 0;
//         // foreach (var kvp in colors)
//         // {
//         //     this.colors[index] = new PaletteColor(kvp.Key, kvp.Value);
//         //     index++;
//         // }
//     }
//
//     private void ApplyColor(PaletteColor target)
//     {
//         if (colors is not { Count: > 0 }) return;
//         
//         if (colors.TryGetValue(target.ID, out var color))
//         {
//             target.ColorRgba = color.ColorRgba;
//         }
//     }
//     public void Apply(IColorPalette palette)
//     {
//         if (colors is not { Count: > 0 }) return;
//         
//         var targetColors = palette.GetColors();
//         foreach (var c in targetColors)
//         {
//             ApplyColor(c);
//         }
//     }
//     
//     public bool HasColors() => colors is { Count: > 0 };
//
//     public List<PaletteColor>? GetCurrentPaletteColors()
//     {
//         if (colors == null || colors.Count <= 0) return null;
//         
//         var currentColors = new List<PaletteColor>();
//         foreach (var color in colors.Values)
//         {
//             currentColors.Add(color.Clone());
//         }
//         return currentColors;
//     }
//     public List<ColorRgba>? GetCurrentColors()
//     {
//         if (colors == null || colors.Count <= 0) return null;
//         
//         var currentColors = new List<ColorRgba>();
//         foreach (var color in colors.Values)
//         {
//             currentColors.Add(color.ColorRgba);
//         }
//         return currentColors;
//     }
//     
//     
//     public static Colorscheme Generate(int[] colors, params int[] colorIDs)
//     {
//         if (colors.Length <= 0 || colorIDs.Length <= 0) return new();
//         List<PaletteColor> container = new();
//         int size = colors.Length;
//         if (colorIDs.Length < size) size = colorIDs.Length;
//         for (int i = 0; i < size; i++)
//         {
//             var paletteColor = new PaletteColor(colorIDs[i], ColorRgba.FromHex(colors[i]));
//             container.Add(paletteColor);
//         }
//         return new(container);
//     }
//     public static Colorscheme Generate(string[] hexColors, params int[] colorIDs)
//     {
//         if (hexColors.Length <= 0 || colorIDs.Length <= 0) return new();
//         List<PaletteColor> container = new();
//         int size = hexColors.Length;
//         if (colorIDs.Length < size) size = colorIDs.Length;
//         for (int i = 0; i < size; i++)
//         {
//             var paletteColor = new PaletteColor(colorIDs[i], ColorRgba.FromHex(hexColors[i]));
//             container.Add(paletteColor);
//         }
//         return new(container);
//     }
// }