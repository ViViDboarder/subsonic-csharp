namespace WindowsFormsApplication1
{
    partial class PlayerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.tbUser = new System.Windows.Forms.TextBox();
            this.btnLogIn = new System.Windows.Forms.Button();
            this.tbResults = new System.Windows.Forms.RichTextBox();
            this.btnGetSongs = new System.Windows.Forms.Button();
            this.tvArtists = new System.Windows.Forms.TreeView();
            this.label5 = new System.Windows.Forms.Label();
            this.lbAlbums = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.lbPlaylist = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.pbSongProgress = new System.Windows.Forms.ProgressBar();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbServer
            // 
            this.tbServer.Location = new System.Drawing.Point(82, 35);
            this.tbServer.Name = "tbServer";
            this.tbServer.Size = new System.Drawing.Size(100, 20);
            this.tbServer.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "SubSonic Login";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Server:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "User:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 114);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Password:";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(82, 107);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(100, 20);
            this.tbPassword.TabIndex = 2;
            // 
            // tbUser
            // 
            this.tbUser.Location = new System.Drawing.Point(82, 75);
            this.tbUser.Name = "tbUser";
            this.tbUser.Size = new System.Drawing.Size(100, 20);
            this.tbUser.TabIndex = 1;
            // 
            // btnLogIn
            // 
            this.btnLogIn.Location = new System.Drawing.Point(23, 150);
            this.btnLogIn.Name = "btnLogIn";
            this.btnLogIn.Size = new System.Drawing.Size(75, 23);
            this.btnLogIn.TabIndex = 3;
            this.btnLogIn.Text = "Log In";
            this.btnLogIn.UseVisualStyleBackColor = true;
            this.btnLogIn.Click += new System.EventHandler(this.btnLogIn_Click);
            // 
            // tbResults
            // 
            this.tbResults.Location = new System.Drawing.Point(23, 499);
            this.tbResults.Name = "tbResults";
            this.tbResults.Size = new System.Drawing.Size(278, 131);
            this.tbResults.TabIndex = 99;
            this.tbResults.Text = "";
            // 
            // btnGetSongs
            // 
            this.btnGetSongs.Enabled = false;
            this.btnGetSongs.Location = new System.Drawing.Point(107, 150);
            this.btnGetSongs.Name = "btnGetSongs";
            this.btnGetSongs.Size = new System.Drawing.Size(75, 23);
            this.btnGetSongs.TabIndex = 4;
            this.btnGetSongs.Text = "Get Songs";
            this.btnGetSongs.UseVisualStyleBackColor = true;
            this.btnGetSongs.Click += new System.EventHandler(this.btnGetSongs_Click);
            // 
            // tvArtists
            // 
            this.tvArtists.Location = new System.Drawing.Point(16, 200);
            this.tvArtists.Name = "tvArtists";
            this.tvArtists.Size = new System.Drawing.Size(281, 248);
            this.tvArtists.TabIndex = 101;
            this.tvArtists.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvArtists_AfterSelect);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 183);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 102;
            this.label5.Text = "Artists";
            // 
            // lbAlbums
            // 
            this.lbAlbums.FormattingEnabled = true;
            this.lbAlbums.Location = new System.Drawing.Point(303, 200);
            this.lbAlbums.Name = "lbAlbums";
            this.lbAlbums.Size = new System.Drawing.Size(212, 251);
            this.lbAlbums.TabIndex = 103;
            this.lbAlbums.SelectedIndexChanged += new System.EventHandler(this.lbAlbums_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(307, 186);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 104;
            this.label6.Text = "Albums";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(188, 150);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 105;
            this.button1.Text = "Pause";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lbPlaylist
            // 
            this.lbPlaylist.FormattingEnabled = true;
            this.lbPlaylist.Location = new System.Drawing.Point(522, 200);
            this.lbPlaylist.Name = "lbPlaylist";
            this.lbPlaylist.Size = new System.Drawing.Size(154, 251);
            this.lbPlaylist.TabIndex = 106;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(524, 186);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 13);
            this.label7.TabIndex = 107;
            this.label7.Text = "Playlist";
            // 
            // pbSongProgress
            // 
            this.pbSongProgress.Location = new System.Drawing.Point(373, 149);
            this.pbSongProgress.Name = "pbSongProgress";
            this.pbSongProgress.Size = new System.Drawing.Size(289, 23);
            this.pbSongProgress.TabIndex = 108;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(270, 149);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 109;
            this.button2.Text = "Skip";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(270, 120);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 110;
            this.button3.Text = "Stop";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // PlayerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 633);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.pbSongProgress);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lbPlaylist);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lbAlbums);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tvArtists);
            this.Controls.Add(this.btnGetSongs);
            this.Controls.Add(this.tbResults);
            this.Controls.Add(this.btnLogIn);
            this.Controls.Add(this.tbUser);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbServer);
            this.Name = "PlayerForm";
            this.Text = "Subsonic";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.TextBox tbUser;
        private System.Windows.Forms.Button btnLogIn;
        private System.Windows.Forms.RichTextBox tbResults;
        private System.Windows.Forms.Button btnGetSongs;
        private System.Windows.Forms.TreeView tvArtists;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox lbAlbums;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox lbPlaylist;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ProgressBar pbSongProgress;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}

