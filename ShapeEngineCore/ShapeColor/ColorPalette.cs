using Raylib_CsLo;
using ShapeLib;

namespace ShapeColor
{

    public interface IColorPalette
    {
        public uint GetID();
        public Color Get(uint id);
        public List<Color> GetAllColors();
        public Dictionary<uint, Color> GetRandomized();
        public void Randomize();
    }


    public class ColorPalette : IColorPalette
    {
        private uint id;
        //public uint ID { get; private set; }
        private Dictionary<uint, Color> palette = new();

        public ColorPalette(uint paletteID) { this.id = paletteID; }
        public ColorPalette(uint paletteID, params (uint id, Color color)[] entries)
        {
            this.id = paletteID;
            foreach (var entry in entries)
            {
                palette.Add(entry.id, entry.color);
            }
        }
        public ColorPalette(uint paletteID, Color[] colors, uint[] ids)
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
                palette.Add(ids[i], SColor.HexToColor(hexColors[i]));
            }
        }
        public ColorPalette(uint paletteID, Dictionary<uint, Color> palette)
        {
            this.id = paletteID;
            this.palette = palette;
        }

        public uint GetID() { return id; }
        public Color Get(uint id)
        {
            if (!palette.ContainsKey(id)) return WHITE;
            else return palette[id];
        }
        public List<Color> GetAllColors()
        {
            return palette.Values.ToList();
        }
        public void Randomize()
        {
            palette = GetRandomized();
        }
        public Dictionary<uint, Color> GetRandomized()
        {
            var colors = GetAllColors();
            Dictionary<uint, Color> result = new();
            foreach (var key in palette.Keys)
            {
                result.Add(key, SRNG.randCollection(colors, true));
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