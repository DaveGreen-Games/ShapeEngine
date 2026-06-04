using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Screen;

namespace ShapeEngine.Core;

//ISSUE: Pixelation mode is not working correctly on windows high dpi monitor with high dpi enabled in exclusive fullscreen mode.
// - so exclusive fullscreen looks different than borderless fullscreen on a 4k monitor on windows for instance
// - borderless fullscreen, exclusive fullscreen on a 1080p and borderless fullscreen on a 4k monitor in windows look the same but exclusive fullscreen on 4k monitor looks different


/// <summary>
/// Manages the main application window, including its state, size, position, monitor, framerate, and mouse/cursor behavior.
/// Provides events for window and mouse state changes.
/// </summary>
public sealed class GameWindow
{
    #region Structs
    /// <summary>
    /// Represents the configuration flags for the window, reflecting various window states.
    /// </summary>
    private readonly struct WindowConfigFlags
    {
        /// <summary>
        /// Indicates whether the window is minimized.
        /// </summary>
        public readonly bool Minimized;

        /// <summary>
        /// Indicates whether the window is maximized.
        /// </summary>
        public readonly bool Maximized;

        /// <summary>
        /// Indicates whether the window is in fullscreen mode.
        /// </summary>
        public readonly bool Fullscreen;

        /// <summary>
        /// Indicates whether the window is hidden.
        /// </summary>
        public readonly bool Hidden;

        /// <summary>
        /// Indicates whether the window is focused.
        /// </summary>
        public readonly bool Focused;

        /// <summary>
        /// Indicates whether the window is undecorated (no border).
        /// </summary>
        public readonly bool Undecorated;

        /// <summary>
        /// Indicates whether the window is resizable.
        /// </summary>
        public readonly bool Resizable;

        /// <summary>
        /// Indicates whether the window is set as topmost.
        /// </summary>
        public readonly bool Topmost;

        /// <summary>
        /// Indicates whether the window is set to always run.
        /// </summary>
        public readonly bool AlwaysRun;

        /// <summary>
        /// Indicates whether the window allows mouse pass-through.
        /// </summary>
        public readonly bool MousePassThrough;
        
        /// <summary>
        /// Gets the current window configuration flags from Raylib.
        /// </summary>
        public static WindowConfigFlags Get() => new();
        /// <summary>
        /// Returns a <see cref="WindowConfigFlags"/> with all flags set to false.
        /// </summary>
        public static WindowConfigFlags GetAllFalse() => new WindowConfigFlags(false);
        /// <summary>
        /// Returns a <see cref="WindowConfigFlags"/> with all flags set to true.
        /// </summary>
        public static WindowConfigFlags GetAllTrue() => new WindowConfigFlags(true);

        /// <summary>
        /// Sets all flags to the specified value.
        /// </summary>
        /// <param name="value">The value to set all flags to.</param>
        public WindowConfigFlags(bool value)
        {
            Undecorated = value;
            Resizable = value;
            Topmost = value;
            Focused = value;
            AlwaysRun = value;
            MousePassThrough = value;
            Minimized = value;
            Maximized = value;
            Fullscreen = value;
            Hidden = value;
        }

        /// <summary>
        /// Gets the current values from Raylib.
        /// </summary>
        public WindowConfigFlags()
        {
            Undecorated = Raylib.IsWindowState(ConfigFlags.UndecoratedWindow);
            Resizable = Raylib.IsWindowState(ConfigFlags.ResizableWindow);
            Topmost = Raylib.IsWindowState(ConfigFlags.TopmostWindow);
            Focused = !Raylib.IsWindowState(ConfigFlags.UnfocusedWindow);
            AlwaysRun = Raylib.IsWindowState(ConfigFlags.AlwaysRunWindow);
            MousePassThrough = Raylib.IsWindowState(ConfigFlags.MousePassthroughWindow);
            Minimized = Raylib.IsWindowState(ConfigFlags.MinimizedWindow);
            Maximized = Raylib.IsWindowState(ConfigFlags.MaximizedWindow);
            Fullscreen = Raylib.IsWindowFullscreen();
            Hidden = Raylib.IsWindowState(ConfigFlags.HiddenWindow);
        }

        /// <summary>Checks if the Undecorated flag has changed compared to another instance.</summary>
        public bool HasUndecoratedChanged(WindowConfigFlags other) => Undecorated != other.Undecorated;
        /// <summary>Checks if the Resizable flag has changed compared to another instance.</summary>
        public bool HasResizableChanged(WindowConfigFlags other) => Resizable != other.Resizable;
        /// <summary>Checks if the Topmost flag has changed compared to another instance.</summary>
        public bool HasTopmostChanged(WindowConfigFlags other) => Topmost != other.Topmost;
        /// <summary>Checks if the Focused flag has changed compared to another instance.</summary>
        public bool HasFocusedChanged(WindowConfigFlags other) => Focused != other.Focused;
        /// <summary>Checks if the AlwaysRun flag has changed compared to another instance.</summary>
        public bool HasAlwaysRunChanged(WindowConfigFlags other) => AlwaysRun != other.AlwaysRun;
        /// <summary>Checks if the MousePassThrough flag has changed compared to another instance.</summary>
        public bool HasMousePassThroughChanged(WindowConfigFlags other) => MousePassThrough != other.MousePassThrough;
        /// <summary>Checks if the Minimized flag has changed compared to another instance.</summary>
        public bool HasMinimizedChanged(WindowConfigFlags other) => Minimized != other.Minimized;
        /// <summary>Checks if the Maximized flag has changed compared to another instance.</summary>
        public bool HasMaximizedChanged(WindowConfigFlags other) => Maximized != other.Maximized;
        /// <summary>Checks if the Fullscreen flag has changed compared to another instance.</summary>
        public bool HasFullscreenChanged(WindowConfigFlags other) => Fullscreen != other.Fullscreen;
        /// <summary>Checks if the Hidden flag has changed compared to another instance.</summary>
        public bool HasHiddenChanged(WindowConfigFlags other) => Hidden != other.Hidden;
    }

