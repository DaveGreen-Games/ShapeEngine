using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Screen;

namespace ShapeEngine.Core;

//totalSeconds
//totalFrames
//delta


public readonly struct GameTime
{
    /// <summary>
    /// Seconds since start of application
    /// </summary>
    public readonly double TotalSeconds;
    /// <summary>
    /// Frames since start of application
    /// </summary>
    public readonly int TotalFrames;
    /// <summary>
    /// Seconds since last frame
    /// </summary>
    public readonly double ElapsedSeconds;
    
    public GameTime(double totalSeconds, int totalFrames, double elapsedSeconds)
    {
        this.TotalSeconds = totalSeconds;
        this.TotalFrames = totalFrames;
        this.ElapsedSeconds = elapsedSeconds;
    }

    #region Tick
    public GameTime Tick(double dt) => new(TotalSeconds + dt, TotalFrames + 1, dt);
    public GameTime TickF(float dt) => new(TotalSeconds + dt, TotalFrames + 1, dt);
    #endregion
    
    #region Conversion
    public readonly double TotalDays => TotalSeconds / 86400;
    public readonly double TotalHours => TotalSeconds / 3600;
    public readonly double TotalMinutes => TotalSeconds / 60;
    
    /// <summary>
    /// 1/1000 of a seconds (1000 (1 Thousand) milli seconds = 1 second)
    /// </summary>
    public readonly double TotalMilliSeconds => TotalSeconds * 1000;
    /// <summary>
    /// 1/1000000 of a seconds (1.000.000 (1 Million) micro seconds = 1 second)
    /// </summary>
    public readonly double TotalMicroSeconds => TotalSeconds * 1000000;
    /// <summary>
    /// 1/1000000000 of a seconds (1.000.000.000 (1 Billion) nano seconds = 1 second)
    /// </summary>
    public readonly double TotalNanoSeconds => TotalSeconds * 1000000000;

    /// <summary>
    /// Elapsed seconds in float instead of double.
    /// </summary>
    public readonly float Delta => (float)ElapsedSeconds;
    public readonly int Fps => ElapsedSeconds <= 0f ? 0 : (int)(1f / ElapsedSeconds);
    /// <summary>
    /// 1/1000 of a seconds (1000 (1 Thousand) milli seconds = 1 second)
    /// </summary>
    public readonly double ElapsedMilliSeconds => ElapsedSeconds * 1000;
    /// <summary>
    /// 1/1000000 of a seconds (1.000.000 (1 Million) micro seconds = 1 second)
    /// </summary>
    public readonly double ElapsedMicroSeconds => ElapsedSeconds * 1000000;
    /// <summary>
    /// 1/1000000000 of a seconds (1.000.000.000 (1 Billion) nano seconds = 1 second)
    /// </summary>
    public readonly double ElapsedNanoSeconds => ElapsedSeconds * 1000000000;
    #endregion

    #region Static
    public static double ToMinutes(double seconds) => seconds / 60;
    public static double ToHours(double seconds) => seconds / 3600;
    public static double ToDays(double seconds) => seconds / 86400;
    
    /// <summary>
    /// 1/1000 of a seconds (1000 (1 Thousand) milli seconds = 1 second)
    /// </summary>
    public static double ToMilliSeconds(double seconds) => seconds / 1000;
    /// <summary>
    /// 1/1000000 of a seconds (1.000.000 (1 Million) micro seconds = 1 second)
    /// </summary>
    public static double ToMicroSeconds(double seconds) => seconds / 1000000;
    /// <summary>
    /// 1/1000000000 of a seconds (1.000.000.000 (1 Billion) nano seconds = 1 second)
    /// </summary>
    public static double ToNanoSeconds(double seconds) => seconds / 1000000000;
    #endregion
    
    
    
    // public readonly float Centuries = Decades / 10f;
    // public readonly float Decades = Years / 10f;
    // public readonly float Years = Months / 12f;
    // public readonly float Months => seconds / 2419200;
    // public readonly float Weeks => seconds / 604800;
}


