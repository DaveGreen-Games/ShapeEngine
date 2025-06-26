using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Text;

public static class BinaryDrawerTester
{
    public static readonly BinaryDrawer BinaryDrawer3x5Standard = 
        new BinaryDrawer
        (
            new Dictionary<char, int[]>()
            {
                {'0', new [] { 1,1,1,1,0,1,1,0,1,1,0,1,1,1,1 }},
                {'1', new [] { 0,1,0,0,1,0,0,1,0,0,1,0,0,1,0 }},
                {'2', new [] { 1,1,1,0,0,1,1,1,1,1,0,0,1,1,1 }},
                {'3', new [] { 1,1,1,0,0,1,0,1,1,0,0,1,1,1,1 }},
                {'4', new [] { 1,0,0,1,0,0,1,1,1,0,1,0,0,1,0 }},
                {'5', new [] { 1,1,1,1,0,0,1,1,1,0,0,1,1,1,1 }},
                {'6', new [] { 1,0,0,1,0,0,1,1,1,1,0,1,1,1,1 }},
                {'7', new [] { 1,1,1,0,0,1,0,1,1,0,0,1,0,0,1 }},
                {'8', new [] { 1,1,1,1,0,1,1,1,1,1,0,1,1,1,1 }},
                {'9', new [] { 1,1,1,1,0,1,1,1,1,0,0,1,0,0,1 }},
            },
            3, 5,
            new((rect, code, index, maxIndex) =>
            {
                if (code > 0)
                {
                    if (index == 1 || index == 7 || index == 13)
                    {
                        rect.ApplyMargins(0f, 0f, 0.1f, 0.1f).Draw(new ColorRgba(255, 0, 0, 255));
                    }
                    else rect.ApplyMargins(0.15f, 0.15f, 0f, 0f).Draw(new ColorRgba(255, 0, 0, 255));
                    // rect.DrawLines(rect.Size.Min() * 0.1f, ColorRgba.White);
                    
                    // rect.Draw(new ColorRgba(255, 0, 0, 255));
                }
            }),
            new((rect, c) => { })
            );
}

public class BinaryDrawer
{
    private readonly Dictionary<char, int[]> binary;
    private readonly int gridWidth;
    private readonly int gridHeight;
    private readonly Action<Rect, int, int, int> cellDrawer;
    private readonly Action<Rect, char> backgroundDrawer;
    public int GridSize => gridWidth * gridHeight;
    public BinaryDrawer(Dictionary<char, int[]> binary, int gridWidth, int gridHeight, Action<Rect, int, int, int> cellDrawer, Action<Rect, char> backgroundDrawer)
    {
        this.binary = binary;
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;
        this.cellDrawer = cellDrawer;
        this.backgroundDrawer = backgroundDrawer;
    }

    public void Draw(string text, Rect rect, float spacing = 0.05f)
    {
        var chars = text.ToCharArray();
        // var split = rect.SplitH(chars.Length);
        var split = rect.GetAlignedRectsGrid(new Grid(chars.Length, 1), new Size(spacing, 0f));
        if (split == null) return;
        
        
        for (int i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if(!binary.TryGetValue(c, out int[]? binaryCode)) continue;
            backgroundDrawer(split[i], c);
            var grid = split[i].Split(gridWidth, gridHeight, true);
            for (int j = 0; j < grid.Count; j++)
            {
                cellDrawer(grid[j], binaryCode[j], j, GridSize);
            }
        }
    }
}