    /// <summary>
    /// Represents the state of the mouse cursor (visibility, enabled, on screen).
    /// This is the single source of truth for all cursor state.
    /// </summary>
    private readonly struct CursorState
    {
        /// <summary>
        /// Whether the cursor is currently visible in Raylib.
        /// </summary>
        public readonly bool Visible;
        
        /// <summary>
        /// Whether the cursor is currently enabled in Raylib.
        /// </summary>
        public readonly bool Enabled;
        
        /// <summary>
        /// Whether the cursor is currently on screen.
        /// </summary>
        public readonly bool OnScreen;
        
        /// <summary>
        /// What the user wants the cursor visibility to be (may differ from Visible when off-screen).
        /// </summary>
        public readonly bool DesiredVisible;
        
        /// <summary>
        /// What the user wants the cursor enabled state to be (may differ from Enabled when off-screen).
        /// </summary>
        public readonly bool DesiredEnabled;

        /// <summary>
        /// Initializes a new instance of <see cref="CursorState"/> with all states set to true.
        /// </summary>
        public CursorState()
        {
            Visible = true;
            Enabled = true;
            OnScreen = true;
            DesiredVisible = true;
            DesiredEnabled = true;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CursorState"/> with specified states.
        /// </summary>
        public CursorState(bool visible, bool enabled, bool onScreen, bool desiredVisible, bool desiredEnabled)
        {
            Visible = visible;
            Enabled = enabled;
            OnScreen = onScreen;
            DesiredVisible = desiredVisible;
            DesiredEnabled = desiredEnabled;
        }
        
        /// <summary>
        /// Creates a new CursorState with updated desired visibility.
        /// </summary>
        public CursorState WithDesiredVisible(bool desiredVisible)
        {
            return new CursorState(Visible, Enabled, OnScreen, desiredVisible, DesiredEnabled);
        }
        
        /// <summary>
        /// Creates a new CursorState with updated desired enabled state.
        /// </summary>
        public CursorState WithDesiredEnabled(bool desiredEnabled)
        {
            return new CursorState(Visible, Enabled, OnScreen, DesiredVisible, desiredEnabled);
        }
        
        /// <summary>
        /// Creates a new CursorState with updated actual visibility.
        /// </summary>
        public CursorState WithVisible(bool visible)
        {
            return new CursorState(visible, Enabled, OnScreen, DesiredVisible, DesiredEnabled);
        }
        
        /// <summary>
        /// Creates a new CursorState with updated actual enabled state.
        /// </summary>
        public CursorState WithEnabled(bool enabled)
        {
            return new CursorState(Visible, enabled, OnScreen, DesiredVisible, DesiredEnabled);
        }
        
        /// <summary>
        /// Creates a new CursorState with updated on-screen state.
        /// </summary>
        public CursorState WithOnScreen(bool onScreen)
        {
            return new CursorState(Visible, Enabled, onScreen, DesiredVisible, DesiredEnabled);
        }
    }
    #endregion

    #region Events

    /// <summary>
    /// Occurs when the mouse leaves the window screen area.
    /// </summary>
    public event Action? OnMouseLeftScreen;
    /// <summary>
    /// Occurs when the mouse enters the window screen area.
    /// </summary>
    public event Action? OnMouseEnteredScreen;
    /// <summary>
    /// Occurs when the mouse visibility changes.
    /// </summary>
    public event Action<bool>? OnMouseVisibilityChanged;
    /// <summary>
    /// Occurs when the mouse enabled state changes.
    /// </summary>
    public event Action<bool>? OnMouseEnabledChanged;

    /// <summary>
    /// Occurs when the window size changes.
    /// </summary>
    public event Action<DimensionConversionFactors>? OnWindowSizeChanged;
    /// <summary>
    /// Occurs when the window position changes.
    /// </summary>
    public event Action<Vector2, Vector2>? OnWindowPositionChanged;
    /// <summary>
    /// Occurs when the monitor changes.
    /// </summary>
    public event Action<MonitorInfo>? OnMonitorChanged;

    /// <summary>
    /// Occurs when the window focus changes.
    /// </summary>
    public event Action<bool>? OnWindowFocusChanged;
    /// <summary>
    /// Occurs when the window fullscreen state changes.
    /// </summary>
    public event Action<bool>? OnWindowFullscreenChanged;
    /// <summary>
    /// Occurs when the window maximize state changes.
    /// </summary>
    public event Action<bool>? OnWindowMaximizeChanged;
    /// <summary>
    /// Occurs when the window minimized state changes.
    /// </summary>
    public event Action<bool>? OnWindowMinimizedChanged;
    /// <summary>
    /// Occurs when the window hidden state changes.
    /// </summary>
    public event Action<bool>? OnWindowHiddenChanged;
    /// <summary>
    /// Occurs when the window topmost state changes.
    /// </summary>
    public event Action<bool>? OnWindowTopmostChanged;

    /// <summary>
    /// Occurs when the window undecorated state changes.
    /// </summary>
    public event Action<bool>? OnWindowUndecoratedChanged;
    /// <summary>
    /// Occurs when the window resizable state changes.
    /// </summary>
    public event Action<bool>? OnWindowResizableChanged;
    /// <summary>
    /// Occurs when the window always-run state changes.
    /// </summary>
    public event Action<bool>? OnWindowAlwaysRunChanged;
    /// <summary>
    /// Occurs when the window mouse pass-through state changes.
    /// </summary>
    public event Action<bool>? OnWindowMousePassThroughChanged;
    /// <summary>
    /// Occurs when the window VSync state changes.
    /// </summary>
    public event Action<VsyncMode>? OnWindowVSyncChanged;

    #endregion

    #region Static Members

    /// <summary>
    /// Gets the singleton instance of the <see cref="GameWindow"/>.
    /// </summary>
    public static GameWindow Instance { get; private set; } = null!;

    #endregion

    #region Public Members

    /// <summary>
    /// Gets the conversion factors from screen to monitor coordinates.
    /// </summary>
    public DimensionConversionFactors ScreenToMonitor { get; private set; }
  
    /// <summary>
    /// Gets the conversion factors from monitor to screen coordinates.
    /// </summary>
    public DimensionConversionFactors MonitorToScreen { get; private set; }
  
    /// <summary>
    /// Gets the monitor device associated with the window.
    /// </summary>
    public MonitorDevice Monitor { get; private set; }
    
    /// <summary>
    /// Gets the current screen size of the window.
    /// </summary>
    public Dimensions CurScreenSize { get; private set; }
    
    /// <summary>
    /// Gets the minimum allowed window size.
    /// </summary>
    public Dimensions WindowMinSize { get; private set; }
    
    /// <summary>
    /// Gets or sets the window size.
    /// </summary>
    public Dimensions WindowSize
    {
        get => windowSize;
        set
        {
            var maxSize = GetCurrentMonitorDimensions(); // Monitor.CurMonitor().Dimensions;
            int w = value.Width;
            if (w < WindowMinSize.Width) w = WindowMinSize.Width;
            else if (w > maxSize.Width) w = maxSize.Width;
            int h = value.Height;
            if (h < WindowMinSize.Height) h = WindowMinSize.Height;
            else if (h > maxSize.Height) h = maxSize.Height;

            windowSize = new(w, h);

            if (DisplayState == WindowDisplayState.Fullscreen)
            {
                ResetMousePosition();
                return;
            }
            Raylib.SetWindowSize(windowSize.Width, windowSize.Height);
            CenterWindow();

            //CheckForWindowChanges();
        }
    }
    
    /// <summary>
    /// Gets the current window position on the screen.
    /// </summary>
    public Vector2 WindowPosition { get; private set; }

    /// <summary>
    /// Gets the current display state of the window.
    /// </summary>
    public WindowDisplayState DisplayState { get; private set; }
    
    /// <summary>
    /// Gets the current window border style.
    /// </summary>
    public WindowBorder WindowBorder { get; private set; }

    /// <summary>
    /// If enabled the FPS limiter will dynamically adjust the frame rate limit based on performance.
    /// If an <see cref="FpsLimit"/> is set (whether through <see cref="VSync"/> with a valid monitor refresh rate or manually),
    /// the adaptive limiter will try to keep the frame rate close to that limit and reduce the limit if performance drops.
    /// If no <see cref="FpsLimit"/> is set (0 = unlimited), the adaptive limiter will try to keep the frame rate between its configured minimum and maximum limits,
    /// trying to reach the maximum limit when possible and never going below the minimum limit.
    /// </summary>
    /// <remarks>
    /// Can be enabled or disabled through <see cref="AdaptiveFpsLimiter.Enabled"/> at any time.
    /// </remarks>
    public AdaptiveFpsLimiter AdaptiveFpsLimiter { get; private set; }
    
    /// <summary>
    /// Gets the minimum frame rate the window should attempt to maintain when applying frame rate limiting.
    /// A value of 0 indicates that no explicit minimum frame rate constraint is enforced.
    /// </summary>
    public int MinFrameRate { get; private set; }
  
    /// <summary>
    /// Gets the maximum frame rate the window should not exceed when applying frame rate limiting.
    /// A value of 0 indicates that no explicit maximum frame rate constraint is enforced.
    /// </summary>
    public int MaxFrameRate { get; private set; }
    
    /// <summary>
    /// Gets or sets the frames-per-second limit used when VSync is disabled. 0 means unlimited.
    /// <see cref="VSync"/> has to be <see cref="VsyncMode.Disabled"/> to allow an unlimited frame rate.
    /// </summary>
    /// <remarks>
    /// When VSync is disabled, assigning to this property will update <see cref="TargetFps"/> accordingly.
    /// Otherwise, changing this property has no immediate effect until VSync is set to <see cref="VsyncMode.Disabled"/>. 
    /// </remarks>
    public int FpsLimit
    {
        get => fpsLimit;
        set
        {
            fpsLimit = value < 0 ? 0 : value;

            if (fpsLimit > 0)
            {
                if(MinFrameRate > 0 && fpsLimit < MinFrameRate) fpsLimit = MinFrameRate;
                if(MaxFrameRate > 0 && fpsLimit > MaxFrameRate) fpsLimit = MaxFrameRate;
            }
            else
            {
                fpsLimit = MaxFrameRate > 0 ? MaxFrameRate : 0;
            }
            
            if(VSync == VsyncMode.Disabled) TargetFps = fpsLimit;
        }
    }
    
    /// <summary>
    /// Gets the current target frames-per-second used by the engine.
    /// </summary>
    /// <remarks>
    /// This property reflects the effective FPS cap whether driven by VSync (monitor refresh)
    /// or by the manual <see cref="FpsLimit"/> when VSync is disabled.
    /// </remarks>
    public int TargetFps
    {
        get => targetFps;
        private set => targetFps = value;
    }

    /// <summary>
    /// Determines whether the unfocused FPS limit is currently active.
    /// </summary>
    /// <remarks>
    /// The unfocused target FPS limit is considered active when <see cref="UnfocusedFrameRateLimit"/>
    /// is greater than zero and the cached window focus state indicates the window is not focused.
    /// This method does not query focus from the OS directly;
    /// it relies on the last-polled window configuration flags.
    /// </remarks>
    /// <returns>
    /// True if an unfocused FPS limit is set and the window is not focused; otherwise false.
    /// </returns>
    internal bool IsUnfocusedFrameRateLimitActive()
    {
        return UnfocusedFrameRateLimit > 0 && !windowConfigFlags.Focused;
    }
  
    /// <summary>
    /// Target frames-per-second to apply when the window is unfocused (in FPS).
    /// A value of 0 disables the unfocused frame rate limit (no restriction).
    /// Used by <see cref="IsUnfocusedFrameRateLimitActive"/> to decide if the unfocused cap is active.
    /// </summary>
    /// <remarks>
    /// If <see cref="GameDef.Game.IdleFrameRateLimit"/> is active as well, the lower of the two limits will be used.
    /// </remarks>
    public int UnfocusedFrameRateLimit;
    
    /// <summary>
    /// Gets or sets the current vertical sync mode for the window.
    /// Changing this property updates the effective <see cref="TargetFps"/> according to the selected <see cref="VsyncMode"/>.
    /// Setting to <see cref="VsyncMode.Disabled"/> causes the engine to use the manual <see cref="FpsLimit"/> for frame limiting.
    /// </summary>
    public VsyncMode VSync
    {
        get => vsync;
        set
        {
            if (vsync == value) return;
            
            int newLimit = ComputeTargetFpsFromMode(value);
            if (newLimit <= 0)
            {
                TargetFps = fpsLimit;
                vsync = VsyncMode.Disabled;
            }
            else
            {
                TargetFps = newLimit;
                vsync = value;
            }
            
            OnWindowVSyncChanged?.Invoke(vsync);
        }
    }
    
    /// <summary>
    /// Computes the effective target frames-per-second for the provided <see cref="VsyncMode"/>.
    /// Returns 0 when VSync is disabled or when the monitor refresh rate is unknown/invalid.
    /// </summary>
    /// <param name="mode">The VSync mode to compute the target FPS for.</param>
    /// <returns>The calculated target FPS based on the current monitor refresh rate and the requested mode.</returns>
    private int ComputeTargetFpsFromMode(VsyncMode mode)
    {
        if(mode == VsyncMode.Disabled) return 0;
        
        int refresh = Monitor.CurMonitor().Refreshrate;
        if(refresh <= 0) return 0;

        int value = 0;
        switch (mode)
        {
            case VsyncMode.Half: value = Math.Max(30, refresh / 2); break;
            case VsyncMode.Normal: value = refresh; break;
            case VsyncMode.Double: value = refresh * 2; break;
            case VsyncMode.Quadruple: value = refresh * 4; break;
        }
        if(MinFrameRate > 0 && value < MinFrameRate) value = MinFrameRate;
        if(MaxFrameRate > 0 && value > MaxFrameRate) value = MaxFrameRate;
        return value;
    }
    
    /// <summary>
    /// Gets whether the mouse is currently on the window screen.
    /// </summary>
    public bool MouseOnScreen
    {
        get => mouseOnScreen;
        private set
        {
            mouseOnScreen = value;
        }
    }
  
    private bool mouseOnScreen;
    
    /// <summary>
    /// Gets the area of the screen as a rectangle.
    /// </summary>
    public Rect ScreenArea { get; private set; }

    /// <summary>
    /// Gets or sets whether the window should automatically restore from fullscreen when focus is lost.
    /// This only affects fullscreen mode and does not affect borderless fullscreen!
    /// </summary>
    public bool FullscreenAutoRestoring { get; set; }
    
    // public bool BorderlessFullscreenAutoRestoring { get; set; }

    public readonly bool HighDpi;
    #endregion

    #region Private Members

    private int fpsLimit;
    private VsyncMode vsync;
    private int targetFps;
    private Dimensions windowSize = new();

    // CursorState is the single source of truth for all cursor/mouse state.
    // This struct contains both desired state (what user wants) and actual state (what's applied to Raylib).
    // When mouse is on screen: desired state is immediately applied to actual state
    // When mouse leaves screen: actual state is forced visible/enabled (OS requirement)
    // When mouse returns: actual state is restored to desired state
    private CursorState cursorState = new();
    private CursorState previousCursorState = new();
    
    private WindowConfigFlags windowConfigFlags;
    private bool wasMaximized;

    private Dimensions prevDisplayStateWindowDimensions = new(128, 128);
    private Vector2 prevDisplayStateWindowPosition = new(128, 128);
    
    private bool fullscreenAutoRestoringActive;
    private bool fullscreenAutoRestoringWindowWasTopmost;
    
    // private bool borderlessFullscreenAutoRestoringActive;
    // private bool borderlessFullscreenAutoRestoringWindowWasTopmost;
    // private bool borderlessFullscreenAutoRestoringActiveCooldown;
    
    #endregion

    #region Internal Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="GameWindow"/> class with the specified settings.
    /// </summary>
    /// <param name="windowSettings">The window settings to use.</param>
    /// <param name="framerateSettings">The framerate settings to use.</param>
    internal GameWindow(WindowSettings windowSettings, FramerateSettings framerateSettings)
    {
        if(windowSettings.Msaa4x) Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
        if(windowSettings.HighDPI) Raylib.SetConfigFlags(ConfigFlags.HighDpiWindow);
        if(windowSettings.FramebufferTransparent) Raylib.SetConfigFlags(ConfigFlags.TransparentWindow);
        
        HighDpi = windowSettings.HighDPI;
        
        Raylib.InitWindow(windowSettings.WindowSize.Width, windowSettings.WindowSize.Height, windowSettings.Title);
        Raylib.SetWindowOpacity(0f);

        Monitor = new MonitorDevice();
        SetupWindowDimensions();
        WindowMinSize = windowSettings.WindowMinSize;
        Raylib.SetWindowMinSize(WindowMinSize.Width, WindowMinSize.Height);

        Raylib.SetWindowState(ConfigFlags.AlwaysRunWindow);

        if (windowSettings.Topmost) Raylib.SetWindowState(ConfigFlags.TopmostWindow);

        FullscreenAutoRestoring = windowSettings.FullscreenAutoRestoring;
        // BorderlessFullscreenAutoRestoring = true;
        
        //Setup frame rate variables and vsync directly bypassing getters and setters to avoid logic errors on startup.
        vsync = windowSettings.Vsync;
        MinFrameRate = framerateSettings.MinFrameRate;
        MaxFrameRate = framerateSettings.MaxFrameRate;
        AdaptiveFpsLimiter = new(framerateSettings.AdaptiveFpsLimiterSettings, framerateSettings.MinFrameRate, framerateSettings.MaxFrameRate);
        fpsLimit = framerateSettings.FrameRateLimit;
        UnfocusedFrameRateLimit = framerateSettings.UnfocusedFrameRateLimit;
        
        int newLimit = ComputeTargetFpsFromMode(vsync);
        if (newLimit <= 0)
        {
            TargetFps = fpsLimit;
            vsync = VsyncMode.Disabled;
        }
        else
        {
            TargetFps = newLimit;
        }

        switch (windowSettings.WindowBorder)
        {
            case WindowBorder.Resizabled: 
                Raylib.SetWindowState(ConfigFlags.ResizableWindow); 
                break;
            case WindowBorder.Fixed: 
                Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
                Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
                break;
            case WindowBorder.Undecorated: Raylib.SetWindowState(ConfigFlags.UndecoratedWindow); break;
        }
        DisplayState = WindowDisplayState.Normal;

        if (windowSettings.WindowSize.Width > 0 && windowSettings.WindowSize.Height > 0)
        {
            WindowSize = windowSettings.WindowSize;
        }

        if (Monitor.CurMonitor().Index != windowSettings.Monitor && windowSettings.Monitor >= 0)
        {
            SetMonitor(windowSettings.Monitor);
        }

        // Only use Raylib.IsCursorOnScreen() - avoid ScreenArea check to prevent false positives on Windows
        // screenArea.ContainsPoint(Raylib.GetMousePosition()) -> old safeguard for macOS
        bool initialMouseOnScreen = Raylib.IsWindowFocused() && Raylib.IsCursorOnScreen();
        MouseOnScreen = initialMouseOnScreen;

        // Initialize cursor state - CursorState is the single source of truth
        cursorState = new CursorState(
            visible: windowSettings.MouseVisible,
            enabled: windowSettings.MouseEnabled,
            onScreen: initialMouseOnScreen,
            desiredVisible: windowSettings.MouseVisible,
            desiredEnabled: windowSettings.MouseEnabled
        );
        previousCursorState = cursorState;
        
        // Apply initial cursor state to Raylib
        ApplyMouseVisibilityToRaylib(cursorState.DesiredVisible);
        ApplyMouseEnabledToRaylib(cursorState.DesiredEnabled);
        
        CalculateMonitorConversionFactors();

        Raylib.SetWindowOpacity(windowSettings.WindowOpacity);
        windowConfigFlags = WindowConfigFlags.Get();
        
        
        // Rationale: ShapeEngine handles frame limiting manually in the game loop for improved timing accuracy and frame pacing.
        // Raylib's built-in frame limiter (SetTargetFPS) can introduce inconsistent frame pacing and timing inaccuracies.
        // By implementing a custom frame limiter, ShapeEngine can:
        //   - Achieve more precise control over frame timing and delta time calculations.
        //   - Ensure consistent frame pacing across different platforms and hardware.
        //   - Integrate frame limiting with the engine's own timing, update, and rendering logic.
        //   - Avoid issues where Raylib's limiter may not synchronize well with vsync or system timers.
        // For these reasons, we disable Raylib's frame limiting by setting the target FPS to 0, and rely on our own implementation.
        Raylib.SetTargetFPS(0); // Prevent Raylib from capping FPS.
        
        Instance = this;
    }

    /// <summary>
    /// Updates the window state, events, and checks for changes.
    /// </summary>
    /// <param name="dt">The delta time since the last update.</param>
    internal void Update(float dt)
    {
        var newMonitor = Monitor.HasMonitorChanged();
        if (newMonitor.Available)
        {
            UpdateWindowAfterMonitorChange(newMonitor);
        }
        CheckForWindowChanges();
        
        ScreenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
        
        CheckForWindowConfigFlagChanges();
        CheckForCursorChanges();
        
        CalculateMonitorConversionFactors();

    }
    /// <summary>
    /// Moves the mouse cursor to the specified position within the window.
    /// </summary>
    /// <param name="mousePos">The position to move the mouse to.</param>
    internal void MoveMouse(Vector2 mousePos)
    {
        mousePos = Vector2.Clamp(mousePos, new Vector2(0, 0), CurScreenSize.ToVector2());

        var mx = (int)MathF.Round(mousePos.X);
        var my = (int)MathF.Round(mousePos.Y);
        Raylib.SetMousePosition(mx, my);
    }
    /// <summary>
    /// Closes the window. (Not implemented)
    /// </summary>
    internal void Close()
    {

    }
    #endregion

    #region Window

    /// <summary>
    /// Restores the window from minimized, maximized, or fullscreen state to normal.
    /// </summary>
    /// <returns>True if the window was restored; otherwise, false.</returns>
    public bool RestoreWindow()
    {
        bool applyFix =  DisplayState == WindowDisplayState.BorderlessFullscreen || DisplayState == WindowDisplayState.Fullscreen;
        
        if (DisplayState == WindowDisplayState.Minimized)
        {
            Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
        }
        else if (DisplayState == WindowDisplayState.Maximized)
        {
            Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
        }
        else if (DisplayState == WindowDisplayState.Fullscreen)
        {
            Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
        }
        else if(DisplayState == WindowDisplayState.BorderlessFullscreen)
        {
            Raylib.ClearWindowState(ConfigFlags.BorderlessWindowMode);
        }

        DisplayState = WindowDisplayState.Normal;

        ResetMousePosition();

        // This is a fix for windows that are moved between monitors after restoring the window from a fullscreen mode
        if (applyFix)
        {
            ApplyMacOSFullscreenFix();
        }

        return false;
    }
    /// <summary>
    /// Centers the window on the current monitor.
    /// </summary>
    public void CenterWindow()
    {
        if (DisplayState == WindowDisplayState.Fullscreen) return;
        var monitor = Monitor.CurMonitor();

        int winPosX = monitor.Width / 2 - windowSize.Width / 2;
        int winPosY = monitor.Height / 2 - windowSize.Height / 2;
        Raylib.SetWindowPosition(winPosX + (int)monitor.Position.X, winPosY + (int)monitor.Position.Y);
        ResetMousePosition();
    }
    /// <summary>
    /// Resizes the window to the specified dimensions.
    /// </summary>
    /// <param name="newDimensions">The new dimensions for the window.</param>
    public void ResizeWindow(Dimensions newDimensions) => WindowSize = newDimensions;
    /// <summary>
    /// Resets the window to its default size and position.
    /// </summary>
    public void ResetWindow()
    {
        RestoreWindow();
        WindowSize = GetCurrentMonitorDimensions() / 2; // Monitor.CurMonitor().Dimensions / 2;
    }

    /// <summary>
    /// Activates fullscreen mode.
    /// </summary>
    /// <returns>True if fullscreen was activated; otherwise, false.</returns>
    public bool ActivateFullscreen()
    {
        if (DisplayState == WindowDisplayState.Fullscreen) return false;

        if (DisplayState == WindowDisplayState.Normal)
        {
            prevDisplayStateWindowDimensions = CurScreenSize;
            prevDisplayStateWindowPosition = Raylib.GetWindowPosition();
        }
        else
        {
            if (DisplayState == WindowDisplayState.BorderlessFullscreen)
            {
                Raylib.ClearWindowState(ConfigFlags.BorderlessWindowMode);
                // Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
                // Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);
            }
            else if (DisplayState == WindowDisplayState.Maximized)
            {
                Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
            }
            else if (DisplayState == WindowDisplayState.Minimized)
            {
                Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
            }

        }

        DisplayState = WindowDisplayState.Fullscreen;
        
        Raylib.SetWindowState(ConfigFlags.FullscreenMode);
        
        ResetMousePosition();
        
        return true;
    }
    /// <summary>
    /// Activates borderless fullscreen mode.
    /// </summary>
    /// <returns>True if borderless fullscreen was activated; otherwise, false.</returns>
    /// <remarks>
    /// <see cref="FullscreenAutoRestoring"/> does not affect borderless fullscreen!
    /// Prefer <see cref="ToggleFullscreen"/> or <see cref="ActivateFullscreen"/> over borderless fullscreen.
    /// Since raylib 6.0 there is not much difference between Fullscreen and Borderless Fullscreen anymore.
    /// </remarks>
    public bool ActivateBorderlessFullscreen()
    {
        if (DisplayState == WindowDisplayState.BorderlessFullscreen) return false;

        if (DisplayState == WindowDisplayState.Normal)
        {
            prevDisplayStateWindowDimensions = CurScreenSize;
            prevDisplayStateWindowPosition = Raylib.GetWindowPosition();
        }
        else
        {
            if (DisplayState == WindowDisplayState.Fullscreen)
            {
                Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
                ApplyMacOSFullscreenFix();
                // Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
                // Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);
            }
            else if (DisplayState == WindowDisplayState.Maximized)
            {
                Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
            }
            else if (DisplayState == WindowDisplayState.Minimized)
            {
                Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
            }
        }

        DisplayState = WindowDisplayState.BorderlessFullscreen;

        Raylib.SetWindowState(ConfigFlags.BorderlessWindowMode);
        
        ResetMousePosition();
        return true;
    }
    /// <summary>
    /// Minimizes the window.
    /// </summary>
    /// <returns>True if the window was minimized; otherwise, false.</returns>
    public bool MinimizeWindow()
    {
        if (DisplayState == WindowDisplayState.Minimized) return false;

        if (DisplayState == WindowDisplayState.Normal)
        {
            // prevDisplayStateWindowDimensions = CurScreenSize;
            // prevDisplayStateWindowPosition = Raylib.GetWindowPosition();
        }

        if (DisplayState == WindowDisplayState.Fullscreen)
        {
            Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
            ApplyMacOSFullscreenFix();
            // Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
            // Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);
        }
        else if (DisplayState == WindowDisplayState.BorderlessFullscreen)
        {
            Raylib.ClearWindowState(ConfigFlags.BorderlessWindowMode);
            ApplyMacOSFullscreenFix();
            // Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
            // Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);
        }
        else if (DisplayState == WindowDisplayState.Maximized)
        {
            Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
        }

        DisplayState = WindowDisplayState.Minimized;
        Raylib.SetWindowState(ConfigFlags.MinimizedWindow);

        return true;
    }
    /// <summary>
    /// Maximizes the window.
    /// </summary>
    /// <returns>True if the window was maximized; otherwise, false.</returns>
    public bool MaximizeWindow()
    {
        if(DisplayState == WindowDisplayState.Maximized) return false;

        if (DisplayState == WindowDisplayState.Normal)
        {
            // prevDisplayStateWindowDimensions = CurScreenSize;
            // prevDisplayStateWindowPosition = Raylib.GetWindowPosition();
        }

        if (DisplayState == WindowDisplayState.Fullscreen)
        {
            Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
            ApplyMacOSFullscreenFix();
            // Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
            // Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);

        }
        else if (DisplayState == WindowDisplayState.BorderlessFullscreen)
        {
            Raylib.ClearWindowState(ConfigFlags.BorderlessWindowMode);
            ApplyMacOSFullscreenFix();
            // Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
            // Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);
        }
        else if (DisplayState == WindowDisplayState.Minimized)
        {
            Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
        }

        DisplayState = WindowDisplayState.Maximized;
        Raylib.SetWindowState(ConfigFlags.MaximizedWindow);

        ResetMousePosition();
        return true;
    }
    /// <summary>
    /// Toggles borderless fullscreen mode.
    /// </summary>
    /// <remarks>
    /// <see cref="FullscreenAutoRestoring"/> does not affect borderless fullscreen!
    /// Prefer <see cref="ToggleFullscreen"/> or <see cref="ActivateFullscreen"/> over borderless fullscreen.
    /// Since raylib 6.0 there is not much difference between Fullscreen and Borderless Fullscreen anymore.
    /// </remarks>
    public void ToggleBorderlessFullscreen()
    {
        if (DisplayState == WindowDisplayState.BorderlessFullscreen)
        {
            RestoreWindow();
        }
        else
        {
            ActivateBorderlessFullscreen();
        }
    }
    /// <summary>
    /// Toggles fullscreen mode.
    /// </summary>
    public void ToggleFullscreen()
    {
        if (DisplayState == WindowDisplayState.Fullscreen)
        {
            RestoreWindow();
        }
        else
        {
            ActivateFullscreen();
        }
    }
    /// <summary>
    /// Toggles maximized state of the window.
    /// </summary>
    public void ToggleMaximizeWindow()
    {
        if (DisplayState == WindowDisplayState.Maximized)
        {
            RestoreWindow();
        }
        else
        {
            MaximizeWindow();
        }
    }
    /// <summary>
    /// Toggles minimized state of the window.
    /// </summary>
    public void ToggleMinimizeWindow()
    {
        if (DisplayState == WindowDisplayState.Minimized)
        {
            RestoreWindow();
        }
        else
        {
            MinimizeWindow();
        }
    }

    /// <summary>
    /// Sets the window border to fixed (not resizable, decorated).
    /// </summary>
    /// <returns>True if the border was changed; otherwise, false.</returns>
    public bool SetWindowBorderFixed()
    {
        if (WindowBorder == WindowBorder.Fixed) return false;

        if (WindowBorder == WindowBorder.Resizabled)
        {
            Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
        }
        else if (WindowBorder == WindowBorder.Undecorated)
        {
            Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
        }
        WindowBorder = WindowBorder.Fixed;
        return true;
    }
    /// <summary>
    /// Sets the window border to resizable.
    /// </summary>
    /// <returns>True if the border was changed; otherwise, false.</returns>
    public bool SetWindowBorderResizable()
    {
        if (WindowBorder == WindowBorder.Resizabled) return false;

        if (WindowBorder == WindowBorder.Undecorated)
        {
            Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
        }
        Raylib.SetWindowState(ConfigFlags.ResizableWindow);

        WindowBorder = WindowBorder.Resizabled;
        return true;
    }
    /// <summary>
    /// Sets the window border to undecorated (no border).
    /// </summary>
    /// <returns>True if the border was changed; otherwise, false.</returns>
    public bool SetWindowBorderUndecorated()
    {
        if (WindowBorder == WindowBorder.Undecorated) return false;

        if (WindowBorder == WindowBorder.Resizabled)
        {
            Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
        }
        Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);

        WindowBorder = WindowBorder.Undecorated;
        return true;
    }

