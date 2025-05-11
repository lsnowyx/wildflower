using Un4seen.Bass;
namespace wildflower
{
    public partial class Form1 : Form
    {
        private Label hoverTimeLabel = new Label();
        private string[] paths;
        private int currentIndex = 0;
        private short shuffleClickCounter = 0;
        private long resumeTimeMs = -1;
        private string musicFolder;
        private string playlistsDir = "playlists";
        private string basePlaylistPath;
        private string musicFolderPath => Path.Combine(basePlaylistPath, "musicFolderPath.txt");
        private string playlistSaveFile => Path.Combine(basePlaylistPath, "playlist.txt");
        private string playbackStateFile => Path.Combine(basePlaylistPath, "state.txt");
        private bool isTransitioning = false;
        private bool isLooped = false;
        private bool isPlayingField = false;
        private bool isPlaying
        {
            get => isPlayingField;
            set
            {
                isPlayingField = value;
                btn_play_pause.Image = value ?
                    Helper.ResizeImage(Image.FromFile("icons\\iconPauseButton.png"), 50, 50) :
                    Helper.ResizeImage(Image.FromFile("icons\\iconPlayButton.png"), 50, 50);
            }
        }
        private int bassStream;
        private bool bassTempIsPlaying = false;
        private bool BassTempIsPlaying
        {
            get => bassTempIsPlaying;
            set
            {
                bassTempIsPlaying = value;
                TempSongIsPlaying(value);
            }
        }
        private int bassTempSongIndex;


