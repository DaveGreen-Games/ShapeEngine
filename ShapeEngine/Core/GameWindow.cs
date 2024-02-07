using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Core.Structs;
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
    public WindowSettings Default => new()
    {
        Undecorated = false,
        Focused = true,
        Visible = true,
        WindowDisplayState = WindowDisplayState.Normal,
        WindowBorder = WindowBorder.Resizabled,
        WindowMinSize = new(480, 270),
        WindowSize = new(960, 540),
        // WindowLocation = new(0, 0),
        Monitor = 0,
        Vsync = true,
        FrameRateLimit = 60,
        MinFramerate = 30,
        MaxFramerate = 240,
        // AutoIconify = true,
        WindowOpacity = 1f
    };
    
    // public Dimensions WindowLocation;
    public Dimensions WindowSize;
    public Dimensions WindowMinSize;
    
    public string Title;
    public bool Undecorated;
    public bool Focused;
    public bool Visible;
    
    public WindowDisplayState WindowDisplayState;
    public WindowBorder WindowBorder;
    
    public bool Vsync;
    public int FrameRateLimit;
    public int MinFramerate;
    public int MaxFramerate;
    public float WindowOpacity; //0-1
    
    public int Monitor;
    // public bool AutoIconify; //(minimizes window automatically if focus changes in fullscreen mode)

}

public class GameWindow
{
    private WindowSettings windowSettings;
    internal GameWindow(WindowSettings windowSettings)
    {
        this.windowSettings = windowSettings;
    }

    internal void Update()
    {
        
    }

    internal void Draw()
    {
        
    }

    #region Public Members
    public MonitorDevice Monitor { get; private set; }
    /// <summary>
    /// Scaling factors from current screen size to development resolution.
    /// </summary>
    public DimensionConversionFactors ScreenToDevelopment { get; private set; }
    /// <summary>
    /// Scaling factors from development resolution to the current screen size.
    /// </summary>
    public DimensionConversionFactors DevelopmentToScreen { get; private set; }
    public Dimensions DevelopmentDimensions { get; private set; }
    public Dimensions CurScreenSize { get; private set; }
    public Dimensions WindowMinSize { get; private set; }
    public Vector2 WindowPosition { get; private set; }
    public float Delta { get; private set; }
    public float DeltaSlow { get; private set; }
    
    public ScreenInfo Game { get; private set; } = new();
    public ScreenInfo UI { get; private set; } = new();
   
    
    public int FpsLimit
    {
        get => frameRateLimit;
        set
        {
            if (value < windowSettings.MinFramerate) frameRateLimit = windowSettings.MinFramerate;
            else if (value > windowSettings.MaxFramerate) frameRateLimit = windowSettings.MaxFramerate;
            
            if(!VSync) Raylib.SetTargetFPS(frameRateLimit);
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
                Raylib.SetTargetFPS(frameRateLimit);
            }
        }
    }

    public WindowDisplayState DisplayState
    {
        get;
        set;
    }

    public WindowBorder Border
    {
        get;
        set;
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
    
    private Vector2 prevWindowPosition = new();
    private Dimensions prevFullscreenWindowSize = new(128, 128);
    private Vector2 prevFullscreenWindowPosition = new(0);
    private bool prevFullscreenWindowMaximized = false;
    private int frameRateLimit = 60;
    private Dimensions windowSize = new();
    
    private bool? wasCursorLocked = null;
    private bool? wasCursorHidden = null;
    
    // private CursorState cursorState = new();
    // private WindowState windowState = new();

    private Vector2 lastControlledMousePosition = new();
    private bool mouseControlled = false;
    #endregion

    #region Window

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
    public bool MouseOnScreen
    {
        get => Raylib.IsCursorOnScreen();
    }

    public void ResetMousePosition()
    {
        var center = WindowPosition / 2 + WindowSize.ToVector2() / 2; // CurScreenSize.ToVector2() / 2;
        Raylib.SetMousePosition((int)center.X, (int)center.Y);
    }
    #endregion

    #region Cursor

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
    
    
}
