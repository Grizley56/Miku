using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Console
{
	class ConsoleEntryAddedEventArgs: EventArgs
	{
		public readonly ConsoleEntry AddedItem;
		public ConsoleEntryAddedEventArgs(ConsoleEntry addedItem)
		{
			AddedItem = addedItem;
		}
	}
}
