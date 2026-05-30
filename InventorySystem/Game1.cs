using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch           _spriteBatch;

    private SpriteFont _fontPixel;
    private Texture2D  _pixel;
    private Texture2D  _defaultItemIcon;

    private GameState       _currentState = GameState.DarkScreen;
    private DarkScreenState _darkScreen;
    private InventoryState  _inventoryState;
    private AlchemyState    _alchemyState;

    private PC _player;

    private const int SCREEN_W = 1920;
    private const int SCREEN_H = 1080;

    private RenderTarget2D _renderTarget;
    private KeyboardState  _prevKeyboard;
    private bool           _isFullscreen = false;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth  = SCREEN_W;
        _graphics.PreferredBackBufferHeight = SCREEN_H;
        _graphics.IsFullScreen              = false;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _renderTarget = new RenderTarget2D(GraphicsDevice, SCREEN_W, SCREEN_H);

        _fontPixel = Content.Load<SpriteFont>("FontPixel");

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _defaultItemIcon = TryLoadIcon("default");

        _player = new PC(1, "Aerin", 30, 100, 15, 20, 12, 8, 10);
        PopulateTestData();
        LoadItemIcons();

        _darkScreen     = new DarkScreenState(_fontPixel, SCREEN_W, SCREEN_H);
        _inventoryState = new InventoryState(_player, _fontPixel, _pixel, SCREEN_W, SCREEN_H, Content);
        _alchemyState   = new AlchemyState(_player, _fontPixel, _pixel, SCREEN_W, SCREEN_H, Content);
    }

    private void LoadItemIcons()
    {
        System.Collections.Generic.List<Item> allItems = new System.Collections.Generic.List<Item>();

        System.Collections.Generic.List<InventorySlot> inv = _player.GetInventoryPublic();
        for (int i = 0; i < inv.Count; i++)
        {
            if (!allItems.Contains(inv[i].Item))
                allItems.Add(inv[i].Item);
        }

        for (int i = 0; i < _player.AllRecipes.Count; i++)
        {
            Recipe r = _player.AllRecipes[i];
            if (!allItems.Contains(r.Ingredient1)) allItems.Add(r.Ingredient1);
            if (!allItems.Contains(r.Ingredient2)) allItems.Add(r.Ingredient2);
            if (!allItems.Contains(r.Result))      allItems.Add(r.Result);
        }

        for (int i = 0; i < allItems.Count; i++)
        {
            allItems[i].Icon = TryLoadIcon(allItems[i].Name);
        }
    }

    private Texture2D TryLoadIcon(string itemName)
    {
        string assetName = "item_" + itemName.ToLower().Replace(" ", "_");
        try
        {
            return Content.Load<Texture2D>(assetName);
        }
        catch
        {
            return _defaultItemIcon;
        }
    }

    private void PopulateTestData()
    {
        Effect healEffect    = new Effect(EffectType.HealHealth,     TargetType.Consumable, 30, 0f);
        Effect staminaEffect = new Effect(EffectType.RestoreStamina, TargetType.Consumable, 25, 10f);
        Effect magickaEffect = new Effect(EffectType.RestoreMagicka, TargetType.Consumable, 20, 0f);
        Effect fireEnchant   = new Effect(EffectType.FireDamage,     TargetType.Weapon,     15);

        Weapon     sword     = new Weapon(1,    "Iron Sword",     50,  "A sturdy iron sword.",  12);
        Weapon     dagger    = new Weapon(2,    "Steel Dagger",   35,  "Fast and light.",         8);
        Armor      helmet    = new Armor(3,     "Iron Helmet",    40,  "Basic head protection.", ArmorSlot.Helmet,     5);
        Armor      chest     = new Armor(4,     "Chain Mail",     80,  "Reliable chest piece.",  ArmorSlot.Chest,     12);
        Armor      boots     = new Armor(5,     "Leather Boots",  25,  "Light footwear.",        ArmorSlot.Boots,      3);
        Armor      gauntlets = new Armor(6,     "Iron Gauntlets", 30,  "Protect your hands.",    ArmorSlot.Gauntlets,  4);
        Armor      shield    = new Armor(7,     "Wooden Shield",  45,  "A basic shield.",        ArmorSlot.Shield,     8);
        Accessory  ring      = new Accessory(8, "Gold Ring",      120, "A shiny ring.",          AccessorySlot.Ring);
        Accessory  necklace  = new Accessory(9, "Amulet",         95,  "Old amulet.",            AccessorySlot.Necklace);

        Item herb    = new Consumable(20, "Red Herb",    5,  "A common herb.",          new Effect(EffectType.HealHealth,     TargetType.Consumable, 1));
        Item fungus  = new Consumable(21, "Blue Fungus", 5,  "A strange fungus.",       new Effect(EffectType.RestoreStamina, TargetType.Consumable, 1));
        Item crystal = new Consumable(22, "Mana Crystal",8, "Crystallised magic.",     new Effect(EffectType.RestoreMagicka, TargetType.Consumable, 1));
        Item dust    = new Consumable(23, "Bone Dust",   4,  "Ground from old bones.", new Effect(EffectType.HealHealth,     TargetType.Consumable, 1));

        Consumable potion   = new Consumable(10, "Health Potion",   20, "Restores 30 health.",   healEffect);
        Consumable stamPot  = new Consumable(11, "Stamina Draft",   18, "Restores 25 stamina.",  staminaEffect);
        Consumable manaPot  = new Consumable(12, "Mana Potion",     22, "Restores 20 magicka.",  magickaEffect);

        EnchantingStone stone = new EnchantingStone(13, "Fire Stone", 60, "Enchants with fire.", fireEnchant);

        _player.AddItem(sword);
        _player.AddItem(dagger);
        _player.AddItem(helmet);
        _player.AddItem(chest);
        _player.AddItem(boots);
        _player.AddItem(gauntlets);
        _player.AddItem(shield);
        _player.AddItem(ring);
        _player.AddItem(necklace);
        _player.AddItem(herb,    4);
        _player.AddItem(fungus,  3);
        _player.AddItem(crystal, 2);
        _player.AddItem(dust,    2);
        _player.AddItem(stone);

        Recipe r1 = new Recipe(herb,   fungus,  potion);
        Recipe r2 = new Recipe(fungus, crystal, stamPot);
        Recipe r3 = new Recipe(crystal, dust,   manaPot);

        _player.AllRecipes.Add(r1);
        _player.AllRecipes.Add(r2);
        _player.AllRecipes.Add(r3);

        _player.LearnRecipe(r1);
    }

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        KeyboardState kb = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            kb.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (kb.IsKeyDown(Keys.F11) && _prevKeyboard.IsKeyUp(Keys.F11))
        {
            _isFullscreen = !_isFullscreen;
            _graphics.IsFullScreen              = _isFullscreen;
            _graphics.PreferredBackBufferWidth  = _isFullscreen
                ? GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width
                : SCREEN_W;
            _graphics.PreferredBackBufferHeight = _isFullscreen
                ? GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
                : SCREEN_H;
            _graphics.ApplyChanges();
        }

        _prevKeyboard = kb;

        // cache state before update so a transition this frame

        GameState stateThisFrame = _currentState;

        switch (stateThisFrame)
        {
            case GameState.DarkScreen:
                _darkScreen.Update(dt, ref _currentState);

                if (_currentState == GameState.Inventory)
                    _inventoryState.OnOpen();
                break;

            case GameState.Inventory:
                _inventoryState.Update(dt, ref _currentState, () => _darkScreen.OnReturn());
                if (_inventoryState.ActiveTab == 1)
                {
                    _alchemyState.Update(dt);
                }
                break;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                           SamplerState.PointClamp, null, null);

        switch (_currentState)
        {
            case GameState.DarkScreen:
                _darkScreen.Draw(_spriteBatch);
                break;

            case GameState.Inventory:
                if (_inventoryState.ActiveTab == 1)
                {
                    _alchemyState.Draw(_spriteBatch);
                    _inventoryState.DrawTabsOnly(_spriteBatch);
                }
                else
                {
                    _inventoryState.Draw(_spriteBatch);
                }
                break;
        }

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        int backW = GraphicsDevice.PresentationParameters.BackBufferWidth;
        int backH = GraphicsDevice.PresentationParameters.BackBufferHeight;

        float scaleX = (float)backW / SCREEN_W;
        float scaleY = (float)backH / SCREEN_H;
        float scale  = System.Math.Min(scaleX, scaleY);

        int destW = (int)(SCREEN_W * scale);
        int destH = (int)(SCREEN_H * scale);
        int destX = (backW - destW) / 2;
        int destY = (backH - destH) / 2;

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
                           SamplerState.PointClamp, null, null);
        _spriteBatch.Draw(_renderTarget,
                          new Rectangle(destX, destY, destW, destH),
                          Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
