using System.Numerics;
using nkast.Aether.Physics2D.Dynamics;
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


//TODO: add effects for collision
// - Shield should flash and scale
// - Energy Sparks particle effect at imact point
// - Energy Blob should be created at impact point? 
// - Start with polygon split function
// - Polygon Crack function (could use create lightning function)

public class Ship : GameObject, ICameraFollowTarget
{
    // private TriangleCollider hull;
    private readonly Triangle hullRelative;
    private Triangle hullAbsolute;
    // private CircleCollider hullCollider;
    private readonly Size hullSize;
    private readonly PaletteColor paletteColor;
    
    private readonly ValueRange linearDampingRange = new (0.01f, 5f);
    private readonly ValueRange angularDampingRange = new (1f, 1f);
    // private ValueRange thrustForceRange = new (150000f, 800000f);
    public float ThrustForce { get; private set; } = 8000000;
    public float SteerForce { get; private set; } = 2000000;
    private float breakTimer = 0f;
    private readonly float breakDuration = 1f;
    
    // private ValueRange rotationSpeedDegRange = new (25f, 200f);
    private float rotationSpeedTimer = 0f;
    private readonly float rotationSpeedDuration = 1.5f;
    
    private Vector2 curRotationDirection = Vector2.Zero;
    private Vector2 curVelocityDirection = Vector2.Zero;
    public float CurSpeed { get; private set; } = 0f;
    public float MaxSpeed { get; private set; }= 1250f;
    public float CurSpeedF => CurSpeed / MaxSpeed;
    private Vector2 curCameraOffset = Vector2.Zero;
    private float curDelta = 0f;

    private readonly Body body;
    private CurveFloat cameraPositionOffsetCurve = new(3)
    {
        (0, 0),
        (0.75f, 500),
        (1f, 1000)
    };
    public Ship(float size, PaletteColor color, World world)
    {
        hullSize = new Size(size, size);
        Transform = new Transform2D(Vector2.Zero, 0f, hullSize, 1f);
        paletteColor = color;
        var a = new Vector2(0.5f, 0f);
        var b = new Vector2(-0.5f, -0.5f);
        var c = new Vector2(-0.5f, 0.5f);
        hullRelative = new Triangle(a, b, c);
        body = world.CreateBody(Transform.Position.ToAetherVector2(), 0, BodyType.Dynamic);
        body.Mass = 1000;
        body.LinearDamping = linearDampingRange.Min;
        body.AngularDamping = angularDampingRange.Min;
        var fixture = body.CreateCircle(hullSize.Radius, 1f);
        fixture.Restitution = 0.3f;
        fixture.Friction = 0.5f;
        
        hullAbsolute = hullRelative.ApplyTransform(Transform);
    }

    public void Spawn(Vector2 position, float rotationDeg)
    {
        body.SetTransform(position.ToAetherVector2(), rotationDeg * ShapeMath.DEGTORAD);
        Transform = Transform.SetPosition(position);
        Transform = Transform.SetRotationRad(rotationDeg);
        curRotationDirection = ShapeVec.VecFromAngleRad(rotationDeg);
        curVelocityDirection = body.LinearVelocity.FromAetherVector2().Normalize();
    }

    public override Rect GetBoundingBox()
    {
        return hullAbsolute.GetBoundingBox();
    }

    public void UpdatePhysicsState()
    {
        Transform = Transform.SetPosition(body.Position.FromAetherVector2());
        Transform = Transform.SetRotationRad(body.Rotation);
        
        curRotationDirection = ShapeVec.VecFromAngleRad(Transform.RotationRad);
        hullAbsolute = hullRelative.ApplyTransform(Transform);
        var curVelocity = body.LinearVelocity.FromAetherVector2();
        curVelocityDirection = curVelocity.Normalize();
        CurSpeed = curVelocity.Length();

        // if (Velocity.LengthSquared() > 0f)
        // {
        //     curVelocityDirection = Velocity.Normalize();
        //     CurSpeed = Velocity.Length();
        //     if (CurSpeed > MaxSpeed)
        //     {
        //         CurSpeed = MaxSpeed;
        //         Velocity = curVelocityDirection * MaxSpeed;
        //     }
        // }
    }
    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        float dt = time.Delta;
        
