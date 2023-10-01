using ShapeEngine.Lib;

namespace ShapeEngine.Color
{

    public interface IColorPalette
    {
        public uint GetID();
        public Raylib_CsLo.Color Get(uint id);
        public List<Raylib_CsLo.Color> GetAllColors();
        public Dictionary<uint, Raylib_CsLo.Color> GetRandomized();
        public void Randomize();
    }


    public class ColorPalette : IColorPalette
    {
        private uint id;
        //public uint ID { get; private set; }
        private Dictionary<uint, Raylib_CsLo.Color> palette = new();

        public ColorPalette(uint paletteID) { this.id = paletteID; }
        public ColorPalette(uint paletteID, params (uint id, Raylib_CsLo.Color color)[] entries)
        {
            this.id = paletteID;
            foreach (var entry in entries)
            {
                palette.Add(entry.id, entry.color);
            }
        }
        public ColorPalette(uint paletteID, Raylib_CsLo.Color[] colors, uint[] ids)
        {
            this.id = paletteID;
            for (int i = 0; i < ids.Length; i++)
            {
                palette.Add(ids[i], colors[i]);
            }
        }
        public ColorPalette(uint paletteID, string[] hexColors, uint[] ids)
        {
            this.id = paletteID;
            for (int i = 0; i < ids.Length; i++)
            {
                palette.Add(ids[i], ShapeColor.HexToColor(hexColors[i]));
            }
        }
        public ColorPalette(uint paletteID, Dictionary<uint, Raylib_CsLo.Color> palette)
        {
            this.id = paletteID;
            this.palette = palette;
        }

        public uint GetID() { return id; }
        public Raylib_CsLo.Color Get(uint id)
        {
            if (!palette.ContainsKey(id)) return WHITE;
            else return palette[id];
        }
        public List<Raylib_CsLo.Color> GetAllColors()
        {
            return palette.Values.ToList();
        }
        public void Randomize()
        {
            palette = GetRandomized();
        }
        public Dictionary<uint, Raylib_CsLo.Color> GetRandomized()
        {
            var colors = GetAllColors();
            Dictionary<uint, Raylib_CsLo.Color> result = new();
            foreach (var key in palette.Keys)
            {
                result.Add(key, ShapeRandom.randCollection(colors, true));
            }
            return result;
        }
    }

    
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