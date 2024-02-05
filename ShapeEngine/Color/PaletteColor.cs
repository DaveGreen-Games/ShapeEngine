using ShapeEngine.Lib;

namespace ShapeEngine.Color;

public class PaletteColor
{
    public ColorRgba ColorRgba { get; internal set; }
    public readonly int ID;

    public PaletteColor()
    {
        this.ID = (int)ShapeID.NextID;
        ColorRgba = new(); //randomize
    }
    public PaletteColor(ColorRgba colorRgba)
    {
        this.ID = (int)ShapeID.NextID;
        this.ColorRgba = colorRgba;
    }
    public PaletteColor(int id, ColorRgba colorRgba)
    {
        this.ID = id;
        this.ColorRgba = colorRgba;
    }
        
}