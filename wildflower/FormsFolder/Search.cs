namespace wildflower
{
    public partial class Search : Form
    {
        private string[] paths;
        private readonly Form1 form1;
        private string[] validSearches;
        public event EventHandler<string> SongToPlay;
        public event EventHandler CloseRequest;
        public Search(string[] paths, Form1 form1)
        {
            InitializeComponent();
            btn_searchTrack.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconFindTrack.png"), btn_searchTrack.Width, btn_searchTrack.Height);
            btn_Play.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconPlayButton.png"), btn_Play.Width, btn_Play.Height);
            btn_Random.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconShuffleTrack.png"), btn_Random.Width, btn_Random.Height);
            this.paths = paths;
            this.form1 = form1;
        }
        private async void btn_searchTrack_Click(object sender, EventArgs e)
        {
            if (this.txbx_search.Text == string.Empty) return;
            if (form1.SuppressAutoPlay) return;
            var getMatchingElements = await GetMatchingElements(paths, this.txbx_search.Text);
            validSearches = getMatchingElements.ToArray();
            await Helper.TrackListAdd(validSearches, track_list);
        }
        public static async Task<List<string>> GetMatchingElements(string[] array, string substring, bool ignoreCase = true)
        {
            if (array == null || array.Length == 0 || substring == null) return null;
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return await Task.Run(() => array
                .Where(filePath =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    var (title, artist) = Helper.GetMetadataFromFile(filePath);

                    return fileName.IndexOf(substring, comparison) >= 0
                        || title.IndexOf(substring, comparison) >= 0
                        || artist.IndexOf(substring, comparison) >= 0;
                })
                .ToList());
        }
        private void btn_Play_Click(object sender, EventArgs e)
        {
            if (this.track_list.SelectedItem == null) return;
            SongToPlay?.Invoke(this, validSearches[track_list.SelectedIndex]);
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                btn_searchTrack_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == (Keys.Enter | Keys.Shift))
            {
                btn_Play_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.Tab)
            {
                btn_Random_Click(this, EventArgs.Empty);
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
        private void Search_Load(object sender, EventArgs e)
        {
            txbx_search.Focus();
            track_list.BackColor = this.BackColor;
        }
        private void btn_Random_Click(object sender, EventArgs e)
        {
            if (form1.SuppressAutoPlay) return;
            Random rng = new Random();
            SongToPlay?.Invoke(this, paths[rng.Next(0, paths.Length)]);
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}