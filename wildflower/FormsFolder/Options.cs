namespace wildflower
{
    public partial class Options : Form
    {
        public event EventHandler OpenPressed;
        public event EventHandler UpdatePressed;
        public event EventHandler SearchPressed;
        public event EventHandler PlaylistPressed;
        public event EventHandler CloseRequest;
        public Options()
        {
            InitializeComponent();
            btn_open.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconOpenFolder.png"), btn_open.Width, btn_open.Height);
            btn_update.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconUpdatePlaylist.png"), btn_update.Width, btn_update.Height);
            btn_searchTrack.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconSpecificTrack.png"), btn_searchTrack.Width, btn_searchTrack.Height);
            btn_selectPlaylist.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconSelectPlaylist.png"), btn_selectPlaylist.Width, btn_selectPlaylist.Height);
        }
        private void btn_open_Click(object sender, EventArgs e)
        {
            OpenPressed?.Invoke(this, EventArgs.Empty);
        }
        private void btn_update_Click(object sender, EventArgs e)
        {
            UpdatePressed?.Invoke(this, EventArgs.Empty);
        }
        private void btn_searchTrack_Click(object sender, EventArgs e)
        {
            SearchPressed?.Invoke(this, EventArgs.Empty);
        }
        private void btn_selectPlaylist_Click(object sender, EventArgs e)
        {
            PlaylistPressed?.Invoke(this, EventArgs.Empty);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                if (Helper.IsAnimatingButton || Helper.IsAnimatingPanel) return true;
                CloseRequest?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void Options_Load(object sender, EventArgs e) => btn_update.Focus();
    }
}