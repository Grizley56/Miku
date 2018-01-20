using System;

namespace Miku.Framework.Input
{
	public class TextEnteredEventArgs: EventArgs
	{
		public readonly string EnteredText;

		public TextEnteredEventArgs(string enteredText)
		{
			EnteredText = enteredText;
		}
	}
}