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

		//internal static int IndexOf(this string source, string subString, int position = 0, Direction direction = Direction.LeftToRight)
		//{
		//	if (direction == Direction.LeftToRight)
		//		return source.IndexOf(subString, position, StringComparison.Ordinal);

		//	string reversed = source.Reverse();
		//	int newPosition = source.Length - position;
		//	int resultIndex = reversed.IndexOf(subString, newPosition, StringComparison.Ordinal);

		//	return resultIndex == -1 ? -1 : source.Length - resultIndex;
		//}

		//internal static int IndexOfAny(this string source, char[] checkingChars, int position = 0, Direction direction = Direction.LeftToRight)
		//{
		//	if (direction == Direction.LeftToRight)
		//		return source.IndexOfAny(checkingChars, position);

		//	string reversed = source.Reverse();
		//	int newPosition = source.Length - position;
		//	int result = reversed.IndexOfAny(checkingChars, newPosition);
		//	return result == -1 ? -1 : source.Length - result;
		//}

		internal static string Reverse(this string source)
		{
			return new string(source.ToCharArray().Reverse().ToArray());
		}

		internal static int LinesCount(this string source) => source.Count(i => i == '\n') + 1;
	}
}