    /// <summary>
    /// Sets the window as topmost or not.
    /// </summary>
    /// <param name="topmost">True to set topmost; false otherwise.</param>
    /// <returns>True if the state was changed; otherwise, false.</returns>
    public bool SetWindowTopmost(bool topmost)
    {
        if (Raylib.IsWindowState(ConfigFlags.TopmostWindow) == topmost) return false;
        if(topmost) Raylib.SetWindowState(ConfigFlags.TopmostWindow);
        else Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
        return true;
    }

    /// <summary>
    /// Returns whether the window is in borderless fullscreen mode.
    /// </summary>
    public bool IsWindowBorderlessFullscreen() => DisplayState == WindowDisplayState.BorderlessFullscreen;
    /// <summary>
    /// Returns whether the window is in fullscreen mode.
    /// </summary>
    public bool IsWindowFullscreen() => DisplayState == WindowDisplayState.Fullscreen;
    /// <summary>
    /// Returns whether the window is maximized.
    /// </summary>
    public bool IsWindowMaximized() => DisplayState == WindowDisplayState.Maximized;
    /// <summary>
    /// Returns whether the window is minimized.
    /// </summary>
    public bool IsWindowMinimized() => DisplayState == WindowDisplayState.Minimized;
    /// <summary>
    /// Returns whether the window is in normal state.
    /// </summary>
    public bool IsWindowNormal() => DisplayState == WindowDisplayState.Normal;

