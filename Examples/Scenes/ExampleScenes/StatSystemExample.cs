using Raylib_cs;
using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Input;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using ShapeEngine.StaticLib;
using ShapeEngine.Stats;
using ShapeEngine.Text;

namespace Examples.Scenes.ExampleScenes;

/// <summary>
/// Contains compact example scenarios for the stat system.
/// </summary>
public static class StatExamples
{
    /// <summary>
    /// Demonstrates an incremental game production stat with flat upgrades and multiplicative prestige.
    /// </summary>
    /// <returns>The calculated production value.</returns>
    public static float IncrementalProduction()
    {
        StatId production = 1;
        var stats = new StatSet();
        stats.AddStat(new Stat(production, 10f, "Production"));
        stats.AddSource(new StatModifierSource(100, StatModifierSourceType.Upgrade, "Conveyor Upgrade", modifiers: StatModifier.Flat(production, 5f)));
        stats.AddSource(new StatModifierSource(101, StatModifierSourceType.Passive, "Prestige", modifiers: StatModifier.MultiplicativePercent(production, 1f)));
        
        stats.TryGetValue(production, out float value);
        return value;
    }

    /// <summary>
    /// Demonstrates a survivor movement stat with a timed slow and temporary haste.
    /// </summary>
    /// <returns>The movement value after both effects are applied.</returns>
    public static float SurvivorMoveSpeed()
    {
        StatId moveSpeed = 2;
        var stats = new StatSet();
        stats.AddStat(new Stat(moveSpeed, 100f, "Move Speed", minValue: 0f));
        stats.AddSource(new TimedStatModifierSource(200, StatModifierSourceType.Debuff, 4f, "Slow", modifiers: StatModifier.AdditivePercent(moveSpeed, -0.3f)));
        stats.AddSource(new TimedStatModifierSource(201, StatModifierSourceType.Buff, 2f, "Haste", modifiers: StatModifier.MultiplicativePercent(moveSpeed, 0.2f)));
        
        stats.TryGetValue(moveSpeed, out float value);
        return value;
    }

    /// <summary>
    /// Demonstrates ARPG-style loot damage with flat, additive, and multiplicative layers.
    /// </summary>
    /// <returns>The calculated damage value.</returns>
    public static float LootDamage()
    {
        StatId damage = 3;
        var stats = new StatSet();
        stats.AddStat(new Stat(damage, 20f, "Damage", minValue: 0f));
        stats.AddSource(new StatModifierSource(300, StatModifierSourceType.Equipment, "Sword Affix", modifiers: StatModifier.Flat(damage, 12f)));
        stats.AddSource(new StatModifierSource(301, StatModifierSourceType.Equipment, "Damage Roll", modifiers: StatModifier.AdditivePercent(damage, 0.5f)));
        stats.AddSource(new StatModifierSource(302, StatModifierSourceType.Equipment, "Unique Bonus", modifiers: StatModifier.MultiplicativePercent(damage, 0.25f)));
        
        stats.TryGetValue(damage, out float value);
        return value;
    }

    /// <summary>
    /// Demonstrates a stackable timed buff with max stacks and duration refresh on reapply.
    /// </summary>
    /// <returns>The calculated attack speed value at max stacks.</returns>
    public static float StackableHaste()
    {
        StatId attackSpeed = 4;
        var stats = new StatSet();
        stats.AddStat(new Stat(attackSpeed, 1f, "Attack Speed", minValue: 0f));
        stats.AddSource(new StackableStatModifierSource(
            400,
            StatModifierSourceType.Buff,
            3f,
            3,
            name: "Haste",
            modifiers: StatModifier.AdditivePercent(attackSpeed, 0.1f)));
        stats.AddStacks(400, 4);
        
        stats.TryGetValue(attackSpeed, out float value);
        return value;
    }

