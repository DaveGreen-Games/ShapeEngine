using Raylib_CsLo;

namespace ShapeEngine.Input;

public class GamepadManager
{
    public event Action<Gamepad, bool>? OnGamepadConnectionChanged;
    public int MaxGamepads => gamepads.Length;
    
    private readonly Gamepad[] gamepads = new Gamepad[8];
    //private readonly List<int> connectedGamepadIndices = new();
    
    public readonly List<Gamepad> LastUsedGamepads = new();

    public Gamepad? LastUsedGamepad =>
        LastUsedGamepads.Count > 0 ? LastUsedGamepads[^1] : null;
    

    public GamepadManager()
    {
        GamepadSetup();
    }

    public void Update()
    {
        CheckGamepadConnections();
    }

    public List<Gamepad> GetConnectedGamepads()
    {
        var result = new List<Gamepad>();

        foreach (var gamepad in gamepads)
        {
            if(gamepad.Connected) result.Add(gamepad);
        }

        return result;
    }
    public List<Gamepad> GetAvailableGamepads()
    {
        var result = new List<Gamepad>();

        foreach (var gamepad in gamepads)
        {
            if(gamepad.Connected && gamepad.Available) result.Add(gamepad);
        }

        return result;
    }
    
    public bool HasGamepad(int index) => index >= 0 && index < gamepads.Length;
    
    public bool IsGamepadConnected(int index) => HasGamepad(index) && gamepads[index].Connected;
    
    public Gamepad? GetGamepad(int index)
    {
        if (!HasGamepad(index)) return null;
        return gamepads[index];
    }
    
    public Gamepad? RequestGamepad(int preferredIndex = -1)
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
    public void ReturnGamepad(Gamepad gamepad) => GetGamepad(gamepad.Index)?.Free();

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
                    if(gamepad.WasUsed) LastUsedGamepads.Add(gamepad);
                }
                // connectedGamepadIndices.Add(i);
            }
            else
            {
                if (gamepad.Connected)
                {
                    gamepad.Disconnect();
                    OnGamepadConnectionChanged?.Invoke(gamepad, false);
                    // ConnectedGamepads.Remove(gamepad);
                }
            }
        }
        
    }
    private void GamepadSetup()
    {
        for (var i = 0; i < gamepads.Length; i++)
        {
            var gamepad =  new Gamepad(i, Raylib.IsGamepadAvailable(i));
            gamepads[i] = gamepad;
            // if(gamepad.Connected) ConnectedGamepads.Add(gamepad);
        }
    }

    public static int WasGamepadUsed(List<int> connectedGamepads, float deadzone = 0.05f)
    {
        foreach (var gamepad in connectedGamepads)
        {
            if (Gamepad.WasGamepadUsed(gamepad, deadzone)) return gamepad;
        }
        return -1;
    }
    public static int WasGamepadButtonUsed(List<int> connectedGamepads)
    {
        foreach (var gamepad in connectedGamepads)
        {
            if (Gamepad.WasGamepadButtonUsed(gamepad)) return gamepad;
        }

        return -1;
    }
    public static int WasGamepadAxisUsed(List<int> connectedGamepads, float deadzone = 0.05f)
    {
        foreach (var gamepad in connectedGamepads)
        {
            if (Gamepad.WasGamepadAxisUsed(gamepad, deadzone)) return gamepad;
        }
        return -1;
    }
    public static List<int> GetUsedGamepads(List<int> connectedGamepads, float deadzone = 0.05f)
    {
        var usedGamepads = new List<int>();
        foreach (var gamepad in connectedGamepads)
        {
            if(Gamepad.WasGamepadUsed(gamepad, deadzone)) usedGamepads.Add(gamepad);
        }

        return usedGamepads;
    }

}