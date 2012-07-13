﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.269
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace DotNetNuke.Website.Specs.UserAccounts
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("User Accounts List")]
    public partial class UserAccountsListFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "UserAccountsList.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "User Accounts List", "In order to manage the Users on my site\r\nAs an Administrator\r\nI want to view a li" +
                    "st of Users", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should be able to edit the user delete the user and edit the Roles for a Regular " +
            "User")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        [NUnit.Framework.CategoryAttribute("MustHaveUser1Added")]
        public virtual void ShouldBeAbleToEditTheUserDeleteTheUserAndEditTheRolesForARegularUser()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should be able to edit the user delete the user and edit the Roles for a Regular " +
                    "User", new string[] {
                        "MustBeDefaultAdminCredentials",
                        "MustHaveUser1Added"});
#line 8
this.ScenarioSetup(scenarioInfo);
#line 9
 testRunner.Given("I am on the site home page");
#line 10
 testRunner.And("I have logged in as the admin");
#line 11
 testRunner.When("I navigate to the admin page User Accounts");
#line 12
 testRunner.Then("The user Testuser DN should have a link to edit the User");
#line 13
 testRunner.And("The user Testuser DN should have a link to delete the User");
#line 14
 testRunner.And("The user Testuser DN should have a link to edit the Security Roles");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should be able to edit the user and edit the Roles for the admin User")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        public virtual void ShouldBeAbleToEditTheUserAndEditTheRolesForTheAdminUser()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should be able to edit the user and edit the Roles for the admin User", new string[] {
                        "MustBeDefaultAdminCredentials"});
#line 17
this.ScenarioSetup(scenarioInfo);
#line 18
 testRunner.Given("I am on the site home page");
#line 19
 testRunner.And("I have logged in as the admin");
#line 20
 testRunner.When("I navigate to the admin page User Accounts");
#line 21
 testRunner.Then("The user Administrator Account should have a link to edit the User");
#line 22
 testRunner.And("The user Administrator Account should have a link to edit the Security Roles");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should not be able to delete the admin User")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        public virtual void ShouldNotBeAbleToDeleteTheAdminUser()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should not be able to delete the admin User", new string[] {
                        "MustBeDefaultAdminCredentials"});
#line 25
this.ScenarioSetup(scenarioInfo);
#line 26
 testRunner.Given("I am on the site home page");
#line 27
 testRunner.And("I have logged in as the admin");
#line 28
 testRunner.When("I navigate to the admin page User Accounts");
#line 29
 testRunner.Then("The user Administrator Account should have a link to edit the User");
#line 30
 testRunner.And("The user Administrator Account should not have a link to delete the User");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should not be able to see Super User in account list")]
        [NUnit.Framework.CategoryAttribute("HostUserMustBeMemberOfPortal")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        public virtual void ShouldNotBeAbleToSeeSuperUserInAccountList()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should not be able to see Super User in account list", new string[] {
                        "HostUserMustBeMemberOfPortal",
                        "MustBeDefaultAdminCredentials"});
#line 34
this.ScenarioSetup(scenarioInfo);
#line 35
 testRunner.Given("I am on the site home page");
#line 36
 testRunner.And("I have logged in as the admin");
#line 37
 testRunner.When("I navigate to the admin page User Accounts");
#line 38
 testRunner.Then("The user SuperUser Account should not display");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should not be able to input invalid value in user name validation")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        public virtual void ShouldNotBeAbleToInputInvalidValueInUserNameValidation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should not be able to input invalid value in user name validation", new string[] {
                        "MustBeDefaultAdminCredentials"});
#line 41
this.ScenarioSetup(scenarioInfo);
#line 42
 testRunner.Given("I am on the site home page");
#line 43
 testRunner.And("I have logged in as the admin");
#line 44
 testRunner.When("I navigate to the admin page Site Settings");
#line 45
 testRunner.And("I input [ in UserName Validation field");
#line 46
 testRunner.And("I Update Site Settings");
#line 47
 testRunner.Then("I should see error message");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Super user\'s photo should be available in all sites")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        public virtual void SuperUserSPhotoShouldBeAvailableInAllSites()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Super user\'s photo should be available in all sites", new string[] {
                        "MustBeDefaultAdminCredentials"});
#line 50
this.ScenarioSetup(scenarioInfo);
#line 51
 testRunner.Given("I am on the site home page");
#line 52
 testRunner.And("I have logged in as the host");
#line 53
 testRunner.And("I am on the Host Page SiteManagement");
#line 54
 testRunner.When("I create a new child portal");
#line 55
 testRunner.And("I click my name to edit profile");
#line 56
 testRunner.And("I click Edit Profile button");
#line 57
 testRunner.And("I upload a picture for photo");
#line 58
 testRunner.And("I click Profile Update button");
#line 59
 testRunner.And("I visit child portal");
#line 60
 testRunner.And("I click my name to edit profile");
#line 61
 testRunner.And("I click Edit Profile button");
#line 62
 testRunner.Then("I should see the picture just upload");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
