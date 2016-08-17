<%@ Page language="C#" MasterPageFile="~masterurl/default.master" title="New Page 1" inherits="Microsoft.SharePoint.WebPartPages.WebPartPage, Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" meta:progid="SharePoint.WebPartPage.Document"%>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %><asp:content id="content2" runat="server" contentplaceholderid="PlaceHolderMain">
	<br>
	<WebPartPages:DataFormWebPart runat="server" IsIncluded="True" FrameType="None" NoDefaultStyle="TRUE" ViewFlag="0" Title="Absences" ListName="{{@ListId:Absences}}" Default="FALSE" DisplayName="Absences" __markuptype="vsattributemarkup" __WebPartId="{B261BFEB-5342-4949-BA7F-CF8E0CCA00C7}" id="g_b261bfeb_5342_4949_ba7f_cf8e0cca00c7" pagesize="1" __designer:Preview="&lt;Regions&gt;&lt;Region Name=&quot;0&quot; Editable=&quot;True&quot; Content=&quot;&quot; NamingContainer=&quot;True&quot; /&gt;&lt;/Regions&gt;&lt;table TOPLEVEL border=&quot;0&quot; cellpadding=&quot;0&quot; cellspacing=&quot;0&quot; width=&quot;100%&quot;&gt;
	&lt;tr&gt;
		&lt;td valign=&quot;top&quot;&gt;&lt;div WebPartID=&quot;00000000-0000-0000-0000-000000000000&quot; HasPers=&quot;true&quot; id=&quot;WebPartg_b261bfeb_5342_4949_ba7f_cf8e0cca00c7&quot; width=&quot;100%&quot; OnlyForMePart=&quot;true&quot; allowDelete=&quot;false&quot; style=&quot;&quot; &gt;&lt;div ID=&quot;WebPartContent&quot;&gt;The DataFormWebPart does not provide a design-time preview.&lt;/div&gt;&lt;/div&gt;&lt;/td&gt;
	&lt;/tr&gt;
