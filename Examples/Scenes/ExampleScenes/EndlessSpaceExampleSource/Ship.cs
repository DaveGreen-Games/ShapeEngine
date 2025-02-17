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
using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class Ship : CollisionObject, ICameraFollowTarget
{

    public event Action? OnKilled;
    
    public static readonly uint CollisionLayer = BitFlag.GetFlagUint(3);
    // private Triangle hull;
    private float shipSize;
    // private Vector2 pivot;
    private TriangleCollider collider;
    private Vector2 movementDir;
    private float angleRad = 0f;
    private float stopTimer = 0f;
    private float accelTimer = 0f;
    private float curSpeed = 0f;
    public float CurSpeed => curSpeed;
    private const float AccelTime = 0.25f;
    private const float StopTime = 0.5f;
    public const float Speed = 750;

    private const float CollisionStunTime = 0.5f;
    private float collisionStunTimer = 0f;
    private float collisionRotationDirection = 1;
    private const float CollisionRotationSpeedRad = MathF.PI * 3;

    
    private readonly PaletteColor hullColor = Colors.PcCold;

    private InputAction iaMoveHor;
    private InputAction iaMoveVer;
    public int Health;
    public float HealthF => (float)Health / (float)MaxHp;
    public const int MaxHp = 3;
    private LaserBeam laserBeam;
    public CollisionHandler? collisionHandler;
    private void SetupInput()
    {
        var moveHorKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
        var moveHor2GP = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
        var moveHorMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
        iaMoveHor = new(moveHorKB, moveHor2GP, moveHorMW);
        
        var moveVerKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
        var moveVer2GP = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
        var moveVerMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
        iaMoveVer = new(moveVerKB, moveVer2GP, moveVerMW);
    }
    public Ship(Vector2 pos, float shipSize)
    {
        this.shipSize = shipSize;
        Transform = new(pos, 0f, new Size(shipSize, 0f), 1f);
        DragCoefficient = 5;
        var hull = CreateHull();
        collider = new(new(), hull.A, hull.B, hull.C);
        collider.ComputeCollision = true;
        collider.ComputeIntersections = true;
        collider.CollisionLayer = CollisionLayer;
        collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
        // collider.OnIntersected += OnColliderCollision;
        // collider.OnCollisionEnded += OnColliderCollisionEnded;
        AddCollider(collider);
        hull = collider.GetTriangleShape();
        SetupInput();

        Health = MaxHp;
        laserBeam = new(new ValueRange(6, 18), Colors.PcWarm, new BitFlag(AsteroidObstacle.CollisionLayer), 10, 2000);
    }

    protected override void Collision(CollisionInformation info)
    {
        if(info.Count <= 0 || info.Other is not AsteroidObstacle a) return;
        if(!info.Validate(out CollisionPoint combined)) return;
            
        a.Cut(GetCutShape());
            
        if (collisionStunTimer <= 0f)
        {
            Health--;
            if (Health <= 0)
            {
                collider.Enabled = false;
                Kill();
                OnKilled?.Invoke();
            }
        }
        if (combined.Valid)
        {
            Velocity = combined.Normal * 3500;
            collisionStunTimer = CollisionStunTime;
            collisionRotationDirection = Rng.Instance.RandDirF();
            laserBeam.Cancel();
        }
    }
    
    
    public Polygon GetCutShape()
    {
        return Polygon.Generate(Transform.Position, 12, shipSize * 1.5f, shipSize * 3);
    }

    // private Triangle CreateHull(Vector2 pos, float size)
    // {
    //     var a = pos + new Vector2(size, 0);
    //     var b = pos + new Vector2(-size, -size * 0.75f);
    //     var c = pos + new Vector2(-size, size * 0.75f);
    //     // pivot = pos;
    //     return new Triangle(a, b, c);
    // }
    private Triangle CreateHull()
    {
        var a = new Vector2(1, 0);
        var b = new Vector2(-1, -0.75f);
        var c = new Vector2(-1, 0.75f);
        return new Triangle(a, b, c);
    }
    public string GetInputDescription(InputDeviceType inputDeviceType)
    {
        string hor = iaMoveHor.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
        string ver = iaMoveVer.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
        return $"Move Horizontal [{hor}] Vertical [{ver}]";
    }
    public void Reset(Vector2 pos, float size)
    {
        if (IsDead) Revive();
        
        shipSize = size;
        Transform = new(pos, 0f, new Size(shipSize, 0f), 1f);
        DragCoefficient = 5;
        var hull = CreateHull();
        collider = new(new(), hull.A, hull.B, hull.C);
        collider.ComputeCollision = true;
        collider.ComputeIntersections = true;
        collider.CollisionLayer = CollisionLayer;
        collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
        // collider.OnIntersected += OnColliderCollision;
        // collider.OnCollisionEnded += OnColliderCollisionEnded;
        AddCollider(collider);
        hull = collider.GetTriangleShape();
        movementDir = new(0, 0);
        angleRad = 0f;
        collisionStunTimer = 0f;
        Velocity = new(0f);
        Health = MaxHp;
    }

    private void Move(float dt, Vector2 dir, float speed)
    {
        movementDir = dir; // amount.Normalize();
        var newAngle = movementDir.AngleRad();
        var angleDif = ShapeMath.GetShortestAngleRad(angleRad, newAngle);
        var movement = movementDir * speed * dt;

        Transform = Transform.ChangePosition(movement);
        
        // hull = hull.ChangePosition(movement);
        // pivot += movement;

        if (collisionStunTimer > 0f)
        {
            var angleMovement = CollisionRotationSpeedRad * collisionRotationDirection * dt;
            Transform = Transform.ChangeRotationRad(angleMovement);
            angleRad += angleMovement; 
        }
        else
        {
            var angleMovement = MathF.Sign(angleDif) * dt * MathF.PI * 4f;
            if (MathF.Abs(angleMovement) > MathF.Abs(angleDif))
            {
                angleMovement = angleDif;
            }

            Transform = Transform.ChangeRotationRad(angleMovement);
            angleRad += angleMovement; 
        }
        
        
        
        // hull = hull.ChangeRotation(angleMovement, hull.GetCentroid());
    }
    

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
    {
        if(IsDead)return;
        base.Update(time, game, gameUi, ui);

        if (collisionStunTimer > 0f)
        {
            collisionStunTimer -= time.Delta;
            
            if (collisionStunTimer <= 0f)
            {
                Velocity = new();
            }
        }

        var gamepad = GAMELOOP.CurGamepad;
        GAMELOOP.MouseControlEnabled = gamepad?.IsDown(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, 0.1f) ?? true;
        
        
        float dt = time.Delta;
        iaMoveHor.Gamepad = GAMELOOP.CurGamepad;
        iaMoveHor.Update(dt);
        
        iaMoveVer.Gamepad = GAMELOOP.CurGamepad;
        iaMoveVer.Update(dt);
        
        Vector2 dir = new(iaMoveHor.State.AxisRaw, iaMoveVer.State.AxisRaw);

        if (collisionStunTimer > 0f) dir = new(0f);

        if (collisionHandler != null)
        {
            laserBeam.Update(GetBarrelPosition(), GetBarrelDirection(), time.Delta, collisionHandler);
        }

        if (laserBeam.IsActive)
        {
            movementDir = (game.MousePos - Transform.Position).Normalize();
            angleRad = movementDir.AngleRad(); 
            Transform = Transform.SetRotationRad(angleRad);
        }
        else
        {
            float lsq = dir.LengthSquared();
            if (lsq > 0f)
            {
                stopTimer = 0f;

                float accelF = 1f;
                if (accelTimer <= AccelTime)
                {
                    accelTimer += dt;
                    accelF = accelTimer / AccelTime;
                }
                curSpeed = Speed * accelF * accelF;
                Move(dt, dir.Normalize(), curSpeed);
            
            }
            else
            {
                accelTimer = 0f;
                if (stopTimer <= StopTime)
                {
                    stopTimer += dt;
                    float stopF = 1f - (stopTimer / StopTime);

                    curSpeed = Speed * stopF * stopF;
                    Move(dt, movementDir, curSpeed);
                }
                else
                {
                    movementDir = new();
                    curSpeed = 0f;
                }
            }
        }
        
        
        // hull = collider.GetTriangleShape();
    }
    public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        
    }
    public void FollowStarted()
    {
        
    }
    public void FollowEnded()
    {
        
    }

    public Vector2 GetPosition() => Transform.Position;

    public Vector2 GetCameraFollowPosition() => Transform.Position;
    public Vector2 GetBarrelPosition() => collider.GetTriangleShape().A;
    // public Vector2 GetLaserBeamPosition() => collider.GetTriangleShape().A;
    public Vector2 GetBarrelDirection() => ShapeVec.VecFromAngleRad(angleRad);
    public float GetCurSpeed() => curSpeed;
    public Vector2 GetCurVelocityVector() => curSpeed * movementDir;

    public override void DrawGame(ScreenInfo game)
    {
        if(IsDead)return;
        collider.GetTriangleShape().DrawLines(4f, hullColor.ColorRgba);
        laserBeam.Draw();
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        
    }
}