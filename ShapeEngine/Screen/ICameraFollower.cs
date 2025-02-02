using ShapeEngine.Core.Shapes;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Screen;


public interface ICameraFollower
{
    public void Reset();
    public Rect Update(float dt, Rect cameraRect);
    
    
    public void OnCameraAttached();
    public void OnCameraDetached();


}