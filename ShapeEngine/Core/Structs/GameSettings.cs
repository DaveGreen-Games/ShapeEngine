using Raylib_cs;
using ShapeEngine.Screen;
using ShapeEngine.Stats;

namespace ShapeEngine.Core.Structs;


/// <summary>
/// A struct representing game settings for a 2D game engine.
/// </summary>
public readonly struct GameSettings
{
    #region Factory
    /// <summary>
    /// Creates a new GameSettings instance with stretch mode.
    /// </summary>
    public static GameSettings StretchMode => new GameSettings(-1, TextureFilter.Bilinear, ShaderSupportType.Multi);

    /// <summary>
    /// Creates a new GameSettings instance with fixed dimensions and the nearest scaling disabled.
    /// </summary>
    public static GameSettings FixedMode =>
        new GameSettings(new Dimensions(320, 180), -1, TextureFilter.Point, ShaderSupportType.Multi, false);

    /// <summary>
    /// Creates a new GameSettings instance with fixed dimensions and the nearest scaling enabled.
    /// </summary>
    public static GameSettings FixedNearestMode =>
        new GameSettings(new Dimensions(320, 180), -1, TextureFilter.Point, ShaderSupportType.Multi, true);

    /// <summary>
    /// Creates a new GameSettings instance with pixelation mode.
    /// </summary>
    public static GameSettings PixelationMode =>  
        new GameSettings(0.25f, -1, TextureFilter.Point, ShaderSupportType.Multi);
    #endregion
   
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the GameSettings struct with the specified fixed framerate, texture filter, and shader support type.
    /// </summary>
    /// <param name="fixedFramerate">The fixed framerate for the fixed update loop.</param>
    /// <param name="textureFilter">The texture filter to be used.</param>
    /// <param name="shaderSupportType">The shader support type.</param>
    public GameSettings(int fixedFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType)
    {
        FixedFramerate = fixedFramerate;
        TextureFilter = textureFilter;
        ShaderSupportType = shaderSupportType;
        FixedDimensions = Dimensions.GetInvalidDimension();
        PixelationFactor = 1f;
        ScreenTextureMode = ScreenTextureMode.Stretch;
    }

    /// <summary>
    /// Initializes a new instance of the GameSettings struct with the specified fixed dimensions, fixed framerate, texture filter, shader support type, and nearest scaling option.
    /// </summary>
    /// <param name="fixedDimensions">The fixed dimensions for the game window.</param>
    /// <param name="fixedFramerate">The fixed framerate for the fixed update loop.</param>
    /// <param name="textureFilter">The texture filter to be used.</param>
    /// <param name="shaderSupportType">The shader support type.</param>
    /// <param name="nearestScaling">A value indicating whether the nearest scaling should be used.</param>
    public GameSettings(Dimensions fixedDimensions, int fixedFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType, bool nearestScaling = false)
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
    }

    /// <summary>
    /// Initializes a new instance of the GameSettings struct with the specified pixelation factor, fixed framerate, texture filter, and shader support type.
    /// </summary>
    /// <param name="pixelationFactor">The pixelation factor for the game window.</param>
    /// <param name="fixedFramerate">The fixed framerate for the fixed update loop.</param>
    /// <param name="textureFilter">The texture filter to be used.</param>
    /// <param name="shaderSupportType">The shader support type.</param>
    public GameSettings(float pixelationFactor, int fixedFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType)
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
    }
    
    #endregion
    
    #region Properties
    /// <summary>
    /// Gets the screen texture mode.
    /// </summary>
    public readonly ScreenTextureMode ScreenTextureMode;
    
    /// <summary>
    /// Gets the fixed framerate for the fixed update loop.
    /// The delta time used in the physics update will always be 1/FixedFramerate.
    /// Setting this to smaller or equal to 0 will disable the fixed update loop! FixedUpdate & InterpolateFixedUpdate will no longer be called!
    /// Fixed Framerate bigger than 0 and smaller than 30 will be set to 30.
    /// The physics update is always called after the normal update function if enabled.
    /// </summary>
    public readonly int FixedFramerate;

    /// <summary>
    /// Gets the shader support type.
    /// </summary>
    public readonly ShaderSupportType ShaderSupportType;

    /// <summary>
    /// Gets the texture filter to be used.
    /// </summary>
    public readonly TextureFilter TextureFilter;

    /// <summary>
    /// Gets the fixed dimensions for the game window.
    /// </summary>
    public readonly Dimensions FixedDimensions;

    /// <summary>
    /// Gets the pixelation factor for the game window.
    /// </summary>
    public readonly float PixelationFactor;
    #endregion
}