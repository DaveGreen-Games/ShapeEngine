using System.Numerics;
using System.Xml.Serialization;
using ShapeEngine.Content;
using ShapeEngine.Core;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using ShapeEngine.Serialization;
using ShapeEngine.Text;

namespace Examples.Scenes.ExampleScenes;


public class XmlDataExample : ExampleScene
{
    private class ImpactSmoke
    {
        private float duration;
        private float timer;
        private Vector2 pos;
        private float size;
        
        public ImpactSmoke(Vector2 pos, float size, float duration)
        {
            this.pos = pos;
            this.size = size;
            this.timer = duration;
            this.duration = duration;
        }

        public bool IsFinished => timer <= 0f;
        public void Update(float dt)
        {
            if (IsFinished) return;

            timer -= dt;
        }

        public void Draw()
        {
            var f = 1f - (timer / duration);

            var startC = Colors.Warm;
            var endC = Colors.Medium.SetAlpha(150);
            var c = startC.Lerp(endC, f);
            CircleDrawing.DrawCircle(pos, ShapeMath.LerpFloat(size * 0.5f, size, f), c, 24);
        }
    }
    private class Planet
    {
        public Circle Shape;
        private readonly List<Circle> impacts = new();
        private readonly List<ImpactSmoke> impactSmokes = new();
        private readonly float rotSpeedRad;
        public Planet(Vector2 center, float radius)
        {
            Shape = new(center, radius);
            rotSpeedRad = Rng.Instance.RandF(-25, 25) * ShapeMath.DEGTORAD;
        }

        public void Impact(Vector2 pos, float r)
        {
            var w = pos - Shape.Center;
            // if (w.LengthSquared() + (r * r) > Shape.Radius * Shape.Radius)
            // {
            //     pos = Shape.Center + w.Normalize() * (Shape.Radius - r);
            // }
            var rRange = Shape.Radius - r;
            var impactPos = Shape.Center + w.Normalize() * Rng.Instance.RandF(-rRange, rRange);
            var impact = new Circle(impactPos, r);
            impacts.Add(impact);

            var impactSmoke = new ImpactSmoke(impactPos, r * 4, Rng.Instance.RandF(0.25f, 1f));
            impactSmokes.Add(impactSmoke);
        }
        public void Update(float dt)
        {
            for (int i = 0; i < impacts.Count; i++)
            {
                impacts[i] = impacts[i].ChangeRotation(rotSpeedRad * dt, Shape.Center);
            }
            for (int i = impactSmokes.Count - 1; i >= 0; i--)
            {
                impactSmokes[i].Update(dt);
                if(impactSmokes[i].IsFinished) impactSmokes.RemoveAt(i);
            }
        }

        public void Draw()
        {
            Shape.Center.Draw(Shape.Radius, Colors.Cold, 36);
            // ShapeDrawing.DrawCircleLines(Shape.Center, Shape.Radius * 1.25f, Shape.Radius * 0.1f, Colors.Cold);

            foreach (var impact in impacts)
            {
                impact.Draw(Colors.Warm, 12);
            }
            foreach (var impactSmoke in impactSmokes)
            {
                impactSmoke.Draw();
            }
            Shape.DrawLines(Shape.Radius / 12, Colors.Light);
        }

        public void DrawGameUI(TextFont textFont)
        {
            if (impacts.Count <= 0) return;
            var text = $"{impacts.Count}";

            var r = Shape.GetBoundingBox();
            r = r.ChangePosition(new Vector2(Shape.Radius * 2.5f, 0f));
            textFont.ColorRgba = Colors.Highlight;
            textFont.DrawWord(text, r, new AnchorPoint(0.5f));
        }
    }
   
    
    public record AsteroidData
    {
        [XmlElement("Name")] 
        public required string Name { get; set; }
   
        [XmlElement("Speed")]
        public float Speed { get; set; }
   
        [XmlElement("Size")]
        public float Size { get; set; }
   
        [XmlElement("Damage")]
        public float Damage { get; set; }
   
        [XmlElement("SpawnWeight")]
        public int SpawnWeight { get; set; }
   
        [XmlElement("ParticleChances")]
        public required List<ParticleChance> ParticleChances { get; set; }
    }
    public enum ParticleType
    {
        Common,
        Rare,
        Legendary
    }
    public class ParticleChance
    {
        [XmlElement("Type")]
        public ParticleType Type { get; set; }
   
        [XmlElement("Chance")]
        public float Chance { get; set; }
    }
    
    
    private class AsteroidSpawner
    {
        private readonly ChanceList<AsteroidData> SpawnList;

        public AsteroidSpawner(List<AsteroidData> asteroidData)
        {
            var spawnEntries = new List<(int amount, AsteroidData data)>();
            foreach (var data in asteroidData)
            {
                spawnEntries.Add((data.SpawnWeight, data));
            }
            SpawnList = new(spawnEntries);
        }
        public Asteroid Create(Vector2 center, float radius, Planet target)
        {
            var randPos = center + Rng.Instance.RandVec2(radius * 0.95f, radius * 1.05f);
            return new Asteroid(randPos, target, SpawnList.Next());
        }
    }
    private class Asteroid
    {
        private AsteroidData data;
        private Vector2 position;
        private Vector2 velocity;
        private Circle shape;
        private Planet target;
        private bool dead = false;
        public bool IsDead => dead;
        public float DistanceToTargetSq => (target.Shape.Center - position).LengthSquared();
        public Asteroid(Vector2 pos, Planet target, AsteroidData data)
        {
            this.data = data;
            this.position = pos;
            var targetPos = target.Shape.Center + Rng.Instance.RandVec2(0f, target.Shape.Radius * 2);
            var w = targetPos - pos;
            velocity = w.Normalize() * data.Speed;
            shape = new(position, data.Size);
            this.target = target;
        }

