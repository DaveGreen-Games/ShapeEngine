using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;

namespace Examples;
public static class Program
{
    public static void Main(string[] args)
    {
        var gameSettings = GameSettings.StretchMode("Shape Engine Examples");
        
        var windowSettings = new WindowSettings
        {
            Title = "Shape Engine Examples",
            Topmost = false,
            FullscreenAutoRestoring = true,
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
            MouseVisible = false,
            Msaa4x = true,
            HighDPI = false,
            FramebufferTransparent = false
        };
        
        var inputSettings = new InputSettings
        (
            new InputSettings.MouseSettings(25, 3, 2, 0.5f, 1f, 0.25f),
            new InputSettings.KeyboardSettings(2, 0.5f, 1f, 2f),
            new InputSettings.GamepadSettings()
        );
        
        GameloopExamples gameloop = new(gameSettings, windowSettings, inputSettings);
        
        gameloop.Run(args);
    }
}