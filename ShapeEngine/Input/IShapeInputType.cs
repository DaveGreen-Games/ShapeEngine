namespace ShapeEngine.Input;

public interface IShapeInputType
{
    public IShapeInputType Copy();
    public void Update(float dt, int gamepadIndex);
    public ShapeInputState GetState();
    public string GetName(bool shorthand = true);
}