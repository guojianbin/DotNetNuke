﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.239
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace DotNetNuke.Tests.BuildVerification
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Add Module")]
    public partial class AddModuleFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "AddModule.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Add Module", "In order to add content to my site\r\nAs an administrator\r\nI want to be able to add" +
                    " modules to my site", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Add HTML Module From Ribbon Bar Same As Page Vis")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        [NUnit.Framework.CategoryAttribute("MustUseRibbonBar")]
        [NUnit.Framework.CategoryAttribute("MustHaveAUserWithFullProfile")]
        public virtual void AddHTMLModuleFromRibbonBarSameAsPageVis()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add HTML Module From Ribbon Bar Same As Page Vis", new string[] {
                        "MustBeDefaultAdminCredentials",
                        "MustUseRibbonBar",
                        "MustHaveAUserWithFullProfile"});
#line 9
this.ScenarioSetup(scenarioInfo);
#line 10
 testRunner.Given("I am on the site home page");
#line 11
 testRunner.And("I have logged in as the admin");
#line 12
 testRunner.And("I have created the blank page called Module Page from the Ribbon Bar");
#line 13
 testRunner.And("The page Module Page has View permission set to Grant for the role All Users");
#line 14
 testRunner.And("I am viewing the page called Module Page");
#line 15
 testRunner.When("I create the module View with Same As Page visibility and the content This is a t" +
                    "est");
#line 16
 testRunner.Then("As an admin on the page Module Page I can see the module View and its content Thi" +
                    "s is a test");
#line 17
 testRunner.And("As MichaelWoods with the password password1234 on the page Module Page I can see " +
                    "the module View and its content This is a test");
#line 18
 testRunner.And("As an anonymous user on the page Module Page I can see the module View and its co" +
                    "ntent This is a test");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Add HTML Module From Ribbon Bar Page Editors Only Vis")]
        [NUnit.Framework.CategoryAttribute("MustBeDefaultAdminCredentials")]
        [NUnit.Framework.CategoryAttribute("MustUseRibbonBar")]
        [NUnit.Framework.CategoryAttribute("MustHaveAUserWithFullProfile")]
        public virtual void AddHTMLModuleFromRibbonBarPageEditorsOnlyVis()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add HTML Module From Ribbon Bar Page Editors Only Vis", new string[] {
                        "MustBeDefaultAdminCredentials",
                        "MustUseRibbonBar",
                        "MustHaveAUserWithFullProfile"});
#line 23
this.ScenarioSetup(scenarioInfo);
#line 24
 testRunner.Given("I am on the site home page");
#line 25
 testRunner.And("I have logged in as the admin");
#line 26
 testRunner.And("I have created the blank page called Module Page 2 from the Ribbon Bar");
#line 27
 testRunner.And("The page Module Page 2 has View permission set to Grant for the role All Users");
#line 28
 testRunner.And("The page Module Page 2 has edit permissions set for the user MichaelWoods with di" +
                    "splay name Michael Woods");
#line 29
 testRunner.And("I am viewing the page called Module Page 2");
#line 30
 testRunner.When("I create the module Edit with Page Editors Only visibility and the content This i" +
                    "s for page editors only");
#line 31
 testRunner.Then("As an admin on the page Module Page 2 I can see the module Edit and its content T" +
                    "his is for page editors only");
#line 32
 testRunner.And("As MichaelWoods with the password password1234 on the page Module Page 2 I can se" +
                    "e the module Edit and its content This is for page editors only");
#line 33
 testRunner.And("As an anonymous user on the page Module Page 2 I can not see the module Edit and " +
                    "its content This is for page editors only");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion