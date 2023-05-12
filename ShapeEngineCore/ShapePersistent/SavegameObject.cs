namespace ShapePersistent
{
    //without getters and setter serialization does not work!!!

    public class SavegameObject
    {
        public SavegameObject() { }

        public string version { get; set; } = "v0.0.0";
        public string name { get; set; } = "";
    }
}
