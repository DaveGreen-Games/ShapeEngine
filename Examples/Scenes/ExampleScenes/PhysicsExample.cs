using System.Diagnostics;
using System.Numerics;
using Examples.Scenes.ExampleScenes.PhysicsExampleSource;
using nkast.Aether.Physics2D;
using nkast.Aether.Physics2D.Controllers;
using nkast.Aether.Physics2D.Dynamics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using ShapeEngine.Text;
using ShapeEngine.StaticLib.Drawing;
using Color = System.Drawing.Color;

namespace Examples.Scenes.ExampleScenes;


/*
 
private World _world;
private Body _playerBody;
private Body _groundBody;
private float _playerBodyRadius = 1.5f / 2f; // player diameter is 1.5 meters
private Vector2 _groundBodySize = new Vector2(8f, 1f); // ground is 8x1 meters

//Create a world
_world = new World();

// Create the player
Vector2 playerPosition = new Vector2(0, _playerBodyRadius);
_playerBody = _world.CreateBody(playerPosition, 0, BodyType.Dynamic);
Fixture pfixture = _playerBody.CreateCircle(_playerBodyRadius, 1f);
// Give it some bounce and friction
pfixture.Restitution = 0.3f;
pfixture.Friction = 0.5f;

// Create the ground
Vector2 groundPosition = new Vector2(0, -(_groundBodySize.Y / 2f));
_groundBody = _world.CreateBody(groundPosition, 0, BodyType.Static);
Fixture gfixture = _groundBody.CreateRectangle(_groundBodySize.X, _groundBodySize.Y, 1f, Vector2.Zero);
gfixture.Restitution = 0.3f;
gfixture.Friction = 0.5f;

// Update the world
float dt = 1f/60f;
_world.Step(dt);

*/


public class InterstellarObject
{
    private readonly Body body;
    private readonly World world;
    private readonly Fixture fixture;

    public InterstellarObject(World world, Vector2 position, float rotationRadians, float radius)
    {
        this.world = world;
        body = world.CreateBody(position.ToAetherVector2(), rotationRadians, BodyType.Dynamic);
        body.Mass = 1000;
        body.LinearDamping = 0f;
        body.AngularDamping = 0f;
        fixture = body.CreateCircle(radius, 1f);
        fixture.Restitution = 0.3f;
        fixture.Friction = 0.5f;
        // var randForce = Rng.Instance.RandVec2(10000, 1000000000);
        // body.ApplyForce(randForce.ToAetherVector2());
        body.LinearVelocity = Rng.Instance.RandVec2(0, 1000000000).ToAetherVector2();
    }
    

    public void Draw()
    {
        var radius = fixture.Shape.Radius;
        var center = body.Position.ToSystemVector2();
        var circle = new Circle(center, radius);
        circle.DrawLines(2f, Colors.Special2, 4f);


        var vel = body.LinearVelocity;
        if (vel.LengthSquared() > 0f)
        {
            var dir = body.LinearVelocity.ToSystemVector2().Normalize();
            var headingPoint = center + dir * radius;
            var headingCircle = new Circle(headingPoint, radius * 0.1f);
            headingCircle.Draw(Colors.Special2);
        }
        
        Console.WriteLine($"InterstellarObject velocity: {body.LinearVelocity.Length():F2} m/s");
    }
    
}


public class PhysicsExample : ExampleScene
{
    public static readonly World PhysicsWorld = new(new nkast.Aether.Physics2D.Common.Vector2(0, 0));
    private readonly Rect sectorRect;
    private readonly ShapeCamera camera;
    private readonly CameraFollowerSingle follower;
    private const float SectorRadiusOutside = 9000;
    private const float SectorRadiusInside = 8250;
   
    private readonly float cellSize;
    
    private readonly List<TextureSurface> starSurfaces = new();
    
    private readonly TextFont gameFont = new(GAMELOOP.GetFont(FontIDs.JetBrainsLarge), 15f, Colors.Warm);

    private readonly PhysicsExampleSource.Ship ship;
    
