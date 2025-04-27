namespace wildflower
{
    public partial class Options : Form
    {
        public bool openBtnPressed = false;
        public bool updateBtnPressed = false;
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
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = new Icon("icons\\wildflowerico.ico");
            btn_open.Image = ResizeImage(Image.FromFile("icons\\iconOpenFolder.png"), 50, 50);
            btn_update.Image = ResizeImage(Image.FromFile("icons\\iconUpdatePlaylist.png"), 50, 50);
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
    }
}
