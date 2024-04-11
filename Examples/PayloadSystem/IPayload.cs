using System.Numerics;

namespace Examples.PayloadSystem;

public interface IPayload
{
    public void Launch(Vector2 start, Vector2 target);
    public void Update(float dt);
    public void Draw();
    public bool IsFinished();

}