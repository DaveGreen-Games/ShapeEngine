using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Screen;

namespace ShapeEngine.Core;


public sealed class GameWindow
{
    
    #region Structs
    private readonly struct WindowConfigFlags
    {
        public static WindowConfigFlags Get() => new();
        public static WindowConfigFlags GetAllFalse() => new WindowConfigFlags(false);
        public static WindowConfigFlags GetAllTrue() => new WindowConfigFlags(true);

        
        /// <summary>
        /// Sets all flags to value.
        /// </summary>
        /// <param name="value"></param>
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
        /// Gets the current values from raylib.
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
        
        public readonly bool Minimized;
        public readonly bool Maximized;
        public readonly bool Fullscreen;
        public readonly bool Hidden;
        public readonly bool Focused;
        
        public readonly bool Undecorated;
        public readonly bool Resizable;
        public readonly bool Topmost;
        public readonly bool AlwaysRun;
        public readonly bool MousePassThrough;
        public readonly bool VSync;

        public bool HasUndecoratedChanged(WindowConfigFlags other) => Undecorated != other.Undecorated;
        public bool HasResizableChanged(WindowConfigFlags other) => Resizable != other.Resizable;
        public bool HasTopmostChanged(WindowConfigFlags other) => Topmost != other.Topmost;
        public bool HasFocusedChanged(WindowConfigFlags other) => Focused != other.Focused;
        public bool HasAlwaysRunChanged(WindowConfigFlags other) => AlwaysRun != other.AlwaysRun;
        public bool HasMousePassThroughChanged(WindowConfigFlags other) => MousePassThrough != other.MousePassThrough;
        public bool HasVSyncChanged(WindowConfigFlags other) => VSync != other.VSync;
        
        public bool HasMinimizedChanged(WindowConfigFlags other) => Minimized != other.Minimized;
        public bool HasMaximizedChanged(WindowConfigFlags other) => Maximized != other.Maximized;
        public bool HasFullscreenChanged(WindowConfigFlags other) => Fullscreen != other.Fullscreen;
        public bool HasHiddenChanged(WindowConfigFlags other) => Hidden != other.Hidden;
        
    }
    private readonly struct CursorState
    {
        public readonly bool Visible;
        public readonly bool Enabled;
        public readonly bool OnScreen;

        public CursorState()
        {
            Visible = true;
            Enabled = true;
            OnScreen = true;
        }

        public CursorState(bool visible, bool enabled, bool onScreen)
        {
            Visible = visible;
            Enabled = enabled;
            OnScreen = onScreen;
        }
    }
    
    #endregion
    
    #region Events

    public event Action? OnMouseLeftScreen;
    public event Action? OnMouseEnteredScreen;
    public event Action<bool>? OnMouseVisibilityChanged;
    public event Action<bool>? OnMouseEnabledChanged;
    
    public event Action<DimensionConversionFactors>? OnWindowSizeChanged;
    public event Action<Vector2, Vector2>? OnWindowPositionChanged;
    public event Action<MonitorInfo>? OnMonitorChanged;

    public event Action<bool>? OnWindowFocusChanged;
    public event Action<bool>? OnWindowFullscreenChanged;
    public event Action<bool>? OnWindowMaximizeChanged;
    public event Action<bool>? OnWindowMinimizedChanged;
    public event Action<bool>? OnWindowHiddenChanged;
    public event Action<bool>? OnWindowTopmostChanged;
    
    public event Action<bool>? OnWindowUndecoratedChanged;
    public event Action<bool>? OnWindowResizableChanged;
    public event Action<bool>? OnWindowAlwaysRunChanged;
    public event Action<bool>? OnWindowMousePassThroughChanged;
    public event Action<bool>? OnWindowVSyncChanged;
    
    #endregion

    #region Static Members

    public static GameWindow Instance { get; private set; } = null!;
    // public static bool IsMouseOnScreen { get; private set; }
    // public static bool IsWindowFocused => Raylib.IsWindowFocused();
    // public static bool IsWindowTopmost => Raylib.IsWindowState(ConfigFlags.TopmostWindow);

    #endregion
    
    #region Public Members
    public MonitorDevice Monitor { get; private set; }
    public Dimensions CurScreenSize { get; private set; }
    public Dimensions WindowMinSize { get; private set; }
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
    public Vector2 WindowPosition { get; private set; }

