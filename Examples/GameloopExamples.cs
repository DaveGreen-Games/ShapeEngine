using System.Numerics;
using System.Runtime.Serialization.Json;
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
            Vector2 center = ui.MousePos;
            float size = ui.Area.Size.Min() * 0.02f;
            Vector2 a = center;
            Vector2 b = center + new Vector2(0, size);
            Vector2 c = center + new Vector2(size, size);
            Triangle cursor = new(a, b, c);
            cursor.Draw(ExampleScene.ColorHighlight2);
            cursor.DrawLines(1f, ExampleScene.ColorHighlight1);
        }
        public void DrawUI(ScreenInfo ui){}
        public void Update(float dt, ScreenInfo ui)
        {
            
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
        public uint GetID()
        {
            return 0;
        }

        public void DrawGameUI(ScreenInfo ui)
        {
            
        }

        public void DrawUI(ScreenInfo ui)
        {
            Vector2 center = ui.MousePos;
            float size = ui.Area.Size.Min() * 0.02f;
            Vector2 a = center;
            Vector2 b = center + new Vector2(0, size);
            Vector2 c = center + new Vector2(size, size);
            Triangle cursor = new(a, b, c);
            cursor.Draw(ExampleScene.ColorHighlight2);
            cursor.DrawLines(1f, ExampleScene.ColorHighlight1);
        }
        public void Update(float dt, ScreenInfo ui)
        {
            
        }

        public void Deactivate()
        {
            
        }

        public void Activate(ICursor oldCursor)
        {
            
        }
    }

    
    public class GameloopExamples : ShapeLoop
    {
        //public BasicCamera GameCam { get; private set; }
        public Font FontDefault { get; private set; }


        private Dictionary<int, Font> fonts = new();
        private List<string> fontNames = new();
        private MainScene? mainScene = null;

        private uint crtShaderID = ShapeID.NextID;
        private Vector2 crtCurvature = new(6, 4);

        
        public Gamepad? CurGamepad = null;

        public NinePatchRect UIZones { get; private set; } = new();
        // public Rect RectTop { get; private set; } = new();
        // public Rect RectBottom { get; private set; } = new();
        // public Rect RectTopCenter { get; private set; } = new();
        // public Rect RectTopLeft { get; private set; } = new();
        // public Rect RectTopRight { get; private set; } = new();
        // public Rect RectBottomLeft { get; private set; } = new();
        // public Rect RectBottomCenter { get; private set; } = new();
        // public Rect RectBottomRight { get; private set; } = new();
        
        public static readonly uint UIAccessTag = 100;
        public static readonly uint GameloopAccessTag = 200;
        public static readonly uint SceneAccessTag = 300;
        public static readonly uint GamepadMouseMovementTag = 1000;
        public static readonly uint KeyboardMouseMovementTag = 2000;
        //ui
        public static readonly uint InputUICancelID = 110;
        public static readonly uint InputUIBackID = 111;
        public static readonly uint InputUIAcceptID = 112;
        public static readonly uint InputUILeftID = 113;
        public static readonly uint InputUIRightID = 114;
        public static readonly uint InputUIUpID = 115;
        public static readonly uint InputUIDownID = 116;
        public static readonly uint InputUINextTab = 117;
        public static readonly uint InputUIPrevTab = 118;
        public static readonly uint InputUINextPage = 119;
        public static readonly uint InputUIPrevPage = 120;
        
        //gameloop controlled
        public static readonly uint InputFullscreenID = 210;
        public static readonly uint InputMaximizeID = 211;
        public static readonly uint InputNextMonitorID = 212;
        public static readonly uint InputCRTMinusID = 213;
        public static readonly uint InputCRTPlusID = 214;
        
        //example scene controlled
        public static readonly uint InputZoomID = 310;
        // public static readonly uint InputZoomInID = 310;
        // public static readonly uint InputZoomOutID = 311;
        public static readonly uint InputPauseID = 312;
        public static readonly uint InputResetID = 313;

        private FPSLabel fpsLabel = new(GetFontDefault(), ExampleScene.ColorHighlight3);

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
            if (Input.CurrentInputDevice == InputDevice.Gamepad && Input.LastUsedGamepad != null)
            {
                float speed = screenArea.Size.Max() * 0.75f * dt; // 500f * dt;
                int gamepad = Input.LastUsedGamepad.Index;
                var x = Input.GetState(ShapeGamepadAxis.LEFT_X, GamepadMouseMovementTag, gamepad, 0.05f).AxisRaw;
                var y = Input.GetState(ShapeGamepadAxis.LEFT_Y, GamepadMouseMovementTag, gamepad, 0.05f).AxisRaw;

                var movement = new Vector2(x, y);
                float l = movement.Length();
                if (l <= 0f) return mousePos;
                
                var dir = movement / l;
                return mousePos + dir * l * speed;
            }

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
                Input.UpdateActionGamepad(CurGamepad.Index);
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
                Input.UpdateActionGamepad(CurGamepad.Index);
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
                    Input.UpdateActionGamepad(-1);
                }
                else
                {
                    Input.UpdateActionGamepad(CurGamepad.Index);
                }
            }
        }

        // protected override void OnWindowHiddenChanged(bool hidden)
        // {
        //     Console.WriteLine($"Window Hidden {hidden}");
        // }
        //
        // protected override void OnWindowMinimizeChanged(bool minimized)
        // {
        //     Console.WriteLine($"Window Minimized {minimized}");
        // }

        // protected override void OnWindowMaximizeChanged(bool maximized)
        // {
        //     Console.WriteLine($"Window Maximized {maximized}");
        // }
        //
        // protected override void OnWindowFullscreenChanged(bool fullscreen)
        // {
        //     Console.WriteLine($"Window Fullscreen {fullscreen}");
        // }
        //
        // protected override void OnWindowFocuseChanged(bool focused)
        // {
        //     Console.WriteLine($"Window Focused {focused}");
        // }
        //
        // protected override void OnCursorEnteredScreen()
        // {
        //     Console.WriteLine("Cursor Entered Screen");
        // }
        //
        // protected override void OnCursorLeftScreen()
        // {
        //     Console.WriteLine("Cursor Left Screen");
        // }

        protected override void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            UIZones = new(ui.Area, 0.2f, 0.6f, 0.06f, 0.88f, 0.01f, 0.01f);
            // RectTop = ui.Area.ApplyMargins(0.01f, 0.01f, 0.01f, 0.94f);
            // var topRects = RectTop.SplitV(0.15f, 0.05f, 0.6f, 0.05f);
            // RectTopLeft = topRects[0];
            // RectTopCenter = topRects[2];
            // RectTopRight = topRects[4];
            //
            // RectBottom = ui.Area.ApplyMargins(0.01f, 0.01f, 0.94f, 0.01f);
            // var bottomRects = RectBottom.SplitV(0.175f, 0.025f, 0.6f, 0.025f);
            // RectBottomLeft = bottomRects[0];
            // RectBottomCenter = bottomRects[2];
            // RectBottomRight = bottomRects[4];
            // var bottomRects = RectBottom.SplitV(0.2f);
            // RectBottomLeft = bottomRects.left;
            // RectBottomRight = bottomRects.right;
            
            
            
            var fullscreenState = Input.ConsumeAction(InputFullscreenID);
            if (fullscreenState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.Fullscreen = !GAMELOOP.Fullscreen;
            }
            var maximizeState = Input.ConsumeAction(InputMaximizeID);
            if (maximizeState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.Maximized = !GAMELOOP.Maximized;
            }
            var nextMonitorState = Input.ConsumeAction(InputNextMonitorID);
            if (nextMonitorState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.NextMonitor();
            }

            if (GAMELOOP.Paused) return;
            
            
            var crtDefault = new Vector2(6, 4);
            var crtSpeed = crtDefault * 0.5f * dt;
            
            
            var crtPlusState = Input.ConsumeAction(InputCRTPlusID);
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

            var crtMinusState = Input.ConsumeAction(InputCRTMinusID);
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
            
            // int speed = 2;
            // int movement = 0;
            // if (IsKeyDown(KeyboardKey.KEY_J))
            // {
            //     movement = 1;
            // }
            // else if (IsKeyDown(KeyboardKey.KEY_K))
            // {
            //     movement = -1;
            // }
            //
            // if (movement != 0)
            // {
            //     float change = movement * speed * dt;
            //     crtCurvature = (crtCurvature + new Vector2(change)).Clamp(new Vector2(1.5f, 1f), new Vector2(12, 8));
            //     
            //     var crtShader = ScreenShaders.Get(crtShaderID);
            //     if (crtShader != null)
            //     {
            //         ShapeShader.SetValueVector2(crtShader.Shader, "curvatureAmount", crtCurvature.X, crtCurvature.Y);
            //     }
            // }
        }

        protected override void DrawUI(ScreenInfo ui)
        {
            fpsLabel.Draw(UIZones.TopRight.SplitV(0.75f).top, new(1f, 0f), 1f);
            
            //UIZones.DrawLines(4f, 2f, WHITE, BLUE);
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
            //gameloop
            var cancelKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ESCAPE);
            var cancelGB = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_LEFT);
            InputAction uiCancel = new(ShapeInput.AllAccessTag, InputUICancelID, cancelKB, cancelGB);
            
          
            var fullscreenKB = new InputTypeKeyboardButton(ShapeKeyboardButton.F);
            var fullscreenGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            InputAction fullscreen = new(GameloopAccessTag, InputFullscreenID, fullscreenKB, fullscreenGB);
            
            var maximizeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.M);
            var maximizeGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_THUMB);
            InputAction maximize = new(GameloopAccessTag, InputMaximizeID, maximizeKB, maximizeGB);
            
            var nextMonitorKB = new InputTypeKeyboardButton(ShapeKeyboardButton.N);
            //var nextMonitorGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            InputAction nextMonitor = new(GameloopAccessTag, InputNextMonitorID, nextMonitorKB);
            
            var crtMinusKB = new InputTypeKeyboardButton(ShapeKeyboardButton.J);
            //var crtPluseGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            InputAction crtMinus = new(GameloopAccessTag, InputCRTMinusID, crtMinusKB);
            
            var crtPlusKB = new InputTypeKeyboardButton(ShapeKeyboardButton.K);
            //var crtMinusGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            InputAction crtPlus = new(GameloopAccessTag, InputCRTPlusID, crtPlusKB);
            
            var pauseKB = new InputTypeKeyboardButton(ShapeKeyboardButton.P);
            var pauseGB = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_RIGHT);
            InputAction pause = new(GameloopAccessTag, InputPauseID, pauseKB, pauseGB);
            
            
            
            //main scene
            var backKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
            var backGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            var backMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            InputAction uiBack = new(UIAccessTag, InputUIBackID, backKB, backGB, backMB);

            var acceptKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var acceptKB2 = new InputTypeKeyboardButton(ShapeKeyboardButton.ENTER);
            var acceptGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            var acceptMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            InputAction uiAccept = new(UIAccessTag, InputUIAcceptID, acceptKB, acceptKB2, acceptGB, acceptMB);

            var leftKB = new InputTypeKeyboardButton(ShapeKeyboardButton.A);
            //var leftKB2 = new InputTypeKeyboardButton(ShapeKeyboardButton.LEFT);
            var leftGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT);
            InputAction uiLeft = new(UIAccessTag, InputUILeftID, leftKB, leftGB);
            
            var rightKB = new InputTypeKeyboardButton(ShapeKeyboardButton.D);
            //var rightKB2 = new InputTypeKeyboardButton(ShapeKeyboardButton.RIGHT);
            var rightGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT);
            InputAction uiRight = new(UIAccessTag, InputUIRightID, rightKB, rightGB);
            
            var upKB = new InputTypeKeyboardButton(ShapeKeyboardButton.W);
            //var upKB2 = new InputTypeKeyboardButton(ShapeKeyboardButton.UP);
            var upGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP);
            InputAction uiUp = new(UIAccessTag, InputUIUpID, upKB, upGB);
            
            var downKB = new InputTypeKeyboardButton(ShapeKeyboardButton.S);
            //var downKB2 = new InputTypeKeyboardButton(ShapeKeyboardButton.DOWN);
            var downGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN);
            InputAction uiDown = new(UIAccessTag, InputUIDownID, downKB, downGB);
            
            var nextTabKB = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
            var nextTabGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            var nextTabMW = new InputTypeMouseButton(ShapeMouseButton.MW_DOWN, 2f);
            InputAction nextTab = new(UIAccessTag, InputUINextTab, nextTabKB, nextTabGB, nextTabMW);
            
            var prevTabKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var prevTabGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
            var prevTabMW = new InputTypeMouseButton(ShapeMouseButton.MW_UP, 2f);
            InputAction prevTab = new(UIAccessTag, InputUIPrevTab, prevTabKB, prevTabGB, prevTabMW);
            
            var nextPageKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var nextPageGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM);
            InputAction nextPage = new(UIAccessTag, InputUINextPage, nextPageKB, nextPageGB);
            
            var prevPageKB = new InputTypeKeyboardButton(ShapeKeyboardButton.X);
            var prevPageGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_BOTTOM);
            InputAction prevPage = new(UIAccessTag, InputUIPrevPage, prevPageKB, prevPageGB);
            
            
            
            //example scene only

            var zoomKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.NINE, ShapeKeyboardButton.ZERO);
            // var zoomMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL);
            var zoomGP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.2f);
            InputAction zoom = new(SceneAccessTag, InputZoomID, zoomKB, zoomGP);
            
            // var zoomInKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ZERO);
            // var zoomInGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
            // InputAction zoomIn = new(SceneAccessTag, InputZoomInID, zoomInKB, zoomInGB);
            
            // var zoomOutKB = new InputTypeKeyboardButton(ShapeKeyboardButton.NINE);
            // var zoomOutGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            // InputAction zoomOut = new(SceneAccessTag, InputZoomOutID, zoomOutKB, zoomOutGB);
            
            var resetKB = new InputTypeKeyboardButton(ShapeKeyboardButton.R);
            var resetGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            InputAction reset = new(SceneAccessTag, InputResetID, resetKB, resetGB);
            
            
            Input.AddActions
            (
                uiCancel, uiBack, uiAccept, uiLeft, uiRight, uiUp, uiDown,
                prevTab, nextTab, prevPage, nextPage,
                fullscreen, maximize, nextMonitor,
                crtMinus, crtPlus,
                zoom,
                pause, reset
            );
        }
    }

    
}
