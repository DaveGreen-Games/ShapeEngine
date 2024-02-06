// global using static Raylib_CsLo.Raylib;


using Raylib_CsLo;
using ShapeEngine.Screen;
using System.Numerics;
using System.Runtime.InteropServices;
using ShapeEngine.Color;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace ShapeEngine.Core;

// public static class RaylibWrapper
// {
//     
// }
//
// public static class RaylibConverter
// {
//     
// }

public class ShapeLoop
{
    #region Static
    public static readonly string CURRENT_DIRECTORY = AppDomain.CurrentDomain.BaseDirectory; // Environment.CurrentDirectory;
    public static OSPlatform OS_PLATFORM { get; private set; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatform.Windows :
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX :
        OSPlatform.FreeBSD;

    public static bool DebugMode { get; private set; } = false;
    public static bool ReleaseMode { get; private set; } = true;
    
    public static bool IsWindows() => OS_PLATFORM == OSPlatform.Windows;
    public static bool IsLinux() => OS_PLATFORM == OSPlatform.Linux;
    public static bool IsOSX() => OS_PLATFORM == OSPlatform.OSX;
    
    
    
    #endregion
    
    #region Public Members
    public string[] LaunchParams { get; protected set; } = Array.Empty<string>();
    
    public ColorRgba BackgroundColorRgba = ColorRgba.Black;
    public float ScreenEffectIntensity = 1.0f;

    public readonly ShaderContainer ScreenShaders = new();
    public ShapeCamera Camera
    {
        get => curCamera;
        set
        {
            if (value == curCamera) return;
            curCamera.Deactivate();
            curCamera = value;
            curCamera.Activate();
            curCamera.SetSize(CurScreenSize, DevelopmentDimensions);
        }
    }

    /// <summary>
    /// Scaling factors from current screen size to development resolution.
    /// </summary>
    public DimensionConversionFactors ScreenToDevelopment { get; private set; } = new();
    /// <summary>
    /// Scaling factors from development resolution to the current screen size.
    /// </summary>
    public DimensionConversionFactors DevelopmentToScreen { get; private set; } = new();
    public Dimensions DevelopmentDimensions { get; private set; } = new();
    public Dimensions CurScreenSize { get; private set; } = new();
    public Dimensions WindowMinSize { get; private set; } = new (128, 128);
    public Vector2 WindowPosition { get; private set; } = new();
    public ScreenInfo Game { get; private set; } = new();
    public ScreenInfo UI { get; private set; } = new();
    public float Delta { get; private set; } = 0f;
    public float DeltaSlow { get; private set; } = 0f;
    //public bool ScreenShaderAffectsUI { get; set; } = false;
    public MonitorDevice Monitor { get; private set; }
    public ICursor Cursor { get; private set; } = new NullCursor();
    public IScene CurScene { get; private set; } = new SceneEmpty();
    public int FrameRateLimit
    {
        get => frameRateLimit;
        set
        {
            if (value < 30) frameRateLimit = 30;
            else if (value > 240) frameRateLimit = 240;
            
            if(!VSync) Raylib.SetTargetFPS(frameRateLimit);
        }
    }
    public int FPS => Raylib.GetFPS();
    public bool VSync
    {
        get =>Raylib.IsWindowState(ConfigFlags.FLAG_VSYNC_HINT);
        
        set
        {
            if (Raylib.IsWindowState(ConfigFlags.FLAG_VSYNC_HINT) == value) return;
            if (value)
            {
                Raylib.SetWindowState(ConfigFlags.FLAG_VSYNC_HINT);
                Raylib.SetTargetFPS(Monitor.CurMonitor().Refreshrate);
            }
            else
            {
                Raylib.ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);
                Raylib.SetTargetFPS(frameRateLimit);
            }
        }
    }
    public bool Fullscreen
    {
        get => Raylib.IsWindowFullscreen();
        set
        {
            if (value == Raylib.IsWindowFullscreen()) return;
            if (value)
            {
                prevFullscreenWindowMaximized = Maximized;
                Maximized = false;
                prevFullscreenWindowPosition = Raylib.GetWindowPosition();
                prevFullscreenWindowSize = CurScreenSize;
                var mDim = Monitor.CurMonitor().Dimensions;
                Raylib.SetWindowSize(mDim.Width, mDim.Height);
                Raylib.SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }
            else
            {
                Raylib.ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                Raylib.SetWindowSize(prevFullscreenWindowSize.Width, prevFullscreenWindowSize.Height);
                Raylib.SetWindowPosition((int)prevFullscreenWindowPosition.X, (int)prevFullscreenWindowPosition.Y);
                
                if (prevFullscreenWindowMaximized) Maximized = true;
                
            }
            ResetMousePosition();
        }
    }
    public bool Maximized
    {
        get => Raylib.IsWindowMaximized();
        set
        {
            if (value == Raylib.IsWindowMaximized()) return;
            if (Fullscreen) Fullscreen = false;
            if(value)Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            else Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            //CheckForWindowChanges();
            ResetMousePosition();
        }
    }

    // private bool windowMinimized = false;
    // public bool Minimized
    // {
    //     get => windowMinimized;
    //     set
    //     {
    //         if (windowMinimized == value) return;
    //         windowMinimized = value;
    //         if(windowMinimized) Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
    //         else Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
    //     }
    // }
    //
    public Dimensions WindowSize
    {
        get => windowSize;
        set
        {
            var maxSize = Monitor.CurMonitor().Dimensions;
            int w = value.Width;
            if (w < WindowMinSize.Width) w = WindowMinSize.Width;
            else if (w > maxSize.Width) w = maxSize.Width;
            int h = value.Height;
            if (h < WindowMinSize.Height) h = WindowMinSize.Height;
            else if (h > maxSize.Height) h = maxSize.Height;
            
            windowSize = new(w, h);

            if (Fullscreen)
            {
                ResetMousePosition();
                return;
            }
            Raylib.SetWindowSize(windowSize.Width, windowSize.Height);
            CenterWindow();

            //CheckForWindowChanges();
        }
    }
    private bool paused = false;
    public bool Paused
    {
        get => paused;
        set
        {
            if (value != paused)
            {
                paused = value;
                ResolveOnPausedChanged(paused);
            }
            
        }
    }
    public SlowMotion SlowMotion { get; private set; } = new SlowMotion();
    
