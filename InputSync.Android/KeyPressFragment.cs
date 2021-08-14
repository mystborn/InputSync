using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace InputSync.Android
{
    public class KeyPressFragment : AndroidX.Fragment.App.Fragment
    {
        public const string ARG_IP = "IP";
        public const string ARG_PORT = "Port";
        private const int KEYBOARD_EVENT = 0;
        private const int MOUSE_EVENT = 1;
        private const int VOLUME_EVENT = 2;

        private const int MOUSE_CLICK = 0;
        private const int MOUSE_DOUBLE_CLICK = 1;
        private const int MOUSE_MOVE = 2;
        private const int MOUSE_SCROLL = 3;
        private const int MOUSE_HOLD = 4;
        private const int MOUSE_RELEASE = 5;

        private const int MOUSE_LEFT = 0;
        private const int MOUSE_RIGHT = 1;


        private string _ip;
        private int _port;
        private UdpClient _client;
        private EditText _input;
        private byte[] _buffer = new byte[1024];
        private TouchCapture _touch;
        private bool _leftDown = false;
        private bool _rightDown = false;

        public static KeyPressFragment Create(string ip, int port)
        {
            var args = new Bundle();
            args.PutString(ARG_IP, ip);
            args.PutInt(ARG_PORT, port);
            var fragment = new KeyPressFragment();
            fragment.Arguments = args;

            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var args = RequireArguments();
            _ip = args.GetString(ARG_IP);
            _port = args.GetInt(ARG_PORT);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_key_press, container, false);
        }

        public override unsafe void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var mouseArea = view.FindViewById(Resource.Id.key_press_mouse_area);
            var leftMouse = view.FindViewById(Resource.Id.key_press_left_mouse);
            var rightMouse = view.FindViewById(Resource.Id.key_press_right_mouse);

            _input = view.FindViewById<EditText>(Resource.Id.key_press_input);

            _touch = new TouchCapture(Context);

            _touch.Clicked += () =>
            {
                _buffer[0] = MOUSE_EVENT;
                _buffer[1] = MOUSE_CLICK;

                _client.BeginSend(_buffer, 2, r => _client.EndSend(r), null);
            };

            _touch.DoubleClicked += () =>
            {
                _buffer[0] = MOUSE_EVENT;
                _buffer[1] = MOUSE_DOUBLE_CLICK;

                _client.BeginSend(_buffer, 2, r => _client.EndSend(r), null);
            };

            _touch.Moved += (x, y) =>
            {
                _buffer[0] = MOUSE_EVENT;
                _buffer[1] = MOUSE_MOVE;
                var span = _buffer.AsSpan(2);
                BitConverter.TryWriteBytes(span, (int)x);
                span = _buffer.AsSpan(6);
                BitConverter.TryWriteBytes(span, (int)y);
                var isLittleEndian = BitConverter.IsLittleEndian;

                _client.BeginSend(_buffer, 10, r => _client.EndSend(r), null);
            };

            _touch.Scrolled += y =>
            {
                _buffer[0] = MOUSE_EVENT;
                _buffer[1] = MOUSE_SCROLL;
                fixed (byte* ptr = &_buffer[2])
                    *(int*)ptr = (int)y;

                _client.BeginSend(_buffer, 6, r => _client.EndSend(r), null);
            };

            mouseArea.SetOnTouchListener(_touch);

            _input.Text = " ";
            _input.TextChanged += (s, e) =>
            {
                if (_client is null)
                    return;

                if (_input.Text == " ")
                    return;

                _buffer[0] = KEYBOARD_EVENT;
                var input = _input.Text.Length == 0 ? "\b" : _input.Text[1..];

                var bytes = Encoding.Unicode.GetBytes(input, 0, input.Length, _buffer, 5);

                fixed (byte* ptr = &_buffer[1])
                    *(int*)ptr = bytes;

                _client.BeginSend(_buffer, bytes + 5, r => _client.EndSend(r), null);
                _input.Text = " ";
                _input.SetSelection(1);
            };

            (Activity as MainActivity).OnVolumeEvent = e =>
            {
                if (_client is null)
                    return false;

                _buffer[0] = VOLUME_EVENT;
                var span = _buffer.AsSpan(1);
                int delta = 0;
                switch(e.KeyCode)
                {
                    case Keycode.VolumeUp:
                        delta = 2;
                        break;
                    case Keycode.VolumeDown:
                        delta = -2;
                        break;
                }

                BitConverter.TryWriteBytes(span, delta);
                _client.BeginSend(_buffer, 5, r => _client.EndSend(r), null);

                return true;
            };

            var leftMouseListener = new MouseButtonListener();
            leftMouseListener.Pressed += () => MouseDown(MOUSE_LEFT);
            leftMouseListener.Released += () => MouseRelease(MOUSE_LEFT);

            var rightMouseListener = new MouseButtonListener();
            rightMouseListener.Pressed += () => MouseDown(MOUSE_RIGHT);
            rightMouseListener.Released += () => MouseRelease(MOUSE_RIGHT);

            leftMouse.SetOnTouchListener(leftMouseListener);
            rightMouse.SetOnTouchListener(rightMouseListener);

        }

        public override void OnResume()
        {
            base.OnResume();

            _client = new UdpClient();
            _client.Connect(new IPEndPoint(IPAddress.Parse(_ip), _port));
        }

        public override void OnPause()
        {
            base.OnPause();

            if (_leftDown)
                MouseRelease(MOUSE_LEFT);

            if (_rightDown)
                MouseRelease(MOUSE_RIGHT);

            _client.Dispose();
        }

        private void MouseDown(int button)
        {
            _buffer[0] = MOUSE_EVENT;
            _buffer[1] = MOUSE_HOLD;
            _buffer[2] = (byte)button;

            if (button == MOUSE_LEFT)
                _leftDown = true;
            else
                _rightDown = true;

            _client.BeginSend(_buffer, 3, r => _client.EndSend(r), null);
        }

        private void MouseRelease(int button)
        {
            _buffer[0] = MOUSE_EVENT;
            _buffer[1] = MOUSE_RELEASE;
            _buffer[2] = (byte)button;

            if (button == MOUSE_LEFT)
                _leftDown = true;
            else
                _rightDown = true;

            _client.BeginSend(_buffer, 3, r => _client.EndSend(r), null);
        }

        private class MouseButtonListener : Java.Lang.Object, View.IOnTouchListener
        {
            public event Action Pressed;
            public event Action Released;

            public bool OnTouch(View v, MotionEvent e)
            {
                switch(e.Action)
                {
                    case MotionEventActions.Down:
                        Pressed?.Invoke();
                        break;
                    case MotionEventActions.Up:
                        Released?.Invoke();
                        break;
                }

                return true;
            }
        }

        private class TouchCapture : GestureDetector.SimpleOnGestureListener, View.IOnTouchListener
        {
            private float _x;
            private float _y;
            private bool _canMove = true;
            private GestureDetector _detector;

            public event Action Clicked;
            public event Action DoubleClicked;
            public event Action<float, float> Moved;
            public event Action<float> Scrolled;

            public TouchCapture(Context context)
            {
                _detector = new GestureDetector(context, this);
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                if(e.PointerCount == 1 && e.Action == MotionEventActions.Down)
                {
                    _x = e.GetX();
                    _y = e.GetY();
                }

                if(e.PointerCount == 1 && e.Action == MotionEventActions.Move && _canMove)
                {
                    var newX = e.GetX();
                    var newY = e.GetY();

                    Moved?.Invoke(newX - _x, newY - _y);

                    _x = newX;
                    _y = newY;
                }

                if(!_canMove && e.Action == MotionEventActions.Up)
                {
                    _canMove = true;
                }

                _detector.OnTouchEvent(e);
                return true;
            }

            public override bool OnDoubleTap(MotionEvent e)
            {
                DoubleClicked?.Invoke();
                return true;
            }

            public override bool OnSingleTapUp(MotionEvent e)
            {
                Clicked?.Invoke();
                return true;
            }

            public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
            {
                if (e2.PointerCount > 1)
                {
                    _canMove = false;
                    Scrolled?.Invoke(distanceY);
                }

                return true;
            }
        }
    }
}