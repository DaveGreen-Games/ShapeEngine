using Raylib_cs;

namespace ShapeEngine.Input;

/// <summary>
/// Manages multiple <see cref="GamepadDevice"/> instances, handling connection state,
/// device claiming, releasing, and tracking last used gamepads.
/// </summary>
public sealed class GamepadDeviceManager
{
    /// <summary>
    /// Event triggered when a gamepad's connection state changes.
    /// </summary>
    public event Action<GamepadDevice, bool>? OnGamepadConnectionChanged;
    /// <summary>
    /// Gets the maximum number of gamepads supported by this manager.
    /// </summary>
    public int MaxGamepads => gamepads.Length;
    
    private readonly GamepadDevice[] gamepads;
    /// <summary>
    /// List of <see cref="GamepadDevice"/> instances that registered input during the last update cycle,
    /// considering any filters or settings applied via <see cref="InputDeviceUsageDetectionSettings"/>.
    /// This list is cleared and repopulated on each update.
    /// </summary>
    public readonly List<GamepadDevice> LastUsedGamepads = [];
    /// <summary>
    /// List of <see cref="GamepadDevice"/> instances that registered input during the last update cycle,
    /// ignoring any filters or settings from <see cref="InputDeviceUsageDetectionSettings"/>.
    /// This provides a raw view of all input activity per update.
    /// </summary>
    public readonly List<GamepadDevice> LastUsedGamepadsRaw = [];

    /// <summary>
    /// The most recently used gamepad that registered input during the last update cycle,
    /// considering any filters or settings applied via <see cref="InputDeviceUsageDetectionSettings"/>.
    /// This is <c>null</c> if no gamepad was used.
    /// </summary>
    public GamepadDevice? LastUsedGamepad;
    
    /// <summary>
    /// The most recently used gamepad that registered input during the last update cycle,
    /// ignoring any filters or settings from <see cref="InputDeviceUsageDetectionSettings"/>.
    /// This is <c>null</c> if no gamepad was used.
    /// </summary>
    public GamepadDevice? LastUsedGamepadRaw;
    
    /// <summary>
    /// Event triggered when a gamepad button is pressed.
    /// </summary>
    public event Action<GamepadDevice, ShapeGamepadButton>? OnGamepadButtonPressed;
    /// <summary>
    /// Event triggered when a gamepad button is released.
    /// </summary>
    public event Action<GamepadDevice, ShapeGamepadButton>? OnGamepadButtonReleased;

    /// <summary>
    /// Gets the usage detection settings for all the gamepad input devices.
    /// </summary>
    public InputDeviceUsageDetectionSettings.GamepadSettings UsageDetectionSettings { get; private set; } = new();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GamepadDeviceManager"/> class.
    /// </summary>
    /// <param name="maxGamepads">Maximum number of gamepads to manage.</param>
    public GamepadDeviceManager(int maxGamepads = 8)
    {
        if (maxGamepads <= 0) maxGamepads = 1;
        gamepads = new GamepadDevice[maxGamepads];
        GamepadSetup();
    }

    /// <summary>
    /// Updates the state of all managed gamepads and checks for connection changes.
    /// </summary>
    public bool Update(float dt, bool wasOtherDeviceUsed)
    {
        return CheckGamepadConnections(dt, wasOtherDeviceUsed);
    }
    
    internal void ApplyInputDeviceChangeSettings(InputDeviceUsageDetectionSettings settings)
    {
        UsageDetectionSettings = settings.Gamepad;
        foreach (var gamepad in gamepads)
        {
            gamepad.OverrideInputDeviceChangeSettings(settings);
        }
    }
    
    #region Gamepad

    /// <summary>
    /// Gets a list of all currently connected gamepads.
    /// </summary>
    public List<GamepadDevice> GetConnectedGamepads()
    {
        var result = new List<GamepadDevice>();

        foreach (var gamepad in gamepads)
        {
            if(gamepad.Connected) result.Add(gamepad);
        }

        return result;
    }

    /// <summary>
    /// Gets a list of all gamepads managed by this manager.
    /// </summary>
    public List<GamepadDevice> GetAllGamepads()
    {
        return gamepads.ToList();
    }

    /// <summary>
    /// Gets a list of all available (connected and not claimed) gamepads.
    /// </summary>
    public List<GamepadDevice> GetAvailableGamepads()
    {
        var result = new List<GamepadDevice>();

        foreach (var gamepad in gamepads)
        {
            if(gamepad.Connected && gamepad.Available) result.Add(gamepad);
        }

        return result;
    }
    
