namespace ShapeEngine.Core.Structs;

public struct GameSettings
{
    public GameSettings Default => new()
    {
        MultiShaderSupport = false,
        DevelopmentDimensions = new(1920, 1080),
        FixedPhysicsTimestep = 60
    };
    
    public bool MultiShaderSupport;
    public Dimensions DevelopmentDimensions;
    public int FixedPhysicsTimestep;
}