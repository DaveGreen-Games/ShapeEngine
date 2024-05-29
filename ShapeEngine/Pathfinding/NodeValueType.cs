namespace ShapeEngine.Pathfinding;

public enum NodeValueType
{
    None = -1,
    Reset = 0,
    SetValue = 1,
    ResetThenSet = 2,
    Block = 3,
    Unblock = 4,
    ResetThenBlock = 5,
    AddBonus = 6,
    RemoveBonus = 7,
    ResetBonus = 8,
    AddFlat = 9,
    RemoveFlat = 10,
    ResetFlat = 11
}