    /// <summary>
    /// Checks if a gamepad exists at the specified index.
    /// </summary>
    public bool HasGamepad(int index) => index >= 0 && index < gamepads.Length;
    
    /// <summary>
    /// Checks if a gamepad is connected at the specified index.
    /// </summary>
    public bool IsGamepadConnected(int index) => HasGamepad(index) && gamepads[index].Connected;
    
    /// <summary>
    /// Gets the gamepad at the specified index, or null if not present.
    /// </summary>
    public GamepadDevice? GetGamepad(int index)
    {
        if (!HasGamepad(index)) return null;
        return gamepads[index];
    }

    /// <summary>
    /// Requests an available gamepad, optionally preferring a specific index.
    /// </summary>
    /// <param name="preferredIndex">Preferred gamepad index, or -1 for any available.</param>
    /// <returns>The claimed gamepad, or null if none available.</returns>
    public GamepadDevice? RequestGamepad(int preferredIndex = -1)
    {
        var preferredGamepad = GetGamepad(preferredIndex);
        if (preferredGamepad is { Connected: true, Available: true })
        {
            preferredGamepad.Claim();
            return preferredGamepad;
        }

        foreach (var gamepad in gamepads)
        {
            if (gamepad is { Connected: true, Available: true })
            {
                gamepad.Claim();
                return gamepad;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Returns (frees) the gamepad at the specified index.
    /// </summary>
    public void ReturnGamepad(int index) => GetGamepad(index)?.Free();
    /// <summary>
    /// Returns (frees) the specified gamepad.
    /// </summary>
    public void ReturnGamepad(GamepadDevice gamepad) => GetGamepad(gamepad.Index)?.Free();

    /// <summary>
    /// Checks and updates the connection state of all managed gamepads.
    /// Triggers connection/disconnection events and tracks last used gamepads.
    /// </summary>
    private bool CheckGamepadConnections(float dt, bool wasOtherDeviceUsed)
    {
        LastUsedGamepads.Clear();
        var used = false;
        for (var i = 0; i < gamepads.Length; i++)
        {
            var gamepad = gamepads[i];
            if (Raylib.IsGamepadAvailable(i))
            {
                if (!gamepad.Connected)
                {
                    gamepad.Connect();
                    OnGamepadConnectionChanged?.Invoke(gamepad, true);
                }
               
                used = gamepad.Update(dt, wasOtherDeviceUsed);
                if(gamepad.WasUsed()) 
                    LastUsedGamepads.Add(gamepad);
            }
            
            if (gamepad.Connected)
            {
                if (gamepad == LastUsedGamepad) LastUsedGamepad = null;
                    
                gamepad.Disconnect();
                OnGamepadConnectionChanged?.Invoke(gamepad, false);
            }
        }


        if (LastUsedGamepads.Count > 0)
        {
            LastUsedGamepad = LastUsedGamepads[^1];
        }

        return used;

    }
    /// <summary>
    /// Initializes all managed gamepads and sets up button event handlers.
    /// </summary>
    private void GamepadSetup()
    {
        for (var i = 0; i < gamepads.Length; i++)
        {
            var gamepad =  new GamepadDevice(i, Raylib.IsGamepadAvailable(i));
            gamepads[i] = gamepad;
            gamepad.OnButtonPressed += GamepadButtonWasPressed;
            gamepad.OnButtonReleased += GamepadButtonWasReleased;
            gamepad.OnInputDeviceChangeSettingsChanged += OnGamepadUsageDetectionSettingsChanged;
        }
    }

    private void OnGamepadUsageDetectionSettingsChanged(GamepadDevice gamepadDevice, InputDeviceUsageDetectionSettings settings)
    {
        UsageDetectionSettings = settings.Gamepad;
        foreach (var gamepad in gamepads)
        {
            if(gamepad == gamepadDevice) continue;
            gamepad.OverrideInputDeviceChangeSettings(settings);
        }
    }

    /// <summary>
    /// Handler for when a gamepad button is released.
    /// </summary>
    private void GamepadButtonWasReleased(GamepadDevice gamepad, ShapeGamepadButton button) => OnGamepadButtonPressed?.Invoke(gamepad, button);
    /// <summary>
    /// Handler for when a gamepad button is pressed.
    /// </summary>
    private void GamepadButtonWasPressed(GamepadDevice gamepad, ShapeGamepadButton button) => OnGamepadButtonReleased?.Invoke(gamepad, button);

    #endregion
    
}