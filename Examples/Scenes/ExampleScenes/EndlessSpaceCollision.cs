using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Screen;
using System.Numerics;
using Examples.PayloadSystem;
using Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.StripedDrawingDef;
using ShapeEngine.Input;
using Size = ShapeEngine.Core.Structs.Size;
using ShapeEngine.Random;

// using Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;
namespace Examples.Scenes.ExampleScenes;

public class EndlessSpaceCollision : ExampleScene
{
    private class ScreenTextureHandler : CustomScreenTextureHandler
    {
        private readonly float parallaxFactor;
        private Rect cameraRect = new();
        
        public ScreenTextureHandler(float parallaxFactor)
        {
            this.parallaxFactor = parallaxFactor;
        }
        
        public void SetCameraRect(Rect newCameraRect)
        {
            cameraRect = newCameraRect;
            cameraRect = new Rect
            (
                newCameraRect.X * parallaxFactor,
                newCameraRect.Y * parallaxFactor,
                newCameraRect.Width,
                newCameraRect.Height
            );
        }
        public override Rect GetSourceRect(Dimensions screenDimensions, Dimensions textureDimensions)
        {
            var offset = textureDimensions.ToVector2() / 2f;
            return new Rect
                (
                    cameraRect.Center + offset,
                    cameraRect.Size,
                    new AnchorPoint(0.5f, 0.5f)
                );
        }

        public override (ColorRgba color, bool clear) GetBackgroundClearColor() => (ColorRgba.Clear, true);
    }

    private List<ScreenTexture> starTextures = new(5);
    private List<ScreenTextureHandler> starTextureHandlers = new(5);
    
    public static Vector2 DestroyerPosition = new(0f);
    public static int Difficulty = 1;
    public static readonly int MaxDifficulty = 100;
    public static float DifficultyFactor => (float)Difficulty / (float)MaxDifficulty;
    public const float BigAsteroidScore = 200;
    public const float SmallAsteroidScore = 5;
    public const float DifficultyIncreaseThreshold = BigAsteroidScore * 4 + SmallAsteroidScore * 40;
    public float DifficultyScore = 0f;
    public float CurScore = 0f;
    public int killedBigAsteroids = 0;
    public int kills = 0;
    
    private Rect universe;
    private readonly Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource.Ship ship;
    private readonly ShapeCamera camera;
    private readonly InputAction iaDrawDebug;

    private readonly InputAction iaPayloadCallinUp;
    private readonly InputAction iaPayloadCallinDown;
    private readonly InputAction iaPayloadCallinLeft;
    private readonly InputAction iaPayloadCallinRight;
    private readonly InputAction singleDestructorAction;
    private readonly InputAction multiDestructorAction;
    private readonly InputAction gameoverScreenAcceptAction;
    private readonly InputActionTree inputActionTree;
    
    private bool drawDebug = false;

    private readonly CameraFollowerSingle follower;
    private readonly List<Destructor> destructors = new();
    private const float singleDestructorCooldown = 2f;
    private const float singleDestructorBurstCooldown = 0.1f;
    private const int singleDestructorBurstCount = 5;
    private int singleDestructorBurstsRemaining = 0;
    private const float multiDestructorCooldown = 12f;
    private float singleDestructorCooldownTimer = 0f;
    private float multiDestructorCooldownTimer = 0f;
    private float singleDestructorBurstTimer = 0f;
    private readonly List<AsteroidObstacle> asteroids = new(128);
    private readonly List<EndlessSpaceExampleSource.AsteroidShard> shards = new(512);
    private readonly List<EndlessSpaceExampleSource.Bullet> bullets = new(1024);
    private const int AsteroidCount = 240; //30
    private const int AsteroidPointCount = 10 ; //14
    private const float AsteroidMinSize = 200; //250
    private const float AsteroidMaxSize = 350; //500
    public const float AsteroidLineThickness = 8f;
    private const float UniverseSize = 15000;
    private const int CollisionRows = 25;
    private const int CollisionCols = 25;
    private const float ShipSize = 70;

    private readonly Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource.Autogun minigun;
    private readonly Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource.Autogun cannon;

    private readonly List<Pds> pdsList;
    
    private readonly float cellSize;

    private bool gameOverScreenActive = false;

