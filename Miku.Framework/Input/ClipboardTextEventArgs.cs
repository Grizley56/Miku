using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Input
{
	public class ClipboardTextEventArgs : EventArgs
	{
		public readonly string Text;
		public ClipboardTextEventArgs(string text)
		{
			Text = text;
		}
	}
}
