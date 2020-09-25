using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;

namespace ProblemTemplateGenerator.Cli
{
	public interface IConfig
	{
		string SrcPath { get; }
		string TestsPath { get; }
		string ReadmePath { get; }
	}

	public class Config : IConfig
	{
		private readonly IConfiguration configuration;

		public string SrcPath => configuration.GetSection(nameof(SrcPath)).Value
			?? throw new NotFoundException($"Config property '{nameof(SrcPath)}' not found.'");
		public string TestsPath => configuration.GetSection(nameof(TestsPath)).Value
			?? throw new NotFoundException($"Config property '{nameof(TestsPath)}' not found.'");
		public string ReadmePath => configuration.GetSection(nameof(ReadmePath)).Value
			?? throw new NotFoundException($"Config property '{nameof(ReadmePath)}' not found.'");
		
		public Config(IConfiguration configuration)
		{
			this.configuration = configuration;
		}
	}
}