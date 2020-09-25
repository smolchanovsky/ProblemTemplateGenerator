using System;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using ProblemTemplateGenerator.Cli.Helpers;
using ProblemTemplateGenerator.Cli.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ProblemTemplateGenerator.Cli.CodeGenerators
{
	public interface IInterfaceGenerator
	{
		Task<SyntaxNode> GenerateAsync(Problem problem);
	}

	public class InterfaceGenerator : CodeGenerator, IInterfaceGenerator
	{
		public InterfaceGenerator(IDirectoryProvider directoryProvider, IAnalyzerManager analyzerManager) 
			: base(directoryProvider, analyzerManager)
		{
		}

		public async Task<SyntaxNode> GenerateAsync(Problem problem)
		{
			var sourceClassSyntax = await CSharpSyntaxTree
				.ParseText(problem.Code)
				.GetRootAsync()
				.ConfigureAwait(false);

			var sourceClass = sourceClassSyntax
				.DescendantNodes()
				.OfType<ClassDeclarationSyntax>()
				.Single();

			var sourceMethod = sourceClass
				.DescendantNodes()
				.OfType<MethodDeclarationSyntax>()
				.Single();

			var interfaceComment = Comment(TextCommentBuilder(problem));
			var interfaceNamespace = NamespaceDeclaration(ParseName($"{NamespaceRoot}.{problem.Title.ToClassName()}"));
			var @interface = InterfaceDeclaration(problem.Title.ToInterfaceName())
				.AddModifiers(Token(SyntaxKind.PublicKeyword));
			var interfaceMethod = MethodDeclaration(sourceMethod.ReturnType, sourceMethod.Identifier)
				.WithParameterList(sourceMethod.ParameterList)
				.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

			var interfaceSyntax = UnitSyntax;
			@interface = @interface.AddMembers(interfaceMethod).WithLeadingTrivia(interfaceComment);
			interfaceNamespace = interfaceNamespace.AddMembers(@interface);
			interfaceSyntax = interfaceSyntax.AddMembers(interfaceNamespace);

			return Formatter.Format(interfaceSyntax, Workspace);
		}

		private static string TextCommentBuilder(Problem problem)
		{
			return "/// <summary>\n" 
				 + $"\t/// <see href=\"{problem.Url}\">{problem.Title}</see>\n" 
				 + $"{problem.Statement.Split(".", StringSplitOptions.RemoveEmptyEntries).Select(x => $"\t/// {x.Trim()}.\n").Join()}"
				 + "\t/// </summary>\n";
		}
	}
}
