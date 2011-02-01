
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SubsonicAPI;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Mono.Data.Sqlite;

namespace SubsonicWeb
{


	public partial class Library : System.Web.UI.Page
	{
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			
			DataTable dtArtists = new DataTable();
			
			#region fetch data for artists			
			
			Mono.Data.Sqlite.SqliteConnection cn = new Mono.Data.Sqlite.SqliteConnection("library.sqlite");
			Mono.Data.Sqlite.SqliteCommand comm = new Mono.Data.Sqlite.SqliteCommand(cn);
			Mono.Data.Sqlite.SqliteDataAdapter adapter = new Mono.Data.Sqlite.SqliteDataAdapter(comm);
			comm.CommandText = @"
SELECT 	name, id, fetched
FROM 	artists
";
			adapter.Fill(dtArtists);
			
			#endregion
			
			if (dtArtists.Rows.Count == 0)
			{
				List<SubsonicItem> artists = Subsonic.GetIndexes();
				
				foreach (SubsonicItem artist in artists)
				{
					DataRow dr = dtArtists.NewRow();
					dr["name"] = artist.name;
					dr["id"] = artist.id;
					dr["feteched"] = DateTime.Now.ToString();
					dtArtists.Rows.Add(dr);
					
					comm = new Mono.Data.Sqlite.SqliteCommand(cn);			
					comm.CommandText = @"
INSERT INTO artists (name, id, fetched)
VALUES(@name, @id, @fetched);
";
					comm.Parameters.AddWithValue("@name", artist.name);
					comm.Parameters.AddWithValue("@id", artist.id);
					comm.Parameters.AddWithValue("@fetched", DateTime.Now.ToString());
					
					if (cn.State != ConnectionState.Open)
						cn.Open();
					comm.ExecuteNonQuery();
				}
				
				if (cn.State != ConnectionState.Closed)
					cn.Close();
			}
			
			rptArtists.DataSource = dtArtists;
			rptArtists.DataBind();
		}
		
		protected void rptArtists_ItemDataBound (object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				HyperLink hlnkArtist = (HyperLink)e.Item.FindControl("hlnkArtist");
			}
		}
		
		
	}
}

