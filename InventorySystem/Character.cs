using System.Collections.Generic;

public abstract class Character
{
    public int    Id             { get; set; }
    public string Name           { get; set; }
    public int    Health         { get; set; }
    public int    Strength       { get; set; }
    public int    Magicka        { get; set; }
    public int    Stamina        { get; set; }
    public int    Defense        { get; set; }
    public int    Damage         { get; set; }
    public int    InventoryLimit { get; set; }

    public Armor     ChestSlot        { get; set; }
    public Armor     HelmetSlot       { get; set; }
    public Armor     BootsSlot        { get; set; }
    public Armor     GauntletsSlot    { get; set; }
    public Armor     ShieldSlot       { get; set; }
    public Accessory RingSlot         { get; set; }
    public Accessory NecklaceSlot     { get; set; }
    public Weapon    RightHandSlot    { get; set; }
    public Weapon    LeftHandWeaponSlot { get; set; }

    private List<InventorySlot> _inventory;

    protected Character(int id, string name, int inventoryLimit,
                        int health, int strength, int magicka,
                        int stamina, int defense, int damage)
    {
        Id             = id;
        Name           = name;
        InventoryLimit = inventoryLimit;
        Health         = health;
        Strength       = strength;
        Magicka        = magicka;
        Stamina        = stamina;
        Defense        = defense;
        Damage         = damage;
        _inventory     = new List<InventorySlot>();
    }

    protected bool HasRoomFor(Item item)
    {
        if (item.Stackable)
        {
            for (int i = 0; i < _inventory.Count; i++)
                if (_inventory[i].Item.Id == item.Id) return true;
        }
        return _inventory.Count < InventoryLimit;
    }

    protected InventorySlot FindSlot(int itemId)
    {
        for (int i = 0; i < _inventory.Count; i++)
            if (_inventory[i].Item.Id == itemId) return _inventory[i];
        return null;
    }

    protected bool AddItemToInventory(Item item, int quantity = 1)
    {
        if (!HasRoomFor(item)) return false;

        if (item.Stackable)
        {
            InventorySlot existing = FindSlot(item.Id);
            if (existing != null) { existing.Add(quantity); return true; }
        }

        _inventory.Add(new InventorySlot(item, quantity));
        return true;
    }

    protected bool RemoveItemFromInventory(int itemId, int quantity = 1)
    {
        InventorySlot slot = FindSlot(itemId);
        if (slot == null || slot.Quantity < quantity) return false;
        if (slot.Remove(quantity)) _inventory.Remove(slot);
        return true;
    }

    protected bool HasItemInInventory(int itemId, int quantity = 1)
    {
        InventorySlot slot = FindSlot(itemId);
        return slot != null && slot.Quantity >= quantity;
    }

    protected List<InventorySlot> GetInventory()
    {
        return _inventory;
    }
}
