using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Miku.Framework.Input;
using Miku.Framework.Drawing;

namespace Miku.Framework.Console
{
	internal class ConsoleRenderManager
	{
		private readonly GraphicsDevice _graphicDevice;

		private ConsoleField _historyField;
		private ConsoleField _inputField;

		private TimeSpan _blinkShowtime = TimeSpan.Zero;
		private TimeSpan _blinkSpeed = TimeSpan.FromSeconds(0.5f);
		private bool _cursorVisiblePhase = true;

		private Vector2 _inputTextPadding = new Vector2(3, 5);
		private Vector2 _historyTextPadding = new Vector2(5, 5);

		private SpriteFont _font;

		internal SpriteFont Font
		{
			get { return _font; }
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(Font));

				_font = HistoryRenderer.Font = value;
			}
		}

		internal ConsoleHistoryRenderer HistoryRenderer;

		public float ScrollSpeed { get; set; } = 1f;

		public ConsoleInputManager InputTarget { get; }

		public Rectangle ViewportBounds => _graphicDevice.Viewport.Bounds;

		public Rectangle ConsoleBounds => new Rectangle(0, 0, ViewportBounds.Width,
			(int) Math.Max(_graphicDevice.Viewport.Bounds.Height / 2.5f, 200f));


		public Color BackColor { get; set; } = Color.White;
		public float BackOpacity { get; set; } = 0.5f;

		public float InputFontOpacity { get; set; } = 1f;
		public Color InputTextColor { get; set; } = Color.White;

		public ConsoleRenderManager(ConsoleInputManager inputManager, GraphicsDevice graphicDevice)
		{
			_graphicDevice = graphicDevice;
			InputTarget = inputManager;

			HistoryRenderer = new ConsoleHistoryRenderer(InputTarget.ConsoleHistory, Font)
			{
				ScrollBarPadding = new Point(1, 1),
				ScrollBarWidth = 5,
				ScrollBarOpacity = 0.5f,
				ScrollStripOpacity = 0.6f
			};

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
				Padding = new Point(5, 5),
				BackColor = Color.Black,
				BackOpacity = 0.5f
			};
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			_graphicDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PlatformContents;

			_blinkShowtime += gameTime.ElapsedGameTime;

			if (_blinkShowtime >= _blinkSpeed)
			{
				_cursorVisiblePhase = !_cursorVisiblePhase;
				_blinkShowtime = TimeSpan.Zero;
			}

			//TODO: add padding for right side of history
			//TODO: renderTarget redraw only if history or client size changed
			RenderTarget2D historyOutput = new RenderTarget2D(_graphicDevice, _historyField.Bounds.Width, 
				_historyField.Bounds.Height, false, SurfaceFormat.Bgra32, DepthFormat.None, 1, RenderTargetUsage.PlatformContents);

			_graphicDevice.SetRenderTarget(historyOutput);

			HistoryRenderer.Render(gameTime, spriteBatch, new Rectangle(_historyTextPadding.ToPoint(), historyOutput.Bounds.Size));

			_graphicDevice.SetRenderTarget(null);

			/////////////////////////////////////////////////////////
			//The better way to draw it on new RenderTarget2D////////
			//but in this case it will lose the different opacities//
			////////////on game-rendered background//////////////////
			/////////////////////////////////////////////////////////
			spriteBatch.Begin();
			
			spriteBatch.DrawRect(ConsoleBounds, BackColor * BackOpacity); // BG

			spriteBatch.DrawRect(_historyField.Bounds, _historyField.BackColor * _historyField.BackOpacity); // BG HISTORY

			spriteBatch.DrawRect(_inputField.Bounds, _inputField.BackColor * _inputField.BackOpacity); // BG INPUT

			spriteBatch.DrawString(Font, InputTarget.CurrentInput,
				_inputField.Bounds.Location.ToVector2() + _inputTextPadding, InputTextColor * InputFontOpacity); // INPUT TEXT

			Vector2 size = Font.MeasureString(InputTarget.CurrentInput.Substring(0, InputTarget.CursorPosition));

			if (_cursorVisiblePhase || InputTarget.TextEditor.CursorMoving)
				spriteBatch.DrawString(Font, "|", new Vector2(_inputField.Bounds.X + size.X, _inputField.Bounds.Y + _inputField.Padding.Y + 1),
					Color.White);

			spriteBatch.Draw(historyOutput, _historyField.Bounds, new Rectangle(Point.Zero, _historyField.Size), Color.White);

			spriteBatch.End();

			historyOutput.Dispose();
			_graphicDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.DiscardContents;
		}

		public void Update(GameTime gameTime)
		{
			int delta = MouseComponent.ScrolledInBounds(_historyField.Bounds);

			HistoryRenderer.ScrollDelta += (int)(delta / gameTime.ElapsedGameTime.TotalMilliseconds) * ScrollSpeed;
		}
	}
}
