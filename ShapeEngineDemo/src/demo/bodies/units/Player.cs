using System.Numerics;
using ShapeEngineCore;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Input;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals.Timing;
using ShapeEngineCore.Globals.Audio;
using ShapeEngineCore.Globals.UI;
using ShapeEngineCore.Globals.Persistent;
using ShapeEngineCore.SimpleCollision;
using Raylib_CsLo;
using ShapeEngineDemo.Guns;
using ShapeEngineDemo.DataObjects;
//using ShapeEngineDemo.Projectiles;



namespace ShapeEngineDemo.Bodies
{
    public struct ArmoryInfo
    {
        public string fixedGun = "";
        public string turretGun = "";
        public string engine = "";

        public ArmoryInfo(string fixedGun, string turretGun, string engine)
        {
            this.fixedGun = fixedGun;
            this.turretGun = turretGun;
            this.engine = engine;
        }


    }


    public class Pin
    {
        private bool active = false;
        private Vector2 location = new();
        private float cooldown = 0f;
        private float duration = 0f;
        private BasicTimer timer = new();
        private bool drawFlag = true;
        public Pin(float duration, float cooldown, bool drawFlag = true)
        {
            this.duration = duration;
            this.cooldown = cooldown;
            this.drawFlag = drawFlag;
        }

        public float F { get { return 1.0f - timer.GetF(); } }
        public bool IsActive() { return active; }
        public bool IsCooldownActive() { return !IsActive() && timer.IsRunning(); }
        public Vector2 Location { get { return location; } }
        public void Drop(Vector2 pos)
        {
            if (IsActive()) return;
            if (IsCooldownActive()) return;
            active = true;
            timer.Start(duration);
            location = pos;
        }
        public void Clear()
        {
            if (IsActive())
            {
                active = false;
                timer.Start(cooldown * F);
            }
        }
        public void Update(float dt)
        {
            timer.Update(dt);
            if (timer.IsFinished())
            {
                if (IsActive())
                {
                    active = false;
                    timer.Start(cooldown);
                }
            }
        }
        public void Draw()
        {
            if (IsActive() && drawFlag)
            {
                Color color = PaletteHandler.C("special1");
                Vector2 endpoint = location + new Vector2(RNG.randF(-1, 2), -8);
                DrawLineEx(location, endpoint, 1, color);
                Vector2 size = new(6f, 4f);
                Rectangle rect = new(endpoint.X, endpoint.Y - size.Y, size.X, size.Y);
                DrawRectangleLinesEx(rect, 1f, color);
            }
        }
    }
    public class WeaponSystem
    {
        private List<Hardpoint> hardpoints = new();
        private string fixedInput = "";
        //private Pin turretPin;
        private Pin aimpointPin;
        public WeaponSystem(List<Hardpoint> hardpoints, float pinDuration, float pinCooldown, string fixedInput)
        {
            this.hardpoints = hardpoints;
            this.fixedInput = fixedInput;
            //turretPin = new(pinDuration, pinCooldown, false);
            aimpointPin = new(pinDuration, pinCooldown, true);
        }

        public bool IsAimPointActive() { return aimpointPin.IsActive(); }
        public bool IsAimPointCooldownActive() { return aimpointPin.IsCooldownActive(); }
        public Vector2 AimPoint { get { return aimpointPin.Location; } }
        public float F { get { return aimpointPin.F; } }
        //public bool AreTurretsDropped() { return turretPin.IsActive(); }
        //public bool IsTurretDropPointCooldownActive() { return turretPin.IsCooldownActive(); }
        //public Vector2 DropPoint { get { return turretPin.Location; } }
        public void Update(float dt)
        {
            aimpointPin.Update(dt);
            //turretPin.Update(dt);
            foreach (var hardpoint in hardpoints)
            {
                hardpoint.Update(dt);
            }
            CheckInput();
        }
        public void UpdateSpacing(Vector2 pos, float angle, float sizeFactor = 1f)
        {
            foreach (var hardpoint in hardpoints)
            {
                hardpoint.UpdateSpacing(pos, angle, sizeFactor);
            }
        }
        public void DropAimPoint(Vector2 pos)
        {
            if (IsAimPointCooldownActive()) return;
            //if (AreTurretsDropped()) return;

            if (IsAimPointActive())
            {
                ClearAimPoint();
            }
            else
            {
                aimpointPin.Drop(pos);
            }
        }
        public void ClearAimPoint()
        {
            aimpointPin.Clear();
        }
        //public void DropTurrets(Vector2 pos)
        //{
        //    if (IsTurretDropPointCooldownActive()) return;
        //    if (AreTurretsDropped()) ReturnTurrets();
        //    else
        //    {
        //        if (IsAimPointActive()) ClearAimPoint();
        //        turretPin.Drop(pos);
        //    }
        //}
        //public void ReturnTurrets()
        //{
        //    turretPin.Clear();
        //}
        private void CheckInput()
        {
            if (InputHandler.IsDown(0, fixedInput))
            {
                Shoot();
            }
            else if (InputHandler.IsReleased(0, fixedInput))
            {
                ReleaseTrigger();
            }
        }
        public void Draw()
        {
            aimpointPin.Draw();
            //turretPin.Draw();
            foreach (var hardpoint in hardpoints)
            {
                hardpoint.Draw();
            }
        }


