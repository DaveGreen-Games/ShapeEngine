using Raylib_CsLo;


namespace ShapeColor
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
        private Dictionary<string, Raylib_CsLo.Color> palette = new();

        public ColorPalette(string paletteName) { this.name = paletteName; }
        public ColorPalette(string paletteName, params (string name, Raylib_CsLo.Color color)[] entries)
        {
            this.name = paletteName;
            foreach (var entry in entries)
            {
                palette.Add(entry.name, entry.color);
            }
        }
        public ColorPalette(string paletteName, Raylib_CsLo.Color[] colors, string[] names)
        {
            this.name = paletteName;
            for (int i = 0; i < names.Length; i++)
            {
                palette.Add(names[i], colors[i]);
            }
        }
        public ColorPalette(string paletteName, string[] hexColors, string[] names)
        {
            this.name = paletteName;
            for (int i = 0; i < names.Length; i++)
            {
                palette.Add(names[i], PaletteHandler.HexToColor(hexColors[i]));
            }
        }
        public ColorPalette(string paletteName, Dictionary<string, Raylib_CsLo.Color> palette)
        {
            this.name = paletteName;
            this.palette = palette;
        }

        
        public string Name { get { return name; } }
        public Raylib_CsLo.Color Get(string name)
        {
            if (!palette.ContainsKey(name)) return WHITE;
            else return palette[name];
        }
        public List<Raylib_CsLo.Color> GetAllColors()
        {
            return palette.Values.ToList();
        }

        public void Draw(int x, int y, int width, int height)
        {
            var colors = GetAllColors();
            int colorWidth = width / colors.Count;
            for (int i = 0; i < colors.Count; i++)
            {
                Raylib.DrawRectangle(x + colorWidth * i, y, colorWidth, height, colors[i]);
            }
        }
    }

    
}
