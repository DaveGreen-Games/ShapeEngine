namespace ShapeEngine.Core.Structs;


/// <summary>
/// Represents configuration settings for a game window in Shape Engine.
/// </summary>
public struct WindowSettings
{
    /// <summary>
    /// Gets the default window settings configuration.
    /// </summary>
    /// <returns>A WindowSettings instance with predefined default values.</returns>
    ///<remarks>
    /// Title = "Shape Engine Window",
    /// FullscreenAutoRestoring = true,
    /// Topmost = false,
    /// WindowBorder = WindowBorder.Resizabled,
    /// WindowMinSize = new(480, 270),
    /// WindowSize = new(960, 540),
    /// Monitor = 0,
    /// Vsync = false,
    /// FrameRateLimit = 60,
    /// AdaptiveFpsLimiterSettings = new(30, 240, true),
    /// WindowOpacity = 1f,
    /// MouseEnabled = true,
    /// MouseVisible = true,
    /// Msaa4x = true,
    /// HighDPI = false,
    /// FramebufferTransparent = false
    ///</remarks>
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
        AdaptiveFpsLimiterSettings = new(30, 240, true),
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
    
    /// <summary>
    /// The dimensions of the window in pixels.
    /// </summary>
    public Dimensions WindowSize;
    
    /// <summary>
    /// The minimum allowed dimensions of the window in pixels.
    /// </summary>
    public Dimensions WindowMinSize;
    
    /// <summary>
    /// The title displayed in the window's title bar.
    /// </summary>
    public string Title;
    
    /// <summary>
    /// Determines whether the window should stay on top of other windows.
    /// </summary>
    public bool Topmost;
    
    /// <summary>
    /// Should fullscreen be automatically left when window loses focus and should the window be restored to
    /// fullscreen when window gains focus again.
    /// </summary>
    public bool FullscreenAutoRestoring;
    
    /// <summary>
    /// The border style of the window.
    /// </summary>
    public WindowBorder WindowBorder;
    
    /// <summary>
    /// Determines whether vertical synchronization is enabled.
    /// </summary>
    public bool Vsync;
    
    /// <summary>
    /// The target frame rate limit for the application. 0 or less means unlimited frame rate.
    /// </summary>
    public int FrameRateLimit;
    
    /// <summary>
    /// Settings for the adaptive FPS limiter which dynamically adjusts the frame cap.
    /// Contains minimum and maximum target frame rates and whether adaptive limiting is enabled.
    /// </summary>
    public AdaptiveFpsLimiter.Settings AdaptiveFpsLimiterSettings;
    
    /// <summary>
    /// The opacity of the window, ranging from 0.0 (completely transparent) to 1.0 (completely opaque).
    /// </summary>
    public float WindowOpacity;
    
    /// <summary>
    /// The index of the monitor on which to display the window.
    /// </summary>
    public int Monitor;
    
    /// <summary>
    /// Determines whether the mouse cursor is visible within the window.
    /// </summary>
    public bool MouseVisible;
    
    /// <summary>
    /// Determines whether mouse input is enabled for the window.
    /// </summary>
    public bool MouseEnabled;
    
    /// <summary>
    /// Determines whether 4x Multi-Sample Anti-Aliasing is enabled.
    /// </summary>
    public bool Msaa4x;
    
    /// <summary>
    /// Currently High DPI mode does not work correctly in raylib and until it is fixed it should be set to false!
    /// </summary>
    public bool HighDPI;
    
    /// <summary>
    /// Determines whether the framebuffer supports transparency.
    /// </summary>
    public bool FramebufferTransparent;
}