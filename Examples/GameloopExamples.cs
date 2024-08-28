

using System.Numerics;
using Raylib_cs;
using ShapeEngine.Lib;
using ShapeEngine.Persistent;
using ShapeEngine.Core;
using Examples.Scenes;
using Examples.UIElements;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Screen;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Text;
using ShapeEngine.UI;


namespace Examples
{
    internal class PaletteInfoBox
    {
        private string name = string.Empty;
        private float timer = 0f;
        private float duration = 0f;
        private readonly TextFont textFont;
        private bool IsVisible => timer > 0f && name.Length > 0;
        
        public PaletteInfoBox()
        {
            textFont = new(GAMELOOP.GetFont(FontIDs.JetBrains), 1f, Colors.Text);
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
        private uint currentShaderID;
        
        public ShapeGamepadDevice? CurGamepad = null;

        // public RectContainerMain UIRects = new("main");

        public RectNode UIRects;

        public bool DrawCursor = true;

        public readonly uint UIAccessTag = InputAction.NextTag; // BitFlag.GetFlagUint(2);
        public readonly uint GameloopAccessTag = InputAction.NextTag; // BitFlag.GetFlagUint(3);
        public readonly uint SceneAccessTag =  InputAction.NextTag; //BitFlag.GetFlagUint(4);
        public readonly uint GamepadMouseMovementTag = InputAction.NextTag; // BitFlag.GetFlagUint(5);
        public readonly uint KeyboardMouseMovementTag =  InputAction.NextTag; //BitFlag.GetFlagUint(6);
        //ui
       
        public static IModifierKey  ModifierKeyGamepad = new ModifierKeyGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, false);
        public static IModifierKey  ModifierKeyGamepadReversed = new ModifierKeyGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, true);
        public static IModifierKey  ModifierKeyGamepad2 = new ModifierKeyGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_BOTTOM, false);
        public static IModifierKey  ModifierKeyGamepad2Reversed = new ModifierKeyGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_BOTTOM, true);
        public static IModifierKey  ModifierKeyKeyboard = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT, false);
        public static IModifierKey  ModifierKeyKeyboardReversed = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT, true);
        public static IModifierKey  ModifierKeyMouse = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT, false);
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
        //public InputAction InputActionCRTMinus {get; private set;}
        //public InputAction InputActionCRTPlus {get; private set;}
        
        //example scene controlled
        public InputAction InputActionZoom {get; private set;}
        // public InputAction InputActionPause {get; private set;}
        public InputAction InputActionCyclePalette {get; private set;}
        public InputAction InputActionReset {get; private set;}

        private readonly List<InputAction> inputActions = new();
        
        private FPSLabel fpsLabel;// = new(GetFontDefault(), ExampleScene.ColorHighlight3);

        private float mouseMovementTimer = 0f;
        private const float mouseMovementDuration = 2f;

        public bool MouseControlEnabled = true;

        private PaletteInfoBox paletteInfoBox;


        private List<ScreenTexture> gameTextures = new(5);
        private int curGameTextureIndex = 0;

        
        public GameloopExamples() : base
            (
                GameSettings.StretchMode, 
                new WindowSettings()
                {
                    Title = "Shape Engine Examples",
                    Topmost = false,
                    FullscreenAutoRestoring = true,
                    WindowBorder = WindowBorder.Resizabled,
                    WindowMinSize = new(480, 270),
                    WindowSize = new(960, 540),
                    Monitor = 0,
                    Vsync = false,
                    FrameRateLimit = 60,
                    MinFramerate = 30,
                    MaxFramerate = 240,
                    WindowOpacity = 1f,
                    MouseEnabled = true,
                    MouseVisible = false,
                    Msaa4x = true,
                    HighDPI = false,
                    FramebufferTransparent = false
                }
                // WindowSettings.Default
            )
        {

            UIRects = new(new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), new Rect.Margins(0.015f), "main");
            var mainTop = new RectNode(new Vector2(0.5f, 0f), new Vector2(1f, 0.1f), "top");
            var mainCenter = new RectNode(new Vector2(0.5f, 0.5f), new Vector2(1f, 0.8f), "center");
            var mainBottom = new RectNode(new Vector2(0.5f, 1f), new Vector2(1f, 0.1f), "bottom");
            UIRects.AddChild(mainTop);
            UIRects.AddChild(mainCenter);
            UIRects.AddChild(mainBottom);
            
            var mainTopLeft = new RectNode(new Vector2(0f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0.1f, 0f, 0f), "left");
            mainTopLeft.MouseFilter = MouseFilter.Stop;
            var mainTopCenter = new RectNode(new Vector2(0.5f, 0.5f), new Vector2(0.6f, 1f), new Rect.Margins(0f, 0.005f, 0f, 0.005f), "center");
            var mainTopRight = new RectNode(new Vector2(1f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0f, 0f, 0.1f), "right");
            mainTop.AddChild(mainTopLeft);
            mainTop.AddChild(mainTopCenter);
            mainTop.AddChild(mainTopRight);
            
            var mainCenterLeft = new RectNode(new Vector2(0f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0.1f, 0f, 0f), "left");
            var mainCenterCenter = new RectNode(new Vector2(0.5f, 0.5f), new Vector2(0.6f, 1f), new Rect.Margins(0f, 0.005f, 0f, 0.005f), "center");
            var mainCenterRight = new RectNode(new Vector2(1f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0f, 0f, 0.1f), "right");
            mainCenter.AddChild(mainCenterLeft);
            mainCenter.AddChild(mainCenterCenter);
            mainCenter.AddChild(mainCenterRight);
            
            var mainBottomLeft = new RectNode(new Vector2(0f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0.1f, 0f, 0f), "left");
            var mainBottomCenter = new RectNode(new Vector2(0.5f, 0.5f), new Vector2(0.6f, 1f), new Rect.Margins(0f, 0.005f, 0f, 0.005f), "center");
            var mainBottomRight = new RectNode(new Vector2(1f, 0.5f), new Vector2(0.2f, 1f), new Rect.Margins(0f, 0f, 0f, 0.1f), "right");
            mainBottom.AddChild(mainBottomLeft);
            mainBottom.AddChild(mainBottomCenter);
            mainBottom.AddChild(mainBottomRight);
            
            var mainTopLeftTop = new RectNode(new Vector2(0.5f, 0f), new Vector2(1f, 0.5f), "top");
            var mainTopLeftBottom = new RectNode(new Vector2(0.5f, 1f), new Vector2(1f, 0.5f), "bottom");
            mainTopLeft.AddChild(mainTopLeftTop);
            mainTopLeft.AddChild(mainTopLeftBottom);
            
            var mainTopRightTop = new RectNode(new Vector2(0.5f, 0f), new Vector2(1f, 0.5f), "top");
            var mainTopRightBottom = new RectNode(new Vector2(0.5f, 1f), new Vector2(1f, 0.5f), "bottom");
            mainTopRight.AddChild(mainTopRightTop);
            mainTopRight.AddChild(mainTopRightBottom);
            
            var mainBottomLeftTop = new RectNode(new Vector2(0.5f, 0f), new Vector2(1f, 0.5f), "top");
            var mainBottomLeftBottom = new RectNode(new Vector2(0.5f, 1f), new Vector2(1f, 0.5f), "bottom");
            mainBottomLeft.AddChild(mainBottomLeftTop);
            mainBottomLeft.AddChild(mainBottomLeftBottom);
            
            var mainBottomRightTop = new RectNode(new Vector2(0.5f, 0f), new Vector2(1f, 0.5f), "top");
            var mainBottomRightBottom = new RectNode(new Vector2(0.5f, 1f), new Vector2(1f, 0.5f), "bottom");
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
                // ScreenShaders.Add(blurShader);
                shapeShaders.Add(blurShader);
                
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
            
            //&& ShapeInput.GamepadDeviceManager.LastUsedGamepad.IsDown(ShapeGamepadAxis.RIGHT_TRIGGER, 0.1f)
            if (ShapeInput.CurrentInputDeviceType == InputDeviceType.Gamepad && ShapeInput.GamepadDeviceManager.LastUsedGamepad != null)
            {
                mouseMovementTimer = 0f;
                float speed = screenArea.Size.Max() * 0.75f * dt;
                int gamepad = ShapeInput.GamepadDeviceManager.LastUsedGamepad.Index;
                var x = InputAction.GetState(ShapeGamepadAxis.LEFT_X, GamepadMouseMovementTag, gamepad, 0.05f).AxisRaw;
                var y = InputAction.GetState(ShapeGamepadAxis.LEFT_Y, GamepadMouseMovementTag, gamepad, 0.05f).AxisRaw;

                var movement = new Vector2(x, y);
                float l = movement.Length();
                if (l <= 0f) return mousePos;
                
                var dir = movement / l;
                return mousePos + dir * l * speed;
            }
            
            if (ShapeInput.CurrentInputDeviceType == InputDeviceType.Keyboard)
            {
                mouseMovementTimer += dt;
                if (mouseMovementTimer >= mouseMovementDuration) mouseMovementTimer = mouseMovementDuration;
                float t = mouseMovementTimer / mouseMovementDuration; 
                float f = ShapeMath.LerpFloat(0.2f, 1f, t);
                float speed = screenArea.Size.Max() * 0.5f * dt * f;
                var x = InputAction.GetState(ShapeKeyboardButton.LEFT, ShapeKeyboardButton.RIGHT, KeyboardMouseMovementTag).AxisRaw;
                var y = InputAction.GetState(ShapeKeyboardButton.UP, ShapeKeyboardButton.DOWN, KeyboardMouseMovementTag).AxisRaw;

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

            CurGamepad = ShapeInput.GamepadDeviceManager.RequestGamepad(0); // Input.RequestGamepad(0);
            if (CurGamepad != null)
            {
                foreach (var action in inputActions)
                {
                    action.Gamepad = CurGamepad;
                }
            }
            
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

        protected override void OnGamepadConnected(ShapeGamepadDevice gamepad)
        {
            if (CurGamepad != null) return;
            CurGamepad = ShapeInput.GamepadDeviceManager.RequestGamepad(0);
            
            if (CurGamepad != null)
            {
                foreach (var action in inputActions)
                {
                    action.Gamepad = CurGamepad;
                }
            }
        }

        protected override void OnGamepadDisconnected(ShapeGamepadDevice gamepad)
        {
            if (CurGamepad == null) return;
            if (CurGamepad.Index == gamepad.Index)
            {
                CurGamepad = ShapeInput.GamepadDeviceManager.RequestGamepad(0);

                foreach (var action in inputActions)
                {
                    action.Gamepad = CurGamepad;
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
            ShapeDrawing.DrawLine(tip, left, 1f, colorRgba, LineCapType.CappedExtended, 3);
            ShapeDrawing.DrawLine(tip, top, 1f, colorRgba, LineCapType.CappedExtended, 3);
            ShapeDrawing.DrawCircleSectorLines(circleCenter, size, 180, 270, 1f, colorRgba, false, 4f);
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

                    var rPixelValue = ShapeRandom.RandF(5.9f, 6.1f);
                
                    ShapeShader.SetValueFloat(pixelationShader.Shader, "pixelWidth", rPixelValue * Camera.BaseZoomLevel);
                    ShapeShader.SetValueFloat(pixelationShader.Shader, "pixelHeight", rPixelValue * Camera.BaseZoomLevel);
                }
            
                var darknessShader = ScreenShaders.Get(darknessID);
                if (darknessShader != null && darknessShader.Enabled)
                {
                    // var f = game.MousePos / game.Area.Size.ToVector2();
                    ShapeShader.SetValueVector2(darknessShader.Shader, "origin", game.RelativeMousePosition);
                }
            
                var overdrawShader = ScreenShaders.Get(overdrawID);
                if (overdrawShader != null && overdrawShader.Enabled)
                {
                    if (ShapeRandom.Chance(0.025f))
                    {
                        ShapeShader.SetValueColor(overdrawShader.Shader, "overdrawColor", ShapeRandom.RandColor(127, 255, 255));
                        ShapeShader.SetValueFloat(overdrawShader.Shader, "blend", ShapeRandom.RandF(0.4f, 0.6f));
                    }
                
                }
            }
            
            UIRects.UpdateRect(ui.Area);
            UIRects.Update(time.Delta, ui.MousePos);

            InputAction.UpdateActions(time.Delta, CurGamepad, inputActions);

            var fullscreenState = InputActionFullscreen.Consume();
            if (fullscreenState is { Consumed: false, Pressed: true })
            {
                // var win = GameWindow.Instance;
                // if (win.IsWindowFullscreen())
                // {
                //     win.RestoreWindow();
                // }
                // else
                // {
                //     Console.WriteLine("---------------------------- Fullscreen mode enabled");
                //     win.ActivateFullscreen(1920 / 2, 1080 / 2);
                // }
                GAMELOOP.Window.ToggleBorderlessFullscreen();
            }

            var maximizeState = InputActionMaximize.Consume();
            if (maximizeState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.Window.ToggleMaximizeWindow();
            }

            var nextMonitorState = InputActionNextMonitor.Consume();
            if (nextMonitorState is { Consumed: false, Pressed: true })
            {
               Window.NextMonitor();
            }

            if (ShapeKeyboardButton.H.GetInputState().Pressed)
            {
                NextGameTexture();
            }

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
            
            
            if (Paused) return;

            
            
            var paletteState = GAMELOOP.InputActionCyclePalette.Consume();
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
                var cycleShaders = InputActionCycleShaders.Consume();
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
            
            //var crtDefault = new Vector2(6, 4);
            //var crtSpeed = crtDefault * 0.5f * time.Delta;
            // var crtPlusState = InputActionCRTPlus.Consume();
            // if (crtPlusState is { Consumed: false, Down: true })
            // {
            //     var crtShader = ScreenShaders.Get(crtShaderID);
            //     if (crtShader is { Enabled: true })
            //     {
            //         crtCurvature += crtSpeed;
            //         if (crtCurvature.X >= 9f)
            //         {
            //             crtCurvature = new(9f, 6f);
            //             crtShader.Enabled = false;
            //         }
            //         ShapeShader.SetValueVector2(crtShader.Shader, "curvatureAmount", crtCurvature.X, crtCurvature.Y);
            //     }
            //     
            // }

            // var crtMinusState = InputActionCRTMinus.Consume();
            // if (crtMinusState is { Consumed: false, Down: true })
            // {
            //     var crtShader = ScreenShaders.Get(crtShaderID);
            //     if (crtShader != null)
            //     {
            //         crtCurvature -= crtSpeed;
            //         if (!crtShader.Enabled && crtCurvature.X < 9f) crtShader.Enabled = true;
            //         
            //         if (crtCurvature.X <= 1.5f)
            //         {
            //             crtCurvature = new(1.5f, 1f);
            //         }
            //         ShapeShader.SetValueVector2(crtShader.Shader, "curvatureAmount", crtCurvature.X, crtCurvature.Y);
            //     }
            // }
            
            
            
            paletteInfoBox.Update(time.Delta);
        }

        // protected override void DrawGame(ScreenInfo game)
        // {
        //     game.MousePos.Draw(50, new ColorRgba(Color.Red));
        //     game.Area.DrawLines(100f, new ColorRgba(Color.Red));
        // }
        //
        // protected override void DrawGameUI(ScreenInfo gameUi)
        // {
        //     // gameUi.MousePos.Draw(25, new ColorRgba(Color.Green));
        //     gameUi.Area.DrawLines(5f, new ColorRgba(Color.Green));
        // }

        protected override void DrawUI(ScreenInfo ui)
        {
            var fpsRect = UIRects.GetRect("top right top");//"top", "right", "top");
            fpsLabel.Draw(fpsRect, new(1f, 0f), 1f);
            
            paletteInfoBox.Draw(ui.Area.ApplyMargins(0.8f,0.025f,0.25f,0.65f));
            // UIRects.DebugDraw(new ColorRgba(Color.Azure), 1f);
        }

        public int GetFontCount() { return fonts.Count; }
        public Font GetFont(int id) { return fonts[id]; }
        public string GetFontName(int id) { return fontNames[id]; }
        public Font GetRandomFont()
        {
            Font? randFont = ShapeRandom.RandCollection<Font>(fonts.Values.ToList(), false);
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
            
            var stretchTexture = new ScreenTexture(shaderMode, TextureFilter.Trilinear);
            var pixelationTexture = new ScreenTexture(0.25f, shaderMode, TextureFilter.Point);
            var fixedTexture = new ScreenTexture(new Dimensions(500, 500), shaderMode, TextureFilter.Point, false);
            var nearestFixedTexture = new ScreenTexture(new Dimensions(480, 240), shaderMode, TextureFilter.Point, true);
            var anchorTexture = new ScreenTexture(new Vector2(0.4f, 0.4f), new Vector2(0.05f, 0.5f), shaderMode, TextureFilter.Trilinear);

            // stretchTexture.BackgroundColor = new ColorRgba(0, 0, 0, 1);
            // pixelationTexture.BackgroundColor = new ColorRgba(0, 0, 0, 1);
            fixedTexture.BackgroundColor = Colors.Background.ChangeBrightness(-0.5f); // new ColorRgba(0, 0, 0, 255);
            // nearestFixedTexture.BackgroundColor = new ColorRgba(0, 0, 0, 1);
            anchorTexture.BackgroundColor = Colors.Background.ChangeBrightness(-0.5f); //new ColorRgba(0, 0, 0, 255);
            
            
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
            // ModifierKeyGamepad = new ModifierKeyGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, false);
            // ModifierKeyGamepadReversed = new ModifierKeyGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, true);
            // ModifierKeyKeyboard = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT, false);
            // ModifierKeyKeyboardReversed = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT, true);
            // ModifierKeyMouse = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT, false);
            // ModifierKeyMouseReversed = new ModifierKeyKeyboardButton(ShapeKeyboardButton.LEFT_SHIFT, true);
            
            //gameloop
            var cancelKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ESCAPE);
            var cancelGB = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_LEFT);
            InputActionUICancel= new(InputAction.AllAccessTag, cancelKB, cancelGB);
            
          
            var fullscreenKB = new InputTypeKeyboardButton(ShapeKeyboardButton.F);
            var fullscreenGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            InputActionFullscreen = new(GameloopAccessTag, fullscreenKB, fullscreenGB);
            
            var maximizeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.M);
            // var maximizeGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_THUMB);
            InputActionMaximize = new(GameloopAccessTag, maximizeKB);
            
            var minimizeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.N);
            InputActionMinimize = new(GameloopAccessTag, minimizeKB);
            
            var nextMonitorKB = new InputTypeKeyboardButton(ShapeKeyboardButton.B);
            //var nextMonitorGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            InputActionNextMonitor = new(GameloopAccessTag, nextMonitorKB);

            var cycleShaderKB = new InputTypeKeyboardButton(ShapeKeyboardButton.J);
            InputActionCycleShaders = new InputAction(GameloopAccessTag, cycleShaderKB);
            
            //var crtMinusKB = new InputTypeKeyboardButton(ShapeKeyboardButton.J);
            // var crtMinusGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP, 0f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            //var crtPluseGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            //InputActionCRTMinus = new(GameloopAccessTag, crtMinusKB);
            
            //var crtPlusKB = new InputTypeKeyboardButton(ShapeKeyboardButton.K);
            // var crtPlusGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP, 0f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            //var crtMinusGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            //InputActionCRTPlus = new(GameloopAccessTag, crtPlusKB);
            
            // var pauseKB = new InputTypeKeyboardButton(ShapeKeyboardButton.P);
            // var pauseGB = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_RIGHT);
            // InputActionPause = new(SceneAccessTag, pauseKB, pauseGB);
            
            var paletteKb = new InputTypeKeyboardButton(ShapeKeyboardButton.P);
            var paletteGp = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_RIGHT);
            var paletteMb = new InputTypeMouseButton(ShapeMouseButton.SIDE);
            InputActionCyclePalette = new(SceneAccessTag, paletteKb, paletteGp, paletteMb);
            
            //main scene
            var backKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
            var backGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            var backMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            InputActionUIBack = new(UIAccessTag, backKB, backGB, backMB);

            var acceptKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var acceptKB2 = new InputTypeKeyboardButton(ShapeKeyboardButton.ENTER);
            var acceptGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            InputActionUIAccept = new(UIAccessTag, acceptKB, acceptKB2, acceptGB);
            
            var acceptMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            InputActionUIAcceptMouse = new(UIAccessTag, acceptMB);

            var leftKB = new InputTypeKeyboardButton(ShapeKeyboardButton.A);
            var leftGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT, 0.1f, ModifierKeyOperator.Or, ModifierKeyGamepadReversed);
            InputActionUILeft = new(UIAccessTag, leftKB, leftGB);
            
            var rightKB = new InputTypeKeyboardButton(ShapeKeyboardButton.D);
            var rightGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT, 0.1f, ModifierKeyOperator.Or, ModifierKeyGamepadReversed);
            InputActionUIRight = new(UIAccessTag, rightKB, rightGB);
            
            var upKB = new InputTypeKeyboardButton(ShapeKeyboardButton.W);
            var upGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP, 0.1f, ModifierKeyOperator.Or, ModifierKeyGamepadReversed);
            InputActionUIUp = new(UIAccessTag, upKB, upGB);
            
            var downKB = new InputTypeKeyboardButton(ShapeKeyboardButton.S);
            var downGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0.1f, ModifierKeyOperator.Or, ModifierKeyGamepadReversed);
            InputActionUIDown = new(UIAccessTag, downKB, downGB);
            
            var nextTabKB = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
            var nextTabGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            var nextTabMW = new InputTypeMouseButton(ShapeMouseButton.MW_DOWN, 2f);
            InputActionUINextTab = new(UIAccessTag, nextTabKB, nextTabGB, nextTabMW);
            
            var prevTabKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var prevTabGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
            var prevTabMW = new InputTypeMouseButton(ShapeMouseButton.MW_UP, 2f);
            InputActionUIPrevTab = new(UIAccessTag, prevTabKB, prevTabGB, prevTabMW);
            
            var nextPageKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var nextPageGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP, 0.1f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            InputActionUINextPage = new(UIAccessTag, nextPageKB, nextPageGB);
            
            var prevPageKB = new InputTypeKeyboardButton(ShapeKeyboardButton.X);
            var prevPageGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, 0.1f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            InputActionUIPrevPage = new(UIAccessTag, prevPageKB, prevPageGB);
            
            //example scene only
            var zoomKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.NINE, ShapeKeyboardButton.ZERO);
            // var zoomGP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.2f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            var zoomGP = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_DOWN, ShapeGamepadButton.LEFT_FACE_UP, 0.2f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            var zoomMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f); //, ModifierKeyOperator.Or, ModifierKeyMouse);
            InputActionZoom = new(SceneAccessTag, zoomKB, zoomGP, zoomMW);
            
            var resetKB = new InputTypeKeyboardButton(ShapeKeyboardButton.R);
            var resetGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_THUMB);
            InputActionReset = new(SceneAccessTag, resetKB, resetGB);
            
            inputActions.Add(InputActionUICancel);
            inputActions.Add(InputActionUIBack);
            inputActions.Add(InputActionUIAccept);
            inputActions.Add(InputActionUIAcceptMouse);
            inputActions.Add(InputActionUILeft);
            inputActions.Add(InputActionUIRight);
            inputActions.Add(InputActionUIUp);
            inputActions.Add(InputActionUIDown);
            inputActions.Add(InputActionUIPrevTab);
            inputActions.Add(InputActionUINextTab);
            inputActions.Add(InputActionUIPrevPage);
            inputActions.Add(InputActionUINextPage);
            inputActions.Add(InputActionFullscreen);
            inputActions.Add(InputActionMaximize);
            inputActions.Add(InputActionMinimize);
            inputActions.Add(InputActionNextMonitor);
            inputActions.Add(InputActionCycleShaders);
            // inputActions.Add(InputActionCRTMinus);
            // inputActions.Add(InputActionCRTPlus);
            inputActions.Add(InputActionZoom);
            inputActions.Add(InputActionCyclePalette);
            inputActions.Add(InputActionReset);
        }
    }

    
}
