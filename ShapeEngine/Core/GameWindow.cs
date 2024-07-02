using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Screen;

namespace ShapeEngine.Core;

public sealed class GameWindow
{
    #region Structs
    private readonly struct WindowFlagState
    {
        public readonly bool Minimized;
        public readonly bool Maximized;
        public readonly bool Fullscreen;
        public readonly bool Hidden;
        public readonly bool Focused;
        public readonly bool Topmost;

        public WindowFlagState()
        {
            Fullscreen = Raylib.IsWindowFullscreen();
            Maximized = Raylib.IsWindowMaximized();
            Minimized = Raylib.IsWindowMinimized();
            Hidden = Raylib.IsWindowHidden();
            Focused = Raylib.IsWindowFocused();
            Topmost = Raylib.IsWindowState(ConfigFlags.TopmostWindow);
        }
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
    #endregion
    
    #region Events

    public event Action? OnMouseLeftScreen;
    public event Action? OnMouseEnteredScreen;
    public event Action<DimensionConversionFactors>? OnWindowSizeChanged;
    public event Action<Vector2, Vector2>? OnWindowPositionChanged;
    public event Action<MonitorInfo>? OnMonitorChanged;

    public event Action<bool>? OnMouseVisibilityChanged;
    public event Action<bool>? OnMouseEnabledChanged;
    
    
    public event Action<bool>? OnWindowFocusChanged;
    public event Action<bool>? OnWindowFullscreenChanged;
    public event Action<bool>? OnWindowMaximizeChanged;
    public event Action<bool>? OnWindowMinimizedChanged;
    public event Action<bool>? OnWindowHiddenChanged;
    public event Action<bool>? OnWindowTopmostChanged;
    #endregion

    #region Public Members
    // public bool MouseOnScreen { get; private set; }
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

    private int minFramerate;
    private int maxFramerate;
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
            IsMouseOnScreen = value;
        }
    }
    public static bool IsMouseOnScreen { get; private set; }
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
                Raylib.SetWindowState(ConfigFlags.TopmostWindow);
                if (displayState == WindowDisplayState.Minimized)
                {
                    Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
                    
                    if (PrevMinimizedDisplayState.DisplayState == WindowDisplayState.Fullscreen)
                    {
                        if (Game.IsOSX())
                        {
                            var mDim = Monitor.CurMonitor().Dimensions;
                            Raylib.SetWindowSize(mDim.Width, mDim.Height);
                            Raylib.SetWindowState(ConfigFlags.FullscreenMode);
                        }
                        else Raylib.ToggleBorderlessWindowed();
                        
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
                    if (Game.IsOSX())
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
                    }
                    else
                    {
                        Raylib.ToggleBorderlessWindowed();
                        if (PrevFullscreenDisplayState.DisplayState == WindowDisplayState.Minimized)
                        {
                            newState = WindowDisplayState.Minimized;
                        }

                        else if (PrevFullscreenDisplayState.DisplayState == WindowDisplayState.Maximized)
                        {
                            newState = WindowDisplayState.Maximized;
                        }
                    }
                    
                }
            }
            else if (value == WindowDisplayState.Maximized)
            {
                Raylib.SetWindowState(ConfigFlags.TopmostWindow);
                
                if (displayState == WindowDisplayState.Fullscreen)
                {
                    if (Game.IsOSX())
                    {
                        Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
                        Raylib.SetWindowSize(PrevFullscreenDisplayState.WindowSize.Width, PrevFullscreenDisplayState.WindowSize.Height);
                        Raylib.SetWindowPosition((int)PrevFullscreenDisplayState.WindowPosition.X, (int)PrevFullscreenDisplayState.WindowPosition.Y);
                    }
                    else
                    {
                        Raylib.ToggleBorderlessWindowed();
                    }
                    
                
                }
                else if (displayState == WindowDisplayState.Minimized)Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
                
                Raylib.SetWindowState(ConfigFlags.MaximizedWindow);
            }
            else if (value == WindowDisplayState.Minimized)
            {
                Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
                PrevMinimizedDisplayState = new(CurScreenSize, Raylib.GetWindowPosition(), DisplayState);
                
                if (displayState == WindowDisplayState.Fullscreen)
                {
                    if (Game.IsOSX())
                    {
                        Raylib.ClearWindowState(ConfigFlags.FullscreenMode);
                        Raylib.SetWindowSize(PrevFullscreenDisplayState.WindowSize.Width, PrevFullscreenDisplayState.WindowSize.Height);
                        Raylib.SetWindowPosition((int)PrevFullscreenDisplayState.WindowPosition.X, (int)PrevFullscreenDisplayState.WindowPosition.Y);
                    }
                    else Raylib.ToggleBorderlessWindowed();
                    
                
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
                Raylib.SetWindowState(ConfigFlags.TopmostWindow);

                if (Game.IsOSX())
                {
                    if (displayState == WindowDisplayState.Maximized) Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
                    else if (displayState == WindowDisplayState.Minimized)
                    {
                        Raylib.ClearWindowState(ConfigFlags.MinimizedWindow);
                    }
                
                    var mDim = Monitor.CurMonitor().Dimensions;
                    Raylib.SetWindowSize(mDim.Width, mDim.Height);
                    Raylib.SetWindowState(ConfigFlags.FullscreenMode);
                }
                else
                {
                    Raylib.ToggleBorderlessWindowed();
                }
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
    public Rect ScreenArea { get; private set; }
    
    
    #endregion

    #region Private Members

    private WindowDisplayState displayState = WindowDisplayState.Normal;
    private WindowBorder windowBorder = WindowBorder.Fixed;
    
    // private Dimensions prevDisplayStateChangeWindowSize = new(128, 128);
    // private Vector2 prevDisplayStateChangeWindowPosition = new(0);
    // private WindowDisplayState prevDisplayStateChangeDisplayState = WindowDisplayState.Normal;
    private PrevDisplayStateInfo PrevFullscreenDisplayState = new(new(128, 128), new(), WindowDisplayState.Normal);
    private PrevDisplayStateInfo PrevMinimizedDisplayState = new(new(128, 128), new(), WindowDisplayState.Normal);
    
    private int fpsLimit = 60;
    private Dimensions windowSize = new();
    
    private bool? wasMouseEnabled = null;
    private bool? wasMouseVisible = null;
    
    private Vector2 lastControlledMousePosition = new();
    private bool mouseControlled = false;

    private CursorState cursorState;
    private WindowFlagState windowFlagState;
    private bool focusLostFullscreen = false;
    #endregion

    #region Internal

    // private float opacityLerpTimer = 0;
    // private const float opacityLerpDuration = 0.25f;
    //
    // private void StartOpacityLerp()
    // {
    //     Raylib.SetWindowOpacity(0f);
    //     opacityLerpTimer = opacityLerpDuration;
    // }
    //
    // private void LerpOpacitiy(float dt)
    // {
    //     if (opacityLerpTimer > 0)
    //     {
    //         opacityLerpTimer -= dt;
    //
    //         if (opacityLerpTimer <= 0) opacityLerpTimer = 0f;
    //
    //         float f = 1f - (opacityLerpTimer / opacityLerpDuration);
    //         Raylib.SetWindowOpacity(f);
    //     }
    // }
    internal GameWindow(WindowSettings windowSettings)
    {
        if(windowSettings.Msaa4x) Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(0, 0, windowSettings.Title);
        Raylib.SetWindowOpacity(0f);
        
        Monitor = new MonitorDevice();
        SetupWindowDimensions();
        WindowMinSize = windowSettings.WindowMinSize;
        Raylib.SetWindowMinSize(WindowMinSize.Width, WindowMinSize.Height);

        Raylib.SetWindowState(ConfigFlags.AlwaysRunWindow);
        
        if (windowSettings.Focused)
        {
            Raylib.SetWindowState(ConfigFlags.TopmostWindow);
            Raylib.ClearWindowState(ConfigFlags.UnfocusedWindow);
        }
        else Raylib.SetWindowState(ConfigFlags.UnfocusedWindow);
        
        
        VSync = windowSettings.Vsync;
        MinFramerate = windowSettings.MinFramerate;
        MaxFramerate = windowSettings.MaxFramerate;
        FpsLimit = windowSettings.FrameRateLimit;

        WindowBorder = windowSettings.WindowBorder;
        DisplayState = windowSettings.WindowDisplayState;
        
        if(windowSettings.WindowSize.Width > 0 && windowSettings.WindowSize.Height > 0)
            WindowSize = windowSettings.WindowSize;

        
        if (Monitor.CurMonitor().Index != windowSettings.Monitor && windowSettings.Monitor >= 0)
        {
            SetMonitor(windowSettings.Monitor);
        }
        var screenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
        MouseOnScreen = displayState == WindowDisplayState.Fullscreen || Raylib.IsCursorOnScreen() || ( Raylib.IsWindowFocused() && screenArea.ContainsPoint(Raylib.GetMousePosition()) );

        MouseVisible = windowSettings.MouseVisible;
        MouseEnabled = windowSettings.MouseEnabled;
        
        cursorState = GetCurCursorState();
        windowFlagState = new WindowFlagState(); // GetCurWindowFlagState();

        Raylib.SetWindowOpacity(windowSettings.WindowOpacity);
        // Raylib.ToggleBorderlessWindowed();
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
        
        CheckForWindowFlagChanges();
        CheckForCursorChanges();
        
        //still necessary?
        if (MouseVisible == Raylib.IsCursorHidden()) MouseVisible = !Raylib.IsCursorHidden();
        
    }
    internal void MoveMouse(Vector2 mousePos)
    {
        mousePos = Vector2.Clamp(mousePos, new Vector2(0, 0), CurScreenSize.ToVector2());
        lastControlledMousePosition = mousePos;
        mouseControlled = true;

        var mx = (int)MathF.Round(mousePos.X);
        var my = (int)MathF.Round(mousePos.Y);
        Raylib.SetMousePosition(mx, my);
    }
    internal void Close()
    {
        
    }
    #endregion
    
    #region Setup

    private void SetupWindowDimensions()
    {
        var monitor = Monitor.CurMonitor();
        WindowSize = monitor.Dimensions / 2;
        WindowPosition = Raylib.GetWindowPosition();
        PrevFullscreenDisplayState = new(WindowSize, WindowPosition, displayState);
        PrevMinimizedDisplayState = new(WindowSize, WindowPosition, displayState);
        CalculateCurScreenSize();
    }
    
    private void CalculateCurScreenSize()
    {
        if (displayState == WindowDisplayState.Fullscreen)
        {
            int monitor = Raylib.GetCurrentMonitor();
            int mw = Raylib.GetMonitorWidth(monitor);
            int mh = Raylib.GetMonitorHeight(monitor);
            var scaleFactor = Game.IsOSX() ? Raylib.GetWindowScaleDPI() : new Vector2(1f, 1f);
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
    
    private void CheckForWindowChanges()
    {
        var prev = CurScreenSize;
        CalculateCurScreenSize();
        if (prev != CurScreenSize)
        {
            if (displayState == WindowDisplayState.Normal) windowSize = CurScreenSize;
            // SetConversionFactors();
            var conversion = new DimensionConversionFactors(prev, CurScreenSize);
            OnWindowSizeChanged?.Invoke(conversion);
        }

        var curWindowPosition = Raylib.GetWindowPosition();
        if (curWindowPosition != WindowPosition)
        {
            // prevWindowPosition = WindowPosition;
            WindowPosition = curWindowPosition;
            OnWindowPositionChanged?.Invoke(WindowPosition, curWindowPosition);
        }
    }

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
            // if (displayState == WindowDisplayState.Minimized) DisplayState = WindowDisplayState.Normal;
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

    private void CheckForCursorChanges()
    {
        var curCursorState = GetCurCursorState();
        
        if (!MouseOnScreen || Raylib.IsWindowState(ConfigFlags.MinimizedWindow))//fullscreen to minimize fix for enabling/showing os cursor
        {
            if (cursorState.OnScreen)
            {
                OnMouseLeftScreen?.Invoke();
                if (wasMouseVisible == null) wasMouseVisible = cursorState.Visible;
                if (wasMouseEnabled == null) wasMouseEnabled = cursorState.Enabled;
                MouseEnabled = true;
                MouseVisible = true;
            }
        }
        else
        {
            if (!cursorState.OnScreen)
            {
                OnMouseEnteredScreen?.Invoke();
                if (wasMouseVisible != null && wasMouseVisible == false) MouseVisible = false;
                if (wasMouseEnabled != null && wasMouseEnabled == false) MouseEnabled = false;

                wasMouseVisible = null;
                wasMouseEnabled = null;
            }
        }
        
        if (curCursorState.Visible && !cursorState.Visible) OnMouseVisibilityChanged?.Invoke(false);
        else if (!curCursorState.Visible && cursorState.Visible) OnMouseVisibilityChanged?.Invoke(true);
            
        if (curCursorState.Enabled && !cursorState.Enabled) OnMouseEnabledChanged?.Invoke(false);
        else if (!curCursorState.Enabled && cursorState.Enabled) OnMouseEnabledChanged?.Invoke(true);

        cursorState = curCursorState;
    }
    #endregion
    
    
    #region Window

    public void GoWindowFullscreen() => DisplayState = WindowDisplayState.Fullscreen;
    public void GoWindowMaximize() => DisplayState = WindowDisplayState.Maximized;
    public void GoWindowMinimize() => DisplayState = WindowDisplayState.Minimized;
    public void GoWindowNormal() => DisplayState = WindowDisplayState.Normal;
    public bool IsWindowFullscreen() => displayState == WindowDisplayState.Fullscreen;
    public bool IsWindowMaximized() => displayState == WindowDisplayState.Maximized;
    public bool IsWindowMinimized() => displayState == WindowDisplayState.Minimized;
    public bool IsWindowNormal() => displayState == WindowDisplayState.Normal;
    public void CenterWindow()
    {
        if (displayState == WindowDisplayState.Fullscreen) return;
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
        // if(Raylib.IsWindowMinimized()) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
        // if(Raylib.IsWindowHidden()) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
        // if (Maximized) Maximized = false;
        // else if (Fullscreen) Fullscreen = false;
        DisplayState = WindowDisplayState.Normal;
        WindowSize = Monitor.CurMonitor().Dimensions / 2;
        //ResetMousePosition();
    }
    public float GetScreenPercentage()
     {
         var screenSize = Monitor.CurMonitor().Dimensions.ToSize();
         var screenRect = new Rect(new(0f), screenSize, new(0f));

         var windowSize = CurScreenSize.ToSize();
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

        PrevFullscreenDisplayState = new(windowDimensions, new(x, y), WindowDisplayState.Normal);
        PrevMinimizedDisplayState = new(windowDimensions, new(x, y), WindowDisplayState.Normal);
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

    public bool MouseEnabled
    {
        get => mouseEnabled;
        set
        {
            if (value == mouseEnabled) return;
            mouseEnabled = value;
            if(mouseEnabled)Raylib.EnableCursor();
            else Raylib.DisableCursor();
        }
    }
    public bool MouseVisible
    {
        get => !Raylib.IsCursorHidden();
        set
        {
            if (value == !Raylib.IsCursorHidden()) return;
            if(value) Raylib.ShowCursor();
            else Raylib.HideCursor();
        }
    }
    // public bool MouseOnScreen
    // {
    //     get => Raylib.IsCursorOnScreen();
    // }

    public void ResetMousePosition()
    {
        var center = WindowPosition / 2 + WindowSize.ToVector2() / 2; // CurScreenSize.ToVector2() / 2;
        Raylib.SetMousePosition((int)center.X, (int)center.Y);
    }
    #endregion

    #region ICursor

    public ICursor Cursor { get; private set; } = new NullCursor();
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


    #endregion

    #region Window & Cursor State

    private CursorState GetCurCursorState()
    {
        return new(MouseVisible, MouseEnabled, MouseOnScreen);
    }
    
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


    #endregion
    
}
