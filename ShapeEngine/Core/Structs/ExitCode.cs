using ShapeEngine.Core.GameDef;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents an exit code returned by the <see cref="GameDef.Game.Run"/> function in the Game class.
/// </summary>
public readonly struct ExitCode
{
    /// <summary>
    /// Gets a value indicating whether the game should restart.
    /// </summary>
    /// <value><c>true</c> if the game should restart; otherwise, <c>false</c>.</value>
    public readonly bool Restart = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExitCode"/> struct.
    /// </summary>
    /// <param name="restart">A value indicating whether the game should restart. 
    /// If <c>true</c>, the game will restart; if <c>false</c>, the game will exit.</param>
    public ExitCode(bool restart) { this.Restart = restart; }
}