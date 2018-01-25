using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Framework.Input
{
	public class CursorPositionChangedEventArgs: EventArgs
	{
		public readonly int Position;

		public CursorPositionChangedEventArgs(int newPosition)
		{
			Position = newPosition;
		}
	}
}