        private void Shoot()
        {
            foreach (var hp in hardpoints)
            {
                if (hp.IsFixed()) hp.Shoot();
            }
        }
        private void ReleaseTrigger()
        {
            foreach (var hp in hardpoints)
            {
                if (hp.IsFixed()) hp.ReleaseTrigger();
            }
        }
    }
    public class Player : Unit
    {
        //private Camera2D camera = new() { rotation = 0f, zoom = 1f, offset = GAMELOOP.GameSize() / 2, target = GAMELOOP.GameCenter()};
        public enum PlayerMovement { SLOW = 0, NORMAL = 1, BOOST = 2 }
        private const float WALL_STUN_TIME = 0.5f;
        private const float WALL_COL_FORCE_FACTOR = 2.0f;
        private const float VEL_LERP_STRENGTH = 6.0f;

        private PlayerMovement curMovement = PlayerMovement.NORMAL;
        private float curStunRotation = 0f;
        private BasicTimer damageTimer = new();
        private string shipName = "";
        private float angle = 0f;
        private ProgressBarPro hpBar;
        private ProgressBarPro pwrBar;
        //private ProgressBarPro ammoBar;
        private ProgressBar hpBarMini = new(new(), new(), BarType.LEFTRIGHT, 0.1f, 0f, true);
        private ProgressBar pwrBarMini = new(new(), new(), BarType.LEFTRIGHT, 0f, 0f, true);
        //private Panel aimpointInputPanel;
        //private InputPrompt aimpointInputPrompt;
        private SkillDisplay aimpointSkillDisplay;

