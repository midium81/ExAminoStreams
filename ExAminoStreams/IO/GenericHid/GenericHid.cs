using DevExpress.Utils.MVVM.Services;
using HidLibrary;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

namespace StatsPerform.CollectionClients.Utilities.IO.GenericHid
{
    public class GenericHid : IDisposable
    {
        protected HidDevice _hidDevice;

        private bool _scanStopped = false;
        private bool _deviceDetected = false;

        // Remember if already mentioned no jog shuttle
        private bool _loggedMissingDevice = false;

        protected byte[] _currentBuffer;

        private readonly Timer _eventTimer;

        private Timer _deviceScanTimer;

        public event EventHandler DeviceRemoved;

        public event EventHandler DeviceAttached;

        public GenericHid(int vendorId, int productId)
        {
            _eventTimer = new Timer() { Interval = 100d };
            _eventTimer.Elapsed += OnEventTimerElapsed;

            _deviceScanTimer = new Timer() { AutoReset = true };
            _deviceScanTimer.Elapsed += OnDeviceScanTimerElapsed;

            _vendorId = vendorId;
            _productId = productId;
        }

        #region Device IO

        /// <summary>
        /// Start searching for device, keeps searching until found
        /// </summary>
        /// <remarks></remarks>
        public void ScanForDevice()
        {
            StartScan();
        }

        #endregion Device IO

        #region Events

        /// <summary>
        /// Stops scanning for device and notifies client that device was attached
        /// </summary>
        protected virtual void OnDeviceAttached()
        {
            _scanStopped = true;
            _deviceScanTimer.Stop();
            DeviceAttached?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Timer callback routine that tries to find a device.
        /// If device is found initializes device and starts asynchronous reading of device input
        /// </summary>
        private void OnDeviceScanTimerElapsed(object sender, EventArgs e)
        {
            if (_scanStopped)
                return;

            if (FindTheHid())
            {
                OnDeviceAttached();
                _scanStopped = true;
                _eventTimer.Start();
            }
        }

        private void OnEventTimerElapsed(object sender, ElapsedEventArgs e)
        {
            HandleInput();
        }

        /// <summary>
        /// Stops current device, notifies client of device removal and starts scanning for device attachment
        /// </summary>
        protected virtual void OnDevicedRemoved()
        {
            CloseCommunications();
            DeviceRemoved?.Invoke(this, new EventArgs());
            StartScan();
        }

        #endregion Events

        #region Helpers

        private void StartScan()
        {
            _scanStopped = false;
            _deviceScanTimer.Interval = DeviceScanInterval;
            _deviceScanTimer.Start();
        }

        /// <summary>
        /// Close the handle and FileStreams for a device.
        /// </summary>
        private void CloseCommunications()
        {
            _hidDevice?.CloseDevice();

            // The next attempt to communicate will get new handles and FileStreams.
            _deviceDetected = false;
        }

        /// <summary>
        /// Uses a series of API calls to locate a HID-class device
        /// by its Vendor ID and Product ID.
        /// </summary>
        /// <returns>
        /// True if the device is detected, False if not detected.
        /// </returns>
        private bool FindTheHid()
        {
            try
            {
                _deviceDetected = false;

                // Searching within the HID devices the one with given vendor and product id
                _hidDevice = HidDevices.Enumerate().FirstOrDefault(hid => hid.Attributes.VendorId == VendorId && hid.Attributes.ProductId == ProductId);

                if (_hidDevice is not null)
                {
                    _hidDevice.OpenDevice();

                    if (_hidDevice.IsOpen)
                    {
                        // The device was detected and connection is open
                        // Register to receive notifications if the device is removed or attached.
                        _deviceDetected = true;

                        _hidDevice.Removed += OnDevicedRemoved;

                        var ReportDevice = Task.Run(async () => await ReadReportAsync());
                        ReportDevice.ContinueWith(t => OnReadReport(ReportDevice.Result));
                        //_hidDevice.MonitorDeviceEvents = true;
                        //_hidDevice.ReadReport(OnReadReport);
                    }
                }

                // The device wasn't detected.
                else if (!_loggedMissingDevice)
                {
                    _loggedMissingDevice = true;
                }

                return _deviceDetected;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<HidReport> ReadReportAsync()
        {
            return await Task.Run(() => _hidDevice.ReadReport());
        }

        /// <summary>
        /// Handles input bytes and raises relevant event for each input
        /// </summary>
        protected virtual void HandleInput()
        {
            var inputAsString = new StringBuilder();

            if (_currentBuffer is null
                || _currentBuffer.Length == 0)
                return;
            for (int count = 0, loopTo = _currentBuffer.Length - 1; count <= loopTo; count++)
                // Display bytes as 2-character Hex strings.
                inputAsString.Append($"{_currentBuffer[count]:X2} ");

        }

        private void OnReadReport(HidReport report)
        {
            _currentBuffer = report?.Data;

            if (_deviceDetected
                && _hidDevice?.IsOpen == true)
            {
                var ReportDevice = Task.Run(async () => await ReadReportAsync());
                ReportDevice.ContinueWith(t => OnReadReport(ReportDevice.Result));
            }
            //_hidDevice.ReadReport(OnReadReport);
        }

        #endregion Helpers

        #region Properties

        private readonly int _vendorId;

        public int VendorId => _vendorId;

        private readonly int _productId;

        public int ProductId => _productId;

        public int DeviceScanInterval { get; set; } = 5000;

        #endregion Properties

        #region IDisposable methods

        public void Dispose()
        {
            try
            {
                _eventTimer.Stop();

                _deviceScanTimer.Stop();
                _deviceScanTimer.Dispose();
                _deviceScanTimer = null;

                _hidDevice?.CloseDevice();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion IDisposable methods
    }
}