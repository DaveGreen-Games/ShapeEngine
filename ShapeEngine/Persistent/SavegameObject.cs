namespace ShapeEngine.Persistent
{

    /// <summary>
    /// Simple base class for a savegame object. To serialize and save class every property needs to have public getter and setter!
    /// </summary>
    public abstract class SavegameObject
    {
        public string version { get; set; } = "v0.0.0";
        public string name { get; set; } = "";
    }
}
