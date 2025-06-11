namespace ShapeEngine.Screen;

/// <summary>
/// Specifies the different modes for handling screen textures.
/// </summary>
/// <remarks>
/// Each mode determines how the texture is sized and positioned relative to the screen.
/// </remarks>
public enum ScreenTextureMode
{
    /// <summary>
    /// Texture will always be the same size as the screen.
    /// </summary>
    Stretch = 1,
    
    /// <summary>
    /// Texture will always maintain the same aspect ratio as the screen but will be scaled by the pixelation factor.
    /// </summary>
    Pixelation = 2,
    
    /// <summary>
    /// Texture will always be the same size and centered on the screen.
    /// </summary>
    Fixed = 4,
    
    /// <summary>
    /// Texture will always maintain the same aspect ratio as the screen but will stay as close as possible to the fixed dimensions.
    /// </summary>
    NearestFixed = 8,
    
    /// <summary>
    /// Texture size will be calculated as screensize multiplied by anchorstretch, and the top-left position will be screensize multiplied by anchorposition.
    /// </summary>
    Anchor = 16,
    
    /// <summary>
    /// Custom mode for user-defined texture sizing and positioning logic.
    /// </summary>
    Custom = 32
}