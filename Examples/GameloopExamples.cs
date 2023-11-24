//global using static Examples.GameloopExamples;
global using static ShapeEngine.Core.ShapeLoop;

using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Lib;
using ShapeEngine.Persistent;
using ShapeEngine.Core;
using Examples.Scenes;
using Examples.UIElements;
using ShapeEngine.Core.Structs;
using ShapeEngine.Screen;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.UI;

namespace Examples
{
    internal class SimpleCursorGameUI : ICursor
    {
        public uint GetID()
        {
            return 0;
        }

        public void DrawGameUI(ScreenInfo ui)
        {
            // Vector2 center = ui.MousePos;
            // float size = ui.Area.Size.Min() * 0.02f;
            // Vector2 a = center;
            // Vector2 b = center + new Vector2(0, size);
            // Vector2 c = center + new Vector2(size, size);
            // Triangle cursor = new(a, b, c);
            // cursor.Draw(ExampleScene.ColorHighlight2);
            // cursor.DrawLines(1f, ExampleScene.ColorHighlight1);
            float size = ui.Area.Size.Min() * 0.02f;
            SimpleCursorUI.DrawRoundedCursor(ui.MousePos, size, ExampleScene.ColorHighlight1);
        }
        public void DrawUI(ScreenInfo ui){}
        public void Update(float dt, ScreenInfo ui)
        {
            
        }

        public void TriggerEffect(string effect)
        {
            throw new NotImplementedException();
        }

        public void Deactivate()
        {
            
        }

        public void Activate(ICursor oldCursor)
        {
            
        }
    }
    internal class SimpleCursorUI : ICursor
    {
        private float effectTimer = 0f;
        private const float EffectDuration = 0.25f;
        
        public uint GetID()
        {
            return 0;
        }

        public void DrawGameUI(ScreenInfo ui)
        {
            
        }

        public void DrawUI(ScreenInfo ui)
        {
            float size = ui.Area.Size.Min() * 0.02f;
            float t = 1f - (effectTimer / EffectDuration);
            //var c = effectTimer <= 0f ? ExampleScene.ColorHighlight1 : ExampleScene.ColorHighlight2;
            var c = ShapeColor.Lerp(ExampleScene.ColorHighlight2, ExampleScene.ColorHighlight1, t);
            //float curSize = effectTimer <= 0f ? size : ShapeMath.LerpFloat(size, size * 1.5f, t);// ShapeTween.Tween(size, size * 1.5f, t, TweenType.BOUNCE_OUT);
            
            DrawRoundedCursor(ui.MousePos, size, c);
        }
        public void Update(float dt, ScreenInfo ui)
        {
            if (effectTimer > 0f)
            {
                effectTimer -= dt;
                if (effectTimer <= 0f)
                {
                    effectTimer = 0f;
                }
            }   
        }

        public void TriggerEffect(string effect)
        {
            if (effect == "scale")
            {
                effectTimer = EffectDuration;
            }
        }

        public void Deactivate()
        {
            
        }

        public void Activate(ICursor oldCursor)
        {
            effectTimer = 0f;
        }

        internal static void DrawRoundedCursor(Vector2 tip, float size, Color color)
        {
            var dir = new Vector2(1, 1).Normalize();
            var circleCenter = tip + dir * size * 2;
            var left = circleCenter + new Vector2(-1, 0) * size;
            var top = circleCenter + new Vector2(0, -1) * size;
            ShapeDrawing.DrawLine(tip, left, 1f, color, LineCapType.CappedExtended, 3);
            ShapeDrawing.DrawLine(tip, top, 1f, color, LineCapType.CappedExtended, 3);
            ShapeDrawing.DrawCircleSectorLines(circleCenter, size, 180, 270, 1f, color, false, 4f);
        }
    }
    
