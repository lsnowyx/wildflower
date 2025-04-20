using Un4seen.Bass;
namespace wildflower
{
    public partial class Form1 : Form
    {
        private Label hoverTimeLabel = new Label();
        private System.Windows.Forms.Timer stateTimer;
        private string[] paths;
        private int currentIndex = 0;
        private short shuffleClickCounter = 0;
        private long resumeTimeMs = -1;
        private string musicFolder;
        private string musicFolderPath = "musicFolderPath.txt";
        private string playlistSaveFile = "playlist.txt";
        private string playbackStateFile = "state.txt";
        private bool isTransitioning = false;
        private bool isLooped = false;
        private bool isPlaying = false;
        private int bassStream;
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
                ResizeImage(Image.FromFile("iconPauseButton.png"), 50, 50) :
                ResizeImage(Image.FromFile("iconPlayButton.png"), 50, 50);
        }
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = new Icon("wildflowerico.ico");

            hoverTimeLabel.AutoSize = true;
            hoverTimeLabel.BackColor = Color.Black;
            hoverTimeLabel.ForeColor = Color.White;
            hoverTimeLabel.Padding = new Padding(4);
            hoverTimeLabel.Visible = false;
            hoverTimeLabel.Font = new Font("Segoe UI", 8);
            hoverTimeLabel.BringToFront();
            this.Controls.Add(hoverTimeLabel);

            lbl_volume.Text = "30%";
            track_volume.Value = 30;

