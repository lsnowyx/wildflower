using wildflower.Models;

namespace wildflower
{
    public partial class Playlists : Form
    {
        private int deleteClickCounter = 0;
        private List<PlaylistInfo> playlists;
        private string currentPlayListNr;

        public event EventHandler<string>? Playlist2Play;
        public event EventHandler<string>? PlaylistDeleteRequested;
        public event EventHandler? CloseRequest;

        public Playlists(IEnumerable<PlaylistInfo> playlists, string currentPlayListNr)
        {
            InitializeComponent();
            this.playlists = playlists.ToList();
            this.currentPlayListNr = currentPlayListNr;
            btn_PlayPlaylist.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconPlayButton.png"), btn_PlayPlaylist.Width, btn_PlayPlaylist.Height);
            btn_delPlaylist.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconDeletePlaylist.png"), btn_delPlaylist.Width, btn_delPlaylist.Height);
        }

        public void SetPlaylists(IEnumerable<PlaylistInfo> playlists, string currentPlayListNr)
        {
            this.playlists = playlists.ToList();
            this.currentPlayListNr = currentPlayListNr;
            RenderPlaylists();
        }

        private void btn_PlayPlaylist_Click(object sender, EventArgs e)
        {
            if (track_list.SelectedIndex < 0 || track_list.SelectedIndex >= playlists.Count) return;

            PlaylistInfo playlist = playlists[track_list.SelectedIndex];
            Playlist2Play?.Invoke(this, playlist.Id);
            CloseRequest?.Invoke(this, EventArgs.Empty);
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
            if (track_list.SelectedIndex < 0 || track_list.SelectedIndex >= playlists.Count) return;

            PlaylistInfo playlist = playlists[track_list.SelectedIndex];
            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete playlist {playlist.MusicFolderPath}?",
                "wildflower Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.No)
                return;

            PlaylistDeleteRequested?.Invoke(this, playlist.Id);
            deleteClickCounter = 0;
        }

        private void Playlists_Load(object sender, EventArgs e)
        {
            track_list.BackColor = BackColor;
            RenderPlaylists();
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

        private void RenderPlaylists()
        {
            track_list.Items.Clear();

            int selectedIndex = -1;
            for (int i = 0; i < playlists.Count; i++)
            {
                PlaylistInfo playlist = playlists[i];
                track_list.Items.Add(playlist.MusicFolderPath);
                if (playlist.Id == currentPlayListNr)
                    selectedIndex = i;
            }

            if (selectedIndex >= 0 && selectedIndex < track_list.Items.Count)
                track_list.SelectedIndex = selectedIndex;
        }
    }
}
