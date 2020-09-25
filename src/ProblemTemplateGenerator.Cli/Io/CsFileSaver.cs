using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProblemTemplateGenerator.Cli.Io
{
	public interface ICsFileSaver
	{
		Task SaveAsync(SyntaxNode syntaxNode, string dirPath);
	}

	public class CsFileSaver : ICsFileSaver
	{
		public async Task SaveAsync(SyntaxNode syntaxNode, string dirPath)
		{
			var className = syntaxNode
				.DescendantNodes()
				.OfType<ClassDeclarationSyntax>()
				.SingleOrDefault()?
				.Identifier.Text;

			var interfaceName = syntaxNode
				.DescendantNodes()
				.OfType<InterfaceDeclarationSyntax>()
				.SingleOrDefault()?
				.Identifier.Text;

			var fileName = (className, interfaceName) switch
			{
				{} x when x.className != null && x.interfaceName == null => x.className,
				{} x when x.className == null && x.interfaceName != null => x.interfaceName,
				{} x when x.className != null && x.interfaceName != null => x.className,
				(_, _) => throw new InvalidOperationException("An interface or class should be saved to the file"),
			};

			var filePath = Path.Combine(dirPath, $"{fileName}.cs");
			if (File.Exists(filePath))
				throw new InvalidOperationException("File already exists");

			Directory.CreateDirectory(dirPath);
			await File.WriteAllTextAsync(filePath, syntaxNode.ToFullString());
		}
	}
}
