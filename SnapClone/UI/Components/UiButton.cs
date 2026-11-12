using Microsoft.Xna.Framework;
using SnapClone.Core.Services;

namespace SnapClone.UI.Components
{
    public enum ButtonVisualState
    {
        Idle,
        Hover,
        Pressed,
        Disabled
    }

    /// <summary>
    /// Boton inmediato con maquina de estados: idle, hover, pressed y disabled.
    /// Solo activa al soltar dentro de sus limites; arrastrar fuera cancela.
    /// </summary>
    public class UiButton
    {
        private const float HoverScale = 1.03f;
        private const float PressScale = 0.96f;
        private const float RestScale = 1f;
        private const float StateSpeed = 8f; // ~150 ms para llegar al objetivo

        private bool _armed;

        public Rectangle Bounds { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Visible { get; set; } = true;
        public string Label { get; set; } = string.Empty;
        public object Tag { get; set; }

        public ButtonVisualState State { get; private set; } = ButtonVisualState.Idle;
        public bool IsPointerInside { get; private set; }
        public bool IsArmed => _armed;

        /// <summary>Escala visual suavizada (0.96 pulsado, 1.03 hover).</summary>
        public float VisualScale { get; private set; } = RestScale;

        public UiButton()
        {
        }

        public UiButton(Rectangle bounds, string label = "")
        {
            Bounds = bounds;
            Label = label ?? string.Empty;
        }

        /// <summary>
        /// Actualiza el estado y devuelve true solo en el frame en que se suelta
        /// dentro del boton habilitado.
        /// </summary>
        public bool Update(InputService input, float deltaSeconds)
        {
            if (input == null || !Visible)
            {
                State = ButtonVisualState.Idle;
                _armed = false;
                IsPointerInside = false;
                UpdateVisual(deltaSeconds);
                return false;
            }

            Vector2 pointer = input.MousePosition;
            bool down = input.IsLeftMouseDown();
            IsPointerInside = IsInside(Bounds, pointer);

            if (ShouldArm(Enabled, input.IsLeftMousePressed(), IsPointerInside))
                _armed = true;

            // Arrastrar fuera cancela la activacion aunque se siga pulsando.
            if (ShouldCancel(_armed, down, IsPointerInside))
                _armed = false;

            bool clicked = ShouldActivate(_armed, input.IsLeftMouseReleased(),
                IsPointerInside);

            if (!Enabled)
                State = ButtonVisualState.Disabled;
            else if (_armed && down && IsPointerInside)
                State = ButtonVisualState.Pressed;
            else if (IsPointerInside)
                State = ButtonVisualState.Hover;
            else
                State = ButtonVisualState.Idle;

            if (input.IsLeftMouseReleased() || !down)
                _armed = false;

            UpdateVisual(deltaSeconds);
            return clicked;
        }

        public bool Update(InputService input) => Update(input, 0f);

        /// <summary>
        /// Estado puro para pruebas: sin entrada real ni animacion.
        /// </summary>
        public static ButtonVisualState ResolveState(bool enabled,
            bool pointerInside, bool armed, bool pointerDown)
        {
            if (!enabled) return ButtonVisualState.Disabled;
            if (armed && pointerDown && pointerInside)
                return ButtonVisualState.Pressed;
            if (pointerInside) return ButtonVisualState.Hover;
            return ButtonVisualState.Idle;
        }

        /// <summary>El boton se arma al pulsar dentro y estando habilitado.</summary>
        public static bool ShouldArm(bool enabled, bool pressed, bool pointerInside) =>
            enabled && pressed && pointerInside;

        /// <summary>Arrastrar fuera mientras se mantiene pulsado cancela.</summary>
        public static bool ShouldCancel(bool armed, bool pointerDown,
            bool pointerInside) => armed && pointerDown && !pointerInside;

        /// <summary>Solo activa al soltar dentro.</summary>
        public static bool ShouldActivate(bool armed, bool released,
            bool pointerInside) => armed && released && pointerInside;

        public void Reset()
        {
            _armed = false;
            State = ButtonVisualState.Idle;
            IsPointerInside = false;
            VisualScale = RestScale;
        }

        private void UpdateVisual(float deltaSeconds)
        {
            float target = State == ButtonVisualState.Pressed
                ? PressScale
                : State == ButtonVisualState.Hover ? HoverScale : RestScale;

            if (deltaSeconds <= 0f)
            {
                VisualScale = target;
                return;
            }

            float amount = MathHelper.Clamp(deltaSeconds * StateSpeed, 0f, 1f);
            VisualScale = MathHelper.Lerp(VisualScale, target, amount);
        }

        public static bool IsInside(Rectangle bounds, Vector2 point)
        {
            return point.X >= bounds.Left && point.X <= bounds.Right &&
                   point.Y >= bounds.Top && point.Y <= bounds.Bottom;
        }
    }
}