        private List<Thruster> thrusters = new();
        private Frame frame = new(2f);
        private WeaponSystem weaponSystem;
        private Core energyCore;
        private TargetFinder targetFinder = new("asteroid");
        private Vector2 slowPos = new(0f);
        public Player(ArmoryInfo armoryInfo, string shipName = "default")
        {
            drawOrder = 50;
            this.shipName = shipName;
            
            stats.AddStats
            (
                ("boostF", 2f),
                ("slowF", 0.25f), ("boostCost", 35f), ("slowCost", 35f),
                ("rotSpeed", 250f), ("size", 10f), ("colDamage", 50f),
                ("maxSpeed", 0f), ("targetingRange", 0f)
            );
            

            var shipData = DataHandler.Get<DataObjects.PlayerData>("player", shipName);
            var engineData = DataHandler.Get<DataObjects.EngineData>("engines", armoryInfo.engine);
            stats.SetStat("boostF", engineData.boostF);
            stats.SetStat("slowF", engineData.slowF);
            stats.SetStat("boostCost", engineData.boostCost);
            stats.SetStat("slowCost", engineData.slowCost);
            stats.SetStat("rotSpeed", shipData.rotSpeed);
            stats.SetStat("size", shipData.size);
            stats.SetStat("colDamage", shipData.colDmg);
            stats.SetStat("maxSpeed", shipData.speed);
            stats.SetStat("targetingRange", shipData.targetingRange);
            stunResistance = shipData.stunRes;
            SetTotalHealth(shipData.health);
            energyCore = new(engineData.energy, engineData.eReplenish, engineData.cooldown);
            energyCore.OnTriggered += CoreTrigger;
            stats.AddStats(energyCore.GetStats("energy"));
            float s = stats.Get("size");
            FrameSetup(s, shipData.frame);
            ThrusterSetup(s, shipData.thrusters);

            this.weaponSystem = new
                (
                    HardpointSetup(s, shipData.targetingRange, armoryInfo.fixedGun, armoryInfo.turretGun, this.energyCore, shipData.hardpoints),
                    shipData.pinDuration,
                    shipData.pinCooldown,
                    "Shoot Fixed"
                );

            targetFinder.Start();
            float size = stats.Get("size");
            collisionMask = new string[] { "asteroid" };
            collider = new(GAMELOOP.GameCenter(), MovementDir * stats.Get("maxSpeed"), size);
            Vector2 barSize = new(125, 500);
            Vector2 start = new(65, ScreenHandler.UIHeight() - barSize.Y - 100);
            Vector2 barOffset = new(-20f, 5f);
            Vector2 gap = new(180, 30);
            aimpointSkillDisplay = new(start + new Vector2(100, -250), new Vector2(250, 100), PaletteHandler.C("text"), PaletteHandler.C("flash"), PaletteHandler.C("neutral"),PaletteHandler.C("energy"), "Drop Pin", "Drop Aim Point", -5f);
            hpBar = new(start, barSize, barOffset, BarType.BOTTOMTOP, 0.1f, -5f);
            start += gap;
            pwrBar = new(start, barSize, barOffset, BarType.BOTTOMTOP, 0f, -5f);
            //aimpointInputPrompt = new(start + new Vector2(100, 0), 50, "Drop Aim Point", -5f, ColorPalette.Cur.text, ColorPalette.Cur.flash, ColorPalette.Cur.energy);
            //aimpointInputPanel = new("K", start + new Vector2 (200, 0), new(120, 120), -5f, FontSize.HUGE, ColorPalette.Cur.text, ColorPalette.Cur.energy);
            //start += gap;
            //ammoBar = new(start, barSize, barOffset, BarType.BOTTOMTOP, 0f, -5f);

            hpBarMini.SetSize(new Vector2(size * 2f, size * 0.2f) * ScreenHandler.GAME_TO_UI);
            pwrBarMini.SetSize(new Vector2(size * 2f, size * 0.2f) * ScreenHandler.GAME_TO_UI);

            hpBarMini.SetColors(PaletteHandler.C("enemy"), new(0, 0, 0, 0), PaletteHandler.C("flash")); //ColorPalette.Cur.neutral
            pwrBarMini.SetColors(PaletteHandler.C("player"), new(0, 0, 0, 0));// ColorPalette.Cur.energy);
            hpBar.SetColors(PaletteHandler.C("enemy"), PaletteHandler.C("neutral"), PaletteHandler.C("flash"));
            pwrBar.SetColors(PaletteHandler.C("player"), PaletteHandler.C("energy"));
            //ammoBar.SetColors(ColorPalette.Cur.special1, ColorPalette.Cur.special12);

            //ScreenHandler.Cam.AddCameraOrderChain("player zoom", new CameraOrder(1f, 2f, 1f, EasingType.BOUNCE_OUT));
            //ScreenHandler.Cam.AddCameraOrderChain("player rot", new CameraOrder(1f, 0f, 360f, 1f, 1f, EasingType.QUAD_OUT));
            ScreenHandler.Cam.AddCameraOrderChain("player translation", new CameraOrder(1f, new Vector2(250f, 0f), new Vector2(0f, 0f), EasingType.BOUNCE_OUT));
            
        }


        public override float GetDamage()
        {
            if (IsMovementBoosting()) return stats.Get("colDamage") * stats.Get("boostF");
            else if (IsMovementSlow()) return stats.Get("colDamage") * stats.Get("slowF");
            else return stats.Get("colDamage");
        }
        public float GetEnergyPercentage()
        {
            return energyCore.F;
        }
        private float GetTotalEnergy()
        {
            return energyCore.Max;
        }

