using System.Numerics;
using ShapePersistent;
using ShapeEngineDemo.DataObjects;
using ShapeEngineDemo.Projectiles;
using ShapeEngineDemo.Bodies;
using ShapeAudio;
using Raylib_CsLo;
using ShapeCore;
using ShapeColor;
using ShapeLib;

namespace ShapeEngineDemo.Guns
{
    public class Gun
    {

        const float alignedTolerance = 0.8f;

        protected IDamageable dmgDealer;
        protected Color color = WHITE;
        protected Vector2 pos = new();
        protected float rotRad = 0f;
        protected float aimOffsetRad = 0f;

        protected string gunName = "";
        protected string bulletName = "";

        //data values
        protected bool semiAuto = false;
        protected float speedVariation = 0f;
        protected int soundID = -1;
        protected string effect = "";
        protected float timer = 0f;
        protected float burstTimer = 0f;
        protected int burstsLeft = 0;
        protected bool locked = false;


        public StatHandler stats = new(
            ("ammoCost", 0f), ("acc", 0f), ("firerate", 0f), ("burstFirerate", 0f), ("burstCount", 0f), ("pellets", 0f),
            ("dmg", 0f), ("critChance", 0f), ("critBonus", 0f), ("lifetime", 0f), ("speed", 0f)
            );
        protected string[] projectileStats = new string[] { "dmg", "critChance", "critBonus", "lifetime", "speed" };
        //public StatHandler projectileStats = new(); // only bonuses & flats are used

        public delegate void Fired(float ammoCost);
        public event Fired? OnGunFired;


        public Gun(Vector2 pos, float angle, string gun, Color color, IDamageable dmgDealer)
        {
            this.pos = pos;
            this.rotRad = angle;
            this.gunName = gun;
            this.dmgDealer = dmgDealer;
            this.color = color;
            //stats.OnStatChanged += StatChanged;
            LoadData();
        }
        public Gun(Vector2 pos, Vector2 dir, string gun, Color color, IDamageable dmgDealer)
        {
            this.pos = pos;
            this.rotRad = SVec.AngleRad(dir);
            this.gunName = gun;
            this.dmgDealer = dmgDealer;
            this.color = color;
            LoadData();
        }
        public Gun(string gun, Color color, IDamageable dmgDealer)
        {
            this.gunName = gun;
            this.dmgDealer = dmgDealer;
            this.color = color;
            LoadData();
        }

        //protected virtual void StatChanged(string statName) { }
        private void LoadData()
        {
            var data = Demo.DATA.GetCDBContainer().Get<GunData>("guns", gunName);
            if (data == null) return;
            bulletName = data.bullet;
            stats.SetStat("acc", data.accuracy * DEG2RAD);
            stats.SetStat("firerate", data.bps);
            stats.SetStat("pellets", data.pellets);
            stats.SetStat("burstCount", data.burstCount);
            stats.SetStat("burstFirerate", 1f / data.burstDelay);
            stats.SetStat("ammoCost", data.ammoCost);
            soundID = SoundIDs.PROJECTILE_Shoot; // data.sound;
            effect = data.effect;
            semiAuto = data.semiAuto;
            speedVariation = data.speedVar;
        }
        
        public bool IsAligned(Vector2 targetPos)
        {
            return IsAlignedDir(SVec.Normalize(targetPos - this.pos));
            //Vector2 dir = targetPos - this.pos;
            //float dot = Vec.Dot(dir, Vec.Rotate(Vec.Right(), rotRad));
            //return dot > 0.6f;
        }
        public bool IsAligned(float targetAngleRad)
        {
            return IsAlignedDir(SVec.Rotate(SVec.Right(), targetAngleRad));

            //float dot = Vec.Dot(Vec.Rotate(Vec.Right(), targetAngleRad), Vec.Rotate(Vec.Right(), rotRad));
            //return dot > 0.6f;
        }
        public bool IsAlignedDir(Vector2 targetDir)
        {
            float dot = SVec.Dot(targetDir, SVec.Rotate(SVec.Right(), rotRad));
            return dot > alignedTolerance;
        }
        
        public Vector2 GetPosition() { return pos; }
        public float GetRotationRad() { return rotRad; }
        public void SetPos(Vector2 newPos) { this.pos = newPos; }
        public void SetRotRad(float newRot) { this.rotRad = SUtils.WrapAngleRad(newRot); }
        public void SetRotDeg(float newRot) { this.rotRad = SUtils.WrapAngleRad(newRot * DEG2RAD); }
        //public void RotateBy(float amountRad, float lowerLimitRad, float upperLimitRad)
        //{
        //    SetRotRad(Clamp(rotRad + amountRad, lowerLimitRad, upperLimitRad));
        //}
        public void RotateBy(float amountRad)
        {
            SetRotRad(rotRad + amountRad);
        }
        public void SetAimOffsetRad(float value) { this.aimOffsetRad = value; }
        public void SetAimOffsetDeg(float value) { this.aimOffsetRad = value * DEG2RAD; }
        public bool CanShoot() { return timer <= 0f && burstTimer <= 0f && !locked; }
        public bool IsBurst() { return (int)stats.Get("burstCount") > 1 && stats.Get("burstFirerate") > 0f; }
        
