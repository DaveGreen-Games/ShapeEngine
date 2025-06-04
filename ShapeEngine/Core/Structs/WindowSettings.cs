namespace ShapeEngine.Core.Structs;


public struct WindowSettings
{
    public static WindowSettings Default => new()
    {
        Title = "Shape Engine Window",
        FullscreenAutoRestoring = true,
        Topmost = false,
        WindowBorder = WindowBorder.Resizabled,
        WindowMinSize = new(480, 270),
        WindowSize = new(960, 540),
        Monitor = 0,
        Vsync = false,
        FrameRateLimit = 60,
        MinFramerate = 30,
        MaxFramerate = 240,
        WindowOpacity = 1f,
        MouseEnabled = true,
        MouseVisible = true,
        Msaa4x = true,
        HighDPI = false,
        FramebufferTransparent = false
        
        // Focused = true,
        // AutoIconify = true,
        // Undecorated = false,
        // WindowDisplayState = WindowDisplayState.Normal,
        // WindowLocation = new(0, 0),
    };
    
    
    public Dimensions WindowSize;
    public Dimensions WindowMinSize;
    
    public string Title;
    public bool Topmost;
    
    /// <summary>
    /// Should fullscreen be automatically left when window loses focus and should the window be restored to
    /// fullscreen when window gains focus again.
    /// </summary>
    public bool FullscreenAutoRestoring;
    
    public WindowBorder WindowBorder;
    
    public bool Vsync;
    public int FrameRateLimit;
    public int MinFramerate;
    public int MaxFramerate;
    public float WindowOpacity; //0-1
    
    public int Monitor;
    public bool MouseVisible;
    public bool MouseEnabled;
    public bool Msaa4x;
    
    /// <summary>
    /// Currently High DPI mode does not work correctly in raylib and until it is fixed it should be set to false!
    /// </summary>
    public bool HighDPI;
    public bool FramebufferTransparent;
    
}