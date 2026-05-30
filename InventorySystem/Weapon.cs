public class Weapon : Item
{
    public Effect Enchantment { get; private set; }
    public int    Damage      { get; set; }

    public Weapon(int id, string name, int value, string description, int damage)
        : base(id, name, value, description)
    {
        Stackable = false;
        Damage    = damage;
    }

    public bool TryEnchant(Effect effect)
    {
        if (effect.Target != TargetType.Weapon) return false;
        Enchantment = effect;
        return true;
    }
}
