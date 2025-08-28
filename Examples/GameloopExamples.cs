using System.Numerics;
using Raylib_cs;
using ShapeEngine.StaticLib;
using ShapeEngine.Content;
using Examples.Scenes;
using Examples.UIElements;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Screen;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Input;
using ShapeEngine.Random;
using ShapeEngine.Text;
using ShapeEngine.UI;
using Game = ShapeEngine.Core.GameDef.Game;


namespace Examples;

internal class PaletteInfoBox
{
    private string name = string.Empty;
    private float timer;
    private float duration;
    private readonly TextFont textFont;
    private bool IsVisible => timer > 0f && name.Length > 0;
    
    public PaletteInfoBox()
    {
        textFont = new(GameloopExamples.Instance.GetFont(FontIDs.JetBrains), 1f, Colors.Text);
    }

    public void Trigger(string paletteName, float dur)
    {
        name = paletteName;
        duration = dur;
        timer = dur;
    }
    
    public void Update(float dt)
    {
        if (timer > 0f)
        {
            timer -= dt;
            if (timer <= 0f)
            {
                name = string.Empty;
                duration = 0f;
                timer = 0f;
            }
        }
    }

    public void Draw(Rect rect)
    {
        if (!IsVisible) return;

        var lt = rect.Size.Max() * 0.01f;
        
        rect.Draw(Colors.PcDark.ColorRgba);
        rect.DrawLines(lt, Colors.PcLight.ColorRgba);

        var margin = rect.Size.Max() * 0.025f;
        var textRect = rect.ApplyMarginsAbsolute(margin, margin, margin, margin);
        var split = textRect.SplitV(0.3f);
        
    
        textFont.ColorRgba = Colors.PcText.ColorRgba;
        textFont.DrawTextWrapNone("Palette Changed To", split.top, new(0.5f));
        
        textFont.ColorRgba = Colors.PcSpecial.ColorRgba;
        textFont.DrawTextWrapNone(name, split.bottom, new(0.5f));

        var f = timer / duration;
        var bar = split.bottom.ApplyMargins(0.025f, 0.025f, 0.9f, 0.025f);
        bar = bar.ApplyMargins(0f, f, 0f, 0f);
        bar.Draw(Colors.PcCold.ColorRgba);
    }
}

public class GameloopExamples : Game
{
    public Font FontDefault { get; private set; }

    private Dictionary<int, Font> fonts = new();
    private List<string> fontNames = new();
    private MainScene? mainScene = null;

    private readonly Vector2 crtCurvature = new(6, 4);
    private readonly uint crtShaderID = ShapeID.NextID;
    private readonly uint pixelationShaderID = ShapeID.NextID;
    private readonly uint bloomShaderID = ShapeID.NextID;
    private readonly uint overdrawID = ShapeID.NextID;
    private readonly uint darknessID = ShapeID.NextID;
    private readonly uint chromaticAberrationID = ShapeID.NextID;
    private readonly uint blurID = ShapeID.NextID;
    private readonly uint alphaCircleID = ShapeID.NextID;
    private uint currentShaderID;

    public RectNode UIRects;

    public bool DrawCursor = true;

    public readonly uint UIAccessTag = InputSystem.NextAccessTag; // BitFlag.GetFlagUint(2);
    public readonly uint GameloopAccessTag = InputSystem.NextAccessTag; // BitFlag.GetFlagUint(3);
    public readonly uint SceneAccessTag =  InputSystem.NextAccessTag; //BitFlag.GetFlagUint(4);
    public readonly uint GamepadMouseMovementTag = InputSystem.NextAccessTag; // BitFlag.GetFlagUint(5);
    public readonly uint KeyboardMouseMovementTag =  InputSystem.NextAccessTag; //BitFlag.GetFlagUint(6);
   
