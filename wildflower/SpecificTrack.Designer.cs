namespace wildflower
{
    partial class SpecificTrack
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txbx_searchTrack = new TextBox();
            track_list = new ListBox();
            btn_searchTracks = new PictureBox();
            btn_play_pause = new PictureBox();
            btn_goBack = new PictureBox();
            progressBar1 = new ProgressBar();
            lbl_track_start = new Label();
            lbl_track_end = new Label();
            ((System.ComponentModel.ISupportInitialize)btn_searchTracks).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_play_pause).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_goBack).BeginInit();
            SuspendLayout();
            // 
            // txbx_searchTrack
            // 
            txbx_searchTrack.BackColor = Color.Gray;
            txbx_searchTrack.BorderStyle = BorderStyle.None;
            txbx_searchTrack.Location = new Point(84, 10);
            txbx_searchTrack.Name = "txbx_searchTrack";
            txbx_searchTrack.Size = new Size(173, 16);
            txbx_searchTrack.TabIndex = 0;
            // 
            // track_list
            // 
            track_list.BackColor = Color.Black;
            track_list.BorderStyle = BorderStyle.None;
            track_list.ForeColor = Color.White;
            track_list.FormattingEnabled = true;
            track_list.ItemHeight = 15;
            track_list.Location = new Point(84, 38);
            track_list.Name = "track_list";
            track_list.Size = new Size(173, 180);
            track_list.TabIndex = 8;
            // 
            // btn_searchTracks
            // 
            btn_searchTracks.Location = new Point(4, 12);
            btn_searchTracks.Name = "btn_searchTracks";
            btn_searchTracks.Size = new Size(50, 50);
            btn_searchTracks.TabIndex = 9;
            btn_searchTracks.TabStop = false;
            btn_searchTracks.Click += btn_searchTracks_Click;
            // 
            // btn_play_pause
            // 
            btn_play_pause.Location = new Point(4, 68);
            btn_play_pause.Name = "btn_play_pause";
            btn_play_pause.Size = new Size(50, 50);
            btn_play_pause.TabIndex = 10;
            btn_play_pause.TabStop = false;
            btn_play_pause.Click += btn_play_pause_Click;
            // 
            // btn_goBack
            // 
            btn_goBack.Location = new Point(4, 124);
            btn_goBack.Name = "btn_goBack";
            btn_goBack.Size = new Size(50, 50);
            btn_goBack.TabIndex = 11;
            btn_goBack.TabStop = false;
            btn_goBack.Click += btn_goBack_Click;
            // 
            // progressBar1
            // 
            progressBar1.Dock = DockStyle.Bottom;
            progressBar1.Location = new Point(0, 224);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(262, 23);
            progressBar1.TabIndex = 12;
            // 
            // lbl_track_start
            // 
            lbl_track_start.AutoSize = true;
            lbl_track_start.ForeColor = Color.White;
            lbl_track_start.Location = new Point(4, 206);
            lbl_track_start.Name = "lbl_track_start";
            lbl_track_start.Size = new Size(34, 15);
            lbl_track_start.TabIndex = 13;
            lbl_track_start.Text = "00:00";
            // 
            // lbl_track_end
            // 
            lbl_track_end.AutoSize = true;
            lbl_track_end.ForeColor = Color.White;
            lbl_track_end.Location = new Point(44, 206);
            lbl_track_end.Name = "lbl_track_end";
            lbl_track_end.Size = new Size(34, 15);
            lbl_track_end.TabIndex = 14;
            lbl_track_end.Text = "00:00";
            // 
            // SpecificTrack
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(262, 247);
            Controls.Add(lbl_track_end);
            Controls.Add(lbl_track_start);
            Controls.Add(progressBar1);
            Controls.Add(btn_goBack);
            Controls.Add(btn_play_pause);
            Controls.Add(btn_searchTracks);
            Controls.Add(track_list);
            Controls.Add(txbx_searchTrack);
            Name = "SpecificTrack";
            Text = "SearchTrack";
            ((System.ComponentModel.ISupportInitialize)btn_searchTracks).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_play_pause).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_goBack).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txbx_searchTrack;
        private ListBox track_list;
        private PictureBox btn_searchTracks;
        private PictureBox btn_play_pause;
        private PictureBox btn_goBack;
        private ProgressBar progressBar1;
        private Label lbl_track_start;
        private Label lbl_track_end;
    }
}