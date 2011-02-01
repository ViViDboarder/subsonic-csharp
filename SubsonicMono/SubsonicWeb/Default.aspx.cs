
using System;
using System.Web;
using System.Web.UI;
using SubsonicAPI;

namespace SubsonicWeb
{


	public partial class Default : System.Web.UI.Page
	{
		protected void btnLogIn_Click (object sender, System.EventArgs e)
		{
			string server = "thefij.kicks-ass.net:4040";
			string username = tbUsername.Text;
			string password = tbPassword.Text;
			
			Subsonic.appName = "IansWebApp";
			string results = Subsonic.LogIn(server, username, password);
			
			if (results != "")
				Response.Redirect("Library.aspx");
		}
		
		
	}
}

