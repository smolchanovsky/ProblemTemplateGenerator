using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProblemTemplateGenerator.Cli.Abstractions;
using ProblemTemplateGenerator.Cli.CodeGenerators;
using ProblemTemplateGenerator.Cli.Helpers;
using ProblemTemplateGenerator.Cli.Io;
using ProblemTemplateGenerator.Cli.Models;

namespace ProblemTemplateGenerator.Cli
{
	public interface IGenerator
	{
		Task GenerateAsync(SourceType source, string name);
	}
	
	public class Generator : IGenerator
	{
		private readonly IReadOnlyCollection<IProblemParser> problemParsers;
		private readonly IInterfaceGenerator interfaceGenerator;
		private readonly IClassGenerator classGenerator;
		private readonly ITestClassGenerator testClassGenerator;
		private readonly ICsFileSaver csFileSaver;
		private readonly IReadmeUpdater readmeUpdater;
		private readonly IDirectoryProvider directoryProvider;
		private readonly IConfig config;
		private readonly ILogger<Generator> logger;

		public Generator(
			IEnumerable<IProblemParser> problemParsers,
			IInterfaceGenerator interfaceGenerator, 
			IClassGenerator classGenerator, 
			ITestClassGenerator testClassGenerator,
			ICsFileSaver csFileSaver,
			IReadmeUpdater readmeUpdater,
			IDirectoryProvider directoryProvider,
			IConfig config,
			ILogger<Generator> logger)
		{
			this.problemParsers = new ReadOnlyCollection<IProblemParser>(problemParsers.ToArray());
			this.interfaceGenerator = interfaceGenerator;
			this.classGenerator = classGenerator;
			this.testClassGenerator = testClassGenerator;
			this.csFileSaver = csFileSaver;
			this.readmeUpdater = readmeUpdater;
			this.directoryProvider = directoryProvider;
			this.config = config;
			this.logger = logger;
		}

		public async Task GenerateAsync(SourceType source, string name)
		{
			try
			{
				var problemParser = GetProblemParser(source);
				var problem = await problemParser.ParseAsync(name).ConfigureAwait(false);
				logger.LogDebug($"Problem parsed from {source}.");
				
				var interfaceNode = await interfaceGenerator.GenerateAsync(problem).ConfigureAwait(false);
				logger.LogInformation($"Interface generated.");
				var classNode = await classGenerator.GenerateAsync(interfaceNode).ConfigureAwait(false);
				logger.LogInformation("Class generated.");
				var testClassNode = await testClassGenerator.GenerateAsync(classNode).ConfigureAwait(false);
				logger.LogInformation("Test class generated.");

				var interfacePath = Path.Combine(directoryProvider.GetCurrentDirectory().FullName, config.SrcPath, problem.Title.ToClassName());
				await csFileSaver.SaveAsync(interfaceNode, interfacePath).ConfigureAwait(false);
				logger.LogInformation($"Interface saved to '{interfacePath}'.");
				
				var classPath = Path.Combine(directoryProvider.GetCurrentDirectory().FullName, config.SrcPath, problem.Title.ToClassName());
				await csFileSaver.SaveAsync(classNode, classPath).ConfigureAwait(false);
				logger.LogInformation($"Class saved to '{classPath}'.");

				var testsPath = Path.Combine(directoryProvider.GetCurrentDirectory().FullName, config.TestsPath);
				await csFileSaver.SaveAsync(testClassNode, testsPath).ConfigureAwait(false);
				logger.LogInformation($"Test class saved to '{testsPath}'.");

				var readmePath = Path.Combine(directoryProvider.GetCurrentDirectory().FullName, config.ReadmePath);
				await readmeUpdater.Insert(problem, readmePath).ConfigureAwait(false);
				logger.LogInformation($"Readme updated ({readmePath}).");
			}
			catch (Exception e)
			{
				logger.LogCritical(e, "Error occurred while generation.");
			}
		}

		private IProblemParser GetProblemParser(SourceType source)
		{
			return problemParsers.SingleOrDefault(x => x.SourceType == source)
				?? throw new NotSupportedException($"The source of the problems '{source}' is not supported.");;
		}
	}
}