        public override string GetCollisionLayer()
        {
            return "player";
        }
        public override void Collide(CastInfo info)
        {
            if (info.collided)
            {
                if (info.other != null)
                {
                    string colLayer = info.other.GetCollisionLayer();
                    //if(colLayer == "wall")
                    //{
                    //    //float dmg = 10f;
                    //    float stunTime = wallStunTime;
                    //    if (IsMovementBoosting()) { stunTime *= boostFactor; }
                    //    else if (IsMovementSlow()) { stunTime *= slowFactor; }
                    //
                    //    Stun(stunTime);
                    //    Damage(25f, info.collisionPoint, info.normal, this);
                    //    collider.Vel = Vec.Normalize(info.reflectVector) * MaxSpeed * wallColForceFactor;
                    //}
                    if (colLayer == "asteroid")
                    {
                        IDamageable? other = info.other as IDamageable;
                        if (other != null)
                        {
                            //Vector2 hitDir = Vec.Normalize(info.other.GetPos() - collider.Pos);
                            var dmgInfo = other.Damage(GetDamage(), info.collisionPoint, info.normal, this, false);
                            if (!dmgInfo.killed && !dmgInfo.dead)
                            {
                                Damage(other.GetDamage(), info.collisionPoint, -info.normal, other, false);
                                float stunTime = WALL_STUN_TIME * 0.5f;
                                float speed = stats.Get("maxSpeed");
                                if (IsMovementBoosting()) { stunTime *= stats.Get("boostF"); speed *= stats.Get("boostF"); }
                                else if (IsMovementSlow()) { stunTime *= stats.Get("slowF"); speed *= stats.Get("slowF"); }
                                Stun(stunTime);
                                Vector2 dir = Vec.Normalize(info.reflectVector);
                                info.other.GetCollider().AddImpulse(-dir * speed);
                                collider.Vel = dir * stats.Get("maxSpeed") * WALL_COL_FORCE_FACTOR;
                            }
                            else 
                            {
                                Damage(other.GetDamage() * 0.25f, info.collisionPoint, -info.normal, other, false);
                            }
                        }
                    }
                }
            }
        }
        public override DamageInfo Damage(float amount, Vector2 pos, Vector2 dir, IDamageable dmgDealer, bool crit)
        {
            //ScreenHandler.ShaderFlash(0.5f, "chrom");
            //GAMELOOP.Stop(0.5f);
            //Color flashColor = ColorPalette.Cur.neutral;
            //flashColor.a = 50;
            //ScreenHandler.Flash(0.2f, flashColor, new(0, 0, 0, 0), "game");
            
            var dmgInfo = base.Damage(amount, pos, dir, dmgDealer, crit);

            if (!dmgInfo.killed) damageTimer.Start(0.25f);
            return dmgInfo;

        }
        public override void Destroy()
        {
            if (IsDead())
            {
                TimerHandler.Add(5f, () => { ScreenHandler.Cam.ClearCameraOrderChains(); GAMELOOP.GoToScene("mainmenu"); });
            }
        }
        protected override void WasKilled()
        {
            base.WasKilled();
            AudioHandler.PlaySFX("player die");
            for (int i = 0; i < 100; i++)
            {
                HitParticle particle = new(collider.Pos + RNG.randVec2(1, stats.Get("size")), RNG.randVec2(), 2f, 2f, PaletteHandler.C("player"));
                GAMELOOP.AddGameObject(particle);
            }
            InputHandler.AddVibration(0, 0.5f, 0.5f, 1.5f);
            ScreenHandler.Cam.AddCameraOrderChain("player died", false, new CameraOrder(5f, 1f, 2f));
        }
        public override void WasDamaged(DamageInfo info)
        {
            base.WasDamaged(info);
            if (!info.killed && info.recieved > 0f) AudioHandler.PlaySFX("player hurt");

            float f = info.recieved / GetTotalHealth();
            int amount = (int)(50 * f);
            Color particleColor = PaletteHandler.C("player");
            if (info.crit) { f += 0.5f; particleColor = PaletteHandler.C("flash"); }
            for (int i = 0; i < amount; i++)
            {
                HitParticle particle = new(info.pos, info.dir, 0.75f + f, 0.5f, particleColor);
                GAMELOOP.AddGameObject(particle);
            }
            InputHandler.AddVibration(0, 0f, 0.25f, 0.5f);
            ScreenHandler.Cam.Shake(0.5f, new(20f, 20f), 1f, 0f, 0.75f);
            //ScreenHandler.Flash(0.3f, ColorPalette.Cur.enemy, BLANK, true);
            ScreenHandler.FlashTint(0.3f, BLACK, false);
        }
        protected override void WasStunned(float duration)
        {
            base.WasStunned(duration);
            curStunRotation = RNG.randF(300f, 600f);
            if (RNG.randF() < 0.5f) curStunRotation *= -1;
            if(IsMovementBoosting()) BoostEnded();
            if(IsMovementSlow()) SlowEnded();

        }
        public override void WasHealed(HealInfo info)
        {
            base.WasHealed(info);
            if (info.recieved > 0f)
            {
                AudioHandler.PlaySFX("player healed");
            }
        }
        public override string GetID()
        {
            return "player";
        }
        public override void Spawn()
        {
            //SetStat("maxSpeed", 75f);
            float randAngle = RNG.randF(0f, 2f * PI);
            Vector2 dir = Vec.Rotate(Vec.Right(), randAngle);
            MovementDir = Vec.Normalize(dir);
            collider.Vel = MovementDir * stats.Get("maxSpeed");
        }

