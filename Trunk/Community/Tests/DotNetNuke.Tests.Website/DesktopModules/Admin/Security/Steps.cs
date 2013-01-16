﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace DotNetNuke.Tests.Website.DesktopModules.Admin.Security
{
    [Binding]
    public class Steps : AutomationBase
    {
        [Given(@"Click the Add New User link\. This opens the Add New User interface")]
        public void GivenClickTheAddNewUserLinkThisOpensTheAddNewUserInterface()
        {
            UI.AddNewUserButton(Driver).Click();
        }

        [When(@"In the User Name text box, enter (.*)")]
        public void WhenInTheUserNameTextBoxEnter(string UserName)
        {
            UI.UserNameTextbox(Driver).SendKeys(UserName);
        }

        [When(@"In the First Name text box, enter (.*)")]
        public void WhenISetFirstNameTo(string FirstName)
        {
            UI.FirstNameTextbox(Driver).SendKeys(FirstName);
        }

        [When(@"In the Last Name text box, enter (.*)")]
        public void WhenISetLastNameTo(string LastName)
        {
            UI.LastNameTextbox(Driver).SendKeys(LastName);
        }

        [When(@"In the Display Name text box, enter (.*)")]
        public void WhenISetDisplayNameTo(string DisplayName)
        {
            UI.DisplayNameTextbox(Driver).SendKeys(DisplayName);
        }

        [When(@"In the Email Address text box, enter (.*)")]
        public void WhenISetEmailTo(string Email)
        {
            UI.EmailTextbox(Driver).SendKeys(Email);
        }

        [When(@"In the Password text box, enter (.*)")]
        public void WhenISetPasswordTo(string Password)
        {
            UI.PasswordTextbox(Driver).SendKeys(Password);
        }

        [When(@"In the Confirm Password text box, re-enter the same password (.*)")]
        public void WhenISetConfirmPasswordTo(string ConfirmPassword)
        {
            UI.ConfirmPasswordTextbox(Driver).SendKeys(ConfirmPassword);
        }

        [When(@"At Authorize, select (.*)")]
        public void WhenISetAuthorizeTo(string Authorize)
        {
            //UI.AuthorizeCheckbox(Driver).SendKeys(Authorize);
        }

        [When(@"At Notify, select (.*)")]
        public void WhenISetNotifyTo(string Notify)
        {
            //UI.NotifyCheckbox(Driver).SendKeys(Notify);
        }

        [When(@"I set Add User Random Password to (.*)")]
        public void WhenISetRandomPasswordTo(string RandomPassword)
        {
            //UI.RandomPasswordCheckbox(Driver).SendKeys(RandomPassword);
        }

        [When(@"Click the Add New User link\.")]
        public void WhenClickTheAddNewUserLink()
        {
            UI.RegisterButton(Driver).Click();
        }

        [Then(@"The newly added user account can now be viewed and modified using the User Accounts module")]
        public void ThenIfTheFormIsValidNewUserIsCreatedTheNewlyAddedUserAccountCanNowBeViewedAndModifiedUsingTheUserAccountsModule()
        {
        }

        [Then(@"If Authorize is checked the new user will automatically gain access to the Registered User role and any roles set for Auto Assignment")]
        public void ThenIfAuthorizeIsCheckedTheNewUserWillAutomaticallyGainAccessToTheRegisteredUserRoleAndAnyRolesSetForAutoAssignment()
        {
        }

        [Then(@"If Authorize is unchecked the new user will be created but will not be able to access the restricted areas of the site")]
        public void ThenIfAuthorizeIsUncheckedTheNewUserWillBeCreatedButWillNotBeAbleToAccessTheRestrictedAreasOfTheSite()
        {
        }

        [Then(@"If Notify is checked the new user will be sent a notification email")]
        public void ThenIfNotifyIsCheckedANotificationEmailWillBeSentToTheNewUser()
        {
        }

        [Then(@"If Notify is unchecked the new user will not be sent a notification email")]
        public void ThenIfNotifyIsUncheckedTheNewUserWillNotBeSentANotificationEmail()
        {
        }
    }
}
