using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class InventoryState
{
    private PC         _pc;
    private SpriteFont _fontPixel;
    private Texture2D  _pixel;

    private Texture2D _texLeftPanel;
    private Texture2D _texRightPanel;
    private Texture2D _texSlotEmpty;
    private Texture2D _texSlotEquipped;
    private Texture2D _texGridCell;
    private Texture2D _texSearchBar;
    private Texture2D _texSortButton;
    private Texture2D _texSortMenuItem;
    private Texture2D _texScrollTrack;
    private Texture2D _texScrollThumb;
    private Texture2D _texTabActive;
    private Texture2D _texTabInactive;
    private Texture2D _texCharacterSprite;
    private Texture2D _texNameBox;

    private int _screenWidth  = 1920;
    private int _screenHeight = 1080;

    private Rectangle _panelRect;

    private Rectangle[] _tabRects;
    private string[]    _tabLabels = { "Inventory", "Alchemy", "Map" };
    private int         _activeTab = 0;

    public int ActiveTab { get { return _activeTab; } }

    public void DrawTabsOnly(SpriteBatch sb)
    {
        DrawTabs(sb);
    }

    private Rectangle _leftPanel;

    private Rectangle[] _equipSlots;
    private string[]    _equipSlotLabels =
    {
        "Head", "Necklace",
        "Armor", "Ring",
        "Gauntlets", "Boots",
        "R.Hand", "L.Hand"
    };

    private Rectangle _characterNameRect;
    private Rectangle _characterSpriteRect;

    private Rectangle _rightPanel;
    private Rectangle _searchBarRect;
    private Rectangle _sortButtonRect;
    private Rectangle _scrollbarTrack;
    private Rectangle _scrollbarThumb;
    private Rectangle _gridArea;

    private const int GRID_COLS       = 6;
    private const int CELL_SIZE       = 64;
    private const int CELL_PADDING    = 6;
    private int       _visibleRows    = 6;
    private Rectangle[,] _cellRects;   

    private int   _scrollRow        = 0;
    private int   _totalRows        = 0;
    private bool  _isDraggingScroll = false;
    private float _scrollDragOffsetY;
    
    private string _searchQuery        = "";
    private bool   _searchBarFocused   = false;
    private List<InventorySlot> _filteredSlots = new List<InventorySlot>();

    private bool      _sortMenuOpen = false;
    private Rectangle _sortByNameRect;
    private Rectangle _sortByValueRect;

    private bool          _isDragging          = false;
    private InventorySlot _draggedSlot         = null;
    private int           _dragSourceIndex     = -1;
    private int           _dragSourceEquipSlot = -1;
    private Vector2       _dragPosition;
    private Vector2       _dragOffset;
    private MouseState    _prevMouse;
    private KeyboardState _prevKeyboard;

    private Tooltip _tooltip;
    private int     _hoveredGridSlot   = -1;
    private int     _hoveredEquipSlot  = -1;

    private Color _colBackground    = new Color(20,  20,  20);
    private Color _colText          = new Color(140, 130, 115);
    private Color _colTextDim       = new Color(140, 130, 115);
    private Color _colHighlight     = new Color(120, 120, 200);

    public InventoryState(PC pc, SpriteFont fontPixel,
                          Texture2D pixel, int screenWidth, int screenHeight,
                          Microsoft.Xna.Framework.Content.ContentManager content)
    {
        _pc        = pc;
        _fontPixel = fontPixel;
        _pixel     = pixel;
        _screenWidth  = screenWidth;
        _screenHeight = screenHeight;

        LoadTextures(content);
        BuildLayout();
        RefreshFilter();
        _tooltip = new Tooltip(fontPixel, pixel, screenWidth, screenHeight);
    }

    public void OnOpen()
    {
        _prevKeyboard = Keyboard.GetState();
    }

    private void LoadTextures(Microsoft.Xna.Framework.Content.ContentManager content)
    {
        _texLeftPanel      = TryLoad(content, "ui_left_panel");
        _texRightPanel     = TryLoad(content, "ui_right_panel");
        _texSlotEmpty      = TryLoad(content, "ui_slot_empty");
        _texSlotEquipped   = TryLoad(content, "ui_slot_equipped");
        _texGridCell       = TryLoad(content, "ui_grid_cell");
        _texSearchBar      = TryLoad(content, "ui_search_bar");
        _texSortButton     = TryLoad(content, "ui_sort_button");
        _texSortMenuItem   = TryLoad(content, "ui_sort_menu_item");
        _texScrollTrack    = TryLoad(content, "ui_scrollbar_track");
        _texScrollThumb    = TryLoad(content, "ui_scrollbar_thumb");
        _texTabActive      = TryLoad(content, "ui_tab_active");
        _texTabInactive    = TryLoad(content, "ui_tab_inactive");
        _texCharacterSprite = TryLoad(content, "ui_character_sprite");
        _texNameBox         = TryLoad(content, "ui_name_box");
    }


    private Texture2D TryLoad(Microsoft.Xna.Framework.Content.ContentManager content, string name)
    {
        try
        {
            return content.Load<Texture2D>(name);
        }
        catch
        {
            return _pixel;
        }
    }

    private void BuildLayout()
    {
        int panelW = 1360;
        int panelH = 820;
        int panelX = (_screenWidth  - panelW) / 2;
        int panelY = (_screenHeight - panelH) / 2;
        _panelRect = new Rectangle(panelX, panelY, panelW, panelH);

        int tabW = 140;
        int tabH = 36;
        _tabRects = new Rectangle[3];
        for (int i = 0; i < 3; i++)
        {
            _tabRects[i] = new Rectangle(panelX + 10 + i * (tabW + 4), panelY - tabH, tabW, tabH);
        }

        int leftW  = 560;
        int margin = 12;
        _leftPanel = new Rectangle(panelX + margin, panelY + margin, leftW, panelH - margin * 2);

        int nameBoxH = 38;
        int nameBoxW = 260;
        _characterNameRect = new Rectangle(
            _leftPanel.X + (leftW - nameBoxW) / 2,
            _leftPanel.Y + 36,
            nameBoxW, nameBoxH
        );

        int spriteW       = 260;
        int spriteH       = 410;
        int spriteCentreY = _leftPanel.Y + (int)(_leftPanel.Height * 0.42f);
        _characterSpriteRect = new Rectangle(
            _leftPanel.X + (leftW - spriteW) / 2,
            spriteCentreY - spriteH / 2,
            spriteW, spriteH
        );

        _equipSlots  = new Rectangle[8];
        int slotSize = 80;
        int slotGapV = 28;

        int totalSlotsH = 4 * slotSize + 3 * slotGapV;
        int slotsStartY = spriteCentreY - totalSlotsH / 2;

        int spriteLeft  = _characterSpriteRect.X;
        int spriteRight = _characterSpriteRect.Right;
        int col0X = spriteLeft  - slotSize - 14;
        int col1X = spriteRight + 14;

        for (int row = 0; row < 4; row++)
        {
            int y = slotsStartY + row * (slotSize + slotGapV);
            _equipSlots[row * 2]     = new Rectangle(col0X, y, slotSize, slotSize);
            _equipSlots[row * 2 + 1] = new Rectangle(col1X, y, slotSize, slotSize);
        }

        int rightX = panelX + leftW + margin * 2;
        int rightW = panelW - leftW - margin * 3;
        _rightPanel = new Rectangle(rightX, panelY + margin, rightW, panelH - margin * 2);

        _searchBarRect = new Rectangle(rightX + 8, panelY + margin + 8, rightW - 100, 32);

        _sortButtonRect = new Rectangle(_searchBarRect.Right + 8, _searchBarRect.Y, 80, 32);

        _sortByNameRect  = new Rectangle(_sortButtonRect.X, _sortButtonRect.Bottom + 2, 80, 28);
        _sortByValueRect = new Rectangle(_sortButtonRect.X, _sortByNameRect.Bottom  + 2, 80, 28);

        int scrollbarW     = 14;
        int gridAreaTop    = _searchBarRect.Bottom + 10;
        int gridAreaBottom = _rightPanel.Bottom - 46;   // extra bottom margin so cells don't crowd the count label
        _scrollbarTrack = new Rectangle(
            _rightPanel.Right - scrollbarW - 4,
            gridAreaTop,
            scrollbarW,
            gridAreaBottom - gridAreaTop
        );

        _gridArea = new Rectangle(
            rightX + 8,
            gridAreaTop,
            _scrollbarTrack.X - rightX - 10,
            gridAreaBottom - gridAreaTop
        );

        int cellAndPad   = _gridArea.Width / GRID_COLS;
        int computedCell = cellAndPad - CELL_PADDING - 4;

        _cellRects = new Rectangle[_visibleRows, GRID_COLS];
        for (int row = 0; row < _visibleRows; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                _cellRects[row, col] = new Rectangle(
                    _gridArea.X + col * cellAndPad,
                    _gridArea.Y + row * (computedCell + CELL_PADDING),
                    computedCell,
                    computedCell
                );
            }
        }

        UpdateScrollThumb();
    }

    private void UpdateScrollThumb()
    {
        if (_totalRows <= _visibleRows)
        {
            // thumb fills the whole track when no scrolling needed
            _scrollbarThumb = _scrollbarTrack;
            return;
        }

        float ratio     = (float)_visibleRows / _totalRows;
        int   thumbH    = (int)(_scrollbarTrack.Height * ratio);
        if (thumbH < 20) thumbH = 20;

        float scrollRatio = (float)_scrollRow / (_totalRows - _visibleRows);
        int   thumbY      = _scrollbarTrack.Y + (int)((_scrollbarTrack.Height - thumbH) * scrollRatio);

        _scrollbarThumb = new Rectangle(_scrollbarTrack.X, thumbY, _scrollbarTrack.Width, thumbH);
    }

    private void RefreshFilter()
    {
        _filteredSlots.Clear();
        List<InventorySlot> inv = _pc.GetInventoryPublic();

        for (int i = 0; i < inv.Count; i++)
        {
            if (_searchQuery == "" ||
                inv[i].Item.Name.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _filteredSlots.Add(inv[i]);
            }
        }

        _totalRows = (int)Math.Ceiling((double)_filteredSlots.Count / GRID_COLS);
        if (_scrollRow > Math.Max(0, _totalRows - _visibleRows))
        {
            _scrollRow = Math.Max(0, _totalRows - _visibleRows);
        }

        UpdateScrollThumb();
    }

    public void Update(float deltaTime, ref GameState currentState, System.Action onClose)
    {
        MouseState    mouse = Mouse.GetState();
        KeyboardState kb    = Keyboard.GetState();

        Vector2 mousePos = new Vector2(mouse.X, mouse.Y);

        if (kb.IsKeyDown(Keys.E) && _prevKeyboard.IsKeyUp(Keys.E))
        {
            currentState = GameState.DarkScreen;
            onClose();
        }

        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
        {
            for (int i = 0; i < _tabRects.Length; i++)
            {
                if (_tabRects[i].Contains(mouse.X, mouse.Y))
                {
                    _activeTab = i;
                }
            }
        }


        if (_activeTab == 0)
        {
            HandleSearch(mouse, kb);
            HandleSortMenu(mouse);
            HandleScrollbar(mouse);
            HandleMouseWheel(mouse);
            HandleDragAndDrop(mouse, mousePos);
            HandleHover(mouse, mousePos);
        }

        _tooltip.Update(deltaTime);
        _prevMouse    = mouse;
        _prevKeyboard = kb;
    }

    private void HandleHover(MouseState mouse, Vector2 mousePos)
    {
        int newHoveredGrid  = -1;
        int newHoveredEquip = -1;

        for (int row = 0; row < _visibleRows; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                int idx = (_scrollRow + row) * GRID_COLS + col;
                if (_cellRects[row, col].Contains(mouse.X, mouse.Y) && idx < _filteredSlots.Count)
                {
                    newHoveredGrid = idx;
                    break;
                }
            }
            if (newHoveredGrid != -1) break;
        }

        Item[] equipped = GetEquippedItemsForHover();
        for (int i = 0; i < _equipSlots.Length; i++)
        {
            if (_equipSlots[i].Contains(mouse.X, mouse.Y) && equipped[i] != null)
            {
                newHoveredEquip = i;
                break;
            }
        }

        if (newHoveredGrid != _hoveredGridSlot || newHoveredEquip != _hoveredEquipSlot)
        {
            _hoveredGridSlot  = newHoveredGrid;
            _hoveredEquipSlot = newHoveredEquip;

            if (newHoveredGrid != -1 && !_isDragging)
                _tooltip.ShowHover(Tooltip.BuildItemTooltip(_filteredSlots[newHoveredGrid].Item), mousePos);
            else if (newHoveredEquip != -1 && !_isDragging)
                _tooltip.ShowHover(Tooltip.BuildItemTooltip(equipped[newHoveredEquip]), mousePos);
            else
                _tooltip.Hide();
        }
        else if ((newHoveredGrid != -1 || newHoveredEquip != -1) && !_isDragging)
        {
            if (newHoveredGrid != -1)
                _tooltip.ShowHover(Tooltip.BuildItemTooltip(_filteredSlots[newHoveredGrid].Item), mousePos);
            else
                _tooltip.ShowHover(Tooltip.BuildItemTooltip(equipped[newHoveredEquip]), mousePos);
        }

        if (newHoveredGrid == -1 && newHoveredEquip == -1)
            _tooltip.Hide();
    }

    private Item[] GetEquippedItemsForHover()
    {
        Item[] e = new Item[8];
        e[0] = _pc.HelmetSlot;
        e[1] = _pc.NecklaceSlot;
        e[2] = _pc.ChestSlot;
        e[3] = _pc.RingSlot;
        e[4] = _pc.GauntletsSlot;
        e[5] = _pc.BootsSlot;
        e[6] = _pc.RightHandSlot;
        e[7] = _pc.LeftHandWeaponSlot != null ? (Item)_pc.LeftHandWeaponSlot : _pc.ShieldSlot;
        return e;
    }

    private void HandleSearch(MouseState mouse, KeyboardState kb)
    {
        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
        {
            _searchBarFocused = _searchBarRect.Contains(mouse.X, mouse.Y);
        }

        if (!_searchBarFocused) return;

        // confirm search on Enter
        Keys[] pressed = kb.GetPressedKeys();
        for (int i = 0; i < pressed.Length; i++)
        {
            if (_prevKeyboard.IsKeyUp(pressed[i]))
            {
                if (pressed[i] == Keys.Enter)
                {
                    _searchBarFocused = false;
                    RefreshFilter();
                    return;
                }

                if (pressed[i] == Keys.Back && _searchQuery.Length > 0)
                {
                    _searchQuery = _searchQuery.Substring(0, _searchQuery.Length - 1);
                    return;
                }

                string ch = KeyToChar(pressed[i], kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift));
                if (ch != "")
                {
                    _searchQuery += ch;
                }
            }
        }
    }

    private string KeyToChar(Keys key, bool shift)
    {
        if (key >= Keys.A && key <= Keys.Z)
        {
            string letter = key.ToString();
            return shift ? letter.ToUpper() : letter.ToLower();
        }

        if (key >= Keys.D0 && key <= Keys.D9)
        {
            return ((int)(key - Keys.D0)).ToString();
        }
        if (key == Keys.Space)  return " ";
        return "";
    }

    private void HandleSortMenu(MouseState mouse)
    {
        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
        {
            if (_sortButtonRect.Contains(mouse.X, mouse.Y))
            {
                _sortMenuOpen = !_sortMenuOpen;
                return;
            }

            if (_sortMenuOpen)
            {
                if (_sortByNameRect.Contains(mouse.X, mouse.Y))
                {
                    _pc.SortByName();
                    _sortMenuOpen = false;
                    RefreshFilter();
                    return;
                }

                if (_sortByValueRect.Contains(mouse.X, mouse.Y))
                {
                    _pc.SortByValue();
                    _sortMenuOpen = false;
                    RefreshFilter();
                    return;
                }

                _sortMenuOpen = false;
            }
        }
    }

    private void HandleScrollbar(MouseState mouse)
    {
        if (_totalRows <= _visibleRows) return;

        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
        {
            if (_scrollbarThumb.Contains(mouse.X, mouse.Y))
            {
                _isDraggingScroll   = true;
                _scrollDragOffsetY  = mouse.Y - _scrollbarThumb.Y;
            }
        }

        if (mouse.LeftButton == ButtonState.Released)
        {
            _isDraggingScroll = false;
        }

        if (_isDraggingScroll)
        {
            float newThumbY   = mouse.Y - _scrollDragOffsetY;
            float trackTop    = _scrollbarTrack.Y;
            float trackRange  = _scrollbarTrack.Height - _scrollbarThumb.Height;

            if (trackRange > 0)
            {
                float ratio    = (newThumbY - trackTop) / trackRange;
                ratio          = MathHelper.Clamp(ratio, 0f, 1f);
                _scrollRow     = (int)Math.Round(ratio * (_totalRows - _visibleRows));
            }

            UpdateScrollThumb();
        }
    }

    private void HandleMouseWheel(MouseState mouse)
    {
        if (_totalRows <= _visibleRows) return;

        int delta = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
        if (delta != 0)
        {
            int direction = delta > 0 ? -1 : 1;
            _scrollRow    = MathHelper.Clamp(_scrollRow + direction, 0, _totalRows - _visibleRows);
            UpdateScrollThumb();
        }
    }

    private void HandleDragAndDrop(MouseState mouse, Vector2 mousePos)
    {

        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
        {
            if (!_isDragging)
            {
                TryBeginDrag(mouse, mousePos);
            }
        }

        if (_isDragging && mouse.LeftButton == ButtonState.Pressed)
        {
            _dragPosition = mousePos - _dragOffset;
        }

        if (_isDragging && mouse.LeftButton == ButtonState.Released)
        {
            TryDrop(mouse);
            _isDragging          = false;
            _draggedSlot         = null;
            _dragSourceIndex     = -1;
            _dragSourceEquipSlot = -1;
        }
    }

    private void TryBeginDrag(MouseState mouse, Vector2 mousePos)
    {
        for (int row = 0; row < _visibleRows; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                if (_cellRects[row, col].Contains(mouse.X, mouse.Y))
                {
                    int slotIndex = (_scrollRow + row) * GRID_COLS + col;
                    if (slotIndex < _filteredSlots.Count)
                    {
                        _isDragging          = true;
                        _dragSourceIndex     = slotIndex;
                        _dragSourceEquipSlot = -1;
                        _draggedSlot         = _filteredSlots[slotIndex];
                        _dragOffset          = mousePos - new Vector2(_cellRects[row, col].X, _cellRects[row, col].Y);
                        _dragPosition        = mousePos - _dragOffset;
                    }
                    return;
                }
            }
        }

        Item[] equipped = GetEquippedItems();
        for (int i = 0; i < _equipSlots.Length; i++)
        {
            if (_equipSlots[i].Contains(mouse.X, mouse.Y) && equipped[i] != null)
            {
                _isDragging          = true;
                _dragSourceIndex     = -1;
                _dragSourceEquipSlot = i;
                _draggedSlot         = new InventorySlot(equipped[i], 1);
                _dragOffset          = mousePos - new Vector2(_equipSlots[i].X, _equipSlots[i].Y);
                _dragPosition        = mousePos - _dragOffset;
                return;
            }
        }
    }

    private void TryDrop(MouseState mouse)
    {
        bool droppedOnGrid = _gridArea.Contains(mouse.X, mouse.Y);

        for (int i = 0; i < _equipSlots.Length; i++)
        {
            if (_equipSlots[i].Contains(mouse.X, mouse.Y))
            {
                
                if (_dragSourceEquipSlot == -1)
                {
                    TryEquipToSlot(i);
                }
                else if (_dragSourceEquipSlot != i)
                {
                    TryUnequipFromSlot(_dragSourceEquipSlot);
                    TryEquipToSlot(i);
                }
                RefreshFilter();
                return;
            }
        }

        if (_dragSourceEquipSlot != -1)
        {
            TryUnequipFromSlot(_dragSourceEquipSlot);
            RefreshFilter();
        }

        // if dragged from grid and dropped back on grid, do nothing item stays
    }

    private void TryUnequipFromSlot(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: 
                if (_pc.HelmetSlot != null)
                {
                    _pc.AddItem(_pc.HelmetSlot);
                    _pc.HelmetSlot = null;
                }
                break;
            case 1: 
                if (_pc.NecklaceSlot != null)
                {
                    _pc.AddItem(_pc.NecklaceSlot);
                    _pc.NecklaceSlot = null;
                }
                break;
            case 2:
                if (_pc.ChestSlot != null)
                {
                    _pc.AddItem(_pc.ChestSlot);
                    _pc.ChestSlot = null;
                }
                break;
            case 3: 
                if (_pc.RingSlot != null)
                {
                    _pc.AddItem(_pc.RingSlot);
                    _pc.RingSlot = null;
                }
                break;
            case 4: 
                if (_pc.GauntletsSlot != null)
                {
                    _pc.AddItem(_pc.GauntletsSlot);
                    _pc.GauntletsSlot = null;
                }
                break;
            case 5:
                if (_pc.BootsSlot != null)
                {
                    _pc.AddItem(_pc.BootsSlot);
                    _pc.BootsSlot = null;
                }
                break;
            case 6:
                if (_pc.RightHandSlot != null)
                {
                    _pc.AddItem(_pc.RightHandSlot);
                    _pc.RightHandSlot = null;
                }
                break;
            case 7:
                if (_pc.LeftHandWeaponSlot != null)
                {
                    _pc.AddItem(_pc.LeftHandWeaponSlot);
                    _pc.LeftHandWeaponSlot = null;
                }
                else if (_pc.ShieldSlot != null)
                {
                    _pc.AddItem(_pc.ShieldSlot);
                    _pc.ShieldSlot = null;
                }
                break;
        }
    }

    private void TryEquipToSlot(int slotIndex)
    {
        if (_draggedSlot == null) return;

        Item item = _draggedSlot.Item;

        switch (slotIndex)
        {
            case 0: 
                if (item is Armor && ((Armor)item).Slot == ArmorSlot.Helmet)
                    _pc.EquipArmor((Armor)item);
                break;

            case 1: 
                if (item is Accessory && ((Accessory)item).Slot == AccessorySlot.Necklace)
                    _pc.EquipAccessory((Accessory)item);
                break;

            case 2: 
                if (item is Armor && ((Armor)item).Slot == ArmorSlot.Chest)
                    _pc.EquipArmor((Armor)item);
                break;

            case 3: 
                if (item is Accessory && ((Accessory)item).Slot == AccessorySlot.Ring)
                    _pc.EquipAccessory((Accessory)item);
                break;

            case 4:
                if (item is Armor && ((Armor)item).Slot == ArmorSlot.Gauntlets)
                    _pc.EquipArmor((Armor)item);
                break;

            case 5:
                if (item is Armor && ((Armor)item).Slot == ArmorSlot.Boots)
                    _pc.EquipArmor((Armor)item);
                break;

            case 6: 
                if (item is Weapon)
                    _pc.EquipRightHand((Weapon)item);
                break;

            case 7: 
                if (item is Weapon)
                    _pc.EquipLeftHandWeapon((Weapon)item);
                else if (item is Armor && ((Armor)item).Slot == ArmorSlot.Shield)
                    _pc.EquipShield((Armor)item);
                break;
        }
    }

    public void Draw(SpriteBatch sb)
    {
        DrawRect(sb, new Rectangle(0, 0, _screenWidth, _screenHeight), _colBackground * 0.85f);

        DrawTabs(sb);

        if (_activeTab == 0)
        {
            DrawLeftPanel(sb);
            DrawRightPanel(sb);
        }
        else
        {
            Vector2 size = _fontPixel.MeasureString(_tabLabels[_activeTab]);
            Vector2 pos  = new Vector2(
                _panelRect.X + (_panelRect.Width  - size.X) / 2f,
                _panelRect.Y + (_panelRect.Height - size.Y) / 2f
            );
            sb.DrawString(_fontPixel, _tabLabels[_activeTab] + " - coming soon", pos, _colTextDim);
        }

        if (_isDragging && _draggedSlot != null)
        {
            Rectangle ghostRect = new Rectangle((int)_dragPosition.X, (int)_dragPosition.Y, CELL_SIZE, CELL_SIZE);
            sb.Draw(_texGridCell, ghostRect, _colHighlight * 0.75f);
            sb.DrawString(_fontPixel, _draggedSlot.Item.Name,
                          new Vector2(_dragPosition.X, _dragPosition.Y + CELL_SIZE + 2), _colText);
        }

        if (_sortMenuOpen)
        {
            sb.Draw(_texSortMenuItem, _sortByNameRect,  Color.White);
            sb.Draw(_texSortMenuItem, _sortByValueRect, Color.White);
            DrawStringCentred(sb, _fontPixel, "By Name",  _sortByNameRect,  _colText);
            DrawStringCentred(sb, _fontPixel, "By Value", _sortByValueRect, _colText);
        }

        _tooltip.Draw(sb);
    }

    private void DrawLeftPanel(SpriteBatch sb)
    {
        sb.Draw(_texLeftPanel, _leftPanel, Color.White);

        sb.Draw(_texNameBox, _characterNameRect, Color.White);
        DrawStringCentred(sb, _fontPixel, _pc.Name, _characterNameRect, _colText);

        sb.Draw(_texCharacterSprite, _characterSpriteRect, Color.White);

        Item[] equipped = GetEquippedItems();
        for (int i = 0; i < _equipSlots.Length; i++)
        {
            bool isBeingDragged = _isDragging && _dragSourceEquipSlot == i;

            Texture2D slotTex = (equipped[i] != null && !isBeingDragged)
                                    ? _texSlotEquipped
                                    : _texSlotEmpty;

            Color slotTint = isBeingDragged ? _colHighlight * 0.5f : Color.White;
            sb.Draw(slotTex, _equipSlots[i], slotTint);


            Vector2 labelSize = _fontPixel.MeasureString(_equipSlotLabels[i]);
            sb.DrawString(_fontPixel, _equipSlotLabels[i],
                          new Vector2(_equipSlots[i].X + (_equipSlots[i].Width  - labelSize.X) / 2f,
                                      _equipSlots[i].Y - labelSize.Y - 3),
                          _colTextDim);

            if (equipped[i] != null && !isBeingDragged)
            {
                if (equipped[i].Icon != null)
                {
                    // icon with a small inset so it doesn't touch the slot border
                    int    inset    = 6;
                    Rectangle iconRect = new Rectangle(
                        _equipSlots[i].X + inset,
                        _equipSlots[i].Y + inset,
                        _equipSlots[i].Width  - inset * 2,
                        _equipSlots[i].Height - inset * 2
                    );
                    sb.Draw(equipped[i].Icon, iconRect, Color.White);
                }
                else
                {
                    string label = TruncateToFit(equipped[i].Name, _fontPixel, _equipSlots[i].Width - 6);
                    DrawStringCentred(sb, _fontPixel, label, _equipSlots[i], _colText);
                }
            }
        }
    }

    private Item[] GetEquippedItems()
    {
        Item[] equipped = new Item[8];
        equipped[0] = _pc.HelmetSlot;
        equipped[1] = _pc.NecklaceSlot;
        equipped[2] = _pc.ChestSlot;
        equipped[3] = _pc.RingSlot;
        equipped[4] = _pc.GauntletsSlot;
        equipped[5] = _pc.BootsSlot;
        equipped[6] = _pc.RightHandSlot;
        equipped[7] = _pc.LeftHandWeaponSlot != null ? (Item)_pc.LeftHandWeaponSlot : _pc.ShieldSlot;
        return equipped;
    }

    private void DrawRightPanel(SpriteBatch sb)
    {
        sb.Draw(_texRightPanel, _rightPanel, Color.White);

        Texture2D searchTex  = _texSearchBar;
        Color     searchTint = _searchBarFocused ? _colHighlight : Color.White;
        sb.Draw(searchTex, _searchBarRect, searchTint);

        string searchDisplay = _searchQuery == "" ? "Search..." : _searchQuery;
        Color  searchTextCol = _searchQuery == "" ? _colTextDim : _colText;
        sb.DrawString(_fontPixel, searchDisplay,
                      new Vector2(_searchBarRect.X + 6, _searchBarRect.Y + 7),
                      searchTextCol);

        sb.Draw(_texSortButton, _sortButtonRect, Color.White);
        DrawStringCentred(sb, _fontPixel, "=== Sort", _sortButtonRect, _colText);

        sb.Draw(_texScrollTrack, _scrollbarTrack, Color.White);
        if (_totalRows > _visibleRows)
        {
            sb.Draw(_texScrollThumb, _scrollbarThumb, Color.White);
        }

        for (int row = 0; row < _visibleRows; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                int slotIndex = (_scrollRow + row) * GRID_COLS + col;
                bool isSource = _isDragging && slotIndex == _dragSourceIndex;

                Color cellTint = isSource ? _colHighlight * 0.4f : Color.White;
                sb.Draw(_texGridCell, _cellRects[row, col], cellTint);

                if (slotIndex < _filteredSlots.Count)
                {
                    InventorySlot slot = _filteredSlots[slotIndex];
                    DrawItemInCell(sb, slot.Item, slot.Quantity, _cellRects[row, col]);
                }
            }
        }

        string countLabel = _filteredSlots.Count + " / " + _pc.InventoryLimit + " items";
        sb.DrawString(_fontPixel, countLabel,
                      new Vector2(_gridArea.X, _gridArea.Bottom + 6),
                      _colTextDim);
    }

    private void DrawRect(SpriteBatch sb, Rectangle rect, Color color)
    {
        sb.Draw(_pixel, rect, color);
    }



    private void DrawStringCentred(SpriteBatch sb, SpriteFont font, string text, Rectangle bounds, Color color)
    {
        Vector2 size = font.MeasureString(text);
        Vector2 pos  = new Vector2(
            bounds.X + (bounds.Width  - size.X) / 2f,
            bounds.Y + (bounds.Height - size.Y) / 2f
        );
        sb.DrawString(font, text, pos, color);
    }

    private void DrawItemInCell(SpriteBatch sb, Item item, int quantity, Rectangle cell)
    {
        if (item.Icon != null)
        {
            int inset = 4;
            Rectangle iconRect = new Rectangle(
                cell.X + inset, cell.Y + inset,
                cell.Width  - inset * 2, cell.Height - inset * 2
            );
            sb.Draw(item.Icon, iconRect, Color.White);
        }
        else
        {
            string label = TruncateToFit(item.Name, _fontPixel, cell.Width - 4);
            sb.DrawString(_fontPixel, label, new Vector2(cell.X + 3, cell.Y + 4), _colTextDim);
        }

        if (item.Stackable && quantity > 1)
        {
            string  qty     = "x" + quantity;
            Vector2 qtySize = _fontPixel.MeasureString(qty);
            sb.DrawString(_fontPixel, qty,
                          new Vector2(cell.Right  - qtySize.X - 3,
                                      cell.Bottom - qtySize.Y - 3),
                          _colTextDim);
        }
    }

    private string TruncateToFit(string text, SpriteFont font, int maxWidth)
    {
        if (font.MeasureString(text).X <= maxWidth)
        {
            return text;
        }

        string truncated = text;
        while (truncated.Length > 0 && font.MeasureString(truncated + "..").X > maxWidth)
        {
            truncated = truncated.Substring(0, truncated.Length - 1);
        }

        return truncated + "..";
    }

    private void DrawTabs(SpriteBatch sb)
    {
        for (int i = 0; i < _tabRects.Length; i++)
        {
            bool      isActive = (i == _activeTab);
            Texture2D tabTex   = isActive ? _texTabActive : _texTabInactive;
            sb.Draw(tabTex, _tabRects[i], Color.White);

            if (isActive)
            {
                DrawStringCentred(sb, _fontPixel, _tabLabels[i], _tabRects[i], _colText);
            }
            else
            {
                // for inactive tabs
                float     scale    = 0.85f;
                Vector2   size     = _fontPixel.MeasureString(_tabLabels[i]) * scale;
                Vector2   pos      = new Vector2(
                    _tabRects[i].X + (_tabRects[i].Width  - size.X) / 2f,
                    _tabRects[i].Y + (_tabRects[i].Height - size.Y) / 2f + 4f
                );
                sb.DrawString(_fontPixel, _tabLabels[i], pos, _colTextDim,
                              0f, Vector2.Zero, scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            }
        }
    }
}
