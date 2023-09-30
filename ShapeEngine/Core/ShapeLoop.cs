global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;

using ShapeEngine.Screen;
using Raylib_CsLo;
using System.Numerics;
using System.Runtime.InteropServices;
using ShapeEngine.Lib;

namespace ShapeEngine.Core;

public class ShapeLoop
{
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
    
    public Raylib_CsLo.Color BackgroundColor = BLACK;
    public float ScreenEffectIntensity = 1.0f;
    
    public delegate void DimensionsChanged(DimensionConversionFactors conversion);
    public event DimensionsChanged? OnWindowDimensionsChanged;
    public string[] LaunchParams { get; protected set; } = Array.Empty<string>();

    
    public readonly ShaderContainer ScreenShaders = new();
    private readonly ShapeTexture gameTexture = new();
    
    private readonly ShapeTexture screenShaderBuffer = new();
    
    private readonly ShapeCamera basicCamera = new ShapeCamera();
    private ShapeCamera curCamera;
    public ShapeCamera Camera
    {
        get => curCamera;
        set
        {
            if (value == curCamera) return;
            curCamera.Deactive();
            curCamera = value;
            curCamera.Activate();
            curCamera.SetSize(CurScreenSize, DevelopmentDimensions);
        }
    }

    public void ResetCamera() => Camera = basicCamera;

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

    public ScreenInfo Game { get; private set; } = new();
    public ScreenInfo UI { get; private set; } = new();
    
    public float Delta { get; private set; } = 0f;

    private bool screenShaderAffectsUI = false;
    private bool quit = false;
    private bool restart = false;
    public MonitorDevice Monitor { get; private set; }
    public ICursor Cursor { get; private set; } = new NullCursor();
    public IScene CurScene { get; private set; } = new SceneEmpty();
    private List<ShapeFlash> shapeFlashes = new();
    private List<DeferredInfo> deferred = new();

