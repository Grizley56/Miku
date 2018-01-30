using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Miku.Framework
{
	public static class PointExtensions
	{
		public static Point Negative(this Point source)
		{
			return new Point(-source.X, -source.Y);
		}
	}
}
