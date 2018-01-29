using System;

namespace Miku.Framework.Input
{
	public class TextSubmittedEventArgs: EventArgs
	{
		public readonly string ResultText;

		public TextSubmittedEventArgs(string resultText)
		{
			ResultText = resultText;
		}
	}
}