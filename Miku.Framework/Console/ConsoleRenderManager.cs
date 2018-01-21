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
		public Rectangle ViewportBounds => _graphicDevice.Viewport.Bounds;

		public Rectangle ConsoleBounds => new Rectangle(0, 0, ViewportBounds.Width,
			(int) Math.Max(_graphicDevice.Viewport.Bounds.Height / 2.5f, 200f));

		public Color BackColor { get; set; } = Color.White;

		public float BackOpacity { get; set; } = 0.5f;
		public float FontOpacity { get; set; } = 1f;

		public ConsoleRenderManager(ConsoleInputManager inputManager, GraphicsDevice graphicDevice)
		{
			_graphicDevice = graphicDevice;
			InputTarget = inputManager;

			_texture = new Texture2D(graphicDevice, 1, 1);
			_spriteBatch = new SpriteBatch(graphicDevice);

			_texture.SetData(new byte[] { 255, 255, 255, 255});

			_historyField = new ConsoleField(() => new Rectangle(ConsoleBounds.X, ConsoleBounds.Y, ConsoleBounds.Width, 
				ConsoleBounds.Height - (int)_inputFieldSize.Y - (int)_inputFieldPadding.Y*2));
			_historyField.Padding = new Point(5, 5);
			_historyField.BackColor = Color.Black;
			_historyField.BackOpacity = 0.5f;

			_inputField = new ConsoleField(() => new Rectangle(ConsoleBounds.X, ConsoleBounds.Y + _historyField.Bounds.Height + 12,
				ConsoleBounds.Width,
				Font.LineSpacing + 5));
			_inputField.Padding = new Point(5, 0);
			_inputField.BackColor = Color.Black;
			_inputField.BackOpacity = 0.5f;
		}

		private ConsoleField _historyField;
		private ConsoleField _inputField;

		private TimeSpan _blinkShowtime = TimeSpan.Zero;
		private TimeSpan _blinkSpeed = TimeSpan.FromSeconds(0.5f);
		private bool _cursorVisiblePhase = true;

		private Rectangle _historyBounds => new Rectangle();
		
		private Vector2 _inputFieldPadding = new Vector2(5);
		private Vector2 _inputFontPadding = new Vector2(3, 5);
		private Vector2 _inputFieldSize => new Vector2(ViewportBounds.Width - _inputFieldPadding.X*2, Font.LineSpacing + 5);

		private Color _inputFieldColor = Color.Black;
		private float _inputFieldOpacity = 0.5f;

		public void Draw(GameTime gameTime)
		{
			_blinkShowtime += gameTime.ElapsedGameTime;

			if (_blinkShowtime >= _blinkSpeed)
			{
				_cursorVisiblePhase = !_cursorVisiblePhase;
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

			_spriteBatch.Draw(_texture, ConsoleBounds, BackColor * BackOpacity); // BG

			_spriteBatch.Draw(_texture, _historyField.Bounds, _historyField.BackColor * _historyField.BackOpacity); // BG HISTORY

			_spriteBatch.Draw(_texture, _inputField.Bounds, _inputField.BackColor * _inputField.BackOpacity); // BG INPUT

			_spriteBatch.DrawString(Font, InputTarget.CurrentInput, 
				_inputField.Bounds.Location.ToVector2() + _inputFontPadding, Color.White * FontOpacity);

			Vector2 size = Font.MeasureString(InputTarget.CurrentInput.Substring(0, InputTarget.CursorPosition));

			if (_cursorVisiblePhase)
				_spriteBatch.DrawString(
				Font, 
				"|", 
				new Vector2(_inputField.Bounds.X + size.X - 1, _inputField.Bounds.Y + _inputFieldPadding.Y), 
				Color.White);


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
