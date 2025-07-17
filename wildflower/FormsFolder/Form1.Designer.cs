namespace wildflower
{
    partial class Form1
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
            components = new System.ComponentModel.Container();
            p_bar = new ColoredProgressBar();
            track_list = new ListBox();
            track_volume = new TrackBar();
            lbl_volume = new Label();
            lbl_track_start = new Label();
            lbl_track_end = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            btn_play_pause = new PictureBox();
            btn_nextTrack = new PictureBox();
            btn_prevTrack = new PictureBox();
            btn_loopTrack = new PictureBox();
            btn_shuffleTrack = new PictureBox();
            btn_options = new PictureBox();
            stateTimer = new System.Windows.Forms.Timer(components);
            lbl_tempSongName = new Label();
            btn_goBack = new PictureBox();
            mainPanel = new Panel();
            lbl_loadingtxt = new Label();
            btn_fullSongName = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)track_volume).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_play_pause).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_nextTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_prevTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_loopTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_shuffleTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_options).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_goBack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_fullSongName).BeginInit();
            SuspendLayout();
            // 
            // p_bar
            // 
            p_bar.Dock = DockStyle.Bottom;
            p_bar.Location = new Point(0, 395);
            p_bar.Margin = new Padding(6);
            p_bar.Name = "p_bar";
            p_bar.Size = new Size(641, 49);
            p_bar.TabIndex = 6;
            p_bar.MouseDown += p_bar_MouseDown;
            p_bar.MouseLeave += p_bar_MouseLeave;
            p_bar.MouseMove += p_bar_MouseMove;
            // 
            // track_list
            // 
            track_list.BackColor = Color.Black;
            track_list.BorderStyle = BorderStyle.None;
            track_list.ForeColor = Color.White;
            track_list.FormattingEnabled = true;
            track_list.Location = new Point(303, 26);
            track_list.Margin = new Padding(6);
            track_list.Name = "track_list";
            track_list.Size = new Size(321, 352);
            track_list.TabIndex = 7;
            track_list.SelectedIndexChanged += track_list_SelectedIndexChanged;
            track_list.MouseDoubleClick += track_list_MouseDoubleClick;
            // 
            // track_volume
            // 
            track_volume.Location = new Point(0, 4);
            track_volume.Margin = new Padding(6);
            track_volume.Maximum = 100;
            track_volume.Name = "track_volume";
            track_volume.Orientation = Orientation.Vertical;
            track_volume.Size = new Size(90, 222);
            track_volume.TabIndex = 8;
            track_volume.TickStyle = TickStyle.TopLeft;
            track_volume.Scroll += track_volume_Scroll;
            // 
            // lbl_volume
            // 
            lbl_volume.AutoSize = true;
            lbl_volume.Location = new Point(0, 224);
            lbl_volume.Margin = new Padding(6, 0, 6, 0);
            lbl_volume.Name = "lbl_volume";
            lbl_volume.Size = new Size(95, 32);
            lbl_volume.TabIndex = 10;
            lbl_volume.Text = "Volume";
            // 
            // lbl_track_start
            // 
            lbl_track_start.AutoSize = true;
            lbl_track_start.Location = new Point(0, 363);
            lbl_track_start.Margin = new Padding(6, 0, 6, 0);
            lbl_track_start.Name = "lbl_track_start";
            lbl_track_start.Size = new Size(71, 32);
            lbl_track_start.TabIndex = 11;
            lbl_track_start.Text = "00:00";
            // 
            // lbl_track_end
            // 
            lbl_track_end.AutoSize = true;
            lbl_track_end.Location = new Point(74, 363);
            lbl_track_end.Margin = new Padding(6, 0, 6, 0);
            lbl_track_end.Name = "lbl_track_end";
            lbl_track_end.Size = new Size(71, 32);
            lbl_track_end.TabIndex = 12;
            lbl_track_end.Text = "00:00";
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // btn_play_pause
            // 
            btn_play_pause.Location = new Point(199, 151);
            btn_play_pause.Margin = new Padding(6);
            btn_play_pause.Name = "btn_play_pause";
            btn_play_pause.Size = new Size(93, 107);
            btn_play_pause.TabIndex = 15;
            btn_play_pause.TabStop = false;
            btn_play_pause.Click += btn_play_pause_Click;
            // 
            // btn_nextTrack
            // 
            btn_nextTrack.Location = new Point(199, 271);
            btn_nextTrack.Margin = new Padding(6);
            btn_nextTrack.Name = "btn_nextTrack";
            btn_nextTrack.Size = new Size(93, 107);
            btn_nextTrack.TabIndex = 16;
            btn_nextTrack.TabStop = false;
            btn_nextTrack.Click += btn_nextTrack_Click;
            // 
            // btn_prevTrack
            // 
            btn_prevTrack.Location = new Point(199, 32);
            btn_prevTrack.Margin = new Padding(6);
            btn_prevTrack.Name = "btn_prevTrack";
            btn_prevTrack.Size = new Size(93, 107);
            btn_prevTrack.TabIndex = 17;
            btn_prevTrack.TabStop = false;
            btn_prevTrack.Click += btn_prevTrack_Click;
            // 
            // btn_loopTrack
            // 
            btn_loopTrack.Location = new Point(95, 32);
            btn_loopTrack.Margin = new Padding(6);
            btn_loopTrack.Name = "btn_loopTrack";
            btn_loopTrack.Size = new Size(93, 107);
            btn_loopTrack.TabIndex = 18;
            btn_loopTrack.TabStop = false;
            btn_loopTrack.Click += btn_loopTrack_Click;
            // 
            // btn_shuffleTrack
            // 
            btn_shuffleTrack.Location = new Point(95, 151);
            btn_shuffleTrack.Margin = new Padding(6);
            btn_shuffleTrack.Name = "btn_shuffleTrack";
            btn_shuffleTrack.Size = new Size(93, 107);
            btn_shuffleTrack.TabIndex = 19;
            btn_shuffleTrack.TabStop = false;
            btn_shuffleTrack.Click += btn_shuffleTrack_Click;
            btn_shuffleTrack.DoubleClick += btn_shuffleTrack_DoubleClick;
            // 
            // btn_options
            // 
            btn_options.Location = new Point(95, 271);
            btn_options.Margin = new Padding(6);
            btn_options.Name = "btn_options";
            btn_options.Size = new Size(93, 107);
            btn_options.TabIndex = 21;
            btn_options.TabStop = false;
            btn_options.Click += btn_options_Click;
            // 
            // stateTimer
            // 
            stateTimer.Interval = 30000;
            stateTimer.Tick += stateTimer_Tick;
            // 
            // lbl_tempSongName
            // 
            lbl_tempSongName.Location = new Point(292, 64);
            lbl_tempSongName.Margin = new Padding(4, 0, 4, 0);
            lbl_tempSongName.Name = "lbl_tempSongName";
            lbl_tempSongName.Size = new Size(251, 213);
            lbl_tempSongName.TabIndex = 22;
            lbl_tempSongName.Text = "tempSongName";
            lbl_tempSongName.Visible = false;
            // 
            // btn_goBack
            // 
            btn_goBack.Enabled = false;
            btn_goBack.Location = new Point(407, 151);
            btn_goBack.Margin = new Padding(6);
            btn_goBack.Name = "btn_goBack";
            btn_goBack.Size = new Size(93, 107);
            btn_goBack.TabIndex = 23;
            btn_goBack.TabStop = false;
            btn_goBack.Visible = false;
            btn_goBack.Click += btn_goBack_Click;
            // 
            // mainPanel
            // 
            mainPanel.Enabled = false;
            mainPanel.Location = new Point(323, 271);
            mainPanel.Margin = new Padding(6);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(59, 107);
            mainPanel.TabIndex = 24;
            mainPanel.Visible = false;
            // 
            // lbl_loadingtxt
            // 
            lbl_loadingtxt.AutoSize = true;
            lbl_loadingtxt.Location = new Point(407, 113);
            lbl_loadingtxt.Margin = new Padding(6, 0, 6, 0);
            lbl_loadingtxt.Name = "lbl_loadingtxt";
            lbl_loadingtxt.Size = new Size(114, 32);
            lbl_loadingtxt.TabIndex = 25;
            lbl_loadingtxt.Text = "Loading...";
            lbl_loadingtxt.Visible = false;
            // 
            // btn_fullSongName
            // 
            btn_fullSongName.Location = new Point(0, 262);
            btn_fullSongName.Margin = new Padding(6);
            btn_fullSongName.Name = "btn_fullSongName";
            btn_fullSongName.Size = new Size(93, 107);
            btn_fullSongName.TabIndex = 26;
            btn_fullSongName.TabStop = false;
            btn_fullSongName.Visible = false;
            btn_fullSongName.Click += btn_fullSongName_Click;
            btn_fullSongName.MouseLeave += btn_fullSongName_MouseLeave;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(641, 444);
            Controls.Add(btn_fullSongName);
            Controls.Add(lbl_loadingtxt);
            Controls.Add(mainPanel);
            Controls.Add(btn_goBack);
            Controls.Add(lbl_tempSongName);
            Controls.Add(btn_options);
            Controls.Add(btn_shuffleTrack);
            Controls.Add(btn_loopTrack);
            Controls.Add(btn_prevTrack);
            Controls.Add(btn_nextTrack);
            Controls.Add(btn_play_pause);
            Controls.Add(lbl_track_end);
            Controls.Add(lbl_track_start);
            Controls.Add(lbl_volume);
            Controls.Add(track_volume);
            Controls.Add(track_list);
            Controls.Add(p_bar);
            ForeColor = Color.White;
            Margin = new Padding(6);
            Name = "Form1";
            Text = "wildflower";
            Load += Form1_Load;
            Click += Form1_Click;
            ((System.ComponentModel.ISupportInitialize)track_volume).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_play_pause).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_nextTrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_prevTrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_loopTrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_shuffleTrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_options).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_goBack).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_fullSongName).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ColoredProgressBar p_bar;
        private ListBox track_list;
        private TrackBar track_volume;
        private Label lbl_volume;
        private Label lbl_track_start;
        private Label lbl_track_end;
        private System.Windows.Forms.Timer timer1;
        private PictureBox btn_play_pause;
        private PictureBox btn_nextTrack;
        private PictureBox btn_prevTrack;
        private PictureBox btn_loopTrack;
        private PictureBox btn_shuffleTrack;
        private PictureBox btn_options;
        private System.Windows.Forms.Timer stateTimer;
        private Label lbl_tempSongName;
        private PictureBox btn_goBack;
        private Panel mainPanel;
        private Label lbl_loadingtxt;
        private PictureBox btn_fullSongName;
    }
}