            btn_play_pause.Image = ResizeImage(Image.FromFile("iconPlayButton.png"), 50, 50);
            btn_prevTrack.Image = ResizeImage(Image.FromFile("iconPreviousTrack.png"), 50, 50);
            btn_nextTrack.Image = ResizeImage(Image.FromFile("iconNextTrack.png"), 50, 50);
            btn_loopTrack.Image = ResizeImage(Image.FromFile("iconLoopTrack.png"), 50, 50);
            btn_shuffleTrack.Image = ResizeImage(Image.FromFile("iconShuffleTrack.png"), 50, 50);

            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
        }
        private void InitStateTimer()
        {
            SavePlaybackState();
            stateTimer = new System.Windows.Forms.Timer();
            stateTimer.Interval = 30000;
            stateTimer.Tick += (s, e) => SavePlaybackState();
            stateTimer.Start();
        }
        private void SavePlaybackState()
        {
            if (paths == null || paths.Length == 0) return;

            int index = currentIndex;
            long time = Bass.BASS_ChannelGetPosition(bassStream);

            File.WriteAllText(playbackStateFile, $"{index}|{time}");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(musicFolderPath) && !File.Exists(playlistSaveFile))
            {
                string savedPath = File.ReadAllText(musicFolderPath);
                if (Directory.Exists(savedPath))
                {
                    musicFolder = savedPath;
                    LoadSongsFromFolder(musicFolder);
                    btn_shuffleTrack_DoubleClick(sender, e);
                }
            }
            if (File.Exists(playbackStateFile) && File.Exists(playlistSaveFile))
            {
                LoadPlaylistFromFile(playlistSaveFile);
                var parts = File.ReadAllText(playbackStateFile).Split('|');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int index) &&
                    long.TryParse(parts[1], out long time))
                {
                    currentIndex = index;
                    resumeTimeMs = time;
                    track_list.SelectedIndex = index;
                    PlayTrack(index, time);
                }
            }
            SavePlaybackState();
            InitStateTimer();
        }
        private void btn_open_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                musicFolder = fbd.SelectedPath;
                File.WriteAllText(musicFolderPath, musicFolder);
                Form1_Load(sender, e);
            }
        }
        private void LoadPlaylistFromFile(string playlistFilePath)
        {
            if (!File.Exists(playlistFilePath)) return;

            var lines = File.ReadAllLines(playlistFilePath);
            var validFiles = new List<string>();

            foreach (var line in lines)
            {
                if (File.Exists(line))
                {
                    string ext = Path.GetExtension(line).ToLower();
                    if (ext == ".mp3" || ext == ".wav" || ext == ".flac" || ext == ".ogg")
                    {
                        validFiles.Add(line);
                    }
                }
            }
            paths = validFiles.ToArray();
            track_list.Items.Clear();
            foreach (var filePath in paths)
            {
                track_list.Items.Add(Path.GetFileName(filePath));
            }
        }
        private void LoadSongsFromFolder(string folderPath)
        {
            paths = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                             .Where(f => f.EndsWith(".mp3") || f.EndsWith(".wav") || f.EndsWith(".flac") || f.EndsWith(".ogg"))
                             .ToArray();
            track_list.Items.Clear();
            foreach (var file in paths)
            {
                track_list.Items.Add(Path.GetFileName(file));
            }
        }
        private void track_list_SelectedIndexChanged(object sender, EventArgs e) => PlayTrack(track_list.SelectedIndex);
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (bassStream != 0 && Bass.BASS_ChannelIsActive(bassStream) != BASSActive.BASS_ACTIVE_STOPPED)
            {
                long pos = Bass.BASS_ChannelGetPosition(bassStream);
                long len = Bass.BASS_ChannelGetLength(bassStream);

                int posMs = (int)Bass.BASS_ChannelBytes2Seconds(bassStream, pos) * 1000;
                int lenMs = (int)Bass.BASS_ChannelBytes2Seconds(bassStream, len) * 1000;

                p_bar.Maximum = lenMs;
                p_bar.Value = Math.Min(posMs, lenMs);

                lbl_track_start.Text = TimeSpan.FromMilliseconds(posMs).ToString(@"mm\:ss");
                lbl_track_end.Text = TimeSpan.FromMilliseconds(lenMs).ToString(@"mm\:ss");
            }
            if (Bass.BASS_ChannelIsActive(bassStream) == BASSActive.BASS_ACTIVE_STOPPED && !isTransitioning && paths != null && paths.Length > 0)
            {
                isTransitioning = true;
                int nextIndex = currentIndex;
                if (!isLooped)
                {
                    nextIndex++;
                }
                if (nextIndex < paths.Length)
                {
                    PlayTrack(nextIndex);
                }
                else
                {
                    btn_shuffleTrack_DoubleClick(sender, e);
                }
                isTransitioning = false;
            }
        }
        private void p_bar_MouseDown(object sender, MouseEventArgs e)
        {
            int seekMs = p_bar.Maximum * e.X / p_bar.Width;
            long bytePos = Bass.BASS_ChannelSeconds2Bytes(bassStream, seekMs / 1000.0);
            Bass.BASS_ChannelSetPosition(bassStream, bytePos);
        }
        private void p_bar_MouseMove(object sender, MouseEventArgs e)
        {
            int hoverMs = p_bar.Maximum * e.X / p_bar.Width;
            string timeStr = TimeSpan.FromMilliseconds(hoverMs).ToString(@"mm\:ss");
            hoverTimeLabel.Text = timeStr;
            hoverTimeLabel.Location = p_bar.PointToScreen(e.Location);
            hoverTimeLabel.Location = this.PointToClient(hoverTimeLabel.Location);
            hoverTimeLabel.Top -= 25;
            hoverTimeLabel.Visible = true;
            hoverTimeLabel.BringToFront();
        }
        private void p_bar_MouseLeave(object sender, EventArgs e) => hoverTimeLabel.Visible = false;
        private void track_volume_Scroll(object sender, EventArgs e)
        {
            lbl_volume.Text = track_volume.Value.ToString() + "%";
            Bass.BASS_ChannelSetAttribute(bassStream, BASSAttribute.BASS_ATTRIB_VOL, track_volume.Value / 100f);
        }
        private static void ShuffleArray<T>(T[] array)
        {
            Random rng = new Random();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
        private void PlayTrack(int index, long startAt = 0)
        {
            if (paths == null || index < 0 || index >= paths.Length) return;

            currentIndex = index;
            Bass.BASS_ChannelStop(bassStream);
            Bass.BASS_StreamFree(bassStream);

            bassStream = Bass.BASS_StreamCreateFile(paths[index], 0L, 0L, BASSFlag.BASS_DEFAULT);

            if (startAt > 0)
            {
                Bass.BASS_ChannelSetPosition(bassStream, startAt);
            }

            Bass.BASS_ChannelPlay(bassStream, false);
            Bass.BASS_ChannelSetAttribute(bassStream, BASSAttribute.BASS_ATTRIB_VOL, track_volume.Value / 100f);
            track_list.SelectedIndex = index;
            isPlaying = true;
            ChangePlayIcon(isPlaying);
        }
        private void SavePlaylistToFile()
        {
            if (paths == null || paths.Length == 0) return;

            File.WriteAllLines(playlistSaveFile, paths);
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
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space)
            {
                btn_play_pause_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.R)
            {
                btn_loopTrack_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void btn_nextTrack_Click(object sender, EventArgs e)
        {
            if (track_list.SelectedIndex < track_list.Items.Count - 1)
            {
                track_list.SelectedIndex += 1;
            }
        }
        private void btn_prevTrack_Click(object sender, EventArgs e)
        {
            if (track_list.SelectedIndex > 0)
            {
                track_list.SelectedIndex -= 1;
            }
        }
        private void btn_loopTrack_Click(object sender, EventArgs e)
        {
            isLooped = !isLooped;
            btn_loopTrack.Image = isLooped ?
                ResizeImage(Image.FromFile("iconUnLoopTrack.png"), 50, 50) :
                ResizeImage(Image.FromFile("iconLoopTrack.png"), 50, 50);
        }
        private void btn_shuffleTrack_DoubleClick(object sender, EventArgs e)
        {
            if (paths == null || paths.Length == 0) return;

            ShuffleArray(paths);
            track_list.Items.Clear();
            foreach (var file in paths)
            {
                track_list.Items.Add(Path.GetFileName(file));
            }
            PlayTrack(0);
            SavePlaylistToFile();
            SavePlaybackState();
            shuffleClickCounter = 0;
        }
        private void btn_shuffleTrack_Click(object sender, EventArgs e)
        {
            shuffleClickCounter++;
            if (shuffleClickCounter > 2)
            {
                shuffleClickCounter = 0;
                MessageBox.Show("Shuffle works only on doubleclick");
            }
        }
    }
}