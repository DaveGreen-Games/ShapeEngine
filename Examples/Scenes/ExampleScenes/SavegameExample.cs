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
using Size = System.Drawing.Size;

namespace Examples.Scenes.ExampleScenes;

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
    public static readonly float RelativeMinRectWidth = 0.1f;
    public static readonly float RelativeMinRectHeight = 0.1f;
    public static readonly float RelativeMaxRectWidth = 0.5f;
    public static readonly float RelativeMaxRectHeight = 0.5f;
    
    public static ExampleSavegameData Random(int slot, Rect area)
    {
        var rand = Rng.Instance;

        const int minValue = 1;
        const int maxValue = 999;

        var randRelativeRectWidth = rand.RandF(RelativeMinRectWidth, RelativeMaxRectWidth);
        var randRelativeRectHeight = rand.RandF(RelativeMinRectHeight, RelativeMaxRectHeight);
        var randRelativeRectX = rand.RandF(0f, 1f - randRelativeRectWidth);
        var randRelativeRectY = rand.RandF(0f, 1f - randRelativeRectHeight);
        
        var randColor = rand.RandColor(100, 255, 255);
        var data = new ExampleSavegameData()
        {
            Slot = slot,
            Value = rand.RandI(minValue, maxValue),
            RectX = randRelativeRectX,
            RectY = randRelativeRectY,
            RectWidth = randRelativeRectWidth,
            RectHeight = randRelativeRectHeight,
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
    public Rect Rect => new(RectX, RectY, RectWidth, RectHeight);

    public void SetRect(Rect rect)
    {
        RectX = rect.X;
        RectY = rect.Y;
        RectWidth = rect.Width;
        RectHeight = rect.Height;
    }

    public void SetRectColor(ColorRgba color)
    {
        RectColorA = color.A;
        RectColorR = color.R;
        RectColorG = color.G;
        RectColorB = color.B;
    }
}
public class SavegameExample : ExampleScene
{
    private const int MaxSavegameSlots = 3;
    private int currentSavegameSlot;
    private string CurrentSavegameFileName => $"SavegameExample-Slot{currentSavegameSlot}.xml";

    private bool dataInitialized = false;
    private ExampleSavegameProfileData currentProfileData = new();
    private ExampleSavegameData currentSavegameData = new();
    
    private readonly DirectoryInfo? saveDirectory;
    private readonly XmlDataObjectSerializer<ExampleSavegameData> dataSerializer;
    private readonly XmlDataObjectSerializer<ExampleSavegameProfileData> profileSerializer;

    private TextFont valueFont;
    private TextFont buttonFont;

    private float rectLineThickness = 2f;
    private float rectCornerSize = 5f;
    private float rectCornerSelectionRadius = 20f;

    private bool prevDragging;
    private bool draggingTopLeft;
    private bool draggingBottomRight;
    private bool nearTopLeft;
    private bool nearBottomRight;

    private Rect curScreenArea;
    private Rect buttonArea;
    
    public SavegameExample()
    {
        Title = "Savegame Example";
        var saveDirectoryPath = Game.Instance.SaveDirectoryPath;
        var saveFolder = "SaveGameExampleScene";
        var fullPath = Path.Combine(saveDirectoryPath, saveFolder);
        
        saveDirectory = ShapeFileManager.CreateDirectory(fullPath);
        
        valueFont = textFont.Clone();
        buttonFont = textFont.Clone();
        buttonFont.ColorRgba = new ColorRgba(Color.AntiqueWhite);
        dataSerializer = new XmlDataObjectSerializer<ExampleSavegameData>();
        profileSerializer = new XmlDataObjectSerializer<ExampleSavegameProfileData>();

        curScreenArea = new();// new Rect(320, 180, 640, 360);
        buttonArea = new(); //new Rect(320, 180 + 360, 640, 180);
    }

    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        var mousePosUI = ui.MousePos;
        curScreenArea = ui.Area.ApplyMargins(0.05f, 0.05f, 0.15f, 0.3f);
        buttonArea = ui.Area.ApplyMargins(0.2f, 0.05f, 0.72f, 0.025f);
        
        prevDragging = draggingTopLeft || draggingBottomRight;
        
        if (!dataInitialized)
        {
            dataInitialized = true;
            currentProfileData = LoadProfileData();
            currentSavegameSlot = currentProfileData.SavegameSlot;
            currentSavegameData = LoadSavegameData();
            // ApplySavegameData();
        }
        
        rectLineThickness = ui.Area.Size.Min() * 0.003f;
        rectCornerSize = rectLineThickness * 2f;
        rectCornerSelectionRadius = rectCornerSize * 2f;
        
        //transform relative rect to screen coordinates
        var uiAreaX = curScreenArea.X;
        var uiAreaY = curScreenArea.Y;
        var uiAreaWidth = curScreenArea.Size.Width;
        var uiAreaHeight = curScreenArea.Size.Height;
        var relativeRect = currentSavegameData.Rect;
        var rect = new Rect(uiAreaX + relativeRect.X * uiAreaWidth, uiAreaY + relativeRect.Y * uiAreaHeight, relativeRect.Width * uiAreaWidth, relativeRect.Height * uiAreaHeight);
        
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

        var rectMinSize = new ShapeEngine.Core.Structs.Size
        (
            uiAreaWidth * ExampleSavegameData.RelativeMinRectWidth,
            uiAreaHeight * ExampleSavegameData.RelativeMinRectHeight
        );
        if (draggingTopLeft)
        {
            var mousePosClamped = new Vector2
            (
                ShapeMath.Clamp(mousePosUI.X, curScreenArea.Left, curScreenArea.Right - rectMinSize.Width),
                ShapeMath.Clamp(mousePosUI.Y, curScreenArea.Top, curScreenArea.Bottom - rectMinSize.Height)
                
            );


            if (mousePosClamped.X > rect.Right - rectMinSize.Width)
            {
                mousePosClamped.X = rect.Right - rectMinSize.Width;
            }

            if (mousePosClamped.Y > rect.Bottom - rectMinSize.Height)
            {
                mousePosClamped.Y = rect.Bottom - rectMinSize.Height;
            }
            
            rect = new Rect(mousePosClamped, rect.BottomRight);
            
        }
        else if (draggingBottomRight)
        {
            var mousePosClamped = new Vector2
            (
                ShapeMath.Clamp(mousePosUI.X, curScreenArea.Left + rectMinSize.Width, curScreenArea.Right),
                ShapeMath.Clamp(mousePosUI.Y, curScreenArea.Top + rectMinSize.Height, curScreenArea.Bottom)
                
            );
            if (mousePosClamped.X < rect.Left + rectMinSize.Width)
            {
                mousePosClamped.X = rect.Left + rectMinSize.Width;
            }
            if (mousePosClamped.Y < rect.Top + rectMinSize.Height)
            {
                mousePosClamped.Y = rect.Top + rectMinSize.Height;
            }
            rect = new Rect(rect.TopLeft, mousePosClamped);
        }
        else
        {
            rect = rect.Clamp(curScreenArea);
        }
        
        
        //transform back to relative coordinates
        var newRelativeRect = new Rect
        (
            (rect.X - uiAreaX) / (uiAreaWidth),
            (rect.Y - uiAreaY) / (uiAreaHeight),
            rect.Width / uiAreaWidth,
            rect.Height / uiAreaHeight
        );
        
        currentSavegameData.SetRect(newRelativeRect);
    }

    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        var uiAreaX = curScreenArea.X;
        var uiAreaY = curScreenArea.Y;
        var uiAreaWidth = curScreenArea.Size.Width;
        var uiAreaHeight = curScreenArea.Size.Height;
        var relativeRect = currentSavegameData.Rect;
        var rect = new Rect(uiAreaX + relativeRect.X * uiAreaWidth, uiAreaY + relativeRect.Y * uiAreaHeight, relativeRect.Width * uiAreaWidth, relativeRect.Height * uiAreaHeight);
        var color = currentSavegameData.RectColor;
        int value = currentSavegameData.Value;
        valueFont.ColorRgba = color;
        
        curScreenArea.DrawStriped(
            rectCornerSelectionRadius * 4,
            45f,
            new LineDrawingInfo(rectLineThickness / 2,
                new ColorRgba(Color.DarkGray).SetAlpha(50)),
            0f);

        rect.Draw(color.SetAlpha(100));
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
        
        var lmbState = ShapeMouseButton.LEFT.GetInputState();
        var valueText = $"{value}";
        valueFont.DrawWord(valueText, rect, AnchorPoint.Center);

        float buttonMargin = 0.04f;
        var buttonAreas = buttonArea.SplitH(0.3f, 0.05f, 0.3f, 0.05f);

        var slotButtonsArea = buttonAreas[0];
        var slotButtonsAreas = slotButtonsArea.SplitV(MaxSavegameSlots);
        
        // Draw buttons for Save, Load, Reset, and Slot selection
        for (int i = 0; i < MaxSavegameSlots; i++)
        {
            var slotButtonArea = slotButtonsAreas[i].ApplyMargins(buttonMargin);
            if (i == currentSavegameSlot)
            {
                slotButtonArea.DrawCornersRelative(new LineDrawingInfo(rectLineThickness / 2, ColorRgba.White), 0.15f);
            }

            ColorRgba slotColor;
            
            if (slotButtonArea.ContainsPoint(ui.MousePos) && !prevDragging)
            {
                if (lmbState.Down)
                {
                    slotColor = new ColorRgba(Color.GreenYellow);
                }
                else if (lmbState.Released)
                {
                    slotColor = new ColorRgba(Color.GreenYellow);
                    SetSavegameSlot(i);
                }
                else
                {
                    slotColor = new ColorRgba(Color.DarkGreen);
                }
                
            }
            else
            {
                slotColor = new ColorRgba(Color.Gray);
            }
            
            slotButtonArea.Draw(slotColor.SetAlpha(100));
            buttonFont.DrawWord($"Slot {i}", slotButtonArea, AnchorPoint.Center);
        }

        var saveGameButtonsArea = buttonAreas[2];

        const int buttons = 3;
        var saveGameButtonsAreas = saveGameButtonsArea.SplitV(buttons);
        var saveButtonArea = saveGameButtonsAreas[0].ApplyMargins(buttonMargin);
        var loadButtonArea = saveGameButtonsAreas[1].ApplyMargins(buttonMargin);
        var resetButtonArea = saveGameButtonsAreas[2].ApplyMargins(buttonMargin);

        ColorRgba buttonColor;
        if (saveButtonArea.ContainsPoint(ui.MousePos) && !prevDragging)
        {
            if (lmbState.Down)
            {
                buttonColor = new ColorRgba(Color.LightSkyBlue);
            }
            else if (lmbState.Released)
            {
                buttonColor = new ColorRgba(Color.LightSkyBlue);
                // UpdateSavegameData();
                SaveSavegameData();
            }
            else
            {
                buttonColor = new ColorRgba(Color.SteelBlue);
            }
                
        }
        else
        {
            buttonColor = new ColorRgba(Color.DarkSlateBlue);
        }
        saveButtonArea.Draw(buttonColor.SetAlpha(100));
        buttonFont.DrawWord("Save", saveButtonArea, AnchorPoint.Center);

        
        if (loadButtonArea.ContainsPoint(ui.MousePos) && !prevDragging)
        {
            if (lmbState.Down)
            {
                buttonColor = new ColorRgba(Color.OrangeRed);
            }
            else if (lmbState.Released)
            {
                buttonColor = new ColorRgba(Color.OrangeRed);
                currentSavegameData = LoadSavegameData();
            }
            else
            {
                buttonColor = new ColorRgba(Color.Coral);
            }
                
        }
        else
        {
            buttonColor = new ColorRgba(Color.DarkSalmon);
        }
        loadButtonArea.Draw(buttonColor.SetAlpha(100));
        buttonFont.DrawWord("Load", loadButtonArea, AnchorPoint.Center);

        if (resetButtonArea.ContainsPoint(ui.MousePos) && !prevDragging)
        {
            if (lmbState.Down)
            {
                buttonColor = new ColorRgba(Color.Red);
            }
            else if (lmbState.Released)
            {
                buttonColor = new ColorRgba(Color.Red);
                ResetSavegameData();
            }
            else
            {
                buttonColor = new ColorRgba(Color.Crimson);
            }
                
        }
        else
        {
            buttonColor = new ColorRgba(Color.DarkRed);
        }
        resetButtonArea.Draw(buttonColor.SetAlpha(100));
        buttonFont.DrawWord("Reset", resetButtonArea, AnchorPoint.Center);
        
        
        var randomizeButtons = buttonAreas[4];

        var randomizeButtonAreas = randomizeButtons.SplitV(0.5f);
        var randomValueButtonArea = randomizeButtonAreas.top.ApplyMargins(buttonMargin);
        var randomColorButtonArea = randomizeButtonAreas.bottom.ApplyMargins(buttonMargin);
        
        // Randomize Value Button
        if (randomValueButtonArea.ContainsPoint(ui.MousePos) && !prevDragging)
        {
            if (lmbState.Down)
            {
                buttonColor = new ColorRgba(Color.Gold);
            }
            else if (lmbState.Released)
            {
                buttonColor = new ColorRgba(Color.Gold);
                value = Rng.Instance.RandI(1, 999);
                currentSavegameData.Value = value;
            }
            else
            {
                buttonColor = new ColorRgba(Color.Yellow);
            }
        }
        else
        {
            buttonColor = new ColorRgba(Color.DarkGoldenrod);
        }
        randomValueButtonArea.Draw(buttonColor.SetAlpha(100));
        buttonFont.DrawWord("Random Value", randomValueButtonArea, AnchorPoint.Center);

        // Randomize Color Button
        if (randomColorButtonArea.ContainsPoint(ui.MousePos) && !prevDragging)
        {
            if (lmbState.Down)
            {
                buttonColor = new ColorRgba(Color.MediumPurple);
            }
            else if (lmbState.Released)
            {
                buttonColor = new ColorRgba(Color.MediumPurple);
                color = Rng.Instance.RandColor(100, 255, 255);
                valueFont.ColorRgba = color;
                currentSavegameData.SetRectColor(color);
            }
            else
            {
                buttonColor = new ColorRgba(Color.Plum);
            }
        }
        else
        {
            buttonColor = new ColorRgba(Color.Indigo);
        }
        randomColorButtonArea.Draw(buttonColor.SetAlpha(100));
        buttonFont.DrawWord("Random Color", randomColorButtonArea, AnchorPoint.Center);

    }


    #region Load and Save
    
    private int NextSavegameSlot()
    {
        var newSlot = currentSavegameSlot + 1;
        if(newSlot >= MaxSavegameSlots) newSlot = 0;
        currentSavegameSlot = newSlot;
        SaveProfileData();
        currentSavegameData = LoadSavegameData();
        return currentSavegameSlot;
    }

    private int PreviousSavegameSlot()
    {
        var newSlot = currentSavegameSlot - 1;
        if(newSlot < 0) newSlot = MaxSavegameSlots - 1;
        currentSavegameSlot = newSlot;
        SaveProfileData();
        currentSavegameData = LoadSavegameData();
        return currentSavegameSlot;
    }
    
    private int SetSavegameSlot(int slot)
    {
        if (slot == currentSavegameSlot) return slot;
        
        currentSavegameSlot = ShapeMath.Clamp(slot, 0, MaxSavegameSlots - 1);
        SaveProfileData();
        currentSavegameData = LoadSavegameData();
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
        if (saveDirectory == null) return ExampleSavegameData.Random(currentSavegameSlot, curScreenArea);
        var fileString = saveDirectory.LoadText(CurrentSavegameFileName);
        if (string.IsNullOrEmpty(fileString))
        {
            var defaultData = ExampleSavegameData.Random(currentSavegameSlot, curScreenArea);
            saveDirectory.SaveText(CurrentSavegameFileName, dataSerializer.Serialize(defaultData));
            return defaultData;
        }
        var savegameData = dataSerializer.Deserialize(fileString);
        return savegameData ?? ExampleSavegameData.Random(currentSavegameSlot, curScreenArea);
    }
    private bool SaveSavegameData()
    {
        if (saveDirectory == null) return false;
        var fileString = dataSerializer.Serialize(currentSavegameData);
        if (string.IsNullOrEmpty(fileString)) return false;
        saveDirectory.SaveText(CurrentSavegameFileName, fileString);
        return true;
    }
    private void ResetSavegameData()
    {
        currentSavegameData = ExampleSavegameData.Random(currentSavegameSlot, curScreenArea);
        SaveSavegameData();
    }
    #endregion
}