using Microsoft.Win32;
using NAudio.CoreAudioApi;
using Un4seen.Bass;
namespace wildflower
{
    public partial class Form1 : Form
    {
        #region FieldsAndProperties
        //FieldsAndProperties

        //InitializationFields
        private Label hoverTimeLabel = new Label();
        private readonly Image OptionsBtnAnimationImage;
        private short shuffleClickCounter = 0;
        //InitializationFields

        //PlayTrack
        private string[] paths;
        private int currentIndex = 0;
        private long resumeTimeMs = -1;
        private bool isTransitioning = false;
        private bool isLoopedField = false;
        private bool isLooped
        {
            get => isLoopedField;
            set
            {
                isLoopedField = value;
                btn_loopTrack.Image = value ?
                    Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconUnLoopTrack.png"), btn_loopTrack.Width, btn_loopTrack.Height) :
                    Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconLoopTrack.png"), btn_loopTrack.Width, btn_loopTrack.Height);
            }
        }
        private bool isPlayingField = false;
        private bool isPlaying
        {
            get => isPlayingField;
            set
            {
                isPlayingField = value;
                btn_play_pause.Image = value ?
                    Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconPauseButton.png"), btn_play_pause.Width, btn_play_pause.Height) :
                    Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconPlayButton.png"), btn_play_pause.Width, btn_play_pause.Height);
            }
        }
        //PlayTrack

        //Directories
        private string musicFolder;
        private string playlistsDir =
            Path.Combine(
                Environment.GetFolderPath
                (Environment.SpecialFolder.ApplicationData), ".wildflower", "playlists");
        private string basePlaylistPath;
        private string musicFolderPath => Path.Combine(basePlaylistPath, "musicFolderPath.txt");
        private string playlistSaveFile => Path.Combine(basePlaylistPath, "playlist.txt");
        private string playbackStateFile => Path.Combine(basePlaylistPath, "state.txt");
        //Directories

        //BassTempSong
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
        //BassTempSong

        //UI Altering Props
        private bool mainPanelVisibleEnabledField = false;
        private bool MainPanelVisibleEnabled
        {
            get => mainPanelVisibleEnabledField;
            set
            {
                if (mainPanelVisibleEnabledField == value) return;
                mainPanelVisibleEnabledField = value;
                if (value)
                {
                    Helper.AnimateRotation(btn_options, OptionsBtnAnimationImage, 90, 10, 10);
                    Helper.AnimateSlideInFromTop(mainPanel);
                }
                else
                {
                    Helper.AnimateRotation(btn_options, OptionsBtnAnimationImage, -90, 10, 10);
                    Helper.AnimateSlideOutToTop(mainPanel);
                }
                mainPanel.Enabled = value;
            }
        }
        private bool suppressAutoPlayField = true;
        private bool SuppressAutoPlay
        {
            get => suppressAutoPlayField; set
            {
                suppressAutoPlayField = value;
                lbl_loadingtxt.Visible = value;
                track_list.Visible = !value;
                if (!MainPanelVisibleEnabled)
                {
                    track_list.Enabled = !value;
                    btn_play_pause.Enabled = !value;
                    btn_prevTrack.Enabled = !value;
                    btn_nextTrack.Enabled = !value;
                    btn_shuffleTrack.Enabled = !value;
                    btn_loopTrack.Enabled = !value;
                }
            }
        }
        //UI Altering Props

        //AudioDeviceChanged
        private MMDeviceEnumerator deviceEnumerator;
        private AudioDeviceWatcher deviceWatcher;
        //AudioDeviceChanged

        //FieldsAndProperties
        #endregion

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            lbl_volume.Text = "30%";
            track_volume.Value = 30;
            lbl_track_end.BringToFront();

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
            OptionsBtnAnimationImage = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconMoreOptions.png"), btn_options.Width, btn_options.Height);
            this.Icon = new Icon(Helper.IconsPath + "wildflowerico.ico");
            btn_play_pause.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconPlayButton.png"), btn_play_pause.Width, btn_play_pause.Height);
            btn_prevTrack.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconPreviousTrack.png"), btn_prevTrack.Width, btn_prevTrack.Height);
            btn_nextTrack.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconNextTrack.png"), btn_nextTrack.Width, btn_nextTrack.Height);
            btn_loopTrack.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconLoopTrack.png"), btn_loopTrack.Width, btn_loopTrack.Height);
            btn_shuffleTrack.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconShuffleTrack.png"), btn_shuffleTrack.Width, btn_shuffleTrack.Height);
            btn_options.Image = OptionsBtnAnimationImage;
            btn_goBack.Image = Helper.ResizeImage(Image.FromFile(Helper.IconsPath + "iconGoBack.png"), btn_goBack.Width, btn_goBack.Height);
            #endregion

