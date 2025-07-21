namespace wildflower
{
    partial class Options
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
            btn_open = new PictureBox();
            btn_update = new PictureBox();
            btn_searchTrack = new PictureBox();
            btn_selectPlaylist = new PictureBox();
            btn_openSaveFolder = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)btn_open).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_update).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_searchTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_selectPlaylist).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_openSaveFolder).BeginInit();
            SuspendLayout();
            // 
            // btn_open
            // 
            btn_open.Location = new Point(113, 26);
            btn_open.Margin = new Padding(6);
            btn_open.Name = "btn_open";
            btn_open.Size = new Size(93, 107);
            btn_open.TabIndex = 22;
            btn_open.TabStop = false;
            btn_open.Click += btn_open_Click;
            // 
            // btn_update
            // 
            btn_update.Location = new Point(217, 26);
            btn_update.Margin = new Padding(6);
            btn_update.Name = "btn_update";
            btn_update.Size = new Size(93, 107);
            btn_update.TabIndex = 23;
            btn_update.TabStop = false;
            btn_update.Click += btn_update_Click;
            // 
            // btn_searchTrack
            // 
            btn_searchTrack.Location = new Point(427, 26);
            btn_searchTrack.Margin = new Padding(6);
            btn_searchTrack.Name = "btn_searchTrack";
            btn_searchTrack.Size = new Size(93, 107);
            btn_searchTrack.TabIndex = 24;
            btn_searchTrack.TabStop = false;
            btn_searchTrack.Click += btn_searchTrack_Click;
            // 
            // btn_selectPlaylist
            // 
            btn_selectPlaylist.Location = new Point(9, 26);
            btn_selectPlaylist.Margin = new Padding(6);
            btn_selectPlaylist.Name = "btn_selectPlaylist";
            btn_selectPlaylist.Size = new Size(93, 107);
            btn_selectPlaylist.TabIndex = 25;
            btn_selectPlaylist.TabStop = false;
            btn_selectPlaylist.Click += btn_selectPlaylist_Click;
            // 
            // btn_openSaveFolder
            // 
            btn_openSaveFolder.Location = new Point(322, 26);
            btn_openSaveFolder.Margin = new Padding(6);
            btn_openSaveFolder.Name = "btn_openSaveFolder";
            btn_openSaveFolder.Size = new Size(93, 107);
            btn_openSaveFolder.TabIndex = 26;
            btn_openSaveFolder.TabStop = false;
            btn_openSaveFolder.Click += btn_openSaveFolder_Click;
            // 
            // Options
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(530, 160);
            Controls.Add(btn_openSaveFolder);
            Controls.Add(btn_selectPlaylist);
            Controls.Add(btn_searchTrack);
            Controls.Add(btn_update);
            Controls.Add(btn_open);
            Margin = new Padding(6);
            Name = "Options";
            Text = "Options";
            Load += Options_Load;
            ((System.ComponentModel.ISupportInitialize)btn_open).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_update).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_searchTrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_selectPlaylist).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_openSaveFolder).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox btn_open;
        private PictureBox btn_update;
        private PictureBox btn_searchTrack;
        private PictureBox btn_selectPlaylist;
        private PictureBox btn_openSaveFolder;
    }
}