    //public static readonly ShapeInput Input = new();
    // public InputDevice CurrentInputDevice { get; private set; } = InputDevice.Keyboard;
    // public int MaxGamepads => gamepads.Length;
    // public Gamepad? LastUsedGamepad { get; private set; } = null;

    public bool CursorHidden { get; private set; } = false;
    public bool CursorLocked   {get; private set;}  = false;
    public static bool CursorOnScreen {get; private set;} = false;//i dont like the static here but input system needs it...
    #endregion

    #region Private Members

    private readonly ShapeTexture gameTexture = new();
    private readonly ShapeTexture screenShaderBuffer = new();
    private readonly ShapeCamera basicCamera = new ShapeCamera();
    private ShapeCamera curCamera;
    private Vector2 prevWindowPosition = new();
    private Dimensions prevFullscreenWindowSize = new(128, 128);
    private Vector2 prevFullscreenWindowPosition = new(0);
    private bool prevFullscreenWindowMaximized = false;
    private bool quit = false;
    private bool restart = false;
    private List<ShapeFlash> shapeFlashes = new();
    private List<DeferredInfo> deferred = new();
    private int frameRateLimit = 60;
    private Dimensions windowSize = new();
    
    // private readonly Gamepad[] gamepads = new Gamepad[8];
    // private readonly List<int> connectedGamepadIndices = new();
    private bool? wasCursorLocked = null;
    private bool? wasCursorHidden = null;
    
    private CursorState cursorState = new();
    private WindowState windowState = new();

    private Vector2 lastControlledMousePosition = new();
    private bool mouseControlled = false;
    #endregion
    
    #region Setup
    public ShapeLoop(Dimensions developmentDimensions, bool multiShaderSupport = false)
    {
        
        #if DEBUG
        DebugMode = true;
        ReleaseMode = false;
        #endif

        this.DevelopmentDimensions = developmentDimensions;
        // SetWindowState(ConfigFlags.FLAG_MSAA_4X_HINT);
        // SetWindowState(ConfigFlags.FLAG_WINDOW_HIGHDPI);
        Raylib.InitWindow(0, 0, "");
        
        Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
        Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
        Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

        Monitor = new MonitorDevice();
        SetupWindowDimensions();
        WindowMinSize = DevelopmentDimensions * 0.2f;
        Raylib.SetWindowMinSize(WindowMinSize.Width, WindowMinSize.Height);
        
        SetConversionFactors();
        
        VSync = true;
        FrameRateLimit = 60;

        curCamera = basicCamera;
        Camera.Activate();
        Camera.SetSize(CurScreenSize, DevelopmentDimensions);
        
        var mousePosUI = Raylib.GetMousePosition();
        var mousePosGame = Camera.ScreenToWorld(mousePosUI);
        var screenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
        var cameraArea = Camera.Area;

        Game = new(cameraArea, mousePosGame);
        UI = new(screenArea, mousePosUI);

        gameTexture.Load(CurScreenSize);
        if (multiShaderSupport) screenShaderBuffer.Load(CurScreenSize);

        
        // for (var i = 0; i < gamepads.Length; i++)
        // {
        //     gamepads[i] = new Gamepad(i, Raylib.IsGamepadAvailable(i));
        // }
        
        //Input = new();
        // Input.InputDeviceManager.OnGamepadConnectionChanged += OnInputInputDeviceManagerConnectionChanged;
        // Input.OnInputDeviceChanged += OnInputInputDeviceChanged;
        ShapeInput.OnInputDeviceChanged += OnInputDeviceChanged;
        ShapeInput.GamepadDeviceManager.OnGamepadConnectionChanged += OnGamepadConnectionChanged;
        
        //CursorOnScreen = Fullscreen || Raylib.IsCursorOnScreen();
        CursorOnScreen = Fullscreen || Raylib.IsCursorOnScreen() || ( Raylib.IsWindowFocused() && screenArea.ContainsPoint(Raylib.GetMousePosition()) );
        cursorState = GetCursorState();
        windowState = GetWindowState();
        
    }
    public void SetupWindow(string windowName, bool undecorated, bool resizable, bool vsync = true, int fps = 60)
    {
        Raylib.SetWindowTitle(windowName);
        if (undecorated) Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
        else Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);

        if (resizable) Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        else Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

