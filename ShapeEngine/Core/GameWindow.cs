using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.Screen;

namespace ShapeEngine.Core;

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
        /// Indicates whether VSync is enabled for the window.
        /// </summary>
        public readonly bool VSync;
        
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
            VSync = value;
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
            VSync = Raylib.IsWindowState(ConfigFlags.VSyncHint);
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
        /// <summary>Checks if the VSync flag has changed compared to another instance.</summary>
        public bool HasVSyncChanged(WindowConfigFlags other) => VSync != other.VSync;
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
    /// </summary>
    private readonly struct CursorState
    {
        public readonly bool Visible;
        public readonly bool Enabled;
        public readonly bool OnScreen;

        /// <summary>
        /// Initializes a new instance of <see cref="CursorState"/> with all states set to true.
        /// </summary>
        public CursorState()
        {
            Visible = true;
            Enabled = true;
            OnScreen = true;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CursorState"/> with specified states.
        /// </summary>
        public CursorState(bool visible, bool enabled, bool onScreen)
        {
            Visible = visible;
            Enabled = enabled;
            OnScreen = onScreen;
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
    public event Action<bool>? OnWindowVSyncChanged;

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
            var maxSize = Monitor.CurMonitor().Dimensions;
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
   /// Gets or sets the minimum allowed framerate.
   /// </summary>
   /// <remarks>If set, updates <see cref="FpsLimit"/> if it is below the new minimum.</remarks>
    public int MinFramerate
    {
        get => minFramerate;
        set
        {
            if (value == minFramerate) return;
            if (value <= 0) minFramerate = 1;
            else if (value >= maxFramerate)
            {
                minFramerate = maxFramerate;
                maxFramerate = value;
            }
            else minFramerate = value;

            if (FpsLimit < minFramerate) fpsLimit = minFramerate;
        }
    }
    /// <summary>
    /// Gets or sets the maximum allowed framerate for the window.
    /// <remarks>
    /// If set below <see cref="MinFramerate"/>, both values will be synchronized.
    /// If set, updates <see cref="FpsLimit"/> if it is above the new maximum.
    /// </remarks>
    /// </summary>
    public int MaxFramerate
    {
        get => maxFramerate;
        set
        {
            if (value == maxFramerate) return;
            if (value <= minFramerate)
            {
                maxFramerate = minFramerate;
                minFramerate = value;
            }
            else maxFramerate = value;

            if (FpsLimit > maxFramerate) fpsLimit = maxFramerate;
        }
    }
    /// <summary>
    /// Gets or sets the framerate limit.
    /// </summary>
    public int FpsLimit
    {
        get => fpsLimit;
        set
        {
            if (value < MinFramerate) fpsLimit = MinFramerate;
            else if (value > MaxFramerate) fpsLimit = MaxFramerate;
            else fpsLimit = value;
            if(!VSync) Raylib.SetTargetFPS(fpsLimit);
        }
    }
    /// <summary>
    /// Gets the current frames per second.
    /// </summary>
    public int Fps => Raylib.GetFPS();
    /// <summary>
    /// Gets or sets whether VSync is enabled.
    /// </summary>
    public bool VSync
    {
        get => Raylib.IsWindowState(ConfigFlags.VSyncHint);
        set
        {
            if (Raylib.IsWindowState(ConfigFlags.VSyncHint) == value) return;
            if (value)
            {
                Raylib.SetWindowState(ConfigFlags.VSyncHint);
                Raylib.SetTargetFPS(Monitor.CurMonitor().Refreshrate);
            }
            else
            {
                Raylib.ClearWindowState(ConfigFlags.VSyncHint);
                Raylib.SetTargetFPS(fpsLimit);
            }
        }
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
            // IsMouseOnScreen = value;
        }
    }
    private bool mouseOnScreen;
    /// <summary>
    /// Gets the area of the screen as a rectangle.
    /// </summary>
    public Rect ScreenArea { get; private set; }

    /// <summary>
    /// Gets or sets whether the window should automatically restore from fullscreen when focus is lost.
    /// </summary>
    public bool FullscreenAutoRestoring { get; set; }
    #endregion

    #region Private Members

    private Vector2 osxWindowScaleDpi;
    private int fpsLimit = 60;
    private int minFramerate;
    private int maxFramerate;
    private Dimensions windowSize = new();

    private bool? wasMouseEnabled;
    private bool? wasMouseVisible;

    private CursorState cursorState;
    private WindowConfigFlags windowConfigFlags;
    private bool wasMaximized;

    private Dimensions prevDisplayStateWindowDimensions = new(128, 128);
    private Vector2 prevDisplayStateWindowPosition = new(128, 128);

    private Size prevFullscreenResolution = new(-1, -1);
    private bool wasFullscreen;

    #endregion

    #region Internal Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="GameWindow"/> class with the specified settings.
    /// </summary>
    /// <param name="windowSettings">The window settings to use.</param>
    internal GameWindow(WindowSettings windowSettings)
    {
        if(windowSettings.Msaa4x) Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
        if(windowSettings.HighDPI) Raylib.SetConfigFlags(ConfigFlags.HighDpiWindow);
        if(windowSettings.FramebufferTransparent) Raylib.SetConfigFlags(ConfigFlags.TransparentWindow);

        // Raylib.InitWindow(windowSettings.WindowMinSize.Width, windowSettings.WindowMinSize.Height, windowSettings.Title);
        Raylib.InitWindow(0,0, windowSettings.Title);//sets autoiconify to false until my changes are in raylib cs
        Raylib.SetWindowOpacity(0f);

        Monitor = new MonitorDevice();
        SetupWindowDimensions();
        WindowMinSize = windowSettings.WindowMinSize;
        Raylib.SetWindowMinSize(WindowMinSize.Width, WindowMinSize.Height);

        Raylib.SetWindowState(ConfigFlags.AlwaysRunWindow);

        // if (windowSettings.Focused)
        // {
        //     Raylib.ClearWindowState(ConfigFlags.UnfocusedWindow);
        // }
        // else Raylib.SetWindowState(ConfigFlags.UnfocusedWindow);

        if (windowSettings.Topmost) Raylib.SetWindowState(ConfigFlags.TopmostWindow);

        FullscreenAutoRestoring = windowSettings.FullscreenAutoRestoring;
        VSync = windowSettings.Vsync;
        MinFramerate = windowSettings.MinFramerate;
        MaxFramerate = windowSettings.MaxFramerate;
        FpsLimit = windowSettings.FrameRateLimit;

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

        var screenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
        MouseOnScreen = Raylib.IsCursorOnScreen() || ( Raylib.IsWindowFocused() && screenArea.ContainsPoint(Raylib.GetMousePosition()) );

        MouseVisible = windowSettings.MouseVisible;
        MouseEnabled = windowSettings.MouseEnabled;

        cursorState = GetCurCursorState();

        if (Game.IsOSX()) osxWindowScaleDpi = Raylib.GetWindowScaleDPI();
        else osxWindowScaleDpi = new Vector2(1, 1);

        CalculateMonitorConversionFactors();

        Raylib.SetWindowOpacity(windowSettings.WindowOpacity);
        windowConfigFlags = WindowConfigFlags.Get();

        Instance = this;
    }

    /// <summary>
    /// Updates the window state, events, and checks for changes.
    /// </summary>
    /// <param name="dt">The delta time since the last update.</param>
    internal void Update(float dt)
    {
        // LerpOpacitiy(dt);

        var newMonitor = Monitor.HasMonitorChanged();
        if (newMonitor.Available)
        {
            ChangeMonitor(newMonitor);
        }
        CheckForWindowChanges();

        ScreenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);

        CheckForWindowConfigFlagChanges();
        // CheckForWindowFlagChanges();
        CheckForCursorChanges();

        CalculateMonitorConversionFactors();

        if (MouseVisible == Raylib.IsCursorHidden()) MouseVisible = !Raylib.IsCursorHidden();

    }
    /// <summary>
    /// Moves the mouse cursor to the specified position within the window.
    /// </summary>
    /// <param name="mousePos">The position to move the mouse to.</param>
    internal void MoveMouse(Vector2 mousePos)
    {
        mousePos = Vector2.Clamp(mousePos, new Vector2(0, 0), CurScreenSize.ToVector2());
        // lastControlledMousePosition = mousePos;
        // mouseControlled = true;

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
        if (DisplayState == WindowDisplayState.Minimized)
        {
            Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
        }
        else if (DisplayState == WindowDisplayState.Maximized)
        {
            Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
        }
        else if (DisplayState == WindowDisplayState.Fullscreen || DisplayState == WindowDisplayState.BorderlessFullscreen)
        {
            Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
        }

        Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
        Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);

        DisplayState = WindowDisplayState.Normal;

        ResetMousePosition();
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
        WindowSize = Monitor.CurMonitor().Dimensions / 2;
    }

    /// <summary>
    /// Activates fullscreen mode with the specified resolution.
    /// </summary>
    /// <param name="width">The width for fullscreen.</param>
    /// <param name="height">The height for fullscreen.</param>
    /// <returns>True if fullscreen was activated; otherwise, false.</returns>
    public bool ActivateFullscreen(int width, int height)
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
                Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
                Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
                Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);
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

        prevFullscreenResolution = new(width, height);

        Raylib.SetWindowSize(width, height);
        Raylib.SetWindowState(ConfigFlags.FullscreenMode);
        // CalculateCurScreenSize();
        ResetMousePosition();
        return true;
    }
    /// <summary>
    /// Activates borderless fullscreen mode.
    /// </summary>
    /// <returns>True if borderless fullscreen was activated; otherwise, false.</returns>
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
                prevFullscreenResolution = new(-1, -1);
                Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
                Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
                Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);
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

        var mDim = Monitor.CurMonitor().Dimensions;
        var dpi = Raylib.GetWindowScaleDPI();
        Raylib.SetWindowSize(mDim.Width * (int)dpi.X, mDim.Height * (int)dpi.Y);
        Raylib.SetWindowState(ConfigFlags.FullscreenMode);

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
            prevDisplayStateWindowDimensions = CurScreenSize;
            prevDisplayStateWindowPosition = Raylib.GetWindowPosition();
        }

        if (DisplayState == WindowDisplayState.Fullscreen || DisplayState == WindowDisplayState.BorderlessFullscreen)
        {
            prevFullscreenResolution = new(-1, -1);
            Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
            Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
            Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);
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
            prevDisplayStateWindowDimensions = CurScreenSize;
            prevDisplayStateWindowPosition = Raylib.GetWindowPosition();
        }

        if (DisplayState == WindowDisplayState.Fullscreen || DisplayState == WindowDisplayState.BorderlessFullscreen)
        {
            prevFullscreenResolution = new(-1, -1);
            Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
            Raylib.SetWindowSize(prevDisplayStateWindowDimensions.Width, prevDisplayStateWindowDimensions.Height);
            Raylib.SetWindowPosition((int)prevDisplayStateWindowPosition.X, (int)prevDisplayStateWindowPosition.Y);

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
        var screenSize = Monitor.CurMonitor().Dimensions.ToSize();
        var screenRect = new Rect(new(0f), screenSize, new(0f));

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
        int monitor = Raylib.GetCurrentMonitor();
        int mw = Raylib.GetMonitorWidth(monitor);
        int mh = Raylib.GetMonitorHeight(monitor);
        if (Game.IsOSX())
        {
             if (IsWindowBorderlessFullscreen() || IsWindowFullscreen())
             {
                 mw = (int)(mw / osxWindowScaleDpi.X);
                 mh = (int)(mh / osxWindowScaleDpi.Y);
             }
        }

        var mDim = new Dimensions(mw, mh);
        ScreenToMonitor = new DimensionConversionFactors(CurScreenSize, mDim);
        MonitorToScreen = new DimensionConversionFactors(mDim, CurScreenSize);
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
        // PrevFullscreenDisplayState = new(WindowSize, WindowPosition, DisplayState);
        // PrevMinimizedDisplayState = new(WindowSize, WindowPosition, DisplayState);
        prevDisplayStateWindowDimensions = WindowSize;
        prevDisplayStateWindowPosition = WindowPosition;
        CalculateCurScreenSize();
    }
    /// <summary>
    /// Calculates the current screen size based on the window state.
    /// </summary>
    private void CalculateCurScreenSize()
    {
        if(DisplayState == WindowDisplayState.Fullscreen)
        {
            // int w = Raylib.GetScreenWidth();
            // int h = Raylib.GetScreenHeight();
            // CurScreenSize = new(w, h);

            int w = Raylib.GetRenderWidth();
            int h = Raylib.GetRenderHeight();
            CurScreenSize = new(w, h);
        }
        else if (DisplayState == WindowDisplayState.BorderlessFullscreen)
        {
            int monitor = Raylib.GetCurrentMonitor();
            int mw = Raylib.GetMonitorWidth(monitor);
            int mh = Raylib.GetMonitorHeight(monitor);
            CurScreenSize = new(mw , mh);


            // int w = Raylib.GetRenderWidth();
            // int h = Raylib.GetRenderHeight();
            // CurScreenSize = new(w, h);

            // var scaleFactor = Game.IsOSX() ? Raylib.GetWindowScaleDPI() : new Vector2(1f, 1f);
            // // var scaleX = (int)scaleFactor.X;
            // // var scaleY = (int)scaleFactor.Y;
            // CurScreenSize = new(mw / scaleFactor.X , mh / scaleFactor.Y );
        }
        else
        {
            // var scaleFactor = Raylib.GetWindowScaleDPI();
            // int scaleX = (int)scaleFactor.X;
            // int scaleY = (int)scaleFactor.Y;

            int w = Raylib.GetScreenWidth();
            int h = Raylib.GetScreenHeight();
            CurScreenSize = new(w, h);
        }
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
            if (FullscreenAutoRestoring)
            {
                if (!cur.Focused)
                {
                    if (DisplayState == WindowDisplayState.BorderlessFullscreen || DisplayState == WindowDisplayState.Fullscreen)
                    {
                        wasFullscreen = true;
                        if (DisplayState == WindowDisplayState.BorderlessFullscreen) prevFullscreenResolution = new(-1, -1);
                        RestoreWindow();
                    }
                }
                else
                {
                    if (wasFullscreen)
                    {
                        if (prevFullscreenResolution.Positive)
                        {
                            ActivateFullscreen((int)prevFullscreenResolution.Width, (int)prevFullscreenResolution.Height);
                        }
                        else
                        {
                            ActivateBorderlessFullscreen();
                        }
                        wasFullscreen = false;
                    }   
                }
            }

        }
        if (cur.HasAlwaysRunChanged(windowConfigFlags))
        {
            OnWindowAlwaysRunChanged?.Invoke(cur.AlwaysRun);
        }
        if (cur.HasMousePassThroughChanged(windowConfigFlags))
        {
            OnWindowMousePassThroughChanged?.Invoke(cur.MousePassThrough);
        }

        if (cur.HasVSyncChanged(windowConfigFlags))
        {
            OnWindowVSyncChanged?.Invoke(cur.VSync);
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
            if (DisplayState == WindowDisplayState.Normal) windowSize = CurScreenSize;
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

        MouseOnScreen = Raylib.IsCursorOnScreen() || (Raylib.IsWindowFocused() && ScreenArea.ContainsPoint(MousePosition));

        var curCursorState = GetCurCursorState();

        if (!MouseOnScreen || Raylib.IsWindowState(ConfigFlags.MinimizedWindow))//fullscreen to minimize fix for enabling/showing os cursor
        {
            if (cursorState.OnScreen)//prev state
            {
                OnMouseLeftScreen?.Invoke();
                if (wasMouseVisible == null) wasMouseVisible = cursorState.Visible;
                if (wasMouseEnabled == null) wasMouseEnabled = cursorState.Enabled;

                if (!mouseEnabled)
                {
                    Raylib.EnableCursor();
                    mouseEnabled = true;
                }

                if (!mouseEnabled)
                {
                    Raylib.ShowCursor();
                    mouseVisible = true;
                }
            }
        }
        else
        {
            if (!cursorState.OnScreen) //prev state
            {
                OnMouseEnteredScreen?.Invoke();
                if (wasMouseVisible != null) MouseVisible = (bool)wasMouseVisible;
                if (wasMouseEnabled != null) MouseEnabled = (bool)wasMouseEnabled;
                // if (wasMouseVisible != null && wasMouseVisible == false) MouseVisible = false;
                // if (wasMouseEnabled != null && wasMouseEnabled == false) MouseEnabled = false;

                wasMouseVisible = null;
                wasMouseEnabled = null;
            }
        }

        if (MouseOnScreen)
        {
            if (curCursorState.Visible && !cursorState.Visible) OnMouseVisibilityChanged?.Invoke(false);
            else if (!curCursorState.Visible && cursorState.Visible) OnMouseVisibilityChanged?.Invoke(true);

            if (curCursorState.Enabled && !cursorState.Enabled) OnMouseEnabledChanged?.Invoke(false);
            else if (!curCursorState.Enabled && cursorState.Enabled) OnMouseEnabledChanged?.Invoke(true);
        }

        cursorState = curCursorState;
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
        if (DisplayState == WindowDisplayState.Fullscreen)
        {
            Raylib.SetWindowMonitor(monitor.Index);
            Raylib.SetWindowSize(monitor.Dimensions.Width, monitor.Dimensions.Height);
            Raylib.SetWindowPosition((int)monitor.Position.X, (int)monitor.Position.Y);
            Raylib.SetWindowState(ConfigFlags.FullscreenMode);
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

        // PrevFullscreenDisplayState = new(windowDimensions, new(x, y), WindowDisplayState.Normal);
        // PrevMinimizedDisplayState = new(windowDimensions, new(x, y), WindowDisplayState.Normal);
        prevDisplayStateWindowDimensions = windowDimensions;
        prevDisplayStateWindowPosition = new(x, y);
        // prevDisplayStateChangeWindowSize = windowDimensions;
        // prevDisplayStateChangeDisplayState = WindowDisplayState.Normal;
        // prevDisplayStateChangeWindowPosition =new(x, y);

        if (DisplayState != WindowDisplayState.Fullscreen)
        {
            Raylib.SetWindowPosition(x, y);
            Raylib.SetWindowSize(windowDimensions.Width, windowDimensions.Height);
        }

        ResetMousePosition();
        OnMonitorChanged?.Invoke(monitor);
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
        get => mouseEnabled;
        set
        {

            if (!MouseOnScreen)
            {
                wasMouseEnabled = value;
                return;
            }

            if (value == mouseEnabled) return;
            mouseEnabled = value;
            if(mouseEnabled)Raylib.EnableCursor();
            else Raylib.DisableCursor();
        }
    }
    private bool mouseEnabled = true;
    private bool mouseVisible = true;
    
    /// <summary>
    /// Gets or sets whether the mouse cursor is visible.
    /// </summary>
    public bool MouseVisible
    {
        get => mouseVisible;
        set
        {
            if (!MouseOnScreen)
            {
                wasMouseVisible = value;
                return;
            }

            if (value == mouseVisible) return;
            mouseVisible = value;
            if(value) Raylib.ShowCursor();
            else Raylib.HideCursor();
        }
    }

    /// <summary>
    /// Resets the mouse position to the center of the window.
    /// </summary>
    public void ResetMousePosition()
    {
        var center = WindowPosition / 2 + WindowSize.ToVector2() / 2; // CurScreenSize.ToVector2() / 2;
        Raylib.SetMousePosition((int)center.X, (int)center.Y);
    }

    /// <summary>
    /// Gets the current cursor state.
    /// </summary>
    /// <returns>The current <see cref="CursorState"/>.</returns>
    private CursorState GetCurCursorState()
    {
        return new(MouseVisible, MouseEnabled, MouseOnScreen);
    }
    #endregion
}