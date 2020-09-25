using System;
using System.IO;
using Buildalyzer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ProblemTemplateGenerator.Cli.CodeGenerators;
using ProblemTemplateGenerator.Cli.Helpers;
using ProblemTemplateGenerator.Cli.Io;
using ProblemTemplateGenerator.Cli.Parsers;
using ProblemTemplateGenerator.Cli.WebDrivers;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ProblemTemplateGenerator.Cli
{
	public class ServiceProviderBuilder
	{
		public IServiceProvider Create()
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("generator.config", optional: false, reloadOnChange: true)
				.AddJsonFile("generator_logging.config", optional: true, reloadOnChange: true)
				.Build();
			
			return new ServiceCollection()
				.AddSingleton<IAnalyzerManager, AnalyzerManager>()
				.AddSingleton<ICsFileSaver, CsFileSaver>()
				.AddSingleton<IReadmeUpdater, ReadmeUpdater>()
				.AddSingleton<IDirectoryProvider, DirectoryProvider>()
				.AddSingleton<IInterfaceGenerator, InterfaceGenerator>()
				.AddSingleton<IClassGenerator, ClassGenerator>()
				.AddSingleton<ITestClassGenerator, TestClassGenerator>()
				.AddSingleton<IFileReaderWriter, FileReaderWriter>()
				.AddSingleton<LeetCodeProblemParser>()
				.AddSingleton<IWebDriver, ChromeDriver>()
				.AddTransient<IWebDriverFactory, WebDriverFactory>()
				.AddSingleton<IGenerator>(LeetCodeGeneratorFactory)
				.AddSingleton<IConfiguration>(configuration)
				.AddSingleton<IConfig, Config>()
				.AddSingleton<JsonTextReader>()
				.AddLogging(builder => ConfigureLogging(builder, configuration))
				.BuildServiceProvider();
		}

		private static IGenerator LeetCodeGeneratorFactory(IServiceProvider serviceProvider)
		{
			return new Generator
			(
				new[] 
				{
					serviceProvider.GetService<LeetCodeProblemParser>()
				},
				serviceProvider.GetService<IInterfaceGenerator>(),
				serviceProvider.GetService<IClassGenerator>(),
				serviceProvider.GetService<ITestClassGenerator>(),
				serviceProvider.GetService<ICsFileSaver>(),
				serviceProvider.GetService<IReadmeUpdater>(), 
				serviceProvider.GetService<IDirectoryProvider>(),
				serviceProvider.GetService<IConfig>(),
				serviceProvider.GetService<ILogger<Generator>>()
			);
		}

		private void ConfigureLogging(ILoggingBuilder builder, IConfiguration configurationRoot)
		{
			var logLevelStr = configurationRoot.GetSection("LogLevel").Value
				?? throw new NotFoundException($"Config property LogLevel not found.'");
					
			if (!Enum.TryParse<LogLevel>(logLevelStr, out var logLevel))
				logLevel = LogLevel.Information;
					
			builder.AddConsole().SetMinimumLevel(logLevel);
		}
	}
}
