namespace ChirpStack_Http;

public class Setting
{
    public int Id { get; }
    public string Name { get; }
    public int Bytes { get; }
    
    public Setting(int id, string setting, int bytes)
    {
        Id = id;
        Name = setting;
        Bytes = bytes;
    }
}