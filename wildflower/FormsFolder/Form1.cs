using Microsoft.Win32;
using NAudio.CoreAudioApi;
using wildflower.Services.Library;
using wildflower.Services.Playback;
using wildflower.Services.Playlist;
using wildflower.Services.Search;
using wildflower.Services.Session;

namespace wildflower
{
    public partial class Form1 : Form
    {
        #region FieldsAndProperties
        private readonly Label hoverTimeLabel = new Label();
        private readonly Image OptionsBtnAnimationImage;
        private readonly IMetadataService metadataService;
        private readonly ISearchService searchService;
        private readonly IPlayerSessionService playerSession;
        private short shuffleClickCounter = 0;
        private bool temporaryPlaybackUiApplied = false;

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
        public bool SuppressAutoPlay
        {
            get => suppressAutoPlayField;
            set
            {
                suppressAutoPlayField = value;
                lbl_loadingtxt.Visible = value;
                track_list.Visible = !value && !playerSession.IsTemporaryPlayback;
                if (!MainPanelVisibleEnabled)
                {
                    track_list.Enabled = !value && !playerSession.IsTemporaryPlayback;
                    btn_play_pause.Enabled = !value;
                    btn_prevTrack.Enabled = !value && !playerSession.IsTemporaryPlayback;
                    btn_nextTrack.Enabled = !value && !playerSession.IsTemporaryPlayback;
                    btn_shuffleTrack.Enabled = !value && !playerSession.IsTemporaryPlayback;
                    btn_loopTrack.Enabled = !value;
                }
            }
        }

        private MMDeviceEnumerator deviceEnumerator = null!;
        private AudioDeviceWatcher deviceWatcher = null!;
        #endregion

        public Form1()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            metadataService = new TagLibMetadataService();
            var scanner = new MusicLibraryScanner();
            var playlistStorage = new FilePlaylistStorage();
            var playlistService = new PlaylistService(playlistStorage);
            var playbackEngine = new BassPlaybackEngine();
            searchService = new SearchService(metadataService);
            playerSession = new PlayerSessionService(playbackEngine, playlistService, scanner, metadataService);

            lbl_volume.Text = "30%";
            track_volume.Value = 30;
            lbl_track_end.BringToFront();

            hoverTimeLabel.AutoSize = true;
            hoverTimeLabel.BackColor = Color.Black;
            hoverTimeLabel.ForeColor = Color.White;
            hoverTimeLabel.Padding = new Padding(4);
            hoverTimeLabel.Visible = false;
            hoverTimeLabel.Font = new Font("Segoe UI", 8);
            hoverTimeLabel.BringToFront();
            Controls.Add(hoverTimeLabel);

            OptionsBtnAnimationImage = Helper.LoadIconImage("iconMoreOptions.png", btn_options.Width, btn_options.Height);
            Icon = new Icon(Path.Combine(Helper.IconsPath, "wildflowerico.ico"));
            SetPictureBoxImage(btn_play_pause, Helper.LoadIconImage("iconPlayButton.png", btn_play_pause.Width, btn_play_pause.Height));
            SetPictureBoxImage(btn_prevTrack, Helper.LoadIconImage("iconPreviousTrack.png", btn_prevTrack.Width, btn_prevTrack.Height));
            SetPictureBoxImage(btn_nextTrack, Helper.LoadIconImage("iconNextTrack.png", btn_nextTrack.Width, btn_nextTrack.Height));
            SetPictureBoxImage(btn_loopTrack, Helper.LoadIconImage("iconLoopTrack.png", btn_loopTrack.Width, btn_loopTrack.Height));
            SetPictureBoxImage(btn_shuffleTrack, Helper.LoadIconImage("iconShuffleTrack.png", btn_shuffleTrack.Width, btn_shuffleTrack.Height));
            btn_options.Image = OptionsBtnAnimationImage;
            SetPictureBoxImage(btn_goBack, Helper.LoadIconImage("iconGoBack.png", btn_goBack.Width, btn_goBack.Height));
            SetPictureBoxImage(btn_fullSongName, Helper.LoadIconImage("iconFullSongName.png", btn_fullSongName.Width, btn_fullSongName.Height));

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            FormClosing += Form1_FormClosing;
            FormClosed += Form1_FormClosed;
            InitAudioWatcher();