&lt;/table&gt;" __designer:Values="&lt;P N='DisplayName' ID='1' T='Absences' /&gt;&lt;P N='ViewFlag' T='0' /&gt;&lt;P N='Default' T='FALSE' /&gt;&lt;P N='ListName' T='{{@ListId:Absences}}' /&gt;&lt;P N='DataSourcesString' T='&amp;lt;%@ Register TagPrefix=&quot;SharePoint&quot; Namespace=&quot;Microsoft.SharePoint.WebControls&quot; Assembly=&quot;Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c&quot; %&amp;gt;&amp;lt;%@ Register TagPrefix=&quot;WebPartPages&quot; Namespace=&quot;Microsoft.SharePoint.WebPartPages&quot; Assembly=&quot;Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c&quot; %&amp;gt;&amp;lt;SharePoint:SPDataSource runat=&quot;server&quot; SelectCommand=&quot;&amp;amp;lt;View&amp;amp;gt;&amp;amp;lt;Query&amp;amp;gt;&amp;amp;lt;Where&amp;amp;gt;&amp;amp;lt;Eq&amp;amp;gt;&amp;amp;lt;FieldRef Name=&amp;amp;quot;ID&amp;amp;quot;/&amp;amp;gt;&amp;amp;lt;Value Type=&amp;amp;quot;Counter&amp;amp;quot;&amp;amp;gt;{Param1}&amp;amp;lt;/Value&amp;amp;gt;&amp;amp;lt;/Eq&amp;amp;gt;&amp;amp;lt;/Where&amp;amp;gt;&amp;amp;lt;/Query&amp;amp;gt;&amp;amp;lt;/View&amp;amp;gt;&quot; DataSourceMode=&quot;List&quot; UseInternalName=&quot;True&quot; InsertCommand=&quot;&quot; ID=&quot;Absences1&quot; DeleteCommand=&quot;&quot; UpdateCommand=&quot;&quot;&amp;gt;&amp;lt;InsertParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;WebPartPages:DataFormParameter PropertyName=&quot;ParameterValues&quot; ParameterKey=&quot;ListID&quot; DefaultValue=&quot;{{@ListId:Absences}}&quot; Name=&quot;ListID&quot;&amp;gt;&amp;lt;/WebPartPages:DataFormParameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;/InsertParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;UpdateParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;WebPartPages:DataFormParameter PropertyName=&quot;ParameterValues&quot; ParameterKey=&quot;ListID&quot; DefaultValue=&quot;{{@ListId:Absences}}&quot; Name=&quot;ListID&quot;&amp;gt;&amp;lt;/WebPartPages:DataFormParameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;/UpdateParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;DeleteParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;WebPartPages:DataFormParameter PropertyName=&quot;ParameterValues&quot; ParameterKey=&quot;ListID&quot; DefaultValue=&quot;{{@ListId:Absences}}&quot; Name=&quot;ListID&quot;&amp;gt;&amp;lt;/WebPartPages:DataFormParameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;/DeleteParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;SelectParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;WebPartPages:DataFormParameter PropertyName=&quot;ParameterValues&quot; ParameterKey=&quot;ListID&quot; DefaultValue=&quot;{{@ListId:Absences}}&quot; Name=&quot;ListID&quot;&amp;gt;&amp;lt;/WebPartPages:DataFormParameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;asp:Parameter DefaultValue=&quot;0&quot; Name=&quot;StartRowIndex&quot;&amp;gt;&amp;lt;/asp:Parameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;asp:Parameter DefaultValue=&quot;0&quot; Name=&quot;nextpagedata&quot;&amp;gt;&amp;lt;/asp:Parameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;asp:Parameter DefaultValue=&quot;1&quot; Name=&quot;MaximumRows&quot;&amp;gt;&amp;lt;/asp:Parameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;WebPartPages:DataFormParameter PropertyName=&quot;ParameterValues&quot; ParameterKey=&quot;Param1&quot; DefaultValue=&quot;1&quot; Name=&quot;Param1&quot;&amp;gt;&amp;lt;/WebPartPages:DataFormParameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;/SelectParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;/SharePoint:SPDataSource&amp;gt;&amp;#xD;&amp;#xA;' /&gt;&lt;P N='PageSize' T='1' /&gt;&lt;P N='Xsl' T='&amp;#xD;&amp;#xA;&amp;lt;xsl:stylesheet xmlns:x=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:d=&quot;http://schemas.microsoft.com/sharepoint/dsp&quot; version=&quot;1.0&quot; exclude-result-prefixes=&quot;xsl msxsl ddwrt&quot; xmlns:ddwrt=&quot;http://schemas.microsoft.com/WebParts/v2/DataView/runtime&quot; xmlns:asp=&quot;http://schemas.microsoft.com/ASPNET/20&quot; xmlns:__designer=&quot;http://schemas.microsoft.com/WebParts/v2/DataView/designer&quot; xmlns:xsl=&quot;http://www.w3.org/1999/XSL/Transform&quot; xmlns:msxsl=&quot;urn:schemas-microsoft-com:xslt&quot; xmlns:SharePoint=&quot;Microsoft.SharePoint.WebControls&quot; xmlns:ddwrt2=&quot;urn:frontpage:internal&quot;&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:output method=&quot;html&quot; indent=&quot;no&quot;/&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:decimal-format NaN=&quot;&quot;/&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:param name=&quot;dvt_apos&quot;&amp;gt;&amp;apos;&amp;lt;/xsl:param&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:param name=&quot;ListID&quot;&amp;gt;{{@ListId:Absences}}&amp;lt;/xsl:param&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:param name=&quot;Param1&quot;&amp;gt;1&amp;lt;/xsl:param&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:variable name=&quot;dvt_1_automode&quot;&amp;gt;0&amp;lt;/xsl:variable&amp;gt;&amp;#xD;&amp;#xA;	&amp;#xD;&amp;#xA;	&amp;#xD;&amp;#xA;	&amp;#xD;&amp;#xA;	&amp;lt;xsl:template match=&quot;/&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:call-template name=&quot;dvt_1&quot;/&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;/xsl:template&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:template name=&quot;dvt_1&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;dvt_StyleName&quot;&amp;gt;RepForm3&amp;lt;/xsl:variable&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;Rows&quot; select=&quot;/dsQueryResponse/Rows/Row&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;dvt_RowCount&quot; select=&quot;count($Rows)&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;RowLimit&quot; select=&quot;1&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;IsEmpty&quot; select=&quot;$dvt_RowCount = 0&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;table border=&quot;0&quot; width=&quot;100%&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;tr&amp;gt;&amp;lt;td colspan=&quot;2&quot; class=&quot;ms-pagetitle&quot;&amp;gt;Approve / Approve&amp;lt;/td&amp;gt;&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:call-template name=&quot;dvt_1.body&quot;&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:with-param name=&quot;Rows&quot; select=&quot;$Rows&quot;/&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:with-param name=&quot;FirstRow&quot; select=&quot;1&quot; /&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:with-param name=&quot;LastRow&quot; select=&quot;$dvt_RowCount&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/xsl:call-template&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/table&amp;gt;		&amp;#xD;&amp;#xA;	&amp;lt;/xsl:template&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:template name=&quot;dvt_1.body&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;Rows&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;FirstRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;LastRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:for-each select=&quot;$Rows&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:variable name=&quot;dvt_KeepItemsTogether&quot; select=&quot;false()&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:variable name=&quot;dvt_HideGroupDetail&quot; select=&quot;false()&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:if test=&quot;(position() &amp;amp;gt;= $FirstRow and position() &amp;amp;lt;= $LastRow) or $dvt_KeepItemsTogether&quot;&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:if test=&quot;not($dvt_HideGroupDetail)&quot; ddwrt:cf_ignore=&quot;1&quot;&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;xsl:call-template name=&quot;dvt_1.rowview&quot; /&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;/xsl:if&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/xsl:if&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/xsl:for-each&amp;gt;&amp;#xD;&amp;#xA;		&amp;#xD;&amp;#xA;	&amp;lt;/xsl:template&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:template name=&quot;dvt_1.rowview&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;td&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;table border=&quot;0&quot; cellspacing=&quot;0&quot; width=&quot;100%&quot;&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;25%&quot; class=&quot;ms-formlabel&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;b&amp;gt;Absentee:&amp;lt;/b&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;75%&quot; class=&quot;ms-formbody&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;xsl:value-of select=&quot;@Author&quot; disable-output-escaping=&quot;yes&quot;/&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;25%&quot; class=&quot;ms-formlabel&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;b&amp;gt;Title:&amp;lt;/b&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;75%&quot; class=&quot;ms-formbody&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;xsl:value-of select=&quot;@Title&quot;/&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;25%&quot; class=&quot;ms-formlabel&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;b&amp;gt;Absence Type:&amp;lt;/b&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;75%&quot; class=&quot;ms-formbody&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;xsl:value-of select=&quot;@AVMAbsenceType&quot;/&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;25%&quot; class=&quot;ms-formlabel&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;b&amp;gt;Start Time:&amp;lt;/b&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;75%&quot; class=&quot;ms-formbody&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;xsl:value-of select=&quot;ddwrt:FormatDate(string(@EventDate), 1033, 5)&quot;/&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;25%&quot; class=&quot;ms-formlabel&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;b&amp;gt;End Time:&amp;lt;/b&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;75%&quot; class=&quot;ms-formbody&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;xsl:value-of select=&quot;ddwrt:FormatDate(string(@EndDate), 1033, 5)&quot;/&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;25%&quot; class=&quot;ms-formlabel&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;b&amp;gt;Description:&amp;lt;/b&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;75%&quot; class=&quot;ms-formbody&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;xsl:value-of select=&quot;@Description&quot; disable-output-escaping=&quot;yes&quot;/&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;xsl:if test=&quot;$dvt_1_automode = &amp;apos;1&amp;apos;&quot; ddwrt:cf_ignore=&quot;1&quot;&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;td colspan=&quot;99&quot; class=&quot;ms-vb&quot;&amp;gt;&amp;#xD;&amp;#xA;								&amp;lt;span ddwrt:amkeyfield=&quot;ID&quot; ddwrt:amkeyvalue=&quot;ddwrt:EscapeDelims(string(@ID))&quot; ddwrt:ammode=&quot;view&quot;&amp;gt;&amp;lt;/span&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/xsl:if&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;/table&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;/xsl:template&amp;gt;	&amp;#xD;&amp;#xA;&amp;lt;/xsl:stylesheet&amp;gt;	' /&gt;&lt;P N='DataFields' T='@Title,Title;@AVMAbsenceType,Absence Type;@EventDate,Start Time;@EndDate,End Time;@Description,Description;@fAllDayEvent,All Day Event;@fRecurrence,Recurrence;@AVMStatus,Status;@AVMComments,Comments;@SetAppro,SetApproval;@ApproveR,Approve-Reject;@ID,ID;@ContentType,Content Type;@Modified,Modified;@Created,Created;@Author,Created By;@Editor,Modified By;@_UIVersionString,Version;@Attachments,Attachments;@File_x0020_Type,File Type;@FileLeafRef,Name (for use in forms);@FileDirRef,Path;@FSObjType,Item Type;@_HasCopyDestinations,Has Copy Destinations;@_CopySource,Copy Source;@ContentTypeId,Content Type ID;@_ModerationStatus,Approval Status;@_UIVersion,UI Version;@Created_x0020_Date,Created;@FileRef,URL Path;' /&gt;&lt;P N='NoDefaultStyle' T='TRUE' /&gt;&lt;P N='ParameterBindings' T='&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;dvt_apos&quot; Location=&quot;Postback;Connection&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;UserID&quot; Location=&quot;CAMLVariable&quot; DefaultValue=&quot;CurrentUserName&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;Today&quot; Location=&quot;CAMLVariable&quot; DefaultValue=&quot;CurrentDate&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;dvt_startposition&quot; Location=&quot;Postback&quot; DefaultValue=&quot;&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;ListID&quot; Location=&quot;None&quot; DefaultValue=&quot;{{@ListId:Absences}}&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;Param1&quot; Location=&quot;QueryString(ID)&quot; DefaultValue=&quot;1&quot;/&amp;gt;&amp;#xD;&amp;#xA;	' /&gt;&lt;P N='ParameterValues' Serial='AAEAAAD/////AQAAAAAAAAAMAgAAAFhNaWNyb3NvZnQuU2hhcmVQb2ludCwgVmVyc2lvbj0xMi4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj03MWU5YmNlMTExZTk0MjljBQEAAAA9TWljcm9zb2Z0LlNoYXJlUG9pbnQuV2ViUGFydFBhZ2VzLlBhcmFtZXRlck5hbWVWYWx1ZUhhc2h0YWJsZQEAAAAFX2NvbGwDHFN5c3RlbS5Db2xsZWN0aW9ucy5IYXNodGFibGUCAAAACgs' /&gt;&lt;P N='FilterValues' Serial='AAEAAAD/////AQAAAAAAAAAMAgAAAFhNaWNyb3NvZnQuU2hhcmVQb2ludCwgVmVyc2lvbj0xMi4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj03MWU5YmNlMTExZTk0MjljBQEAAAA9TWljcm9zb2Z0LlNoYXJlUG9pbnQuV2ViUGFydFBhZ2VzLlBhcmFtZXRlck5hbWVWYWx1ZUhhc2h0YWJsZQEAAAAFX2NvbGwDHFN5c3RlbS5Db2xsZWN0aW9ucy5IYXNodGFibGUCAAAACgs' /&gt;&lt;P N='Title' R='1' /&gt;&lt;P N='FrameType' E='0' /&gt;&lt;P N='ID' ID='2' T='g_b261bfeb_5342_4949_ba7f_cf8e0cca00c7' /&gt;&lt;P N='UseDefaultStyles' T='False' /&gt;&lt;P N='Qualifier' R='2' /&gt;&lt;P N='ClientName' T='varPartg_b261bfeb_5342_4949_ba7f_cf8e0cca00c7' /&gt;&lt;P N='EffectiveTitle' R='1' /&gt;&lt;P N='EffectiveFrameType' E='0' /&gt;&lt;P N='ChromeType' E='2' /&gt;&lt;P N='DisplayTitle' R='1' /&gt;&lt;P N='ExportMode' E='1' /&gt;&lt;P N='WebBrowsableObject' R='0' /&gt;&lt;P N='Font' ID='3' /&gt;&lt;P N='Page' ID='4' /&gt;&lt;P N='TemplateControl' R='4' /&gt;&lt;P N='AppRelativeTemplateSourceDirectory' R='-1' /&gt;" __AllowXSLTEditing="true" WebPart="true" Height="" Width="">
	<DataSources>
		<SharePoint:SPDataSource runat="server" DataSourceMode="List" UseInternalName="true" selectcommand="&lt;View&gt;&lt;Query&gt;&lt;Where&gt;&lt;Eq&gt;&lt;FieldRef Name=&quot;ID&quot;/&gt;&lt;Value Type=&quot;Counter&quot;&gt;{Param1}&lt;/Value&gt;&lt;/Eq&gt;&lt;/Where&gt;&lt;/Query&gt;&lt;/View&gt;" id="Absences1"><SelectParameters><WebPartPages:DataFormParameter Name="ListID" ParameterKey="ListID" PropertyName="ParameterValues" DefaultValue="{{@ListId:Absences}}"/><asp:Parameter Name="StartRowIndex" DefaultValue="0"/><asp:Parameter Name="nextpagedata" DefaultValue="0"/><asp:Parameter Name="MaximumRows" DefaultValue="1"/><WebPartPages:DataFormParameter Name="Param1" ParameterKey="Param1" PropertyName="ParameterValues" DefaultValue="1"/></SelectParameters><DeleteParameters><WebPartPages:DataFormParameter Name="ListID" ParameterKey="ListID" PropertyName="ParameterValues" DefaultValue="{{@ListId:Absences}}"/></DeleteParameters><UpdateParameters><WebPartPages:DataFormParameter Name="ListID" ParameterKey="ListID" PropertyName="ParameterValues" DefaultValue="{{@ListId:Absences}}"/></UpdateParameters><InsertParameters><WebPartPages:DataFormParameter Name="ListID" ParameterKey="ListID" PropertyName="ParameterValues" DefaultValue="{{@ListId:Absences}}"/></InsertParameters></SharePoint:SPDataSource>
	</DataSources>
	<ParameterBindings>
		<ParameterBinding Name="dvt_apos" Location="Postback;Connection"/>
		<ParameterBinding Name="UserID" Location="CAMLVariable" DefaultValue="CurrentUserName"/>
		<ParameterBinding Name="Today" Location="CAMLVariable" DefaultValue="CurrentDate"/>
		<ParameterBinding Name="dvt_startposition" Location="Postback" DefaultValue=""/>
		<ParameterBinding Name="ListID" Location="None" DefaultValue="{{@ListId:Absences}}"/>
		<ParameterBinding Name="Param1" Location="QueryString(ID)" DefaultValue="1"/>
	</ParameterBindings>
	<datafields>@Title,Title;@AVMAbsenceType,Absence Type;@EventDate,Start Time;@EndDate,End Time;@Description,Description;@fAllDayEvent,All Day Event;@fRecurrence,Recurrence;@AVMStatus,Status;@AVMComments,Comments;@SetAppro,SetApproval;@ApproveR,Approve-Reject;@ID,ID;@ContentType,Content Type;@Modified,Modified;@Created,Created;@Author,Created By;@Editor,Modified By;@_UIVersionString,Version;@Attachments,Attachments;@File_x0020_Type,File Type;@FileLeafRef,Name (for use in forms);@FileDirRef,Path;@FSObjType,Item Type;@_HasCopyDestinations,Has Copy Destinations;@_CopySource,Copy Source;@ContentTypeId,Content Type ID;@_ModerationStatus,Approval Status;@_UIVersion,UI Version;@Created_x0020_Date,Created;@FileRef,URL Path;</datafields>
	<XSL>