        curRotationDirection = ShapeVec.VecFromAngleRad(Transform.RotationRad);
        if (ShapeKeyboardButton.W.GetInputState().Down)
        {
            if (breakTimer > 0f)
            {
                breakTimer -= dt * 4f;
                if(breakTimer < 0f) breakTimer = 0f;
            }

            body.LinearDamping = linearDampingRange.Min;
            body.ApplyForce(curRotationDirection.ToAetherVector2() * ThrustForce);
        }
        else if (ShapeKeyboardButton.S.GetInputState().Down)
        {
            if (breakTimer < breakDuration)
            {
                breakTimer += dt;
                if(breakTimer > breakDuration) breakTimer = breakDuration;
            }
            float f = breakTimer / breakDuration;
            body.LinearDamping = linearDampingRange.Lerp(f);
        }
        else
        {
            if (breakTimer > 0f)
            {
                breakTimer -= dt * 2;
                if(breakTimer < 0f) breakTimer = 0f;
            }

            body.LinearDamping = linearDampingRange.Min;
            
        }
        
        if (ShapeKeyboardButton.A.GetInputState().Down)
        {
            if (rotationSpeedTimer < rotationSpeedDuration)
            {
                rotationSpeedTimer += dt;
                if(rotationSpeedTimer > rotationSpeedDuration) rotationSpeedTimer = rotationSpeedDuration;
            }

            float f = rotationSpeedTimer / rotationSpeedDuration;
            body.AngularDamping = angularDampingRange.Lerp(f);
            body.ApplyAngularImpulse(-SteerForce);
        }
        else if (ShapeKeyboardButton.D.GetInputState().Down)
        {
            if (rotationSpeedTimer < rotationSpeedDuration)
            {
                rotationSpeedTimer += dt;
                if(rotationSpeedTimer > rotationSpeedDuration) rotationSpeedTimer = rotationSpeedDuration;
            }

            float f = rotationSpeedTimer / rotationSpeedDuration;
            body.AngularDamping = angularDampingRange.Lerp(f);
            body.ApplyAngularImpulse(SteerForce);
        }
        else
        {
            if (rotationSpeedTimer > 0f)
            {
                rotationSpeedTimer -= dt * 4;
                if(rotationSpeedTimer < 0f) rotationSpeedTimer = 0f;
            }
            float f = rotationSpeedTimer / rotationSpeedDuration;
            body.AngularDamping = angularDampingRange.Lerp(f);
        }
    }

    public override void DrawGame(ScreenInfo game)
    {
        // var t = hull.GetTriangleShape();
        var thickness = hullSize.Length * 0.025f;
        var c = paletteColor.ColorRgba;
        
        
        // var shield = hullCollider.GetCircleShape();
        // shield.DrawStriped(shield.Diameter / 10, 45f, new LineDrawingInfo(2f, Colors.Special.SetAlpha(200), LineCapType.None, 0), 0f);
        Circle shield = new Circle(Transform.Position, hullSize.Radius);
        shield.DrawLines(thickness, Colors.Special, 4f);
        
        hullAbsolute.DrawStriped(10f, Transform.RotationDeg + 90, new LineDrawingInfo(2f,c, LineCapType.None, 0), 0f);
        hullAbsolute.DrawLines(thickness, c, LineCapType.Capped, 6);

        var radius = hullSize.Radius * 2;
        var center = Transform.Position;

        var speedF = CurSpeed / MaxSpeed;
        var p = center + curVelocityDirection * radius * speedF;
        var s = ShapeMath.LerpFloat(thickness, thickness * 8, speedF);
        p.Draw(s, c, 16);
        
        var bodyPosition = body.Position.FromAetherVector2();
        bodyPosition.Draw(25, Colors.Special2, 32);
        
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

    public Vector2 GetCameraFollowPosition()
    {
        return body.Position.FromAetherVector2();
        // var targetCameraOffset = curVelocityDirection * cameraPositionOffsetCurve.Sample(CurSpeedF);
        //
        // curCameraOffset = curCameraOffset.ExpDecayLerp(targetCameraOffset, 0.9f, curDelta);
        // // curCameraOffset = curCameraOffset.Lerp(targetCameraOffset, 0.05f);
        //
        // return Transform.Position + curCameraOffset; //  ShapeMath.LerpFloat(0, 1500, CurSpeedF);
    }
}