using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using ProblemTemplateGenerator.Cli.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ProblemTemplateGenerator.Cli.CodeGenerators
{
	public interface IClassGenerator
	{
		Task<SyntaxNode> GenerateAsync(SyntaxNode interfaceSyntaxNode);
	}

	public class ClassGenerator : CodeGenerator, IClassGenerator
	{
		public ClassGenerator(IDirectoryProvider directoryProvider, IAnalyzerManager analyzerManager) 
			: base(directoryProvider, analyzerManager)
		{
		}

		public Task<SyntaxNode> GenerateAsync(SyntaxNode interfaceSyntaxNode)
		{
			var interfaceNamespace = interfaceSyntaxNode
				.DescendantNodes()
				.OfType<NamespaceDeclarationSyntax>()
				.Single();

			var @interface = interfaceSyntaxNode
				.DescendantNodes()
				.OfType<InterfaceDeclarationSyntax>()
				.Single();

			var interfaceMethod = interfaceSyntaxNode
				.DescendantNodes()
				.OfType<MethodDeclarationSyntax>()
				.Single();

			var classComment = Comment(TextCommentBuilder());
			var classNamespace = NamespaceDeclaration(interfaceNamespace.Name);
			var @class = ClassDeclaration($"{interfaceNamespace.Name.GetLastToken()}Approach1")
				.AddModifiers(Token(SyntaxKind.PublicKeyword))
				.AddBaseListTypes(SimpleBaseType(ParseTypeName(@interface.Identifier.Text)));
			var classMethod = MethodDeclaration(interfaceMethod.ReturnType, interfaceMethod.Identifier)
				.WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
				.WithParameterList(interfaceMethod.ParameterList)
				.WithBody(Block(SingletonList(ThrowStatement(ObjectCreationExpression(IdentifierName("NotImplementedException")).WithArgumentList(ArgumentList())))));

			var classSyntax = UnitSyntax;
			@class = @class.AddMembers(classMethod).WithLeadingTrivia(classComment);
			classNamespace = classNamespace.AddMembers(@class);
			classSyntax = classSyntax.AddMembers(classNamespace);

			return Task.FromResult(Formatter.Format(classSyntax, Workspace));
		}
		
		private static string TextCommentBuilder()
		{
			return "/// <summary>\n"
				 + "\t/// Time complexity: O(...).\n" 
				 + "\t/// Space complexity: O(...).\n" 
				 + "\t/// Runtime: .\n" 
				 + "\t/// Memory Usage: .\n" 
				 + "\t/// </summary>\n";
		}
	}
}
