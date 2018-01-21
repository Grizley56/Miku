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

		private readonly KeyboardHoldingTracer _leftArrowTracer;
		private readonly KeyboardHoldingTracer _rightArrowTracer;

		private TimeSpan _controlKeysRepeatSpeed = TimeSpan.FromMilliseconds(50);

		public KeyboardTextEditor(GameWindow window, Func<char, bool> knownCharacters = null) : base(window, knownCharacters)
		{
			_leftArrowTracer = new KeyboardHoldingTracer(Keys.Left, TimeSpan.FromSeconds(0.5f), ControlKeysRepeatSpeed);
			_rightArrowTracer = new KeyboardHoldingTracer(Keys.Right, TimeSpan.FromSeconds(0.5f), ControlKeysRepeatSpeed);
		}


		protected override void WriteControlToBuffer(char control)
		{
			switch (control)
			{
				case '\r':
					TextEntered?.Invoke(this, new TextEnteredEventArgs(Text));
					Clear();
					CursorPosition = 0;
					break;
			}
		}

		protected override bool RemoveTextFromBuffer()
		{
			if (base.RemoveTextFromBuffer())
			{
				CursorPosition--;
				return true;
			}
			return false;
		}

		protected override void WriteTextToBuffer(char chr)
		{
			base.WriteTextToBuffer(chr);
			CursorPosition++;
		}

		protected override void WriteTextToBuffer(string str)
		{
			base.WriteTextToBuffer(str);
			CursorPosition += str.Length;
		}

		public void SetCursorTo(InputCursorPosition position)
		{
			switch (position)
			{
				case InputCursorPosition.Begin:
					CursorPosition = 0;
					break;
				case InputCursorPosition.Center:
					CursorPosition = _buffer.Length == 0 ? 0 : _buffer.Length / 2;
					break;
				case InputCursorPosition.End:
					CursorPosition = _buffer.Length;
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
				{
					int offset = _buffer.ToString().IndexOf(CursorPosition, " ", Direction.RightToLeft) + 1;
					CursorPosition = _buffer.Length - offset;
				}
				else
					CursorPosition--;
			}

			if (_rightArrowTracer.Check() && CursorPosition < _buffer.Length)
			{
				if (KeyboardComponent.IsKeyDown(Keys.LeftControl) || KeyboardComponent.IsKeyDown(Keys.RightControl))
				{
					int offset = _buffer.ToString().IndexOf(' ', CursorPosition) + 1;
					CursorPosition = offset;
				}
				else
					CursorPosition++;
			}
		}
	}

}
