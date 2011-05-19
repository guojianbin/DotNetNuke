<%@ Control Inherits="DotNetNuke.Modules.Admin.Newsletters.Newsletter" Language="C#" AutoEventWireup="false" CodeFile="Newsletter.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="TextEditor" Src="~/controls/TextEditor.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="URLControl" Src="~/controls/URLControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<div class="dnnForm dnnNewsletters dnnClear" id="dnnNewsletters">
    <ul class="dnnAdminTabNav dnnClear">
		<li><a href="#newBasicSettings"><%=LocalizeString("BasicSettings")%></a></li>
		<li><a href="#newMessage"><%=LocalizeString("Message")%></a></li>
		<li><a href="#newAdvancedSettings"><%=LocalizeString("AdvancedSettings")%></a></li>
	</ul>
    <div class="newBasicSettings" id="newBasicSettings">
        <fieldset>
            <div class="dnnFormItem">
                <dnn:Label ID="plRoles" runat="server" ControlName="chkRoles" />
                <dnn:RolesSelectionGrid runat="server" ID="dgSelectedRoles" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label id="plLanguages" runat="server" ControlName="selLanguage" />
                <dnn:LanguageSelector runat="server" ID="selLanguage" SelectionMode="Multiple" ListDirection="Vertical" ItemStyle="FlagAndCaption" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plEmail" runat="server" controlname="txtEmail"/>
                <asp:TextBox ID="txtEmail" runat="server" TextMode="MultiLine" rows="3" Columns="60" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plFrom" runat="server" controlname="txtFrom"/>
                <asp:TextBox ID="txtFrom" runat="server" MaxLength="100"/>
                <asp:RegularExpressionValidator ID="revEmailAddress" runat="server" resourcekey="revEmailAddress.ErrorMessage"
                    CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtFrom"
                    ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plReplyTo" runat="server" controlname="txtReplyTo" />
                <asp:TextBox ID="txtReplyTo" runat="server" MaxLength="100" />
                <asp:RegularExpressionValidator ID="revReplyTo" runat="server" resourcekey="revEmailAddress.ErrorMessage" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtReplyTo" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="plSubject" runat="server" ControlName="txtSubject" />
                <asp:TextBox ID="txtSubject" runat="server" MaxLength="100" />
            </div>
        </fieldset>
    </div>
    <div class="newMessage" id="newMessage">
        <div class="dnnFormItem dnnTextEditor">
            <dnn:TextEditor ID="teMessage" runat="server" Width="750" TextRenderMode="Raw" HtmlEncode="False" defaultmode="Rich" height="350" choosemode="True" chooserender="False" />
        </div>
    </div>    
    <div class="newAdvancedSettings" id="newAdvancedSettings">
        <fieldset>
            <div class="dnnFormItem dnnNewsletterAttachment">
                <dnn:Label id="plAttachment" runat="server" ControlName="ctlAttachment" />
                <dnn:URLControl id="ctlAttachment" runat="server" Required="False" ShowUpLoad="true" ShowTrack="False" ShowLog="False" ShowTabs="False" ShowUrls="False" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plReplaceTokens" runat="server" controlname="chkReplaceTokens" />
                <asp:CheckBox id="chkReplaceTokens" runat="server" Checked="true" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label id="plPriority" runat="server" ControlName="cboPriority" />
                <asp:DropDownList id="cboPriority" runat="server">
                    <asp:ListItem resourcekey="High" Value="1" />
                    <asp:ListItem resourcekey="Normal" Value="2" Selected="True" />
                    <asp:ListItem resourcekey="Low" Value="3" />
                </asp:DropDownList>
            </div>
            <div class="dnnFormItem">
                <dnn:Label id="plSendMethod" runat="server" ControlName="cboSendMethod" />
                <asp:DropDownList id="cboSendMethod" runat="server" AutoPostBack="true">
                    <asp:ListItem resourcekey="SendTo" Value="TO" Selected="True" />
                    <asp:ListItem resourcekey="SendBCC" Value="BCC" />
                    <asp:ListItem resourcekey="SendRelay" Value="RELAY" />
                </asp:DropDownList>
            </div>
            <div id="pnlRelayAddress" runat="server" visible="false" class="dnnFormItem">
                <dnn:label id="plRelayAddress" runat="server" controlname="txtRelayAddress" />
                <asp:TextBox ID="txtRelayAddress" runat="server" Columns="40" MaxLength="100" />
                <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" resourcekey="revEmailAddress.ErrorMessage" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtRelayAddress" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label id="plSendAction" runat="server" ControlName="optSendAction" />
                <asp:RadioButtonList id="optSendAction" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="dnnNewsletterRadios">
                    <asp:ListItem resourcekey="Synchronous" Value="S" />
                    <asp:ListItem resourcekey="Asynchronous" Value="A" Selected="True" />
                </asp:RadioButtonList>
            </div>
        </fieldset>
    </div>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton ID="cmdSend" resourcekey="cmdSend" runat="server" CssClass="dnnPrimaryAction" /></li>
        <li><asp:LinkButton ID="cmdPreview" resourcekey="cmdPreview" runat="server" CssClass="dnnSecondaryAction dnnDeleteRole" /></li>
    </ul>
</div>
<asp:Panel ID="pnlPreview" runat="server" EnableViewState="false" Visible="false">
    <table>
        <tr>
            <td style="width:100px; vertical-align:top"><asp:Label ID="label3" runat="server" resourcekey="Preview" >Preview</asp:Label></td>
            <td style="width:460px;"></td>
        </tr>
        <tr>
            <td style="width:100px; vertical-align:top"><asp:Label ID="label1" runat="server" resourcekey="plSubject" ></asp:Label></td>
            <td style="width:460px;"><asp:Label id="lblPreviewSubject" runat="server"></asp:Label></td>
        </tr>
        <tr>
            <td style="width:100px; vertical-align:top"><asp:Label ID="label2" runat="server" resourcekey="Message" ></asp:Label></td>
            <td style="width:460px;"><asp:Label id="lblPreviewBody" runat="server"></asp:Label></td>
        </tr>
    </table>
</asp:Panel>
<script language="javascript" type="text/javascript">
    function setUpDnnNewsletter() {
        $('#dnnNewsletters').dnnTabs();
    }
    $(document).ready(function () {
        setUpDnnNewsletter();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setUpDnnNewsletter();
        });
    });
</script>