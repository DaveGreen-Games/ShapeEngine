using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.StaticLib;
using ShapeEngine.StaticLib.Drawing;

namespace Examples.Scenes.ExampleScenes.PhysicsExampleSource;

//TODO: make physics controls
// - thrust forward
// - rotate left/right
// - drag for thrusting and drag for no input
// - ship should be controlled by phsics -> use velocity and add force etc.
public class Ship : CollisionObject, ICameraFollowTarget
{
    private TriangleCollider hull;
    private Size hullSize;
    private readonly PaletteColor paletteColor;
    
    private ValueRange dragRange = new (0.1f, 0.9f);
    // private ValueRange thrustForceRange = new (150000f, 800000f);
    private float thrustForce = 400000;
    private float breakTimer = 0f;
    private float breakDuration = 1f;
    
    private ValueRange rotationSpeedDegRange = new (25f, 200f);
    private float rotationSpeedTimer = 0f;
    private float rotationSpeedDuration = 1.5f;
    
    private Vector2 curRotationDirection = Vector2.Zero;
    private Vector2 curVelocityDirection = Vector2.Zero;
    public float CurSpeed { get; private set; } = 0f;
    public float MaxSpeed { get; private set; }= 1250f;
    public Ship(float size, PaletteColor color)
    {
        hullSize = new Size(size, size);
        Transform = new Transform2D(Vector2.Zero, 0f, hullSize, 1f);
        paletteColor = color;
        var offset = new Transform2D();
        var a = new Vector2(0.5f, 0f);
        var b = new Vector2(-0.5f, -0.5f);
        var c = new Vector2(-0.5f, 0.5f);
        hull = new TriangleCollider(offset, a, b, c);
        hull.ComputeCollision = true;
        hull.ComputeIntersections = true;
        hull.CollisionLayer = (uint)CollisionLayers.Ship;
        hull.CollisionMask = new BitFlag((uint)CollisionLayers.Asteroid, (uint)CollisionLayers.FrictionZone);
        AddCollider(hull);

        DragCoefficient = dragRange.Min;
        Mass = 1000;
    }

    public void Spawn(Vector2 position, float rotationDeg)
    {
        Transform = Transform.SetPosition(position);
        Transform = Transform.SetRotationRad(rotationDeg);
        curRotationDirection = ShapeVec.VecFromAngleRad(rotationDeg);
        curVelocityDirection = curRotationDirection;
    }
    
    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        
        
        var dt = time.Delta;
        curRotationDirection = ShapeVec.VecFromAngleRad(Transform.RotationRad);
        if (ShapeKeyboardButton.W.GetInputState().Down)
        {
            if (breakTimer > 0f)
            {
                breakTimer -= dt * 4f;
                if(breakTimer < 0f) breakTimer = 0f;
            }
            

            DragCoefficient = dragRange.Min;
            AddForce(curRotationDirection * thrustForce);
        }
        else if (ShapeKeyboardButton.S.GetInputState().Down)
        {
            if (breakTimer < breakDuration)
            {
                breakTimer += dt;
                if(breakTimer > breakDuration) breakTimer = breakDuration;
            }
            float f = breakTimer / breakDuration;
            DragCoefficient = dragRange.Lerp(f);
        }
        else
        {
            if (breakTimer > 0f)
            {
                breakTimer -= dt * 2;
                if(breakTimer < 0f) breakTimer = 0f;
            }

            DragCoefficient = dragRange.Min;
            // float f = thrustForceTimer / thrustForceDuration;
            // Drag = dragRange.Lerp(f);
        }
        
        if (ShapeKeyboardButton.A.GetInputState().Down)
        {
            if (rotationSpeedTimer < rotationSpeedDuration)
            {
                rotationSpeedTimer += dt;
                if(rotationSpeedTimer > rotationSpeedDuration) rotationSpeedTimer = rotationSpeedDuration;
            }

            float f = rotationSpeedTimer / rotationSpeedDuration;
            var rotationSpeedDeg = rotationSpeedDegRange.Lerp(f);
            
            Transform = Transform.ChangeRotationDeg(-rotationSpeedDeg * dt);
        }
        else if (ShapeKeyboardButton.D.GetInputState().Down)
        {
            if (rotationSpeedTimer < rotationSpeedDuration)
            {
                rotationSpeedTimer += dt;
                if(rotationSpeedTimer > rotationSpeedDuration) rotationSpeedTimer = rotationSpeedDuration;
            }

            float f = rotationSpeedTimer / rotationSpeedDuration;
            var rotationSpeedDeg = rotationSpeedDegRange.Lerp(f);
            
            Transform = Transform.ChangeRotationDeg(rotationSpeedDeg * dt);
        }
        else
        {
            if (rotationSpeedTimer > 0f)
            {
                rotationSpeedTimer -= dt * 4;
                if(rotationSpeedTimer < 0f) rotationSpeedTimer = 0f;
            }
        }
        
        
        base.Update(time, game, gameUi, ui);
        
        if (Velocity.LengthSquared() > 0f)
        {
            curVelocityDirection = Velocity.Normalize();
            CurSpeed = Velocity.Length();
            if (CurSpeed > MaxSpeed)
            {
                CurSpeed = MaxSpeed;
                Velocity = curVelocityDirection * MaxSpeed;
            }
        }
    }

    public override void DrawGame(ScreenInfo game)
    {
        var t = hull.GetTriangleShape();
        var thickness = hullSize.Length * 0.025f;
        var c = paletteColor.ColorRgba;
        t.DrawLines(thickness, c, LineCapType.Capped, 6);

        var radius = hullSize.Radius * 2;
        var center = Transform.Position;
        var circle = new Circle(center, radius );
        circle.DrawLines(thickness, c, 4f);

        var speedF = CurSpeed / MaxSpeed;
        var p = center + curVelocityDirection * radius * speedF;
        var s = ShapeMath.LerpFloat(thickness, thickness * 8, speedF);
        p.Draw(s, c, 16);
        
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        
    }

    public void FollowStarted()
    {
        
    }

    public void FollowEnded()
    {
        
    }

    public Vector2 GetCameraFollowPosition() => Transform.Position;
}