<xsl:stylesheet xmlns:x="http://www.w3.org/2001/XMLSchema" xmlns:d="http://schemas.microsoft.com/sharepoint/dsp" version="1.0" exclude-result-prefixes="xsl msxsl ddwrt" xmlns:ddwrt="http://schemas.microsoft.com/WebParts/v2/DataView/runtime" xmlns:asp="http://schemas.microsoft.com/ASPNET/20" xmlns:__designer="http://schemas.microsoft.com/WebParts/v2/DataView/designer" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:SharePoint="Microsoft.SharePoint.WebControls" xmlns:ddwrt2="urn:frontpage:internal">
	<xsl:output method="html" indent="no"/>
	<xsl:decimal-format NaN=""/>
	<xsl:param name="dvt_apos">'</xsl:param>
	<xsl:param name="ListID">{{@ListId:Absences}}</xsl:param>
	<xsl:param name="Param1">1</xsl:param>
	<xsl:variable name="dvt_1_automode">0</xsl:variable>
	<xsl:template match="/">
		<xsl:call-template name="dvt_1"/>
	</xsl:template>
	<xsl:template name="dvt_1">
		<xsl:variable name="dvt_StyleName">RepForm3</xsl:variable>
		<xsl:variable name="Rows" select="/dsQueryResponse/Rows/Row"/>
		<xsl:variable name="dvt_RowCount" select="count($Rows)" />
		<xsl:variable name="RowLimit" select="1" />
		<xsl:variable name="IsEmpty" select="$dvt_RowCount = 0" />
		<table border="0" width="100%">
		<tr><td colspan="2" class="ms-pagetitle">Approve / Reject</td></tr>
			<xsl:call-template name="dvt_1.body">
				<xsl:with-param name="Rows" select="$Rows"/>
				<xsl:with-param name="FirstRow" select="1" />
				<xsl:with-param name="LastRow" select="$dvt_RowCount" />
			</xsl:call-template>
		</table>		
	</xsl:template>
	<xsl:template name="dvt_1.body">
		<xsl:param name="Rows"/>
		<xsl:param name="FirstRow" />
		<xsl:param name="LastRow" />
		<xsl:for-each select="$Rows">
			<xsl:variable name="dvt_KeepItemsTogether" select="false()" />
			<xsl:variable name="dvt_HideGroupDetail" select="false()" />
			<xsl:if test="(position() &gt;= $FirstRow and position() &lt;= $LastRow) or $dvt_KeepItemsTogether">
				<xsl:if test="not($dvt_HideGroupDetail)" ddwrt:cf_ignore="1">
					<xsl:call-template name="dvt_1.rowview" />
				</xsl:if>
			</xsl:if>
		</xsl:for-each>
		
	</xsl:template>
	<xsl:template name="dvt_1.rowview">
		<tr>
			<td>
				<table border="0" cellspacing="0" width="100%">
					<tr>
						<td width="25%" class="ms-formlabel">
							<b>Absentee:</b>
						</td>
						<td width="75%" class="ms-formbody">
							<xsl:value-of select="@Author" disable-output-escaping="yes"/>
						</td>
					</tr>
					<tr>
						<td width="25%" class="ms-formlabel">
							<b>Title:</b>
						</td>
						<td width="75%" class="ms-formbody">
							<xsl:value-of select="@Title"/>
						</td>
					</tr>
					<tr>
						<td width="25%" class="ms-formlabel">
							<b>Absence Type:</b>
						</td>
						<td width="75%" class="ms-formbody">
							<xsl:value-of select="@AVMAbsenceType"/>
						</td>
					</tr>
					<tr>
						<td width="25%" class="ms-formlabel">
							<b>Start Time:</b>
						</td>
						<td width="75%" class="ms-formbody">
							<xsl:value-of select="ddwrt:FormatDate(string(@EventDate), 1033, 5)"/>
						</td>
					</tr>
					<tr>
						<td width="25%" class="ms-formlabel">
							<b>End Time:</b>
						</td>
						<td width="75%" class="ms-formbody">
							<xsl:value-of select="ddwrt:FormatDate(string(@EndDate), 1033, 5)"/>
						</td>
					</tr>
					<tr>
						<td width="25%" class="ms-formlabel">
							<b>Description:</b>
						</td>
						<td width="75%" class="ms-formbody">
							<xsl:value-of select="@Description" disable-output-escaping="yes"/>
						</td>
					</tr>
					<xsl:if test="$dvt_1_automode = '1'" ddwrt:cf_ignore="1">
						<tr>
							<td colspan="99" class="ms-vb">
								<span ddwrt:amkeyfield="ID" ddwrt:amkeyvalue="ddwrt:EscapeDelims(string(@ID))" ddwrt:ammode="view"></span>
							</td>
						</tr>
					</xsl:if>
				</table>
			</td>
		</tr>
	</xsl:template>	