        public override void Update(float dt)
        {
            var prevMovement = curMovement;
            var prevStunned = IsStunned();
            base.Update(dt);

            if (InputHandler.IsReleased(0, "Heal Player")) Heal(RNG.randF(10, 35), collider.Pos, this);

            damageTimer.Update(dt);

            hpBar.SetF(GetHealthPercentage());
            hpBarMini.SetF(GetHealthPercentage());

            if (energyCore.IsCooldownActive())
            {
                pwrBar.SetF(1.0f - energyCore.CooldownF);
                pwrBarMini.SetF(1.0f - energyCore.CooldownF);
            }
            else
            {
                pwrBar.SetF(GetEnergyPercentage());
                pwrBarMini.SetF(GetEnergyPercentage());
            }

            hpBar.Update(dt, GAMELOOP.MOUSE_POS_UI);
            hpBarMini.Update(dt, GAMELOOP.MOUSE_POS_UI);
            pwrBar.Update(dt, GAMELOOP.MOUSE_POS_UI);
            pwrBarMini.Update(dt, GAMELOOP.MOUSE_POS_UI);
            //ammoBar.Update(dt, GAMELOOP.MOUSE_POS_UI);

            if (!IsStunned())
            {
                if (prevStunned) AudioHandler.PlaySFX("player stun ended");

                if (InputHandler.IsReleased(0, "Drop Aim Point")) weaponSystem.DropAimPoint(collider.Pos);
                //if (InputHandler.IsReleased("Drop Turrets")) weaponSystem.DropTurrets(collider.Pos);

                energyCore.Update(dt);
                if (energyCore.IsCooldownActive())
                {
                    int randAmount = RNG.randI(1, 4);
                    for (int i = 0; i < randAmount; i++)
                    {
                        float aChange = (RNG.randF() < 0.5f ? 90 : -90) * DEG2RAD;
                        float a = angle + aChange;
                        LineParticle p = new(collider.Pos, a, 25*DEG2RAD, RNG.randF(150, 200), PaletteHandler.C("energy"), RNG.randF(3f, 5f), RNG.randF(1.5f, 2f)*energyCore.CooldownF, 1f);
                        GAMELOOP.AddGameObject(p);
                    }
                }
                Vector2 targetFinderPos = collider.Pos;
                if (weaponSystem.IsAimPointActive()) targetFinderPos = weaponSystem.AimPoint;
                //else if (weaponSystem.AreTurretsDropped()) targetFinderPos = weaponSystem.DropPoint;
                targetFinder.Update(targetFinderPos, stats.Get("targetingRange"), dt);

                curMovement = PlayerMovement.NORMAL;


                if (InputHandler.IsGamepad())
                {
                    float gamepadRotation = InputHandler.GetGamepadAxis(0, "Rotate");
                    float amount = gamepadRotation * gamepadRotation;
                    if (gamepadRotation > 0)
                    {
                        MovementDir = Vec.Rotate(MovementDir, stats.Get("rotSpeed") * DEG2RAD * dt * amount);
                    }
                    else if (gamepadRotation < 0)
                    {
                        MovementDir = Vec.Rotate(MovementDir, -stats.Get("rotSpeed") * DEG2RAD * dt * amount);
                    }
                }
                else
                {
                    if (InputHandler.IsDown(0, "Rotate Left"))
                    {
                        MovementDir = Vec.Rotate(MovementDir, -stats.Get("rotSpeed") * DEG2RAD * dt);
                    }
                    else if (InputHandler.IsDown(0, "Rotate Right"))
                    {
                        MovementDir = Vec.Rotate(MovementDir, stats.Get("rotSpeed") * DEG2RAD * dt);
                    }
                }


                float speed = stats.Get("maxSpeed");
                if (InputHandler.IsDown(0, "Boost") && CanBoost())
                {
                    if (prevMovement != PlayerMovement.BOOST)
                    {
                        AudioHandler.PlaySFX("boost");
                        BoostStarted();
                    }
                    curMovement = PlayerMovement.BOOST;
                    speed *= stats.Get("boostF");
                    energyCore.Use(stats.Get("boostCost") * dt);
                }
                else if (InputHandler.IsDown(0, "Slow") && CanSlow())
                {
                    if (prevMovement != PlayerMovement.SLOW)
                    {
                        AudioHandler.PlaySFX("slow");
                        SlowStarted();
                    }
                    curMovement = PlayerMovement.SLOW;
                    speed *= stats.Get("slowF");
                    energyCore.Use(stats.Get("slowCost") * dt);
                }
                else
                {
                    if (prevMovement == PlayerMovement.SLOW) SlowEnded();
                    else if (prevMovement == PlayerMovement.BOOST) BoostEnded();
                }
                //else
                //{
                //    engineCore.Update(dt);
                //}

                collider.Vel = Vec.Lerp(collider.Vel, Vec.Normalize(MovementDir) * speed, dt * VEL_LERP_STRENGTH);

            }
            else
            {
                curMovement = PlayerMovement.NORMAL;
                MovementDir = Vec.Rotate(MovementDir, curStunRotation * DEG2RAD * dt);
            }
            collider.Pos = collider.Pos + collider.Vel * dt;
            angle = Vec.AngleDeg(MovementDir) * DEG2RAD;

            float sizeFactor = stats.GetStat("size").GetF();
            if (damageTimer.IsRunning()) sizeFactor += 0.15f;
            Color drawColor = PaletteHandler.C("player");
            if (damageTimer.IsRunning()) drawColor = PaletteHandler.C("flash");
            else if (energyCore.IsCooldownActive()) drawColor = PaletteHandler.C("energy");

            frame.Update(collider.Pos, angle, sizeFactor, drawColor, dt);

            foreach (var thruster in thrusters)
            {
                thruster.Update(collider.Pos, angle, sizeFactor, drawColor, dt);
            }


            weaponSystem.UpdateSpacing(collider.Pos, angle, sizeFactor);


            if (!IsStunned())
            {
                weaponSystem.Update(dt);

                float particleFactor = 1f;
                if (IsMovementBoosting()) particleFactor = stats.Get("boostF");
                else if (IsMovementSlow()) particleFactor = stats.Get("slowF");
                foreach (var thruster in thrusters)
                {
                    thruster.SpawnParticles(stats.Get("maxSpeed"), new(0, 4), particleFactor);
                }
            }

            Vector2 miniBarOffset = new Vector2(stats.Get("size"), stats.Get("size") * 2f);
            Vector2 miniBarPos = ScreenHandler.TransformPositionToUI(collider.Pos - miniBarOffset);
            if (GetHealthPercentage() < 1f)
            {
                hpBarMini.SetTopLeft(miniBarPos);

            }
            if (GetEnergyPercentage() < 1f)
            {
                pwrBarMini.SetTopLeft(miniBarPos - new Vector2(0f, hpBarMini.GetHeight() + 10));
            }

            var info = GAMELOOP.GetCurArea().GetCurPlayfield().Collide(collider.Pos, collider.Radius);
            if (info.collided)
            {
                float stunTime = WALL_STUN_TIME;
                if (IsMovementBoosting()) { stunTime *= stats.Get("boostF"); }
                else if (IsMovementSlow()) { stunTime *= stats.Get("slowF"); }

                Stun(stunTime);
                Damage(25f, info.hitPoint, info.n, this, false);
                collider.Vel = Vec.Normalize(Vec.Reflect(collider.Vel, info.n)) * stats.Get("maxSpeed") * WALL_COL_FORCE_FACTOR;

            }
            if (weaponSystem.IsAimPointCooldownActive()) aimpointSkillDisplay.SetBarF(weaponSystem.F);
            else if(weaponSystem.IsAimPointActive()) aimpointSkillDisplay.SetBarF(1.0f - weaponSystem.F);
            else aimpointSkillDisplay.SetBarF(1f);
        }


