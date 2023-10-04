namespace ShapeEngine.Timing;

public interface ISequenceable
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dt"></param>
    /// <returns>Returns if finished.</returns>
    public bool Update(float dt);

    public ISequenceable Copy();
}