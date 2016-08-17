<%@ Page language="C#" MasterPageFile="~masterurl/default.master"    Inherits="Microsoft.SharePoint.WebPartPages.WebPartPage,Microsoft.SharePoint,Version=12.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c" meta:progid="SharePoint.WebPartPage.Document" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Import Namespace="Microsoft.SharePoint" %> <%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
	<SharePoint:ListFormPageTitle runat="server"/>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderPageTitleInTitleArea" runat="server">
	<SharePoint:ListProperty Property="LinkTitle" runat="server" id="ID_LinkTitle"/>
	: 
	<SharePoint:ListItemProperty id="ID_ItemProperty" MaxLength=40 runat="server"/>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderPageImage" runat="server">
	<IMG SRC="/_layouts/images/blank.gif" width=1 height=1 alt="">
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderLeftNavBar" runat="server"/>
<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">
<table cellpadding=0 cellspacing=0 id="onetIDListForm">
 <tr>
  <td>
 <WebPartPages:WebPartZone runat="server" FrameType="None" ID="Main" Title="loc:Main"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone>
 <IMG SRC="/_layouts/images/blank.gif" width=590 height=1 alt="">
  </td>
 </tr>
</table>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderTitleLeftBorder" runat="server">
<table cellpadding=0 height=100% width=100% cellspacing=0>
 <tr><td class="ms-areaseparatorleft"><IMG SRC="/_layouts/images/blank.gif" width=1 height=1 alt=""></td></tr>
</table>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderTitleAreaClass" runat="server">
<script id="onetidPageTitleAreaFrameScript">
	document.getElementById("onetidPageTitleAreaFrame").className="ms-areaseparator";
</script>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderBodyAreaClass" runat="server">
<style type="text/css">
.ms-bodyareaframe {
	padding: 8px;
	border: none;
}
</style>
<script type="text/javascript" >
				
	var query = window.location.search.substring(1);
	var vars = query.split("&");
	var meetingID = '';
	
	for (var i=0; i<vars.length; i++) 
    {
    	var pair = vars[i].split("=");
    	
    	if (pair[0] == 'ID') 
    	{
    	  meetingID = pair[1];
    	  
    	}
  	}
  	
  	// set the links
  	if(meetingID != '')
  	{
  		// create the absolute url
  		var url = window.location.toString();
  		var url = url.substring(0, url.indexOf('Calendar'));
		
		// create discussion href
  		var discussionLink = document.getElementById('createDiscussionLink');
  		
  		if(discussionLink != null)
  		{	
  			var href = url + 'Team%20Discussion/NewForm.aspx?MeetingID=' + meetingID;
  			discussionLink.setAttribute('href', href);						  			
  		}
  		
  		// create task href
  		var taskLink = document.getElementById('createTaskLink');
  		
  		if(taskLink != null)
  		{ 			
  			taskLink.setAttribute('href', url + 'Tasks/NewForm.aspx?MeetingID=' + meetingID);						  			
  		}

  	}

</script>

</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderBodyLeftBorder" runat="server">
<div class='ms-areaseparatorleft'><IMG SRC="/_layouts/images/blank.gif" width=8 height=100% alt=""></div>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderTitleRightMargin" runat="server">
<div class='ms-areaseparatorright'><IMG SRC="/_layouts/images/blank.gif" width=8 height=100% alt=""></div>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderBodyRightMargin" runat="server">
<div class='ms-areaseparatorright'><IMG SRC="/_layouts/images/blank.gif" width=8 height=100% alt=""></div>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderTitleAreaSeparator" runat="server"/>
