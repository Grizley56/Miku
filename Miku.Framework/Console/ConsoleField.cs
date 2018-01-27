using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Miku.Framework.Console
{
	class ConsoleField
	{
		private Func<Rectangle> _getBounds;

		public Color BackColor { get; set; }

		public Rectangle Bounds
		{
			get
			{
				Rectangle currentBounds = _getBounds();
				return new Rectangle(Padding.X + currentBounds.X, Padding.Y + currentBounds.Y, Size.X, Size.Y);
			}
		}

		public Point Padding { get; set; }

		public Point Size
		{
			get
			{
				Rectangle currentBounds = _getBounds();
				return new Point(currentBounds.Width - Padding.X*2, currentBounds.Height - Padding.Y*2);
			}
		}

		public ConsoleField(Func<Rectangle> getBounds)
		{
			if (getBounds == null)
				throw new ArgumentNullException(nameof(getBounds));
			
			_getBounds = getBounds;
		}
		
	}
}
