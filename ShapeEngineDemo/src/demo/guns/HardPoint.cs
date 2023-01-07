using System.Numerics;
using Raylib_CsLo;
using ShapeEngineDemo.Bodies;
using ShapeCore;
using ShapeColor;
using ShapeLib;

namespace ShapeEngineDemo.Guns
{
    public class Hardpoint
    {
        public enum HardpointType
        {
            FIXED = 0,
            TURRET = 1
        }
        public enum HardpointSize
        {
            SMALL = 0,
            MEDIUM = 1,
            LARGE = 2
        }
        


        protected Gun? gun = null;
        protected HardpointType type;
        protected HardpointSize size;
        protected StatHandler stats = new(("rotSpeed", 200f));

        protected Vector2 offset;
        protected Vector2 pos;
        protected float rotRad = 0f;
        protected float rotOffsetRad = 0f;
        protected Color color = WHITE;
        protected Color offColor = WHITE;

        protected TargetFinder? targetFinder;
        protected Core? energyCore;
        protected bool energySaving = false;
        //protected bool enabled = false;

        public Hardpoint(Vector2 offset, Color color, Color offColor, float rotOffsetRad, float targetingRange, HardpointType type = HardpointType.FIXED, HardpointSize size = HardpointSize.SMALL)
        {
            this.type = type;
            this.size = size;
            this.color = color;
            this.offColor = offColor;
            this.offset = offset;
            this.rotOffsetRad = rotOffsetRad;
            this.rotRad = rotOffsetRad;

            stats.SetBase("targetingRange", targetingRange);

            //if (IsTurret()) targetFinder = new TargetFinder(turretMode, new string[] { "asteroid" });
        }
        public Hardpoint(Vector2 offset, Color color, Color offColor, float rotOffsetRad, float targetingRange, Core energyCore, TargetFinder? targetFinder = null, HardpointType type = HardpointType.FIXED, HardpointSize size = HardpointSize.SMALL)
        {
            this.type = type;
            this.size = size;
            this.color = color;
            this.offColor = offColor;
            this.offset = offset;
            this.rotOffsetRad = rotOffsetRad;
            this.rotRad = rotOffsetRad;
            this.energyCore = energyCore;
            this.targetFinder = targetFinder;

            stats.SetBase("targetingRange", targetingRange);

            //if (IsTurret()) targetFinder = new TargetFinder(turretMode, new string[] { "asteroid" });
        }
        public void SetGun(Gun? newGun)
        { 
            this.gun = newGun;
            if (HasGun())
            {
                //enabled = true;
                this.gun.SetAimOffsetRad(rotOffsetRad);
                //if (targetFinder != null) targetFinder.Start();
                SetTypeBonuses();
                SetSizeBonuses();
                if (HasEnergyCore())
                {
                    gun.OnGunFired += GunFired;
                }
            }
            else
            {
                //enabled = false;
                //if (targetFinder != null) targetFinder.Stop();
                stats.Reset();
                if (HasEnergyCore())
                {
                    gun.OnGunFired -= GunFired;
                }
            }
        }
        public void SetGun(string gunName, IDamageable dmgDealer)
        {
            this.gun = new(gunName, color, dmgDealer);
            this.gun.SetAimOffsetRad(rotOffsetRad);
            //enabled = true;
            //if (targetFinder != null) targetFinder.Start();
            SetTypeBonuses();
            SetSizeBonuses();
            if (HasEnergyCore())
            {
                gun.OnGunFired += GunFired;
            }
        }

        
        //public void Toggle()
        //{
        //    enabled = !enabled;
        //    if(targetFinder != null)
        //    {
        //        targetFinder.Toggle();
        //    }
        //}
        
        public void Shoot()
        {
            //if (!enabled) return;
            if (!HasGun()) return;
            //if (!HasEnergy()) return;
            gun.Shoot();
        }
        public void ReleaseTrigger()
        {
            //if (!enabled) return;
            if (!HasGun()) return;
            gun.ReleaseTrigger();
        }
        
        public void Update(float dt)
        {
            if (HasGun())
            {
                if (HasEnergyCore())
                {
                    if(!HasEnergy() && energySaving == false)
                    {
                        energySaving = true;
                        gun.stats.AddBonus("firerate", -0.5f);
                        gun.stats.AddBonus("dmg", -0.25f);
                    }
                    else if(HasEnergy() && energySaving == true)
                    {
                        energySaving = false;
                        gun.stats.RemoveBonus("firerate", -0.5f);
                        gun.stats.RemoveBonus("dmg", -0.25f);
                    }
                }

                gun.SetPos(pos);

                if (IsTurret())
                {
                    if(targetFinder != null)
                    {
                        
                        if (targetFinder.HasTarget())
                        {
                            var target = targetFinder.GetTarget();
                            AimAt(target.GetPos(), dt);
                            if (HasEnergy() && gun.IsAligned(target.GetPos()))
                            {
                                gun.Shoot();
                                gun.ReleaseTrigger();
                            }
                        }
                    }
                }
                else
                {
                    if (!CanRotate()) gun.SetRotRad(rotRad);
                }
                
                gun.Update(dt);
            }
        }
        public void Draw()
        {
            float drawSize = 2f;
            if (this.size == HardpointSize.MEDIUM) drawSize = 3f;
            else if (this.size == HardpointSize.LARGE) drawSize = 4f;

            Color drawColor = color;
            if (HasEnergyCore() && !HasEnergy()) drawColor = offColor;
           
            if(this.type == HardpointType.FIXED)
            {
                Vector2 s = new Vector2(drawSize, drawSize);
                DrawRectanglePro(new(pos.X, pos.Y, drawSize, drawSize), s * 0.5f, rotRad, drawColor);
            }
            else
            {
                DrawCircleV(pos, drawSize, drawColor);

                if (targetFinder != null && targetFinder.HasTarget())
                {
                    var target = targetFinder.GetTarget();
                    Color targetColor = SColor.ChangeAlpha(PaletteHandler.C("enemy"), 125);
                    Vector2 targetPos = target.GetPos() + SRNG.randVec2(0f, 3f);
                    Vector2 size = new(5f, 5f);
                    var rect = new Rectangle(targetPos.X - size.X / 2, targetPos.Y - size.Y / 2, size.X, size.Y);
                    DrawRectangleLinesEx(rect, 1f, PaletteHandler.C("enemy"));
                    DrawLineEx(pos, targetPos, 1f, PaletteHandler.C("enemy"));
                }
            }
        }
        
