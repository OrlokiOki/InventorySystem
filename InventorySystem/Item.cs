using Microsoft.Xna.Framework.Graphics;

public abstract class Item
{
    public int       Id          { get; set; }
    public string    Name        { get; set; }
    public bool      Stackable   { get; protected set; }
    public int       Value       { get; set; }
    public string    Description { get; set; }
    public Texture2D Icon        { get; set; }

    protected Item(int id, string name, int value, string description)
    {
        Id          = id;
        Name        = name;
        Value       = value;
        Description = description;
    }
}
