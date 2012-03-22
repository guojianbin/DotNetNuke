<%@ Control Language="C#" Inherits="DotNetNuke.Providers.FiftyOneClientCapabilityProvider.Administration, DotNetNuke.Providers.FiftyOneClientCapabilityProvider" AutoEventWireup="true" CodeBehind="Administration.ascx.cs" %>
<%@ Register Assembly="FiftyOne.Foundation" Namespace="FiftyOne.Foundation.UI.Web" TagPrefix="fiftyOne" %>
<%@ Register Assembly="DotNetNuke.Providers.FiftyOneClientCapabilityProvider" Namespace="DotNetNuke.Providers.FiftyOneClientCapabilityProvider" tagPrefix="dnn" %>
<%@ Register TagPrefix="dnn" TagName="jQuery" Src="~/Admin/Skins/jQuery.ascx" %>

<div class="DnnModule DnnModule-Device-Detection" id="fiftyOneDegrees">

    <div class="dnnForm ui-tabs ui-widget ui-widget-content ui-corner-all">    
        <div class="header"></div>
        <div class="content">
            <div class="deviceMsg">
                <% if (!this.IsPremium) { %>
                    <h2><%= LocalizeString("Lite.Header")%></h2>
                    <fieldset class="upgradePremium">
                        <h5><%= LocalizeString("LiteUpgrade.Label")%></h5>
                        <fiftyOne:Activate runat="server" ID="Activate" LogoEnabled="False" ErrorCssClass="dnnFormMessage dnnFormValidationSummary" SuccessCssClass="dnnFormMessage dnnFormSuccess" FooterEnabled="False" />
                        <fiftyOne:Stats runat="server" ID="LiteStats" />
                        <p><em><%= LocalizeString("PurchaseInfo.Text")%><a href="<%=LocalizeString("PurchaseInfoLink.Href") %>" target="_blank"><%= LocalizeString("PurchaseInfoLink.Text")%></a><%= LocalizeString("PurchaseInfoEnd.Text")%></em></p>
                    </fieldset>
                    
                    <fieldset class="upgradeInfo">
                        <h5><%= LocalizeString("WhyUpgrade.Text")%></h5>
                        <ul class="upgradePoints">
                            <li><%= LocalizeString("UpgradePoint1.Text")%></li>
                            <li><%= LocalizeString("UpgradePoint2.Text")%></li>
                            <li><%= LocalizeString("UpgradePoint3.Text")%></li>
                            <li><%= LocalizeString("UpgradePoint4.Text")%></li>
                        </ul>
                    </fieldset>
                <% } else { %>
                    <h2><%= LocalizeString("Premium.Header") %></h2>
                    <fieldset>
                        <p><%= LocalizeString("PremiumIntro.Text") %></p>
                    </fieldset>
                <% } %>
            </div>
            <% if (this.IsPremium) { %>
            <div class="deviceColGroup">
                <div class="deviceCol-left">
                    <h4><%= LocalizeString("SearchPremium.Header") %></h4>
                    <p><%= LocalizeString("SearchPremium.Text")%></p>
                    <p id="NoResultsMessage" runat="server" class="dnnFormMessage dnnFormWarning"><%=LocalizeString("NoResultsFound.Text")%></p>
                    <p><asp:TextBox runat="server" ID="SearchTextBox" MaxLength="800" ValidationGroup="DeviceSearch" /></p>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="SearchTextBox" ValidationGroup="DeviceSearch" ResourceKey="DeviceSearchRequired" CssClass="dnnFormMessage dnnFormError"></asp:RequiredFieldValidator>
                    <p><asp:LinkButton runat="server" ID="SearchButton" ResourceKey="SearchButton" CssClass="dnnPrimaryAction" CausesValidation="True" ValidationGroup="DeviceSearch" /></p>
                </div>
                <div class="deviceCol-right">
                    <div class="glossary" style="display:none;">
                        <div class="contents-header">
                            <h4><%= LocalizeString("Introduction.Header") %></h4>
                            <p><%= LocalizeString("Introduction.Text") %></p>
                            <div class="contents-back">
                                <a class="back" href="#"><%= LocalizeString("BackToExplorer.Text") %></a>
                            </div>
                        </div>
                        <div class="body">
                            <h2><%= LocalizeString("Glossary.Header") %></h2>
                            <p class="help-key"><%= LocalizeString("Glossary.Text") %></p>
                        </div>
                        <asp:Repeater runat="server" ID="HardwareList">
                            <HeaderTemplate>
                                    <table>
                                    <thead>
                                        <tr>
                                            <td colspan="2"><%= LocalizeString("HardwareList.Header") %></td>
                                        </tr>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                        <tr>
                                            <td>
                                                <%# DataBinder.Eval(Container.DataItem, "Name") %>
                                                <span class="premium" id="Premium" runat="server">*</span>
                                            </td>
                                            <td>
                                                <p><%# DataBinder.Eval(Container.DataItem, "Description") %></p>
                                                <p class="values" id="Values" runat="server"></p>
                                            </td>
                                        </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                    </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:Repeater runat="server" ID="SoftwareList">
                            <HeaderTemplate>
                                    <table>
                                    <thead>
                                        <tr>
                                            <td colspan="2"><%= LocalizeString("SoftwareList.Header") %></td>
                                        </tr>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                        <tr>
                                            <td><%# DataBinder.Eval(Container.DataItem, "Name") %></td>
                                            <td><%# DataBinder.Eval(Container.DataItem, "Description") %></td>
                                        </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                    </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:Repeater runat="server" ID="BrowserList">
                            <HeaderTemplate>
                                    <table>
                                    <thead>
                                        <tr>
                                            <td colspan="2"><%= LocalizeString("BrowserList.Header") %></td>
                                        </tr>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                        <tr>
                                            <td><%# DataBinder.Eval(Container.DataItem, "Name") %></td>
                                            <td><%# DataBinder.Eval(Container.DataItem, "Description") %></td>
                                        </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                    </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:Repeater runat="server" ID="ContentList">
                            <HeaderTemplate>
                                    <table>
                                    <thead>
                                        <tr>
                                            <td colspan="2"><%= LocalizeString("ContentList.Header") %></td>
                                        </tr>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                        <tr>
                                            <td><%# DataBinder.Eval(Container.DataItem, "Name") %></td>
                                            <td><%# DataBinder.Eval(Container.DataItem, "Description") %></td>
                                        </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                    </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                    <div class="explorer">
                        <div class="contents-header">
                        <% if (this.IsPremium) { %>
                            <h4><%=LocalizeString("DeviceExplorer.Header") %></h4>
                            <p><%=LocalizeString("LiteSearchResults.Text") %></p>
                        <% } else {%>
                            <h4><%=LocalizeString("LiteSearchResults.Header") %></h4>
                            <p><%=LocalizeString("LiteSearchResults.Text") %></p>
                        <% } %>
                        </div>
                        <a href="#" class="contents-help"><strong><%=LocalizeString("Help.Text") %></strong></a>
                        <dnn:DnnDeviceExplorer runat="server" ID="DeviceExplorer" FooterEnabled="False" LogoEnabled="False" BackButtonCssClass="dnnTertiaryAction"/>
                    </div>
                </div>
            </div>
            <fiftyOne:Stats runat="server" ID="FooterStats" />
            <% } %>
        </div>
    </div>
    
</div>

<dnn:jQuery runat="server" DnnjQueryPlugins="True" />

<script type="text/javascript">
    $('.dnnTooltip').dnnTooltip();
    
    $('.contents-help').click(function (e) {
        e.preventDefault();
        $('.explorer').slideToggle(function () {
            $('.glossary').slideToggle();    
        }); 
    });

    $('.contents-back').click(function (e) {
        e.preventDefault();
        $('.glossary').slideToggle(function () {
            $('.explorer').slideToggle();
        });
    });
    

</script>