    public EndlessSpaceCollision()
    {
        drawInputDeviceInfo = false;
        drawTitle = false;
        Title = "Endless Space Collision";

        universe = new(new Vector2(0f), new Size(UniverseSize, UniverseSize) , new AnchorPoint(0.5f));

        DestroyerPosition = universe.Center + Rng.Instance.RandVec2(UniverseSize * 1.25f, UniverseSize * 2f);

        InitCollisionHandler(universe, CollisionRows, CollisionCols);
        cellSize = UniverseSize / CollisionRows;
        camera = new();
        follower = new(0, 300, 500);
        camera.Follower = follower;
        ship = new(new Vector2(0f), ShipSize);
        ship.collisionHandler = CollisionHandler;
        ship.OnKilled += OnShipKilled;
        
        var minigunStats = new AutogunStats(250, 2, 20, 800, MathF.PI / 15, AutogunStats.TargetingType.Closest);
        var minigunBulletStats = new BulletStats(12, 1250, 15, 0.75f);
        minigun = new(CollisionHandler, minigunStats, minigunBulletStats, Colors.PcCold);
        
        var cannonStats = new AutogunStats(20, 4, 4f, 1750, MathF.PI / 24, AutogunStats.TargetingType.LowestHp);
        var cannonBulletStats = new BulletStats(18, 2500, 200, 1f);
        cannon = new(CollisionHandler, cannonStats, cannonBulletStats, Colors.PcCold);

        var orbitalStrikePdsInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.Bomb, 2f, 8f, 0f, 0);
        var barrage350mmInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.Grenade350mm, 4f, 24f, 30f, 15);
        var barrage100mmInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.Grenade100mm, 4f, 18f, 10f, 40);
        var hypeStrafeInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.HyperBullet, 1.5f, 12f, 2f, 100);
        var turretInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.Turret, 5f, 60f, 1f, 2);
        var penetratorInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.Penetrator, 1f, 5f, 3f, 3);
        
        var orbitalStrike = new Pds(orbitalStrikePdsInfo, DestroyerPosition, new PayloadTargetingSystemBarrage(500f), CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Down, KeyDirection.Down, KeyDirection.Right);
        var barrage350mm = new Pds(barrage350mmInfo, DestroyerPosition, new PayloadTargetingSystemBarrage(2000f),CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Up, KeyDirection.Left, KeyDirection.Left, KeyDirection.Down);
        var barrage100mm = new Pds(barrage100mmInfo, DestroyerPosition, new PayloadTargetingSystemBarrage(1000f),CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Up, KeyDirection.Right, KeyDirection.Right, KeyDirection.Down, KeyDirection.Right);
        var hyperStrafe = new Pds(hypeStrafeInfo, DestroyerPosition, new PayloadTargetingSystemStrafe(3000f, 1500f),CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Left, KeyDirection.Right, KeyDirection.Up);
        var turret = new Pds(turretInfo, DestroyerPosition, new PayloadTargetingSystemBarrage(1500),CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Right, KeyDirection.Down, KeyDirection.Down, KeyDirection.Left, KeyDirection.Right);
        var penetrator = new Pds(penetratorInfo, DestroyerPosition, new PayloadTargetingSystemBarrage(750),CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Down, KeyDirection.Down, KeyDirection.Left, KeyDirection.Left);
        
        pdsList = new()
        {
            orbitalStrike, barrage100mm, barrage350mm, hyperStrafe, turret, penetrator
        };

        foreach (var pds in pdsList)
        {
            pds.OnPayloadLaunched += OnPdsPayloadLaunched;
        }
        
        minigun.BulletFired += OnBulletFired;
        cannon.BulletFired += OnBulletFired;
         
        CollisionHandler?.Add(ship);
        UpdateFollower(camera.BaseSize.Min());
        
        InputActionSettings defaultSettings = new();
        var modifierKeySetGpReversed = new ModifierKeySet(ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
        var toggleDrawKB = new InputTypeKeyboardButton(ShapeKeyboardButton.T);
        var toggleDrawGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
        iaDrawDebug = new(defaultSettings,toggleDrawKB, toggleDrawGP);
        
        var callInUpKb = new InputTypeKeyboardButton(ShapeKeyboardButton.UP);
        var callInUpGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP, 0.05f,  modifierKeySetGpReversed);
        iaPayloadCallinUp = new(defaultSettings,callInUpKb, callInUpGp);
            
        var callInDownKb = new InputTypeKeyboardButton(ShapeKeyboardButton.DOWN);
        var callInDownGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0.05f,  modifierKeySetGpReversed);
        iaPayloadCallinDown = new(defaultSettings,callInDownKb, callInDownGp);
        
        var callInLeftKb = new InputTypeKeyboardButton(ShapeKeyboardButton.LEFT);
        var callInLeftGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT, 0.05f,  modifierKeySetGpReversed);
        iaPayloadCallinLeft = new(defaultSettings,callInLeftKb, callInLeftGp);
        
        var callInRightKb = new InputTypeKeyboardButton(ShapeKeyboardButton.RIGHT);
        var callInRightGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT, 0.05f,  modifierKeySetGpReversed);
        iaPayloadCallinRight = new(defaultSettings,callInRightKb, callInRightGp);
        
        var singleDestructorKb = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
        var singleDestructorGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
        singleDestructorAction = new(defaultSettings,singleDestructorKb, singleDestructorGp);
        
        var multiDestructorKb = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
        var multiDestructorGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
        multiDestructorAction = new(defaultSettings,multiDestructorKb, multiDestructorGp);
        
        var gameoverScreenAcceptKb1 = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
        var gameoverScreenAcceptKb2 = new InputTypeKeyboardButton(ShapeKeyboardButton.ENTER);
        var gameoverScreenAcceptGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
        gameoverScreenAcceptAction = new(defaultSettings,gameoverScreenAcceptKb1, gameoverScreenAcceptKb2, gameoverScreenAcceptGp);
        
        inputActionTree = [
            iaDrawDebug,
            iaPayloadCallinUp,
            iaPayloadCallinDown,
            iaPayloadCallinLeft,
            iaPayloadCallinRight,
            singleDestructorAction,
            multiDestructorAction,
            gameoverScreenAcceptAction
        ];
        
        AddAsteroids(AsteroidCount);

        var factors = new float[] { 1f, 1f, 0.99f , 0.97f, 0.94f};
        for (int i = 0; i < 5; i++)
        {
            var textureHandler = new ScreenTextureHandler(factors[i]);
            var texture = new ScreenTexture(new Dimensions(2000, 2000), textureHandler, ShaderSupportType.None, TextureFilter.Point);
            texture.DrawToScreenOrder = -100 + i;
            texture.OnDrawGame += OnDrawStarTexture;
            texture.Initialize(GameloopExamples.Instance.Window.CurScreenSize, GameloopExamples.Instance.Window.MousePosition, null);
            starTextures.Add(texture);
            starTextureHandlers.Add(textureHandler);
            
        }
    }

    
    private void OnDrawStarTexture(ScreenInfo screeninfo, ScreenTexture texture)
    {
        texture.DrawToTextureEnabled = false;
        float f = (texture.DrawToScreenOrder + 100) / 5f;
        byte alpha = (byte)ShapeMath.LerpInt(75, 200, f);
        for (int i = 0; i < 150; i++)
        {
            var pos = screeninfo.Area.GetRandomPointInside();
            
            CircleDrawing.DrawCircleFast(pos, Rng.Instance.RandF(1, 5), Colors.Highlight.SetAlpha(alpha));
        }
    }

    private void OnPdsPayloadLaunched(IPayload payload, int cur, int max)
    {
        if (payload is TurretPayload tp)
        {
            tp.Turret.BulletFired += OnBulletFired;
        }
    }
    
    private void OnShipKilled()
    {
        gameOverScreenActive = true;
        drawDebug = false;
        foreach (var a in asteroids)
        {
            a.target = null;
        }
        destructors.Clear();
        multiDestructorCooldownTimer = 0f;
        singleDestructorBurstTimer = 0f;
        singleDestructorCooldownTimer = 0f;
    }

    private void OnBulletFired(Autogun gun, EndlessSpaceExampleSource.Bullet bullet)
    {
        bullets.Add(bullet);
    }
    protected override void OnActivate(Scene oldScene)
    {
        Colors.OnColorPaletteChanged += OnColorPaletteChanged;
        foreach (var t in starTextures)
        {
            t.DrawToTextureEnabled = true; // in case color palette has changed
            GameloopExamples.Instance.AddScreenTexture(t);
        }
        
        
        GameloopExamples.Instance.Camera = camera;
        UpdateFollower(camera.BaseSize.Min());
        camera.SetZoom(0.35f);
        follower.SetTarget(ship);

        GameloopExamples.Instance.MouseControlEnabled = false;
    }
    protected override void OnDeactivate()
    {
        Colors.OnColorPaletteChanged -= OnColorPaletteChanged;
        foreach (var t in starTextures)
        {
            GameloopExamples.Instance.RemoveScreenTexture(t);
        }
        GameloopExamples.Instance.ResetCamera();
        
        GameloopExamples.Instance.MouseControlEnabled = true;
    }
    private void OnColorPaletteChanged()
    {
        foreach (var t in starTextures)
        {
            t.DrawToTextureEnabled = true;
        }
    }
   
   
    public override void Reset()
    {
        if (gameOverScreenActive) return;

        drawDebug = false;
        kills = 0;
        CurScore = 0f;
        Difficulty = 1;
        DifficultyScore = 0f;
        killedBigAsteroids = 0;
        
        universe = new(new Vector2(0f), new Size(UniverseSize, UniverseSize) , new AnchorPoint(0.5f));
        CollisionHandler?.Clear();
        destructors.Clear();
        asteroids.Clear();
        shards.Clear();
        bullets.Clear();
        camera.Reset();
        follower.Reset();

        multiDestructorCooldownTimer = 0f;
        singleDestructorBurstTimer = 0f;
        singleDestructorCooldownTimer = 0f;
        
        ship.Reset(new Vector2(0f), ShipSize);
        
        cannon.Reset();
        minigun.Reset();
        foreach (var pds in pdsList)
        {
            pds.Reset();
        }
        CollisionHandler?.Add(ship);
        follower.SetTarget(ship);
        
        UpdateFollower(camera.BaseSize.Min());
        camera.SetZoom(0.35f);

        AddAsteroids(AsteroidCount);
        
    }

    protected override void OnClose()
    {
        foreach (var t in starTextures)
        {
            t.Unload();
        }
    }

    private void AddAsteroids(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AddAsteroid(Rng.Instance.Chance(0.85f));
        }
    }
    private void AddAsteroid(bool big)
    {
        var pos = GetRandomUniversePosition(2500);

        // var minSize = big ? AsteroidMinSize : AsteroidMinSize / 4f;
        var maxSize = big ? AsteroidMaxSize : AsteroidMaxSize / 4f;
        
        // var shape = Polygon.Generate(pos, AsteroidPointCount, minSize, maxSize);
        var shape = Polygon.GenerateRelative(AsteroidPointCount, 0.5f, 1f);
        if (shape == null) return;
        var a = new AsteroidObstacle(shape, pos, maxSize, big);
        if (!big) a.target = ship;
        asteroids.Add(a);
        CollisionHandler?.Add(a);
    }
    private void AddAsteroid(Vector2 pos, bool big)
    {
        // var minSize = big ? AsteroidMinSize : AsteroidMinSize / 4f;
        var maxSize = big ? AsteroidMaxSize : AsteroidMaxSize / 4f;
        
        // var shape = Polygon.Generate(pos, AsteroidPointCount, minSize, maxSize);
        var shape = Polygon.GenerateRelative(AsteroidPointCount, 0.5f, 1f);
        if (shape == null) return;
        var a = new AsteroidObstacle(shape, pos, maxSize, big);
        if (!big) a.target = ship;
        asteroids.Add(a);
        CollisionHandler?.Add(a);
    }
    
    private Vector2 GetRandomUniversePosition(float safeDistance = 2500)
    {
        var pos = universe.GetRandomPointInside();
        var shipPos = ship.GetPosition();
        var safeDistanceSq = safeDistance * safeDistance;
        while ((pos - shipPos).LengthSquared() < safeDistanceSq)
        {
            pos = universe.GetRandomPointInside();
        }

        return pos;
    }
    
    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
    {
        var gamepad = Input.GamepadManager.LastUsedGamepad;
        inputActionTree.CurrentGamepad = gamepad;
        inputActionTree.Update(dt);
        
        if (gameOverScreenActive)
        {
            // if (ShapeKeyboardButton.SPACE.GetInputState().Pressed || 
            //     ShapeKeyboardButton.ENTER.GetInputState().Pressed || 
            //     (Input.GamepadManager.LastUsedGamepad != null && Input.GamepadManager.LastUsedGamepad.IsDown(ShapeGamepadButton.RIGHT_FACE_DOWN)))
            if(gameoverScreenAcceptAction.State.Pressed)
            {
                gameOverScreenActive = false;
                Reset();
            }
            
            return;
        }
        
        if (iaDrawDebug.State.Pressed)
        {
            drawDebug = !drawDebug;
        }

        KeyDirection dir = KeyDirection.None;
        
        if (iaPayloadCallinUp.State.Pressed)
        {
            dir = KeyDirection.Up;
        }
        
        if (iaPayloadCallinDown.State.Pressed)
        {
            dir = KeyDirection.Down;
        }
        
        if (iaPayloadCallinLeft.State.Pressed)
        {
            dir = KeyDirection.Left;
        }
        
        if (iaPayloadCallinRight.State.Pressed)
        {
            dir = KeyDirection.Right;
        }

        if (dir != KeyDirection.None)
        {

            var launched = false;
            var sequenceFailedCount = 0;
            for (int i = 0; i < pdsList.Count; i++)
            {
                var pds = pdsList[i];
                if (pds.KeyPressed(dir))
                {
                    PayloadMarkerSimple marker = new();
                    var speed = Rng.Instance.RandF(3250, 3750);
                    marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f);
                    pds.RequestPayload(marker);
                    launched = true;
                    break;
                }

                if (pds.SequenceFailed || !pds.IsReady) sequenceFailedCount++;

            }

            if (launched || sequenceFailedCount >= pdsList.Count)
            {
                foreach (var pds in pdsList)
                {
                    pds.ResetSequence();
                }
            }
        }

        if (CollisionHandler != null)
        {
            
            var singleDestructorPressed = singleDestructorBurstsRemaining <= 0 && singleDestructorCooldownTimer <= 0f && singleDestructorAction.State.Pressed;
            var multiDestructorPressed = multiDestructorCooldownTimer <= 0 && multiDestructorAction.State.Pressed;
        
            if (singleDestructorPressed)
            {
                singleDestructorBurstsRemaining = singleDestructorBurstCount;
            }
            if (multiDestructorPressed)
            {
                var count = 8;
                var angleStep = 360f / count;
                var pos = ship.Transform.Position;
                Vector2 direction;
                for (int i = 0; i < count; i++)
                {
                    direction = ShapeVec.VecFromAngleDeg(angleStep * i);
                    var destructor = new Destructor(pos, direction,  Colors.PcCold.ColorRgba);
                    destructors.Add(destructor);
                    CollisionHandler.Add(destructor);
                }
                multiDestructorCooldownTimer = multiDestructorCooldown;
            }
        }
        
    }

    private void UpdateFollower(float size)
    {
        // var sliderF = 0.5f;
        // var minBoundary = 0.12f * size;
        // var maxBoundary = ShapeMath.LerpFloat(0.55f, 0.15f, sliderF) * size;
        // var boundary = new Vector2(minBoundary, maxBoundary) * camera.ZoomFactor;
        follower.Speed = EndlessSpaceExampleSource.Ship.Speed * 2.5f;
        // follower.BoundaryDis = new(boundary);
    }
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
    {
        
        foreach (var h in starTextureHandlers)
        {
            h.SetCameraRect(game.Area);
        }

        if (!gameOverScreenActive)
        {
            ship.Update(time, game, gameUi, ui);
            minigun.Update(time.Delta, ship.GetPosition(), ship.GetCurSpeed());
            cannon.Update(time.Delta, ship.GetPosition(), ship.GetCurSpeed());

            foreach (var pds in pdsList)
            {
                pds.Update(time.Delta);
            }

            UpdateFollower(camera.BaseSize.Min());

            var coordinates = ship.GetPosition() / cellSize;
            var uX = (int)coordinates.X * cellSize;
            var uY = (int)coordinates.Y * cellSize;
        
            universe = universe.SetPosition(new Vector2(uX, uY), new(0.5f));
        }

        if (singleDestructorCooldownTimer > 0)
        {
            singleDestructorCooldownTimer -= time.Delta;
            if (singleDestructorCooldownTimer <= 0)
            {
                singleDestructorCooldownTimer = 0;
            }
        }

        if (CollisionHandler != null && singleDestructorBurstsRemaining > 0)
        {
            if (singleDestructorBurstTimer > 0)
            {
                singleDestructorBurstTimer -= time.Delta;
                if (singleDestructorBurstTimer <= 0)
                {
                    singleDestructorBurstTimer = 0;
                }
            }
            else
            {
                var pos = ship.Transform.Position;
                var direction = Game.Instance.Input.CurrentInputDeviceType == InputDeviceType.Gamepad ?  ship.Transform.GetDirection() :  (game.MousePos - pos).Normalize();
                var accuracy = Rng.Instance.RandF(-15, 15) * ShapeMath.DEGTORAD;
                direction = direction.Rotate(accuracy);
                var destructor = new Destructor(pos, direction, ship.CurSpeed,  Colors.PcCold.ColorRgba);
                destructors.Add(destructor);
                CollisionHandler.Add(destructor);
                
                singleDestructorBurstsRemaining--;
                if (singleDestructorBurstsRemaining <= 0) // burst finished
                {
                    singleDestructorCooldownTimer = singleDestructorCooldown;
                    singleDestructorBurstTimer = 0f;
                }
                else
                {
                    singleDestructorBurstTimer = singleDestructorBurstCooldown;
                }
                
            }
            
        }
        

        if (multiDestructorCooldownTimer > 0)
        {
            multiDestructorCooldownTimer -= time.Delta;
            if (multiDestructorCooldownTimer <= 0)
            {
                multiDestructorCooldownTimer = 0;
            }
        }
        
        CollisionHandler?.ResizeBounds(universe);
        CollisionHandler?.Update(time.Delta);

        // var removed = 0;
        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var a = asteroids[i];
            a.Update(time, game, gameUi, ui);
            if (!universe.OverlapShape(a.GetShape()))
            {
                a.MoveTo(GetRandomUniversePosition(2500));
                // asteroids.RemoveAt(i);
                // CollisionHandler?.Remove(a);
                // removed++;
            }

            if (a.IsDead)
            {
                kills++;
                asteroids.RemoveAt(i);
                CollisionHandler?.Remove(a);
                foreach (var tri in a.Triangulation)
                {
                    var shard = new EndlessSpaceExampleSource.AsteroidShard(tri);
                    shards.Add(shard);
                }

                float scoreBonus = 1f; // ShapeMath.LerpFloat(0.5f, 2, DifficultyFactor);
                
                if (a.Big)
                {
                    killedBigAsteroids++;
                    CurScore += BigAsteroidScore * scoreBonus;
                    DifficultyScore += BigAsteroidScore * scoreBonus;
                    
                    int amount; // = ShapeRandom.RandI(6, 12);
                    if (DifficultyFactor < 0.2f) amount = 3;
                    else if (DifficultyFactor < 0.4f) amount = 6;
                    else if (DifficultyFactor < 0.6f) amount = 9;
                    else if (DifficultyFactor < 0.8f) amount = 12;
                    else if (DifficultyFactor < 1f) amount = 15;
                    else amount = 20;
                    
                    for (int j = 0; j < amount; j++)
                    {
                        var randPos = a.Transform.Position + Rng.Instance.RandVec2(0, AsteroidMaxSize);
                        AddAsteroid(randPos, false);
                    }
                }
                else
                {
                    CurScore += SmallAsteroidScore * scoreBonus;
                    DifficultyScore += SmallAsteroidScore * scoreBonus;
                }

                if (DifficultyScore >= DifficultyIncreaseThreshold)
                {
                    Difficulty++;
                    DifficultyScore = DifficultyScore - DifficultyIncreaseThreshold;

                    float f = ShapeMath.LerpFloat(1f, 2f, DifficultyFactor);
                    AddAsteroids((int)(killedBigAsteroids * f));
                    killedBigAsteroids = 0;
                }
            }
        }

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            bullet.Update(time, game, gameUi, ui);

            if (bullet.IsDead)
            {
                bullets.RemoveAt(i);
                CollisionHandler?.Remove(bullet);
            }
        }
        
        for (int i = shards.Count - 1; i >= 0; i--)
        {
            var shard = shards[i];
            shard.Update(time.Delta);
            if(shard.IsDead) shards.RemoveAt(i);
        }

        for (int i = destructors.Count - 1; i >= 0; i--)
        {
            var destructor = destructors[i];
            destructor.Update(time, game, gameUi, ui);
            
            if (destructor.IsDead)
            {
                destructors.RemoveAt(i);
                CollisionHandler?.Remove(destructor);
                
            }
        }
    }
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        universe.DrawLines(12f, Colors.Dark);
        var lineInfo = new LineDrawingInfo(12f, Colors.Dark.SetAlpha(200), LineCapType.None, 0);
        universe.DrawGrid(CollisionRows, lineInfo);
        
        if (drawDebug)
        {
            
            CollisionHandler?.DebugDraw(Colors.Light, Colors.Medium.SetAlpha(150));
            
            float thickness = 2f * camera.ZoomFactor;
            var boundarySize = follower.BoundaryDis.ToVector2();
            var boundaryCenter = camera.BasePosition;

            if (boundarySize.X > 0)
            {
                var innerBoundary = new Circle(boundaryCenter, boundarySize.X);
                var innerColor = Colors.Highlight.ChangeAlpha((byte)150);
                innerBoundary.DrawLines(thickness, innerColor);
                
            }

            if (boundarySize.Y > 0)
            {
                var outerBoundary = new Circle(boundaryCenter, boundarySize.Y);
                var outerColor = Colors.Special.ChangeAlpha((byte)150);
                outerBoundary.DrawLines(thickness, outerColor);
            }


            var minigunRange = new Circle(ship.GetPosition(), minigun.Stats.DetectionRange);
            var cannonRange = new Circle(ship.GetPosition(), cannon.Stats.DetectionRange);
            var c = Colors.PcCold.ColorRgba.ChangeAlpha((byte)150);
            minigunRange.DrawLines(4f, c);
            cannonRange.DrawLines(4f, c);

        }

        
        foreach (var shard in shards)
        {
            shard.Draw();
        }
        foreach (var asteroid in asteroids)
        {
            asteroid.DrawGame(game);
        }
        foreach (var destructor in destructors)
        {
            destructor.DrawGame(game);
        }
        
        
        ship.DrawGame(game);
        minigun.Draw();
        cannon.Draw();

        foreach (var pds in pdsList)
        {
            pds.Draw();
        }

        foreach (var bullet in bullets)
        {
            bullet.DrawGame(game);
        }
        // var cutShapeColor = Colors.Warm.SetAlpha(100);
        // foreach (var cutShape in lastCutShapes)
        // {
        //     cutShape.Draw(cutShapeColor);
        // }
       
        
        CircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -80, -10, 12f, Colors.PcDark.ColorRgba, false, 8f);
        CircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -100, -170, 12f, Colors.PcDark.ColorRgba, false, 8f);
        CircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, 170, 10, 12f, Colors.PcDark.ColorRgba, false, 8f);

        if (minigun.ReloadF > 0f)
        {
            CircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -80, ShapeMath.LerpFloat(-80, -10, minigun.ReloadF), 4f, Colors.PcWarm.ColorRgba, false, 8f);
        }
        else CircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -80, ShapeMath.LerpFloat(-80, -10, 1f - minigun.ClipSizeF), 4f, Colors.PcCold.ColorRgba, false, 8f);

        if (cannon.ReloadF > 0f)
        {
            CircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -100, ShapeMath.LerpFloat(-100, -170, cannon.ReloadF), 4f, Colors.PcWarm.ColorRgba, false, 8f);
        }
        else CircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -100, ShapeMath.LerpFloat(-100, -170, 1f - cannon.ClipSizeF), 4f, Colors.PcCold.ColorRgba, false, 8f);
        
        
        CircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, 170, ShapeMath.LerpFloat(170, 10, ship.HealthF), 4f, Colors.PcWarm.ColorRgba, false, 8f);
    }
    protected override void OnDrawGameUIExample(ScreenInfo gameUi)
    {
    }
    protected override void OnDrawUIExample(ScreenInfo ui)
    {

        if (gameOverScreenActive)
        {

            var area = ui.Area.ApplyMargins(0.1f, 0.1f, 0.2f, 0.2f);
            
            area.Draw(Colors.Dark.SetAlpha(200));

            var rects = area.ApplyMargins(0.05f, 0.05f, 0.05f, 0.05f).SplitV(0.4f, 0.4f);
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone("Game Over", rects[0], new(0.5f));
            
            textFont.ColorRgba = Colors.Special;
            textFont.DrawTextWrapNone($"Final Score: {CurScore} | Kills: {kills}", rects[1], new(0.5f));
            
            textFont.ColorRgba = Colors.Highlight;
            textFont.DrawTextWrapNone($"Press Space", rects[2], new(0.5f));
            return;
        }
        
        if(drawTitle) DrawGameInfo(GameloopExamples.Instance.UIRects.GetRect("center"));
        else DrawGameInfoNoTitle(GameloopExamples.Instance.UIRects.GetRect("top center"));


        var bottomUiZone = GameloopExamples.Instance.UIRects.GetRect("bottom");
        var bottomUiSplit = bottomUiZone.SplitV(0.3f);
        var destructorZone = bottomUiSplit.top.ApplyMargins(0.25f, 0.25f, 0f, 0f);
        var destructorZoneSplit = destructorZone.SplitH(0.4f, 0.2f);
        var singleDestructorRect = destructorZoneSplit[0];
        var multiDestructorRect = destructorZoneSplit[2];
        float thickness = singleDestructorRect.Height * 0.05f;
        float cornerLength = singleDestructorRect.Height * 0.35f;
        var singleDestructorRectBar = singleDestructorRect.ApplyMarginsAbsolute(thickness * 2);
        var multiDestructorRectBar = multiDestructorRect.ApplyMarginsAbsolute(thickness * 2);
        var singleDestructorF = singleDestructorCooldownTimer / singleDestructorCooldown;
        var multiDestructorF = multiDestructorCooldownTimer / multiDestructorCooldown;

        var singleDestructorStripedBarRect = singleDestructorRectBar.GetProgressRect(singleDestructorF, 1f, 0f, 0f, 0f).ApplyMargins(0.01f, 0.01f, 0.04f, 0.04f);
        var multiDestructorStripedBarRect = multiDestructorRectBar.GetProgressRect(multiDestructorF, 0f, 1f, 0f, 0f).ApplyMargins(0.01f, 0.01f, 0.04f, 0.04f);
        LineDrawingInfo stripedBarInfo = new LineDrawingInfo(thickness, Colors.Warm, LineCapType.Capped, 4);
        
        singleDestructorRect.DrawCorners(new LineDrawingInfo(thickness, Colors.Warm, LineCapType.Capped, 4), cornerLength);
        singleDestructorRectBar.Draw(Colors.Medium);
        singleDestructorStripedBarRect.DrawStriped(singleDestructorRectBar.Width * 0.015f, -15, stripedBarInfo);

        
        multiDestructorRect.DrawCorners(new LineDrawingInfo(thickness, Colors.Warm, LineCapType.Capped, 4), cornerLength);
        multiDestructorRectBar.Draw(Colors.Medium);
        multiDestructorStripedBarRect.DrawStriped(multiDestructorRectBar.Width * 0.015f, 15, stripedBarInfo);

        var strategemZone = bottomUiSplit.bottom.ApplyMargins(0f, 0f, 0.1f, 0f);
        var splitStrategem = strategemZone.SplitH(pdsList.Count);
        
        for (int i = 0; i < pdsList.Count; i++)
        {
            var pds = pdsList[i];
            var pdsRect = splitStrategem[i].ApplyMargins(0.025f, 0.025f, 0f, 0f);
            pds.DrawUI(pdsRect);
        }
    }

    private void DrawGameInfo(Rect rect)
    {
        rect = rect.ApplyMargins(0.2f, 0.2f, 0.025f, 0.92f);
        string text =
            $"[{Difficulty} {ShapeMath.RoundToDecimals(DifficultyScore / DifficultyIncreaseThreshold, 2) * 100}%] | Score: {ShapeMath.RoundToDecimals(CurScore, 2)}";
        textFont.ColorRgba = Colors.Special;
        textFont.DrawTextWrapNone(text, rect, new(0.5f, 0f));
    }
    private void DrawGameInfoNoTitle(Rect rect)
    {
        rect = rect.ApplyMargins(0.1f, 0.1f, 0.025f, 0.5f);
        string text =
            $"[{Difficulty} {ShapeMath.RoundToDecimals(DifficultyScore / DifficultyIncreaseThreshold, 2) * 100}%] | Score: {ShapeMath.RoundToDecimals(CurScore, 2)}";
        textFont.ColorRgba = Colors.Special;
        textFont.DrawTextWrapNone(text, rect, new(0.5f, 0f));
    }
}