    private int frameRateLimit = 60;
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
                SetTargetFPS(Monitor.CurMonitor().Refreshrate);
            }
            else
            {
                Raylib.ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);
                SetTargetFPS(frameRateLimit);
            }
        }
    }
    public bool Fullscreen
    {
        get => Raylib.IsWindowFullscreen();
        set
        {
            if (value == Raylib.IsWindowFullscreen()) return;
            if(value)Raylib.SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            else
            {
                Raylib.ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                Raylib.SetWindowSize(windowSize.Width, windowSize.Height);
                CenterWindow();
            }
            CheckWindowDimensionsChanged();
        }
    }
    public bool Maximized
    {
        get => Raylib.IsWindowMaximized();
        set
        {
            if (value == Raylib.IsWindowMaximized()) return;
            if(value)Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            else Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            CheckWindowDimensionsChanged();
        }
    }

    private Dimensions windowSize = new();
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
            
            if (Fullscreen) return;
            SetWindowSize(windowSize.Width, windowSize.Height);
            CenterWindow();

            CheckWindowDimensionsChanged();
        }
    }
    public void CenterWindow()
    {
        if (Fullscreen) return;
        var monitor = Monitor.CurMonitor();

        int winPosX = monitor.Width / 2 - windowSize.Width / 2;
        int winPosY = monitor.Height / 2 - windowSize.Height / 2;
        SetWindowPosition(winPosX + (int)monitor.Position.X, winPosY + (int)monitor.Position.Y);
    }
    public void ResizeWindow(Dimensions newDimensions) => WindowSize = newDimensions;
    public void ResetWindow()
    {
        if (Fullscreen) Raylib.ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
        WindowSize = Monitor.CurMonitor().Dimensions / 2;
    }

    
    
    public ShapeLoop(Dimensions developmentDimensions, bool multiShaderSupport = false, bool screenShadersAffectUI = false)
    {
        #if DEBUG
        DebugMode = true;
        ReleaseMode = false;
        #endif

        this.screenShaderAffectsUI = screenShadersAffectUI;
        this.DevelopmentDimensions = developmentDimensions;
        InitWindow(0, 0, "");
        
        ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
        SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

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
        
        var mousePosUI = GetMousePosition();
        var mousePosGame = Camera.ScreenToWorld(mousePosUI);
        var screenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
        var cameraArea = Camera.Area;

        Game = new(cameraArea, mousePosGame);
        UI = new(screenArea, mousePosUI);
        
        
        gameTexture.Load(CurScreenSize);
        if (multiShaderSupport) screenShaderBuffer.Load(CurScreenSize);
    }
    public void SetupWindow(string windowName, bool undecorated, bool resizable, bool vsync = true, int fps = 60)
    {
        SetWindowTitle(windowName);
        if (undecorated) SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
        else ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);

        if (resizable) SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        else ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

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
        CloseWindow();

        return new ExitCode(restart);
    }
    
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

    
    public void Flash(float duration, Raylib_CsLo.Color startColor, Raylib_CsLo.Color endColor)
    {
        if (duration <= 0.0f) return;
        if (ScreenEffectIntensity <= 0f) return;
        byte startColorAlpha = (byte)(startColor.a * ScreenEffectIntensity);
        startColor.a = startColorAlpha;
        byte endColorAlpha = (byte)(endColor.a * ScreenEffectIntensity);
        endColor.a = endColorAlpha;

        ShapeFlash flash = new(duration, startColor, endColor);
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
    
    private void CalculateCurScreenSize()
    {
        if (IsWindowFullscreen())
        {
            var monitor = GetCurrentMonitor();
            var mw = GetMonitorWidth(monitor);
            var mh = GetMonitorHeight(monitor);
            var scaleFactor = GetWindowScaleDPI();
            int scaleX = (int)scaleFactor.X;
            int scaleY = (int)scaleFactor.Y;
            CurScreenSize = new(mw * scaleX, mh * scaleY);
        }
        else
        {
            var w = GetScreenWidth();
            var h = GetScreenHeight();
            CurScreenSize = new(w, h);
        }
    }

    private void StartGameloop()
    {
        LoadContent();
        BeginRun();
    }
    private void RunGameloop()
    {
        while (!quit)
        {
            if (WindowShouldClose())
            {
                Quit();
                continue;
            }
            var dt = GetFrameTime();
            Delta = dt;
            UpdateMonitorDevice(dt);
            CheckWindowDimensionsChanged();
            Camera.SetSize(CurScreenSize, DevelopmentDimensions);
            Camera.Update(dt);
            gameTexture.UpdateDimensions(CurScreenSize);
            screenShaderBuffer.UpdateDimensions(CurScreenSize);
            
            var mousePosUI = GetMousePosition();
            var mousePosGame = Camera.ScreenToWorld(mousePosUI);
            var screenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
            var cameraArea = Camera.Area;

            Game = new(cameraArea, mousePosGame);
            UI = new(screenArea, mousePosUI);
            
            UpdateFlashes(dt);
            Cursor.Update(dt, UI);
            Update(dt);

            BeginTextureMode(gameTexture.RenderTexture);
            ClearBackground(new(0,0,0,0));
            
            BeginMode2D(Camera.Camera);
            DrawGame(Game);
            EndMode2D();
            
            foreach (var flash in shapeFlashes) screenArea.Draw(flash.GetColor());
            if (screenShaderAffectsUI)
            {
                DrawUI(UI);
                Cursor.Draw(UI);
            }
            EndTextureMode();

            DrawToScreen(screenArea, mousePosUI);
            // var activeScreenShaders = ScreenShaders.GetActiveShaders();
            //
            // BeginDrawing();
            // ClearBackground(BackgroundColor);
            //
            // if (activeScreenShaders.Count > 0)
            // {
            //     BeginShaderMode(activeScreenShaders[0].Shader);
            //     gameTexture.Draw();
            //     EndShaderMode();
            // }
            // else
            // {
            //     gameTexture.Draw();
            // }
            //
            // //foreach (var flash in shapeFlashes) screenArea.Draw(flash.GetColor());
            //
            // DrawUI(UI);
            // DrawCursor(screenArea.Size, mousePosUI);
            // EndDrawing();
            
            
            //CheckWindowSizeChanged();

            
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
                BeginTextureMode(target.RenderTexture);
                ClearBackground(new(0,0,0,0));
                BeginShaderMode(shader.Shader);
                source.Draw();
                EndShaderMode();
                EndTextureMode();
                temp = source;
                source = target;
                target = temp;
            }
            
            BeginDrawing();
            ClearBackground(BackgroundColor);

            BeginShaderMode(lastShader.Shader);
            target.Draw();
            EndShaderMode();

            if (!screenShaderAffectsUI)
            {
                DrawUI(UI);
                Cursor.Draw(UI);
            }
            
            EndDrawing();
            
        }
        else //single shader mode or only 1 screen shader is active
        {
            BeginDrawing();
            ClearBackground(BackgroundColor);

            if (activeScreenShaders.Count > 0)
            {
                BeginShaderMode(activeScreenShaders[0].Shader);
                gameTexture.Draw();
                EndShaderMode();
            }
            else
            {
                gameTexture.Draw();
            }
            
            if (!screenShaderAffectsUI)
            {
                DrawUI(UI);
                Cursor.Draw(UI);
            }
            EndDrawing();
            
        }
    }
    private void EndGameloop()
    {
        EndRun();
        UnloadContent();
        screenShaderBuffer.Unload();
        gameTexture.Unload();
    }
    private void UpdateMonitorDevice(float dt)
    {
        var newMonitor = Monitor.HasMonitorSetupChanged();
        if (newMonitor.Available)
        {
            MonitorChanged(newMonitor);
        }
    }
    

    #region Virtual

    /// <summary>
    /// Called first after starting the gameloop.
    /// </summary>
    protected virtual void LoadContent() { }
    /// <summary>
    /// Called after LoadContent but before the main loop has started.
    /// </summary>
    protected virtual void BeginRun() { }

    //protected virtual void HandleInput(float dt) { }
    protected virtual void Update(float dt) { }
    protected virtual void DrawGame(ScreenInfo game) { }
    protected virtual void DrawUI(ScreenInfo ui) { }

    /// <summary>
    /// Called before UnloadContent is called after the main gameloop has been exited.
    /// </summary>
    protected virtual void EndRun() { }
    /// <summary>
    /// Called after EndRun before the application terminates.
    /// </summary>
    protected virtual void UnloadContent() { }

    protected virtual void WindowSizeChanged(DimensionConversionFactors conversion) { }
    protected void UpdateScene() => CurScene.Update(Delta, Game, UI);

    protected void DrawGameScene() => CurScene.DrawGame(Game);

    protected void DrawUIScene() => CurScene.DrawUI(UI);

    #endregion
    
    public bool SetMonitor(int newMonitor)
    {
        var monitor = Monitor.SetMonitor(newMonitor);
        if (monitor.Available)
        {
            MonitorChanged(monitor);
            return true;
        }
        return false;
    }
    public void NextMonitor()
    {
        var nextMonitor = Monitor.NextMonitor();
        if (nextMonitor.Available)
        {
            MonitorChanged(nextMonitor);
        }
    }
    private void MonitorChanged(MonitorInfo monitor)
    {
        // var prevDimensions = CurScreenSize;

        // if (IsWindowFullscreen())
        // {
            // SetWindowMonitor(monitor.Index);
            // ChangeWindowDimensions(monitor.Dimensions, true);
            // SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
        // }
        // else
        // {
            // var windowDimensions = prevDimensions;
            // if (windowDimensions.Width > monitor.Width || windowDimensions.Height > monitor.Height)
            // {
                // windowDimensions = monitor.Dimensions / 2;
            // }
            // ChangeWindowDimensions(monitor.Dimensions, true);
            // SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            // SetWindowMonitor(monitor.Index);
            // ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            // ChangeWindowDimensions(windowDimensions, false);
        // }
        // if (VSync)
        // {
            // SetFPS(monitor.Refreshrate);
        // }
    }
    private void SetupWindowDimensions()
    {
        var monitor = Monitor.CurMonitor();
        WindowSize = monitor.Dimensions / 2;
        CenterWindow();
        CalculateCurScreenSize();
    }
    private void WriteDebugInfo()
    {
        Console.WriteLine("--------Shape Engine Monitor Info--------");
        if(Fullscreen)Console.WriteLine("Fullscreen is Enabled");
        else Console.WriteLine("Fullscreen is Disabled");
        
        if(IsWindowMaximized()) Console.WriteLine("Window is Maximized");
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
    private void CheckWindowDimensionsChanged()
    {
        var prev = CurScreenSize;
        CalculateCurScreenSize();
        if (prev != CurScreenSize)
        {
            //TODO Matching aspect ratio
            //if !fullscreen
            //find matching resolution for dev aspect ratio
            //set window size to it
            //center window
            //calculate cur screen size
            
            
            SetConversionFactors();
            var conversion = new DimensionConversionFactors(prev, CurScreenSize);
            OnWindowDimensionsChanged?.Invoke(conversion);
            WindowSizeChanged(conversion);
            CurScene.WindowSizeChanged(conversion);
        }
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

    private void SetConversionFactors()
    {
        ScreenToDevelopment = new(CurScreenSize, DevelopmentDimensions);
        DevelopmentToScreen = new(DevelopmentDimensions, CurScreenSize);
    }
}
    