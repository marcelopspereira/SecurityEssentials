﻿using SecurityEssentials.Acceptance.Tests.Web.Pages;
using TechTalk.SpecFlow;

namespace SecurityEssentials.Acceptance.Tests.Web.Extensions
{
	[Binding]
	public class RegisterPageSteps
	{

		[When(@"I submit my registration details")]
		public void WhenISubmitMyRegistrationDetails()
		{
			var registerPage = ScenarioContext.Current.GetPage<RegisterPage>();
			registerPage.ClickSubmit();
		}

        [Given(@"I enter the following registration details:")]
        public void GivenIEnterTheFollowingRegistrationDetails(Table table)
        {
            var registerPage = ScenarioContext.Current.GetPage<RegisterPage>();
            registerPage.EnterDetails(table);
        }


	}
}
