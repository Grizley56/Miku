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
		private Texture2D _solidColorTexture;

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

			_solidColorTexture = new Texture2D(graphicDevice, 1, 1);
			_solidColorTexture.SetData(new byte[] { 255, 255, 255, 255});

			InitializeFacade();
		}

		private void InitializeFacade()
		{
			_inputField = new ConsoleField(() => new Rectangle(ConsoleBounds.X,
				ConsoleBounds.Height - Font.LineSpacing - (int)_inputTextPadding.Y * 2, ConsoleBounds.Width,
				Font.LineSpacing + (int)_inputTextPadding.Y * 2))
			{
				Padding = new Point(5, 3),
				BackColor = Color.Black,
				BackOpacity = 0.5f
			};

			_historyField = new ConsoleField(() => new Rectangle(ConsoleBounds.X, ConsoleBounds.Y, ConsoleBounds.Width,
				ConsoleBounds.Height - _inputField.Bounds.Height - _inputField.Padding.Y * 2))
			{
				Padding = new Point(5, 0),
				BackColor = Color.Black,
				BackOpacity = 0.5f
			};
		}

		private ConsoleField _historyField;
		private ConsoleField _inputField;

		private TimeSpan _blinkShowtime = TimeSpan.Zero;
		private TimeSpan _blinkSpeed = TimeSpan.FromSeconds(0.5f);
		private bool _cursorVisiblePhase = true;

		private Vector2 _inputTextPadding = new Vector2(3, 5);
		private Vector2 _historyTextPadding = new Vector2(5, 5);

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			_blinkShowtime += gameTime.ElapsedGameTime;

			if (_blinkShowtime >= _blinkSpeed)
			{
				_cursorVisiblePhase = !_cursorVisiblePhase;
				_blinkShowtime = TimeSpan.Zero;
			}



			//////////////////////////////////////////////////////
			//The better way to draw it on new RenderTarget2D/////
			//but in this case i'll lose the different opacities//
			////////////on game-rendered background///////////////
			//////////////////////////////////////////////////////
			spriteBatch.Begin();

			spriteBatch.Draw(_solidColorTexture, ConsoleBounds, BackColor * BackOpacity); // BG

			spriteBatch.Draw(_solidColorTexture, _historyField.Bounds, _historyField.BackColor * _historyField.BackOpacity); // BG HISTORY

			spriteBatch.Draw(_solidColorTexture, _inputField.Bounds, _inputField.BackColor * _inputField.BackOpacity); // BG INPUT

			for (var i = 0; i < InputTarget.ConsoleHistory.Count; i++)
			{
				string entry = InputTarget.ConsoleHistory[i];
				Vector2 offset = new Vector2(_historyField.Bounds.X + _historyTextPadding.X,
					_historyField.Bounds.Y + _historyTextPadding.Y + (i * Font.LineSpacing));
				spriteBatch.DrawString(Font, entry, offset, Color.White * FontOpacity); // HISTORY TEXT
			}

			spriteBatch.DrawString(Font, InputTarget.CurrentInput,
				_inputField.Bounds.Location.ToVector2() + _inputTextPadding, Color.White * FontOpacity); // INPUT TEXT

			Vector2 size = Font.MeasureString(InputTarget.CurrentInput.Substring(0, InputTarget.CursorPosition));

			if (_cursorVisiblePhase || InputTarget.TextEditor.CursorMoving)
				spriteBatch.DrawString(Font, "|", new Vector2(_inputField.Bounds.X + size.X, _inputField.Bounds.Y + _inputField.Padding.Y + 1),
					Color.White);

			spriteBatch.End();
		}


		public void Update(GameTime gameTime)
		{
			
		}

	}
}
