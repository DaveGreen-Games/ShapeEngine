using System.Text;
using Raylib_CsLo;

namespace ShapeEngine.Input;



public static class ShapeInput
{
    public static readonly uint AllAccessTag = 0;
    public static bool Locked { get; private set; } = false;
    private static List<uint> lockExceptionTags = new();

    #region Lock System
    public static void Lock()
    {
        Locked = true;
        lockExceptionTags.Clear();
    }
    public static void Lock(params uint[] exceptionTags)
    {
        Locked = true;
        lockExceptionTags.Clear();
        if(exceptionTags.Length > 0) lockExceptionTags.AddRange(exceptionTags);
    }
    public static void Unlock()
    {
        Locked = false;
        lockExceptionTags.Clear();
    }
    public static bool HasAccess(uint tag) => lockExceptionTags.Contains(tag);
    #endregion
    
    #region Input Map System with Actions

    //works for all gamepad / keyboard / mouse buttons -> action is device independent
    // IsPressed(uint mapID, uint actionID) -> map has access tag for locked system
    // IsReleased
    // IsDown
    // IsUp
    
    //works for all axis
    // GetAxis

    #endregion


    #region Basic
    //GetAxis(GamepadAxis, accessTag)
    //Mouse Down/Up/Released/Pressed

    public static bool IsKeyPressed(KeyboardKey key)
    {
        if (Locked) return false;

        return Raylib.IsKeyPressed(key);
    }
    public static bool IsKeyReleased(KeyboardKey key)
    {
        if (Locked) return false;

        return Raylib.IsKeyReleased(key);
    }
    public static bool IsKeyDown(KeyboardKey key)
    {
        if (Locked) return false;

        return Raylib.IsKeyDown(key);
    }
    public static bool IsKeyUp(KeyboardKey key)
    {
        if (Locked) return false;

        return Raylib.IsKeyUp(key);
    }
    public static bool IsKeyPressed(KeyboardKey key, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return false;

        return Raylib.IsKeyPressed(key);
    }
    public static bool IsKeyReleased(KeyboardKey key, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return false;

        return Raylib.IsKeyReleased(key);
    }
    public static bool IsKeyDown(KeyboardKey key, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return false;

        return Raylib.IsKeyDown(key);
    }
    public static bool IsKeyUp(KeyboardKey key, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return false;

        return Raylib.IsKeyUp(key);
    }
    
    public static List<char> GetKeyboardStreamChar()
    {
        if (Locked) return new();
        int unicode = Raylib.GetCharPressed();
        List<char> chars = new();
        while (unicode != 0)
        {
            var c = (char)unicode;
            chars.Add(c);

            unicode = Raylib.GetCharPressed();
        }
        return chars;
    }
    public static List<char> GetKeyboardStreamChar(uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return new();
        int unicode = Raylib.GetCharPressed();
        List<char> chars = new();
        while (unicode != 0)
        {
            var c = (char)unicode;
            chars.Add(c);

            unicode = Raylib.GetCharPressed();
        }
        return chars;
    }
    public static string GetKeyboardStream(uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return "";
        int unicode = Raylib.GetCharPressed();
        List<char> chars = new();
        while (unicode != 0)
        {
            var c = (char)unicode;
            chars.Add(c);

            unicode = Raylib.GetCharPressed();
        }

        StringBuilder b = new(chars.Count);
        b.Append(chars);
        return b.ToString();
    }
    public static string GetKeyboardStream(string curText, uint accessTag)
    {
        if (Locked && !HasAccess(accessTag)) return "";
        var chars = GetKeyboardStreamChar(accessTag);
        var b = new StringBuilder(chars.Count + curText.Length);
        b.Append(curText);
        b.Append(chars);
        return b.ToString();
    }
    #endregion
}