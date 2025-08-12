using Raylib_cs;

namespace ShapeEngine.Input;

/// <summary>
/// Manages multiple <see cref="GamepadDevice"/> instances, handling connection state,
/// device claiming, releasing, and tracking last used gamepads.
/// </summary>
public sealed class GamepadDeviceManager
{
    /// <summary>
    /// Gets the maximum number of gamepads supported by this manager.
    /// </summary>
    public int MaxGamepads => gamepads.Length;

    private InputSettings.GamepadSettings gamepadSettings;
    private GamepadDevice[] gamepads;
    /// <summary>
    /// List of <see cref="GamepadDevice"/> instances that registered input during the last update cycle,
    /// considering any filters or settings applied via <see cref="InputSettings"/>.
    /// This list is cleared and repopulated on each update.
    /// </summary>
    public readonly List<GamepadDevice> LastUsedGamepads = [];
    /// <summary>
    /// The most recently used gamepad that registered input during the last update cycle,
    /// considering any filters or settings applied via <see cref="InputSettings"/>.
    /// This is <c>null</c> if no gamepad was used.
    /// </summary>
    public GamepadDevice? LastUsedGamepad { get; private set; } = null;
    
    private readonly List<GamepadDevice> claimedGamepads = [];
   /// <summary>
   /// The most recently claimed gamepad, or <c>null</c> if none is claimed.
   /// </summary>
   public GamepadDevice? LastClaimedGamepad { get; private set; } = null;
   
   /// <summary>
   /// Gets the number of currently claimed gamepads.
   /// </summary>
   public int ClaimedGamepadsCount => claimedGamepads.Count;
   
   /// <summary>
   /// The button that, when pressed, will automatically claim a gamepad if available.
   /// </summary>
   public ShapeGamepadButton AutomaticGamepadClaimButton = ShapeGamepadButton.RIGHT_FACE_DOWN;
    
    /// <summary>
    /// Event triggered when a gamepad button is pressed.
    /// </summary>
    public event Action<GamepadDevice, ShapeGamepadButton>? OnGamepadButtonPressed;
    /// <summary>
    /// Event triggered when a gamepad button is released.
    /// </summary>
    public event Action<GamepadDevice, ShapeGamepadButton>? OnGamepadButtonReleased;
    /// <summary>
    /// Event triggered when a gamepad's connection state changes.
    /// The <c>bool</c> parameter indicates whether the gamepad is connected (<c>true</c>) or disconnected (<c>false</c>).
    /// </summary>
    public event Action<GamepadDevice, bool>? OnGamepadConnectionChanged;
    
    /// <summary>
    /// Event triggered when a gamepad is claimed (reserved for use).
    /// </summary>
    public event Action<GamepadDevice>? OnGamepadClaimed;
    
    /// <summary>
    /// Event triggered when a gamepad is freed (released from being claimed).
    /// </summary>
    public event Action<GamepadDevice>? OnGamepadFreed;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GamepadDeviceManager"/> class.
    /// </summary>
    /// <param name="maxGamepadCount">Maximum number of gamepads to manage.</param>
    /// <param name="settings">The input settings to use for gamepad configuration.</param>
    internal GamepadDeviceManager(int maxGamepadCount, InputSettings.GamepadSettings settings)
    {
        gamepadSettings = settings;
        gamepads = new GamepadDevice[maxGamepadCount];
        if(maxGamepadCount > 0) GamepadSetup();
    }

