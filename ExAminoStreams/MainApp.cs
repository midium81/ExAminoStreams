using DevExpress.Xpo.Logger;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using ExAminoStreams.API;
using SingleExePOC;
using StatsPerform.CollectionClients.Utilities.IO.Devices.ShuttleXPress;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using static DevExpress.XtraEditors.RoundedSkinPanel;
using MessageBox = System.Windows.MessageBox;

namespace ExAminoStreams
{
    public partial class MainApp : Form
    {
        #region JogShuttle
        private ShuttleXPress _usbShuttle;

        private ShuttleXPress UsbShuttle // Module level Shuttle Xpress object
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _usbShuttle;

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_usbShuttle is not null)
                {
                    _usbShuttle.JogWheelTurned -= OnJogWheelTurned;
                    _usbShuttle.InnerJogWheelTurned -= OnInnerJogWheelTurned;
                    _usbShuttle.Button2Pressed -= OnButtonPressed;
                    _usbShuttle.Button3Pressed -= OnButtonPressed;
                }

                _usbShuttle = value;
                if (_usbShuttle is not null)
                {
                    _usbShuttle.JogWheelTurned += OnJogWheelTurned;
                    _usbShuttle.InnerJogWheelTurned += OnInnerJogWheelTurned;
                    _usbShuttle.Button2Pressed += OnButtonPressed;
                    _usbShuttle.Button3Pressed += OnButtonPressed;
                }
            }
        }

        public void AttachJogShuttle(ShuttleXPress usbShuttle)
        {
            UsbShuttle = usbShuttle;
        }

        public void DetachJogShuttle()
        {
            if (_usbShuttle is not null)
            {
                _usbShuttle.JogWheelTurned -= OnJogWheelTurned;
                _usbShuttle.InnerJogWheelTurned -= OnInnerJogWheelTurned;
                _usbShuttle.Button2Pressed -= OnButtonPressed;
                _usbShuttle.Button3Pressed -= OnButtonPressed;
            }
            UsbShuttle = null;
        }

        #region USB Shuttle Handlers

        private void OnJogWheelTurned(object sender, ShuttleEventArgs e)
        {
            if (InvokeIfRequired(new ShuttleXPress.JogWheelTurnedEventHandler(OnJogWheelTurned), sender, e))
                return;

            if (e.Direction == ShuttleEventArgs.DirectionTurned.Left)
                RewindVideo(e.AmountTurned);
            else
                ForwardVideo(e.AmountTurned);
        }

        private void OnInnerJogWheelTurned(object sender, ShuttleEventArgs e)
        {
            if (InvokeIfRequired(new ShuttleXPress.JogWheelTurnedEventHandler(OnInnerJogWheelTurned), sender, e))
                return;

            if (e.Direction == ShuttleEventArgs.DirectionTurned.Left)
                RewindVideoSmall(e.AmountTurned);
            else
                ForwardVideoSmall(e.AmountTurned);
        }

        private void OnButtonPressed(object sender, EventArgs e)
        {
            if (InvokeIfRequired(new EventHandler(OnButtonPressed), sender, e))
                return;

            TogglePlayPause();
        }

        #endregion USB Shuttle

        #region Video methods

        private bool IsPlaying = false;

        public virtual void RewindVideoSmall(int amount)
        {
            Console.Write($"VIDEO SMALL MOVEMENT BACKWARDS BY {amount}");
            RewindVideo(amount);
        }

        public virtual void PauseVideo()
        {
            try
            {
                // Stop playback of video file
                if (_video is not null)
                    _video.StopPlayback();
                IsPlaying = false;

                UpdateButtonsStatus();

            }
            catch (Exception ex)
            {
                throw new Exception("The following error occurred while attempting to pause the video file:" + Environment.NewLine + ex.Message, ex);
            }
        }

        public virtual void PlayVideo()
        {
            try
            {
                // Start playback of video file
                if (_video is not null)
                {
                    _video.StartPlayback();
                    IsPlaying = true;

                    UpdateButtonsStatus();

                }

            }
            catch (Exception ex)
            {
                throw new Exception("The following error occurred while attempting to play video file:" + Environment.NewLine + ex.Message, ex);
            }
        }

        private delegate void TogglePlayPauseDelegate();
        public void TogglePlayPause()
        {

            if (InvokeIfRequired(new TogglePlayPauseDelegate(TogglePlayPause)))
                return;

            if (IsPlaying)
                PauseVideo();
            else
                PlayVideo();
        }

        public virtual void RewindVideo(int amount)
        {
            try
            {
                IsPlaying = false;

                // Rewind video file
                _video?.BackwardSeek(amount);

                UpdateButtonsStatus();


            }
            catch (Exception ex)
            {
                Console.WriteLine("The following error occurred while rewinding video file:" + ex.Message);
            }
        }

        public virtual void ForwardVideo(int amount)
        {
            try
            {
                IsPlaying = false;

                // Seek forward in video file
                _video?.ForwardSeek(amount);

                UpdateButtonsStatus();

            }
            catch (Exception ex)
            {
                Console.WriteLine("The following error occurred while seeking forward in video file:" + ex.Message);
            }
        }

        private void UpdateButtonsStatus()
        {
            if (IsPlaying)
                btPlayPause.Text = "||";
            else
                btPlayPause.Text = ">";
        }

        public virtual void ForwardVideoSmall(int amount)
        {
            Console.Write($"VIDEO SMALL MOVEMENT FORWARD BY {amount}");
            ForwardVideo(-amount);
        }
        #endregion
        #endregion

        private Form _owner;

        public bool InvokeIfRequired(Delegate action, params object[] args)
        {
            if (_owner is not null && !_owner.IsDisposed && !_owner.Disposing && _owner.InvokeRequired)
            {
                _owner.Invoke(action, args);
                return true;
            }

            return false;
        }

        public MainApp()
        {

            InitializeComponent();
            _owner = this;
            InitializeGrid();

            _usbShuttle = new ShuttleXPress();

            _usbShuttle.ScanForDevice();
            AttachJogShuttle(_usbShuttle);

        }

        private void btGetStreams_Click(object sender, EventArgs e)
        {
            var fixtureId = txtFixtureID.Text;

            if (string.IsNullOrEmpty(fixtureId))
                MessageBox.Show("Please provide a FixtureId first from: https://examino.statsperform.io/");

            Cursor = Cursors.WaitCursor;
            var api = new FlowAPI();
            var streams = AsyncUtil.RunSync(() => api.GetStreams(fixtureId));

            if (streams is not null)
                FillUpStreams(streams);

            Cursor = null;
        }

        private void FillUpStreams(Streams streams)
        {
            DXGrid.DataSource = streams.AvailableStreams;
        }

        #region Grid

        public const string TitleColumn = "Title";
        public const string PlatformColumn = "Platform";
        public const string ProviderColumn = "Provider";
        public const string StreamColumn = "Stream";
        public const string ResolutionColumn = "Resolution";

        private void InitializeGrid()
        {
            dgView.RowHeight = 24;

            dgView.BorderStyle = BorderStyles.Simple;
            dgView.PaintStyleName = "Flat";

            dgView.Appearance.Empty.BackColor = Color.FromArgb(64, 64, 64);
            dgView.Appearance.FocusedRow.BackColor = Color.LightBlue;
            dgView.Appearance.FocusedRow.ForeColor = Color.Black;
            dgView.Appearance.Row.BackColor = Color.White;
            dgView.Appearance.RowSeparator.BackColor = Color.FromArgb(64, 64, 64);
            dgView.Appearance.HeaderPanel.Font = new Font(dgView.Appearance.HeaderPanel.Font, System.Drawing.FontStyle.Bold);

            dgView.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            dgView.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            dgView.OptionsBehavior.AutoPopulateColumns = false;
            dgView.OptionsBehavior.CacheValuesOnRowUpdating = DevExpress.Data.CacheRowValuesMode.Disabled;
            dgView.OptionsBehavior.KeepFocusedRowOnUpdate = true;

            dgView.OptionsCustomization.AllowColumnMoving = false;
            dgView.OptionsCustomization.AllowColumnResizing = false;
            dgView.OptionsCustomization.AllowFilter = false;
            dgView.OptionsCustomization.AllowGroup = false;
            dgView.OptionsCustomization.AllowRowSizing = false;
            dgView.OptionsCustomization.AllowSort = false;
            dgView.OptionsCustomization.AllowQuickHideColumns = false;

            dgView.OptionsDetail.EnableMasterViewMode = false;
            dgView.OptionsDetail.ShowDetailTabs = false;

            dgView.OptionsMenu.EnableGroupPanelMenu = false;
            dgView.OptionsMenu.EnableColumnMenu = false;
            dgView.OptionsMenu.EnableFooterMenu = false;

            dgView.OptionsSelection.MultiSelect = false;
            dgView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;

            dgView.OptionsView.ShowFilterPanelMode = ShowFilterPanelMode.Never;
            dgView.OptionsView.ShowGroupPanel = false;
            dgView.OptionsView.ShowIndicator = false;
            dgView.OptionsView.ShowDetailButtons = false;

            dgView.HorzScrollVisibility = ScrollVisibility.Never;
            dgView.LevelIndent = 20;

            dgView.Columns.Clear();
            AddTitleColumn();
            AddStreamColumn();
            AddResolutionColumn();
            AddProviderColumn();
            AddPlatformColumn();

            dgView.CustomUnboundColumnData += OnCustomUnboundColumnData;
            dgView.MouseUp += OnMouseUp;
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DevExpress.XtraGrid.Views.Grid.GridView gridView = (DevExpress.XtraGrid.Views.Grid.GridView)sender;
                if (gridView is null)
                    return;

                var hi = gridView.CalcHitInfo(gridView.GridControl.PointToClient(Cursor.Position));

                if (gridView.IsDataRow(hi.RowHandle) && gridView.SelectedRowsCount <= 1)
                {
                    var row = (StreamData)gridView.GetRow(hi.RowHandle);

                    if (row is null)
                        return;

                    var cm = new ContextMenuStrip();
                    ToolStripMenuItem cmItemEventDetails = new("Copy stream url");
                    cmItemEventDetails.Click += (sender, e) => CopyUrl(row.Source);
                    cm.Items.Add(cmItemEventDetails);

                    gridView.GridControl.ContextMenuStrip = cm;
                    gridView.GridControl.ContextMenuStrip.Show(Cursor.Position);

                }
            }

        }

        private void CopyUrl(string source)
        {
            Clipboard.SetText(source);
            MessageBox.Show("Source copied to clipboard");
        }

        public GridColumn AddTitleColumn()
        {
            var col = new GridColumn
            {
                FieldName = TitleColumn,
                Name = TitleColumn,
                Caption = TitleColumn,

                UnboundType = DevExpress.Data.UnboundColumnType.String,

                Width = 120
            };
            col.OptionsColumn.FixedWidth = false;
            col.OptionsColumn.AllowSize = false;
            col.OptionsColumn.ShowCaption = true;
            col.OptionsFilter.AllowFilter = false;
            col.OptionsFilter.AllowAutoFilter = false;
            col.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            col.OptionsColumn.AllowMove = false;
            col.OptionsColumn.AllowFocus = false;

            dgView.Columns.Add(col);
            col.VisibleIndex = dgView.Columns.Count - 1;
            return col;
        }

        public GridColumn AddPlatformColumn()
        {
            var col = new GridColumn
            {
                FieldName = PlatformColumn,
                Name = PlatformColumn,
                Caption = PlatformColumn,

                UnboundType = DevExpress.Data.UnboundColumnType.String,

                Width = 50
            };
            col.OptionsColumn.FixedWidth = false;
            col.OptionsColumn.AllowSize = false;
            col.OptionsColumn.ShowCaption = true;
            col.OptionsFilter.AllowFilter = false;
            col.OptionsFilter.AllowAutoFilter = false;
            col.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            col.OptionsColumn.AllowMove = false;
            col.OptionsColumn.AllowFocus = false;

            dgView.Columns.Add(col);
            col.VisibleIndex = dgView.Columns.Count - 1;
            return col;
        }

        public GridColumn AddStreamColumn()
        {
            var col = new GridColumn
            {
                FieldName = StreamColumn,
                Name = StreamColumn,
                Caption = StreamColumn,

                UnboundType = DevExpress.Data.UnboundColumnType.String,

                Width = 50
            };
            col.OptionsColumn.FixedWidth = false;
            col.OptionsColumn.AllowSize = false;
            col.OptionsColumn.ShowCaption = true;
            col.OptionsFilter.AllowFilter = false;
            col.OptionsFilter.AllowAutoFilter = false;
            col.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            col.OptionsColumn.AllowMove = false;
            col.OptionsColumn.AllowFocus = false;

            dgView.Columns.Add(col);
            col.VisibleIndex = dgView.Columns.Count - 1;
            return col;
        }

        public GridColumn AddResolutionColumn()
        {
            var col = new GridColumn
            {
                FieldName = ResolutionColumn,
                Name = ResolutionColumn,
                Caption = ResolutionColumn,

                UnboundType = DevExpress.Data.UnboundColumnType.String,

                Width = 50
            };
            col.OptionsColumn.FixedWidth = false;
            col.OptionsColumn.AllowSize = false;
            col.OptionsColumn.ShowCaption = true;
            col.OptionsFilter.AllowFilter = false;
            col.OptionsFilter.AllowAutoFilter = false;
            col.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            col.OptionsColumn.AllowMove = false;
            col.OptionsColumn.AllowFocus = false;

            dgView.Columns.Add(col);
            col.VisibleIndex = dgView.Columns.Count - 1;
            return col;
        }

        public GridColumn AddProviderColumn()
        {
            var col = new GridColumn
            {
                FieldName = ProviderColumn,
                Name = ProviderColumn,
                Caption = ProviderColumn,

                UnboundType = DevExpress.Data.UnboundColumnType.String,

                Width = 50
            };
            col.OptionsColumn.FixedWidth = false;
            col.OptionsColumn.AllowSize = false;
            col.OptionsColumn.ShowCaption = true;
            col.OptionsFilter.AllowFilter = false;
            col.OptionsFilter.AllowAutoFilter = false;
            col.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            col.OptionsColumn.AllowMove = false;
            col.OptionsColumn.AllowFocus = false;

            dgView.Columns.Add(col);
            col.VisibleIndex = dgView.Columns.Count - 1;
            return col;
        }

        private void OnCustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
        {
            if (!e.IsGetData)
                return;

            if (e.Row is StreamData data)
            {
                switch (e.Column.FieldName ?? "")
                {
                    case TitleColumn:
                        {
                            e.Value = data.Title;
                            break;
                        }
                    case ResolutionColumn:
                        {
                            if (data.VideoInfo is not null)
                                e.Value = $"{data.VideoInfo.Width} x {data.VideoInfo.Height}";
                            else
                                e.Value = "Unknown";
                            break;
                        }
                    case StreamColumn:
                        {
                            e.Value = data.Source;
                            break;
                        }
                    case PlatformColumn:
                        {
                            e.Value = data.Platform;
                            break;
                        }
                    case ProviderColumn:
                        {
                            e.Value = data.Provider;
                            break;
                        }
                }
            }
        }
        #endregion Grid

        #region Video
        private LibVLCVideo _video;

        private void PlayStream(string streamUri)
        {
            if (_video is not null)
                _video.Dispose();

            Cursor = Cursors.WaitCursor;

            _video = new LibVLCVideo(pnlVideoPlayer);
            _video.OpenStream(streamUri, false);
            _video.StartPlayback();
            _video.VideoLoaded += OnVideoLoaded;
            _video.TimeElapsed += OnTimeElapsed;

            Cursor = null;

        }

        private void OnTimeElapsed(object? sender, EventArgs e)
        {
            lblTime.Text = _video.Position.TotalWholeMilliseconds().ToString();

        }

        private void OnVideoLoaded(object? sender, EventArgs e)
        {
            trkVideoTime.Maximum = _video.Length.TotalWholeMilliseconds();
            trkVideoTime.SmallChange = 100; // 100 milliseconds...
            trkVideoTime.LargeChange = 60000; // 1 minute
            trkVideoTime.Enabled = true;

            IsPlaying = true;

            UpdateButtonsStatus();
        }

        #endregion

        private void btPlayStream_Click(object sender, EventArgs e)
        {
            var row = dgView.GetRow(dgView.GetSelectedRows()[0]);
            if (row is StreamData streamData)
                PlayStream(streamData.Source);
        }

        private void btNextFrame_Click(object sender, EventArgs e)
        {
            ForwardVideoSmall(1);
        }

        private void btPrevFrame_Click(object sender, EventArgs e)
        {
            RewindVideoSmall(1);
        }

        private void btForward_Click(object sender, EventArgs e)
        {
            ForwardVideo(10);
        }

        private void btBackward_Click(object sender, EventArgs e)
        {
            RewindVideo(10);
        }

        private void btPlayPause_Click(object sender, EventArgs e)
        {
            TogglePlayPause();
            UpdateButtonsStatus();
        }

        protected void MoveToPosition(TimeSpan position)
        {
            int milliseconds = position.TotalWholeMilliseconds();
            if (milliseconds <= trkVideoTime.Maximum & milliseconds >= trkVideoTime.Minimum)
            {
                // Do not seek to "end of video" as it makes problems
                if (milliseconds != 0 && milliseconds == trkVideoTime.Maximum)
                    milliseconds -= 1;

                MillisecondsScroll(milliseconds);
                trkVideoTime.Value = milliseconds;
            }

        }

        public virtual void Seek(TimeSpan position)
        {
            MoveToPosition(position);
        }

        protected virtual void MillisecondsScroll(int milliseconds)
        {
            try
            {
                Console.WriteLine($"{DateTime.UtcNow:o} MILLISECONDS SCROLL {trkVideoTime.Value}");

                // Seek to specified value of progress bar in video file
                _video.SeekMs(milliseconds);

            }
            catch (Exception ex)
            {
                Console.WriteLine("The following error occurred while seeking through video file:" + ex.Message);
            }
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            MillisecondsScroll(trkVideoTime.Value);
        }

        private bool _isMute = false;

        private void btMute_Click(object sender, EventArgs e)
        {
            _isMute = !_isMute;
            if(_isMute)
                _video.Volume = 0;
            else
                _video.Volume = 50;
        }
    }
}
