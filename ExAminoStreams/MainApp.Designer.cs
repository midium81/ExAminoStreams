namespace ExAminoStreams
{
    partial class MainApp
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btGetStreams = new Button();
            label1 = new Label();
            txtFixtureID = new TextBox();
            label2 = new Label();
            label3 = new Label();
            btPlayStream = new Button();
            DXGrid = new DevExpress.XtraGrid.GridControl();
            dgView = new DevExpress.XtraGrid.Views.Grid.GridView();
            pnlVideoPlayer = new Panel();
            btNextFrame = new Button();
            btPrevFrame = new Button();
            btForward = new Button();
            btBackward = new Button();
            btPlayPause = new Button();
            trkVideoTime = new TrackBar();
            btMute = new Button();
            lblTime = new Label();
            ((System.ComponentModel.ISupportInitialize)DXGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkVideoTime).BeginInit();
            SuspendLayout();
            // 
            // btGetStreams
            // 
            btGetStreams.Location = new Point(12, 55);
            btGetStreams.Name = "btGetStreams";
            btGetStreams.Size = new Size(378, 23);
            btGetStreams.TabIndex = 1;
            btGetStreams.Text = "Get Streams";
            btGetStreams.UseVisualStyleBackColor = true;
            btGetStreams.Click += btGetStreams_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 8);
            label1.Name = "label1";
            label1.Size = new Size(66, 15);
            label1.TabIndex = 2;
            label1.Text = "Fixture ID:";
            // 
            // txtFixtureID
            // 
            txtFixtureID.Location = new Point(12, 26);
            txtFixtureID.Name = "txtFixtureID";
            txtFixtureID.Size = new Size(378, 23);
            txtFixtureID.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(396, 9);
            label2.Name = "label2";
            label2.Size = new Size(42, 15);
            label2.TabIndex = 4;
            label2.Text = "Video:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(12, 94);
            label3.Name = "label3";
            label3.Size = new Size(107, 15);
            label3.TabIndex = 5;
            label3.Text = "Available streams:";
            // 
            // btPlayStream
            // 
            btPlayStream.Location = new Point(12, 407);
            btPlayStream.Name = "btPlayStream";
            btPlayStream.Size = new Size(378, 31);
            btPlayStream.TabIndex = 7;
            btPlayStream.Text = "Play Selected Stream";
            btPlayStream.UseVisualStyleBackColor = true;
            btPlayStream.Click += btPlayStream_Click;
            // 
            // DXGrid
            // 
            DXGrid.Location = new Point(12, 112);
            DXGrid.LookAndFeel.UseWindowsXPTheme = true;
            DXGrid.MainView = dgView;
            DXGrid.Name = "DXGrid";
            DXGrid.Size = new Size(378, 289);
            DXGrid.TabIndex = 0;
            DXGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { dgView });
            // 
            // dgView
            // 
            dgView.GridControl = DXGrid;
            dgView.Name = "dgView";
            // 
            // pnlVideoPlayer
            // 
            pnlVideoPlayer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlVideoPlayer.BackColor = Color.Black;
            pnlVideoPlayer.Location = new Point(396, 26);
            pnlVideoPlayer.Name = "pnlVideoPlayer";
            pnlVideoPlayer.Size = new Size(392, 349);
            pnlVideoPlayer.TabIndex = 8;
            // 
            // btNextFrame
            // 
            btNextFrame.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btNextFrame.Location = new Point(741, 407);
            btNextFrame.Name = "btNextFrame";
            btNextFrame.Size = new Size(47, 31);
            btNextFrame.TabIndex = 9;
            btNextFrame.Text = ">|";
            btNextFrame.UseVisualStyleBackColor = true;
            btNextFrame.Click += btNextFrame_Click;
            // 
            // btPrevFrame
            // 
            btPrevFrame.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btPrevFrame.Location = new Point(396, 407);
            btPrevFrame.Name = "btPrevFrame";
            btPrevFrame.Size = new Size(47, 31);
            btPrevFrame.TabIndex = 10;
            btPrevFrame.Text = "|<";
            btPrevFrame.UseVisualStyleBackColor = true;
            btPrevFrame.Click += btPrevFrame_Click;
            // 
            // btForward
            // 
            btForward.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btForward.Location = new Point(688, 407);
            btForward.Name = "btForward";
            btForward.Size = new Size(47, 31);
            btForward.TabIndex = 11;
            btForward.Text = ">>";
            btForward.UseVisualStyleBackColor = true;
            btForward.Click += btForward_Click;
            // 
            // btBackward
            // 
            btBackward.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btBackward.Location = new Point(449, 407);
            btBackward.Name = "btBackward";
            btBackward.Size = new Size(47, 31);
            btBackward.TabIndex = 12;
            btBackward.Text = "<<";
            btBackward.UseVisualStyleBackColor = true;
            btBackward.Click += btBackward_Click;
            // 
            // btPlayPause
            // 
            btPlayPause.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btPlayPause.Location = new Point(570, 407);
            btPlayPause.Name = "btPlayPause";
            btPlayPause.Size = new Size(47, 31);
            btPlayPause.TabIndex = 13;
            btPlayPause.Text = ">";
            btPlayPause.UseVisualStyleBackColor = true;
            btPlayPause.Click += btPlayPause_Click;
            // 
            // trkVideoTime
            // 
            trkVideoTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trkVideoTime.AutoSize = false;
            trkVideoTime.Enabled = false;
            trkVideoTime.Location = new Point(396, 381);
            trkVideoTime.Name = "trkVideoTime";
            trkVideoTime.Size = new Size(392, 20);
            trkVideoTime.TabIndex = 14;
            trkVideoTime.TickStyle = TickStyle.None;
            trkVideoTime.Scroll += OnValueChanged;
            // 
            // btMute
            // 
            btMute.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btMute.Location = new Point(517, 407);
            btMute.Name = "btMute";
            btMute.Size = new Size(47, 31);
            btMute.TabIndex = 15;
            btMute.Text = "Mute";
            btMute.UseVisualStyleBackColor = true;
            btMute.Click += btMute_Click;
            // 
            // lblTime
            // 
            lblTime.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblTime.AutoSize = true;
            lblTime.Location = new Point(626, 415);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(49, 15);
            lblTime.TabIndex = 16;
            lblTime.Text = "00:00:00";
            // 
            // MainApp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblTime);
            Controls.Add(btMute);
            Controls.Add(trkVideoTime);
            Controls.Add(btPlayPause);
            Controls.Add(btBackward);
            Controls.Add(btForward);
            Controls.Add(btPrevFrame);
            Controls.Add(btNextFrame);
            Controls.Add(pnlVideoPlayer);
            Controls.Add(btPlayStream);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtFixtureID);
            Controls.Add(label1);
            Controls.Add(btGetStreams);
            Controls.Add(DXGrid);
            MinimizeBox = false;
            Name = "MainApp";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ExAmino / Flow Streams POC";
            ((System.ComponentModel.ISupportInitialize)DXGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgView).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkVideoTime).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btGetStreams;
        private Label label1;
        private TextBox txtFixtureID;
        private Label label2;
        private Label label3;
        private Button btPlayStream;
        private DevExpress.XtraGrid.GridControl DXGrid;
        private DevExpress.XtraGrid.Views.Grid.GridView dgView;
        private Panel pnlVideoPlayer;
        private Button btNextFrame;
        private Button btPrevFrame;
        private Button btForward;
        private Button btBackward;
        private Button btPlayPause;
        private TrackBar trkVideoTime;
        private Button btMute;
        private Label lblTime;
    }
}
