using Raylib_cs;
using ShapeEngine.Screen;

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
    /// <param name="applicationName">The name of the application. Will also be used for savegame folder name.</param>
    /// <param name="saveDirectory">The directory for saving game data. If set to null, no directory will be created.
    /// Savegame location: saveDirectory/applicationName.</param>
    public static GameSettings StretchMode(string applicationName = "ShapeEngineGame", 
        Environment.SpecialFolder? saveDirectory = Environment.SpecialFolder.LocalApplicationData)
    {
        return new GameSettings(-1, TextureFilter.Bilinear, ShaderSupportType.Multi, 
            applicationName, saveDirectory);
    }

    /// <summary>
    /// Creates a new GameSettings instance with fixed dimensions and the nearest scaling disabled.
    /// </summary>
    /// <param name="applicationName">The name of the application. Will also be used for savegame folder name.</param>
    /// <param name="saveDirectory">The directory for saving game data. If set to null, no directory will be created.
    /// Savegame location: saveDirectory/applicationName.</param>
    public static GameSettings FixedMode(string applicationName = "ShapeEngineGame", 
        Environment.SpecialFolder? saveDirectory = Environment.SpecialFolder.LocalApplicationData)
    {
        return new GameSettings(new Dimensions(320, 180), -1, TextureFilter.Point, ShaderSupportType.Multi, false, 
            applicationName, saveDirectory);
    }

    /// <summary>
    /// Creates a new GameSettings instance with fixed dimensions and the nearest scaling enabled.
    /// </summary>
    /// <param name="applicationName">The name of the application. Will also be used for savegame folder name.</param>
    /// <param name="saveDirectory">The directory for saving game data. If set to null, no directory will be created.
    /// Savegame location: saveDirectory/applicationName.</param>

    public static GameSettings FixedNearestMode(string applicationName = "ShapeEngineGame", 
        Environment.SpecialFolder? saveDirectory = Environment.SpecialFolder.LocalApplicationData)
    {
        return new GameSettings(new Dimensions(320, 180), -1, TextureFilter.Point, ShaderSupportType.Multi, true, 
            applicationName, saveDirectory);
    }

    /// <summary>
    /// Creates a new GameSettings instance with pixelation mode.
    /// </summary>
    /// <param name="applicationName">The name of the application. Will also be used for savegame folder name.</param>
    /// <param name="saveDirectory">The directory for saving game data. If set to null, no directory will be created.
    /// Savegame location: saveDirectory/applicationName.</param>
    public static GameSettings PixelationMode(string applicationName = "ShapeEngineGame", 
        Environment.SpecialFolder? saveDirectory = Environment.SpecialFolder.LocalApplicationData)
    {
        return new GameSettings(0.25f, -1, TextureFilter.Point, ShaderSupportType.Multi, 
            applicationName, saveDirectory);
    }

    #endregion
   
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the GameSettings struct with the specified fixed framerate, texture filter, and shader support type.
    /// </summary>
    /// <param name="fixedFramerate">The fixed framerate for the fixed update loop.</param>
    /// <param name="textureFilter">The texture filter to be used.</param>
    /// <param name="shaderSupportType">The shader support type.</param>
    /// <param name="applicationName">The name of the application. Will also be used for savegame folder name.</param>
    /// <param name="saveDirectory">The directory for saving game data. If set to null, no directory will be created.
    /// Savegame location: saveDirectory/applicationName.</param>
    public GameSettings (int fixedFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType, 
        string applicationName = "ShapeEngineGame", Environment.SpecialFolder? saveDirectory = Environment.SpecialFolder.LocalApplicationData)
    {
        FixedFramerate = fixedFramerate;
        TextureFilter = textureFilter;
        ShaderSupportType = shaderSupportType;
        FixedDimensions = Dimensions.GetInvalidDimension();
        PixelationFactor = 1f;
        ScreenTextureMode = ScreenTextureMode.Stretch;
        ApplicationName = applicationName;
        SaveDirectory = saveDirectory;
    }

    /// <summary>
    /// Initializes a new instance of the GameSettings struct with the specified fixed dimensions, fixed framerate, texture filter, shader support type, and nearest scaling option.
    /// </summary>
    /// <param name="fixedDimensions">The fixed dimensions for the game window.</param>
    /// <param name="fixedFramerate">The fixed framerate for the fixed update loop.</param>
    /// <param name="textureFilter">The texture filter to be used.</param>
    /// <param name="shaderSupportType">The shader support type.</param>
    /// <param name="nearestScaling">A value indicating whether the nearest scaling should be used.</param>
    /// <param name="applicationName">The name of the application. Will also be used for savegame folder name.</param>
    /// <param name="saveDirectory">The directory for saving game data. If set to null, no directory will be created.
    /// Savegame location: saveDirectory/applicationName.</param>
    public GameSettings(Dimensions fixedDimensions, int fixedFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType, 
        bool nearestScaling = false, string applicationName = "ShapeEngineGame", Environment.SpecialFolder? saveDirectory = Environment.SpecialFolder.LocalApplicationData)
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
        ApplicationName = applicationName;
        SaveDirectory = saveDirectory;
    }

    /// <summary>
    /// Initializes a new instance of the GameSettings struct with the specified pixelation factor, fixed framerate, texture filter, and shader support type.
    /// </summary>
    /// <param name="pixelationFactor">The pixelation factor for the game window.</param>
    /// <param name="fixedFramerate">The fixed framerate for the fixed update loop.</param>
    /// <param name="textureFilter">The texture filter to be used.</param>
    /// <param name="shaderSupportType">The shader support type.</param>
    /// <param name="applicationName">The name of the application. Will also be used for savegame folder name.</param>
    /// <param name="saveDirectory">The directory for saving game data. If set to null, no directory will be created.
    /// Savegame location: saveDirectory/applicationName.</param>
    public GameSettings(float pixelationFactor, int fixedFramerate, TextureFilter textureFilter, ShaderSupportType shaderSupportType, 
        string applicationName = "ShapeEngineGame", Environment.SpecialFolder? saveDirectory = Environment.SpecialFolder.LocalApplicationData)
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
        ApplicationName = applicationName;
        SaveDirectory = saveDirectory;
    }
    
    #endregion
    
    #region Properties
    /// <summary>
    /// Gets the screen texture mode.
    /// </summary>
    public readonly ScreenTextureMode ScreenTextureMode;
    
    /// <summary>
    /// Gets the fixed framerate used for the fixed update loop.
    /// <list type="bullet">
    /// <item>The physics update uses a delta time of <c>1 / FixedFramerate</c>.</item>
    /// <item>If set to 0 or less, the fixed update loop is disabled and <c>FixedUpdate</c>/<c>InterpolateFixedUpdate</c> will not be called.</item>
    /// <item>Values greater than 0 but less than 30 are clamped to 30.</item>
    /// <item>When enabled, the physics update runs after the normal update function.</item>
    /// </list>
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

    /// <summary>
    /// The name of the application. Also used for the save game folder name if <c>SaveGameDirectory</c> is not set to null.
    /// </summary>
    public readonly string ApplicationName = "ShapeEngineGame";
    
    /// <summary>
    /// The directory where save game data is stored.
    /// Uses <c>Environment.SpecialFolder.LocalApplicationData</c> by default.
    /// Savegame location will be <see cref="SaveDirectory"/> \ <see cref="ApplicationName"/>.
    /// </summary>
    /// <remarks>
    /// Good alternatives for save game locations include:
    /// <list type="bullet">
    /// <item><c>Environment.SpecialFolder.ApplicationData</c> \- for roaming user data.</item>
    /// <item><c>Environment.SpecialFolder.MyDocuments</c> \- for user-accessible files.</item>
    /// <item><c>Environment.SpecialFolder.CommonApplicationData</c> \- for data shared among all users.</item>
    /// </list>
    /// Choose based on your application's requirements and platform conventions.
    /// </remarks>
    public readonly Environment.SpecialFolder? SaveDirectory = Environment.SpecialFolder.LocalApplicationData;

    // /// <summary>
    // /// Gets the maximum number of savegame backup files to keep.
    // /// </summary>
    // /// <remarks>
    // /// If set to 0 or less, no backups will be created and no backup directory will be created.
    // /// </remarks>
    // public readonly int MaxSavegameBackups = 3;

    #endregion
}