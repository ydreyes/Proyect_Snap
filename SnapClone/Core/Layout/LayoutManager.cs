using Microsoft.Xna.Framework;

namespace SnapClone.Core.Layout
{
    /// <summary>
    /// Calcula todas las regiones de la escena de combate a partir del
    /// tamano real del viewport, usando proporciones. Asi el layout es
    /// adaptativo a cualquier resolucion portrait.
    /// </summary>
    public class LayoutManager
    {
        public const int MaxCardsPerSide = 4;
        public const int SlotsPerRow = 2;
        public const int MaxTurns = 6;

        private readonly GraphicsDeviceManager _graphics;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }
        public float Scale { get; private set; }

        public LayoutManager(GraphicsDeviceManager graphics)
        {
            _graphics = graphics;
            CalculateLayout();
        }

        public void CalculateLayout()
        {
            ScreenWidth = _graphics.GraphicsDevice.Viewport.Width;
            ScreenHeight = _graphics.GraphicsDevice.Viewport.Height;

            float byHeight = ScreenHeight / 1280f;
            float byWidth = ScreenWidth / 720f;
            Scale = byHeight < byWidth ? byHeight : byWidth;
        }

        private Rectangle Percent(float x, float y, float w, float h) => new Rectangle(
            (int)(ScreenWidth * x),
            (int)(ScreenHeight * y),
            (int)(ScreenWidth * w),
            (int)(ScreenHeight * h));

        // ------------------------------------------------------------------
        // Cabecera: avatares + banner central
        // ------------------------------------------------------------------

        public Rectangle TopBar => Percent(0f, 0f, 1f, 0.118f);
        public Rectangle LeftAvatar => Percent(0.010f, 0.004f, 0.205f, 0.110f);
        public Rectangle RightAvatar => Percent(0.785f, 0.004f, 0.205f, 0.110f);
        public Rectangle StatusBanner => Percent(0.245f, 0.014f, 0.510f, 0.078f);

        // ------------------------------------------------------------------
        // Columnas de locaciones
        // ------------------------------------------------------------------

        private float ColumnGap => 0.012f;
        private float SidePad => 0.016f;

        public Rectangle LocationsArea => Percent(0f, 0.118f, 1f, 0.600f);

        public Rectangle GetLocationColumn(int index)
        {
            float gap = ColumnGap;
            float pad = SidePad;
            float width = (1f - pad * 2f - gap * 2f) / 3f;
            float x = pad + index * (width + gap);
            return Percent(x, 0.118f, width, 0.600f);
        }

        /// <summary>
        /// Placa compacta entre las dos rejillas 2x2 (0.345 - 0.470).
        /// </summary>
        public Rectangle GetLocationPanel(int index)
        {
            var column = GetLocationColumn(index);
            int padX = (int)(column.Width * 0.02f);
            int top = (int)(ScreenHeight * 0.345f);
            int height = (int)(ScreenHeight * 0.125f);
            return new Rectangle(column.X + padX, top, column.Width - padX * 2, height);
        }

        // ------------------------------------------------------------------
        // Slots de cartas (2 x 2 por lado)
        // ------------------------------------------------------------------

        public Rectangle GetOpponentCardSlot(int zoneIndex, int slotIndex) =>
            GetCardSlot(zoneIndex, slotIndex, true);

        public Rectangle GetPlayerCardSlot(int zoneIndex, int slotIndex) =>
            GetCardSlot(zoneIndex, slotIndex, false);

        private Rectangle GetCardSlot(int zoneIndex, int slotIndex, bool opponent)
        {
            var column = GetLocationColumn(zoneIndex);
            int pad = (int)(column.Width * 0.03f);
            int gap = (int)(column.Width * 0.02f);
            int slotWidth = (column.Width - pad * 2 - gap * (SlotsPerRow - 1)) / SlotsPerRow;
            int slotHeight = (int)(slotWidth * 1.35f);
            int rowGap = (int)(ScreenHeight * 0.006f);

            int row = slotIndex / SlotsPerRow;
            int col = slotIndex % SlotsPerRow;

            int x = column.X + pad + col * (slotWidth + gap);
            int baseY = opponent
                ? (int)(ScreenHeight * 0.120f)
                : (int)(ScreenHeight * 0.472f);
            int y = baseY + row * (slotHeight + rowGap);

            return new Rectangle(x, y, slotWidth, slotHeight);
        }

        /// <summary>
        /// Region de drop de una locacion para el jugador: placa + rejilla
        /// 2x2. El snap magnetico busca el slot libre mas cercano dentro.
        /// </summary>
        public Rectangle GetPlayerDropRegion(int zoneIndex)
        {
            var column = GetLocationColumn(zoneIndex);
            var panel = GetLocationPanel(zoneIndex);
            var lastSlot = GetPlayerCardSlot(zoneIndex, MaxCardsPerSide - 1);
            return new Rectangle(column.X, panel.Y, column.Width, lastSlot.Bottom - panel.Y);
        }

