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

		private TimeSpan _shouldIgnoreBlinking = TimeSpan.Zero;
		private TimeSpan _blinkShowtime = TimeSpan.Zero;
		private TimeSpan _blinkSpeed = TimeSpan.FromSeconds(0.5f);
		private bool _cursorVisiblePhase = true;

		private Point _inputTextPadding = new Point(3, 5);
		private Point _historyTextPadding = new Point(5, 5);
		private Point _highlightingPadding = new Point(0, 2);

		private Point _visibleInputRange;
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
		public Color HighlightColor { get; set; } = Color.White * 0.5f;
		public Color CursorColor { get; set; } = Color.White;

		public ConsoleRenderManager(ConsoleInputManager inputManager, GraphicsDevice graphicDevice)
		{
			_graphicDevice = graphicDevice;
			InputTarget = inputManager;
			
			InputTarget.TextEditor.CursorPositionChanged += (_, __) => _shouldIgnoreBlinking = TimeSpan.FromSeconds(0.5f);
			InputTarget.TextEditor.CursorPositionChanged += CursorPositionChanged;
			InputTarget.TextEditor.TextRemoved += InputTextRemoved;
			HistoryRenderer = new ConsoleHistoryRenderer(InputTarget.ConsoleHistory, Font)
			{
				ScrollBarPadding = new Point(1, 1),
				ScrollBarWidth = 5,
				ScrollBarOpacity = 0.5f,
				ScrollStripOpacity = 0.6f
			};

			InitializeFacade();
		}

		private void InputTextRemoved(object sender, TextRemovedEventArgs e)
		{
			if (_visibleInputRange.Y > e.ResultText.Length)
			{
				if (_visibleInputRange.X == 0)
					_visibleInputRange.Y -= _visibleInputRange.Y - InputTarget.CurrentInput.Length;
				else
					_visibleInputRange -= new Point(_visibleInputRange.Y - InputTarget.CurrentInput.Length);
			}
		}
		
		private void CursorPositionChanged(object sender, CursorPositionChangedEventArgs e)
		{
			_shouldIgnoreBlinking = TimeSpan.FromSeconds(0.5f);

			if (e.NewPosition > _visibleInputRange.Y)
				_visibleInputRange += new Point(Math.Min(e.NewPosition - e.OldPosition, InputTarget.CurrentInput.Length - _visibleInputRange.Y));
			else if (e.NewPosition < _visibleInputRange.X)
				_visibleInputRange -= new Point(Math.Min(e.OldPosition - e.NewPosition, _visibleInputRange.X));
		}

		private void InitializeFacade()
		{
			_inputField = new ConsoleField(() => new Rectangle(ConsoleBounds.X,
				ConsoleBounds.Height - Font.LineSpacing - _inputTextPadding.Y * 2, ConsoleBounds.Width,
				Font.LineSpacing + _inputTextPadding.Y * 2))
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

			//TODO: renderTarget redraw only if history or client size changed
			RenderTarget2D historyOutput = new RenderTarget2D(_graphicDevice, _historyField.Bounds.Width, 
				_historyField.Bounds.Height, false, SurfaceFormat.Bgra32, DepthFormat.None, 1, RenderTargetUsage.PlatformContents);

			_graphicDevice.SetRenderTarget(historyOutput);

			HistoryRenderer.Render(gameTime, spriteBatch, new Rectangle(_historyTextPadding, historyOutput.Bounds.Size));

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

			string croppedInput = InputTarget.CurrentInput.Substring(_visibleInputRange.X, 
																															 _visibleInputRange.Y - _visibleInputRange.X);
			
			spriteBatch.DrawString(Font, croppedInput,
				_inputField.Bounds.Location.ToVector2() + _inputTextPadding.ToVector2(), InputTextColor * InputFontOpacity); // INPUT TEXT

			if (InputTarget.TextEditor.IsTextHighlighted)
			{
				Point highlightRange = InputTarget.TextEditor.HighlightRange;

				int offsetBeforeHighlight = _inputTextPadding.X;

				// If the beginning of the highlight is further
				// than the beginning of the fragment
				// we should calculate size of string before highlight is start
				if (highlightRange.X > _visibleInputRange.X)
				{
					string textBeforeHighlighting = 
						InputTarget.CurrentInput.Substring(_visibleInputRange.X, highlightRange.X - _visibleInputRange.X);
					offsetBeforeHighlight += (int)Font.MeasureString(textBeforeHighlighting).X;
				}

				Point visiblePartOfHighlight = new Point(Math.Max(highlightRange.X, _visibleInputRange.X), 
																								 Math.Min(highlightRange.Y, _visibleInputRange.Y));

				int highlightWidth = (int)Font.MeasureString(InputTarget.CurrentInput
																			.Substring(visiblePartOfHighlight.X, visiblePartOfHighlight.Y - visiblePartOfHighlight.X))
																			.X;

				Rectangle highlightBounds = new Rectangle(_inputField.Bounds.Location +
					new Point(offsetBeforeHighlight, _highlightingPadding.Y),
					new Point(highlightWidth, _inputField.Bounds.Height - _highlightingPadding.Y*2));
				
				spriteBatch.DrawRect(highlightBounds, HighlightColor);
			}
			
			int relativeCursorPosition = InputTarget.CursorPosition - _visibleInputRange.X;
			int offsetBeforeCursor = (int)Font.MeasureString(croppedInput.Substring(0, relativeCursorPosition)).X;

			if (_cursorVisiblePhase || InputTarget.TextEditor.CursorMoving || _shouldIgnoreBlinking > TimeSpan.Zero)
			{
				spriteBatch.DrawString(Font, "|", new Vector2(_inputField.Bounds.X + offsetBeforeCursor, _inputField.Bounds.Y 
					+ _inputField.Padding.Y + 1), CursorColor);
				_shouldIgnoreBlinking -= gameTime.ElapsedGameTime;
			}

			spriteBatch.Draw(historyOutput, _historyField.Bounds, new Rectangle(Point.Zero, _historyField.Size), Color.White);

			spriteBatch.End();

			historyOutput.Dispose();
			_graphicDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.DiscardContents;
		}

		public void Update(GameTime gameTime)
		{
			int delta = MouseComponent.ScrolledInBounds(_historyField.Bounds);

			HistoryRenderer.ScrollDelta += (int)(delta / gameTime.ElapsedGameTime.TotalMilliseconds) * ScrollSpeed;

			UpdateInputText();
		}

		private void UpdateInputText()
		{
			int maxWidth = _inputField.Bounds.Width - _inputTextPadding.X * 2;
			string fullInput = InputTarget.CurrentInput;

			if (Font.MeasureString(InputTarget.CurrentInput).X <= maxWidth)
			{
				_visibleInputRange = new Point(0, fullInput.Length);
				return;
			}

			string croppedInput = fullInput.Substring(_visibleInputRange.X, _visibleInputRange.Y - _visibleInputRange.X);
			int croppedInputWidth = (int)Font.MeasureString(croppedInput).X;

			float averageCharWidth = Font.MeasureString(croppedInput).X / croppedInput.Length;

			if (croppedInputWidth > maxWidth)
			{
				float excessedWidth = croppedInputWidth - maxWidth;
				int excessedChars = (int)Math.Ceiling(excessedWidth / averageCharWidth);
				for (int i = 0; i < excessedChars; i++)
				{
					if (InputTarget.CursorPosition - _visibleInputRange.X != 0)
						_visibleInputRange.X++; // Release left
					else
						_visibleInputRange.Y--; // Release right
				}
			}
			else if (croppedInputWidth + averageCharWidth < maxWidth)
			{
				int freeSpace = maxWidth - croppedInputWidth;
				int canAddCount = (int)Math.Floor(freeSpace / averageCharWidth);
				for (int i = 0; i < canAddCount; i++)
				{
					if (_visibleInputRange.Y < InputTarget.CurrentInput.Length)
						_visibleInputRange.Y++; // Add on right
					else
						_visibleInputRange.X--; // Add on left
				}
			}
		}
	}
}
