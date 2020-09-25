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
	public interface ITestClassGenerator
	{
		Task<SyntaxNode> GenerateAsync(SyntaxNode classSyntaxNode);
	}

	public class TestClassGenerator : CodeGenerator, ITestClassGenerator
	{
		public TestClassGenerator(IDirectoryProvider directoryProvider, IAnalyzerManager analyzerManager) 
			: base(directoryProvider, analyzerManager)
		{
		}

		public async Task<SyntaxNode> GenerateAsync(SyntaxNode classSyntaxNode)
		{
			var testedClassNamespace = classSyntaxNode
				.DescendantNodes()
				.OfType<NamespaceDeclarationSyntax>()
				.Single();

			var testedClass = classSyntaxNode
				.DescendantNodes()
				.OfType<ClassDeclarationSyntax>()
				.Single();

			var testedClassInterface = testedClass.BaseList!.Types.Single();

			var testedMethod = classSyntaxNode
				.DescendantNodes()
				.OfType<MethodDeclarationSyntax>()
				.Single();

			var testClassTemplateCode = GetTestClassTemplate(
				testedClassNamespace.Name.GetLastToken().ToString(),
				testedClass.Identifier.ToString(),
				testedClassInterface.Type.ToString(), 
				testedMethod.Identifier.ToString());

			var templateSyntax = await CSharpSyntaxTree
				.ParseText(testClassTemplateCode)
				.GetRootAsync()
				.ConfigureAwait(false);

			var templateClass = templateSyntax
				.DescendantNodes()
				.OfType<ClassDeclarationSyntax>()
				.Single();

			var templateClassMethod = templateClass
				.DescendantNodes()
				.OfType<MethodDeclarationSyntax>()
				.Single(x => x.Identifier.Text.Contains(testedMethod.Identifier.Text));

			var testClassNamespace = NamespaceDeclaration(ParseName("LeetCode.Solutions.Tests"));
			var testClassMethodParams = testedMethod.ParameterList.Parameters
				.Add(Parameter(Identifier("expectedResult")).WithType(testedMethod.ReturnType));
			var testClassMethod = templateClassMethod.AddParameterListParameters(testClassMethodParams.ToArray());
			var testClass = templateClass.ReplaceNode(templateClassMethod, testClassMethod);

			var testClassSyntax = UnitSyntax;
			testClassNamespace = testClassNamespace.AddMembers(testClass);
			testClassSyntax = testClassSyntax.AddMembers(testClassNamespace);
			testClassSyntax = testClassSyntax.AddUsings(new[]
			{
				UsingDirective(ParseName("LeetCode.Helpers")),
				UsingDirective(ParseName(testedClassNamespace.Name.ToString())),
				UsingDirective(ParseName("NUnit.Framework")),
				UsingDirective(ParseName("FluentAssertions"))
			});

			return Formatter.Format(testClassSyntax, Workspace);
		}

		private static string GetTestClassTemplate(string problemName, string testedClassName, string testedClassInterfaceName, string testedMethodName)
		{
			return 
			@$"
				[TestFixture]
				public class {problemName}Test
				{{
					private static IEnumerable testCases = new[] {{ new TestCaseData().SetName("""") }};

					private {testedClassInterfaceName} approach1;

					[OneTimeSetUp]
					public void OneTimeSetUp()
					{{
						approach1 = new {testedClassName}();
					}}

					[TestCaseSource(nameof(testCases))]
					public void Approach1_{testedMethodName}()
					{{
						var actualResult = approach1.{testedMethodName}();

						actualResult.Should().Be(expectedResult);
					}}
				}}
			";
		}
	}
}
