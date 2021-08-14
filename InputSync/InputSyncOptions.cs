using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputSync
{
    public class InputSyncOptions
    {
        private int _port = 45321;
        private double _mouseSensitivity = 1;

        public int Port
        {
            get => _port;
            set
            {
                if (value < 1024 || value > 65535)
                    throw new ArgumentException("Invalid port value");

                _port = value;
            }
        }

        public double MouseSensitivity
        {
            get => _mouseSensitivity;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Invalid mouse sensitivity value. Must be a number and greater than 0.");

                _mouseSensitivity = value;
            }
        }
    }
}