    public static IModifierKey  ModifierKeyGamepad = new ModifierKeyGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM);
    public static IModifierKey  ModifierKeyGamepadReversed = new ModifierKeyGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, 0.15f, 0.15f, true);
    public static IModifierKey  ModifierKeyGamepad2 = new ModifierKeyGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_BOTTOM);
    public static IModifierKey  ModifierKeyGamepad2Reversed = new ModifierKeyGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_BOTTOM, 0.15f, 0.15f, true);
    public static IModifierKey  ModifierKeyKeyboard = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT);
    public static IModifierKey  ModifierKeyKeyboardReversed = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT, true);
    public static IModifierKey  ModifierKeyMouse = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT);
    public static IModifierKey  ModifierKeyMouseReversed = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT, true);
    
    public InputAction InputActionUICancel   {get; private set;}
    public InputAction InputActionUIBack     {get; private set;}
    public InputAction InputActionUIAccept   {get; private set;}
    public InputAction InputActionUIAcceptMouse { get; private set; }
    public InputAction InputActionUILeft     {get; private set;}
    public InputAction InputActionUIRight    {get; private set;}
    public InputAction InputActionUIUp       {get; private set;}
    public InputAction InputActionUIDown     {get; private set;}
    public InputAction InputActionUINextTab  {get; private set;}
    public InputAction InputActionUIPrevTab  {get; private set;}
    public InputAction InputActionUINextPage {get; private set;}
    public InputAction InputActionUIPrevPage {get; private set;}

    //gameloop controlled
    public InputAction InputActionFullscreen {get; private set;}
    public InputAction InputActionMaximize {get; private set;}
    public InputAction InputActionMinimize {get; private set;}
    public InputAction InputActionNextMonitor {get; private set;}
    public InputAction InputActionCycleShaders {get; private set;}
    public InputAction InputActionCycleScreenMode {get; private set;}

    
    //example scene controlled
    public InputAction InputActionZoom {get; private set;}
    public InputAction InputActionCyclePalette {get; private set;}
    public InputAction InputActionReset {get; private set;}


    private readonly InputActionTree inputActionTree = [];
    
    private FPSLabel fpsLabel;

    private float mouseMovementTimer = 0f;
    private const float mouseMovementDuration = 2f;

    public bool MouseControlEnabled = true;

    private PaletteInfoBox paletteInfoBox;
    
    private List<ScreenTexture> gameTextures = new(5);
    private int curGameTextureIndex = 0;
    
    public new static GameloopExamples Instance  => examplesInstance?? throw new NullReferenceException("Instance is not initialized! You need to create a GameloopExamples instance before accessing this property!");
    private static GameloopExamples? examplesInstance;
    
    
    public GameloopExamples(GameSettings gameSettings, WindowSettings windowSettings, InputSettings inputSettings) : base(gameSettings, windowSettings, inputSettings)
    {
        //Game.Instance is already checked to never be instantiated twice, so this is safe
        examplesInstance = GetInstanceAs<GameloopExamples>();
        
        UIRects = new(new AnchorPoint(0.5f, 0.5f), new Vector2(1f, 1f), new Rect.Margins(0.015f), "main");
        var mainTop = new RectNode(new AnchorPoint(0.5f, 0f), new Vector2(1f, 0.1f), "top");
        var mainCenter = new RectNode(new AnchorPoint(0.5f, 0.5f), new Vector2(1f, 0.8f), "center");
        var mainBottom = new RectNode(new AnchorPoint(0.5f, 1f), new Vector2(1f, 0.1f), "bottom");
        UIRects.AddChild(mainTop);
        UIRects.AddChild(mainCenter);
        UIRects.AddChild(mainBottom);
        
        var mainTopLeft = new RectNode(new AnchorPoint(0f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0.1f, 0f, 0f), "left");
        mainTopLeft.MouseFilter = MouseFilter.Stop;
        var mainTopCenter = new RectNode(new AnchorPoint(0.5f, 0.5f), new Vector2(0.6f, 1f), new Rect.Margins(0f, 0.005f, 0f, 0.005f), "center");
        var mainTopRight = new RectNode(new AnchorPoint(1f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0f, 0f, 0.1f), "right");
        mainTop.AddChild(mainTopLeft);
        mainTop.AddChild(mainTopCenter);
        mainTop.AddChild(mainTopRight);
        
        var mainCenterLeft = new RectNode(new AnchorPoint(0f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0.1f, 0f, 0f), "left");
        var mainCenterCenter = new RectNode(new AnchorPoint(0.5f, 0.5f), new Vector2(0.6f, 1f), new Rect.Margins(0f, 0.005f, 0f, 0.005f), "center");
        var mainCenterRight = new RectNode(new AnchorPoint(1f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0f, 0f, 0.1f), "right");
        mainCenter.AddChild(mainCenterLeft);
        mainCenter.AddChild(mainCenterCenter);
        mainCenter.AddChild(mainCenterRight);
        
        var mainBottomLeft = new RectNode(new AnchorPoint(0f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0.1f, 0f, 0f), "left");
        var mainBottomCenter = new RectNode(new AnchorPoint(0.5f, 0.5f), new Vector2(0.6f, 1f), new Rect.Margins(0f, 0.005f, 0f, 0.005f), "center");
        var mainBottomRight = new RectNode(new AnchorPoint(1f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0f, 0f, 0.1f), "right");
        mainBottom.AddChild(mainBottomLeft);
        mainBottom.AddChild(mainBottomCenter);
        mainBottom.AddChild(mainBottomRight);
        
        var mainTopLeftTop = new RectNode(new AnchorPoint(0.5f, 0f), new Vector2(1f, 0.5f), "top");
        var mainTopLeftBottom = new RectNode(new AnchorPoint(0.5f, 1f), new Vector2(1f, 0.5f), "bottom");
        mainTopLeft.AddChild(mainTopLeftTop);
        mainTopLeft.AddChild(mainTopLeftBottom);
        
        var mainTopRightTop = new RectNode(new AnchorPoint(0.5f, 0f), new Vector2(1f, 0.5f), "top");
        var mainTopRightBottom = new RectNode(new AnchorPoint(0.5f, 1f), new Vector2(1f, 0.5f), "bottom");
        mainTopRight.AddChild(mainTopRightTop);
        mainTopRight.AddChild(mainTopRightBottom);
        
        var mainBottomLeftTop = new RectNode(new AnchorPoint(0.5f, 0f), new Vector2(1f, 0.5f), "top");
        var mainBottomLeftBottom = new RectNode(new AnchorPoint(0.5f, 1f), new Vector2(1f, 0.5f), "bottom");
        mainBottomLeft.AddChild(mainBottomLeftTop);
        mainBottomLeft.AddChild(mainBottomLeftBottom);
        
        var mainBottomRightTop = new RectNode(new AnchorPoint(0.5f, 0f), new Vector2(1f, 0.5f), "top");
        var mainBottomRightBottom = new RectNode(new AnchorPoint(0.5f, 1f), new Vector2(1f, 0.5f), "bottom");
        mainBottomRight.AddChild(mainBottomRightTop);
        mainBottomRight.AddChild(mainBottomRightBottom);
    }
    
    protected override void LoadContent()
    {
        BackgroundColorRgba = Colors.Background; // ExampleScene.ColorDark;
        
        fonts.Add(FontIDs.GruppoRegular, ContentLoader.LoadFont("Resources/Fonts/Gruppo-Regular.ttf", 100));
        fonts.Add(FontIDs.IndieFlowerRegular, ContentLoader.LoadFont("Resources/Fonts/IndieFlower-Regular.ttf", 100));
        fonts.Add(FontIDs.OrbitRegular, ContentLoader.LoadFont("Resources/Fonts/Orbit-Regular.ttf", 100));
        fonts.Add(FontIDs.OrbitronBold, ContentLoader.LoadFont("Resources/Fonts/Orbitron-Bold.ttf", 100));
        fonts.Add(FontIDs.OrbitronRegular, ContentLoader.LoadFont("Resources/Fonts/Orbitron-Regular.ttf", 100));
        fonts.Add(FontIDs.PromptLightItalic, ContentLoader.LoadFont("Resources/Fonts/Prompt-LightItalic.ttf", 100));
        fonts.Add(FontIDs.PromptRegular, ContentLoader.LoadFont("Resources/Fonts/Prompt-Regular.ttf", 100));
        fonts.Add(FontIDs.PromptThin, ContentLoader.LoadFont("Resources/Fonts/Prompt-Thin.ttf", 100));
        fonts.Add(FontIDs.TekoMedium, ContentLoader.LoadFont("Resources/Fonts/Teko-Medium.ttf", 100));
        fonts.Add(FontIDs.JetBrains, ContentLoader.LoadFont("Resources/Fonts/JetBrainsMono.ttf", 100));
        fonts.Add(FontIDs.JetBrainsLarge, ContentLoader.LoadFont("Resources/Fonts/JetBrainsMono.ttf", 500));
        
        fontNames.Add("Gruppo Regular");
        fontNames.Add("Indie Flower Regular");
        fontNames.Add("Orbit Regular");
        fontNames.Add("Orbitron Bold");
        fontNames.Add("Orbitron Regular");
        fontNames.Add("Prompt Light Italic");
        fontNames.Add("Prompt Regular");
        fontNames.Add("Prompt Thin");
        fontNames.Add("Teko Medium");
        fontNames.Add("Jet Brains Mono");

        if (ScreenShaders != null)
        {
            var shapeShaders = new List<ShapeShader>(7);
            
            var crt = ContentLoader.LoadFragmentShader("Resources/Shaders/CRTShader.frag");
            ShapeShader crtShader = new(crt, crtShaderID, true, 1);
            ShapeShader.SetValueFloat(crtShader.Shader, "renderWidth", Window.CurScreenSize.Width);
            ShapeShader.SetValueFloat(crtShader.Shader, "renderHeight", Window.CurScreenSize.Height);
            var bgColor = BackgroundColorRgba;
            ShapeShader.SetValueColor(crtShader.Shader, "cornerColor", bgColor);
            ShapeShader.SetValueFloat(crtShader.Shader, "vignetteOpacity", 0.35f);
            ShapeShader.SetValueVector2(crtShader.Shader, "curvatureAmount", crtCurvature.X, crtCurvature.Y);//smaller values = bigger curvature
            // ScreenShaders.Add(crtShader);
            shapeShaders.Add(crtShader);
            
            var pixel = ContentLoader.LoadFragmentShader("Resources/Shaders/PixelationShader.frag");
            ShapeShader pixelationShader = new(pixel, pixelationShaderID, false, 2);
            ShapeShader.SetValueFloat(pixelationShader.Shader, "renderWidth", Window.CurScreenSize.Width);
            ShapeShader.SetValueFloat(pixelationShader.Shader, "renderHeight", Window.CurScreenSize.Height);
            // ScreenShaders.Add(pixelationShader);
            shapeShaders.Add(pixelationShader);

            var bloom = ContentLoader.LoadFragmentShader("Resources/Shaders/BloomShader.frag");
            ShapeShader bloomShader = new(bloom, bloomShaderID, false, 3);
            ShapeShader.SetValueVector2(bloomShader.Shader, "size", Window.CurScreenSize.ToVector2());
            // ScreenShaders.Add(bloomShader);
            shapeShaders.Add(bloomShader);

            var overdraw = ContentLoader.LoadFragmentShader("Resources/Shaders/OverdrawShader.frag");
            ShapeShader overdrawShader = new(overdraw, overdrawID, false, 4);
            // ScreenShaders.Add(overdrawShader);
            shapeShaders.Add(overdrawShader);
            
            var darkness = ContentLoader.LoadFragmentShader("Resources/Shaders/Darkness.frag");
            ShapeShader darknessShader = new(darkness, darknessID, false, 5);
            // ScreenShaders.Add(darknessShader);
            shapeShaders.Add(darknessShader);
            
            var blur = ContentLoader.LoadFragmentShader("Resources/Shaders/BlurShader.frag");
            ShapeShader blurShader = new(blur, blurID, false, 6);
            ShapeShader.SetValueFloat(blurShader.Shader, "renderWidth", Window.CurScreenSize.Width);
            ShapeShader.SetValueFloat(blurShader.Shader, "renderHeight", Window.CurScreenSize.Height);
            shapeShaders.Add(blurShader);
            
            var alphaCircle = ContentLoader.LoadFragmentShader("Resources/Shaders/AlphaCircle.frag");
            ShapeShader alphaCircleShader = new(alphaCircle, alphaCircleID, false, 7);
            ShapeShader.SetValueVector2(alphaCircleShader.Shader, "origin", new Vector2(0f, 0f));
            ShapeShader.SetValueFloat(alphaCircleShader.Shader, "minDis", 0.25f);
            ShapeShader.SetValueFloat(alphaCircleShader.Shader, "maxDis", 1f);
            shapeShaders.Add(alphaCircleShader);
            
            var chromaticAberration = ContentLoader.LoadFragmentShader("Resources/Shaders/ChromaticAberrationShader.frag");
            ShapeShader chromaticAberrationShader = new(chromaticAberration, chromaticAberrationID, false, 7);
            ScreenShaders.Add(chromaticAberrationShader);
            
            currentShaderID = crtShaderID;
            
            CreateGameTextures(shapeShaders);
            var old = ChangeGameTexture(gameTextures[curGameTextureIndex]);
            old?.Unload();
        }
        else
        {
            CreateGameTextures(null);
            var old = ChangeGameTexture(gameTextures[curGameTextureIndex]);
            old?.Unload();
        }
        
        FontDefault = GetFont(FontIDs.JetBrains);

        this.Window.FpsLimit = 60;
        this.Window.VSync = false;
        // this.Window.MaxFramerate = 480;
        // this.Window.FpsLimit = 240;

        fpsLabel = new(FontDefault, Colors.PcCold, Colors.PcText, Colors.PcHighlight);
        
        paletteInfoBox = new();
    }

    protected override Vector2 ChangeMousePos(float dt, Vector2 mousePos, Rect screenArea)
    {
        if (!MouseControlEnabled) return mousePos;
        
        if (Input.CurrentInputDeviceType == InputDeviceType.Gamepad && Input.GamepadManager.LastUsedGamepad != null && 
            Input.GamepadManager.LastUsedGamepad.IsDown(ShapeGamepadTriggerAxis.RIGHT))
        {
            mouseMovementTimer = 0f;
            float speed = screenArea.Size.Max() * 0.75f * dt;
            int gamepad = Input.GamepadManager.LastUsedGamepad.Index;
            var x = ShapeGamepadJoyAxis.LEFT_X.CreateInputState(GamepadMouseMovementTag, gamepad, 0.2f).AxisRaw;
            var y = ShapeGamepadJoyAxis.LEFT_Y.CreateInputState(GamepadMouseMovementTag, gamepad, 0.2f).AxisRaw;

            var movement = new Vector2(x, y);
            float l = movement.Length();
            if (l <= 0f) return mousePos;
            
            var dir = movement / l;
            return mousePos + dir * l * speed;
        }
        
        if (Input.CurrentInputDeviceType == InputDeviceType.Keyboard)
        {
            mouseMovementTimer += dt;
            if (mouseMovementTimer >= mouseMovementDuration) mouseMovementTimer = mouseMovementDuration;
            float t = mouseMovementTimer / mouseMovementDuration; 
            float f = ShapeMath.LerpFloat(0.2f, 1f, t);
            float speed = screenArea.Size.Max() * 0.5f * dt * f;
            var x = ShapeKeyboardButton.LEFT.CreateInputState(ShapeKeyboardButton.RIGHT, KeyboardMouseMovementTag).AxisRaw;
            var y = ShapeKeyboardButton.UP.CreateInputState(ShapeKeyboardButton.DOWN, KeyboardMouseMovementTag).AxisRaw;

            var movement = new Vector2(x, y);
            if (movement.LengthSquared() <= 0f) mouseMovementTimer = 0f;
            return mousePos + movement.Normalize() * speed;
        }

        mouseMovementTimer = 0f;
        return mousePos;
    }

    protected override void EndRun()
    {
        mainScene?.Close();
    }

    protected override void UnloadContent()
    {
        
        ContentLoader.UnloadFonts(fonts.Values);
    }
    protected override void BeginRun()
    {
        SetupInput();

        inputActionTree.CurrentGamepad = null;
        
        mainScene = new MainScene();
        GoToScene(mainScene);
    }

    protected override void OnGameTextureResized(int w, int h)
    {
        if (ScreenShaders != null)
        {
            var crtShader = ScreenShaders.Get(crtShaderID);
            if (crtShader != null)
            {
                ShapeShader.SetValueFloat(crtShader.Shader, "renderWidth", w);
                ShapeShader.SetValueFloat(crtShader.Shader, "renderHeight", h);
            }

            var pixelationShader = ScreenShaders.Get(pixelationShaderID);
            if (pixelationShader != null)
            {
                ShapeShader.SetValueFloat(pixelationShader.Shader, "renderWidth", w);
                ShapeShader.SetValueFloat(pixelationShader.Shader, "renderHeight", h);
            }
       
            var blurShader = ScreenShaders.Get(blurID);
            if (blurShader != null)
            {
                ShapeShader.SetValueFloat(blurShader.Shader, "renderWidth", w);
                ShapeShader.SetValueFloat(blurShader.Shader, "renderHeight", h);
            }
        
            var bloomShader = ScreenShaders.Get(bloomShaderID);
            if (bloomShader != null)
            {
                ShapeShader.SetValueVector2(bloomShader.Shader, "size", new Vector2(w, h));
            }
        }
    }

    

    protected override void UpdateCursor(float dt, ScreenInfo gameInfo, ScreenInfo gameUiInfo, ScreenInfo uiInfo)
    {
        
    }

    protected override void DrawCursorUi(ScreenInfo uiInfo)
    {
        if (!DrawCursor) return;
        
        float size = uiInfo.Area.Size.Min() * 0.02f;
        // float t = 1f - (effectTimer / EffectDuration);
        var c = Colors.Warm; // Colors.Special.Lerp(Colors.Warm, t);
        DrawRoundedCursor(uiInfo.MousePos, size, c);
    }
    
    private void DrawRoundedCursor(Vector2 tip, float size, ColorRgba colorRgba)
    {
        var dir = new Vector2(1, 1).Normalize();
        var circleCenter = tip + dir * size * 2;
        var left = circleCenter + new Vector2(-1, 0) * size;
        var top = circleCenter + new Vector2(0, -1) * size;
        SegmentDrawing.DrawSegment(tip, left, 1f, colorRgba, LineCapType.CappedExtended, 3);
        SegmentDrawing.DrawSegment(tip, top, 1f, colorRgba, LineCapType.CappedExtended, 3);
        CircleDrawing.DrawCircleSectorLines(circleCenter, size, 180, 270, 1f, colorRgba, false, 4f);
    }

    protected override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        gameTextures[2].BackgroundColor = Colors.Background.ChangeBrightness(-0.25f).SetAlpha(200);
        gameTextures[4].BackgroundColor = Colors.Background.ChangeBrightness(-0.25f).SetAlpha(200);
        
        if (ScreenShaders != null)
        {
            var pixelationShader = ScreenShaders.Get(pixelationShaderID);
            if (pixelationShader != null && pixelationShader.Enabled)
            {

                var rPixelValue = Rng.Instance.RandF(5.9f, 6.1f);
            
                ShapeShader.SetValueFloat(pixelationShader.Shader, "pixelWidth", rPixelValue * Camera.BaseZoomLevel);
                ShapeShader.SetValueFloat(pixelationShader.Shader, "pixelHeight", rPixelValue * Camera.BaseZoomLevel);
            }
        
            var darknessShader = ScreenShaders.Get(darknessID);
            if (darknessShader != null && darknessShader.Enabled)
            {
                ShapeShader.SetValueVector2(darknessShader.Shader, "origin", game.RelativeMousePosition);
            }
            
            var overdrawShader = ScreenShaders.Get(overdrawID);
            if (overdrawShader != null && overdrawShader.Enabled)
            {
                if (Rng.Instance.Chance(0.025f))
                {
                    ShapeShader.SetValueColor(overdrawShader.Shader, "overdrawColor", Rng.Instance.RandColor(127, 255, 255));
                    ShapeShader.SetValueFloat(overdrawShader.Shader, "blend", Rng.Instance.RandF(0.4f, 0.6f));
                }
            
            }
        }
        
        UIRects.UpdateRect(ui.Area);
        UIRects.Update(time.Delta, ui.MousePos);

        inputActionTree.CurrentGamepad = Input.GamepadManager.LastUsedGamepad;
        inputActionTree.Update(time.Delta);
        
        var fullscreenState = InputActionFullscreen.Consume(out _);
        if (fullscreenState is { Consumed: false, Pressed: true })
        {
            Instance.Window.ToggleBorderlessFullscreen();
        }

        var maximizeState = InputActionMaximize.Consume(out _);
        if (maximizeState is { Consumed: false, Pressed: true })
        {
            Instance.Window.ToggleMaximizeWindow();
        }

        var nextMonitorState = InputActionNextMonitor.Consume(out _);
        if (nextMonitorState is { Consumed: false, Pressed: true })
        {
           Window.NextMonitor();
        }

        var screenModeState = Instance.InputActionCycleScreenMode.Consume(out _);
        if (screenModeState is { Consumed: false, Pressed: true })
        {
            NextGameTexture();
        }

        if (InputSystem.IsInputAvailable(GameloopAccessTag))
        {
            int keypadNumber = -1;
            if (ShapeKeyboardButton.KP_1.GetInputState().Pressed) keypadNumber = 1;
            else if (ShapeKeyboardButton.KP_2.GetInputState().Pressed) keypadNumber = 2;
            else if (ShapeKeyboardButton.KP_3.GetInputState().Pressed) keypadNumber = 3;
            else if (ShapeKeyboardButton.KP_4.GetInputState().Pressed) keypadNumber = 4;
            else if (ShapeKeyboardButton.KP_5.GetInputState().Pressed) keypadNumber = 5;
            else if (ShapeKeyboardButton.KP_6.GetInputState().Pressed) keypadNumber = 6;
            else if (ShapeKeyboardButton.KP_7.GetInputState().Pressed) keypadNumber = 7;
            else if (ShapeKeyboardButton.KP_8.GetInputState().Pressed) keypadNumber = 8;
            else if (ShapeKeyboardButton.KP_9.GetInputState().Pressed) keypadNumber = 9;
            
            if (keypadNumber > 0)
            {
                var anchorTexture = gameTextures[4];
                var newAnchor = AnchorPoint.GetKeypadAnchorPosition(keypadNumber);
                anchorTexture.ChangeAnchorPosition(newAnchor.ToVector2());
            }
        }
        
        if (Paused) return;

        
        
        var paletteState = Instance.InputActionCyclePalette.Consume(out _);
        if (paletteState is { Consumed: false, Pressed: true })
        {
            Colors.NextColorscheme();
            BackgroundColorRgba = Colors.PcBackground.ColorRgba;
            if (ScreenShaders != null)
            {
                var screenShader = ScreenShaders.Get(crtShaderID);
                if(screenShader != null) ShapeShader.SetValueColor(screenShader.Shader, "cornerColor", BackgroundColorRgba);
            }
            
            
            paletteInfoBox.Trigger(Colors.CurColorschemeName, 2f);
        }
        if (ScreenShaders != null)
        {
            var cycleShaders = InputActionCycleShaders.Consume(out _);
            if (cycleShaders is { Consumed: false, Pressed: true })
            {
                var currentShader = ScreenShaders.Get(currentShaderID);
                if (currentShader != null) currentShader.Enabled = false;
            
                var shadersIds = ScreenShaders.GetAllIDs();
                var nextShaderIDIndex = shadersIds.IndexOf(currentShaderID);
                nextShaderIDIndex += 1;
                if (nextShaderIDIndex >= shadersIds.Count) {nextShaderIDIndex = 0;}

                var nextId = shadersIds[nextShaderIDIndex];
                var nextShader = ScreenShaders.Get(nextId);
                if (nextShader != null)
                {
                    currentShaderID = nextId;
                    nextShader.Enabled = true;
                }
            
            }
        }
        paletteInfoBox.Update(time.Delta);
    }

    

    protected override void DrawUI(ScreenInfo ui)
    {
        if(mainScene == null || ! mainScene.Active) DrawFpsBox();
        
        paletteInfoBox.Draw(ui.Area.ApplyMargins(0.8f,0.025f,0.25f,0.65f));
    }

    public void DrawFpsBox()
    {
        var fpsRect = UIRects.GetRect("top right top");
        fpsLabel.Draw(fpsRect, new(1f, 0f));
    }

    public int GetFontCount() { return fonts.Count; }
    public Font GetFont(int id) { return fonts[id]; }
    public string GetFontName(int id) { return fontNames[id]; }
    public Font GetRandomFont()
    {
        Font? randFont = Rng.Instance.RandCollection<Font>(fonts.Values.ToList(), false);
        return randFont != null ? (Font)randFont : FontDefault;
    }
    public void GoToMainScene()
    {
        if (mainScene == null) return;
        if (CurScene == mainScene) return;
        GoToScene(mainScene);
    }

    
    private void NextGameTexture()
    {
        curGameTextureIndex++;
        if(curGameTextureIndex >= gameTextures.Count) curGameTextureIndex = 0;
        var next = gameTextures[curGameTextureIndex];
        next.Camera = Camera;
        ChangeGameTexture(next);
    }
    private void CreateGameTextures(List<ShapeShader>? shaders)
    {
        var shaderMode = shaders != null ? ShaderSupportType.Multi : ShaderSupportType.None;
        
        var stretchTexture = new ScreenTexture(shaderMode, TextureFilter.Bilinear);
        var pixelationTexture = new ScreenTexture(0.25f, shaderMode, TextureFilter.Point);
        
        //low res
        var fixedTexture = new ScreenTexture(new Dimensions(500, 500), shaderMode, TextureFilter.Point, false);
        //high res
        var nearestFixedTexture = new ScreenTexture(new Dimensions(1920*2, 1080*2), shaderMode, TextureFilter.Bilinear, true);
        
        var anchorTexture = new ScreenTexture(new Vector2(0.8f, 0.4f), new Vector2(0.05f, 0.5f), shaderMode, TextureFilter.Bilinear);
        fixedTexture.BackgroundColor = Colors.Background.ChangeBrightness(-0.5f); 
        anchorTexture.BackgroundColor = Colors.Background.ChangeBrightness(-0.5f);
        
        
        if (shaders != null)
        {
            foreach (var shader in shaders)
            {
                stretchTexture.Shaders?.Add(shader);
                pixelationTexture.Shaders?.Add(shader);
                fixedTexture.Shaders?.Add(shader);
                nearestFixedTexture.Shaders?.Add(shader);
                anchorTexture.Shaders?.Add(shader);
            }
        }
        
        gameTextures.Add(stretchTexture);
        gameTextures.Add(pixelationTexture);
        gameTextures.Add(fixedTexture);
        gameTextures.Add(nearestFixedTexture);
        gameTextures.Add(anchorTexture);
    }
    
    private void SetupInput()
    {
        InputActionSettings defaultSettings = new();
        
        var cancelKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ESCAPE);
        var cancelGB = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_LEFT);
        InputActionUICancel= new(InputSystem.AllAccessTag, defaultSettings, cancelKB, cancelGB);
        
      
        var fullscreenKB = new InputTypeKeyboardButton(ShapeKeyboardButton.F);
        var fullscreenGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
        InputActionFullscreen = new(GameloopAccessTag, defaultSettings,fullscreenKB, fullscreenGB);
        
        var maximizeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.M);
        // var maximizeGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_THUMB);
        InputActionMaximize = new(GameloopAccessTag, defaultSettings,maximizeKB);
        
        var minimizeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.N);
        InputActionMinimize = new(GameloopAccessTag, defaultSettings,minimizeKB);
        
        var nextMonitorKB = new InputTypeKeyboardButton(ShapeKeyboardButton.B);
        InputActionNextMonitor = new(GameloopAccessTag, defaultSettings,nextMonitorKB);

        var cycleShaderKB = new InputTypeKeyboardButton(ShapeKeyboardButton.J);
        InputActionCycleShaders = new InputAction(GameloopAccessTag, defaultSettings,cycleShaderKB);
        
        var cycleScreenMode = new InputTypeKeyboardButton(ShapeKeyboardButton.H);
        InputActionCycleScreenMode = new InputAction(GameloopAccessTag, defaultSettings,cycleScreenMode);
        
        var paletteKb = new InputTypeKeyboardButton(ShapeKeyboardButton.P);
        var paletteGp = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_RIGHT);
        var paletteMb = new InputTypeMouseButton(ShapeMouseButton.SIDE);
        InputActionCyclePalette = new(SceneAccessTag, defaultSettings,paletteKb, paletteGp, paletteMb);
        
        //main scene
        var backKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
        var backGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
        var backMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
        InputActionUIBack = new(UIAccessTag, defaultSettings,backKB, backGB, backMB);

        var acceptKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
        var acceptKB2 = new InputTypeKeyboardButton(ShapeKeyboardButton.ENTER);
        var acceptGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
        InputActionUIAccept = new(UIAccessTag, defaultSettings,acceptKB, acceptKB2, acceptGB);
        
        var acceptMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
        InputActionUIAcceptMouse = new(UIAccessTag, defaultSettings,acceptMB);

        var modifierKeySetGamepadReversed = new ModifierKeySet(ModifierKeyOperator.Or, ModifierKeyGamepadReversed);
        var modifierKeySetGamepad = new ModifierKeySet(ModifierKeyOperator.Or, ModifierKeyGamepad);
        var leftKB = new InputTypeKeyboardButton(ShapeKeyboardButton.A);
        var leftGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT, 0.1f, modifierKeySetGamepadReversed);
        InputActionUILeft = new(UIAccessTag, defaultSettings,leftKB, leftGB);
        
        var rightKB = new InputTypeKeyboardButton(ShapeKeyboardButton.D);
        var rightGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT, 0.1f, modifierKeySetGamepadReversed);
        InputActionUIRight = new(UIAccessTag, defaultSettings,rightKB, rightGB);
        
        var upKB = new InputTypeKeyboardButton(ShapeKeyboardButton.W);
        var upGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP, 0.1f, modifierKeySetGamepadReversed);
        InputActionUIUp = new(UIAccessTag, defaultSettings,upKB, upGB);
        
        var downKB = new InputTypeKeyboardButton(ShapeKeyboardButton.S);
        var downGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0.1f, modifierKeySetGamepadReversed);
        InputActionUIDown = new(UIAccessTag, defaultSettings,downKB, downGB);
        
        var nextTabKB = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
        var nextTabGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
        var nextTabMW = new InputTypeMouseButton(ShapeMouseButton.MW_DOWN, 2f);
        InputActionUINextTab = new(UIAccessTag, defaultSettings,nextTabKB, nextTabGB, nextTabMW);
        
        var prevTabKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
        var prevTabGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
        var prevTabMW = new InputTypeMouseButton(ShapeMouseButton.MW_UP, 2f);
        InputActionUIPrevTab = new(UIAccessTag, defaultSettings,prevTabKB, prevTabGB, prevTabMW);
        
        var nextPageKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
        var nextPageGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP, 0.1f, modifierKeySetGamepad);
        InputActionUINextPage = new(UIAccessTag, defaultSettings,nextPageKB, nextPageGB);
        
        var prevPageKB = new InputTypeKeyboardButton(ShapeKeyboardButton.X);
        var prevPageGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, 0.1f, modifierKeySetGamepad);
        InputActionUIPrevPage = new(UIAccessTag, defaultSettings,prevPageKB, prevPageGB);
        
        //example scene only
        var zoomKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.NINE, ShapeKeyboardButton.ZERO);
        var zoomGP = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_DOWN, ShapeGamepadButton.LEFT_FACE_UP, 0.2f, modifierKeySetGamepad);
        var zoomMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f);
        InputActionZoom = new(SceneAccessTag, defaultSettings,zoomKB, zoomGP, zoomMW);
        
        var resetKB = new InputTypeKeyboardButton(ShapeKeyboardButton.R);
        var resetGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_THUMB);
        InputActionReset = new(SceneAccessTag, defaultSettings,resetKB, resetGB);
        
        inputActionTree.Add(InputActionUICancel);
        inputActionTree.Add(InputActionUIBack);
        inputActionTree.Add(InputActionUIAccept);
        inputActionTree.Add(InputActionUIAcceptMouse);
        inputActionTree.Add(InputActionUILeft);
        inputActionTree.Add(InputActionUIRight);
        inputActionTree.Add(InputActionUIUp);
        inputActionTree.Add(InputActionUIDown);
        inputActionTree.Add(InputActionUIPrevTab);
        inputActionTree.Add(InputActionUINextTab);
        inputActionTree.Add(InputActionUIPrevPage);
        inputActionTree.Add(InputActionUINextPage);
        inputActionTree.Add(InputActionFullscreen);
        inputActionTree.Add(InputActionMaximize);
        inputActionTree.Add(InputActionMinimize);
        inputActionTree.Add(InputActionNextMonitor);
        inputActionTree.Add(InputActionCycleShaders);
        inputActionTree.Add(InputActionCycleScreenMode);
        inputActionTree.Add(InputActionZoom);
        inputActionTree.Add(InputActionCyclePalette);
        inputActionTree.Add(InputActionReset);
    }
}
