namespace ShapeEngine.Core.Structs;

public struct GameSettings
{
    public GameSettings Default => new()
    {
        MultiShaderSupport = false,
        DevelopmentDimensions = new(1920, 1080),
        FixedPhysicsFramerate = 60
    };
    
    public bool MultiShaderSupport;
    public Dimensions DevelopmentDimensions;
    /// <summary>
    /// Set a fixed framerate for the physics update loop.
    /// The delta time used in the physics update will always be 1/FixedPhysicsFramerate.
    /// Fixed Framerate can not be lower than 30!
    /// The physics update is always called after the normal update function.
    /// </summary>
    public int FixedPhysicsFramerate;
}