    public WindowDisplayState DisplayState { get; private set; }
    public WindowBorder WindowBorder { get; private set; }
    
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
    public int Fps => Raylib.GetFPS();
    public bool VSync
    {
        get =>Raylib.IsWindowState(ConfigFlags.VSyncHint);
        
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

    private bool mouseOnScreen = false;
    public bool MouseOnScreen
    {
        get => mouseOnScreen;
        private set
        {
            mouseOnScreen = value;
            // IsMouseOnScreen = value;
        }
    }
    public Rect ScreenArea { get; private set; }

    public bool FullscreenAutoRestoring { get; set; }
    #endregion

    #region Private Members
    
    private int fpsLimit = 60;
    private int minFramerate;
    private int maxFramerate;
    private Dimensions windowSize = new();
    
    private bool? wasMouseEnabled = null;
    private bool? wasMouseVisible = null;

    private CursorState cursorState;
    private WindowConfigFlags windowConfigFlags;
    private bool wasMaximized = false;
    
    private Dimensions prevDisplayStateWindowDimensions = new(128, 128);
    private Vector2 prevDisplayStateWindowPosition = new(128, 128);
    
    private Size prevFullscreenResolution = new(-1, -1);
    private bool wasFullscreen = false;
    
    #endregion

    #region Internal Methods
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

        Raylib.SetWindowOpacity(windowSettings.WindowOpacity);
        windowConfigFlags = WindowConfigFlags.Get();

        CurrentGameWindowInstance = this;
    }
    
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
        