    /// <summary>
    /// Runs small example-driven checks that cover the intended calculation and source behavior.
    /// </summary>
    /// <returns>True if all checks pass.</returns>
    public static bool Verify()
    {
        const float tolerance = 0.0001f;

        static bool Close(float a, float b) => MathF.Abs(a - b) <= tolerance;

        StatId test = 10;
        var stats = new StatSet();
        stats.AddStat(new Stat(test, 10f, "Test", minValue: 0f, maxValue: 100f));
        stats.AddSource(new StatModifierSource(1, StatModifierSourceType.Upgrade, modifiers: StatModifier.Flat(test, 5f)));
        stats.AddSource(new StatModifierSource(2, StatModifierSourceType.Passive, modifiers: StatModifier.AdditivePercent(test, 1f)));
        stats.AddSource(new StatModifierSource(3, StatModifierSourceType.Passive, modifiers: StatModifier.MultiplicativePercent(test, 0.5f)));
        
        stats.TryGetValue(test, out float value);
        if (!Close(value, 45f)) return false;

        stats.AddSource(new StatModifierSource(4, StatModifierSourceType.Custom, modifiers: StatModifier.Max(test, 40f)));
        stats.TryGetValue(test, out value);
        if (!Close(value, 40f)) return false;

        stats.AddSource(new StatModifierSource(5, StatModifierSourceType.Custom, modifiers: StatModifier.Override(test, 7f, priority: 1)));
        stats.TryGetValue(test, out value);
        if (!Close(value, 7f)) return false;

        stats.RemoveSource(5);
        stats.TryGetValue(test, out value);
        if (!Close(value, 40f)) return false;

        stats.AddSource(new TimedStatModifierSource(6, StatModifierSourceType.Buff, 1f, modifiers: StatModifier.Flat(test, 10f)));
        stats.Update(2f);
        if (stats.TryGetSource(6, out _)) return false;

        stats.AddSource(new StackableStatModifierSource(7, StatModifierSourceType.Buff, 5f, 3, modifiers: StatModifier.Flat(test, 2f)));
        if (stats.AddStacks(7, 10) != 2) return false;
        if (!stats.TryGetSource(7, out var stackable) || stackable.Stacks != 3) return false;
        var remaining = stackable.RemainingDuration;
        stats.Update(1f);
        stats.AddSource(new StackableStatModifierSource(7, StatModifierSourceType.Buff, 5f, 3, modifiers: StatModifier.Flat(test, 2f)));
        if (!stats.TryGetSource(7, out stackable) || stackable.RemainingDuration <= remaining - 1f) return false;

        return true;
    }
}

public class StatsSystemExample : ExampleScene
{
    #region Nested Types

    private enum PowerupColorType
    {
        Green,
        Blue,
        Red,
        Rainbow
    }

