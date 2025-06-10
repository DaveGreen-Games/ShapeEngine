using Raylib_cs;

namespace ShapeEngine.Input;

/// <summary>
/// Manages multiple <see cref="ShapeGamepadDevice"/> instances, handling connection state,
/// device claiming, releasing, and tracking last used gamepads.
/// </summary>
public sealed class ShapeGamepadDeviceManager
{
    /// <summary>
    /// Event triggered when a gamepad's connection state changes.
    /// </summary>
    public event Action<ShapeGamepadDevice, bool>? OnGamepadConnectionChanged;
    /// <summary>
    /// Gets the maximum number of gamepads supported by this manager.
    /// </summary>
    public int MaxGamepads => gamepads.Length;
    
    private readonly ShapeGamepadDevice[] gamepads;
    /// <summary>
    /// List of gamepads that were used in the last update.
    /// </summary>
    public readonly List<ShapeGamepadDevice> LastUsedGamepads = new();

    /// <summary>
    /// The most recently used gamepad, or null if none.
    /// </summary>
    public ShapeGamepadDevice? LastUsedGamepad;
    
    /// <summary>
    /// Event triggered when a gamepad button is pressed.
    /// </summary>
    public event Action<ShapeGamepadDevice, ShapeGamepadButton>? OnGamepadButtonPressed;
    /// <summary>
    /// Event triggered when a gamepad button is released.
    /// </summary>
    public event Action<ShapeGamepadDevice, ShapeGamepadButton>? OnGamepadButtonReleased;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeGamepadDeviceManager"/> class.
    /// </summary>
    /// <param name="maxGamepads">Maximum number of gamepads to manage.</param>
    public ShapeGamepadDeviceManager(int maxGamepads = 8)
    {
        if (maxGamepads <= 0) maxGamepads = 1;
        gamepads = new ShapeGamepadDevice[maxGamepads];
        GamepadSetup();
    }

    /// <summary>
    /// Updates the state of all managed gamepads and checks for connection changes.
    /// </summary>
    public void Update()
    {
        CheckGamepadConnections();
    }
    
    #region Gamepad

    /// <summary>
    /// Gets a list of all currently connected gamepads.
    /// </summary>
    public List<ShapeGamepadDevice> GetConnectedGamepads()
    {
        var result = new List<ShapeGamepadDevice>();

        foreach (var gamepad in gamepads)
        {
            if(gamepad.Connected) result.Add(gamepad);
        }

        return result;
    }

    /// <summary>
    /// Gets a list of all gamepads managed by this manager.
    /// </summary>
    public List<ShapeGamepadDevice> GetAllGamepads()
    {
        return gamepads.ToList();
    }

    /// <summary>
    /// Gets a list of all available (connected and not claimed) gamepads.
    /// </summary>
    public List<ShapeGamepadDevice> GetAvailableGamepads()
    {
        var result = new List<ShapeGamepadDevice>();

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
    public ShapeGamepadDevice? GetGamepad(int index)
    {
        if (!HasGamepad(index)) return null;
        return gamepads[index];
    }

    /// <summary>
    /// Requests an available gamepad, optionally preferring a specific index.
    /// </summary>
    /// <param name="preferredIndex">Preferred gamepad index, or -1 for any available.</param>
    /// <returns>The claimed gamepad, or null if none available.</returns>
    public ShapeGamepadDevice? RequestGamepad(int preferredIndex = -1)
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
    public void ReturnGamepad(ShapeGamepadDevice gamepad) => GetGamepad(gamepad.Index)?.Free();

    /// <summary>
    /// Checks and updates the connection state of all managed gamepads.
    /// Triggers connection/disconnection events and tracks last used gamepads.
    /// </summary>
    private void CheckGamepadConnections()
    {
        //connectedGamepadIndices.Clear();
        LastUsedGamepads.Clear();
        for (var i = 0; i < gamepads.Length; i++)
        {
            var gamepad = gamepads[i];
            if (Raylib.IsGamepadAvailable(i))
            {
                if (!gamepad.Connected)
                {
                    gamepad.Connect();
                    // ConnectedGamepads.Add(gamepad);
                    OnGamepadConnectionChanged?.Invoke(gamepad, true);
                }
                else
                {
                    gamepad.Update();
                    if(gamepad.WasUsed()) 
                        LastUsedGamepads.Add(gamepad);
                }
                // connectedGamepadIndices.Add(i);
            }
            else
            {
                if (gamepad.Connected)
                {
                    if (gamepad == LastUsedGamepad) LastUsedGamepad = null;
                    
                    gamepad.Disconnect();
                    OnGamepadConnectionChanged?.Invoke(gamepad, false);
                    // ConnectedGamepads.Remove(gamepad);
                }
            }
        }


        if (LastUsedGamepads.Count > 0)
        {
            LastUsedGamepad = LastUsedGamepads[^1];
        }
        

    }
    /// <summary>
    /// Initializes all managed gamepads and sets up button event handlers.
    /// </summary>
    private void GamepadSetup()
    {
        for (var i = 0; i < gamepads.Length; i++)
        {
            var gamepad =  new ShapeGamepadDevice(i, Raylib.IsGamepadAvailable(i));
            gamepads[i] = gamepad;
            gamepad.OnButtonPressed += GamepadButtonWasPressed;
            gamepad.OnButtonReleased += GamepadButtonWasReleased;
        }
    }

    /// <summary>
    /// Handler for when a gamepad button is released.
    /// </summary>
    private void GamepadButtonWasReleased(ShapeGamepadDevice gamepad, ShapeGamepadButton button) => OnGamepadButtonPressed?.Invoke(gamepad, button);
    /// <summary>
    /// Handler for when a gamepad button is pressed.
    /// </summary>
    private void GamepadButtonWasPressed(ShapeGamepadDevice gamepad, ShapeGamepadButton button) => OnGamepadButtonReleased?.Invoke(gamepad, button);

    #endregion
    
}