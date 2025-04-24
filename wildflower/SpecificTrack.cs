using Un4seen.Bass;

namespace wildflower
{
    public partial class SpecificTrack : Form
    {
        private string[] paths;
        private int bassStream;
        private bool isPlaying = false;
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
        private void ChangePlayIcon(bool isPlayingg)
        {
            btn_play_pause.Image = isPlayingg ?
                ResizeImage(Image.FromFile("icons\\iconPauseButton.png"), 50, 50) :
                ResizeImage(Image.FromFile("icons\\iconPlayButton.png"), 50, 50);
        }
        public SpecificTrack(string[] paths)
        {
            this.paths = paths;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = new Icon("icons\\wildflowerico.ico");
            btn_searchTracks.Image = ResizeImage(Image.FromFile("icons\\iconFindTrack.png"), 50, 50);
            btn_play_pause.Image = ResizeImage(Image.FromFile("icons\\iconPlayButton.png"), 50, 50);
            btn_goBack.Image = ResizeImage(Image.FromFile("icons\\iconGoBack.png"), 50, 50);
        }
        private void btn_goBack_Click(object sender, EventArgs e) => this.Close();
        private void SearchTracks(string query)
        {
            var matches = paths
                .Where(p => Path.GetFileName(p).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            track_list.Items.Clear();
            foreach (var file in matches)
            {
                track_list.Items.Add(Path.GetFileName(file));
            }

            if (matches.Length == 0)
            {
                track_list.Items.Add("No results found...");
            }
        }
        private void btn_searchTracks_Click(object sender, EventArgs e)
        {
            if (this.txbx_searchTrack.Text != string.Empty)
            {
                SearchTracks(this.txbx_searchTrack.Text);
            }
        }
        private void btn_play_pause_Click(object sender, EventArgs e)
        {
            if (track_list.Items.Count > 0)
            {
                if (isPlaying)
                {
                    Bass.BASS_ChannelPause(bassStream);
                }
                if (!isPlaying)
                {
                    Bass.BASS_ChannelPlay(bassStream, false);
                }
                isPlaying = !isPlaying;
                ChangePlayIcon(isPlaying);
            }
        }
        private void PlayTrack(int index)
        {
            if (paths == null || index < 0 || index >= paths.Length) return;

            string selectedTrackName = track_list.Items[index].ToString();
            string trackPath = paths.FirstOrDefault(p => Path.GetFileName(p) == selectedTrackName);

            if (string.IsNullOrEmpty(trackPath)) return;

            Bass.BASS_ChannelStop(bassStream);
            Bass.BASS_StreamFree(bassStream);

            bassStream = Bass.BASS_StreamCreateFile(trackPath, 0L, 0L, BASSFlag.BASS_DEFAULT);
            if (bassStream != 0)
            {
                Bass.BASS_ChannelSetAttribute(bassStream, BASSAttribute.BASS_ATTRIB_VOL, 1.0f);
                Bass.BASS_ChannelPlay(bassStream, false);
                isPlaying = true;
                ChangePlayIcon(isPlaying);
            }
        }
        private void track_list_SelectedIndexChanged(object sender, EventArgs e) => PlayTrack(track_list.SelectedIndex);
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space)
            {
                btn_play_pause_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.Enter)
            {
                btn_searchTracks_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.Escape)
            {
                btn_goBack_Click(this, EventArgs.Empty);
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
