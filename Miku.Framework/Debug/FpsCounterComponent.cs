using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miku.Framework.Debug
{
	public class FpsCounterComponent: DrawableGameComponent
	{
		public SpriteFont Font
		{
			get { return _font; }
			set
			{
				if (_font == value)
					return;

				_font = value;
				UpdateFontPosition();
			}
		}

		// ReSharper disable once InconsistentNaming
		public int FPS
		{
			get { return _fps; }
			private set
			{
				if (_fps == value)
					return;

				_fps = value;
				UpdateFontPosition();
			}
		}

		private TimeSpan _tmpTimer;
		private int _framesCounter;

		private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
		private static readonly Color BadFpsColor = Color.Red;
		private static readonly Color NormalFpsColor = Color.Yellow;
		private static readonly Color GoodFpsColor = Color.Green;

		private Vector2 _position;
		private Vector2 _padding = new Vector2(6);
		private SpriteBatch _batch;
		private SpriteFont _font;
		private int _fps;

		public FpsCounterComponent(Game game, SpriteFont font) : base(game)
		{
			Font = font;
			DrawOrder = Int32.MaxValue;

			Game.Components.Add(this);
			Game.Services.AddService(this);

			_batch = Game.Services.GetService<SpriteBatch>();
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible)
				return;

			_tmpTimer += gameTime.ElapsedGameTime;

			if (_tmpTimer >= OneSecond)
			{
				_tmpTimer = TimeSpan.Zero;
				FPS = _framesCounter;
				_framesCounter = 0;
			}
			base.Update(gameTime);
		}
		
		public override void Draw(GameTime gameTime)
		{
			_framesCounter++;
			
			_batch.Begin();
			_batch.DrawString(Font, FPS.ToString(), _position, FPS <= 20 ? BadFpsColor : FPS <= 40 ? NormalFpsColor : GoodFpsColor);
			_batch.End();

			base.Draw(gameTime);
		}
		
		private void UpdateFontPosition()
		{
			Vector2 fontSize = Font.MeasureString(FPS.ToString());
			_position = new Vector2(Game.GraphicsDevice.Viewport.Bounds.Width - fontSize.X - _padding.X, _padding.Y);
		}
	}
}
