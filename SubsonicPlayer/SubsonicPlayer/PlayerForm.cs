using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.IO;
using NAudio.Wave;
using System.Threading;
using SubsonicAPI;

namespace WindowsFormsApplication1
{
    public partial class PlayerForm : Form
    {
        public PlayerForm()
        {
            InitializeComponent();
            Subsonic.appName = "IansCsharpApp";
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            string server = tbServer.Text;
            string user = tbUser.Text;
            string password = tbPassword.Text;

            string result = Subsonic.LogIn(server, user, password);

            tbResults.Text = result;

            btnGetSongs.Enabled = true;
        }

        Dictionary<string, string> artists;

        private void btnGetSongs_Click(object sender, EventArgs e)
        {
            artists = Subsonic.GetIndexes();
            tvArtists.BeginUpdate();

            string firstLetter = "";
            int treeIndex = -1;
            foreach (KeyValuePair<string, string> kvp in artists)
            {
                string thisFirstLetter = GetFirstLetter(kvp.Key);
                if (thisFirstLetter != firstLetter)
                {
                    firstLetter = thisFirstLetter;
                    tvArtists.Nodes.Add(firstLetter);
                    treeIndex++;
                }

                tvArtists.Nodes[treeIndex].Nodes.Add(kvp.Key);
            }

            tvArtists.EndUpdate();
        }

        private string GetFirstLetter(string name)
        {
            name = name.ToUpper();
            string theFirstLetter = "";
            if (name.StartsWith("THE "))
                theFirstLetter = name.Substring(4, 1);
            else if (name.StartsWith("LOS "))
                theFirstLetter = name.Substring(4, 1);
            else if (name.StartsWith("LAS "))
                theFirstLetter = name.Substring(4, 1);
            else if (name.StartsWith("LA "))
                theFirstLetter = name.Substring(3, 1);
            else
                theFirstLetter = name.Substring(0, 1); // No leading word

            return theFirstLetter.ToUpper();
        }

        private void tvArtists_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string theArtist = tvArtists.SelectedNode.Text;

            if (artists.ContainsKey(theArtist))
            {
                string theID = artists[theArtist];

                UpdateAlbumListView(theID);
            }
        }

        Stack<string> _AlbumListHistory;

        public Stack<string> AlbumListHistory
        {
            get
            {
                if (_AlbumListHistory == null)
                {
                    _AlbumListHistory = new Stack<string>();
                    _AlbumListHistory.Push("Root");
                }
                return _AlbumListHistory;
            }
            set
            {
                _AlbumListHistory = value;
            }
        }

        string _LastAlbumId;

        public string LastAlbumId
        {
            get
            {
                if (string.IsNullOrEmpty(_LastAlbumId))
                    _LastAlbumId = "Root";
                return _LastAlbumId;
            }
            set
            {
                _LastAlbumId = value;
            }
        }

        private void UpdateAlbumListView(string theID)
        {
            LastAlbumId = theID;

            MusicFolder FolderContents = Subsonic.GetMusicDirectory(theID);

            lbAlbums.BeginUpdate();
            lbAlbums.Items.Clear();

            if (AlbumListHistory.Peek() != "Root")
                lbAlbums.Items.Add(new MusicFolder("..", AlbumListHistory.Peek()));

            foreach (MusicFolder mf in FolderContents.Folders)
            {
                lbAlbums.Items.Add(mf);
            }

            foreach (Song sg in FolderContents.Songs)
            {
                lbAlbums.Items.Add(sg);
            }

            lbAlbums.EndUpdate();
        }

        MusicPlayer _thePlayer;

        private MusicPlayer thePlayer
        {
            get
            {
                if (_thePlayer == null)
                    _thePlayer = new MusicPlayer(this);
                return _thePlayer;
            }
            set
            {
                _thePlayer = value;
            }
        }

        private void lbAlbums_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            SubsonicItem theItem = (SubsonicItem)lbAlbums.SelectedItem;
            if (theItem.ItemType == SubsonicItem.SubsonicItemType.Folder)
            {
                if (theItem.Name == "..")
                    AlbumListHistory.Pop();
                else
                    AlbumListHistory.Push(LastAlbumId);
                UpdateAlbumListView(theItem.id);
            }
            else if (theItem.ItemType == SubsonicItem.SubsonicItemType.Song)
            {
                thePlayer.addToPlaylist((Song)theItem);
                thePlayer.play();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            thePlayer.pause();
        }