    /// <summary>
    /// Gets whether the window is currently focused.
    /// </summary>
    public bool IsWindowFocused => Raylib.IsWindowFocused();
    /// <summary>
    /// Gets whether the window is currently topmost.
    /// </summary>
    public bool IsWindowTopmost => Raylib.IsWindowState(ConfigFlags.TopmostWindow);

    /// <summary>
    /// Calculates the percentage of the window area that is visible on the screen.
    /// </summary>
    /// <returns>A value between 0 and 1 representing the visible area.</returns>
    public float GetScreenPercentage()
    {
        // Get current monitor info to get both dimensions AND position
        var monitor = Monitor.CurMonitor();
        // var screenSize = monitor.Dimensions.ToSize();
        var screenSize = GetCurrentMonitorDimensions().ToSize();
        
        // FIXED: Use monitor's actual position, not (0,0), to work on non-primary monitors
        var screenRect = new Rect(monitor.Position, screenSize, new(0f));

        var wSize = CurScreenSize.ToSize();
        var windowPos = Raylib.GetWindowPosition();
        var windowRect = new Rect(windowPos, wSize, new(0f));
        float p = CalculateScreenPercentage(screenRect, windowRect);
        return p;
    }
    /// <summary>
    /// Reports how much of the window area is shown on the screen. 0 means window is not on the screen, 1 means whole window is on screen.
    /// </summary>
    /// <param name="screen">The screen rectangle.</param>
    /// <param name="window">The window rectangle.</param>
    /// <returns>The percentage of the window area visible on the screen.</returns>
    private float CalculateScreenPercentage(Rect screen, Rect window)
    {
        var intersection = screen.Difference(window);
        if (intersection.Width <= 0f && intersection.Height <= 0f) return 0f;

        var screenArea = screen.GetArea();
        var intersectionArea = intersection.GetArea();
        var f = intersectionArea / screenArea;
        return f;
    }

