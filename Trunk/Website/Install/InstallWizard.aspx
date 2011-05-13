<%@ Page Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Services.Install.InstallWizard" CodeFile="InstallWizard.aspx.cs" %>
<%@ Import Namespace="DotNetNuke.UI.Utilities" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="WizardUser" Src="~/Install/WizardUser.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="../Portals/_default/default.css" />
    <link rel="stylesheet" type="text/css" href="Install.css" />
</head>
<body>
    <script type="text/javascript">
        var BARS = 20;  // The number of bars to display
        var CLASS = "Progress"; // Style class name when bar is off
        var CLASS_ON_SUFFIX = "On";  // Suffix to append to style class name when bar is on
        var SPEED = 200; // Delay in milliseconds for animation

        var ELEMENT_ID_PREFIX = "Progress_"; // ID prefix for progress bar elements
        var current = 0; // Last rendered progress bar element
        var pageNo = <%=ClientAPI.GetSafeJSString(wizInstall.ActiveStepIndex.ToString())%>;
        var initialAction = '<%=GetBaseDatabaseVersion()%>';
        var nextButtonID = 'wizInstall_StepNavigationTemplateContainerID_StepNextButton';
        var prevButtonID = 'wizInstall_StepNavigationTemplateContainerID_StepPreviousButton';
        var timeOut;

         //Localization Vars
        var m_sLocaleComplete = '<%=ClientAPI.GetSafeJSString(LocalizeString("Complete"))%>';
        var m_sLocaleSuccess = '<%=ClientAPI.GetSafeJSString(LocalizeString("Success"))%>';
        var m_sLocaleFailure = '<%=ClientAPI.GetSafeJSString(LocalizeString("Failure"))%>';
        var m_sLocaleInstalling = '<%=ClientAPI.GetSafeJSString(LocalizeString("Installing"))%>';
        var m_sLocaleInstallComplete = '<%=ClientAPI.GetSafeJSString(LocalizeString("InstallComplete"))%>';
        var m_sLocaleInstallDatabase = '<%=ClientAPI.GetSafeJSString(LocalizeString("InstallDatabase"))%>';
        var m_sLocaleInstallScript = '<%=ClientAPI.GetSafeJSString(LocalizeString("InstallScript"))%>';
        var m_sLocaleInstallFailed = '<%=ClientAPI.GetSafeJSString(LocalizeString("InstallFailed"))%>';
        
        function animate()
        {
           var progressObj = document.getElementById(ELEMENT_ID_PREFIX + current);
           progressObj.className = CLASS + (progressObj.className == CLASS ? CLASS_ON_SUFFIX : "");

           current++;
           if (current == BARS)
               current = 0;      

           timeOut = window.setTimeout("animate()", SPEED);
        }

        function createAnimation()
        {
           // Create the progress bar
           var progress = document.getElementById("Progress");
           for(var p=0;p<BARS;p++)
           {
               var pDiv = document.createElement("div");
               pDiv.innerHTML = "&nbsp;&nbsp;";
               pDiv.className = CLASS;
               pDiv.id = "Progress_" + p;
               progress.appendChild(pDiv);
           }

           // Start the animation
           animate();
        }
        
        function stopAnimation()
        {
            window.clearTimeout(timeOut);                 
        }
        
        function installScripts()
        {
            if (pageNo == 3)
            {
                createAnimation();
                doCallbackAction(initialAction);
            }
        }

        function doCallbackAction(sAction)
        {
            var sFeedback;
            if (sAction == initialAction)
            {
                sFeedback = m_sLocaleInstallDatabase.replace('{0}', sAction);
            }
            else 
            {
                if (sAction.indexOf(".") > 0)
                {
                    sFeedback = m_sLocaleInstallScript.replace('{0}', sAction);
                }
            }
            reportAction(sFeedback + '...');
            var sCB = dnn.getVar('ActionCallback');
            eval(sCB.replace('[ACTIONTOKEN]', "'" + sAction + "'"));
        }
        
        function checkDisabled(button)
        {
            //return false;
            if (button.className == "dnnPrimaryAction dnnDisabledAction")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        function reportAction(s)
        {
            $get('txtFeedback').value += s;
        }

        function successFunc(result, ctx)
        {
	        if (result != 'Done')
            {
                if (result.indexOf("ERROR") < 0)
                {
                    reportAction(m_sLocaleSuccess + '\n');
                    doCallbackAction(result);
                }
                else
                    errorFunc(result, ctx);
            }
            else
            {
                reportAction(m_sLocaleSuccess + '\n');
	            reportAction(m_sLocaleComplete + '\n');
                var progress = document.getElementById("Progress");
                progress.innerHTML = m_sLocaleInstallComplete;
                stopAnimation();
                var nextButton = document.getElementById(nextButtonID);
                nextButton.disabled = false;
                nextButton.className = "dnnPrimaryAction";
	        }
        }

        function errorFunc(result, ctx)
        {
	        reportAction(m_sLocaleFailure + ' ' + result + '\n');
            var progress = document.getElementById("Progress");
            progress.innerHTML = m_sLocaleInstallFailed;
            stopAnimation();
        }
    </script>

    <form id="form1" runat="server">
        <div class="dnnForm dnnInstall dnnClear" id="dnnInstall">
            <asp:Wizard ID="wizInstall" runat="server" 
                ActiveStepIndex="0"
                Font-Names="Verdana" CellPadding="5" CellSpacing="5"
                FinishCompleteButtonType="Link"
                FinishPreviousButtonType="Link" 
                StartNextButtonType="Link" 
                StepNextButtonType="Link"
                StepPreviousButtonType="Link" 
                DisplaySideBar="false">
                <StepStyle VerticalAlign="Top" />
                <StartNavigationTemplate>
                    <ul class="dnnActions dnnClear">
    	                <li><asp:LinkButton id="StartNextButton" runat="server" CssClass="dnnPrimaryAction" CommandName="MoveNext" resourcekey="Next" /></li>
                    </ul>
                </StartNavigationTemplate>
                <StepNavigationTemplate>
                    <ul class="dnnActions dnnClear">
    	                <li><asp:LinkButton id="StepNextButton" runat="server" CssClass="dnnPrimaryAction" CommandName="MoveNext" resourcekey="Next" /></li>
                        <li><asp:LinkButton id="StepPreviousButton" runat="server" CssClass="dnnSecondaryAction" CommandName="MovePrevious" resourcekey="Previous" /></li>
                        <li><asp:LinkButton id="CustomButton" runat="server" CssClass="dnnSecondaryAction" Visible="false" /></li>
                    </ul>
                </StepNavigationTemplate>
                <HeaderTemplate>
                    <img src="../images/branding/logo.gif" border="0" alt="DotNetNuke">
                </HeaderTemplate>
                <WizardSteps>
                    <asp:WizardStep ID="Step0" runat="Server" Title="Welcome">
                        <h2><asp:Label ID="lblStep0Title" runat="server" /></h2>
                        <asp:Label ID="lblStep0Detail" runat="Server" />
                        <hr />
                        <div id="languagePanel" runat="server" class="dnnForm">
                            <div class="dnnFormItem">
                                <dnn:Label ID="Label2" runat="server" resourceKey="ChooseInstall" ControlName="rblInstall" />
                                <asp:RadioButtonList ID="rblInstall" runat="Server" RepeatDirection="Vertical" CssClass="installRadioButtons" />
                            </div>                   
                            <div class="dnnFormItem">
                                <dnn:Label ID="Label3" runat="server" resourceKey="ChooseLanguage" ControlName="cboLanguages" />
                                <asp:DropDownList ID="cboLanguages" AutoPostBack="true" runat="Server" DataTextField="Text" DataValueField="Code" />
                            </div>
                        </div>                   
                        <asp:Label ID="lblDataBaseWarning" runat="server" CssClass="NormalRed" ResourceKey="DatabaseWarning" />
                        <asp:Label ID="lblHostWarning" runat="server" CssClass="NormalRed" ResourceKey="HostWarning" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Step1" runat="server" Title="FilePermissions">
                        <h2><asp:Label ID="lblStep1Title" runat="server" /></h2>
                        <asp:Label ID="lblStep1Detail" runat="Server" />
                        <hr />
                        <div class="dnnForm">
                            <div class="dnnFormItem">
                                <dnn:Label ID="Label1" runat="server" resourceKey="Permissions" ControlName="lstPermissions" />
                            </div>
                            <div class="dnnFormItem">
                                <asp:CheckBoxList ID="lstPermissions" runat="server" CssClass="permissionsCheckBoxes" DataTextField="Name" DataValueField="Permission" TextAlign="Left" />
                            </div>
                        </div>
                        <asp:Label ID="lblPermissionsError" runat="server" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Step2" runat="server" Title="ConnectionString" AllowReturn="false">
                        <h2><asp:Label ID="lblStep2Title" runat="server" /></h2>
                        <asp:Label ID="lblStep2Detail" runat="Server" />
                        <hr />
                        <div class="dnnForm">
                            <div class="dnnFormItem">
                                <dnn:Label ID="Label4" runat="server" resourceKey="ChooseDatabase" ControlName="rblDatabases" />
                                <asp:RadioButtonList ID="rblDatabases" runat="Server" CssClass="databaseRadioButtons" 
                                        AutoPostBack="true" RepeatDirection="Vertical" RepeatColumns="1" />
                            </div>
                        </div>
                        <div id="databasePanel" runat="Server" visible="False" class="dnnForm">
                            <div class="dnnFormItem">
                                <dnn:Label ID="serverLabel" runat="server" ControlName="txtServer" ResourceKey="Server" HelpKey="ServerHelp" />
                                <asp:TextBox ID="txtServer" runat="Server" />
                            </div>
                            <div id="fileRow" runat="server" class="dnnFormItem">
                                <dnn:Label ID="fileLabel" runat="server" ControlName="txtFile" ResourceKey="DatabaseFile" HelpKey="DatabaseFileHelp" />
                                <asp:TextBox ID="txtFile" runat="Server" />
                            </div>
                            <div id="databaseRow" runat="server" class="dnnFormItem">
                                <dnn:Label ID="databaseLabel" runat="server" ControlName="txtDatabase" ResourceKey="Database" HelpKey="DatabaseHelp" />
                                <asp:TextBox ID="txtDatabase" runat="Server" />
                            </div>
                            <div id="integratedRow" runat="server" class="dnnFormItem">
                                <dnn:Label ID="integratedLabel" runat="server" ControlName="chkIntegrated" ResourceKey="Integrated" HelpKey="IntegratedHelp" />
                                <asp:CheckBox ID="chkIntegrated" runat="Server" AutoPostBack="True" />
                            </div>
                            <div id="userRow" runat="server" class="dnnFormItem">
                                <dnn:Label ID="userIdLabel" runat="server" ControlName="txtUserId" ResourceKey="UserId" HelpKey="UserHelp" />
                                <asp:TextBox ID="txtUserId" runat="Server" />
                            </div>
                            <div id="passwordRow" runat="server" class="dnnFormItem">
                                <dnn:Label ID="passwordLabel" runat="server" ControlName="txtPassword" ResourceKey="Password" HelpKey="PasswordHelp" />
                                <asp:TextBox ID="txtPassword" runat="Server" />
                            </div>
                            <div class="dnnFormItem">
                                <dnn:Label ID="ownerLabel" runat="server" ControlName="chkOwner" ResourceKey="Owner" HelpKey="OwnerHelp" />
                                <asp:CheckBox ID="chkOwner" runat="Server" />
                            </div>
                            <div class="dnnFormItem">
                                <dnn:Label ID="qualifierLabel" runat="server" ControlName="txtqualifier" ResourceKey="Qualifier" HelpKey="QualifierHelp" />
                                <asp:TextBox ID="txtqualifier" runat="Server" />
                            </div>
                        </div>
                        <br />
                        <asp:Label ID="lblDataBaseError" runat="server" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Step3" runat="server" Title="Database" AllowReturn="false">
                        <h2><asp:Label ID="lblStep3Title" runat="server" /></h2>
                        <asp:Label ID="lblStep3Detail" runat="Server" />
                        <br />
                        <div id="Progress"><asp:Label ID="Label5" runat="server" resourceKey="Installing" /></div>
                        <br />
                        <textarea id="txtFeedback" class="FeedBack" cols="80" rows="15"></textarea>
                        <asp:Label ID="lblInstallError" runat="server" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Step4" runat="server" Title="HostSettings" AllowReturn="false">
                        <h2><asp:Label ID="lblStep4Title" runat="server" /></h2>
                        <asp:Label ID="lblStep4Detail" runat="Server" />
                        <hr />
                        <dnn:WizardUser ID="usrHost" runat="server" />
                        <hr />
                        <asp:Panel ID="SMTPSettingsPanel" runat="server" Visible="false">
                            <h3><asp:Label ID="lblSMTPSettings" runat="server" /></h3>
                            <asp:Label ID="lblSMTPSettingsHelp" runat="Server" />
                            <div class="dnnForm">
                                <div class="dnnFormItem">
                                    <dnn:Label ID="Label6" runat="server" resourceKey="SMTPServer" ControlName="txtSMTPServer" />
                                    <asp:TextBox ID="txtSMTPServer" runat="server" MaxLength="256"  />
                                </div>                   
                                <div class="dnnFormItem">
                                    <dnn:Label ID="Label7" runat="server" resourceKey="SMTPAuthentication" ControlName="optSMTPAuthentication" />
                                    <asp:RadioButtonList ID="optSMTPAuthentication" runat="server" RepeatDirection="Horizontal">
                                        <asp:ListItem Value="0" resourcekey="SMTPAnonymous" Selected="True" />
                                        <asp:ListItem Value="1" resourcekey="SMTPBasic" />
                                        <asp:ListItem Value="2" resourcekey="SMTPNTLM" />
                                    </asp:RadioButtonList>
                                </div>
                                <div class="dnnFormItem">
                                    <dnn:Label ID="Label8" runat="server" resourceKey="SMTPEnableSSL" ControlName="chkSMTPEnableSSL" />
                                    <asp:Checkbox ID="chkSMTPEnableSSL" runat="server" />
                                </div>                           
                                <div id="SMTPUserNameRow" class="dnnFormItem">
                                    <dnn:Label ID="Label9" runat="server" resourceKey="SMTPUsername" ControlName="txtSMTPUsername" />
                                    <asp:TextBox ID="txtSMTPUsername" runat="server" MaxLength="256"  />
                                </div>                   
                                <div id="SMTPPasswordRow" class="dnnFormItem">
                                    <dnn:Label ID="Label10" runat="server" resourceKey="SMTPPassword" ControlName="txtSMTPPassword" />
                                    <asp:TextBox ID="txtSMTPPassword" runat="server" MaxLength="256" TextMode="Password"  />
                                </div>                   
                            </div>                   
                        </asp:Panel>
                        <asp:Label ID="lblHostUserError" runat="server" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Step5" runat="server" Title="Modules" AllowReturn="false">
                        <h2><asp:Label ID="lblStep5Title" runat="server" /></h2>
                        <asp:Label ID="lblStep5Detail" runat="Server" />
                        <hr />
                        <h3><asp:Label ID="lblModules" runat="server" /></h3>
                        <asp:CheckBoxList ID="lstModules" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
                        <asp:Label ID="lblNoModules" runat="server" />
                        <hr />
                        <asp:Label ID="lblModulesError" runat="server" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Step6" runat="server" Title="Skins" AllowReturn="false">
                        <h2><asp:Label ID="lblStep6Title" runat="server" /></h2>
                        <asp:Label ID="lblStep6Detail" runat="Server" />
                        <hr />
                        <h3><asp:Label ID="lblSkins" runat="server" /></h3>
                        <asp:CheckBoxList ID="lstSkins" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
                        <asp:Label ID="lblNoSkins" runat="server" />
                        <br />
                        <h3><asp:Label ID="lblContainers" runat="server" /></h3>
                        <asp:CheckBoxList ID="lstContainers" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
                        <asp:Label ID="lblNoContainers" runat="server" />
                        <hr />
                        <asp:Label ID="lblSkinsError" runat="server" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Step7" runat="server" Title="Languages" AllowReturn="false">
                        <h2><asp:Label ID="lblStep7Title" runat="server" /></h2>
                        <asp:Label ID="lblStep7Detail" runat="Server" />
                        <hr />
                        <h3><asp:Label ID="lblLanguages" runat="server" /></h3>
                        <asp:CheckBoxList ID="lstLanguages" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
                        <asp:Label ID="lblNoLanguages" runat="server" />
                        <hr />
                        <asp:Label ID="lblLanguagesError" runat="server" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Step8" runat="server" Title="AuthSystems" AllowReturn="false">
                        <h2><asp:Label ID="lblStep8Title" runat="server" /></h2>
                        <asp:Label ID="lblStep8Detail" runat="Server" />
                        <hr />
                        <h3><asp:Label ID="lblAuthSystems" runat="server" /></h3>
                        <asp:CheckBoxList ID="lstAuthSystems" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
                        <asp:Label ID="lblNoAuthSystems" runat="server" />
                        <hr />
                        <asp:Label ID="lblAuthSystemsError" runat="server" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Step9" runat="server" Title="Providers" AllowReturn="false">
                        <h2><asp:Label ID="lblStep9Title" runat="server" /></h2>
                        <asp:Label ID="lblStep9Detail" runat="Server" />
                        <hr />
                        <h3><asp:Label ID="lblProviders" runat="server" /></h3>
                        <asp:CheckBoxList ID="lstProviders" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
                        <asp:Label ID="lblNoProviders" runat="server" />
                        <hr />
                        <asp:Label ID="lblProvidersError" runat="server" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Step10" runat="server" Title="Portal" AllowReturn="false">
                        <h2><asp:Label ID="lblStep10Title" runat="server" /></h2>
                        <asp:Label ID="lblStep10Detail" runat="Server" />
                        <hr />
                        <h3><asp:Label ID="lblAdminUser" runat="server" /></h3>
                        <dnn:WizardUser ID="usrAdmin" runat="server" />
                        <hr />
                        <h3><asp:Label ID="lblPortal" runat="server" /></h3>
                        <div class="dnnForm">
                            <div class="dnnFormItem">
                                <dnn:Label ID="Label11" runat="server" resourceKey="PortalTitle" ControlName="txtPortalTitle" />
                                <asp:TextBox ID="txtPortalTitle" runat="server" MaxLength="128"  />
                            </div>
                            <div class="dnnFormItem">
                                <dnn:Label ID="Label12" runat="server" resourceKey="PortalTemplate" ControlName="cboPortalTemplate" />
                                <asp:DropDownList ID="cboPortalTemplate" runat="server"  />
                            </div>
                        </div>                   
                        <asp:Label ID="lblPortalError" runat="server" />
                    </asp:WizardStep>
                    <asp:WizardStep ID="Complete" runat="server" StepType="Finish" Title="Installation Complete">
                        <h2><asp:Label ID="lblCompleteTitle" runat="server" /></h2>
                        <asp:Label ID="lblCompleteDetail" runat="server" />
                    </asp:WizardStep>
                </WizardSteps>
            </asp:Wizard>
        </div>
        <input id="ScrollTop" runat="server" name="ScrollTop" type="hidden" />
        <input type="hidden" id="__dnnVariable" runat="server" />
        <asp:Label ID="txtErrorMessage" runat="server" />
    </form>
    <script src="../Resources/Shared/scripts/jquery/jquery.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        function toggleSmtpCredentials() {
            var smtpVal = $('#<%= optSMTPAuthentication.ClientID %> input:checked').val(); //0,1,2
            if (smtpVal == "1") {
                $('#SMTPUserNameRow,#SMTPPasswordRow').slideDown();
            }
            else {
                $('#SMTPUserNameRow,#SMTPPasswordRow').slideUp();
            }
        }

        function setUp() {
            installScripts();
            toggleSmtpCredentials();
            $('#<%= optSMTPAuthentication.ClientID %>').click(function () {
                toggleSmtpCredentials();
            });
        }

        $(document).ready(function () {
            setUp();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setUp();
            });
        });
    </script>
</body>
</html>
