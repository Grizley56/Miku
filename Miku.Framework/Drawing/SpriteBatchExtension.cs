using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miku.Framework.Drawing
{
	public static class SpriteBatchExtension
	{
		private static Texture2D _solidColorTexture;

		public static Texture2D CreateSolidTexture(this SpriteBatch batch)
		{
			Texture2D texture = new Texture2D(batch.GraphicsDevice, 1, 1);
			texture.SetData(new byte[] { 255, 255, 255, 255 });
			return texture;
		}

		public static void DrawRect(this SpriteBatch batch, Rectangle bounds, Color color)
		{
			if (_solidColorTexture == null)
				_solidColorTexture = batch.CreateSolidTexture();

			batch.Draw(_solidColorTexture, bounds, color);
		}
	}
}
