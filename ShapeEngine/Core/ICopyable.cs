namespace ShapeEngine.Core;

/// <summary>
/// Defines a method for creating a copy of the current instance.
/// </summary>
public interface ICopyable<out T>
{
    /// <summary>
    /// Creates a copy of the current instance.
    /// </summary>
    /// <returns>A new instance of type <typeparamref name="T"/> that is a copy of this instance.</returns>
    T Copy();
}