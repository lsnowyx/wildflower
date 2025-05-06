namespace wildflower
{
    public partial class Playlists : Form
    {
        private int deleteClickCounter = 0;
        private string playlistsDir;
        private string currentPlayListNr;
        public string Playlist2Play = "-1";
        private static Image ResizeImage(Image img, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, width, height);
            }
            return bmp;
        }
        public Playlists(string playlistsDir, string currentPlayListNr)
        {
            InitializeComponent();
            CenterToScreen();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.playlistsDir = playlistsDir;
            this.currentPlayListNr = currentPlayListNr;
            Playlist2Play = currentPlayListNr;
            btn_PlayPlaylist.Image = ResizeImage(Image.FromFile("icons\\iconPlayButton.png"), 50, 50);
            btn_delPlaylist.Image = ResizeImage(Image.FromFile("icons\\iconDeletePlaylist.png"), 50, 50);
        }
        private void btn_PlayPlaylist_Click(object sender, EventArgs e)
        {
            if (this.track_list.SelectedItem == null) return;

            string musicFolder = track_list.SelectedItem.ToString();


            foreach (string dir in Directory.GetDirectories(playlistsDir))
            {
                string existingPathFile = Path.Combine(dir, "musicFolderPath.txt");
                if (File.Exists(existingPathFile))
                {
                    string existingPath = File.ReadAllText(existingPathFile);
                    if (string.Equals(existingPath, musicFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        Playlist2Play = Path.GetFileName(dir);
                        this.Close();
                    }
                }
            }
        }
        private void btn_delPlaylist_Click(object sender, EventArgs e)
        {
            deleteClickCounter++;
            if (deleteClickCounter > 2)
            {
                deleteClickCounter = 0;
                MessageBox.Show("Delete works only on doubleclick");
            }
        }
        private void btn_delPlaylist_DoubleClick(object sender, EventArgs e)
        {
            if (this.track_list.SelectedItem == null) return;

            string musicFolder = track_list.SelectedItem.ToString();


            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete playlist ${musicFolder}?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (result == DialogResult.No)
            {
                return;
            }


            foreach (string dir in Directory.GetDirectories(playlistsDir))
            {
                string existingPathFile = Path.Combine(dir, "musicFolderPath.txt");
                if (File.Exists(existingPathFile))
                {
                    string existingPath = File.ReadAllText(existingPathFile);
                    if (string.Equals(existingPath, musicFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Path.GetFileName(dir) == currentPlayListNr)
                        {
                            Playlist2Play = "0";
                        }
                        Directory.Delete(dir, recursive: true);
                        track_list.Items.Remove(track_list.SelectedItem);
                        if (track_list.Items.Count == 0)
                        {
                            this.Close();
                        }
                    }
                }
            }
            deleteClickCounter = 0;
        }
        private void Playlists_Load(object sender, EventArgs e)
        {
            int i = 0;
            foreach (string dir in Directory.GetDirectories(playlistsDir))
            {
                if (File.Exists(Path.Combine(dir, "musicFolderPath.txt")))
                {
                    string playlistPath = File.ReadAllText(Path.Combine(dir, "musicFolderPath.txt"));
                    this.track_list.Items.Add(playlistPath);
                    if (Path.GetFileName(dir) == currentPlayListNr)
                    {
                        track_list.SelectedIndex = i;
                    }
                    i++;
                }
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space)
            {
                btn_PlayPlaylist_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}