namespace wildflower
{
    public partial class Search : Form
    {
        private string[] paths;
        public string songToPlay { get; set; }
        public bool playBtnPressed { get; set; } = false;
        public Search(string[] paths)
        {
            InitializeComponent();
            CenterToScreen();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = new Icon("icons\\wildflowerico.ico");
            btn_searchTrack.Image = Helper.ResizeImage(Image.FromFile("icons\\iconFindTrack.png"), 50, 50);
            btn_Play.Image = Helper.ResizeImage(Image.FromFile("icons\\iconPlayButton.png"), 50, 50);
            this.paths = paths;
        }
        private void btn_searchTrack_Click(object sender, EventArgs e)
        {
            if (this.txbx_search.Text == string.Empty) return;
            this.track_list.Items.Clear();
            this.track_list.Items.AddRange(GetMatchingElements(paths, this.txbx_search.Text).ToArray());
        }
        public static List<string> GetMatchingElements(string[] array, string substring, bool ignoreCase = true)
        {
            if (array == null || substring == null) return null;
            return array.Where(f => f
                .IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(f => Path.GetFileName(f)).ToList();
        }
        private void btn_Play_Click(object sender, EventArgs e)
        {
            if (this.track_list.SelectedItem == null) return;
            songToPlay = track_list.SelectedItem.ToString();
            playBtnPressed = true;
            this.Close();
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
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