            if (!playerSession.InitializePlaybackEngine())
            {
                MessageBox.Show("Could not initialize audio playback.", "wildflower", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #region MusicLibraryDependentCode
        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (SuppressAutoPlay || !playerSession.HasTracks)
                return;

            RemoveGhostPanel();
            UpdateProgressUi(playerSession.GetProgress());
            SessionActionResult result = await playerSession.AdvanceIfStoppedAsync();
            await ApplySessionResultAsync(result);
        }

        private void btn_play_pause_Click(object sender, EventArgs e)
        {
            if (!playerSession.HasTracks) return;
            playerSession.TogglePlayPause();
            UpdatePlayPauseIcon();
        }

        private void p_bar_MouseDown(object sender, MouseEventArgs e)
        {
            if (!playerSession.HasTracks || p_bar.Width <= 0) return;
            int seekMs = p_bar.Maximum * e.X / p_bar.Width;
            playerSession.SeekToMilliseconds(seekMs);
        }

        private void track_volume_Scroll(object sender, EventArgs e)
        {
            lbl_volume.Text = track_volume.Value + "%";
            playerSession.SetVolume(track_volume.Value / 100f);
        }
        #endregion

        #region WinFormsEventsCode
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (MainPanelVisibleEnabled)
                return base.ProcessCmdKey(ref msg, keyData);
            if (keyData == Keys.Escape && playerSession.IsTemporaryPlayback)
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
            if (SuppressAutoPlay)
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
            if (keyData == Keys.Left && !playerSession.IsTemporaryPlayback)
            {
                btn_prevTrack_Click(this, EventArgs.Empty);
                return true;
            }
            if (keyData == Keys.Right && !playerSession.IsTemporaryPlayback)
            {
                btn_nextTrack_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            SessionActionResult result = await InitializePlayerSessionAsync();
            await ApplySessionResultAsync(result, rebuildTrackList: true);
            stateTimer.Start();
            timer1.Start();
        }

        private async void stateTimer_Tick(object sender, EventArgs e) => await playerSession.SavePlaybackStateAsync();

        private void p_bar_MouseMove(object sender, MouseEventArgs e)
        {
            if (p_bar.Width <= 0) return;
            int hoverMs = p_bar.Maximum * e.X / p_bar.Width;
            hoverTimeLabel.Text = TimeSpan.FromMilliseconds(hoverMs).ToString(@"mm\:ss");
            int x = e.X;
            if (x + 1.3 * hoverTimeLabel.Width > Width) x -= hoverTimeLabel.Width;
            hoverTimeLabel.Location = new Point(x, p_bar.Location.Y - hoverTimeLabel.Height);
            hoverTimeLabel.Visible = true;
            hoverTimeLabel.BringToFront();
        }

        private void p_bar_MouseLeave(object sender, EventArgs e) => hoverTimeLabel.Visible = false;

        private void btn_nextTrack_Click(object sender, EventArgs e)
        {
            if (playerSession.NextTrack())
                SyncPlaybackUi();
        }

        private void btn_prevTrack_Click(object sender, EventArgs e)
        {
            if (playerSession.PreviousTrack())
                SyncPlaybackUi();
        }

        private void btn_loopTrack_Click(object sender, EventArgs e)
        {
            playerSession.SetLooped(!playerSession.IsLooped);
            UpdateLoopIcon();
        }

        private async void btn_shuffleTrack_DoubleClick(object sender, EventArgs e)
        {
            SessionActionResult result = await playerSession.ShuffleTracksAsync();
            await ApplySessionResultAsync(result);
            shuffleClickCounter = 0;
        }

        private void btn_shuffleTrack_Click(object sender, EventArgs e)
        {
            shuffleClickCounter++;
            if (shuffleClickCounter > 2)
            {
                shuffleClickCounter = 0;
                MessageBox.Show("Shuffle works only on doubleclick", "wildflower", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btn_options_Click(object sender, EventArgs e)
        {
            if (Helper.IsAnimatingButton || Helper.IsAnimatingPanel) return;
            if (playerSession.IsPlaying)
            {
                playerSession.TogglePlayPause();
                UpdatePlayPauseIcon();
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
                if (!playerSession.HasTracks)
                {
                    MessageBox.Show("Nowhere to update from", "wildflower", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                PanelEnabledVisible(false);
                SessionActionResult result = await playerSession.RefreshPlaylistAndRestoreAsync();
                await ApplySessionResultAsync(result);
            };

            f2.SearchPressed += (s, args) =>
            {
                SearchButtonPressed();
            };

            f2.PlaylistPressed += async (s, args) =>
            {
                await PlayListButtonPressed();
            };

            f2.OpenSaveFolderPressed += (s, args) =>
            {
                string? saveFolder = playerSession.CurrentPlaylist?.DirectoryPath;
                if (saveFolder == null || !Directory.Exists(saveFolder))
                {
                    MessageBox.Show("No playlist folder selected", "wildflower", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                System.Diagnostics.Process.Start("explorer.exe", saveFolder);
                PanelEnabledVisible(false);
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

        private async void btn_goBack_Click(object sender, EventArgs e)
        {
            SessionActionResult result = await playerSession.ReturnFromTemporaryPlaybackAsync();
            await ApplySessionResultAsync(result);
        }

        private void track_list_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = track_list.IndexFromPoint(e.Location);
            if (index == ListBox.NoMatches) return;
            if (index < 0 || index >= playerSession.Tracks.Count) return;
            if (!playerSession.IsTemporaryPlayback && index == playerSession.CurrentIndex) return;

            if (playerSession.PlayTrack(index))
                SyncPlaybackUi();
        }

        private void track_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (playerSession.IsTemporaryPlayback) return;
            if (track_list.SelectedIndex < 0 || track_list.SelectedIndex >= track_list.Items.Count) return;

            btn_fullSongName.Visible = Helper.IsItemClipped(track_list);
            if (lbl_tempSongName.Text != track_list.Items[track_list.SelectedIndex].ToString())
            {
                track_list.Visible = true;
                lbl_tempSongName.Visible = false;
            }
        }

        private void btn_fullSongName_Click(object sender, EventArgs e)
        {
            if (track_list.SelectedIndex < 0 || track_list.SelectedIndex >= track_list.Items.Count) return;

            track_list.Visible = false;
            lbl_tempSongName.Visible = true;
            lbl_tempSongName.Text = track_list.Items[track_list.SelectedIndex].ToString();
        }

        private void btn_fullSongName_MouseLeave(object sender, EventArgs e)
        {
            if (playerSession.IsTemporaryPlayback) return;
            track_list.Visible = true;
            lbl_tempSongName.Visible = false;
        }
        #endregion

        #region Form1Logic
        private async Task ApplySessionResultAsync(SessionActionResult result, bool rebuildTrackList = false)
        {
            if (result.MissingMusicFolder)
            {
                MessageBox.Show(result.Message ?? "Update your music folder path", "wildflower", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await AddPlaylistLogic();
                return;
            }

            if (result.ClearedPlaylist)
            {
                track_list.Items.Clear();
                p_bar.Value = 0;
                lbl_track_start.Text = "00:00";
                lbl_track_end.Text = "00:00";
            }

            if (rebuildTrackList || result.TrackListChanged)
                await RebuildTrackListAsync();

            if (result.TemporaryPlaybackChanged)
                ApplyTemporaryPlaybackUi(playerSession.IsTemporaryPlayback, result.TemporaryTrackDisplayName);

            bool syncTrackSelection = rebuildTrackList ||
                                      result.TrackListChanged ||
                                      result.PlaybackChanged ||
                                      result.TemporaryPlaybackChanged ||
                                      result.ClearedPlaylist;
            SyncPlaybackUi(syncTrackSelection);
        }

        private async Task RebuildTrackListAsync()
        {
            SuppressAutoPlay = true;
            track_list.Items.Clear();

            var displayNames = await playerSession.GetTrackDisplayNamesAsync();
            foreach (string displayName in displayNames)
                track_list.Items.Add(displayName);

            SuppressAutoPlay = false;
        }

        private async Task<SessionActionResult> InitializePlayerSessionAsync()
        {
            PlayerSessionInitializationResult initializationResult = await playerSession.InitializeAsync();
            if (initializationResult.Loaded)
                return initializationResult.SessionResult;

            if (initializationResult.Failed)
            {
                MessageBox.Show(
                    initializationResult.Message ?? "Could not initialize the player session.",
                    "wildflower",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return initializationResult.SessionResult;
            }

            if (initializationResult.SessionResult.MissingMusicFolder)
            {
                MessageBox.Show(
                    initializationResult.Message ?? "Update your music folder path",
                    "wildflower",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            string? musicFolder = await PromptForMusicFolderAsync(requireSelection: true);
            if (musicFolder == null)
                return new SessionActionResult(false, Message: "No playlist selected.");

            SessionActionResult addResult = await playerSession.AddPlaylistAsync(musicFolder);
            if (!addResult.Succeeded)
            {
                MessageBox.Show(
                    addResult.Message ?? "Could not add playlist.",
                    "wildflower",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            return addResult;
        }

        private void SyncPlaybackUi(bool syncTrackSelection = true)
        {
            if (syncTrackSelection)
                SyncSelectedTrack();

            UpdatePlayPauseIcon();
            UpdateLoopIcon();
            UpdateProgressUi(playerSession.GetProgress());
        }

        private void SyncSelectedTrack()
        {
            if (!playerSession.HasTracks || track_list.Items.Count == 0)
            {
                track_list.SelectedIndex = -1;
                btn_fullSongName.Visible = false;
                return;
            }

            int index = playerSession.CurrentIndex;
            if (index < 0 || index >= track_list.Items.Count)
                return;

            if (track_list.SelectedIndex != index)
                track_list.SelectedIndex = index;

            track_list.TopIndex = index;
        }

        private void UpdateProgressUi(Models.PlaybackProgress progress)
        {
            int maximum = Math.Max(1, progress.LengthMilliseconds);
            p_bar.Maximum = maximum;
            p_bar.Value = Math.Min(progress.PositionMilliseconds, maximum);
            lbl_track_start.Text = TimeSpan.FromMilliseconds(progress.PositionMilliseconds).ToString(@"mm\:ss");
            lbl_track_end.Text = TimeSpan.FromMilliseconds(progress.LengthMilliseconds).ToString(@"mm\:ss");
        }

        private async Task<string?> PromptForMusicFolderAsync(bool requireSelection)
        {
            using FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            while (requireSelection && result != DialogResult.OK)
                result = fbd.ShowDialog();

            return await Task.FromResult(result == DialogResult.OK ? fbd.SelectedPath : null);
        }

        private async Task AddPlaylistLogic()
        {
            string? musicFolder = await PromptForMusicFolderAsync(requireSelection: playerSession.CurrentPlaylist == null);
            if (musicFolder == null) return;

            SessionActionResult result = await playerSession.AddPlaylistAsync(musicFolder);
            if (!result.Succeeded)
            {
                MessageBox.Show(result.Message ?? "Could not add playlist.", "wildflower", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await ApplySessionResultAsync(result, rebuildTrackList: true);
        }
        #endregion

        #region OptionsLogic
        private void RemoveGhostPanel()
        {
            if (mainPanel.Visible && !mainPanel.Enabled && !Helper.IsAnimatingPanel)
            {
                mainPanel.Visible = false;
                mainPanel.Enabled = false;
                PanelEnabledVisible(false);
            }
            if (!MainPanelVisibleEnabled && !Helper.IsAnimatingButton && Helper.CurrentAngle != 0f)
            {
                Helper.AnimateRotation(btn_options, OptionsBtnAnimationImage, -Helper.CurrentAngle, 10, 10);
            }
        }

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
            foreach (Control ctrl in Controls)
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

        private async Task PlayListButtonPressed()
        {
            var playlists = await playerSession.GetPlaylistsAsync();
            Playlists f2 = new Playlists(playlists, playerSession.CurrentPlaylist?.Id ?? string.Empty);

            f2.Playlist2Play += async (e, playlistToPlay) =>
            {
                if (SuppressAutoPlay) return;
                SessionActionResult result = await playerSession.SelectPlaylistAsync(playlistToPlay);
                await ApplySessionResultAsync(result, rebuildTrackList: result.TrackListChanged);
                PanelEnabledVisible(false);
            };

            f2.PlaylistDeleteRequested += async (e, playlistToDelete) =>
            {
                DeletePlaylistResult result = await playerSession.DeletePlaylistAsync(playlistToDelete);
                if (!result.HasAnyPlaylist)
                {
                    MessageBox.Show(result.Message ?? "All playlists have been deleted", "wildflower", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await ApplySessionResultAsync(result.SessionResult, rebuildTrackList: true);
                    PanelEnabledVisible(false);
                    return;
                }

                if (result.DeletedCurrentPlaylist)
                {
                    await ApplySessionResultAsync(result.SessionResult, rebuildTrackList: true);
                    PanelEnabledVisible(false);
                    return;
                }

                f2.SetPlaylists(await playerSession.GetPlaylistsAsync(), playerSession.CurrentPlaylist?.Id ?? string.Empty);
            };

            f2.CloseRequest += (e, args) =>
            {
                PanelEnabledVisible(false);
            };
            LoadFormIntoPanel(f2);
        }

        private void SearchButtonPressed()
        {
            if (!playerSession.HasTracks)
            {
                MessageBox.Show("Nowhere to search from", "wildflower", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Search f2 = new Search(playerSession.Tracks, searchService, metadataService, () => SuppressAutoPlay);
            f2.SongToPlay += async (e, songToPlay) =>
            {
                if (songToPlay == null) return;
                SuppressAutoPlay = true;
                SessionActionResult result = await playerSession.PlayTemporaryTrackAsync(songToPlay);
                SuppressAutoPlay = false;
                await ApplySessionResultAsync(result);
                PanelEnabledVisible(false);
            };
            f2.CloseRequest += (e, args) =>
            {
                PanelEnabledVisible(false);
            };
            LoadFormIntoPanel(f2);
        }

        private void ApplyTemporaryPlaybackUi(bool tempSongIsPlaying, string? tempSongName = null)
        {
            if (temporaryPlaybackUiApplied == tempSongIsPlaying)
            {
                if (tempSongIsPlaying && tempSongName != null)
                    lbl_tempSongName.Text = tempSongName;
                return;
            }

            temporaryPlaybackUiApplied = tempSongIsPlaying;
            lbl_tempSongName.Visible = tempSongIsPlaying;

            if (!tempSongIsPlaying)
            {
                SyncSelectedTrack();
                track_list_SelectedIndexChanged(this, EventArgs.Empty);
            }
            else
            {
                btn_fullSongName.Visible = false;
            }

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
                Width -= 90;
                lbl_tempSongName.Left -= 50;
                lbl_tempSongName.Top -= 15;
                stateTimer.Stop();
                lbl_tempSongName.Text = tempSongName ?? string.Empty;
            }
            else
            {
                Width += 90;
                lbl_tempSongName.Left += 50;
                lbl_tempSongName.Top += 15;
                stateTimer.Start();
                lbl_tempSongName.Text = string.Empty;
            }

            mainPanel.Visible = false;
        }
        #endregion

        #region Extra
        private void InitAudioWatcher()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            deviceWatcher = new AudioDeviceWatcher();
            deviceWatcher.DefaultDeviceChanged += () =>
            {
                RunOnUiThread(() =>
                {
                    if (playerSession.IsPlaying)
                    {
                        playerSession.TogglePlayPause();
                        UpdatePlayPauseIcon();
                    }
                });
            };
            deviceEnumerator.RegisterEndpointNotificationCallback(deviceWatcher);
        }

        private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                await playerSession.SavePlaybackStateAsync();
                RunOnUiThread(() =>
                {
                    if (playerSession.IsPlaying)
                    {
                        playerSession.TogglePlayPause();
                        UpdatePlayPauseIcon();
                    }
                });
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            stateTimer.Stop();
            try
            {
                playerSession.SavePlaybackStateAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Best-effort save on shutdown.
            }
        }

        private void Form1_FormClosed(object? sender, FormClosedEventArgs e)
        {
            FormClosing -= Form1_FormClosing;
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            deviceEnumerator.UnregisterEndpointNotificationCallback(deviceWatcher);
            deviceEnumerator.Dispose();
            playerSession.Dispose();
            OptionsBtnAnimationImage.Dispose();
        }

        private void RunOnUiThread(Action action)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                BeginInvoke(action);
                return;
            }

            action();
        }

        private void SetPictureBoxImage(PictureBox pictureBox, Image image)
        {
            Image? previous = pictureBox.Image;
            pictureBox.Image = image;
            if (previous != null && !ReferenceEquals(previous, image) && !ReferenceEquals(previous, OptionsBtnAnimationImage))
                previous.Dispose();
        }

        private void UpdatePlayPauseIcon()
        {
            SetPictureBoxImage(
                btn_play_pause,
                Helper.LoadIconImage(playerSession.IsPlaying ? "iconPauseButton.png" : "iconPlayButton.png", btn_play_pause.Width, btn_play_pause.Height));
        }

        private void UpdateLoopIcon()
        {
            SetPictureBoxImage(
                btn_loopTrack,
                Helper.LoadIconImage(playerSession.IsLooped ? "iconUnLoopTrack.png" : "iconLoopTrack.png", btn_loopTrack.Width, btn_loopTrack.Height));
        }
        #endregion
    }
}
