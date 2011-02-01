<%@ Page Language="C#" Inherits="SubsonicWeb.Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head runat="server">
	<title>Default</title>
</head>
<body>
	<form id="form1" runat="server">
		<asp:Panel id="pnlLogin" runat="server">
			<div>
				<asp:Label id="lblUsername" runat="server" AssociatedControlID="tbUsername">Username</asp:Label>
				<br />
				<asp:TextBox id="tbUsername" runat="server" />
				<br />
				<br />
				<asp:Label id="lblPassword" runat="server" AssociatedControlID="tbPassword">Password</asp:Label>
				<br />
				<asp:TextBox id="tbPassword" runat="server" TextMode="Password" />
				<br />
				<asp:Button id="btnLogIn" runat="server" OnClick="btnLogIn_Click" Text="Login" />
			</div>
		</asp:Panel>
	</form>
</body>
</html>
