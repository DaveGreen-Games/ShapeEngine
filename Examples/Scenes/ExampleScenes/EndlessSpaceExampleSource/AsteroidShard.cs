using System.Numerics;
using ShapeEngine.Geometry.Triangle;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;
using ShapeEngine.StaticLib.Drawing;
namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class AsteroidShard
{
    public Triangle Triangle;
    private Triangle scaledTriangle;
    public Vector2 Velocity;
    public float Lifetime;
    public float LifetimeTimer;
    public AsteroidShard(Triangle triangle)
    {
        this.Triangle = triangle;
        scaledTriangle = triangle;
        Velocity = Rng.Instance.RandVec2(1250, 1500);
        Lifetime = Rng.Instance.RandF(2f, 2.5f);
        LifetimeTimer = Lifetime;
    }

    public bool IsDead => LifetimeTimer <= 0f;
    public void Update(float dt)
    {
        if (IsDead) return;
        LifetimeTimer -= dt;

        Velocity = ShapePhysics.ApplyDragForce(Velocity, 0.8f, dt);
        
        var f = LifetimeTimer / Lifetime;

        var scale = ShapeMath.LerpFloat(0.1f, 1f, f * f);
        
        Triangle = Triangle.ChangePosition(Velocity * dt);
        scaledTriangle = Triangle.ScaleSize(scale, Triangle.GetCentroid());


    }

    public void Draw()
    {
        if (IsDead) return;
        scaledTriangle.Draw(Colors.PcBackground.ColorRgba);
        scaledTriangle.DrawLines(EndlessSpaceCollision.AsteroidLineThickness / 2, Colors.PcHighlight.ColorRgba.SetAlpha(150));
    }
}