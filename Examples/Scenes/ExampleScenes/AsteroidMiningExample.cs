using ShapeEngine.Core;
using ShapeEngine.Lib;
using System.Numerics;
using System.Text;
using ShapeEngine.Color;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using Color = System.Drawing.Color;
using ShapeEngine.Random;
namespace Examples.Scenes.ExampleScenes
{
    // public abstract class SpaceObject : GameObject
    // {
    //     // protected bool dead = false;
    //     // public int Layer { get; set; } = 0;
    //     // public bool Kill()
    //     // {
    //         // if (dead) return false;
    //         // dead = true;
    //         // return true;
    //     // }
    //
    //     // public abstract Vector2 GetPosition();
    //     // public abstract Rect GetBoundingBox();
    //
    //     public virtual void Update(GameTime time, ScreenInfo game, ScreenInfo ui) { }
    //     public virtual void DrawGame(ScreenInfo game) { }
    //     public virtual void DrawGameUI(ScreenInfo ui) { }
    //     // public virtual void Overlap(CollisionInformation info) { }
    //     // public virtual void OverlapEnded(ICollidable other) { }
    //     public virtual void AddedToHandler(GameObjectHandler gameObjectHandler) { }
    //     public virtual void RemovedFromHandler(GameObjectHandler gameObjectHandler) { }
    //     
    //     
    //     public bool IsDead() { return dead; }
    //     public bool DrawToGame(Rect gameArea) { return true; }
    //     public bool DrawToGameUI(Rect uiArea) { return false; }
    //     public bool CheckHandlerBounds() { return false; }
    //     public void LeftHandlerBounds(BoundsCollisionInfo info) { }
    //
    //     public virtual bool HasCollisionBody() => false;
    //
    //     public virtual CollisionBody? GetCollisionBody() => null;
    // }
    public class AsteroidShard : GameObject
    {
        // private Polygon shape;
        private Triangle shape;
        private Vector2 pos;
        private Vector2 vel;
        private float rotDeg = 0f;
        private float angularVelDeg = 0f;
        private float lifetimeTimer = 0f;
        private float lifetime = 0f;
        private float lifetimeF = 1f;

        private float delay = 1f;

        private PaletteColor color;
        public AsteroidShard(Triangle shape, Vector2 fractureCenter, PaletteColor color)
        {
            this.shape = shape;
            this.rotDeg = 0f;
            this.pos = shape.GetCentroid();
            var dir = (pos - fractureCenter).Normalize();
            this.vel = dir * Rng.Instance.RandF(100, 300);
            this.angularVelDeg = Rng.Instance.RandF(-90, 90);
            this.lifetime = Rng.Instance.RandF(1.5f, 3f);
            this.lifetimeTimer = this.lifetime;
            this.delay = 0.5f;
            this.color = color;
        }
        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            if(lifetimeTimer > 0f)
            {
                lifetimeTimer -= time.Delta;
                if(lifetimeTimer <= 0f)
                {
                    lifetimeTimer = 0f;
                    Kill();
                    // dead = true;
                }
                else
                {
                    lifetimeF = lifetimeTimer / lifetime;

                    if (lifetime - lifetimeTimer > delay)
                    {
                        
                        float prevRotDeg = rotDeg;
                        pos += vel * time.Delta;
                        rotDeg += angularVelDeg * time.Delta;

                        float rotDifDeg = rotDeg - prevRotDeg;
                        
                        shape = shape.SetPosition(pos).ChangeRotation(rotDifDeg * ShapeMath.DEGTORAD);
                        // shape = shape.Rotate(shape.GetCentroid(), rotDifDeg * ShapeMath.DEGTORAD);
                    }
                    
                }
            }
        }