        FrameRateLimit = fps;
        VSync = vsync;
    }
    public ExitCode Run(params string[] launchParameters)
    {
        this.LaunchParams = launchParameters;

        quit = false;
        restart = false;
        Raylib.SetExitKey(-1);

        StartGameloop();
        RunGameloop();
        EndGameloop();
        Raylib.CloseWindow();

        return new ExitCode(restart);
    }
    #endregion

    #region Public
    public void Restart()
    {
        restart = true;
        quit = true;
    }
    public void Quit()
    {
        restart = false;
        quit = true;
    }

    /// <summary>
    /// Switches to the new scene. Deactivate is called on the old scene and then Activate is called on the new scene.
    /// </summary>
    /// <param name="newScene"></param>
    public void GoToScene(IScene newScene)
    {
        if (newScene == CurScene) return;
        CurScene.Deactivate();
        //newScene.SetInput(Input);
        newScene.Activate(CurScene);
        CurScene = newScene;
    }

    public void CallDeferred(Action action, int afterFrames = 0)
    {
        deferred.Add(new(action, afterFrames));
    }
    private void ResolveDeferred()
    {
        for (int i = deferred.Count - 1; i >= 0; i--)
        {
            var info = deferred[i];
            if (info.Call()) deferred.RemoveAt(i);
        }
    }

    public void Flash(float duration, ColorRgba startColorRgba, ColorRgba endColorRgba)
    {
        if (duration <= 0.0f) return;
        if (ScreenEffectIntensity <= 0f) return;
        startColorRgba = startColorRgba.SetAlpha((byte)(startColorRgba.A * ScreenEffectIntensity));
        endColorRgba = endColorRgba.SetAlpha((byte)(endColorRgba.A * ScreenEffectIntensity));
        // byte startColorAlpha = (byte)(startColor.A * ScreenEffectIntensity);
        // startColor.A = startColorAlpha;
        // byte endColorAlpha = (byte)(endColor.A * ScreenEffectIntensity);
        // endColor.A = endColorAlpha;

        ShapeFlash flash = new(duration, startColorRgba, endColorRgba);
        shapeFlashes.Add(flash);
    }

    public void ClearFlashes() => shapeFlashes.Clear();
    
    public bool SwitchCursor(ICursor newCursor)
    {
        if (Cursor != newCursor)
        {
            Cursor.Deactivate();
            newCursor.Activate(Cursor);
            Cursor = newCursor;
            return true;
        }
        return false;
    }
    public void HideCursor() => SwitchCursor(new NullCursor());

    public bool HideOSCursor()
    {
        if (CursorHidden) return false;
        CursorHidden = true;
        Raylib.HideCursor();
        return true;
    }
    public bool ShowOSCursor()
    {
        if (!CursorHidden) return false;
        CursorHidden = false;
        Raylib.ShowCursor();;
        return true;
    }
    public bool LockOSCursor()
    {
        if (CursorLocked) return false;
        CursorLocked = true;
        Raylib.DisableCursor();
        return true;
    }
    public bool UnlockOSCursor()
    {
        if (!CursorLocked) return false;
        CursorLocked = false;
        Raylib.EnableCursor();
        return true;
    }
    
    
    public void CenterWindow()
    {
        if (Fullscreen) return;
        var monitor = Monitor.CurMonitor();

        int winPosX = monitor.Width / 2 - windowSize.Width / 2;
        int winPosY = monitor.Height / 2 - windowSize.Height / 2;
        Raylib.SetWindowPosition(winPosX + (int)monitor.Position.X, winPosY + (int)monitor.Position.Y);
        ResetMousePosition();
    }
    public void ResizeWindow(Dimensions newDimensions) => WindowSize = newDimensions;
    public void ResetWindow()
    {
        // if (Fullscreen) Raylib.ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
        // WindowSize = Monitor.CurMonitor().Dimensions / 2;
        
        if(Raylib.IsWindowMinimized()) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
        if(Raylib.IsWindowHidden()) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
        if (Maximized) Maximized = false;
        else if (Fullscreen) Fullscreen = false;
        WindowSize = Monitor.CurMonitor().Dimensions / 2;
        //ResetMousePosition();
    }
    public void ResetMousePosition()
    {
        // var monitor = Monitor.CurMonitor();
        // var center = monitor.Position + monitor.Dimensions.ToVector2() / 2;
        var center = WindowPosition / 2 + WindowSize.ToVector2() / 2; // CurScreenSize.ToVector2() / 2;
        Raylib.SetMousePosition((int)center.X, (int)center.Y);
    }
    public void ResetCamera() => Camera = basicCamera;

    
    #endregion
    
    #region  Gameloop
    private void StartGameloop()
    {
        LoadContent();
        BeginRun();
    }

    private void RunGameloop()
    {
        while (!quit)
        {
            if (Raylib.WindowShouldClose())
            {
                Quit();
                continue;
            }
            var dt = Raylib.GetFrameTime();
            Delta = dt;
            
            var newMonitor = Monitor.HasMonitorChanged();
            if (newMonitor.Available)
            {
                ChangeMonitor(newMonitor);
            }
            
            CheckForWindowChanges();
            ShapeInput.Update();
            Camera.SetSize(CurScreenSize, DevelopmentDimensions);
            if(!Paused) Camera.Update(dt);
            
            gameTexture.UpdateDimensions(CurScreenSize);
            screenShaderBuffer.UpdateDimensions(CurScreenSize);
            
            var screenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
            var cameraArea = Camera.Area;

            var mousePos = Raylib.GetMousePosition();
            if (mouseControlled) mousePos = lastControlledMousePosition;
            mouseControlled = false;

            CursorOnScreen = false;
            bool windowHidden = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
            bool windowMinimized = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
            bool windowTopmost = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
            bool windowUnfocused = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
            bool windowMousePassthrough = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_MOUSE_PASSTHROUGH);
            bool windowTransparent = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_TRANSPARENT);
            // bool windowInterlaced = Raylib.IsWindowState(ConfigFlags.FLAG_INTERLACED_HINT);
            
            if (windowTopmost && !windowUnfocused && !windowHidden && !windowMinimized 
                && !windowTransparent && !windowMousePassthrough)
            {
                if (Fullscreen || Raylib.IsCursorOnScreen() || screenArea.ContainsPoint(mousePos)) CursorOnScreen = true;
            }
            // if (Fullscreen) CursorOnScreen = true;
            // else if (Raylib.IsWindowFocused())
            // {
            //     if (Raylib.IsCursorOnScreen() || screenArea.ContainsPoint(mousePos)) CursorOnScreen = true;
            // }
            
            //CursorOnScreen = Fullscreen || Raylib.IsCursorOnScreen() || ( Raylib.IsWindowFocused() && screenArea.ContainsPoint(mousePos) );
            
            if (CursorOnScreen)
            {
                if (ShapeInput.CurrentInputDeviceType is InputDeviceType.Keyboard or InputDeviceType.Gamepad)
                {
                    mousePos = ChangeMousePos(dt, mousePos, screenArea);
                    mousePos = mousePos.Clamp(new Vector2(0, 0), CurScreenSize.ToVector2());
                    lastControlledMousePosition = mousePos;
                    mouseControlled = true;
                    
                    var mx = (int) MathF.Round(mousePos.X);
                    var my = (int) MathF.Round(mousePos.Y);
                    Raylib.SetMousePosition(mx, my);
                }

                
                // var prevMousePos = mousePos;
                // mousePos = ChangeMousePos(dt, mousePos, screenArea);
                // bool mousePosChanged = (mousePos - prevMousePos).LengthSquared() > 0f;
                // if (mousePosChanged)
                // {
                //     mousePos = mousePos.Clamp(new Vector2(0, 0), CurScreenSize.ToVector2());
                //     lastControlledMousePosition = mousePos;
                //     mouseControlled = true;
                //     Raylib.SetMousePosition((int)mousePos.X, (int)mousePos.Y);
                // }
                // if (Fullscreen || mousePosChanged)
                // {
                //     mousePos = mousePos.Clamp(new Vector2(0, 0), CurScreenSize.ToVector2());
                //
                //     // mouseMovementPosition = mousePos;
                //     
                //     var mx = (int) MathF.Round(mousePos.X);
                //     var my = (int) MathF.Round(mousePos.Y);
                //     Raylib.SetMousePosition(mx, my);
                //
                // }

            }
            
            if (!CursorOnScreen)
            {
                if (cursorState.OnScreen)
                {
                    ResolveOnCursorLeftScreen();
                    if(wasCursorHidden == null) wasCursorHidden = cursorState.Hidden;
                    if(wasCursorLocked == null) wasCursorLocked = cursorState.Locked;
                    UnlockOSCursor();
                    ShowOSCursor();
                }
            }
            else
            {
                if (!cursorState.OnScreen)
                {
                    ResolveOnCursorEnteredScreen();
                    if (wasCursorHidden != null && wasCursorHidden == true) HideOSCursor();
                    if (wasCursorLocked != null && wasCursorLocked == true) LockOSCursor();

                    wasCursorHidden = null;
                    wasCursorLocked = null;
                }
            }

            var curWindowState = GetWindowState();
            
            if (curWindowState.Focused && !windowState.Focused)
            {
                ResolveOnWindowFocusChanged(true);
                Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
            }
            else if (!curWindowState.Focused && windowState.Focused)
            {
                ResolveOnWindowFocusChanged(false);
                Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
            }

            if (curWindowState.Maximized && !windowState.Maximized)
            {
                ResolveOnWindowMaximizeChanged(true);
            }
            else if (!curWindowState.Maximized && windowState.Maximized)
            {
                ResolveOnWindowMaximizeChanged(false);
            }
            
            if (curWindowState.Fullscreen && !windowState.Fullscreen)
            {
                ResolveOnWindowFullscreenChanged(true);
            }
            else if (!curWindowState.Fullscreen && windowState.Fullscreen)
            {
                ResolveOnWindowFullscreenChanged(false);
            }

            //safety measure
            if (CursorHidden != Raylib.IsCursorHidden()) CursorHidden = Raylib.IsCursorHidden();
            
            var curCursorState = GetCursorState();
            if (curCursorState.Hidden && !cursorState.Hidden) ResolveOnCursorHiddenChanged(true);
            else if (!curCursorState.Hidden && cursorState.Hidden) ResolveOnCursorHiddenChanged(false);
            
            if (curCursorState.Locked && !cursorState.Locked) ResolveOnCursorLockChanged(true);
            else if (!curCursorState.Locked && cursorState.Locked) ResolveOnCursorLockChanged(false);

            cursorState = curCursorState;
            windowState = curWindowState;
            
            var mousePosUI = mousePos;// GetMousePosition();
            var mousePosGame = Camera.ScreenToWorld(mousePosUI);
            
            Game = new(cameraArea, mousePosGame);
            UI = new(screenArea, mousePosUI);

            
            if (!Paused)
            {
                SlowMotion.Update(dt);
                UpdateFlashes(dt);
            }
            var defaultFactor = SlowMotion.GetFactor(SlowMotion.TagDefault);
            DeltaSlow = Delta * defaultFactor;
            Cursor.Update(dt, UI);
            
            ResolveUpdate(dt, DeltaSlow, Game, UI);
            
            Raylib.BeginTextureMode(gameTexture.RenderTexture);
            Raylib.ClearBackground(new(0,0,0,0));
            
            Raylib.BeginMode2D(Camera.Camera);
            ResolveDrawGame(Game);
            Raylib.EndMode2D();
            
            foreach (var flash in shapeFlashes) screenArea.Draw(flash.GetColor());
            ResolveDrawGameUI(UI);
            if(CursorOnScreen) Cursor.DrawGameUI(UI);
            Raylib.EndTextureMode();
            
            DrawToScreen(screenArea, mousePosUI);

            ResolveDeferred();
        }
    }
    private void DrawToScreen(Rect screenArea, Vector2 mousePosUI)
    {
        var activeScreenShaders = ScreenShaders.GetActiveShaders();
        
        //multi shader support enabled and multiple screen shaders are active
        if (activeScreenShaders.Count > 1 && screenShaderBuffer.Loaded)
        {
            int lastIndex = activeScreenShaders.Count - 1;
            ShapeShader lastShader = activeScreenShaders[lastIndex];
            activeScreenShaders.RemoveAt(lastIndex);
            
            ShapeTexture source = gameTexture;
            ShapeTexture target = screenShaderBuffer;
            ShapeTexture temp;
            foreach (var shader in activeScreenShaders)
            {
                Raylib.BeginTextureMode(target.RenderTexture);
                Raylib.ClearBackground(new(0,0,0,0));
                Raylib.BeginShaderMode(shader.Shader);
                source.Draw();
                Raylib.EndShaderMode();
                Raylib.EndTextureMode();
                temp = source;
                source = target;
                target = temp;
            }
            
            Raylib.BeginDrawing();
            Raylib.ClearBackground(BackgroundColorRgba.ToRayColor());

            Raylib.BeginShaderMode(lastShader.Shader);
            target.Draw();
            Raylib.EndShaderMode();

            ResolveDrawUI(UI);
            if(CursorOnScreen) Cursor.DrawUI(UI);
            Raylib.EndDrawing();
            
        }
        else //single shader mode or only 1 screen shader is active
        {
            Raylib. BeginDrawing();
            Raylib.ClearBackground(BackgroundColorRgba.ToRayColor());

            if (activeScreenShaders.Count > 0)
            {
                Raylib.BeginShaderMode(activeScreenShaders[0].Shader);
                gameTexture.Draw();
                Raylib.EndShaderMode();
            }
            else
            {
                gameTexture.Draw();
            }
            
            ResolveDrawUI(UI);
            if(CursorOnScreen) Cursor.DrawUI(UI);
            Raylib.EndDrawing();
            
        }
    }
    private void EndGameloop()
    {
        EndRun();
        UnloadContent();
        screenShaderBuffer.Unload();
        gameTexture.Unload();
    }
    #endregion

    #region Virtual

    /// <summary>
    /// Called first after starting the gameloop.
    /// </summary>
    protected virtual void LoadContent() { }
    /// <summary>
    /// Called after LoadContent but before the main loop has started.
    /// </summary>
    protected virtual void BeginRun() { }

    protected virtual void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui) { }
    protected virtual void DrawGame(ScreenInfo game) { }
    protected virtual void DrawGameUI(ScreenInfo ui) { }
    protected virtual void DrawUI(ScreenInfo ui) { }

    /// <summary>
    /// Called before UnloadContent is called after the main gameloop has been exited.
    /// </summary>
    protected virtual void EndRun() { }
    /// <summary>
    /// Called after EndRun before the application terminates.
    /// </summary>
    protected virtual void UnloadContent() { }

    protected virtual void OnWindowSizeChanged(DimensionConversionFactors conversion) { }
    protected virtual void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos) { }
    protected virtual void OnMonitorChanged(MonitorInfo newMonitor) { }
    protected virtual void OnPausedChanged(bool newPaused) { }
    protected virtual void OnInputDeviceChanged(InputDeviceType prevDeviceType, InputDeviceType newDeviceType) { }
    protected virtual void OnGamepadConnected(ShapeGamepadDevice gamepad) { }
    protected virtual void OnGamepadDisconnected(ShapeGamepadDevice gamepad) { }
    protected virtual void OnCursorEnteredScreen() { }
    protected virtual void OnCursorLeftScreen() { }
    protected virtual void OnCursorHiddenChanged(bool hidden) { }
    protected virtual void OnCursorLockChanged(bool locked) { }
    protected virtual void OnWindowFocusChanged(bool focused) { }
    protected virtual void OnWindowFullscreenChanged(bool fullscreen) { }
    protected virtual void OnWindowMaximizeChanged(bool maximized) { }
    //protected virtual void OnWindowMinimizeChanged(bool minimized) { }
    //protected virtual void OnWindowHiddenChanged(bool hidden) { }
    
    /// <summary>
    /// Override this function to manipulate the final mouse position that is used for the rest of the frame.
    /// </summary>
    /// <param name="dt">The current delta time.</param>
    /// <param name="mousePos">The raw mouse position for this frame.</param>
    /// <returns>Return the new mouse position that should be used.</returns>
    protected virtual Vector2 ChangeMousePos(float dt, Vector2 mousePos, Rect screenArea) => mousePos;
    #endregion

    #region Monitor
    public bool SetMonitor(int newMonitor)
    {
        var monitor = Monitor.SetMonitor(newMonitor);
        if (monitor.Available)
        {
            ChangeMonitor(monitor);
            return true;
        }
        return false;
    }
    public void NextMonitor()
    {
        var nextMonitor = Monitor.NextMonitor();
        if (nextMonitor.Available)
        {
            ChangeMonitor(nextMonitor);
        }
    }
    private void ChangeMonitor(MonitorInfo monitor)
    {
        if (Fullscreen)
        {
            Raylib.SetWindowMonitor(monitor.Index);
            Raylib.SetWindowSize(monitor.Dimensions.Width, monitor.Dimensions.Height);
            Raylib.SetWindowPosition((int)monitor.Position.X, (int)monitor.Position.Y);
            Raylib.SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
        }

        var windowDimensions = windowSize;
        if (windowDimensions.Width > monitor.Width || windowDimensions.Height > monitor.Height)
        {
            windowDimensions = monitor.Dimensions / 2;
        }
        
        windowSize = windowDimensions;
        
        int winPosX = monitor.Width / 2 - windowDimensions.Width / 2;
        int winPosY = monitor.Height / 2 - windowDimensions.Height / 2;
        int x = winPosX + (int)monitor.Position.X;
        int y = winPosY + (int)monitor.Position.Y;
        
        prevFullscreenWindowSize = windowDimensions;
        prevFullscreenWindowMaximized = false;
        prevFullscreenWindowPosition =new(x, y);
        
        if (!Fullscreen)
        {
            Raylib.SetWindowPosition(x, y);
            Raylib.SetWindowSize(windowDimensions.Width, windowDimensions.Height);
        }
        
        ResetMousePosition();
        ResolveOnMonitorChanged(monitor);
    }
    #endregion

    #region Private Functions
    
    private void SetupWindowDimensions()
    {
        var monitor = Monitor.CurMonitor();
        WindowSize = monitor.Dimensions / 2;
        prevFullscreenWindowSize = windowSize;
        //CenterWindow();
        WindowPosition = Raylib.GetWindowPosition();
        prevWindowPosition = WindowPosition;
        prevFullscreenWindowPosition = WindowPosition;
        CalculateCurScreenSize();
    }
    private void CheckForWindowChanges()
    {
        var prev = CurScreenSize;
        CalculateCurScreenSize();
        if (prev != CurScreenSize)
        {
            if (!Maximized && !Fullscreen) windowSize = CurScreenSize;
            SetConversionFactors();
            var conversion = new DimensionConversionFactors(prev, CurScreenSize);
            //OnWindowDimensionsChanged?.Invoke(conversion);
            ResolveOnWindowSizeChanged(conversion);
            //CurScene.OnWindowSizeChanged(conversion);
        }

        var curWindowPosition = Raylib.GetWindowPosition();
        if (curWindowPosition != WindowPosition)
        {
            prevWindowPosition = WindowPosition;
            WindowPosition = curWindowPosition;
            ResolveOnWindowPositionChanged(WindowPosition, curWindowPosition);
        }
    }
    private void CalculateCurScreenSize()
    {
        if (Raylib.IsWindowFullscreen())
        {
            int monitor = Raylib.GetCurrentMonitor();
            int mw = Raylib.GetMonitorWidth(monitor);
            int mh = Raylib.GetMonitorHeight(monitor);
            var scaleFactor = Raylib.GetWindowScaleDPI();
            var scaleX = (int)scaleFactor.X;
            var scaleY = (int)scaleFactor.Y;
            CurScreenSize = new(mw * scaleX, mh * scaleY);
        }
        else
        {
            // var scaleFactor = GetWindowScaleDPI();
            // int scaleX = (int)scaleFactor.X;
            // int scaleY = (int)scaleFactor.Y;
            
            int w = Raylib.GetScreenWidth();
            int h = Raylib.GetScreenHeight();
            CurScreenSize = new(w , h);
        }
    }
    private void SetConversionFactors()
    {
        ScreenToDevelopment = new(CurScreenSize, DevelopmentDimensions);
        DevelopmentToScreen = new(DevelopmentDimensions, CurScreenSize);
    }
    
    private void UpdateFlashes(float dt)
    {
        for (int i = shapeFlashes.Count() - 1; i >= 0; i--)
        {
            var flash = shapeFlashes[i];
            flash.Update(dt);
            if (flash.IsFinished()) { shapeFlashes.RemoveAt(i); }
        }
    }
    private void ResolveUpdate(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
    {
        Update(dt, deltaSlow, game, ui);
        CurScene.Update(dt, deltaSlow, Game, UI);
    }
    private void ResolveDrawGame(ScreenInfo game)
    {
        DrawGame(game);
        CurScene.DrawGame(game);
    }
    private void ResolveDrawGameUI(ScreenInfo ui)
    {
        DrawGameUI(ui);
        CurScene.DrawGameUI(ui);
    }
    private void ResolveDrawUI(ScreenInfo ui)
    {
        DrawUI(ui);
        CurScene.DrawUI(ui);
    }
    private void ResolveOnWindowSizeChanged(DimensionConversionFactors conversion)
    {
        OnWindowSizeChanged(conversion);
        CurScene.OnWindowSizeChanged(conversion);
    }
    private void ResolveOnWindowPositionChanged(Vector2 oldPos, Vector2 newPos)
    {
        //Console.WriteLine($"Window Pos: {Raylib.GetWindowPosition()}");
        OnWindowPositionChanged(oldPos, newPos);
        CurScene.OnWindowPositionChanged(oldPos, newPos);
    }
    private void ResolveOnMonitorChanged(MonitorInfo newMonitor)
    {
        OnMonitorChanged(newMonitor);
        CurScene.OnMonitorChanged(newMonitor);
    }
    private void ResolveOnPausedChanged(bool newPaused)
    {
        OnPausedChanged(newPaused);
        CurScene.OnPauseChanged(newPaused);
    }
    private void ResolveOnCursorEnteredScreen()
    {
        OnCursorEnteredScreen();
        CurScene.OnCursorEnteredScreen();
    }
    private void ResolveOnCursorLeftScreen()
    {
        OnCursorLeftScreen();
        CurScene.OnCursorLeftScreen();
    }
    private void ResolveOnCursorHiddenChanged(bool hidden)
    {
        OnCursorHiddenChanged(hidden);
        CurScene.OnCursorHiddenChanged(hidden);
    }
    private void ResolveOnCursorLockChanged(bool locked)
    {
        OnCursorLockChanged(locked);
        CurScene.OnCursorLockChanged(locked);
    }
    private void ResolveOnWindowFocusChanged(bool focused)
    {
        OnWindowFocusChanged(focused);
        CurScene.OnWindowFocusChanged(focused);
    }
    private void ResolveOnWindowFullscreenChanged(bool fullscreen)
    {
        OnWindowFullscreenChanged(fullscreen);
        CurScene.OnWindowFullscreenChanged(fullscreen);
    }
    private void ResolveOnWindowMaximizeChanged(bool maximized)
    {
        OnWindowMaximizeChanged(maximized);
        CurScene.OnWindowMaximizeChanged(maximized);
        
    }
    private void OnGamepadConnectionChanged(ShapeGamepadDevice gamepad, bool connected)
    {
        if (connected)
        {
            OnGamepadConnected(gamepad);
            CurScene.OnGamepadConnected(gamepad);
        }
        else
        {
            OnGamepadDisconnected(gamepad);
            CurScene.OnGamepadDisconnected(gamepad);
        }
    }
    private void OnInputInputDeviceChanged(InputDeviceType prevDeviceType, InputDeviceType newDeviceType)
    {
        OnInputDeviceChanged(prevDeviceType, newDeviceType);
        CurScene.OnInputDeviceChanged(prevDeviceType, newDeviceType);
    }
    
    private CursorState GetCursorState()
    {
        // var cursorHidden = CursorHidden; // Raylib.IsCursorHidden();
        // var cursorOnScreen = this.CursorOnScreen;// Raylib.IsCursorOnScreen();
        return new(CursorHidden, CursorLocked, CursorOnScreen);
    }
    private WindowState GetWindowState()
    {
        var fullscreen = Fullscreen; // Raylib.IsWindowFullscreen();
        var maximized = Maximized; // Raylib.IsWindowMaximized();
        var minimized = Raylib.IsWindowMinimized(); //does not work...
        var hidden = Raylib.IsWindowHidden();
        var focused = Raylib.IsWindowFocused();
        return new(minimized, maximized, fullscreen, hidden, focused);
    }

    public float GetScreenPercentage()
    {
        var screenSize = Monitor.CurMonitor().Dimensions.ToVector2();
        var screenRect = new Rect(new(0f), screenSize, new(0f));

        var windowSize = CurScreenSize.ToVector2();
        var windowPos = Raylib.GetWindowPosition();
        var windowRect = new Rect(windowPos, windowSize, new(0f));
        float p = CalculateScreenPercentage(screenRect, windowRect);
        return p;
    }
    /// <summary>
    /// Reports how much of the window area is shown on the screen. 0 means window is not on the screen, 1 means whole window is on screen.
    /// </summary>
    /// <param name="screen"></param>
    /// <param name="window"></param>
    /// <returns></returns>
    private float CalculateScreenPercentage(Rect screen, Rect window)
    {
        var intersection = screen.Difference(window);
        if (intersection.Width <= 0f && intersection.Height <= 0f) return 0f;
        
        var screenArea = screen.GetArea();
        var intersectionArea = intersection.GetArea();
        var f = intersectionArea / screenArea;
        return f;
    }
    
    private void WriteDebugInfo()
    {
        Console.WriteLine("--------Shape Engine Monitor Info--------");
        if(Fullscreen)Console.WriteLine("Fullscreen is Enabled");
        else Console.WriteLine("Fullscreen is Disabled");
        
        if(Raylib.IsWindowMaximized()) Console.WriteLine("Window is Maximized");
        else Console.WriteLine("Window is NOT Maximized");
        
        var dpi = Raylib.GetWindowScaleDPI();
        Console.WriteLine($"DPI: {dpi.X}/{dpi.Y}");

        var sWidth = Raylib.GetScreenWidth();
        var sHeight = Raylib.GetScreenHeight();
        Console.WriteLine($"Screen: {sWidth}/{sHeight}");

        var monitor = Raylib.GetCurrentMonitor();
        var mWidth = Raylib.GetMonitorWidth(monitor);
        var mHeight = Raylib.GetMonitorHeight(monitor);
        var mpWidth = Raylib.GetMonitorPhysicalWidth(monitor);
        var mpHeight = Raylib.GetMonitorPhysicalHeight(monitor);
        Console.WriteLine($"[{monitor}] Monitor: {mWidth}/{mHeight} Physical: {mpWidth}/{mpHeight}");


        var rWidth = Raylib.GetRenderWidth();
        var rHeight = Raylib.GetRenderHeight();
        Console.WriteLine($"Render Size: {rWidth}/{rHeight}");

        Monitor.CurMonitor().WriteDebugInfo();
        Console.WriteLine("---------------------------------------");
    }
    #endregion
}
   

