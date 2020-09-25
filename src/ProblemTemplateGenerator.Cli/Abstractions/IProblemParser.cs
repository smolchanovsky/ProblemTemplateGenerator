using System.Threading.Tasks;
using ProblemTemplateGenerator.Cli.Models;

namespace ProblemTemplateGenerator.Cli.Abstractions
{
	public interface IProblemParser
	{
		SourceType SourceType { get; } 
		Task<Problem> ParseAsync(string name);
	}
}