        public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            
        }

        public override void DrawGame(ScreenInfo game)
        {
            //SDrawing.DrawCircleFast(pos, 4f, RED);
            var c = this.color.ColorRgba.ChangeAlpha((byte)(255 * lifetimeF));
            //color = this.color;
            shape.DrawLines(2f * lifetimeF, c);
        }

        public override void DrawGameUI(ScreenInfo gameUi)
        {
            
        }

        public override Rect GetBoundingBox() { return shape.GetBoundingBox(); }
    }
    public class Asteroid : CollisionObject
    {
        internal class DamagedSegment
        {

            public Segment Segment;
            private float timer;
            private const float Lifetime = 1f;
            public DamagedSegment(Segment segment)
            {
                this.Segment = segment;
                this.timer = Lifetime;
            }
            public bool IsFinished() { return timer <= 0f; }
            public void Update(float dt)
            {
                if (timer > 0f)
                {
                    timer -= dt;
                    if (timer <= 0f) timer = 0f;
                }
            }
            public void Draw()
            {
                float f = timer / Lifetime;
                //Color color = YELLOW.ChangeAlpha((byte)(255 * f));
                Segment.Draw(Rng.Instance.RandF(4, 8) * f, Colors.Special2, LineCapType.CappedExtended, 3);
            }
            public void Renew() { timer = Lifetime; }
        }
        internal class DamagedSegments : List<DamagedSegment>
        {
            public void AddSegment(Segment segment)
            {
                foreach (var seg in this)
                {
                    if (seg.Segment.Equals(segment))
                    {
                        seg.Renew();
                        return;
                    }
                }

                Add(new(segment));
            }
            public bool ContainsSegment(Segment segment)
            {
                foreach (var seg in this)
                {
                    if (seg.Segment.Equals(segment)) return true;
                }
                return false;
            }

            public void Update(float dt)
            {
                if (Count > 0)
                {
                    for (int i = Count - 1; i >= 0; i--)
                    {
                        var seg = this[i];
                        seg.Update(dt);
                        if (seg.IsFinished()) this.RemoveAt(i);
                    }
                }
            }
            public void Draw()
            {
                foreach (var seg in this)
                {
                    seg.Draw();
                }
            }
        }
        
        private const float DamageThreshold = 50f;

        private bool overlapped = false;
        private float curThreshold = DamageThreshold;

        public event Action<Asteroid, Vector2>? Fractured;
        private PaletteColor curColor = Colors.PcWarm;

        private DamagedSegments damagedSegments = new();
        
        private readonly AsteroidCollider collider;

        // public Asteroid(Vector2 pos, params Vector2[] shape)
        // {
        //     collider = new AsteroidCollider(new Polygon(shape), new());
        //     AddCollider(collider);
        //     
        //     SetDamageTreshold(0f);
        // }
        public Asteroid(Polygon relativeShape, Vector2 pos, float size)
        {
            
            Transform = new(pos, 0f, new Size(size, 0f), 1f);
            // var pos = shape.GetCentroid();
            collider = new AsteroidCollider(new(), relativeShape);
            AddCollider(collider);
            
            SetDamageTreshold(0f);
            
        }
        public Asteroid(Polygon relativeShape, Transform2D transform)
        {

            Transform = transform;
            collider = new AsteroidCollider(new(), relativeShape);
            AddCollider(collider);
            
            SetDamageTreshold(0f);
            
        }

        public Polygon GetPolygon() { return collider.GetPolygonShape(); }

        private void SetDamageTreshold(float overshoot = 0f)
        {
            curThreshold = DamageThreshold * Rng.Instance.RandF(0.5f, 2f) + overshoot;
        }
        public void Overlapped()
        {
            overlapped = true;
        }
        public PaletteColor GetColor()
        {
            return curColor;
        }
        public void Damage(float amount, Vector2 point)
        {
            //find segments close to point
            //fade the color from impact color to cur color over several segments
            if (amount <= 0) return;


            var shape = GetPolygon();
            var seg = shape.GetClosestSegment(point);
            damagedSegments.AddSegment(seg.Segment);

            curThreshold -= amount;
            if(curThreshold <= 0f)
            {
                SetDamageTreshold(MathF.Abs(curThreshold));
                Fractured?.Invoke(this, point);
                
                //cut piece
                //var cutShape = SPoly.Generate(point, SRNG.randI(6, 12), 50, 250);
                
            }
        }

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui) 
        {
            damagedSegments.Update(time.Delta);
        }
        public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            
        }
        public override void DrawGame(ScreenInfo game)
        {
            var p = GetPolygon();
            if (overlapped)
            {
                curColor = Colors.PcHighlight;
                p.DrawLines(6f, curColor.ColorRgba);
            }
            else
            {
                curColor = Colors.PcWarm;
                p.DrawLines(3f, curColor.ColorRgba);
            }
            damagedSegments.Draw();

            overlapped = false;
        }

        public override void DrawGameUI(ScreenInfo gameUi)
        {
        }

        public override Rect GetBoundingBox() { return collider.GetBoundingBox(); }
    }

    public class AsteroidCollider : PolyCollider
    {
        public AsteroidCollider(Transform2D offset, Polygon relativeShape) : base(offset, relativeShape)
        {
            ComputeCollision = false;
            ComputeIntersections = false;
            CollisionLayer = AsteroidMiningExample.AsteriodLayer;
            CollisionMask = BitFlag.Empty;
        }
        
        // public AsteroidCollider(Polygon absoluteShape, Vector2 offset) : base(absoluteShape, offset)
        // {
        //     ComputeCollision = false;
        //     ComputeIntersections = false;
        //     CollisionLayer = AsteroidMiningExample.AsteriodLayer;
        //     CollisionMask = BitFlag.Empty;
        // }
        // public AsteroidCollider(List<Vector2> relativePoints, Vector2 offset) : base(relativePoints, offset)
        // {
        //     ComputeCollision = false;
        //     ComputeIntersections = false;
        //     CollisionLayer = AsteroidMiningExample.AsteriodLayer;
        //     CollisionMask = BitFlag.Empty;
        // }
    }
    
    public class LaserDevice : GameObject
    {
        private const float LaserRange = 1200;
        private const float DamagePerSecond = 50;
        private bool aimingMode = true;
        private bool hybernate = false;
        private bool laserEnabled = false;

        private Vector2 pos;
        private float rotRad;
        private float size;
        private Triangle shape;

        private Vector2 tip;
        private Points laserPoints = new();
        //private Vector2 laserEndPoint;
        private Vector2 aimDir = new();
        // private SpawnAreaCollision spawnArea;
        private CollisionHandler? collisionHandler;
        public InputAction iaShootLaser;
        public LaserDevice(Vector2 pos, float size, CollisionHandler? collisionHandler) 
        {
            this.collisionHandler = collisionHandler;
            this.pos = pos;
            this.size = size;
            this.rotRad = 0f;
            UpdateTriangle();

            var shootKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var shootGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            var shootMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaShootLaser = new(shootKB, shootGP, shootMB);
            //this.laserEndPoint = tip;
        }
        public void SetHybernate(bool enabled)
        {
            if (enabled)
            {
                aimingMode = true;
                hybernate = true;
            }
            else
            {
                aimingMode = true;
                hybernate = false;
            }
            
        }
        public void SetAimingMode(bool enabled)
        {
            aimingMode = enabled;

        }
        
        private void UpdateTriangle()
        {
            Vector2 a = pos + new Vector2(size / 2, 0f).Rotate(rotRad);
            Vector2 b = pos + new Vector2(-size / 2, -size / 4).Rotate(rotRad);
            Vector2 c = pos + new Vector2(-size / 2, size / 4).Rotate(rotRad);

            this.shape = new Triangle(a, b, c);
            this.tip = a;
        }

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            laserPoints.Clear();
            laserEnabled = false;
            if (hybernate) return;
            
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            iaShootLaser.Gamepad = GAMELOOP.CurGamepad;
            iaShootLaser.Update(time.Delta);
            
            if (aimingMode)
            {
                Vector2 dir = game.MousePos - pos;
                aimDir = dir.Normalize();
                rotRad = dir.AngleRad();

                laserEnabled = iaShootLaser.State.Down; // IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT);
            }
            else
            {
                pos = game.MousePos;
            }
            
            UpdateTriangle();

            if (laserEnabled && collisionHandler != null)
            {
                laserPoints.Add(tip);
                    
                float remainingLength = LaserRange;
                Vector2 curLaserPos = tip;
                Vector2 curLaserDir = aimDir;
                float curDamagePerSecond = DamagePerSecond;
                while(remainingLength > 0) //reflecting laser
                {
                    var result = CastLaser(time.Delta, curLaserPos, curLaserDir, remainingLength, curDamagePerSecond, collisionHandler);
                    laserPoints.Add(result.endPoint);
                    curLaserPos = result.endPoint;
                    remainingLength = result.remainingLength;
                    curLaserDir = result.newDir;
                    curDamagePerSecond *= 0.5f;
                    remainingLength *= 0.5f;
                    remainingLength = 0f; //reflecting turned off
                }

            }
        }
        public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            
        }
        private (Vector2 endPoint, float remainingLength, Vector2 newDir) CastLaser(float dt, Vector2 start, Vector2 dir, float length, float damagePerSecond, CollisionHandler col)
        {
            var endPoint = start + dir * length;
            var newEndPoint = endPoint;
            var newDir = dir;

            BitFlag bf = new(AsteroidMiningExample.AsteriodLayer);
            var queryInfos = col.QuerySpace(new Segment(start, endPoint), start, bf, true);
            if (queryInfos is { Count: > 0 })
            {
                var closest = queryInfos[0];
                if (closest.Points.Valid)
                {
                    var other = closest.Collider;
                    if (other is AsteroidCollider a)
                    {
                        //perfect naming:)
                        if (a.Parent is Asteroid body)
                        {
                            newDir = dir.Reflect(closest.Points.Closest.Normal);
                            newEndPoint = closest.Points.Closest.Point;  //closest.intersection.ColPoints[0].Point;
                            body.Damage(damagePerSecond * dt, newEndPoint);
                        }
                        
                    }
                }
            }

            float usedLength = (newEndPoint - start).Length();
            //if (usedLength < 10) return (newEndPoint, 0, dir);

            float remainingLength = length - usedLength;
            if (remainingLength <= 1) return new(newEndPoint, 0f, dir);
            return (newEndPoint - dir * 10f, remainingLength, newDir);
            //return (newEndPoint, remainingLength, newDir);
        }

        public override void DrawGame(ScreenInfo game)
        {
            if (hybernate) return;
            var c = Colors.Cold;
            shape.DrawLines(4f, c);
            ShapeDrawing.DrawCircle(tip, 8f, c);

            if (laserEnabled && laserPoints.Count > 1)
            {
                for (int i = 0; i < laserPoints.Count - 1; i++)
                {
                    Segment laserSegment = new(laserPoints[i], laserPoints[i + 1]);
                    laserSegment.Draw(4f, c);
                    ShapeDrawing.DrawCircle(laserPoints[i + 1], Rng.Instance.RandF(6f, 12f), c, 12);
                }
                
            }


        }

        public override void DrawGameUI(ScreenInfo gameUi)
        {
        }


        public override Rect GetBoundingBox() { return shape.GetBoundingBox(); }
    }
    
    public class AsteroidMiningExample : ExampleScene
    {
        internal class Cutout
        {

            private Polygon shape;
            private float timer;
            private const float Lifetime = 0.5f;
            public Cutout(Polygon shape)
            {
                this.shape = shape;
                this.timer = Lifetime;
            }
            public bool IsFinished() { return timer <= 0f; }
            public void Update(float dt)
            {
                if (timer > 0f)
                {
                    timer -= dt;
                    if (timer <= 0f) timer = 0f;
                }
            }
            public void Draw()
            {
                float f = timer / Lifetime;
                //Color color = YELLOW.ChangeAlpha((byte)(255 * f));
                shape.DrawLines(6f * f, Colors.Special2);
            }
        }

        public static uint AsteriodLayer = 1;
        private const float MinPieceArea = 3000f;

        internal enum ShapeType { None = 0, Triangle = 1, Rect = 2, Poly = 3};

        // private Font font;
        // private SpawnAreaCollision spawnArea;
        private Rect boundaryRect = new();

        private bool polyModeActive = false;
        private ShapeType curShapeType = ShapeType.None;

        private Polygon curShape = new();
        private List<Cutout> lastCutOuts = new();
        private Vector2 curPos = new();
        private float curRot = 0f;
        private float curSize = 50;

        private LaserDevice laserDevice;

        private FractureHelper fractureHelper = new(250, 1500, 0.75f, 0.1f);

        //private float crossResult = 0f;

        //Polygons testShapes = new();
        //Rect clipRect = new();
        //RectD clipperRect = new();

        private readonly InputAction iaModeChange;
        private readonly InputAction iaAddShape;
        private readonly InputAction iaCutShape;
        private readonly InputAction iaRegenerateShape;
        private readonly InputAction iaRotateShape;
        private readonly InputAction iaScaleShape;
        private readonly InputAction iaPickTriangleShape;
        private readonly InputAction iaPickRectangleShape;
        private readonly InputAction iaPickPolygonShape;
        private readonly InputAction iaDragLaser;
        private readonly List<InputAction> inputActions;
        
        public AsteroidMiningExample()
        {
            Title = "Asteroid Mining Example";
            // font = GAMELOOP.GetFont(FontIDs.JetBrains);
            UpdateBoundaryRect(GAMELOOP.GameScreenInfo.Area);
            // spawnArea = new SpawnAreaCollision(boundaryRect, 4, 4);
            InitSpawnArea(boundaryRect);
            InitCollisionHandler(boundaryRect, 4, 4);
            
            // if (InitSpawnArea(boundaryRect)) SpawnArea?.InitCollisionHandler(4, 4);

            laserDevice = new(new Vector2(0f), 100, CollisionHandler);
            SpawnArea?.AddGameObject(laserDevice);

            var modeChangeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
            var modeChangeGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            iaModeChange = new(modeChangeKB, modeChangeGP);

            var addShapeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var addShapeMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            var addShapeGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            iaAddShape = new(addShapeKB, addShapeMB, addShapeGP);

            var cutShapeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var cutShapeMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            var cutShapeGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            iaCutShape = new(cutShapeKB, cutShapeMB, cutShapeGP);
            
            var regenShapeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var regenShapeGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP);
            iaRegenerateShape = new(regenShapeKB, regenShapeGP);
            
            var rotateShapeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
            var rotateShapeGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            iaRotateShape = new(rotateShapeKB, rotateShapeGP);
            
            var scaleShapeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.X);
            var scaleShapeGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
            iaScaleShape = new(scaleShapeKB, scaleShapeGP);
            
            var pickTriangleKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ONE);
            var pickTriangleGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT);
            iaPickTriangleShape = new(pickTriangleKB, pickTriangleGP);
            
            var pickRectangleKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TWO);
            var pickRectangleGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT);
            iaPickRectangleShape = new(pickRectangleKB, pickRectangleGP);
            
            var pickPolygonKB = new InputTypeKeyboardButton(ShapeKeyboardButton.THREE);
            var pickPolygonGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN);
            iaPickPolygonShape = new(pickPolygonKB, pickPolygonGP);

            var dragLaserKB = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
            var dragLaserGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_BOTTOM);
            var dragLaserMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            iaDragLaser = new(dragLaserMB, dragLaserKB, dragLaserGP);
            
            inputActions = new()
            {
                iaModeChange,
                iaAddShape,
                iaCutShape,
                iaRegenerateShape,
                iaRotateShape,
                iaScaleShape,
                iaPickTriangleShape,
                iaPickRectangleShape,
                iaPickPolygonShape,
                iaDragLaser
            };
            
            textFont.FontSpacing = 1f;
            
        }
        public override void Reset()
        {
            SpawnArea?.Clear();
            CollisionHandler?.Clear();
            polyModeActive = false;
            curRot = 0f;
            curSize = 50f;
            curShapeType = ShapeType.Triangle;
            RegenerateShape();
            laserDevice = new(new Vector2(0), 100, CollisionHandler);
            SpawnArea?.AddGameObject(laserDevice);
        }

        private void UpdateBoundaryRect(Rect gameArea)
        {
            //boundaryRect = new Rect(new Vector2(0f), game.GetSize(), new Vector2(0.5f)).ApplyMargins(0.005f, 0.005f, 0.1f, 0.005f);
            boundaryRect = gameArea.ApplyMargins(0.005f, 0.005f, 0.1f, 0.005f);
        }
        
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
        {
            UpdateBoundaryRect(game.Area);
            SpawnArea?.ResizeBounds(boundaryRect);
            CollisionHandler?.ResizeBounds(boundaryRect);
            // spawnArea.Update(time, game, ui);

            for (int i = lastCutOuts.Count - 1; i >= 0; i--)
            {
                var c = lastCutOuts[i];
                c.Update(time.Delta);
                if (c.IsFinished()) lastCutOuts.RemoveAt(i);
            }
        }
        private void OnAsteroidFractured(Asteroid a, Vector2 point)
        {
            var cutShape = Polygon.Generate(point, Rng.Instance.RandI(6, 12), 35, 100);
            
            FractureAsteroid(a, cutShape);
        }
        private void FractureAsteroid(Asteroid a, Polygon cutShape)
        {
            RemoveAsteroid(a);
            var asteroidShape = a.GetPolygon();
            var color = a.GetColor();
            var fracture = fractureHelper.Fracture(asteroidShape, cutShape);
            
            foreach (var cutoutShape in fracture.Cutouts)
            {
                lastCutOuts.Add(new Cutout(cutoutShape));
            }

            var center = cutShape.GetCentroid();
            foreach (var piece in fracture.Pieces)
            {
                // Vector2 center = piece.GetCentroid();
                AsteroidShard shard = new(piece, center, color);
                SpawnArea?.AddGameObject(shard);
            }
            if (fracture.NewShapes.Count > 0)
            {
                foreach (var shape in fracture.NewShapes)
                {
                    float shapeArea = shape.GetArea();
                    if (shapeArea > MinPieceArea)
                    {

                        var relativeInfo = shape.ToRelative();
                        
                        Asteroid newAsteroid = new(relativeInfo.shape, relativeInfo.transform);
                        AddAsteroid(newAsteroid);
                    }
                }
            }
        }
        private void AddAsteroid(Asteroid a)
        {
            a.Fractured += OnAsteroidFractured;
            SpawnArea?.AddGameObject(a);
            CollisionHandler?.Add(a);
        }
        private void RemoveAsteroid(Asteroid a)
        {
            a.Fractured -= OnAsteroidFractured;
            SpawnArea?.RemoveGameObject(a);
            CollisionHandler?.Remove(a);
        }
        private void SetCurPos(Vector2 pos)
        {
            curPos = pos;
        }
        private void CycleRotation()
        {
            float step = 45f;
            curRot += step;
            curRot = ShapeMath.WrapAngleDeg(curRot);// ShapeMath.WrapF(curRot, 0f, 360f);
        }
        private void CycleSize()
        {
            const float step = 100f;
            const float min = 100f;
            const int max = 800;
            curSize += step;
            curSize = ShapeMath.WrapF(curSize, min, max);
        }
        private void RegenerateShape()
        {
            if (curShapeType == ShapeType.Triangle)
            {
                GenerateTriangle();
            }
            else if (curShapeType == ShapeType.Rect)
            {
                GenerateRect();
            }
            else if (curShapeType == ShapeType.Poly)
            {
                GeneratePoly();
            }
        }
        private void GenerateTriangle()
        {
            curShape = Polygon.Generate(curPos, 3, curSize / 2, curSize);
        }
        private void GenerateRect()
        {
            Rect r = new(curPos, new Size(curSize), new AnchorPoint(0.5f));
            curShape = r.Rotate(curRot, new(0.5f));// r.RotateList(new Vector2(0.5f), curRot);
        }
        private void GeneratePoly()
        {
            curShape = Polygon.Generate(curPos, 16, curSize * 0.25f, curSize);
        }
        private void PolyModeStarted()
        {
            if (curShapeType == ShapeType.None)
            {
                curShapeType = ShapeType.Triangle;
                RegenerateShape();
            }
            laserDevice.SetHybernate(true);
        }
        private void PolyModeEnded()
        {
            laserDevice.SetHybernate(false);
        }
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
        {
            //clipRect = new(mousePosGame, new Vector2(100, 300), new Vector2(0f, 1f));
            //clipperRect = clipRect.ToClipperRect();
            //if (IsKeyPressed(KeyboardKey.KEY_NINE))
            //{
            //    Polygons newShapes = new Polygons();
            //    foreach (var shape in testShapes)
            //    {
            //        if (shape.OverlapShape(clipRect))
            //        {
            //            var result = SClipper.ClipRect(clipRect, shape, 2, false).ToPolygons(true);
            //            if (result.Count > 0) newShapes.AddRange(result);
            //        }
            //        else newShapes.Add(shape);
            //    }
            //    testShapes = newShapes;
            //}

            if (CollisionHandler == null) return;

            var gamepad = GAMELOOP.CurGamepad;
            InputAction.UpdateActions(dt, gamepad, inputActions);
            
            
            if (iaModeChange.State.Pressed)
            {
                polyModeActive = !polyModeActive;
                if(polyModeActive) PolyModeStarted();
                else PolyModeEnded();
            }
            
            if (polyModeActive)
            {
                SetCurPos(mousePosGame);
                curShape.SetPosition(curPos);
                BitFlag mask = new(AsteriodLayer);
                var candidates = new List<Collider>();
                CollisionHandler.CastSpace(curShape, mask, ref candidates);
                foreach (var candidate in candidates)
                {
                    if (candidate is AsteroidCollider asteroid)
                    {
                        if (asteroid.Parent is Asteroid body)
                        {
                            body.Overlapped();
                        }
                        
                        // asteroid.Overlapped();
                    }
                }

                if (iaAddShape.State.Pressed) //add polygon (merge)
                {
                    Polygons polys = new();

                    if (candidates.Count > 0)
                    {
                        foreach (var candidate in candidates)
                        {
                            if (candidate is AsteroidCollider asteroid)
                            {
                                //area.RemoveAreaObject(asteroid);
                                if (asteroid.Parent is Asteroid body)
                                {
                                    RemoveAsteroid(body);
                                    polys.Add(asteroid.GetPolygonShape());
                                }
                                
                            }
                        }
                        var finalShapes = ShapeClipper.UnionMany(curShape.ToPolygon(), polys, Clipper2Lib.FillRule.NonZero).ToPolygons(true);
                        if (finalShapes.Count > 0)
                        {
                            foreach (var f in finalShapes)
                            {
                                var rel = f.ToRelative();
                                Asteroid a = new(rel.shape, rel.transform);
                                AddAsteroid(a);
                                //area.AddAreaObject(a);
                            }
                        }
                        else
                        {
                            // Asteroid a = new(curShape.ToPolygon());
                            var rel = curShape.ToRelative();
                            Asteroid a = new(rel.shape, rel.transform);
                            AddAsteroid(a);
                            //area.AddAreaObject(a);
                        }
                    }
                    else
                    {
                        // Asteroid a = new(curShape.ToPolygon());
                        var rel = curShape.ToRelative();
                        Asteroid a = new(rel.shape, rel.transform);
                        AddAsteroid(a);
                        //area.AddAreaObject(a);
                    }

                }
                if (iaCutShape.State.Pressed) //cut polygon
                {
                    var cutShape = curShape.ToPolygon();
                    Polygons allCutOuts = new();
                    foreach (var candidate in candidates)
                    {
                        if (candidate is AsteroidCollider asteroid)
                        {
                            if (asteroid.Parent is Asteroid body)
                            {
                                FractureAsteroid(body, cutShape);
                            }
                            

                            /*
                            //area.RemoveAreaObject(asteroid);
                            RemoveAsteroid(asteroid);
                            var asteroidShape = asteroid.GetPolygon();

                            var fracture = fractureHelper.Fracture(asteroidShape, cutShape);

                            if (fracture.Cutouts.Count > 0) allCutOuts.AddRange(fracture.Cutouts);

                            foreach (var piece in fracture.Pieces)
                            {
                                float pieceArea = piece.GetArea();
                                //if (pieceArea < MinPieceArea) continue;

                                Vector2 center = piece.GetCentroid();
                                AsteroidShard shard = new(piece.ToPolygon(), center);
                                area.AddAreaObject(shard);
                            }
                            if(fracture.NewShapes.Count > 0)
                            {
                                foreach (var shape in fracture.NewShapes)
                                {
                                    float shapeArea = shape.GetArea();
                                    if(shapeArea > MinPieceArea)
                                    {
                                        Asteroid a = new(shape);
                                        AddAsteroid(a);
                                        //area.AddAreaObject(a);
                                    }
                                }
                            }
                            */
                        }
                    }
                    if (allCutOuts.Count > 0)
                    {
                        foreach (var cutoutShape in allCutOuts)
                        {
                            lastCutOuts.Add(new Cutout(cutoutShape));
                        }
                    }
                }

                if (iaRegenerateShape.State.Pressed)//regenerate
                {
                    RegenerateShape();
                }

                if (iaRotateShape.State.Pressed)//rotate
                {
                    float oldRot = curRot;
                    CycleRotation();
                    
                    float dif = curRot - oldRot;
                    curShape.ChangeRotation(dif * ShapeMath.DEGTORAD, new Vector2(0.5f));
                    curShape.SetPosition(curPos);
                }

                if (iaScaleShape.State.Pressed)//scale
                {
                    float oldSize = curSize;
                    CycleSize();
                    float scale = curSize / oldSize;
                    curShape.ScaleSize(scale);
                    curShape.SetPosition(curPos);
                }

                
                if (iaPickTriangleShape.State.Pressed)//choose triangle
                {
                    if(curShapeType != ShapeType.Triangle)
                    {
                        GenerateTriangle();
                        curShapeType = ShapeType.Triangle;
                    }
                    
                }
                if (iaPickRectangleShape.State.Pressed)//choose rectangle
                {
                    if(curShapeType != ShapeType.Rect)
                    {
                        GenerateRect();
                        curShapeType = ShapeType.Rect;
                    }
                    
                }
                if (iaPickPolygonShape.State.Pressed)//choose polygon 
                {
                    if (curShapeType != ShapeType.Poly)
                    {
                        GeneratePoly();
                        curShapeType = ShapeType.Poly;
                    }
                    
                }
            }
            else
            {
                if(iaDragLaser.State.Pressed)
                {
                    laserDevice.SetAimingMode(false);
                }
                else if (iaDragLaser.State.Released)
                {
                    laserDevice.SetAimingMode(true);
                }
            }
        }
        protected override void OnDrawGameExample(ScreenInfo game)
        {
            
            //boundaryRect.DrawLines(4f, ColorLight);
            if(polyModeActive && curShapeType != ShapeType.None)
            {
                curShape.DrawLines(2f, Colors.Warm);
            }

            // spawnArea.DrawGame(game);

            foreach (var cutOut in lastCutOuts)
            {
                cutOut.Draw();
            }
        }
        // protected override void OnDrawGameUIExample(ScreenInfo ui)
        // {
        //     // spawnArea.DrawGameUI(ui);
        // }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            var infoRect = GAMELOOP.UIRects.GetRect("bottom center");
            DrawInputText(infoRect);
            
            // var polymodeText = "[Tab] Polymode | [LMB] Place/Merge | [RMB] Cut | [1] Triangle | [2] Rect | [3] Poly | [Q] Regenerate | [X] Rotate | [C] Scale";
            // var laserText = "[Tab] Lasermode | [LMB] Move | [RMB] Shoot Laser";
            // string text = polyModeActive ? polymodeText : laserText;
            
            // font.DrawText(text, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

        private void DrawInputText(Rect rect)
        {
            var split = rect.SplitV(0.35f);
            var sbBottom = new StringBuilder();
            var sbTop = new StringBuilder();
            
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            string changeModeText = iaModeChange.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string zoomCamera = GAMELOOP.InputActionZoom.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            sbTop.Append($"Mode {changeModeText} | ");
            
            if (polyModeActive)
            {
                
                
                string placeText = iaAddShape.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
                string cutText = iaCutShape.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
                string pickTri = iaPickTriangleShape.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
                string pickRect = iaPickRectangleShape.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
                string pickPoly = iaPickPolygonShape.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
                string regenerateShape = iaRegenerateShape.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
                string rotateShape = iaRotateShape.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
                string scaleShape = iaScaleShape.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
                
                sbTop.Append($"Place {placeText} | ");
                sbTop.Append($"Cut {cutText} | ");
                
                sbBottom.Append($"Triangle {pickTri} | ");
                sbBottom.Append($"Rect {pickRect} | ");
                sbBottom.Append($"Poly {pickPoly} | ");
                
                sbBottom.Append($"Regen {regenerateShape} | ");
                sbBottom.Append($"Rotate {rotateShape} | ");
                sbBottom.Append($"Scale {scaleShape}");
                
                
            }
            else
            {
                string moveLaser = iaDragLaser.GetInputTypeDescription(curInputDeviceAll, true, 1, false);   
                string shootLaser = laserDevice.iaShootLaser.GetInputTypeDescription(curInputDeviceAll, true, 1, false);   
                sbBottom.Append($"Laser: Drag {moveLaser} | ");
                sbBottom.Append($"Shoot {shootLaser}");
                
            }
            
            sbTop.Append($"Zoom {zoomCamera}");
            textFont.ColorRgba = Colors.Special;
            textFont.DrawTextWrapNone(sbTop.ToString(), split.top, new(0.5f));
            
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(sbBottom.ToString(), split.bottom, new(0.5f));
            
        }
    }
}
