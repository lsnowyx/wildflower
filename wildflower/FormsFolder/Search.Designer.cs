namespace wildflower
{
    partial class Search
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
            txbx_search = new TextBox();
            btn_searchTrack = new PictureBox();
            track_list = new ListBox();
            btn_Play = new PictureBox();
            btn_Random = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)btn_searchTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_Play).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_Random).BeginInit();
            SuspendLayout();
            // 
            // txbx_search
            // 
            txbx_search.Location = new Point(6, 13);
            txbx_search.Margin = new Padding(2, 1, 2, 1);
            txbx_search.Name = "txbx_search";
            txbx_search.Size = new Size(80, 23);
            txbx_search.TabIndex = 0;
            // 
            // btn_searchTrack
            // 
            btn_searchTrack.Location = new Point(91, 13);
            btn_searchTrack.Name = "btn_searchTrack";
            btn_searchTrack.Size = new Size(23, 23);
            btn_searchTrack.TabIndex = 18;
            btn_searchTrack.TabStop = false;
            btn_searchTrack.Click += btn_searchTrack_Click;
            // 
            // track_list
            // 
            track_list.BackColor = Color.Black;
            track_list.BorderStyle = BorderStyle.None;
            track_list.ForeColor = Color.White;
            track_list.FormattingEnabled = true;
            track_list.ItemHeight = 15;
            track_list.Location = new Point(124, 17);
            track_list.Name = "track_list";
            track_list.Size = new Size(173, 75);
            track_list.TabIndex = 19;
            // 
            // btn_Play
            // 
            btn_Play.Location = new Point(64, 42);
            btn_Play.Name = "btn_Play";
            btn_Play.Size = new Size(50, 50);
            btn_Play.TabIndex = 20;
            btn_Play.TabStop = false;
            btn_Play.Click += btn_Play_Click;
            // 
            // btn_Random
            // 
            btn_Random.Location = new Point(8, 42);
            btn_Random.Name = "btn_Random";
            btn_Random.Size = new Size(50, 50);
            btn_Random.TabIndex = 21;
            btn_Random.TabStop = false;
            btn_Random.Click += btn_Random_Click;
            // 
            // Search
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(305, 103);
            Controls.Add(btn_Random);
            Controls.Add(btn_Play);
            Controls.Add(track_list);
            Controls.Add(btn_searchTrack);
            Controls.Add(txbx_search);
            Margin = new Padding(2, 1, 2, 1);
            Name = "Search";
            Text = "Search";
            Load += Search_Load;
            ((System.ComponentModel.ISupportInitialize)btn_searchTrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_Play).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_Random).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txbx_search;
        private PictureBox btn_searchTrack;
        private ListBox track_list;
        private PictureBox btn_Play;
        private PictureBox btn_Random;
    }
}