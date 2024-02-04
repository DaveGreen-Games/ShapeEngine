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
       
       
    public static ShapeColor Background      => PcBackground.Color;
    public static ShapeColor Dark       => PcDark.Color; 
    public static ShapeColor Medium     => PcMedium.Color; 
    public static ShapeColor Light      => PcLight.Color; 
    public static ShapeColor Text => PcText.Color; 
    public static ShapeColor Highlight => PcHighlight.Color; 
    public static ShapeColor Special => PcSpecial.Color;
    public static ShapeColor Warm   => PcWarm.Color;
    public static ShapeColor Cold   => PcCold.Color;

    //public static readonly PaletteColor BackgroundP = new();
       
    public static readonly PaletteColor PcBackground = new(0, new ShapeColor(System.Drawing.Color.DarkSlateGray));
    public static readonly PaletteColor PcDark = new(1, new(System.Drawing.Color.DimGray));
    public static readonly PaletteColor PcMedium = new(2, new(System.Drawing.Color.DarkGray));
    public static readonly PaletteColor PcLight = new(3, new(System.Drawing.Color.LightGray));
    public static readonly PaletteColor PcText = new(4, new(System.Drawing.Color.AntiqueWhite));
    public static readonly PaletteColor PcHighlight = new(5, new(System.Drawing.Color.Aquamarine));
    public static readonly PaletteColor PcSpecial = new(6, new(System.Drawing.Color.Coral));
    public static readonly PaletteColor PcWarm = new(7, new(System.Drawing.Color.IndianRed));
    public static readonly PaletteColor PcCold = new(8, new(System.Drawing.Color.CornflowerBlue));
       
    private static readonly Palette colorPalette = new
    (
        PcBackground, 
        PcDark, PcMedium, PcLight, 
        PcText,
        PcHighlight, PcSpecial,
        PcWarm, PcCold
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