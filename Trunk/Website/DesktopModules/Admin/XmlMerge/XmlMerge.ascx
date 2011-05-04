<%@ Control language="C#" Inherits="DotNetNuke.Modules.XmlMerge.XmlMerge" CodeFile="XmlMerge.ascx.cs" AutoEventWireup="false" Explicit="True" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>

<script language="javascript" type="text/javascript">
    $(document).ready(function () {
        $('#dnnConfigManager').dnnPanels();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            $('#dnnConfigManager').dnnPanels();
        });
    });

</script>

<div class="dnnForm dnnConfigManager dnnClear" id="dnnConfigManager">
    <h2 id="Panel-Configuration" class="dnnFormSectionHead"><a href="" class=""><%=LocalizeString("Configuration")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label ID="plConfig" runat="server" ControlName="ddlConfig" />
            <dnn:DnnComboBox ID="ddlConfig" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlConfig_SelectedIndexChanged" />                
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="fileLabel" runat="server" ControlName="txtConfiguration" />
            <asp:TextBox ID="txtConfiguration" runat="server" TextMode="MultiLine" Rows="20" Columns="75" EnableViewState="True" Enabled="false"/>
        </div>
        <ul class="dnnActions dnnClear">
            <li><asp:LinkButton ID="cmdSave" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdSave"/></li>
        </ul>
    </fieldset>       
    <h2 id="Panel-Merge" class="dnnFormSectionHead"><a href="" class=""><%=LocalizeString("Merge")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label ID="plScript" runat="server" ControlName="uplScript" Suffix=""></dnn:Label>
            <asp:FileUpload ID="uplScript" runat="server" />
            <asp:LinkButton ID="cmdUpload" resourcekey="cmdUpload" EnableViewState="False" CssClass="dnnSecondaryAction" runat="server"/>
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="scriptLabel" runat="server" ControlName="txtScript" />
             <asp:TextBox ID="txtScript" runat="server" TextMode="MultiLine" Rows="20" Columns="75" EnableViewState="False"/>           
        </div>
        <ul class="dnnActions dnnClear">
            <li><asp:LinkButton ID="cmdExecute" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdExecute"/></li>
        </ul>
     </fieldset>       
</div>

<asp:Label ID="lblMessage" runat="server" CssClass="NormalRed" EnableViewState="False"></asp:Label>