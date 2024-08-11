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
    public static GameSettings StretchMode => new GameSettings(60, TextureFilter.Bilinear, ShaderSupportType.Multi);

    public static GameSettings FixedMode =>
        new GameSettings(new Dimensions(320, 180), 60, TextureFilter.Point, ShaderSupportType.Multi);
    public static GameSettings FixedNearestMode =>
        new GameSettings(new Dimensions(320, 180), 60, TextureFilter.Point, ShaderSupportType.Multi, true);

    public static GameSettings PixelationMode =>  
        new GameSettings(0.25f, 60, TextureFilter.Point, ShaderSupportType.Multi);

    public GameSettings(int fixedPhysicsFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType)
    {
        FixedPhysicsFramerate = fixedPhysicsFramerate;
        TextureFilter = textureFilter;
        ShaderSupportType = shaderSupportType;
        FixedDimensions = Dimensions.GetInvalidDimension();
        PixelationFactor = 1f;
        ScreenTextureMode = ScreenTextureMode.Stretch;
    }

    public GameSettings(Dimensions fixedDimensions, int fixedPhysicsFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType, bool nearestScaling = false)
    {
        FixedPhysicsFramerate = fixedPhysicsFramerate;
        TextureFilter = textureFilter;
        ShaderSupportType = shaderSupportType;
        PixelationFactor = 1f;

        if (fixedDimensions.IsValid())
        {
            FixedDimensions = fixedDimensions;
        
            if (nearestScaling)
            {
                ScreenTextureMode = ScreenTextureMode.NearestFixed;
            }
            else
            {
                ScreenTextureMode = ScreenTextureMode.Fixed;
            }
        }
        else
        {
            FixedDimensions = Dimensions.GetInvalidDimension();
            ScreenTextureMode = ScreenTextureMode.Stretch;
        }
        
    }
    
    public GameSettings(float pixelationFactor, int fixedPhysicsFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType)
    {
        FixedPhysicsFramerate = fixedPhysicsFramerate;
        TextureFilter = textureFilter;
        ShaderSupportType = shaderSupportType;
        FixedDimensions = Dimensions.GetInvalidDimension();

        if (pixelationFactor <= 0 || pixelationFactor >= 1f)
        {
            PixelationFactor = 1f;
            ScreenTextureMode = ScreenTextureMode.Stretch;
        }
        else
        {
            PixelationFactor = pixelationFactor;
            ScreenTextureMode = ScreenTextureMode.Pixelation;
        }
        
    }
    
    // public ScreenTextureMode GetScreenTextureMode()
    // {
    //     if (FixedDimensions.IsValid()) return ScreenTextureMode.Fixed;
    //     if (PixelationFactor < 1f) return ScreenTextureMode.Pixelation;
    //     return ScreenTextureMode.Stretch;
    // }

    public readonly ScreenTextureMode ScreenTextureMode;
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