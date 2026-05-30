public class InventorySlot
{
    public Item Item     { get; private set; }
    public int  Quantity { get; private set; }

    public InventorySlot(Item item, int quantity = 1)
    {
        if (!item.Stackable && quantity > 1)
            throw new System.InvalidOperationException(item.Name + " is not stackable.");
        Item     = item;
        Quantity = quantity;
    }

    public void Add(int amount = 1)
    {
        if (!Item.Stackable)
            throw new System.InvalidOperationException(Item.Name + " is not stackable.");
        Quantity += amount;
    }

    public bool Remove(int amount = 1)
    {
        Quantity -= amount;
        return Quantity <= 0;
    }
}