    public class RectContainerMain : RectContainer
    {
        public RectContainerMain(string name) : base(name)
        {
            var top = new RectContainerH("top", 0.01f, 0.9f);
               
                var topLeft = new RectContainerV("left", 0.01f, 0.8f);
                    var topLeftTop = new RectContainerH("top", 0f, 0.5f);
                    var topLeftBottom = new RectContainerH("bottom", 0.5f, 0f);
                    topLeft.AddChild(topLeftTop);
                    topLeft.AddChild(topLeftBottom);
                
                var topCenter = new RectContainerV("center", 0.2f, 0.2f);
                
                var topRight = new RectContainerV("right", 0.8f, 0.01f);
                    var topRightTop = new RectContainerH("top", 0f, 0.5f);
                    var topRightBottom = new RectContainerH("bottom", 0.5f, 0f);
                    topRight.AddChild(topRightTop);
                    topRight.AddChild(topRightBottom);
                
                top.AddChild(topLeft);
                top.AddChild(topCenter);
                top.AddChild(topRight);
            
            var center = new RectContainerH("center", 0.1f, 0.1f);
                var centerLeft = new RectContainerV("left", 0.01f, 0.5f);
                var centerRight = new RectContainerV("right", 0.5f, 0.01f);
                center.AddChild(centerLeft);
                center.AddChild(centerRight);
            
            var bottom = new RectContainerH("bottom", 0.9f, 0.01f);
                var bottomLeft = new RectContainerV("left", 0.01f, 0.85f);
                var bottomCenter = new RectContainerV("center", 0.2f, 0.2f);
                var bottomRight = new RectContainerV("right", 0.85f, 0.01f);
                bottom.AddChild(bottomLeft);
                bottom.AddChild(bottomCenter);
                bottom.AddChild(bottomRight);
            
            AddChild(top);
            AddChild(center);
            AddChild(bottom);
        }
    }
    public class RectContainerH : RectContainer
    {
        public float TopFactor;
        public float BottomFactor;
        public RectContainerH(string name, float topFactor, float bottomFactor) : base(name)
        {
            TopFactor = topFactor;
            BottomFactor = bottomFactor;
        }
        protected override Rect OnRectUpdateRequested(Rect newRect)
        {
            return newRect.ApplyMargins(0.01f, 0.01f, TopFactor, BottomFactor);
        }
    }
    public class RectContainerV : RectContainer
    {
        public float LeftFactor;
        public float RightFactor;
        public RectContainerV(string name, float leftFactor, float rightFactor) : base(name)
        {
            LeftFactor = leftFactor;
            RightFactor = rightFactor;
        }
        protected override Rect OnRectUpdateRequested(Rect newRect)
        {
            return newRect.ApplyMargins(LeftFactor, RightFactor, 0.01f, 0.01f);
        }
    }
    
    
    public class GameloopExamples : ShapeLoop
    {
        public Font FontDefault { get; private set; }


        private Dictionary<int, Font> fonts = new();
        private List<string> fontNames = new();
        private MainScene? mainScene = null;

        private uint crtShaderID = ShapeID.NextID;
        private Vector2 crtCurvature = new(6, 4);

        
        public Gamepad? CurGamepad = null;

        public RectContainerMain UIRects = new("main");
        
        
        public readonly uint UIAccessTag = 100;
        public readonly uint GameloopAccessTag = 200;
        public readonly uint SceneAccessTag = 300;
        public readonly uint GamepadMouseMovementTag = 1000;
        public readonly uint KeyboardMouseMovementTag = 2000;
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
        public InputAction InputActionNextMonitor {get; private set;}
        public InputAction InputActionCRTMinus {get; private set;}
        public InputAction InputActionCRTPlus {get; private set;}
        
        //example scene controlled
        public InputAction InputActionZoom {get; private set;}
        public InputAction InputActionPause {get; private set;}
        public InputAction InputActionReset {get; private set;}

        private readonly List<InputAction> inputActions = new();
        
        private FPSLabel fpsLabel = new(GetFontDefault(), ExampleScene.ColorHighlight3);

        private float mouseMovementTimer = 0f;
        private const float mouseMovementDuration = 2f;

        // public bool UseMouseMovement = true;
        public GameloopExamples() : base(new(1920, 1080), true) {}

        protected override void LoadContent()
        {
            BackgroundColor = ExampleScene.ColorDark;
            
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


            Shader crt = ContentLoader.LoadFragmentShader("Resources/Shaders/CRTShader.fs");
            ShapeShader crtShader = new(crt, crtShaderID, true, 1);
            ShapeShader.SetValueFloat(crtShader.Shader, "renderWidth", CurScreenSize.Width);
            ShapeShader.SetValueFloat(crtShader.Shader, "renderHeight", CurScreenSize.Height);
            var bgColor = BackgroundColor;
            ShapeShader.SetValueColor(crtShader.Shader, "cornerColor", bgColor);// 1, 0, 0, 1);
            ShapeShader.SetValueFloat(crtShader.Shader, "vignetteOpacity", 0.35f);
            ShapeShader.SetValueVector2(crtShader.Shader, "curvatureAmount", crtCurvature.X, crtCurvature.Y);//smaller values = bigger curvature
            ScreenShaders.Add(crtShader);
            
            FontDefault = GetFont(FontIDs.JetBrains);
            fpsLabel.Font = FontDefault;
            this.VSync = false;
            this.FrameRateLimit = 60;

            HideOSCursor();
            //LockOSCursor();
            SwitchCursor(new SimpleCursorGameUI());

        }