public class Game
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
            curCamera.SetSize(Window.CurScreenSize, DevelopmentDimensions);
        }
    }
    public ScreenInfo GameScreenInfo { get; private set; } = new();
    public ScreenInfo UIScreenInfo { get; private set; } = new();
    public float Delta { get; private set; } = 0f;
    // public float DeltaSlow { get; private set; } = 0f;
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
    //public SlowMotion SlowMotion { get; private set; } = new SlowMotion();

    /// <summary>
    /// Scaling factors from current screen size to development resolution.
    /// </summary>
    public DimensionConversionFactors ScreenToDevelopment { get; private set; }
    /// <summary>
    /// Scaling factors from development resolution to the current screen size.
    /// </summary>
    public DimensionConversionFactors DevelopmentToScreen { get; private set; }
    public Dimensions DevelopmentDimensions { get; private set; }
    
    public GameWindow Window { get; private set; }
    public IScene CurScene { get; private set; } = new SceneEmpty();
    #endregion
    
    #region Private Members

    private readonly ScreenTexture gameTexture = new();
    private readonly ScreenTexture screenShaderBuffer = new();
    private readonly ShapeCamera basicCamera = new ShapeCamera();
    private ShapeCamera curCamera;
    
    private bool quit = false;
    private bool restart = false;
    
    private List<ShapeFlash> shapeFlashes = new();
    private List<DeferredInfo> deferred = new();
    
    private Vector2 lastControlledMousePosition = new();
    private bool mouseControlled = false;
    #endregion

    public Game(GameSettings gameSettings, WindowSettings windowSettings)
    {
        #if DEBUG
        DebugMode = true;
        ReleaseMode = false;
        #endif

        this.DevelopmentDimensions = gameSettings.DevelopmentDimensions;
        Window = new(windowSettings);
        Window.OnWindowSizeChanged += ResolveOnWindowSizeChanged;
        Window.OnWindowPositionChanged += ResolveOnWindowPositionChanged;
        Window.OnMonitorChanged += ResolveOnMonitorChanged;
        Window.OnMouseVisibilityChanged += ResolveOnMouseVisibilityChanged;
        Window.OnMouseEnabledChanged += ResolveOnMouseEnabledChanged;
        Window.OnMouseEnteredScreen += ResolveOnMouseEnteredScreen;
        Window.OnMouseLeftScreen += ResolveOnMouseLeftScreen;
        
        
        Window.OnWindowFocusChanged += ResolveOnWindowFocusChanged;
        Window.OnWindowFullscreenChanged += ResolveOnWindowFullscreenChanged;
        Window.OnWindowMaximizeChanged += ResolveOnWindowMaximizeChanged;
        Window.OnWindowMinimizedChanged += ResolveOnWindowMinimizedChanged;
        Window.OnWindowHiddenChanged += ResolveOnWindowHiddenChanged;
        Window.OnWindowTopmostChanged += ResolveOnWindowTopmostChanged;

        SetConversionFactors();
        

        curCamera = basicCamera;
        Camera.Activate();
        Camera.SetSize(Window.CurScreenSize, DevelopmentDimensions);
        
        var mousePosUI = Window.MousePosition;
        var mousePosGame = Camera.ScreenToWorld(mousePosUI);
        var cameraArea = Camera.Area;

        GameScreenInfo = new(cameraArea, mousePosGame);
        UIScreenInfo = new(Window.ScreenArea, mousePosUI);

        gameTexture.Load(Window.CurScreenSize);
        if (gameSettings.MultiShaderSupport) screenShaderBuffer.Load(Window.CurScreenSize);

        ShapeInput.OnInputDeviceChanged += OnInputDeviceChanged;
        ShapeInput.GamepadDeviceManager.OnGamepadConnectionChanged += OnGamepadConnectionChanged;
        
    }
    public ExitCode Run(params string[] launchParameters)
    {
        this.LaunchParams = launchParameters;

        quit = false;
        restart = false;
        Raylib.SetExitKey(KeyboardKey.Null);

        StartGameloop();
        RunGameloop();
        EndGameloop();
        Raylib.CloseWindow();

        return new ExitCode(restart);
    }
 
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

            
            
            Window.Update(dt);
            
            
            ShapeInput.Update();
            Camera.SetSize(Window.CurScreenSize, DevelopmentDimensions);
            if(!Paused) Camera.Update(dt);

            if (Window.MouseOnScreen)
            {
                if (ShapeInput.CurrentInputDeviceType is InputDeviceType.Keyboard or InputDeviceType.Gamepad)
                {
                    Window.MoveMouse(ChangeMousePos(dt, Window.MousePosition, Window.ScreenArea));
                }
            }
            
            
            gameTexture.UpdateDimensions(Window.CurScreenSize);
            screenShaderBuffer.UpdateDimensions(Window.CurScreenSize);
            
            var cameraArea = Camera.Area;

            var mousePosUI = Window.MousePosition; //mousePos;// GetMousePosition();
            var mousePosGame = Camera.ScreenToWorld(mousePosUI);
            
            GameScreenInfo = new(cameraArea, mousePosGame);
            UIScreenInfo = new(Window.ScreenArea, mousePosUI);

            
            if (!Paused)
            {
                // SlowMotion.Update(dt);
                UpdateFlashes(dt);
            }
            // var defaultFactor = SlowMotion.GetFactor(SlowMotion.TagDefault);
            // DeltaSlow = Delta * defaultFactor;
            Window.Cursor.Update(dt, UIScreenInfo);
            
            ResolveUpdate(dt, dt, GameScreenInfo, UIScreenInfo);
            
            Raylib.BeginTextureMode(gameTexture.RenderTexture);
            Raylib.ClearBackground(new(0,0,0,0));

            Raylib.BeginMode2D(Camera.Camera);
            ResolveDrawGame(GameScreenInfo);
            Raylib.EndMode2D();

            ResolveDrawGameUI(UIScreenInfo);
            if(Window.MouseOnScreen) Window.Cursor.DrawGameUI(UIScreenInfo);
            
            Raylib.EndTextureMode();
            
            DrawToScreen(Window.ScreenArea, mousePosUI);

            ResolveDeferred();
        }
    }

    
    
    private void DrawToScreen(Rect screenArea, Vector2 mousePosUI)
    {
        // Stopwatch watch = new();
        // watch.Restart();
        // Console.WriteLine($"Draw screen {watch.ElapsedMilliseconds}ms");
        var activeScreenShaders = ScreenShaders.GetActiveShaders();
        
        //multi shader support enabled and multiple screen shaders are active
        if (activeScreenShaders.Count > 1 && screenShaderBuffer.Loaded)
        {
            int lastIndex = activeScreenShaders.Count - 1;
            ShapeShader lastShader = activeScreenShaders[lastIndex];
            activeScreenShaders.RemoveAt(lastIndex);
            
            ScreenTexture source = gameTexture;
            ScreenTexture target = screenShaderBuffer;
            ScreenTexture temp;
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

            ResolveDrawUI(UIScreenInfo);
            if(Window.MouseOnScreen) Window.Cursor.DrawUI(UIScreenInfo);
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
            
            ResolveDrawUI(UIScreenInfo);
            if(Window.MouseOnScreen) Window.Cursor.DrawUI(UIScreenInfo);
            
            Raylib.EndDrawing();
            
        }
    }
    private void EndGameloop()
    {
        EndRun();
        UnloadContent();
        Window.Close();
        screenShaderBuffer.Unload();
        gameTexture.Unload();
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
    
    
    public void ResetCamera() => Camera = basicCamera;

    
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
    protected virtual void OnMouseEnteredScreen() { }
    protected virtual void OnMouseLeftScreen() { }
    protected virtual void OnMouseVisibilityChanged(bool visible) { }
    protected virtual void OnMouseEnabledChanged(bool enabled) { }
    protected virtual void OnWindowFocusChanged(bool focused) { }
    protected virtual void OnWindowFullscreenChanged(bool fullscreen) { }
    protected virtual void OnWindowMaximizeChanged(bool maximized) { }
    protected virtual void OnWindowMinimizedChanged(bool minimized) { }
    protected virtual void OnWindowHiddenChanged(bool hidden) { }
    protected virtual void OnWindowTopmostChanged(bool topmost) { }
    protected virtual Vector2 ChangeMousePos(float dt, Vector2 mousePos, Rect screenArea) => mousePos;
    #endregion

    #region Resolve
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
        CurScene.Update(dt, deltaSlow, GameScreenInfo, UIScreenInfo);
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
    private void ResolveOnMouseEnteredScreen()
    {
        OnMouseEnteredScreen();
        CurScene.OnMouseEnteredScreen();
    }
    private void ResolveOnMouseLeftScreen()
    {
        OnMouseLeftScreen();
        CurScene.OnMouseLeftScreen();
    }
    private void ResolveOnMouseVisibilityChanged(bool visible)
    {
        OnMouseVisibilityChanged(visible);
        CurScene.OnMouseVisibilityChanged(visible);
    }
    private void ResolveOnMouseEnabledChanged(bool enabled)
    {
        OnMouseEnabledChanged(enabled);
        CurScene.OnMouseEnabledChanged(enabled);
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
    
    private void ResolveOnWindowMinimizedChanged(bool minimized)
    {
       OnWindowMinimizedChanged(minimized);
    }
    private void ResolveOnWindowHiddenChanged(bool hidden)
    {
        OnWindowHiddenChanged(hidden);
        
    }
    private void ResolveOnWindowTopmostChanged(bool topmost)
    {
        OnWindowTopmostChanged(topmost);
    }

    #endregion
    
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
    private void SetConversionFactors()
    {
        ScreenToDevelopment = new(Window.CurScreenSize, DevelopmentDimensions);
        DevelopmentToScreen = new(DevelopmentDimensions, Window.CurScreenSize);
    }
}