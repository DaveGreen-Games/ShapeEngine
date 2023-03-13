using System.Numerics;
using ShapeCore;
using ShapeInput;
using ShapeScreen;
using ShapeTiming;
using ShapeAudio;
using ShapeUI;
using ShapePersistent;
using ShapeCollision;
using Raylib_CsLo;
using ShapeEngineDemo.Guns;
using ShapeEngineDemo.DataObjects;
using ShapeLib;
using ShapeColor;
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
                Color color = Demo.PALETTES.C(ColorIDs.Special1);
                Vector2 endpoint = location + new Vector2(SRNG.randF(-1, 2), -8);
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
        private ProgressBar hpBar;
        private ProgressBar pwrBar;
        //private ProgressCircle pwrBarCircle;
        //private ProgressRing hpBarRing;
        //private ProgressBarPro ammoBar;
        private ProgressBar hpBarMini = new(0.1f, 0f);
        private ProgressBar pwrBarMini = new(0f, 0f);
        //private Panel aimpointInputPanel;
        //private InputPrompt aimpointInputPrompt;
        private SkillDisplay aimpointSkillDisplay;

        private List<Thruster> thrusters = new();
        private Frame frame = new(2f);
        private WeaponSystem weaponSystem;
        private Core energyCore;
        private TargetFinder targetFinder = new("asteroid");
        private Vector2 slowPos = new(0f);
        private bool playfieldCollision = false;


        private InputActionWrapper healPlayerInput = new(2, 0.2f, 1f);
        public Player(ArmoryInfo armoryInfo, string shipName = "default")
        {
            DrawOrder = 50;
            this.shipName = shipName;
            
            stats.AddStats
            (
                ("boostF", 2f),
                ("slowF", 0.25f), ("boostCost", 35f), ("slowCost", 35f),
                ("rotSpeed", 250f), ("size", 10f), ("colDamage", 50f),
                ("maxSpeed", 0f), ("targetingRange", 0f)
            );
            
            
            var shipData = Demo.DATA.GetCDBContainer().Get<DataObjects.PlayerData>("player", shipName);
            var engineData = Demo.DATA.GetCDBContainer().Get<DataObjects.EngineData>("engines", armoryInfo.engine);
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
            collider.CheckCollision = true;
            collider.CheckIntersections = true;

            Vector2 barOffset = new(-0.15f, 0.05f);
            aimpointSkillDisplay = new(Demo.PALETTES.C(ColorIDs.Text), Demo.PALETTES.C(ColorIDs.Flash), Demo.PALETTES.C(ColorIDs.Neutral),Demo.PALETTES.C(ColorIDs.Energy), "Drop Pin", "Drop Aim Point", -5f);
            hpBar = new(-5f, new Vector2(0, 1), barOffset, 0.1f, 0);
            pwrBar = new(-5f, new Vector2(0, 1), barOffset, 0f, 0f);
            hpBar.SetProgressDirections(0, 0, 1, 0);
            pwrBar.SetProgressDirections(0, 0, 1, 0);
            //pwrBarCircle = new(new Vector2(0f), Alignement.BOTTOMCENTER, 0.5f, 0.04f);
            //hpBarRing = new(90, -90, new Vector2(0f), 0f, 0.8f, 0.75f, 0.1f, 0f);
            //hpBarRing.SetReservedF(0.223f);
            //aimpointInputPrompt = new(start + new Vector2(100, 0), 50, "Drop Aim Point", -5f, ColorPalette.Cur.text, ColorPalette.Cur.flash, ColorPalette.Cur.energy);
            //aimpointInputPanel = new("K", start + new Vector2 (200, 0), new(120, 120), -5f, FontSize.HUGE, ColorPalette.Cur.text, ColorPalette.Cur.energy);
            //start += gap;
            //ammoBar = new(start, barSize, barOffset, BarType.BOTTOMTOP, 0f, -5f);


            hpBarMini.SetProgressDirections(0.5f, 0.5f, 0, 0);
            pwrBarMini.SetProgressDirections(0.5f, 0.5f, 0, 0);
            hpBarMini.SetColors(Demo.PALETTES.C(ColorIDs.Enemy), new(0, 0, 0, 0), Demo.PALETTES.C(ColorIDs.Flash)); //ColorPalette.Cur.neutral
            pwrBarMini.SetColors(Demo.PALETTES.C(ColorIDs.Player), new(0, 0, 0, 0));// ColorPalette.Cur.energy);
            hpBar.SetColors(Demo.PALETTES.C(ColorIDs.Enemy), Demo.PALETTES.C(ColorIDs.Neutral), Demo.PALETTES.C(ColorIDs.Flash));
            pwrBar.SetColors(Demo.PALETTES.C(ColorIDs.Player), Demo.PALETTES.C(ColorIDs.Energy));
            //pwrBarCircle.SetColors(PaletteHandler.C("player"), PaletteHandler.C("energy"), new(0,0,0,0), PaletteHandler.C("flash"), ORANGE);
            //hpBarRing.SetColors(PaletteHandler.C("enemy"), PaletteHandler.C("neutral"), YELLOW, PaletteHandler.C("flash"), ORANGE);
            //ammoBar.SetColors(ColorPalette.Cur.special1, ColorPalette.Cur.special12);

            //ScreenHandler.Cam.AddCameraOrderChain("player zoom", new CameraOrder(1f, 2f, 1f, EasingType.BOUNCE_OUT));
            //ScreenHandler.Cam.AddCameraOrderChain("player rot", new CameraOrder(1f, 0f, 360f, 1f, 1f, EasingType.QUAD_OUT));
            ScreenHandler.CAMERA.AddCameraOrderChain("player translation", new CameraOrder(1f, new Vector2(250f, 0f), new Vector2(0f, 0f), EasingType.BOUNCE_OUT));

            //UpdateSlowResistance = 0f;
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
        public override void Overlap(CollisionInfo info)
        {
            if (info.collision)// || info.overlapping)
            {
                if (info.other != null)
                {
                    string colLayer = info.other.GetCollisionLayer();
                    if (colLayer == "asteroid")
                    {
                        IDamageable? other = info.other as IDamageable;
                        if (other != null && info.intersection.valid)
                        {
                            Vector2 normal = info.intersection.n;
                            Vector2 colP = info.intersection.p;
                            var dmgInfo = other.Damage(GetDamage(), colP, -normal, this, false);
                            if (!dmgInfo.killed && !dmgInfo.dead)
                            {
                                Damage(other.GetDamage(), colP, normal, other, false);
                                float stunTime = WALL_STUN_TIME * 0.5f;
                                float speed = stats.Get("maxSpeed");
                                if (IsMovementBoosting()) { stunTime *= stats.Get("boostF"); speed *= stats.Get("boostF"); }
                                else if (IsMovementSlow()) { stunTime *= stats.Get("slowF"); speed *= stats.Get("slowF"); }
                                Stun(stunTime);
                                Vector2 otherDir = SVec.Normalize(SVec.Reflect(info.other.GetCollider().Vel, -normal));
                                info.other.GetCollider().AddImpulse(otherDir * speed);
                                Vector2 selfDir = SVec.Normalize(SVec.Reflect(collider.Vel, normal)); //SVec.Normalize(info.reflectVector);
                                collider.Vel = selfDir * stats.Get("maxSpeed") * WALL_COL_FORCE_FACTOR;
                            }
                            //else 
                            //{
                            //    Damage(other.GetDamage() * 0.25f, colP, -normal, other, false);
                            //}
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
                Demo.TIMER.Add(5f, () => { ScreenHandler.CAMERA.ClearCameraOrderChains(); GAMELOOP.GoToScene("mainmenu"); });
            }
        }
        protected override void WasKilled()
        {
            base.WasKilled();
            AudioHandler.PlaySFX("player die");
            for (int i = 0; i < 100; i++)
            {
                HitParticle particle = new(collider.Pos + SRNG.randVec2(1, stats.Get("size")), SRNG.randVec2(), 2f, 2f, Demo.PALETTES.C(ColorIDs.Player));
                GAMELOOP.AddGameObject(particle);
            }
            InputHandler.AddVibration(0, 0.5f, 0.5f, 1.5f);
            ScreenHandler.CAMERA.AddCameraOrderChain("player died", false, new CameraOrder(5f, 1f, 2f));
        }
        public override void WasDamaged(DamageInfo info)
        {
            base.WasDamaged(info);
            if (!info.killed && info.recieved > 0f) AudioHandler.PlaySFX("player hurt");

            float f = info.recieved / GetTotalHealth();
            int amount = (int)(50 * f);
            Color particleColor = Demo.PALETTES.C(ColorIDs.Player);
            if (info.crit) { f += 0.5f; particleColor = Demo.PALETTES.C(ColorIDs.Flash); }
            for (int i = 0; i < amount; i++)
            {
                HitParticle particle = new(info.pos, info.dir, 0.75f + f, 0.5f, particleColor);
                GAMELOOP.AddGameObject(particle);
            }
            InputHandler.AddVibration(0, 0f, 0.25f, 0.5f);
            ScreenHandler.CAMERA.Shake(0.5f, new(20f, 20f), 1f, 0f, 0.75f);
            //ScreenHandler.Flash(0.3f, ColorPalette.Cur.enemy, BLANK, true);
            //ScreenHandler.FlashTint(0.3f, BLACK, false);
            ScreenHandler.Flash(0.5f, new(225, 25, 50, 150), new(50, 0, 0, 50), true);
            GAMELOOP.GetCurArea().Slow("player hurt", 0.25f, 0.5f);
            //GAMELOOP.Slow(0.3f, 0.5f, 0.1f);
            //GAMELOOP.GetCurArea().UpdateSlowFactor = 0.3f;
            //GAMELOOP.CallDeferred(() => GAMELOOP.Slow(0.1f, 0.5f, 0f), 1);
            //GAMELOOP.Stop(0.5f, 0.1f);
        }
        protected override void WasStunned(float duration)
        {
            base.WasStunned(duration);
            curStunRotation = SRNG.randF(300f, 600f);
            if (SRNG.randF() < 0.5f) curStunRotation *= -1;
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
        public override void Start()
        {
            //SetStat("maxSpeed", 75f);
            float randAngle = SRNG.randF(0f, 2f * PI);
            Vector2 dir = SVec.Rotate(SVec.Right(), randAngle);
            MovementDir = SVec.Normalize(dir);
            collider.Vel = MovementDir * stats.Get("maxSpeed");
        }

        public override void Update(float dt)
        {
            var prevMovement = curMovement;
            var prevStunned = IsStunned();
            base.Update(dt);

            var healInput = healPlayerInput.Update(dt, InputHandler.IsPressed(0, "Heal Player"), InputHandler.IsReleased(0, "Heal Player"));
            if (healInput.holdFinished) Heal(500, collider.Pos, this);
            if(healInput.tapFinished) Heal(50, collider.Pos, this);


            //if (InputHandler.IsReleased(0, "Heal Player")) Heal(RNG.randF(10, 35), collider.Pos, this);
            //if (InputHandler.GetHoldF(0, "Heal Player") == 0f) Heal(500, collider.Pos, this);

            damageTimer.Update(dt);

            hpBar.SetF(GetHealthPercentage());
            //hpBarRing.SetF(GetHealthPercentage());
            hpBarMini.SetF(GetHealthPercentage());

            if (energyCore.IsCooldownActive())
            {
                pwrBar.SetF(1.0f - energyCore.CooldownF);
                //pwrBarCircle.SetF(1.0f - energyCore.CooldownF);
                pwrBarMini.SetF(1.0f - energyCore.CooldownF);
            }
            else
            {
                pwrBar.SetF(GetEnergyPercentage());
                //pwrBarCircle.SetF(GetEnergyPercentage());
                pwrBarMini.SetF(GetEnergyPercentage());
            }

            hpBar.Update(dt, GAMELOOP.MOUSE_POS_UI);
            //hpBarRing.Update(dt, GAMELOOP.MOUSE_POS_UI);
            hpBarMini.Update(dt, GAMELOOP.MOUSE_POS_UI);
            pwrBar.Update(dt, GAMELOOP.MOUSE_POS_UI);
            //pwrBarCircle.Update(dt, GAMELOOP.MOUSE_POS_UI);
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
                    int randAmount = SRNG.randI(1, 4);
                    for (int i = 0; i < randAmount; i++)
                    {
                        float aChange = (SRNG.randF() < 0.5f ? 90 : -90) * DEG2RAD;
                        float a = angle + aChange;
                        LineParticle p = new(collider.Pos, a, 25*DEG2RAD, SRNG.randF(150, 200), Demo.PALETTES.C(ColorIDs.Energy), SRNG.randF(3f, 5f), SRNG.randF(1.5f, 2f)*energyCore.CooldownF, 1f);
                        //p.SetDrag(0.5f);
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
                        MovementDir = SVec.Rotate(MovementDir, stats.Get("rotSpeed") * DEG2RAD * dt * amount);
                    }
                    else if (gamepadRotation < 0)
                    {
                        MovementDir = SVec.Rotate(MovementDir, -stats.Get("rotSpeed") * DEG2RAD * dt * amount);
                    }
                }
                else
                {
                    if (InputHandler.IsDown(0, "Rotate Left"))
                    {
                        MovementDir = SVec.Rotate(MovementDir, -stats.Get("rotSpeed") * DEG2RAD * dt);
                    }
                    else if (InputHandler.IsDown(0, "Rotate Right"))
                    {
                        MovementDir = SVec.Rotate(MovementDir, stats.Get("rotSpeed") * DEG2RAD * dt);
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

                collider.Vel = SVec.Lerp(collider.Vel, SVec.Normalize(MovementDir) * speed, dt * VEL_LERP_STRENGTH);

            }
            else
            {
                curMovement = PlayerMovement.NORMAL;
                MovementDir = SVec.Rotate(MovementDir, curStunRotation * DEG2RAD * dt);
            }
            collider.Pos = collider.Pos + collider.Vel * dt;
            angle = SVec.AngleRad(MovementDir);
            float sizeFactor = stats.GetStat("size").GetF();
            if (damageTimer.IsRunning()) sizeFactor += 0.15f;
            Color drawColor = Demo.PALETTES.C(ColorIDs.Player);
            if (damageTimer.IsRunning()) drawColor = Demo.PALETTES.C(ColorIDs.Flash);
            else if (energyCore.IsCooldownActive()) drawColor = Demo.PALETTES.C(ColorIDs.Energy);

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

            Vector2 miniBarSize = new Vector2(stats.Get("size") * 2f, stats.Get("size") * 0.2f) * ScreenHandler.GAME_TO_UI * ScreenHandler.CAMERA.RawZoom;
            Vector2 miniBarOffset = new Vector2(0, stats.Get("size") * 2f);
            Vector2 miniBarPos = ScreenHandler.TransformPositionToUI(collider.Pos - miniBarOffset);
            //hpBarMini.SetSize();
            //pwrBarMini.SetSize(new Vector2(size * 2f, size * 0.2f) * ScreenHandler.GAME_TO_UI);

            
            if (GetHealthPercentage() < 1f)
            {
                hpBarMini.UpdateRect(miniBarPos, miniBarSize, new(0.5f));

            }
            if (GetEnergyPercentage() < 1f)
            {
                pwrBarMini.UpdateRect(miniBarPos - new Vector2(0, miniBarSize.Y * 1.1f) * 1.2f, miniBarSize, new(0.5f));
            }

            //var info = GAMELOOP.GetCurArea().GetCurPlayfield().Collide(collider.Pos, collider.Radius);
            var info = SRect.CollidePlayfield(GAMELOOP.GetCurArea().GetInnerArea(), collider.Pos, collider.Radius);
            if (info.collided)
            {
                if (!playfieldCollision)
                {
                    playfieldCollision = true;
                    float stunTime = WALL_STUN_TIME;
                    if (IsMovementBoosting()) { stunTime *= stats.Get("boostF"); }
                    else if (IsMovementSlow()) { stunTime *= stats.Get("slowF"); }

                    Stun(stunTime);
                    Damage(25f, info.hitPoint, info.n, this, false);
                    collider.Vel = SVec.Normalize(SVec.Reflect(collider.Vel, info.n)) * stats.Get("maxSpeed") * WALL_COL_FORCE_FACTOR;
                }
            }
            else
            {
                if (playfieldCollision) playfieldCollision = false;
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
            if (DEBUG_DRAWCOLLIDERS)
            {
                if (collider.IsEnabled()) collider.DebugDrawShape(DEBUG_ColliderColor);
                else collider.DebugDrawShape(DEBUG_ColliderDisabledColor);

                DrawRectangleLinesEx(GetBoundingBox(), 1f, GREEN);
            }

        }
        public override void DrawUI(Vector2 uiSize, Vector2 stretchFactor)
        {
            if (IsDead()) return;

            //Vector2 barSize = hpBar.GetSize();
            //Vector2 topLeft = hpBar.GetTopLeft();
            //Vector2 offset = hpBar.Transform(new Vector2(barSize.X / 2, 0));

            //UIHandler.DrawTextAligned(String.Format("Rad: {0} // Deg: {1}", angle, angle * RAD2DEG), new(1000, 200), 120, 1, WHITE, Alignement.CENTER);
            //hpBarRing.UpdateRect(uiSize * new Vector2(0.95f, 0.95f), uiSize * new Vector2(0.08f, 0.08f), Alignement.BOTTOMRIGHT);
            //hpBarRing.Draw(uiSize, stretchFactor);
            //pwrBarCircle.UpdateRect(uiSize * new Vector2(0.5f, 0.9f), uiSize * new Vector2(0.04f, 0.04f), Alignement.BOTTOMCENTER);
            //pwrBarCircle.Draw(uiSize, stretchFactor);
            Vector2 barSize = uiSize * new Vector2(0.03f, 0.2f);
            Vector2 center = uiSize * new Vector2(0.03f, 0.98f) - new Vector2(0, barSize.Y / 2);
            Vector2 gap = new Vector2(barSize.X * 1.5f, 0);
            hpBar.UpdateRect(center, barSize, new(0.5f));
            pwrBar.UpdateRect(center + gap, barSize, new(0.5f));
            SDrawing.DrawTextAlignedPro("HP", hpBar.GetPos(new(0.5f)) - hpBar.Transform(new Vector2(0, barSize.Y / 2)), hpBar.GetAngleDeg(), 60, 2, Demo.PALETTES.C(ColorIDs.Enemy), Demo.FONT.GetFont(), new(0.5f, 1f));
            hpBar.Draw(uiSize, stretchFactor);

            SDrawing.DrawTextAlignedPro("PWR", pwrBar.GetPos(new(0.5f)) - pwrBar.Transform(new Vector2(0, barSize.Y / 2)), pwrBar.GetAngleDeg(), 60, 2, Demo.PALETTES.C(ColorIDs.Player), Demo.FONT.GetFont(), new(0.5f, 1f));
            pwrBar.Draw(uiSize, stretchFactor);
            if (energyCore.IsCooldownActive())
            {
                Vector2 bottomRight = pwrBar.GetPos(new Vector2(0.75f, 0.5f)) + pwrBar.Transform(barSize / 2);// + new Vector2(20, 0);
                SDrawing.DrawTextAlignedPro("REBOOT", bottomRight, pwrBar.GetAngleDeg() - 90f, 90, 10, Demo.PALETTES.C(ColorIDs.Background2), Demo.FONT.GetFont(), new(0,1));
            }
            aimpointSkillDisplay.UpdateRect(uiSize * new Vector2(0.05f, 0.5f), uiSize * new Vector2(0.08f, 0.04f), new(0,0.5f));
            aimpointSkillDisplay.Draw(uiSize, stretchFactor);
            //aimpointInputPanel.Draw();
            //topLeft = ammoBar.GetTopLeft();
            //UIHandler.DrawTextAlignedPro("AMMO", topLeft + offset, ammoBar.GetRotationDeg(), FontSize.LARGE, 2, ColorPalette.Cur.special1, Alignement.BOTTOMCENTER);
            //ammoBar.Draw();

            if (GetHealthPercentage() < 1f)
            {
                hpBarMini.Draw(uiSize, stretchFactor);
            }
            if (GetEnergyPercentage() < 1f)
            {
                
                pwrBarMini.Draw(uiSize, stretchFactor);
            }

            //Drawing.DrawCircleLines(ScreenHandler.TransformPositionToUI(collider.Pos), 50, 5, WHITE, 8);
            //Drawing.DrawRect(new(500, 500), new(200, 50), new(0f, 0.5f), angle * RAD2DEG, VIOLET);
            //Drawing.DrawCircleOutlineBar(GAMELOOP.UICenter(), 200, -90f, 20, GetEnergyPercentage(), YELLOW);
            //Vector2 tl = collider.Origin + new Vector2(-size, -size*2f);
            //Vector2 bs = new Vector2(size * 2f, size * 0.2f) * ScreenHandler.GAME_TO_UI;
            //UIHandler.DrawBar(tl * ScreenHandler.GAME_TO_UI, bs, GetHealthPercentage(), ColorPalette.Cur.enemy2, ColorPalette.Cur.neutral, BarType.LEFTRIGHT);

            //Vector2 c = uiSize * new Vector2(0.5f, 0.5f);
            //Vector2 g = uiSize * new Vector2(0.15f, 0f);
            //Vector2 lineGap = uiSize * new Vector2(0f, 0.07f);
            //float r = uiSize.X * 0.05f;
            //float lt = r * 0.1f;
            //float startAngleDeg = 180;
            //float endAngleDeg = 0;
            //float endAngleDeg = Vec.AngleDeg(Vec.Normalize(GAMELOOP.MOUSE_POS_UI - c));

            //Drawing.DrawCircleSector(c + g * 2, r, startAngleDeg, endAngleDeg, 24, WHITE);
            //Drawing.DrawCircleSectorLinesEx(c + g * 2, r, startAngleDeg, endAngleDeg, lt, ORANGE, true, 8);
            //
            //Drawing.DrawRingFilled(c - g * 2, r * 0.5f, r, startAngleDeg, endAngleDeg, WHITE, 10);
            //Drawing.DrawRingLinesEx(c - g * 2, r * 0.5f, r, startAngleDeg, endAngleDeg, lt, ORANGE, 8);
            //
            //Vector2 textSize = uiSize * new Vector2(0.35f, 0.05f);
            //UIHandler.DrawTextAligned(String.Format("SA: {0}", startAngleDeg), c - lineGap, textSize, 1, WHITE);
            //UIHandler.DrawTextAligned(String.Format("EA: {0}", endAngleDeg), c - lineGap * 2, textSize, 1, WHITE);
            //UIHandler.DrawTextAligned(String.Format("Dif: {0}", endAngleDeg - startAngleDeg), c - lineGap * 3, textSize, 1, WHITE);
            //
            //UIHandler.DrawTextAligned(String.Format("SAW: {0}", Wrap(startAngleDeg, -360, 360)), c + lineGap, textSize, 1, WHITE);
            //UIHandler.DrawTextAligned(String.Format("EAW: {0}", Wrap(endAngleDeg, -360, 360)), c + lineGap * 2, textSize, 1, WHITE);
            //UIHandler.DrawTextAligned(String.Format("DifW: {0}", Wrap(endAngleDeg, -360, 360) - Wrap(startAngleDeg, -360, 360)), c + lineGap * 3,textSize, 1, WHITE);
        }


        public override Vector2 GetCameraPosition(Vector2 camPos, float dt, float smoothness = 1f, float boundary = 0f)
        {
            if (dt <= 0f) return collider.Pos;

            Vector2 targetPos = collider.Pos;
            if (IsMovementSlow()) { targetPos = slowPos; }
            else if (IsMovementBoosting()) { targetPos = collider.Pos + collider.Vel * 1.2f;}
            //else { targetPos = collider.Pos; }
            return SVec.Lerp(camPos, targetPos, dt * smoothness);
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
                //hpBarMini.SetSize(new Vector2(size * 2f, size * 0.2f) * ScreenHandler.GAME_TO_UI);
                //pwrBarMini.SetSize(new Vector2(size * 2f, size * 0.2f) * ScreenHandler.GAME_TO_UI);
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
            ScreenHandler.CAMERA.AddCameraOrderChain("player boost", false, new CameraOrder(0.4f, 1f, 1.1f));
        }
        private void BoostEnded()
        {
            InputHandler.RemoveVibration(0, "boost");
            ScreenHandler.CAMERA.AddCameraOrderChain("player boost", true, new CameraOrder(0.4f, 1.1f, 1f));
        }

        private void CoreTrigger(string triggerName, params float[] values)
        {
            if (triggerName == "emptied")
            {
                pwrBarMini.SetColors(Demo.PALETTES.C(ColorIDs.Flash));
                pwrBar.SetColors(Demo.PALETTES.C(ColorIDs.Flash));

                CoreParticle cp = new(collider.Pos, angle + PI / 2, Demo.PALETTES.C(ColorIDs.Energy));
                GAMELOOP.AddGameObject(cp);

                AudioHandler.PlaySFX("player pwr down", -1, -1, 0);
            }
            else if(triggerName == "reloaded")
            {
                pwrBarMini.SetColors(Demo.PALETTES.C(ColorIDs.Player));
                pwrBar.SetColors(Demo.PALETTES.C(ColorIDs.Player));

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
            Color color = Demo.PALETTES.C(ColorIDs.Player);
            Color offColor = Demo.PALETTES.C(ColorIDs.Energy);
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