/*
 public struct Rectangle : IEquatable<Rectangle>
   {
     private static Rectangle emptyRectangle;
     [DataMember]
     public int X;
     [DataMember]
     public int Y;
     [DataMember]
     public int Width;
     [DataMember]
     public int Height;

     public static Rectangle Empty => Rectangle.emptyRectangle;

     public int Left => this.X;

     public int Right => this.X + this.Width;

     public int Top => this.Y;

     public int Bottom => this.Y + this.Height;

     public bool IsEmpty => this.Width == 0 && this.Height == 0 && this.X == 0 && this.Y == 0;

     public Point Location
     {
       get => new Point(this.X, this.Y);
       set
       {
         this.X = value.X;
         this.Y = value.Y;
       }
     }

     public Point Size
     {
       get => new Point(this.Width, this.Height);
       set
       {
         this.Width = value.X;
         this.Height = value.Y;
       }
     }

     public Point Center => new Point(this.X + this.Width / 2, this.Y + this.Height / 2);

     internal string DebugDisplayString
     {
       get
       {
         return this.X.ToString() + "  " + (object) this.Y + "  " + (object) this.Width + "  " + (object) this.Height;
       }
     }

     public Rectangle(int x, int y, int width, int height)
     {
       this.X = x;
       this.Y = y;
       this.Width = width;
       this.Height = height;
     }

     public Rectangle(Point location, Point size)
     {
       this.X = location.X;
       this.Y = location.Y;
       this.Width = size.X;
       this.Height = size.Y;
     }

     public static bool operator ==(Rectangle a, Rectangle b)
     {
       return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
     }

     public static bool operator !=(Rectangle a, Rectangle b) => !(a == b);

     public bool Contains(int x, int y)
     {
       return this.X <= x && x < this.X + this.Width && this.Y <= y && y < this.Y + this.Height;
     }

     public bool Contains(float x, float y)
     {
       return (double) this.X <= (double) x && (double) x < (double) (this.X + this.Width) && (double) this.Y <= (double) y && (double) y < (double) (this.Y + this.Height);
     }

     public bool Contains(Point value)
     {
       return this.X <= value.X && value.X < this.X + this.Width && this.Y <= value.Y && value.Y < this.Y + this.Height;
     }

     public void Contains(ref Point value, out bool result)
     {
       result = this.X <= value.X && value.X < this.X + this.Width && this.Y <= value.Y && value.Y < this.Y + this.Height;
     }

     public bool Contains(Vector2 value)
     {
       return (double) this.X <= (double) value.X && (double) value.X < (double) (this.X + this.Width) && (double) this.Y <= (double) value.Y && (double) value.Y < (double) (this.Y + this.Height);
     }

     public void Contains(ref Vector2 value, out bool result)
     {
       result = (double) this.X <= (double) value.X && (double) value.X < (double) (this.X + this.Width) && (double) this.Y <= (double) value.Y && (double) value.Y < (double) (this.Y + this.Height);
     }

     public bool Contains(Rectangle value)
     {
       return this.X <= value.X && value.X + value.Width <= this.X + this.Width && this.Y <= value.Y && value.Y + value.Height <= this.Y + this.Height;
     }

     public void Contains(ref Rectangle value, out bool result)
     {
       result = this.X <= value.X && value.X + value.Width <= this.X + this.Width && this.Y <= value.Y && value.Y + value.Height <= this.Y + this.Height;
     }

     public override bool Equals(object obj) => obj is Rectangle rectangle && this == rectangle;

     public bool Equals(Rectangle other) => this == other;

     public override int GetHashCode()
     {
       return (((17 * 23 + this.X.GetHashCode()) * 23 + this.Y.GetHashCode()) * 23 + this.Width.GetHashCode()) * 23 + this.Height.GetHashCode();
     }

     public void Inflate(int horizontalAmount, int verticalAmount)
     {
       this.X -= horizontalAmount;
       this.Y -= verticalAmount;
       this.Width += horizontalAmount * 2;
       this.Height += verticalAmount * 2;
     }

     public void Inflate(float horizontalAmount, float verticalAmount)
     {
       this.X -= (int) horizontalAmount;
       this.Y -= (int) verticalAmount;
       this.Width += (int) horizontalAmount * 2;
       this.Height += (int) verticalAmount * 2;
     }

     public bool Intersects(Rectangle value)
     {
       return value.Left < this.Right && this.Left < value.Right && value.Top < this.Bottom && this.Top < value.Bottom;
     }

     public void Intersects(ref Rectangle value, out bool result)
     {
       result = value.Left < this.Right && this.Left < value.Right && value.Top < this.Bottom && this.Top < value.Bottom;
     }

     public static Rectangle Intersect(Rectangle value1, Rectangle value2)
     {
       Rectangle result;
       Rectangle.Intersect(ref value1, ref value2, out result);
       return result;
     }

     public static void Intersect(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
     {
       if (value1.Intersects(value2))
       {
         int num1 = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
         int x = Math.Max(value1.X, value2.X);
         int y = Math.Max(value1.Y, value2.Y);
         int num2 = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
         result = new Rectangle(x, y, num1 - x, num2 - y);
       }
       else
         result = new Rectangle(0, 0, 0, 0);
     }
     public static Rectangle Union(Rectangle value1, Rectangle value2)
   {
     int x = Math.Min(value1.X, value2.X);
     int y = Math.Min(value1.Y, value2.Y);
     return new Rectangle(x, y, Math.Max(value1.Right, value2.Right) - x, Math.Max(value1.Bottom, value2.Bottom) - y);
   }

   public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
   {
     result.X = Math.Min(value1.X, value2.X);
     result.Y = Math.Min(value1.Y, value2.Y);
     result.Width = Math.Max(value1.Right, value2.Right) - result.X;
     result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
   }

     public void Offset(int offsetX, int offsetY)
     {
       this.X += offsetX;
       this.Y += offsetY;
     }

     public void Offset(float offsetX, float offsetY)
     {
       this.X += (int) offsetX;
       this.Y += (int) offsetY;
     }

     public void Offset(Point amount)
     {
       this.X += amount.X;
       this.Y += amount.Y;
     }

     public void Offset(Vector2 amount)
     {
       this.X += (int) amount.X;
       this.Y += (int) amount.Y;
     }

     public override string ToString()
     {
       return "{X:" + this.X.ToString() + " Y:" + this.Y.ToString() + " Width:" + this.Width.ToString() + " Height:" + this.Height.ToString() + "}";
     }

     

     public void Deconstruct(out int x, out int y, out int width, out int height)
     {
       x = this.X;
       y = this.Y;
       width = this.Width;
       height = this.Height;
     }
   }
 */


