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
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = new Icon("icons\\wildflowerico.ico");
            btn_open.Image = Helper.ResizeImage(Image.FromFile("icons\\iconOpenFolder.png"), 50, 50);
            btn_update.Image = Helper.ResizeImage(Image.FromFile("icons\\iconUpdatePlaylist.png"), 50, 50);
            btn_searchTrack.Image = Helper.ResizeImage(Image.FromFile("icons\\iconSpecificTrack.png"), 50, 50);
            btn_selectPlaylist.Image = Helper.ResizeImage(Image.FromFile("icons\\iconSelectPlaylist.png"), 50, 50);
        }
        private void btn_open_Click(object sender, EventArgs e)
        {
            OpenPressed?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
        private void btn_update_Click(object sender, EventArgs e)
        {
            UpdatePressed?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
        private void btn_searchTrack_Click(object sender, EventArgs e)
        {
            SearchPressed?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
        private void btn_selectPlaylist_Click(object sender, EventArgs e)
        {
            PlaylistPressed?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                CloseRequest?.Invoke(this, EventArgs.Empty);
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
