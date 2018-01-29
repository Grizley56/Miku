using System;

namespace Miku.Framework.Input
{
	public class CharHandlingEventArgs: EventArgs
	{
		public readonly char Char;
		public bool IgnoreHandle { get; set; }
		public CharHandlingEventArgs(char c)
		{
			Char = c;
			IgnoreHandle = false;
		}
	}
}
