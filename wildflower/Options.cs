namespace wildflower
{
    public partial class Options : Form
    {
        public bool openBtnPressed = false;
        public bool updateBtnPressed = false;
        public bool searchBtnPressed = false;
        public bool playlistBtnPressed = false;
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
        public Options()
        {
            InitializeComponent();
            CenterToScreen();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = new Icon("icons\\wildflowerico.ico");
            btn_open.Image = ResizeImage(Image.FromFile("icons\\iconOpenFolder.png"), 50, 50);
            btn_update.Image = ResizeImage(Image.FromFile("icons\\iconUpdatePlaylist.png"), 50, 50);
            btn_searchTrack.Image = ResizeImage(Image.FromFile("icons\\iconSpecificTrack.png"), 50, 50);
            btn_selectPlaylist.Image = ResizeImage(Image.FromFile("icons\\iconSelectPlaylist.png"), 50, 50);
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
