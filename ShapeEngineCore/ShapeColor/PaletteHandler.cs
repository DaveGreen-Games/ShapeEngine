using Raylib_CsLo;
using ShapeLib;

namespace ShapeColor
{
    public class PaletteHandler
    {
        private Dictionary<uint, ColorPalette> palettes = new();
        private int curPaletteIndex = 0;
        private List<uint> paletteIDs = new() { };
        private ColorPalette? curPalette = null;// = new(SID.NextID);



        //public List<uint> GetAllPaletteNames() { return paletteNames; }
        public int CurPaletteIndex { get { return curPaletteIndex; } }
        public ColorPalette? CurPalette { get { return curPalette; } }
        public Color C(uint id) { return curPalette != null ? curPalette.Get(id) : WHITE; }

        public void AddPalette(uint paletteID, string[] hexColors, uint[] ids)
        {
            ColorPalette cp = new(paletteID, hexColors, ids);
            if (palettes.ContainsKey(paletteID)) palettes[paletteID] = cp;
            else
            {
                palettes.Add(paletteID, cp);
                paletteIDs.Add(paletteID);
            }
        }
        public void AddPalette(uint paletteID, Color[] colors, uint[] ids)
        {
            ColorPalette cp = new(paletteID, colors, ids);
            if (palettes.ContainsKey(paletteID)) palettes[paletteID] = cp;
            else
            {
                palettes.Add(paletteID, cp);
                paletteIDs.Add(paletteID);
            }
        }
        public void AddPalette(uint paletteID, params (uint id, Color color)[] entries)
        {
            ColorPalette cp = new(paletteID, entries);
            if (palettes.ContainsKey(paletteID)) palettes[paletteID] = cp;
            else
            {
                palettes.Add(paletteID, cp);
                paletteIDs.Add(paletteID);
            }
        }
        public void AddPalette(uint paletteID, Dictionary<uint, Color> palette)
        {
            ColorPalette cp = new(paletteID, palette);
            if (palettes.ContainsKey(paletteID)) palettes[paletteID] = cp;
            else
            {
                palettes.Add(paletteID, cp);
                paletteIDs.Add(paletteID);
            }
        }
        public void AddPalette(ColorPalette palette)
        {
            //string paletteName = palette.Name;
            if (palettes.ContainsKey(palette.ID)) palettes[palette.ID] = palette;
            else
            {
                palettes.Add(palette.ID, palette);
                paletteIDs.Add(palette.ID);
            }
        }
        
        
        public void RemovePalette(uint paletteID)
        {
            //if (paletteName == "default") return;
            if (!palettes.ContainsKey(paletteID)) return;
            if (curPalette.ID == paletteID) Next();
            palettes.Remove(paletteID);
            paletteIDs.Remove(paletteID);
            //if (curPalette.Name == paletteName)
            //{
            //    Next();
            //}
        }
        public void ChangePalette(uint paletteID)
        {
            //if (CurName == newPalette) return;

            if (palettes.ContainsKey(paletteID))
            {
                curPalette = palettes[paletteID];
                //curPaletteName = newPalette;
                curPaletteIndex = paletteIDs.IndexOf(paletteID);
            }
        }
        public void ChangePalette(int index)
        {
            //if (index == curPaletteIndex) return;
            if (index < 0) { index = paletteIDs.Count - 1; }
            else if (index >= paletteIDs.Count) { index = 0; }

            curPaletteIndex = index;
            //curPaletteName = paletteNames[index];
            curPalette = palettes[paletteIDs[index]];

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
            paletteIDs.Clear();
            paletteIDs.Clear();
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


        public static Dictionary<uint, Color> GeneratePalette(int[] colors, params uint[] colorIDs)
        {
            if (colors.Length <= 0 || colorIDs.Length <= 0) return new();
            Dictionary<uint, Color> palette = new();
            int size = colors.Length;
            if (colorIDs.Length < size) size = colorIDs.Length;
            for (int i = 0; i < size; i++)
            {
                palette.Add(colorIDs[i], HexToColor(colors[i]));
            }
            return palette;
        }
        public static Dictionary<uint, Color> GeneratePalette(string[] hexColors, params uint[] colorIDs)
        {
            if (hexColors.Length <= 0 || colorIDs.Length <= 0) return new();
            Dictionary<uint, Color> palette = new();
            int size = hexColors.Length;
            if (colorIDs.Length < size) size = colorIDs.Length;
            for (int i = 0; i < size; i++)
            {
                palette.Add(colorIDs[i], HexToColor(hexColors[i]));
            }
            return palette;
        }

        public static ColorPalette GeneratePalette(uint paletteID, int[] colors, params uint[] colorIDs)
        {
            return new(paletteID, GeneratePalette(colors, colorIDs));
        }
        public static ColorPalette GeneratePalette(uint paletteID, string[] hexColors, params uint[] colorIDs)
        {
            return new(paletteID, GeneratePalette(hexColors, colorIDs));
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
