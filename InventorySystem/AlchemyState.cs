using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class AlchemyState
{
    private PC         _pc;
    private SpriteFont _fontPixel;
    private Texture2D  _pixel;

    private Texture2D _texLeftPanel;
    private Texture2D _texRightPanel;
    private Texture2D _texMixSlot;
    private Texture2D _texResultSlot;
    private Texture2D _texGridCell;
    private Texture2D _texRecipeSlot;
    private Texture2D _texRecipeSlotDimmed;
    private Texture2D _texScrollTrack;
    private Texture2D _texScrollThumb;

    private int _screenW;
    private int _screenH;

    private Rectangle _leftPanel;
    private Rectangle _rightPanel;

    private Rectangle _mixSlot1;
    private Rectangle _mixSlot2;
    private Rectangle _resultSlot;
    private Rectangle _mixButton;
    private Rectangle _arrowRect1;
    private Rectangle _arrowRect2;

    private Rectangle _recipeListArea;
    private List<Rectangle> _recipeRects = new List<Rectangle>();
    private const int RECIPE_ROWS_VISIBLE = 4;
    private int _recipeScrollRow  = 0;
    private Rectangle _recipeScrollTrack;
    private Rectangle _recipeScrollThumb;

    private Rectangle   _gridArea;
    private Rectangle[,] _cellRects;
    private Rectangle   _scrollbarTrack;
    private Rectangle   _scrollbarThumb;
    private const int   GRID_COLS      = 6;
    private const int   CELL_PADDING   = 6;
    private int         _visibleRows   = 6;
    private int         _scrollRow     = 0;
    private int         _totalRows     = 0;

    // alchem states
    private InventorySlot _mixSlot1Item  = null;
    private InventorySlot _mixSlot2Item  = null;
    private Recipe        _previewRecipe = null;

    private List<InventorySlot> _filteredSlots = new List<InventorySlot>();

    private bool          _isDragging          = false;
    private InventorySlot _draggedSlot         = null;
    private int           _dragSourceGrid      = -1;   // index in _filteredSlots
    private int           _dragSourceMixSlot   = -1;   // 0, 1 or -1
    private Vector2       _dragPosition;
    private Vector2       _dragOffset;

    private bool  _isDraggingScroll    = false;
    private float _scrollDragOffsetY;
    private bool  _isDraggingRecipeScroll = false;
    private float _recipeScrollDragOffsetY;

    private Tooltip _tooltip;
    private int     _hoveredGridSlot   = -1;
    private int     _hoveredRecipe     = -1;

    private Tooltip _message;

    private MouseState    _prevMouse;
    private KeyboardState _prevKeyboard;

    private Color _colSlot       = new Color(50,  45,  40);
    private Color _colSlotBorder = new Color(100, 85,  65);
    private Color _colText       = new Color(140, 130, 115);
    private Color _colTextDim    = new Color(140, 130, 115);
    private Color _colHighlight  = new Color(180, 140, 80);
    private Color _colSuccess    = new Color(120, 200, 100);
    private Color _colFail       = new Color(200, 80,  80);

    public AlchemyState(PC pc, SpriteFont fontPixel,
                        Texture2D pixel, int screenW, int screenH,
                        Microsoft.Xna.Framework.Content.ContentManager content)
    {
        _pc        = pc;
        _fontPixel = fontPixel;
        _pixel     = pixel;
        _screenW   = screenW;
        _screenH   = screenH;

        LoadTextures(content);
        BuildLayout();
        RefreshGrid();

        _tooltip = new Tooltip(fontPixel, pixel, screenW, screenH);
        _message = new Tooltip(fontPixel, pixel, screenW, screenH);
    }

    private void LoadTextures(Microsoft.Xna.Framework.Content.ContentManager content)
    {
        _texLeftPanel        = TryLoad(content, "ui_alchemy_left_panel");
        _texRightPanel       = TryLoad(content, "ui_right_panel");
        _texMixSlot          = TryLoad(content, "ui_mix_slot");
        _texResultSlot       = TryLoad(content, "ui_result_slot");
        _texGridCell         = TryLoad(content, "ui_grid_cell");
        _texRecipeSlot       = TryLoad(content, "ui_recipe_slot");
        _texRecipeSlotDimmed = TryLoad(content, "ui_recipe_slot_dimmed");
        _texScrollTrack      = TryLoad(content, "ui_scrollbar_track");
        _texScrollThumb      = TryLoad(content, "ui_scrollbar_thumb");
    }

    private Texture2D TryLoad(Microsoft.Xna.Framework.Content.ContentManager content, string name)
    {
        try   { return content.Load<Texture2D>(name); }
        catch { return _pixel; }
    }


    private void BuildLayout()
    {
        int panelW = 1360;
        int panelH = 820;
        int panelX = (_screenW - panelW) / 2;
        int panelY = (_screenH - panelH) / 2;
        int margin = 12;

        int leftW = 560;
        _leftPanel  = new Rectangle(panelX + margin,              panelY + margin, leftW,                panelH - margin * 2);
        _rightPanel = new Rectangle(panelX + leftW + margin * 2,  panelY + margin, panelW - leftW - margin * 3, panelH - margin * 2);

        //mix area
        int mixTop  = _leftPanel.Y + 100;
        int slotSz  = 90;
        int mixAreaW = leftW - 40;
        int totalMixW = slotSz * 3 + 60 * 2;
        int mixStartX = _leftPanel.X + (mixAreaW - totalMixW) / 2 + 20;

        _mixSlot1   = new Rectangle(mixStartX,               mixTop, slotSz, slotSz);
        _arrowRect1 = new Rectangle(_mixSlot1.Right + 8,     mixTop + slotSz / 2 - 10, 44, 20);
        _mixSlot2   = new Rectangle(_arrowRect1.Right + 8,   mixTop, slotSz, slotSz);
        _arrowRect2 = new Rectangle(_mixSlot2.Right + 8,     mixTop + slotSz / 2 - 10, 44, 20);
        _resultSlot = new Rectangle(_arrowRect2.Right + 8,   mixTop, slotSz, slotSz);

        _mixButton = new Rectangle(
            _leftPanel.X + (leftW - 120) / 2,
            _mixSlot1.Bottom + 14,
            120, 34
        );

        // recip area
        int recipeTop   = _mixButton.Bottom + 70;
        int recipeH     = _leftPanel.Bottom - recipeTop - margin - 20;
        int recipeSlotH = 56;
        int scrollW     = 10;

        // scrolbar positioning stuff
        _recipeScrollTrack = new Rectangle(
            _leftPanel.Right - scrollW - 30,
            recipeTop,
            scrollW,
            recipeH
        );

        int recipeListX = _leftPanel.X + 20;
        int recipeListW = _recipeScrollTrack.X - recipeListX - 4;
        _recipeListArea = new Rectangle(
            recipeListX,
            recipeTop,
            recipeListW,
            recipeH
        );

        _recipeRects.Clear();
        for (int i = 0; i < RECIPE_ROWS_VISIBLE; i++)
        {
            _recipeRects.Add(new Rectangle(
                _recipeListArea.X,
                _recipeListArea.Y + i * (recipeSlotH + 6),
                _recipeListArea.Width,
                recipeSlotH
            ));
        }

        UpdateRecipeScrollThumb();

        int rightX      = _rightPanel.X;
        int rightW      = _rightPanel.Width;
        int gridTop     = _rightPanel.Y + 10;
        int gridBottom  = _rightPanel.Bottom - 36;

        _scrollbarTrack = new Rectangle(
            _rightPanel.Right - scrollW - 4,
            gridTop,
            scrollW,
            gridBottom - gridTop
        );

        _gridArea = new Rectangle(
            rightX + 8,
            gridTop,
            _scrollbarTrack.X - rightX - 10,
            gridBottom - gridTop
        );

        int cellAndPad  = _gridArea.Width / GRID_COLS;
        int computedCell = cellAndPad - CELL_PADDING - 4;

        _cellRects = new Rectangle[_visibleRows, GRID_COLS];
        for (int row = 0; row < _visibleRows; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                _cellRects[row, col] = new Rectangle(
                    _gridArea.X + col * cellAndPad,
                    _gridArea.Y + row * (computedCell + CELL_PADDING),
                    computedCell, computedCell
                );
            }
        }

        UpdateGridScrollThumb();
    }


    private void UpdateGridScrollThumb()
    {
        if (_totalRows <= _visibleRows) { _scrollbarThumb = _scrollbarTrack; return; }
        float ratio  = (float)_visibleRows / _totalRows;
        int   thumbH = Math.Max(20, (int)(_scrollbarTrack.Height * ratio));
        float pct    = (float)_scrollRow / (_totalRows - _visibleRows);
        int   thumbY = _scrollbarTrack.Y + (int)((_scrollbarTrack.Height - thumbH) * pct);
        _scrollbarThumb = new Rectangle(_scrollbarTrack.X, thumbY, _scrollbarTrack.Width, thumbH);
    }

    private void UpdateRecipeScrollThumb()
    {
        int total = _pc.KnownRecipes.Count;
        if (total <= RECIPE_ROWS_VISIBLE) { _recipeScrollThumb = _recipeScrollTrack; return; }
        float ratio  = (float)RECIPE_ROWS_VISIBLE / total;
        int   thumbH = Math.Max(20, (int)(_recipeScrollTrack.Height * ratio));
        float pct    = (float)_recipeScrollRow / (total - RECIPE_ROWS_VISIBLE);
        int   thumbY = _recipeScrollTrack.Y + (int)((_recipeScrollTrack.Height - thumbH) * pct);
        _recipeScrollThumb = new Rectangle(_recipeScrollTrack.X, thumbY, _recipeScrollTrack.Width, thumbH);
    }


    private void RefreshGrid()
    {
        _filteredSlots.Clear();
        List<InventorySlot> inv = _pc.GetInventoryPublic();
        for (int i = 0; i < inv.Count; i++)
            _filteredSlots.Add(inv[i]);

        _totalRows = (int)Math.Ceiling((double)_filteredSlots.Count / GRID_COLS);
        if (_scrollRow > Math.Max(0, _totalRows - _visibleRows))
            _scrollRow = Math.Max(0, _totalRows - _visibleRows);
        UpdateGridScrollThumb();
    }

    // preview recipe based on current mix slots

    private void RefreshPreview()
    {
        if (_mixSlot1Item == null || _mixSlot2Item == null)
        {
            _previewRecipe = null;
            return;
        }
        _previewRecipe = _pc.FindRecipe(_mixSlot1Item.Item, _mixSlot2Item.Item);
    }


    public void Update(float dt)
    {
        MouseState    mouse = Mouse.GetState();
        KeyboardState kb    = Keyboard.GetState();
        Vector2       mpos  = new Vector2(mouse.X, mouse.Y);

        _tooltip.Update(dt);
        _message.Update(dt);

        HandleGridScroll(mouse);
        HandleRecipeScroll(mouse);
        HandleDragDrop(mouse, mpos);
        HandleMixButton(mouse);
        HandleRecipeClick(mouse);
        HandleHover(mouse);

        _prevMouse    = mouse;
        _prevKeyboard = kb;
    }

    private void HandleGridScroll(MouseState mouse)
    {
        if (_totalRows <= _visibleRows) return;

        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
        {
            if (_scrollbarThumb.Contains(mouse.X, mouse.Y))
            {
                _isDraggingScroll  = true;
                _scrollDragOffsetY = mouse.Y - _scrollbarThumb.Y;
            }
        }
        if (mouse.LeftButton == ButtonState.Released) _isDraggingScroll = false;

        if (_isDraggingScroll)
        {
            float range = _scrollbarTrack.Height - _scrollbarThumb.Height;
            if (range > 0)
            {
                float pct  = MathHelper.Clamp((mouse.Y - _scrollDragOffsetY - _scrollbarTrack.Y) / range, 0f, 1f);
                _scrollRow = (int)Math.Round(pct * (_totalRows - _visibleRows));
            }
            UpdateGridScrollThumb();
        }

        int wheel = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
        if (wheel != 0 && _gridArea.Contains(mouse.X, mouse.Y))
        {
            _scrollRow = MathHelper.Clamp(_scrollRow + (wheel > 0 ? -1 : 1), 0, _totalRows - _visibleRows);
            UpdateGridScrollThumb();
        }
    }

    private void HandleRecipeScroll(MouseState mouse)
    {
        int total = _pc.KnownRecipes.Count;
        if (total <= RECIPE_ROWS_VISIBLE) return;

        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
        {
            if (_recipeScrollThumb.Contains(mouse.X, mouse.Y))
            {
                _isDraggingRecipeScroll  = true;
                _recipeScrollDragOffsetY = mouse.Y - _recipeScrollThumb.Y;
            }
        }
        if (mouse.LeftButton == ButtonState.Released) _isDraggingRecipeScroll = false;

        if (_isDraggingRecipeScroll)
        {
            float range = _recipeScrollTrack.Height - _recipeScrollThumb.Height;
            if (range > 0)
            {
                float pct       = MathHelper.Clamp((mouse.Y - _recipeScrollDragOffsetY - _recipeScrollTrack.Y) / range, 0f, 1f);
                _recipeScrollRow = (int)Math.Round(pct * (total - RECIPE_ROWS_VISIBLE));
            }
            UpdateRecipeScrollThumb();
        }
    }

