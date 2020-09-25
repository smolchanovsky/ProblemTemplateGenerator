using System;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;

namespace ProblemTemplateGenerator.Cli.WebDrivers
{
	public interface IWebDriverFactory
	{
		IWebDriver Create();
	}

	public class WebDriverFactory : IWebDriverFactory
	{
		private readonly IServiceProvider serviceProvider;

		public WebDriverFactory(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}
		
		public IWebDriver Create()
		{
			return serviceProvider.GetService<IWebDriver>();
		}
	}
}
