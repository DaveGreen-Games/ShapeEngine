namespace ShapeEngine.Core.Structs;

/// <summary>
/// Returned by the Run() function in the GameLoop class.
/// </summary>
public readonly struct ExitCode
{
    public readonly bool Restart = false;
    public ExitCode(bool restart) { this.Restart = restart; }

}