        public override void Draw()
        {
            base.Draw();

            frame.Draw();
            if (!IsStunned())
            {
                foreach (var thruster in thrusters)
                {
                    thruster.Draw();
                }

                weaponSystem.Draw();
                targetFinder.Draw(stats.Get("targetingRange"));
            }
            if (DEBUG_DrawColliders)
            {
                if (collider.IsEnabled()) collider.DebugDrawShape(DEBUG_ColliderColor);
                else collider.DebugDrawShape(DEBUG_ColliderDisabledColor);

                DrawRectangleLinesEx(GetBoundingBox(), 1f, GREEN);
            }

        }
        public override void DrawUI()
        {
            if (IsDead()) return;

            Vector2 barSize = hpBar.GetSize();
            Vector2 topLeft = hpBar.GetTopLeft();
            Vector2 offset = hpBar.Transform(new Vector2(barSize.X / 2, 0));


            UIHandler.DrawTextAlignedPro("HP", topLeft + offset, hpBar.GetRotationDeg(), FontSize.LARGE, 2, PaletteHandler.C("enemy"), Alignement.BOTTOMCENTER);
            hpBar.Draw();

            topLeft = pwrBar.GetTopLeft();
            UIHandler.DrawTextAlignedPro("PWR", topLeft + offset, pwrBar.GetRotationDeg(), FontSize.LARGE, 2, PaletteHandler.C("player"), Alignement.BOTTOMCENTER);
            pwrBar.Draw();
            if (energyCore.IsCooldownActive())
            {
                Vector2 bottomRight = pwrBar.GetBottomRight() + new Vector2(20, 0);
                UIHandler.DrawTextAlignedPro("REBOOT", bottomRight, pwrBar.GetRotationDeg() - 90f, FontSize.HUGE, 10, PaletteHandler.C("bg2"), Alignement.BOTTOMLEFT);
            }
            aimpointSkillDisplay.Draw();
            //aimpointInputPanel.Draw();
            //topLeft = ammoBar.GetTopLeft();
            //UIHandler.DrawTextAlignedPro("AMMO", topLeft + offset, ammoBar.GetRotationDeg(), FontSize.LARGE, 2, ColorPalette.Cur.special1, Alignement.BOTTOMCENTER);
            //ammoBar.Draw();

            if (GetHealthPercentage() < 1f)
            {
                hpBarMini.Draw();
            }
            if (GetEnergyPercentage() < 1f)
            {
                
                pwrBarMini.Draw();
            }
            //Drawing.DrawRect(new(500, 500), new(200, 50), new(0f, 0.5f), angle * RAD2DEG, VIOLET);
            //Drawing.DrawCircleOutlineBar(GAMELOOP.UICenter(), 200, -90f, 20, GetEnergyPercentage(), YELLOW);
            //Vector2 tl = collider.Origin + new Vector2(-size, -size*2f);
            //Vector2 bs = new Vector2(size * 2f, size * 0.2f) * ScreenHandler.GAME_TO_UI;
            //UIHandler.DrawBar(tl * ScreenHandler.GAME_TO_UI, bs, GetHealthPercentage(), ColorPalette.Cur.enemy2, ColorPalette.Cur.neutral, BarType.LEFTRIGHT);

        }


