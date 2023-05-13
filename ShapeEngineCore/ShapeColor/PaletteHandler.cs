using Raylib_CsLo;
using ShapeLib;

namespace ShapeColor
{

    //public struct C
    //{
    //    public byte r;
    //    public byte g;
    //    public byte b;
    //    public byte a;
    //
    //
    //    public C(byte r, byte g, byte b, byte a)
    //    {
    //        this.r = r;
    //        this.g = g;
    //        this.b = b;
    //        this.a = a;
    //    }
    //
    //    public C(int r, int g, int b, int a)
    //    {
    //        this.r = (byte)r;
    //        this.g = (byte)g;
    //        this.b = (byte)b;
    //        this.a = (byte)a;
    //    }
    //
    //
    //
    //    public Color Color { get { return  new Color(r, g, b, a); } }
    //}


    public class PaletteHandler
    {
        private Dictionary<uint, IColorPalette> palettes = new();
        private List<uint> paletteIDs = new() { };
        
        public int CurPaletteIndex { get; private set; } = 0;
        public IColorPalette? CurPalette { get; private set; } = null;

        
        public Color C(uint id) { return CurPalette != null ? CurPalette.Get(id) : WHITE; }

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
        public void AddPalette(IColorPalette palette)
        {
            if (palettes.ContainsKey(palette.GetID())) palettes[palette.GetID()] = palette;
            else
            {
                palettes.Add(palette.GetID(), palette);
                paletteIDs.Add(palette.GetID());
            }
        }
        
        
        public bool RemovePalette(uint paletteID)
        {
            if (!palettes.ContainsKey(paletteID)) return false;
            if (CurPalette != null && CurPalette.GetID() == paletteID) Next();
            palettes.Remove(paletteID);
            paletteIDs.Remove(paletteID);
            return true;
        }
        public bool ChangePalette(uint paletteID)
        {
            if (palettes.ContainsKey(paletteID))
            {
                CurPalette = palettes[paletteID];
                CurPaletteIndex = paletteIDs.IndexOf(paletteID);
                return true;
            }
            return false;
        }
        public bool ChangePalette(int index)
        {
            if (paletteIDs.Count <= 1) return false;
            if (index == CurPaletteIndex) return false;
            if (index < 0) { index = paletteIDs.Count - 1; }
            else if (index >= paletteIDs.Count) { index = 0; }

            CurPaletteIndex = index;
            CurPalette = palettes[paletteIDs[index]];
            return true;
        }
        public bool Next() { return ChangePalette(CurPaletteIndex + 1); }
        public bool Previous() { return ChangePalette(CurPaletteIndex - 1); }
        public void Close()
        {
            paletteIDs.Clear();
            paletteIDs.Clear();
        }


        public static ColorPalette GeneratePalette(uint paletteID, int[] colors, params uint[] colorIDs)
        {
            return new(paletteID, SColor.GeneratePalette(colors, colorIDs));
        }
        public static ColorPalette GeneratePalette(uint paletteID, string[] hexColors, params uint[] colorIDs)
        {
            return new(paletteID, SColor.GeneratePalette(hexColors, colorIDs));
        }

    }

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