using Raylib_cs;
using ShapeEngine.Screen;
using ShapeEngine.Stats;

namespace ShapeEngine.Core.Structs;

public readonly struct GameSettings
{
    #region Factory
    public static GameSettings StretchMode => new GameSettings(-1, TextureFilter.Bilinear, ShaderSupportType.Multi, true);

    public static GameSettings FixedMode =>
        new GameSettings(new Dimensions(320, 180), -1, TextureFilter.Point, ShaderSupportType.Multi, false, true);
    public static GameSettings FixedNearestMode =>
        new GameSettings(new Dimensions(320, 180), -1, TextureFilter.Point, ShaderSupportType.Multi, true, true);

    public static GameSettings PixelationMode =>  
        new GameSettings(0.25f, -1, TextureFilter.Point, ShaderSupportType.Multi, true);
    #endregion
   
    #region Constructors

    public GameSettings(int fixedFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType, bool autoClearBackground = true)
    {
        FixedFramerate = fixedFramerate;
        TextureFilter = textureFilter;
        ShaderSupportType = shaderSupportType;
        FixedDimensions = Dimensions.GetInvalidDimension();
        PixelationFactor = 1f;
        ScreenTextureMode = ScreenTextureMode.Stretch;
        AutoClearBackground = autoClearBackground;
    }
    public GameSettings(Dimensions fixedDimensions, int fixedFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType, bool nearestScaling = false, bool autoClearBackground = true)
    {
        FixedFramerate = fixedFramerate;
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
        AutoClearBackground = autoClearBackground;
    }
    public GameSettings(float pixelationFactor, int fixedFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType, bool autoClearBackground = true)
    {
        FixedFramerate = fixedFramerate;
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
        AutoClearBackground = autoClearBackground;
    }
    
    #endregion
    
    #region Properties
    public readonly ScreenTextureMode ScreenTextureMode;
    
    /// <summary>
    /// Set a fixed framerate for the fixed update loop.
    /// The delta time used in the physics update will always be 1/FixedFramerate.
    /// Setting this to smaller or equal to 0 will disable the fixed update loop! FixedUpdate & InterpolateFixedUpdate will no longer be called!
    /// Fixed Framerate bigger than 0 and smaller than 30 will be set to 30.
    /// The physics update is always called after the normal update function if enabled.
    /// </summary>
    public readonly int FixedFramerate;

    public readonly ShaderSupportType ShaderSupportType;
    public readonly TextureFilter TextureFilter;
    public readonly Dimensions FixedDimensions;
    public readonly float PixelationFactor;
    public readonly bool AutoClearBackground;
    #endregion
}