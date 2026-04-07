using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Color;

/// <summary>
/// Represents an RGBA color with 8-bit components for red, green, blue, and alpha channels.
/// </summary>
/// <remarks>
/// This immutable struct provides color manipulation, conversion between different color formats,
/// and various utility methods for color transformations.
/// It implements IEquatable for efficient comparison operations and integrates with both System.Drawing.Color and Raylib_cs.Color.
/// </remarks>
public readonly struct ColorRgba : IEquatable<ColorRgba>
{
    #region Members
    /// <summary>
    /// The red component of the color (0-255).
    /// </summary>
    public readonly byte R;
    
    /// <summary>
    /// The green component of the color (0-255).
    /// </summary>
    public readonly byte G;
    
    /// <summary>
    /// The blue component of the color (0-255).
    /// </summary>
    public readonly byte B;
    
    /// <summary>
    /// The alpha (transparency) component of the color (0-255).
    /// </summary>
    public readonly byte A;
    #endregion
    
    #region Predefined Colors
    
    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #00FFFFFF.
    /// </summary>
    public static ColorRgba Transparent => new(System.Drawing.Color.Transparent);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF0F8FF.
    /// </summary>
    public static ColorRgba AliceBlue => new(System.Drawing.Color.AliceBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFAEBD7.
    /// </summary>
    public static ColorRgba AntiqueWhite => new(System.Drawing.Color.AntiqueWhite);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF00FFFF.
    /// </summary>
    public static ColorRgba Aqua => new(System.Drawing.Color.Aqua);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF7FFFD4.
    /// </summary>
    public static ColorRgba Aquamarine => new(System.Drawing.Color.Aquamarine);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF0FFFF.
    /// </summary>
    public static ColorRgba Azure => new(System.Drawing.Color.Azure);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF5F5DC.
    /// </summary>
    public static ColorRgba Beige => new(System.Drawing.Color.Beige);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFE4C4.
    /// </summary>
    public static ColorRgba Bisque => new(System.Drawing.Color.Bisque);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF000000.
    /// </summary>
    public static ColorRgba Black => new(System.Drawing.Color.Black);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFEBCD.
    /// </summary>
    public static ColorRgba BlanchedAlmond => new(System.Drawing.Color.BlanchedAlmond);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF0000FF.
    /// </summary>
    public static ColorRgba Blue => new(System.Drawing.Color.Blue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF8A2BE2.
    /// </summary>
    public static ColorRgba BlueViolet => new(System.Drawing.Color.BlueViolet);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFA52A2A.
    /// </summary>
    public static ColorRgba Brown => new(System.Drawing.Color.Brown);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFDEB887.
    /// </summary>
    public static ColorRgba BurlyWood => new(System.Drawing.Color.BurlyWood);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF5F9EA0.
    /// </summary>
    public static ColorRgba CadetBlue => new(System.Drawing.Color.CadetBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF7FFF00.
    /// </summary>
    public static ColorRgba Chartreuse => new(System.Drawing.Color.Chartreuse);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFD2691E.
    /// </summary>
    public static ColorRgba Chocolate => new(System.Drawing.Color.Chocolate);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFF7F50.
    /// </summary>
    public static ColorRgba Coral => new(System.Drawing.Color.Coral);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF6495ED.
    /// </summary>
    public static ColorRgba CornflowerBlue => new(System.Drawing.Color.CornflowerBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFF8DC.
    /// </summary>
    public static ColorRgba Cornsilk => new(System.Drawing.Color.Cornsilk);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFDC143C.
    /// </summary>
    public static ColorRgba Crimson => new(System.Drawing.Color.Crimson);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF00FFFF.
    /// </summary>
    public static ColorRgba Cyan => new(System.Drawing.Color.Cyan);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF00008B.
    /// </summary>
    public static ColorRgba DarkBlue => new(System.Drawing.Color.DarkBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF008B8B.
    /// </summary>
    public static ColorRgba DarkCyan => new(System.Drawing.Color.DarkCyan);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFB8860B.
    /// </summary>
    public static ColorRgba DarkGoldenrod => new(System.Drawing.Color.DarkGoldenrod);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFA9A9A9.
    /// </summary>
    public static ColorRgba DarkGray => new(System.Drawing.Color.DarkGray);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF006400.
    /// </summary>
    public static ColorRgba DarkGreen => new(System.Drawing.Color.DarkGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFBDB76B.
    /// </summary>
    public static ColorRgba DarkKhaki => new(System.Drawing.Color.DarkKhaki);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF8B008B.
    /// </summary>
    public static ColorRgba DarkMagenta => new(System.Drawing.Color.DarkMagenta);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF556B2F.
    /// </summary>
    public static ColorRgba DarkOliveGreen => new(System.Drawing.Color.DarkOliveGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFF8C00.
    /// </summary>
    public static ColorRgba DarkOrange => new(System.Drawing.Color.DarkOrange);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF9932CC.
    /// </summary>
    public static ColorRgba DarkOrchid => new(System.Drawing.Color.DarkOrchid);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF8B0000.
    /// </summary>
    public static ColorRgba DarkRed => new(System.Drawing.Color.DarkRed);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFE9967A.
    /// </summary>
    public static ColorRgba DarkSalmon => new(System.Drawing.Color.DarkSalmon);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF8FBC8F.
    /// </summary>
    public static ColorRgba DarkSeaGreen => new(System.Drawing.Color.DarkSeaGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF483D8B.
    /// </summary>
    public static ColorRgba DarkSlateBlue => new(System.Drawing.Color.DarkSlateBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF2F4F4F.
    /// </summary>
    public static ColorRgba DarkSlateGray => new(System.Drawing.Color.DarkSlateGray);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF00CED1.
    /// </summary>
    public static ColorRgba DarkTurquoise => new(System.Drawing.Color.DarkTurquoise);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF9400D3.
    /// </summary>
    public static ColorRgba DarkViolet => new(System.Drawing.Color.DarkViolet);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFF1493.
    /// </summary>
    public static ColorRgba DeepPink => new(System.Drawing.Color.DeepPink);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF00BFFF.
    /// </summary>
    public static ColorRgba DeepSkyBlue => new(System.Drawing.Color.DeepSkyBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF696969.
    /// </summary>
    public static ColorRgba DimGray => new(System.Drawing.Color.DimGray);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF1E90FF.
    /// </summary>
    public static ColorRgba DodgerBlue => new(System.Drawing.Color.DodgerBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFB22222.
    /// </summary>
    public static ColorRgba Firebrick => new(System.Drawing.Color.Firebrick);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFFAF0.
    /// </summary>
    public static ColorRgba FloralWhite => new(System.Drawing.Color.FloralWhite);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF228B22.
    /// </summary>
    public static ColorRgba ForestGreen => new(System.Drawing.Color.ForestGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFF00FF.
    /// </summary>
    public static ColorRgba Fuchsia => new(System.Drawing.Color.Fuchsia);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFDCDCDC.
    /// </summary>
    public static ColorRgba Gainsboro => new(System.Drawing.Color.Gainsboro);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF8F8FF.
    /// </summary>
    public static ColorRgba GhostWhite => new(System.Drawing.Color.GhostWhite);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFD700.
    /// </summary>
    public static ColorRgba Gold => new(System.Drawing.Color.Gold);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFDAA520.
    /// </summary>
    public static ColorRgba Goldenrod => new(System.Drawing.Color.Goldenrod);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF808080.
    /// </summary>
    public static ColorRgba Gray => new(System.Drawing.Color.Gray);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF008000.
    /// </summary>
    public static ColorRgba Green => new(System.Drawing.Color.Green);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFADFF2F.
    /// </summary>
    public static ColorRgba GreenYellow => new(System.Drawing.Color.GreenYellow);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF0FFF0.
    /// </summary>
    public static ColorRgba Honeydew => new(System.Drawing.Color.Honeydew);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFF69B4.
    /// </summary>
    public static ColorRgba HotPink => new(System.Drawing.Color.HotPink);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFCD5C5C.
    /// </summary>
    public static ColorRgba IndianRed => new(System.Drawing.Color.IndianRed);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF4B0082.
    /// </summary>
    public static ColorRgba Indigo => new(System.Drawing.Color.Indigo);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFFFF0.
    /// </summary>
    public static ColorRgba Ivory => new(System.Drawing.Color.Ivory);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF0E68C.
    /// </summary>
    public static ColorRgba Khaki => new(System.Drawing.Color.Khaki);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFE6E6FA.
    /// </summary>
    public static ColorRgba Lavender => new(System.Drawing.Color.Lavender);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFF0F5.
    /// </summary>
    public static ColorRgba LavenderBlush => new(System.Drawing.Color.LavenderBlush);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF7CFC00.
    /// </summary>
    public static ColorRgba LawnGreen => new(System.Drawing.Color.LawnGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFFACD.
    /// </summary>
    public static ColorRgba LemonChiffon => new(System.Drawing.Color.LemonChiffon);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFADD8E6.
    /// </summary>
    public static ColorRgba LightBlue => new(System.Drawing.Color.LightBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF08080.
    /// </summary>
    public static ColorRgba LightCoral => new(System.Drawing.Color.LightCoral);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFE0FFFF.
    /// </summary>
    public static ColorRgba LightCyan => new(System.Drawing.Color.LightCyan);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFAFAD2.
    /// </summary>
    public static ColorRgba LightGoldenrodYellow => new(System.Drawing.Color.LightGoldenrodYellow);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFD3D3D3.
    /// </summary>
    public static ColorRgba LightGray => new(System.Drawing.Color.LightGray);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF90EE90.
    /// </summary>
    public static ColorRgba LightGreen => new(System.Drawing.Color.LightGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFB6C1.
    /// </summary>
    public static ColorRgba LightPink => new(System.Drawing.Color.LightPink);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFA07A.
    /// </summary>
    public static ColorRgba LightSalmon => new(System.Drawing.Color.LightSalmon);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF20B2AA.
    /// </summary>
    public static ColorRgba LightSeaGreen => new(System.Drawing.Color.LightSeaGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF87CEFA.
    /// </summary>
    public static ColorRgba LightSkyBlue => new(System.Drawing.Color.LightSkyBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF778899.
    /// </summary>
    public static ColorRgba LightSlateGray => new(System.Drawing.Color.LightSlateGray);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFB0C4DE.
    /// </summary>
    public static ColorRgba LightSteelBlue => new(System.Drawing.Color.LightSteelBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFFFE0.
    /// </summary>
    public static ColorRgba LightYellow => new(System.Drawing.Color.LightYellow);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF00FF00.
    /// </summary>
    public static ColorRgba Lime => new(System.Drawing.Color.Lime);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF32CD32.
    /// </summary>
    public static ColorRgba LimeGreen => new(System.Drawing.Color.LimeGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFAF0E6.
    /// </summary>
    public static ColorRgba Linen => new(System.Drawing.Color.Linen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFF00FF.
    /// </summary>
    public static ColorRgba Magenta => new(System.Drawing.Color.Magenta);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF800000.
    /// </summary>
    public static ColorRgba Maroon => new(System.Drawing.Color.Maroon);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF66CDAA.
    /// </summary>
    public static ColorRgba MediumAquamarine => new(System.Drawing.Color.MediumAquamarine);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF0000CD.
    /// </summary>
    public static ColorRgba MediumBlue => new(System.Drawing.Color.MediumBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFBA55D3.
    /// </summary>
    public static ColorRgba MediumOrchid => new(System.Drawing.Color.MediumOrchid);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF9370DB.
    /// </summary>
    public static ColorRgba MediumPurple => new(System.Drawing.Color.MediumPurple);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF3CB371.
    /// </summary>
    public static ColorRgba MediumSeaGreen => new(System.Drawing.Color.MediumSeaGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF7B68EE.
    /// </summary>
    public static ColorRgba MediumSlateBlue => new(System.Drawing.Color.MediumSlateBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF00FA9A.
    /// </summary>
    public static ColorRgba MediumSpringGreen => new(System.Drawing.Color.MediumSpringGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF48D1CC.
    /// </summary>
    public static ColorRgba MediumTurquoise => new(System.Drawing.Color.MediumTurquoise);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFC71585.
    /// </summary>
    public static ColorRgba MediumVioletRed => new(System.Drawing.Color.MediumVioletRed);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF191970.
    /// </summary>
    public static ColorRgba MidnightBlue => new(System.Drawing.Color.MidnightBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF5FFFA.
    /// </summary>
    public static ColorRgba MintCream => new(System.Drawing.Color.MintCream);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFE4E1.
    /// </summary>
    public static ColorRgba MistyRose => new(System.Drawing.Color.MistyRose);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFE4B5.
    /// </summary>
    public static ColorRgba Moccasin => new(System.Drawing.Color.Moccasin);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFDEAD.
    /// </summary>
    public static ColorRgba NavajoWhite => new(System.Drawing.Color.NavajoWhite);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF000080.
    /// </summary>
    public static ColorRgba Navy => new(System.Drawing.Color.Navy);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFDF5E6.
    /// </summary>
    public static ColorRgba OldLace => new(System.Drawing.Color.OldLace);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF808000.
    /// </summary>
    public static ColorRgba Olive => new(System.Drawing.Color.Olive);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF6B8E23.
    /// </summary>
    public static ColorRgba OliveDrab => new(System.Drawing.Color.OliveDrab);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFA500.
    /// </summary>
    public static ColorRgba Orange => new(System.Drawing.Color.Orange);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFF4500.
    /// </summary>
    public static ColorRgba OrangeRed => new(System.Drawing.Color.OrangeRed);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFDA70D6.
    /// </summary>
    public static ColorRgba Orchid => new(System.Drawing.Color.Orchid);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFEEE8AA.
    /// </summary>
    public static ColorRgba PaleGoldenrod => new(System.Drawing.Color.PaleGoldenrod);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF98FB98.
    /// </summary>
    public static ColorRgba PaleGreen => new(System.Drawing.Color.PaleGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFAFEEEE.
    /// </summary>
    public static ColorRgba PaleTurquoise => new(System.Drawing.Color.PaleTurquoise);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFDB7093.
    /// </summary>
    public static ColorRgba PaleVioletRed => new(System.Drawing.Color.PaleVioletRed);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFEFD5.
    /// </summary>
    public static ColorRgba PapayaWhip => new(System.Drawing.Color.PapayaWhip);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFDAB9.
    /// </summary>
    public static ColorRgba PeachPuff => new(System.Drawing.Color.PeachPuff);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFCD853F.
    /// </summary>
    public static ColorRgba Peru => new(System.Drawing.Color.Peru);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFC0CB.
    /// </summary>
    public static ColorRgba Pink => new(System.Drawing.Color.Pink);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFDDA0DD.
    /// </summary>
    public static ColorRgba Plum => new(System.Drawing.Color.Plum);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFB0E0E6.
    /// </summary>
    public static ColorRgba PowderBlue => new(System.Drawing.Color.PowderBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF800080.
    /// </summary>
    public static ColorRgba Purple => new(System.Drawing.Color.Purple);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFF0000.
    /// </summary>
    public static ColorRgba Red => new(System.Drawing.Color.Red);
    
    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFBC8F8F.
    /// </summary>
    public static ColorRgba RosyBrown => new(System.Drawing.Color.RosyBrown);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF4169E1.
    /// </summary>
    public static ColorRgba RoyalBlue => new(System.Drawing.Color.RoyalBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF8B4513.
    /// </summary>
    public static ColorRgba SaddleBrown => new(System.Drawing.Color.SaddleBrown);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFA8072.
    /// </summary>
    public static ColorRgba Salmon => new(System.Drawing.Color.Salmon);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF4A460.
    /// </summary>
    public static ColorRgba SandyBrown => new(System.Drawing.Color.SandyBrown);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF2E8B57.
    /// </summary>
    public static ColorRgba SeaGreen => new(System.Drawing.Color.SeaGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFF5EE.
    /// </summary>
    public static ColorRgba SeaShell => new(System.Drawing.Color.SeaShell);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFA0522D.
    /// </summary>
    public static ColorRgba Sienna => new(System.Drawing.Color.Sienna);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFC0C0C0.
    /// </summary>
    public static ColorRgba Silver => new(System.Drawing.Color.Silver);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF87CEEB.
    /// </summary>
    public static ColorRgba SkyBlue => new(System.Drawing.Color.SkyBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF6A5ACD.
    /// </summary>
    public static ColorRgba SlateBlue => new(System.Drawing.Color.SlateBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF708090.
    /// </summary>
    public static ColorRgba SlateGray => new(System.Drawing.Color.SlateGray);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFFAFA.
    /// </summary>
    public static ColorRgba Snow => new(System.Drawing.Color.Snow);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF00FF7F.
    /// </summary>
    public static ColorRgba SpringGreen => new(System.Drawing.Color.SpringGreen);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF4682B4.
    /// </summary>
    public static ColorRgba SteelBlue => new(System.Drawing.Color.SteelBlue);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFD2B48C.
    /// </summary>
    public static ColorRgba Tan => new(System.Drawing.Color.Tan);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF008080.
    /// </summary>
    public static ColorRgba Teal => new(System.Drawing.Color.Teal);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFD8BFD8.
    /// </summary>
    public static ColorRgba Thistle => new(System.Drawing.Color.Thistle);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFF6347.
    /// </summary>
    public static ColorRgba Tomato => new(System.Drawing.Color.Tomato);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF40E0D0.
    /// </summary>
    public static ColorRgba Turquoise => new(System.Drawing.Color.Turquoise);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFEE82EE.
    /// </summary>
    public static ColorRgba Violet => new(System.Drawing.Color.Violet);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF5DEB3.
    /// </summary>
    public static ColorRgba Wheat => new(System.Drawing.Color.Wheat);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFFFFF.
    /// </summary>
    public static ColorRgba White => new(System.Drawing.Color.White);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFF5F5F5.
    /// </summary>
    public static ColorRgba WhiteSmoke => new(System.Drawing.Color.WhiteSmoke);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FFFFFF00.
    /// </summary>
    public static ColorRgba Yellow => new(System.Drawing.Color.Yellow);

    /// <summary>
    /// Gets a system-defined color that has an ARGB value of #FF9ACD32.
    /// </summary>
    public static ColorRgba YellowGreen => new(System.Drawing.Color.YellowGreen);
    #endregion
    
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct with all components set to 0 (fully transparent black).
    /// </summary>
    public ColorRgba()
    {
        this.R = 0;
        this.G = 0;
        this.B = 0;
        this.A = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct from a Raylib color.
    /// </summary>
    /// <param name="color">The Raylib color to convert.</param>
    public ColorRgba(Raylib_cs.Color color)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = color.A;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct from a Raylib color with a custom alpha value.
    /// </summary>
    /// <param name="color">The Raylib color to convert.</param>
    /// <param name="a">The alpha value to use (0-255).</param>
    public ColorRgba(Raylib_cs.Color color, byte a)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = a;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct from a System.Drawing color.
    /// </summary>
    /// <param name="color">The System.Drawing color to convert.</param>
    public ColorRgba(System.Drawing.Color color)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = color.A;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct from a System.Drawing color with a custom alpha value.
    /// </summary>
    /// <param name="color">The System.Drawing color to convert.</param>
    /// <param name="a">The alpha value to use (0-255).</param>
    public ColorRgba(System.Drawing.Color color, byte a)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
        this.A = a;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct with specified RGBA components.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    /// <param name="a">The alpha component (0-255).</param>
    public ColorRgba(byte r, byte g, byte b, byte a)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct with specified RGB components and full opacity.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    public ColorRgba(byte r, byte g, byte b)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = byte.MaxValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct with specified RGBA components as integers.
    /// Values are automatically clamped to the valid range (0-255).
    /// </summary>
    /// <param name="r">The red component as an integer (will be clamped to 0-255).</param>
    /// <param name="g">The green component as an integer (will be clamped to 0-255).</param>
    /// <param name="b">The blue component as an integer (will be clamped to 0-255).</param>
    /// <param name="a">The alpha component as an integer (will be clamped to 0-255).</param>
    public ColorRgba(int r, int g, int b, int a)
    {
        this.R = (byte)ShapeMath.Clamp(r, 0, 255);
        this.G = (byte)ShapeMath.Clamp(g, 0, 255);
        this.B = (byte)ShapeMath.Clamp(b, 0, 255);
        this.A = (byte)ShapeMath.Clamp(a, 0, 255);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorRgba"/> struct with specified RGB components as integers and full opacity.
    /// Values are automatically clamped to the valid range (0-255).
    /// </summary>
    /// <param name="r">The red component as an integer (will be clamped to 0-255).</param>
    /// <param name="g">The green component as an integer (will be clamped to 0-255).</param>
    /// <param name="b">The blue component as an integer (will be clamped to 0-255).</param>
    public ColorRgba(int r, int g, int b)
    {
        this.R = (byte)ShapeMath.Clamp(r, 0, 255);
        this.G = (byte)ShapeMath.Clamp(g, 0, 255);
        this.B = (byte)ShapeMath.Clamp(b, 0, 255);
        this.A = byte.MaxValue;
    }
        
    #endregion

    #region Information
    /// <summary>
    /// Determines whether the color is completely transparent (alpha is zero).
    /// </summary>
    /// <returns>True if the alpha component is zero; otherwise, false.</returns>
    public bool IsClear() => A == byte.MinValue;

    /// <summary>
    /// Determines whether the color is partially transparent (alpha is between 0 and 255).
    /// </summary>
    /// <returns>True if the alpha component is greater than zero and less than 255; otherwise, false.</returns>
    public bool IsTransparent() => A is > byte.MinValue and < byte.MaxValue;

    /// <summary>
    /// Determines whether the color is completely opaque (alpha is 255).
    /// </summary>
    /// <returns>True if the alpha component is 255; otherwise, false.</returns>
    public bool IsOpaque() => A == byte.MaxValue;

    /// <summary>
    /// Determines whether the color corresponds to a named color in the System.Drawing.Color enumeration.
    /// </summary>
    /// <returns>True if the color matches a named color; otherwise, false.</returns>
    public bool IsNamed() => ToSysColor().IsNamedColor;

    /// <summary>
    /// Converts the color to its corresponding KnownColor enumeration value if it matches a known color.
    /// </summary>
    /// <returns>The KnownColor enumeration value that corresponds to this color.</returns>
    public KnownColor ToKnownColor() => ToSysColor().ToKnownColor();

    /// <summary>
    /// Creates a new ColorRgba from a named color.
    /// </summary>
    /// <param name="name">The name of the color to create.</param>
    /// <returns>A new ColorRgba instance representing the named color.</returns>
    public static ColorRgba FromName(string name) => new(System.Drawing.Color.FromName(name));

    /// <summary>
    /// Calculates the relative luminance of the color as a byte value.
    /// Uses the standard formula: 0.2126*R + 0.7152*G + 0.0722*B
    /// </summary>
    /// <returns>The relative luminance as a byte value (0-255).</returns>
    public byte GetRelativeLuminance() => (byte)(0.2126f * R + 0.7152f * G + 0.0722f * B);

    /// <summary>
    /// Calculates the relative luminance of the color as a normalized float value.
    /// Uses the standard formula: 0.2126*R + 0.7152*G + 0.0722*B with normalized RGB components.
    /// </summary>
    /// <returns>The relative luminance as a float value (0.0-1.0).</returns>
    public float GetRelativeLuminanceF() => 0.2126f * (R / 255f) + 0.7152f * (G / 255f) + 0.0722f * (B / 255f);
    #endregion

    #region Transformation
    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    /// <param name="from">The source color to interpolate from.</param>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="f">The interpolation factor in the range [0.0, 1.0], where 0 returns the source color and 1 returns the target color.</param>
    /// <returns>A new color that is the linear interpolation between the source and target colors.</returns>
    public static ColorRgba Lerp(ColorRgba from, ColorRgba to, float f)
    {
        return new ColorRgba(
            (byte)ShapeMath.LerpInt(from.R, to.R, f),
            (byte)ShapeMath.LerpInt(from.G, to.G, f),
            (byte)ShapeMath.LerpInt(from.B, to.B, f),
            (byte)ShapeMath.LerpInt(from.A, to.A, f));
    }

    /// <summary>
    /// Performs an exponential decay interpolation between two colors.
    /// </summary>
    /// <param name="from">The source color to interpolate from.</param>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="f">The decay factor that controls the rate of interpolation. (0 - 1)</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the exponential decay interpolation between the source and target colors.</returns>
    public static ColorRgba ExpDecayLerp(ColorRgba from, ColorRgba to, float f, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.ExpDecayLerpInt(from.R, to.R, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(from.G, to.G, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(from.B, to.B, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(from.A, to.A, f, dt));
    }

    /// <summary>
    /// Performs a power-based interpolation between two colors. (expensive!)
    /// Framerate independent lerp.
    /// <see cref="ExpDecayLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/> should be used if possible.
    /// </summary>
    /// <param name="from">The source color to interpolate from.</param>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="remainder">The remaining interpolation factor.
    /// How much fraction should remain after 1 second?</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the power-based interpolation between the source and target colors.</returns>
    public static ColorRgba PowLerp(ColorRgba from, ColorRgba to, float remainder, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.PowLerpInt(from.R, to.R, remainder, dt),
            (byte)ShapeMath.PowLerpInt(from.G, to.G, remainder, dt),
            (byte)ShapeMath.PowLerpInt(from.B, to.B, remainder, dt),
            (byte)ShapeMath.PowLerpInt(from.A, to.A, remainder, dt));
    }

    /// <summary>
    /// Performs a complex exponential decay interpolation between two colors.
    /// Framerate independent lerp.
    /// Less expensive alternative to <see cref="PowLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/>.
    /// Base function for <see cref="ExpDecayLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/>.
    /// </summary>
    /// <param name="from">The source color to interpolate from.</param>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="decay">The decay rate that controls the interpolation curve.
    /// Best results between 1-25</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the complex exponential decay interpolation between the source and target colors.</returns>
    public static ColorRgba ExpDecayLerpComplex(ColorRgba from, ColorRgba to, float decay, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.ExpDecayLerpIntComplex(from.R, to.R, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(from.G, to.G, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(from.B, to.B, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(from.A, to.A, decay, dt));
    }

    /// <summary>
    /// Linearly interpolates from this color to another color.
    /// </summary>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="f">The interpolation factor in the range [0.0, 1.0], where 0 returns this color and 1 returns the target color.</param>
    /// <returns>A new color that is the linear interpolation between this color and the target color.</returns>
    public ColorRgba Lerp(ColorRgba to, float f)
    {
        return new ColorRgba(
            (byte)ShapeMath.LerpInt(R, to.R, f),
            (byte)ShapeMath.LerpInt(G, to.G, f),
            (byte)ShapeMath.LerpInt(B, to.B, f),
            (byte)ShapeMath.LerpInt(A, to.A, f));
    }

    /// <summary>
    /// Performs a frame rate independent exponential decay interpolation from this color to another color.
    /// Less expensive alternative to <see cref="PowLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/>
    /// </summary>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="f">The decay factor that controls the rate of interpolation. (0 - 1)</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the exponential decay interpolation between this color and the target color.</returns>
    public ColorRgba ExpDecayLerp(ColorRgba to, float f, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.ExpDecayLerpInt(R, to.R, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(G, to.G, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(B, to.B, f, dt),
            (byte)ShapeMath.ExpDecayLerpInt(A, to.A, f, dt));
    }

    /// <summary>
    /// Performs a frame rate independent power-based interpolation from this color to another color. (expensive!)
    /// <see cref="ExpDecayLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/> should be used if possible.
    /// </summary>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="remainder">The remaining interpolation factor.</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the power-based interpolation between this color and the target color.</returns>
    public ColorRgba PowLerp(ColorRgba to, float remainder, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.PowLerpInt(R, to.R, remainder, dt),
            (byte)ShapeMath.PowLerpInt(G, to.G, remainder, dt),
            (byte)ShapeMath.PowLerpInt(B, to.B, remainder, dt),
            (byte)ShapeMath.PowLerpInt(A, to.A, remainder, dt));
    }

    /// <summary>
    /// Performs a framerate independent exponential decay interpolation from this color to another color.
    /// Less expensive alternative to <see cref="PowLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/>
    /// Base function for <see cref="ExpDecayLerp(ShapeEngine.Color.ColorRgba,ShapeEngine.Color.ColorRgba,float,float)"/>.
    /// </summary>
    /// <param name="to">The target color to interpolate to.</param>
    /// <param name="decay">The decay rate that controls the interpolation curve. Best results between value 1-25.</param>
    /// <param name="dt">The time delta used to scale the interpolation.</param>
    /// <returns>A new color that is the complex exponential decay interpolation between this color and the target color.</returns>
    public ColorRgba ExpDecayLerpComplex(ColorRgba to, float decay, float dt)
    {
        return new ColorRgba(
            (byte)ShapeMath.ExpDecayLerpIntComplex(R, to.R, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(G, to.G, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(B, to.B, decay, dt),
            (byte)ShapeMath.ExpDecayLerpIntComplex(A, to.A, decay, dt));
    }

    /// <summary>
    /// Adds two colors together, clamping the result to valid color component ranges.
    /// </summary>
    /// <param name="left">The first color operand.</param>
    /// <param name="right">The second color operand.</param>
    /// <returns>A new color with each component being the sum of the corresponding components from the operands, clamped to the range [0, 255].</returns>
    public static ColorRgba operator +(ColorRgba left, ColorRgba right)
    {
        return new
        (
            Clamp(left.R + right.R),
            Clamp(left.G + right.G),
            Clamp(left.B + right.B),
            Clamp(left.A + right.A)
        );
    }

    /// <summary>
    /// Subtracts the second color from the first, clamping the result to valid color component ranges.
    /// </summary>
    /// <param name="left">The color to subtract from.</param>
    /// <param name="right">The color to subtract.</param>
    /// <returns>A new color with each component being the difference of the corresponding components from the operands, clamped to the range [0, 255].</returns>
    public static ColorRgba operator -(ColorRgba left, ColorRgba right)
    {
        return new
        (
            Clamp(left.R - right.R),
            Clamp(left.G - right.G),
            Clamp(left.B - right.B),
            Clamp(left.A - right.A)
        );
    }

    /// <summary>
    /// Multiplies two colors together, clamping the result to valid color component ranges.
    /// </summary>
    /// <param name="left">The first color operand.</param>
    /// <param name="right">The second color operand.</param>
    /// <returns>A new color with each component being the product of the corresponding components from the operands, clamped to the range [0, 255].</returns>
    public static ColorRgba operator *(ColorRgba left, ColorRgba right)
    {
        return new
        (
            Clamp(left.R * right.R),
            Clamp(left.G * right.G),
            Clamp(left.B * right.B),
            Clamp(left.A * right.A)
        );
    }

    /// <summary>
    /// Change the brightness of the color.
    /// </summary>
    /// <param name="correctionFactor">Range -1 to 1</param>
    public ColorRgba ChangeBrightness(float correctionFactor) => new(Raylib.ColorBrightness(ToRayColor(), correctionFactor));
    /// <summary>
    /// Change the contrast of the color.
    /// </summary>
    /// <param name="correctionFactor">Range -1 to 1</param>
    public ColorRgba ChangeContrast(float correctionFactor) => new(Raylib.ColorContrast(ToRayColor(), correctionFactor));

    /// <summary>
    /// Creates a new color with the specified alpha value while preserving the RGB components.
    /// </summary>
    /// <param name="a">The new alpha value (0-255).</param>
    /// <returns>A new color with the specified alpha value and the same RGB components as this color.</returns>
    public ColorRgba SetAlpha(byte a) => new(R, G, B, a);

    /// <summary>
    /// Creates a new color with the alpha value adjusted by the specified amount while preserving the RGB components.
    /// </summary>
    /// <param name="amount">The amount to adjust the alpha value by. The result is clamped to the range [0, 255].</param>
    /// <returns>A new color with the adjusted alpha value and the same RGB components as this color.</returns>
    public ColorRgba ChangeAlpha(int amount) => new(R, G, B, Clamp(A + amount));

    /// <summary>
    /// Creates a new color with the specified red value while preserving the other components.
    /// </summary>
    /// <param name="r">The new red value (0-255).</param>
    /// <returns>A new color with the specified red value and the same green, blue, and alpha components as this color.</returns>
    public ColorRgba SetRed(byte r) => new(r, G, B, A);

    /// <summary>
    /// Creates a new color with the red value adjusted by the specified amount while preserving the other components.
    /// </summary>
    /// <param name="amount">The amount to adjust the red value by. The result is clamped to the range [0, 255].</param>
    /// <returns>A new color with the adjusted red value and the same green, blue, and alpha components as this color.</returns>
    public ColorRgba ChangeRed(int amount) => new(Clamp(R + amount), G, B, A);

    /// <summary>
    /// Creates a new color with the specified green value while preserving the other components.
    /// </summary>
    /// <param name="g">The new green value (0-255).</param>
    /// <returns>A new color with the specified green value and the same red, blue, and alpha components as this color.</returns>
    public ColorRgba SetGreen(byte g) => new(R, g, B, A);

    /// <summary>
    /// Creates a new color with the green value adjusted by the specified amount while preserving the other components.
    /// </summary>
    /// <param name="amount">The amount to adjust the green value by. The result is clamped to the range [0, 255].</param>
    /// <returns>A new color with the adjusted green value and the same red, blue, and alpha components as this color.</returns>
    public ColorRgba ChangeGreen(int amount) => new(R, Clamp(G + amount), B, A);

    /// <summary>
    /// Creates a new color with the specified blue value while preserving the other components.
    /// </summary>
    /// <param name="b">The new blue value (0-255).</param>
    /// <returns>A new color with the specified blue value and the same red, green, and alpha components as this color.</returns>
    public ColorRgba SetBlue(byte b) => new(R, G, b, A);

    /// <summary>
    /// Creates a new color with the blue value adjusted by the specified amount while preserving the other components.
    /// </summary>
    /// <param name="amount">The amount to adjust the blue value by. The result is clamped to the range [0, 255].</param>
    /// <returns>A new color with the adjusted blue value and the same red, green, and alpha components as this color.</returns>
    public ColorRgba ChangeBlue(int amount) => new(R, G, Clamp(B + amount), A);
    #endregion

    #region Implicit Conversion

    public static implicit operator System.Drawing.Color(ColorRgba c) 
        => System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);

    public static implicit operator ColorRgba(System.Drawing.Color c) 
        => new ColorRgba(c.R, c.G, c.B, c.A);
    
    public static implicit operator Raylib_cs.Color(ColorRgba c) 
        => new Raylib_cs.Color(c.R, c.G, c.B, c.A);

    public static implicit operator ColorRgba(Raylib_cs.Color c) 
        => new ColorRgba(c.R, c.G, c.B, c.A);
    
    #endregion
    
    #region Conversion
    
    /// <summary>
    /// Converts this ColorRgba to a System.Drawing.Color.
    /// </summary>
    /// <returns>A System.Drawing.Color equivalent to this ColorRgba.</returns>
    public System.Drawing.Color ToSysColor() => System.Drawing.Color.FromArgb(R, G, B, A);

    /// <summary>
    /// Converts this ColorRgba to a Raylib_cs.Color.
    /// </summary>
    /// <returns>A Raylib_cs.Color equivalent to this ColorRgba.</returns>
    public Raylib_cs.Color ToRayColor() => new (R, G, B, A);
    
    /// <summary>
    /// Creates a <see cref="ColorRgba"/> from a <see cref="KnownColor"/>.
    /// </summary>
    /// <param name="c">The known color to convert.</param>
    /// <returns>A new <see cref="ColorRgba"/> instance representing the known color.</returns>
    public static ColorRgba FromKnownColor(KnownColor c) => new(System.Drawing.Color.FromKnownColor(c));
    
    /// <summary>
    /// Creates a <see cref="ColorRgba"/> from a <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="c">The <see cref="System.Drawing.Color"/> to convert.</param>
    /// <returns>A new <see cref="ColorRgba"/> instance representing the system color.</returns>
    public static ColorRgba FromSystemColor(System.Drawing.Color c) => new(c);
    
    /// <summary>
    /// Creates a <see cref="ColorRgba"/> from a <see cref="Raylib_cs.Color"/>.
    /// </summary>
    /// <param name="c">The <see cref="Raylib_cs.Color"/> to convert.</param>
    /// <returns>A new <see cref="ColorRgba"/> instance representing the Raylib color.</returns>
    public static ColorRgba FromRayColor(Raylib_cs.Color c) => new(c);
    
    /// <summary>
    /// Normalizes the color components from the range [0, 255] to [0.0, 1.0].
    /// </summary>
    /// <returns>A tuple containing the normalized RGBA components as float values between 0.0 and 1.0.</returns>
    public (float r, float g, float b, float a) Normalize()
    {
        return 
        (
            R / 255f,
            G / 255f,
            B / 255f,
            A / 255f
        );
    }

    /// <summary>
    /// Creates a ColorRgba from normalized RGBA components.
    /// </summary>
    /// <param name="r">The normalized red component (0.0 to 1.0).</param>
    /// <param name="g">The normalized green component (0.0 to 1.0).</param>
    /// <param name="b">The normalized blue component (0.0 to 1.0).</param>
    /// <param name="a">The normalized alpha component (0.0 to 1.0).</param>
    /// <returns>A new ColorRgba with the specified normalized components converted to the range [0, 255].</returns>
    public static ColorRgba FromNormalize(float r, float g, float b, float a) => new((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)(a * 255f));

    /// <summary>
    /// Creates a ColorRgba from a tuple of normalized RGBA components.
    /// </summary>
    /// <param name="normalizedColor">A tuple containing the normalized RGBA components as float values between 0.0 and 1.0.</param>
    /// <returns>A new ColorRgba with the specified normalized components converted to the range [0, 255].</returns>
    public static ColorRgba FromNormalize((float r, float g, float b, float a) normalizedColor) => FromNormalize(normalizedColor.r, normalizedColor.g, normalizedColor.b, normalizedColor.a);

    /// <summary>
    /// Converts this RGB color to HSL (Hue, Saturation, Lightness) color space.
    /// </summary>
    /// <returns>A ColorHsl representing this color in the HSL color space.</returns>
    public ColorHsl ToHSL()
    {
        float r = R/255.0f;
        float g = G/255.0f;
        float b = B/255.0f;
        float v;
        float m;
        float vm;
        float r2, g2, b2;
     
        float h = 0; // default to black
        float s = 0;
        v = Math.Max(r,g);
        v = Math.Max(v,b);
        m = Math.Min(r,g);
        m = Math.Min(m,b);
        float l = (m + v) / 2.0f;
        if (l <= 0.0)
        {
            return new ColorHsl(h,s,l);
        }
        vm = v - m;
        s = vm;
        if (s > 0.0)
        {
            s /= (l <= 0.5f) ? (v + m ) : (2.0f - v - m) ;
        }
        else
        {
            return new ColorHsl(h,s,l);
        }
        r2 = (v - r) / vm;
        g2 = (v - g) / vm;
        b2 = (v - b) / vm;
        if (Math.Abs(r - v) < 0.0001f)
        {
            h = (Math.Abs(g - m) < 0.0001f ? 5.0f + b2 : 1.0f - g2);
        }
        else if (Math.Abs(g - v) < 0.0001f)
        {
            h = (Math.Abs(b - m) < 0.0001f ? 1.0f + r2 : 3.0f - b2);
        }
        else
        {
            h = (Math.Abs(r - m) < 0.0001f ? 3.0f + g2 : 5.0f - r2);
        }
        h /= 6.0f;
        
        return new ColorHsl(h,s,l);
    }

    /// <summary>
    /// Converts this color to a hexadecimal integer representation.
    /// </summary>
    /// <returns>An integer representing the color in hexadecimal format.</returns>
    public int ToHex() => Raylib.ColorToInt(ToRayColor());

    /// <summary>
    /// Creates a ColorRgba from a hexadecimal integer representation with full opacity.
    /// </summary>
    /// <param name="colorValue">The hexadecimal integer representation of the color.</param>
    /// <returns>A new ColorRgba with the RGB components from the hexadecimal value and full opacity.</returns>
    public static ColorRgba FromHex(int colorValue) => FromHex(colorValue, byte.MaxValue);

    /// <summary>
    /// Creates a ColorRgba from a hexadecimal integer representation with the specified alpha value.
    /// </summary>
    /// <param name="colorValue">The hexadecimal integer representation of the color.</param>
    /// <param name="a">The alpha component value (0-255).</param>
    /// <returns>A new ColorRgba with the RGB components from the hexadecimal value and the specified alpha.</returns>
    public static ColorRgba FromHex(int colorValue, byte a)
    {
        byte[] rgb = BitConverter.GetBytes(colorValue);
        if (!BitConverter.IsLittleEndian) Array.Reverse(rgb);
        return new(rgb[2], rgb[1], rgb[0], a);
    }

    /// <summary>
    /// Creates a ColorRgba from a hexadecimal string representation with full opacity.
    /// </summary>
    /// <param name="hexColor">The hexadecimal string representation of the color (e.g., "FF0000" or "#FF0000" for red).</param>
    /// <returns>A new ColorRgba with the RGB components from the hexadecimal string and full opacity.</returns>
    public static ColorRgba FromHex(string hexColor) => FromHex(hexColor, byte.MaxValue);

    /// <summary>
    /// Creates a ColorRgba from a hexadecimal string representation with the specified alpha value.
    /// </summary>
    /// <param name="hexColor">The hexadecimal string representation of the color (e.g., "FF0000" or "#FF0000" for red).</param>
    /// <param name="a">The alpha component value (0-255).</param>
    /// <returns>A new ColorRgba with the RGB components from the hexadecimal string and the specified alpha.</returns>
    public static ColorRgba FromHex(string hexColor, byte a)
    {
        //Remove # if present
        if (hexColor.IndexOf('#') != -1)
            hexColor = hexColor.Replace("#", "");

        int red = 0;
        int green = 0;
        int blue = 0;

        if (hexColor.Length == 6)
        {
            //#RRGGBB
            red = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            green = int.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            blue = int.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
        }
        else if (hexColor.Length == 3)
        {
            //#RGB
            red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
            green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
            blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
        }

        return new((byte)red, (byte)green, (byte)blue, a);
    }

    /// <summary>
    /// Creates an array of ColorRgba objects from an array of hexadecimal integer values.
    /// </summary>
    /// <param name="colors">An array of hexadecimal integer values representing colors.</param>
    /// <returns>An array of ColorRgba objects corresponding to the provided hexadecimal values.</returns>
    public static ColorRgba[] ParseColors(params int[] colors)
    {
        ColorRgba[] palette = new ColorRgba[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            palette[i] = ColorRgba.FromHex(colors[i]);
        }
        return palette;
    }

    /// <summary>
    /// Creates an array of ColorRgba objects from an array of hexadecimal string representations.
    /// </summary>
    /// <param name="hexColors">An array of hexadecimal string representations of colors.</param>
    /// <returns>An array of ColorRgba objects corresponding to the provided hexadecimal strings.</returns>
    public static ColorRgba[] ParseColors(params string[] hexColors)
    {
        ColorRgba[] palette = new ColorRgba[hexColors.Length];
        for (int i = 0; i < hexColors.Length; i++)
        {
            palette[i] = ColorRgba.FromHex(hexColors[i]);
        }
        return palette;
    }
    #endregion
    
    #region Equatable & ToString
    /// <summary>
    /// Returns a string representation of this color.
    /// </summary>
    /// <returns>A string representation of the color in the format provided by System.Drawing.Color.</returns>
    public override string ToString() => ToSysColor().ToString();

    /// <summary>
    /// Determines whether two ColorRgba instances are equal by comparing their component values.
    /// </summary>
    /// <param name="left">The first ColorRgba to compare.</param>
    /// <param name="right">The second ColorRgba to compare.</param>
    /// <returns>True if all RGBA components of both colors are equal; otherwise, false.</returns>
    public static bool operator ==(ColorRgba left, ColorRgba right) =>
        left.A == right.A && left.R == right.R && left.G == right.G && left.B == right.B;

    /// <summary>
    /// Determines whether two ColorRgba instances are not equal.
    /// </summary>
    /// <param name="left">The first ColorRgba to compare.</param>
    /// <param name="right">The second ColorRgba to compare.</param>
    /// <returns>True if any RGBA component differs between the colors; otherwise, false.</returns>
    public static bool operator !=(ColorRgba left, ColorRgba right) => !(left == right);

    /// <summary>
    /// Determines whether the specified object is equal to the current ColorRgba.
    /// </summary>
    /// <param name="obj">The object to compare with the current ColorRgba.</param>
    /// <returns>True if the specified object is a ColorRgba and has the same component values as this ColorRgba; otherwise, false.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ColorRgba other && this.Equals(other);

    /// <summary>
    /// Determines whether the specified ColorRgba is equal to the current ColorRgba.
    /// </summary>
    /// <param name="other">The ColorRgba to compare with the current ColorRgba.</param>
    /// <returns>True if the specified ColorRgba has the same component values as this ColorRgba; otherwise, false.</returns>
    public bool Equals(ColorRgba other) => this == other;

    /// <summary>
    /// Creates a stable 64-bit hash key for this color.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize channel values before hashing.</param>
    /// <returns>A 64-bit hash key suitable for cache keys and change detection.</returns>
    public ulong GetHashKey(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces)
    {
        if (decimalPlaces < 0) decimalPlaces = DecimalPrecision.DefaultDecimalPlaces;

        Fnv1aHashQuantizer hashQuantizer = new(decimalPlaces);
        return hashQuantizer.GetHash(R, G, B, A);
    }

    /// <summary>
    /// Creates a fixed-width hexadecimal string representation of this color hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize channel values before hashing.</param>
    /// <returns>A 16-character uppercase hexadecimal hash key string.</returns>
    public string GetHashKeyHex(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces) => GetHashKey(decimalPlaces).ToString("X16");

    /// <summary>
    /// Creates a string representation of this color hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize channel values before hashing.</param>
    /// <returns>A stable hexadecimal hash key string.</returns>
    public string GetHashKeyString(int decimalPlaces = DecimalPrecision.DefaultDecimalPlaces) => GetHashKeyHex(decimalPlaces);

    /// <summary>
    /// Returns a hash code for this ColorRgba.
    /// </summary>
    /// <returns>A hash code for the current ColorRgba, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        ulong hashKey = GetHashKey();
        return unchecked((int)(hashKey ^ (hashKey >> 32)));
    }
    #endregion
    
    #region Clamp
    /// <summary>
    /// Clamps an integer value to the valid color component range of 0-255.
    /// </summary>
    /// <param name="value">The integer value to clamp.</param>
    /// <returns>A byte value clamped to the range [0, 255].</returns>
    public static byte Clamp(byte value) => ShapeMath.Clamp(value, byte.MinValue, byte.MaxValue);
    
    /// <summary>
    /// Clamps an integer value to the valid color component range of 0-255 and returns it as a byte.
    /// </summary>
    /// <param name="value">The integer value to clamp.</param>
    /// <returns>A byte value clamped to the range [0, 255].</returns>
    public static byte Clamp(int value) => (byte)ShapeMath.Clamp(value, byte.MinValue, byte.MaxValue);

    /// <summary>
    /// Clamps a byte value to the specified minimum and maximum range.
    /// </summary>
    /// <param name="value">The byte value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The clamped byte value within the specified range.</returns>
    public static byte Clamp(byte value, byte min, byte max) => ShapeMath.Clamp(value, min, max);
    
    /// <summary>
    /// Clamps each RGBA component of this color to the valid range [0, 255].
    /// </summary>
    /// <returns>A new <see cref="ColorRgba"/> with all components clamped to [0, 255].</returns>
    public ColorRgba Clamp()
    {
        return new
        (
            Clamp(R),
            Clamp(G),
            Clamp(B),
            Clamp(A)
        );
    }
    /// <summary>
    /// Clamps each RGBA component of this color to the specified minimum and maximum values.
    /// </summary>
    /// <param name="min">The minimum allowed value for each component.</param>
    /// <param name="max">The maximum allowed value for each component.</param>
    /// <returns>A new <see cref="ColorRgba"/> with all components clamped to the specified range.</returns>
    public ColorRgba Clamp(byte min, byte max)
    {
        return new
        (
            Clamp(R, min, max),
            Clamp(G, min, max),
            Clamp(B, min, max),
            Clamp(A, min, max)
        );
    }
    /// <summary>
    /// Clamps each RGBA component of this color to the range [0, maxX] for each respective component.
    /// </summary>
    /// <param name="maxR">The maximum allowed value for the red component.</param>
    /// <param name="maxG">The maximum allowed value for the green component.</param>
    /// <param name="maxB">The maximum allowed value for the blue component.</param>
    /// <param name="maxA">The maximum allowed value for the alpha component.</param>
    /// <returns>A new <see cref="ColorRgba"/> with each component clamped to its specified maximum value.</returns>
    public ColorRgba Clamp(byte maxR, byte maxG, byte maxB, byte maxA)
    {
        return new
        (
            Clamp(R, byte.MinValue, maxR),
            Clamp(G, byte.MinValue, maxG),
            Clamp(B, byte.MinValue, maxB),
            Clamp(A, byte.MinValue, maxA)
        );
    }
    /// <summary>
    /// Clamps each RGBA component of this color to the specified minimum and maximum values for each channel.
    /// </summary>
    /// <param name="minR">The minimum allowed value for the red component.</param>
    /// <param name="maxR">The maximum allowed value for the red component.</param>
    /// <param name="minG">The minimum allowed value for the green component.</param>
    /// <param name="maxG">The maximum allowed value for the green component.</param>
    /// <param name="minB">The minimum allowed value for the blue component.</param>
    /// <param name="maxB">The maximum allowed value for the blue component.</param>
    /// <param name="minA">The minimum allowed value for the alpha component.</param>
    /// <param name="maxA">The maximum allowed value for the alpha component.</param>
    /// <returns>A new <see cref="ColorRgba"/> with each component clamped to its specified range.</returns>
    public ColorRgba Clamp(byte minR, byte maxR, byte minG, byte maxG, byte minB, byte maxB, byte minA, byte maxA)
    {
        return new
        (
            Clamp(R, minR, maxR),
            Clamp(G, minG, maxG),
            Clamp(B, minB, maxB),
            Clamp(A, minA, maxA)
        );
    }
    
    #endregion
}