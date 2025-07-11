namespace ShapeEngine.Input;

/// <summary>
/// Represents mouse buttons and mouse wheel/axis actions for input handling.
/// </summary>
public enum ShapeMouseButton
{
    /// <summary>Left mouse button.</summary>
    LEFT = 0,
    /// <summary>Right mouse button.</summary>
    RIGHT = 1,
    /// <summary>Middle mouse button (usually the scroll wheel button).</summary>
    MIDDLE = 2,
    /// <summary>Side mouse button (often used for extra functions).</summary>
    SIDE = 3,
    /// <summary>Extra mouse button.</summary>
    EXTRA = 4,
    /// <summary>Forward mouse button.</summary>
    FORWARD = 5,
    /// <summary>Back mouse button.</summary>
    BACK = 6,
    /// <summary>Mouse wheel scrolled up.</summary>
    MW_UP = 10,
    /// <summary>Mouse wheel scrolled down.</summary>
    MW_DOWN = 11,
    /// <summary>Mouse wheel scrolled left.</summary>
    MW_LEFT = 12,
    /// <summary>Mouse wheel scrolled right.</summary>
    MW_RIGHT = 13,
    /// <summary>Mouse axis up - Mouse moved up (for analog or custom input).</summary>
    UP_AXIS = 20,
    /// <summary>Mouse axis down - Mouse moved down (for analog or custom input).</summary>
    DOWN_AXIS = 21,
    /// <summary>Mouse axis left - Mouse moved left (for analog or custom input).</summary>
    LEFT_AXIS = 22,
    /// <summary>Mouse axis right - Mouse moved right (for analog or custom input).</summary>
    RIGHT_AXIS = 23
}