// drag n drop stuff

    private void HandleDragDrop(MouseState mouse, Vector2 mpos)
    {
        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
        {
            if (!_isDragging) TryBeginDrag(mouse, mpos);
        }

        if (_isDragging && mouse.LeftButton == ButtonState.Pressed)
            _dragPosition = mpos - _dragOffset;

        if (_isDragging && mouse.LeftButton == ButtonState.Released)
        {
            TryDrop(mouse);
            _isDragging        = false;
            _draggedSlot       = null;
            _dragSourceGrid    = -1;
            _dragSourceMixSlot = -1;
        }
    }

    private void TryBeginDrag(MouseState mouse, Vector2 mpos)
    {
        for (int row = 0; row < _visibleRows; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                if (_cellRects[row, col].Contains(mouse.X, mouse.Y))
                {
                    int idx = (_scrollRow + row) * GRID_COLS + col;
                    if (idx < _filteredSlots.Count)
                    {
                        _isDragging     = true;
                        _dragSourceGrid = idx;
                        _draggedSlot    = _filteredSlots[idx];
                        _dragOffset     = mpos - new Vector2(_cellRects[row, col].X, _cellRects[row, col].Y);
                        _dragPosition   = mpos - _dragOffset;
                    }
                    return;
                }
            }
        }

        if (_mixSlot1.Contains(mouse.X, mouse.Y) && _mixSlot1Item != null)
        {
            _isDragging        = true;
            _dragSourceMixSlot = 0;
            _draggedSlot       = _mixSlot1Item;
            _dragOffset        = mpos - new Vector2(_mixSlot1.X, _mixSlot1.Y);
            _dragPosition      = mpos - _dragOffset;
            return;
        }

        if (_mixSlot2.Contains(mouse.X, mouse.Y) && _mixSlot2Item != null)
        {
            _isDragging        = true;
            _dragSourceMixSlot = 1;
            _draggedSlot       = _mixSlot2Item;
            _dragOffset        = mpos - new Vector2(_mixSlot2.X, _mixSlot2.Y);
            _dragPosition      = mpos - _dragOffset;
            return;
        }
    }

    private void TryDrop(MouseState mouse)
    {
        if (_mixSlot1.Contains(mouse.X, mouse.Y))
        {
            PlaceInMixSlot(0);
            return;
        }

        if (_mixSlot2.Contains(mouse.X, mouse.Y))
        {
            PlaceInMixSlot(1);
            return;
        }

        if (_dragSourceMixSlot != -1)
        {
            ReturnMixSlotToInventory(_dragSourceMixSlot);
            return;
        }

    }

    private void PlaceInMixSlot(int slot)
    {
        if (_dragSourceMixSlot != -1)
        {
            if (_dragSourceMixSlot == slot) 
                return;   // same slot, no-op
            InventorySlot tmp = _mixSlot1Item;
            _mixSlot1Item = _mixSlot2Item;
            _mixSlot2Item = tmp;
            RefreshPreview();
            return;
        }

        // coming from grid take item out of inventory
        if (_dragSourceGrid == -1 || _draggedSlot == null) return;

        if (slot == 0 && _mixSlot1Item != null) ReturnMixSlotToInventory(0);
        if (slot == 1 && _mixSlot2Item != null) ReturnMixSlotToInventory(1);

        _pc.RemoveItem(_draggedSlot.Item.Id, 1);

        InventorySlot placed = new InventorySlot(_draggedSlot.Item, 1);
        if (slot == 0) _mixSlot1Item = placed;
        else           _mixSlot2Item = placed;

        RefreshGrid();
        RefreshPreview();
    }

    private void ReturnMixSlotToInventory(int slot)
    {
        InventorySlot returning = (slot == 0) ? _mixSlot1Item : _mixSlot2Item;
        if (returning == null) return;
        _pc.AddItem(returning.Item, 1);
        if (slot == 0) _mixSlot1Item = null;
        else           _mixSlot2Item = null;
        RefreshGrid();
        RefreshPreview();
    }

    private void HandleMixButton(MouseState mouse)
    {
        if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
        {
            if (!_mixButton.Contains(mouse.X, mouse.Y)) return;
            if (_mixSlot1Item == null || _mixSlot2Item == null) return;

            Item item1 = _mixSlot1Item.Item;
            Item item2 = _mixSlot2Item.Item;

            // clear slots the ingredients are consumed regardless
            _mixSlot1Item = null;
            _mixSlot2Item = null;

            Recipe recipe = _pc.FindRecipe(item1, item2);

            if (recipe != null)
            {
                bool isNew = _pc.LearnRecipe(recipe);
                _pc.AddItem(recipe.Result, 1);
                string msg = isNew ? "New recipe discovered: " + recipe.Result.Name + "!"
                                   : recipe.Result.Name + " crafted!";
                ShowMessage(msg, _colSuccess);
                UpdateRecipeScrollThumb();
            }
            else
            {
                ShowMessage("The mixture produced nothing. Ingredients lost.", _colFail);
            }

            RefreshGrid();
            RefreshPreview();
        }
    }
    private void HandleRecipeClick(MouseState mouse)
    {
        if (mouse.LeftButton != ButtonState.Pressed || _prevMouse.LeftButton != ButtonState.Released) return;

        for (int i = 0; i < _recipeRects.Count; i++)
        {
            int recipeIdx = _recipeScrollRow + i;
            if (recipeIdx >= _pc.KnownRecipes.Count) break;

            if (_recipeRects[i].Contains(mouse.X, mouse.Y))
            {
                Recipe r = _pc.KnownRecipes[recipeIdx];

                bool has1 = _pc.HasItem(r.Ingredient1.Id);
                bool has2 = _pc.HasItem(r.Ingredient2.Id);

                if (has1 && has2)
                {
                    _pc.RemoveItem(r.Ingredient1.Id, 1);
                    _pc.RemoveItem(r.Ingredient2.Id, 1);
                    _pc.AddItem(r.Result, 1);
                    ShowMessage(r.Result.Name + " crafted!", _colSuccess);
                    RefreshGrid();
                }
                // if missing ingredients do nothing figure it out on your own
                return;
            }
        }
    }

    // hover tooltip

    private void HandleHover(MouseState mouse)
    {
        Vector2 mpos = new Vector2(mouse.X, mouse.Y);

        int newHoveredGrid = -1;
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

        if (newHoveredGrid != _hoveredGridSlot)
        {
            _hoveredGridSlot = newHoveredGrid;
            if (newHoveredGrid != -1 && !_isDragging)
            {
                List<string> lines = Tooltip.BuildItemTooltip(_filteredSlots[newHoveredGrid].Item);
                _tooltip.ShowHover(lines, mpos);
            }
            else
            {
                _tooltip.Hide();
            }
        }
        else if (newHoveredGrid != -1 && !_isDragging)
        {
            // update anchor as mouse moves
            List<string> lines = Tooltip.BuildItemTooltip(_filteredSlots[newHoveredGrid].Item);
            _tooltip.ShowHover(lines, mpos);
        }

        int newHoveredRecipe = -1;
        for (int i = 0; i < _recipeRects.Count; i++)
        {
            int idx = _recipeScrollRow + i;
            if (idx >= _pc.KnownRecipes.Count) break;
            if (_recipeRects[i].Contains(mouse.X, mouse.Y))
            {
                newHoveredRecipe = idx;
                break;
            }
        }

        if (newHoveredRecipe != _hoveredRecipe)
        {
            _hoveredRecipe = newHoveredRecipe;
            if (newHoveredRecipe != -1)
            {
                List<string> lines = Tooltip.BuildRecipeTooltip(_pc.KnownRecipes[newHoveredRecipe]);
                _tooltip.ShowHover(lines, mpos);
            }
            else if (_hoveredGridSlot == -1)
            {
                _tooltip.Hide();
            }
        }

        if (_hoveredGridSlot == -1 && _hoveredRecipe == -1)
            _tooltip.Hide();
    }

    private void ShowMessage(string text, Color color)
    {
        // anchor at bottom centre of screen
        Vector2 anchor = new Vector2(_screenW / 2f - 150, _screenH - 100f);
        _message.ShowTimed(text, 3.0f, anchor);
    }

    public void Draw(SpriteBatch sb)
    {
        DrawRect(sb, new Rectangle(0, 0, _screenW, _screenH), new Color(20, 20, 20) * 0.85f);

        sb.Draw(_texLeftPanel,  _leftPanel,  Color.White);
        sb.Draw(_texRightPanel, _rightPanel, Color.White);

        DrawMixingArea(sb);
        DrawRecipeList(sb);
        DrawGrid(sb);

        if (_isDragging && _draggedSlot != null)
        {
            int cellW = _cellRects[0, 0].Width;
            Rectangle ghost = new Rectangle((int)_dragPosition.X, (int)_dragPosition.Y, cellW, cellW);
            sb.Draw(_texGridCell, ghost, _colHighlight * 0.75f);
            sb.DrawString(_fontPixel, _draggedSlot.Item.Name,
                          new Vector2(_dragPosition.X, _dragPosition.Y + cellW + 2), _colText);
        }

        _tooltip.Draw(sb);
        _message.Draw(sb);
    }

    private void DrawMixingArea(SpriteBatch sb)
    {
        Color s1Tint = (_isDragging && _dragSourceMixSlot == 0) ? _colHighlight * 0.4f : Color.White;
        sb.Draw(_texMixSlot, _mixSlot1, s1Tint);
        if (_mixSlot1Item != null && !(_isDragging && _dragSourceMixSlot == 0))
        {
            if (_mixSlot1Item.Item.Icon != null)
            {
                int inset = 6;
                sb.Draw(_mixSlot1Item.Item.Icon, new Rectangle(
                    _mixSlot1.X + inset, _mixSlot1.Y + inset,
                    _mixSlot1.Width - inset * 2, _mixSlot1.Height - inset * 2), Color.White);
            }
            else
                DrawStringCentred(sb, _fontPixel, TruncateToFit(_mixSlot1Item.Item.Name, _fontPixel, _mixSlot1.Width - 6), _mixSlot1, _colTextDim);
        }
        else
            DrawStringCentred(sb, _fontPixel, "?", _mixSlot1, _colTextDim);

        DrawStringCentred(sb, _fontPixel, "+", _arrowRect1, _colTextDim);
        Color s2Tint = (_isDragging && _dragSourceMixSlot == 1) ? _colHighlight * 0.4f : Color.White;
        sb.Draw(_texMixSlot, _mixSlot2, s2Tint);
        if (_mixSlot2Item != null && !(_isDragging && _dragSourceMixSlot == 1))
        {
            if (_mixSlot2Item.Item.Icon != null)
            {
                int inset = 6;
                sb.Draw(_mixSlot2Item.Item.Icon, new Rectangle(
                    _mixSlot2.X + inset, _mixSlot2.Y + inset,
                    _mixSlot2.Width - inset * 2, _mixSlot2.Height - inset * 2), Color.White);
            }
            else
                DrawStringCentred(sb, _fontPixel, TruncateToFit(_mixSlot2Item.Item.Name, _fontPixel, _mixSlot2.Width - 6), _mixSlot2, _colTextDim);
        }
        else
            DrawStringCentred(sb, _fontPixel, "?", _mixSlot2, _colTextDim);

        DrawStringCentred(sb, _fontPixel, "->", _arrowRect2, _colTextDim);


        sb.Draw(_texResultSlot, _resultSlot, Color.White);
        if (_mixSlot1Item != null && _mixSlot2Item != null && _previewRecipe != null)
            DrawStringCentred(sb, _fontPixel, TruncateToFit(_previewRecipe.Result.Name, _fontPixel, _resultSlot.Width - 6), _resultSlot, _colTextDim);
        else
            DrawStringCentred(sb, _fontPixel, "?", _resultSlot, _colTextDim);

        DrawRect(sb, _mixButton, _colSlot);
        DrawRectBorder(sb, _mixButton, _colSlotBorder, 1);
        DrawStringCentred(sb, _fontPixel, "Mix", _mixButton, _colTextDim);
    }


    private void DrawRecipeList(SpriteBatch sb)
    {
        sb.DrawString(_fontPixel, "Known Recipes",
                      new Vector2(_leftPanel.X + 40, _recipeListArea.Y - 26), _colTextDim);

        // everything inside the list area is hard-clipped to prevent bleeding
        sb.End();

        RasterizerState scissorState = new RasterizerState();
        scissorState.ScissorTestEnable = true;

        GraphicsDevice gd = sb.GraphicsDevice;
        Rectangle prevScissor = gd.ScissorRectangle;

        // expand scissor slightly to include the scrollbar column
        Rectangle clipRect = new Rectangle(
            _recipeListArea.X,
            _recipeListArea.Y,
            _recipeScrollTrack.Right - _recipeListArea.X,
            _recipeListArea.Height
        );
        gd.ScissorRectangle = clipRect;

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                 SamplerState.PointClamp, null, scissorState);

        sb.Draw(_texScrollTrack, _recipeScrollTrack, Color.White);
        if (_pc.KnownRecipes.Count > RECIPE_ROWS_VISIBLE)
            sb.Draw(_texScrollThumb, _recipeScrollThumb, Color.White);

        for (int i = 0; i < _recipeRects.Count; i++)
        {
            int recipeIdx = _recipeScrollRow + i;
            if (recipeIdx >= _pc.KnownRecipes.Count) break;

            Recipe r       = _pc.KnownRecipes[recipeIdx];
            bool canCraft  = _pc.HasItem(r.Ingredient1.Id) && _pc.HasItem(r.Ingredient2.Id);
            bool isHovered = (recipeIdx == _hoveredRecipe);

            Texture2D slotTex = canCraft ? _texRecipeSlot : _texRecipeSlotDimmed;
            sb.Draw(slotTex, _recipeRects[i], isHovered ? _colHighlight * 0.3f : Color.White);

            string  line     = r.Ingredient1.Name + "  +  " + r.Ingredient2.Name + "  ->  " + r.Result.Name;
            string  display  = TruncateToFit(line, _fontPixel, _recipeRects[i].Width - 12);
            Vector2 lineSize = _fontPixel.MeasureString(display);
            float   textY    = _recipeRects[i].Y + (_recipeRects[i].Height - lineSize.Y) / 2f;

            sb.DrawString(_fontPixel, display,
                          new Vector2(_recipeRects[i].X + 8, textY),
                          canCraft ? _colTextDim : new Color(90, 80, 70));
        }

        sb.End();
        gd.ScissorRectangle = prevScissor;
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                 SamplerState.PointClamp, null, null);
    }

    private void DrawGrid(SpriteBatch sb)
    {
        sb.Draw(_texScrollTrack, _scrollbarTrack, Color.White);
        if (_totalRows > _visibleRows)
            sb.Draw(_texScrollThumb, _scrollbarThumb, Color.White);

        for (int row = 0; row < _visibleRows; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                int idx    = (_scrollRow + row) * GRID_COLS + col;
                bool isSrc = _isDragging && idx == _dragSourceGrid;
                sb.Draw(_texGridCell, _cellRects[row, col], isSrc ? _colHighlight * 0.35f : Color.White);

                if (idx < _filteredSlots.Count)
                {
                    InventorySlot slot = _filteredSlots[idx];
                    DrawItemInCell(sb, slot.Item, slot.Quantity, _cellRects[row, col]);
                }
            }
        }

        sb.DrawString(_fontPixel, _filteredSlots.Count + " / " + _pc.InventoryLimit + " items",
                      new Vector2(_gridArea.X, _gridArea.Bottom + 6), _colTextDim);
    }

    private void DrawRect(SpriteBatch sb, Rectangle r, Color c)
    {
        sb.Draw(_pixel, r, c);
    }

    private void DrawRectBorder(SpriteBatch sb, Rectangle r, Color c, int t)
    {
        sb.Draw(_pixel, new Rectangle(r.X, r.Y,          r.Width, t),        c);
        sb.Draw(_pixel, new Rectangle(r.X, r.Bottom - t, r.Width, t),        c);
        sb.Draw(_pixel, new Rectangle(r.X, r.Y,          t,       r.Height), c);
        sb.Draw(_pixel, new Rectangle(r.Right - t, r.Y,  t,       r.Height), c);
    }

    private void DrawStringCentred(SpriteBatch sb, SpriteFont font, string text, Rectangle bounds, Color color)
    {
        Vector2 size = font.MeasureString(text);
        sb.DrawString(font, text,
                      new Vector2(bounds.X + (bounds.Width - size.X) / 2f,
                                  bounds.Y + (bounds.Height - size.Y) / 2f),
                      color);
    }

    private void DrawItemInCell(SpriteBatch sb, Item item, int quantity, Rectangle cell)
    {
        if (item.Icon != null)
        {
            int inset = 4;
            sb.Draw(item.Icon, new Rectangle(
                cell.X + inset, cell.Y + inset,
                cell.Width - inset * 2, cell.Height - inset * 2),
                Color.White);
        }
        else
        {
            string label = TruncateToFit(item.Name, _fontPixel, cell.Width - 4);
            sb.DrawString(_fontPixel, label,
                          new Vector2(cell.X + 3, cell.Y + 4), _colTextDim);
        }

        if (item.Stackable && quantity > 1)
        {
            string  qty  = "x" + quantity;
            Vector2 qs   = _fontPixel.MeasureString(qty);
            sb.DrawString(_fontPixel, qty,
                          new Vector2(cell.Right - qs.X - 3, cell.Bottom - qs.Y - 3),
                          _colTextDim);
        }
    }

    private string TruncateToFit(string text, SpriteFont font, int maxWidth)
    {
        if (font.MeasureString(text).X <= maxWidth) return text;
        string t = text;
        while (t.Length > 0 && font.MeasureString(t + "..").X > maxWidth)
            t = t.Substring(0, t.Length - 1);
        return t + "..";
    }
}
