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
		private readonly GameConsole _console;
		private ConsoleField _historyField;
		private ConsoleField _inputField;
		private ConsoleField _autoCompleteField;
		private SpriteFont _font;
		private ConsoleSkin _skin;
		private TimeSpan _shouldIgnoreBlinking = TimeSpan.Zero;
		private TimeSpan _blinkShowtime = TimeSpan.Zero;
		private bool _cursorVisiblePhase = true;
		private Point _highlightingPadding = new Point(0, 2);
		private Point _visibleInputRange;
		private float _scrollSpeed = 1f;

		internal ConsoleHistoryRenderer HistoryRenderer;

		public ConsoleInputManager InputTarget => _console.InputManager;
		public GraphicsDevice GraphicsDevice => _console.Game.GraphicsDevice;
		public Rectangle ViewportBounds => GraphicsDevice.Viewport.Bounds;
		public Rectangle ConsoleBounds => new Rectangle(0, 0, ViewportBounds.Width,
			(int) Math.Max(ViewportBounds.Height / 2.5f, 200f));

		public SpriteFont Font
		{
			get { return _font; }
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(Font));
				_font = value;
			}
		}
		public ConsoleSkin Skin
		{
			get { return _skin; }
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(Skin));
				_skin = value;
			}
		}
		public float ScrollSpeed
		{
			get { return _scrollSpeed; }
			set
			{
				if (value < 0)
					throw new ArgumentException(nameof(ScrollSpeed));

				_scrollSpeed = value;
			}
		}

		public int GetAutoCompleteRealWidth()
		{
			if (InputTarget.AutoCompleteVariation.Length == 0)
				return 0;

			return InputTarget.AutoCompleteVariation.Select(variant =>
				       (int)Font.MeasureString(variant.CommandName).X).Max()
			       + _autoCompleteField.TextPadding.X * 2 + _autoCompleteField.Padding.X * 2;
		}

		public ConsoleRenderManager(GameConsole console, SpriteFont font, ConsoleSkin skin)
		{
			if (console == null)
				throw new ArgumentNullException(nameof(console));

			Font = font;
			Skin = skin;
			_console = console;

			InputTarget.TextEditor.CursorPositionChanged += (_, __) => _shouldIgnoreBlinking = TimeSpan.FromSeconds(0.5f);
			InputTarget.TextEditor.CursorPositionChanged += CursorPositionChanged;
			InputTarget.TextEditor.TextRemoved += InputTextRemoved;

			HistoryRenderer = new ConsoleHistoryRenderer(this);
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
			_inputField = new ConsoleField(() => new Rectangle(
				ConsoleBounds.X,
				ConsoleBounds.Height - Font.LineSpacing - _inputField.TextPadding.Y * 2, 
				ConsoleBounds.Width,
				Font.LineSpacing + _inputField.TextPadding.Y * 2), 
				new Point(5, 3),
				new Point(3, 5));

			_historyField = new ConsoleField(() => new Rectangle(
				ConsoleBounds.X,
				ConsoleBounds.Y,
				ConsoleBounds.Width,
				ConsoleBounds.Height - _inputField.Bounds.Height - _inputField.Padding.Y * 2), 
				new Point(5, 5),
				new Point(5, 5));

			_autoCompleteField = new ConsoleField(() => new Rectangle(
				ConsoleBounds.X,
				ConsoleBounds.Y + ConsoleBounds.Height,
				Math.Min(ConsoleBounds.Width, GetAutoCompleteRealWidth()),
				Font.LineSpacing * InputTarget.AutoCompleteVariation.Length), 
				new Point(5, 0),
				new Point(10, 1));
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PlatformContents;

			_blinkShowtime += gameTime.ElapsedGameTime;

			if (_blinkShowtime >= Skin.CursorBlinkSpeed)
			{
				_cursorVisiblePhase = !_cursorVisiblePhase;
				_blinkShowtime = TimeSpan.Zero;
			}

			var inputFieldBounds = _inputField.Bounds;
			var historyFieldBounds = _historyField.Bounds;

			//TODO: renderTarget redraw only if history or client size changed
			RenderTarget2D historyOutput = new RenderTarget2D(GraphicsDevice, historyFieldBounds.Width,
				historyFieldBounds.Height, false, SurfaceFormat.Bgra32, DepthFormat.None, 1, RenderTargetUsage.PlatformContents);

			GraphicsDevice.SetRenderTarget(historyOutput);

			HistoryRenderer.Render(gameTime, spriteBatch, new Rectangle(_historyField.TextPadding, historyOutput.Bounds.Size));

			GraphicsDevice.SetRenderTarget(null);

			/////////////////////////////////////////////////////////
			//The better way to draw it on new RenderTarget2D////////
			//but in this case it will lose the different opacities//
			////////////on game-rendered background//////////////////
			/////////////////////////////////////////////////////////
			spriteBatch.Begin();
			
			spriteBatch.DrawRect(ConsoleBounds, Skin.ConsoleBackground); // BG

			spriteBatch.DrawRect(historyFieldBounds, Skin.HistoryField.BackColor); // BG HISTORY

			spriteBatch.DrawRect(inputFieldBounds, Skin.InputField.BackColor); // BG INPUT

			string croppedInput = InputTarget.CurrentInput.Substring(_visibleInputRange.X, 
																															 _visibleInputRange.Y - _visibleInputRange.X);
			
			spriteBatch.DrawString(Font, croppedInput,
				inputFieldBounds.Location.ToVector2() + _inputField.TextPadding.ToVector2(), 
				Skin.InputField.TextColor); // INPUT TEXT

			if (InputTarget.TextEditor.IsTextHighlighted)
			{
				Point highlightRange = InputTarget.TextEditor.HighlightRange;

				int offsetBeforeHighlight = _inputField.TextPadding.X;

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

				Rectangle highlightBounds = new Rectangle(inputFieldBounds.Location +
					new Point(offsetBeforeHighlight, _highlightingPadding.Y),
					new Point(highlightWidth, inputFieldBounds.Height - _highlightingPadding.Y*2));
				
				spriteBatch.DrawRect(highlightBounds, Skin.HighlightColor);
			}

			//===============Cursor================//
			bool forciblyIgnore = _shouldIgnoreBlinking > TimeSpan.Zero;

			if (_cursorVisiblePhase || InputTarget.TextEditor.CursorMoving || forciblyIgnore)
			{
				int relativeCursorPosition = InputTarget.CursorPosition - _visibleInputRange.X;
				int offsetBeforeCursor = (int)Font.MeasureString(croppedInput.Substring(0, relativeCursorPosition)).X + 2;

				if (relativeCursorPosition != 0)
					offsetBeforeCursor += (int)Font.Spacing / 2;

				spriteBatch.DrawRect(new Rectangle(inputFieldBounds.X + offsetBeforeCursor, inputFieldBounds.Y
					+ _inputField.Padding.Y + 1, Skin.CursorWidth, 
					inputFieldBounds.Height - _inputField.Padding.Y * 2 - 1), 
					Skin.CursorColor);

				if(forciblyIgnore)
					_shouldIgnoreBlinking -= gameTime.ElapsedGameTime;
			}

			//===============History================//
			spriteBatch.Draw(historyOutput, historyFieldBounds, new Rectangle(Point.Zero, historyFieldBounds.Size), Color.White);


			//===============AutoComplete============//
			var autoCompleteBounds = _autoCompleteField.Bounds;

			spriteBatch.DrawRect(autoCompleteBounds, Skin.AutoCompleteField.BackColor);

			spriteBatch.DrawRect(new Rectangle(autoCompleteBounds.X, autoCompleteBounds.Y, 3, autoCompleteBounds.Height + 1),
				Skin.AutoCompleteBorder);

			spriteBatch.DrawRect(new Rectangle(autoCompleteBounds.Right - 3, autoCompleteBounds.Y, 3, 
				autoCompleteBounds.Height + 1), Skin.AutoCompleteBorder);
			
			for (int i = 0; i < InputTarget.AutoCompleteVariation.Length; i++)
			{
				var position = new Vector2(autoCompleteBounds.X + _autoCompleteField.TextPadding.X, 
					autoCompleteBounds.Y + _autoCompleteField.TextPadding.Y + Font.LineSpacing * i);

				spriteBatch.DrawString(Font, InputTarget.AutoCompleteVariation[i].CommandName, position, 
					i == InputTarget.AutoCompleteSelectedIndex 
						? Skin.AutoCompleteSelectedTextColor
						: Skin.AutoCompleteField.TextColor);

				spriteBatch.DrawRect(new Rectangle((int)position.X - _autoCompleteField.TextPadding.X + 3, 
					(int)position.Y + Font.LineSpacing - 2, autoCompleteBounds.Width - 6, 2), 
					Skin.AutoCompleteBorder);
			}

			spriteBatch.End();


			historyOutput.Dispose();
			GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.DiscardContents;
		}

		public void Update(GameTime gameTime)
		{
			int delta = MouseComponent.ScrolledInBounds(_historyField.Bounds);

			HistoryRenderer.ScrollDelta += (int)(delta / gameTime.ElapsedGameTime.TotalMilliseconds) * ScrollSpeed;
			
			UpdateInputText();
		}

		private void UpdateInputText()
		{
			int maxWidth = _inputField.Bounds.Width - _inputField.TextPadding.X * 2;
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
