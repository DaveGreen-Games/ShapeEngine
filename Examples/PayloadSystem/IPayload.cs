using System.Numerics;

namespace Examples.PayloadSystem;

public interface IPayload
{
    public void Launch(Vector2 launchPosition, Vector2 targetPosition, Vector2 markerPosition, Vector2 markerDirection);
    public void Update(float dt);
    public void Draw();
    public bool IsFinished();

}