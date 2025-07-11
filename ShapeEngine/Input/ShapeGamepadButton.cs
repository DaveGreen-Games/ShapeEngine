namespace ShapeEngine.Input;

/// <summary>
/// Represents the various buttons available on a gamepad.
/// </summary>
public enum ShapeGamepadButton
{
    /// <summary>Unknown button.</summary>
    UNKNOWN = 0,

    /// <summary>Top button on the left face (e.g., Y on Xbox, Triangle on PlayStation).</summary>
    LEFT_FACE_UP = 1,
    /// <summary>Right button on the left face (e.g., B on Xbox, Circle on PlayStation).</summary>
    LEFT_FACE_RIGHT = 2,
    /// <summary>Bottom button on the left face (e.g., A on Xbox, Cross on PlayStation).</summary>
    LEFT_FACE_DOWN = 3,
    /// <summary>Left button on the left face (e.g., X on Xbox, Square on PlayStation).</summary>
    LEFT_FACE_LEFT = 4,

    /// <summary>Top button on the right face (e.g., D-pad Up).</summary>
    RIGHT_FACE_UP = 5,
    /// <summary>Right button on the right face (e.g., D-pad Right).</summary>
    RIGHT_FACE_RIGHT = 6,
    /// <summary>Bottom button on the right face (e.g., D-pad Down).</summary>
    RIGHT_FACE_DOWN = 7,
    /// <summary>Left button on the right face (e.g., D-pad Left).</summary>
    RIGHT_FACE_LEFT = 8,

    /// <summary>Top button on the left trigger (e.g., LB on Xbox, L1 on PlayStation).</summary>
    LEFT_TRIGGER_TOP = 9,
    /// <summary>Bottom button on the left trigger (e.g., LT on Xbox, L2 on PlayStation).</summary>
    LEFT_TRIGGER_BOTTOM = 10,
    /// <summary>Top button on the right trigger (e.g., RB on Xbox, R1 on PlayStation).</summary>
    RIGHT_TRIGGER_TOP = 11,
    /// <summary>Bottom button on the right trigger (e.g., RT on Xbox, R2 on PlayStation).</summary>
    RIGHT_TRIGGER_BOTTOM = 12,

    /// <summary>Left button in the middle section (e.g., View/Select/Share).</summary>
    MIDDLE_LEFT = 13,
    /// <summary>Center button in the middle section (e.g., Start/Options/Menu).</summary>
    MIDDLE = 14,
    /// <summary>Right button in the middle section (e.g., Menu/Start/Options).</summary>
    MIDDLE_RIGHT = 15,

    /// <summary>Left thumbstick button (pressing the stick).</summary>
    LEFT_THUMB = 16,
    /// <summary>Right thumbstick button (pressing the stick).</summary>
    RIGHT_THUMB = 17,

    /// <summary>Left stick moved to the right.</summary>
    LEFT_STICK_RIGHT = 30,
    /// <summary>Left stick moved to the left.</summary>
    LEFT_STICK_LEFT = 40,
    /// <summary>Left stick moved down.</summary>
    LEFT_STICK_DOWN = 31,
    /// <summary>Left stick moved up.</summary>
    LEFT_STICK_UP = 41,

    /// <summary>Right stick moved to the right.</summary>
    RIGHT_STICK_RIGHT = 32,
    /// <summary>Right stick moved to the left.</summary>
    RIGHT_STICK_LEFT = 42,
    /// <summary>Right stick moved down.</summary>
    RIGHT_STICK_DOWN = 33,
    /// <summary>Right stick moved up.</summary>
    RIGHT_STICK_UP = 43,
}