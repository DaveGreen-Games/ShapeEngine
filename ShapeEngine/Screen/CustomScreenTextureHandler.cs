using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Screen;

public abstract class CustomScreenTextureHandler
{
    /// <summary>
    /// The destination rectangle for drawing the screen texture to the screen. This dictates the area of the screen the
    /// screen texture is drawn to.
    /// </summary>
    /// <param name="screenDimensions"></param>
    /// <param name="textureDimensions"></param>
    /// <returns></returns>
    public virtual Rect GetDestinationRect(Dimensions screenDimensions, Dimensions textureDimensions) => new Rect(0, 0, screenDimensions.Width, screenDimensions.Height);
    
    /// <summary>
    /// The source rectangle of the screen texture used for drawing to the screen. Dictates what area of the screen texture
    /// is drawn to the screen. Height needs to be inverted (multiplied by negative one) due to open gl.
    /// </summary>
    /// <param name="screenDimensions"></param>
    /// <param name="textureDimensions"></param>
    /// <returns></returns>
    public virtual Rect GetSourceRect(Dimensions screenDimensions, Dimensions textureDimensions) => new Rect(0, 0, textureDimensions.Width, -textureDimensions.Height);
    
    /// <summary>
    /// Dictates the origin of the screen texture. Should be 0,0 and x, y of the the destination rectangle should be
    /// used instead:
    /// </summary>
    /// <param name="screenDimensions"></param>
    /// <param name="textureDimensions"></param>
    /// <returns></returns>
    public virtual Vector2 GetOrigin(Dimensions screenDimensions, Dimensions textureDimensions) => new(0, 0);
    
    /// <summary>
    /// The rotation the screen texture should be drawn to the screen with. 
    /// </summary>
    /// <returns></returns>
    public virtual float GetRotation() => 0f;
    
    /// <summary>
    /// Called in update to scale the global mouse position.
    /// </summary>
    /// <param name="mousePosition">The global mouse position.</param>
    /// <param name="screenDimensions"></param>
    /// <param name="textureDimensions"></param>
    /// <returns>Return the scaled mouse position for the custom screen texture.</returns>
    public virtual Vector2 GetScaledMousePosition(Vector2 mousePosition, Dimensions screenDimensions, Dimensions textureDimensions) => mousePosition;
    /// <summary>
    /// Called in update to get the current texture rect of the custom screen texture.
    /// </summary>
    /// <param name="screenDimensions"></param>
    /// <param name="textureDimensions"></param>
    /// <returns></returns>
    public virtual Rect GetTextureRect(Dimensions screenDimensions, Dimensions textureDimensions) => new Rect(0, 0, textureDimensions.Width, textureDimensions.Height);

    /// <summary>
    /// Return the new dimensions for the custom screen texture. Return invalid dimensions to keep the screen texture the same.
    /// </summary>
    /// <param name="newScreenDimensions"></param>
    /// <returns></returns>
    public virtual Dimensions OnScreenDimensionsChanged(Dimensions newScreenDimensions) => Dimensions.GetInvalidDimension();
    public virtual (ColorRgba color, bool clear) GetBackgroundClearColor() => (ColorRgba.Clear, true);
}