    private readonly struct StatDef
    {
        public readonly StatId Id;
        public readonly string Name;
        public readonly string Abbr;
        public readonly string Desc;
        public readonly float BaseValue;
        public readonly float MinValue;
        public readonly float MaxValue;

        public StatDef(StatId id, string name, string abbr, string desc, float baseValue, float minValue, float maxValue)
        {
            Id = id;
            Name = name;
            Abbr = abbr;
            Desc = desc;
            BaseValue = baseValue;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }

    private sealed class PickupPopup
    {
        public Vector2 Position;
        public string Text;
        public float Duration;
        public float Remaining;
        public ColorRgba Color;

        public PickupPopup(Vector2 position, string text, float duration, ColorRgba color)
        {
            Position = position;
            Text = text;
            Duration = duration;
            Remaining = duration;
            Color = color;
        }

        public bool Update(float dt)
        {
            Remaining -= dt;
            Position += new Vector2(0f, -30f * dt);
            return Remaining <= 0f;
        }

        public float GetFade()
        {
            if (Duration <= 0f) return 0f;
            return ShapeMath.Clamp(Remaining / Duration, 0f, 1f);
        }
    }

    private sealed class PlayerShip : ICameraFollowTarget
    {
        private readonly InputAction iaMoveHor;
        private readonly InputAction iaMoveVer;
        private readonly InputActionTree inputActionTree;

        private readonly PaletteColor hullColor = Colors.PcCold;
        private readonly PaletteColor outlineColor = Colors.PcLight;
        private readonly PaletteColor cockpitColor = Colors.PcWarm;

        private Vector2 movementDir = new(0f, 1f);
        private Vector2 currentVelocity = Vector2.Zero;

        public Circle Hull { get; private set; }
        public float FacingDeg { get; private set; } = 90f;

        public PlayerShip(Vector2 pos, float radius)
        {
            Hull = new Circle(pos, radius);

            var defaultSettings = new InputActionSettings();
            var modifierKeySetGpReversed = new ModifierKeySet(ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            var modifierKeySetMouseReversed = new ModifierKeySet(ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);

            var moveHorKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var moveHorGP = new InputTypeGamepadJoyAxis(ShapeGamepadJoyAxis.LEFT_X, 0.15f, false, modifierKeySetGpReversed);
            var moveHorMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, modifierKeySetMouseReversed);
            iaMoveHor = new InputAction(defaultSettings, moveHorKB, moveHorGP, moveHorMW);

            var moveVerKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var moveVerGP = new InputTypeGamepadJoyAxis(ShapeGamepadJoyAxis.LEFT_Y, 0.15f, false, modifierKeySetGpReversed);
            var moveVerMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, modifierKeySetMouseReversed);
            iaMoveVer = new InputAction(defaultSettings, moveVerKB, moveVerGP, moveVerMW);

            inputActionTree = [iaMoveHor, iaMoveVer];
        }

        public void Reset(Vector2 pos, float radius)
        {
            Hull = new Circle(pos, radius);
            movementDir = new Vector2(0f, 1f);
            FacingDeg = 90f;
        }

        public void GetInputDescription(InputDeviceType inputDeviceType, out string top, out string bottom)
        {
            if (inputDeviceType == InputDeviceType.Mouse)
            {
                top = "Move Horizontal [Mouse X]";
                bottom = "Move Vertical [Mouse Y]";
                return;
            }

            string hor = iaMoveHor.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
            string ver = iaMoveVer.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
            
            top = $"Move Horizontal [{hor}]";
            bottom = $"Move Vertical [{ver}]";
            
        }

        public void Update(float dt, float moveSpeed, float turningSpeed)
        {
            inputActionTree.CurrentGamepad = Game.Instance.Input.GamepadManager.LastUsedGamepad;
            inputActionTree.Update(dt);

            Vector2 moveInput;
            if (Game.Instance.Input.CurrentInputDeviceType == InputDeviceType.Mouse)
            {
                moveInput = ExampleScene.CalculateMouseMovementDirection(GameloopExamples.Instance.GameScreenInfo.MousePos, GameloopExamples.Instance.Camera);
            }
            else
            {
                moveInput = new Vector2(iaMoveHor.State.AxisRaw, iaMoveVer.State.AxisRaw);
            }

            if (moveInput.LengthSquared() > 0f)
            {
                movementDir = moveInput.Normalize();

                float targetAngle = MathF.Atan2(movementDir.Y, movementDir.X) * ShapeMath.RADTODEG;
                float maxTurnStep = turningSpeed * dt;

                var velDir = currentVelocity.Normalize();
                var angle = velDir.AngleDeg();
                FacingDeg = RotateTowardsDeg(angle, targetAngle, maxTurnStep);
                currentVelocity = ShapeVec.VecFromAngleDeg(FacingDeg) * moveSpeed;
                
            }
            else
            {
                currentVelocity *= 0.9f;//simple drag
            }
            
            //move ship based on veloctiy
            Hull = new Circle(Hull.Center + currentVelocity * dt, Hull.Radius);
        }

        public void Draw()
        {
            Vector2 forward = Vector2.UnitX.RotateDeg(FacingDeg);
            var rightThruster = forward.RotateDeg(-25f);
            var leftThruster = forward.RotateDeg(25f);

            var rightThrusterCircle = new Circle(Hull.Center - rightThruster * Hull.Radius, Hull.Radius / 6f);
            var leftThrusterCircle = new Circle(Hull.Center - leftThruster * Hull.Radius, Hull.Radius / 6f);
            var cockpitCircle = new Circle(Hull.Center + forward * Hull.Radius * 0.66f, Hull.Radius * 0.33f);

            rightThrusterCircle.Draw(outlineColor.ColorRgba, 0.25f);
            leftThrusterCircle.Draw(outlineColor.ColorRgba, 0.25f);
            Hull.Draw(hullColor.ColorRgba);
            cockpitCircle.Draw(cockpitColor.ColorRgba, 0.25f);
            Hull.DrawLines(4f, outlineColor.ColorRgba);
        }

        public void FollowStarted() { }
        public void FollowEnded() { }
        public Vector2 GetCameraFollowPosition() => Hull.Center;

        private static float RotateTowardsDeg(float current, float target, float maxStep)
        {
            float delta = DeltaAngleDeg(current, target);
            if (MathF.Abs(delta) <= maxStep) return target;
            return current + MathF.Sign(delta) * maxStep;
        }

        private static float DeltaAngleDeg(float from, float to)
        {
            float delta = (to - from) % 360f;
            if (delta > 180f) delta -= 360f;
            if (delta < -180f) delta += 360f;
            return delta;
        }
    }

    private sealed class Powerup
    {
        public Circle Area;
        public readonly PowerupColorType ColorType;
        public readonly TimedStatModifierSource Source;
        public readonly bool IsStrong;
        public float RainbowPhase;