        public override Vector2 GetCameraPosition(Vector2 camPos, float dt, float smoothness = 1f, float boundary = 0f)
        {
            if (dt <= 0f) return collider.Pos;

            Vector2 targetPos = collider.Pos;
            if (IsMovementSlow()) { targetPos = slowPos; }
            else if (IsMovementBoosting()) { targetPos = collider.Pos + collider.Vel * 1.2f;}
            //else { targetPos = collider.Pos; }
            return Vec.Lerp(camPos, targetPos, dt * smoothness);
        }
        public bool CanBoost() { return !IsEnergyEmpty() && !energyCore.IsCooldownActive(); }
        public bool CanSlow() { return !IsEnergyEmpty() && !energyCore.IsCooldownActive(); }
        public bool IsEnergyFull() { return energyCore.IsFull(); }
        public bool IsEnergyEmpty() { return energyCore.IsEmpty(); }
        public bool IsMovementBoosting() { return curMovement == PlayerMovement.BOOST; }
        public bool IsMovementSlow() { return curMovement == PlayerMovement.SLOW; }
        public bool IsMovementNormal() { return curMovement == PlayerMovement.NORMAL; }


        protected override void StatChanged(string statName)
        {
            if (statName == "size")
            {
                float size = stats.Get("size");
                hpBarMini.SetSize(new Vector2(size * 2f, size * 0.2f) * ScreenHandler.GAME_TO_UI);
                pwrBarMini.SetSize(new Vector2(size * 2f, size * 0.2f) * ScreenHandler.GAME_TO_UI);
                collider.Radius = size;
            }
        }
        private void SlowStarted()
        {
            slowPos = collider.Pos;
            InputHandler.AddVibration(0, 0f, 0.05f, -1f, "slow");
        }
        private void SlowEnded()
        {
            InputHandler.RemoveVibration(0, "slow");
        }
        private void BoostStarted()
        {
            InputHandler.AddVibration(0, 0.1f, 0.05f, -1f, "boost");
            ScreenHandler.Cam.AddCameraOrderChain("player boost", false, new CameraOrder(0.4f, 1f, 1.1f));
        }
        private void BoostEnded()
        {
            InputHandler.RemoveVibration(0, "boost");
            ScreenHandler.Cam.AddCameraOrderChain("player boost", true, new CameraOrder(0.4f, 1.1f, 1f));
        }

        private void CoreTrigger(string triggerName, params float[] values)
        {
            if (triggerName == "emptied")
            {
                pwrBarMini.SetColors(PaletteHandler.C("flash"));
                pwrBar.SetColors(PaletteHandler.C("flash"));

                CoreParticle cp = new(collider.Pos, angle + PI / 2, PaletteHandler.C("energy"));
                GAMELOOP.AddGameObject(cp);

                AudioHandler.PlaySFX("player pwr down", -1, -1, 0);
            }
            else if(triggerName == "reloaded")
            {
                pwrBarMini.SetColors(PaletteHandler.C("player"));
                pwrBar.SetColors(PaletteHandler.C("player"));

                AudioHandler.PlaySFX("player pwr up", -1, -1, 0);
            }
        }