    /// <summary>
    /// Changes the number of managed gamepads to the specified value.
    /// Disconnects and removes gamepads if the new count is lower, or adds new gamepads if higher.
    /// Returns 0 if unchanged, -1 if reduced, 1 if increased.
    /// </summary>
    /// <param name="newGamepadCount">The new number of gamepads to manage.</param>
    /// <returns>
    /// 0 if the count is unchanged, -1 if the count was reduced, 1 if increased.
    /// </returns>
    public int ChangedGamepadCount(int newGamepadCount)
    {
        if(newGamepadCount == gamepads.Length) return 0;

        if (newGamepadCount <= 0)
        {
            foreach(var gamepad in gamepads) gamepad.Disconnect();
            gamepads = [];
            return -1;
        }
        
        if (newGamepadCount < gamepads.Length)
        {
            for (int i = gamepads.Length - 1; i >= newGamepadCount; i--)
            {
                gamepads[i].Disconnect();
            }
            var newGamepads = new GamepadDevice[newGamepadCount];
            Array.Copy(gamepads, newGamepads, newGamepadCount);
            gamepads = newGamepads;
            return -1;
        }
        else //bigger
        {
            var newGamepads = new GamepadDevice[newGamepadCount];
            Array.Copy(gamepads, newGamepads, gamepads.Length);
            for (int i = gamepads.Length; i < newGamepadCount; i++)
            {
                newGamepads[i] = CreateGamepad(i);
            }
            gamepads = newGamepads;
            return 1;
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
    /// Gets a list of all claimed gamepads (connected but not available).
    /// </summary>
    public List<GamepadDevice> GetClaimedGamepads()
    {
        var copy = new List<GamepadDevice>(claimedGamepads);
        return copy;
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
    /// <remarks>
    /// If <see cref="preferredIndex"/> is not available, the method will search through all gamepads.
    /// </remarks>
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
    /// Claims the gamepad at the specified index if it is connected and available.
    /// </summary>
    /// <param name="index">The index of the gamepad to claim.</param>
    /// <returns>The claimed <see cref="GamepadDevice"/> if successful; otherwise, <c>null</c>.</returns>
    public GamepadDevice? ClaimGamepad(int index)
    {
        var gamepad = GetGamepad(index);
        if (gamepad is { Connected: true, Available: true })
        {
            gamepad.Claim();
            return gamepad;
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
    /// Initializes all managed gamepads and sets up button event handlers.
    /// </summary>
    private void GamepadSetup()
    {
        for (var i = 0; i < gamepads.Length; i++)
        {
            gamepads[i] = CreateGamepad(i);
        }
    }
    
    private GamepadDevice CreateGamepad(int index)
    {
        var gamepad = new GamepadDevice(index, gamepadSettings);
        gamepad.OnButtonPressed += GamepadButtonWasPressed;
        gamepad.OnButtonReleased += GamepadButtonWasReleased;
        gamepad.OnConnectionChanged += GamepadConnectionHasChanged;
        gamepad.OnClaimed += GamepadWasClaimed;
        gamepad.OnFreed += GamepadWasFreed;
        return gamepad;
    }

    private void GamepadWasFreed(GamepadDevice gamepad)
    {
        OnGamepadFreed?.Invoke(gamepad);
        
        claimedGamepads.Remove(gamepad);
        if (LastClaimedGamepad == gamepad)
        {
            LastClaimedGamepad = claimedGamepads.Count > 0 ? claimedGamepads[^1] : null;
        }
    }

    private void GamepadWasClaimed(GamepadDevice gamepad)
    {
        OnGamepadClaimed?.Invoke(gamepad);
    }

    /// <summary>
    /// Applies new input device usage detection settings to all managed gamepads.
    /// Propagates the settings to each gamepad.
    /// </summary>
    /// <param name="settings">The new input device usage detection settings to apply.</param>
    internal void ApplyInputDeviceChangeSettings(InputSettings settings)
    {
        gamepadSettings = settings.Gamepad;
        foreach (var gamepad in gamepads)
        {
            gamepad.ApplyInputDeviceChangeSettings(settings);
        }
    }
    /// <summary>
    /// Handler for when a gamepad button is released.
    /// </summary>
    private void GamepadButtonWasReleased(GamepadDevice gamepad, ShapeGamepadButton button) => OnGamepadButtonReleased?.Invoke(gamepad, button);
    /// <summary>
    /// Handler for when a gamepad button is pressed.
    /// </summary>
    private void GamepadButtonWasPressed(GamepadDevice gamepad, ShapeGamepadButton button)
    {
        OnGamepadButtonPressed?.Invoke(gamepad, button);
        if (AutomaticGamepadClaimButton != ShapeGamepadButton.UNKNOWN && button == AutomaticGamepadClaimButton)
        {
            if (gamepad.Claim())
            {
                claimedGamepads.Add(gamepad);
                LastClaimedGamepad = gamepad;
            }
        }
    }

    /// <summary>
    /// Updates the state of all managed gamepads and checks for connection changes.
    /// </summary>
    internal void ClearUsedGamepads()
    {
        LastUsedGamepads.Clear();
    }

    private void GamepadConnectionHasChanged(GamepadDevice gamepad, bool connected)
    {
        OnGamepadConnectionChanged?.Invoke(gamepad, connected);
        if (!connected)
        {
            LastUsedGamepads.Remove(gamepad);
            if (LastUsedGamepad == gamepad) LastUsedGamepad = null;
            
            LastUsedGamepad ??= LastUsedGamepads.Count > 0 ? LastUsedGamepads[^1] : null;
            
            
            claimedGamepads.Remove(gamepad);
            if(LastClaimedGamepad == gamepad) LastClaimedGamepad = null;
            
            LastClaimedGamepad ??= claimedGamepads.Count > 0 ? claimedGamepads[^1] : null;
        }
    }

    internal void GamepadWasUsed(GamepadDevice gamepad)
    {
        LastUsedGamepads.Add(gamepad);
        LastUsedGamepad = gamepad;
    }
    #endregion
    
}