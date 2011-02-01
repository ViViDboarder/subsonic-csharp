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
        public enum SubsonicItemType
        {
            Folder, Song, Artist, Library
        }
		
        public string name;
        public string id;
		public string lastModified;
		public string lastAccessed;
        public SubsonicItemType itemType;
		public SubsonicItem parent;
		private List<SubsonicItem> _children;
		
		public List<SubsonicItem> children
		{
			get
			{
				if (_children == null)
				{
					if (this.itemType == SubsonicItemType.Song)
						_children = null;
					else 
						_children = Subsonic.GetItemChildren(this, "");
				}
				return _children;
			}
			set
			{
				_children = value;
			}
		}
		
		public SubsonicItem()
		{
			this.name = "";
			this.id = "";
			this.lastAccessed = DateTime.Now.ToString();
		}
		
		public SubsonicItem(string name, string id)
		{			
			this.name = name;
			this.id = name;
			this.lastAccessed = DateTime.Now.ToString();
		}

		public SubsonicItem(string name, string id, SubsonicItemType itemType, SubsonicItem parent)
		{			
			this.name = name;
			this.id = id;
			this.itemType = itemType;
			this.parent = parent;
			this.lastAccessed = DateTime.Now.ToString();
		}
		
        public override string ToString()
        {
            return name;
        }
		
		public SubsonicItem FindItemById(string id)
		{
			SubsonicItem foundItem = null;
			
			// If the current item is the item we are looking for, return it
			if (this.id == id)
				foundItem = this;
			// Otherwise, we check the children if they exist
			else if (_children != null)
			{
				foreach(SubsonicItem child in _children)
				{
					// If this child is the item we are looking for, return it
					if (child.id == id)
					{
						foundItem = child;
						break;
					}
					else
					{
						foundItem = child.FindItemById(id);
						if (foundItem != null)
							break;
					}
				}
			}
			
			return foundItem;			
		}
		
		public SubsonicItem GetChildByName(string childName)
		{
			SubsonicItem theItem = null;
			
			if (_children != null)
			{
				theItem = _children.Find(
	                delegate(SubsonicItem itm)
	                {
	                    return itm.name == childName	;
	                }
	            );
			}
			
			return theItem;
		}
    }
	
	public class Song : SubsonicItem
	{
		public string artist;
		public string album;
		public string title;
		
		public Song()
		{
			this.artist = "";
			this.title = "";
			this.album = "";
			this.name = "";
			this.id = "";
			this.itemType = SubsonicItem.SubsonicItemType.Song;
			this.parent = null;
			this.lastAccessed = DateTime.Now.ToString();
		}
		
		public Song(string title,string artist, string album, string id)
		{
			this.artist = artist;
			this.title = title;
			this.album = album;
			this.name = title;
			this.id = id;
			this.itemType = SubsonicItem.SubsonicItemType.Song;
			this.parent = null;
			this.lastAccessed = DateTime.Now.ToString();
		}
		
		public Song(string title, string artist, string album, string id, SubsonicItem parent)
		{
			this.artist = artist;
			this.title = title;
			this.album = album;
			this.name = title;
			this.id = id;
			this.itemType = SubsonicItem.SubsonicItemType.Song;
			this.parent = parent;
			this.lastAccessed = DateTime.Now.ToString();
		}
		
		public Stream getStream()
		{
			return Subsonic.StreamSong(this.id);
		}
		
		public override string ToString()
		{
			return artist + " - " + title;
		}
	}

    #endregion Classes

    /// <summary>
    /// Open Source C# Implementation of the Subsonic API
    /// http://www.subsonic.org/pages/api.jsp
    /// </summary>
    public static class Subsonic
    {
		private static SubsonicItem _MyLibrary;
		
		/// <summary>
		/// Public Property that can be used for auto-retrieving children
		/// </summary>
		public static SubsonicItem MyLibrary
		{
			get
			{
				return _MyLibrary;
			}
			set
			{
				_MyLibrary = value;
			}
		}
		
        // Should be set from application layer when the application is loaded
        public static string appName;

        // Min version of the REST API implemented
        private static string apiVersion = "1.3.0";

        // Set with the login method
        static string server;
        static string authHeader;
		
		// Used for generating direct URLS
		static string encPass;
		static string username;

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
			
			// Store user and encoded password for alternate authentication
			username = user;			
			Byte[] passwordBytes = Encoding.Default.GetBytes(password);
			for (int i = 0; i < passwordBytes.Length; i++)
				encPass += passwordBytes[i].ToString("x2");
			
            Stream theStream = MakeGenericRequest("ping", null);

            StreamReader sr = new StreamReader(theStream);

            result = sr.ReadToEnd();

            /// TODO: Parse the result and determine if logged in or not

			_MyLibrary = new SubsonicItem("LibraryRoot", "-1", SubsonicItem.SubsonicItemType.Library, null);
			
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
		/// Creates a URL for a command with username and encoded pass in the URL
		/// </summary>
		/// <param name="method"></param>
		/// <param name="parameters"></param>
		/// <returns>URL for streaming a song or retrieving the results of a call</returns>
		public static string BuildDirectURL(string method, Dictionary<string, string> parameters)
		{
			string callURL = "http://" + server + "/rest/" + method + "?v=" + apiVersion + "&c=" + appName
				+ "&u=" + username + "&p=enc:" + encPass;
            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> parameter in parameters)
                {
                    callURL += "&" + parameter.Key + "=" + parameter.Value;
                }
            }
            return callURL;
		}
		
		/// <summary>
		/// Returns a list of SubsonicItems that fall inside the parent object 
		/// </summary>
		/// <param name="parent">
		/// A <see cref="SubsonicItem"/>
		/// </param>
		/// <param name="ifModifiedSince">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="List<SubsonicItem>"/>
		/// </returns>
		public static List<SubsonicItem> GetItemChildren(SubsonicItem parent, string ifModifiedSince)
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();
            
			// Generate the proper request for the parent type
			string requestType, musicFolderId;
			if (parent.itemType == SubsonicItem.SubsonicItemType.Library)
			{
				requestType = "getIndexes";
				if (parent.id != "-1")
					parameters.Add("musicFolderId", parent.id);
			}
			else
			{
				requestType = "getMusicDirectory";
				parameters.Add("id", parent.id);
			}
			
			// Load the parameters if provided
            if (!string.IsNullOrEmpty(ifModifiedSince))
                parameters.Add("ifModifiedSince", ifModifiedSince);

            // Make the request
            Stream theStream = MakeGenericRequest(requestType, parameters);
            // Read the response as a string
            StreamReader sr = new StreamReader(theStream);
            string result = sr.ReadToEnd();

            // Parse the resulting XML string into an XmlDocument
            XmlDocument myXML = new XmlDocument();
            myXML.LoadXml(result);
			
			List<SubsonicItem> children = new List<SubsonicItem>();
			
			// Parse the artist
			if (parent.itemType == SubsonicItem.SubsonicItemType.Library)
			{
				if (myXML.ChildNodes[1].Name == "subsonic-response")
	            {
	                if (myXML.ChildNodes[1].FirstChild.Name == "indexes")
	                {
	                    for (int i = 0; i < myXML.ChildNodes[1].FirstChild.ChildNodes.Count; i++)
	                    {
	                        for (int j = 0; j < myXML.ChildNodes[1].FirstChild.ChildNodes[i].ChildNodes.Count; j++)
	                        {
	                            string artist = myXML.ChildNodes[1].FirstChild.ChildNodes[i].ChildNodes[j].Attributes["name"].Value;
	                            string id = myXML.ChildNodes[1].FirstChild.ChildNodes[i].ChildNodes[j].Attributes["id"].Value;
	
	                            children.Add(new SubsonicItem(artist, id, SubsonicItem.SubsonicItemType.Folder, parent));
	                        }
	                    }
	                }
	            }
			}
			// Parse the directory
			else if (parent.itemType == SubsonicItem.SubsonicItemType.Folder)
			{
				if (myXML.ChildNodes[1].Name == "subsonic-response")
	            {
	                if (myXML.ChildNodes[1].FirstChild.Name == "directory")
	                {
						for (int i = 0; i < myXML.ChildNodes[1].FirstChild.ChildNodes.Count; i++)
	                    {
	                        bool isDir = bool.Parse(myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["isDir"].Value);
	                        string title = myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["title"].Value;
	                        string id = myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["id"].Value;
	
	                       	SubsonicItem theItem = new SubsonicItem(title, id, (isDir ? SubsonicItem.SubsonicItemType.Folder : SubsonicItem.SubsonicItemType.Song), parent);
							children.Add(theItem);
	                    }
	                }
	            }
			}
			
			return children;
		}
		
        /// <summary>
        /// Returns an indexed structure of all artists.
        /// </summary>
        /// <param name="parent">Required: No; If specified, only return artists in the music folder with the given ID.</param>
        /// <param name="ifModifiedSince">Required: No; If specified, only return a result if the artist collection has changed since the given time.</param>
        /// <returns>Dictionary, Key = Artist and Value = id</returns>
        public static List<SubsonicItem> GetIndexes(string musicFolderId, string ifModifiedSince)
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

            // Parse the XML document into a List
            List<SubsonicItem> artists = new List<SubsonicItem>();
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

                            artists.Add(new SubsonicItem(artist, id));
                        }
                    }
                }
            }
            
            return artists;
        }
		
		public static List<SubsonicItem> GetIndexes(string musicFolderId)
        {
			return GetIndexes(musicFolderId, "");	
		}
		
		public static List<SubsonicItem> GetIndexes()
        {
			return GetIndexes("", "");
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
        public static Stream StreamSong(string id, int? maxBitRate)
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

		public static Stream StreamSong(string id)
        {
			return StreamSong(id, null);
		}

        /// <summary>
        /// Returns a listing of all files in a music directory. Typically used to get list of albums for an artist, or list of songs for an album.
        /// </summary>
        /// <param name="id">A string which uniquely identifies the music folder. Obtained by calls to getIndexes or getMusicDirectory.</param>
        /// <returns>MusicFolder object containing info for the specified directory</returns>
        public static List<SubsonicItem> GetMusicDirectory(string id)
        {
            Dictionary<string, string> theParameters = new Dictionary<string, string>();
            theParameters.Add("id", id);
            Stream theStream = MakeGenericRequest("getMusicDirectory", theParameters);

            StreamReader sr = new StreamReader(theStream);

            string result = sr.ReadToEnd();

            XmlDocument myXML = new XmlDocument();
            myXML.LoadXml(result);

            List<SubsonicItem> theContents = new List<SubsonicItem>();

            if (myXML.ChildNodes[1].Name == "subsonic-response")
            {
                if (myXML.ChildNodes[1].FirstChild.Name == "directory")
                {
					SubsonicItem theParent = new SubsonicItem();
                    theParent.name = myXML.ChildNodes[1].FirstChild.Attributes["name"].Value;
                    theParent.id = myXML.ChildNodes[1].FirstChild.Attributes["id"].Value;

                    int i = 0;
                    for (i = 0; i < myXML.ChildNodes[1].FirstChild.ChildNodes.Count; i++)
                    {
                        bool isDir = bool.Parse(myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["isDir"].Value);
                        string title = myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["title"].Value;
                        string theId = myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["id"].Value;

                       	SubsonicItem theItem = new SubsonicItem(title, theId, (isDir ? SubsonicItem.SubsonicItemType.Folder : SubsonicItem.SubsonicItemType.Song), theParent);
						theContents.Add(theItem);
                    }
                }
            }

            return theContents;
        }
		
		/// <summary>
		/// Returns what is currently being played by all users. Takes no extra parameters. 
		/// </summary>
		public static List<SubsonicItem> GetNowPlaying()
		{
			List<SubsonicItem> nowPlaying = new List<SubsonicItem>();
			
			Dictionary<string, string> theParameters = new Dictionary<string, string>();
			Stream theStream = MakeGenericRequest("getNowPlaying", theParameters);
			StreamReader sr = new StreamReader(theStream);
			string result = sr.ReadToEnd();

			
			return nowPlaying;
		}
		
		/// <summary>
		/// Performs a search valid for the current version of the subsonic server 
		/// </summary>
		/// <param name="query">The Term you want to search for</param>
		/// <returns>A List of SubsonicItem objects</returns>
		public static List<SubsonicItem> Search(string query)
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();            
			Version apiV = new Version(apiVersion);
			Version Search2Min = new Version("1.4.0");
			string request = "";
			if (apiV >= Search2Min)
			{
				request = "search2";
				parameters.Add("query", query);
			}
			else
			{
				request = "search";
				parameters.Add("any", query);
			}

            // Make the request
            Stream theStream = MakeGenericRequest(request, parameters);
            // Read the response as a string
            StreamReader sr = new StreamReader(theStream);
            string result = sr.ReadToEnd();

            // Parse the resulting XML string into an XmlDocument
            XmlDocument myXML = new XmlDocument();
            myXML.LoadXml(result);
			
			List<SubsonicItem> searchResults = new List<SubsonicItem>();
			
			// Parse the artist
			if (myXML.ChildNodes[1].Name == "subsonic-response")
            {
                if (myXML.ChildNodes[1].FirstChild.Name == "searchResult")
                {
                    for (int i = 0; i < myXML.ChildNodes[1].FirstChild.ChildNodes.Count; i++)
                    {
                        bool isDir = bool.Parse(myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["isDir"].Value);
                        string title = myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["title"].Value;
                        string theId = myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["id"].Value;
						string artist = "";
						string album = "";
						
						if (!isDir)
						{								
	                        artist = myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["artist"].Value;
	                        album = myXML.ChildNodes[1].FirstChild.ChildNodes[i].Attributes["album"].Value;
						}
						
						SubsonicItem theItem;
						if (isDir)
                       		theItem = new SubsonicItem(title, theId, SubsonicItem.SubsonicItemType.Folder, null);
						else
							theItem = new Song(title, artist, album, theId);

                        searchResults.Add(theItem);
                    }
                }
            }
			
			return searchResults;
		}
		
    }

}