﻿using System;
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
		private ConsoleField _autoCompleteField;

		private TimeSpan _shouldIgnoreBlinking = TimeSpan.Zero;
		private TimeSpan _blinkShowtime = TimeSpan.Zero;
		private TimeSpan _blinkSpeed = TimeSpan.FromSeconds(0.5f);
		private bool _cursorVisiblePhase = true;

		private Point _inputTextPadding = new Point(3, 5);
		private Point _historyTextPadding = new Point(5, 5);
		private Point _highlightingPadding = new Point(0, 2);
		private Point _autocompleteTextPadding = new Point(10, 1);
		private Point _visibleInputRange;

		private SpriteFont _font;
		private float _scrollSpeed = 1f;
		private int _cursorWidth = 2;
		private Console _console;

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
		public int CursorWidth
		{
			get { return _cursorWidth; }
			set
			{
				if (value < 0)
					throw new ArgumentException(nameof(CursorWidth));

				_cursorWidth = value;
			}
		}

		public ConsoleInputManager InputTarget { get; }

		public Rectangle ViewportBounds => _graphicDevice.Viewport.Bounds;
		public int AutocompleteFieldReadlWidth => InputTarget.AutoCompleteVariation.Length == 0
			? 0
			: InputTarget.AutoCompleteVariation.Select(variant =>
				(int)Font.MeasureString(variant.CommandName).X).Max();

		public Rectangle ConsoleBounds => new Rectangle(0, 0, ViewportBounds.Width,
			(int) Math.Max(_graphicDevice.Viewport.Bounds.Height / 2.5f, 200f));

		public Color BackColor { get; set; } = Color.White;
		public Color InputTextColor { get; set; } = Color.White;
		public Color HighlightColor { get; set; } = Color.White * 0.5f;
		public Color CursorColor { get; set; } = Color.White;
		public Color AutoCompleteTextColor { get; set; } = Color.White;
		public Color AutoCompleteSelectedTextColor { get; set; } = Color.Gold;
		public Color AutoCompleteBorderColor { get; set; } = Color.White * 0.6f;

		public ConsoleRenderManager(Console console, GraphicsDevice graphicDevice)
		{
			_graphicDevice = graphicDevice;
			_console = console;
			InputTarget = console.InputManager;
			
			InputTarget.TextEditor.CursorPositionChanged += (_, __) => _shouldIgnoreBlinking = TimeSpan.FromSeconds(0.5f);
			InputTarget.TextEditor.CursorPositionChanged += CursorPositionChanged;
			InputTarget.TextEditor.TextRemoved += InputTextRemoved;

			HistoryRenderer = new ConsoleHistoryRenderer(InputTarget.ConsoleHistory, Font)
			{
				ScrollBarPadding = new Point(1, 1),
				ScrollBarWidth = 5
			};

			HistoryRenderer.ScrollBarColor *= 0.5f;
			HistoryRenderer.ScrollStripColor *= 0.6f;

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
				BackColor = Color.Black * 0.5f,
			};

			_historyField = new ConsoleField(() => new Rectangle(ConsoleBounds.X, ConsoleBounds.Y, ConsoleBounds.Width,
				ConsoleBounds.Height - _inputField.Bounds.Height - _inputField.Padding.Y * 2))
			{
				Padding = new Point(5, 5),
				BackColor = Color.Black * 0.5f
			};

			_autoCompleteField = new ConsoleField(() => new Rectangle(ConsoleBounds.X, ConsoleBounds.Y + ConsoleBounds.Height,
				Math.Min(ConsoleBounds.Width, AutocompleteFieldReadlWidth + _autocompleteTextPadding.X*2 + _autoCompleteField.Padding.X*2), 
				Font.LineSpacing * InputTarget.AutoCompleteVariation.Length))
			{
				BackColor = new Color(55, 55, 87) * 0.7f,
				Padding = new Point(5, 0)
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

			var inputFieldBounds = _inputField.Bounds;
			var historyFieldBounds = _historyField.Bounds;

			//TODO: renderTarget redraw only if history or client size changed
			RenderTarget2D historyOutput = new RenderTarget2D(_graphicDevice, historyFieldBounds.Width,
				historyFieldBounds.Height, false, SurfaceFormat.Bgra32, DepthFormat.None, 1, RenderTargetUsage.PlatformContents);

			_graphicDevice.SetRenderTarget(historyOutput);

			HistoryRenderer.Render(gameTime, spriteBatch, new Rectangle(_historyTextPadding, historyOutput.Bounds.Size));

			_graphicDevice.SetRenderTarget(null);

			/////////////////////////////////////////////////////////
			//The better way to draw it on new RenderTarget2D////////
			//but in this case it will lose the different opacities//
			////////////on game-rendered background//////////////////
			/////////////////////////////////////////////////////////
			spriteBatch.Begin();
			
			spriteBatch.DrawRect(ConsoleBounds, BackColor); // BG

			spriteBatch.DrawRect(historyFieldBounds, _historyField.BackColor); // BG HISTORY

			spriteBatch.DrawRect(inputFieldBounds, _inputField.BackColor); // BG INPUT

			string croppedInput = InputTarget.CurrentInput.Substring(_visibleInputRange.X, 
																															 _visibleInputRange.Y - _visibleInputRange.X);
			
			spriteBatch.DrawString(Font, croppedInput,
				inputFieldBounds.Location.ToVector2() + _inputTextPadding.ToVector2(), InputTextColor); // INPUT TEXT

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

				Rectangle highlightBounds = new Rectangle(inputFieldBounds.Location +
					new Point(offsetBeforeHighlight, _highlightingPadding.Y),
					new Point(highlightWidth, inputFieldBounds.Height - _highlightingPadding.Y*2));
				
				spriteBatch.DrawRect(highlightBounds, HighlightColor);
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
					+ _inputField.Padding.Y + 1, CursorWidth, inputFieldBounds.Height - _inputField.Padding.Y * 2 - 1), CursorColor);

				if(forciblyIgnore)
					_shouldIgnoreBlinking -= gameTime.ElapsedGameTime;
			}

			//===============History================//
			spriteBatch.Draw(historyOutput, historyFieldBounds, new Rectangle(Point.Zero, historyFieldBounds.Size), Color.White);


			//===============AutoComplete============//
			var autoCompleteBounds = _autoCompleteField.Bounds;

			spriteBatch.DrawRect(autoCompleteBounds, _autoCompleteField.BackColor);

			spriteBatch.DrawRect(new Rectangle(autoCompleteBounds.X, autoCompleteBounds.Y, 3, autoCompleteBounds.Height + 1), 
				AutoCompleteBorderColor);

			spriteBatch.DrawRect(new Rectangle(autoCompleteBounds.X + autoCompleteBounds.Width - 3, autoCompleteBounds.Y, 3, 
				autoCompleteBounds.Height + 1), AutoCompleteBorderColor);
			
			for (int i = 0; i < InputTarget.AutoCompleteVariation.Length; i++)
			{
				var position = new Vector2(autoCompleteBounds.X + _autocompleteTextPadding.X, 
					autoCompleteBounds.Y + _autocompleteTextPadding.Y + Font.LineSpacing * i);

				spriteBatch.DrawString(Font, InputTarget.AutoCompleteVariation[i].CommandName, position, 
					i == InputTarget.AutoCompleteSelectedIndex 
						? AutoCompleteSelectedTextColor 
						: AutoCompleteTextColor);

				spriteBatch.DrawRect(new Rectangle((int)position.X - _autocompleteTextPadding.X + 3, 
					(int)position.Y + Font.LineSpacing - 2, autoCompleteBounds.Width - 6, 2), AutoCompleteBorderColor);
			}

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