        public void Update(float dt)
        {
            stats.Update(dt);

            if(timer > 0f)
            {
                timer -= dt;
                if(timer <= 0f)
                {
                    timer = 0f;
                }
            }

            if (burstTimer > 0f)
            {
                burstTimer -= dt;
                if(burstTimer <= 0f)
                {
                    if(burstsLeft > 0)
                    {
                        burstTimer = 1f / stats.Get("burstFirerate");
                        burstsLeft--;
                        SpawnBullets((int)stats.Get("pellets"));
                    }
                    else
                    {
                        burstTimer = 0f;
                        burstsLeft = 0;
                        timer = 1f / stats.Get("firerate");
                    }
                }
            }
        }
        
        public virtual void ReleaseTrigger()
        {
            if (semiAuto && locked) locked = false;
        }
        public virtual void Shoot()
        {
            if(!CanShoot()) return;
            if(IsBurst())
            {
                burstTimer = 1f / stats.Get("burstFirerate");
                burstsLeft = (int)stats.Get("burstCount");
            }
            else
            {
                timer = 1f / stats.Get("firerate");
            }
            if (semiAuto) locked = true;
            SpawnBullets((int)stats.Get("pellets"));
        }
        public void Shoot(float angleRad)
        {
            if (!CanShoot()) return;
            SetRotRad(angleRad);
            Shoot();
        }
        public void Shoot(Vector2 pos, float angleRad)
        {
            if (!CanShoot()) return;
            SetRotRad(angleRad);
            SetPos(pos);
            Shoot();
        }
        public void Shoot(Vector2 dir)
        {
            if (!CanShoot()) return;
            SetRotRad(SVec.AngleRad(dir));
            Shoot();
        }
        public void Shoot(Vector2 pos, Vector2 dir)
        {
            if (!CanShoot()) return;
            SetPos(pos);
            SetRotRad(SVec.AngleRad(dir));
            Shoot();
        }

        private void SpawnBullets(int amount)
        {
            if(amount <= 0) return;

            for (int i = 0; i < amount; i++)
            {
                float acc = stats.Get("acc");
                Vector2 dir = SVec.Rotate(SVec.Right(), rotRad + aimOffsetRad + SRNG.randF(-acc, acc));
                ProjectileInfo pi = new(pos, dir, color, dmgDealer, speedVariation);
                Projectile projectile;
                
                switch (bulletName)
                {
                    case "bullet": projectile = new Bullet(pi, stats.GetAllStats(projectileStats)); break;
                    case "piercer": projectile = new Piercer(pi, stats.GetAllStats(projectileStats)); break;
                    case "bouncer": projectile = new Bouncer(pi, stats.GetAllStats(projectileStats)); break;

                    default: projectile = new Bullet(pi, stats.GetAllStats(projectileStats)); break;
                }

                GAMELOOP.AddGameObject(projectile);
            }
            Demo.AUDIO.SFXPlay(soundID, -1f, -1f, 0.1f);
            SpawnEffect();
            SpawnCasing();
            OnGunFired?.Invoke(stats.Get("ammoCost"));
        }
        private void SpawnEffect()
        {
            if (effect == "") return;
            Effect fx;
            //var effect = new LineEffect(pos, 0.4f, 30f, 4f, ColorPalette.Cur.player, angle * RAD2DEG + RNG.randF(-15f, 15), 0f);
            switch (effect)
            {
                case "circle":
                    fx = new CircleEffect(pos + SVec.Rotate(SVec.Right(), rotRad) * 10f, SRNG.randF(0.15f, 0.2f), SRNG.randF(7f, 10f), Demo.PALETTES.C(ColorIDs.Flash));
                    break;
                default: 
                    fx = new CircleEffect(pos + SVec.Rotate(SVec.Right(), rotRad) * 10f, SRNG.randF(0.15f, 0.2f), SRNG.randF(7f, 10f), Demo.PALETTES.C(ColorIDs.Flash));
                    break;
            }
            GAMELOOP.AddGameObject(fx);
        }
    
        private void SpawnCasing()
        {
            Vector2 dir = SVec.Rotate(SVec.Right(), rotRad + PI / 2);
            var casing = new CasingParticle(pos, dir, 1.5f, color, 1f, 1f);
            GAMELOOP.AddGameObject(casing);
        }
    }
}
