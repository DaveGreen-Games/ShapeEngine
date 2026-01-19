using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.StripedDrawingDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class AsteroidObstacle : CollisionObject
{
    public static readonly uint CollisionLayer = BitFlag.GetPowerOfTwo(2);
    
    private static readonly LineDrawingInfo GappedLineInfo = new(EndlessSpaceCollision.AsteroidLineThickness, Colors.Special, LineCapType.CappedExtended, 4);
    private static readonly GappedOutlineDrawingInfo BigAsteroidGappedOutlineInfo = new GappedOutlineDrawingInfo(6, 0f, 0.35f);
    private static readonly GappedOutlineDrawingInfo SmallAsteroidGappedOutlineInfo = new GappedOutlineDrawingInfo(2, 0f, 0.75f);
    
    
    private PolygonCollider collider;
    public Triangulation Triangulation;
    private Rect bb;

    private float damageFlashTimer = 0f;
    private const float DamageFlashDuration = 0.25f;

    public float Health { get; private set; }

    public bool Big;

    private float perimeter = 0f;

    private float markTimer = 0f;
    private const float MarkDuration = 0.5f;

    

    public Ship? target = null;
    private readonly float chaseStrength = 0f;
    private float speed;
    private Vector2 damageForce = new Vector2(0f);

    private GappedOutlineDrawingInfo gappedOutlineInfo;
    
    // private bool moved = false;
    // public AsteroidObstacle(Vector2 center)
    // {
    //     this.shape = GenerateShape(center);
    //     this.bb = this.shape.GetBoundingBox();
    //     this.triangulation = shape.Triangulate();
    //
    // }

    public AsteroidObstacle(Polygon relativeShape, Vector2 pos, float size, bool big)
    {
        
        this.Big = big;
        if (big) Mass = 12f;
        else Mass = 1f;
        Transform = new(pos, 0f, new Size(size, 0f), 1f);
        var s = ShapeMath.LerpFloat(50, Ship.Speed / 5, EndlessSpaceCollision.DifficultyFactor);
        speed = Rng.Instance.RandF(0.9f, 1f) * s;
        Velocity = Rng.Instance.RandVec2() * speed;
        chaseStrength = ShapeMath.LerpFloat(0.5f, 1f, EndlessSpaceCollision.DifficultyFactor);
        if (!big) Velocity *= 3f;
        
        collider = new PolygonCollider(new(0f), relativeShape);
        collider.ComputeCollision = false;
        collider.ComputeIntersections = false;
        collider.CollisionLayer = CollisionLayer;
        // collider.CollisionMask = new BitFlag(CollisionLayer);
        // collider.OnCollision += OnColliderCollision;
        // collider.OnCollisionEnded += OnColliderCollisionEnded;
        
        AddCollider(collider);
        var shape = collider.GetPolygonShape();
        this.bb = shape.GetBoundingBox();
        this.Triangulation = shape.Triangulate();

        if (big)
        {
            Health = ShapeMath.LerpFloat(300, 650, EndlessSpaceCollision.DifficultyFactor) * Rng.Instance.RandF(0.9f, 1.1f);
            gappedOutlineInfo = BigAsteroidGappedOutlineInfo.ChangeStartOffset(Rng.Instance.RandF());
        }
        else
        {
            Health = ShapeMath.LerpFloat(25, 100, EndlessSpaceCollision.DifficultyFactor) * Rng.Instance.RandF(0.9f, 1.1f);
            gappedOutlineInfo = SmallAsteroidGappedOutlineInfo.ChangeStartOffset(Rng.Instance.RandF());
        }


    }

    public void Damage(Vector2 pos, float amount, Vector2 force)
    {
        if (IsDead) return;
        
        Health -= amount;
        if (Health <= 0f)
        {
            Kill();
        }
        else
        {
            damageFlashTimer = DamageFlashDuration;
            damageForce = force / Mass;
        }
    }

    protected override void Collision(CollisionInformation info)
    {
        if(info.Count <= 0) return;
        if(!info.Validate(out IntersectionPoint combined)) return;
        foreach (var collision in info)
        {
            if(collision.Points == null || collision.Points.Count <= 0 || !collision.FirstContact)continue;
                
            Velocity = Velocity.Reflect(combined.Normal);
        }
    }
    public void MoveTo(Vector2 newPosition)
    {
        var moved = newPosition - Transform.Position;
        Transform = Transform.SetPosition(newPosition);
        Triangulation.ChangePosition(moved);
        // moved = true;
    }
    
    protected override void WasKilled(string? killMessage = null, GameObject? killer = null)
    {
        target = null;
        markTimer = 0f;
    }

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
    {
        if (IsDead) return;
        var prevPosition = Transform.Position;

        if (markTimer > 0)
        {
            markTimer -= time.Delta;
            if (markTimer <= 0)
            {
                markTimer = 0f;
            }
        }
        
        base.Update(time, game, gameUi, ui);

        if (target != null)
        {
            var chasePosition = target.GetPosition();
            var chaseDir = (chasePosition - Transform.Position).Normalize();
            Velocity = Velocity.LerpDirection(chaseDir, chaseStrength); // chaseDir * speed;
        }
        
        var shape = collider.GetPolygonShape();
        bb = shape.GetBoundingBox();
        // Triangulation = shape.Triangulate();
        

        damageForce = ShapePhysics.ApplyDragForce(damageForce, 0.95f, time.Delta);
        Transform = Transform.ChangePosition(damageForce * time.Delta);

        if (damageFlashTimer > 0f)
        {
            damageFlashTimer -= time.Delta;
        }

        var moved = Transform.Position - prevPosition;
        Triangulation.ChangePosition(moved);
    }
    public Polygon GetShape() => collider.GetPolygonShape();

    public void Cut(Polygon cutShape)
    {
        // var shape = GetShape();
        // var result = shape.CutShape(cutShape);
        // if (result.newShapes.Count == 1)
        // {
        //     var newShape = result.newShapes[0];
        //     Transform = Transform.SetPosition(newShape.GetCentroid());
        //
        // }
        // else if (result.newShapes.Count > 1)
        // {
        //     
        // }
    }

    //marked by ship inactive laser
    public void Mark()
    {
        if (IsDead) return;
        markTimer = MarkDuration;
    }
    
    public override void DrawGame(ScreenInfo game)
    {
        if (IsDead) return;
        
        if (!bb.OverlapShape(game.Area)) return;

        Triangulation.Draw(Colors.PcBackground.ColorRgba);
        
        if (EndlessSpaceCollision.AsteroidLineThickness > 1)
        {
            var shape = collider.GetPolygonShape();
            var c = damageFlashTimer > 0f ? Colors.PcWarm.ColorRgba : Colors.PcHighlight.ColorRgba;
            shape.DrawLines(EndlessSpaceCollision.AsteroidLineThickness, c);
            
            if (Big)
            {

                shape.ScaleSize(1.25f);
                perimeter = shape.DrawGappedOutline(perimeter, GappedLineInfo, gappedOutlineInfo);
                gappedOutlineInfo = gappedOutlineInfo.MoveStartOffset(Game.Instance.Time.Delta * 0.1f);
            }
            else
            {
                shape.ScaleSize(1.5f);
                perimeter = shape.DrawGappedOutline(perimeter, GappedLineInfo, gappedOutlineInfo);
                gappedOutlineInfo = gappedOutlineInfo.MoveStartOffset(Game.Instance.Time.Delta * 0.25f);
            }

        }

        if (markTimer > 0f)
        {
            float markTimerF = markTimer / MarkDuration;
            var clearColor = Colors.Warm.SetAlpha(0);
            var color = clearColor.Lerp(Colors.Warm, markTimerF);
            
            if (markTimerF < 0.95f || !Big)
            {
                Transform.Position.Draw(24f, color);
            }
            else
            {
                var poly = collider.GetPolygonShape();
                poly.GetFurthestVertex(Transform.Position, out float disSquared, out int index);
                var c = new Circle(Transform.Position, MathF.Sqrt(disSquared));
                var info = new LineDrawingInfo(8f, color, LineCapType.None, 0);
                c.DrawStriped(112, 45, info);
            }
            
        }
    }
    public override void DrawGameUI(ScreenInfo gameUi)
    {
       
    }
}