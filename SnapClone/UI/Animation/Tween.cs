using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SnapClone.UI.Animation
{
    /// <summary>Curvas de interpolacion compartidas por toda la UI.</summary>
    public enum EasingKind
    {
        Linear,
        EaseOutCubic,
        EaseOutBack,
        EaseInOutQuad,
        SinePulse
    }

    public static class Easing
    {
        public static float Evaluate(EasingKind kind, float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);

            switch (kind)
            {
                case EasingKind.EaseOutCubic:
                {
                    float inverse = 1f - t;
                    return 1f - inverse * inverse * inverse;
                }

                case EasingKind.EaseOutBack:
                {
                    const float overshoot = 1.70158f;
                    float inverse = t - 1f;
                    return 1f + (overshoot + 1f) * inverse * inverse * inverse +
                           overshoot * inverse * inverse;
                }

                case EasingKind.EaseInOutQuad:
                    return t < 0.5f
                        ? 2f * t * t
                        : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;

                // Empieza en 0, llega a 1 a mitad y vuelve a 0.
                case EasingKind.SinePulse:
                    return 0.5f - 0.5f * MathF.Cos(t * MathF.PI * 2f);

                default:
                    return t;
            }
        }
    }

    /// <summary>
    /// Un valor animado con retardo, duracion, curva y callback final.
    /// Es determinista: no depende de relojes ni de aleatoriedad.
    /// </summary>
    public class Tween
    {
        private float _elapsed;
        private bool _completed;

        public float Delay { get; set; }
        public float Duration { get; set; }
        public float From { get; set; }
        public float To { get; set; }
        public EasingKind Kind { get; set; } = EasingKind.EaseOutCubic;
        public Action OnComplete { get; set; }

        /// <summary>Repite la animacion indefinidamente (util para pulsos).</summary>
        public bool Loop { get; set; }

        public Tween(float from, float to, float duration, EasingKind easing =
            EasingKind.EaseOutCubic, float delay = 0f)
        {
            From = from;
            To = to;
            Duration = MathF.Max(0f, duration);
            Kind = easing;
            Delay = MathF.Max(0f, delay);
        }

        /// <summary>Progreso 0..1 dentro de la duracion, sin aplicar la curva.</summary>
        public float Progress
        {
            get
            {
                if (Duration <= 0f) return 1f;
                return MathHelper.Clamp((_elapsed - Delay) / Duration, 0f, 1f);
            }
        }

        public bool IsComplete => _completed;
        public bool IsRunning => !_completed;

        public float Value => MathHelper.Lerp(From, To,
            Easing.Evaluate(Kind, Progress));

        /// <summary>Avanza el tween y devuelve true si sigue activo.</summary>
        public bool Advance(float deltaSeconds)
        {
            if (_completed) return false;

            _elapsed += MathF.Max(0f, deltaSeconds);

            if (Progress >= 1f)
            {
                if (Loop)
                {
                    _elapsed = Delay;
                    return true;
                }

                _completed = true;
                OnComplete?.Invoke();
                return false;
            }

            return true;
        }

        public void Reset()
        {
            _elapsed = 0f;
            _completed = false;
        }

        public void Complete()
        {
            if (_completed) return;
            _elapsed = Delay + Duration;
            _completed = true;
            OnComplete?.Invoke();
        }
    }

    /// <summary>
    /// Lista de tweens por clave. Las escenas consultan <see cref="Value"/>
    /// durante el dibujo y llaman a <see cref="Update(float)"/> una vez por frame.
    /// </summary>
    public class UiAnimator
    {
        private readonly List<Tween> _tweens = new List<Tween>();
        private readonly Dictionary<string, Tween> _byKey =
            new Dictionary<string, Tween>(StringComparer.Ordinal);

        public bool IsRunning => _tweens.Count > 0;
        public int Count => _tweens.Count;

        public Tween Add(Tween tween)
        {
            if (tween == null) return null;
            _tweens.Add(tween);
            return tween;
        }

        public Tween Add(string key, float from, float to, float duration,
            EasingKind easing = EasingKind.EaseOutCubic, float delay = 0f,
            Action onComplete = null)
        {
            var tween = new Tween(from, to, duration, easing, delay)
            {
                OnComplete = onComplete
            };

            if (string.IsNullOrEmpty(key))
            {
                _tweens.Add(tween);
                return tween;
            }

            _byKey[key] = tween;
            _tweens.Add(tween);
            return tween;
        }

        /// <summary>Reproduce un tween de 0 a 1 al entrar, con retardo escalonado.</summary>
        public Tween AddEntrance(string key, int index, float stagger = 0.05f,
            float duration = 0.32f)
        {
            return Add(key, 0f, 1f, duration, EasingKind.EaseOutCubic,
                MathF.Max(0f, index) * stagger);
        }

        public float Value(string key, float fallback = 0f)
        {
            if (!string.IsNullOrEmpty(key) &&
                _byKey.TryGetValue(key, out Tween tween))
                return tween.Value;

            return fallback;
        }

        public bool IsComplete(string key)
        {
            return !string.IsNullOrEmpty(key) &&
                   _byKey.TryGetValue(key, out Tween tween) &&
                   tween.IsComplete;
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (!_byKey.TryGetValue(key, out Tween tween)) return;

            _byKey.Remove(key);
            _tweens.Remove(tween);
        }

        public void Update(float deltaSeconds)
        {
            for (int i = _tweens.Count - 1; i >= 0; i--)
            {
                var tween = _tweens[i];
                if (tween.Advance(deltaSeconds))
                    continue;

                if (tween.IsComplete)
                {
                    // La clave conserva el valor final: las escenas lo siguen
                    // leyendo despues de que la animacion termine.
                    _tweens.RemoveAt(i);
                }
            }
        }

        public void Update(GameTime gameTime) =>
            Update((float)gameTime.ElapsedGameTime.TotalSeconds);

        /// <summary>Completa todo de inmediato (cambio de escena, reinicio).</summary>
        public void CompleteAll()
        {
            for (int i = 0; i < _tweens.Count; i++)
                _tweens[i].Complete();

            _tweens.Clear();
            _byKey.Clear();
        }

        public void Clear()
        {
            _tweens.Clear();
            _byKey.Clear();
        }
    }
}
