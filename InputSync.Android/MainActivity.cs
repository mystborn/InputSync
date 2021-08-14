using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using System;

namespace InputSync.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public Func<KeyEvent, bool> OnVolumeEvent { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.Action == KeyEventActions.Down || e.Action == KeyEventActions.Multiple)
            {
                switch (e.KeyCode)
                {
                    case Keycode.VolumeUp:
                    case Keycode.VolumeDown:
                        if (OnVolumeEvent != null)
                        {
                            int count = Math.Max(e.RepeatCount, 1);
                            for(int i = 0; i < count; i++)
                            {
                                if (!OnVolumeEvent(e))
                                    break;
                            }

                            return true;
                        }
                        break;
                }
            }
            return base.DispatchKeyEvent(e);
        }
    }
}