        // ------------------------------------------------------------------
        // Mano, boton de turno y energia
        // ------------------------------------------------------------------

        /// <summary>
        /// Franja de objetos entre los controles de turno y la mano. No se
        /// solapa con el tablero (termina en 0.718), el boton de turno
        /// (termina en 0.758) ni la mano (empieza en 0.820).
        /// </summary>
        public Rectangle ItemBar => Percent(0.015f, 0.760f, 0.97f, 0.056f);

        public Rectangle HandArea => Percent(0f, 0.820f, 1f, 0.180f);

        public Rectangle GetHandCard(int index, int count)
        {
            if (count <= 0) count = 1;

            var area = HandArea;
            int cardHeight = (int)(area.Height * 0.78f);
            int cardWidth = (int)(cardHeight / 1.42f);
            int available = area.Width - 20;

            int gap = count > 1 ? (int)(cardWidth * 0.18f) : 0;
            int totalWidth = count * cardWidth + (count - 1) * gap;

            // Si la fila no cabe, se solapan las cartas conservando al menos
            // un 40% de ancho visible por carta para que sigan siendo legibles.
            if (count > 1 && totalWidth > available)
            {
                int pitch = (available - cardWidth) / (count - 1);
                int minPitch = (int)(cardWidth * 0.40f);
                gap = (pitch < minPitch ? minPitch : pitch) - cardWidth;
                totalWidth = count * cardWidth + (count - 1) * gap;
            }

            int startX = area.X + (area.Width - totalWidth) / 2;

            // Con muchas cartas y solape minimo la fila puede exceder el
            // ancho: se ancla a la izquierda para no perder la primera carta.
            if (startX < area.X + 10) startX = area.X + 10;

            int y = area.Y + (area.Height - cardHeight) / 2;

            return new Rectangle(startX + index * (cardWidth + gap), y, cardWidth, cardHeight);
        }

        public Rectangle EndTurnButton => Percent(0.575f, 0.700f, 0.395f, 0.058f);
        public Rectangle EnergyBadgeBounds => Percent(0.020f, 0.700f, 0.130f, 0.058f);

        // ------------------------------------------------------------------
        // Visor de pilas (popover anclado al avatar)
        // ------------------------------------------------------------------

        public Rectangle GetPileViewerBounds(bool player)
        {
            int width = (int)(ScreenWidth * 0.46f);
            int height = (int)(ScreenHeight * 0.30f);
            int y = (int)(ScreenHeight * 0.118f);
            int x = player
                ? (int)(ScreenWidth * 0.010f)
                : (int)(ScreenWidth * 0.990f) - width;

            return new Rectangle(x, y, width, height);
        }

        public Rectangle GetPileViewerHeader(bool player)
        {
            var bounds = GetPileViewerBounds(player);
            int pad = ViewerViewPadding(bounds);
            int height = (int)(bounds.Height * 0.12f);
            return new Rectangle(bounds.X + pad, bounds.Y + pad, bounds.Width - pad * 2, height);
        }

        public Rectangle GetPileViewerCardArea(bool player)
        {
            var bounds = GetPileViewerBounds(player);
            var header = GetPileViewerHeader(player);
            var tabs = GetPileViewerTabs(player);
            int pad = ViewerViewPadding(bounds);
            int top = header.Bottom + pad;
            int height = tabs.Y - pad - top;
            return new Rectangle(bounds.X + pad, top, bounds.Width - pad * 2, height);
        }

        public Rectangle GetPileViewerTabs(bool player)
        {
            var bounds = GetPileViewerBounds(player);
            int pad = ViewerViewPadding(bounds);
            int height = (int)(bounds.Height * 0.30f);
            return new Rectangle(bounds.X + pad, bounds.Bottom - pad - height, bounds.Width - pad * 2, height);
        }

        private static int ViewerViewPadding(Rectangle viewerBounds) =>
            System.Math.Max(4, viewerBounds.Width / 32);

        public Vector2 EnergyBadgeCenter => new Vector2(
            EnergyBadgeBounds.Center.X,
            EnergyBadgeBounds.Center.Y);

        // ------------------------------------------------------------------
        // Panel de detalle (carta o zona)
        // ------------------------------------------------------------------

        public Rectangle DetailPanel => Percent(0.04f, 0.06f, 0.92f, 0.88f);

        public Rectangle DetailPanelClose
        {
            get
            {
                var panel = DetailPanel;
                int size = System.Math.Max(30, (int)(panel.Width * 0.13f));
                int margin = System.Math.Max(6, (int)(panel.Height * 0.018f));
                return new Rectangle(
                    panel.Center.X - size / 2,
                    panel.Bottom - size - margin,
                    size,
                    size);
            }
        }
    }
}
