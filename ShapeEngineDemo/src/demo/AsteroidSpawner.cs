using System.Numerics;
using ShapeEngineCore;
using ShapeEngineDemo.Bodies;
using ShapeEngineCore.Globals.Timing;
using ShapeEngineCore.Globals;
using Raylib_CsLo;

namespace ShapeEngineDemo
{
   
    public class AsteroidSpawner
    {
        const float WAVE_BUDGET = 150f;
        const float MIN_DURATION = 3f;
        const float MAX_DURATION = 8f;
        const float MIN_VEL = 5f;
        const float MAX_VEL = 25f;
        Dictionary<string, float> COSTS = new()
        {
            {"small", 25f },
            {"medium", 40f },
            {"large", 60f },
            {"xlarge", 100f }
        };
        //Rectangle targetArea;
        //Vector2 targetAreaCenter;
        //RangeVector2 distanceRange;
        Area area;
        SpawnArea spawnArea;
        float curBudget = 0f;
        RangeFloat durationRange;
        RangeFloat velRange;
        BasicTimer timer = new();

        //ChanceList<string> chances = new((5, "small"), (15, "medium"), (30, "large"), (50, "xlarge"));
        public AsteroidSpawner(Area area, float difficultyFactor = 1f, float maxDisFactor = 2f)
        {
            //this.targetArea = area.GetInnerArea();
            //float minDis = MathF.Sqrt(targetArea.width * targetArea.width + targetArea.height * targetArea.height) * 0.55f;
            //float maxDis = minDis * maxDisFactor;
            //this.distanceRange = new(this.targetAreaCenter, minDis, maxDis);
            //this.targetAreaCenter = area.GetInnerCenter();
            this.area = area;
            this.spawnArea = new(area.GetInnerArea(), area.GetOuterArea());
            this.durationRange = new(MIN_DURATION * difficultyFactor, MAX_DURATION * difficultyFactor);
            this.velRange = new(MIN_VEL * difficultyFactor, MAX_VEL * difficultyFactor);
            this.curBudget = WAVE_BUDGET * difficultyFactor;
        }


        public void Update(float dt)
        {
            timer.Update(dt);
            if (timer.IsFinished())
            {
                SpawnWave();
                Start();
            }
        }
        public void Draw()
        {
            if(DEBUG_DrawHelpers)spawnArea.Debug_DrawSegments();
        }
        public void SpawnWave()
        {
            var picked = PickAsteroids(curBudget);
            foreach (var asteroid in picked)
            {
                SpawnAsteroid(asteroid);
            }
        }
        public void SpawnAsteroid(string name)
        {
            Vector2 pos = spawnArea.Rand();
            Vector2 targetPos = spawnArea.RandInner();
            Vector2 dir = Vec.Normalize(targetPos - pos);
            Vector2 vel = dir * velRange.Rand();
            if (RNG.randF() < 0.05f) vel *= 5f;
            Asteroid a = new(pos, vel, name);
            area.AddGameObject(a, false, "asteroids");
        }

        public void Start()
        {
            timer.Start(durationRange.Rand());
            SpawnWave();
        }
        public void Stop()
        {
            timer.Stop();
        }
        public void Pause()
        {
            timer.Pause();
        }
        public void Resume()
        {
            timer.Resume();
        }

        private List<string> PickAsteroids(float budget)
        {
            List<string> asteroids = new List<string>();
            List<string> keys = COSTS.Keys.ToList();
            while(budget > 0)
            {
                var filtered = keys.FindAll(k => COSTS[k] <= budget);
                if (filtered.Count <= 0) { budget = 0; continue; }
                var key = RNG.randCollection(filtered);
                //var key = chances.Next(a => COSTS[a] <= budget);
                if (key != null && COSTS.ContainsKey(key))
                {
                    float cost = COSTS[key];
                    budget -= cost;
                    asteroids.Add(key);
                }
            }

            return asteroids;
        }
    }
}
