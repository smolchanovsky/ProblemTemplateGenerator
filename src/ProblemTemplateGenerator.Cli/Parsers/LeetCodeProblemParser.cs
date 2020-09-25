using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ProblemTemplateGenerator.Cli.Abstractions;
using ProblemTemplateGenerator.Cli.Helpers;
using ProblemTemplateGenerator.Cli.Models;
using ProblemTemplateGenerator.Cli.WebDrivers;

namespace ProblemTemplateGenerator.Cli.Parsers
{
	public class LeetCodeProblemParser : IProblemParser
	{		
		private readonly Regex titleParagraphParser = new Regex(@"(\d+).\s(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(10));
		private readonly Regex statementTrimmer = new Regex(@"[ \t]+(\r?$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(10));
		private readonly Uri baseUri = new Uri("https://leetcode.com/problems/");
		
		private readonly IWebDriverFactory webDriverFactory;
		private readonly ILogger<LeetCodeProblemParser> logger;

		public SourceType SourceType { get; } = SourceType.LeetCode;
		
		public LeetCodeProblemParser(IWebDriverFactory webDriverFactory, ILogger<LeetCodeProblemParser> logger)
		{
			this.webDriverFactory = webDriverFactory;
			this.logger = logger;
		}
		
		public Task<Problem> ParseAsync(string problemName)
		{
			using var webDriver = webDriverFactory.Create();
			webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
			
			var problemUri = new Uri(baseUri, problemName);
			webDriver.Navigate().GoToUrl(problemUri); 
			SelectCSharpCode(webDriver);
			logger.LogDebug($"Page {problemUri} configured.");

			var (number, title) = ParseHead(webDriver);
			logger.LogDebug($"Problem head parsed.");
			var difficult = ParseDifficult(webDriver);
			logger.LogDebug($"Problem difficult parsed.");
			var statement = ParseStatement(webDriver);
			logger.LogDebug($"Problem statement parsed.");
			var code = ParseCode(webDriver);
			logger.LogDebug($"Problem code parsed.");

			return Task.FromResult(new Problem(SourceType.LeetCode, problemUri.ToString(), number, title, statement, difficult, code));
		}

		private static void SelectCSharpCode(ISearchContext searchContext)
		{
			searchContext
				.FindElement(By.XPath("//*[contains(@data-cy,'lang-select')]"))
				.Click();

			searchContext
				.FindElement(By.XPath("//*[contains(@data-cy,'lang-select-C#')]"))
				.Click();
		}

		private (int Number, string Title) ParseHead(ISearchContext searchContext)
		{
			var titleParagraph = searchContext
				.FindElementOrDefault(By.XPath("//*[contains(@data-cy,'question-title')]"))
				?? throw new NotFoundException("Title not found");

			var match = titleParagraphParser.Match(titleParagraph.Text);
			var numberStr = match.Groups[1].Value;
			var titleStr = match.Groups[2].Value;

			return (int.Parse(numberStr), titleStr);
		}

		private static DifficultType ParseDifficult(ISearchContext searchContext)
		{
			var difficult = searchContext
				.FindElementOrDefault(By.XPath("//*[@diff]"))
				?? throw new NotFoundException("Difficult not found");

			return Enum.Parse<DifficultType>(difficult.Text);
		}

		private string ParseStatement(ISearchContext searchContext)
		{
			var statementParagraphs = searchContext
				.FindElementOrDefault(By.XPath("//*[contains(@class,'question-content')]"))?
				.FindElementOrDefault(By.TagName("div"))?
				.FindElements(By.TagName("p"))
				.Select(x => x.Text)
				.TakeWhile(x => x != "Example 1:")
				.ToArray()
				?? throw new NotFoundException("Statement not found");

			if (statementParagraphs.Length == 0)
				throw new NotFoundException("Statement not found");

			return statementTrimmer.Replace(string.Join(" ", statementParagraphs), "$1");
		}

		private static string ParseCode(ISearchContext searchContext)
		{
			var codeLines = searchContext
				.FindElementOrDefault(By.XPath("//*[contains(@class,'CodeMirror-code')]"))?
				.FindElements(By.XPath("//*[contains(@class,' CodeMirror-line ')]"))
				.Select(x => x.Text)
				.ToArray()
				?? throw new NotFoundException("Code lines not found");

			if (codeLines.Length == 0)
				throw new NotFoundException("Code lines not found");

			return string.Join("\n", codeLines);
		}
	}
}
