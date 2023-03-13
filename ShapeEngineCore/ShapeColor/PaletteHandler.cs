using Raylib_CsLo;

namespace ShapeColor
{
    public class PaletteHandler
    {
        private Dictionary<string, ColorPalette> palettes = new();
        private int curPaletteIndex = 0;
        private List<string> paletteNames = new() { };
        private ColorPalette curPalette = new("empty");


        public PaletteHandler()
        {
            //curPalette = new ColorPalette(
            //    "default",
            //
            //    ("black", BLACK), 
            //    ("white", WHITE), 
            //    ("gray", GRAY),
            //    ("green", GREEN), 
            //    ("blue", BLUE), 
            //    ("red", RED),
            //    ("orange", ORANGE), 
            //    ("purple", PURPLE), 
            //    ("pink", PINK));
        }
        //public PaletteHandler(string paletteName, params (string name, Raylib_CsLo.Color color)[] entries)
        //{
        //    curPalette = new(paletteName, entries);
        //    palettes.Add(paletteName, curPalette);
        //    paletteNames.Add(paletteName);
        //    curPaletteIndex= 0;
        //}

        public List<string> GetAllPaletteNames() { return paletteNames; }
        public int CurPaletteIndex { get { return curPaletteIndex; } }
        public string CurPaletteName { get { return curPalette.Name; } }
        public ColorPalette CurPalette { get { return curPalette; } }
        public Color C(int id) { return curPalette.Get(id); }

        public void AddPalette(string paletteName, string[] hexColors, int[] ids)
        {
            ColorPalette cp = new(paletteName, hexColors, ids);
            if (palettes.ContainsKey(paletteName)) palettes[paletteName] = cp;
            else
            {
                palettes.Add(paletteName, cp);
                paletteNames.Add(paletteName);
            }
        }
        public void AddPalette(string paletteName, Color[] colors, int[] ids)
        {
            ColorPalette cp = new(paletteName, colors, ids);
            if (palettes.ContainsKey(paletteName)) palettes[paletteName] = cp;
            else
            {
                palettes.Add(paletteName, cp);
                paletteNames.Add(paletteName);
            }
        }
        public void AddPalette(string paletteName, params (int id, Color color)[] entries)
        {
            ColorPalette cp = new(paletteName, entries);
            if (palettes.ContainsKey(paletteName)) palettes[paletteName] = cp;
            else
            {
                palettes.Add(paletteName, cp);
                paletteNames.Add(paletteName);
            }
        }
        public void AddPalette(string paletteName, Dictionary<int, Color> palette)
        {
            ColorPalette cp = new(paletteName, palette);
            if (palettes.ContainsKey(paletteName)) palettes[paletteName] = cp;
            else
            {
                palettes.Add(paletteName, cp);
                paletteNames.Add(paletteName);
            }
        }
        public void AddPalette(ColorPalette palette)
        {
            string paletteName = palette.Name;
            if (palettes.ContainsKey(paletteName)) palettes[paletteName] = palette;
            else
            {
                palettes.Add(paletteName, palette);
                paletteNames.Add(paletteName);
            }
        }
        //public void AddPalette(string paletteName, Image source, params string[] colorNames)
        //{
        //    AddPalette(GeneratePaletteFromImage(paletteName, source, colorNames));
        //}
        
        
        public void RemovePalette(string paletteName)
        {
            //if (paletteName == "default") return;
            if (!palettes.ContainsKey(paletteName)) return;

            if (curPalette.Name == paletteName)
            {
                Next();
            }
            palettes.Remove(paletteName);
            paletteNames.Remove(paletteName);
        }
        public void ChangePalette(string newPalette)
        {
            //if (CurName == newPalette) return;

            if (palettes.ContainsKey(newPalette))
            {
                curPalette = palettes[newPalette];
                //curPaletteName = newPalette;
                curPaletteIndex = paletteNames.IndexOf(newPalette);
            }
        }
        public void ChangePalette(int index)
        {
            //if (index == curPaletteIndex) return;
            if (index < 0) { index = paletteNames.Count - 1; }
            else if (index >= paletteNames.Count) { index = 0; }

            curPaletteIndex = index;
            //curPaletteName = paletteNames[index];
            curPalette = palettes[paletteNames[index]];

        }
        public void Next()
        {
            ChangePalette(curPaletteIndex + 1);
        }
        public void Previous()
        {
            ChangePalette(curPaletteIndex - 1);
        }
        public void Close()
        {
            paletteNames.Clear();
            paletteNames.Clear();
        }


