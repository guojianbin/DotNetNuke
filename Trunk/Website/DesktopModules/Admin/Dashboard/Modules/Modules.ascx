﻿<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Dashboard.Controls.Modules" CodeFile="Modules.ascx.cs" %>

<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<dnn:Label id="plModules" runat="Server" CssClass="Head" ControlName="grdModules" />
<asp:DataGrid ID="grdModules" runat="server" GridLines="None" 
    AutoGenerateColumns="false" EnableViewState="False">
    <Columns>
        <asp:BoundColumn DataField="FriendlyName" HeaderText="Module" ItemStyle-Width="250px"/>
        <asp:BoundColumn DataField="Version" HeaderText="Version" ItemStyle-Width="100px"/>
        <asp:BoundColumn DataField="Instances" HeaderText="Instances" ItemStyle-Width="100px"/>
    </Columns>
</asp:DataGrid>