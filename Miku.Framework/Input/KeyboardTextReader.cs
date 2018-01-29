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
	public class KeyboardTextReader
	{
		private enum IgnoreType { All, Keys, Characters, KeyCharcter }

		protected StringBuilder _buffer = new StringBuilder();
		protected internal Queue<KeyInfo> _keysToHandle = new Queue<KeyInfo>();

		private int _inputIgnoreCount;
		private Keys _inputIgnoreKey;
		private char _inputIgnoreChar;
		private IgnoreType _inputIgnoreType;

		public void Pause() => Enabled = false;
		public void Unpause() => Enabled = true;
		public bool Enabled { get; set; } = true;

		public event EventHandler<CharHandlingEventArgs> CharHandling;

		public string Text
		{
			get { return _buffer.ToString(); }
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(Text));
				_buffer.Clear();
				_buffer.Append(value);
			}
		}

		public KeyboardTextReader(GameWindow window)
		{
			if (window == null)
				throw new ArgumentNullException(nameof(window));

			window.TextInput += TextInput;
		}

		private void TextInput(object sender, TextInputEventArgs e)
		{
			if (!Enabled)
				return;

			// Is there empty every e.Key
			// TODO: detect key by other way
			// (for keys ignore)

			_keysToHandle.Enqueue(new KeyInfo(e.Key, e.Character));
		}

		protected virtual void WriteTextToBuffer(string str)
		{
			_buffer.Append(str);
		}

		protected virtual void WriteTextToBuffer(char chr)
		{
			_buffer.Append(chr);
		}

		protected virtual void WriteControlToBuffer(char control) {  }

		public virtual void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;


			while (_keysToHandle.Count > 0)
			{
				KeyInfo key = _keysToHandle.Dequeue();

				if (ShouldIgnore(key))
				{
					//TODO: fix bug (if i tap a key before open console, console opening with console-open key);
					_inputIgnoreCount--;
					continue;
				}

				CharHandlingEventArgs eventArgs = new CharHandlingEventArgs(key.Character);
				CharHandling?.Invoke(this, eventArgs);

				if (eventArgs.IgnoreHandle)
					continue;

				if (!char.IsControl(key.Character))
					WriteTextToBuffer(key.Character);
				else
					WriteControlToBuffer(key.Character);
			}
			
		}

		public void AbsolveIgnore() => IgnoreInput(0);
		public void IgnoreInput(int count) => IgnoreInput(count, Keys.None, '\0');
		public void IgnoreInput(int count, char keyChar) => IgnoreInput(count, Keys.None, keyChar);


		[Obsolete("Key ignore by enum value not supported")]
		public void IgnoreInput(int count, Keys key) => IgnoreInput(count, key, '\0');

		[Obsolete("Key ignore by enum value not supported")]
		public void IgnoreInput(int count, KeyInfo keyInfo) => IgnoreInput(count, keyInfo.Key, keyInfo.Character);

		private void IgnoreInput(int count, Keys key, char chr)
		{
			_inputIgnoreCount = count;
			_inputIgnoreKey = key;
			_inputIgnoreChar = chr;

			if (key == Keys.None && chr == '\0')
				_inputIgnoreType = IgnoreType.All;
			else if (key != Keys.None && chr == '\0')
				_inputIgnoreType = IgnoreType.Keys;
			else if (key == Keys.None && chr != '\0')
				_inputIgnoreType = IgnoreType.Characters;
			else
				_inputIgnoreType = IgnoreType.KeyCharcter;
		}

		private bool ShouldIgnore(KeyInfo key)
		{
			if (_inputIgnoreCount == 0)
				return false;

			switch (_inputIgnoreType)
			{
				case IgnoreType.All:
					return true;
				case IgnoreType.Characters:
					if (_inputIgnoreChar == key.Character)
						return true;
					break;
				case IgnoreType.Keys:
					if (_inputIgnoreKey == key.Key)
						return true;
					break;
				case IgnoreType.KeyCharcter:
					if (_inputIgnoreKey == key.Key && _inputIgnoreChar == key.Character)
						return true;
					break;
			}

			return false;
		}
	}
}
