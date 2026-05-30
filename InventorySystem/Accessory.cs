public class Accessory : Item
{
    public AccessorySlot Slot   { get; set; }
    public Effect        Effect { get; set; }

    public Accessory(int id, string name, int value, string description,
                     AccessorySlot slot, Effect effect = null)
        : base(id, name, value, description)
    {
        Stackable = false;
        Slot      = slot;
        Effect    = effect;
    }
}
