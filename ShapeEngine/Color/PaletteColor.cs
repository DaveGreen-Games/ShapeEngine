using ShapeEngine.Lib;

namespace ShapeEngine.Color;

public class PaletteColor
{
    public ShapeColor Color;
    public readonly int ID;

    public PaletteColor()
    {
        this.ID = (int)ShapeID.NextID;
        Color = new(); //randomize
    }
    public PaletteColor(ShapeColor color)
    {
        this.ID = (int)ShapeID.NextID;
        this.Color = color;
    }
    public PaletteColor(int id, ShapeColor color)
    {
        this.ID = id;
        this.Color = color;
    }
        
}