using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Miku.Framework.Input
{
	public struct KeyInfo
	{
		public Keys Key { get; }
		public char Character { get; }

		public KeyInfo(Keys key, char character)
		{
			Key = key;
			Character = character;
		}
	}
}