</xsl:stylesheet>	</XSL>
</WebPartPages:DataFormWebPart>
<WebPartPages:DataFormWebPart runat="server" IsIncluded="True" FrameType="None" NoDefaultStyle="TRUE" ViewFlag="0" Title="" __markuptype="vsattributemarkup" __WebPartId="{3A2BF5A5-A054-4E05-AB07-F2E417B70329}" id="InitiationForm" pagesize="1" __AllowXSLTEditing="true" WebPart="true" Height="" Width="" __designer:Preview="&lt;Regions&gt;&lt;Region Name=&quot;0&quot; Editable=&quot;True&quot; Content=&quot;&quot; NamingContainer=&quot;True&quot; /&gt;&lt;/Regions&gt;&lt;table TOPLEVEL border=&quot;0&quot; cellpadding=&quot;0&quot; cellspacing=&quot;0&quot; width=&quot;100%&quot;&gt;
	&lt;tr&gt;
		&lt;td valign=&quot;top&quot;&gt;&lt;div WebPartID=&quot;00000000-0000-0000-0000-000000000000&quot; HasPers=&quot;true&quot; id=&quot;WebPartInitiationForm&quot; width=&quot;100%&quot; OnlyForMePart=&quot;true&quot; allowDelete=&quot;false&quot; style=&quot;&quot; &gt;&lt;div ID=&quot;WebPartContent&quot;&gt;The DataFormWebPart does not provide a design-time preview.&lt;/div&gt;&lt;/div&gt;&lt;/td&gt;
	&lt;/tr&gt;
