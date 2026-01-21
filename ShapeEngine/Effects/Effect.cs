using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Effects;

/// <summary>
/// Represents a basic effect object with position, size, and rotation.
/// </summary>
public class Effect : EffectObject
{
    /// <summary>
    /// Gets or sets the rotation in radians.
    /// </summary>
    public float RotRad { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Effect"/> class.
    /// </summary>
    /// <param name="pos">The position of the effect.</param>
    /// <param name="size">The size of the effect.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    public Effect(Vector2 pos, Size size, float rotRad) : base(pos, size) { RotRad = rotRad; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Effect"/> class with a specified lifetime.
    /// </summary>
    /// <param name="pos">The position of the effect.</param>
    /// <param name="size">The size of the effect.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="lifeTime">The lifetime of the effect in seconds.</param>
    public Effect(Vector2 pos, Size size, float rotRad, float lifeTime) : base(pos, size, lifeTime) { RotRad = rotRad; }
    
    /// <summary>
    /// Draws the effect on the game screen.
    /// </summary>
    /// <param name="game">The game screen info.</param>
    public override void DrawGame(ScreenInfo game)
    {
        
    }

    /// <summary>
    /// Draws the effect on the game UI screen.
    /// </summary>
    /// <param name="gameUi">The game UI screen info.</param>
    public override void DrawGameUI(ScreenInfo gameUi)
    {
        
    }
}

