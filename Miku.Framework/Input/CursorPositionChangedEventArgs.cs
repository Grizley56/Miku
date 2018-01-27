using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Input
{
	public class CursorPositionChangedEventArgs: EventArgs
	{
		public readonly int NewPosition;
		public readonly int OldPosition;

		public CursorPositionChangedEventArgs(int oldPosition, int newPosition)
		{
			OldPosition = oldPosition;
			NewPosition = newPosition;
		}
	}
}
