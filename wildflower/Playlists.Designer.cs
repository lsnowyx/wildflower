namespace wildflower
{
    partial class Playlists
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
            btn_PlayPlaylist = new PictureBox();
            track_list = new ListBox();
            btn_delPlaylist = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)btn_PlayPlaylist).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_delPlaylist).BeginInit();
            SuspendLayout();
            // 
            // btn_PlayPlaylist
            // 
            btn_PlayPlaylist.Location = new Point(65, 12);
            btn_PlayPlaylist.Name = "btn_PlayPlaylist";
            btn_PlayPlaylist.Size = new Size(50, 50);
            btn_PlayPlaylist.TabIndex = 24;
            btn_PlayPlaylist.TabStop = false;
            btn_PlayPlaylist.Click += btn_PlayPlaylist_Click;
            // 
            // track_list
            // 
            track_list.BackColor = Color.Black;
            track_list.BorderStyle = BorderStyle.None;
            track_list.ForeColor = Color.White;
            track_list.FormattingEnabled = true;
            track_list.ItemHeight = 15;
            track_list.Location = new Point(121, 12);
            track_list.Name = "track_list";
            track_list.Size = new Size(173, 60);
            track_list.TabIndex = 23;
            // 
            // btn_delPlaylist
            // 
            btn_delPlaylist.Location = new Point(9, 12);
            btn_delPlaylist.Name = "btn_delPlaylist";
            btn_delPlaylist.Size = new Size(50, 50);
            btn_delPlaylist.TabIndex = 22;
            btn_delPlaylist.TabStop = false;
            btn_delPlaylist.Click += btn_delPlaylist_Click;
            btn_delPlaylist.DoubleClick += btn_delPlaylist_DoubleClick;
            // 
            // Playlists
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(302, 80);
            Controls.Add(btn_PlayPlaylist);
            Controls.Add(track_list);
            Controls.Add(btn_delPlaylist);
            Name = "Playlists";
            Text = "Playlists";
            Load += Playlists_Load;
            ((System.ComponentModel.ISupportInitialize)btn_PlayPlaylist).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_delPlaylist).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox btn_PlayPlaylist;
        private ListBox track_list;
        private PictureBox btn_delPlaylist;
    }
}