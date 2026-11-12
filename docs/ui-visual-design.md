# UI visual design

Este documento describe la capa visual de las siete pantallas de jugador:
Menu, Draft, RunSummary, Collection, LevelUp, GlobalStore y Shop. La logica de
juego, la economia, el formato de guardado y el combate no cambian.

## Typography

- Fuente runtime: `FontStashSharp.MonoGame` 1.6.2.
- Roles:
  - `Display` = Bebas Neue Regular (titulos y cifras grandes).
  - `Ui` = Rajdhani SemiBold (etiquetas, botones, nombres de carta).
  - `Body` = Inter Regular (descripciones y textos largos).
  - `Numeric` = Rajdhani Bold (coste, poder, precios, puntos).
- Recursos embebidos en `SnapClone/Content/Fonts`:
  `BebasNeue-Regular.ttf`, `Rajdhani-SemiBold.ttf`, `Inter-Regular.ttf`,
  `Rajdhani-Bold.ttf` y sus archivos OFL.
- Cada rol usa un `FontSystem` con atlas de 1024 x 1024, generado una sola vez.
  Los tamanos se cachean por rol y pixel size; no se crea un atlas por frame.
- Si la carga de cualquier TTF falla, `FontService` usa `DefaultFont` para todos
  los metodos (`Measure`, `DrawText`, `DrawRole*`). La galeria de UI permite
  forzar ese camino con `new FontService(content, forceFallback: true)` y
  muestra el estado `RUNTIME TTF`/`FALLBACK FORZADO`.
- Los metodos heredados conservan la base de 22 px para no mover el layout del
  combate ni de las escenas no redisenadas.
- El rol numerico usa cifras tabulares: cada digito ocupa una celda del ancho
  maximo de la fuente, de modo que coste, poder, precios y puntos no desplazan
  el texto al cambiar de valor.
- Spike Android: **GO**. El APK instala y las tres caras (display, UI y body)
  mas los numeros se dibujan y siguen siendo legibles a 1080 x 2400. No fue
  necesario el fallback de multiples `SpriteFont`.

## Tokens

Fuente unica: `SnapClone/UI/Theme/UiTheme.cs`.

| Grupo | Tokens |
| --- | --- |
| Fondo | `BackgroundTop`, `BackgroundBottom`, `BackgroundDeep` |
| Paneles | `Panel`, `PanelRaised`, `PanelSunken`, `Border`, `BorderBright` |
| Acentos | `Accent`, `AccentSoft`, `Gold`, `GoldSoft`, `Violet`, `Perk` |
| Estados | `Success`, `Danger`, `DangerSoft`, `TextPrimary`, `TextSecondary`, `TextMuted` |
| Semantica | `StateAffordable`, `StateUnaffordable`, `StateSelected`, `StateSold`, `StateDangerous` |

Type scale (unidades de diseno 720 x 1280):

| Rol | Tamano |
| --- | --- |
| `DisplayXl` | 64 |
| `Display` | 44 |
| `Heading` | 32 |
| `Numeric` | 34 |
| `NumericSmall` | 24 |
| `Label` | 22 |
| `LabelSmall` | 18 |
| `Body` | 17 |
| `Caption` | 14 |

Espaciado: `SpaceXs` 4, `SpaceSm` 8, `SpaceMd` 12, `SpaceLg` 18, `SpaceXl` 28,
`SpaceXxl` 40. Geometria: `CornerCut` 10, `BorderThickness` 2, `TouchTarget`
44, `ElevationShadow` 3.

Escalado: `UiScale = clamp(min(width / 720, height / 1280), 0.75, 2)`. Todos los
tamanos de diseno se convierten con `Px(units, uiScale)`.

## Safe area and scaffold

`ScreenScaffold.Create(viewport, insets, headerDesign, footerDesign)` descuenta
los insets y resuelve `Header`, `Content`, `Footer` y `ContentInner`. Las escenas
dibujan en coordenadas locales del viewport; `Game1.ApplySafeArea` reduce el
viewport y `InputService.PointerOffset` mantiene el hit-test alineado.

Reglas:

- Ningun texto, panel o control se dibuja dentro de los insets.
- Cada control interactivo mide al menos 44 x 44 px logicos y se activa al
  soltar dentro; no requiere hover.
- La cabecera mide 104 unidades (88 en la galeria) y el pie 96; las escenas de
  run usan el pie para acciones y la cabecera para titulo y moneda.

## Chrome and controls

`ChromeRenderer` es la unica implementacion de:

- paneles de 9 segmentos en tonos `Default`, `Raised`, `Sunken`, `Accent`,
  `Gold`, `Danger`, `Perk`, `Violet`;
- botones con estados `Idle`, `Hover`, `Pressed`, `Disabled`;
- chips de moneda, cintas, badges, tags de estado, slots de progreso, divisores,
  overlay con vineta y glow aditivo.

`UiButton` resuelve el estado por frame y solo activa si el puntero se suelta
dentro. `UiAnimator` conserva el valor final de cada clave para que una escena
pueda seguir leyendo el valor tras terminar la animacion.

## Procedure card art

- `CardArtComposer` es puro y determinista: FNV-1a de `cardId`, tema y coste.
- Salida RGBA de 128 x 192. La composicion se elige por coste: plano cercano
  (1-2), fragmentos (3-4), estallido (5-6).
- La firma visual combina cuatro patrones de fondo, ocho emblemas rotados,
  cuatro trazos de firma y un tono derivado del tema/coste. Dos cartas del mismo
  tema no comparten la misma silueta.
- `CardArtProvider` cachea cada textura de forma perezosa y la reutiliza; no se
  regenera por frame. `CardRenderer` dibuja arte, marco, badges, nombre, tag y
  precio en capas separadas.
