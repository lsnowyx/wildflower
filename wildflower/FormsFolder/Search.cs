using wildflower.Services.Library;
using wildflower.Services.Search;

namespace wildflower
{
    public partial class Search : Form
    {
        private readonly string[] paths;
        private readonly ISearchService searchService;
        private readonly IMetadataService metadataService;
        private readonly Func<bool> isSearchSuppressed;
        private string[] validSearches = Array.Empty<string>();

        public event EventHandler<string>? SongToPlay;
        public event EventHandler? CloseRequest;

        public Search(
            IEnumerable<string> paths,
            ISearchService searchService,
            IMetadataService metadataService,
            Func<bool> isSearchSuppressed)
        {
            InitializeComponent();
            btn_searchTrack.Image = Helper.LoadIconImage("iconFindTrack.png", btn_searchTrack.Width, btn_searchTrack.Height);
            btn_Play.Image = Helper.LoadIconImage("iconPlayButton.png", btn_Play.Width, btn_Play.Height);
            btn_Random.Image = Helper.LoadIconImage("iconShuffleTrack.png", btn_Random.Width, btn_Random.Height);
            this.paths = paths.ToArray();
            this.searchService = searchService;
            this.metadataService = metadataService;
            this.isSearchSuppressed = isSearchSuppressed;
        }

        private async void btn_searchTrack_Click(object sender, EventArgs e)
        {
            if (txbx_search.Text == string.Empty) return;
            if (isSearchSuppressed()) return;

            var getMatchingElements = await searchService.FindMatchingTracksAsync(paths, txbx_search.Text);
            validSearches = getMatchingElements.ToArray();
            await AddSearchResultsToListAsync(validSearches);
        }

        private void btn_Play_Click(object sender, EventArgs e)
        {
            if (track_list.SelectedItem == null) return;
            if (track_list.SelectedIndex < 0 || track_list.SelectedIndex >= validSearches.Length) return;

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
            track_list.BackColor = BackColor;
        }

        private void btn_Random_Click(object sender, EventArgs e)
        {
            if (isSearchSuppressed()) return;
            if (paths.Length == 0) return;

            Random rng = new Random();
            SongToPlay?.Invoke(this, paths[rng.Next(0, paths.Length)]);
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

        private async Task AddSearchResultsToListAsync(string[] searchResults)
        {
            track_list.Items.Clear();
            var trackInfo = await metadataService.GetTrackInfoAsync(searchResults);
            foreach (var track in trackInfo)
                track_list.Items.Add(track.DisplayName);
        }
    }
}
