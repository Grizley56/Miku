using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Miku.Framework.Console
{
	internal class ConsoleRenderManager
	{
		private RenderTarget2D _consoleFacade;

		private Texture2D _texture;
		private SpriteBatch _spriteBatch;
		internal SpriteFont Font { get; set; }

		private readonly GraphicsDevice _graphicDevice;
		public ConsoleInputManager InputTarget { get; }
		public Rectangle Bounds => _graphicDevice.Viewport.Bounds;
		public int Height => (int)Math.Max(_graphicDevice.Viewport.Bounds.Height / 2.5f, 200f);

		public Color BackColor { get; set; } = Color.DimGray;

		public float BackOpacity { get; set; } = 0.5f;
		public float FontOpacity { get; set; } = 1f;

		public ConsoleRenderManager(ConsoleInputManager inputManager, GraphicsDevice graphicDevice)
		{
			_graphicDevice = graphicDevice;
			InputTarget = inputManager;

			_texture = new Texture2D(graphicDevice, 1, 1);
			_spriteBatch = new SpriteBatch(graphicDevice);

			_texture.SetData(new byte[] { 255, 255, 255, 255});
		}


		private TimeSpan _blinkShowtime = TimeSpan.Zero;
		private TimeSpan _blinkSpeed = TimeSpan.FromSeconds(0.5f);
		private bool CursorVisiblePhase = true;


		private Vector2 _inputFieldPadding = new Vector2(5);
		private Vector2 _inputFontPadding = new Vector2(3);
		private Vector2 _inputFieldSize => new Vector2(Bounds.Width - _inputFieldPadding.X*2, Font.LineSpacing + 5);
		private Color _inputFieldColor = Color.Black;
		private float _inputFieldOpacity = 0.5f;

		public void Draw(GameTime gameTime)
		{
			_blinkShowtime += gameTime.ElapsedGameTime;

			if (_blinkShowtime >= _blinkSpeed)
			{
				CursorVisiblePhase = !CursorVisiblePhase;
				_blinkShowtime = TimeSpan.Zero;
			}

			//_consoleFacade = new RenderTarget2D(_graphicDevice, _graphicDevice.Viewport.Width, _graphicDevice.Viewport.Height);
			//_graphicDevice.SetRenderTarget(_consoleFacade);
			//_spriteBatch.Begin();

			////Draw background
			//_spriteBatch.Draw(_texture, new Rectangle(0, 0, Bounds.Width, Height), _texture.Bounds, Color.White * BackOpacity);
			//_spriteBatch.Draw()
			//_spriteBatch.End();

			//_graphicDevice.SetRenderTarget(null);

			_spriteBatch.Begin();
			_spriteBatch.Draw(_texture, new Rectangle(0, 0, Bounds.Width, Height), _texture.Bounds, BackColor * BackOpacity);


			_spriteBatch.Draw(_texture, new Rectangle((int) _inputFieldPadding.X, (int) _inputFieldPadding.Y,
				(int) _inputFieldSize.X,
				Height - 50), _inputFieldColor * _inputFieldOpacity);

			Rectangle inputFieldBounds = new Rectangle((int) _inputFieldPadding.X,
				Height - (int) _inputFieldSize.Y - (int) _inputFieldPadding.Y,
				(int) _inputFieldSize.X, (int) _inputFieldSize.Y);

			_spriteBatch.Draw(_texture, inputFieldBounds, _inputFieldColor * _inputFieldOpacity);

			_spriteBatch.DrawString(Font, InputTarget.CurrentInput, new Vector2(_inputFontPadding.X + _inputFieldPadding.X, inputFieldBounds.Y + _inputFieldPadding.Y), Color.White * FontOpacity);
			Vector2 size = Font.MeasureString(InputTarget.CurrentInput.Substring(0, InputTarget.CursorPosition));


			if(CursorVisiblePhase)
				_spriteBatch.DrawString(Font, "|", new Vector2(_inputFieldPadding.X + size.X + 1, inputFieldBounds.Y + _inputFieldPadding.Y), Color.White);


			Vector2 startOffset = new Vector2(10, 10); // TODO: padding

			for (var i = 0; i < InputTarget.ConsoleHistory.Count; i++)
			{
				string entry = InputTarget.ConsoleHistory[i];
				Vector2 offset = new Vector2(startOffset.X, startOffset.Y + (i * Font.LineSpacing));
				_spriteBatch.DrawString(Font, entry, offset, Color.White * FontOpacity);
			}


			_spriteBatch.End();
			

		}


		public void Update(GameTime gameTime)
		{
			
		}

	}
}
