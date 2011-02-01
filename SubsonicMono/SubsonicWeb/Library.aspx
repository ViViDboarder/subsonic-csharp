<%@ Page Language="C#" Inherits="SubsonicWeb.Library" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head runat="server">
	<title>Library</title>
</head>
<body>
	<form id="form1" runat="server">
		<table>
			<tr>
				<td>
					<asp:Repeater id="rptArtists" runat="server" OnItemDataBound="rptArtists_ItemDataBound">
						<ItemTemplate>
							<asp:LinkButton id="lbtnArtist" runat="server" />
							<asp:HyperLink id="hlnkArtist" runat="server" />
							<br />
						</ItemTemplate>
					</asp:Repeater>
				</td>
				<td>
					<asp:Label id="lblSelArtist" runat="server">Select an Artist</asp:Label>
					<asp:Repeater id="rptItems" runat="server">
						<ItemTemplate>
						
						</ItemTemplate>
					</asp:Repeater>
				</td>
			</tr>
		</table>
	</form>
</body>
</html>
