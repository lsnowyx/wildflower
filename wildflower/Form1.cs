#region using directives
using LibVLCSharp.Shared;
#endregion

namespace wildflower
{
    public partial class Form1 : Form
    {
        #region Fields
        private Label hoverTimeLabel = new Label();
        private LibVLC libVLC;
        private MediaPlayer mediaPlayer;
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
        #endregion

        #region Constructor
        public Form1()
        {
            InitializeComponent();
            this.Icon = new Icon("wildflowerico.ico");

            // Progress bar hover label setup
            hoverTimeLabel.AutoSize = true;
            hoverTimeLabel.BackColor = Color.Black;
            hoverTimeLabel.ForeColor = Color.White;
            hoverTimeLabel.Padding = new Padding(4);
            hoverTimeLabel.Visible = false;
            hoverTimeLabel.Font = new Font("Segoe UI", 8);
            hoverTimeLabel.BringToFront();
            this.Controls.Add(hoverTimeLabel);

            // VLC setup
            libVLC = new LibVLC();
            mediaPlayer = new MediaPlayer(libVLC);

            // Volume setup
            lbl_volume.Text = "50%";
            track_volume.Value = 50;
            mediaPlayer.Volume = track_volume.Value;

            // Events
            mediaPlayer.EndReached += MediaPlayer_EndReached;
            mediaPlayer.Playing += MediaPlayer_Playing;
        }
        #endregion

        #region State Timer
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
            long time = mediaPlayer.Time;

            File.WriteAllText(playbackStateFile, $"{index}|{time}");
        }
        #endregion

        #region VLC Event Handlers
        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            if (resumeTimeMs <= 0) return;

            long timeToSeek = resumeTimeMs;
            resumeTimeMs = -1;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                this.BeginInvoke(() =>
                {
                    mediaPlayer.Time = timeToSeek;
                });
            });
        }

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            if (isTransitioning) return;
            isTransitioning = true;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                this.BeginInvoke(() =>
                {
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
                });
            });
        }
        #endregion

        #region Form Load
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
                    var media = new Media(libVLC, paths[index], FromType.FromPath);
                    mediaPlayer.Play(media);
                }
            }
            SavePlaybackState();
            InitStateTimer();
        }
        #endregion

        #region File & Folder Loading
        private void btn_open_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                musicFolder = fbd.SelectedPath;
                File.WriteAllText(musicFolderPath, musicFolder);
                LoadSongsFromFolder(musicFolder);
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
        #endregion

        #region Playback Controls
        private void btn_play_Click(object sender, EventArgs e) => mediaPlayer.Play();

        private void btn_pause_Click(object sender, EventArgs e) => mediaPlayer.Pause();

        private void btn_stop_Click(object sender, EventArgs e) => mediaPlayer.Stop();

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
        #endregion

        #region Timer & Progress Bar
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.State == VLCState.Playing)
            {
                p_bar.Maximum = (int)mediaPlayer.Length;
                p_bar.Value = (int)mediaPlayer.Time;
                TimeSpan current = TimeSpan.FromMilliseconds(mediaPlayer.Time);
                TimeSpan total = TimeSpan.FromMilliseconds(mediaPlayer.Length);
                lbl_track_start.Text = current.ToString(@"mm\:ss");
                lbl_track_end.Text = total.ToString(@"mm\:ss");
            }
        }

        private void p_bar_MouseDown(object sender, MouseEventArgs e)
        {
            mediaPlayer.Time = mediaPlayer.Length * e.X / p_bar.Width;
            p_bar.Value = (int)mediaPlayer.Time;
        }

        private void p_bar_MouseMove(object sender, MouseEventArgs e)
        {
            if (mediaPlayer.Length > 0)
            {
                long hoveredTime = mediaPlayer.Length * e.X / p_bar.Width;
                TimeSpan t = TimeSpan.FromMilliseconds(hoveredTime);
                string timeStr = t.ToString(@"mm\:ss");
                hoverTimeLabel.Text = timeStr;
                hoverTimeLabel.Location = p_bar.PointToScreen(e.Location);
                hoverTimeLabel.Location = this.PointToClient(hoverTimeLabel.Location);
                hoverTimeLabel.Top -= 25;
                hoverTimeLabel.Visible = true;
            }
        }

        private void p_bar_MouseLeave(object sender, EventArgs e)
        {
            hoverTimeLabel.Visible = false;
        }
        #endregion

        #region Volume
        private void track_volume_Scroll(object sender, EventArgs e)
        {
            lbl_volume.Text = track_volume.Value.ToString() + "%";
            mediaPlayer.Volume = track_volume.Value;
        }
        #endregion

        #region Playback Logic
        private static void ShuffleArray<T>(T[] array)
        {
            Random rng = new Random();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        private void PlayTrack(int index)
        {
            if (paths == null || index < 0 || index >= paths.Length) return;

            currentIndex = index;

            var media = new Media(libVLC, paths[index], FromType.FromPath);
            media.AddOption(":volume=256");
            media.AddOption(":aout=wasapi");
            media.AddOption(":audio-filter=equalizer");
            media.AddOption(":equalizer-bands=10 8 6 5 5 4 4 3 3 2");
            media.AddOption(":equalizer-preamp=8");

            mediaPlayer.Play(media);
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
        #endregion
    }
}