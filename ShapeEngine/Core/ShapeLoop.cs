global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
using ShapeEngine.Screen;
using Raylib_CsLo;
using System.Numerics;
using System.Runtime.InteropServices;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace ShapeEngine.Core;

public class Gamepad
{
    public readonly int Index;
    
    public bool Available { get; private set; } = true;
    public bool Connected { get; private set; }

    public string Name { get; private set; } = "No Device";
    public int AxisCount { get; private set; } = 0;
    
    public event Action? OnConnectionChanged;
    public event Action? OnAvailabilityChanged;
    
    public Gamepad(int index, bool connected)
    {
        Index = index;
        Connected = connected;
        if (Connected)
        {
            unsafe
            {
                Name = Raylib.GetGamepadName(index)->ToString();
            }

            AxisCount = Raylib.GetGamepadAxisCount(index);
        }
        
    }

    public void Connect()
    {
        if (Connected) return;
        Connected = true;
        unsafe
        {
            Name = Raylib.GetGamepadName(Index)->ToString();
        }

        AxisCount = Raylib.GetGamepadAxisCount(Index);
        OnConnectionChanged?.Invoke();
    }
    public void Disconnect()
    {
        if (!Connected) return;
        Connected = false;
        OnConnectionChanged?.Invoke();
    }
    public bool Claim()
    {
        if (!Connected || !Available) return false;
        Available = false;
        OnAvailabilityChanged?.Invoke();
        return true;
    }
    public bool Free()
    {
        if (Available) return false;
        Available = true;
        OnAvailabilityChanged?.Invoke();
        return true;
    }
}

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
    
    public Raylib_CsLo.Color BackgroundColor = BLACK;
    public float ScreenEffectIntensity = 1.0f;

    public readonly ShaderContainer ScreenShaders = new();
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
        }
    }
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
    public InputDevice CurrentInputDevice { get; private set; } = InputDevice.Keyboard;
    
    public int MaxGamepads => gamepads.Length;
    public readonly ShapeInput Input = new();
    public Gamepad? LastUsedGamepad { get; private set; } = null;
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
    
    private readonly Gamepad[] gamepads = new Gamepad[8];
    private readonly List<int> connectedGamepadIndices = new();
    #endregion
    
    #region Setup
    public ShapeLoop(Dimensions developmentDimensions, bool multiShaderSupport = false)
    {
        #if DEBUG
        DebugMode = true;
        ReleaseMode = false;
        #endif

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

        for (var i = 0; i < gamepads.Length; i++)
        {
            gamepads[i] = new Gamepad(i, Raylib.IsGamepadAvailable(i));
        }
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
    public void ResetCamera() => Camera = basicCamera;

    public bool HasGamepad(int index) => index >= 0 && index < gamepads.Length;
    public bool IsGamepadConnected(int index) => HasGamepad(index) && gamepads[index].Connected;
    public Gamepad? GetGamepad(int index)
    {
        if (!HasGamepad(index)) return null;
        return gamepads[index];
    }
    public Gamepad? RequestGamepad(int preferredIndex = -1)
    {
        var preferredGamepad = GetGamepad(preferredIndex);
        if (preferredGamepad is { Connected: true, Available: true })
        {
            preferredGamepad.Claim();
            return preferredGamepad;
        }

        foreach (var gamepad in gamepads)
        {
            if (gamepad is { Connected: true, Available: true })
            {
                gamepad.Claim();
                return gamepad;
            }
        }
        return null;
    }
    public void ReturnGamepad(int index) => GetGamepad(index)?.Free();
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
            if (WindowShouldClose())
            {
                Quit();
                continue;
            }
            var dt = GetFrameTime();
            Delta = dt;
            
            var newMonitor = Monitor.HasMonitorChanged();
            if (newMonitor.Available)
            {
                ChangeMonitor(newMonitor);
            }
            
            CheckForWindowChanges();
            CheckGamepadConnections();
            CheckInputDevice();
            
            Camera.SetSize(CurScreenSize, DevelopmentDimensions);
            if(!Paused) Camera.Update(dt);
            
            gameTexture.UpdateDimensions(CurScreenSize);
            screenShaderBuffer.UpdateDimensions(CurScreenSize);
            var mousePos = ChangeMousePos(dt, GetMousePosition());
            Raylib.SetMousePosition((int)mousePos.X, (int)mousePos.Y);
            var mousePosUI = mousePos;// GetMousePosition();
            var mousePosGame = Camera.ScreenToWorld(mousePosUI);
            
            var screenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
            var cameraArea = Camera.Area;

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
            
            BeginTextureMode(gameTexture.RenderTexture);
            ClearBackground(new(0,0,0,0));
            
            BeginMode2D(Camera.Camera);
            ResolveDrawGame(Game);
            EndMode2D();
            
            foreach (var flash in shapeFlashes) screenArea.Draw(flash.GetColor());
            ResolveDrawGameUI(UI);
            Cursor.DrawGameUI(UI);
            EndTextureMode();
            
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

            ResolveDrawUI(UI);
            Cursor.DrawUI(UI);
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
            
            ResolveDrawUI(UI);
            Cursor.DrawUI(UI);
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
    
    protected virtual void OnInputDeviceChanged(InputDevice prevDevice, InputDevice newDevice) { }
    protected virtual void OnGamepadConnected(Gamepad gamepad) { }
    protected virtual void OnGamepadDisconnected(Gamepad gamepad) { }

    /// <summary>
    /// Override this function to manipulate the final mouse position that is used for the rest of the frame.
    /// </summary>
    /// <param name="dt">The current delta time.</param>
    /// <param name="mousePos">The raw mouse position for this frame.</param>
    /// <returns>Return the new mouse position that should be used.</returns>
    protected virtual Vector2 ChangeMousePos(float dt, Vector2 mousePos) => mousePos;
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
            SetWindowSize(monitor.Dimensions.Width, monitor.Dimensions.Height);
            SetWindowPosition((int)monitor.Position.X, (int)monitor.Position.Y);
            SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
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
            SetWindowPosition(x, y);
            SetWindowSize(windowDimensions.Width, windowDimensions.Height);
        }
        
        
        ResolveOnMonitorChanged(monitor);
    }
    #endregion

    #region Private Functions
    private void CheckInputDevice()
    {
        var prevInputDevice = CurrentInputDevice;
        if (CurrentInputDevice == InputDevice.Keyboard)
        {
            if (ShapeInput.WasMouseUsed()) CurrentInputDevice = InputDevice.Mouse;
            else
            {
                var index = ShapeInput.WasGamepadUsed(connectedGamepadIndices);
                if (index >= 0)
                {
                    CurrentInputDevice = InputDevice.Gamepad;
                    LastUsedGamepad = GetGamepad(index);
                }
            }
            //else if (ShapeInput.WasGamepadUsed(connectedGamepadIndices)) CurrentInputDevice = InputDevice.Gamepad;
        }
        else if (CurrentInputDevice == InputDevice.Mouse)
        {
            if (ShapeInput.WasKeyboardUsed()) CurrentInputDevice = InputDevice.Keyboard;
            else
            {
                var index = ShapeInput.WasGamepadUsed(connectedGamepadIndices);
                if (index >= 0)
                {
                    CurrentInputDevice = InputDevice.Gamepad;
                    LastUsedGamepad = GetGamepad(index);
                }
            }
            //else if (ShapeInput.WasGamepadUsed(connectedGamepadIndices)) CurrentInputDevice = InputDevice.Gamepad;
        }
        else //gamepad
        {
            if (ShapeInput.WasMouseUsed()) CurrentInputDevice = InputDevice.Mouse;
            else if (ShapeInput.WasKeyboardUsed()) CurrentInputDevice = InputDevice.Keyboard;
        }

        if (CurrentInputDevice != prevInputDevice)
        {
            ResolveOnInputDeviceChanged(prevInputDevice, CurrentInputDevice);
        }
    }
    private void CheckGamepadConnections()
    {
        connectedGamepadIndices.Clear();
        for (var i = 0; i < gamepads.Length; i++)
        {
            var gamepad = gamepads[i];
            if (Raylib.IsGamepadAvailable(i))
            {
                if (!gamepad.Connected)
                {
                    gamepad.Connect();
                    ResolveOnGamepadConnected(gamepad);
                }
                connectedGamepadIndices.Add(i);
            }
            else
            {
                if (gamepad.Connected)
                {
                    gamepad.Disconnect();
                    ResolveOnGamepadDisconnected(gamepad);
                }
            }
        }
        
    }

    private void SetupWindowDimensions()
    {
        var monitor = Monitor.CurMonitor();
        WindowSize = monitor.Dimensions / 2;
        prevFullscreenWindowSize = windowSize;
        //CenterWindow();
        WindowPosition = GetWindowPosition();
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
    private void SetConversionFactors()
    {
        ScreenToDevelopment = new(CurScreenSize, DevelopmentDimensions);
        DevelopmentToScreen = new(DevelopmentDimensions, CurScreenSize);
    }
    
    //TODO remove all resolve functions
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
    private void ResolveOnInputDeviceChanged(InputDevice prevDevice, InputDevice newDevice)
    {
        OnInputDeviceChanged(prevDevice, newDevice);
        CurScene.OnInputDeviceChanged(prevDevice, newDevice);
    }
    private void ResolveOnGamepadConnected(Gamepad gamepad)
    {
        OnGamepadConnected(gamepad);
        CurScene.OnGamepadConnected(gamepad);
    }
    private void ResolveOnGamepadDisconnected(Gamepad gamepad)
    {
        OnGamepadDisconnected(gamepad);
        CurScene.OnGamepadDisconnected(gamepad);
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
    #endregion
}
    