namespace ShapeEngine.Core.Structs;

public struct GameSettings
{
    public GameSettings Default => new()
    {
        MultiShaderSupport = false,
        DevelopmentDimensions = new(1920, 1080)
    };
    
    public bool MultiShaderSupport;
    public Dimensions DevelopmentDimensions;
    
}