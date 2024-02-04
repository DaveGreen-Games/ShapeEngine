using ShapeEngine.Color;

namespace Examples;

public static class Colors
{
    internal class Palette : IColorPalette
    {
        private readonly List<PaletteColor> colors;

        public Palette(params PaletteColor[] colors)
        {
            this.colors = colors.ToList();
        }
        public List<PaletteColor> GetColors() => colors;
    }
       
       
    public static ShapeColor Background      => background.Color;
    public static ShapeColor Dark       => dark.Color; 
    public static ShapeColor Medium     => medium.Color; 
    public static ShapeColor Light      => light.Color; 
    public static ShapeColor Text => text.Color; 
    public static ShapeColor Highlight => highlight.Color; 
    public static ShapeColor Special => special.Color;
    public static ShapeColor Warm   => warm.Color;
    public static ShapeColor Cold   => cold.Color;

    //public static readonly PaletteColor BackgroundP = new();
       
    private static readonly PaletteColor background = new PaletteColor(0, new(System.Drawing.Color.DarkSlateGray));
    private static readonly PaletteColor dark = new PaletteColor(1, new(System.Drawing.Color.DimGray));
    private static readonly PaletteColor medium = new PaletteColor(2, new(System.Drawing.Color.DarkGray));
    private static readonly PaletteColor light = new PaletteColor(3, new(System.Drawing.Color.LightGray));
    private static readonly PaletteColor text = new PaletteColor(4, new(System.Drawing.Color.AntiqueWhite));
    private static readonly PaletteColor highlight = new PaletteColor(5, new(System.Drawing.Color.Aquamarine));
    private static readonly PaletteColor special = new PaletteColor(6, new(System.Drawing.Color.Coral));
    private static readonly PaletteColor warm = new PaletteColor(7, new(System.Drawing.Color.IndianRed));
    private static readonly PaletteColor cold = new PaletteColor(8, new(System.Drawing.Color.CornflowerBlue));
       
    private static readonly Palette colorPalette = new
    (
        background, 
        dark, medium, light, 
        text,
        highlight, special,
        warm, cold
    );

    public static readonly Colorscheme DefaultColorscheme = new
    (
        new ShapeColor(System.Drawing.Color.DarkSlateGray),
        new ShapeColor(System.Drawing.Color.DimGray),
        new ShapeColor(System.Drawing.Color.DarkGray),
        new ShapeColor(System.Drawing.Color.LightGray),
        new ShapeColor(System.Drawing.Color.AntiqueWhite),
        new ShapeColor(System.Drawing.Color.Aquamarine),
        new ShapeColor(System.Drawing.Color.Coral),
        new ShapeColor(System.Drawing.Color.IndianRed),
        new ShapeColor(System.Drawing.Color.CornflowerBlue)
    );
        
    public static readonly Colorscheme WarmColorscheme = new
    (
        new ShapeColor(System.Drawing.Color.DarkRed),
        new ShapeColor(System.Drawing.Color.SaddleBrown),
        new ShapeColor(System.Drawing.Color.Sienna),
        new ShapeColor(System.Drawing.Color.Salmon),
        new ShapeColor(System.Drawing.Color.Tomato),
        new ShapeColor(System.Drawing.Color.OrangeRed),
        new ShapeColor(System.Drawing.Color.HotPink),
        new ShapeColor(System.Drawing.Color.Crimson),
        new ShapeColor(System.Drawing.Color.Orchid)
    );
       
    public static readonly Colorscheme ColdColorscheme = new
    (
        new ShapeColor(System.Drawing.Color.Navy),
        new ShapeColor(System.Drawing.Color.DarkSlateBlue),
        new ShapeColor(System.Drawing.Color.SlateBlue),
        new ShapeColor(System.Drawing.Color.LightSteelBlue),
        new ShapeColor(System.Drawing.Color.AliceBlue),
        new ShapeColor(System.Drawing.Color.Aqua),
        new ShapeColor(System.Drawing.Color.Aquamarine),
        new ShapeColor(System.Drawing.Color.GreenYellow),
        new ShapeColor(System.Drawing.Color.RoyalBlue)
    );
       
    public static void ApplyColorscheme(Colorscheme cc) => cc.Apply(colorPalette);
}