        public void Update(float dt)
        {
            if (dead) return;
            
            position += velocity * dt;
            shape = new(position, data.Size);
            if (target.Shape.ContainsShape(shape))
            {
                dead = true;
                velocity = new();
                target.Impact(position, data.Size);
            }
        }

        public void Draw()
        {
            shape.Draw(Colors.Warm);
        }
    }
    
    private const float CameraBaseZoom = 0.45f;
    private const float AsteroidSpawnRadius = 2350f;
    private const float AsteroidSpawnInterval = 0.5f;
    
    private Planet planet;
    private readonly ShapeCamera camera;
    private readonly List<Asteroid> asteroids = new();
    private float asteroidSpawnTimer = 0f;
    private AsteroidSpawner asteroidSpawner;
    private XmlClassSerializer<AsteroidData> serializer;

    private DirectoryInfo? externalDataSavePath;
    
    public XmlDataExample()
    {
        Title = "Xml Data Example";
        camera = new();
        serializer = new XmlClassSerializer<AsteroidData>();
        
        var saveDirectoryPath = Game.Instance.SaveDirectoryPath;
        var saveFolder = "XmlDataExamples";
        var fullPath = Path.Combine(saveDirectoryPath, saveFolder);
        
        externalDataSavePath = ShapeFileManager.CreateDirectory(fullPath);
        
        CreateDefaultSavegameXmlData();
        var data = LoadXmlData();
        asteroidSpawner = new(data);
        CreatePlanet();
    }

    private void CreateDefaultSavegameXmlData()
    {
        if (externalDataSavePath == null) return;

        if (!ShapeFileManager.FileExists(externalDataSavePath, "slowAsteroid.xml"))
        {
            var slowAsteroid = new AsteroidData()
            {
                Name = "Slow Asteroid",
                Speed = 5,
                Size = 5f,
                Damage = 1f,
                SpawnWeight = 5,
                ParticleChances =
                [
                    new() { Type = ParticleType.Common, Chance = 0.5f },
                    new() { Type = ParticleType.Rare, Chance = 0.1f },
                    new() { Type = ParticleType.Legendary, Chance = 0.01f }
                ]
            };
            var xml = serializer.Serialize(slowAsteroid);
            xml += "\n<!-- ParticaleTypes: Common, Rare, Legendary -->\n";
            externalDataSavePath.SaveText("slowAsteroid.xml", xml, null, false ,false);
        }
    }
    private List<AsteroidData> LoadXmlData()
    {
        //load source
        //TODO: functions for loading all files in directory
        var small = ContentLoader.LoadText("Resources/XmlDataExampleSource/AsteroidSmall.xml");
        var medium = ContentLoader.LoadText("Resources/XmlDataExampleSource/AsteroidMedium.xml");
        var large = ContentLoader.LoadText("Resources/XmlDataExampleSource/AsteroidLarge.xml");
        var fast = ContentLoader.LoadText("Resources/XmlDataExampleSource/AsteroidFast.xml");
        
        //load external
        
        //combine source and external strings

        //deserialize all strings to data objects
        
        //setup asteroid spawner
        return new();
    }
    
    
    protected override void OnActivate(Scene oldScene)
    {
        GameloopExamples.Instance.Camera = camera;
        camera.SetZoom(CameraBaseZoom);
    }

    protected override void OnDeactivate()
    {
        GameloopExamples.Instance.ResetCamera();
    }
    public override void Reset()
    {
        camera.SetZoom(CameraBaseZoom);
        CreatePlanet();
        asteroids.Clear();
    }
    
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
    {
        if (asteroidSpawnTimer > 0f)
        {
            asteroidSpawnTimer -= time.Delta;
            if (asteroidSpawnTimer <= 0f)
            {
                var nextAsteroid = asteroidSpawner.Create(planet.Shape.Center, AsteroidSpawnRadius, planet);
                asteroids.Add(nextAsteroid);
                asteroidSpawnTimer = AsteroidSpawnInterval;
            }
        }
        else asteroidSpawnTimer = AsteroidSpawnInterval;
        
        
        planet.Update(time.Delta);
        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var asteroid = asteroids[i];
            asteroid.Update(time.Delta);
            if (asteroid.IsDead || asteroid.DistanceToTargetSq > (AsteroidSpawnRadius * AsteroidSpawnRadius) * 1.25f)
            {
                asteroids.RemoveAt(i);
            }
        }
    }
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        
        CircleDrawing.DrawCircleLines(planet.Shape.Center, AsteroidSpawnRadius, 12f, Colors.Highlight, 4f);
        
        planet.Draw();
        planet.DrawGameUI(textFont);
        foreach (var asteroid in asteroids)
        {
            asteroid.Draw();
        }
    }
    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        var r = GameloopExamples.Instance.UIRects.GetRect("bottom center");
        DrawDescription(r);
    }

    protected override void OnDrawGameUIExample(ScreenInfo gameUi)
    {
        
    }

    private void DrawDescription(Rect rect)
    {
        var split = rect.SplitV(0.6f);
        
        textFont.FontSpacing = 1f;
        textFont.ColorRgba = Colors.Medium;
        textFont.DrawTextWrapNone("There is nothing to do", split.top, new(0.5f));
        textFont.DrawTextWrapNone("Asteroids are randomly spawned based on data structs.", split.bottom, new(0.5f));
        
    }
    private void CreatePlanet()
    {
        planet = new(new(), Rng.Instance.RandF(112, 128));
    }
}