        private List<Hardpoint> HardpointSetup(float size, float targetingRange, string fixedGun, string turretGun, Core energyCore, params HardpointInfo[] info)
        {
            List<Hardpoint> hardpoints = new();
            foreach (var hp in info)
            {
                var hardpoint = CreateHardpoint(hp, energyCore, size, targetingRange);
                hardpoints.Add(hardpoint);
            }
            foreach (var hardpoint in hardpoints)
            {
                if (hardpoint.IsTurret()) hardpoint.SetGun(turretGun, this);
                else hardpoint.SetGun(fixedGun, this);
            }
            return hardpoints;
        }
        private Hardpoint CreateHardpoint(HardpointInfo info, Core engineCore, float size, float targetingRange)
        {
            Color color = PaletteHandler.C("player");
            Color offColor = PaletteHandler.C("energy");
            Vector2 offset = new Vector2(info.x, info.y) * size;
            TargetFinder? tf = null;
            if (info.type == (int)Hardpoint.HardpointType.TURRET) tf = targetFinder;
            Hardpoint hp = new(
                offset, color, offColor,
                info.rotOffset,
                targetingRange,
                engineCore,
                tf,
                (Hardpoint.HardpointType)info.type,
                (Hardpoint.HardpointSize)info.size
                );
            return hp;
        }


        private void ThrusterSetup(float size, params ThrusterInfo[] info)
        {
            foreach (var thruster in info)
            {
                thrusters.Add(new(new(thruster.x * size, thruster.y * size), size * thruster.size, size * thruster.pSize));
            }
        }
        private void FrameSetup(float size, params FrameInfo[] info)
        {
            if (info.Length <= 0) return;
            Vector2[] points = new Vector2[info.Length];
            float lineThickness = info[0].lineThickness * size;
            for (int i = 0; i < points.Length; i++)
            {
                points[i].X = info[i].x * size;
                points[i].Y = info[i].y * size;
            }
            frame = new(lineThickness, points);
        }
    }


}



    /*
    public class AimPin
    {
        Vector2 pos = new();
        float radius = 0f;
        float moveSpeed = 0f;
        int enemyCount = 0;
        string[] mask = new string[0];
        Playfield curPlayfield;
        public AimPin(float radius, float moveSpeed, Playfield playfield, params string[] mask)
        {
            this.radius = radius;
            this.moveSpeed = moveSpeed;
            this.curPlayfield = playfield;
            this.mask = mask;
        }
        public Vector2 GetAimPoint()
        {
            return pos + RNG.randVec2(radius * 0.8f, radius);
        }
        public bool HasEnemy() { return enemyCount > 0; }
        public void SetPos(Vector2 newPos) { this.pos = newPos; }
        public void MoveTo(Vector2 targetPos, float dt)
        {
            if (moveSpeed <= 0f) this.pos = targetPos;
            else
            {
                Vector2 dir = targetPos - pos;
                float length = Vec.Length(dir);
                float amount = MathF.Min(moveSpeed * dt, length);
                pos += Vec.Normalize(dir) * amount;
            }
        }
        public void MoveBy(Vector2 amount, float dt)
        {
            if (moveSpeed <= 0f) this.pos += amount;
            else
            {
                pos += amount * moveSpeed * dt;
            }
        }
        public void Update(float dt)
        {
            var info = curPlayfield.Collide(pos, radius);
            if (info.collided) pos = info.newPos;

            if (mask.Length <= 0)
            {
                enemyCount = 0;
                return;
            }
            if (GAMELOOP.CUR_SCENE == null) return;
            var area = GAMELOOP.CUR_SCENE.GetCurArea();
            if (area == null) return;
            enemyCount = area.colHandler.CastSpace(pos, radius, mask).Count;
        }
        public void Draw(Vector2 other)
        {
            Color color = ColorPalette.ChangeAlpha(ColorPalette.Cur.flash, 200);
            float lineThickness = 1.5f;
            if (!HasEnemy())
            {
                color.a = 125;
                lineThickness = 1f;
            }

            Drawing.DrawCircleLines(pos, radius + RNG.randF(-0.5f, 0.5f), lineThickness, color, 8f);
            Vector2 targetPos = pos + RNG.randVec2(0f, 1f);
            Vector2 size = new(5f, 5f);
            var rect = new Rectangle(targetPos.X - size.X / 2, targetPos.Y - size.Y / 2, size.X, size.Y);
            DrawRectangleLinesEx(rect, lineThickness, color);
            DrawLineEx(other, targetPos, lineThickness, color);
        }
    }
*/
