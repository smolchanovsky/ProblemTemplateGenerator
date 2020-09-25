using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using ProblemTemplateGenerator.Cli.Models;

namespace ProblemTemplateGenerator.Cli
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var serviceProvider = new ServiceProviderBuilder().Create();
			var logger = serviceProvider.GetService<ILogger<Program>>();
			logger.LogDebug("Service provider initialized.");
			
			var rootCommand = new RootCommand(description: "Problem Template Generator")
			{
				new Option<SourceType>(aliases: new [] {"--source", "-s"}, 
									   description: "Source of a problem (LeetCode)")
				{
					IsRequired = true
				},
				new Option<string>(aliases: new [] {"--name", "-n"}, 
								   description: "Name of a problem from url (https://leetcode.com/problems/two-sum/ => two-sum)")
				{
					IsRequired = true
				}
			};
			rootCommand.Handler = CommandHandler.Create<SourceType, string>(serviceProvider.GetService<IGenerator>().GenerateAsync);
			logger.LogDebug("RootCommand created.");

			logger.LogInformation("Generation problem template starts.");
			await rootCommand.InvokeAsync(args).ConfigureAwait(false);
			logger.LogInformation("Generation problem template ended successfully.");
        }
	}
}