        protected override Vector2 ChangeMousePos(float dt, Vector2 mousePos, Rect screenArea)
        {
            // if (!UseMouseMovement) return mousePos;
            
            if (Input.CurrentInputDevice == InputDevice.Gamepad && Input.LastUsedGamepad != null)
            {
                mouseMovementTimer = 0f;
                float speed = screenArea.Size.Max() * 0.75f * dt;
                int gamepad = Input.LastUsedGamepad.Index;
                var x = Input.GetState(ShapeGamepadAxis.LEFT_X, GamepadMouseMovementTag, gamepad, 0.05f).AxisRaw;
                var y = Input.GetState(ShapeGamepadAxis.LEFT_Y, GamepadMouseMovementTag, gamepad, 0.05f).AxisRaw;

                var movement = new Vector2(x, y);
                float l = movement.Length();
                if (l <= 0f) return mousePos;
                
                var dir = movement / l;
                return mousePos + dir * l * speed;
            }
            
            if (Input.CurrentInputDevice == InputDevice.Keyboard)
            {
                mouseMovementTimer += dt;
                if (mouseMovementTimer >= mouseMovementDuration) mouseMovementTimer = mouseMovementDuration;
                float t = mouseMovementTimer / mouseMovementDuration; 
                float f = ShapeMath.LerpFloat(0.2f, 1f, t);
                float speed = screenArea.Size.Max() * 0.5f * dt * f;
                var x = Input.GetState(ShapeKeyboardButton.LEFT, ShapeKeyboardButton.RIGHT, KeyboardMouseMovementTag).AxisRaw;
                var y = Input.GetState(ShapeKeyboardButton.UP, ShapeKeyboardButton.DOWN, KeyboardMouseMovementTag).AxisRaw;

                var movement = new Vector2(x, y);
                if (movement.LengthSquared() <= 0f) mouseMovementTimer = 0f;
                return mousePos + movement.Normalize() * speed;
            }

            mouseMovementTimer = 0f;
            return mousePos;
        }

        protected override void UnloadContent()
        {
            ContentLoader.UnloadFonts(fonts.Values);
        }
        protected override void BeginRun()
        {
            SetupInput();
            
            CurGamepad = Input.RequestGamepad(0);
            if (CurGamepad != null)
            {
                foreach (var action in inputActions)
                {
                    action.Gamepad = CurGamepad.Index;
                }
            }
            
            mainScene = new MainScene();
            GoToScene(mainScene);
        }
        
        protected override void OnWindowSizeChanged(DimensionConversionFactors conversionFactors)
        {
            var crtShader = ScreenShaders.Get(crtShaderID);
            if (crtShader != null)
            {
                ShapeShader.SetValueFloat(crtShader.Shader, "renderWidth", CurScreenSize.Width);
                ShapeShader.SetValueFloat(crtShader.Shader, "renderHeight", CurScreenSize.Height);
            }
        }

        protected override void OnGamepadConnected(Gamepad gamepad)
        {
            if (CurGamepad != null) return;
            CurGamepad = Input.RequestGamepad(0);
            
            if (CurGamepad != null)
            {
                foreach (var action in inputActions)
                {
                    action.Gamepad = CurGamepad.Index;
                }
            }
        }

        protected override void OnGamepadDisconnected(Gamepad gamepad)
        {
            if (CurGamepad == null) return;
            if (CurGamepad.Index == gamepad.Index)
            {
                CurGamepad = Input.RequestGamepad(0);

                if (CurGamepad == null)
                {
                    foreach (var action in inputActions)
                    {
                        action.Gamepad = -1;
                    }
                }
                else
                {
                    foreach (var action in inputActions)
                    {
                        action.Gamepad = CurGamepad.Index;
                    }
                }
            }
        }

        protected override void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            UIRects.SetRect(ui.Area);

            int gamepadIndex = CurGamepad?.Index ?? -1;
            Input.UpdateActions(dt, gamepadIndex, inputActions);
            
            var fullscreenState = Input.ConsumeAction(InputActionFullscreen);
            if (fullscreenState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.Fullscreen = !GAMELOOP.Fullscreen;
            }
            var maximizeState = Input.ConsumeAction(InputActionMaximize);
            if (maximizeState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.Maximized = !GAMELOOP.Maximized;
            }
            var nextMonitorState = Input.ConsumeAction(InputActionNextMonitor);
            if (nextMonitorState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.NextMonitor();
            }

