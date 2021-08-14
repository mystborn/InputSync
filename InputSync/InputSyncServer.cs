using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace InputSync
{
    public class InputSyncServer : IDisposable
    {
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

        private UdpClient _udpServer;
        private bool _disposed;
        private IPEndPoint _endpoint;
        private InputSimulator _input = new InputSimulator();
        private double _mouseScale;

        public InputSyncServer(InputSyncOptions options)
        {
            _endpoint = new IPEndPoint(IPAddress.Any, options.Port);
            _mouseScale = options.MouseSensitivity;
        }

        public void Start()
        {
            _udpServer = new UdpClient(_endpoint);
            _udpServer.BeginReceive(Receive, null);
        }

        public void Stop()
        {
            _udpServer.Close();
        }

        private void Receive(IAsyncResult result)
        {
            if (_udpServer.Client is null)
                return;

            byte[] bytes = _udpServer.EndReceive(result, ref _endpoint);

            try
            {
                switch (bytes[0])
                {
                    case KEYBOARD_EVENT:
                        KeyboardEvent(bytes);
                        break;
                    case MOUSE_EVENT:
                        MouseEvent(bytes);
                        break;
                    case VOLUME_EVENT:
                        VolumeEvent(bytes);
                        break;
                }
            }
            catch
            {
            }

            if (_udpServer.Client != null)
                _udpServer.BeginReceive(Receive, null);
        }

        private void KeyboardEvent(byte[] bytes)
        {
            var count = BitConverter.ToInt32(bytes, 1);
            var letters = Encoding.Unicode.GetChars(bytes, 5, count);

            foreach (var letter in letters)
            {
                if (letter == '\0')
                    break;

                switch (letter)
                {
                    case '\b':
                        _input.Keyboard.KeyPress(VirtualKeyCode.BACK);
                        break;
                    case '\n':
                        _input.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                        break;
                    default:
                        _input.Keyboard.TextEntry(letter);
                        break;
                }
            }
        }

        private void MouseEvent(byte[] buffer)
        {
            switch (buffer[1])
            {
                case MOUSE_CLICK:
                    _input.Mouse.LeftButtonClick();
                    break;
                case MOUSE_DOUBLE_CLICK:
                    _input.Mouse.LeftButtonDoubleClick();
                    break;
                case MOUSE_MOVE:
                    var deltaX = (int)(BitConverter.ToInt32(buffer, 2) * _mouseScale);
                    var deltaY = (int)(BitConverter.ToInt32(buffer, 6) * _mouseScale);
                    _input.Mouse.MoveMouseBy(deltaX, deltaY);
                    break;
                case MOUSE_SCROLL:
                    deltaY = (int)(BitConverter.ToInt32(buffer, 2) * .1);
                    _input.Mouse.VerticalScroll(deltaY);
                    break;
                case MOUSE_HOLD:
                    switch(buffer[2])
                    {
                        case MOUSE_LEFT:
                            _input.Mouse.LeftButtonDown();
                            break;
                        case MOUSE_RIGHT:
                            _input.Mouse.RightButtonDown();
                            break;
                    }
                    break;
                case MOUSE_RELEASE:
                    switch (buffer[2])
                    {
                        case MOUSE_LEFT:
                            _input.Mouse.LeftButtonUp();
                            break;
                        case MOUSE_RIGHT:
                            _input.Mouse.RightButtonUp();
                            break;
                    }
                    break;
            }
        }

        private void VolumeEvent(byte[] buffer)
        {
            var delta = BitConverter.ToInt32(buffer, 1);
            if (delta < 0)
            {
                _input.Keyboard.KeyPress(VirtualKeyCode.VOLUME_DOWN);
            }
            else
            {
                _input.Keyboard.KeyPress(VirtualKeyCode.VOLUME_UP);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _udpServer.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
