using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Screen;
using System.Numerics;
using Examples.PayloadSystem;
using Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Circle;
using ShapeEngine.Geometry.Polygon;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.Input;
using Size = ShapeEngine.Core.Structs.Size;
using ShapeEngine.Random;
using ShapeEngine.StaticLib.Drawing;

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
        
        var toggleDrawKB = new InputTypeKeyboardButton(ShapeKeyboardButton.T);
        var toggleDrawGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
        iaDrawDebug = new(toggleDrawKB, toggleDrawGP);
        
        var callInUpKb = new InputTypeKeyboardButton(ShapeKeyboardButton.UP);
        var callInUpGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP);
        iaPayloadCallinUp = new(callInUpKb, callInUpGp);
            
        var callInDownKb = new InputTypeKeyboardButton(ShapeKeyboardButton.DOWN);
        var callInDownGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN);
        iaPayloadCallinDown = new(callInDownKb, callInDownGp);
        
        var callInLeftKb = new InputTypeKeyboardButton(ShapeKeyboardButton.LEFT);
        var callInLeftGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT);
        iaPayloadCallinLeft = new(callInLeftKb, callInLeftGp);
        
        var callInRightKb = new InputTypeKeyboardButton(ShapeKeyboardButton.RIGHT);
        var callInRightGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT);
        iaPayloadCallinRight = new(callInRightKb, callInRightGp);
        
        AddAsteroids(AsteroidCount);

        var factors = new float[] { 1f, 1f, 0.99f , 0.97f, 0.94f};
        for (int i = 0; i < 5; i++)
        {
            var textureHandler = new ScreenTextureHandler(factors[i]);
            var texture = new ScreenTexture(new Dimensions(2000, 2000), textureHandler, ShaderSupportType.None, TextureFilter.Point);
            texture.DrawToScreenOrder = -100 + i;
            texture.OnDrawGame += OnDrawStarTexture;
            texture.Initialize(GAMELOOP.Window.CurScreenSize, GAMELOOP.Window.MousePosition, null);
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
            
            ShapeCircleDrawing.DrawCircleFast(pos, Rng.Instance.RandF(1, 5), Colors.Highlight.SetAlpha(alpha));
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
            GAMELOOP.AddScreenTexture(t);
        }
        
        
        GAMELOOP.Camera = camera;
        UpdateFollower(camera.BaseSize.Min());
        camera.SetZoom(0.35f);
        follower.SetTarget(ship);
    }
    protected override void OnDeactivate()
    {
        Colors.OnColorPaletteChanged -= OnColorPaletteChanged;
        foreach (var t in starTextures)
        {
            GAMELOOP.RemoveScreenTexture(t);
        }
        GAMELOOP.ResetCamera();
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
        if (gameOverScreenActive)
        {
            if (ShapeKeyboardButton.SPACE.GetInputState().Pressed)
            {
                gameOverScreenActive = false;
                Reset();
            }
            
            return;
        }
        
        var gamepad = GAMELOOP.CurGamepad;
        iaDrawDebug.Gamepad = gamepad;
        iaDrawDebug.Update(dt);
        
        iaPayloadCallinUp.Gamepad = gamepad;
        iaPayloadCallinUp.Update(dt);
        
        iaPayloadCallinDown.Gamepad = gamepad;
        iaPayloadCallinDown.Update(dt);
        
        iaPayloadCallinLeft.Gamepad = gamepad;
        iaPayloadCallinLeft.Update(dt);
        
        iaPayloadCallinRight.Gamepad = gamepad;
        iaPayloadCallinRight.Update(dt);
        
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
            
            /*var finished = orbitalStrike.KeyPressed(dir);
            if (finished)
            {
                PayloadMarkerSimple marker = new();
                var speed = ShapeRandom.RandF(3250, 3750);
                marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
                orbitalStrike.RequestPayload(marker);
            }
            else
            {
                finished = barrage350mm.KeyPressed(dir);
                if (finished)
                {
                    PayloadMarkerSimple marker = new();
                    var speed = ShapeRandom.RandF(3250, 3750);
                    marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
                    barrage350mm.RequestPayload(marker);
                }
                else
                {
                    finished = barrage100mm.KeyPressed(dir);
                    if (finished)
                    {
                        PayloadMarkerSimple marker = new();
                        var speed = ShapeRandom.RandF(3250, 3750);
                        marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
                        barrage100mm.RequestPayload(marker);
                    }
                    else
                    {
                        finished = hyperStrafe.KeyPressed(dir);
                        if (finished)
                        {
                            PayloadMarkerSimple marker = new();
                            var speed = ShapeRandom.RandF(3250, 3750);
                            marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
                            hyperStrafe.RequestPayload(marker);
                        }

                    }
                }
            }

            if (finished)
            {
                orbitalStrike.ResetSequence();
                barrage350mm.ResetSequence();
                barrage100mm.ResetSequence();
                hyperStrafe.ResetSequence();
            }*/
            
        }

        if (CollisionHandler != null)
        {
            var singleDestructorPressed = singleDestructorBurstsRemaining <= 0 && singleDestructorCooldownTimer <= 0f && (ShapeKeyboardButton.Q.GetInputState().Pressed);
            var multiDestructorPressed = multiDestructorCooldownTimer <= 0 && (ShapeKeyboardButton.E.GetInputState().Pressed);
        
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
        

        /*if ((ShapeKeyboardButton.ONE.GetInputState().Pressed || ShapeKeyboardButton.UP.GetInputState().Pressed) && orbitalStrike.IsReady)
        {
            PayloadMarkerSimple marker = new();
            var speed = ShapeRandom.RandF(3250, 3750);
            marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
            orbitalStrike.RequestPayload(marker);
        }
        if ((ShapeKeyboardButton.TWO.GetInputState().Pressed || ShapeKeyboardButton.DOWN.GetInputState().Pressed) && barrage350mm.IsReady)
        {
            PayloadMarkerSimple marker = new();
            var speed = ShapeRandom.RandF(3250, 3750);
            marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
            barrage350mm.RequestPayload(marker);
        }
        if ((ShapeKeyboardButton.THREE.GetInputState().Pressed || ShapeKeyboardButton.LEFT.GetInputState().Pressed) && barrage100mm.IsReady)
        {
            PayloadMarkerSimple marker = new();
            var speed = ShapeRandom.RandF(3250, 3750);
            marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
            barrage100mm.RequestPayload(marker);
        }
        if ((ShapeKeyboardButton.FOUR.GetInputState().Pressed || ShapeKeyboardButton.RIGHT.GetInputState().Pressed) && hyperStrafe.IsReady)
        {
            PayloadMarkerSimple marker = new();
            var speed = ShapeRandom.RandF(3250, 3750);
            marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
            hyperStrafe.RequestPayload(marker);
        }
        // if ((ShapeKeyboardButton.ONE.GetInputState().Pressed || ShapeKeyboardButton.UP.GetInputState().Pressed) && orbitalStrike.IsReady)
        // {
        //     strategemChargeTimer = dt;
        // }
        //
        // if ((ShapeKeyboardButton.ONE.GetInputState().Released || ShapeKeyboardButton.UP.GetInputState().Released) && orbitalStrike.IsReady)
        // {
        //     var speed = ShapeMath.LerpFloat(1000, 2500, StrategemChargeF);
        //     orbitalStrike.Request(ship.GetBarrelPosition(), ship.GetBarrelDirection() * speed);
        //     strategemChargeTimer = 0f;
        // }
        //
        // if ((ShapeKeyboardButton.TWO.GetInputState().Pressed || ShapeKeyboardButton.DOWN.GetInputState().Pressed) && barrage350mm.IsReady)
        // {
        //     strategemChargeTimer = dt;
        // }
        //
        // if ((ShapeKeyboardButton.TWO.GetInputState().Released || ShapeKeyboardButton.DOWN.GetInputState().Released) && barrage350mm.IsReady)
        // {
        //     var speed = ShapeMath.LerpFloat(1000, 2500, StrategemChargeF);
        //     barrage350mm.Request(ship.GetBarrelPosition(), ship.GetBarrelDirection() * speed);
        //     strategemChargeTimer = 0f;
        // }
        //
        */
        
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
        
        // if (lastCutShapeTimers.Count > 0)
        // {
        //     for (int i = lastCutShapeTimers.Count - 1; i >= 0; i--)
        //     {
        //         var timer = lastCutShapeTimers[i];
        //         timer -= time.Delta;
        //         if (timer <= 0f)
        //         {
        //             lastCutShapeTimers.RemoveAt(i);
        //             lastCutShapes.RemoveAt(i);
        //         }
        //         else lastCutShapeTimers[i] = timer;
        //     }
        // }

        if (!gameOverScreenActive)
        {
            // if (strategemChargeTimer > 0f)
            // {
            //     strategemChargeTimer += time.Delta;
            //     if (strategemChargeTimer > strategemMaxChargeTime) strategemChargeTimer = strategemMaxChargeTime;
            // }
            
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
                var direction = (game.MousePos - pos).Normalize();
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
       
        
        ShapeCircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -80, -10, 12f, Colors.PcDark.ColorRgba, false, 8f);
        ShapeCircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -100, -170, 12f, Colors.PcDark.ColorRgba, false, 8f);
        ShapeCircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, 170, 10, 12f, Colors.PcDark.ColorRgba, false, 8f);

        if (minigun.ReloadF > 0f)
        {
            ShapeCircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -80, ShapeMath.LerpFloat(-80, -10, minigun.ReloadF), 4f, Colors.PcWarm.ColorRgba, false, 8f);
        }
        else ShapeCircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -80, ShapeMath.LerpFloat(-80, -10, 1f - minigun.ClipSizeF), 4f, Colors.PcCold.ColorRgba, false, 8f);

        if (cannon.ReloadF > 0f)
        {
            ShapeCircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -100, ShapeMath.LerpFloat(-100, -170, cannon.ReloadF), 4f, Colors.PcWarm.ColorRgba, false, 8f);
        }
        else ShapeCircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -100, ShapeMath.LerpFloat(-100, -170, 1f - cannon.ClipSizeF), 4f, Colors.PcCold.ColorRgba, false, 8f);
        
        
        ShapeCircleDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, 170, ShapeMath.LerpFloat(170, 10, ship.HealthF), 4f, Colors.PcWarm.ColorRgba, false, 8f);
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
        
        // DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
        // DrawCameraInfo(GAMELOOP.UIRects.GetRect("bottom right"));
        
        if(drawTitle) DrawGameInfo(GAMELOOP.UIRects.GetRect("center"));
        else DrawGameInfoNoTitle(GAMELOOP.UIRects.GetRect("top center"));


        var bottomUiZone = GAMELOOP.UIRects.GetRect("bottom");
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

        
        // singleDestructorRectBar.DrawBar(singleDestructorF, Colors.Warm, Colors.Medium, 0.5f, 0.5f, 0f, 0f);
        
        multiDestructorRect.DrawCorners(new LineDrawingInfo(thickness, Colors.Warm, LineCapType.Capped, 4), cornerLength);
        multiDestructorRectBar.Draw(Colors.Medium);
        multiDestructorStripedBarRect.DrawStriped(multiDestructorRectBar.Width * 0.015f, 15, stripedBarInfo);
        // multiDestructorRectBar.DrawBar(multiDestructorF, Colors.Warm, Colors.Medium, 0.5f, 0.5f, 0f, 0f);

        var strategemZone = bottomUiSplit.bottom.ApplyMargins(0f, 0f, 0.1f, 0f);//  GAMELOOP.UIRects.GetRect("bottom").ApplyMargins(0f, 0f, 0.25f, 0f); // topBottomRect.top.ApplyMargins(0f, 0f, 0f, 0.25f); // ui.Area.ApplyMargins(0.2f, 0.2f, 0.91f, 0.06f);
        var splitStrategem = strategemZone.SplitH(pdsList.Count);// strategemZone.SplitH(0.225f,0.033f,0.225f,0.033f,0.225f,0.033f);
        
        for (int i = 0; i < pdsList.Count; i++)
        {
            var pds = pdsList[i];
            var pdsRect = splitStrategem[i].ApplyMargins(0.025f, 0.025f, 0f, 0f);
            pds.DrawUI(pdsRect);
        }
        // var gunRect = GAMELOOP.UIRects.GetRect("center right");// GAMELOOP.UIRects.GetRect("bottom right").Union(GAMELOOP.UIRects.GetRect("center right"));
        // gunRect = gunRect.ApplyMargins(0.65f, 0f, 0f, 0.5f);
        // var gunSplit = gunRect.SplitH(0.2f, 0.35f);
        // minigun.DrawUI(gunSplit[2].ApplyMargins(0.15f, 0f, 0f, 0));
        // cannon.DrawUI(gunSplit[1].ApplyMargins(0.15f, 0f, 0f, 0));
        //
        // var count = Ship.MaxHp;
        // var hpRects = gunSplit[0].ApplyMargins(0f, 0f, 0f, 0.5f).SplitV(count);
        // for (int i = 0; i < count; i++)
        // {
        //     var hpRect = hpRects[i].ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
        //     if (i < ship.Health)
        //     {
        //         hpRect.Draw(Colors.Cold);
        //     }
        //     else
        //     {
        //         hpRect.Draw(Colors.Medium);
        //     }
        // }
    }

    // private void DrawCameraInfo(Rect rect)
    // {
    //     var pos = camera.BasePosition;
    //     var x = (int)pos.X;
    //     var y = (int)pos.Y;
    //     var rot = (int)camera.RotationDeg;
    //     var zoom = (int)(ShapeMath.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
    //     
    //     // string text = $"Pos {x}/{y} | Rot {rot} | Zoom {zoom}";
    //     // string text = $"Path requests {pathfinder.DEBUG_PATH_REQUEST_COUNT}";
    //
    //     var count = 0;
    //     foreach (var asteroid in asteroids)
    //     {
    //         count += asteroid.GetShape().Count;
    //     }
    //     
    //     textFont.FontSpacing = 1f;
    //     textFont.ColorRgba = Colors.Warm;
    //     var rects = rect.SplitV(0.33f, 0.33f);
    //     textFont.DrawTextWrapNone($"Asteroids {asteroids.Count} | V{count}", rects[0], new(0.5f));
    //     textFont.DrawTextWrapNone($"Bullets {bullets.Count}", rects[1], new(0.5f));
    //     // textFont.DrawTextWrapNone($"Ship Transform {ship.Transform.Position},{ship.Transform.RotationRad},{ship.Transform.Size}", rects[1], new(0.5f));
    //     
    // }
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