    /// <summary>
    /// Calculates conversion factors between screen and monitor dimensions.
    /// </summary>
    private void CalculateMonitorConversionFactors()
    {
        var mDim = GetCurrentMonitorDimensions();
        ScreenToMonitor = new DimensionConversionFactors(CurScreenSize, mDim);
        MonitorToScreen = new DimensionConversionFactors(mDim, CurScreenSize);
    }

    private Dimensions GetCurrentMonitorDimensions()
    {
        var mDim = Monitor.CurMonitor().Dimensions;
        if (Game.IsWindows())
        {
            var dpiScale = Raylib.GetWindowScaleDPI();
            mDim = new Dimensions(
                (int)(mDim.Width / dpiScale.X),
                (int)(mDim.Height / dpiScale.Y)
            );
        }
        return mDim;
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Sets up the initial window dimensions and position.
    /// </summary>
    private void SetupWindowDimensions()
    {
        var monitor = Monitor.CurMonitor();
        WindowSize = monitor.Dimensions / 2;
        WindowPosition = Raylib.GetWindowPosition();
        prevDisplayStateWindowDimensions = WindowSize;
        prevDisplayStateWindowPosition = WindowPosition;
        CalculateCurScreenSize();
    }
    
    /// <summary>
    /// Calculates the current screen size based on the window state.
    /// </summary>
    private void CalculateCurScreenSize()
    {
        int w = Raylib.GetScreenWidth();
        int h = Raylib.GetScreenHeight();
        CurScreenSize = new(w, h);
    }
    
    /// <summary>
    /// Checks for changes in window configuration flags and raises events.
    /// </summary>
    private void CheckForWindowConfigFlagChanges()
    {
        var cur = WindowConfigFlags.Get();

        if (cur.HasResizableChanged(windowConfigFlags))
        {
            OnWindowResizableChanged?.Invoke(cur.Resizable);
        }
        if (cur.HasTopmostChanged(windowConfigFlags))
        {
            OnWindowTopmostChanged?.Invoke(cur.Topmost);
        }
        if (cur.HasUndecoratedChanged(windowConfigFlags))
        {
            OnWindowUndecoratedChanged?.Invoke(cur.Undecorated);
        }

        if (cur.HasFocusedChanged(windowConfigFlags))
        {
            OnWindowFocusChanged?.Invoke(cur.Focused);
            
            HandleFullscreenAutoRestoring(cur.Focused);
            // HandleFullscreenBorderlessAutoRestoring(cur.Focused);
        }
        if (cur.HasAlwaysRunChanged(windowConfigFlags))
        {
            OnWindowAlwaysRunChanged?.Invoke(cur.AlwaysRun);
        }
        if (cur.HasMousePassThroughChanged(windowConfigFlags))
        {
            OnWindowMousePassThroughChanged?.Invoke(cur.MousePassThrough);
        }

        if (cur.HasFullscreenChanged(windowConfigFlags))
        {
            OnWindowFullscreenChanged?.Invoke(cur.Fullscreen);
        }
        if (cur.HasMaximizedChanged(windowConfigFlags))
        {
            OnWindowMaximizeChanged?.Invoke(cur.Maximized);

            if (cur.Maximized && DisplayState != WindowDisplayState.Maximized)
            {
                DisplayState = WindowDisplayState.Maximized;
            }
            else if (!cur.Maximized && DisplayState == WindowDisplayState.Maximized)
            {
                DisplayState = WindowDisplayState.Normal;
            }
        }

        if (cur.HasMinimizedChanged(windowConfigFlags))
        {
            OnWindowMinimizedChanged?.Invoke(cur.Minimized);
            if (cur.Minimized && DisplayState != WindowDisplayState.Minimized)
            {
                if (DisplayState == WindowDisplayState.Maximized) wasMaximized = true;
                DisplayState = WindowDisplayState.Minimized;
            }
            else if (!cur.Minimized && DisplayState == WindowDisplayState.Minimized)
            {
                if(wasMaximized) Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
                DisplayState = WindowDisplayState.Normal;
                wasMaximized = false;
            }
        }
        if (cur.HasHiddenChanged(windowConfigFlags))
        {
            OnWindowHiddenChanged?.Invoke(cur.Hidden);
        }

        windowConfigFlags = cur;
    }
    
    /// <summary>
    /// Checks for changes in window size and position and raises events.
    /// </summary>
    private void CheckForWindowChanges()
    {
        var prev = CurScreenSize;
        CalculateCurScreenSize();
        
        if (prev != CurScreenSize)
        {
            if (DisplayState == WindowDisplayState.Normal)
            {
                windowSize = CurScreenSize;
            }
            var conversion = new DimensionConversionFactors(prev, CurScreenSize);
            OnWindowSizeChanged?.Invoke(conversion);
        }

        var curWindowPosition = Raylib.GetWindowPosition();
        if (curWindowPosition != WindowPosition)
        {
            WindowPosition = curWindowPosition;
            OnWindowPositionChanged?.Invoke(WindowPosition, curWindowPosition);
        }
    }
  
    /// <summary>
    /// Checks for changes in cursor state and raises events.
    /// </summary>
    private void CheckForCursorChanges()
    {
        // Determine current window state
        bool isWindowFocused = Raylib.IsWindowFocused();
        bool wasMinimized = Raylib.IsWindowState(ConfigFlags.MinimizedWindow);
        
        // Determine current mouse on screen state
        // Mouse is only "on screen" if cursor is within window bounds AND window is focused
        // Use ONLY Raylib.IsCursorOnScreen() - don't fallback to ScreenArea check because
        // MousePosition can get clamped at window edges on Windows, causing false positives
        bool currentMouseOnScreen = isWindowFocused && Raylib.IsCursorOnScreen();
        
        // Update MouseOnScreen property and cursor state
        MouseOnScreen = currentMouseOnScreen;
        cursorState = cursorState.WithOnScreen(currentMouseOnScreen);
        
        // Handle mouse leaving/entering screen transitions
        if (!currentMouseOnScreen || wasMinimized || !isWindowFocused)
        {
            // Mouse is off screen, window is minimized, or window is not focused
            if (previousCursorState.OnScreen) // Was on screen previously
            {
                // Fire event and force cursor to be visible/enabled for OS compatibility
                OnMouseLeftScreen?.Invoke();
                ForceShowAndEnableCursor();
            }
            // When window is not focused, always ensure cursor is visible/enabled for OS control
            else if (!isWindowFocused && (cursorState.Visible != true || cursorState.Enabled != true))
            {
                ForceShowAndEnableCursor();
            }
        }
        else
        {
            // Mouse is on screen, window is focused and not minimized
            if (!previousCursorState.OnScreen) // Was off screen previously
            {
                // Fire event and restore desired cursor state
                OnMouseEnteredScreen?.Invoke();
                RestoreDesiredCursorState();
            }
            else
            {
                // Mouse stayed on screen - validate and apply any pending state changes
                ValidateAndSyncCursorState();
                
                // Ensure desired state is applied (handles case where user changed properties while on screen)
                if (cursorState.Enabled != cursorState.DesiredEnabled)
                {
                    ApplyMouseEnabledToRaylib(cursorState.DesiredEnabled);
                }
                
                if (cursorState.Visible != cursorState.DesiredVisible)
                {
                    ApplyMouseVisibilityToRaylib(cursorState.DesiredVisible);
                }
            }
        }
        
        // Fire events for state changes (only when mouse is on screen and window is focused)
        if (currentMouseOnScreen && !wasMinimized && isWindowFocused)
        {
            // Check for visibility changes
            if (cursorState.Visible != previousCursorState.Visible)
            {
                OnMouseVisibilityChanged?.Invoke(cursorState.Visible);
            }
            
            // Check for enabled state changes
            if (cursorState.Enabled != previousCursorState.Enabled)
            {
                OnMouseEnabledChanged?.Invoke(cursorState.Enabled);
            }
        }
        
        // Update cached cursor state for next frame
        previousCursorState = cursorState;
    }
    
    private void UpdateWindowAfterMonitorChange(MonitorInfo monitor)
    {
        var windowDimensions = windowSize;
        if (windowDimensions.Width > monitor.Width || windowDimensions.Height > monitor.Height)
        {
            windowDimensions = monitor.Dimensions / 2;
        }
        
        windowSize = windowDimensions;
        prevDisplayStateWindowDimensions = windowDimensions;
    }

    private void HandleFullscreenAutoRestoring(bool focused)
    {
        if (!FullscreenAutoRestoring) return;

        if (fullscreenAutoRestoringActive)
        {
            if (focused)
            {
                if(fullscreenAutoRestoringWindowWasTopmost) Raylib.SetWindowState(ConfigFlags.TopmostWindow);
                ActivateFullscreen();
                fullscreenAutoRestoringActive = false;
            }
        }
        else if (DisplayState == WindowDisplayState.Fullscreen && !focused)
        {
            fullscreenAutoRestoringActive = true;
            RestoreWindow();
                        
            fullscreenAutoRestoringWindowWasTopmost = Raylib.IsWindowState(ConfigFlags.TopmostWindow);
            Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
        }
    }

    // private void HandleFullscreenBorderlessAutoRestoring(bool focused)
    // {
    //     if (!BorderlessFullscreenAutoRestoring) return;
    //
    //     if (borderlessFullscreenAutoRestoringActive)
    //     {
    //         if (focused)
    //         {
    //             if(borderlessFullscreenAutoRestoringWindowWasTopmost) Raylib.SetWindowState(ConfigFlags.TopmostWindow);
    //             ActivateBorderlessFullscreen();
    //             borderlessFullscreenAutoRestoringActive = false;
    //             borderlessFullscreenAutoRestoringActiveCooldown = false;
    //         }
    //     }
    //     else if (DisplayState == WindowDisplayState.BorderlessFullscreen && !focused)
    //     {
    //         borderlessFullscreenAutoRestoringActive = true;
    //         RestoreWindow();
    //                     
    //         borderlessFullscreenAutoRestoringWindowWasTopmost = Raylib.IsWindowState(ConfigFlags.TopmostWindow);
    //         Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
    //     }
    // }
    
    private void ApplyMacOSFullscreenFix()
    {
        //when entering any fullscreen mode on macOS,
        //exiting fullscreen and then dragging the window to another monitor will increase the window size to the monitors size
        //this fixes the issue
        
        if (!Game.IsOSX()) return;
        if(Monitor.MonitorCount() <= 1) return;
        
        var currentMonitorIndex = Monitor.GetCurIndex();
        foreach (var monitorInfo in Monitor.GetAllMonitorInfo())
        {
            if(monitorInfo.Index == currentMonitorIndex) continue;
            Raylib.SetWindowMonitor(monitorInfo.Index);
        }
        Raylib.SetWindowMonitor(currentMonitorIndex);

        Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
        Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);
    }
    #endregion