&lt;/table&gt;" __designer:Values="&lt;P N='ViewFlag' T='0' /&gt;&lt;P N='DataSourcesString' T='&amp;lt;%@ Register TagPrefix=&quot;SharePoint&quot; Namespace=&quot;Microsoft.SharePoint.WebControls&quot; Assembly=&quot;Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c&quot; %&amp;gt;&amp;lt;%@ Register TagPrefix=&quot;WebPartPages&quot; Namespace=&quot;Microsoft.SharePoint.WebPartPages&quot; Assembly=&quot;Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c&quot; %&amp;gt;&amp;lt;SharePoint:SPWorkflowDataSource runat=&quot;server&quot; ListId=&quot;{{@ListId:Absences}}&quot; ID=&quot;SPWorkflowDataSource2&quot; ItemId=&quot;0&quot; BaseTemplateId=&quot;{D6292D7C-2B89-41DA-9169-02E606872E88}&quot;&amp;gt;&amp;lt;SelectParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;WebPartPages:DataFormParameter PropertyName=&quot;ParameterValues&quot; ParameterKey=&quot;AssociatedTemplateID&quot; Name=&quot;AssociatedTemplateID&quot;&amp;gt;&amp;lt;/WebPartPages:DataFormParameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;/SelectParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;InsertParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;WebPartPages:DataFormParameter PropertyName=&quot;ParameterValues&quot; ParameterKey=&quot;ItemID&quot; Name=&quot;ItemID&quot;&amp;gt;&amp;lt;/WebPartPages:DataFormParameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;WebPartPages:DataFormParameter PropertyName=&quot;ParameterValues&quot; ParameterKey=&quot;AssociatedTemplateID&quot; Name=&quot;AssociatedTemplateID&quot;&amp;gt;&amp;lt;/WebPartPages:DataFormParameter&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;/InsertParameters&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;/SharePoint:SPWorkflowDataSource&amp;gt;&amp;#xD;&amp;#xA;' /&gt;&lt;P N='PageSize' T='1' /&gt;&lt;P N='Xsl' T='&amp;#xD;&amp;#xA;&amp;lt;xsl:stylesheet xmlns:x=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:dsp=&quot;http://schemas.microsoft.com/sharepoint/dsp&quot; version=&quot;1.0&quot; exclude-result-prefixes=&quot;xsl msxsl ddwrt&quot; xmlns:ddwrt=&quot;http://schemas.microsoft.com/WebParts/v2/DataView/runtime&quot; xmlns:asp=&quot;http://schemas.microsoft.com/ASPNET/20&quot; xmlns:__designer=&quot;http://schemas.microsoft.com/WebParts/v2/DataView/designer&quot; xmlns:xsl=&quot;http://www.w3.org/1999/XSL/Transform&quot; xmlns:msxsl=&quot;urn:schemas-microsoft-com:xslt&quot; xmlns:SharePoint=&quot;Microsoft.SharePoint.WebControls&quot; xmlns:ddwrt2=&quot;urn:frontpage:internal&quot;&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:output method=&quot;html&quot; indent=&quot;no&quot;/&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:decimal-format NaN=&quot;&quot;/&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:param name=&quot;dvt_apos&quot;&amp;gt;&amp;apos;&amp;lt;/xsl:param&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:param name=&quot;dvt_firstrow&quot;&amp;gt;1&amp;lt;/xsl:param&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:param name=&quot;dvt_nextpagedata&quot; /&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:param name=&quot;AssociatedTemplateID&quot; /&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:param name=&quot;ItemID&quot; /&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:param name=&quot;Id&quot;&amp;gt;0&amp;lt;/xsl:param&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:param name=&quot;ListName&quot; /&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:variable name=&quot;dvt_1_automode&quot;&amp;gt;0&amp;lt;/xsl:variable&amp;gt;&amp;#xD;&amp;#xA;	&amp;#xD;&amp;#xA;	&amp;lt;xsl:template match=&quot;/&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:call-template name=&quot;dvt_1&quot;/&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;/xsl:template&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:template name=&quot;dvt_1&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;dvt_StyleName&quot;&amp;gt;RepForm3&amp;lt;/xsl:variable&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;Rows&quot; select=&quot;/dsQueryResponse/NewDataSet/Row&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;dvt_RowCount&quot; select=&quot;count($Rows)&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;RowLimit&quot; select=&quot;1&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;FirstRow&quot; select=&quot;$dvt_firstrow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;LastRow&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:choose&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:when test=&quot;($FirstRow + $RowLimit - 1) &amp;amp;gt; $dvt_RowCount&quot;&amp;gt;&amp;lt;xsl:value-of select=&quot;$dvt_RowCount&quot; /&amp;gt;&amp;lt;/xsl:when&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:otherwise&amp;gt;&amp;lt;xsl:value-of select=&quot;$FirstRow + $RowLimit - 1&quot; /&amp;gt;&amp;lt;/xsl:otherwise&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/xsl:choose&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/xsl:variable&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;IsEmpty&quot; select=&quot;$dvt_RowCount = 0&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;table border=&quot;0&quot; width=&quot;100%&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:call-template name=&quot;dvt_1.body&quot;&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:with-param name=&quot;Rows&quot; select=&quot;$Rows[position() &amp;amp;gt;= $FirstRow and position() &amp;amp;lt;= $LastRow]&quot;/&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:with-param name=&quot;FirstRow&quot; select=&quot;1&quot; /&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:with-param name=&quot;LastRow&quot; select=&quot;$dvt_RowCount&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/xsl:call-template&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/table&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:call-template name=&quot;dvt_1.commandfooter&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:with-param name=&quot;FirstRow&quot; select=&quot;$FirstRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:with-param name=&quot;LastRow&quot; select=&quot;$LastRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:with-param name=&quot;RowLimit&quot; select=&quot;$RowLimit&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:with-param name=&quot;dvt_RowCount&quot; select=&quot;$dvt_RowCount&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:with-param name=&quot;RealLastRow&quot; select=&quot;number(ddwrt:NameChanged(&amp;apos;&amp;apos;,-100))&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/xsl:call-template&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;/xsl:template&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:template name=&quot;dvt_1.body&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;Rows&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;FirstRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;LastRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:for-each select=&quot;$Rows&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:variable name=&quot;dvt_KeepItemsTogether&quot; select=&quot;false()&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:variable name=&quot;dvt_HideGroupDetail&quot; select=&quot;false()&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:if test=&quot;(position() &amp;amp;gt;= $FirstRow and position() &amp;amp;lt;= $LastRow) or $dvt_KeepItemsTogether&quot;&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:if test=&quot;not($dvt_HideGroupDetail)&quot; ddwrt:cf_ignore=&quot;1&quot;&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;xsl:call-template name=&quot;dvt_1.rowedit&quot;&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;xsl:with-param name=&quot;Pos&quot; select=&quot;concat(&amp;apos;_&amp;apos;, position())&quot; /&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/xsl:call-template&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;/xsl:if&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/xsl:if&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/xsl:for-each&amp;gt;		&amp;#xD;&amp;#xA;	&amp;lt;/xsl:template&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:template name=&quot;dvt_1.rowedit&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;Pos&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;td&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;table border=&quot;0&quot; cellspacing=&quot;0&quot; width=&quot;100%&quot;&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;25%&quot; class=&quot;ms-formlabel&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;b&amp;gt;Approve/Reject:&amp;lt;/b&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;75%&quot; class=&quot;ms-formbody&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;SharePoint:DVDropDownList runat=&quot;server&quot; id=&quot;ff1{$Pos}&quot; SelectedValue=&quot;{@ApproveReject}&quot; __designer:bind=&quot;{ddwrt:DataBind(&amp;apos;i&amp;apos;,concat(&amp;apos;ff1&amp;apos;,$Pos),&amp;apos;SelectedValue&amp;apos;,&amp;apos;SelectedIndexChanged&amp;apos;,&amp;apos;&amp;apos;,ddwrt:EscapeDelims(string(&amp;apos;&amp;apos;)),&amp;apos;@ApproveReject&amp;apos;)}&quot;&amp;gt;&amp;#xD;&amp;#xA;								&amp;lt;asp:ListItem&amp;gt;Approved&amp;lt;/asp:ListItem&amp;gt;&amp;#xD;&amp;#xA;								&amp;lt;asp:ListItem&amp;gt;Rejected&amp;lt;/asp:ListItem&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;/SharePoint:DVDropDownList&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;25%&quot; class=&quot;ms-formlabel&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;b&amp;gt;Comments:&amp;lt;/b&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;td width=&quot;75%&quot; class=&quot;ms-formbody&quot;&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;asp:textbox runat=&quot;server&quot; id=&quot;ff2{$Pos}&quot; text=&quot;{@Comments}&quot; TextMode=&quot;MultiLine&quot; __designer:bind=&quot;{ddwrt:DataBind(&amp;apos;i&amp;apos;,concat(&amp;apos;ff2&amp;apos;,$Pos),&amp;apos;Text&amp;apos;,&amp;apos;TextChanged&amp;apos;,&amp;apos;&amp;apos;,ddwrt:EscapeDelims(string(&amp;apos;&amp;apos;)),&amp;apos;@Comments&amp;apos;)}&quot; Width=&quot;350px&quot;/&amp;gt;&amp;#xD;&amp;#xA;							&amp;#xD;&amp;#xA;							&amp;#xD;&amp;#xA;							&amp;#xD;&amp;#xA;						&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;xsl:if test=&quot;$dvt_1_automode = &amp;apos;1&amp;apos;&quot; ddwrt:cf_ignore=&quot;1&quot;&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;td colspan=&quot;99&quot; class=&quot;ms-vb&quot;&amp;gt;&amp;#xD;&amp;#xA;								&amp;lt;span ddwrt:amkeyfield=&quot;&quot; ddwrt:amkeyvalue=&quot;ddwrt:EscapeDelims(string(&amp;apos;&amp;apos;))&quot; ddwrt:ammode=&quot;edit&quot;&amp;gt;&amp;lt;/span&amp;gt;&amp;#xD;&amp;#xA;							&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/xsl:if&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;/table&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;		&amp;#xD;&amp;#xA;	&amp;lt;/xsl:template&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:template name=&quot;dvt_1.commandfooter&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;FirstRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;LastRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;RowLimit&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;dvt_RowCount&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;RealLastRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;table cellspacing=&quot;0&quot; cellpadding=&quot;4&quot; border=&quot;0&quot; width=&quot;100%&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;tr&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:call-template name=&quot;dvt_1.formactions&quot; /&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:if test=&quot;$FirstRow &amp;amp;gt; 1 or $LastRow &amp;amp;lt; $dvt_RowCount&quot;&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;xsl:call-template name=&quot;dvt_1.navigation&quot;&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;xsl:with-param name=&quot;FirstRow&quot; select=&quot;$FirstRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;xsl:with-param name=&quot;LastRow&quot; select=&quot;$LastRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;xsl:with-param name=&quot;RowLimit&quot; select=&quot;$RowLimit&quot; /&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;xsl:with-param name=&quot;dvt_RowCount&quot; select=&quot;$dvt_RowCount&quot; /&amp;gt;&amp;#xD;&amp;#xA;						&amp;lt;xsl:with-param name=&quot;RealLastRow&quot; select=&quot;$RealLastRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;/xsl:call-template&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;/xsl:if&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/tr&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/table&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;/xsl:template&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:template name=&quot;dvt_1.navigation&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;FirstRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;LastRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;RowLimit&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;dvt_RowCount&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:param name=&quot;RealLastRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;PrevRow&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:choose&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:when test=&quot;$FirstRow - $RowLimit &amp;amp;lt; 1&quot;&amp;gt;1&amp;lt;/xsl:when&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:otherwise&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;xsl:value-of select=&quot;$FirstRow - $RowLimit&quot; /&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;/xsl:otherwise&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/xsl:choose&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/xsl:variable&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;LastRowValue&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:choose&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:when test=&quot;$LastRow &amp;amp;gt; $RealLastRow&quot;&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;xsl:value-of select=&quot;$LastRow&quot;&amp;gt;&amp;lt;/xsl:value-of&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;/xsl:when&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:otherwise&amp;gt;&amp;#xD;&amp;#xA;					&amp;lt;xsl:value-of select=&quot;$RealLastRow&quot;&amp;gt;&amp;lt;/xsl:value-of&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;/xsl:otherwise&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/xsl:choose&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/xsl:variable&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;xsl:variable name=&quot;NextRow&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:value-of select=&quot;$LastRowValue + 1&quot;&amp;gt;&amp;lt;/xsl:value-of&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/xsl:variable&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;td nowrap=&quot;nowrap&quot; class=&quot;ms-paging&quot; align=&quot;right&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:if test=&quot;$dvt_firstrow &amp;amp;gt; 1&quot; ddwrt:cf_ignore=&quot;1&quot;&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;a&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:attribute name=&quot;href&quot;&amp;gt;javascript: &amp;lt;xsl:value-of select=&quot;ddwrt:GenFireServerEvent(&amp;apos;dvt_firstrow={1}&amp;apos;)&quot; /&amp;gt;;&amp;lt;/xsl:attribute&amp;gt;&amp;#xD;&amp;#xA;				Start&amp;lt;/a&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:text disable-output-escaping=&quot;yes&quot; ddwrt:nbsp-preserve=&quot;yes&quot;&amp;gt;&amp;amp;amp;nbsp;&amp;lt;/xsl:text&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;a&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:attribute name=&quot;href&quot;&amp;gt;javascript: &amp;lt;xsl:value-of select=&quot;ddwrt:GenFireServerEvent(concat(&amp;apos;dvt_firstrow={&amp;apos;,$PrevRow,&amp;apos;}&amp;apos;))&quot; /&amp;gt;;&amp;lt;/xsl:attribute&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;img src=&quot;/_layouts/images/prev.gif&quot; border=&quot;0&quot; alt=&quot;Previous&quot; /&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;/a&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:text disable-output-escaping=&quot;yes&quot; ddwrt:nbsp-preserve=&quot;yes&quot;&amp;gt;&amp;amp;amp;nbsp;&amp;lt;/xsl:text&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/xsl:if&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:value-of select=&quot;$FirstRow&quot; /&amp;gt;&amp;#xD;&amp;#xA;			 - &amp;lt;xsl:value-of select=&quot;$LastRowValue&quot; /&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:text disable-output-escaping=&quot;yes&quot; ddwrt:nbsp-preserve=&quot;yes&quot;&amp;gt;&amp;amp;amp;nbsp;&amp;lt;/xsl:text&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;xsl:if test=&quot;$LastRowValue &amp;amp;lt; $dvt_RowCount or string-length($dvt_nextpagedata)!=0&quot; ddwrt:cf_ignore=&quot;1&quot;&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;a&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;xsl:attribute name=&quot;href&quot;&amp;gt;javascript: &amp;lt;xsl:value-of select=&quot;ddwrt:GenFireServerEvent(concat(&amp;apos;dvt_firstrow={&amp;apos;,$NextRow,&amp;apos;}&amp;apos;))&quot; /&amp;gt;;&amp;lt;/xsl:attribute&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;img src=&quot;/_layouts/images/next.gif&quot; border=&quot;0&quot; alt=&quot;Next&quot; /&amp;gt;&amp;#xD;&amp;#xA;				&amp;lt;/a&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;/xsl:if&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;/xsl:template&amp;gt;&amp;#xD;&amp;#xA;	&amp;lt;xsl:template name=&quot;dvt_1.formactions&quot;&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;td nowrap=&quot;nowrap&quot; class=&quot;ms-vb&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;input type=&quot;button&quot; value=&quot;Save&quot; name=&quot;btnSave&quot; onclick=&quot;javascript: {ddwrt:GenFireServerEvent(concat(&amp;apos;__insert;__commit;__redirectsource;__redirectToList={&amp;apos;,ddwrt:EcmaScriptEncode($ListName),&amp;apos;};&amp;apos;))}&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/td&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;td nowrap=&quot;nowrap&quot; class=&quot;ms-vb&quot; width=&quot;99%&quot;&amp;gt;&amp;#xD;&amp;#xA;			&amp;lt;input type=&quot;button&quot; value=&quot;Cancel&quot; name=&quot;btnCancel&quot; onclick=&quot;javascript: {ddwrt:GenFireServerEvent(concat(&amp;apos;__cancel;__redirectsource;__redirectToList={&amp;apos;,ddwrt:EcmaScriptEncode($ListName),&amp;apos;};&amp;apos;))}&quot; /&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;/td&amp;gt;&amp;lt;/xsl:template&amp;gt;&amp;#xD;&amp;#xA;&amp;lt;/xsl:stylesheet&amp;gt;	' /&gt;&lt;P N='DataFields' T='@ApproveReject,Approve/Reject;@Comments,Comments;' /&gt;&lt;P N='NoDefaultStyle' T='TRUE' /&gt;&lt;P N='ParameterBindings' T='&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;dvt_apos&quot; Location=&quot;Postback;Connection&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;UserID&quot; Location=&quot;CAMLVariable&quot; DefaultValue=&quot;CurrentUserName&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;Today&quot; Location=&quot;CAMLVariable&quot; DefaultValue=&quot;CurrentDate&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;dvt_firstrow&quot; Location=&quot;Postback;Connection&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;dvt_nextpagedata&quot; Location=&quot;Postback;Connection&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;AssociatedTemplateID&quot; Location=&quot;QueryString(TemplateID)&quot; DefaultValue=&quot;&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;ItemID&quot; Location=&quot;QueryString(ID)&quot; DefaultValue=&quot;&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;Id&quot; Location=&quot;QueryString(ID)&quot; DefaultValue=&quot;0&quot;/&amp;gt;&amp;#xD;&amp;#xA;		&amp;lt;ParameterBinding Name=&quot;ListName&quot; Location=&quot;QueryString(List)&quot; DefaultValue=&quot;&quot;/&amp;gt;&amp;#xD;&amp;#xA;	' /&gt;&lt;P N='ParameterValues' Serial='AAEAAAD/////AQAAAAAAAAAMAgAAAFhNaWNyb3NvZnQuU2hhcmVQb2ludCwgVmVyc2lvbj0xMi4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj03MWU5YmNlMTExZTk0MjljBQEAAAA9TWljcm9zb2Z0LlNoYXJlUG9pbnQuV2ViUGFydFBhZ2VzLlBhcmFtZXRlck5hbWVWYWx1ZUhhc2h0YWJsZQEAAAAFX2NvbGwDHFN5c3RlbS5Db2xsZWN0aW9ucy5IYXNodGFibGUCAAAACgs' /&gt;&lt;P N='FilterValues' Serial='AAEAAAD/////AQAAAAAAAAAMAgAAAFhNaWNyb3NvZnQuU2hhcmVQb2ludCwgVmVyc2lvbj0xMi4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj03MWU5YmNlMTExZTk0MjljBQEAAAA9TWljcm9zb2Z0LlNoYXJlUG9pbnQuV2ViUGFydFBhZ2VzLlBhcmFtZXRlck5hbWVWYWx1ZUhhc2h0YWJsZQEAAAAFX2NvbGwDHFN5c3RlbS5Db2xsZWN0aW9ucy5IYXNodGFibGUCAAAACgs' /&gt;&lt;P N='FrameType' E='0' /&gt;&lt;P N='ID' ID='1' T='InitiationForm' /&gt;&lt;P N='UseDefaultStyles' T='False' /&gt;&lt;P N='Qualifier' R='1' /&gt;&lt;P N='ClientName' T='varPartInitiationForm' /&gt;&lt;P N='EffectiveFrameType' E='0' /&gt;&lt;P N='ChromeType' E='2' /&gt;&lt;P N='ExportMode' E='1' /&gt;&lt;P N='WebBrowsableObject' R='0' /&gt;&lt;P N='Font' ID='2' /&gt;&lt;P N='Page' ID='3' /&gt;&lt;P N='TemplateControl' R='3' /&gt;&lt;P N='AppRelativeTemplateSourceDirectory' R='-1' /&gt;">
	<DataSources>
		<SharePoint:SPWorkflowDataSource BaseTemplateID="{D6292D7C-2B89-41DA-9169-02E606872E88}" ListID="{{@ListId:Absences}}" runat="server" id="SPWorkflowDataSource2"><SelectParameters><WebPartPages:DataFormParameter Name="AssociatedTemplateID" ParameterKey="AssociatedTemplateID" PropertyName="ParameterValues"/></SelectParameters><InsertParameters><WebPartPages:DataFormParameter Name="ItemID" ParameterKey="ItemID" PropertyName="ParameterValues"/><WebPartPages:DataFormParameter Name="AssociatedTemplateID" ParameterKey="AssociatedTemplateID" PropertyName="ParameterValues"/></InsertParameters></SharePoint:SPWorkflowDataSource>
	</DataSources>
	<ParameterBindings>
		<ParameterBinding Name="dvt_apos" Location="Postback;Connection"/>
		<ParameterBinding Name="UserID" Location="CAMLVariable" DefaultValue="CurrentUserName"/>
		<ParameterBinding Name="Today" Location="CAMLVariable" DefaultValue="CurrentDate"/>
		<ParameterBinding Name="dvt_firstrow" Location="Postback;Connection"/>
		<ParameterBinding Name="dvt_nextpagedata" Location="Postback;Connection"/>
		<ParameterBinding Name="AssociatedTemplateID" Location="QueryString(TemplateID)" DefaultValue=""/>
		<ParameterBinding Name="ItemID" Location="QueryString(ID)" DefaultValue=""/>
		<ParameterBinding Name="Id" Location="QueryString(ID)" DefaultValue="0"/>
		<ParameterBinding Name="ListName" Location="QueryString(List)" DefaultValue=""/>
	</ParameterBindings>
	<datafields>@ApproveReject,Approve/Reject;@Comments,Comments;</datafields>
	<XSL>
