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
            btn_preview = new Button();
            btn_next = new Button();
            btn_play = new Button();
            btn_pause = new Button();
            btn_stop = new Button();
            btn_open = new Button();
            p_bar = new ProgressBar();
            track_list = new ListBox();
            track_volume = new TrackBar();
            lbl_volume = new Label();
            lbl_track_start = new Label();
            lbl_track_end = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            btn_shuffle = new Button();
            btn_loop = new Button();
            ((System.ComponentModel.ISupportInitialize)track_volume).BeginInit();
            SuspendLayout();
            // 
            // btn_preview
            // 
            btn_preview.FlatStyle = FlatStyle.Flat;
            btn_preview.Location = new Point(10, 208);
            btn_preview.Name = "btn_preview";
            btn_preview.Size = new Size(75, 23);
            btn_preview.TabIndex = 0;
            btn_preview.Text = "Preview";
            btn_preview.UseVisualStyleBackColor = true;
            btn_preview.Click += btn_preview_Click;
            // 
            // btn_next
            // 
            btn_next.FlatStyle = FlatStyle.Flat;
            btn_next.Location = new Point(91, 208);
            btn_next.Name = "btn_next";
            btn_next.Size = new Size(75, 23);
            btn_next.TabIndex = 1;
            btn_next.Text = "Next";
            btn_next.UseVisualStyleBackColor = true;
            btn_next.Click += btn_next_Click;
            // 
            // btn_play
            // 
            btn_play.FlatStyle = FlatStyle.Flat;
            btn_play.Location = new Point(172, 208);
            btn_play.Name = "btn_play";
            btn_play.Size = new Size(75, 23);
            btn_play.TabIndex = 2;
            btn_play.Text = "Play";
            btn_play.UseVisualStyleBackColor = true;
            btn_play.Click += btn_play_Click;
            // 
            // btn_pause
            // 
            btn_pause.FlatStyle = FlatStyle.Flat;
            btn_pause.Location = new Point(253, 208);
            btn_pause.Name = "btn_pause";
            btn_pause.Size = new Size(75, 23);
            btn_pause.TabIndex = 3;
            btn_pause.Text = "Pause";
            btn_pause.UseVisualStyleBackColor = true;
            btn_pause.Click += btn_pause_Click;
            // 
            // btn_stop
            // 
            btn_stop.FlatStyle = FlatStyle.Flat;
            btn_stop.Location = new Point(334, 208);
            btn_stop.Name = "btn_stop";
            btn_stop.Size = new Size(75, 23);
            btn_stop.TabIndex = 4;
            btn_stop.Text = "Stop";
            btn_stop.UseVisualStyleBackColor = true;
            btn_stop.Click += btn_stop_Click;
            // 
            // btn_open
            // 
            btn_open.FlatStyle = FlatStyle.Flat;
            btn_open.Location = new Point(415, 208);
            btn_open.Name = "btn_open";
            btn_open.Size = new Size(75, 23);
            btn_open.TabIndex = 5;
            btn_open.Text = "Open";
            btn_open.UseVisualStyleBackColor = true;
            btn_open.Click += btn_open_Click;
            // 
            // p_bar
            // 
            p_bar.Location = new Point(10, 179);
            p_bar.Name = "p_bar";
            p_bar.Size = new Size(478, 23);
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
            track_list.Location = new Point(10, 79);
            track_list.Name = "track_list";
            track_list.Size = new Size(478, 90);
            track_list.TabIndex = 7;
            track_list.SelectedIndexChanged += track_list_SelectedIndexChanged;
            // 
            // track_volume
            // 
            track_volume.Location = new Point(50, 28);
            track_volume.Maximum = 100;
            track_volume.Name = "track_volume";
            track_volume.Size = new Size(104, 45);
            track_volume.TabIndex = 8;
            track_volume.TickStyle = TickStyle.TopLeft;
            track_volume.Scroll += track_volume_Scroll;
            // 
            // lbl_volume
            // 
            lbl_volume.AutoSize = true;
            lbl_volume.Location = new Point(82, 10);
            lbl_volume.Name = "lbl_volume";
            lbl_volume.Size = new Size(47, 15);
            lbl_volume.TabIndex = 10;
            lbl_volume.Text = "Volume";
            // 
            // lbl_track_start
            // 
            lbl_track_start.AutoSize = true;
            lbl_track_start.Location = new Point(10, 45);
            lbl_track_start.Name = "lbl_track_start";
            lbl_track_start.Size = new Size(34, 15);
            lbl_track_start.TabIndex = 11;
            lbl_track_start.Text = "00:00";
            // 
            // lbl_track_end
            // 
            lbl_track_end.AutoSize = true;
            lbl_track_end.Location = new Point(160, 45);
            lbl_track_end.Name = "lbl_track_end";
            lbl_track_end.Size = new Size(34, 15);
            lbl_track_end.TabIndex = 12;
            lbl_track_end.Text = "00:00";
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;
            // 
            // btn_shuffle
            // 
            btn_shuffle.FlatStyle = FlatStyle.Flat;
            btn_shuffle.Location = new Point(261, 41);
            btn_shuffle.Name = "btn_shuffle";
            btn_shuffle.Size = new Size(75, 23);
            btn_shuffle.TabIndex = 13;
            btn_shuffle.Text = "Shuffle";
            btn_shuffle.UseVisualStyleBackColor = true;
            btn_shuffle.Click += btn_shuffle_Click;
            // 
            // btn_loop
            // 
            btn_loop.FlatStyle = FlatStyle.Flat;
            btn_loop.Location = new Point(352, 41);
            btn_loop.Name = "btn_loop";
            btn_loop.Size = new Size(75, 23);
            btn_loop.TabIndex = 14;
            btn_loop.Text = "Loop";
            btn_loop.UseVisualStyleBackColor = true;
            btn_loop.Click += btn_loop_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(502, 244);
            Controls.Add(btn_loop);
            Controls.Add(btn_shuffle);
            Controls.Add(lbl_track_end);
            Controls.Add(lbl_track_start);
            Controls.Add(lbl_volume);
            Controls.Add(track_volume);
            Controls.Add(track_list);
            Controls.Add(p_bar);
            Controls.Add(btn_open);
            Controls.Add(btn_stop);
            Controls.Add(btn_pause);
            Controls.Add(btn_play);
            Controls.Add(btn_next);
            Controls.Add(btn_preview);
            ForeColor = Color.White;
            Name = "Form1";
            Text = "wildflower";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)track_volume).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_preview;
        private Button btn_next;
        private Button btn_play;
        private Button btn_pause;
        private Button btn_stop;
        private Button btn_open;
        private ProgressBar p_bar;
        private ListBox track_list;
        private TrackBar track_volume;
        private Label lbl_volume;
        private Label lbl_track_start;
        private Label lbl_track_end;
        private System.Windows.Forms.Timer timer1;
        private Button btn_shuffle;
        private Button btn_loop;
    }
}
