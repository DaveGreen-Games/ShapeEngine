using System.Numerics;
using ShapeCollision;
using ShapeCore;
using ShapePersistent;
using ShapeAudio;
using Raylib_CsLo;
using ShapeLib;
using ShapeColor;
using ShapeAchievements;

namespace ShapeEngineDemo.Bodies
{

    class AsteroidDeathEffect : SquareEffect
    {
        private float duration = 0f;
        private bool second = false;
        public AsteroidDeathEffect(Vector2 pos, float duration, float size, Color color) : base(pos, duration * 0.8f, size, color, 0f)
        {
            this.duration = duration;
        }

        public override void Update(float dt)
        {
            if (IsDead()) return;
            base.Update(dt);
            
            if(lifetimeTimer.IsFinished() && !second)
            {
                lifetimeTimer.Start(duration * 0.2f);
                second = true;
            }
        }

        protected override float GetCurSize()
        {
            if (!second) return size;
            else return SUtils.LerpFloat(size, 0, 1.0f - lifetimeTimer.GetF());
        }
        //public override void Draw()
        //{
        //    if (IsDead()) return;
        //    float curSize = GetCurSize();
        //    Rectangle rect = new(pos.X,pos.Y, curSize*2f, curSize*2f);
        //    DrawRectanglePro(rect, new(curSize, curSize), rotation, color);
        //}
    }
    public class Asteroid : Body
    {
        
        string asteroidType = "small";
        float size = 10f;
        int spawnCount = 0;
        string spawnType = "";
        CircleCollider collider;
        List<Vector2> polygon;
        public Asteroid(Vector2 pos, string asteroidType)
        {
            DrawOrder = 10;
            this.asteroidType = asteroidType;
            collisionMask = new string[] { "asteroid"};
            //var data = DataHandler.asteroidData[this.asteroidType];
            Vector2 vel = new(0f, 0f);
            var data = Demo.DATA.GetCDBContainer().Get<DataObjects.AsteroidData>("asteroids", this.asteroidType);
            if (data != null)
            {
                this.spawnCount = data.spawnCount;
                this.spawnType = data.spawnName;
                this.size = data.size;
                SetTotalHealth(data.health);
                vel = SRNG.randVec2(data.velMin, data.velMax);
            }
            collider = new(pos, vel, this.size);
            collider.Mass = size / 2;
            polygon = SPoly.GeneratePolygon(SRNG.randI(10, 15), new(), this.size * 0.75f, this.size*1.25f);
        }
        public Asteroid(Vector2 pos, Vector2 vel, string asteroidType)
        {
            DrawOrder = 10;
            this.asteroidType = asteroidType;
            collisionMask = new string[] { "asteroid" };
            //var data = DataHandler.asteroidData[this.asteroidType];
            var data = Demo.DATA.GetCDBContainer().Get("asteroids", this.asteroidType) as DataObjects.AsteroidData;
            if (data != null)
            {
                this.spawnCount = data.spawnCount;
                this.spawnType = data.spawnName;
                this.size = data.size;
                SetTotalHealth(data.health);
            }
            collider = new(pos, vel, this.size);
            collider.Mass = size / 2;
            polygon = SPoly.GeneratePolygon(SRNG.randI(10, 15), new(), this.size * 0.75f, this.size * 1.25f);
        }
        public override void Overlap(CollisionInfo info)
        {
            if (info.collision)
            {
                if (info.other != null)
                {
                    string colLayer = info.other.GetCollisionLayer();
                    //if(colLayer == "player")
                    //{
                    //    IDamageable? other = info.other as IDamageable;
                    //    if(other != null)
                    //    {
                    //        other.Damage(GetDamage(), collider.Pos, this);
                    //        //Damage(other.GetDamage(), info.other.GetPos(), other);
                    //    }
                    //}
                    if(colLayer == "asteroid")
                    {
                        var otherCol = info.other.GetCollider();
                        collider.Vel = SPhysics.ElasticCollision2D(collider.Pos, info.selfVel, collider.Mass, otherCol.Pos, info.otherVel, otherCol.Mass, 1f);
                    }
                }
            }
        }

