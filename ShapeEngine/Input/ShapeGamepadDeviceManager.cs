using Raylib_CsLo;

namespace ShapeEngine.Input;


public sealed class ShapeGamepadDeviceManager
{
    public event Action<ShapeGamepadDevice, bool>? OnGamepadConnectionChanged;
    public int MaxGamepads => gamepads.Length;
    
    private readonly ShapeGamepadDevice[] gamepads;// = new ShapeGamepadDevice[8];
    public readonly List<ShapeGamepadDevice> LastUsedGamepads = new();

    public ShapeGamepadDevice? LastUsedGamepad = null;

    public ShapeGamepadDeviceManager(int maxGamepads = 8)
    {
        if (maxGamepads <= 0) maxGamepads = 1;
        gamepads = new ShapeGamepadDevice[maxGamepads];
        GamepadSetup();
    }
    public void Update()
    {
        CheckGamepadConnections();
    }
    
    #region Gamepad
    public List<ShapeGamepadDevice> GetConnectedGamepads()
    {
        var result = new List<ShapeGamepadDevice>();

        foreach (var gamepad in gamepads)
        {
            if(gamepad.Connected) result.Add(gamepad);
        }

        return result;
    }
    public List<ShapeGamepadDevice> GetAvailableGamepads()
    {
        var result = new List<ShapeGamepadDevice>();

        foreach (var gamepad in gamepads)
        {
            if(gamepad.Connected && gamepad.Available) result.Add(gamepad);
        }

        return result;
    }
    
    public bool HasGamepad(int index) => index >= 0 && index < gamepads.Length;
    
    public bool IsGamepadConnected(int index) => HasGamepad(index) && gamepads[index].Connected;
    
    public ShapeGamepadDevice? GetGamepad(int index)
    {
        if (!HasGamepad(index)) return null;
        return gamepads[index];
    }
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
    
    public void ReturnGamepad(int index) => GetGamepad(index)?.Free();
    public void ReturnGamepad(ShapeGamepadDevice gamepad) => GetGamepad(gamepad.Index)?.Free();

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
    private void GamepadSetup()
    {
        for (var i = 0; i < gamepads.Length; i++)
        {
            var gamepad =  new ShapeGamepadDevice(i, Raylib.IsGamepadAvailable(i));
            gamepads[i] = gamepad;
            // if(gamepad.Connected) ConnectedGamepads.Add(gamepad);
        }
    }
    #endregion
    
}