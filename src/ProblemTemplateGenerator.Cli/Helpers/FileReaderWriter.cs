using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProblemTemplateGenerator.Cli.Helpers
{
	public interface IFileReaderWriter
	{
		Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default);
		Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default);
	}

	public class FileReaderWriter : IFileReaderWriter
	{
		public async Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
		{
			return await File.ReadAllLinesAsync(path, cancellationToken).ConfigureAwait(false);
		}

		public async Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default)
		{
			await File.WriteAllLinesAsync(path, contents, cancellationToken).ConfigureAwait(false);
		}
	}
}
