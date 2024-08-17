namespace ShapeEngine.Screen;

public enum ScreenTextureMode
{
    /// <summary>
    /// Texture will always be the same size as the screen.
    /// </summary>
    Stretch = 1,
    
    /// <summary>
    /// Texture will always be the same aspect ratio as the screen but scaled by the pixelation factor.
    /// </summary>
    Pixelation = 2,
    
    /// <summary>
    /// Texture will always be the same size and centered on the screen.
    /// </summary>
    Fixed = 4,
    
    /// <summary>
    /// Texture will always be the same aspect ratio as the screen but stay as close as possible to the fixed dimensions
    /// </summary>
    NearestFixed = 8,
    
    /// <summary>
    /// Texture size will be screensize * anchorstretch and topleft position will be screensize * anchorposition
    /// </summary>
    Anchor = 16,
    
    /// <summary>
    /// 
    /// </summary>
    Custom = 32
}