- `CardArtProvider.HasRealArt(id)` permite sustituir solo la capa de arte por
  una textura real; el marco, badges y texto no cambian. El crop usa
  `CropToFill` para respetar el area segura del arte.
- Las tarjetas de las siete pantallas usan `CardRenderer`; `CardView` sigue
  siendo el renderer del combate.

## Background and transitions

`SpaceBackground` combina gradiente vertical, parallax de hexagonos, vineta y
un campo de particulas acotado. Se crea una instancia por escena con semilla
propia y no asigna texturas por frame.

`SceneManager` implementa `Idle -> FadeOut -> cambio diferido -> FadeIn ->
Idle` con 0.18 s de salida y 0.22 s de entrada. Durante la transicion:

- `Game1` avanza el fundido antes que el input;
- la escena no se actualiza y el input de escena se ignora;
- el frame que provoco el cambio se consume para que la escena nueva no procese
  el mismo toque;
- un cambio pedido durante el fade-in se convierte en una salida desde el alpha
  actual, sin apilar transiciones.

El boton de pausa se dibuja en la cabecera central de las escenas de run para no
solaparse con tarjetas, filas de servicio ni acciones. Atras cierra la pausa,
vuelve al menu desde Summary, Collection, GlobalStore y la galeria, y sale de la
app desde Menu.

## Screen states

### Menu

- Reactor central: `SIN RUN`, `RUN ACTIVA` o `RUN TERMINADA`.
- Chip de puntos del perfil.
- Run activa: enemigo pendiente, combates, nivel y tira del mazo.
- Sin run: tarjetas de catalogo (cartas, perks, combates).
- Accion primaria: `CONTINUAR`, `VER RESUMEN` o `NUEVA RUN`.
- Confirmacion de nueva run cuando hay una run activa.
- Navegacion inferior: Coleccion, Tienda, Nueva run, Salir.

### Draft

- Tira de ocho slots con el pick actual resaltado.
- Cinta de tema (`CONTROL`, `DESTRUIR`, `AL REVELARSE` o `ALEATORIO`).
- Tres cartas con arte, coste y poder; entrada escalonada.
- Al elegir, la carta vuela al slot y el pick se guarda en el checkpoint.
- 8/8 habilita `CONTINUAR` hacia el siguiente nodo.

### RunSummary

- Reactor de resultado (`W`/`L`) con glow, nivel, XP y combates.
- Mensaje de vida extra consumida cuando
  `RunDirector.LastDefeatConsumedExtraLife` esta activo.
- Desglose de puntos: combates normales, subjefes, nivel, jefe final y total.
- Estado del cobro: `PUNTOS COBRADOS`, `YA COBRADO` o `COBRO PENDIENTE`.
- Mazo final en seis columnas.
- Acciones: menu, re-run y, en derrota con rematch, reintentar combate. Si el
  cobro falla, el boton de reintento permanece accesible.

### Collection

- Pestanas `CARTAS` y `PERKS`.
- Grilla de cuatro columnas por tres filas con arte, coste, poder, nombre y tag
  de estado (`DESCUBIERTA`, `EN COLECCION`, `EN TIENDA`, `DESCONOCIDA`).
- Filtros ciclicos de estado, tema y coste; paginado de 14 cartas por pagina en un catalogo de 217.
- Overlay de detalle con carta grande, reglas, estado, ruta de obtencion y
  coste/poder.
- Pestana de perks con nombre, regla, copias y ruta de obtencion; detalle en
  overlay.

### LevelUp

- Plantilla de ceremonia (`UPGRADE UNLOCKED`, nivel y mazo) en las siete fases.
- `ELIGE UNA RAMA`: carta o perk, con el estado del mazo y vista previa.
- `PickCard`: tres ofertas con arte.
- `PickUpgradeCard` y `PickPerkTarget`: grilla del mazo con coste y poder
  efectivos; las cartas no elegibles se atenuan.
- `PickUpgradeType`: `-1 COSTE` y `+2 PODER` con precio y disponibilidad.
- `PickPerk`: tres perks con nombre y regla.
- `Unavailable`: estado sin opciones con `CONTINUAR` alcanzable.

### GlobalStore

- Chip de puntos y pestanas `OFERTAS` / `CARTAS`.
- Ofertas en filas con nombre, descripcion, precio y estado `VENDIDA`.
- Cartas en grilla de tres columnas por dos filas con arte, precio y estado.
- Paginado de seis entradas; el saldo y el estado vendido se actualizan en el
  mismo frame de la compra.
- Mensajes de exito o error bajo la grilla.

### Shop

- Chip de dinero de la run.
- Cuatro ofertas con arte, precio y estado vendido.
- Tira del mazo con coste y poder compactos.
- Filas de servicio: mejora, retirada, vida extra y salir.
- Fases de seleccion de carta (`ReplaceCard`, `UpgradeCard`, `RemoveCard`) con
  coste/poder efectivos y controles de confirmar/cancelar.
- `UpgradeType` muestra la carta elegida y los dos tipos de mejora.

## Responsive, touch and budget

- Todas las pantallas se validaron a 1080 x 2400 (escala 1.5) en el emulador
  Android y la composicion base es 720 x 1280. La grilla de coleccion, las
  tarjetas y los botones usan el area de contenido, no medidas fijas.
- Los objetivos tactiles nunca bajan de 44 px logicos.
- Texturas generadas una vez: gradiente, fade, glow radial, ruido, hex, panel,
  scanline, vineta y arte de carta.
- Sin asignaciones de textura por frame; el arte se cachea por id y se reutiliza
  entre pantallas y visitas.
- Presupuesto de arte: 128 x 192 x 4 bytes por carta (98 KB) y un atlas de
  1024 x 1024 por rol tipografico.
