using System;

namespace Miku.Framework.Input
{
	public class TextRemovedEventArgs: EventArgs
	{
		public readonly string RemovedText;
		public readonly int RemovedTextStartIndex;
		public readonly string ResultText;
		public TextRemovedEventArgs(string removedText, string resultText, int removedTextStartIndex)
		{
			RemovedText = removedText;
			ResultText = resultText;
			RemovedTextStartIndex = removedTextStartIndex;
		}
	}
}