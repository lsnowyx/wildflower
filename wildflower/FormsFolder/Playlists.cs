namespace wildflower
{
    public partial class Playlists : Form
    {
        private int deleteClickCounter = 0;
        private string playlistsDir;
        private string currentPlayListNr;
        public event EventHandler<string> Playlist2Play;
        public event EventHandler CloseRequest;
        public Playlists(string playlistsDir, string currentPlayListNr)
        {
            InitializeComponent();
            this.playlistsDir = playlistsDir;
            this.currentPlayListNr = currentPlayListNr;
            btn_PlayPlaylist.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconPlayButton.png"), btn_PlayPlaylist.Width, btn_PlayPlaylist.Height);
            btn_delPlaylist.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconDeletePlaylist.png"), btn_delPlaylist.Width, btn_delPlaylist.Height);
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
                        Playlist2Play?.Invoke(this, Path.GetFileName(dir));
                        CloseRequest?.Invoke(this, EventArgs.Empty);
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
                MessageBox.Show("Delete works only on doubleclick", "wildflower", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btn_delPlaylist_DoubleClick(object sender, EventArgs e)
        {
            if (this.track_list.SelectedItem == null) return;

            string musicFolder = track_list.SelectedItem.ToString();


            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete playlist ${musicFolder}?",
                "wildflower Confirm Deletion",
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
                        Directory.Delete(dir, recursive: true);
                        track_list.Items.Remove(track_list.SelectedItem);
                        if (track_list.Items.Count == 0)
                        {
                            CloseRequest?.Invoke(this, EventArgs.Empty);
                        }
                        if (Path.GetFileName(dir) == currentPlayListNr)
                        {
                            Playlist2Play?.Invoke(this, "-1");
                        }
                    }
                }
            }
            deleteClickCounter = 0;
        }
        private void Playlists_Load(object sender, EventArgs e)
        {
            track_list.BackColor = this.BackColor;
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
            track_list.Focus();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                btn_PlayPlaylist_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.Escape)
            {
                if (Helper.IsAnimatingButton || Helper.IsAnimatingPanel) return true;
                CloseRequest?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}