        public override void OnPlayfield(bool inner, bool outer)
        {
            if (inner)
            {
                if (!collider.IsEnabled()) collider.Enable();
            }
            else
            {
                if (collider.IsEnabled()) collider.Disable();
            }
        }
        public override float GetDamage()
        {
            return size * 0.5f;
        }
        protected override void WasKilled()
        {
            SpawnDeathEffect();
            SpawnAsteroids();
            Demo.AUDIO.SFXPlay(SoundIDs.ASTEROID_Die, -1f, -1f, 0.1f);
            Demo.ACHIEVEMENTS.UpdateStatValue("asteroidKills", 1);
        }
        public override void WasDamaged(DamageInfo info)
        {
            base.WasDamaged(info);
            //AudioHandler.PlaySFX("asteroid hurt", -1f, -1f, 0.1f);

            float f = 0.75f + (info.recieved / GetTotalHealth());
            Color particleColor = Demo.PALETTES.C(ColorIDs.Neutral);
            if (info.crit) particleColor = Demo.PALETTES.C(ColorIDs.Flash); f += 0.5f;
            for (int i = 0; i < SRNG.randI(5, 10); i++)
            {
                HitParticle particle = new(info.pos, info.dir, f, 0.5f, particleColor);
                GAMELOOP.AddGameObject(particle);
            }
            //Vector2 pos = ScreenHandler.GAME_TO_UI * (info.pos + RNG.randVec2(size * 0.5f, size));
            Vector2 pos = info.pos + SRNG.randVec2(size * 0.5f, size);
            string text = String.Format("{0}", MathF.Floor(info.recieved));
            //var textEffect = new TextEffectEaseColor(pos, text, 1.0f, WHITE, 0.25f, new(255, 255, 255, 0), EasingType.BOUNCE_OUT);
            //var textEffect = new TextEffectEaseSize(pos, text, 1.0f, WHITE, 1f, -80, EasingType.BACK_IN);
            var textEffect = new TextEffectEaseSize(pos, text, 1f, WHITE, 50, Demo.FONT.GetFont(Demo.FONT_Light), 0, 1, new(0.5f), 1f, EasingType.BACK_IN);
            //var textEffect = new TextEffectEasePos(pos, text, 1.0f, WHITE, 0.5f, new Vector2(0, 150), EasingType.CUBIC_OUT);
            GAMELOOP.AddGameObject(textEffect, true);
        }
        public override Rectangle GetBoundingBox()
        {
            return collider.GetBoundingRect();
        }

        public override void Update(float dt)
        {
            if (IsDead()) return;


            base.Update(dt);
            collider.ApplyAccumulatedForce(dt);
            collider.Pos += collider.Vel * dt;
        }
        public override void Draw()
        {
            SDrawing.DrawPolygon(polygon, 2f, Demo.PALETTES.C(ColorIDs.Neutral), collider.Pos);
            if (DEBUG_DRAWCOLLIDERS)
            {
                if(collider.IsEnabled()) collider.DebugDrawShape(DEBUG_ColliderColor);
                else collider.DebugDrawShape(DEBUG_ColliderDisabledColor);

                DrawRectangleLinesEx(GetBoundingBox(), 1f, GREEN);
            }
        }
        //public override void DrawUI()
        //{
        //    Camera2D c = ScreenHandler.curCamera;
        //    Vector2 cPos = c.target - c.offset;
        //    Vector2 pos = (collider.Pos - cPos) * ScreenHandler.GAME_TO_UI;
        //    Rectangle rect = new(pos.X, pos.Y, size * 4f, size * 0.25f);
        //    DrawRectangleRec(rect, WHITE);
        //}

        public override Collider GetCollider()
        {
            return collider;
        }
        public override string GetCollisionLayer()
        {
            return "asteroid";
        }
        public override string GetID()
        {
            return "asteroid";
        }

        public override Vector2 GetPosition()
        {
            return collider.Pos;
        }
        public override Vector2 GetPos()
        {
            return collider.Pos;
        }
        public override bool CanBeHealed() { return false; }

        private void SpawnDeathEffect()
        {
            AsteroidDeathEffect ade = new(collider.Pos, 0.5f, size*1.25f, Demo.PALETTES.C(ColorIDs.Neutral));
            GAMELOOP.AddGameObject(ade);
        }
        private void SpawnAsteroids()
        {
            if (spawnCount <= 0 || spawnType == "") return;
            Area? area = GAMELOOP.GetCurArea();
            if (area == null) return;

            for (int i = 0; i < spawnCount; i++)
            {
                
                Asteroid a = new(collider.Pos, spawnType);
                area.AddGameObject(a);
                
                
            }
        }
    }
}
