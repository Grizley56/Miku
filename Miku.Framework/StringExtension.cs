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

		internal static int IndexOf(this string source, int position, string subString, Direction direction = Direction.LeftToRight)
		{
			if (direction == Direction.LeftToRight)
				return source.IndexOf(subString, position, StringComparison.Ordinal);

			string reversed = source.Reverse();
			int newPosition = source.Length - position;

			return reversed.IndexOf(subString, newPosition, StringComparison.Ordinal);
		}

		internal static string Reverse(this string source)
		{
			return new string(source.ToCharArray().Reverse().ToArray());
		}
	}
}
