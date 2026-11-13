using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Gameplay.Run;
using SnapClone.UI.Primitives;

namespace SnapClone.UI.Components
{
    public enum PauseAction
    {
        None,
        Resume,
        SaveAndExit,
        ExitToMenu,
        Abandon
    }

    /// <summary>
    /// Overlay de pausa compartido por todas las escenas de run. No es una
    /// escena: se dibuja encima y el juego detiene la escena mientras esta
    /// abierto.
    /// </summary>
    public class PauseOverlay
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly GameServices _services;

        private ShapeDrawer _shapeDrawer;
        private FontService _fontService;

        public bool IsOpen { get; private set; }
        public bool ConfirmAbandon { get; private set; }
        public string Error { get; private set; }

        public PauseOverlay(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            GameServices services)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _services = services;
        }

        public void LoadContent()
        {
            _shapeDrawer = new ShapeDrawer(_graphicsDevice, _spriteBatch);

            var content = _services.Has<Microsoft.Xna.Framework.Content.ContentManager>()
                ? _services.Get<Microsoft.Xna.Framework.Content.ContentManager>()
                : _services.Get<Game>().Content;

            _fontService = new FontService(content);
        }

        public void Open()
        {
            IsOpen = true;
            ConfirmAbandon = false;
            Error = null;
        }

        public void Close()
        {
            IsOpen = false;
            ConfirmAbandon = false;
            Error = null;
        }

        public void ShowError(string message)
        {
            Error = message;
        }

        public Rectangle PauseButtonBounds =>
            GetPauseButtonBounds(_graphicsDevice.Viewport.Width,
                _graphicsDevice.Viewport.Height);

        public static Rectangle GetPauseButtonBounds(int width, int height) =>
            new Rectangle(
                (int)(width * 0.45f),
                (int)(height * 0.012f),
                Math.Max(44, (int)(width * 0.10f)),
                Math.Max(44, (int)(height * 0.042f)));

        public bool HitPauseButton(Vector2 point) =>
            IsInside(PauseButtonBounds, point);

        public PauseAction Update(InputService input, bool isCombat)
        {
            if (!IsOpen || !input.IsLeftMousePressed())
                return PauseAction.None;

            var mouse = input.MousePosition;

            if (ConfirmAbandon)
            {
                if (IsInside(GetButton(0), mouse))
                    return PauseAction.Abandon;

                if (IsInside(GetButton(1), mouse))
                    ConfirmAbandon = false;

                return PauseAction.None;
            }

            if (IsInside(GetButton(0), mouse))
                return PauseAction.Resume;

            if (IsInside(GetButton(1), mouse))
                return isCombat ? PauseAction.ExitToMenu : PauseAction.SaveAndExit;

            if (IsInside(GetButton(2), mouse))
                ConfirmAbandon = true;

            return PauseAction.None;
        }

        public void Draw(SpriteBatch spriteBatch, bool isCombat)
        {
            var viewport = _graphicsDevice.Viewport;
            _shapeDrawer.DrawRectangle(
                new Rectangle(0, 0, viewport.Width, viewport.Height),
                Color.Black * 0.72f);

            var panel = GetPanel();
            _shapeDrawer.DrawBeveledPanel(panel, new Color(16, 24, 38),
                new Color(120, 170, 230), 3, 16);

            _fontService.DrawTextCentered(spriteBatch, "PAUSA",
                new Rectangle(panel.X, panel.Y + (int)(panel.Height * 0.04f),
                    panel.Width, (int)(panel.Height * 0.10f)),
                new Color(238, 224, 188), 1.0f);

            var runState = _services.Has<RunState>() ? _services.Get<RunState>() : null;
            if (runState != null)
            {
                _fontService.DrawTextCenteredFit(spriteBatch,
                    RunModifierSystem.Describe(runState),
                    new Rectangle(panel.X + (int)(panel.Width * 0.06f),
                        panel.Y + (int)(panel.Height * 0.145f),
                        (int)(panel.Width * 0.88f),
                        (int)(panel.Height * 0.06f)),
                    new Color(170, 190, 215), 0.56f, 0.34f);
            }

            if (!string.IsNullOrEmpty(Error))
            {
                _fontService.DrawTextCenteredFit(spriteBatch, Error,
                    new Rectangle(panel.X + (int)(panel.Width * 0.08f),
                        panel.Y + (int)(panel.Height * 0.21f),
                        (int)(panel.Width * 0.84f),
                        (int)(panel.Height * 0.06f)),
                    new Color(244, 170, 150), 0.58f, 0.36f);
            }

            if (ConfirmAbandon)
            {
                DrawButton(spriteBatch, GetButton(0), "CONFIRMAR ABANDONO",
                    new Color(120, 48, 44), new Color(240, 160, 140));
                DrawButton(spriteBatch, GetButton(1), "CANCELAR",
                    new Color(38, 52, 46), new Color(150, 170, 160));
                return;
            }

            DrawButton(spriteBatch, GetButton(0), "REANUDAR",
                new Color(30, 104, 74), new Color(126, 214, 170));

            if (isCombat)
            {
                DrawButton(spriteBatch, GetButton(1), "SALIR AL MENU",
                    new Color(30, 84, 160), new Color(140, 190, 250));
            }
            else
            {
                DrawButton(spriteBatch, GetButton(1), "GUARDAR Y SALIR",
                    new Color(30, 84, 160), new Color(140, 190, 250));
            }

            DrawButton(spriteBatch, GetButton(2), "ABANDONAR RUN",
                new Color(120, 48, 44), new Color(240, 160, 140));
        }

        public void DrawPauseButton(SpriteBatch spriteBatch)
        {
            var bounds = PauseButtonBounds;
            _shapeDrawer.DrawBeveledPanel(bounds, new Color(24, 34, 52),
                new Color(140, 170, 210), 2, 8);
            _fontService.DrawTextCentered(spriteBatch, "II", bounds,
                new Color(220, 230, 245), 0.95f);
        }

        private Rectangle GetPanel()
        {
            var viewport = _graphicsDevice.Viewport;
            return new Rectangle(
                (int)(viewport.Width * 0.10f),
                (int)(viewport.Height * 0.26f),
                (int)(viewport.Width * 0.80f),
                (int)(viewport.Height * 0.40f));
        }

        private Rectangle GetButton(int index)
        {
            var panel = GetPanel();
            int height = (int)(panel.Height * 0.15f);
            int gap = (int)(panel.Height * 0.07f);
            int x = panel.X + (int)(panel.Width * 0.10f);
            int width = panel.Width - (int)(panel.Width * 0.20f);
            int y = panel.Y + (int)(panel.Height * 0.28f) +
                    index * (height + gap);

            return new Rectangle(x, y, width, height);
        }

        private void DrawButton(SpriteBatch spriteBatch, Rectangle bounds,
            string label, Color fill, Color border)
        {
            _shapeDrawer.DrawBeveledPanel(bounds, fill, border, 3, 10);
            _fontService.DrawTextCenteredFit(spriteBatch, label, bounds,
                Color.White, 0.85f, 0.48f);
        }

        private static bool IsInside(Rectangle bounds, Vector2 point)
        {
            return point.X >= bounds.Left && point.X <= bounds.Right &&
                   point.Y >= bounds.Top && point.Y <= bounds.Bottom;
        }
    }
}
