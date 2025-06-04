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
            msgPanel = new Panel();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)track_volume).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_play_pause).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_nextTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_prevTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_loopTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_shuffleTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_options).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_goBack).BeginInit();
            SuspendLayout();
            // 
            // p_bar
            // 
            p_bar.Dock = DockStyle.Bottom;
            p_bar.Location = new Point(0, 185);
            p_bar.Name = "p_bar";
            p_bar.Size = new Size(345, 23);
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
            track_list.ItemHeight = 15;
            track_list.Location = new Point(163, 12);
            track_list.Name = "track_list";
            track_list.Size = new Size(173, 165);
            track_list.TabIndex = 7;
            track_list.MouseDoubleClick += track_list_MouseDoubleClick;
            // 
            // track_volume
            // 
            track_volume.Location = new Point(0, 2);
            track_volume.Maximum = 100;
            track_volume.Name = "track_volume";
            track_volume.Orientation = Orientation.Vertical;
            track_volume.Size = new Size(45, 104);
            track_volume.TabIndex = 8;
            track_volume.TickStyle = TickStyle.TopLeft;
            track_volume.Scroll += track_volume_Scroll;
            // 
            // lbl_volume
            // 
            lbl_volume.AutoSize = true;
            lbl_volume.Location = new Point(0, 105);
            lbl_volume.Name = "lbl_volume";
            lbl_volume.Size = new Size(47, 15);
            lbl_volume.TabIndex = 10;
            lbl_volume.Text = "Volume";
            // 
            // lbl_track_start
            // 
            lbl_track_start.AutoSize = true;
            lbl_track_start.Location = new Point(0, 170);
            lbl_track_start.Name = "lbl_track_start";
            lbl_track_start.Size = new Size(34, 15);
            lbl_track_start.TabIndex = 11;
            lbl_track_start.Text = "00:00";
            // 
            // lbl_track_end
            // 
            lbl_track_end.AutoSize = true;
            lbl_track_end.Location = new Point(40, 170);
            lbl_track_end.Name = "lbl_track_end";
            lbl_track_end.Size = new Size(34, 15);
            lbl_track_end.TabIndex = 12;
            lbl_track_end.Text = "00:00";
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // btn_play_pause
            // 
            btn_play_pause.Location = new Point(107, 71);
            btn_play_pause.Name = "btn_play_pause";
            btn_play_pause.Size = new Size(50, 50);
            btn_play_pause.TabIndex = 15;
            btn_play_pause.TabStop = false;
            btn_play_pause.Click += btn_play_pause_Click;
            // 
            // btn_nextTrack
            // 
            btn_nextTrack.Location = new Point(107, 127);
            btn_nextTrack.Name = "btn_nextTrack";
            btn_nextTrack.Size = new Size(50, 50);
            btn_nextTrack.TabIndex = 16;
            btn_nextTrack.TabStop = false;
            btn_nextTrack.Click += btn_nextTrack_Click;
            // 
            // btn_prevTrack
            // 
            btn_prevTrack.Location = new Point(107, 15);
            btn_prevTrack.Name = "btn_prevTrack";
            btn_prevTrack.Size = new Size(50, 50);
            btn_prevTrack.TabIndex = 17;
            btn_prevTrack.TabStop = false;
            btn_prevTrack.Click += btn_prevTrack_Click;
            // 
            // btn_loopTrack
            // 
            btn_loopTrack.Location = new Point(51, 15);
            btn_loopTrack.Name = "btn_loopTrack";
            btn_loopTrack.Size = new Size(50, 50);
            btn_loopTrack.TabIndex = 18;
            btn_loopTrack.TabStop = false;
            btn_loopTrack.Click += btn_loopTrack_Click;
            // 
            // btn_shuffleTrack
            // 
            btn_shuffleTrack.Location = new Point(51, 71);
            btn_shuffleTrack.Name = "btn_shuffleTrack";
            btn_shuffleTrack.Size = new Size(50, 50);
            btn_shuffleTrack.TabIndex = 19;
            btn_shuffleTrack.TabStop = false;
            btn_shuffleTrack.Click += btn_shuffleTrack_Click;
            btn_shuffleTrack.DoubleClick += btn_shuffleTrack_DoubleClick;
            // 
            // btn_options
            // 
            btn_options.Location = new Point(51, 127);
            btn_options.Name = "btn_options";
            btn_options.Size = new Size(50, 50);
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
            lbl_tempSongName.Location = new Point(157, 30);
            lbl_tempSongName.Margin = new Padding(2, 0, 2, 0);
            lbl_tempSongName.Name = "lbl_tempSongName";
            lbl_tempSongName.Size = new Size(135, 100);
            lbl_tempSongName.TabIndex = 22;
            lbl_tempSongName.Text = "tempSongName";
            lbl_tempSongName.Visible = false;
            // 
            // btn_goBack
            // 
            btn_goBack.Enabled = false;
            btn_goBack.Location = new Point(219, 71);
            btn_goBack.Name = "btn_goBack";
            btn_goBack.Size = new Size(50, 50);
            btn_goBack.TabIndex = 23;
            btn_goBack.TabStop = false;
            btn_goBack.Visible = false;
            btn_goBack.Click += btn_goBack_Click;
            // 
            // mainPanel
            // 
            mainPanel.Enabled = false;
            mainPanel.Location = new Point(174, 127);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(32, 50);
            mainPanel.TabIndex = 24;
            mainPanel.Visible = false;
            // 
            // lbl_loadingtxt
            // 
            lbl_loadingtxt.AutoSize = true;
            lbl_loadingtxt.Location = new Point(219, 53);
            lbl_loadingtxt.Name = "lbl_loadingtxt";
            lbl_loadingtxt.Size = new Size(59, 15);
            lbl_loadingtxt.TabIndex = 25;
            lbl_loadingtxt.Text = "Loading...";
            lbl_loadingtxt.Visible = false;
            // 
            // msgPanel
            // 
            msgPanel.Enabled = false;
            msgPanel.Location = new Point(212, 127);
            msgPanel.Name = "msgPanel";
            msgPanel.Size = new Size(32, 50);
            msgPanel.TabIndex = 25;
            msgPanel.Visible = false;
            // 
            // button1
            // 
            button1.ForeColor = Color.Black;
            button1.Location = new Point(8, 127);
            button1.Name = "button1";
            button1.Size = new Size(37, 23);
            button1.TabIndex = 26;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(345, 208);
            Controls.Add(button1);
            Controls.Add(msgPanel);
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
        private Panel msgPanel;
        private Button button1;
    }
}
