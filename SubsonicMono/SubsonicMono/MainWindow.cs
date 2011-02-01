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
using Gtk;
using SubsonicAPI;
using System.Collections.Generic;
using HollyLibrary;
using System.Threading;

public partial class MainWindow : Gtk.Window
{
	HTreeView tvLibrary;
	HSimpleList slPlaylist;
	
	public MainWindow () : base(Gtk.WindowType.Toplevel)
	{
		Build ();
		Subsonic.appName = "IansCsharpApp";
		
		InitializeTreeView();
		InitializePlaylist();
	}
	
	private void InitializeTreeView()
	{
		tvLibrary = new HTreeView();
		swLibrary.Add(tvLibrary);
		tvLibrary.NodeExpanded += tvLibraryNodeExpanded;
		tvLibrary.Editable = false;
		tvLibrary.Visible = true;
	}
	
	private void InitializePlaylist()
	{
		slPlaylist = new HSimpleList();
		swPlayQueue.Add(slPlaylist);
		slPlaylist.Visible = true;
	}

	void tvLibraryNodeExpanded (object sender, NodeEventArgs args)
	{
		// Fetch any items inside the node...
		HTreeNode thisNode = args.Node;
		
		// Check to see if it has any children
		if (thisNode.Nodes.Count == 1 && thisNode.Nodes[0].Text == "")
		{			
			// Node child is a dummy
			thisNode.Nodes[0].Text = "Loading...";
			
			// Get path to the selected node to expandsimp
			Queue<string> nodePath = GetNodePath(thisNode);
			
			// Dive into library to selected node
			SubsonicItem thisItem = Subsonic.MyLibrary;
			while (nodePath.Count > 0)
			{
				thisItem = thisItem.GetChildByName(nodePath.Dequeue());
			}
			
			// Should now have the correct selected item
			foreach(SubsonicItem child in thisItem.children)
			{
				HTreeNode childNode = new HTreeNode(child.name);
				thisNode.Nodes.Add(childNode);
				
				// Adding a dummy node for any Folders
				if (child.itemType == SubsonicItem.SubsonicItemType.Folder)
					childNode.Nodes.Add(new HTreeNode(""));
			}			
			
			// Remove dummy node
			thisNode.Nodes.RemoveAt(0);
		}		
	}

	private Queue<string> GetNodePath(HTreeNode theNode)
	{
		// Create a queue that will hold the name of all parent objects
		Queue<string> nodePath;
		if (theNode.ParentNode != null)
		{
			// If this node has a parent, then recurse
			nodePath = GetNodePath(theNode.ParentNode);
		}
		else
		{
			// If the praent, then initialize the path
			nodePath = new Queue<string>();
		}
		
		// Add enqueue this item in the path
		nodePath.Enqueue(theNode.Text);
		
		return nodePath;
	}
	
	private SubsonicItem GetNodeItem(HTreeNode theNode)
	{
		// Get path to the selected node
		Queue<string> nodePath = GetNodePath(theNode);
		
		// Dive into library to selected node
		SubsonicItem thisItem = Subsonic.MyLibrary;
		while (nodePath.Count > 0)
		{
			thisItem = thisItem.GetChildByName(nodePath.Dequeue());
		}
		
		return thisItem;
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	
	protected virtual void OnBtnLogin2Clicked (object sender, System.EventArgs e)
	{
		string server = tbServer.Text;
		string user = tbUsername.Text;
		string passw0rdd = tbPaassw0rd.Text;
		
		string loginResult = Subsonic.LogIn(server, user, passw0rdd);
		Console.WriteLine("Login Result: " + loginResult);
				
		SubsonicItem thisLibrary = Subsonic.MyLibrary;
		foreach(SubsonicItem artist in thisLibrary.children)
		{
			HTreeNode artistNode = new HTreeNode(artist.name);
			tvLibrary.Nodes.Add(artistNode);
			
			// Adding a dummy node for the artist
			artistNode.Nodes.Add(new HTreeNode(""));
		}
	}
	
	protected virtual void OnBtnQueueSongClicked (object sender, System.EventArgs e)
	{
		HTreeNode theNode = tvLibrary.SelectedNode;
		
		// Check if node has children
		if (theNode.Nodes.Count > 0)
		{
			// Will add all children to queue
		}
		else
		{
			// Node is a leaf (song)
			SubsonicItem theItem = GetNodeItem(theNode);
			
			// Confirm that the item is  asong
			if (theItem.itemType == SubsonicItem.SubsonicItemType.Song)
			{
				//slPlaylist.Items.Add(theItem);
				
				Dictionary<string, string> songId = new Dictionary<string, string>();
				songId.Add("id", theItem.id);
				string streamURL = Subsonic.BuildDirectURL("download.view", songId);
				
				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.StartInfo.FileName = "vlc";
				proc.StartInfo.Arguments = "--one-instance --playlist-enqueue " + streamURL;
				proc.Start();				
			}
			
		}
	}
	
	protected virtual void OnBtnSearchClicked (object sender, System.EventArgs e)
	{
		string search = tbSearch.Text;
		
		List<SubsonicItem> results = Subsonic.Search(search);
		
		foreach (SubsonicItem si in results)
			slPlaylist.Items.Add(si);
	}
	
	
	
	
	
	
}