        ///// <summary>
        ///// Get all the colors from the source image.
        ///// </summary>
        ///// <param name="source"> Image that contains the colors. Each color should only be 1 pixel wide.</param>
        ///// <param name="colorCount">How many colors there are in the image. </param>
        ///// <returns></returns>
        //public static Raylib_CsLo.Color[] GetColorsFromImage(Image source, int colorCount)
        //{
        //    unsafe
        //    {
        //        var cptr = Raylib.LoadImageColors(source);
        //        Raylib_CsLo.Color[] colors = new Raylib_CsLo.Color[colorCount];
        //        for (int i = 0; i < colorCount; i++)
        //        {
        //            colors[i] = *(cptr + i);
        //        }
        //        Raylib.UnloadImageColors(cptr);
        //        return colors;
        //    }
        //}
        ///// <summary>
        ///// Get all the colors from the source image.
        ///// </summary>
        ///// <param name="source"> Image that contains the colors. Each color should only be 1 pixel wide.</param>
        ///// <param name="colorCount">How many colors there are in the image. </param>
        ///// <returns></returns>
        //public static Dictionary<string, Raylib_CsLo.Color> GeneratePaletteFromImage(Image source, params string[] colorNames)
        //{
        //    unsafe
        //    {
        //        int size = colorNames.Length;
        //        int* colorCount = null;
        //        var cptr = Raylib.LoadImageColors(source);
        //        Dictionary<string, Raylib_CsLo.Color> colors = new();
        //        for (int i = 0; i < size; i++)
        //        {
        //            Raylib_CsLo.Color color = *(cptr + i);
        //            colors.Add(colorNames[i], color);
        //        }
        //        Raylib.UnloadImageColors(cptr);
        //        return colors;
        //    }
        //}
        ///// <summary>
        ///// Get all the colors from the source image.
        ///// </summary>
        ///// <param name="source"> Image that contains the colors. Each color should only be 1 pixel wide.</param>
        ///// <param name="colorCount">How many colors there are in the image. </param>
        ///// <returns></returns>
        //public static ColorPalette GeneratePaletteFromImage(string paletteName, Image source, params string[] colorNames)
        //{
        //    return new(paletteName, GeneratePaletteFromImage(source, colorNames));
        //}


        public static Dictionary<int, Color> GeneratePalette(int[] colors, params int[] colorIDs)
        {
            if (colors.Length <= 0 || colorIDs.Length <= 0) return new();
            Dictionary<int, Color> palette = new();
            int size = colors.Length;
            if (colorIDs.Length < size) size = colorIDs.Length;
            for (int i = 0; i < size; i++)
            {
                palette.Add(colorIDs[i], HexToColor(colors[i]));
            }
            return palette;
        }
        public static Dictionary<int, Color> GeneratePalette(string[] hexColors, params int[] colorIDs)
        {
            if (hexColors.Length <= 0 || colorIDs.Length <= 0) return new();
            Dictionary<int, Color> palette = new();
            int size = hexColors.Length;
            if (colorIDs.Length < size) size = colorIDs.Length;
            for (int i = 0; i < size; i++)
            {
                palette.Add(colorIDs[i], HexToColor(hexColors[i]));
            }
            return palette;
        }

        public static ColorPalette GeneratePalette(string paletteName, int[] colors, params int[] colorIDs)
        {
            return new(paletteName, GeneratePalette(colors, colorIDs));
        }
        public static ColorPalette GeneratePalette(string paletteName, string[] hexColors, params int[] colorIDs)
        {
            return new(paletteName, GeneratePalette(hexColors, colorIDs));
        }

        public static Color[] GeneratePalette(params int[] colors)
        {
            Raylib_CsLo.Color[] palette = new Raylib_CsLo.Color[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                palette[i] = HexToColor(colors[i]);
            }
            return palette;
        }
        public static Color[] GeneratePalette(params string[] hexColors)
        {
            Raylib_CsLo.Color[] palette = new Raylib_CsLo.Color[hexColors.Length];
            for (int i = 0; i < hexColors.Length; i++)
            {
                palette[i] = HexToColor(hexColors[i]);
            }
            return palette;
        }

        public static Color HexToColor(int colorValue)
        {
            byte[] rgb = BitConverter.GetBytes(colorValue);
            if (!BitConverter.IsLittleEndian) Array.Reverse(rgb);
            byte r = rgb[2];
            byte g = rgb[1];
            byte b = rgb[0];
            byte a = 255;
            return new(r, g, b, a);
        }
        public static Color HexToColor(string hexColor)
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

            return new(red, green, blue, 255);
        }


    }

}
