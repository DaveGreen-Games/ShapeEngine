using System.Numerics;
using Examples.Scenes.ExampleScenes.PhysicsExampleSource;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.StripedDrawingDef;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using ShapeEngine.Text;
using Color = System.Drawing.Color;

namespace Examples.Scenes.ExampleScenes;

public class PhysicsExample : ExampleScene
{
    private readonly Rect sectorRect;
    private readonly ShapeCamera camera;
    private readonly CameraFollowerSingle follower;
    private const float SectorRadiusOutside = 9000;
    private const float SectorRadiusInside = 8250;
    private const int CollisionRows = 10;
    private const int CollisionCols = 10;
   
    private readonly float cellSize;
    
    private readonly List<TextureSurface> starSurfaces = new();
    
    private readonly TextFont gameFont = new(GameloopExamples.Instance.GetFont(FontIDs.JetBrainsLarge), 15f, Colors.Warm);

    private readonly PhysicsExampleSource.Ship ship;
    
    private readonly List<PhysicsExampleSource.Asteroid> asteroids = new();
    private readonly List<PhysicsExampleSource.Asteroid> attractorAsteroids = new();
    private readonly List<PhysicsExampleSource.Asteroid> repulsorAsteroids = new();
    
    

    public PhysicsExample()
    {
        Title = "Physics!";

        var rectSize = SectorRadiusOutside * 2;
        sectorRect = new(new Vector2(0f), new Size(rectSize, rectSize) , new AnchorPoint(0.5f));

        var spatialHash = new BroadphaseSpatialHash(sectorRect, CollisionRows, CollisionCols);
        InitCollisionHandler(spatialHash);
        cellSize = rectSize / CollisionRows;
        
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
                CircleDrawing.DrawCircle(pos, size, color);
            }
            t.EndDraw();
            
            starSurfaces.Add(t);
        }

        
        if (CollisionHandler != null)
        {
            ship = new(65, Colors.PcWarm);
            CollisionHandler.Add(ship);
            
            
            for (int i = 0; i < 100; i++)
            {
                var randPos = Rng.Instance.RandVec2(500, SectorRadiusInside - 250);
                var asteroid = new PhysicsExampleSource.Asteroid(randPos, Colors.PcCold);
                asteroids.Add(asteroid);
                CollisionHandler.Add(asteroid);
                if(asteroid.AsteroidType == AsteroidType.Attractor) attractorAsteroids.Add(asteroid);
                else if(asteroid.AsteroidType == AsteroidType.Repulsor) repulsorAsteroids.Add(asteroid);
            }
        }
        
    }
    
    protected override void OnActivate(Scene oldScene)
    {
        FontDimensions.FontSizeRange = new(10, 500);
        
        Colors.OnColorPaletteChanged += OnColorPaletteChanged;
        
        GameloopExamples.Instance.Camera = camera;
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
        
        GameloopExamples.Instance.ResetCamera();
    }
    private void OnColorPaletteChanged()
    {
        
    }

    public override void Reset()
    {
        
    }
    

    
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        foreach (var asteroid in asteroids)
        {
            asteroid.Update(time, game, gameUi, ui);
            var asteroidOutOfBoundsForce = ShapePhysics.CalculateReverseAttractionForce(
                Vector2.Zero,
                5000 * asteroid.Mass,
                new ValueRange(SectorRadiusInside, SectorRadiusOutside),
                asteroid.Transform.Position
            );
            asteroid.AddForce(asteroidOutOfBoundsForce);
        }
        
        foreach (var attractor in attractorAsteroids)
        {
            ship.ApplyAttraction(attractor.Transform.Position, attractor.AttractionForce, 1.25f);
        }
        
        foreach (var repulsor in repulsorAsteroids)
        {
            ship.ApplyRepulsion(repulsor.Transform.Position, repulsor.RepulsorForce, 1.25f);
        }
        
        ship.Update(time, game, gameUi, ui);
        var shipOutOfBoundsForce = ShapePhysics.CalculateReverseAttractionForce(
            Vector2.Zero,
            5000 * ship.Mass,
            new ValueRange(SectorRadiusInside, SectorRadiusOutside),
            ship.Transform.Position
            );
        ship.AddForce(shipOutOfBoundsForce);
        
        follower.Speed = ship.CurSpeed * 1.5f;
        var speedF = ship.CurSpeed / ship.MaxSpeed;
        var targetZoom = ShapeMath.LerpFloat(0.75f, 0.32f, speedF);
        var curZoom = camera.BaseZoomLevel;
        
        var newZoom = ShapeMath.ExpDecayLerpFloat(curZoom, targetZoom, 0.5f, time.Delta);
        camera.SetZoom(newZoom);

        
        
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
        
        StripedDrawing.DrawStripedRing(Vector2.Zero, SectorRadiusInside, SectorRadiusOutside, 1f, stripedLineInfo, 0f);
        
        CircleDrawing.DrawCircleLines(Vector2.Zero, SectorRadiusInside, universeLineInfo , 0f, 24f);
        CircleDrawing.DrawCircleLines(Vector2.Zero, SectorRadiusOutside, universeLineInfo, 0f, 24f);
        
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