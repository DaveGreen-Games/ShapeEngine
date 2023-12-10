namespace ShapeEngine.Input;

public interface ShapeInputDevice
{
    public bool WasUsed();
    public void Update();

    public bool IsLocked();
    public void Lock();
    public void Unlock();

    public void Calibrate();

}