    #region Monitor

    /// <summary>
    /// Sets the window to use the specified monitor.
    /// </summary>
    /// <param name="newMonitor">The index of the new monitor.</param>
    /// <returns>True if the monitor was changed; otherwise, false.</returns>
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
    
    /// <summary>
    /// Switches to the next available monitor.
    /// </summary>
    public void NextMonitor()
    {
        var nextMonitor = Monitor.NextMonitor();
        if (nextMonitor.Available)
        {
            ChangeMonitor(nextMonitor);
        }
    }
   
    /// <summary>
    /// Changes the window to use the specified monitor info.
    /// </summary>
    /// <param name="monitor">The monitor info to switch to.</param>
    private void ChangeMonitor(MonitorInfo monitor)
    {
        bool activateBorderless = false;
        bool activateFullscreen = false;
        
        if (DisplayState == WindowDisplayState.Fullscreen)
        {
            RestoreWindow();
            activateFullscreen = true;
        }
        else if (DisplayState == WindowDisplayState.BorderlessFullscreen)
        {
            RestoreWindow();
            activateBorderless = true;
        }

        var windowDimensions = windowSize;
        if (windowDimensions.Width > monitor.Width || windowDimensions.Height > monitor.Height)
        {
            windowDimensions = monitor.Dimensions / 2;
        }

        windowSize = windowDimensions;

        if (Game.IsOSX())
        {
            Raylib.SetWindowMonitor(monitor.Index);
            
            int winPosX = monitor.Width / 2 - windowDimensions.Width / 2;
            int winPosY = monitor.Height / 2 - windowDimensions.Height / 2;
            int x = winPosX + (int)monitor.Position.X;
            int y = winPosY + (int)monitor.Position.Y;
            
            prevDisplayStateWindowDimensions = windowDimensions;
            prevDisplayStateWindowPosition = new(x, y);
            
            CurScreenSize = new(windowDimensions.Width, windowDimensions.Height);
            
            Raylib.SetWindowPosition(x, y);
            Raylib.SetWindowSize(windowDimensions.Width, windowDimensions.Height);
        }
        else
        {
            Raylib.SetWindowMonitor(monitor.Index);
            
            //I am setting all of this here to be sure.
            //Currently it does work, fixes some problems and does not create new problems (that I am aware of), 
            //so I am leaving it for now.
            var pos = Raylib.GetWindowPosition();
            var w = Raylib.GetScreenWidth();
            var h = Raylib.GetScreenHeight();

            windowDimensions = new(w, h);
            windowSize = new(w, h);
            CurScreenSize = new(w, h);
            prevDisplayStateWindowDimensions = windowDimensions;
            prevDisplayStateWindowPosition = pos;
        }
        
        ResetMousePosition();
        OnMonitorChanged?.Invoke(monitor);

        if (activateBorderless)
        {
            ActivateBorderlessFullscreen();
        }
        else if (activateFullscreen)
        {
            ActivateFullscreen();
        }
    }


