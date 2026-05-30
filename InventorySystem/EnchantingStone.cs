public class EnchantingStone : Item
{
    public Effect Effect { get; set; }

    public EnchantingStone(int id, string name, int value, string description, Effect effect)
        : base(id, name, value, description)
    {
        if (effect.Target != TargetType.Weapon && effect.Target != TargetType.Armor)
            throw new System.ArgumentException("EnchantingStone must target Weapon or Armor.");
        Stackable = true;
        Effect    = effect;
    }

    public bool TryApplyTo(Weapon weapon) { return weapon.TryEnchant(Effect); }
    public bool TryApplyTo(Armor armor)   { return armor.TryEnchant(Effect); }
}
