using System.IO;
using System.Linq;
using System.Reflection;
using OpenQA.Selenium;

namespace ProblemTemplateGenerator.Cli.Helpers
{
	public interface IDirectoryProvider
	{
		DirectoryInfo GetCurrentDirectory();
		DirectoryInfo GetSolutionDirectory(string? currentPath = null);
		DirectoryInfo? TryGetSolutionDirectory(string? currentPath = null);
	}

	public class DirectoryProvider : IDirectoryProvider
	{
		public DirectoryInfo GetCurrentDirectory()
		{
			return new DirectoryInfo(Directory.GetCurrentDirectory());
		}
		
		public DirectoryInfo GetSolutionDirectory(string? currentPath = null)
		{
			return TryGetSolutionDirectory(currentPath)
					?? throw new NotFoundException("Solution directory not found.");
		}
		
		public DirectoryInfo? TryGetSolutionDirectory(string? currentPath = null)
		{
			var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
				?? throw new NotFoundException("Assembly directory not found");
			
			var directory = new DirectoryInfo(currentPath ?? assemblyDirectory);
			while (directory != null && !directory.GetFiles("*.sln").Any())
			{
				directory = directory.Parent;
			}
			
			return directory;
		}
	}
}