using System;
using System.Collections.Generic;

public class PC : Character
{
    public List<Recipe> KnownRecipes { get; private set; }
    public List<Recipe> AllRecipes   { get; private set; }

    public PC(int id, string name, int inventoryLimit,
              int health, int strength, int magicka,
              int stamina, int defense, int damage)
        : base(id, name, inventoryLimit, health, strength, magicka, stamina, defense, damage)
    {
        KnownRecipes = new List<Recipe>();
        AllRecipes   = new List<Recipe>();
    }

    public bool LearnRecipe(Recipe recipe)
    {
        for (int i = 0; i < KnownRecipes.Count; i++)
        {
            if (KnownRecipes[i].Matches(recipe.Ingredient1, recipe.Ingredient2))
                return false;
        }
        KnownRecipes.Add(recipe);
        return true;
    }

    public Recipe FindRecipe(Item a, Item b)
    {
        for (int i = 0; i < AllRecipes.Count; i++)
        {
            if (AllRecipes[i].Matches(a, b))
                return AllRecipes[i];
        }
        return null;
    }

    public bool AddItem(Item item, int quantity = 1)
    {
        return AddItemToInventory(item, quantity);
    }

    public bool RemoveItem(int itemId, int quantity = 1)
    {
        return RemoveItemFromInventory(itemId, quantity);
    }

    public bool HasItem(int itemId, int quantity = 1)
    {
        return HasItemInInventory(itemId, quantity);
    }

    public List<InventorySlot> GetInventoryPublic()
    {
        return GetInventory();
    }

    public void SortByName()
    {
        List<InventorySlot> inv = GetInventory();
        for (int i = 1; i < inv.Count; i++)
        {
            InventorySlot current = inv[i];
            int j = i - 1;
            while (j >= 0 && string.Compare(inv[j].Item.Name, current.Item.Name, StringComparison.OrdinalIgnoreCase) > 0)
            {
                inv[j + 1] = inv[j];
                j--;
            }
            inv[j + 1] = current;
        }
    }

    public void SortByValue()
    {
        List<InventorySlot> inv = GetInventory();
        for (int i = 1; i < inv.Count; i++)
        {
            InventorySlot current = inv[i];
            int j = i - 1;
            while (j >= 0 && inv[j].Item.Value < current.Item.Value)
            {
                inv[j + 1] = inv[j];
                j--;
            }
            inv[j + 1] = current;
        }
    }

    public Consumable Craft(int ingredient1Id, int ingredient2Id)
    {
        InventorySlot slot1 = FindSlot(ingredient1Id);
        InventorySlot slot2 = FindSlot(ingredient2Id);

        if (slot1 == null || slot2 == null)
            return null;

        Item item1 = slot1.Item;
        Item item2 = slot2.Item;

        RemoveItem(ingredient1Id);
        RemoveItem(ingredient2Id);

        Recipe recipe = FindRecipe(item1, item2);
        if (recipe == null)
            return null;

        AddItem(recipe.Result);
        LearnRecipe(recipe);
        return recipe.Result;
    }

    public bool EquipRightHand(Weapon weapon)
    {
        if (!HasItem(weapon.Id)) return false;
        if (RightHandSlot != null) AddItem(RightHandSlot);
        RemoveItem(weapon.Id);
        RightHandSlot = weapon;
        return true;
    }

    public bool EquipLeftHandWeapon(Weapon weapon)
    {
        if (!HasItem(weapon.Id)) return false;
        if (ShieldSlot != null) { AddItem(ShieldSlot); ShieldSlot = null; }
        if (LeftHandWeaponSlot != null) AddItem(LeftHandWeaponSlot);
        RemoveItem(weapon.Id);
        LeftHandWeaponSlot = weapon;
        return true;
    }

    public bool EquipShield(Armor shield)
    {
        if (shield.Slot != ArmorSlot.Shield) return false;
        if (!HasItem(shield.Id)) return false;
        if (LeftHandWeaponSlot != null) { AddItem(LeftHandWeaponSlot); LeftHandWeaponSlot = null; }
        if (ShieldSlot != null) AddItem(ShieldSlot);
        RemoveItem(shield.Id);
        ShieldSlot = shield;
        return true;
    }

    public bool EquipArmor(Armor armor)
    {
        if (armor.Slot == ArmorSlot.Shield) return EquipShield(armor);
        if (!HasItem(armor.Id)) return false;

        Armor previous = null;
        if      (armor.Slot == ArmorSlot.Chest)     previous = ChestSlot;
        else if (armor.Slot == ArmorSlot.Helmet)    previous = HelmetSlot;
        else if (armor.Slot == ArmorSlot.Boots)     previous = BootsSlot;
        else if (armor.Slot == ArmorSlot.Gauntlets) previous = GauntletsSlot;

        if (previous != null) AddItem(previous);
        RemoveItem(armor.Id);

        if      (armor.Slot == ArmorSlot.Chest)     ChestSlot     = armor;
        else if (armor.Slot == ArmorSlot.Helmet)    HelmetSlot    = armor;
        else if (armor.Slot == ArmorSlot.Boots)     BootsSlot     = armor;
        else if (armor.Slot == ArmorSlot.Gauntlets) GauntletsSlot = armor;

        return true;
    }

    public bool EquipAccessory(Accessory accessory)
    {
        if (!HasItem(accessory.Id)) return false;

        Accessory previous = null;
        if      (accessory.Slot == AccessorySlot.Ring)     previous = RingSlot;
        else if (accessory.Slot == AccessorySlot.Necklace) previous = NecklaceSlot;

        if (previous != null) AddItem(previous);
        RemoveItem(accessory.Id);

        if      (accessory.Slot == AccessorySlot.Ring)     RingSlot     = accessory;
        else if (accessory.Slot == AccessorySlot.Necklace) NecklaceSlot = accessory;

        return true;
    }
}
