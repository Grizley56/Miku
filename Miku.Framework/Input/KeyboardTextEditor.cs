using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Miku.Framework.Console;

namespace Miku.Framework.Input
{
	public class KeyboardTextEditor : KeyboardTextReader
	{
		public event EventHandler<TextEnteredEventArgs> TextEntered;
		public event EventHandler<CursorPositionChangedEventArgs> CursorPositionChanged;
		public int CursorPosition { get; private set; }
		public TimeSpan ControlKeysRepeatSpeed
		{
			get { return _controlKeysRepeatSpeed; }
			set
			{
				if (_controlKeysRepeatSpeed == value)
					return;

				_controlKeysRepeatSpeed = value;
				_leftArrowTracer.TimeBetweenRepeat = value;
				_rightArrowTracer.TimeBetweenRepeat = value;
			}
		}
		public bool CursorMoving => _leftArrowTracer.IsHolded || _rightArrowTracer.IsHolded;

		private readonly KeyboardHoldingTracer _leftArrowTracer;
		private readonly KeyboardHoldingTracer _rightArrowTracer;
		private readonly KeyboardHoldingTracer _deleteTracer;

		private TimeSpan _controlKeysRepeatSpeed = TimeSpan.FromMilliseconds(35);
		private readonly char[] _sectionSplitSymbols =  {' ', ',', '.'};

		public KeyboardTextEditor(GameWindow window, Func<char, bool> knownCharacters = null) : base(window, knownCharacters)
		{
			_leftArrowTracer = new KeyboardHoldingTracer(Keys.Left, TimeSpan.FromSeconds(0.3f), ControlKeysRepeatSpeed);
			_rightArrowTracer = new KeyboardHoldingTracer(Keys.Right, TimeSpan.FromSeconds(0.3f), ControlKeysRepeatSpeed);
			_deleteTracer = new KeyboardHoldingTracer(Keys.Delete, TimeSpan.FromSeconds(0.3f), ControlKeysRepeatSpeed);
		}


		protected override void WriteControlToBuffer(char control)
		{
			int count;
			switch (control)
			{
				case '\r': // Enter
					OnTextEntered(new TextEnteredEventArgs(Text));
					Clear();
					CursorPosition = 0;
					goto default;
				case '\b': // backspace
					if (CursorPosition == 0)
						break;
					RemoveTextFromBuffer(--CursorPosition, 1);
					goto default;
				case '\u007f': // Ctrl + backspace
					int startIndex = GetIndexOfPreviousSection(_buffer.ToString(), _sectionSplitSymbols, CursorPosition);
					count = CursorPosition - startIndex;
					RemoveTextFromBuffer(startIndex, count);
					CursorPosition -= count;
					goto default;
				case '\u0000': // Del
					RemoveTextFromBuffer(CursorPosition, 1);
					break;
				case '\u0010': // Ctrl + Del
					int endIndex = GetIndexOfNextSection(_buffer.ToString(), _sectionSplitSymbols, CursorPosition);
					count = endIndex - CursorPosition;
					RemoveTextFromBuffer(CursorPosition, count);
					break;
				default:
					OnCursorPositionChanged();
					break;
			}
		}

		protected virtual bool RemoveTextFromBuffer(int startIndex, int count)
		{
			if (startIndex + count > _buffer.Length)
				throw new ArgumentOutOfRangeException();

			if (count == 0 || startIndex == _buffer.Length)
				return false;

			_buffer.Remove(startIndex, count);
			return true;
		}

		protected override void WriteTextToBuffer(char chr)
		{
			_buffer.Insert(CursorPosition++, chr);
			OnCursorPositionChanged();
		}

		protected override void WriteTextToBuffer(string str)
		{
			_buffer.Insert(CursorPosition, str);
			CursorPosition += str.Length;
			OnCursorPositionChanged();
		}

		public void SetCursorTo(InputCursorPosition position)
		{
			switch (position)
			{
				case InputCursorPosition.Begin:
					CursorPosition = 0;
					goto default;
				case InputCursorPosition.Center:
					CursorPosition = _buffer.Length == 0 ? 0 : _buffer.Length / 2;
					goto default;
				case InputCursorPosition.End:
					CursorPosition = _buffer.Length;
					goto default;
				default:
					OnCursorPositionChanged();
					break;
			}
		}

		public override void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;

			base.Update(gameTime);

			if (_leftArrowTracer.Check() && CursorPosition > 0)
			{
				if (KeyboardComponent.IsKeyDown(Keys.LeftControl) || KeyboardComponent.IsKeyDown(Keys.RightControl))
					CursorPosition = GetIndexOfPreviousSection(_buffer.ToString(), _sectionSplitSymbols, CursorPosition);
				else
					CursorPosition--;

				OnCursorPositionChanged();
			}

			if (_rightArrowTracer.Check() && CursorPosition < _buffer.Length)
			{
				if (KeyboardComponent.IsKeyDown(Keys.LeftControl) || KeyboardComponent.IsKeyDown(Keys.RightControl))
					CursorPosition = GetIndexOfNextSection(_buffer.ToString(), _sectionSplitSymbols, CursorPosition);
				else
					CursorPosition++;
				OnCursorPositionChanged();
			}

			if (_deleteTracer.Check() && CursorPosition < _buffer.Length)
			{
				// I send unused range of Unicode symbols
				// for reduce count of code (for example \u0010 is DLE symbol (Data link escape))
				if (KeyboardComponent.IsKeyDown(Keys.LeftControl) || KeyboardComponent.IsKeyDown(Keys.RightControl))
					WriteControlToBuffer('\u0010');
				else
					WriteControlToBuffer('\u0000');
			}

		}

		private static int GetIndexOfNextSection(string str, char[] sectionSplitChars, int startIndex)
		{
			int nextSectionIndex = str.IndexOfAny(sectionSplitChars, startIndex);

			if (nextSectionIndex == startIndex)
				return GetIndexOfNextSection(str, sectionSplitChars, ++startIndex);

			if (nextSectionIndex == -1)
				nextSectionIndex = str.Length;

			return nextSectionIndex;
		}

		private static int GetIndexOfPreviousSection(string str, char[] sectionSplitChars, int startIndex)
		{
			int nextSectionIndex = str.IndexOfAny(sectionSplitChars, startIndex, Direction.RightToLeft);

			if (nextSectionIndex == startIndex)
				return GetIndexOfPreviousSection(str, sectionSplitChars, --startIndex);

			if (nextSectionIndex == -1)
				nextSectionIndex = 0;

			return nextSectionIndex;
		}

		protected virtual void OnCursorPositionChanged()
		{
			CursorPositionChanged?.Invoke(this, new CursorPositionChangedEventArgs(CursorPosition));
		}

		protected virtual void OnTextEntered(TextEnteredEventArgs e)
		{
			TextEntered?.Invoke(this, e);
		}
	}
}
