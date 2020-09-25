using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ProblemTemplateGenerator.Cli.Helpers;
using ProblemTemplateGenerator.Cli.Models;

namespace ProblemTemplateGenerator.Cli.Io
{
	public interface IReadmeUpdater
	{
		Task Insert(Problem problem, string path);
	}

	public class ReadmeUpdater : IReadmeUpdater
	{
		private readonly IFileReaderWriter fileReaderWriter;

		private readonly Regex problemTableParser = 
			new Regex(@"\|\s*(\d*)\s*\|\s*(.*)\s*\|\s*(.*)\s*\|\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(10));

		public ReadmeUpdater(IFileReaderWriter fileReaderWriter)
		{
			this.fileReaderWriter = fileReaderWriter;
		}
		
		public async Task Insert(Problem problem, string path)
		{
			var readmeLines = (await fileReaderWriter.ReadAllLinesAsync(path).ConfigureAwait(false)).ToList();
			
			var lineForInsert = readmeLines
				.Select((line, index) => (Value: line, Index: index))
				.First(x =>
				{
					if (x.Index == readmeLines.Count - 1)
						return true;
					
					var problemRow = TryGetProblemTableRow(x.Value);
					if (problemRow == null)
						return false;

					return problemRow!.Value.Number > problem.Number;
				});

			var newLine = CreateProblemTableRow(problem);
			readmeLines.Insert(lineForInsert.Index, newLine);
			await fileReaderWriter.WriteAllLinesAsync(path, readmeLines).ConfigureAwait(false); 
		}

		private (int Number, string Name, DifficultType Difficult)? TryGetProblemTableRow(string line)
		{
			var match = problemTableParser.Match(line);
			if (!match.Success)
				return null;

			if (!int.TryParse(match.Groups[1].Value, out var problemNumber))
				throw new InvalidOperationException("Problem number parsing error. The table is invalid.");

			var problemName = match.Groups[2].Value;
			if (string.IsNullOrEmpty(problemName))
				throw new InvalidOperationException("Problem name parsing error. The table is invalid.");
			
			if (!Enum.TryParse<DifficultType>(match.Groups[3].Value, out var problemDifficult))
				throw new InvalidOperationException("Problem difficult parsing error. The table is invalid.");
			
			return (problemNumber, problemName, problemDifficult);
		}

		private static string CreateProblemTableRow(Problem problem)
		{
			const int numberColumnWidth = 4;
			const int nameColumnWidth = 49;
			const int difficultColumnWidth = 10;

			return $"| {problem.Number}{new string(' ', numberColumnWidth - problem.Number.ToString().Length)} " 
				 + $"| {problem.Title}{new string(' ', nameColumnWidth - problem.Title.Length)} "
				 + $"| {problem.Difficult}{new string(' ', difficultColumnWidth - problem.Difficult.ToString().Length)} |";
		}
	}
}