// internal static class RaylibWrapper
// {
//     #region Flags
//
//     #region SetFlags
//
//     public static void SetFullscreenFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
//         else ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
//     }
//     public static void SetWindowResizableFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
//     }
//     public static void SetWindowUndecoratedFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
//     }
//     public static void SetWindowTransparentFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_TRANSPARENT);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_TRANSPARENT);
//     }
//     public static void SetMSAA4XHintFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_MSAA_4X_HINT);
//         else ClearWindowState(ConfigFlags.FLAG_MSAA_4X_HINT);
//     }
//     public static void SetVsyncFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_VSYNC_HINT);
//         else ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);
//     }
//     public static void SetWindowHiddenFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
//     }
//     public static void SetWindowAlwaysRunFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);
//     }
//     public static void SetWindowMinimizedFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
//     }
//     public static void SetWindowMaximizedFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
//     }
//     public static void SetWindowUnfocusedFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
//         
//     }
//     public static void SetWindowTopmostFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
//         
//     }
//     public static void SetWindowHighDpiFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_HIGHDPI);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_HIGHDPI);
//         
//     }
//     public static void SetWindowMousePassthrougFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_WINDOW_MOUSE_PASSTHROUGH);
//         else ClearWindowState(ConfigFlags.FLAG_WINDOW_MOUSE_PASSTHROUGH);
//         
//     }
//     public static void SetInterlacedHintFlag(bool set = true)
//     {
//         if (set) SetWindowState(ConfigFlags.FLAG_INTERLACED_HINT);
//         else ClearWindowState(ConfigFlags.FLAG_INTERLACED_HINT);
//     }
//     #endregion
//
//     #region CheckFlag
//     public static bool IsFullscreenFlag() => IsWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
//     public static bool IsWindowResizeableFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
//     public static bool IsWindowUndecoratedFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
//     public static bool IsWindowTransparentFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_TRANSPARENT);
//     public static bool IsMSAA4XHintFlag() => IsWindowState(ConfigFlags.FLAG_MSAA_4X_HINT);
//     public static bool IsVsyncFlag() => IsWindowState(ConfigFlags.FLAG_VSYNC_HINT);
//     public static bool IsWindowHiddenFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
//     public static bool IsWindowAlwaysRunFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);
//     public static bool IsWindowMinimizedFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
//     public static bool IsWindowMaximizedFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
//     public static bool IsWindowUnfocusedFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
//     public static bool IsWindowTopmostFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
//     public static bool IsWindowHighDpiFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_HIGHDPI);
//     public static bool IsWindowMousePassthroughFlag() => IsWindowState(ConfigFlags.FLAG_WINDOW_MOUSE_PASSTHROUGH);
//     public static bool IsInterlacedHintFlag() => IsWindowState(ConfigFlags.FLAG_INTERLACED_HINT);
//     #endregion
//     
//     #endregion
// }