<xsl:stylesheet xmlns:x="http://www.w3.org/2001/XMLSchema" xmlns:dsp="http://schemas.microsoft.com/sharepoint/dsp" version="1.0" exclude-result-prefixes="xsl msxsl ddwrt" xmlns:ddwrt="http://schemas.microsoft.com/WebParts/v2/DataView/runtime" xmlns:asp="http://schemas.microsoft.com/ASPNET/20" xmlns:__designer="http://schemas.microsoft.com/WebParts/v2/DataView/designer" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:SharePoint="Microsoft.SharePoint.WebControls" xmlns:ddwrt2="urn:frontpage:internal">
	<xsl:output method="html" indent="no"/>
	<xsl:decimal-format NaN=""/>
	<xsl:param name="dvt_apos">'</xsl:param>
	<xsl:param name="dvt_firstrow">1</xsl:param>
	<xsl:param name="dvt_nextpagedata" />
	<xsl:param name="AssociatedTemplateID" />
	<xsl:param name="ItemID" />
	<xsl:param name="Id">0</xsl:param>
	<xsl:param name="ListName" />
	<xsl:variable name="dvt_1_automode">0</xsl:variable>
	
	<xsl:template match="/">
		<xsl:call-template name="dvt_1"/>
	</xsl:template>
	<xsl:template name="dvt_1">
		<xsl:variable name="dvt_StyleName">RepForm3</xsl:variable>
		<xsl:variable name="Rows" select="/dsQueryResponse/NewDataSet/Row"/>
		<xsl:variable name="dvt_RowCount" select="count($Rows)" />
		<xsl:variable name="RowLimit" select="1" />
		<xsl:variable name="FirstRow" select="$dvt_firstrow" />
		<xsl:variable name="LastRow">
			<xsl:choose>
				<xsl:when test="($FirstRow + $RowLimit - 1) &gt; $dvt_RowCount"><xsl:value-of select="$dvt_RowCount" /></xsl:when>
				<xsl:otherwise><xsl:value-of select="$FirstRow + $RowLimit - 1" /></xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="IsEmpty" select="$dvt_RowCount = 0" />
		<table border="0" width="100%">
			<xsl:call-template name="dvt_1.body">
				<xsl:with-param name="Rows" select="$Rows[position() &gt;= $FirstRow and position() &lt;= $LastRow]"/>
				<xsl:with-param name="FirstRow" select="1" />
				<xsl:with-param name="LastRow" select="$dvt_RowCount" />
			</xsl:call-template>
		</table>
		<xsl:call-template name="dvt_1.commandfooter">
			<xsl:with-param name="FirstRow" select="$FirstRow" />
			<xsl:with-param name="LastRow" select="$LastRow" />
			<xsl:with-param name="RowLimit" select="$RowLimit" />
			<xsl:with-param name="dvt_RowCount" select="$dvt_RowCount" />
			<xsl:with-param name="RealLastRow" select="number(ddwrt:NameChanged('',-100))" />
		</xsl:call-template>
	</xsl:template>
	<xsl:template name="dvt_1.body">
		<xsl:param name="Rows"/>
		<xsl:param name="FirstRow" />
		<xsl:param name="LastRow" />
		<xsl:for-each select="$Rows">
			<xsl:variable name="dvt_KeepItemsTogether" select="false()" />
			<xsl:variable name="dvt_HideGroupDetail" select="false()" />
			<xsl:if test="(position() &gt;= $FirstRow and position() &lt;= $LastRow) or $dvt_KeepItemsTogether">
				<xsl:if test="not($dvt_HideGroupDetail)" ddwrt:cf_ignore="1">
					<xsl:call-template name="dvt_1.rowedit">
						<xsl:with-param name="Pos" select="concat('_', position())" />
					</xsl:call-template>
				</xsl:if>
			</xsl:if>
		</xsl:for-each>		
	</xsl:template>
	<xsl:template name="dvt_1.rowedit">
		<xsl:param name="Pos" />
		<tr>
			<td>
				<table border="0" cellspacing="0" width="100%">
					<tr>
						<td width="25%" class="ms-formlabel">
							<b>Approve/Reject:</b>
						</td>
						<td width="75%" class="ms-formbody">
							<SharePoint:DVDropDownList runat="server" id="ff1{$Pos}" SelectedValue="{@ApproveReject}" __designer:bind="{ddwrt:DataBind('i',concat('ff1',$Pos),'SelectedValue','SelectedIndexChanged','',ddwrt:EscapeDelims(string('')),'@ApproveReject')}">
								<asp:ListItem>Approved</asp:ListItem>
								<asp:ListItem>Rejected</asp:ListItem>
							</SharePoint:DVDropDownList>
						</td>
					</tr>
					<tr>
						<td width="25%" class="ms-formlabel">
							<b>Comments:</b>
						</td>
						<td width="75%" class="ms-formbody">
							<asp:textbox runat="server" id="ff2{$Pos}" text="{@Comments}" TextMode="MultiLine" __designer:bind="{ddwrt:DataBind('i',concat('ff2',$Pos),'Text','TextChanged','',ddwrt:EscapeDelims(string('')),'@Comments')}" Width="350px"/>
							
							
							
						</td>
					</tr>
					<xsl:if test="$dvt_1_automode = '1'" ddwrt:cf_ignore="1">
						<tr>
							<td colspan="99" class="ms-vb">
								<span ddwrt:amkeyfield="" ddwrt:amkeyvalue="ddwrt:EscapeDelims(string(''))" ddwrt:ammode="edit"></span>
							</td>
						</tr>
					</xsl:if>
				</table>
			</td>
		</tr>
		
	</xsl:template>
	<xsl:template name="dvt_1.commandfooter">
		<xsl:param name="FirstRow" />
		<xsl:param name="LastRow" />
		<xsl:param name="RowLimit" />
		<xsl:param name="dvt_RowCount" />
		<xsl:param name="RealLastRow" />
		<table cellspacing="0" cellpadding="4" border="0" width="100%">
			<tr>
				<xsl:call-template name="dvt_1.formactions" />
				<xsl:if test="$FirstRow &gt; 1 or $LastRow &lt; $dvt_RowCount">
					<xsl:call-template name="dvt_1.navigation">
						<xsl:with-param name="FirstRow" select="$FirstRow" />
						<xsl:with-param name="LastRow" select="$LastRow" />
						<xsl:with-param name="RowLimit" select="$RowLimit" />
						<xsl:with-param name="dvt_RowCount" select="$dvt_RowCount" />
						<xsl:with-param name="RealLastRow" select="$RealLastRow" />
					</xsl:call-template>
				</xsl:if>
			</tr>
		</table>
	</xsl:template>
	<xsl:template name="dvt_1.navigation">
		<xsl:param name="FirstRow" />
		<xsl:param name="LastRow" />
		<xsl:param name="RowLimit" />
		<xsl:param name="dvt_RowCount" />
		<xsl:param name="RealLastRow" />
		<xsl:variable name="PrevRow">
			<xsl:choose>
				<xsl:when test="$FirstRow - $RowLimit &lt; 1">1</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$FirstRow - $RowLimit" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="LastRowValue">
			<xsl:choose>
				<xsl:when test="$LastRow &gt; $RealLastRow">
					<xsl:value-of select="$LastRow"></xsl:value-of>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$RealLastRow"></xsl:value-of>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="NextRow">
			<xsl:value-of select="$LastRowValue + 1"></xsl:value-of>
		</xsl:variable>
		<td nowrap="nowrap" class="ms-paging" align="right">
			<xsl:if test="$dvt_firstrow &gt; 1" ddwrt:cf_ignore="1">
				<a>
				<xsl:attribute name="href">javascript: <xsl:value-of select="ddwrt:GenFireServerEvent('dvt_firstrow={1}')" />;</xsl:attribute>
				Start</a>
				<xsl:text disable-output-escaping="yes" ddwrt:nbsp-preserve="yes">&amp;nbsp;</xsl:text>
				<a>
				<xsl:attribute name="href">javascript: <xsl:value-of select="ddwrt:GenFireServerEvent(concat('dvt_firstrow={',$PrevRow,'}'))" />;</xsl:attribute>
				<img src="/_layouts/images/prev.gif" border="0" alt="Previous" />
				</a>
				<xsl:text disable-output-escaping="yes" ddwrt:nbsp-preserve="yes">&amp;nbsp;</xsl:text>
			</xsl:if>
			<xsl:value-of select="$FirstRow" />
			 - <xsl:value-of select="$LastRowValue" />
			<xsl:text disable-output-escaping="yes" ddwrt:nbsp-preserve="yes">&amp;nbsp;</xsl:text>
			<xsl:if test="$LastRowValue &lt; $dvt_RowCount or string-length($dvt_nextpagedata)!=0" ddwrt:cf_ignore="1">
				<a>
				<xsl:attribute name="href">javascript: <xsl:value-of select="ddwrt:GenFireServerEvent(concat('dvt_firstrow={',$NextRow,'}'))" />;</xsl:attribute>
				<img src="/_layouts/images/next.gif" border="0" alt="Next" />
				</a>
			</xsl:if>
		</td>
	</xsl:template>
	<xsl:template name="dvt_1.formactions">
		<td nowrap="nowrap" class="ms-vb">
			<input type="button" value="Save" name="btnSave" onclick="javascript: {ddwrt:GenFireServerEvent(concat('__insert;__commit;__redirectsource;__redirectToList={',ddwrt:EcmaScriptEncode($ListName),'};'))}" />
		</td>
		<td nowrap="nowrap" class="ms-vb" width="99%">
			<input type="button" value="Cancel" name="btnCancel" onclick="javascript: {ddwrt:GenFireServerEvent(concat('__cancel;__redirectsource;__redirectToList={',ddwrt:EcmaScriptEncode($ListName),'};'))}" />
		</td></xsl:template>
</xsl:stylesheet>	</XSL>
</WebPartPages:DataFormWebPart></asp:content>