        #region musicLibraryDependentCode
        //musicLibraryDependentCode
        private void PlayTrack(int index, long startAt = 0)
        {
            if (paths == null || index < 0 || index >= paths.Length) return;
            if (!BassTempIsPlaying)
            {
                currentIndex = index;
            }
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
                if (BassTempIsPlaying)
                {
                    if (isLooped)
                    {
                        PlayTrack(bassTempSongIndex);
                    }
                    else
                    {
                        btn_goBack_Click(sender, e);
                    }
                }
                isTransitioning = false;
            }
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
            }
        }
        private void p_bar_MouseDown(object sender, MouseEventArgs e)
        {
            int seekMs = p_bar.Maximum * e.X / p_bar.Width;
            long bytePos = Bass.BASS_ChannelSeconds2Bytes(bassStream, seekMs / 1000.0);
            Bass.BASS_ChannelSetPosition(bassStream, bytePos);
        }
        private void track_volume_Scroll(object sender, EventArgs e)
        {
            lbl_volume.Text = track_volume.Value.ToString() + "%";
            Bass.BASS_ChannelSetAttribute(bassStream, BASSAttribute.BASS_ATTRIB_VOL, track_volume.Value / 100f);
        }
        //musicLibraryDependentCode
        #endregion

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            lbl_volume.Text = "30%";
            track_volume.Value = 30;

            btn_goBack.Enabled = false;
            btn_goBack.Visible = false;

            #region p_barHoverLabelData
            hoverTimeLabel.AutoSize = true;
            hoverTimeLabel.BackColor = Color.Black;
            hoverTimeLabel.ForeColor = Color.White;
            hoverTimeLabel.Padding = new Padding(4);
            hoverTimeLabel.Visible = false;
            hoverTimeLabel.Font = new Font("Segoe UI", 8);
            hoverTimeLabel.BringToFront();
            this.Controls.Add(hoverTimeLabel);
            #endregion

            #region iconsInit
            this.Icon = new Icon("icons\\wildflowerico.ico");
            btn_play_pause.Image = Helper.ResizeImage(Image.FromFile("icons\\iconPlayButton.png"), 50, 50);
            btn_prevTrack.Image = Helper.ResizeImage(Image.FromFile("icons\\iconPreviousTrack.png"), 50, 50);
            btn_nextTrack.Image = Helper.ResizeImage(Image.FromFile("icons\\iconNextTrack.png"), 50, 50);
            btn_loopTrack.Image = Helper.ResizeImage(Image.FromFile("icons\\iconLoopTrack.png"), 50, 50);
            btn_shuffleTrack.Image = Helper.ResizeImage(Image.FromFile("icons\\iconShuffleTrack.png"), 50, 50);
            btn_options.Image = Helper.ResizeImage(Image.FromFile("icons\\iconMoreOptions.png"), 50, 50);
            btn_goBack.Image = Helper.ResizeImage(Image.FromFile("icons\\iconGoBack.png"), 50, 50);
            #endregion

            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            InitializePlaylistPath();
        }
        private void InitStateTimer()
        {
            stateTimer.Tick += (s, e) => SavePlaybackState();
            stateTimer.Start();
        }
        private void SavePlaybackState()
        {
            if (paths == null || paths.Length == 0) return;

            int index = currentIndex;
            long time = Bass.BASS_ChannelGetPosition(bassStream);
            resumeTimeMs = time;

            File.WriteAllText(playbackStateFile, $"{index}|{time}");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(musicFolderPath))
            {
                MessageBox.Show("Please select a song folder");
                btn_open_Click(sender, e);
                return;
            }
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
                    RefreshPlaylist();
                    PlayTrack(currentIndex, resumeTimeMs);
                }
            }
            SavePlaybackState();
            InitStateTimer();
        }
        private void LoadPlaylistFromFile(string playlistFilePath)
        {
            if (!File.Exists(playlistFilePath)) return;
            var lines = File.ReadAllLines(playlistFilePath);
            paths = lines.ToArray();
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
        private void SavePlaylistToFile()
        {
            if (paths == null || paths.Length == 0) return;

            File.WriteAllLines(playlistSaveFile, paths);
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
            if (keyData == Keys.Escape && BassTempIsPlaying)
            {
                btn_goBack_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void btn_open_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                musicFolder = fbd.SelectedPath;
                foreach (string dir in Directory.GetDirectories(playlistsDir))
                {
                    string existingPathFile = Path.Combine(dir, "musicFolderPath.txt");
                    if (File.Exists(existingPathFile))
                    {
                        string existingPath = File.ReadAllText(existingPathFile);
                        if (string.Equals(existingPath, musicFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("This folder is already part of a playlist.", "Duplicate Playlist", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                }
                int nextIndex = 0;
                while (Directory.Exists(Path.Combine(playlistsDir, nextIndex.ToString())))
                    nextIndex++;

                string newPlaylistDir = Path.Combine(playlistsDir, nextIndex.ToString());
                Directory.CreateDirectory(newPlaylistDir);

                File.WriteAllText(Path.Combine(playlistsDir, "lastUsed.txt"), nextIndex.ToString());

                File.WriteAllText(Path.Combine(newPlaylistDir, "musicFolderPath.txt"), musicFolder);


                basePlaylistPath = newPlaylistDir;

                Form1_Load(sender, e);
            }
        }
        private void btn_nextTrack_Click(object sender, EventArgs e)
        {
            if (currentIndex < paths.Length - 1)
            {
                currentIndex++;
                PlayTrack(currentIndex);
            }
        }
        private void btn_prevTrack_Click(object sender, EventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                PlayTrack(currentIndex);
            }
        }
        private void btn_loopTrack_Click(object sender, EventArgs e)
        {
            isLooped = !isLooped;
            btn_loopTrack.Image = isLooped ?
                Helper.ResizeImage(Image.FromFile("icons\\iconUnLoopTrack.png"), 50, 50) :
                Helper.ResizeImage(Image.FromFile("icons\\iconLoopTrack.png"), 50, 50);
        }
        private void btn_shuffleTrack_DoubleClick(object sender, EventArgs e)
        {
            if (paths == null || paths.Length == 0) return;

            Random rng = new Random();
            for (int i = paths.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (paths[i], paths[j]) = (paths[j], paths[i]);
            }

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
        private void btn_options_Click(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                btn_play_pause_Click(sender, e);
            }
            Options f2 = new Options();
            f2.ShowDialog();
            if (f2.openBtnPressed)
            {
                btn_open_Click(sender, e);
            }
            if (f2.updateBtnPressed)
            {
                if (paths == null || paths.Length == 0)
                {
                    MessageBox.Show("Nowhere to update from");
                    return;
                }
                RefreshPlaylist();
                SavePlaybackState();
                PlayTrack(currentIndex, resumeTimeMs);
                return;
            }
            if (f2.searchBtnPressed)
            {
                if (isPlaying)
                {
                    btn_play_pause_Click(sender, e);
                }
                SearchButtonPressed();
                return;
            }
            if (f2.playlistBtnPressed)
            {
                if (isPlaying)
                {
                    btn_play_pause_Click(sender, e);
                }
                PlayListButtonPressed();
            }
        }
        private void PlayListButtonPressed()
        {
            Playlists f2 = new Playlists(playlistsDir, Path.GetFileName(basePlaylistPath));
            f2.ShowDialog();
            basePlaylistPath = Path.Combine(playlistsDir, f2.Playlist2Play);
            if (Path.GetFileName(basePlaylistPath) == f2.Playlist2Play) return;
            if (!File.Exists(basePlaylistPath + "\\musicFolderPath.txt"))
            {
                if (!FindAvailablePlaylist())
                {
                    MessageBox.Show("All playlists have been deleted");
                    paths = null;
                    track_list.Items.Clear();
                }
            }
            Form1_Load(this, EventArgs.Empty);
            File.WriteAllText("playlists\\lastUsed.txt", f2.Playlist2Play);
        }
        private void SearchButtonPressed()
        {
            if (paths == null || paths.Length == 0)
            {
                MessageBox.Show("Nowhere to search from");
                return;
            }
            Search f2 = new Search(paths);
            f2.ShowDialog();
            if (f2.playBtnPressed && (f2.songToPlay != null || f2.songToPlay != string.Empty))
            {
                if (paths == null || f2.songToPlay == null) return;
                bassTempSongIndex = Array.FindIndex(paths, f => f.Contains(f2.songToPlay));
                BassTempIsPlaying = true;
            }
        }
        private void TempSongIsPlaying(bool tempSongIsPlaying)
        {
            lbl_tempSongName.Visible = tempSongIsPlaying;

            btn_prevTrack.Enabled = !tempSongIsPlaying;
            btn_prevTrack.Visible = !tempSongIsPlaying;

            btn_nextTrack.Enabled = !tempSongIsPlaying;
            btn_nextTrack.Visible = !tempSongIsPlaying;

            btn_shuffleTrack.Enabled = !tempSongIsPlaying;
            btn_shuffleTrack.Visible = !tempSongIsPlaying;

            btn_options.Enabled = !tempSongIsPlaying;
            btn_options.Visible = !tempSongIsPlaying;

            track_list.Enabled = !tempSongIsPlaying;
            track_list.Visible = !tempSongIsPlaying;

            btn_goBack.Enabled = tempSongIsPlaying;
            btn_goBack.Visible = tempSongIsPlaying;

            var tempLocation = btn_play_pause.Location;
            btn_play_pause.Location = btn_shuffleTrack.Location;
            btn_shuffleTrack.Location = tempLocation;

            tempLocation = btn_goBack.Location;
            btn_goBack.Location = btn_options.Location;
            btn_options.Location = tempLocation;

            if (tempSongIsPlaying)
            {
                this.Width -= 90;
                lbl_tempSongName.Left -= 50;
                lbl_tempSongName.Top -= 15;
            }
            else
            {
                this.Width += 90;
                lbl_tempSongName.Left += 50;
                lbl_tempSongName.Top += 15;
            }

            if (tempSongIsPlaying)
            {
                stateTimer.Stop();
            }
            else
            {
                stateTimer.Start();
            }
            if (tempSongIsPlaying)
            {
                PlayTrack(bassTempSongIndex);
            }
            else
            {
                track_list.SelectedIndex = currentIndex;
                PlayTrack(currentIndex, resumeTimeMs);
            }
            if (tempSongIsPlaying)
            {
                lbl_tempSongName.Text = Path.GetFileName(paths[bassTempSongIndex]);
            }
            else
            {
                lbl_tempSongName.Text = string.Empty;
            }
        }
        private void btn_goBack_Click(object sender, EventArgs e)
        {
            BassTempIsPlaying = false;
        }
        private void track_list_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = track_list.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                if (track_list.SelectedIndex == currentIndex) return;

                track_list.SelectedIndex = index;
                PlayTrack(index);
            }
        }
        private void InitializePlaylistPath()
        {
            string lastUsedFile = Path.Combine(playlistsDir, "lastUsed.txt");

            if (!Directory.Exists(playlistsDir))
                Directory.CreateDirectory(playlistsDir);

            string[] allPlaylists = Directory.GetDirectories(playlistsDir);

            string validPlaylistIndex = null;

            // 1. Check lastUsed.txt
            if (File.Exists(lastUsedFile))
            {
                string savedIndex = File.ReadAllText(lastUsedFile).Trim();
                string savedPath = Path.Combine(playlistsDir, savedIndex);
                if (Directory.Exists(savedPath) && File.Exists(Path.Combine(savedPath, "musicFolderPath.txt")))
                {
                    validPlaylistIndex = savedIndex;
                }
            }

            // 2. If lastUsed is missing/invalid, find first valid playlist
            if (validPlaylistIndex == null)
            {
                foreach (string dir in allPlaylists)
                {
                    if (File.Exists(Path.Combine(dir, "musicFolderPath.txt")))
                    {
                        validPlaylistIndex = Path.GetFileName(dir);
                        break;
                    }
                }
            }

            // 3. If none found, ask user to select a folder
            if (validPlaylistIndex == null)
            {
                MessageBox.Show("No valid playlist found. Please select a folder.");
                btn_open_Click(this, EventArgs.Empty); // force open
                return;
            }

            // 4. Save and assign
            File.WriteAllText(lastUsedFile, validPlaylistIndex);
            basePlaylistPath = Path.Combine(playlistsDir, validPlaylistIndex);
        }
        private void RefreshPlaylist()
        {
            musicFolder = File.ReadAllText(musicFolderPath);
            if (string.IsNullOrEmpty(musicFolder) || !Directory.Exists(musicFolder))
            {
                MessageBox.Show("Update your music folder path");
                RemoveInvalidPlaylists();
                btn_open_Click(this, EventArgs.Empty);
                if (paths == null)
                {
                    InitializePlaylistPath();
                    Form1_Load(this, EventArgs.Empty);
                }
                return;
            }
            CleanMissingTracks();
            UpdatePlaylistWithNewSongs();
        }
        private void UpdatePlaylistWithNewSongs()
        {
            var allSongsInFolder = Directory.GetFiles(musicFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".mp3") || f.EndsWith(".wav") || f.EndsWith(".flac") || f.EndsWith(".ogg"))
                .ToList();

            var currentPathsSet = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);

            var newSongs = allSongsInFolder.Where(f => !currentPathsSet.Contains(f)).ToList();

            if (newSongs.Count == 0)
                return;

            paths = paths.Concat(newSongs).ToArray();

            foreach (var newSong in newSongs)
            {
                track_list.Items.Add(Path.GetFileName(newSong));
            }

            File.WriteAllLines(playlistSaveFile, paths);
        }
        private void CleanMissingTracks()
        {
            int removedBeforeCurrent = 0;
            List<string> validPaths = new List<string>();
            for (int i = 0; i < paths.Length; i++)
            {
                string file = paths[i];
                bool exists = File.Exists(file);

                if (exists)
                {
                    validPaths.Add(file);
                }
                else
                {
                    if (i < currentIndex)
                    {
                        removedBeforeCurrent++;
                    }
                }
            }
            track_list.Items.Clear();
            foreach (string path in validPaths)
            {
                track_list.Items.Add(Path.GetFileName(path));
            }
            currentIndex = Math.Max(0, currentIndex - removedBeforeCurrent);
            paths = validPaths.ToArray();
            File.WriteAllLines(playlistSaveFile, paths);
        }
        private bool FindAvailablePlaylist()
        {
            foreach (string dir in Directory.GetDirectories(playlistsDir))
            {
                string existingPathFile = Path.Combine(dir, "musicFolderPath.txt");
                if (File.Exists(existingPathFile))
                {
                    basePlaylistPath = dir;
                    File.WriteAllText("playlists\\lastUsed.txt", Path.GetFileName(dir));
                    return true;
                }
            }
            return false;
        }
        private void RemoveInvalidPlaylists()
        {
            paths = null;
            foreach (string playlistDir in Directory.GetDirectories(playlistsDir))
            {
                string pathFile = Path.Combine(playlistDir, "musicFolderPath.txt");
                if (File.Exists(pathFile))
                {
                    string folderPath = File.ReadAllText(pathFile).Trim();
                    if (!Directory.Exists(folderPath))
                    {
                        try
                        {
                            Directory.Delete(playlistDir, recursive: true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Could not delete playlist folder '{playlistDir}': {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}