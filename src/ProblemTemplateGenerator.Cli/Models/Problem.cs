namespace ProblemTemplateGenerator.Cli.Models
{
	public class Problem
	{
		public SourceType Source { get; }
		public string Url { get; }
		public int Number { get; }
		public string Title { get; }
		public string Statement { get; }
		public DifficultType Difficult { get; }
		public string Code { get; }

		public Problem(SourceType source, string url, int number, string title, string statement, DifficultType difficult, string code)
		{
			Source = source;
			Url = url;
			Number = number;
			Title = title;
			Statement = statement;
			Difficult = difficult;
			Code = code;
		}
	}
}
