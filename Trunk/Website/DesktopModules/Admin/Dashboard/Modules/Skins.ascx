﻿<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Dashboard.Controls.Skins" CodeFile="Skins.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<dnn:Label id="plSkins" runat="Server" CssClass="Head" ControlName="grdSkins" />
<asp:DataGrid ID="grdSkins" runat="server" GridLines="None" 
    AutoGenerateColumns="false" EnableViewState="False">
    <Columns>
        <asp:BoundColumn DataField="SkinName" HeaderText="Skin"  ItemStyle-Width="250px"/>
        <asp:BoundColumn DataField="InUse" HeaderText="InUse" ItemStyle-Width="100px"/>
    </Columns>
</asp:DataGrid>
