using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using AndroidX.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace InputSync.Android
{
    public class ConnectFragment : AndroidX.Fragment.App.Fragment
    {
        private const string PREFS = "PREFS";
        private const string PREFS_IP = "IP";
        private const string PREFS_PORT = "PORT";

        private EditText _ip;
        private EditText _port;
        private Button _connect;
        private CheckBox _saveSettings;
        private ISharedPreferences _prefs;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_connect, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            _prefs = Activity.GetSharedPreferences(PREFS, FileCreationMode.Append);

            _ip = view.FindViewById<EditText>(Resource.Id.connect_ip_field);
            _port = view.FindViewById<EditText>(Resource.Id.connect_port_field);
            _saveSettings = view.FindViewById<CheckBox>(Resource.Id.connect_save_settings_checkbox);
            _connect = view.FindViewById<Button>(Resource.Id.connect_connect_button);

            _connect.Click += (s, e) => Connect();

            _ip.Text = _prefs.GetString(PREFS_IP, "");
            var port = _prefs.GetInt(PREFS_PORT, -1);
            if (port != -1)
                _port.Text = port.ToString();
        }

        private void Connect()
        {
            Java.Lang.ICharSequence clearError = null;
            _ip.SetError(clearError, null);
            _port.SetError(clearError, null);

            var hasError = false;

            if(!IPAddress.TryParse(_ip.Text, out var ip))
            {
                _ip.SetError(
                    GetString(Resource.String.connect_error_ip_invalid), 
                    ContextCompat.GetDrawable(Context, Resource.Drawable.ic_error));

                hasError = true;
            }

            if(!int.TryParse(_port.Text, out var port))
            {
                _port.SetError(
                    GetString(Resource.String.connect_error_port_invalid),
                    ContextCompat.GetDrawable(Context, Resource.Drawable.ic_error));

                hasError = true;
            }

            if (hasError)
                return;


            IPEndPoint endpoint;
            try
            {
                endpoint = new IPEndPoint(ip, port);
            }
            catch(Exception)
            {
                _port.SetError(
                    GetString(Resource.String.connect_error_port_invalid),
                    ContextCompat.GetDrawable(Context, Resource.Drawable.ic_error));
                return;
            }

            if(_saveSettings.Checked)
            {
                _prefs
                    .Edit()
                    .PutString(PREFS_IP, _ip.Text)
                    .PutInt(PREFS_PORT, port)
                    .Apply();
            }

            var bundle = new Bundle();
            bundle.PutString(KeyPressFragment.ARG_IP, _ip.Text);
            bundle.PutInt(KeyPressFragment.ARG_PORT, port);

            Navigation
                .FindNavController(Activity, Resource.Id.nav_host_fragment)
                .Navigate(Resource.Id.keyPressFragment, bundle);
        }
    }
}