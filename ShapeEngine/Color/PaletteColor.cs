using ShapeEngine.Lib;

namespace ShapeEngine.Color;

public class PaletteColor
{
    private static int IdCounter = 0;
    private static int GetNextId() => IdCounter++;
    
    public ColorRgba ColorRgba { get; internal set; }
    public readonly int ID;

    public PaletteColor()
    {
        this.ID = GetNextId();
        ColorRgba = new(); //randomize
    }
    public PaletteColor(ColorRgba colorRgba)
    {
        this.ID = GetNextId();
        this.ColorRgba = colorRgba;
    }
    public PaletteColor(int id, ColorRgba colorRgba)
    {
        this.ID = id;
        this.ColorRgba = colorRgba;
    }
    
    public PaletteColor Clone() => new(this.ID, this.ColorRgba);
    public PaletteColor Clone(ColorRgba colorRgba) => new(this.ID, colorRgba);
        
}