using Raylib_CsLo;
using ShapeEngineCore.Globals.Persistent;


namespace ShapeEngineCore.Globals
{
    /*
    public struct Palette
    {
        public readonly Color bg1;
        public readonly Color bg2;
        public readonly Color flash;
        public readonly Color special1;
        public readonly Color special2;
        public readonly Color text;
        public readonly Color header;
        public readonly Color player;
        public readonly Color neutral;
        public readonly Color enemy;
        public readonly Color armor;
        public readonly Color acid;
        public readonly Color shield;
        public readonly Color radiation;
        public readonly Color energy;
        public readonly Color darkMatter;

        public Palette(ColorData data)
        {

            bg1 = PaletteHandler.HexToColor(data.bg1);
            bg2 = PaletteHandler.HexToColor(data.bg2);
            flash = PaletteHandler.HexToColor(data.flash);
            text = PaletteHandler.HexToColor(data.text);
            header = PaletteHandler.HexToColor(data.header);
            special1 = PaletteHandler.HexToColor(data.special1);
            special2 = PaletteHandler.HexToColor(data.special2);
            player = PaletteHandler.HexToColor(data.player);
            neutral = PaletteHandler.HexToColor(data.neutral);
            enemy = PaletteHandler.HexToColor(data.enemy);
            armor = PaletteHandler.HexToColor(data.armor);
            acid = PaletteHandler.HexToColor(data.acid);
            shield = PaletteHandler.HexToColor(data.shield);
            radiation = PaletteHandler.HexToColor(data.radiation);
            energy = PaletteHandler.HexToColor(data.energy);
            darkMatter = PaletteHandler.HexToColor(data.darkMatter);
        }


        //public Palette(Color[] colors)
        //{
        //    bg1 = colors[0];
        //    bg2 = colors[1];
        //    text = colors[2];
        //    neutral = colors[3];
        //    player = colors[4];
        //    addon = colors[5];
        //    flash = colors[6];
        //    enemy = colors[7];
        //    special = colors[8];
        //    special2 = colors[9];
        //}
        //
        //public Palette(string[] colors)
        //{
        //    bg1 = ColorPalette.HexToColor(colors[0]);
        //    bg2 = ColorPalette.HexToColor(colors[1]);
        //    text = ColorPalette.HexToColor(colors[2]);
        //    neutral = ColorPalette.HexToColor(colors[3]);
        //    player = ColorPalette.HexToColor(colors[4]);
        //    addon = ColorPalette.HexToColor(colors[5]);
        //    flash = ColorPalette.HexToColor(colors[6]);
        //    enemy = ColorPalette.HexToColor(colors[7]);
        //    special = ColorPalette.HexToColor(colors[8]);
        //    special2 = ColorPalette.HexToColor(colors[9]);
        //}

    }
    */
    public class ColorPalette
    {
        private string name = "";
        private Dictionary<string, Color> palette = new();

        public ColorPalette(string paletteName) { this.name = paletteName; }
        public ColorPalette(string paletteName, params (string name, Color color)[] entries)
        {
            this.name = paletteName;
            foreach (var entry in entries)
            {
                palette.Add(entry.name, entry.color);
            }
        }

        public ColorPalette(string paletteName, Dictionary<string, Color> palette)
        {
            this.name = paletteName;
            this.palette = palette;
        }

        public string Name { get { return name; } }
        public Color Get(string name)
        {
            if (!palette.ContainsKey(name)) return WHITE;
            else return palette[name];
        }
    }

    public static class PaletteHandler
    {
        private static Dictionary<string, ColorPalette> palettes = new();
        private static int curPaletteIndex = 0;
        private static List<string> paletteNames = new() { "default" };
        private static ColorPalette curPalette = new ColorPalette("default", 
            ("black", BLACK), ("white", WHITE), ("gray", GRAY), 
            ("green", GREEN), ("blue", BLUE), ("red", RED), 
            ("orange", ORANGE), ("purple", PURPLE), ("pink", PINK) );

        

        public static void Initialize()
        {
            AddPalette(curPalette);
        }

        public static int CurIndex { get { return curPaletteIndex; } }
        public static string CurName { get { return curPalette.Name; } }
        public static ColorPalette Cur { get { return curPalette; } }
        public static Color C(string name) {return curPalette.Get(name);}
        public static void AddPalette(string paletteName, params (string name, Color color)[] entries)
        {
            ColorPalette cp = new(paletteName, entries);
            if (palettes.ContainsKey(paletteName)) palettes[paletteName] = cp;
            else
            {
                palettes.Add(paletteName, cp);
                paletteNames.Add(paletteName);
            }
        }
        public static void AddPalette(string paletteName, Dictionary<string, Color> palette)
        {
            ColorPalette cp = new(paletteName, palette);
            if (palettes.ContainsKey(paletteName)) palettes[paletteName] = cp;
            else
            {
                palettes.Add(paletteName, cp);
                paletteNames.Add(paletteName);
            }
        }
        public static void AddPalette(ColorPalette palette)
        {
            string paletteName = palette.Name;
            if (palettes.ContainsKey(paletteName)) palettes[paletteName] = palette;
            else
            {
                palettes.Add(paletteName, palette);
                paletteNames.Add(paletteName);
            }
        }
        public static void RemovePalette(string paletteName)
        {
            if (paletteName == "default") return;
            if (!palettes.ContainsKey(paletteName)) return;

            if(curPalette.Name == paletteName)
            {
                Next();
            }
            palettes.Remove(paletteName);
            paletteNames.Remove(paletteName);
        }
        public static void ChangePalette(string newPalette)
        {
            if (CurName == newPalette) return;

            if (palettes.ContainsKey(newPalette))
            {
                curPalette = palettes[newPalette];
                //curPaletteName = newPalette;
                curPaletteIndex = paletteNames.IndexOf(newPalette);
            }
        }
        public static void ChangePalette(int index)
        {
            if (index == curPaletteIndex) return;
            if (index < 0) { index = paletteNames.Count - 1; }
            else if (index >= paletteNames.Count) { index = 0; }

            curPaletteIndex = index;
            //curPaletteName = paletteNames[index];
            curPalette = palettes[paletteNames[index]];

        }
        public static void Next()
        {
            ChangePalette(curPaletteIndex + 1);
        }
        public static void Previous()
        {
            ChangePalette(curPaletteIndex - 1);
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

        public static Color ChangeAlpha(Color c, byte a)
        {
            c.a = a;
            return c;
        }
        public static Color ChangeAlpha(Color c, int change)
        {
            int newAlpha = c.a - change;
            c.a = (byte)newAlpha;
            return c;
        }
        public static Color ChangeRed(Color c, byte r)
        {
            c.r = r;
            return c;
        }
        public static Color ChangeGreen(Color c, byte g)
        {
            c.g = g;
            return c;
        }
        public static Color ChangeBlue(Color c, byte b)
        {
            c.b = b;
            return c;
        }
    }
}
