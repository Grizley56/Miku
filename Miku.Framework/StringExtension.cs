using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework
{
	internal enum Direction
	{
		LeftToRight,
		RightToLeft
	}
	static class StringExtension
	{
		internal static string Reverse(this string source)
		{
			return new string(source.ToCharArray().Reverse().ToArray());
		}

		internal static int LinesCount(this string source) => source.Count(i => i == '\n') + 1;
	}
}
