using Un4seen.Bass;
using wildflower.Models;

namespace wildflower.FormsFolder
{
    public partial class ErrorsPanel : UserControl
    {
        private int bassStream;
        private readonly string errorMsg;
        private readonly ErrorAudio errorAudio;
        private readonly Size f1Size;
        private int lbl_InitHeight;
        public bool Yes { get; set; } = false;
        public event EventHandler CloseRequest;
        public ErrorsPanel(string errorMsg, ErrorAudio errorAudio, Size f1Size)
        {
            InitializeComponent();
            this.errorMsg = errorMsg;
            this.errorAudio = errorAudio;
            this.f1Size = f1Size;
            Left = (f1Size.Width - Width) / 2;
            Top = (f1Size.Height - Height) / 2;
            errorIco.Left = Width / 2 - errorIco.Width / 2;
            lbl_InitHeight = lbl_error.Height;
        }
        private void ErrorsPanel_Load(object sender, EventArgs e)
        {
            lbl_error.Text = errorMsg;
            Bass.BASS_ChannelStop(bassStream);
            Bass.BASS_StreamFree(bassStream);
            string who = string.Empty;
            switch (errorAudio)
            {
                case ErrorAudio.TexasINFO:
                    Random rng = new Random();
                    int j = rng.Next(1, 3);
                    who = $"{Helper.IconsPath}\\Error\\Texas{j}INFO";
                    BackColor = ColorTranslator.FromHtml("#00609c");
                    btn_close.Text = "Ok";
                    break;
                case ErrorAudio.FiammettaWARNING:
                    who = $"{Helper.IconsPath}\\Error\\FiammettaWARNING";
                    BackColor = ColorTranslator.FromHtml("#ff6f00");
                    btn_yes.Text = "Yes";
                    btn_yes.Enabled = true;
                    btn_yes.Visible = true;
                    btn_close.Text = "No";
                    break;
            }
            btn_close.Enabled = true;
            btn_close.Visible = true;
            btn_close.Focus();
            bassStream = Bass.BASS_StreamCreateFile(who + ".mp3", 0L, 0L, BASSFlag.BASS_DEFAULT);
            errorIco.Image = Helper.ResizeImage(Image.FromFile(who + ".png"), errorIco.Width, errorIco.Height);
            Bass.BASS_ChannelPlay(bassStream, false);
            Bass.BASS_ChannelSetAttribute(bassStream, BASSAttribute.BASS_ATTRIB_VOL, 30 / 100f);
        }
        private void lbl_error_SizeChanged(object sender, EventArgs e)
        {
            if (lbl_error.Size.Width < Width) return;
            Width = lbl_error.Size.Width;
            Height = Height + lbl_error.Size.Height - lbl_InitHeight;
            btn_close.Left = Width - btn_close.Width;
            errorIco.Left = Width / 2 - errorIco.Width / 2;
            Left = (f1Size.Width - Width) / 2;
            Top = (f1Size.Height - Height) / 2;
            panel1.Width = Width;
        }
        private void btn_close_Click(object sender, EventArgs e)
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
        private void btn_yes_Click(object sender, EventArgs e)
        {
            Yes = true;
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}