using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace SnapClone.Core.Services
{
    public class InputService
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;

        // Un solo puntero logico: en escritorio lo mueve el raton, en Android
        // el primer dedo activo. Las escenas solo ven estas primitivas.
        private Vector2 _pointerPosition;
        private bool _pointerDown;
        private bool _previousPointerDown;
        private int _activeTouchId = -1;

        /// <summary>
        /// Origen del area de juego dentro de la superficie. En Android coincide
        /// con los insets del area segura; en escritorio es cero.
        /// </summary>
        public Vector2 PointerOffset { get; set; }

        public bool IsDragging { get; set; }
        public object DraggedCard { get; set; }
        public Vector2 DragStartPosition { get; set; }
        public Vector2 DragOffset { get; set; }

        public void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _previousMouseState = _currentMouseState;
            _previousPointerDown = _pointerDown;

            if (OperatingSystem.IsAndroid())
            {
                UpdateTouchPointer();
                return;
            }

            _currentKeyboardState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();
            _pointerPosition = new Vector2(_currentMouseState.X, _currentMouseState.Y) - PointerOffset;
            _pointerDown = _currentMouseState.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Lee el panel tactil una sola vez por frame (el estado envejece al
        /// consultarlo) y sigue el primer dedo que empezo el contacto.
        /// </summary>
        private void UpdateTouchPointer()
        {
            var touches = TouchPanel.GetState();

            bool down = false;
            bool found = false;
            var position = _pointerPosition;

            if (_activeTouchId >= 0)
            {
                foreach (var touch in touches)
                {
                    if (touch.Id != _activeTouchId)
                        continue;

                    found = true;

                    if (touch.State == TouchLocationState.Released || touch.State == TouchLocationState.Invalid)
                    {
                        _activeTouchId = -1;
                    }
                    else
                    {
                        position = touch.Position - PointerOffset;
                        down = true;
                    }

                    break;
                }

                // El dedo desaparecio sin un evento de liberacion: se suelta igual.
                if (!found)
                    _activeTouchId = -1;
            }

            if (_activeTouchId < 0)
            {
                foreach (var touch in touches)
                {
                    if (touch.State != TouchLocationState.Pressed)
                        continue;

                    _activeTouchId = touch.Id;
                    position = touch.Position - PointerOffset;
                    down = true;
                    break;
                }
            }

            _pointerPosition = position;
            _pointerDown = down;
        }

        public bool IsKeyDown(Keys key) => _currentKeyboardState.IsKeyDown(key);
        public bool IsKeyPressed(Keys key) => _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        public bool IsKeyReleased(Keys key) => !_currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyDown(key);

        public MouseState CurrentMouseState => _currentMouseState;
        public MouseState PreviousMouseState => _previousMouseState;
        public Vector2 MousePosition => _pointerPosition;
        public bool IsLeftMousePressed() => _pointerDown && !_previousPointerDown;
        public bool IsLeftMouseDown() => _pointerDown;
        public bool IsLeftMouseReleased() => !_pointerDown && _previousPointerDown;
    }
}
