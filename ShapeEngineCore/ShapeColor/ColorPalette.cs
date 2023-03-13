using Raylib_CsLo;


namespace ShapeColor
{
    public class ColorPalette
    {
        private string name = "";
        private Dictionary<int, Color> palette = new();

        public ColorPalette(string paletteName) { this.name = paletteName; }
        public ColorPalette(string paletteName, params (int id, Color color)[] entries)
        {
            this.name = paletteName;
            foreach (var entry in entries)
            {
                palette.Add(entry.id, entry.color);
            }
        }
        public ColorPalette(string paletteName, Color[] colors, int[] ids)
        {
            this.name = paletteName;
            for (int i = 0; i < ids.Length; i++)
            {
                palette.Add(ids[i], colors[i]);
            }
        }
        public ColorPalette(string paletteName, string[] hexColors, int[] ids)
        {
            this.name = paletteName;
            for (int i = 0; i < ids.Length; i++)
            {
                palette.Add(ids[i], PaletteHandler.HexToColor(hexColors[i]));
            }
        }
        public ColorPalette(string paletteName, Dictionary<int, Color> palette)
        {
            this.name = paletteName;
            this.palette = palette;
        }

        
        public string Name { get { return name; } }
        public Color Get(int id)
        {
            if (!palette.ContainsKey(id)) return WHITE;
            else return palette[id];
        }
        public List<Color> GetAllColors()
        {
            return palette.Values.ToList();
        }

        //public void Draw(int x, int y, int width, int height)
        //{
        //    var colors = GetAllColors();
        //    int colorWidth = width / colors.Count;
        //    for (int i = 0; i < colors.Count; i++)
        //    {
        //        Raylib.DrawRectangle(x + colorWidth * i, y, colorWidth, height, colors[i]);
        //    }
        //}
    }

    
}
