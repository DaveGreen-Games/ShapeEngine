using System.Numerics;
using System.Windows.Markup;
using Raylib_CsLo;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Screen;

namespace ShapeEngine.Core;
//Game 
// -> Basic Gameloop
// -> MonitorHandler
// -> exit / restart
// -> entry point
// -> deferred system

//GameWindow
// -> Window Stuff (Position, Size, etc)
// -> Mouse
// -> Cursor
// -> Cur Monitor
// -> GameWindow should draw but get some info from game class

// -> shaders ??
// -> camera ??
// -> Draw ??
// -> Game/UI Texture ???
// -> Input ???
// -> 


public enum WindowDisplayState
{
    Normal = 0,
    Minimized = 1,
    Maximized = 2,
    Fullscreen = 3
}
public enum WindowBorder
{
    Resizabled = 0,
    Fixed = 1,
    Hidden = 2
}


public struct WindowSettings
{
    public static WindowSettings Default => new()
    {
        Undecorated = false,
        Focused = true,
        WindowDisplayState = WindowDisplayState.Normal,
        WindowBorder = WindowBorder.Resizabled,
        WindowMinSize = new(480, 270),
        WindowSize = new(960, 540),
        // WindowLocation = new(0, 0),
        Monitor = 0,
        Vsync = false,
        FrameRateLimit = 60,
        MinFramerate = 30,
        MaxFramerate = 240,
        // AutoIconify = true,
        WindowOpacity = 1f,
        MouseEnabled = true,
        MouseVisible = true
    };
    
    // public Dimensions WindowLocation;
    public Dimensions WindowSize;
    public Dimensions WindowMinSize;
    
    public string Title;
    public bool Undecorated;
    public bool Focused;
    
    public WindowDisplayState WindowDisplayState;
    public WindowBorder WindowBorder;
    
    public bool Vsync;
    public int FrameRateLimit;
    public int MinFramerate;
    public int MaxFramerate;
    public float WindowOpacity; //0-1
    
    public int Monitor;
    public bool MouseVisible;
    public bool MouseEnabled;
    // public bool AutoIconify; //(minimizes window automatically if focus changes in fullscreen mode)

}

public sealed class GameWindow
{

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

            var newState = value;
            if (value == WindowDisplayState.Normal)
            {
                if (displayState == WindowDisplayState.Minimized)
                {
                    Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
                    
                    if (prevFullscreenDisplayState == WindowDisplayState.Fullscreen)
                    {
                        var mDim = Monitor.CurMonitor().Dimensions;
                        Raylib.SetWindowSize(mDim.Width, mDim.Height);
                        Raylib.SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                        newState = WindowDisplayState.Fullscreen;
                    }

                    else if (prevFullscreenDisplayState == WindowDisplayState.Maximized)
                    {
                        Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
                        newState = WindowDisplayState.Maximized;
                    }
                    
                }
                else if (displayState == WindowDisplayState.Maximized)
                {
                    Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
                }
                else if (displayState == WindowDisplayState.Fullscreen)
                {
                    Raylib.ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                    Raylib.SetWindowSize(prevFullscreenWindowSize.Width, prevFullscreenWindowSize.Height);
                    Raylib.SetWindowPosition((int)prevFullscreenWindowPosition.X, (int)prevFullscreenWindowPosition.Y);

                    if (prevFullscreenDisplayState == WindowDisplayState.Minimized)
                    {
                        Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
                        newState = WindowDisplayState.Minimized;
                    }

                    else if (prevFullscreenDisplayState == WindowDisplayState.Maximized)
                    {
                        Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
                        newState = WindowDisplayState.Maximized;
                    }
                }
            }
            else if (value == WindowDisplayState.Maximized)
            {
                if (displayState == WindowDisplayState.Fullscreen)
                {
                    Raylib.ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                    Raylib.SetWindowSize(prevFullscreenWindowSize.Width, prevFullscreenWindowSize.Height);
                    Raylib.SetWindowPosition((int)prevFullscreenWindowPosition.X, (int)prevFullscreenWindowPosition.Y);
                
                }
                else if (displayState == WindowDisplayState.Minimized)Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
                
                Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            }
            else if (value == WindowDisplayState.Minimized)
            {
                prevFullscreenDisplayState = displayState;
                prevFullscreenWindowPosition = Raylib.GetWindowPosition();
                prevFullscreenWindowSize = CurScreenSize;
                
                if (displayState == WindowDisplayState.Fullscreen)
                {
                    Raylib.ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                    Raylib.SetWindowSize(prevFullscreenWindowSize.Width, prevFullscreenWindowSize.Height);
                    Raylib.SetWindowPosition((int)prevFullscreenWindowPosition.X, (int)prevFullscreenWindowPosition.Y);
                
                }
                else if (displayState == WindowDisplayState.Maximized)
                {
                    Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
                }
                
                Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
            }
            else if (value == WindowDisplayState.Fullscreen)
            {
                prevFullscreenDisplayState = displayState;
                prevFullscreenWindowPosition = Raylib.GetWindowPosition();
                prevFullscreenWindowSize = CurScreenSize;

                if (displayState == WindowDisplayState.Maximized) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
                else if (displayState == WindowDisplayState.Minimized) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
                
                var mDim = Monitor.CurMonitor().Dimensions;
                Raylib.SetWindowSize(mDim.Width, mDim.Height);
                Raylib.SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            }
            