        public Powerup(Vector2 pos, float radius, PowerupColorType colorType, TimedStatModifierSource source, bool isStrong)
        {
            Area = new Circle(pos, radius);
            ColorType = colorType;
            Source = source;
            IsStrong = isStrong;
            RainbowPhase = Rng.Instance.RandF(0f, 10f);
        }

        public void Update(float dt)
        {
            RainbowPhase += dt * (IsStrong ? 2.5f : 1.25f);
        }

        public ColorRgba GetDrawColor()
        {
            return ColorType switch
            {
                PowerupColorType.Green => ColorRgba.LawnGreen,
                PowerupColorType.Blue => ColorRgba.DodgerBlue,
                PowerupColorType.Red => ColorRgba.IndianRed,
                PowerupColorType.Rainbow => RainbowColor(RainbowPhase),
                _ => Colors.Light
            };
        }

        private static ColorRgba RainbowColor(float t)
        {
            float r = 0.5f + 0.5f * MathF.Sin(t + 0f);
            float g = 0.5f + 0.5f * MathF.Sin(t + 2.094f);
            float b = 0.5f + 0.5f * MathF.Sin(t + 4.188f);
            return new ColorRgba((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)255);
        }
    }

    private readonly struct PowerupBlueprint
    {
        public readonly string Name;
        public readonly string Description;
        public readonly float Duration;
        public readonly StatModifierSourceType SourceType;
        public readonly StatModifier[] Modifiers;

        public PowerupBlueprint(string name, string description, float duration, StatModifierSourceType sourceType, params StatModifier[] modifiers)
        {
            Name = name;
            Description = description;
            Duration = duration;
            SourceType = sourceType;
            Modifiers = modifiers;
        }
    }

    #endregion

    #region Constants / Stat Ids

    private static readonly StatId StatSpeed = new(1);
    private static readonly StatId StatTurningSpeed = new(2);
    private static readonly StatId StatCollectionRadius = new(3);

    #endregion

    #region Fields

    private readonly Font font;
    private readonly ShapeCamera camera;
    private readonly CameraFollowerSingle follower;
    private readonly PlayerShip ship = new(new Vector2(0f), 30f);

    private readonly StatSet shipStats = new();
    private readonly List<Powerup> powerups = new();
    private readonly List<PickupPopup> popups = new();

    private readonly List<PowerupBlueprint> greenBlueprints = new();
    private readonly List<PowerupBlueprint> blueBlueprints = new();
    private readonly List<PowerupBlueprint> redBlueprints = new();

    private readonly TextFont hudFont;
    private readonly TextFont popupFont;

    private readonly StatDef[] statDefs =
    [
        new StatDef(StatSpeed, "Speed", "SPD", "Ship movement speed in units per second.", 500f, 120f, 1400f),
        new StatDef(StatTurningSpeed, "Turning Speed", "TRN", "Maximum turn rate in degrees per second.", 260f, 40f, 1080f),
        new StatDef(StatCollectionRadius, "Collection Radius", "COL", "Radius used to collect powerups.", 70f, 20f, 280f),
    ];

    private const int MaxPowerups = 28;
    private const float MinSpawnDistance = 200f;
    private const float MaxSpawnDistance = 1500f;
    private const float RelocationDistance = 2600f;
    private const float PowerupRadius = 18f;
    private const float PopupDuration = 1.4f;

    private uint nextSourceId = 1000u;

    #endregion

    public StatsSystemExample()
    {
        Title = "Stats System Example";

        font = GameloopExamples.Instance.GetFont(FontIDs.JetBrains);
        hudFont = new TextFont(font, 1f, ColorRgba.White);
        popupFont = new TextFont(font, 1f, ColorRgba.White);

        camera = new ShapeCamera();
        follower = new CameraFollowerSingle(0, 100, 500);
        camera.Follower = follower;

        SetupStats();
        BuildModifierLibraries();
        FillPowerupsToCap();
    }

    protected override void OnActivate(Scene oldScene)
    {
        GameloopExamples.Instance.Camera = camera;
        GameloopExamples.Instance.MouseControlEnabled = false;

        follower.Reset();
        follower.SetTarget(ship);
        UpdateFollower(GameloopExamples.Instance.UIScreenInfo.Area.Size.Min());
    }

    protected override void OnDeactivate()
    {
        GameloopExamples.Instance.ResetCamera();
        GameloopExamples.Instance.MouseControlEnabled = true;
    }

