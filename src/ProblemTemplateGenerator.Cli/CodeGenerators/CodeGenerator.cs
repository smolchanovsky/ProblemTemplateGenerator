using System.IO;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProblemTemplateGenerator.Cli.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ProblemTemplateGenerator.Cli.CodeGenerators
{
	public abstract class CodeGenerator
	{
		protected const string NamespaceRoot = "LeetCode.Solutions";

		protected readonly AdhocWorkspace Workspace;
		protected readonly CompilationUnitSyntax UnitSyntax;

		protected CodeGenerator(IDirectoryProvider directoryProvider, IAnalyzerManager analyzerManager)
		{
			var projectPath = Path.Combine(directoryProvider.GetSolutionDirectory().FullName, @"src\ProblemTemplateGenerator.Cli\ProblemTemplateGenerator.Cli.csproj");
			var analyzer = analyzerManager.GetProject(projectPath);
			Workspace = analyzer.GetWorkspace();

			UnitSyntax = CompilationUnit();
			UnitSyntax = UnitSyntax.AddUsings(new[]
			{
				UsingDirective(ParseName("System")),
				UsingDirective(ParseName("System.Collections")),
				UsingDirective(ParseName("System.Collections.Generic")),
				UsingDirective(ParseName("System.Linq")),
				UsingDirective(ParseName("System.Text"))
			});
		}
	}
}
