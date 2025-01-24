using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Lib;

namespace Examples.PayloadSystem;

public abstract class PayloadMarker
{
    public event Action? OnTargetReached;

    public Vector2 Location { get; private set; }
    public Vector2 Velocity { get; private set; }
    public Vector2 Direction { get; private set; }
    public bool Launched { get; private set; } = false;
    public bool TargetReached => Launched && travelTimer <= 0f;

    private float dragCoefficient = 2f;
    protected float travelTime {get; private set;}= 1f;
    protected float travelTimer{get; private set;}= 0f;
    public float TravelF => travelTime <= 0f ? 0f : travelTimer / travelTime;

    public bool Launch(Vector2 start, Vector2 dir, float speed, float time = 1f, float drag = 2f)
    {
        if (Launched) return false;

        Launched = true;
        Location = start;
        Direction = dir;
        Velocity = dir * speed;
        travelTimer = time;
        travelTime = time;
        dragCoefficient = drag;
        
        WasLaunched();
        return true;
    }
    public bool Dismiss()
    {
        if (!Launched) return false;

        Launched = false;
        WasDismissed();
        return true;
    }
    public void Update(float dt)
    {
        if (travelTimer > 0f)
        {
            Velocity = ShapePhysics.ApplyDragForce(Velocity, dragCoefficient, dt);
            Location += Velocity * dt;
            travelTimer -= dt;
            if (travelTimer <= 0f)
            {
                travelTimer = 0f;
                Velocity = new();
                OnTargetReached?.Invoke();
                TargetWasReached();
            }
        }
        OnUpdate(dt);
    }

    public abstract void Draw();


    protected abstract void TargetWasReached();
    protected abstract void WasDismissed();
    protected abstract void WasLaunched();
    protected abstract void OnUpdate(float dt);
}