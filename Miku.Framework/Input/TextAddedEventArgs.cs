using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Input
{
	public class TextAddedEventArgs: EventArgs
	{
		public readonly int AddedTextStartIndex;
		public readonly string ResultText;
		public readonly string AddedText;
		public TextAddedEventArgs(string addedText, string resultText, int addedTextStartIndex)
		{
			AddedText = addedText;
			ResultText = resultText;
			AddedTextStartIndex = addedTextStartIndex;
		}
	}
}
