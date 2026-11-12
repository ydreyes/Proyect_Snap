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
            _game.DataDirectory = FilesDir.AbsolutePath;
            _game.ExitRequested = () => RunOnUiThread(Finish);

            var gameView = _game.Services.GetService(typeof(View)) as View;
            if (gameView != null)
                gameView.SetOnKeyListener(new GameKeyListener(this));

            SetContentView(gameView);
            _game.Run();
        }

        private sealed class GameKeyListener : Java.Lang.Object, View.IOnKeyListener
        {
            private readonly MainActivity _activity;

            public GameKeyListener(MainActivity activity)
            {
                _activity = activity;
            }

            public bool OnKey(View v, Keycode keyCode, KeyEvent e)
            {
                if (keyCode != Keycode.Back)
                    return false;

                if (e != null && e.Action == KeyEventActions.Down)
                    _activity._game?.RequestBack();

                return true;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            ApplyImmersiveMode();
            _game?.OnAppResumed();
        }

        protected override void OnPause()
        {
            _game?.OnAppPaused();
            Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
            base.OnPause();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                _game?.RequestBack();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        public override void OnBackPressed()
        {
            if (_game != null)
            {
                _game.RequestBack();
                return;
            }

            base.OnBackPressed();
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
