/**************************************************************************
    Subsonic Csharp
    Copyright (C) 2010  Ian Fijolek
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
**************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;

namespace SubsonicAPI
{
    #region Classes

    public class SubsonicItem
    {
        public string Name;
        public string id;

        public enum SubsonicItemType
        {
            Folder, Song
        }

        public SubsonicItemType ItemType;

        public override string ToString()
        {
            return Name;
        }
    }

    public class MusicFolder : SubsonicItem
    {
        #region private vars

        private List<MusicFolder> _Folders;
        private List<Song> _Songs;

        #endregion private vars

        #region properties

        public List<MusicFolder> Folders
        {
            get { return _Folders; }
            set { _Folders = value; }
        }

        public List<Song> Songs
        {
            get { return _Songs; }
            set { _Songs = value; }
        }

        #endregion properties

        public MusicFolder()
        {
            _Folders = new List<MusicFolder>();
            _Songs = new List<Song>();

            base.ItemType = SubsonicItemType.Folder;
        }

        public MusicFolder(string theName, string theId)
        {
            _Folders = new List<MusicFolder>();
            _Songs = new List<Song>();

            base.Name = theName;
            base.id = theId;

            base.ItemType = SubsonicItemType.Folder;
        }

        ~MusicFolder() { }

        public void AddSong(string title, string id)
        {
            Song newSong = new Song(title, id);
            _Songs.Add(newSong);
        }

        public void AddFolder(string name, string id)
        {
            MusicFolder newFolder = new MusicFolder(name, id);
            _Folders.Add(newFolder);
        }

        public Song FindSong(string theTitle)
        {
            Song theSong = _Songs.Find(
                delegate(Song sng)
                {
                    return sng.Name == theTitle;
                }
            );

            return theSong;
        }

        public MusicFolder FindFolder(string theFolderName)
        {
            MusicFolder theFolder = _Folders.Find(
                delegate(MusicFolder fldr)
                {
                    return fldr.Name == theFolderName;
                }
            );

            return theFolder;
        }
    }

    public class Song : SubsonicItem
    {
        public Song()
        {
            base.ItemType = SubsonicItemType.Song;
        }

        public Song(string theTitle, string theId)
        {
            Name = theTitle;
            id = theId;

            base.ItemType = SubsonicItemType.Song;
        }
    }

    #endregion Classes

    /// <summary>
    /// Open Source C# Implementation of the Subsonic API
    /// http://www.subsonic.org/pages/api.jsp
    /// </summary>
    public static class Subsonic
    {
        // Should be set from application layer when the application is loaded
        public static string appName;

        // Version of the REST API implemented
        private static string apiVersion = "1.3.0";

        // Set with the login method
        static string server;
        static string authHeader;

        /// <summary>
        /// Takes parameters for server, username and password to generate an auth header
        /// and Pings the server
        /// </summary>
        /// <param name="theServer"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns>Resulting XML (Future boolean)</returns>
        public static string LogIn(string theServer, string user, string password)
        {
            string result = "Nothing Happened";

            server = theServer;
            authHeader = user + ":" + password;
            authHeader = Convert.ToBase64String(Encoding.Default.GetBytes(authHeader));

            Stream theStream = MakeGenericRequest("ping", null);

            StreamReader sr = new StreamReader(theStream);

            result = sr.ReadToEnd();

            /// TODO: Parse the result and determine if logged in or not

            return result;
        }

        /// <summary>
        /// Uses the Auth Header for logged in user to make an HTTP request to the server 
        /// with the given Subsonic API method and parameters
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>Datastream of the server response</returns>
        public static Stream MakeGenericRequest(string method, Dictionary<string, string> parameters)
        {
            // Check to see if Logged In yet
            if (string.IsNullOrEmpty(authHeader))
            {
                // Throw a Not Logged In exception
                Exception e = new Exception("No Authorization header.  Must Log In first");
                return null;
            }
            else
            {
                if (!method.EndsWith(".view"))
                    method += ".view";

                string requestURL = BuildRequestURL(method, parameters);

                WebRequest theRequest = WebRequest.Create(requestURL);
                theRequest.Method = "GET";

                theRequest.Headers["Authorization"] = "Basic " + authHeader;

                WebResponse response = theRequest.GetResponse();

                Stream dataStream = response.GetResponseStream();

                return dataStream;
            }
        }

        /// <summary>
        /// Creates a URL for a request but does not make the actual request using set login credentials an dmethod and parameters
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns>Proper Subsonic API URL for a request</returns>
        public static string BuildRequestURL(string method, Dictionary<string, string> parameters)
        {
            string requestURL = "http://" + server + "/rest/" + method + "?v=" + apiVersion + "&c=" + appName;
            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> parameter in parameters)
                {
                    requestURL += "&" + parameter.Key + "=" + parameter.Value;
                }
            }
            return requestURL;
        }

        /// <summary>
        /// Returns an indexed structure of all artists.
        /// </summary>
        /// <param name="musicFolderId">Required: No; If specified, only return artists in the music folder with the given ID.</param>
        /// <param name="ifModifiedSince">Required: No; If specified, only return a result if the artist collection has changed since the given time.</param>
        /// <returns>Dictionary, Key = Artist and Value = id</returns>
        public static Dictionary<string, string> GetIndexes(string musicFolderId = "", string ifModifiedSince = "")
        {
            // Load the parameters if provided
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(musicFolderId))
                parameters.Add("musicFolderId", musicFolderId);

            if (!string.IsNullOrEmpty(ifModifiedSince))
                parameters.Add("ifModifiedSince", ifModifiedSince);

            // Make the request
            Stream theStream = MakeGenericRequest("getIndexes", parameters);
            // Read the response as a string
            StreamReader sr = new StreamReader(theStream);
            string result = sr.ReadToEnd();

            // Parse the resulting XML string into an XmlDocument
            XmlDocument myXML = new XmlDocument();
            myXML.LoadXml(result);

            // Parse the XML document into a Dictionary
            Dictionary<string, string> artists = new Dictionary<string, string>();            
            if (myXML.ChildNodes[1].Name == "subsonic-response")
            {
                if (myXML.ChildNodes[1].FirstChild.Name == "indexes")
                {
                    int i = 0;
                    for (i = 0; i < myXML.ChildNodes[1].FirstChild.ChildNodes.Count; i++)
                    {
                        int j = 0;
                        for (j = 0; j < myXML.ChildNodes[1].FirstChild.ChildNodes[i].ChildNodes.Count; j++)
                        {
                            string artist = myXML.ChildNodes[1].FirstChild.ChildNodes[i].ChildNodes[j].Attributes["name"].Value;
                            string id = myXML.ChildNodes[1].FirstChild.ChildNodes[i].ChildNodes[j].Attributes["id"].Value;

                            artists.Add(artist, id);
                        }
                    }
                }
            }
            
            return artists;
        }

        /// <summary>
        /// Streams a given music file. (Renamed from request name "stream")
        /// </summary>
        /// <param name="id">Required: Yes; A string which uniquely identifies the file to stream. 
        /// Obtained by calls to getMusicDirectory.</param>
        /// <param name="maxBitRate">Required: No; If specified, the server will attempt to 
        /// limit the bitrate to this value, in kilobits per second. If set to zero, no limit 
        /// is imposed. Legal values are: 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256 and 320. </param>
        /// <returns></returns>
        public static Stream StreamSong(string id, int? maxBitRate = null)
        {
            // Reades the id of the song and sets it as a parameter
            Dictionary<string, string> theParameters = new Dictionary<string,string>();
            theParameters.Add("id", id);
            if (maxBitRate.HasValue)
                theParameters.Add("maxBitRate", maxBitRate.ToString());

            // Makes the request
            Stream theStream = MakeGenericRequest("stream", theParameters);

            return theStream;
        }


        /// <summary>
        /// Returns a listing of all files in a music directory. Typically used to get list of albums for an artist, or list of songs for an album.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the music folder. Obtained by calls to getIndexes or getMusicDirectory.</param>
        /// <returns>MusicFolder object containing info for the specified directory</returns>
        public static MusicFolder GetMusicDirectory(string id)
        {
            Dictionary<string, string> theParameters = new Dictionary<string, string>();
            theParameters.Add("id", id);
            Stream theStream = MakeGenericRequest("getMusicDirectory", theParameters);

            StreamReader sr = new StreamReader(theStream);

            string result = sr.ReadToEnd();

            XmlDocument myXML = new XmlDocument();
            myXML.LoadXml(result);

            MusicFolder theFolder = new MusicFolder("ArtistFolder", id);

            if (myXML.ChildNodes[1].Name == "subsonic-response")
            {
                if (myXML.ChildNodes[1].FirstChild.Name == "directory")
                {
                    theFolder.Name = myXML.ChildNodes[1].FirstChild.Attributes["name"].Value;
                    theFolder.id = myXML.ChildNodes[1].FirstChild.Attributes["id"].Value;

                    int i = 0;
                    for (i = 0; i < myXML.ChildNodes[1].FirstChild.ChildNodes.Count; i++)
                    {
                        bool isDir = bool.Parse(myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["isDir"].Value);
                        string title = myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["title"].Value;
                        string theId = myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["id"].Value;

                        if (isDir)
                            theFolder.AddFolder(title, theId);
                        else
                            theFolder.AddSong(title, theId);
                    }
                }
            }

            return theFolder;
        }
		
		/// <summary>
		/// Returns what is currently being played by all users. Takes no extra parameters. 
		/// </summary>
		public static List<Song> GetNowPlaying()
		{
			List<Song> nowPlaying = new List<Song>();
			
			Dictionary<string, string> theParameters = new Dictionary<string, string>();
			Stream theStream = MakeGenericRequest("getNowPlaying", theParameters);
			StreamReader sr = new StreamReader(theStream);
			string result = sr.ReadToEnd();

			
			return nowPlaying;
		}
    }

}