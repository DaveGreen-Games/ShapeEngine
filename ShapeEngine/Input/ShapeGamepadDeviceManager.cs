using Raylib_CsLo;

namespace ShapeEngine.Input;


//TODO implement session system
//each session has a list of gamepads & a list of last used gamepads
//default session is created automatically
//new sessions can be created or removed
//session is a class that does most of what device manager does right now

public class ShapeGamepadDeviceManager
{
    
    public event Action<ShapeGamepadDevice, bool>? OnGamepadConnectionChanged;
    public int MaxGamepads => gamepads.Length;
    
    private readonly ShapeGamepadDevice[] gamepads = new ShapeGamepadDevice[8];
    //private readonly List<int> connectedGamepadIndices = new();
    
    public readonly List<ShapeGamepadDevice> LastUsedGamepads = new();

    public ShapeGamepadDevice? LastUsedGamepad = null;
        // LastUsedGamepads.Count > 0 ? LastUsedGamepads[^1] : null;

    internal ShapeGamepadDeviceManager()
    {
        GamepadSetup();
    }
    internal void Update()
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
    
    // public static int WasGamepadUsed(List<int> connectedGamepads, float deadzone = 0.05f)
    // {
    //     foreach (var gamepad in connectedGamepads)
    //     {
    //         if (Gamepad.WasGamepadUsed(gamepad, deadzone)) return gamepad;
    //     }
    //     return -1;
    // }
    // public static int WasGamepadButtonUsed(List<int> connectedGamepads)
    // {
    //     foreach (var gamepad in connectedGamepads)
    //     {
    //         if (Gamepad.WasGamepadButtonUsed(gamepad)) return gamepad;
    //     }
    //
    //     return -1;
    // }
    // public static int WasGamepadAxisUsed(List<int> connectedGamepads, float deadzone = 0.05f)
    // {
    //     foreach (var gamepad in connectedGamepads)
    //     {
    //         if (Gamepad.WasGamepadAxisUsed(gamepad, deadzone)) return gamepad;
    //     }
    //     return -1;
    // }
    // public static List<int> GetUsedGamepads(List<int> connectedGamepads, float deadzone = 0.05f)
    // {
    //     var usedGamepads = new List<int>();
    //     foreach (var gamepad in connectedGamepads)
    //     {
    //         if(Gamepad.WasGamepadUsed(gamepad, deadzone)) usedGamepads.Add(gamepad);
    //     }
    //
    //     return usedGamepads;
    // }

}