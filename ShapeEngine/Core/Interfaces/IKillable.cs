namespace ShapeEngine.Core.Interfaces;

public interface IKillable
{
    /// <summary>
    /// Try to kill the object.
    /// </summary>
    /// <returns>Return true if kill was successful.</returns>
    public bool Kill();
    /// <summary>
    /// Check if object is dead.
    /// </summary>
    /// <returns></returns>
    public bool IsDead();
}