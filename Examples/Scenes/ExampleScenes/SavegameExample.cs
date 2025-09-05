using ShapeEngine.Serialization;
using System.Drawing;
using System.Xml.Serialization;
using ShapeEngine.Color;
using ShapeEngine.Geometry.RectDef;

namespace Examples.Scenes.ExampleScenes;



// Todo:
// - This scene should be able to generate something, then save it to a savegame, then load it back from the savegame.
// - It should be able to reset the savegame as well.
// - Maybe it has multiple savegame slots as well (to demonstrate that functionality).
// - For instance:
//   - Generate a random number in a random positioned / sized rect with a random color
//   - The user can:
//      - regenerate the number
//      - drag the top left corner to move the rect
//      - drag the bottom right corner to resize the rect
//      - regnerate a new random color
//      - save the current state to a savegame slot
//      - load the current state from a savegame slot
//      - reset the savegame slot
//      - switch between savegame slots


public record ExampleSavegameData : DataObject
{
    [XmlElement("Slot")]
    public int Slot { get; set; }
    
    [XmlElement("Value")]
    public int Value { get; set; }
    [XmlElement("RectX")]
    public float RectX { get; set; }
    [XmlElement("RectY")]
    public float RectY { get; set; }
    [XmlElement("RectWidth")]
    public float RectWidth { get; set; }
    [XmlElement("RectHeight")]
    public float RectHeight { get; set; }
    [XmlElement("RectColor")]
    public Color RectColor { get; set; }

    [XmlElement("Name")]
    public new string Name { get; set; }
    [XmlElement("SpawnWeight")]
    public new int SpawnWeight { get; set; }
    
    public ExampleSavegameData(int slot, int value, Rect rect, ColorRgba color)
    {
        Slot = slot;
        Value = value;
        RectX = rect.X;
        RectY = rect.Y;
        RectWidth = rect.Width;
        RectHeight = rect.Height;
        RectColor = color.ToSysColor();
        
        Name = $"ExampleSavegameData-Slot{Slot}";
        SpawnWeight = 0;
    }
    
}




public class SavegameExample : ExampleScene
{
    
}