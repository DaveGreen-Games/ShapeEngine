namespace ShapeEngine.ShapeClipper;

public readonly struct ShapeClipperScale
{
    public readonly int DecimalPlaces;
    public readonly double Scale;
    public readonly double InvScale;
    
    public ShapeClipperScale(int decimalPlaces = 4)
    {
        DecimalPlaces = Math.Clamp(decimalPlaces, 0, 8);;
        Scale = Pow10(DecimalPlaces);
        InvScale = 1.0 / Scale;
    }
    
    private double Pow10(int dp)
    {
        if (dp <= 0) return 1.0;
        double s = 1.0;
        for (int i = 0; i < dp; i++) s *= 10.0;
        return s;
    }
}