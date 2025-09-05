using ShapeEngine.Serialization;
using System.Drawing;
using System.Numerics;
using System.Xml.Serialization;
using ShapeEngine.Color;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.StripedDrawingDef;
using ShapeEngine.Input;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;
using ShapeEngine.Text;

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



public record ExampleSavegameProfileData : DataObject
{
    public static ExampleSavegameProfileData Default()
    {
        var data = new ExampleSavegameProfileData()
        {
            SavegameSlot = 0,
            Name = "ExampleSavegameProfileData",
            SpawnWeight = 0
        };
        return data;

    }

    [XmlElement("SavegameSlot")]
    public int SavegameSlot { get; set; }

    [XmlElement("Name")]
    public new string Name { get; set; }

    [XmlElement("SpawnWeight")]
    public new int SpawnWeight { get; set; }
}
public record ExampleSavegameData : DataObject
{

    public static ExampleSavegameData Random(int slot, Rect area)
    {
        var rand = Rng.Instance;
        var minAreaSize = area.Size.Min();
        float minRectSize = minAreaSize * 0.1f;
        float maxRectSize = minRectSize * 0.5f;
        int minValue = 1;
        int maxValue = 999;
        
        var randRect = rand.RandRect(area, minRectSize, maxRectSize, AnchorPoint.TopLeft);
        var randColor = rand.RandColor(100, 255, 255);
        var data = new ExampleSavegameData()
        {
            Slot = slot,
            Value = rand.RandI(minValue, maxValue),
            RectX = randRect.X,
            RectY = randRect.Y,
            RectWidth = randRect.Width,
            RectHeight = randRect.Height,
            RectColorA = randColor.A,
            RectColorR = randColor.R,
            RectColorG = randColor.G,
            RectColorB = randColor.B,
            Name = "ExampleSavegameData-Slot" + slot,
            SpawnWeight = 0
        };
        return data;
    }
    
    
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
    
    [XmlElement("RectColorR")]
    public int RectColorR { get; set; }
    
    [XmlElement("RectColorG")]
    public int RectColorG { get; set; }
    
    [XmlElement("RectColorB")]
    public int RectColorB { get; set; }
    
    [XmlElement("RectColorA")]
    public int RectColorA { get; set; }

    [XmlElement("Name")]
    public new string Name { get; set; }
    
    [XmlElement("SpawnWeight")]
    public new int SpawnWeight { get; set; }
    
    public ColorRgba RectColor => new(RectColorR, RectColorG, RectColorB, RectColorA);
}




public class SavegameExample : ExampleScene
{
    private const int MaxSavegameSlots = 3;
    private int currentSavegameSlot;
    private string CurrentSavegameFileName => $"SavegameExample-Slot{currentSavegameSlot}.xml";

    private ExampleSavegameProfileData currentProfileData;
    private ExampleSavegameData currentSavegameData;
    
    private readonly DirectoryInfo? saveDirectory;
    private readonly XmlDataObjectSerializer<ExampleSavegameData> dataSerializer;
    private readonly XmlDataObjectSerializer<ExampleSavegameProfileData> profileSerializer;

    
    private Rect rect = new();
    private ColorRgba color = new();
    private int value;
    private TextFont valueFont;

    private float rectLineThickness = 2f;
    private float rectCornerSize = 5f;
    private float rectCornerSelectionRadius = 20f;
    
    private bool draggingTopLeft;
    private bool draggingBottomRight;
    private bool nearTopLeft;
    private bool nearBottomRight;
    
    public SavegameExample()
    {
        var saveDirectoryPath = Game.Instance.SaveDirectoryPath;
        var saveFolder = "SaveGameExampleScene";
        var fullPath = Path.Combine(saveDirectoryPath, saveFolder);
        
        saveDirectory = ShapeFileManager.CreateDirectory(fullPath);
        
        valueFont = textFont;
        
        dataSerializer = new XmlDataObjectSerializer<ExampleSavegameData>();
        profileSerializer = new XmlDataObjectSerializer<ExampleSavegameProfileData>();
        currentProfileData = LoadProfileData();
        currentSavegameSlot = currentProfileData.SavegameSlot;
        currentSavegameData = LoadSavegameData();

        ApplySavegameData();

    }

    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        var mousePosUI = ui.MousePos;
        
        rectLineThickness = ui.Area.Size.Min() * 0.004f;
        rectCornerSize = rectLineThickness * 2f;
        rectCornerSelectionRadius = rectCornerSize * 2f;
        
        var lmbState = ShapeMouseButton.LEFT.GetInputState();
        
        nearTopLeft = (rect.TopLeft - mousePosUI).LengthSquared() < rectCornerSelectionRadius * rectCornerSelectionRadius;
        if (nearTopLeft)
        {
            if (lmbState.Pressed)
            {
                nearTopLeft = false;
                nearBottomRight = false;
                draggingBottomRight = false;
                draggingTopLeft = true;
            }
        }
        else
        {
            nearBottomRight = (rect.BottomRight - mousePosUI).LengthSquared() < rectCornerSelectionRadius * rectCornerSelectionRadius;
            if (nearBottomRight)
            {
                if (lmbState.Pressed)
                {
                    nearTopLeft = false;
                    nearBottomRight = false;
                    draggingTopLeft = false;
                    draggingBottomRight = true;
                }
            }
        }

