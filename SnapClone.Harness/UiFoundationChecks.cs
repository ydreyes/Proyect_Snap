using System;
using System.Linq;
using Microsoft.Xna.Framework;
using SnapClone.Data.Definitions;
using SnapClone.Scenes;
using SnapClone.UI.Animation;
using SnapClone.UI.Components;
using SnapClone.UI.Layout;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.Harness
{
    /// <summary>
    /// Comprobaciones puras de la base visual: tokens, scaffold, boton,
    /// animacion, arte procedural, transiciones y recursos de fuente.
    /// No crea GraphicsDevice ni abre ventana.
    /// </summary>
    internal static partial class Program
    {
        private static void RunUiFoundationChecks()
        {
            RunTokenChecks();
            RunScaffoldChecks();
            RunButtonChecks();
            RunEasingChecks();
            RunTransitionChecks();
            RunCardArtChecks();
            RunFontResourceChecks();
        }

        private static void RunTokenChecks()
        {
            Check("ui tokens: escala ordenada",
                UiTheme.DisplayXl > UiTheme.Display &&
                UiTheme.Display > UiTheme.Heading &&
                UiTheme.Heading > UiTheme.Label &&
                UiTheme.Label > UiTheme.LabelSmall &&
                UiTheme.LabelSmall > UiTheme.Body &&
                UiTheme.Body > UiTheme.Caption,
                "la escala tipografica no es decreciente");

            Check("ui tokens: numeros dominantes",
                UiTheme.Numeric > UiTheme.NumericSmall &&
                UiTheme.NumericSmall > UiTheme.Caption,
                "los numeros no dominan sobre las etiquetas");

            Check("ui tokens: touch target",
                UiTheme.TouchTarget >= 44 && UiTheme.BorderThickness >= 1 &&
                UiTheme.CornerCut > 0,
                "touch target o geometria fuera de rango");

            double primaryContrast = UiTheme.ContrastRatio(
                UiTheme.TextPrimary, UiTheme.Panel);
            double secondaryContrast = UiTheme.ContrastRatio(
                UiTheme.TextSecondary, UiTheme.Panel);

            Check("ui tokens: contraste primario >= 4.5",
                primaryContrast >= 4.5,
                $"contraste {primaryContrast:F2}");
            Check("ui tokens: contraste secundario >= 3.0",
                secondaryContrast >= 3.0,
                $"contraste {secondaryContrast:F2}");

            Check("ui tokens: estados semanticos distintos",
                UiTheme.StateAffordable != UiTheme.StateUnaffordable &&
                UiTheme.StateSelected != UiTheme.StateSold,
                "los estados comparten color");

            Check("ui tokens: ui scale acotado",
                Math.Abs(UiTheme.UiScale(720, 1280) -
                    UiTheme.ReadabilityScale) < 0.001f &&
                UiTheme.UiScale(360, 640) >= 1f &&
                UiTheme.UiScale(2160, 3840) <= 3f,
                "el escalado sale del rango 1..3");
        }

        private static void RunScaffoldChecks()
        {
            var plain = ScreenScaffold.Create(new Rectangle(0, 0, 720, 1280));
            Check("scaffold: cabecera y contenido contiguos",
                plain.Header.Bottom == plain.Content.Top,
                "cabecera y contenido se solapan");
            Check("scaffold: contenido y pie contiguos",
                plain.Content.Bottom == plain.Footer.Top,
                "contenido y pie se solapan");
            Check("scaffold: pie termina en el borde",
                plain.Footer.Bottom == 1280 && plain.Header.Top == 0,
                "el scaffold no cubre el viewport");
            Check("scaffold: contenido util",
                plain.Content.Height > 720 && plain.Content.Width == 720,
                "el contenido es demasiado pequeno");
            Check("scaffold: sin insets el area segura es el viewport",
                plain.SafeBounds == new Rectangle(0, 0, 720, 1280),
                "el area segura no coincide");

            var inset = ScreenScaffold.Create(new Rectangle(0, 0, 720, 1280),
                new ScreenInsets(24, 40, 24, 40));
            Check("scaffold: area segura con insets",
                inset.SafeBounds == new Rectangle(24, 40, 672, 1200),
                $"area segura {inset.SafeBounds}");
            Check("scaffold: regiones dentro del area segura",
                Contains(inset.SafeBounds, inset.Header) &&
                Contains(inset.SafeBounds, inset.Content) &&
                Contains(inset.SafeBounds, inset.Footer),
                "alguna region invade los insets");
            Check("scaffold: insets no solapan regiones",
                inset.Header.Top >= 40 && inset.Footer.Bottom <= 1240 &&
                inset.Content.Left >= 24 && inset.Content.Right <= 696,
                "el contenido invade el notch o los gestos");
            Check("scaffold: ui scale con insets",
                Math.Abs(inset.UiScale -
                    MathF.Min(672f / 720f, 1200f / 1280f) *
                    UiTheme.ReadabilityScale) < 0.001f,
                $"escala {inset.UiScale}");
        }

        private static void RunButtonChecks()
        {
            Check("boton: deshabilitado nunca activa",
                UiButton.ResolveState(false, true, true, true) ==
                ButtonVisualState.Disabled &&
                !UiButton.ShouldArm(false, true, true),
                "un boton deshabilitado puede activarse");

            Check("boton: hover",
                UiButton.ResolveState(true, true, false, false) ==
                ButtonVisualState.Hover,
                "el hover no se resuelve");

            Check("boton: pulsado",
                UiButton.ResolveState(true, true, true, true) ==
                ButtonVisualState.Pressed,
                "el estado pulsado no se resuelve");

            Check("boton: reposo",
                UiButton.ResolveState(true, false, false, false) ==
                ButtonVisualState.Idle,
                "el reposo no se resuelve");

            Check("boton: arma solo dentro",
                UiButton.ShouldArm(true, true, true) &&
                !UiButton.ShouldArm(true, true, false),
                "armado fuera de limites");

            Check("boton: drag-off cancela",
                UiButton.ShouldCancel(true, true, false) &&
                !UiButton.ShouldCancel(true, true, true),
                "arrastrar fuera no cancela");

            Check("boton: activa solo al soltar dentro",
                UiButton.ShouldActivate(true, true, true) &&
                !UiButton.ShouldActivate(true, false, true) &&
                !UiButton.ShouldActivate(true, true, false),
                "la activacion no exige soltar dentro");
        }

        private static void RunEasingChecks()
        {
            EasingKind[] kinds =
            {
                EasingKind.Linear,
                EasingKind.EaseOutCubic,
                EasingKind.EaseOutBack,
                EasingKind.EaseInOutQuad
            };

            foreach (EasingKind kind in kinds)
            {
                Check($"easing {kind}: inicio 0",
                    Math.Abs(Easing.Evaluate(kind, 0f)) < 0.0001f,
                    "no empieza en 0");
                Check($"easing {kind}: fin 1",
                    Math.Abs(Easing.Evaluate(kind, 1f) - 1f) < 0.0001f,
                    "no termina en 1");
            }

            Check("easing: pulse empieza en 0",
                Math.Abs(Easing.Evaluate(EasingKind.SinePulse, 0f)) < 0.0001f,
                "el pulso no empieza en 0");
            Check("easing: pulse pico en 0.5",
                Math.Abs(Easing.Evaluate(EasingKind.SinePulse, 0.5f) - 1f) < 0.0001f,
                "el pulso no llega a 1");
            Check("easing: pulse vuelve a 0",
                Math.Abs(Easing.Evaluate(EasingKind.SinePulse, 1f)) < 0.0001f,
                "el pulso no cierra");

            var tween = new Tween(0f, 100f, 1f, EasingKind.Linear, 0.25f);
            Check("tween: respeta el retardo",
                Math.Abs(tween.Value) < 0.0001f,
                "el valor cambia antes del retardo");
            tween.Advance(0.25f);
            Check("tween: mitad",
                Math.Abs(tween.Value - 0f) < 0.0001f,
                $"valor {tween.Value}");
            tween.Advance(0.5f);
            Check("tween: interpolacion",
                Math.Abs(tween.Value - 50f) < 0.0001f,
                $"valor {tween.Value}");

            int completions = 0;
            tween.OnComplete = () => completions++;
            bool running = tween.Advance(0.5f);
            Check("tween: completa",
                !running && tween.IsComplete &&
                Math.Abs(tween.Value - 100f) < 0.0001f,
                "no termina en el destino");
            Check("tween: callback una vez",
                completions == 1,
                $"callbacks {completions}");

            var animator = new UiAnimator();
            animator.AddEntrance("card0", 0);
            animator.AddEntrance("card1", 1, 0.05f);
            animator.Update(0.16f);
            Check("animator: stagger",
                animator.Value("card0") > animator.Value("card1"),
                "los elementos no entran escalonados");
            animator.Update(0.5f);
            Check("animator: entrada completa",
                Math.Abs(animator.Value("card0") - 1f) < 0.0001f &&
                Math.Abs(animator.Value("card1") - 1f) < 0.0001f &&
                !animator.IsRunning,
                "la entrada no termina");
        }

        private static void RunTransitionChecks()
        {
            var manager = new SceneManager();
            var first = new FakeScene();
            var second = new FakeScene();
            manager.AddScene("A", first);
            manager.AddScene("B", second);

            manager.ChangeScene("A", SceneTransitionMode.Instant);
            Check("transicion: cambio instantaneo",
                manager.CurrentSceneName == "A" && first.Enters == 1 &&
                !manager.IsTransitioning,
                "el cambio instantaneo falla");

            manager.ChangeScene("B");
            Check("transicion: inicia fade-out",
                manager.Phase == TransitionPhase.FadeOut &&
                manager.CurrentSceneName == "A" &&
                manager.InputBlocked,
                "el fade-out no inicia");

            manager.UpdateTransition(Frame(0.09f));
            Check("transicion: alpha intermedio",
                manager.FadeAlpha > 0.4f && manager.FadeAlpha < 0.6f &&
                manager.CurrentSceneName == "A" && second.Enters == 0,
                $"alpha {manager.FadeAlpha}");

            manager.UpdateTransition(Frame(0.10f));
            Check("transicion: cambio diferido en el punto medio",
                manager.CurrentSceneName == "B" && second.Enters == 1 &&
                manager.Phase == TransitionPhase.FadeIn &&
                manager.ConsumeSceneChange(),
                "la escena no cambia en el medio");
            Check("transicion: entrada inicia en negro",
                manager.FadeAlpha >= 0.99f,
                $"alpha {manager.FadeAlpha}");

            manager.UpdateTransition(Frame(0.10f));
            Check("transicion: escena no se actualiza",
                second.Updates == 0,
                "la escena se actualiza durante el fundido");

            manager.UpdateTransition(Frame(0.20f));
            Check("transicion: termina acotada",
                manager.Phase == TransitionPhase.Idle &&
                manager.FadeAlpha <= 0.001f && !manager.InputBlocked,
                "la transicion no termina");

            manager.Update(Frame(0.016f));
            Check("transicion: input y update tras el fundido",
                second.Updates == 1,
                "la escena no reanuda su update");

            // Cambio repetido durante el fade-in: no se apilan.
            manager.ChangeScene("A");
            manager.UpdateTransition(Frame(0.20f)); // cruza el switch a A
            manager.ChangeScene("B");
            manager.UpdateTransition(Frame(0.20f));
            manager.ChangeScene("A");
            manager.UpdateTransition(Frame(0.60f));
            manager.UpdateTransition(Frame(0.30f));
            Check("transicion: cambios repetidos no se apilan",
                manager.Phase == TransitionPhase.Idle &&
                manager.FadeAlpha <= 0.001f,
                "la transicion acumula duracion");

            manager.ChangeScene("NoExiste");
            Check("transicion: escena desconocida ignorada",
                manager.Phase == TransitionPhase.Idle &&
                manager.CurrentSceneName == "A",
                "una escena desconocida altera el estado");
        }

        private static void RunCardArtChecks()
        {
            byte[] first = CardArtComposer.Compose("card_a", DraftTheme.Control, 2);
            byte[] second = CardArtComposer.Compose("card_a", DraftTheme.Control, 2);

            Check("arte: tamano 128x192 RGBA",
                first.Length == 128 * 192 * 4,
                $"bytes {first.Length}");
            Check("arte: determinista",
                first.SequenceEqual(second),
                "dos composiciones difieren");
            Check("arte: alfa opaco",
                Enumerable.Range(0, first.Length / 4)
                    .All(i => first[i * 4 + 3] == 255),
                "hay pixeles transparentes");

            byte[] otherTheme = CardArtComposer.Compose("card_a",
                DraftTheme.Destroy, 2);
            byte[] otherCost = CardArtComposer.Compose("card_a",
                DraftTheme.Control, 5);
            byte[] otherId = CardArtComposer.Compose("card_b",
                DraftTheme.Control, 2);

            Check("arte: varia por tema",
                !first.SequenceEqual(otherTheme),
                "el tema no altera el arte");
            Check("arte: varia por coste",
                !first.SequenceEqual(otherCost),
                "el coste no altera el arte");
            Check("arte: varia por id",
                !first.SequenceEqual(otherId),
                "el id no altera el arte");

            Check("arte: composicion por coste",
                CardArtComposer.CompositionForCost(1) == 0 &&
                CardArtComposer.CompositionForCost(2) == 0 &&
                CardArtComposer.CompositionForCost(3) == 1 &&
                CardArtComposer.CompositionForCost(4) == 1 &&
                CardArtComposer.CompositionForCost(5) == 2 &&
                CardArtComposer.CompositionForCost(6) == 2,
                "los tramos de coste no coinciden");

            uint seed = CardArtComposer.SeedFor("card_a", DraftTheme.Control, 2);
            Check("arte: semilla estable",
                seed == CardArtComposer.SeedFor("card_a", DraftTheme.Control, 2) &&
                seed != CardArtComposer.SeedFor("card_a", DraftTheme.Destroy, 2) &&
                seed != CardArtComposer.SeedFor("card_a", DraftTheme.Control, 3),
                "la semilla no mezcla id, tema y coste");

            var destination = new Rectangle(0, 0, 120, 80);
            Rectangle crop = CardArtProvider.CropToFill(128, 192, destination);
            Check("arte: recorte sin deformar",
                crop.Width > 0 && crop.Height > 0 &&
                crop.Width <= 128 && crop.Height <= 192 &&
                Math.Abs(crop.Width / (float)crop.Height - 1.5f) < 0.02f,
                $"recorte {crop}");

            Rectangle wide = CardArtProvider.CropToFill(400, 100, destination);
            Check("arte: recorte horizontal",
                wide.Width == 150 && wide.Height == 100,
                $"recorte {wide}");
        }

        private static void RunFontResourceChecks()
        {
            string[] expected =
            {
                "Fonts.BebasNeue-Regular.ttf",
                "Fonts.Rajdhani-SemiBold.ttf",
                "Fonts.Rajdhani-Bold.ttf",
                "Fonts.Inter-Regular.ttf",
                "Fonts.BebasNeue-OFL.txt",
                "Fonts.Rajdhani-OFL.txt",
                "Fonts.Inter-OFL.txt"
            };

            string[] resources = typeof(FontService).Assembly
                .GetManifestResourceNames();

            foreach (string name in expected)
            {
                Check($"fuentes: recurso {name}",
                    resources.Any(resource => resource.EndsWith(name,
                        StringComparison.OrdinalIgnoreCase)),
                    "recurso embebido ausente");
            }
        }

        private static bool Contains(Rectangle outer, Rectangle inner)
        {
            return inner.Left >= outer.Left && inner.Top >= outer.Top &&
                   inner.Right <= outer.Right && inner.Bottom <= outer.Bottom;
        }

        private static GameTime Frame(float seconds) =>
            new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(seconds));

        private sealed class FakeScene : IScene
        {
            public int Initializes { get; private set; }
            public int Enters { get; private set; }
            public int Updates { get; private set; }

            public void Initialize() => Initializes++;
            public void OnEnter() => Enters++;
            public void LoadContent() { }
            public void Update(GameTime gameTime) => Updates++;
            public void Draw(GameTime gameTime,
                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }
    }
}
