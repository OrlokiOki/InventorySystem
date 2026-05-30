public class Recipe
{
    public Item       Ingredient1 { get; private set; }
    public Item       Ingredient2 { get; private set; }
    public Consumable Result      { get; private set; }

    public Recipe(Item ingredient1, Item ingredient2, Consumable result)
    {
        Ingredient1 = ingredient1;
        Ingredient2 = ingredient2;
        Result      = result;
    }

    public bool Matches(Item a, Item b)
    {
        return (a.Id == Ingredient1.Id && b.Id == Ingredient2.Id) ||
               (a.Id == Ingredient2.Id && b.Id == Ingredient1.Id);
    }
}
