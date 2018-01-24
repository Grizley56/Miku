using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miku.Framework.Console
{
	public class ConsoleEntry
	{
		private float _textOpacity;
		private float _timeOpactiy;

		public DateTime Time { get; }
		public string Data { get; }
		public Color TextColor { get; }
		public Color TimeColor { get; } = Color.Green;
		public bool TimeVisible { get; set; }

		public float TextOpacity
		{
			get { return _textOpacity; }
			set { _textOpacity = MathHelper.Clamp(value, 0f, 1f); }
		}

		public float TimeOpactiy
		{
			get { return _timeOpactiy; }
			set { _timeOpactiy = MathHelper.Clamp(value, 0f, 1f); }
		}

		public ConsoleEntry(string data, Color? color = null, bool showTime = true)
		{
			Data = data;
			TextColor = color ?? Color.White;
			TimeVisible = showTime;
			Time = DateTime.Now;
			TextOpacity = 1f;
		}
	}
}