            displayState = newState;
            ResetMousePosition();
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
                if(windowBorder == WindowBorder.Resizabled) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
                else if(windowBorder == WindowBorder.Hidden) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
                
            }
            else if (value == WindowBorder.Resizabled)
            {
                if(windowBorder == WindowBorder.Hidden) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
                Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            }
            else if(value == WindowBorder.Hidden)
            {
                //if hidden does not work use window opacity !
                if(windowBorder == WindowBorder.Resizabled) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
                Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
            }

            windowBorder = value;
        }
    }
    public Rect ScreenArea { get; private set; }
    
    
    #endregion

    #region Private Members

    private WindowDisplayState displayState = WindowDisplayState.Normal;
    private WindowBorder windowBorder = WindowBorder.Fixed;
    
    private Dimensions prevFullscreenWindowSize = new(128, 128);
    private Vector2 prevFullscreenWindowPosition = new(0);
    private WindowDisplayState prevFullscreenDisplayState = WindowDisplayState.Normal;
    private int fpsLimit = 60;
    private Dimensions windowSize = new();
    
    private bool? wasMouseEnabled = null;
    private bool? wasMouseVisible = null;
    
    private Vector2 lastControlledMousePosition = new();
    private bool mouseControlled = false;

    private CursorState cursorState;
    private WindowState windowState;
    #endregion

    #region Internal
    internal GameWindow(WindowSettings windowSettings)
    {
        Raylib.InitWindow(0, 0, windowSettings.Title);
        Monitor = new MonitorDevice();
        SetupWindowDimensions();
        WindowMinSize = windowSettings.WindowMinSize;
        Raylib.SetWindowMinSize(WindowMinSize.Width, WindowMinSize.Height);

        VSync = windowSettings.Vsync;
        MinFramerate = windowSettings.MinFramerate;
        MaxFramerate = windowSettings.MaxFramerate;
        FpsLimit = windowSettings.FrameRateLimit;

        WindowBorder = windowSettings.WindowBorder;
        DisplayState = windowSettings.WindowDisplayState;
        WindowSize = windowSettings.WindowSize;

        
        Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);

        if(windowSettings.Focused) Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
        else Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
        
        if (windowSettings.Undecorated) Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
        else Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
        
        if (Monitor.CurMonitor().Index != windowSettings.Monitor && windowSettings.Monitor >= 0)
        {
            SetMonitor(windowSettings.Monitor);
        }
        var screenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
        MouseOnScreen = displayState == WindowDisplayState.Fullscreen || Raylib.IsCursorOnScreen() || ( Raylib.IsWindowFocused() && screenArea.ContainsPoint(Raylib.GetMousePosition()) );

        MouseVisible = windowSettings.MouseVisible;
        MouseEnabled = windowSettings.MouseEnabled;
        
        cursorState = GetCursorState();
        windowState = GetWindowState();
        
        Raylib.SetWindowOpacity(windowSettings.WindowOpacity);
        
        
        Console.WriteLine("---------------|Flags|----------------");
        Console.WriteLine($"Unfocused {Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED)}");
        Console.WriteLine("--------------------------------------------");
    }
    internal void Update()
    {
        var newMonitor = Monitor.HasMonitorChanged();
        if (newMonitor.Available)
        {
            ChangeMonitor(newMonitor);
        }
        CheckForWindowChanges();

        var mousePos = MousePosition;
        if (mouseControlled) mousePos = lastControlledMousePosition;
        mouseControlled = false;
            
        ScreenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
        MouseOnScreen = false;
        bool windowHidden = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
        bool windowMinimized = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_MINIMIZED);
        bool windowTopmost = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
        bool windowUnfocused = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
        bool windowMousePassthrough = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_MOUSE_PASSTHROUGH);
        bool windowTransparent = Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_TRANSPARENT);
            
        if (windowTopmost && !windowUnfocused && !windowHidden && !windowMinimized 
            && !windowTransparent && !windowMousePassthrough)
        {
            if (displayState == WindowDisplayState.Fullscreen || Raylib.IsCursorOnScreen() || ScreenArea.ContainsPoint(mousePos)) MouseOnScreen = true;
        }
        
        // Console.WriteLine("---------------|MOUSE INFO|----------------");
        // Console.WriteLine($"Visible {MouseVisible} | Enabled {MouseEnabled} | Pos {mousePos} | OnScreen {mouseOnScreen} | Screen {CurScreenSize.Width},{CurScreenSize.Height}");
        // Console.WriteLine("--------------------------------------------");

        if (MouseOnScreen)
        {
            if (!MouseOnScreen)
            {
                if (cursorState.OnScreen)
                {
                    OnMouseLeftScreen?.Invoke();
                    // ResolveOnCursorLeftScreen();
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
                    // ResolveOnCursorEnteredScreen();
                    if (wasMouseVisible != null && wasMouseVisible == false) MouseVisible = false; // HideOSCursor();
                    if (wasMouseEnabled != null && wasMouseEnabled == false) MouseEnabled = false; // LockOSCursor();

                    wasMouseVisible = null;
                    wasMouseEnabled = null;
                }
            }
        }
        
        var curWindowState = GetWindowState();
            
        if (curWindowState.Focused && !windowState.Focused)
        {
            OnWindowFocusChanged?.Invoke(true);
            Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
        }
        else if (!curWindowState.Focused && windowState.Focused)
        {
            OnWindowFocusChanged?.Invoke(false);
            Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
        }

        if (curWindowState.Maximized && !windowState.Maximized)
        {
            OnWindowMaximizeChanged?.Invoke(true);
        }
        else if (!curWindowState.Maximized && windowState.Maximized)
        {
            OnWindowMaximizeChanged?.Invoke(false);
        }
            
        if (curWindowState.Fullscreen && !windowState.Fullscreen)
        {
            OnWindowFullscreenChanged?.Invoke(true);
        }
        else if (!curWindowState.Fullscreen && windowState.Fullscreen)
        {
            OnWindowFullscreenChanged?.Invoke(false);
        }
        
        //safety measure
        if (MouseVisible == Raylib.IsCursorHidden()) MouseVisible = Raylib.IsCursorHidden();
            
        var curCursorState = GetCursorState();
        if (curCursorState.Visible && !cursorState.Visible) OnMouseVisibilityChanged?.Invoke(false);
        else if (!curCursorState.Visible && cursorState.Visible) OnMouseVisibilityChanged?.Invoke(true);
            
        if (curCursorState.Enabled && !cursorState.Enabled) OnMouseEnabledChanged?.Invoke(false);
        else if (!curCursorState.Enabled && cursorState.Enabled) OnMouseEnabledChanged?.Invoke(true);

        cursorState = curCursorState;
        windowState = curWindowState;
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
        prevFullscreenWindowSize = windowSize;
        //CenterWindow();
        WindowPosition = Raylib.GetWindowPosition();
        // prevWindowPosition = WindowPosition;
        prevFullscreenWindowPosition = WindowPosition;
        CalculateCurScreenSize();
    }
    
    private void CalculateCurScreenSize()
    {
        if (displayState == WindowDisplayState.Fullscreen)
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

    #endregion
    
    
    #region Window
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
        prevFullscreenDisplayState = WindowDisplayState.Normal;
        prevFullscreenWindowPosition =new(x, y);
        
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

    private CursorState GetCursorState()
    {
        return new(MouseVisible, MouseEnabled, MouseOnScreen);
    }
    private WindowState GetWindowState()
    {
        var fullscreen = DisplayState == WindowDisplayState.Fullscreen; // Raylib.IsWindowFullscreen();
        var maximized = DisplayState == WindowDisplayState.Maximized; // Raylib.IsWindowMaximized();
        var minimized = DisplayState == WindowDisplayState.Minimized; // Raylib.IsWindowMinimized(); //does not work...
        var hidden = WindowBorder == WindowBorder.Hidden; // Raylib.IsWindowHidden();
        var focused = Raylib.IsWindowFocused();
        return new(minimized, maximized, fullscreen, hidden, focused);
    }


    #endregion
    
}
