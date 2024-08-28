using System.Numerics;
using System.Runtime.CompilerServices;

namespace ShapeEngine.Core.Structs;

public readonly struct AnchorPoint(float x, float y)
{
    public static AnchorPoint Zero = new(0, 0);
    public static AnchorPoint One = new(1, 1);
    public static AnchorPoint Invalid = new(0, 0); // ?????
    
    public static AnchorPoint TopLeft = new(0, 0);
    public static AnchorPoint TopCenter = new(0.5f, 0);
    public static AnchorPoint TopRight = new(1, 0);
    
    public static AnchorPoint Left = new(0, 0.5f);
    public static AnchorPoint Center = new(0.5f, 0.5f);
    public static AnchorPoint Right = new(1, 0.5f);
    
    public static AnchorPoint BottomLeft = new(0, 1f);
    public static AnchorPoint BottomCenter = new(0.5f, 1f);
    public static AnchorPoint BottomRight = new(1f, 1f);

    public readonly float X = x;
    public readonly float Y = y;
    
    
    public AnchorPoint() : this(0f, 0f) { }
    public AnchorPoint(float v): this(v, v) { }


    public Vector2 ToVector2() => new Vector2(X, Y);
    
    /// <summary>
    /// Returns a vector2 between 0,0 and 1,1.
    /// 7 , 8 , 9
    /// 4 , 5 , 6
    /// 1 , 2 , 3
    /// 7 is top left and returns 0, 0
    /// 3 is bottom right and returns 1, 1
    /// </summary>
    /// <param name="keypadNumber"></param>
    /// <returns></returns>
    public static AnchorPoint GetKeypadAnchorPosition(int keypadNumber)
    {
        if(keypadNumber < 1 || keypadNumber > 9) return TopLeft;
        
        if(keypadNumber == 1) return new (0f, 1f);
        if(keypadNumber == 2) return new (0.5f, 1f);
        if(keypadNumber == 3) return new (1f, 1f);
        if(keypadNumber == 4) return new (0f, 0.5f);
        if(keypadNumber == 5) return new (0.5f, 0.5f);
        if(keypadNumber == 6) return new (1f, 0.5f);
        if(keypadNumber == 7) return new (0f, 0f);
        if(keypadNumber == 8) return new (0.5f, 0f);
        return new AnchorPoint(1f, 0f);
    }
   
    /// <summary>
    /// Returns a vector2 between 0,0 and 1,1.
    /// 1 , 2 , 3
    /// 4 , 5 , 6
    /// 7 , 8 , 9
    /// 1 is top left and returns 0, 0
    /// 9 is bottom right and returns 1, 1
    /// </summary>
    /// <param name="keypadNumber"></param>
    /// <returns></returns>
    public static AnchorPoint GetKeypadAnchorPositionReversed(int keypadNumber)
    {
        if(keypadNumber < 1 || keypadNumber > 9) return TopLeft;
        
        if(keypadNumber == 1) return new (0f, 0f);
        if(keypadNumber == 2) return new (0.5f, 0f);
        if(keypadNumber == 3) return new (1f, 0f);
        if(keypadNumber == 4) return new (0f, 0.5f);
        if(keypadNumber == 5) return new (0.5f, 0.5f);
        if(keypadNumber == 6) return new (1f, 0.5f);
        if(keypadNumber == 7) return new (0f, 1f);
        if(keypadNumber == 8) return new (0.5f, 1f);
        return new AnchorPoint(1f, 1f);
    }
}