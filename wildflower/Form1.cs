using Un4seen.Bass;
namespace wildflower
{
    public partial class Form1 : Form
    {
        private Label hoverTimeLabel = new Label();
        private System.Windows.Forms.Timer stateTimer;
        private string[] paths;
        private int currentIndex = 0;
        private long resumeTimeMs = -1;
        private string musicFolder;
        private string musicFolderPath = "musicFolderPath.txt";
        private string playlistSaveFile = "playlist.txt";
        private string playbackStateFile = "state.txt";
        private bool isTransitioning = false;
        private bool isLooped = false;
        private int bassStream;
        public Form1()
        {
            InitializeComponent();
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
                    btn_shuffle_Click(sender, e);
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
        private void btn_play_Click(object sender, EventArgs e) => Bass.BASS_ChannelPlay(bassStream, false);
        private void btn_pause_Click(object sender, EventArgs e) => Bass.BASS_ChannelPause(bassStream);
        private void btn_stop_Click(object sender, EventArgs e) => Bass.BASS_ChannelStop(bassStream);
        private void track_list_SelectedIndexChanged(object sender, EventArgs e) => PlayTrack(track_list.SelectedIndex);
        private void btn_next_Click(object sender, EventArgs e)
        {
            if (track_list.SelectedIndex < track_list.Items.Count - 1)
            {
                track_list.SelectedIndex += 1;
            }
        }
        private void btn_preview_Click(object sender, EventArgs e)
        {
            if (track_list.SelectedIndex > 0)
            {
                track_list.SelectedIndex -= 1;
            }
        }
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
            if (Bass.BASS_ChannelIsActive(bassStream) == BASSActive.BASS_ACTIVE_STOPPED && !isTransitioning)
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
                    btn_shuffle_Click(sender, e);
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
        }
        private void btn_shuffle_Click(object sender, EventArgs e)
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
        }
        private void btn_loop_Click(object sender, EventArgs e)
        {
            isLooped = !isLooped;
            btn_loop.Text = isLooped ? "UnLoop" : "Loop";
        }
        private void SavePlaylistToFile()
        {
            if (paths == null || paths.Length == 0) return;

            File.WriteAllLines(playlistSaveFile, paths);
        }
    }
}