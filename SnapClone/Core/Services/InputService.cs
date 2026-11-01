using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SnapClone.Core.Services
{
    public class InputService
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        
        public bool IsDragging { get; set; }
        public object DraggedCard { get; set; }
        public Vector2 DragStartPosition { get; set; }
        public Vector2 DragOffset { get; set; }
        
        public void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _previousMouseState = _currentMouseState;
            _currentKeyboardState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();
        }
        
        public bool IsKeyDown(Keys key) => _currentKeyboardState.IsKeyDown(key);
        public bool IsKeyPressed(Keys key) => _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        public bool IsKeyReleased(Keys key) => !_currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyDown(key);
        
        public MouseState CurrentMouseState => _currentMouseState;
        public MouseState PreviousMouseState => _previousMouseState;
        public Vector2 MousePosition => new Vector2(_currentMouseState.X, _currentMouseState.Y);
        public bool IsLeftMousePressed() => _currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        public bool IsLeftMouseDown() => _currentMouseState.LeftButton == ButtonState.Pressed;
        public bool IsLeftMouseReleased() => _currentMouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed;
    }
}
