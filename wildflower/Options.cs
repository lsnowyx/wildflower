namespace wildflower
{
    public partial class Options : Form
    {
        public bool openBtnPressed { get; set; } = false;
        public bool updateBtnPressed { get; set; } = false;
        public bool searchBtnPressed { get; set; } = false;
        public bool playlistBtnPressed { get; set; } = false;
        public Options()
        {
            InitializeComponent();
            CenterToScreen();
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
            openBtnPressed = true;
            this.Close();
        }
        private void btn_update_Click(object sender, EventArgs e)
        {
            updateBtnPressed = true;
            this.Close();
        }
        private void btn_searchTrack_Click(object sender, EventArgs e)
        {
            searchBtnPressed = true;
            this.Close();
        }
        private void btn_selectPlaylist_Click(object sender, EventArgs e)
        {
            playlistBtnPressed = true;
            this.Close();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
