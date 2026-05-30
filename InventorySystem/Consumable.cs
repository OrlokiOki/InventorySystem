public class Consumable : Item
{
    public Effect Effect { get; set; }

    public Consumable(int id, string name, int value, string description, Effect effect)
        : base(id, name, value, description)
    {
        Stackable = true;
        Effect    = effect;
    }
}