    #endregion

    #region Mouse

    /// <summary>
    /// Gets or sets the mouse position relative to the window.
    /// </summary>
    public Vector2 MousePosition
    {
        get => Raylib.GetMousePosition();
        set => Raylib.SetMousePosition((int)value.X, (int)value.Y);
    }

    /// <summary>
    /// Gets the mouse movement delta since the last frame.
    /// </summary>
    public Vector2 MouseDelta => Raylib.GetMouseDelta();
    /// <summary>
    /// Gets the X coordinate of the mouse position.
    /// </summary>
    public float MouseX => MousePosition.X;
    /// <summary>
    /// Gets the Y coordinate of the mouse position.
    /// </summary>
    public float MouseY => MousePosition.Y;

    /// <summary>
    /// Gets or sets whether the mouse is enabled for input.
    /// </summary>
    public bool MouseEnabled
    {
        get => cursorState.DesiredEnabled;
        set
        {
            if (value == cursorState.DesiredEnabled) return;
            cursorState = cursorState.WithDesiredEnabled(value);
            
            // Apply immediately if mouse is on screen, otherwise will apply when mouse returns
            if (cursorState.OnScreen)
            {
                ApplyMouseEnabledToRaylib(value);
            }
        }
    }
    
    /// <summary>
    /// Gets or sets whether the mouse cursor is visible.
    /// </summary>
    public bool MouseVisible
    {
        get => cursorState.DesiredVisible;
        set
        {
            if (value == cursorState.DesiredVisible) return;
            cursorState = cursorState.WithDesiredVisible(value);
            
            // Apply immediately if mouse is on screen, otherwise will apply when mouse returns
            if (cursorState.OnScreen)
            {
                ApplyMouseVisibilityToRaylib(value);
            }
        }
    }