// private void CheckInputDevice()
    // {
    //     var prevInputDevice = CurrentInputDevice;
    //     if (CurrentInputDevice == InputDevice.Keyboard)
    //     {
    //         if (ShapeInput.WasMouseUsed()) CurrentInputDevice = InputDevice.Mouse;
    //         else
    //         {
    //             var index = ShapeInput.WasGamepadUsed(connectedGamepadIndices);
    //             if (index >= 0)
    //             {
    //                 CurrentInputDevice = InputDevice.Gamepad;
    //                 LastUsedGamepad = GetGamepad(index);
    //             }
    //         }
    //         //else if (ShapeInput.WasGamepadUsed(connectedGamepadIndices)) CurrentInputDevice = InputDevice.Gamepad;
    //     }
    //     else if (CurrentInputDevice == InputDevice.Mouse)
    //     {
    //         if (ShapeInput.WasKeyboardUsed()) CurrentInputDevice = InputDevice.Keyboard;
    //         else
    //         {
    //             var index = ShapeInput.WasGamepadUsed(connectedGamepadIndices);
    //             if (index >= 0)
    //             {
    //                 CurrentInputDevice = InputDevice.Gamepad;
    //                 LastUsedGamepad = GetGamepad(index);
    //             }
    //         }
    //         //else if (ShapeInput.WasGamepadUsed(connectedGamepadIndices)) CurrentInputDevice = InputDevice.Gamepad;
    //     }
    //     else //gamepad
    //     {
    //         if (ShapeInput.WasMouseUsed()) CurrentInputDevice = InputDevice.Mouse;
    //         else if (ShapeInput.WasKeyboardUsed()) CurrentInputDevice = InputDevice.Keyboard;
    //     }
    //
    //     if (CurrentInputDevice != prevInputDevice)
    //     {
    //         ResolveOnInputDeviceChanged(prevInputDevice, CurrentInputDevice);
    //     }
    // }
    // private void CheckGamepadConnections()
    // {
    //     connectedGamepadIndices.Clear();
    //     for (var i = 0; i < gamepads.Length; i++)
    //     {
    //         var gamepad = gamepads[i];
    //         if (Raylib.IsGamepadAvailable(i))
    //         {
    //             if (!gamepad.Connected)
    //             {
    //                 gamepad.Connect();
    //                 ResolveOnGamepadConnected(gamepad);
    //             }
    //             connectedGamepadIndices.Add(i);
    //         }
    //         else
    //         {
    //             if (gamepad.Connected)
    //             {
    //                 gamepad.Disconnect();
    //                 ResolveOnGamepadDisconnected(gamepad);
    //             }
    //         }
    //     }
    //     
    // }

// public bool HasGamepad(int index) => index >= 0 && index < gamepads.Length;
// public bool IsGamepadConnected(int index) => HasGamepad(index) && gamepads[index].Connected;
// public Gamepad? GetGamepad(int index)
// {
//     if (!HasGamepad(index)) return null;
//     return gamepads[index];
// }
// public Gamepad? RequestGamepad(int preferredIndex = -1)
// {
//     var preferredGamepad = GetGamepad(preferredIndex);
//     if (preferredGamepad is { Connected: true, Available: true })
//     {
//         preferredGamepad.Claim();
//         return preferredGamepad;
//     }
//
//     foreach (var gamepad in gamepads)
//     {
//         if (gamepad is { Connected: true, Available: true })
//         {
//             gamepad.Claim();
//             return gamepad;
//         }
//     }
//     return null;
// }
// public void ReturnGamepad(int index) => GetGamepad(index)?.Free();

    
    
    
    
    