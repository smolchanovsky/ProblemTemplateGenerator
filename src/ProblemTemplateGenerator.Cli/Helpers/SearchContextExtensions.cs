using System;
using OpenQA.Selenium;

namespace ProblemTemplateGenerator.Cli.Helpers
{
	public static class SearchContextExtensions
	{
		public static IWebElement? FindElementOrDefault(this ISearchContext searchContext, By by)
		{
			if (searchContext == null)
				throw new ArgumentNullException(nameof(searchContext));

			try
			{
				return searchContext.FindElement(by);
			}
			catch (NoSuchElementException)
			{
				return null;
			}
		}
	}
}
