using Raylib_CsLo;


namespace ShapeColor
{
    public class ColorPalette
    {
        public uint ID { get; private set; }
        private Dictionary<uint, Color> palette = new();

        public ColorPalette(uint paletteID) { this.ID = paletteID; }
        public ColorPalette(uint paletteID, params (uint id, Color color)[] entries)
        {
            this.ID = paletteID;
            foreach (var entry in entries)
            {
                palette.Add(entry.id, entry.color);
            }
        }
        public ColorPalette(uint paletteID, Color[] colors, uint[] ids)
        {
            this.ID = paletteID;
            for (int i = 0; i < ids.Length; i++)
            {
                palette.Add(ids[i], colors[i]);
            }
        }
        public ColorPalette(uint paletteID, string[] hexColors, uint[] ids)
        {
            this.ID = paletteID;
            for (int i = 0; i < ids.Length; i++)
            {
                palette.Add(ids[i], PaletteHandler.HexToColor(hexColors[i]));
            }
        }
        public ColorPalette(uint paletteID, Dictionary<uint, Color> palette)
        {
            this.ID = paletteID;
            this.palette = palette;
        }

        
        public Color Get(uint id)
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
