using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using System.Numerics;
using System.Text;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace Examples.Scenes.ExampleScenes
{
    internal abstract class GameObject : IGameObject
    {
        // protected List<ICollidable> Collidables = new();
        protected GameobjectBody body;

        // protected GameObject(GameobjectBody body)
        // {
        //     this.body = body;
        // }
        public int Layer { get; set; } = 0;

        public virtual bool IsDead()
        {
            return false;
        }

        public virtual bool Kill()
        {
            return false;
        }
        public void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            body.Update(time.Delta);
            OnUpdate(time.Delta);
            // var collidables = GetCollidables();
            // foreach (var c in collidables)
            // {
            // c.GetCollider().UpdateState(time.Delta);
            // }
        }

        protected virtual void OnUpdate(float dt)
        {
            
        }
        public abstract void DrawGame(ScreenInfo game);



        public Vector2 GetPosition() => body.Transform.Position;
        public Rect GetBoundingBox() => body.GetBoundingBox();

        // public abstract bool HasCollidables();
        // public List<ICollidable> GetCollidables() => Collidables;

        
        public bool HasCollisionBody() => true;
        public CollisionBody? GetCollisionBody() => body;

        public virtual void AddedToHandler(GameObjectHandler gameObjectHandler) { }

        public virtual void RemovedFromHandler(GameObjectHandler gameObjectHandler) { }

        public virtual void DrawGameUI(ScreenInfo ui)
        {
            
        }

        public bool CheckHandlerBounds()
        {
            return false;
        }

        public void LeftHandlerBounds(BoundsCollisionInfo info)
        {
            
        }

        // public void DeltaFactorApplied(float f)
        // {
        // }

        public bool DrawToGame(Rect gameArea)
        {
            return true;
        }

        public bool DrawToGameUI(Rect screenArea)
        {
            return false;
        }
    }

    internal class GameobjectBody : CollisionBody
    {
        public static readonly uint WallFlag = BitFlag.GetFlagUint(1); //2
        public static readonly uint RockFlag = BitFlag.GetFlagUint(2); //4
        public static readonly uint BirdFlag = BitFlag.GetFlagUint(3); //8
        public static readonly uint BallFlag = BitFlag.GetFlagUint(4); //16
        public static readonly uint BulletFlag = BitFlag.GetFlagUint(5); //32
    
        // protected BitFlag collisionMask = BitFlag.Empty;
        // protected ColorRgba BuffColorRgba = new(System.Drawing.Color.Gold);
        // protected bool buffed = false;
        // protected float startSpeed = 0f;
        // private float totalSpeedFactor = 1f;
        public float RotationSpeedDeg;
        public GameobjectBody(Vector2 position) : base(position)
        {
            this.RotationSpeedDeg = 0f;
        }
        public GameobjectBody(Vector2 position, float rotationSpeedDeg) : base(position)
        {
            this.RotationSpeedDeg = rotationSpeedDeg;
        }

        protected override void OnUpdateState(float dt)
        {
            if (RotationSpeedDeg != 0f)
            {
                Transform += RotationSpeedDeg * ShapeMath.DEGTORAD * dt;
                // Transform = Transform.RotateByDeg(RotationSpeedDeg * dt);
            }
           
        }

        // public void Buff(float f)
        // {
        //     if (totalSpeedFactor < 0.01f) return;
        //
        //     totalSpeedFactor *= f;
        //     Velocity = Velocity.Normalize() * startSpeed * totalSpeedFactor;
        //
        //     if (totalSpeedFactor != 1f) buffed = true;
        // }
        // public void EndBuff(float f)
        // {
        //     totalSpeedFactor /= f;
        //     Velocity = Velocity.Normalize() * startSpeed * totalSpeedFactor;
        //     if (totalSpeedFactor == 1f) buffed = false;
        // }


        // public abstract uint GetCollisionLayer();

        // public BitFlag GetCollisionMask() => collisionMask;

        // public virtual void Overlap(CollisionInformation info) { }

        // public virtual void OverlapEnded(Collider other) { }

        // public virtual void Update(float dt)
        // {
        //     //collider.UpdatePreviousPosition(dt);
        //     collider.UpdateState(dt);
        // }



    }
    internal class PolyWall : GameObject
    {
        private readonly PolyCollider polyCollider;
        // private Vector2 s;
        // private Vector2 e;
        // private Polygon p;
        public PolyWall(Vector2 start, Vector2 end)
        {
            // this.p = new Segment(start, end).Inflate(40f, 0.5f).ToPolygon();
            var col = new PolyCollider(new(),new Segment(start, end), 40f, 0.5f);
            col.ComputeCollision = false;
            col.ComputeIntersections = false;
            col.Enabled = true;
            col.CollisionMask = BitFlag.Empty;
            col.CollisionLayer = GameobjectBody.WallFlag;

            this.body = new((start + end) / 2 );
            
            body.AddCollider(col);

            polyCollider = col;
            // this.s = start;
            // this.e = end;
        }

        public override void DrawGame(ScreenInfo game)
        {
            // ShapeDrawing.DrawCircle(s, 15f, Colors.Cold, 12);
            // ShapeDrawing.DrawCircle(e, 15f, Colors.Warm, 12);
            // ShapeDrawing.DrawCircle(body.Position, 15f, Colors.Special, 12);
            // p.DrawLines(6f, Colors.Highlight);
            polyCollider.GetPolygonShape().DrawLines(4f, Colors.Special);
        }
    }
    internal class BoundaryWall : GameObject
    {
        private PolyCollider polyCollider;
        public BoundaryWall(Vector2 start, Vector2 end)
        {
            var col = new PolyCollider(new(),new Segment(start, end), 40f, 0.5f);
            col.ComputeCollision = false;
            col.ComputeIntersections = false;
            col.Enabled = true;
            col.CollisionMask = BitFlag.Empty;
            col.CollisionLayer = GameobjectBody.WallFlag;
            
            this.body = new(col.GetPolygonShape().GetCentroid());
            
            body.AddCollider(col);

            polyCollider = col;
        }

        public override void DrawGame(ScreenInfo game)
        {
            polyCollider.GetPolygonShape().DrawLines(8f, Colors.Highlight);
        }
    }

    internal class Ball : GameObject
    {
        private CircleCollider circleCollider;
        public Ball(Vector2 pos)
        {
            var col = new CircleCollider(new(0f), 12f);
            col.ComputeCollision = true;
            col.ComputeIntersections = true;
            col.Enabled = true;
            col.CollisionMask = new(GameobjectBody.WallFlag);
            col.CollisionLayer = GameobjectBody.BallFlag;

            this.body = new(pos);
            body.Velocity = ShapeRandom.RandVec2(50, 250);
            body.AddCollider(col);

            circleCollider = col;
            circleCollider.OnCollision += Overlap;
        }

        private void Overlap(CollisionInformation info)
        {
            if (info.CollisionSurface.Valid)
            {
                // timer = 0.25f;
                body.Velocity = body.Velocity.Reflect(info.CollisionSurface.Normal);
            }
        }
        public override void DrawGame(ScreenInfo game)
        {
            circleCollider.GetCircleShape().DrawLines(4f, Colors.Warm);
            // polyCollider.GetPolygonShape().DrawLines(4f, Colors.Highlight);
        }
    }
    internal class Bullet : GameObject
    {
        private CircleCollider circleCollider;
        private bool isDead = false;
        private float deadTimer = 0f;
        public Bullet(Vector2 pos)
        {
            var col = new CircleCollider(new(0f), 8f);
            col.ComputeCollision = true;
            col.ComputeIntersections = false;
            col.Enabled = true;
            col.CollisionMask = new(GameobjectBody.WallFlag);
            col.CollisionLayer = GameobjectBody.BulletFlag;

            this.body = new(pos);
            body.Velocity = ShapeRandom.RandVec2(1500, 2000);
            body.AddCollider(col);

            circleCollider = col;
            circleCollider.OnCollision += Overlap;
        }

        protected override void OnUpdate(float dt)
        {
            if (deadTimer > 0f)
            {
                deadTimer -= dt;
            }
        }

        public override bool IsDead() => isDead && deadTimer <= 0f;
        private void Overlap(CollisionInformation info)
        {
            if (info.Collisions.Count > 0)
            {
                body.Velocity = new();
                body.Enabled = false;
                deadTimer = 2f;
                isDead = true;
            }
            // if (info.CollisionSurface.Valid)
            // {
            //     // timer = 0.25f;
            //     body.Velocity = body.Velocity.Reflect(info.CollisionSurface.Normal);
            // }
        }
        public override void DrawGame(ScreenInfo game)
        {
            circleCollider.GetCircleShape().Draw( Colors.Cold);
            // polyCollider.GetPolygonShape().DrawLines(4f, Colors.Highlight);
        }
    }

    internal class Rock : GameObject
    {
        private PolyCollider polyCollider;
        public Rock(Vector2 pos)
        {
            var shape = Polygon.Generate(pos, 6, 5, 50);
            var col = new PolyCollider(shape, new(0f));
            col.ComputeCollision = true;
            col.ComputeIntersections = true;
            col.Enabled = true;
            col.CollisionMask = new(GameobjectBody.WallFlag);
            col.CollisionMask = col.CollisionMask.Add(GameobjectBody.RockFlag);
            col.CollisionLayer = GameobjectBody.RockFlag;

            var rotSpeedDeg = ShapeRandom.RandF(-90, 90);
            this.body = new(pos, rotSpeedDeg);
            body.Velocity = ShapeRandom.RandVec2(50, 250);
            body.AddCollider(col);

            polyCollider = col;
            polyCollider.OnCollision += Overlap;
        }

        
        private void Overlap(CollisionInformation info)
        {
            if (info.CollisionSurface.Valid)
            {
                // body.Transform = body.Transform.ScaleBy(ShapeRandom.RandF(0.1f, 0.2f));
                // body.Transform = body.Transform.SetScale(ShapeRandom.RandF(0.5f, 4f));
                // timer = 0.25f;
                body.Velocity = body.Velocity.Reflect(info.CollisionSurface.Normal);
            }
        }
        public override void DrawGame(ScreenInfo game)
        {
            polyCollider.GetPolygonShape().DrawLines(4f, Colors.Warm);
            // Circle c = new(body.Transform.Position, 5f);
            // c.Draw(Colors.Cold);
            // polyCollider.GetPolygonShape().DrawLines(4f, Colors.Highlight);
        }
    }
    internal class Bird : GameObject
    {
        private CircleCollider circleCollider;
        private TriangleCollider triangleCollider;
        public Bird(Vector2 pos)
        {
            const float radius = 24f;
            var cCol = new CircleCollider(new(0f), radius);
            cCol.ComputeCollision = true;
            cCol.ComputeIntersections = true;
            cCol.Enabled = true;
            cCol.CollisionMask = new(GameobjectBody.WallFlag);
            cCol.CollisionLayer = GameobjectBody.BirdFlag;

            var ta = new Vector2(0, -radius / 2);
            var tb = new Vector2(0, radius / 2);
            var tc = new Vector2(radius, 0);
            var tOffset = new Vector2(radius, 0f);
            var tCol = new TriangleCollider(ta, tb, tc, tOffset);
            tCol.ComputeCollision = true;
            tCol.ComputeIntersections = true;
            tCol.Enabled = true;
            tCol.CollisionMask = new(GameobjectBody.WallFlag);
            tCol.CollisionMask = tCol.CollisionMask.Add(GameobjectBody.BirdFlag);
            tCol.CollisionLayer = GameobjectBody.BirdFlag;
            

            this.body = new(pos);
            body.Velocity = ShapeRandom.RandVec2(50, 250);
            body.Transform = body.Transform.SetRotationRad(body.Velocity.AngleRad());
            body.AddCollider(cCol);
            body.AddCollider(tCol);

            circleCollider = cCol;
            circleCollider.OnCollision += Overlap;

            triangleCollider = tCol;
            triangleCollider.OnCollision += Overlap;
        }

        private void Overlap(CollisionInformation info)
        {
            if (info.CollisionSurface.Valid)
            {
                // timer = 0.25f;
                body.Velocity = body.Velocity.Reflect(info.CollisionSurface.Normal);
                body.Transform = body.Transform.SetRotationRad(body.Velocity.AngleRad());
            }
        }
        public override void DrawGame(ScreenInfo game)
        {
            circleCollider.GetCircleShape().DrawLines(4f, Colors.Warm);
            triangleCollider.GetTriangleShape().DrawLines(2f, Colors.Warm);

            // var vel = body.Velocity;
            // Segment s = new(body.Transform.Position, body.Transform.Position + vel);
            // s.Draw(2f, Colors.Cold);

            // var trianglePos = triangleCollider.CurTransform.Position;
            // var trianglePosCircle = new Circle(trianglePos, 6f);
            // trianglePosCircle.Draw(Colors.Cold);
        }
    }


    
    
    // internal class WallCollidable : Collidable
    // {
    //     public WallCollidable(Vector2 start, Vector2 end)
    //     {
    //         var col = new PolyCollider(new Segment(start, end), 10f, 1f);
    //         this.collider = col;
    //         this.collider.ComputeCollision = false;
    //         this.collider.ComputeIntersections = false;
    //         this.collider.Enabled = true;
    //
    //         
    //         this.collisionMask = BitFlag.Empty;
    //         
    //     }
    //
    //     public override uint GetCollisionLayer() => WallFlag;
    // }
    // internal class Wall : Gameobject
    // {
    //     WallCollidable wallCollidable;
    //     public Wall(Vector2 start, Vector2 end)
    //     {
    //         wallCollidable = new(start, end);
    //         Collidables.Add(wallCollidable);
    //     }
    //     public override void DrawGame(ScreenInfo game)
    //     {
    //         wallCollidable.GetCollider().DrawShape(8f, Colors.Highlight);
    //     }
    //
    //     public override Rect GetBoundingBox()
    //     {
    //         return wallCollidable.GetCollider().GetShape().GetBoundingBox();
    //     }
    //
    //
    //     // public override List<ICollidable> GetCollidables() => Collidables;
    //     // {
    //     //     return new() { wallCollidable };
    //     // }
    //
    //     public override Vector2 GetPosition()
    //     {
    //         return wallCollidable.GetCollider().Pos;
    //     }
    //
    //     public override bool HasCollidables()
    //     {
    //         return true;
    //     }
    // }
    //
    

    // internal class TrapCollidable : Collidable
    // {
    //     public TrapCollidable(Vector2 pos, Vector2 size)
    //     {
    //         this.collider = new RectCollider(pos, size, new Vector2(0.5f));
    //         this.collider.ComputeCollision = false;
    //         this.collider.ComputeIntersections = false;
    //         this.collider.Enabled = true;
    //         this.collider.FlippedNormals = true;
    //         this.collisionMask = new uint[] { };
    //     }
    //     public override uint GetCollisionLayer()
    //     {
    //         return WALL_ID;
    //     }
    // }
    // internal class Trap : Gameobject
    // {
    //     TrapCollidable trapCollidable;
    //     public Trap(Vector2 pos, Vector2 size)
    //     {
    //         this.trapCollidable = new(pos, size);
    //     }
    //     public override void DrawGame(ScreenInfo game)
    //     {
    //         trapCollidable.GetCollider().DrawShape(2f, ExampleScene.ColorHighlight1);
    //     }
    //
    //     public override Rect GetBoundingBox()
    //     {
    //         return trapCollidable.GetCollider().GetShape().GetBoundingBox();
    //     }
    //
    //     
    //     public override List<ICollidable> GetCollidables()
    //     {
    //         return new() { trapCollidable };
    //     }
    //
    //     public override Vector2 GetPosition()
    //     {
    //         return trapCollidable.GetCollider().Pos;
    //     }
    //
    //     public override bool HasCollidables()
    //     {
    //         return true;
    //     }
    // }

    // internal class AuraCollidable : Collidable
    // {
    //     float buffFactor = 1f;
    //     //HashSet<ICollidable> others = new();
    //     public AuraCollidable(Vector2 pos, float radius, float f)
    //     {
    //         var shape = Polygon.Generate(pos, 12, radius * 0.5f, radius);
    //         this.collider = new PolyCollider(shape, pos, new Vector2(0f));
    //         this.collider.ComputeCollision = true;
    //         this.collider.ComputeIntersections = false;
    //         this.collider.Enabled = true;
    //
    //         this.collisionMask = new(RockFlag); // BitFlag.Empty;
    //         this.collisionMask = this.collisionMask.Add(BallFlag);
    //         this.collisionMask = this.collisionMask.Add(BoxFlag);
    //         
    //         buffFactor= f;
    //     }
    //
    //     public override uint GetCollisionLayer() => AuraFlag;
    //     public override void Overlap(CollisionInformation info)
    //     {
    //         foreach (var c in info.Collisions)
    //         {
    //             if (c.FirstContact)
    //             {
    //                 if (c.Other is Collidable g) g.Buff(buffFactor);
    //             }
    //         }
    //     }
    //     public override void OverlapEnded(ICollidable other)
    //     {
    //         if (other is Collidable g) g.EndBuff(buffFactor);
    //     }
    // }
    // internal class Aura : Gameobject
    // {
    //     AuraCollidable auraCollidable;
    //     
    //     public Aura(Vector2 pos, float radius, float f)
    //     {
    //         this.auraCollidable = new(pos, radius, f);
    //         this.Collidables.Add(auraCollidable);
    //         
    //     }
    //
    //     public override void DrawGame(ScreenInfo game)
    //     {
    //         auraCollidable.GetCollider().DrawShape(2f, Colors.Highlight);
    //     }
    //
    //     public override Rect GetBoundingBox()
    //     {
    //         return auraCollidable.GetCollider().GetShape().GetBoundingBox();
    //     }
    //
    //
    //     // public override List<ICollidable> GetCollidables()
    //     // {
    //     //     return new() { auraCollidable };
    //     // }
    //
    //     public override Vector2 GetPosition()
    //     {
    //         return auraCollidable.GetCollider().Pos;
    //     }
    //
    //     public override bool HasCollidables()
    //     {
    //         return true;
    //     }
    // }
    //
    // internal class RockCollidable : Collidable
    // {
    //     float timer = 0f;
    //     public RockCollidable(Vector2 pos, Vector2 vel, float size)
    //     {
    //         this.collider = new CircleCollider(pos, vel, size * 0.5f);
    //         this.collider.ComputeCollision = true;
    //         this.collider.ComputeIntersections = true;
    //         this.collider.Enabled = true;
    //         this.collider.SimplifyCollision = false;
    //         this.collisionMask = new BitFlag(WallFlag); // ShapeFlag.SetUintFlag(0, WallFlag);
    //         this.startSpeed = vel.Length();
    //     }
    //
    //     public override uint GetCollisionLayer() => RockFlag;
    //     public override void Overlap(CollisionInformation info)
    //     {
    //         if (info.CollisionSurface.Valid)
    //         {
    //             timer = 0.25f;
    //             collider.Vel = collider.Vel.Reflect(info.CollisionSurface.Normal);
    //         }
    //     }
    //     public override void Update(float dt)
    //     {
    //         if (timer > 0f)
    //         {
    //             timer -= dt;
    //         }
    //     }
    //     public void Draw()
    //     {
    //         var color = new ColorRgba(System.Drawing.Color.CornflowerBlue);
    //         if (timer > 0) color = Colors.Highlight;
    //         if (buffed) color = BuffColorRgba;
    //         collider.DrawShape(2f, color);
    //
    //     }
    // }
    // internal class Rock : Gameobject
    // {
    //     RockCollidable rockCollidable;
    //     
    //     public Rock(Vector2 pos, Vector2 vel, float size)
    //     {
    //         this.rockCollidable = new(pos, vel, size);
    //         this.Collidables.Add(rockCollidable);
    //     }
    //
    //     public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
    //     {
    //         base.Update(time, game, ui);
    //         rockCollidable.Update(time.Delta);
    //     }
    //     public override void DrawGame(ScreenInfo game)
    //     {
    //         rockCollidable.Draw();
    //     }
    //
    //     // public override void DrawUI(ScreenInfo ui)
    //     // {
    //     //     GAMELOOP.Camera.WorldToScreen(GetBoundingBox()).DrawLines(2f, WHITE);
    //     // }
    //
    //     public override Vector2 GetPosition()
    //     {
    //         return rockCollidable.GetCollider().Pos;
    //     }
    //
    //     public override Rect GetBoundingBox()
    //     {
    //         return rockCollidable.GetCollider().GetShape().GetBoundingBox();
    //     }
    //
    //     public override bool HasCollidables()
    //     {
    //         return true;
    //     }
    //
    //     // public override List<ICollidable> GetCollidables()
    //     // {
    //     //     return new() { rockCollidable };
    //     // }
    //
    // }
    //
    // internal class BoxCollidable : Collidable
    // {
    //     float timer = 0f;
    //     public BoxCollidable(Vector2 pos, Vector2 vel, float size)
    //     {
    //         this.collider = new RectCollider(pos, vel, new Vector2(size, size), new Vector2(0.5f));
    //
    //         this.collider.ComputeCollision = true;
    //         this.collider.ComputeIntersections = true;
    //         this.collider.Enabled = true;
    //         this.collider.SimplifyCollision = false;
    //         this.collisionMask = new(WallFlag);
    //         this.collisionMask = this.collisionMask.Add(BallFlag);
    //         this.startSpeed = vel.Length();
    //     }
    //
    //     public override uint GetCollisionLayer() => BoxFlag;
    //     public override void Overlap(CollisionInformation info)
    //     {
    //         if (info.CollisionSurface.Valid)
    //         {
    //             timer = 0.25f;
    //             collider.Vel = collider.Vel.Reflect(info.CollisionSurface.Normal);
    //         }
    //     }
    //     public override void Update(float dt)
    //     {
    //         if (timer > 0f)
    //         {
    //             timer -= dt;
    //         }
    //     }
    //     public void Draw()
    //     {
    //         var color = new ColorRgba(System.Drawing.Color.MediumOrchid);
    //         if (timer > 0) color = Colors.Highlight;
    //         if (buffed) color = BuffColorRgba;
    //         if (collider is RectCollider r)
    //         {
    //             Rect shape = r.GetRectShape();
    //             shape.DrawLines(2f, color);
    //         }
    //     }
    // }
    // internal class Box : Gameobject
    // {
    //     BoxCollidable boxCollidable;
    //
    //     public Box(Vector2 pos, Vector2 vel, float size)
    //     {
    //         this.boxCollidable = new(pos, vel, size);
    //         this.Collidables.Add(boxCollidable);
    //     }
    //
    //     public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
    //     {
    //         base.Update(time, game, ui);
    //         boxCollidable.Update(time.Delta);
    //     }
    //     public override void DrawGame(ScreenInfo game)
    //     {
    //         boxCollidable.Draw();
    //     }
    //
    //     public override Vector2 GetPosition()
    //     {
    //         return boxCollidable.GetCollider().Pos;
    //     }
    //
    //     public override Rect GetBoundingBox()
    //     {
    //         return boxCollidable.GetCollider().GetShape().GetBoundingBox();
    //     }
    //
    //     public override bool HasCollidables()
    //     {
    //         return true;
    //     }
    //
    //     // public override List<ICollidable> GetCollidables()
    //     // {
    //     //     return new() { boxCollidable };
    //     // }
    //
    // }
    //
    // internal class BallCollidable : Collidable
    // {
    //     float timer = 0f;
    //     public BallCollidable(Vector2 pos, Vector2 vel, float size)
    //     {
    //         this.collider = new CircleCollider(pos, vel, size);
    //
    //         this.collider.ComputeCollision = true;
    //         this.collider.ComputeIntersections = true;
    //         this.collider.Enabled = true;
    //         this.collider.SimplifyCollision = false;
    //         this.collisionMask = new(WallFlag);
    //         this.collisionMask = this.collisionMask.Add(BoxFlag);
    //         this.startSpeed = vel.Length();
    //     }
    //
    //     public override uint GetCollisionLayer() => BallFlag;
    //     public override void Overlap(CollisionInformation info)
    //     {
    //         if (info.CollisionSurface.Valid)
    //         {
    //             timer = 0.25f;
    //             collider.Vel = collider.Vel.Reflect(info.CollisionSurface.Normal);
    //         }
    //     }
    //     public override void Update(float dt)
    //     {
    //         if (timer > 0f)
    //         {
    //             timer -= dt;
    //         }
    //     }
    //     public void Draw()
    //     {
    //         var color = new ColorRgba(System.Drawing.Color.ForestGreen);
    //         if (timer > 0) color = Colors.Highlight;
    //         if (buffed) color = BuffColorRgba;
    //
    //         if(collider is CircleCollider c)
    //         {
    //             ShapeDrawing.DrawCircleFast(c.Pos, c.Radius, color);
    //
    //         }
    //     }
    // }
    // internal class Ball : Gameobject
    // {
    //     BallCollidable ballCollidable;
    //
    //     public Ball(Vector2 pos, Vector2 vel, float size)
    //     {
    //         this.ballCollidable = new(pos, vel, size);
    //         this.Collidables.Add(ballCollidable);
    //     }
    //     
    //
    //     public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
    //     {
    //         base.Update(time, game, ui);
    //         ballCollidable.Update(time.Delta);
    //     }
    //     public override void DrawGame(ScreenInfo game)
    //     {
    //         ballCollidable.Draw();
    //     }
    //
    //     public override Vector2 GetPosition()
    //     {
    //         return ballCollidable.GetCollider().Pos;
    //     }
    //
    //     public override Rect GetBoundingBox()
    //     {
    //         return ballCollidable.GetCollider().GetShape().GetBoundingBox();
    //     }
    //
    //     public override bool HasCollidables()
    //     {
    //         return true;
    //     }
    //
    //     // public override List<ICollidable> GetCollidables()
    //     // {
    //     //     return new() { ballCollidable };
    //     // }
    //
    // }

    

    public class GameObjectHandlerExample : ExampleScene
    {
        GameObjectHandlerCollision gameObjectHandler;
        

        Rect boundaryRect;
        // private List<Wall> boundaryWalls = new();
        Font font;

        Vector2 startPoint = new();
        bool segmentStarted = false;
        bool drawDebug = false;

        private readonly InputAction iaSpawnRock;
        private readonly InputAction iaSpawnBall;
        private readonly InputAction iaSpawnBird;
        private readonly InputAction iaSpawnBullet;
        // private readonly InputAction iaSpawnBox;
        // private readonly InputAction iaSpawnAura;
        private readonly InputAction iaToggleDebug;
        private readonly InputAction iaPlaceWall;
        private readonly InputAction iaCancelWall;
        private readonly InputAction iaMoveCameraH;
        private readonly InputAction iaMoveCameraV;
        private readonly List<InputAction> inputActions;
        public GameObjectHandlerExample()
        {
            Title = "Gameobject Handler Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            var placeWallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var placeWallGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            var placeWallMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            iaPlaceWall = new(placeWallKB, placeWallGP, placeWallMB);
            
            var cancelWallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var cancelWallGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            var cancelWallMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaCancelWall = new(cancelWallKB, cancelWallGP, cancelWallMB);
            
            var spawnRockKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ONE);
            var spawnRockGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnRock = new(spawnRockKB, spawnRockGB);
            
            // var spawnBoxKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TWO);
            // var spawnBoxGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            // iaSpawnBox = new(spawnBoxKB, spawnBoxGB);
            
            var spawnBallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.THREE);
            var spawnBallGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnBall = new(spawnBallKB, spawnBallGB);
            
            var spawnBirdKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TWO);
            var spawnBirdGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnBird = new(spawnBirdKB, spawnBirdGp);
            
            var spawnBulletKb = new InputTypeKeyboardButton(ShapeKeyboardButton.FOUR);
            var spawnBulletGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP , 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnBullet = new(spawnBulletKb, spawnBulletGp);
            
            var toggleDebugKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var toggleDebugGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            iaToggleDebug = new(toggleDebugKB, toggleDebugGP);

            var cameraHorizontalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var cameraHorizontalGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraHorizontalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraHorizontalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraH = new(cameraHorizontalKB, cameraHorizontalGP, cameraHorizontalGP2, cameraHorizontalMW);
            
            var cameraVerticalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var cameraVerticalGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraVerticalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraVerticalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraV = new(cameraVerticalKB, cameraVerticalGP, cameraVerticalGP2, cameraVerticalMW);

            inputActions = new()
            {
                iaPlaceWall, iaCancelWall,
                iaSpawnRock, iaSpawnBall, iaSpawnBird, iaSpawnBullet,
                iaToggleDebug,
                iaMoveCameraH, iaMoveCameraV
            };
            
            boundaryRect = new(new(0f), new(5000,5000), new(0.5f));
            gameObjectHandler = new(boundaryRect, 50, 50);
            SetupBoundary();
        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return gameObjectHandler;
        }

        public override void Activate(IScene oldScene)
        {
            // CameraTweenZoomFactor zoomFactorStart = new(1f, 0.75f, 0.25f, TweenType.LINEAR);
            // CameraTweenZoomFactor zoomFactorHold = new(0.75f, 0.75f, 0.5f, TweenType.LINEAR);
            // CameraTweenZoomFactor zoomFactorEnd = new(0.75f, 1f, 0.25f, TweenType.LINEAR);
            // CameraTweenOffset tweenRight = new(new(0), new(100, 0), 0.25f, TweenType.LINEAR);
            // CameraTweenOffset tweenLeft = new(new(100, 0), new(-25, 0), 0.25f, TweenType.LINEAR);
            // CameraTweenOffset tweenEnd = new(new(-25, 0), new(-0, 0), 0.25f, TweenType.LINEAR);
            // GAMELOOP.Camera.StartTweenSequence(zoomFactorStart, zoomFactorHold, zoomFactorEnd);
            // GAMELOOP.Camera.StartTweenSequence(tweenRight, tweenLeft, tweenEnd);
        }

        
        
        public override void Reset()
        {
            gameObjectHandler.Clear();
            SetupBoundary();
            drawDebug = false;
        }

        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var gamepad = GAMELOOP.CurGamepad;
            InputAction.UpdateActions(dt, gamepad, inputActions);
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            // foreach (var ia in inputActions)
            // {
            //     ia.Gamepad = gamepadIndex;
            //     ia.Update(dt);
            // }
            
            if (iaSpawnRock.State.Pressed)
            {
                for (int i = 0; i < 50; i++)
                {
                    var spawnPos = mousePosGame + ShapeRandom.RandVec2(0, 200);
                    var r = new Rock(spawnPos);
                    gameObjectHandler.AddAreaObject(r);
                }
            
            }
            //
            // if (iaSpawnBox.State.Pressed)
            // {
            //     for (int i = 0; i < 5; i++)
            //     {
            //         Box b = new(mousePosGame + ShapeRandom.RandVec2(0, 10), ShapeRandom.RandVec2() * 75, 25);
            //         gameObjectHandler.AddAreaObject(b);
            //     }
            //
            // }
            if (iaSpawnBird.State.Pressed)
            {
                Bird b = new(mousePosGame);
                gameObjectHandler.AddAreaObject(b);
            
            }
            if (iaSpawnBall.State.Down)
            {
                for (var i = 0; i < 10; i++)
                {
                    // Ball b = new(mousePosGame + ShapeRandom.RandVec2(0, 5), ShapeRandom.RandVec2() * 300, 10);
                    // gameObjectHandler.AddAreaObject(b);
                    var ball = new Ball(mousePosGame);
                    gameObjectHandler.AddAreaObject(ball);
                }

            }
            if (iaSpawnBullet.State.Pressed)
            {
                for (var i = 0; i < 100; i++)
                {
                    var bullet = new Bullet(mousePosGame);
                    gameObjectHandler.AddAreaObject(bullet);
                }
                
            }
            // if (iaSpawnTrap.State.Pressed)
            // {
            //     Trap t = new(mousePosGame, new Vector2(250, 250));
            //     gameObjectHandler.AddAreaObject(t);
            // }

            // if (iaSpawnAura.State.Pressed)
            // {
            //     Aura a = new(mousePosGame, 150, 0.75f);
            //     gameObjectHandler.AddAreaObject(a);
            // }

            if (iaToggleDebug.State.Pressed) { drawDebug = !drawDebug; }


            var moveCameraH = iaMoveCameraH.State.AxisRaw;
            var moveCameraV = iaMoveCameraV.State.AxisRaw;
            var moveCameraDir = new Vector2(moveCameraH, moveCameraV);
            var cam = GAMELOOP.Camera;
            var f = cam.ZoomFactor;
            cam.Position += moveCameraDir * 500 * dt * f;
            
            HandleWalls(mousePosGame);
        }

        protected override void UpdateExample(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            gameObjectHandler.Update(time, game, ui);
        }

        protected override void DrawGameExample(ScreenInfo game)
        {
            // if (drawDebug) return;
            
            if (drawDebug)
            {
                var boundsColor = Colors.Light;
                var gridColor = Colors.Light;
                var fillColor = Colors.Medium.ChangeAlpha(100);
                gameObjectHandler.DrawDebug(boundsColor, gridColor, fillColor);
            }

            DrawWalls(game.MousePos);

            gameObjectHandler.DrawGame(game);
            
            // GAMELOOP.Camera.Area.DrawLines(12f, RED);
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            gameObjectHandler.DrawGameUI(ui);
        }

        protected override void DrawUIExample(ScreenInfo ui)
        {
            // Vector2 uiSize = ui.Area.Size;
            // Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            // string infoText =
            //     $"[LMB] Add Segment | [RMB] Cancel Segment | [Space] Shoot | Objs: {gameObjectHandler.GetCollisionHandler().Count}";
            // font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            
            var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
            DrawInputText(bottomCenter);
            
            var bottomRight = GAMELOOP.UIRects.GetRect("bottom right");
            var rects = bottomRight.SplitV(0.5f);
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone("Object Count", rects.top, new(0.5f, 0f));
            textFont.DrawTextWrapNone($"{gameObjectHandler.GetCollisionHandler().Count}", rects.bottom, new(0.5f));
            // font.DrawText("Object Count", rects.top, 1f, new Vector2(0.5f, 0f), ColorHighlight3);
            // font.DrawText(, rects.bottom, 1f, new Vector2(0.5f, 0.5f), ColorHighlight3);
        }

        private void DrawInputText(Rect rect)
        {
            var top = rect.ApplyMargins(0, 0, 0, 0.5f);
            var bottom = rect.ApplyMargins(0, 0, 0.5f, 0f);
            
            var sb = new StringBuilder();
            var sbCamera = new StringBuilder();
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            
            string placeWallText = iaPlaceWall.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
            string cancelWallText = iaCancelWall.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
            string spawnRockText = iaSpawnRock.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            // string spawnBoxText = iaSpawnBox.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnBirdText = iaSpawnBird.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnBallText = iaSpawnBall.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnBulletText = iaSpawnBullet.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            // string spawnAuraText = iaSpawnAura.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            //string spawnTrapText = iaSpawnTrap.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string toggleDebugText = iaToggleDebug.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            
            string moveCameraH = iaMoveCameraH.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string moveCameraV = iaMoveCameraV.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string zoomCamera = GAMELOOP.InputActionZoom.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            sbCamera.Append($"Zoom Camera {zoomCamera} | ");
            sbCamera.Append($"Move Camera {moveCameraH} {moveCameraV}");
            
            sb.Append($"Add/Cancel Wall [{placeWallText}/{cancelWallText}] | ");
            //sb.Append($"Spawn: Rock/Box/Ball/Aura [{spawnRockText}/{spawnBoxText}/{spawnBallText}/{spawnAuraText}] | ");
            sb.Append($"Spawn: ");
            sb.Append($"Rock {spawnRockText} - ");
            sb.Append($"Bird {spawnBirdText} - ");
            // sb.Append($"Box {spawnBoxText} - ");
            sb.Append($"Ball {spawnBallText} - ");
            sb.Append($"Bullet {spawnBulletText} | ");
            // sb.Append($"Aura {spawnAuraText} | ");
            if(drawDebug) sb.Append($"Normal Mode {toggleDebugText}");
            else sb.Append($"Debug Mode {toggleDebugText}");
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(sbCamera.ToString(), top, new(0.5f));
            textFont.DrawTextWrapNone(sb.ToString(), bottom, new(0.5f));
            // font.DrawText(sbCamera.ToString(), top, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            // font.DrawText(sb.ToString(), bottom, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
        private void SetupBoundary()
        {
            BoundaryWall top = new(boundaryRect.TopLeft, boundaryRect.TopRight);
            BoundaryWall bottom = new(boundaryRect.BottomRight, boundaryRect.BottomLeft);
            BoundaryWall left = new(boundaryRect.TopLeft, boundaryRect.BottomLeft);
            BoundaryWall right = new(boundaryRect.BottomRight, boundaryRect.TopRight);
            gameObjectHandler.AddAreaObjects(top, right, bottom, left);
        }
        private void DrawWalls(Vector2 mousePos)
        {
            if (segmentStarted)
            {
                ShapeDrawing.DrawCircle(startPoint, 15f, Colors.Highlight);
                Segment s = new(startPoint, mousePos);
                s.Draw(4, Colors.Highlight);

            }
        }
        private void HandleWalls(Vector2 mousePos)
        {
            if (iaPlaceWall.State.Pressed)
            {
                if (segmentStarted)
                {
                    segmentStarted = false;
                    float lSq = (mousePos - startPoint).LengthSquared();
                    if (lSq > 400)
                    {
                        PolyWall w = new(startPoint, mousePos);
                        gameObjectHandler.AddAreaObject(w);
                    }

                }
                else
                {
                    startPoint = mousePos;
                    segmentStarted = true;
                }
            }
            else if (iaCancelWall.State.Pressed)
            {
                if (segmentStarted)
                {
                    segmentStarted = false;
                }
            }


        }

    }
}