        if (lmbState.Released)
        {
            draggingTopLeft = false;
            draggingBottomRight = false;
        }
        
        
        if (draggingTopLeft)
        {
            rect = new Rect(ui.MousePos, rect.BottomRight);
        }
        else if (draggingBottomRight)
        {
            rect = new Rect(rect.TopLeft, ui.MousePos);
        }
    }

    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        
        rect.DrawStriped(rectCornerSelectionRadius, 45f, new LineDrawingInfo(rectLineThickness / 2, color.SetAlpha(150)), 0f);
        rect.DrawLines(rectLineThickness, color);

        if (draggingTopLeft)
        {
            CircleDrawing.DrawCircleFast(rect.TopLeft, rectCornerSelectionRadius, ColorRgba.White);
            CircleDrawing.DrawCircleFast(rect.BottomRight, rectCornerSize, color);
        }
        else if (draggingBottomRight)
        {
            CircleDrawing.DrawCircleFast(rect.TopLeft, rectCornerSize, color);
            CircleDrawing.DrawCircleFast(rect.BottomRight, rectCornerSelectionRadius, ColorRgba.White);
        }
        else if (nearTopLeft)
        {
            CircleDrawing.DrawCircleFast(rect.TopLeft, rectCornerSelectionRadius, color);
            CircleDrawing.DrawCircleFast(rect.BottomRight, rectCornerSize, color);
        }
        else if (nearBottomRight)
        {
            CircleDrawing.DrawCircleFast(rect.TopLeft, rectCornerSize, color);
            CircleDrawing.DrawCircleFast(rect.BottomRight, rectCornerSelectionRadius, color);
        }
        else
        {
            CircleDrawing.DrawCircleFast(rect.TopLeft, rectCornerSize, color);
            CircleDrawing.DrawCircleFast(rect.BottomRight, rectCornerSize, color);
        }
        
        
        
        var valueText = $"Value: {value}";
        valueFont.DrawWord(valueText, rect, AnchorPoint.Center);
    }


    #region Load and Save
    
    private int NextSavegameSlot()
    {
        var newSlot = currentSavegameSlot + 1;
        if(newSlot >= MaxSavegameSlots) newSlot = 0;
        currentSavegameSlot = newSlot;
        SaveProfileData();
        currentSavegameData = LoadSavegameData();
        ApplySavegameData();
        return currentSavegameSlot;
    }

    private int PreviousSavegameSlot()
    {
        var newSlot = currentSavegameSlot - 1;
        if(newSlot < 0) newSlot = MaxSavegameSlots - 1;
        currentSavegameSlot = newSlot;
        SaveProfileData();
        currentSavegameData = LoadSavegameData();
        ApplySavegameData();
        return currentSavegameSlot;
    }
    
    private int SetSavegameSlot(int slot)
    {
        if (slot == currentSavegameSlot) return slot;
        
         currentSavegameSlot = slot < 0 ? 0 : slot >= MaxSavegameSlots ? MaxSavegameSlots - 1 : slot;
         SaveProfileData();
         currentSavegameData = LoadSavegameData();
         ApplySavegameData();
         return currentSavegameSlot;
    }
    
    private ExampleSavegameProfileData LoadProfileData()
    {
        if (saveDirectory == null) return ExampleSavegameProfileData.Default();
        var fileString = saveDirectory.LoadText("SavegameExample-Profile.xml");
        if (string.IsNullOrEmpty(fileString))
        {
            var defaultProfileData = ExampleSavegameProfileData.Default();
            saveDirectory.SaveText("SavegameExample-Profile.xml", profileSerializer.Serialize(defaultProfileData));
            return defaultProfileData;
        }
        var profileData = profileSerializer.Deserialize(fileString);
        return profileData ?? ExampleSavegameProfileData.Default();
    }
    private bool SaveProfileData()
    {
        if (saveDirectory == null) return false;
        currentProfileData.SavegameSlot = currentSavegameSlot;
        var fileString = profileSerializer.Serialize(currentProfileData);
        if (string.IsNullOrEmpty(fileString)) return false;
        saveDirectory.SaveText("SavegameExample-Profile.xml", fileString);
        return true;
    }
    private ExampleSavegameProfileData ResetProfileData()
    {
        currentProfileData = ExampleSavegameProfileData.Default();
        SaveProfileData();
        return currentProfileData;
    }
    
    private ExampleSavegameData LoadSavegameData()
    {
        if (saveDirectory == null) return ExampleSavegameData.Default();
        var fileString = saveDirectory.LoadText(CurrentSavegameFileName);
        if (string.IsNullOrEmpty(fileString))
        {
            var defaultData = ExampleSavegameData.Default();
            saveDirectory.SaveText(CurrentSavegameFileName, dataSerializer.Serialize(defaultData));
            return defaultData;
        }
        var savegameData = dataSerializer.Deserialize(fileString);
        return savegameData ?? ExampleSavegameData.Default();
        
    }
    private bool SaveSavgameData()
    {
        if (saveDirectory == null) return false;
        var fileString = dataSerializer.Serialize(currentSavegameData);
        if (string.IsNullOrEmpty(fileString)) return false;
        saveDirectory.SaveText(CurrentSavegameFileName, fileString);
        return true;
    }
    private void ApplySavegameData()
    {
        rect = new Rect(currentSavegameData.RectX, currentSavegameData.RectY, currentSavegameData.RectWidth, currentSavegameData.RectHeight);
        color = new ColorRgba(currentSavegameData.RectColorR, currentSavegameData.RectColorG, currentSavegameData.RectColorB, currentSavegameData.RectColorA);
        value = currentSavegameData.Value;
        valueFont.ColorRgba = color;
    }

    private void ResetSavegameData()
    {
        currentSavegameData = ExampleSavegameData.Default();
        SaveSavgameData();
        ApplySavegameData();
    }
    #endregion
}