        public bool HasEnergy() 
        {
            if (!HasEnergyCore()) return true;
            return !energyCore.IsEmpty(); 
        }
        public bool HasEnergyCore() { return energyCore != null; }
        public bool HasGun() { return gun != null; }
        public bool CanRotate() { return IsTurret(); }
        public bool IsTurret() { return type == HardpointType.TURRET; }
        public bool IsFixed() { return type == HardpointType.FIXED; }
        public HardpointType GetHardpointType() { return type; }
        public void UpdateSpacing(Vector2 pos, float angle, float sizeFactor = 1f) 
        {
            this.rotRad = angle;// + rotOffsetRad;
            this.pos = pos + SVec.Rotate(offset * sizeFactor, angle) + SRNG.randVec2(0.5f, 1f);
        }
        //public void SetAimAngle(float angleRad) { if (!HasGun()) return; gun.SetRotRad(angleRad); }
        public void Aim(float dt, int dir)
        {
            if (!HasGun()) return;
            if (!CanRotate()) return;
            
            //if (rotLimiterRad > 0)
            //{
            //    gun.RotateBy(stats.Get("rotSpeed") * DEG2RAD * dt * dir, rotRad - rotLimiterRad, rotRad + rotLimiterRad);
            //}
            gun.RotateBy(stats.Get("rotSpeed") * DEG2RAD * dt * dir);
        }
        public void AimAt(Vector2 pos, float dt)
        {
            if (!HasGun()) return;
            if (!CanRotate()) return;
            AimAt(SVec.AngleRad(pos - this.pos), dt);
        }
        public void AimAt(float targetAngleRad, float dt)
        {
            if(!HasGun()) return;
            if (!CanRotate()) return;
            float dif = SUtils.GetShortestAngleRad(gun.GetRotationRad(), targetAngleRad);
            float amount = MathF.Min(stats.Get("rotSpeed") * DEG2RAD * dt, MathF.Abs(dif));
            float dir = 1;
            if (dif < 0) dir = -1;
            else if (dir == 0) dir = 0;
            //if(rotLimiterRad > 0)
            //{
            //    gun.RotateBy(dir * amount, rotRad - rotLimiterRad, rotRad + rotLimiterRad);
            //}
            //float prevRot = gun.GetRotationRad();
            gun.RotateBy(dir * amount);
            //if (!gun.IsAligned(rotRad))
            //{
            //    gun.SetRotRad(prevRot);
            //}
        }

        public void DrawDebugDirection(float length, float width)
        {
            if (!HasGun()) return;
            DrawLineEx(gun.GetPosition(), gun.GetPosition() + SVec.Rotate(SVec.Right(), gun.GetRotationRad()) * length, width, WHITE);
        }

        private void GunFired(float ammoCost)
        {
            if (!HasEnergyCore()) return;
            if (IsTurret()) return;
            energyCore.Use(ammoCost);
        }


        private void SetSizeBonuses()
        {
            switch (this.size)
            {
                case HardpointSize.SMALL:
                    stats.AddBonus("rotSpeed", 0.5f);

                    break;
                case HardpointSize.MEDIUM:
                    gun.stats.AddBonus("ammoCost", 0.1f);
                    gun.stats.AddBonus("dmg", 0.05f);
                    gun.stats.AddBonus("lifetime", 0.05f);
                    break;
                case HardpointSize.LARGE:
                    gun.stats.AddBonus("ammoCost", 0.25f);
                    gun.stats.AddBonus("dmg", 0.15f);
                    gun.stats.AddBonus("lifetime", 0.15f);
                    
                    stats.AddBonus("rotSpeed", -0.5f);
                    break;
                default:
                    break;
            }
            
        }
        private void SetTypeBonuses()
        {
            switch (this.type)
            {
                case HardpointType.FIXED:
                    gun.stats.AddBonus("dmg", 0.2f);
                    gun.stats.AddBonus("acc", 0.1f);
                    break;
                case HardpointType.TURRET:
                    gun.stats.AddBonus("ammoCost", -0.5f);
                    gun.stats.AddBonus("firerate", -0.1f);
                    gun.stats.AddBonus("dmg", -0.1f);
                    break;
                default:
                    break;
            }
        }
    
    }
}