        public void updatePlaylist(Queue<Song> playlist)
        {
            lbPlaylist.BeginUpdate();
            lbPlaylist.Items.Clear();

            foreach (Song sng in playlist)
            {
                lbPlaylist.Items.Add(sng);
            }

            lbPlaylist.EndUpdate();
        }

        public void updateSongProgress(int progress)
        {
            if (progress > 100)
                progress = 100;
            pbSongProgress.Value = progress;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            thePlayer.skipSong();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            thePlayer.stop();
        }
    }

    public class MusicPlayer
    {
        private Song currentSong;
        private Queue<Song> playlist;

        private BackgroundWorker playerThread;

        public PlaybackState playState;

        private PlayerForm mainForm;

        public MusicPlayer()
        {

        }

        public MusicPlayer(PlayerForm theMainForm)
        {
            mainForm = theMainForm;
        }

        /// <summary>
        /// Public method called by the main view to start playing the playlist
        /// </summary>
        public void play()
        {
            // If there is no backgroundworker initialized, do that
            if (playerThread == null)
            {
                playerThread = new BackgroundWorker();
                playerThread.DoWork += new DoWorkEventHandler(playerThread_DoWork);
                playerThread.ProgressChanged += new ProgressChangedEventHandler(playerThread_ProgressChanged);
                playerThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(playerThread_RunWorkerCompleted);
                playerThread.WorkerReportsProgress = true;

            }

            // Set state to playing
            this.playState = PlaybackState.Playing;

            // start playing the entire queue
            playQueue();
        }

        private void playQueue()
        {
            if (playlist.Count > 0 && this.playState == PlaybackState.Playing)
            {
                currentSong = playlist.Peek();

                if (waveOut == null || waveOut.PlaybackState != PlaybackState.Playing)
                    NewPlaySong();
                // If the player is not busy yet then start it
                if (!playerThread.IsBusy)
                    playerThread.RunWorkerAsync();
            }
            else
            {
                this.playState = PlaybackState.Stopped;
            }
        }

        IWavePlayer waveOut;
        WaveStream mainOutputStream;
        WaveChannel32 volumeStream;

        private void NewPlaySong()
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    return;
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                    return;
                }
            }
            else
            {
                CreateWaveOut();

                mainOutputStream = CreateInputStream();
                //trackBarPosition.Maximum = (int)mainOutputStream.TotalTime.TotalSeconds;
                //labelTotalTime.Text = String.Format("{0:00}:{1:00}", (int)mainOutputStream.TotalTime.TotalMinutes,
                //    mainOutputStream.TotalTime.Seconds);
                //trackBarPosition.TickFrequency = trackBarPosition.Maximum / 30;

                waveOut.Init(mainOutputStream);

                volumeStream.Volume = 15; //volumeSlider1.Volume;
                waveOut.Play();
            }
        }

        private WaveStream CreateInputStream()
        {
            WaveChannel32 inputStream;

            Stream stream = Subsonic.StreamSong(currentSong.id);

            // Try to move this filling of memory stream into the background...
            Stream ms = new MemoryStream();
            byte[] buffer = new byte[32768];
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                playerThread.ReportProgress(50);

                ms.Write(buffer, 0, read);
            }

            ms.Position = 0;

            WaveStream mp3Reader = new Mp3FileReader(ms);
            WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader);
            WaveStream blockAlignedStream = new BlockAlignReductionStream(pcmStream);
            inputStream = new WaveChannel32(blockAlignedStream);
            
            // we are not going into a mixer so we don't need to zero pad
            //((WaveChannel32)inputStream).PadWithZeroes = false;
            volumeStream = inputStream;

            //var meteringStream = new MeteringStream(inputStream, inputStream.WaveFormat.SampleRate / 10);
            //meteringStream.StreamVolume += new EventHandler<StreamVolumeEventArgs>(meteringStream_StreamVolume);

            return volumeStream;
        }

        private void CreateWaveOut()
        {
            CloseWaveOut();
            
            if (true)
            {
                WaveCallbackInfo callbackInfo = WaveCallbackInfo.FunctionCallback();
                WaveOut outputDevice = new WaveOut(callbackInfo);
                //outputDevice.NumberOfBuffers = 1;
                //outputDevice.DesiredLatency = latency;
                waveOut = outputDevice;
            }
        }

        private void CloseWaveOut()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
            }
            if (mainOutputStream != null)
            {
                // this one really closes the file and ACM conversion
                volumeStream.Close();
                volumeStream = null;
                // this one does the metering stream
                mainOutputStream.Close();
                mainOutputStream = null;
            }
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
        }

        /// <summary>
        /// Defines what should be done when the playerThread starts working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            //playSong();
            TrackPlayer();
        }

        /// <summary>
        /// Updates the main thread on the progress of the player thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playerThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mainForm.updateSongProgress(e.ProgressPercentage);
        }

        /// <summary>
        /// Defines what should be done when the player thread finishes working
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playerThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SongFinished();
        }

        private void SongFinished()
        {
            // Remove the top song from the playlist
            currentSong = playlist.Dequeue();
            // Update the mainform playlist
            mainForm.updatePlaylist(playlist);
            
            // Start playing the next song
            if (playState == PlaybackState.Playing)
                playQueue();
        }

        private void TrackPlayer()
        {
            while (waveOut != null && waveOut.PlaybackState != PlaybackState.Stopped)
            {
                int progress = (int)((double)mainOutputStream.CurrentTime.TotalSeconds * 100.0 / (double)mainOutputStream.TotalTime.TotalSeconds);
                playerThread.ReportProgress(progress);
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Public method to add a song to the playlist
        /// </summary>
        /// <param name="theSong"></param>
        public void addToPlaylist(Song theSong)
        {
            if (playlist == null)
                playlist = new Queue<Song>();
            playlist.Enqueue(theSong);

            // Update the playlist on the main form
            mainForm.updatePlaylist(playlist);
        }

        bool skipThis;

        /// <summary>
        /// Method that plays whatever the current song is
        /// </summary>
        private void playSong()
        {
            skipThis = false;
            Stream stream = Subsonic.StreamSong(currentSong.id);

            // Try to move this filling of memory stream into the background...
            Stream ms = new MemoryStream();
            byte[] buffer = new byte[32768];
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                playerThread.ReportProgress(50);

                ms.Write(buffer, 0, read);
            }

            ms.Position = 0;
            Mp3FileReader mp3Reader = new Mp3FileReader(ms);
            WaveStream blockAlignedStream =
                new BlockAlignReductionStream(
                    WaveFormatConversionStream.CreatePcmStream(mp3Reader));
            WaveOut waveOut; 
            waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
            waveOut.Init(blockAlignedStream);
            waveOut.Play();
            playState = PlaybackState.Playing;
            bool songEnd = false;
            while (playState != PlaybackState.Stopped && !songEnd && !skipThis)
            {
                if (waveOut.PlaybackState == PlaybackState.Stopped)
                    songEnd = true;
                else
                {
                    switch (playState)
                    {
                        case PlaybackState.Paused:
                            waveOut.Pause();
                            break;
                        case PlaybackState.Playing:
                            if (waveOut.PlaybackState != PlaybackState.Playing)
                                waveOut.Play();
                            else
                            {
                                int progress = (int)(100.0 * mp3Reader.CurrentTime.TotalSeconds / mp3Reader.TotalTime.TotalSeconds);
                                playerThread.ReportProgress(progress);
                                Thread.Sleep(100);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            //if (playState == PlaybackState.Stopped)                    
            waveOut.Stop();
            //waveOut.Dispose();
        }

        internal void pause()
        {
            //if (playState == PlaybackState.Playing)
            //    playState = PlaybackState.Paused;
            //else if (playState == PlaybackState.Paused)
            //    playState = PlaybackState.Playing;

            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    playState = PlaybackState.Paused;
                    waveOut.Pause();
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    playState = PlaybackState.Playing;
                    waveOut.Play();
                }
            }
        }

        internal void skipSong()
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Stop();
                }
            }
        }

        internal void stop()
        {
            playState = PlaybackState.Stopped;
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Stop();
                }
            }
        }
    }
}
