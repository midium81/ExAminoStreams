using DevExpress.Office.NumberConverters;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace SingleExePOC
{
    public class LibVLCVideo : IDisposable
    {
        protected LibVLC _libVLC;
        protected MediaPlayer _mp;

        protected System.Timers.Timer _playbackTimer = new System.Timers.Timer();

        protected const double UI_REFRESH_INTERVAL = 100;

        public Size OriginalVideoSize
        {
            get
            {
                if (_mp.Media.Tracks.Any(x => x.TrackType == TrackType.Video))
                {
                    var videoTrack = _mp.Media.Tracks.FirstOrDefault(x => x.TrackType == TrackType.Video);
                    return new Size((int)videoTrack.Data.Video.Width,
                            (int)videoTrack.Data.Video.Height);
                }

                return new Size(0, 0);
            }
        }

        public TimeSpan Length => TimeSpan.FromMilliseconds(_mp?.Length ?? -1);

        public TimeSpan Position => TimeSpan.FromMilliseconds(Math.Ceiling(_mp.Position * _mp.Length));

        protected int _volume = 100;
        private readonly Panel _playbackContainer;

        public int Volume
        {
            get => _volume;
            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                _volume = value;
                _mp.Volume = _volume;
            }
        }

        public bool IsFinished => Position >= Length;

        public event EventHandler TimeElapsed;

        public event EventHandler VideoLoaded;

        public event EventHandler<string> VideoLoadError;

        public event EventHandler<string> VideoFatalPlaybackError;

        public LibVLCVideo(Panel panel)
        {
            Core.Initialize();

            var videoView = new VideoView
            {
                BackColor = Color.Black,
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                MediaPlayer = null,
                Name = "videoView1",
                Size = new Size(800, 450),
                TabIndex = 0
            };

            _playbackContainer = panel;
            panel.Controls.Clear();
            panel.Controls.Add(videoView);

            _libVLC = new LibVLC(new[]
            {
                "--input-repeat=5",
                "--network-caching=8000",
                "--directx-use-sysmem",
                "--avcodec-hw=none",
                "--adaptive-logic=highest",
                /*"--prefetch-buffer-size=50000"/*,
                "--prefetch-read-size=5000000",
                "--prefetch-seek-threshold=5000000"*/
            });
            _libVLC.Log += _libVLC_Log;

            _mp = new MediaPlayer(_libVLC);
            _mp.EnableKeyInput = false;
            _mp.EnableMouseInput = false;
            _mp.EncounteredError += _mp_EncounteredError;
            _mp.Buffering += _mp_Buffering;

            videoView.MediaPlayer = _mp;

            _playbackTimer.Interval = UI_REFRESH_INTERVAL;
            _playbackTimer.Elapsed += _playbackTimer_Elapsed;

            // LengthChanged is the last event run by LibVLC
            // while asynchronously loading video,
            // after that we're ready to run anything
            _mp.LengthChanged += _mp_LengthChanged;
        }

        public bool IsFirstBufferingCompleted { get; internal set; }
        bool buffering = false;

        private void _mp_Buffering(object sender, MediaPlayerBufferingEventArgs e)
        {
            if (e.Cache == 0f)
                buffering = true;
            else if (e.Cache == 100f)
            {
                buffering = false;
                if (!IsFirstBufferingCompleted)
                    IsFirstBufferingCompleted = true;

            }
            Console.WriteLine($"[VLC_VIDEO_BUFFERING] {DateTime.UtcNow:o} BUFFERING {buffering} / {e.Cache} {Position}");
            //NLogManager.Trace($"[VLC_VIDEO_BUFFERING] {DateTime.UtcNow:o} BUFFERING {buffering} / {e.Cache} {Position}", _localLogger);
        }

        private void _mp_EncounteredError(object sender, EventArgs e)
        {
            Console.WriteLine($"[VLC_VIDEO_ERROR] {DateTime.UtcNow:o} ENCOUNTERED ERROR");
            //NLogManager.Error($"[VLC_VIDEO_ERROR] {DateTime.UtcNow:o} ENCOUNTERED ERROR", _localLogger, _cloudLogger);
        }

        private readonly List<string> _fatalErrors = new List<string>() {
            "buffer deadlock prevented",
            "cannot get packet header, track disabled",
            "all tracks have failed, exiting..."
        };

        // Raise the error once
        private bool _fatalVideoErrorRaised = false;

        private void _libVLC_Log(object sender, LogEventArgs e)
        {
            if (e.Level == LibVLCSharp.Shared.LogLevel.Error
                || e.Level == LibVLCSharp.Shared.LogLevel.Warning)
            {

                bool fatalError = _fatalErrors.Any(err => e.Message.Contains(err));

                if (fatalError && !_fatalVideoErrorRaised)
                {
                    _playbackContainer.BeginInvoke(new Action(() => VideoFatalPlaybackError?.Invoke(this, e.Message)));
                    _fatalVideoErrorRaised = true;
                }
            }
            else
            {
                Console.WriteLine($"[VLC_VIDEO_TRACE] {e.Level} {e.Module} {e.Message}");
                //NLogManager.Trace($"[VLC_VIDEO_TRACE] {e.Level} {e.Module} {e.Message}", _localLogger);
            }
        }

        private bool _loadHandlerFired = false;
        private bool _isStreamingVideo = false;

        private void _mp_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            // run once
            if (!_loadHandlerFired)
            {
                _loadHandlerFired = true;

                if (e.Length <= 0 || (!_mp.Media.Tracks.Any(t => t.TrackType == TrackType.Video) && !_isStreamingVideo))
                {
                    // Workaround for loading audio files - they would still be played
                    _mp.Mute = true;
                    _playbackContainer.BeginInvoke((Action)(() => _mp.Stop()));

                    _playbackContainer.BeginInvoke((Action)(() => VideoLoadError?.Invoke(this, "Invalid video file format")));
                }
                else
                {
                    _playbackTimer.Start();
                    _playbackContainer.BeginInvoke((Action)(() => VideoLoaded?.Invoke(this, new EventArgs())));
                }
            }
        }

        private void _playbackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!_playbackContainer.IsDisposed && !_playbackContainer.Disposing && _playbackContainer.InvokeRequired)
                {
                    _playbackContainer.BeginInvoke(new Action<object, ElapsedEventArgs>(_playbackTimer_Elapsed), this, e);
                    return;
                }

                TimeElapsed?.Invoke(this, new EventArgs());
            }
            catch (ObjectDisposedException)
            {
                ; // may happen when closing application
            }
            catch (InvalidAsynchronousStateException)
            {
                ; // may happen when closing application
            }
        }

        public void BackwardSeek(int amount)
        {
            Console.WriteLine($"[VLC_VIDEO_BACKWARD] {DateTime.UtcNow:o} BACKWARD SEEK {amount}");

            if (_mp.IsPlaying)
                _mp.SetPause(true);

            if (buffering && _mp.Position < 1.0f)
            {
                Console.WriteLine($"[VLC_VIDEO_CANCELLING] {DateTime.UtcNow:o} CANCELLING ACTION - STILL BUFFERING");
                return;
            }

            float ms;
            if (amount < 0)
                ms = 1.0f / _mp.Fps * 1000 * (-amount);
            else
                ms = amount * 100f; // 0.1s per amount

            float newPosition = _mp.Position -= ms / _mp.Length;
            if (newPosition < 0.0f)
                newPosition = 0.0f;
            _mp.Position = newPosition;            
        }

        public void ForwardSeek(int amount)
        {
            Console.WriteLine($"[VLC_VIDEO_FORWARD] {DateTime.UtcNow:o} FORWARD SEEK {amount}");
            if (_mp.IsPlaying)
                _mp.SetPause(true);

            if (buffering && _mp.Position < 1.0f)
            {
                Console.WriteLine($"[VLC_VIDEO_CANCELLING] {DateTime.UtcNow:o} CANCELLING ACTION - STILL BUFFERING");
                return;
            }

            if (amount < 0)
            {
                int max = -amount;
                for (int i = 0; i < max; i++)
                    _mp.NextFrame();
            }
            else
            {
                float ms = amount * 100f; // 0.1s per amount
                float newPosition = _mp.Position += ms / _mp.Length;
                if (newPosition > 1.0f)
                    newPosition = 1.0f;

                _mp.Position = newPosition;
            }
        }

        public async void OpenStream(string streamUri, bool mute)
        {
            _loadHandlerFired = false;
            _isStreamingVideo = true;

            var media = new Media(_libVLC, new Uri(streamUri));
            await media.Parse(MediaParseOptions.ParseNetwork);

            if (mute)
                _mp.Volume = 0;

            _mp.Play(media);
            _playbackTimer.Start();
        }


        public void OpenFile(string filename, bool mute)
        {
            _loadHandlerFired = false;

            var media = new Media(_libVLC, filename, FromType.FromPath);

            if (mute)
                _mp.Volume = 0;

            _mp.Play(media);
            _playbackTimer.Start();
        }

        public void Rewind()
        {
            if (_mp.State == VLCState.Ended)
                _mp.Play(_mp.Media);

            if (_mp.IsPlaying)
                _mp.SetPause(true);
            _mp.Position = 0.0f;
        }

        public void SeekMs(int ms)
        {
            //NLogManager.Trace($"[VLC_VIDEO_SEEK] {DateTime.UtcNow:o} SEEK MS PLAYBACK PLAYING {_mp.IsPlaying} STATE {_mp.State}", _localLogger);
            //NLogManager.Trace($"[VLC_VIDEO_SEEK] {DateTime.UtcNow:o} SEEK MS {ms}", _localLogger);

            if (buffering && _mp.Position < 1.0f)
            {
                //NLogManager.Trace($"[VLC_VIDEO_CANCELLING] {DateTime.UtcNow:o} CANCELLING ACTION - STILL BUFFERING", _localLogger);
                return;
            }

            if (_mp.State == VLCState.Ended)
                _mp.Play(_mp.Media);

            float newPosition = ms / (float)_mp.Length;

            _mp.Position = newPosition;
        }

        public void StartPlayback()
        {
            if (_mp.State == VLCState.Ended)
                _mp.Play(_mp.Media);
            else
                _mp.Play();
        }

        public void StopPlayback()
        {
            if (_mp.State == VLCState.Ended)
                _mp.Play(_mp.Media);

            _mp.SetPause(true);
        }

        public void ToEnd()
        {
            _mp.SetPause(true);
            _mp.Position = 1.0f;
        }

        public void SetScale(float scale)
        {
            _mp.Scale = scale;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // LengthChanged is the last event run by LibVLC
            // while asynchronously loading video,
            // after that we're ready to run anything

            if (_playbackTimer is not null)
            {
                _playbackTimer.Elapsed -= _playbackTimer_Elapsed;
                _playbackTimer.Dispose();
                _playbackTimer = null;
            }

            if (_mp is not null)
            {
                _mp.LengthChanged -= _mp_LengthChanged;
                _mp.EncounteredError -= _mp_EncounteredError;
                _mp.Buffering -= _mp_Buffering;
                _mp.Dispose();
                _mp = null;
            }

            if (_libVLC is not null)
            {
                _libVLC.Log -= _libVLC_Log;
                _libVLC.Dispose();
                _libVLC = null;
            }
        }
    }
}
