using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProblemTemplateGenerator.Cli.Helpers
{
	public static class StringExtensions
	{
		private static readonly Regex specialCharReplacer = new Regex(@"[^a-zA-Z0-9 .]+", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(10));
		
		public static string Join(this IEnumerable<string> enumerable, string? separator = null)
		{
			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));

			return separator != null ? string.Join(separator, enumerable) : string.Concat(enumerable);
		}

		public static string FirstCharToUpper(this string str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			if (str == "")
				throw new ArgumentException($"{nameof(str)} cannot be empty", nameof(str));

			return str.First().ToString().ToUpper() + str.Substring(1);
		}

		public static string ToClassName(this string str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return specialCharReplacer.Replace(str, "").Split(' ').Select(FirstCharToUpper).Join();
		}

		public static string ToInterfaceName(this string str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return $"I{ToClassName(str)}";
		}
	}
}
