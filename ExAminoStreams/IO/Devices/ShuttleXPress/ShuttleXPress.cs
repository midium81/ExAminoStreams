using System.Globalization;

namespace StatsPerform.CollectionClients.Utilities.IO.Devices.ShuttleXPress
{
    /// <summary>
    /// This class allows to interface against USB ShuttleXPress device
    /// </summary>
    public class ShuttleXPress : GenericHid.GenericHid
    {
        // Array that stores last buffer read data used for checking whether a button is held down
        protected byte[] _lastBuffer;

        #region Events

        public delegate void JogWheelTurnedEventHandler(object sender, ShuttleEventArgs e);

        public event JogWheelTurnedEventHandler JogWheelTurned;

        public event JogWheelTurnedEventHandler InnerJogWheelTurned;

        public event EventHandler Button1Pressed;

        public event EventHandler Button2Pressed;

        public event EventHandler Button3Pressed;

        public event EventHandler Button4Pressed;

        public event EventHandler Button5Pressed;

        #endregion Events

        #region Enums

        /// <summary>
        /// Specifies which element in the devices byte array corresponds to which input function on the device
        /// </summary>
        protected enum InputBytes : int
        {
            JogWheel = 0,
            InnerWheel = 1,
            FirstFourButtons = 3,
            FifthButton = 4
        }

        #endregion Enums

        public ShuttleXPress() : base(int.Parse("0b33", NumberStyles.AllowHexSpecifier), int.Parse("0020", NumberStyles.AllowHexSpecifier))
        {
        }

        /// <summary>
        /// Translate byte values of current "tick" on inner wheel
        /// to values like -3, -2, -1, 0, 1, 2, 3...
        ///
        /// We compute it by diff of previous state
        /// and if it's 0 - 254 then we tell it's -2
        ///
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static int GetDiffBetweenBuffers(int previous, int current)
        {
            int diff = current - previous;

            if (diff < 0)
            {
                if (diff >= -128)
                    return diff;         // 255 -> 254 = -1
                else
                    return diff + 256;   // 255 -> 1   = -2
            }
            else if (diff <= 128)
                return diff;         // 0 -> 1   = +1
            else
                return diff - 256; // 0 -> 255 = -1
        }

        protected override void HandleInput()
        {
            if (_currentBuffer is null)
                return;
            if (_currentBuffer.Length < 4)
                return;

            if (_lastBuffer is null)
                _lastBuffer = new byte[_currentBuffer.GetUpperBound(0) + 1];

            if (_currentBuffer[(int)InputBytes.JogWheel] != 0)
                OnJogWheelTurned(_currentBuffer[(int)InputBytes.JogWheel]);

            int lastAmount = _lastBuffer[(int)InputBytes.InnerWheel];
            int currentAmount = _currentBuffer[(int)InputBytes.InnerWheel];

            if (lastAmount != currentAmount)
            {
                int realDiff = GetDiffBetweenBuffers(lastAmount, currentAmount);
                OnInnerJogWheelTurned(realDiff, _currentBuffer[(int)InputBytes.InnerWheel]);
            }

            int firstFourButtons = _currentBuffer[(int)InputBytes.FirstFourButtons];
            int lastButton;

            if (_lastBuffer is null)
                lastButton = 0;
            else
                lastButton = _lastBuffer[(int)InputBytes.FirstFourButtons];

            if (firstFourButtons != 0
                && firstFourButtons != lastButton)
            {
                switch (firstFourButtons)
                {
                    case var @case when @case == 16:
                        {
                            Button1Pressed?.Invoke(this, new EventArgs());
                            break;
                        }
                    case var case1 when case1 == 32:
                        {
                            Button2Pressed?.Invoke(this, new EventArgs());
                            break;
                        }
                    case var case2 when case2 == 64:
                        {
                            Button3Pressed?.Invoke(this, new EventArgs());
                            break;
                        }
                    case var case3 when case3 == 128:
                        {
                            Button4Pressed?.Invoke(this, new EventArgs());
                            break;
                        }
                }
            }

            if (_currentBuffer[(int)InputBytes.FifthButton] != 0
                && _currentBuffer[(int)InputBytes.FifthButton] != _lastBuffer[(int)InputBytes.FifthButton])
            {
                Button5Pressed?.Invoke(this, new EventArgs());
            }

            _lastBuffer = new byte[_currentBuffer.GetUpperBound(0) + 1];
            _currentBuffer.CopyTo(_lastBuffer, 0);
        }

        #region Event raisers

        /// <summary>
        /// Works out which direction jog wheel has been turned and raises event to notify client
        /// </summary>
        protected virtual void OnJogWheelTurned(int amountTurned)
        {
            var eventArgs = new ShuttleEventArgs();

            if (amountTurned > 7)
            {
                eventArgs.AmountTurned = 256 - amountTurned;
                eventArgs.Direction = ShuttleEventArgs.DirectionTurned.Left;
            }
            else
            {
                eventArgs.AmountTurned = amountTurned;
                eventArgs.Direction = ShuttleEventArgs.DirectionTurned.Right;
            }

            JogWheelTurned?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Works out which direction the internal jog wheel has been turned and raises event to notify client
        /// </summary>
        protected virtual void OnInnerJogWheelTurned(int amountTurned, byte innerWheelValue)
        {
            if (innerWheelValue != _lastBuffer[(int)InputBytes.InnerWheel])
            {
                var eventArgs = new ShuttleEventArgs();

                if (amountTurned > 0)
                    eventArgs.Direction = ShuttleEventArgs.DirectionTurned.Right;
                else
                    eventArgs.Direction = ShuttleEventArgs.DirectionTurned.Left;

                eventArgs.AmountTurned = amountTurned;
                InnerJogWheelTurned?.Invoke(this, eventArgs);
            }
        }

        #endregion Event raisers
    }
}