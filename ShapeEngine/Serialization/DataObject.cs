namespace ShapeEngine.Serialization;

public abstract record DataObject
{
    public required string Name { get; set; }//set needs to be for serialization
    public required int SpawnWeight { get; set; } 
}