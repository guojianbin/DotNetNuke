﻿<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="EditVocabularyControl.ascx.cs" Inherits="DotNetNuke.Modules.Taxonomy.Views.Controls.EditVocabularyControl" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnEditVocabControl dnnClear">
    <div class="dnnFormItem">
        <dnn:DnnFieldLabel id="nameFieldLabel" runat="server" Text="Name.Text" ToolTip="Name.ToolTip" />
        <asp:Label ID="nameLabel" runat="server" />
        <asp:TextBox ID="nameTextBox" runat="server" />
        <asp:RequiredFieldValidator ID="nameValidator" runat="server" ResourceKey="Name.Required" Display="Dynamic" ControlToValidate="nameTextBox" CssClass="dnnFormMessage dnnFormError" SetFocusOnError="true" />
    </div>
    <div class="dnnFormItem">
        <dnn:DnnFieldLabel id="descriptionFieldLabel" runat="server" Text="Description.Text" ToolTip="Description.ToolTip" />
        <asp:TextBox ID="descriptionTextBox" runat="server" TextMode="MultiLine" />
    </div>
    <div class="dnnFormItem">
        <dnn:DnnFieldLabel id="typeFieldLabel" runat="server" Text="Type.Text" ToolTip="Type.ToolTip" />
        <asp:RadioButtonList ID="typeList" runat="server" RepeatDirection="Horizontal" CssClass="dnnVocabRadioButtons">
            <asp:ListItem Value="Simple" resourceKey="Simple" />
            <asp:ListItem Value="Hierarchy" resourceKey="Hierarchy" />
        </asp:RadioButtonList>
        <asp:Label ID="typeLabel" runat="server" />
    </div>
    <div class="dnnFormItem" id="divScope" runat="server">
        <dnn:DnnFieldLabel id="scopeFieldLabel" runat="server" Text="Scope.Text" ToolTip="Scope.ToolTip" />
        <asp:RadioButtonList ID="scopeList" runat="server" RepeatDirection="Horizontal" CssClass="dnnVocabRadioButtons">
            <asp:ListItem Value="Application" resourceKey="Application" />
            <asp:ListItem Value="Portal" resourceKey="Portal" />
        </asp:RadioButtonList>
        <asp:Label ID="scopeLabel" runat="server" />
    </div>
</div>