            if (GAMELOOP.Paused) return;
            
            
            var crtDefault = new Vector2(6, 4);
            var crtSpeed = crtDefault * 0.5f * dt;
            
            
            var crtPlusState = Input.ConsumeAction(InputActionCRTPlus);
            if (crtPlusState is { Consumed: false, Down: true })
            {
                var crtShader = ScreenShaders.Get(crtShaderID);
                if (crtShader is { Enabled: true })
                {
                    crtCurvature += crtSpeed;
                    if (crtCurvature.X >= 9f)
                    {
                        crtCurvature = new(9f, 6f);
                        crtShader.Enabled = false;
                    }
                    ShapeShader.SetValueVector2(crtShader.Shader, "curvatureAmount", crtCurvature.X, crtCurvature.Y);
                }
                
            }

            var crtMinusState = Input.ConsumeAction(InputActionCRTMinus);
            if (crtMinusState is { Consumed: false, Down: true })
            {
                var crtShader = ScreenShaders.Get(crtShaderID);
                if (crtShader != null)
                {
                    crtCurvature -= crtSpeed;
                    if (!crtShader.Enabled && crtCurvature.X < 9f) crtShader.Enabled = true;
                    
                    if (crtCurvature.X <= 1.5f)
                    {
                        crtCurvature = new(1.5f, 1f);
                    }
                    ShapeShader.SetValueVector2(crtShader.Shader, "curvatureAmount", crtCurvature.X, crtCurvature.Y);
                }
            }
        }

        
        protected override void DrawUI(ScreenInfo ui)
        {
            var fpsRect = UIRects.GetRect("top", "right", "top");
            fpsLabel.Draw(fpsRect, new(1f, 0f), 1f);
        }

        public int GetFontCount() { return fonts.Count; }
        public Font GetFont(int id) { return fonts[id]; }
        public string GetFontName(int id) { return fontNames[id]; }
        public Font GetRandomFont()
        {
            Font? randFont = ShapeRandom.randCollection<Font>(fonts.Values.ToList(), false);
            return randFont != null ? (Font)randFont : FontDefault;
        }
        public void GoToMainScene()
        {
            if (mainScene == null) return;
            if (CurScene == mainScene) return;
            GoToScene(mainScene);
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
            InputActionUICancel= new(ShapeInput.AllAccessTag, cancelKB, cancelGB);
            
          
            var fullscreenKB = new InputTypeKeyboardButton(ShapeKeyboardButton.F);
            var fullscreenGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            InputActionFullscreen = new(GameloopAccessTag, fullscreenKB, fullscreenGB);
            
            var maximizeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.M);
            var maximizeGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_THUMB);
            InputActionMaximize = new(GameloopAccessTag, maximizeKB, maximizeGB);
            
            var nextMonitorKB = new InputTypeKeyboardButton(ShapeKeyboardButton.N);
            //var nextMonitorGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            InputActionNextMonitor = new(GameloopAccessTag, nextMonitorKB);
            
            var crtMinusKB = new InputTypeKeyboardButton(ShapeKeyboardButton.J);
            var crtMinusGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            //var crtPluseGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            InputActionCRTMinus = new(GameloopAccessTag, crtMinusKB, crtMinusGP);
            
            var crtPlusKB = new InputTypeKeyboardButton(ShapeKeyboardButton.K);
            var crtPlusGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP, 0f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            //var crtMinusGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            InputActionCRTPlus = new(GameloopAccessTag, crtPlusKB, crtPlusGP);
            
            var pauseKB = new InputTypeKeyboardButton(ShapeKeyboardButton.P);
            var pauseGB = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_RIGHT);
            InputActionPause = new(SceneAccessTag, pauseKB, pauseGB);
            
            
            
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
            var nextPageGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT, 0.1f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            InputActionUINextPage = new(UIAccessTag, nextPageKB, nextPageGB);
            
            var prevPageKB = new InputTypeKeyboardButton(ShapeKeyboardButton.X);
            var prevPageGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT, 0.1f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            InputActionUIPrevPage = new(UIAccessTag, prevPageKB, prevPageGB);
            
            //example scene only
            var zoomKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.NINE, ShapeKeyboardButton.ZERO);
            var zoomGP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.2f, ModifierKeyOperator.Or, ModifierKeyGamepad);
            var zoomMW =
                new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, ModifierKeyMouse);
            InputActionZoom = new(SceneAccessTag, zoomKB, zoomGP, zoomMW);
            
            var resetKB = new InputTypeKeyboardButton(ShapeKeyboardButton.R);
            var resetGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
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
            inputActions.Add(InputActionNextMonitor);
            inputActions.Add(InputActionCRTMinus);
            inputActions.Add(InputActionCRTPlus);
            inputActions.Add(InputActionZoom);
            inputActions.Add(InputActionPause);
            inputActions.Add(InputActionReset);
        }
    }

    
}
