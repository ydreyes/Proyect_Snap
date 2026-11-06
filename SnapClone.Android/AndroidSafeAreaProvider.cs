using System;
using Android.App;
using Android.Views;
using Microsoft.Xna.Framework;
using SnapClone.Core.Services;

namespace SnapClone.Android
{
    public class AndroidSafeAreaProvider : ISafeAreaProvider
    {
        private readonly Activity _activity;

        public AndroidSafeAreaProvider(Activity activity)
        {
            _activity = activity;
        }

        public Rectangle GetInsets()
        {
            var decorView = _activity.Window?.DecorView;
            var insets = decorView?.RootWindowInsets;
            if (insets == null)
                return Rectangle.Empty;

            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                var bars = insets.GetInsets(
                    WindowInsets.Type.SystemBars() | WindowInsets.Type.DisplayCutout());

                return new Rectangle(bars.Left, bars.Top, bars.Right, bars.Bottom);
            }

#pragma warning disable CA1416, CA1422 // Legacy fallback for API 24-29; deprecated from API 30.
            var legacy = insets.SystemWindowInsets;
            return new Rectangle(legacy.Left, legacy.Top, legacy.Right, legacy.Bottom);
#pragma warning restore CA1416, CA1422
        }
    }
}
