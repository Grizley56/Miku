using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Miku.Framework.Console
{
	internal class ConsoleField
	{
		private readonly Func<Rectangle> _getBounds;

		public Rectangle Bounds
		{
			get
			{
				Rectangle currentBounds = _getBounds();
				return new Rectangle(Padding.X + currentBounds.X, Padding.Y + currentBounds.Y, Size.X, Size.Y);
			}
		}

		public Point Padding { get; set; }
		public Point TextPadding { get; set; }

		public Point Size
		{
			get
			{
				Rectangle currentBounds = _getBounds();
				return new Point(currentBounds.Width - Padding.X*2, currentBounds.Height - Padding.Y*2);
			}
		}

		public ConsoleField(Func<Rectangle> getBounds, Point padding, Point textPadding)
		{
			if (getBounds == null)
				throw new ArgumentNullException(nameof(getBounds));
			
			Padding = padding;
			TextPadding = textPadding;
			_getBounds = getBounds;
			
		}
	}
}