    public override void Reset()
    {
        ship.Reset(Vector2.Zero, 30f);
        shipStats.ClearSources();

        powerups.Clear();
        popups.Clear();
        nextSourceId = 1000u;

        camera.Reset();
        follower.Reset();
        follower.SetTarget(ship);

        FillPowerupsToCap();
    }
    
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        shipStats.Update(time.Delta);

        shipStats.TryGetValue(StatSpeed, out float speed);
        shipStats.TryGetValue(StatTurningSpeed, out float turningSpeed);
        shipStats.TryGetValue(StatCollectionRadius, out float collectionRadius);

        ship.Update(time.Delta, speed, turningSpeed);
        UpdateFollower(ui.Area.Size.Min());

        for (int i = powerups.Count - 1; i >= 0; i--)
        {
            var p = powerups[i];
            p.Update(time.Delta);

            float sqrDis = (p.Area.Center - ship.Hull.Center).LengthSquared();
            if (sqrDis <= collectionRadius * collectionRadius)
            {
                shipStats.AddSource(p.Source);

                string popupText = $"{p.Source.Name} {MathF.Round(p.Source.Duration):0}s";
                popups.Add(new PickupPopup(p.Area.Center, popupText, PopupDuration, p.GetDrawColor()));

                powerups.RemoveAt(i);
                continue;
            }

            if (sqrDis > RelocationDistance * RelocationDistance)
                p.Area = p.Area.SetPosition(GetRandomPosAroundShip(MinSpawnDistance, MaxSpawnDistance));
        }

        for (int i = popups.Count - 1; i >= 0; i--)
        {
            if (popups[i].Update(time.Delta))
                popups.RemoveAt(i);
        }

