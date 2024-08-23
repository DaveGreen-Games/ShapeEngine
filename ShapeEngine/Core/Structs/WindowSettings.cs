namespace ShapeEngine.Core.Structs;

public struct WindowSettings
{
    public static WindowSettings Default => new()
    {
        Undecorated = false,
        Focused = true,
        // WindowDisplayState = WindowDisplayState.Normal,
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
        MouseVisible = true,
        Msaa4x = true,
        HighDPI = false,
        FramebufferTransparent = false
    };
    
    // public Dimensions WindowLocation;
    public Dimensions WindowSize;
    public Dimensions WindowMinSize;
    
    public string Title;
    public bool Undecorated;
    public bool Focused;
    
    // public WindowDisplayState WindowDisplayState;
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
    // public bool AutoIconify; //(minimizes window automatically if focus changes in fullscreen mode)
    
    
    

}