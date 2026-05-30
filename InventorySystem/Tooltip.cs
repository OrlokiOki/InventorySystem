using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Tooltip
{
    
    private SpriteFont _fontPixel;
    private Texture2D  _pixel;

    private bool    _visible      = false;
    private float   _fadeTimer    = 0f;
    private float   _fadeDuration = 0f;
    private float   _alpha        = 1f;

    private List<string> _lines = new List<string>();

    private Rectangle _bounds;
    private int _screenW;
    private int _screenH;

    private const int PADDING   = 10;
    private const int LINE_GAP  = 4;

    private Color _colBg     = new Color(25, 20, 18);
    private Color _colBorder = new Color(110, 85, 60);
    private Color _colTitle  = new Color(220, 200, 160);
    private Color _colBody   = new Color(180, 170, 155);

    public Tooltip(SpriteFont fontPixel, Texture2D pixel, int screenW, int screenH)
    {
        
        _fontPixel = fontPixel;
        _pixel     = pixel;
        _screenW   = screenW;
        _screenH   = screenH;
    }

    public void ShowHover(List<string> lines, Vector2 anchor)
    {
        _lines        = lines;
        _fadeDuration = 0f;
        _fadeTimer    = 0f;
        _alpha        = 1f;
        _visible      = true;
        CalculateBounds(anchor);
    }

    public void ShowTimed(string message, float duration, Vector2 anchor)
    {
        _lines        = new List<string> { message };
        _fadeDuration = duration;
        _fadeTimer    = duration;
        _alpha        = 1f;
        _visible      = true;
        CalculateBounds(anchor);
    }

    public void Hide()
    {
        _visible = false;
    }

    public bool IsVisible()
    {
        return _visible;
    }

    public void Update(float dt)
    {
        if (!_visible) return;
        if (_fadeDuration <= 0f) return;

        _fadeTimer -= dt;
        if (_fadeTimer <= 0f)
        {
            _visible = false;
            return;
        }

        if (_fadeTimer < 0.6f)
        {
            _alpha = _fadeTimer / 0.6f;
        }
    }

    public void Draw(SpriteBatch sb)
    {
        if (!_visible) return;

        Color bg     = _colBg     * _alpha;
        Color border = _colBorder * _alpha;

        sb.Draw(_pixel, _bounds, bg);

        int t = 1;
        sb.Draw(_pixel, new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, t), border);
        sb.Draw(_pixel, new Rectangle(_bounds.X, _bounds.Bottom - t, _bounds.Width, t), border);
        sb.Draw(_pixel, new Rectangle(_bounds.X, _bounds.Y, t, _bounds.Height), border);
        sb.Draw(_pixel, new Rectangle(_bounds.Right - t, _bounds.Y, t, _bounds.Height), border);

        int y = _bounds.Y + PADDING;
        for (int i = 0; i < _lines.Count; i++)
        {
            if (i == 0)
            {
                sb.DrawString(_fontPixel, _lines[i],
                              new Vector2(_bounds.X + PADDING, y),
                              _colTitle * _alpha);
                y += (int)_fontPixel.MeasureString(_lines[i]).Y + LINE_GAP;
            }
            else
            {
                sb.DrawString(_fontPixel, _lines[i],
                              new Vector2(_bounds.X + PADDING, y),
                              _colBody * _alpha);
                y += (int)_fontPixel.MeasureString(_lines[i]).Y + LINE_GAP;
            }
        }
    }

    private void CalculateBounds(Vector2 anchor)
    {
        int maxW = 0;
        int totalH = PADDING * 2;

        for (int i = 0; i < _lines.Count; i++)
        {
            int lineW;
            int lineH;

            if (i == 0)
            {
                Vector2 s = _fontPixel.MeasureString(_lines[i]);
                lineW = (int)s.X;
                lineH = (int)s.Y;
            }
            else
            {
                Vector2 s = _fontPixel.MeasureString(_lines[i]);
                lineW = (int)s.X;
                lineH = (int)s.Y;
            }

            if (lineW > maxW) maxW = lineW;
            totalH += lineH + LINE_GAP;
        }

        int w = maxW + PADDING * 2;
        int h = totalH;

        int x = (int)anchor.X + 14;
        int y = (int)anchor.Y + 14;

        if (x + w > _screenW - 10) x = (int)anchor.X - w - 6;
        if (y + h > _screenH - 10) y = (int)anchor.Y - h - 6;

        _bounds = new Rectangle(x, y, w, h);
    }

    public static List<string> BuildItemTooltip(Item item)
    {
        List<string> lines = new List<string>();
        lines.Add(item.Name);
        lines.Add(item.Description);
        lines.Add("Value: " + item.Value);

        if (item is Weapon)
        {
            Weapon w = (Weapon)item;
            lines.Add("Damage: " + w.Damage);
            if (w.Enchantment != null)
                lines.Add("Enchant: " + w.Enchantment.Type);
        }
        else if (item is Armor)
        {
            Armor a = (Armor)item;
            lines.Add("Defense: " + a.Defense);
            if (a.Enchantment != null)
                lines.Add("Enchant: " + a.Enchantment.Type);
        }
        else if (item is Consumable)
        {
            Consumable c = (Consumable)item;
            lines.Add("Effect: " + c.Effect.Type + " +" + c.Effect.Magnitude);
            if (c.Effect.Duration > 0f)
                lines.Add("Duration: " + c.Effect.Duration + "s");
        }
        else if (item is Accessory)
        {
            Accessory acc = (Accessory)item;
            if (acc.Effect != null)
                lines.Add("Effect: " + acc.Effect.Type + " +" + acc.Effect.Magnitude);
        }
        else if (item is EnchantingStone)
        {
            EnchantingStone es = (EnchantingStone)item;
            lines.Add("Enchants: " + es.Effect.Target);
            lines.Add("Effect: " + es.Effect.Type + " +" + es.Effect.Magnitude);
        }

        return lines;
    }

    public static List<string> BuildRecipeTooltip(Recipe recipe)
    {
        List<string> lines = new List<string>();
        lines.Add(recipe.Result.Name);
        lines.Add(recipe.Ingredient1.Name + " + " + recipe.Ingredient2.Name);
        lines.Add(recipe.Result.Description);
        lines.Add("Effect: " + recipe.Result.Effect.Type + " +" + recipe.Result.Effect.Magnitude);
        if (recipe.Result.Effect.Duration > 0f)
            lines.Add("Duration: " + recipe.Result.Effect.Duration + "s");
        return lines;
    }
}