    private readonly List<PhysicsExampleSource.Asteroid> asteroids = new();
    private readonly List<PhysicsExampleSource.Asteroid> attractorAsteroids = new();
    private readonly List<PhysicsExampleSource.Asteroid> repulsorAsteroids = new();
    
    
    private readonly List<InterstellarObject> interstellarObjects = new();
    
    
    public PhysicsExample()
    {
        // var limiter = new VelocityLimitController(0f, 0f);
        // PhysicsWorld.Add(limiter);
        
        Title = "Physics!";
        var rectSize = SectorRadiusOutside * 2;
        sectorRect = new(new Vector2(0f), new Size(rectSize, rectSize) , new AnchorPoint(0.5f));
        
        camera = new();
        follower = new(0, 300, 500);
        camera.Follower = follower;

        const int textureCount = 3;
        const int starCount = 1000;
        for (int i = 0; i < textureCount; i++)
        {
            var f = i / (float)(textureCount - 1);
            var t = new TextureSurface(2048, 2048);
            t.SetTextureFilter(TextureFilter.Bilinear);
            var sizeMin = ShapeMath.LerpFloat(1f, 1.5f, f);
            var sizeMax = ShapeMath.LerpFloat(1.5f, 3f, f);
            var sizeRange = new ValueRange(sizeMin, sizeMax);
            var color = ColorRgba.Lerp(ColorRgba.White, ColorRgba.White.SetAlpha(220), f);
            t.BeginDraw(ColorRgba.Clear);
            for (int j = 0; j < starCount; j++)
            {
                var x = Rng.Instance.RandF(0f, 2048);
                var y = Rng.Instance.RandF(0f, 2048);
                var pos = new Vector2(x, y);
                var size = sizeRange.Rand();
                ShapeCircleDrawing.DrawCircle(pos, size, color);
            }
            t.EndDraw();
            
            starSurfaces.Add(t);
        }

        ship = new PhysicsExampleSource.Ship(65, Colors.PcWarm, PhysicsWorld);
        
        // if (CollisionHandler != null)
        // {
        //     ship = new(65, Colors.PcWarm);
        //     CollisionHandler.Add(ship);
        //     
        //     
        //     for (int i = 0; i < 100; i++)
        //     {
        //         var randPos = Rng.Instance.RandVec2(500, SectorRadiusInside - 250);
        //         var asteroid = new PhysicsExampleSource.Asteroid(randPos, Colors.PcCold);
        //         asteroids.Add(asteroid);
        //         CollisionHandler.Add(asteroid);
        //         if(asteroid.AsteroidType == AsteroidType.Attractor) attractorAsteroids.Add(asteroid);
        //         else if(asteroid.AsteroidType == AsteroidType.Repulsor) repulsorAsteroids.Add(asteroid);
        //     }
        // }
        
    }
    
    protected override void OnActivate(Scene oldScene)
    {
        FontDimensions.FontSizeRange = new(10, 500);
        
        Colors.OnColorPaletteChanged += OnColorPaletteChanged;
        
        GAMELOOP.Camera = camera;
        camera.SetZoom(0.65f);
        ship.Spawn(sectorRect.Center, Rng.Instance.RandAngleDeg());
        follower.SetTarget(ship);
        follower.Speed = 1000;
        follower.BoundaryDis = new ValueRange(100, 200);
    }
    protected override void OnDeactivate()
    {
        FontDimensions.FontSizeRange = FontDimensions.FontSizeRangeDefault;
        
        Colors.OnColorPaletteChanged -= OnColorPaletteChanged;
        
        GAMELOOP.ResetCamera();
    }
    private void OnColorPaletteChanged()
    {
        
    }

    public override void Reset()
    {
        
    }
    
    
    
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        ship.Update(time, game, gameUi, ui);
        
        var shipOutOfBoundsForce = ShapePhysics.CalculateReverseAttractionForce(
            Vector2.Zero,
            ship.ThrustForce * 2.5f,
            // 12 * ship.Mass,
            new ValueRange(SectorRadiusInside, SectorRadiusOutside),
            ship.Transform.Position
            );
        ship.AddForce(shipOutOfBoundsForce);
        
        foreach (var attractor in attractorAsteroids)
        {
            var w = attractor.Transform.Position - ship.Transform.Position;
            var lsq = w.LengthSquared();
            if(lsq <= 0f) continue;
            
            var l = (float)Math.Sqrt(lsq);
            var dir = w / l;
            ship.AddForce(attractor.AttractionForce * dir / l);
            // ship.ApplyAttraction(attractor.Transform.Position, attractor.AttractionForce, 1.25f);
        }
        
        foreach (var repulsor in repulsorAsteroids)
        {
            var w = repulsor.Transform.Position - ship.Transform.Position;
            var lsq = w.LengthSquared();
            if(lsq <= 0f) continue;
            
            var l = (float)Math.Sqrt(lsq);
            var dir = w / l;
            ship.AddForce(repulsor.RepulsorForce * dir / l);
            // ship.ApplyRepulsion(repulsor.Transform.Position, repulsor.RepulsorForce, 1.25f);
        }
        
        PhysicsWorld.Step(time.Delta);
        
        ship.UpdatePhysicsState();
        