        if (MouseVisible == Raylib.IsCursorHidden()) MouseVisible = !Raylib.IsCursorHidden();
        
    }
    internal void MoveMouse(Vector2 mousePos)
    {
        mousePos = Vector2.Clamp(mousePos, new Vector2(0, 0), CurScreenSize.ToVector2());
        // lastControlledMousePosition = mousePos;
        // mouseControlled = true;

        var mx = (int)MathF.Round(mousePos.X);
        var my = (int)MathF.Round(mousePos.Y);
        Raylib.SetMousePosition(mx, my);
    }
    internal void Close()
    {
        
    }
    #endregion
    
    #region Window
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
    public void CenterWindow()
    {
        if (DisplayState == WindowDisplayState.Fullscreen) return;
        var monitor = Monitor.CurMonitor();

        int winPosX = monitor.Width / 2 - windowSize.Width / 2;
        int winPosY = monitor.Height / 2 - windowSize.Height / 2;
        Raylib.SetWindowPosition(winPosX + (int)monitor.Position.X, winPosY + (int)monitor.Position.Y);
        ResetMousePosition();
    }
    public void ResizeWindow(Dimensions newDimensions) => WindowSize = newDimensions;
    public void ResetWindow()
    {
        RestoreWindow();
        WindowSize = Monitor.CurMonitor().Dimensions / 2;
    }
    
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
        CalculateCurScreenSize();
        ResetMousePosition();
        return true;
    }
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

    public bool SetWindowTopmost(bool topmost)
    {
        if (Raylib.IsWindowState(ConfigFlags.TopmostWindow) == topmost) return false;
        if(topmost) Raylib.SetWindowState(ConfigFlags.TopmostWindow);
        else Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
        return true;
    }
    
    public bool IsWindowBorderlessFullscreen() => DisplayState == WindowDisplayState.BorderlessFullscreen;
    public bool IsWindowFullscreen() => DisplayState == WindowDisplayState.Fullscreen;
    public bool IsWindowMaximized() => DisplayState == WindowDisplayState.Maximized;
    public bool IsWindowMinimized() => DisplayState == WindowDisplayState.Minimized;
    public bool IsWindowNormal() => DisplayState == WindowDisplayState.Normal;
    
    public bool IsWindowFocused => Raylib.IsWindowFocused();
    public bool IsWindowTopmost => Raylib.IsWindowState(ConfigFlags.TopmostWindow);
    
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
    
    #endregion
    
    #region Private Methods

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
    private void CalculateCurScreenSize()
    {
        if(DisplayState == WindowDisplayState.Fullscreen)
        {
            int w = Raylib.GetScreenWidth();
            int h = Raylib.GetScreenHeight();
            CurScreenSize = new(w, h);
        }
        else if (DisplayState == WindowDisplayState.BorderlessFullscreen)
        {
            int monitor = Raylib.GetCurrentMonitor();
            int mw = Raylib.GetMonitorWidth(monitor);
            int mh = Raylib.GetMonitorHeight(monitor);
            // var scaleFactor = Game.IsOSX() ? Raylib.GetWindowScaleDPI() : new Vector2(1f, 1f);
            // var scaleX = (int)scaleFactor.X;
            // var scaleY = (int)scaleFactor.Y;
            // CurScreenSize = new(mw * scaleX, mh * scaleY);
            CurScreenSize = new(mw , mh );
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

    public Vector2 MousePosition
    {
        get => Raylib.GetMousePosition();
        set => Raylib.SetMousePosition((int)value.X, (int)value.Y);
    }

    public Vector2 MouseDelta => Raylib.GetMouseDelta();
    public float MouseX => MousePosition.X;
    public float MouseY => MousePosition.Y;

    private bool mouseEnabled = true;
    private bool mouseVisible = true;
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

    public void ResetMousePosition()
    {
        var center = WindowPosition / 2 + WindowSize.ToVector2() / 2; // CurScreenSize.ToVector2() / 2;
        Raylib.SetMousePosition((int)center.X, (int)center.Y);
    }
    
    private CursorState GetCurCursorState()
    {
        return new(MouseVisible, MouseEnabled, MouseOnScreen);
    }
    #endregion
    
}





    // /// <summary>
    // /// Stores prev fullscreen state when window loses focus while in fullscreen
    // /// </summary>
    // private bool wasFullscreen = false;
        
    // private Vector2 lastControlledMousePosition = new();
    // private bool mouseControlled = false;
    // private bool focusLostFullscreen = false;
    // private WindowFlagState windowFlagState;
    // private WindowConfigFlags prevWindowConfigFlags;
    // private Dimensions prevDisplayStateChangeWindowSize = new(128, 128);
    // private Vector2 prevDisplayStateChangeWindowPosition = new(0);
    // private WindowDisplayState prevDisplayStateChangeDisplayState = WindowDisplayState.Normal;

    // public WindowDisplayState CurrentDisplayState => displayState;
    // public WindowBorder CurrentBorder => windowBorder;
    // private PrevDisplayStateInfo PrevFullscreenDisplayState = new(new(128, 128), new(), WindowDisplayState.Normal);
    // private PrevDisplayStateInfo PrevMinimizedDisplayState = new(new(128, 128), new(), WindowDisplayState.Normal);
        
    // public WindowDisplayState DisplayState { get; private set; }
    // public WindowBorder WindowBorder { get; private set; }
    // private PrevDisplayStateInfo displayStateInfo = new(new(128, 128), new(), WindowDisplayState.Normal);

    //display state is always what it is set to
    //prev state will always be WindowDisplayState.Normal
    //if you go from fullscreen to minimized the prev state will not be fullscreen anymore
    //therefore restore window always restores to normal and not any other display state

    // private WindowFlagState GetCurWindowFlagState()
    // {
    //     var fullscreen = Raylib.IsWindowFullscreen();
    //     var maximized = Raylib.IsWindowMaximized();
    //     var minimized = Raylib.IsWindowMinimized();
    //     var hidden = Raylib.IsWindowHidden();
    //     var focused = Raylib.IsWindowFocused();
    //     var topmost = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
    //     return new(minimized, maximized, fullscreen, hidden, focused, topmost);
    // }
    
    /*
    private void CheckForWindowFlagChanges()
    {
        // Console.WriteLine($"--------Minimized: {Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED)}");
        var mousePos = MousePosition;
        if (mouseControlled) mousePos = lastControlledMousePosition;
        mouseControlled = false;
        MouseOnScreen = false;
        var curWindowFlagState = new WindowFlagState();
        
        bool windowMousePassthrough = Raylib.IsWindowState(ConfigFlags.MousePassthroughWindow);
        bool windowTransparent = Raylib.IsWindowState(ConfigFlags.TransparentWindow);
            
        if (curWindowFlagState is { Topmost: true, Focused: true, Hidden: false, Minimized: false }
            && !windowTransparent && !windowMousePassthrough)
        {
            if (displayState == WindowDisplayState.Fullscreen || Raylib.IsCursorOnScreen() || ScreenArea.ContainsPoint(mousePos)) MouseOnScreen = true;
        }
        
        if (curWindowFlagState.Focused && !windowFlagState.Focused)
        {
            OnWindowFocusChanged?.Invoke(true);
            Raylib.SetWindowState(ConfigFlags.TopmostWindow);
            Raylib.ClearWindowState(ConfigFlags.HiddenWindow);
            if (focusLostFullscreen)
            {
                focusLostFullscreen = false;
                DisplayState = WindowDisplayState.Fullscreen;
            }
            
        }
        else if (!curWindowFlagState.Focused && windowFlagState.Focused)
        {
            OnWindowFocusChanged?.Invoke(false);
            if (Raylib.IsWindowFullscreen())
            {
                DisplayState = WindowDisplayState.Normal;
                focusLostFullscreen = true;
            }
            Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
        }
        
        if (curWindowFlagState.Minimized && !windowFlagState.Minimized)
        {
            OnWindowMinimizedChanged?.Invoke(true);
            displayState = WindowDisplayState.Minimized;
        }
        else if (!curWindowFlagState.Minimized && windowFlagState.Minimized)
        {
            OnWindowMinimizedChanged?.Invoke(false);
            DisplayState = WindowDisplayState.Normal; //works for some reason....
        }

        if (curWindowFlagState.Maximized && !windowFlagState.Maximized)
        {
            OnWindowMaximizeChanged?.Invoke(true);
            displayState = WindowDisplayState.Maximized;
        }
        else if (!curWindowFlagState.Maximized && windowFlagState.Maximized)
        {
            OnWindowMaximizeChanged?.Invoke(false);
            if (Raylib.IsWindowState(ConfigFlags.MinimizedWindow)) displayState = WindowDisplayState.Minimized;
            else if (Raylib.IsWindowState(ConfigFlags.FullscreenMode))
                displayState = WindowDisplayState.Fullscreen;
            else displayState = WindowDisplayState.Normal;
        }
            
        if (curWindowFlagState.Fullscreen && !windowFlagState.Fullscreen)
        {
            OnWindowFullscreenChanged?.Invoke(true);
            displayState = WindowDisplayState.Fullscreen;
        }
        else if (!curWindowFlagState.Fullscreen && windowFlagState.Fullscreen)
        {
            OnWindowFullscreenChanged?.Invoke(false);
            if (Raylib.IsWindowState(ConfigFlags.MinimizedWindow)) displayState = WindowDisplayState.Minimized;
            else if (Raylib.IsWindowState(ConfigFlags.MaximizedWindow)) displayState = WindowDisplayState.Maximized;
            else displayState = WindowDisplayState.Normal;
        }
        
        
        if (curWindowFlagState.Topmost && !windowFlagState.Topmost)
        {
            OnWindowTopmostChanged?.Invoke(true);
            Raylib.ClearWindowState(ConfigFlags.UnfocusedWindow);
            Raylib.ClearWindowState(ConfigFlags.HiddenWindow);
        }
        else if (!curWindowFlagState.Topmost && windowFlagState.Topmost)
        {
            OnWindowTopmostChanged?.Invoke(false);
            Raylib.SetWindowState(ConfigFlags.UnfocusedWindow);
        }
        
        if (curWindowFlagState.Hidden && !windowFlagState.Hidden)
        {
            OnWindowHiddenChanged?.Invoke(true);
            Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
            Raylib.SetWindowState(ConfigFlags.UnfocusedWindow);
        }
        else if (!curWindowFlagState.Hidden && windowFlagState.Hidden)
        {
            OnWindowHiddenChanged?.Invoke(false);
            Raylib.SetWindowState(ConfigFlags.TopmostWindow);
            Raylib.ClearWindowState(ConfigFlags.UnfocusedWindow);
        }
        
        windowFlagState = curWindowFlagState;
    }
    */
    
    /*
private readonly struct PrevDisplayStateInfo
{
    public readonly Dimensions WindowSize;
    public readonly Vector2 WindowPosition;
    public readonly WindowDisplayState DisplayState;

    public PrevDisplayStateInfo(Dimensions windowSize, Vector2 windowPosition, WindowDisplayState displayState)
    {
        this.WindowPosition = windowPosition;
        this.WindowSize = windowSize;
        this.DisplayState = displayState;
    }
}
*/
    
    // private readonly struct WindowFlagState
    // {
    //     public readonly bool Minimized;
    //     public readonly bool Maximized;
    //     public readonly bool Fullscreen;
    //     public readonly bool Hidden;
    //     public readonly bool Focused;
    //     public readonly bool Topmost;
    //
    //     public WindowFlagState()
    //     {
    //         Fullscreen = Raylib.IsWindowFullscreen();
    //         Maximized = Raylib.IsWindowMaximized();
    //         Minimized = Raylib.IsWindowMinimized();
    //         Hidden = Raylib.IsWindowHidden();
    //         Focused = Raylib.IsWindowFocused();
    //         Topmost = Raylib.IsWindowState(ConfigFlags.TopmostWindow);
    //
    //     }
    // }

    // public bool HideWindow()
    // {
    //     return false;
    // }

    /*
     public WindowDisplayState DisplayState
    {
        get => displayState;
        set
        {
            if (value == displayState) return;
            // StartOpacityLerp();
            var newState = value;
            if (value == WindowDisplayState.Normal)
            {
                // Raylib.SetWindowState(ConfigFlags.TopmostWindow);
                if (displayState == WindowDisplayState.Minimized)
                {
                    Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
                    
                    if (PrevMinimizedDisplayState.DisplayState == WindowDisplayState.Fullscreen)
                    {
                        var mDim = Monitor.CurMonitor().Dimensions;
                        Raylib.SetWindowSize(mDim.Width, mDim.Height);
                        Raylib.SetWindowState(ConfigFlags.FullscreenMode);
                        // if (Game.IsOSX())
                        // {
                        //     var mDim = Monitor.CurMonitor().Dimensions;
                        //     Raylib.SetWindowSize(mDim.Width, mDim.Height);
                        //     Raylib.SetWindowState(ConfigFlags.FullscreenMode);
                        // }
                        // else Raylib.ToggleBorderlessWindowed();
                        
                        newState = WindowDisplayState.Fullscreen;
                    }

                    else if (PrevMinimizedDisplayState.DisplayState == WindowDisplayState.Maximized)
                    {
                        Raylib.SetWindowState(ConfigFlags.MaximizedWindow);
                        
                        newState = WindowDisplayState.Maximized;
                    }
                    else if (PrevMinimizedDisplayState.DisplayState == WindowDisplayState.Normal)
                    {
                        Raylib.SetWindowSize(PrevMinimizedDisplayState.WindowSize.Width, PrevMinimizedDisplayState.WindowSize.Height);
                        Raylib.SetWindowPosition((int)PrevMinimizedDisplayState.WindowPosition.X, (int)PrevMinimizedDisplayState.WindowPosition.Y);
                    }
                    
                }
                else if (displayState == WindowDisplayState.Maximized)
                {
                    Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
                }
                else if (displayState == WindowDisplayState.Fullscreen)
                {
                    Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
                    Raylib.SetWindowSize(PrevFullscreenDisplayState.WindowSize.Width, PrevFullscreenDisplayState.WindowSize.Height);
                    Raylib.SetWindowPosition((int)PrevFullscreenDisplayState.WindowPosition.X, (int)PrevFullscreenDisplayState.WindowPosition.Y);

                    if (PrevFullscreenDisplayState.DisplayState == WindowDisplayState.Minimized)
                    {
                        Raylib.SetWindowState(ConfigFlags.MinimizedWindow);
                        newState = WindowDisplayState.Minimized;
                    }

                    else if (PrevFullscreenDisplayState.DisplayState == WindowDisplayState.Maximized)
                    {
                        Raylib.SetWindowState(ConfigFlags.MaximizedWindow);
                        newState = WindowDisplayState.Maximized;
                    }

                    // if (Game.IsOSX())
                    // {
                    //     
                    // }
                    // else
                    // {
                    //     Raylib.ToggleBorderlessWindowed();
                    //     if (PrevFullscreenDisplayState.DisplayState == WindowDisplayState.Minimized)
                    //     {
                    //         newState = WindowDisplayState.Minimized;
                    //     }
                    //
                    //     else if (PrevFullscreenDisplayState.DisplayState == WindowDisplayState.Maximized)
                    //     {
                    //         newState = WindowDisplayState.Maximized;
                    //     }
                    // }
                    
                }
            }
            else if (value == WindowDisplayState.Maximized)
            {
                // Raylib.SetWindowState(ConfigFlags.TopmostWindow);
                
                if (displayState == WindowDisplayState.Fullscreen)
                {
                    Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
                    Raylib.SetWindowSize(PrevFullscreenDisplayState.WindowSize.Width, PrevFullscreenDisplayState.WindowSize.Height);
                    Raylib.SetWindowPosition((int)PrevFullscreenDisplayState.WindowPosition.X, (int)PrevFullscreenDisplayState.WindowPosition.Y);
                    
                    // if (Game.IsOSX())
                    // {
                    //     Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
                    //     Raylib.SetWindowSize(PrevFullscreenDisplayState.WindowSize.Width, PrevFullscreenDisplayState.WindowSize.Height);
                    //     Raylib.SetWindowPosition((int)PrevFullscreenDisplayState.WindowPosition.X, (int)PrevFullscreenDisplayState.WindowPosition.Y);
                    // }
                    // else
                    // {
                    //     Raylib.ToggleBorderlessWindowed();
                    // }
                
                }
                else if (displayState == WindowDisplayState.Minimized)Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
                
                Raylib.SetWindowState(ConfigFlags.MaximizedWindow);
            }
            else if (value == WindowDisplayState.Minimized)
            {
                // Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
                PrevMinimizedDisplayState = new(CurScreenSize, Raylib.GetWindowPosition(), DisplayState);
                
                if (displayState == WindowDisplayState.Fullscreen)
                {
                    Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
                    Raylib.SetWindowSize(PrevFullscreenDisplayState.WindowSize.Width, PrevFullscreenDisplayState.WindowSize.Height);
                    Raylib.SetWindowPosition((int)PrevFullscreenDisplayState.WindowPosition.X, (int)PrevFullscreenDisplayState.WindowPosition.Y);
                    // if (Game.IsOSX())
                    // {
                    //     Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
                    //     Raylib.SetWindowSize(PrevFullscreenDisplayState.WindowSize.Width, PrevFullscreenDisplayState.WindowSize.Height);
                    //     Raylib.SetWindowPosition((int)PrevFullscreenDisplayState.WindowPosition.X, (int)PrevFullscreenDisplayState.WindowPosition.Y);
                    // }
                    // else Raylib.ToggleBorderlessWindowed();
                    
                
                }
                else if (displayState == WindowDisplayState.Maximized)
                {
                    Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
                }
                
                Raylib.SetWindowState(ConfigFlags.MinimizedWindow);
                
            }
            else if (value == WindowDisplayState.Fullscreen)
            {
                PrevFullscreenDisplayState = new(CurScreenSize, Raylib.GetWindowPosition(), displayState);

                if (displayState == WindowDisplayState.Maximized) Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
                else if (displayState == WindowDisplayState.Minimized)
                {
                    Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
                }
                
                var mDim = Monitor.CurMonitor().Dimensions;
                var dpi = Raylib.GetWindowScaleDPI();
                Raylib.SetWindowSize(mDim.Width * (int)dpi.X, mDim.Height * (int)dpi.Y);
                Raylib.SetWindowState(ConfigFlags.FullscreenMode);
                
                // if (Game.IsOSX())
                // {
                //     if (displayState == WindowDisplayState.Maximized) Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
                //     else if (displayState == WindowDisplayState.Minimized)
                //     {
                //         Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
                //     }
                //
                //     var mDim = Monitor.CurMonitor().Dimensions;
                //     var dpi = Raylib.GetWindowScaleDPI();
                //     Raylib.SetWindowSize(mDim.Width * (int)dpi.X, mDim.Height * (int)dpi.Y);
                //     Raylib.SetWindowState(ConfigFlags.FullscreenMode);
                // }
                // else
                // {
                //     Raylib.ToggleBorderlessWindowed();
                // }
            }
            
            displayState = newState;
            if(displayState != WindowDisplayState.Minimized) ResetMousePosition();
        }
    }
    public WindowBorder WindowBorder
    {
        get => windowBorder;
        set
        {
            if (value == windowBorder) return;

            if (value == WindowBorder.Fixed)
            { 
                if(windowBorder == WindowBorder.Resizabled) Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
                else if(windowBorder == WindowBorder.Undecorated) Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
                
            }
            else if (value == WindowBorder.Resizabled)
            {
                if(windowBorder == WindowBorder.Undecorated) Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
                Raylib.SetWindowState(ConfigFlags.ResizableWindow);
            }
            else if(value == WindowBorder.Undecorated)
            {
                if(windowBorder == WindowBorder.Resizabled) Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
                Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
            }

            windowBorder = value;
        }
    }
    */
    