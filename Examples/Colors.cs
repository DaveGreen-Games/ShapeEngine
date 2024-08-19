using ShapeEngine.Color;

namespace Examples;

public static class Colors
{
    public static ColorRgba Background      => PcBackground.ColorRgba;
    public static ColorRgba Dark       => PcDark.ColorRgba; 
    public static ColorRgba Medium     => PcMedium.ColorRgba; 
    public static ColorRgba Light      => PcLight.ColorRgba; 
    public static ColorRgba Text => PcText.ColorRgba; 
    public static ColorRgba Highlight => PcHighlight.ColorRgba; 
    public static ColorRgba Special => PcSpecial.ColorRgba;
    public static ColorRgba Special2 => PcSpecial2.ColorRgba;
    public static ColorRgba Warm   => PcWarm.ColorRgba;
    public static ColorRgba Cold   => PcCold.ColorRgba;

       
    public static readonly PaletteColor PcBackground = new(0, new ColorRgba(System.Drawing.Color.DarkSlateGray).ToHSL().ChangeLightness(-0.15f).ToRGB());
    public static readonly PaletteColor PcDark = new(1, new ColorRgba(System.Drawing.Color.DarkSlateGray).ToHSL().ChangeLightness(-0.1f).ToRGB());
    public static readonly PaletteColor PcMedium = new(2, new(System.Drawing.Color.DarkGray));
    public static readonly PaletteColor PcLight = new(3, new(System.Drawing.Color.LightGray));
    public static readonly PaletteColor PcText = new(4, new(System.Drawing.Color.AntiqueWhite));
    public static readonly PaletteColor PcHighlight = new(5, new(System.Drawing.Color.Aquamarine));
    public static readonly PaletteColor PcSpecial = new(6, new(System.Drawing.Color.Coral));
    public static readonly PaletteColor PcSpecial2 = new(7, new(System.Drawing.Color.Goldenrod));
    public static readonly PaletteColor PcWarm = new(8, new(System.Drawing.Color.IndianRed));
    public static readonly PaletteColor PcCold = new(9, new(System.Drawing.Color.CornflowerBlue));
    
    private static readonly ColorPalette colorPalette = new ColorPalette
    (
        PcBackground, 
        PcDark, PcMedium, PcLight, 
        PcText,
        PcHighlight, PcSpecial, PcSpecial2,
        PcWarm, PcCold
    );

    public static readonly ColorScheme DefaultColorscheme = new
    (
        PcBackground.Clone(new ColorRgba(System.Drawing.Color.DarkSlateGray).ToHSL().ChangeLightness(-0.15f).ToRGB()),
        PcDark.Clone(new ColorRgba(System.Drawing.Color.DimGray).ToHSL().ChangeLightness(-0.1f).ToRGB()),
        PcMedium.Clone(new ColorRgba(System.Drawing.Color.DarkGray)),
        PcLight.Clone(new ColorRgba(System.Drawing.Color.LightGray)),
        PcText.Clone(new ColorRgba(System.Drawing.Color.AntiqueWhite)),
        PcHighlight.Clone(new ColorRgba(System.Drawing.Color.Aquamarine)),
        PcSpecial.Clone(new ColorRgba(System.Drawing.Color.Coral)),
        PcSpecial2.Clone(new ColorRgba(System.Drawing.Color.Goldenrod)),
        PcWarm.Clone(new ColorRgba(System.Drawing.Color.IndianRed)),
        PcCold.Clone(new ColorRgba(System.Drawing.Color.CornflowerBlue))
    );
        
    public static readonly ColorScheme WarmColorscheme = new
    (
        PcBackground.Clone(new ColorRgba(System.Drawing.Color.DarkRed).ToHSL().ChangeLightness(-0.2f).ToRGB()),
        PcDark.Clone(new ColorRgba(System.Drawing.Color.SaddleBrown).ToHSL().ChangeLightness(-0.15f).ToRGB()),
        PcMedium.Clone(new ColorRgba(System.Drawing.Color.Sienna)),
        PcLight.Clone(new ColorRgba(System.Drawing.Color.Salmon)),
        PcText.Clone(new ColorRgba(System.Drawing.Color.Tomato)),
        PcHighlight.Clone(new ColorRgba(System.Drawing.Color.OrangeRed)),
        PcSpecial.Clone(new ColorRgba(System.Drawing.Color.HotPink)),
        PcSpecial2.Clone(new ColorRgba(System.Drawing.Color.PaleVioletRed)),
        PcWarm.Clone(new ColorRgba(System.Drawing.Color.Crimson)),
        PcCold.Clone(new ColorRgba(System.Drawing.Color.Orchid))
    );
       
    public static readonly ColorScheme ColdColorscheme = new
    (
        PcBackground.Clone(new ColorRgba(System.Drawing.Color.Navy).ToHSL().ChangeLightness(-0.2f).ToRGB()),
        PcDark.Clone(new ColorRgba(System.Drawing.Color.DarkSlateBlue).ToHSL().ChangeLightness(-0.15f).ToRGB()),
        PcMedium.Clone(new ColorRgba(System.Drawing.Color.SlateBlue)),
        PcLight.Clone(new ColorRgba(System.Drawing.Color.LightSteelBlue)),
        PcText.Clone(new ColorRgba(System.Drawing.Color.AliceBlue)),
        PcHighlight.Clone(new ColorRgba(System.Drawing.Color.Aqua)),
        PcSpecial.Clone(new ColorRgba(System.Drawing.Color.Aquamarine)),
        PcSpecial2.Clone(new ColorRgba(System.Drawing.Color.DeepSkyBlue)),
        PcWarm.Clone(new ColorRgba(System.Drawing.Color.GreenYellow)),
        PcCold.Clone(new ColorRgba(System.Drawing.Color.RoyalBlue))
    );
    
    
    private static readonly ColorScheme[] ColorSchemes = new[] { DefaultColorscheme, WarmColorscheme, ColdColorscheme };
    private static readonly string[] ColorschemeNames = new[] { "Default", "Warm", "Cold" };
    private static int curColorschemeIndex = 0;
    public static int CurColorschemeIndex
    {
        get => curColorschemeIndex;
        private set
        {
            if (value < 0) curColorschemeIndex = ColorSchemes.Length - 1;
            else if (value >= ColorSchemes.Length) curColorschemeIndex = 0;
            else curColorschemeIndex = value;
        }
    }

    public static string CurColorschemeName => ColorschemeNames[curColorschemeIndex];
    private static void NextColorschemeIndex() => CurColorschemeIndex += 1;
    private static void PreviousColorschemeIndex() => CurColorschemeIndex -= 1;

    public static void PreviousColorscheme()
    {
        PreviousColorschemeIndex();
        colorPalette.ApplyColorScheme(ColorSchemes[curColorschemeIndex]);
    }
    public static void NextColorscheme()
    {
        NextColorschemeIndex();
        colorPalette.ApplyColorScheme(ColorSchemes[curColorschemeIndex]);
    }
    public static bool ApplyColorscheme(ColorScheme cc) => colorPalette.ApplyColorScheme(cc);
}