        // foreach (var asteroid in asteroids)
        // {
        //     asteroid.Update(time, game, gameUi, ui);
        //     var asteroidOutOfBoundsForce = ShapePhysics.CalculateReverseAttractionForce(
        //         Vector2.Zero,
        //         5000 * asteroid.Mass,
        //         new ValueRange(SectorRadiusInside, SectorRadiusOutside),
        //         asteroid.Transform.Position
        //     );
        //     asteroid.AddForce(asteroidOutOfBoundsForce);
        // }
        
        
        
        
        
        follower.Speed = ship.CurSpeed * 1.5f * Vector2Extensions.PositionScaleFactor;
        var speedF = ship.CurSpeed / ship.MaxSpeed;
        var targetZoom = ShapeMath.LerpFloat(0.75f, 0.4f, speedF);
        var curZoom = camera.BaseZoomLevel;
        
        var newZoom = ShapeMath.ExpDecayLerpFloat(curZoom, targetZoom, 0.5f, time.Delta);
        camera.SetZoom(newZoom);


        if (ShapeMouseButton.LEFT.GetInputState().Pressed)
        {
            var mousePos = game.MousePos;
            for (int i = 0; i < 10; i++)
            {
                var randPos = mousePos + Rng.Instance.RandVec2(-100, 100);
                var io = new InterstellarObject(PhysicsWorld, randPos, Rng.Instance.RandAngleRad(), Rng.Instance.RandF(5, 15));
                interstellarObjects.Add(io);
            }
        }
    }


    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
    {
        
    }

    

    protected override void OnDrawGameExample(ScreenInfo game)
    {
        
        
        var target = camera.BasePosition;
        var count = starSurfaces.Count;
        for (int i = 0; i < count; i++)
        {
            var surface = starSurfaces[i];
            var f = i / (float)(count - 1);
            var scaleFactor = i == 0 ? 0f : i == 1 ? 0.01f : 0.75f;
            var pos = target * scaleFactor;

            var sourceRect = new Rect(pos, surface.Rect.Size, AnchorPoint.Center);
            var targetRect = new Rect(target, surface.Rect.Size * 3, AnchorPoint.Center);
            surface.Draw(sourceRect, targetRect, ColorRgba.White);
        }
        
        DrawCircleSector();
        
        foreach (var asteroid in asteroids)
        {
            asteroid.DrawGame(game);
        }
        
        ship.DrawGame(game);


        foreach (var interstellarObject in interstellarObjects)
        {
            interstellarObject.Draw();
        }
        
    }
    protected override void OnDrawGameUIExample(ScreenInfo gameUi)
    {
        ship.DrawGameUI(gameUi);
    }
    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        
    }

    private void DrawCircleSector()
    {
        var universeLineInfo = new LineDrawingInfo(14f, Colors.Warm, LineCapType.Extended, 0);
        var stripedLineInfo = new LineDrawingInfo(14f, Colors.Warm.SetAlpha(200), LineCapType.None, 0);
        
        ShapeStripedDrawing.DrawStripedRing(Vector2.Zero, SectorRadiusInside, SectorRadiusOutside, 1f, stripedLineInfo, 0f);
        
        ShapeCircleDrawing.DrawCircleLines(Vector2.Zero, SectorRadiusInside, universeLineInfo , 0f, 24f);
        ShapeCircleDrawing.DrawCircleLines(Vector2.Zero, SectorRadiusOutside, universeLineInfo, 0f, 24f);
        
        var textCount = 12;
        var angleStepRad = (360f / textCount) * ShapeMath.DEGTORAD;
        var textRect = new Rect(Vector2.Zero, new Size(SectorRadiusInside * 0.2f, SectorRadiusInside * 0.05f), AnchorPoint.Center);
        for (int i = 0; i < textCount; i++)
        {
            var rotationRad = angleStepRad * i;
            var center = new Vector2(0, -SectorRadiusInside * 1.02f).Rotate(rotationRad);
            textRect = textRect.SetPosition(center, AnchorPoint.Center);
            gameFont.Draw("SECTOR END", textRect, rotationRad * ShapeMath.RADTODEG, AnchorPoint.Center);
        }
    }
}

public static class Vector2Extensions
{
    public static float PositionScaleFactor = 100;
    
    public static nkast.Aether.Physics2D.Common.Vector2 ToAetherVector2(this Vector2 v)
    {
        return new nkast.Aether.Physics2D.Common.Vector2(v.X, v.Y);
    }

    public static Vector2 ToSystemVector2(this nkast.Aether.Physics2D.Common.Vector2 v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static nkast.Aether.Physics2D.Common.Vector2 ScalePositionToAetherVector2(this Vector2 v)
    {
        return v.ToAetherVector2() / PositionScaleFactor;
    }

    public static Vector2 ScalePositionToSystemVector2(this nkast.Aether.Physics2D.Common.Vector2 v)
    {
        return v.ToSystemVector2() * PositionScaleFactor;
    }
}