using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Geometry.Circle;
using ShapeEngine.StaticLib;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class LaserBeamParticle
{
    private Vector2 position;
    private Vector2 velocity;
    private readonly PaletteColor paletteColor;
    private float lifetime;
    private float size;
    private float lifetimeTimer;
    private float lifetimeF;
    
    public LaserBeamParticle(Vector2 position, Vector2 velocity, float size, float lifetime, PaletteColor paletteColor)
    {
        this.position = position;
        this.velocity = velocity;
        this.paletteColor = paletteColor;
        this.lifetime = lifetime;
        this.size = size;
        lifetimeTimer = lifetime;
        lifetimeF = 0f;
    }
    
    public bool IsDead => lifetimeTimer <= 0f;
    
    public void Update(float dt)
    {
        if (IsDead) return;
        
        lifetimeTimer -= dt;
        if(lifetimeTimer <= 0f) lifetimeTimer = 0f;
        
        lifetimeF = 1f - (lifetimeTimer / lifetime);
        
        position += velocity * dt;
        ShapeVec.ExpDecayLerp(velocity, Vector2.Zero, lifetimeF, dt);
    }
    
    public void Draw()
    {
        var color = paletteColor.ColorRgba;
        var s = ShapeMath.LerpFloat(size, size * 0.25f, lifetimeF);
        var c = color.Lerp(color.ChangeAlpha(150), lifetimeF);
        CircleDrawing.DrawCircleFast(position, s, c);
    }
}