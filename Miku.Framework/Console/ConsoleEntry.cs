using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miku.Framework.Console
{
	public class ConsoleEntry
	{
		private Color? _textColor;
		private Color? _timeColor;

		public DateTime Time { get; }
		public string Data { get; }

		public Color TextColor
		{
			get
			{
				return _textColor ?? GameConsole.Instance.Skin.HistoryField.TextColor;
			}
			set
			{
				_textColor = value;
			}
		} 
		public Color TimeColor
		{
			get { return _timeColor ?? GameConsole.Instance.Skin.DefaultHistoryTimeColor; }
			set
			{
				_timeColor = value;
			}
		} 

		public bool TimeVisible { get; set; }

		public ConsoleEntry(string data, Color? color = null, bool showTime = true)
		{
			Data = data;
			_textColor = color;
			_timeColor = null;

			TimeVisible = showTime;
			Time = DateTime.Now;
		}
	}
}