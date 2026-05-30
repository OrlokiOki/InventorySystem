public class Armor : Item
{
    public ArmorSlot Slot        { get; set; }
    public Effect    Enchantment { get; private set; }
    public int       Defense     { get; set; }

    public Armor(int id, string name, int value, string description, ArmorSlot slot, int defense)
        : base(id, name, value, description)
    {
        Stackable = false;
        Slot      = slot;
        Defense   = defense;
    }

    public bool TryEnchant(Effect effect)
    {
        if (effect.Target != TargetType.Armor) return false;
        Enchantment = effect;
        return true;
    }
}