    /// <summary>
    /// Resets the mouse position to the center of the window.
    /// </summary>
    /// <remarks>
    /// On Windows, setting mouse position when cursor is disabled or window is not focused
    /// can cause the OS cursor to get stuck. This method includes guards to prevent that.
    /// </remarks>
    public void ResetMousePosition()
    {
        // Don't reset position if window is not focused - this can cause cursor to get stuck on Windows
        if (!Raylib.IsWindowFocused()) return;
        
        // Don't reset position if cursor is currently disabled - this can cause cursor to lock on Windows
        // Check both our internal state and Raylib state for safety
        if (!cursorState.Enabled || Raylib.IsCursorHidden()) return;
        
        var center = WindowPosition / 2 + WindowSize.ToVector2() / 2;
        Raylib.SetMousePosition((int)center.X, (int)center.Y);
    }

    /// <summary>
    /// Applies mouse visibility state to Raylib only if it differs from current Raylib state.
    /// </summary>
    /// <param name="visible">Whether the cursor should be visible.</param>
    private void ApplyMouseVisibilityToRaylib(bool visible)
    {
        bool raylibCursorHidden = Raylib.IsCursorHidden();
        if (visible && raylibCursorHidden)
        {
            Raylib.ShowCursor();
            cursorState = cursorState.WithVisible(true);
        }
        else if (!visible && !raylibCursorHidden)
        {
            Raylib.HideCursor();
            cursorState = cursorState.WithVisible(false);
        }
        else
        {
            // Already in correct state - just sync our state
            cursorState = cursorState.WithVisible(visible);
        }
    }
    
    /// <summary>
    /// Applies mouse enabled state to Raylib.
    /// Note: Raylib doesn't provide a way to query cursor enabled state, so we track it ourselves.
    /// </summary>
    /// <param name="enabled">Whether the cursor should be enabled.</param>
    private void ApplyMouseEnabledToRaylib(bool enabled)
    {
        if (enabled && !cursorState.Enabled)
        {
            Raylib.EnableCursor();
            cursorState = cursorState.WithEnabled(true);
        }
        else if (!enabled && cursorState.Enabled)
        {
            Raylib.DisableCursor();
            cursorState = cursorState.WithEnabled(false);
        }
    }
    
    /// <summary>
    /// Forces cursor to be visible and enabled (used when mouse leaves screen or window loses focus).
    /// </summary>
    private void ForceShowAndEnableCursor()
    {
        if (!cursorState.Enabled)
        {
            Raylib.EnableCursor();
            cursorState = cursorState.WithEnabled(true);
        }
        
        if (!cursorState.Visible)
        {
            Raylib.ShowCursor();
            cursorState = cursorState.WithVisible(true);
        }
    }
    
    /// <summary>
    /// Restores cursor to desired state (used when mouse returns to screen).
    /// </summary>
    private void RestoreDesiredCursorState()
    {
        ApplyMouseEnabledToRaylib(cursorState.DesiredEnabled);
        ApplyMouseVisibilityToRaylib(cursorState.DesiredVisible);
    }
    
    /// <summary>
    /// Validates and synchronizes cursor state with Raylib.
    /// Ensures our internal state matches Raylib's actual state.
    /// </summary>
    private void ValidateAndSyncCursorState()
    {
        // Sync visibility with Raylib's actual state
        bool raylibCursorHidden = Raylib.IsCursorHidden();
        bool raylibVisible = !raylibCursorHidden;
        
        if (cursorState.Visible != raylibVisible)
        {
            // State desync detected - Raylib state is authoritative
            cursorState = cursorState.WithVisible(raylibVisible);
        }
    }

    #endregion
}