namespace ShapeEngine.Core.Interfaces;

public interface IHandlerDeltaFactor
{
    public int ApplyOrder { get; set; }

    public uint GetID();
       
    public bool IsAffectingLayer(int layer);

    /// <summary>
    /// Update the delta factor.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns>Returns true when finished.</returns>
    public bool Update(float dt);

    /// <summary>
    /// Recieves the current total delta factor.
    /// </summary>
    /// <param name="totalFactor">The current total delta factor.</param>
    /// <returns>Returns the new total delta factor.</returns>
    public float Apply(float totalFactor);
}