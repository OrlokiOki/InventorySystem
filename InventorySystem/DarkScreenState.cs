using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class DarkScreenState
{
    private SpriteFont    _fontPixel;
    private string        _promptText;
    private Vector2       _textPosition;
    private int           _screenWidth;
    private int           _screenHeight;
    private float         _blinkTimer    = 0f;
    private float         _blinkInterval = 0.6f;
    private bool          _showPrompt    = true;
    private bool          _showText      = true;
    private KeyboardState _prevKeyboard;

    public DarkScreenState(SpriteFont fontPixel, int screenWidth, int screenHeight)
    {
        _fontPixel    = fontPixel;
        _screenWidth  = screenWidth;
        _screenHeight = screenHeight;
        _promptText   = "Press E to open inventory";

        Vector2 textSize  = _fontPixel.MeasureString(_promptText);
        _textPosition = new Vector2(
            (_screenWidth  - textSize.X) / 2f,
            _screenHeight - textSize.Y - 60f
        );
    }

    public void OnReturn()
    {
        _showText     = true;
        _showPrompt   = true;
        _blinkTimer   = 0f;
        // snapshot current keyboard so the E that closed inventory
        // does not immediately reopen it. (annoying)
        _prevKeyboard = Keyboard.GetState();
    }

    public void Update(float deltaTime, ref GameState currentState)
    {
        KeyboardState kb = Keyboard.GetState();

        if (_showText)
        {
            _blinkTimer += deltaTime;
            if (_blinkTimer >= _blinkInterval)
            {
                _showPrompt = !_showPrompt;
                _blinkTimer = 0f;
            }
        }
        if (kb.IsKeyDown(Keys.E) && _prevKeyboard.IsKeyUp(Keys.E))
        {
            _showText    = false;
            _showPrompt  = false;
            currentState = GameState.Inventory;
        }

        _prevKeyboard = kb;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_showText && _showPrompt)
        {
            spriteBatch.DrawString(_fontPixel, _promptText, _textPosition, Color.White);
        }
    }
}