            #region Extra
            //Extra
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            InitAudioWatcher();
            //Extra
            #endregion

            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
        }

        #region MusicLibraryDependentCode
        //MusicLibraryDependentCode
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
            if (track_list.InvokeRequired)
            {
                track_list.Invoke(() =>
                {
                    if (index >= 0 && index < track_list.Items.Count)
                        track_list.SelectedIndex = index;
                });
            }
            else
            {
                if (index >= 0 && index < track_list.Items.Count)
                    track_list.SelectedIndex = index;
            }
            isPlaying = true;
        }
        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (bassStream != 0 &&
                Bass.BASS_ChannelIsActive(bassStream) != BASSActive.BASS_ACTIVE_STOPPED &&
                !SuppressAutoPlay)
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
            if (Bass.BASS_ChannelIsActive(bassStream) == BASSActive.BASS_ACTIVE_STOPPED &&
                !isTransitioning &&
                paths != null &&
                paths.Length > 0 &&
                !SuppressAutoPlay)
            {
                isTransitioning = true;
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
                    isTransitioning = false;
                    return;
                }
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
                    await ShuffleTracksLogic();
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
                    stateTimer.Stop();
                    timer1.Stop();
                }
                if (!isPlaying)
                {
                    Bass.BASS_ChannelPlay(bassStream, false);
                    stateTimer.Start();
                    timer1.Start();
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
        //MusicLibraryDependentCode
        #endregion

        #region WinFormsEventsCode
        //WinFormsEventsCode
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (MainPanelVisibleEnabled)
                return base.ProcessCmdKey(ref msg, keyData);
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
            if (keyData == Keys.Escape)
            {
                if (Helper.IsAnimatingButton || Helper.IsAnimatingPanel) return true;
                btn_options_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            await InitializePlaylistPath();
            await LoadEverything();
            stateTimer.Start();
            timer1.Start();
        }
        private async void stateTimer_Tick(object sender, EventArgs e) => await SavePlaybackState();
        private void p_bar_MouseMove(object sender, MouseEventArgs e)
        {
            int hoverMs = p_bar.Maximum * e.X / p_bar.Width;
            string timeStr = TimeSpan.FromMilliseconds(hoverMs).ToString(@"mm\:ss");
            hoverTimeLabel.Text = timeStr;
            hoverTimeLabel.Location = p_bar.PointToScreen(e.Location);
            hoverTimeLabel.Location = this.PointToClient(hoverTimeLabel.Location);
            hoverTimeLabel.Top -= 22;
            if (hoverMs > p_bar.Maximum * 0.9) hoverTimeLabel.Left -= 42;
            hoverTimeLabel.Visible = true;
            hoverTimeLabel.BringToFront();
        }
        private void p_bar_MouseLeave(object sender, EventArgs e) => hoverTimeLabel.Visible = false;
        private void btn_nextTrack_Click(object sender, EventArgs e)
        {
            if (paths == null || paths.Length == 0) return;
            if (currentIndex < paths.Length - 1)
            {
                currentIndex++;
                PlayTrack(currentIndex);
            }
        }
        private void btn_prevTrack_Click(object sender, EventArgs e)
        {
            if (paths == null || paths.Length == 0) return;
            if (currentIndex > 0)
            {
                currentIndex--;
                PlayTrack(currentIndex);
            }
        }
        private void btn_loopTrack_Click(object sender, EventArgs e)
        {
            isLooped = !isLooped;
        }
        private async void btn_shuffleTrack_DoubleClick(object sender, EventArgs e)
        {
            await ShuffleTracksLogic();
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
            if (Helper.IsAnimatingButton || Helper.IsAnimatingPanel) return;
            if (isPlaying)
            {
                btn_play_pause_Click(sender, e);
            }

            Options f2 = new Options();

            f2.OpenPressed += async (s, args) =>
            {
                await AddPlaylistLogic();
                PanelEnabledVisible(false);
            };

            f2.UpdatePressed += async (s, args) =>
            {
                if (SuppressAutoPlay) return;
                if (paths == null || paths.Length == 0)
                {
                    MessageBox.Show("Nowhere to update from");
                    return;
                }
                PanelEnabledVisible(false);
                await RefreshPlaylist();
                await SavePlaybackState();
                PlayTrack(currentIndex, resumeTimeMs);
            };

            f2.SearchPressed += (s, args) =>
            {
                if (isPlaying)
                {
                    btn_play_pause_Click(sender, EventArgs.Empty);
                }
                SearchButtonPressed();
            };

            f2.PlaylistPressed += (s, args) =>
            {
                if (isPlaying)
                {
                    btn_play_pause_Click(sender, EventArgs.Empty);
                }
                PlayListButtonPressed();
            };
            f2.CloseRequest += (e, args) =>
            {
                PanelEnabledVisible(false);
            };
            LoadFormIntoPanel(f2);
        }
        private void Form1_Click(object sender, EventArgs e)
        {
            if (MainPanelVisibleEnabled)
            {
                if (Helper.IsAnimatingButton || Helper.IsAnimatingPanel) return;
                PanelEnabledVisible(false);
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
        //WinFormsEventsCode
        #endregion

        #region Form1Logic
        //Form1Logic

        #region SongInitLogic
        //SongInitLogic
        private async Task LoadEverything()
        {
            SuppressAutoPlay = true;
            if (!File.Exists(musicFolderPath))
            {
                MessageBox.Show("Please select a song folder");
                await AddPlaylistLogic();
                SuppressAutoPlay = false;
                return;
            }
            if (File.Exists(musicFolderPath) && !File.Exists(playlistSaveFile))
            {
                string savedPath = await File.ReadAllTextAsync(musicFolderPath);
                if (Directory.Exists(savedPath))
                {
                    musicFolder = savedPath;
                    await LoadSongsFromFolder(musicFolder);
                    SuppressAutoPlay = false;
                    await ShuffleTracksLogic();
                }
            }
            if (File.Exists(playbackStateFile) && File.Exists(playlistSaveFile))
            {
                await LoadPlaylistFromFile(playlistSaveFile);
                var part = await File.ReadAllTextAsync(playbackStateFile);
                var parts = part.Split('|');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int index) &&
                    long.TryParse(parts[1], out long time))
                {
                    currentIndex = index;
                    resumeTimeMs = time;
                    await RefreshPlaylist();
                    SuppressAutoPlay = false;
                    PlayTrack(currentIndex, resumeTimeMs);
                }
            }
            await SavePlaybackState();
        }
        private async Task InitializePlaylistPath()
        {
            string lastUsedFile = Path.Combine(playlistsDir, "lastUsed.txt");

            if (!Directory.Exists(playlistsDir))
                Directory.CreateDirectory(playlistsDir);

            string[] allPlaylists = Directory.GetDirectories(playlistsDir);

            string validPlaylistIndex = null;

            // 1. Check lastUsed.txt
            if (File.Exists(lastUsedFile))
            {
                string lastUsedFileData = await File.ReadAllTextAsync(lastUsedFile);
                string savedIndex = lastUsedFileData.Trim();
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
                await AddPlaylistLogic();
                return;
            }

            // 4. Save and assign
            await File.WriteAllTextAsync(lastUsedFile, validPlaylistIndex);
            basePlaylistPath = Path.Combine(playlistsDir, validPlaylistIndex);
        }
        private async Task LoadPlaylistFromFile(string playlistFilePath)
        {
            if (!File.Exists(playlistFilePath)) return;
            var lines = await File.ReadAllLinesAsync(playlistFilePath);
            paths = lines.ToArray();
        }
        private async Task LoadSongsFromFolder(string folderPath)
        {
            await Task.Run(() =>
                paths = Directory
                    .GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => f.EndsWith(".mp3") ||
                    f.EndsWith(".wav") ||
                    f.EndsWith(".flac") ||
                    f.EndsWith(".ogg"))
                    .ToArray());
            SuppressAutoPlay = true;
            await Helper.TrackListAdd(paths, track_list);
            SuppressAutoPlay = false;
        }
        private async Task ShuffleTracksLogic()
        {
            if (paths == null || paths.Length == 0) return;
            Random rng = new Random();
            await Task.Run(() =>
            {
                for (int i = paths.Length - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    (paths[i], paths[j]) = (paths[j], paths[i]);
                }
            });
            SuppressAutoPlay = true;
            await Helper.TrackListAdd(paths, track_list);
            SuppressAutoPlay = false;
            PlayTrack(0);
            await SavePlaylistToFile();
            await SavePlaybackState();
        }
        //SongInitLogic
        #endregion

        #region SongSaveLogic
        //SongSaveLogic
        private async Task SavePlaylistToFile()
        {
            if (paths == null || paths.Length == 0) return;
            await File.WriteAllLinesAsync(playlistSaveFile, paths);
        }
        private async Task SavePlaybackState()
        {
            if (paths == null || paths.Length == 0) return;

            int index = currentIndex;
            long time = Bass.BASS_ChannelGetPosition(bassStream);
            resumeTimeMs = time;
            track_list.SelectedIndex = currentIndex;
            await File.WriteAllTextAsync(playbackStateFile, $"{index}|{time}");
        }
        //SongSaveLogic
        #endregion

        //Form1Logic
        #endregion

        #region OptionsLogic
        //OptionsLogic

        #region Panel
        //Panel
        private void LoadFormIntoPanel(Form childForm)
        {
            if (mainPanel.Controls.Count > 0)
                mainPanel.Controls[0].Dispose();
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            mainPanel.Size = childForm.Size;
            childForm.BackColor = ColorTranslator.FromHtml("#1f1e33");
            mainPanel.Controls.Add(childForm);
            mainPanel.Tag = childForm;
            PanelEnabledVisible(true);
            childForm.Show();
        }
        private void PanelEnabledVisible(bool value)
        {
            MainPanelVisibleEnabled = value;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl == mainPanel ||
                    ctrl == this ||
                    ctrl == btn_goBack ||
                    ctrl == lbl_loadingtxt)
                {
                    continue;
                }
                ctrl.Enabled = !value;
            }
        }
        //Panel
        #endregion

        #region PlayListButtonPressed
        //PlayListButtonPressed
        private void PlayListButtonPressed()
        {
            Playlists f2 = new Playlists(playlistsDir, Path.GetFileName(basePlaylistPath));
            f2.Playlist2Play += async (e, Playlist2Play) =>
            {
                if (Path.GetFileName(basePlaylistPath) == Playlist2Play) return;
                basePlaylistPath = Path.Combine(playlistsDir, Playlist2Play);
                if (!File.Exists(basePlaylistPath + "\\musicFolderPath.txt"))
                {
                    if (!FindAvailablePlaylist())
                    {
                        MessageBox.Show("All playlists have been deleted");
                        paths = null;
                        track_list.Items.Clear();
                    }
                }
                await LoadEverything();
                await File.WriteAllTextAsync(Path.Combine(playlistsDir, "lastUsed.txt"), Playlist2Play);
                PanelEnabledVisible(false);
            };
            f2.CloseRequest += (e, args) =>
            {
                PanelEnabledVisible(false);
            };
            LoadFormIntoPanel(f2);
        }
        private bool FindAvailablePlaylist()
        {
            foreach (string dir in Directory.GetDirectories(playlistsDir))
            {
                string existingPathFile = Path.Combine(dir, "musicFolderPath.txt");
                if (File.Exists(existingPathFile))
                {
                    basePlaylistPath = dir;
                    File.WriteAllText(Path.Combine(playlistsDir, "lastUsed.txt"), Path.GetFileName(dir));
                    return true;
                }
            }
            return false;
        }
        //PlayListButtonPressed
        #endregion

        #region SearchButtonPressed
        //SearchButtonPressed
        private void SearchButtonPressed()
        {
            if (paths == null || paths.Length == 0)
            {
                MessageBox.Show("Nowhere to search from");
                return;
            }
            Search f2 = new Search(paths);
            f2.SongToPlay += async (e, songToPlay) =>
            {
                if (paths == null || songToPlay == null) return;
                SuppressAutoPlay = true;
                bassTempSongIndex = await Task.Run(() => Array.FindIndex(paths, f => f.Contains(songToPlay)));
                SuppressAutoPlay = false;
                BassTempIsPlaying = true;
                PanelEnabledVisible(false);
            };
            f2.CloseRequest += (e, args) =>
            {
                PanelEnabledVisible(false);
            };
            LoadFormIntoPanel(f2);
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
                var Metadata = Helper.GetMetadataFromFile(paths[bassTempSongIndex]);
                lbl_tempSongName.Text = Metadata.title + Metadata.artist;
            }
            else
            {
                lbl_tempSongName.Text = string.Empty;
            }
            mainPanel.Visible = false;
        }
        //SearchButtonPressed
        #endregion

        #region AddPlaylistLogic
        //AddPlaylistLogic
        private async Task AddPlaylistLogic()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            var fbdShowDialog = fbd.ShowDialog();
            while (fbdShowDialog != DialogResult.OK && basePlaylistPath == null)
            {
                fbdShowDialog = fbd.ShowDialog();
            }
            musicFolder = fbd.SelectedPath;
            foreach (string dir in Directory.GetDirectories(playlistsDir))
            {
                string existingPathFile = Path.Combine(dir, "musicFolderPath.txt");
                if (File.Exists(existingPathFile))
                {
                    string existingPath = await File.ReadAllTextAsync(existingPathFile);
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

            await File.WriteAllTextAsync(Path.Combine(playlistsDir, "lastUsed.txt"), nextIndex.ToString());

            await File.WriteAllTextAsync(Path.Combine(newPlaylistDir, "musicFolderPath.txt"), musicFolder);
            basePlaylistPath = newPlaylistDir;
            await LoadEverything();
        }
        //AddPlaylistLogic
        #endregion

        #region RefreshPlaylist
        //RefreshPlaylist
        private async Task RefreshPlaylist()
        {
            musicFolder = await File.ReadAllTextAsync(musicFolderPath);
            if (string.IsNullOrEmpty(musicFolder) || !Directory.Exists(musicFolder))
            {
                MessageBox.Show("Update your music folder path");
                RemoveInvalidPlaylists();
                await AddPlaylistLogic();
                if (paths == null)
                {
                    await InitializePlaylistPath();
                    await LoadEverything();
                }
                return;
            }
            await CleanMissingTracks();
            await UpdatePlaylistWithNewSongs();
        }
        private async Task UpdatePlaylistWithNewSongs()
        {
            List<string> newSongs = new List<string>();
            await Task.Run(() =>
            {
                var currentPathsSet = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
                var allSongsInFolder = Directory.GetFiles(musicFolder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => f.EndsWith(".mp3") || f.EndsWith(".wav") || f.EndsWith(".flac") || f.EndsWith(".ogg"))
                    .ToList();
                newSongs = allSongsInFolder.Where(f => !currentPathsSet.Contains(f)).ToList();
            });
            if (newSongs.Count == 0) return;
            paths = paths.Concat(newSongs).ToArray();
            SuppressAutoPlay = true;
            await Helper.TrackListAdd(newSongs.ToArray(), track_list, false);
            SuppressAutoPlay = false;
            await File.WriteAllLinesAsync(playlistSaveFile, paths);
        }
        private async Task CleanMissingTracks()
        {
            int removedBeforeCurrent = 0;
            List<string> validPaths = new List<string>();
            await Task.Run(() =>
            {
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
            });
            SuppressAutoPlay = true;
            await Helper.TrackListAdd(validPaths.ToArray(), track_list);
            SuppressAutoPlay = false;
            currentIndex = Math.Max(0, currentIndex - removedBeforeCurrent);
            paths = validPaths.ToArray();
            await File.WriteAllLinesAsync(playlistSaveFile, paths);
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
        //RefreshPlaylist
        #endregion

        //OptionsLogic
        #endregion

        #region Extra
        //Extra
        private void InitAudioWatcher()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            deviceWatcher = new AudioDeviceWatcher();
            deviceWatcher.DefaultDeviceChanged += () =>
            {
                if (isPlaying)
                {
                    btn_play_pause_Click(this, EventArgs.Empty); // pause
                }
            };
            deviceEnumerator.RegisterEndpointNotificationCallback(deviceWatcher);
        }
        private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                await SavePlaybackState();
                if (isPlaying) btn_play_pause_Click(sender, e);
            }
        }
        //Extra
        #endregion
    }
}