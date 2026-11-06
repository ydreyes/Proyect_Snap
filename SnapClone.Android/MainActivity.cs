using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace SnapClone.Android
{
    [Activity(
        Label = "SnapClone",
        Icon = "@drawable/snapclone_icon",
        MainLauncher = true,
        ScreenOrientation = ScreenOrientation.Portrait,
        Theme = "@style/SnapCloneTheme",
        ConfigurationChanges = ConfigChanges.Orientation
            | ConfigChanges.ScreenSize
            | ConfigChanges.UiMode
            | ConfigChanges.Keyboard
            | ConfigChanges.KeyboardHidden)]
    public class MainActivity : AndroidGameActivity
    {
        private Game1 _game;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _game = new Game1();
            _game.SafeAreaProvider = new AndroidSafeAreaProvider(this);
            SetContentView(_game.Services.GetService(typeof(View)) as View);
            _game.Run();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            ApplyImmersiveMode();
        }

        protected override void OnPause()
        {
            Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
            base.OnPause();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            if (hasFocus)
                ApplyImmersiveMode();
        }

        private void ApplyImmersiveMode()
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
#pragma warning disable CA1422 // SetDecorFitsSystemWindows is deprecated on API 35 but still functional.
                Window.SetDecorFitsSystemWindows(false);
#pragma warning restore CA1422

                var controller = Window.InsetsController;
                if (controller == null)
                    return;

                controller.Hide(WindowInsets.Type.SystemBars());
                controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                return;
            }

#pragma warning disable CS0618
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                SystemUiFlags.LayoutStable
                | SystemUiFlags.LayoutHideNavigation
                | SystemUiFlags.LayoutFullscreen
                | SystemUiFlags.HideNavigation
                | SystemUiFlags.Fullscreen
                | SystemUiFlags.ImmersiveSticky);
#pragma warning restore CS0618
        }
    }
}
