using Raylib_cs;
using ShapeEngine.Screen;
using ShapeEngine.Stats;

namespace ShapeEngine.Core.Structs;

public readonly struct GameSettings
{
    // public GameSettings Default => new()
    // {
    //     FixedPhysicsFramerate = 60,
    //     ShaderSupportType = ShaderSupportType.None,
    //     TextureFilter = TextureFilter.Bilinear,
    //     FixedDimensions = new(),
    //     PixelationFactor = 1f,
    // };
    public static GameSettings Default => new GameSettings(60, TextureFilter.Bilinear, ShaderSupportType.Multi);

    public static GameSettings Fixed =>
        new GameSettings(new Dimensions(400, 400), 60, TextureFilter.Bilinear, ShaderSupportType.None);

    public static GameSettings Pixelation =>  
        new GameSettings(0.5f, 60, TextureFilter.Bilinear, ShaderSupportType.Multi);

    public GameSettings(int fixedPhysicsFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType)
    {
        FixedPhysicsFramerate = fixedPhysicsFramerate;
        TextureFilter = textureFilter;
        ShaderSupportType = shaderSupportType;
        FixedDimensions = Dimensions.GetInvalidDimension();
        PixelationFactor = 1f;
    }

    public GameSettings(Dimensions fixedDimensions, int fixedPhysicsFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType)
    {
        FixedPhysicsFramerate = fixedPhysicsFramerate;
        TextureFilter = textureFilter;
        ShaderSupportType = shaderSupportType;
        FixedDimensions = fixedDimensions;
        PixelationFactor = 1f;
    }
    
    public GameSettings(float pixelationFactor, int fixedPhysicsFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType)
    {
        FixedPhysicsFramerate = fixedPhysicsFramerate;
        TextureFilter = textureFilter;
        ShaderSupportType = shaderSupportType;
        FixedDimensions = Dimensions.GetInvalidDimension();
        PixelationFactor = pixelationFactor;
    }
    
    public ScreenTextureMode GetScreenTextureMode()
    {
        if (FixedDimensions.IsValid()) return ScreenTextureMode.Fixed;
        if (PixelationFactor < 1f) return ScreenTextureMode.Pixelation;
        return ScreenTextureMode.Stretch;
    }

    /// <summary>
    /// Set a fixed framerate for the physics update loop.
    /// The delta time used in the physics update will always be 1/FixedPhysicsFramerate.
    /// Fixed Framerate can not be lower than 30!
    /// The physics update is always called after the normal update function.
    /// </summary>
    public readonly int FixedPhysicsFramerate;

    public readonly ShaderSupportType ShaderSupportType;
    public readonly TextureFilter TextureFilter;
    public readonly Dimensions FixedDimensions;
    public readonly float PixelationFactor;
}