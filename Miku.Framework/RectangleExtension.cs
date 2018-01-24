using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Miku.Framework
{
	public static class RectangleExtension
	{
		public static Rectangle CreateTranslated(this Rectangle source, int offsetX, int offsetY)
		{
			return source.CreateTranslated(new Point(offsetX, offsetY));
		}

		public static Rectangle CreateTranslated(this Rectangle source, Point offset)
		{
			return new Rectangle(source.Location + offset, source.Size);
		}

		public static Rectangle CreateScaled(this Rectangle source, int sizeX, int sizeY)
		{
			return source.CreateScaled(new Point(sizeX, sizeY));
		}

		public static Rectangle CreateScaled(this Rectangle source, Point size)
		{
			return new Rectangle(source.Size + size, source.Location);
		}

		public static Rectangle CreateModified(this Rectangle source, Point position, Point size)
		{
			return new Rectangle(source.Location + position, source.Size - size);
		}

	}
}