        FillPowerupsToCap();
    }

    protected override void OnDrawGameExample(ScreenInfo game)
    {
        DrawPowerups();
        DrawCollectionRadius();
        ship.Draw();
        DrawPickupsPopups();
    }

    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        var bottomPanel = ui.Area.ApplyMargins(0.2f, 0.2f, 0.8f, 0.01f);
        var topPanel = ui.Area.ApplyMargins(0.05f, 0.05f, 0.12f, 0.82f);
        DrawHud(bottomPanel, topPanel);
        DrawInputDescription(GameloopExamples.Instance.UIRects.GetRect("bottom right"));
        // DrawCameraInfo(GameloopExamples.Instance.UIRects.GetRect("bottom right"));
    }

    private void SetupStats()
    {
        foreach (var def in statDefs)
        {
            shipStats.AddStat(new Stat(def.Id, def.BaseValue, def.Name, def.Abbr, def.Desc, def.MinValue, def.MaxValue));
        }
    }

    private void BuildModifierLibraries()
    {
        greenBlueprints.Add(new PowerupBlueprint("Speedup", "Flat speed increase.", 8f, StatModifierSourceType.Buff, StatModifier.Flat(StatSpeed, 120f)));
        greenBlueprints.Add(new PowerupBlueprint("Control Matrix", "Additive turn bonus.", 9f, StatModifierSourceType.Buff, StatModifier.AdditivePercent(StatTurningSpeed, 0.22f)));
        greenBlueprints.Add(new PowerupBlueprint("Magnet Coil", "Collection radius additive bonus.", 10f, StatModifierSourceType.Buff, StatModifier.AdditivePercent(StatCollectionRadius, 0.35f)));
        greenBlueprints.Add(new PowerupBlueprint("Thruster Burst", "Multiplicative speed boost.", 6f, StatModifierSourceType.Buff, StatModifier.MultiplicativePercent(StatSpeed, 0.28f)));
        greenBlueprints.Add(new PowerupBlueprint("Precision Gyro", "Improves speed and turning.", 8f, StatModifierSourceType.Buff, StatModifier.Flat(StatSpeed, 80f), StatModifier.AdditivePercent(StatTurningSpeed, 0.16f)));
        greenBlueprints.Add(new PowerupBlueprint("Collector Suite", "Minimum collection radius.", 12f, StatModifierSourceType.Buff, StatModifier.Min(StatCollectionRadius, 140f, priority: 1)));
        greenBlueprints.Add(new PowerupBlueprint("Pilot Override", "High turning override.", 4f, StatModifierSourceType.Buff, StatModifier.Override(StatTurningSpeed, 720f, priority: 2)));

        blueBlueprints.Add(new PowerupBlueprint("Overclocked RCS", "Turn up, speed down.", 8f, StatModifierSourceType.Custom, StatModifier.AdditivePercent(StatTurningSpeed, 0.35f), StatModifier.AdditivePercent(StatSpeed, -0.15f)));
        blueBlueprints.Add(new PowerupBlueprint("Collector Drag", "Collection up, turning down.", 10f, StatModifierSourceType.Custom, StatModifier.Flat(StatCollectionRadius, 45f), StatModifier.Flat(StatTurningSpeed, -55f)));
        blueBlueprints.Add(new PowerupBlueprint("High Gear", "Speed up, handling down.", 7f, StatModifierSourceType.Custom, StatModifier.MultiplicativePercent(StatSpeed, 0.42f), StatModifier.AdditivePercent(StatTurningSpeed, -0.28f)));
        blueBlueprints.Add(new PowerupBlueprint("Hard Clamp", "Speed up, collection max down.", 7f, StatModifierSourceType.Custom, StatModifier.Flat(StatSpeed, 180f), StatModifier.Max(StatCollectionRadius, 90f, priority: 1)));
        blueBlueprints.Add(new PowerupBlueprint("Stability Anchor", "Turning min up, speed max down.", 9f, StatModifierSourceType.Custom, StatModifier.Min(StatTurningSpeed, 300f, priority: 1), StatModifier.Max(StatSpeed, 620f, priority: 1)));
        blueBlueprints.Add(new PowerupBlueprint("Greedy Intake", "Collection override, move penalties.", 6f, StatModifierSourceType.Custom, StatModifier.Override(StatCollectionRadius, 220f, priority: 2), StatModifier.AdditivePercent(StatSpeed, -0.18f), StatModifier.AdditivePercent(StatTurningSpeed, -0.18f)));

        redBlueprints.Add(new PowerupBlueprint("Jammed Thrusters", "Flat speed reduction.", 8f, StatModifierSourceType.Debuff, StatModifier.Flat(StatSpeed, -140f)));
        redBlueprints.Add(new PowerupBlueprint("Heavy Hull", "Multiplicative speed penalty.", 9f, StatModifierSourceType.Debuff, StatModifier.MultiplicativePercent(StatSpeed, -0.30f)));
        redBlueprints.Add(new PowerupBlueprint("Gyro Failure", "Turning reduction.", 7f, StatModifierSourceType.Debuff, StatModifier.AdditivePercent(StatTurningSpeed, -0.45f)));
        redBlueprints.Add(new PowerupBlueprint("Sensor Blindness", "Collection max clamp.", 10f, StatModifierSourceType.Debuff, StatModifier.Max(StatCollectionRadius, 55f, priority: 2)));
        redBlueprints.Add(new PowerupBlueprint("Control Lock", "Turning override low.", 4f, StatModifierSourceType.Debuff, StatModifier.Override(StatTurningSpeed, 65f, priority: 3)));
        redBlueprints.Add(new PowerupBlueprint("System Crash", "All stats penalized.", 6f, StatModifierSourceType.Debuff, StatModifier.AdditivePercent(StatSpeed, -0.22f), StatModifier.AdditivePercent(StatTurningSpeed, -0.22f), StatModifier.AdditivePercent(StatCollectionRadius, -0.22f)));
    }

    private void FillPowerupsToCap()
    {
        while (powerups.Count < MaxPowerups)
            powerups.Add(CreateRandomPowerup());
    }

    private Powerup CreateRandomPowerup()
    {
        var pos = GetRandomPosAroundShip(MinSpawnDistance, MaxSpawnDistance);
        float roll = Rng.Instance.RandF();

        PowerupColorType colorType;
        bool rainbow = false;

        if (roll < 0.52f) colorType = PowerupColorType.Green;
        else if (roll < 0.82f) colorType = PowerupColorType.Blue;
        else if (roll < 0.97f) colorType = PowerupColorType.Red;
        else { colorType = PowerupColorType.Rainbow; rainbow = true; }

        var source = CreateSourceFor(colorType, rainbow);
        float radius = rainbow ? PowerupRadius * 1.3f : PowerupRadius;

        return new Powerup(pos, radius, colorType, source, rainbow);
    }

    private TimedStatModifierSource CreateSourceFor(PowerupColorType colorType, bool rainbow)
    {
        if (rainbow)
        {
            float familyRoll = Rng.Instance.RandF();
            if (familyRoll < 0.5f) colorType = PowerupColorType.Green;
            else if (familyRoll < 0.8f) colorType = PowerupColorType.Blue;
            else colorType = PowerupColorType.Red;
        }

        PowerupBlueprint bp = colorType switch
        {
            PowerupColorType.Green => greenBlueprints[Rng.Instance.RandI(0, greenBlueprints.Count)],
            PowerupColorType.Blue => blueBlueprints[Rng.Instance.RandI(0, blueBlueprints.Count)],
            PowerupColorType.Red => redBlueprints[Rng.Instance.RandI(0, redBlueprints.Count)],
            _ => greenBlueprints[Rng.Instance.RandI(0, greenBlueprints.Count)]
        };

        uint id = nextSourceId++;
        float duration = bp.Duration * (rainbow ? Rng.Instance.RandF(1.25f, 1.7f) : Rng.Instance.RandF(0.9f, 1.15f));

        StatModifier[] modifiers = new StatModifier[bp.Modifiers.Length];
        for (int i = 0; i < bp.Modifiers.Length; i++)
        {
            var m = bp.Modifiers[i];
            float scale = rainbow ? Rng.Instance.RandF(1.35f, 2.05f) : Rng.Instance.RandF(0.9f, 1.1f);
            modifiers[i] = m with
            {
                Amount = m.Amount * scale,
                SourceId = id,
                Priority = rainbow ? m.Priority + 1 : m.Priority
            };
        }

        string name = rainbow ? $"Rainbow {bp.Name}" : bp.Name;
        string desc = rainbow ? $"[Strong] {bp.Description}" : bp.Description;
        StatModifierSourceType sourceType = colorType == PowerupColorType.Red ? StatModifierSourceType.Debuff : StatModifierSourceType.Buff;

        return new TimedStatModifierSource(id, sourceType, duration, name, desc, modifiers);
    }

    private Vector2 GetRandomPosAroundShip(float minDis, float maxDis)
    {
        Vector2 dir = Rng.Instance.RandVec2().Normalize();
        float dis = Rng.Instance.RandF(minDis, maxDis);
        return ship.Hull.Center + dir * dis;
    }

    private void DrawPowerups()
    {
        foreach (var p in powerups)
        {
            ColorRgba c = p.GetDrawColor();
            p.Area.Draw(c, 0.2f);
            p.Area.DrawLines(3f, c.ChangeAlpha(220));
            if (p.ColorType == PowerupColorType.Rainbow)
                new Circle(p.Area.Center, p.Area.Radius * 1.4f).DrawLines(2f, c.ChangeAlpha(120));
        }
    }

    private void DrawCollectionRadius()
    {
        shipStats.TryGetValue(StatCollectionRadius, out float radius);
        new Circle(ship.Hull.Center, radius).DrawLines(2f * camera.ZoomFactor, Colors.Special.ChangeAlpha(120));
    }

    private void DrawPickupsPopups()
    {
        foreach (var popup in popups)
        {
            float f = popup.GetFade();
            var c = popup.Color.ChangeAlpha((byte)(255f * f));
            popupFont.ColorRgba = c;
            popupFont.FontSpacing = 1f;

            var rect = new Rect(popup.Position, new Size(260f, 36f), new AnchorPoint(0.5f));
            popupFont.DrawTextWrapNone(popup.Text, rect, new AnchorPoint(0.5f));
        }
    }

    private void DrawHud(Rect bottomPanel, Rect topPanel)
    {
        bottomPanel.Draw(ColorRgba.Black.SetAlpha(150));
        // topPanel.Draw(ColorRgba.DarkGray.ChangeAlpha(210));

        var bottomRows = bottomPanel.SplitV(3);
        DrawStatRow(bottomRows[0], StatSpeed);
        DrawStatRow(bottomRows[1], StatTurningSpeed);
        DrawStatRow(bottomRows[2], StatCollectionRadius);
        
        var topSplit = topPanel.SplitV(2);
        DrawPowerupLegend(topSplit[0]);
        DrawActiveSourceInfo(topSplit[1]);
    }

    private void DrawStatRow(Rect row, StatId statId)
    {
        if (!shipStats.TryGetStat(statId, out var stat)) return;

        row = row.ApplyMargins(0.01f, 0.01f, 0.0f, 0.0f);
        // Rect labelRect = row.ApplyMargins(0.02f, 0.66f, 0.15f, 0.55f);
        // Rect barBgRect = row.ApplyMargins(0.02f, 0.02f, 0.52f, 0.20f);
        // Rect valueRect = row.ApplyMargins(0.69f, 0.02f, 0.15f, 0.55f);
        // Rect modRect = row.ApplyMargins(0.69f, 0.02f, 0.55f, 0.15f);
        
        var splitH = row.SplitH(0.7f);
        var left = splitH.left.ApplyMargins(0f, 0f, 0.12f, 0.12f);
        Rect labelRect = left;
        Rect barBgRect = left;
        var splitV = splitH.right.SplitV(0.5f);
        var valueRect = splitV.top;
        var modRect = splitV.bottom;

        float min = stat.MinValue ?? stat.BaseValue;
        float max = stat.MaxValue ?? (stat.BaseValue + 1f);
        float fill = max > min ? ShapeMath.Clamp(ShapeMath.GetFactor(stat.Value, min, max), 0f, 1f) : 1f;

        var barFill = barBgRect.SetSize(new Size(barBgRect.Size.Width * fill, barBgRect.Size.Height), new AnchorPoint(0f, 0.5f));
        barBgRect.Draw(Colors.Medium.ChangeAlpha(100));
        barFill.Draw(Colors.Highlight);

        int affectingModifiers = CountAffectingModifiers(statId);

        hudFont.ColorRgba = ColorRgba.Black;
        hudFont.DrawTextWrapNone($"{stat.Name} ({stat.NameAbbreviation})", labelRect, new AnchorPoint(0.5f, 0.5f));

        hudFont.ColorRgba = Colors.Special;
        hudFont.DrawTextWrapNone($"{stat.Value:0.##} [{min:0.#}-{max:0.#}]", valueRect, new AnchorPoint(1f, 0.5f));

        hudFont.ColorRgba = Colors.Warm;
        hudFont.DrawTextWrapNone($"Mods: {affectingModifiers}", modRect, new AnchorPoint(1f, 0.5f));
    }

    private int CountAffectingModifiers(StatId statId)
    {
        int count = 0;
        foreach (var source in shipStats.Sources)
            foreach (var m in source.GetModifiers())
                if (m.Target == statId) count++;
        return count;
    }

    private void DrawPowerupLegend(Rect rect)
    {
        var cols = rect.SplitH(4);
        DrawLegendBox(cols[0], "Green: Positive",ColorRgba.DarkGreen);
        DrawLegendBox(cols[1], "Blue: Mixed", ColorRgba.DarkBlue);
        DrawLegendBox(cols[2], "Red: Negative", ColorRgba.DarkRed);
        DrawLegendBox(cols[3], "Rainbow: Strong ?", ColorRgba.DeepPink);
    }

    private void DrawLegendBox(Rect r, string text, ColorRgba c)
    {
        r.Draw(c);
        // r.DrawLines(2f, ColorRgba.Black);
        hudFont.ColorRgba = ColorRgba.LightGray;
        hudFont.DrawTextWrapNone(text, r, new AnchorPoint(0.5f, 0.5f));
    }

    private void DrawActiveSourceInfo(Rect rect)
    {
        int sourceCount = shipStats.Sources.Count;
        float totalRemaining = 0f;
        foreach (var s in shipStats.Sources)
            if (s.Duration > 0f) totalRemaining += s.RemainingDuration;

        string text = $"Active Sources: {sourceCount} | Avg Remaining: {(sourceCount > 0 ? totalRemaining / sourceCount : 0f):0.0}s | Powerups: {powerups.Count}/{MaxPowerups}";
        hudFont.ColorRgba = ColorRgba.Red;
        hudFont.DrawTextWrapNone(text, rect, new AnchorPoint(0.5f, 0.5f));
    }

    private void DrawInputDescription(Rect rect)
    {
        var rects = rect.SplitV(0.4f);
        var curDevice = Input.CurrentInputDeviceType;

        ship.GetInputDescription(curDevice, out string topText, out string bottomText);

        textFont.FontSpacing = 1f;
        textFont.ColorRgba = Colors.Light;
        textFont.DrawTextWrapNone(topText, rects.top, new(0.5f));

        textFont.ColorRgba = Colors.Light;
        textFont.DrawTextWrapNone(bottomText, rects.bottom, new(0.5f));
    }
    
    private void UpdateFollower(float size)
    {
        float minBoundary = 0.12f * size;
        float maxBoundary = 0.30f * size;
        var boundary = new Vector2(minBoundary, maxBoundary) * camera.ZoomFactor;

        shipStats.TryGetValue(StatSpeed, out float speed);
        follower.Speed = speed;
        follower.BoundaryDis = new(boundary);
    }
}
